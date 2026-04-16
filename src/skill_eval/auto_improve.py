"""Autonomous skill/plugin improvement loop.

Iteratively runs the evaluation pipeline, applies suggested improvements
via the Copilot CLI, and repeats until the skill reaches a target quality
level, stops improving, or exhausts the allowed number of turns.
"""

from __future__ import annotations

import json
import os
import shutil
import subprocess
import time
from datetime import datetime, timezone
from pathlib import Path

import click

from skill_eval.config import Configuration, EvalConfig
from skill_eval.source_resolver import SourceResolver


# ---------------------------------------------------------------------------
# Score extraction
# ---------------------------------------------------------------------------

def _read_weighted_average(
    scores_path: Path,
    target_config: str,
) -> tuple[float | None, dict[str, float]]:
    """Read scores-data.json and compute the mean weighted total for *target_config*.

    Returns ``(weighted_average, per_dimension_means)`` or ``(None, {})`` on
    failure.  The weighted total already accounts for dimension weights
    (computed by the aggregator).
    """
    if not scores_path.exists():
        return None, {}

    try:
        data = json.loads(scores_path.read_text(encoding="utf-8"))
    except (json.JSONDecodeError, OSError):
        return None, {}

    runs: list[dict] = data.get("runs", [])
    if not runs:
        return None, {}

    # Weighted totals per run for the target config
    totals: list[float] = []
    for run in runs:
        wt = run.get("weighted_totals", {})
        if target_config in wt:
            totals.append(wt[target_config])

    if not totals:
        return None, {}

    weighted_avg = sum(totals) / len(totals)

    # Per-dimension means
    dim_scores: dict[str, list[float]] = {}
    for run in runs:
        scores = run.get("scores", {})
        for dim_name, cfg_scores in scores.items():
            if target_config in cfg_scores:
                dim_scores.setdefault(dim_name, []).append(cfg_scores[target_config])

    per_dim_means = {
        dim: sum(vals) / len(vals) for dim, vals in dim_scores.items() if vals
    }

    return weighted_avg, per_dim_means


# ---------------------------------------------------------------------------
# Skill directory backup / rollback
# ---------------------------------------------------------------------------

def _snapshot_skill_dirs(
    skill_paths: list[Path],
    plugin_paths: list[Path],
    turn: int,
    backup_root: Path,
) -> Path:
    """Create a file-system backup of skill/plugin directories.

    Returns the backup directory path.
    """
    backup_dir = backup_root / f"turn-{turn}"
    backup_dir.mkdir(parents=True, exist_ok=True)

    for i, p in enumerate(skill_paths):
        if p.is_dir():
            dest = backup_dir / f"skill-{i}-{p.name}"
            shutil.copytree(p, dest, dirs_exist_ok=True)

    for i, p in enumerate(plugin_paths):
        if p.is_dir():
            dest = backup_dir / f"plugin-{i}-{p.name}"
            shutil.copytree(p, dest, dirs_exist_ok=True)

    return backup_dir


def _rollback_skill_dirs(
    skill_paths: list[Path],
    plugin_paths: list[Path],
    backup_dir: Path,
) -> None:
    """Restore skill/plugin directories from a backup."""
    for i, p in enumerate(skill_paths):
        backup_src = backup_dir / f"skill-{i}-{p.name}"
        if backup_src.is_dir() and p.is_dir():
            shutil.rmtree(p)
            shutil.copytree(backup_src, p)

    for i, p in enumerate(plugin_paths):
        backup_src = backup_dir / f"plugin-{i}-{p.name}"
        if backup_src.is_dir() and p.is_dir():
            shutil.rmtree(p)
            shutil.copytree(backup_src, p)


# ---------------------------------------------------------------------------
# Applying improvements
# ---------------------------------------------------------------------------

