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
@click.option(
    "--skill-sources", "-s",
    "skill_sources_path",
    type=click.Path(exists=False, path_type=Path),
    default=None,
    help="Path to skill-sources.yaml (default: ./skill-sources.yaml)",
)
@click.option(
    "--cache-dir",
    type=click.Path(file_okay=False, path_type=Path),
    default=None,
    help="Cache directory for cloned skill repos (default: ~/.skill-eval/cache/)",
)
@click.option(
    "--output-dir",
    type=click.Path(file_okay=False, path_type=Path),
    default=None,
    help="Output directory for generated code (overrides eval.yaml)",
)
@click.option(
    "--reports-dir",
    type=click.Path(file_okay=False, path_type=Path),
    default=None,
    help="Reports directory (overrides eval.yaml)",
)
@click.pass_context
def main(
    ctx: click.Context,
    config_path: Path | None,
    project_root: Path | None,
    skill_sources_path: Path | None,
    cache_dir: Path | None,
    output_dir: Path | None,
    reports_dir: Path | None,
) -> None:
    """Copilot Skill Evaluation Framework.

    Evaluate how GitHub Copilot custom skills impact code generation quality.
    Generate the same apps under different skill configurations, then produce
    a comparative analysis report.

    Use 'skill-eval init' to set up a new evaluation project.
    """
    ctx.ensure_object(dict)
    ctx.obj["project_root"] = project_root or Path.cwd()
    ctx.obj["config_path"] = config_path
    ctx.obj["skill_sources_path"] = skill_sources_path
    ctx.obj["cache_dir"] = cache_dir
    ctx.obj["output_dir"] = output_dir
    ctx.obj["reports_dir"] = reports_dir


@main.command()
@click.pass_context
def init(ctx: click.Context) -> None:
    """Initialize a new skill evaluation project.

    Interactively creates eval.yaml, scenario prompt files, and directory structure.
    """
    from skill_eval.init_cmd import run_init

    run_init(ctx.obj["project_root"])


@main.command("ci-setup")
@click.option("--runs-on", default="ubuntu-latest",
              help="GitHub Actions runner (default: ubuntu-latest)")
@click.option("--python-version", default="3.12",
              help="Python version for the CI job (default: 3.12)")
@click.option("--schedule", default=None,
              help="Cron schedule for automatic runs (e.g., '0 6 * * 1' for weekly)")
@click.option("--timeout", "timeout_minutes", type=int, default=120,
              help="Job timeout in minutes (default: 120)")
@click.pass_context
def ci_setup(
    ctx: click.Context,
    runs_on: str,
    python_version: str,
    schedule: str | None,
    timeout_minutes: int,
) -> None:
    """Generate a GitHub Actions workflow for running evaluations in CI.

    Creates .github/workflows/skill-eval.yml configured for this project.
    """
    from jinja2 import Environment, FileSystemLoader

    from skill_eval.prompt_renderer import _PACKAGE_TEMPLATES, _REPO_TEMPLATES

    project_root: Path = ctx.obj["project_root"]
    config = _load(ctx)

    # Find template directory
    template_dir = _PACKAGE_TEMPLATES if _PACKAGE_TEMPLATES.is_dir() else _REPO_TEMPLATES
    env = Environment(
        loader=FileSystemLoader(str(template_dir)),
        keep_trailing_newline=True,
    )
    template = env.get_template("ci-workflow.yml.j2")

    # Determine config paths relative to project root
    config_path = ctx.obj.get("config_path")
    config_rel = str(config_path) if config_path else "eval.yaml"

    skill_sources_path = ctx.obj.get("skill_sources_path")
    skill_sources_rel = None
    if skill_sources_path:
        skill_sources_rel = str(skill_sources_path)
    elif (project_root / "skill-sources.yaml").exists():
        skill_sources_rel = "skill-sources.yaml"

    rendered = template.render(
        config_path=config_rel,
        skill_sources_path=skill_sources_rel,
        runs_on=runs_on,
        python_version=python_version,
        schedule=schedule,
        timeout_minutes=timeout_minutes,
        reports_directory=config.output.reports_directory,
        output_directory=config.output.directory,
    )

    workflow_dir = project_root / ".github" / "workflows"
    workflow_dir.mkdir(parents=True, exist_ok=True)
    workflow_path = workflow_dir / "skill-eval.yml"
    workflow_path.write_text(rendered, encoding="utf-8")

    click.echo(f"✅ GitHub Actions workflow created: {workflow_path}")
    click.echo(f"   Runner:  {runs_on}")
    click.echo(f"   Python:  {python_version}")
    click.echo(f"   Timeout: {timeout_minutes} minutes")
    if schedule:
        click.echo(f"   Schedule: {schedule}")
    click.echo("\nTo run manually: go to Actions → Skill Evaluation → Run workflow")


