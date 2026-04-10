"""Run comparative analysis by generating an analysis prompt and invoking Copilot."""

from __future__ import annotations

import copy
import os
import random
import subprocess
import sys
import time
from concurrent.futures import ThreadPoolExecutor, as_completed
from pathlib import Path

import click

from skill_eval.config import EvalConfig
from skill_eval.prompt_renderer import render_analyze_prompt


def _analyze_single_run(
    config: EvalConfig,
    run_id: int,
    project_root: Path,
    idle_timeout: int = 300,
) -> tuple[int, bool, dict | None]:
    """Analyze a single run. Returns (run_id, success, usage_stats).

    Includes a watchdog that kills the Copilot process if idle.
    """
    reports_dir = project_root / config.output.reports_directory
    run_analysis_file = config.output.per_run_analysis_pattern.format(run=run_id)
    run_analysis_path = reports_dir / run_analysis_file

    # Randomize config order (different seed per run)
    shuffled_config = copy.deepcopy(config)
    random.seed(run_id)
    random.shuffle(shuffled_config.configurations)
    shuffled_config.output.analysis_file = run_analysis_file

    prompt = render_analyze_prompt(shuffled_config, project_root, run_id=run_id)

    cmd = ["copilot", "-p", prompt, "--yolo"]
    if config.analysis_model:
        cmd.extend(["--model", config.analysis_model])

    # Give the Copilot CLI more memory to avoid OOM on large analyses
    env = {**os.environ, "NODE_OPTIONS": "--max-old-space-size=8192"}

    start_time = time.time()
    proc = subprocess.Popen(cmd, cwd=project_root, env=env)

    from skill_eval.generate import _watchdog_wait, _kill_process_tree, _parse_copilot_log_usage
    timed_out = _watchdog_wait(proc, idle_timeout)
    elapsed = time.time() - start_time

    if timed_out:
        _kill_process_tree(proc)
        return run_id, False, None

    # Success = the analysis file was written, regardless of exit code.
    # The Copilot CLI may exit non-zero for reasons unrelated to the
    # analysis (e.g., notification script failures, post-completion errors).
    success = run_analysis_path.exists()
    if proc.returncode != 0 and success:
        click.echo(
            f"  ⚠️  Run {run_id}: Copilot exited with code {proc.returncode} "
            f"but analysis file was written — treating as success"
        )

    usage = _parse_copilot_log_usage(proc.pid)
    if usage:
        usage["wall_time_seconds"] = round(elapsed, 1)
        usage["run_id"] = run_id
        usage["step"] = "analysis"
    return run_id, success, usage


def run_analyze(config: EvalConfig, project_root: Path, parallel: int = 2) -> None:
    """Run per-run analysis in parallel, then aggregate results."""
    reports_dir = project_root / config.output.reports_directory
    reports_dir.mkdir(parents=True, exist_ok=True)

    output_dir = project_root / config.output.directory
    if not output_dir.exists():
        raise click.ClickException(
            f"Output directory not found: {output_dir}\n"
            "Run 'skill-eval generate' first."
        )

    num_runs = config.runs

    click.echo(f"\n{'=' * 60}")
    click.echo("Running comparative analysis")
    click.echo(f"{'=' * 60}")
    click.echo(f"  Dimensions:     {len(config.dimensions)}")
    click.echo(f"  Configurations: {len(config.configurations)}")
    click.echo(f"  Scenarios:      {len(config.scenarios)}")
    click.echo(f"  Runs:           {num_runs}")
    click.echo(f"  Parallel:       {parallel}")

    # Find which runs have output
    runs_to_analyze: list[int] = []
    for run_id in range(1, num_runs + 1):
        has_output = any(
            (output_dir / cfg.name / f"run-{run_id}").exists()
            for cfg in config.configurations
        )
        if has_output:
            runs_to_analyze.append(run_id)
        else:
            click.echo(f"  ⚠️  Run {run_id}: no output found, skipping")

    if not runs_to_analyze:
        click.echo("  ❌ No runs to analyze")
        return

    # Phase 1: Per-run analysis (sequential, with retries)
    click.echo(f"\n  Analyzing {len(runs_to_analyze)} runs...")

    max_retries = 2
    analysis_usage: list[dict] = []
    for run_id in runs_to_analyze:
        click.echo(f"\n  --- Analyzing run {run_id}/{num_runs} ---")
        succeeded = False
        for attempt in range(1, max_retries + 1):
            if attempt > 1:
                click.echo(f"  🔄 Retry {attempt}/{max_retries} for run {run_id}")
                # Remove partial output from previous attempt
                partial = reports_dir / config.output.per_run_analysis_pattern.format(run=run_id)
                if partial.exists():
                    partial.unlink()
            try:
                _, success, usage = _analyze_single_run(config, run_id, project_root)
                if success:
                    if usage:
                        mins, secs = divmod(int(usage.get("wall_time_seconds", 0)), 60)
                        click.echo(
                            f"  ✅ Run {run_id}: "
                            f"{usage.get('input_tokens', 0):,} in / "
                            f"{usage.get('output_tokens', 0):,} out / "
                            f"{mins}m {secs}s"
                        )
                        analysis_usage.append(usage)
                    else:
                        click.echo(f"  ✅ Run {run_id} analysis complete")
                    succeeded = True
                    break
                else:
                    click.echo(f"  ⚠️  Run {run_id} analysis failed or timed out")
            except Exception as e:
                click.echo(f"  ❌ Run {run_id} error: {e}")
        if not succeeded:
            click.echo(f"  ❌ Run {run_id} failed after {max_retries} attempts")

    # Phase 2: Aggregate scores (fast, Python-only)
    click.echo(f"\n  --- Aggregating results ---")
    from skill_eval.aggregator import aggregate_results
    aggregate_results(config, project_root)

    analysis_path = reports_dir / config.output.analysis_file
    if analysis_path.exists():
        click.echo(f"\n  ✅ Aggregated analysis written to: {analysis_path}")
    else:
        click.echo(f"\n  ⚠️  Aggregation completed but {analysis_path} not found")