def _apply_improvements(
    improvements_path: Path,
    skill_paths: list[Path],
    plugin_paths: list[Path],
    model: str | None,
    idle_timeout: int = 600,
) -> bool:
    """Invoke Copilot CLI to apply the improvements file to skill/plugin sources.

    Returns True if the process completed without timing out.
    """
    from skill_eval.generate import _kill_process_tree, _watchdog_wait

    all_paths = skill_paths + plugin_paths
    paths_str = "\n".join(f"  - {p}" for p in all_paths)

    prompt = (
        f"Read the improvement suggestions in the file at {improvements_path}. "
        f"Apply the suggested changes to the skill/plugin source files located at:\n"
        f"{paths_str}\n\n"
        f"Rules:\n"
        f"- Only modify files within the directories listed above.\n"
        f"- Do NOT delete any files.\n"
        f"- Do NOT create files outside those directories.\n"
        f"- Make the concrete changes described in the improvements file.\n"
        f"- If a suggestion is vague or unclear, skip it rather than guessing.\n"
        f"- After applying changes, briefly summarize what you changed."
    )

    cmd = ["copilot", "-p", prompt, "--yolo"]
    if model:
        cmd.extend(["--model", model])

    env = {**os.environ, "NODE_OPTIONS": "--max-old-space-size=8192"}
    proc = subprocess.Popen(cmd, env=env)

    timed_out = _watchdog_wait(proc, idle_timeout)
    if timed_out:
        _kill_process_tree(proc)
        return False

    return proc.returncode == 0 or proc.returncode is not None


# ---------------------------------------------------------------------------
# Stopping conditions
# ---------------------------------------------------------------------------

class StopReason:
    TARGET_REACHED = "target_score_reached"
    PLATEAU = "score_plateau"
    MAX_TURNS = "max_turns_reached"
    REGRESSION = "score_regression"
    APPLY_FAILED = "apply_failed"
    PIPELINE_FAILED = "pipeline_failed"


def _check_stop(
    history: list[dict],
    target_score: float,
    min_improvement: float,
    max_turns: int,
) -> str | None:
    """Evaluate stopping conditions. Returns a StopReason or None to continue."""
    if not history:
        return None

    latest = history[-1]
    score = latest.get("weighted_average")

    if score is not None and score >= target_score:
        return StopReason.TARGET_REACHED

    if len(history) >= max_turns:
        return StopReason.MAX_TURNS

    # Check for plateau (score delta below threshold)
    if len(history) >= 2:
        prev_score = history[-2].get("weighted_average")
        if score is not None and prev_score is not None:
            delta = score - prev_score
            if delta < min_improvement:
                return StopReason.PLATEAU

    # Check for regression (2 consecutive decreases)
    if len(history) >= 3:
        s1 = history[-3].get("weighted_average")
        s2 = history[-2].get("weighted_average")
        s3 = history[-1].get("weighted_average")
        if all(x is not None for x in (s1, s2, s3)):
            if s2 < s1 and s3 < s2:
                return StopReason.REGRESSION

    return None


# ---------------------------------------------------------------------------
# Iteration history
# ---------------------------------------------------------------------------

def _write_history(history_path: Path, config_name: str, iterations: list[dict], stop_reason: str | None) -> None:
    """Persist iteration history to JSON."""
    final_score = None
    if iterations:
        final_score = iterations[-1].get("weighted_average")

    data = {
        "configuration": config_name,
        "iterations": iterations,
        "stop_reason": stop_reason,
        "final_score": final_score,
        "timestamp": datetime.now(timezone.utc).isoformat(),
    }
    history_path.parent.mkdir(parents=True, exist_ok=True)
    history_path.write_text(json.dumps(data, indent=2), encoding="utf-8")


# ---------------------------------------------------------------------------
# Summary output
# ---------------------------------------------------------------------------

_STOP_LABELS = {
    StopReason.TARGET_REACHED: "🎯 Target score reached",
    StopReason.PLATEAU: "📊 Score plateau — improvement below threshold",
    StopReason.MAX_TURNS: "🔄 Maximum turns reached",
    StopReason.REGRESSION: "📉 Score regression — 2 consecutive decreases",
    StopReason.APPLY_FAILED: "❌ Failed to apply improvements",
    StopReason.PIPELINE_FAILED: "❌ Pipeline failed to produce scores",
}


