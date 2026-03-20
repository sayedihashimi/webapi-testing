"""Interactive initialization for a new skill evaluation project."""

from __future__ import annotations

from pathlib import Path

import click
import yaml

from skill_eval.prompt_renderer import render_scenario_template


def run_init(project_root: Path) -> None:
    """Interactively create eval.yaml and starter scenario prompts."""
    eval_path = project_root / "eval.yaml"

    if eval_path.exists():
        if not click.confirm("eval.yaml already exists. Overwrite?", default=False):
            click.echo("Aborted.")
            return

    click.echo("\n🚀 Copilot Skill Evaluation — Project Setup\n")

    # Project metadata
    name = click.prompt("Evaluation name", default="My Skill Evaluation")
    description = click.prompt(
        "Short description",
        default="Evaluate how my custom skills improve code generation",
    )

    # Tech stack (informational — affects scenario templates)
    tech_stack = click.prompt(
        "Tech stack (e.g., ASP.NET Core, React, Go, Python Flask)",
        default="ASP.NET Core",
    )

    # Scenarios
    click.echo("\n📋 Scenarios — realistic apps to generate for comparison")
    scenarios = []
    while True:
        scenario_name = click.prompt(
            f"  Scenario {len(scenarios) + 1} name (or 'done' to finish)",
            default="done" if len(scenarios) >= 2 else "",
        )
        if scenario_name.lower() == "done" and len(scenarios) >= 1:
            break
        if scenario_name.lower() == "done":
            click.echo("  At least one scenario is required.")
            continue

        scenario_desc = click.prompt(f"  Brief description of {scenario_name}", default="")
        scenarios.append({"name": scenario_name, "description": scenario_desc})

    # Configurations
    click.echo("\n⚙️  Configurations — skill sets to compare")
    click.echo("  A 'no-skills' baseline will be created automatically.")
    configurations = [
        {"name": "no-skills", "label": "Baseline (no skills)", "skills": [], "plugins": []},
    ]
    while True:
        config_name = click.prompt(
            f"  Configuration {len(configurations) + 1} name (or 'done' to finish)",
            default="done" if len(configurations) >= 2 else "",
        )
        if config_name.lower() == "done" and len(configurations) >= 2:
            break
        if config_name.lower() == "done":
            click.echo("  At least two configurations (baseline + one skill) are required.")
            continue

        config_label = click.prompt(f"  Display label for '{config_name}'", default=config_name)
        skill_dir = click.prompt(
            f"  Skill directory path (relative, or empty for none)", default=""
        )
        skills = [skill_dir] if skill_dir else []
        plugin_dir = click.prompt(
            f"  Plugin directory path (relative, or empty for none)", default=""
        )
        plugins = [plugin_dir] if plugin_dir else []
        configurations.append({
            "name": config_name,
            "label": config_label,
            "skills": skills,
            "plugins": plugins,
        })

    # Verification commands
    click.echo(f"\n🔨 Verification — how to build/run {tech_stack} projects")
    build_cmd = click.prompt("  Build command", default=_default_build_cmd(tech_stack))
    run_cmd = click.prompt(
        "  Run command (or empty to skip run verification)",
        default=_default_run_cmd(tech_stack),
    )

    # Dimensions
    click.echo("\n📊 Analysis dimensions — quality criteria to evaluate")
    click.echo("  You can add more later by editing eval.yaml.")
    dimensions = _suggest_dimensions(tech_stack)
    click.echo(f"  Suggested {len(dimensions)} dimensions for {tech_stack}.")

    # Build the config dict
    verification: dict = {
        "build": {"command": build_cmd},
    }
    if run_cmd:
        verification["run"] = {"command": run_cmd, "timeout_seconds": 15}

    config = {
        "name": name,
        "description": description,
        "scenarios": [
            {
                "name": s["name"],
                "prompt": f"prompts/scenarios/{_slugify(s['name'])}.prompt.md",
                "description": s["description"],
            }
            for s in scenarios
        ],
        "configurations": configurations,
        "verification": verification,
        "dimensions": dimensions,
        "output": {
            "directory": "output",
            "reports_directory": "reports",
            "analysis_file": "analysis.md",
            "notes_file": "build-notes.md",
        },
    }

    # Write eval.yaml
    eval_path.write_text(
        yaml.dump(config, default_flow_style=False, sort_keys=False, allow_unicode=True),
        encoding="utf-8",
    )
    click.echo(f"\n✅ Created: {eval_path}")

    # Create scenario prompt files
    prompts_dir = project_root / "prompts" / "scenarios"
    prompts_dir.mkdir(parents=True, exist_ok=True)

    for s in scenarios:
        prompt_file = prompts_dir / f"{_slugify(s['name'])}.prompt.md"
        if not prompt_file.exists():
            content = render_scenario_template(s["name"], project_root)
            prompt_file.write_text(content, encoding="utf-8")
            click.echo(f"✅ Created: {prompt_file}")
        else:
            click.echo(f"⏭️  Exists:  {prompt_file}")

    # Create skills directory
    skills_dir = project_root / "skills"
    skills_dir.mkdir(exist_ok=True)

    # Print next steps
    click.echo(f"""
{'=' * 60}
🎉 Setup complete!

Next steps:
  1. Edit the scenario prompts in prompts/scenarios/
     Describe each app's entities, business rules, and endpoints.

  2. Copy your skill(s) into the skills/ directory.

  3. Review eval.yaml and adjust dimensions as needed.

  4. Run the evaluation:
     skill-eval run

  Or use the agent:
     @skill-eval run the evaluation
{'=' * 60}
""")