@main.command()
@click.option(
    "--configurations", "-c",
    multiple=True,
    help="Only generate these configurations (can be repeated).",
)
@click.option("--runs", "-r", type=int, default=None,
              help="Number of generation runs per configuration (overrides eval.yaml)")
@click.option("--resume", is_flag=True, help="Skip runs where output already exists.")
@click.option("--generation-model", type=str, default=None,
              help="AI model for code generation (overrides eval.yaml generation_model).")
@click.pass_context
def generate(ctx: click.Context, configurations: tuple[str, ...], runs: int | None, resume: bool, generation_model: str | None) -> None:
    """Generate code using Copilot CLI for each skill configuration.

    Invokes the Copilot CLI once per configuration per run, each time with
    different skills registered. Output goes to output/{config}/run-{N}/.
    """
    from skill_eval.generate import run_generate

    config = _load(ctx)
    resolver = _build_resolver(ctx)
    if runs is not None:
        config.runs = runs
    if generation_model is not None:
        config.generation_model = generation_model
    click.echo(f"  Runs per configuration: {config.runs}")
    click.echo(f"  Generation model: {config.generation_model}")
    run_generate(
        config,
        ctx.obj["project_root"],
        configurations=list(configurations) if configurations else None,
        resume=resume,
        resolver=resolver,
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
@click.option("--analysis-model", "-m", type=str, default=None,
              help="AI model for analysis (overrides eval.yaml analysis_model).")
@click.pass_context
def analyze(ctx: click.Context, analysis_model: str | None) -> None:
    """Run comparative analysis across all configurations.

    Generates an analysis prompt from the configured dimensions and invokes
    Copilot CLI to produce reports/analysis.md.
    """
    from skill_eval.analyze import run_analyze

    config = _load(ctx)
    if analysis_model is not None:
        config.analysis_model = analysis_model
    run_analyze(config, ctx.obj["project_root"])


@main.command("suggest-improvements")
@click.option("--model", "-m", type=str, default=None,
              help="AI model for improvement suggestions (overrides eval.yaml improvement_model).")
@click.option("--research-dir", type=click.Path(exists=True, file_okay=False, path_type=Path),
              default=None,
              help="Directory containing best-practice research files to include in the prompt.")
@click.pass_context
def suggest_improvements(ctx: click.Context, model: str | None, research_dir: Path | None) -> None:
    """Generate improvement suggestions for skills/plugins.

    Analyzes evaluation results and skill/plugin source files to produce
    actionable improvement suggestions for configurations marked with
    suggest_improvements: true in eval.yaml.

    Includes plugin structure analysis and best-practices reference automatically.
    Use --research-dir to include additional domain-specific research.

    Requires that the analysis step has already been run.
    """
    from skill_eval.suggest_improvements import run_suggest_improvements

    config = _load(ctx)
    resolver = _build_resolver(ctx)
    # Resolve research dir from CLI option, config, or None
    effective_research_dir = research_dir
    if effective_research_dir is None and config.research_dir:
        effective_research_dir = Path(config.research_dir)
    run_suggest_improvements(
        config, ctx.obj["project_root"], resolver,
        model_override=model,
        research_dir=effective_research_dir,
    )


@main.command()
@click.option("--skip-generate", is_flag=True, help="Skip the generation step.")
@click.option("--skip-verify", is_flag=True, help="Skip the verification step.")
@click.option("--skip-improvements", is_flag=True, help="Skip the improvement suggestions step.")
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
@click.option("--generation-model", type=str, default=None,
              help="AI model for code generation (overrides eval.yaml generation_model).")
@click.option("--analysis-model", "-m", type=str, default=None,
              help="AI model for analysis (overrides eval.yaml analysis_model).")
@click.option("--improvement-model", type=str, default=None,
              help="AI model for improvement suggestions (overrides eval.yaml improvement_model).")
@click.option("--research-dir", type=click.Path(exists=True, file_okay=False, path_type=Path),
              default=None,
              help="Directory containing best-practice research files to include in improvement prompts.")
@click.pass_context
def run(
    ctx: click.Context,
    skip_generate: bool,
    skip_verify: bool,
    skip_improvements: bool,
    analyze_only: bool,
    configurations: tuple[str, ...],
    runs: int | None,
    resume: bool,
    generation_model: str | None,
    analysis_model: str | None,
    improvement_model: str | None,
    research_dir: Path | None,
) -> None:
    """Run the full evaluation pipeline: generate → verify → analyze → suggest improvements.

    This is the main command that runs the entire evaluation. Use flags
    to skip individual steps.
    """
    import time as _time

    from skill_eval.analyze import run_analyze
    from skill_eval.generate import run_generate
    from skill_eval.suggest_improvements import run_suggest_improvements
    from skill_eval.verify import run_verify

    config = _load(ctx)
    resolver = _build_resolver(ctx)
    if runs is not None:
        config.runs = runs
    if generation_model is not None:
        config.generation_model = generation_model
    if analysis_model is not None:
        config.analysis_model = analysis_model
    if improvement_model is not None:
        config.improvement_model = improvement_model
    click.echo(f"  Runs per configuration: {config.runs}")
    click.echo(f"  Generation model: {config.generation_model}")
    click.echo(f"  Analysis model:   {config.analysis_model}")
    project_root = ctx.obj["project_root"]
    timings: dict[str, float] = {}
    has_improvement_targets = bool(config.improvement_targets)
    total_steps = 3 + (1 if has_improvement_targets and not skip_improvements else 0)
    pipeline_start = _time.monotonic()

    if analyze_only:
        skip_generate = True
        skip_verify = True

    if not skip_generate:
        click.echo(f"\n📦 Step 1/{total_steps}: Generating code...")
        t0 = _time.monotonic()
        run_generate(
            config,
            project_root,
            configurations=list(configurations) if configurations else None,
            resume=resume,
            resolver=resolver,
        )
        timings["generate"] = _time.monotonic() - t0
    else:
        click.echo(f"\n⏭️  Step 1/{total_steps}: Generate — skipped")

    if not skip_verify and config.verification is not None:
        click.echo(f"\n🔨 Step 2/{total_steps}: Verifying builds...")
        t0 = _time.monotonic()
        run_verify(config, project_root)
        timings["verify"] = _time.monotonic() - t0
    else:
        reason = "not configured" if config.verification is None else "skipped"
        click.echo(f"\n⏭️  Step 2/{total_steps}: Verify — {reason}")

    click.echo(f"\n📊 Step 3/{total_steps}: Running analysis...")
    t0 = _time.monotonic()
    run_analyze(config, project_root)
    timings["analyze"] = _time.monotonic() - t0

    if has_improvement_targets and not skip_improvements:
        click.echo(f"\n💡 Step 4/{total_steps}: Generating improvement suggestions...")
        t0 = _time.monotonic()
        effective_research_dir = research_dir
        if effective_research_dir is None and config.research_dir:
            effective_research_dir = Path(config.research_dir)
        run_suggest_improvements(
            config, project_root, resolver, model_override=improvement_model,
            research_dir=effective_research_dir,
        )
        timings["improvements"] = _time.monotonic() - t0
    elif has_improvement_targets and skip_improvements:
        click.echo(f"\n⏭️  Step 4/{total_steps}: Improvement suggestions — skipped")

    total_time = _time.monotonic() - pipeline_start

    click.echo("\n🎉 Evaluation complete!")
    click.echo(f"   Report: {config.output.reports_directory}/{config.output.analysis_file}")
    if has_improvement_targets and not skip_improvements:
        for t in config.improvement_targets:
            imp_file = config.output.improvements_file_pattern.format(config=t.name)
            click.echo(f"   Improvements: {config.output.reports_directory}/{imp_file}")
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


@main.command("auto-improve")
@click.option("--configuration", "-c", required=True,
              help="Name of the configuration to improve (must have suggest_improvements: true).")
@click.option("--max-turns", type=int, default=5,
              help="Maximum number of improvement iterations (default: 5).")
@click.option("--target-score", type=float, default=9.0,
              help="Stop when weighted average score reaches this value (default: 9.0).")
@click.option("--min-improvement", type=float, default=0.5,
              help="Stop if score improvement is below this threshold (default: 0.5).")
@click.option("--runs-per-iteration", type=int, default=1,
              help="Number of generation runs per iteration (default: 1).")
@click.option("--final-runs", type=int, default=None,
              help="Number of runs for the final validation pass (default: same as --runs-per-iteration).")
@click.option("--generation-model", type=str, default=None,
              help="AI model for code generation (overrides eval.yaml).")
@click.option("--analysis-model", "-m", type=str, default=None,
              help="AI model for analysis (overrides eval.yaml).")
@click.option("--improvement-model", type=str, default=None,
              help="AI model for improvement suggestions (overrides eval.yaml).")
@click.option("--no-rollback", is_flag=True,
              help="Disable automatic rollback on score regression.")
@click.option("--dimensions", type=str, default=None,
              help="Comma-separated dimension names to focus improvement on (e.g., 'Error Handling,Performance').")
@click.option("--focus-lowest", type=int, default=None,
              help="Auto-select the N lowest-scoring dimensions after first evaluation.")
@click.option("--research-dir", type=click.Path(exists=True, file_okay=False, path_type=Path),
              default=None,
              help="Directory containing best-practice research files to include in improvement prompts.")
@click.option("--max-retries", type=int, default=3,
              help="Maximum retry attempts per plateau before giving up (default: 3). Set to 0 to disable retries.")
@click.option("--lessons-file", type=click.Path(path_type=Path), default=None,
              help="Path to lessons-learned JSON file for cross-run persistence (default: reports/lessons-learned-{config}.json).")
@click.option("--no-lessons", is_flag=True,
              help="Disable lessons learned entirely.")
@click.pass_context
def auto_improve(
    ctx: click.Context,
    configuration: str,
    max_turns: int,
    target_score: float,
    min_improvement: float,
    runs_per_iteration: int,
    final_runs: int | None,
    generation_model: str | None,
    analysis_model: str | None,
    improvement_model: str | None,
    no_rollback: bool,
    dimensions: str | None,
    focus_lowest: int | None,
    research_dir: Path | None,
    max_retries: int,
    lessons_file: Path | None,
    no_lessons: bool,
) -> None:
    """Iteratively improve a skill/plugin through automated evaluation loops.

    Runs the evaluation pipeline, generates improvement suggestions, applies
    them via Copilot CLI, and repeats until the target score is reached,
    improvement plateaus, or the maximum number of turns is exhausted.

    The target configuration must have suggest_improvements: true in eval.yaml.

    Use --dimensions to focus on specific dimensions, or --focus-lowest to
    auto-select the N weakest dimensions after the first evaluation.

    Use --research-dir to include domain-specific best practices in prompts.

    When improvement stalls (plateau or regression), the loop will retry up to
    --max-retries times with lessons learned from failed attempts. Set
    --max-retries 0 to restore the old stop-on-plateau behavior.
    """
    from skill_eval.auto_improve import run_auto_improve

    # Parse comma-separated dimension names
    focus_dims: list[str] | None = None
    if dimensions:
        focus_dims = [d.strip() for d in dimensions.split(",") if d.strip()]

    config = _load(ctx)
    resolver = _build_resolver(ctx)
    effective_research_dir = research_dir
    if effective_research_dir is None and config.research_dir:
        effective_research_dir = Path(config.research_dir)
    run_auto_improve(
        config,
        ctx.obj["project_root"],
        resolver,
        configuration,
        max_turns=max_turns,
        target_score=target_score,
        min_improvement=min_improvement,
        runs_per_iteration=runs_per_iteration,
        final_runs=final_runs,
        generation_model=generation_model,
        analysis_model=analysis_model,
        improvement_model=improvement_model,
        no_rollback=no_rollback,
        focus_dimensions=focus_dims,
        focus_lowest=focus_lowest,
        research_dir=effective_research_dir,
        max_retries=max_retries,
        lessons_file=lessons_file,
        no_lessons=no_lessons,
    )


@main.command()
@click.pass_context
def validate_config(ctx: click.Context) -> None:
    """Validate eval.yaml without running anything.

    Checks that the configuration file is valid, all referenced prompt files
    exist, and skill/plugin sources are accessible.
    """
    config = _load(ctx)
    resolver = _build_resolver(ctx)
    project_root = ctx.obj["project_root"]

    errors = []

    # Check scenario prompt files exist
    for s in config.scenarios:
        prompt_path = project_root / s.prompt
        if not prompt_path.exists():
            errors.append(f"Scenario prompt not found: {s.prompt}")

    # Check skill/plugin references resolve to valid paths
    for c in config.configurations:
        for ref in c.skills:
            try:
                resolved = resolver.resolve(ref)
                if not resolved.exists():
                    errors.append(
                        f"Skill directory not found: {ref.display_name} "
                        f"(resolved to {resolved}, config: {c.name})"
                    )
            except (KeyError, ValueError, RuntimeError) as e:
                errors.append(f"Skill resolution failed: {ref.display_name} (config: {c.name}): {e}")
        for ref in c.plugins:
            try:
                resolved = resolver.resolve(ref)
                if not resolved.exists():
                    errors.append(
                        f"Plugin directory not found: {ref.display_name} "
                        f"(resolved to {resolved}, config: {c.name})"
                    )
            except (KeyError, ValueError, RuntimeError) as e:
                errors.append(f"Plugin resolution failed: {ref.display_name} (config: {c.name}): {e}")

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
    """Load config, applying any CLI overrides for output/reports directories."""
    config = load_config(ctx.obj.get("config_path"))
    output_dir = ctx.obj.get("output_dir")
    reports_dir = ctx.obj.get("reports_dir")
    if output_dir:
        config.output.directory = str(output_dir)
    if reports_dir:
        config.output.reports_directory = str(reports_dir)
    return config


def _build_resolver(ctx: click.Context):
    """Build a SourceResolver from CLI context, loading skill-sources.yaml if available."""
    from skill_eval.source_config import load_skill_sources
    from skill_eval.source_resolver import SourceResolver

    project_root: Path = ctx.obj["project_root"]
    skill_sources_path: Path | None = ctx.obj.get("skill_sources_path")
    cache_dir: Path | None = ctx.obj.get("cache_dir")

    # Try to load skill-sources.yaml
    sources_config = None
    if skill_sources_path is not None:
        sources_config = load_skill_sources(skill_sources_path)
    else:
        default_path = project_root / "skill-sources.yaml"
        if default_path.exists():
            sources_config = load_skill_sources(default_path)

    # Respect cache_dir from skill-sources.yaml if not overridden by CLI
    if sources_config and sources_config.cache_dir and cache_dir is None:
        cache_dir = Path(sources_config.cache_dir).expanduser()

    return SourceResolver(sources_config, project_root, cache_dir=cache_dir)