def _format_summary_table(iterations: list[dict], stop_reason: str | None) -> list[str]:
    """Build the score progression table as a list of lines (reused for console and report)."""
    lines: list[str] = []
    lines.append(f"| Turn | Score | Delta | Status |")
    lines.append(f"|-----:|------:|------:|--------|")

    for it in iterations:
        turn = it["turn"]
        score = it.get("weighted_average")
        delta = it.get("delta")
        score_str = f"{score:.2f}" if score is not None else "—"
        delta_str = f"+{delta:.2f}" if delta is not None and delta >= 0 else (
            f"{delta:.2f}" if delta is not None else "—"
        )

        if it.get("is_final_validation"):
            status = "🏁 Final validation"
        elif it.get("improvements_applied"):
            status = "✅ Improvements applied"
        elif it == iterations[-1] and stop_reason:
            status = _STOP_LABELS.get(stop_reason, stop_reason)
        else:
            status = "—"

        lines.append(f"| {turn} | {score_str} | {delta_str} | {status} |")

    return lines


def _print_summary(iterations: list[dict], stop_reason: str | None) -> None:
    """Print a score progression table."""
    click.echo(f"\n{'=' * 60}")
    click.echo("Auto-Improve Summary")
    click.echo(f"{'=' * 60}")
    click.echo(f"\n  {'Turn':>4s}  {'Score':>8s}  {'Delta':>8s}  Status")
    click.echo(f"  {'----':>4s}  {'-----':>8s}  {'-----':>8s}  ------")

    for it in iterations:
        turn = it["turn"]
        score = it.get("weighted_average")
        delta = it.get("delta")
        score_str = f"{score:.1f}" if score is not None else "—"
        delta_str = f"+{delta:.1f}" if delta is not None and delta >= 0 else (
            f"{delta:.1f}" if delta is not None else "—"
        )

        if it.get("is_final_validation"):
            status = "🏁 Final validation"
        elif it.get("improvements_applied"):
            status = "✅ Improvements applied"
        elif it == iterations[-1] and stop_reason:
            status = _STOP_LABELS.get(stop_reason, stop_reason)
        else:
            status = "—"

        click.echo(f"  {turn:>4}  {score_str:>8s}  {delta_str:>8s}  {status}")

    if stop_reason:
        click.echo(f"\n  Result: {_STOP_LABELS.get(stop_reason, stop_reason)}")

    if iterations:
        first_score = iterations[0].get("weighted_average")
        last_score = iterations[-1].get("weighted_average")
        if first_score is not None and last_score is not None:
            total_delta = last_score - first_score
            sign = "+" if total_delta >= 0 else ""
            click.echo(f"  Total improvement: {sign}{total_delta:.1f} ({first_score:.1f} → {last_score:.1f})")


# ---------------------------------------------------------------------------
# Results report (Markdown)
# ---------------------------------------------------------------------------

def _write_results_report(
    report_path: Path,
    config_name: str,
    iterations: list[dict],
    stop_reason: str | None,
    total_seconds: float,
    settings: dict,
) -> None:
    """Write a detailed auto-improve results report in Markdown.

    Includes the score progression table, per-dimension analysis showing
    which dimensions improved or regressed, and run settings.
    """
    lines: list[str] = []

    # Header
    lines.append("# Auto-Improve Results")
    lines.append("")
    lines.append(f"**Configuration:** {config_name}  ")
    lines.append(f"**Date:** {datetime.now(timezone.utc).strftime('%Y-%m-%d %H:%M UTC')}  ")
    mins, secs = divmod(int(total_seconds), 60)
    hrs, mins_r = divmod(mins, 60)
    time_str = f"{hrs}h {mins_r}m {secs}s" if hrs else f"{mins}m {secs}s"
    lines.append(f"**Total time:** {time_str}  ")
    lines.append(f"**Iterations:** {len([i for i in iterations if not i.get('is_final_validation')])}  ")
    result_label = _STOP_LABELS.get(stop_reason, stop_reason) if stop_reason else "In progress"
    lines.append(f"**Result:** {result_label}  ")
    lines.append("")

    # Overall score change
    eval_iterations = [i for i in iterations if not i.get("is_final_validation")]
    if eval_iterations:
        first = eval_iterations[0].get("weighted_average")
        last = eval_iterations[-1].get("weighted_average")
        if first is not None and last is not None:
            delta = last - first
            sign = "+" if delta >= 0 else ""
            lines.append("## Overall Score Change")
            lines.append("")
            lines.append(f"| Metric | Value |")
            lines.append(f"|--------|------:|")
            lines.append(f"| Starting score | {first:.2f} |")
            lines.append(f"| Final score | {last:.2f} |")
            lines.append(f"| Net change | {sign}{delta:.2f} |")
            pct = (delta / first * 100) if first != 0 else 0
            sign_pct = "+" if pct >= 0 else ""
            lines.append(f"| Percent change | {sign_pct}{pct:.1f}% |")
            lines.append("")

    # Score progression table
    lines.append("## Score Progression")
    lines.append("")
    lines.extend(_format_summary_table(iterations, stop_reason))
    lines.append("")

    # Per-dimension analysis
    _write_dimension_analysis(lines, iterations)

    # Iteration details
    _write_iteration_details(lines, iterations)

    # Settings
    lines.append("## Settings")
    lines.append("")
    lines.append("| Setting | Value |")
    lines.append("|---------|-------|")
    for key, val in settings.items():
        lines.append(f"| {key} | {val} |")
    lines.append("")

    report_path.parent.mkdir(parents=True, exist_ok=True)
    report_path.write_text("\n".join(lines), encoding="utf-8")


