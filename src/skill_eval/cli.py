"""CLI entry point for the Copilot Skill Evaluation Framework."""

from __future__ import annotations

from pathlib import Path

import click

from skill_eval.config import load_config


@click.group()
@click.option(
    "--config",
    "config_path",
    type=click.Path(exists=False, path_type=Path),
    default=None,
    help="Path to eval.yaml (default: ./eval.yaml)",
)
@click.option(
    "--project-root",
    type=click.Path(exists=True, file_okay=False, path_type=Path),
    default=None,
    help="Project root directory (default: current directory)",
)
@click.pass_context
def main(ctx: click.Context, config_path: Path | None, project_root: Path | None) -> None:
    """Copilot Skill Evaluation Framework.

    Evaluate how GitHub Copilot custom skills impact code generation quality.
    Generate the same apps under different skill configurations, then produce
    a comparative analysis report.

    Use 'skill-eval init' to set up a new evaluation project.
    """
    ctx.ensure_object(dict)
    ctx.obj["project_root"] = project_root or Path.cwd()
    ctx.obj["config_path"] = config_path


@main.command()
@click.pass_context
def init(ctx: click.Context) -> None:
    """Initialize a new skill evaluation project.

    Interactively creates eval.yaml, scenario prompt files, and directory structure.
    """
    from skill_eval.init_cmd import run_init

    run_init(ctx.obj["project_root"])


@main.command()
@click.option(
    "--configurations", "-c",
    multiple=True,
    help="Only generate these configurations (can be repeated).",
)
@click.option("--runs", "-r", type=int, default=None,
              help="Number of generation runs per configuration (overrides eval.yaml)")
@click.option("--resume", is_flag=True, help="Skip runs where output already exists.")
@click.pass_context
def generate(ctx: click.Context, configurations: tuple[str, ...], runs: int | None, resume: bool) -> None:
    """Generate code using Copilot CLI for each skill configuration.

    Invokes the Copilot CLI once per configuration per run, each time with
    different skills registered. Output goes to output/{config}/run-{N}/.
    """
    from skill_eval.generate import run_generate

    config = _load(ctx)
    if runs is not None:
        config.runs = runs
    click.echo(f"  Runs per configuration: {config.runs}")
    run_generate(
        config,
        ctx.obj["project_root"],
        configurations=list(configurations) if configurations else None,
        resume=resume,
    )


@main.command()
@click.pass_context
def verify(ctx: click.Context) -> None:
    """Verify generated projects build and run.

    Runs the configured build command (and optionally run command) for every
    generated project. Results are written to reports/build-notes.md.
    """
    from skill_eval.verify import run_verify

    config = _load(ctx)
    run_verify(config, ctx.obj["project_root"])


@main.command()
@click.option("--model", "-m", type=str, default=None,
              help="AI model for analysis (overrides eval.yaml analysis_model).")
@click.pass_context
def analyze(ctx: click.Context, model: str | None) -> None:
    """Run comparative analysis across all configurations.

    Generates an analysis prompt from the configured dimensions and invokes
    Copilot CLI to produce reports/analysis.md.
    """
    from skill_eval.analyze import run_analyze

    config = _load(ctx)
    if model is not None:
        config.analysis_model = model
    run_analyze(config, ctx.obj["project_root"])


@main.command()
@click.option("--skip-generate", is_flag=True, help="Skip the generation step.")
@click.option("--skip-verify", is_flag=True, help="Skip the verification step.")
@click.option("--analyze-only", is_flag=True, help="Only run the analysis step.")
@click.option(
    "--configurations", "-c",
    multiple=True,
    help="Only generate these configurations (can be repeated).",
)
@click.option("--runs", "-r", type=int, default=None,
              help="Number of generation runs per configuration (overrides eval.yaml)")
@click.option("--resume", is_flag=True,
              help="Skip runs where output already exists.")
@click.option("--model", "-m", type=str, default=None,
              help="AI model for analysis (overrides eval.yaml analysis_model).")
