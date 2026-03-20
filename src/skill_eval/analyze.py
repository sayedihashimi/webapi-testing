"""Run comparative analysis by generating an analysis prompt and invoking Copilot."""

from __future__ import annotations

import subprocess
from pathlib import Path

import click

from skill_eval.config import EvalConfig
from skill_eval.prompt_renderer import render_analyze_prompt


def run_analyze(config: EvalConfig, project_root: Path) -> None:
    """Generate an analysis prompt from dimensions and run Copilot to produce the report."""
    reports_dir = project_root / config.output.reports_directory
    reports_dir.mkdir(parents=True, exist_ok=True)

    output_dir = project_root / config.output.directory
    if not output_dir.exists():
        raise click.ClickException(
            f"Output directory not found: {output_dir}\n"
            "Run 'skill-eval generate' first."
        )

    click.echo(f"\n{'=' * 60}")
    click.echo("Running comparative analysis")
    click.echo(f"{'=' * 60}")
    click.echo(f"  Dimensions:     {len(config.dimensions)}")
    click.echo(f"  Configurations: {len(config.configurations)}")
    click.echo(f"  Scenarios:      {len(config.scenarios)}")

    # Render the analysis prompt from the template
    prompt = render_analyze_prompt(config, project_root)

    click.echo(f"\n  Generated analysis prompt ({len(prompt)} chars)")
    click.echo("  Invoking Copilot CLI for analysis...")

    cmd = ["copilot", "-p", prompt, "--yolo"]
    result = subprocess.run(cmd)

    if result.returncode != 0:
        raise click.ClickException(
            f"Copilot CLI exited with code {result.returncode} during analysis"
        )

    analysis_path = reports_dir / config.output.analysis_file
    if analysis_path.exists():
        click.echo(f"\n  ✅ Analysis report written to: {analysis_path}")
    else:
        click.echo(
            f"\n  ⚠️  Copilot finished but {analysis_path} was not found. "
            "Check the Copilot output above."
        )