def _write_dimension_analysis(lines: list[str], iterations: list[dict]) -> None:
    """Analyze per-dimension score changes across iterations."""
    eval_iters = [i for i in iterations if not i.get("is_final_validation")]
    if len(eval_iters) < 1:
        return

    first_dims = eval_iters[0].get("per_dimension", {})
    last_dims = eval_iters[-1].get("per_dimension", {})

    if not first_dims and not last_dims:
        return

    all_dims = list(dict.fromkeys(list(first_dims.keys()) + list(last_dims.keys())))

    improved: list[tuple[str, float, float, float]] = []
    regressed: list[tuple[str, float, float, float]] = []
    unchanged: list[tuple[str, float, float, float]] = []

    for dim in all_dims:
        first_val = first_dims.get(dim)
        last_val = last_dims.get(dim)
        if first_val is None or last_val is None:
            continue
        delta = last_val - first_val
        entry = (dim, first_val, last_val, delta)
        if delta > 0.1:
            improved.append(entry)
        elif delta < -0.1:
            regressed.append(entry)
        else:
            unchanged.append(entry)

    lines.append("## Per-Dimension Analysis")
    lines.append("")

    if improved:
        improved.sort(key=lambda x: x[3], reverse=True)
        lines.append("### ✅ Improved Dimensions")
        lines.append("")
        lines.append("| Dimension | Start | End | Change |")
        lines.append("|-----------|------:|----:|-------:|")
        for dim, start, end, delta in improved:
            lines.append(f"| {dim} | {start:.2f} | {end:.2f} | +{delta:.2f} |")
        lines.append("")

    if regressed:
        regressed.sort(key=lambda x: x[3])
        lines.append("### ⚠️ Regressed Dimensions")
        lines.append("")
        lines.append("| Dimension | Start | End | Change |")
        lines.append("|-----------|------:|----:|-------:|")
        for dim, start, end, delta in regressed:
            lines.append(f"| {dim} | {start:.2f} | {end:.2f} | {delta:.2f} |")
        lines.append("")

    if unchanged:
        lines.append("### ➡️ Unchanged Dimensions")
        lines.append("")
        lines.append("| Dimension | Start | End | Change |")
        lines.append("|-----------|------:|----:|-------:|")
        for dim, start, end, delta in unchanged:
            sign = "+" if delta >= 0 else ""
            lines.append(f"| {dim} | {start:.2f} | {end:.2f} | {sign}{delta:.2f} |")
        lines.append("")

    if not improved and not regressed and not unchanged:
        lines.append("No per-dimension data available.")
        lines.append("")