def _slugify(name: str) -> str:
    """Convert a name to a URL-friendly slug."""
    return name.lower().replace(" ", "-").replace("_", "-")


def _default_build_cmd(tech_stack: str) -> str:
    """Suggest a build command based on tech stack."""
    stack = tech_stack.lower()
    if "dotnet" in stack or "asp.net" in stack or ".net" in stack:
        return "dotnet build"
    if "react" in stack or "next" in stack or "node" in stack or "npm" in stack:
        return "npm run build"
    if "go" in stack or "golang" in stack:
        return "go build ./..."
    if "python" in stack or "flask" in stack or "django" in stack or "fastapi" in stack:
        return "python -m py_compile"
    return "make build"


def _default_run_cmd(tech_stack: str) -> str:
    """Suggest a run command based on tech stack."""
    stack = tech_stack.lower()
    if "dotnet" in stack or "asp.net" in stack or ".net" in stack:
        return "dotnet run"
    if "react" in stack or "next" in stack or "node" in stack or "npm" in stack:
        return "npm start"
    if "go" in stack or "golang" in stack:
        return "go run ."
    if "python" in stack or "flask" in stack or "django" in stack or "fastapi" in stack:
        return "python -m flask run"
    return ""


def _suggest_dimensions(tech_stack: str) -> list[dict]:
    """Suggest analysis dimensions based on tech stack."""
    # Universal dimensions that apply to any stack
    dimensions = [
        {
            "name": "Code Organization",
            "description": "How files and modules are structured",
            "what_to_look_for": "Check folder structure, file naming, separation of concerns, module boundaries.",
            "why_it_matters": "Good organization improves maintainability and discoverability.",
        },
        {
            "name": "Error Handling",
            "description": "How errors and exceptions are handled",
            "what_to_look_for": "Check for consistent error handling patterns, custom error types, proper HTTP status codes or error responses.",
            "why_it_matters": "Consistent error handling is critical for reliability and debugging.",
        },
        {
            "name": "Type Safety",
            "description": "Use of type annotations, strict types, and immutability",
            "what_to_look_for": "Check for type annotations, strict mode settings, use of immutable data structures.",
            "why_it_matters": "Type safety catches bugs at compile/lint time and improves code clarity.",
        },
        {
            "name": "Input Validation",
            "description": "How user input and request data is validated",
            "what_to_look_for": "Check for validation libraries, data annotations, schema validation, or manual checks.",
            "why_it_matters": "Proper validation prevents security issues and improves data integrity.",
        },
        {
            "name": "Testing Support",
            "description": "Whether the generated code is structured for testability",
            "what_to_look_for": "Check for dependency injection, interface abstractions, separation of business logic from I/O.",
            "why_it_matters": "Testable code is a strong signal of good architecture.",
        },
    ]

    stack = tech_stack.lower()

    # Stack-specific dimensions
    if "dotnet" in stack or "asp.net" in stack or ".net" in stack:
        dimensions.extend([
            {
                "name": "API Style",
                "description": "Controllers (MVC) vs Minimal APIs",
                "what_to_look_for": "Check Program.cs and endpoint files for MapGet/MapPost (minimal) vs [ApiController] classes.",
                "why_it_matters": "Minimal APIs have lower overhead and are the modern .NET default.",
            },
            {
                "name": "CancellationToken Propagation",
                "description": "Whether tokens are forwarded through all layers",
                "what_to_look_for": "Check endpoint handlers for CancellationToken parameters. Trace through service methods to EF Core calls.",
                "why_it_matters": "Critical for production — prevents wasted server resources on cancelled requests.",
            },
            {
                "name": "Sealed Types",
                "description": "Whether classes and records are declared sealed",
                "what_to_look_for": "Check class/record declarations for the sealed modifier.",
                "why_it_matters": "Sealed types enable JIT optimizations and signal design intent.",
            },
        ])
    elif "react" in stack or "next" in stack:
        dimensions.extend([
            {
                "name": "Component Architecture",
                "description": "Component composition patterns and reusability",
                "what_to_look_for": "Check for atomic design, compound components, render props, custom hooks.",
                "why_it_matters": "Good component architecture enables reuse and simplifies testing.",
            },
            {
                "name": "State Management",
                "description": "How application state is managed",
                "what_to_look_for": "Check for Context, Redux, Zustand, React Query, or local state patterns.",
                "why_it_matters": "State management approach affects performance, complexity, and maintainability.",
            },
        ])
    elif "go" in stack or "golang" in stack:
        dimensions.extend([
            {
                "name": "Error Handling Idioms",
                "description": "Go-idiomatic error handling patterns",
                "what_to_look_for": "Check for error wrapping, sentinel errors, custom error types, proper error checking.",
                "why_it_matters": "Idiomatic Go error handling improves debuggability and API clarity.",
            },
            {
                "name": "Interface Design",
                "description": "Interface usage and consumer-side definition",
                "what_to_look_for": "Check if interfaces are defined at the consumer site, kept small, and used for testing.",
                "why_it_matters": "Small, consumer-defined interfaces are a Go best practice for loose coupling.",
            },
        ])

    return dimensions