@click.pass_context
def run(
    ctx: click.Context,
    skip_generate: bool,
    skip_verify: bool,
    analyze_only: bool,
    configurations: tuple[str, ...],
    runs: int | None,
    resume: bool,
    model: str | None,
) -> None:
    """Run the full evaluation pipeline: generate → verify → analyze.

    This is the main command that runs the entire evaluation. Use flags
    to skip individual steps.
    """
    import time as _time

    from skill_eval.analyze import run_analyze
    from skill_eval.generate import run_generate
    from skill_eval.verify import run_verify

    config = _load(ctx)
    if runs is not None:
        config.runs = runs
    if model is not None:
        config.analysis_model = model
    click.echo(f"  Runs per configuration: {config.runs}")
    project_root = ctx.obj["project_root"]
    timings: dict[str, float] = {}
    pipeline_start = _time.monotonic()

    if analyze_only:
        skip_generate = True
        skip_verify = True

    if not skip_generate:
        click.echo("\n📦 Step 1/3: Generating code...")
        t0 = _time.monotonic()
        run_generate(
            config,
            project_root,
            configurations=list(configurations) if configurations else None,
            resume=resume,
        )
        timings["generate"] = _time.monotonic() - t0
    else:
        click.echo("\n⏭️  Step 1/3: Generate — skipped")

    if not skip_verify:
        click.echo("\n🔨 Step 2/3: Verifying builds...")
        t0 = _time.monotonic()
        run_verify(config, project_root)
        timings["verify"] = _time.monotonic() - t0
    else:
        click.echo("\n⏭️  Step 2/3: Verify — skipped")

    click.echo("\n📊 Step 3/3: Running analysis...")
    t0 = _time.monotonic()
    run_analyze(config, project_root)
    timings["analyze"] = _time.monotonic() - t0

    total_time = _time.monotonic() - pipeline_start

    click.echo("\n🎉 Evaluation complete!")
    click.echo(f"   Report: {config.output.reports_directory}/{config.output.analysis_file}")
    click.echo(f"\n⏱️  Pipeline timing:")
    for step, elapsed in timings.items():
        mins, secs = divmod(int(elapsed), 60)
        hrs, mins = divmod(mins, 60)
        if hrs:
            click.echo(f"   {step:12s}: {hrs}h {mins}m {secs}s")
        elif mins:
            click.echo(f"   {step:12s}: {mins}m {secs}s")
        else:
            click.echo(f"   {step:12s}: {secs}s")
    mins, secs = divmod(int(total_time), 60)
    hrs, mins = divmod(mins, 60)
    click.echo(f"   {'total':12s}: {hrs}h {mins}m {secs}s" if hrs else f"   {'total':12s}: {mins}m {secs}s")


@main.command()
@click.pass_context
def validate_config(ctx: click.Context) -> None:
    """Validate eval.yaml without running anything.

    Checks that the configuration file is valid, all referenced prompt files
    exist, and skill directories are accessible.
    """
    config = _load(ctx)
    project_root = ctx.obj["project_root"]

    errors = []

    # Check scenario prompt files exist
    for s in config.scenarios:
        prompt_path = project_root / s.prompt
        if not prompt_path.exists():
            errors.append(f"Scenario prompt not found: {s.prompt}")

    # Check skill directories exist
    for c in config.configurations:
        for skill in c.skills:
            skill_path = project_root / skill
            if not skill_path.exists():
                errors.append(f"Skill directory not found: {skill} (config: {c.name})")
        for plugin in c.plugins:
            plugin_path = project_root / plugin
            if not plugin_path.exists():
                errors.append(f"Plugin directory not found: {plugin} (config: {c.name})")

    if errors:
        click.echo("❌ Validation failed:\n")
        for e in errors:
            click.echo(f"   • {e}")
        raise SystemExit(1)

    click.echo("✅ Configuration is valid!")
    click.echo(f"   Name:           {config.name}")
    click.echo(f"   Scenarios:      {len(config.scenarios)}")
    click.echo(f"   Configurations: {len(config.configurations)}")
    click.echo(f"   Dimensions:     {len(config.dimensions)}")


def _load(ctx: click.Context):
    """Load config, using the path from context."""
    return load_config(ctx.obj.get("config_path"))