def _write_iteration_details(lines: list[str], iterations: list[dict]) -> None:
    """Write per-iteration dimension scores as a detailed breakdown."""
    eval_iters = [i for i in iterations if not i.get("is_final_validation")]
    if len(eval_iters) < 2:
        return

    # Collect all dimension names
    all_dims: list[str] = []
    seen: set[str] = set()
    for it in eval_iters:
        for dim in it.get("per_dimension", {}):
            if dim not in seen:
                all_dims.append(dim)
                seen.add(dim)

    if not all_dims:
        return

    lines.append("## Per-Iteration Dimension Scores")
    lines.append("")

    # Build header
    turn_headers = " | ".join(f"Turn {it['turn']}" for it in eval_iters)
    lines.append(f"| Dimension | {turn_headers} |")
    separator = " | ".join("-----:" for _ in eval_iters)
    lines.append(f"|-----------|{separator}|")

    for dim in all_dims:
        vals = []
        for it in eval_iters:
            v = it.get("per_dimension", {}).get(dim)
            vals.append(f"{v:.2f}" if v is not None else "—")
        lines.append(f"| {dim} | {' | '.join(vals)} |")

    lines.append("")


# ---------------------------------------------------------------------------
# Clean previous run outputs
# ---------------------------------------------------------------------------

def _clean_previous_outputs(config: EvalConfig, project_root: Path) -> None:
    """Remove output and per-run analysis files from a previous iteration.

    This ensures the pipeline produces fresh results rather than reusing
    stale data from the prior turn.
    """
    output_dir = project_root / config.output.directory
    reports_dir = project_root / config.output.reports_directory

    # Remove generated output directories
    if output_dir.exists():
        for child in output_dir.iterdir():
            if child.is_dir():
                shutil.rmtree(child, ignore_errors=True)

    # Remove per-run analysis files and aggregated analysis
    if reports_dir.exists():
        for f in reports_dir.iterdir():
            if f.name.startswith("analysis-run-") or f.name in (
                config.output.analysis_file,
                config.output.scores_data_file,
                "generation-usage.json",
            ):
                f.unlink(missing_ok=True)


# ---------------------------------------------------------------------------
# Main loop
# ---------------------------------------------------------------------------

