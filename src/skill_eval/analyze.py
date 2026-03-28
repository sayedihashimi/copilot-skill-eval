"""Run comparative analysis by generating an analysis prompt and invoking Copilot."""

from __future__ import annotations

import copy
import random
import subprocess
from pathlib import Path

import click

from skill_eval.config import EvalConfig
from skill_eval.prompt_renderer import render_analyze_prompt


def run_analyze(config: EvalConfig, project_root: Path) -> None:
    """Run per-run analysis and then aggregate results.

    For each run, invokes Copilot CLI to produce analysis-run-{N}.md.
    Then parses scores and writes the aggregated analysis.md.
    """
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

    # Phase 1: Per-run analysis
    for run_id in range(1, num_runs + 1):
        run_analysis_file = config.output.per_run_analysis_pattern.format(run=run_id)
        run_analysis_path = reports_dir / run_analysis_file

        # Check if run output exists
        has_run_output = False
        for cfg in config.configurations:
            run_dir = output_dir / cfg.name / f"run-{run_id}"
            if run_dir.exists():
                has_run_output = True
                break

        if not has_run_output:
            click.echo(f"\n  ⚠️  Run {run_id}: no output found, skipping")
            continue

        click.echo(f"\n  --- Analyzing run {run_id}/{num_runs} ---")

        # Randomize config order for bias mitigation (different seed per run)
        shuffled_config = copy.deepcopy(config)
        random.seed(run_id)
        random.shuffle(shuffled_config.configurations)
        click.echo(f"  Config order: {[c.name for c in shuffled_config.configurations]}")

        # Override the analysis output file for this run
        shuffled_config.output.analysis_file = run_analysis_file

        prompt = render_analyze_prompt(shuffled_config, project_root, run_id=run_id)
        click.echo(f"  Generated analysis prompt ({len(prompt)} chars)")
        click.echo("  Invoking Copilot CLI for analysis...")

        cmd = ["copilot", "-p", prompt, "--yolo"]
        result = subprocess.run(cmd)

        if result.returncode != 0:
            click.echo(f"  ⚠️  Copilot CLI exited with code {result.returncode} for run {run_id}")
            continue

        if run_analysis_path.exists():
            click.echo(f"  ✅ Run {run_id} analysis written to: {run_analysis_path}")
        else:
            click.echo(f"  ⚠️  Run {run_id}: Copilot finished but {run_analysis_path} not found")

    # Phase 2: Aggregate scores
    click.echo(f"\n  --- Aggregating results across {num_runs} runs ---")
    from skill_eval.aggregator import aggregate_results
    aggregate_results(config, project_root)

    analysis_path = reports_dir / config.output.analysis_file
    if analysis_path.exists():
        click.echo(f"\n  ✅ Aggregated analysis written to: {analysis_path}")
    else:
        click.echo(f"\n  ⚠️  Aggregation completed but {analysis_path} not found")