def run_auto_improve(
    config: EvalConfig,
    project_root: Path,
    resolver: SourceResolver,
    target_config_name: str,
    *,
    max_turns: int = 5,
    target_score: float = 9.0,
    min_improvement: float = 0.5,
    runs_per_iteration: int = 1,
    final_runs: int | None = None,
    generation_model: str | None = None,
    analysis_model: str | None = None,
    improvement_model: str | None = None,
    no_rollback: bool = False,
) -> None:
    """Run the autonomous improvement loop."""
    from skill_eval.analyze import run_analyze
    from skill_eval.generate import run_generate
    from skill_eval.suggest_improvements import run_suggest_improvements
    from skill_eval.verify import run_verify

    # Validate the target configuration exists and has suggest_improvements: true
    target_cfg: Configuration | None = None
    for c in config.configurations:
        if c.name == target_config_name:
            target_cfg = c
            break

    if target_cfg is None:
        available = ", ".join(c.name for c in config.configurations)
        raise click.ClickException(
            f"Configuration '{target_config_name}' not found.\n"
            f"Available: {available}"
        )

    if not target_cfg.suggest_improvements:
        raise click.ClickException(
            f"Configuration '{target_config_name}' does not have "
            f"suggest_improvements: true in eval.yaml. "
            f"Set it to true to use auto-improve."
        )

    # Resolve skill/plugin paths for the target configuration
    skill_paths = [resolver.resolve(ref) for ref in target_cfg.skills]
    plugin_paths = [resolver.resolve(ref) for ref in target_cfg.plugins]

    if not skill_paths and not plugin_paths:
        raise click.ClickException(
            f"Configuration '{target_config_name}' has no skills or plugins to improve."
        )

    # Apply model overrides
    if generation_model:
        config.generation_model = generation_model
    if analysis_model:
        config.analysis_model = analysis_model
    if improvement_model:
        config.improvement_model = improvement_model

    reports_dir = project_root / config.output.reports_directory
    reports_dir.mkdir(parents=True, exist_ok=True)
    scores_path = reports_dir / config.output.scores_data_file
    history_path = reports_dir / "auto-improve-history.json"
    backup_root = project_root / ".auto-improve-backups"

    imp_model = improvement_model or config.effective_improvement_model

    click.echo(f"\n{'=' * 60}")
    click.echo("Auto-Improve Loop")
    click.echo(f"{'=' * 60}")
    click.echo(f"  Configuration:     {target_config_name}")
    click.echo(f"  Max turns:         {max_turns}")
    click.echo(f"  Target score:      {target_score}")
    click.echo(f"  Min improvement:   {min_improvement}")
    click.echo(f"  Runs/iteration:    {runs_per_iteration}")
    click.echo(f"  Generation model:  {config.generation_model}")
    click.echo(f"  Analysis model:    {config.analysis_model}")
    click.echo(f"  Improvement model: {imp_model}")
    click.echo(f"  Skill paths:       {', '.join(str(p) for p in skill_paths)}")
    click.echo(f"  Plugin paths:      {', '.join(str(p) for p in plugin_paths) or '(none)'}")

    iterations: list[dict] = []
    stop_reason: str | None = None

    pipeline_start = time.monotonic()

    for turn in range(1, max_turns + 1):
        turn_start = time.monotonic()
        click.echo(f"\n{'─' * 60}")
        click.echo(f"  Turn {turn}/{max_turns}")
        click.echo(f"{'─' * 60}")

        # Clean outputs from previous iteration
        if turn > 1:
            click.echo("  🧹 Cleaning previous outputs...")
            _clean_previous_outputs(config, project_root)

        # --- Step 1: Run the pipeline ---
        config.runs = runs_per_iteration

        click.echo(f"\n  📦 Generating code ({runs_per_iteration} run(s))...")
        try:
            run_generate(config, project_root, resume=False, resolver=resolver)
        except Exception as e:
            click.echo(f"  ❌ Generate failed: {e}")
            stop_reason = StopReason.PIPELINE_FAILED
            break

        if config.verification is not None:
            click.echo("  🔨 Verifying builds...")
            try:
                run_verify(config, project_root)
            except Exception as e:
                click.echo(f"  ⚠️  Verify failed: {e}")

        click.echo("  📊 Running analysis...")
        try:
            run_analyze(config, project_root)
        except Exception as e:
            click.echo(f"  ❌ Analysis failed: {e}")
            stop_reason = StopReason.PIPELINE_FAILED
            break

        # --- Step 2: Read scores ---
        weighted_avg, per_dim = _read_weighted_average(scores_path, target_config_name)

        if weighted_avg is None:
            click.echo("  ❌ Could not parse scores from scores-data.json")
            stop_reason = StopReason.PIPELINE_FAILED
            break

        delta = None
        if iterations:
            prev = iterations[-1].get("weighted_average")
            if prev is not None:
                delta = weighted_avg - prev

        turn_elapsed = time.monotonic() - turn_start
        mins, secs = divmod(int(turn_elapsed), 60)

        iteration_record = {
            "turn": turn,
            "weighted_average": round(weighted_avg, 2),
            "delta": round(delta, 2) if delta is not None else None,
            "per_dimension": {k: round(v, 2) for k, v in per_dim.items()},
            "timestamp": datetime.now(timezone.utc).isoformat(),
            "elapsed_seconds": round(turn_elapsed, 1),
            "improvements_applied": False,
        }

        delta_str = f" (delta: {'+' if delta >= 0 else ''}{delta:.2f})" if delta is not None else ""
        click.echo(f"  📈 Score: {weighted_avg:.2f}{delta_str}  [{mins}m {secs}s]")

        iterations.append(iteration_record)

        # --- Step 3: Check stopping conditions ---
        stop_reason = _check_stop(iterations, target_score, min_improvement, max_turns)
        if stop_reason:
            _write_history(history_path, target_config_name, iterations, stop_reason)
            break

        # --- Step 4: Handle rollback on regression ---
        if not no_rollback and delta is not None and delta < 0 and len(iterations) >= 2:
            prev_backup = backup_root / f"turn-{turn - 1}"
            if prev_backup.exists():
                click.echo(f"  ⚠️  Score decreased — rolling back skill changes from turn {turn - 1}")
                _rollback_skill_dirs(skill_paths, plugin_paths, prev_backup)

        # --- Step 5: Generate improvement suggestions ---
        click.echo("  💡 Generating improvement suggestions...")
        try:
            run_suggest_improvements(
                config, project_root, resolver, model_override=imp_model
            )
        except Exception as e:
            click.echo(f"  ❌ Improvement suggestions failed: {e}")
            stop_reason = StopReason.APPLY_FAILED
            _write_history(history_path, target_config_name, iterations, stop_reason)
            break

        improvements_file = config.output.improvements_file_pattern.format(
            config=target_config_name
        )
        improvements_path = reports_dir / improvements_file

        if not improvements_path.exists():
            click.echo(f"  ❌ Improvements file not generated: {improvements_path}")
            stop_reason = StopReason.APPLY_FAILED
            _write_history(history_path, target_config_name, iterations, stop_reason)
            break

        # --- Step 6: Snapshot and apply improvements ---
        click.echo(f"  💾 Backing up skill/plugin files (turn {turn})...")
        _snapshot_skill_dirs(skill_paths, plugin_paths, turn, backup_root)

        click.echo("  🔧 Applying improvements via Copilot CLI...")
        apply_success = _apply_improvements(
            improvements_path, skill_paths, plugin_paths, model=imp_model
        )

        if not apply_success:
            click.echo("  ❌ Copilot CLI failed or timed out applying improvements")
            stop_reason = StopReason.APPLY_FAILED
            _write_history(history_path, target_config_name, iterations, stop_reason)
            break

        iteration_record["improvements_applied"] = True
        click.echo("  ✅ Improvements applied")

        _write_history(history_path, target_config_name, iterations, stop_reason)

    # --- Final validation pass (optional) ---
    if final_runs and final_runs > runs_per_iteration and stop_reason in (
        StopReason.TARGET_REACHED, StopReason.PLATEAU, StopReason.MAX_TURNS, None
    ):
        click.echo(f"\n{'─' * 60}")
        click.echo(f"  Final validation pass ({final_runs} runs)")
        click.echo(f"{'─' * 60}")

        _clean_previous_outputs(config, project_root)
        config.runs = final_runs

        try:
            run_generate(config, project_root, resume=False, resolver=resolver)
            if config.verification is not None:
                run_verify(config, project_root)
            run_analyze(config, project_root)

            final_avg, final_dim = _read_weighted_average(scores_path, target_config_name)
            if final_avg is not None:
                click.echo(f"  📈 Final validated score: {final_avg:.2f}")
                iterations.append({
                    "turn": "final",
                    "weighted_average": round(final_avg, 2),
                    "delta": None,
                    "per_dimension": {k: round(v, 2) for k, v in final_dim.items()},
                    "timestamp": datetime.now(timezone.utc).isoformat(),
                    "improvements_applied": False,
                    "is_final_validation": True,
                })
        except Exception as e:
            click.echo(f"  ⚠️  Final validation failed: {e}")

    total_time = time.monotonic() - pipeline_start
    mins, secs = divmod(int(total_time), 60)
    hrs, mins = divmod(mins, 60)

    _write_history(history_path, target_config_name, iterations, stop_reason)
    _print_summary(iterations, stop_reason)

    # Write the Markdown results report
    results_path = reports_dir / "auto-improve-results.md"
    _write_results_report(
        results_path,
        target_config_name,
        iterations,
        stop_reason,
        total_time,
        settings={
            "Max turns": max_turns,
            "Target score": target_score,
            "Min improvement": min_improvement,
            "Runs per iteration": runs_per_iteration,
            "Final runs": final_runs or runs_per_iteration,
            "Generation model": config.generation_model,
            "Analysis model": config.analysis_model,
            "Improvement model": imp_model,
            "Rollback enabled": not no_rollback,
        },
    )
    click.echo(f"\n  📄 Results report: {results_path}")

    if hrs:
        click.echo(f"  Total time: {hrs}h {mins}m {secs}s")
    else:
        click.echo(f"  Total time: {mins}m {secs}s")

    click.echo(f"  History: {history_path}")

    # Cleanup backups
    if backup_root.exists():
        click.echo(f"  Backups: {backup_root}")
