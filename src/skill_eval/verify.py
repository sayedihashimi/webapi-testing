"""Verify generated projects by running build and run commands."""

from __future__ import annotations

import json
import os
import re
import signal
import subprocess
import sys
import time
from concurrent.futures import ThreadPoolExecutor, as_completed
from datetime import datetime, timezone
from pathlib import Path
from urllib.error import URLError
from urllib.request import urlopen

import click

from skill_eval.config import EvalConfig


def _find_build_directory(project_dir: Path, working_directory: str) -> Path:
    """Find the best directory to run the build command from.

    If the configured working_directory contains a project/solution file, use it.
    Otherwise, search for a .sln or .csproj file and use its parent directory.
    """
    cwd = project_dir / working_directory

    # Check if configured directory already has buildable files
    if cwd.exists():
        if list(cwd.glob("*.sln")) or list(cwd.glob("*.csproj")):
            return cwd

    # Search for .sln first (preferred), then .csproj
    sln_files = list(project_dir.rglob("*.sln"))
    if sln_files:
        return sln_files[0].parent

    csproj_files = list(project_dir.rglob("*.csproj"))
    if csproj_files:
        return csproj_files[0].parent

    # Fall back to configured directory
    return cwd


def _inject_analyzers(project_dir: Path, analyzers: list[str]) -> None:
    """Inject Directory.Build.props with analyzer config if not already present."""
    build_props = project_dir / "Directory.Build.props"
    if build_props.exists():
        return  # Don't overwrite existing config

    # Find the template
    template_path = Path(__file__).parent.parent.parent / "templates" / "Directory.Build.props"
    if not template_path.exists():
        return

    content = template_path.read_text(encoding="utf-8")

    # Add analyzer package references if specified
    if analyzers:
        pkg_refs = "\n".join(
            f'    <PackageReference Include="{pkg}" Version="*">\n'
            f'      <PrivateAssets>all</PrivateAssets>\n'
            f'    </PackageReference>'
            for pkg in analyzers
        )
        item_group = f"\n  <ItemGroup>\n{pkg_refs}\n  </ItemGroup>"
        content = content.replace("</Project>", f"{item_group}\n</Project>")

    # Write to the build directory, not project root
    build_dir = _find_build_directory(project_dir, ".")
    target = build_dir / "Directory.Build.props"
    if not target.exists():
        target.write_text(content, encoding="utf-8")


def _parse_build_warnings(build_output: str) -> dict[str, int]:
    """Parse Roslyn analyzer warnings from build output into category counts."""
    categories = {
        "naming": 0,      # CA17xx
        "performance": 0,  # CA18xx
        "reliability": 0,  # CA20xx
        "security": 0,     # CA21xx, CA53xx
        "usage": 0,        # CA22xx
        "style": 0,        # IDExxxx
        "other": 0,
    }
    total = 0

    for match in re.finditer(r"warning (CA|IDE|CS)(\d{4,5})", build_output):
        prefix = match.group(1)
        code = int(match.group(2))
        total += 1
        if prefix == "IDE":
            categories["style"] += 1
        elif prefix == "CS":
            categories["other"] += 1
        elif 1700 <= code <= 1799:
            categories["naming"] += 1
        elif 1800 <= code <= 1899:
            categories["performance"] += 1
        elif 2000 <= code <= 2099:
            categories["reliability"] += 1
        elif 2100 <= code <= 2199 or 5300 <= code <= 5399:
            categories["security"] += 1
        elif 2200 <= code <= 2299:
            categories["usage"] += 1
        else:
            categories["other"] += 1

    categories["total"] = total
    return categories


def _run_format(project_dir: Path, command: str, working_directory: str) -> tuple[bool, int, str]:
    """Run dotnet format --check and return (passed, issue_count, output)."""
    cwd = _find_build_directory(project_dir, working_directory)
    try:
        result = subprocess.run(
            command, shell=True, cwd=cwd, capture_output=True, text=True, timeout=120,
        )
        output = result.stdout + result.stderr
        # Count files with formatting issues
        issue_count = len(re.findall(r"Formatted code file", output))
        if result.returncode != 0:
            # dotnet format --check returns non-zero when files need formatting
            if issue_count == 0:
                issue_count = output.count(".cs")  # fallback count
        return result.returncode == 0, issue_count, output
    except subprocess.TimeoutExpired:
        return False, 0, "Format check timed out"
    except Exception as e:
        return False, 0, str(e)


def _run_security_scan(project_dir: Path) -> tuple[int, dict[str, int], str]:
    """Run dotnet list package --vulnerable and return (total_vulns, severity_counts, output)."""
    cwd = _find_build_directory(project_dir, ".")
    try:
        result = subprocess.run(
            "dotnet list package --vulnerable",
            shell=True, cwd=cwd, capture_output=True, text=True, timeout=60,
        )
        output = result.stdout + result.stderr
        severities = {"Critical": 0, "High": 0, "Moderate": 0, "Low": 0}
        for severity in severities:
            severities[severity] = output.lower().count(severity.lower())
        total = sum(severities.values())
        return total, severities, output
    except subprocess.TimeoutExpired:
        return 0, {}, "Security scan timed out"
    except Exception as e:
        return 0, {}, str(e)


def _run_build(
    command: str,
    project_dir: Path,
    working_directory: str,
    success_pattern: str | None,
) -> tuple[bool, str]:
    """Run the build command and return (success, output)."""
    cwd = _find_build_directory(project_dir, working_directory)
    try:
        result = subprocess.run(
            command,
            shell=True,
            cwd=cwd,
            capture_output=True,
            text=True,
            timeout=120,
        )
        output = result.stdout + result.stderr

        if result.returncode != 0:
            return False, output

        if success_pattern and not re.search(success_pattern, output):
            return False, f"Build exited 0 but success pattern not found: {success_pattern}\n{output}"

        return True, output

    except subprocess.TimeoutExpired:
        return False, "Build timed out after 120 seconds"
    except Exception as e:
        return False, str(e)


def _run_app(
    command: str,
    project_dir: Path,
    timeout_seconds: int,
    health_check_url: str | None,
    expected_status: int,
) -> tuple[bool, str]:
    """Start the app, optionally check health, then stop it."""
    try:
        run_dir = _find_build_directory(project_dir, ".")
        kwargs: dict = dict(
            shell=True,
            cwd=run_dir,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
        )
        # On Windows, create a new process group so we can kill the tree
        if sys.platform == "win32":
            kwargs["creationflags"] = subprocess.CREATE_NEW_PROCESS_GROUP
        proc = subprocess.Popen(command, **kwargs)

        # Wait for the app to start
        time.sleep(timeout_seconds)

        # Check if the process is still running
        if proc.poll() is not None:
            stdout = proc.stdout.read().decode() if proc.stdout else ""
            stderr = proc.stderr.read().decode() if proc.stderr else ""
            return False, f"App exited early (code {proc.returncode}):\n{stdout}\n{stderr}"

        # Optional health check
        if health_check_url:
            try:
                resp = urlopen(health_check_url, timeout=10)
                if resp.status != expected_status:
                    _terminate(proc)
                    return False, f"Health check returned {resp.status}, expected {expected_status}"
            except URLError as e:
                _terminate(proc)
                return False, f"Health check failed: {e}"

        _terminate(proc)
        return True, "App started and responded successfully"

    except Exception as e:
        return False, str(e)


def _terminate(proc: subprocess.Popen) -> None:
    """Terminate a subprocess and its entire process tree."""
    if sys.platform == "win32":
        # On Windows, taskkill /T kills the process tree
        try:
            subprocess.run(
                f"taskkill /F /T /PID {proc.pid}",
                shell=True, capture_output=True, timeout=10,
            )
        except Exception:
            pass
    else:
        # On Unix, kill the process group
        try:
            os.killpg(os.getpgid(proc.pid), signal.SIGTERM)
        except (ProcessLookupError, OSError):
            pass
    try:
        proc.wait(timeout=5)
    except subprocess.TimeoutExpired:
        proc.kill()
        try:
            proc.wait(timeout=5)
        except Exception:
            pass


def _verify_single(
    config: "EvalConfig",
    cfg_name: str,
    run_id: int,
    scenario_name: str,
    project_dir: Path,
) -> dict:
    """Verify a single project (build, run, format, security). Thread-safe."""
    if not project_dir.exists():
        return {
            "config": cfg_name,
            "run_id": run_id,
            "scenario": scenario_name,
            "build": "⚠️ Not found",
            "build_success": False,
            "run": "⚠️ Not found",
            "run_success": False,
            "notes": "Project directory not found",
        }

    # Inject analyzers if configured
    if config.verification.analyzers:
        _inject_analyzers(project_dir, config.verification.analyzers)

    # Build
    build_ok, build_output = _run_build(
        config.verification.build.command,
        project_dir,
        config.verification.build.working_directory,
        config.verification.build.success_pattern,
    )
    warnings = _parse_build_warnings(build_output)

    # Run (optional)
    run_status = "⏭️ Skipped"
    run_ok = False
    run_notes = ""
    if config.verification.run and build_ok:
        hc = config.verification.run.health_check
        run_ok, run_output = _run_app(
            config.verification.run.command,
            project_dir,
            config.verification.run.timeout_seconds,
            hc.url if hc else None,
            hc.expected_status if hc else 200,
        )
        run_status = "✅ Pass" if run_ok else "❌ Fail"
        if not run_ok:
            run_notes = run_output[:200]

    # Format check (optional)
    format_status = "⏭️ Skipped"
    format_issues = 0
    if config.verification.format and build_ok:
        fmt_ok, fmt_count, _ = _run_format(
            project_dir,
            config.verification.format.command,
            config.verification.format.working_directory,
        )
        format_status = "✅ Pass" if fmt_ok else f"❌ {fmt_count} issues"
        format_issues = fmt_count

    # Security scan (optional)
    security_status = "⏭️ Skipped"
    vuln_sev: dict = {}
    if config.verification.security and config.verification.security.vulnerability_scan and build_ok:
        vuln_total_count, vuln_sev, _ = _run_security_scan(project_dir)
        security_status = "✅ Clean" if vuln_total_count == 0 else f"⚠️ {vuln_total_count} vulns"

    return {
        "config": cfg_name,
        "run_id": run_id,
        "scenario": scenario_name,
        "build": "✅ Pass" if build_ok else "❌ Fail",
        "build_success": build_ok,
        "run": run_status,
        "run_success": run_ok,
        "warnings": warnings,
        "format": format_status,
        "format_issues": format_issues,
        "security": security_status,
        "security_vulnerabilities": vuln_sev,
        "notes": build_output[:200] if not build_ok else run_notes,
    }


def run_verify(config: EvalConfig, project_root: Path) -> None:
    """Verify all generated projects build and run (sequential)."""
    output_base = project_root / config.output.directory
    reports_dir = project_root / config.output.reports_directory
    reports_dir.mkdir(parents=True, exist_ok=True)

    num_runs = config.runs
    results: list[dict] = []

    for cfg in config.configurations:
        config_dir = output_base / cfg.name
        if not config_dir.exists():
            click.echo(f"⚠️  Skipping {cfg.name}: output directory not found")
            continue

        for run_id in range(1, num_runs + 1):
            run_dir = config_dir / f"run-{run_id}"
            if not run_dir.exists():
                click.echo(f"⚠️  Skipping {cfg.name}/run-{run_id}: not found")
                continue

            for scenario in config.scenarios:
                project_dir = run_dir / scenario.name
                label = f"{cfg.name}/run-{run_id}/{scenario.name}"
                click.echo(f"  Verifying: {label}")

                result = _verify_single(config, cfg.name, run_id, scenario.name, project_dir)
                build = result["build"]
                run_status = result["run"]
                warns = result.get("warnings", {}).get("total", 0)
                click.echo(f"    Build {build} | Run {run_status} | Warnings: {warns}")
                results.append(result)

    # Write build-notes.md
    notes_path = reports_dir / config.output.notes_file
    _write_build_notes(notes_path, config, results)
    click.echo(f"\n📝 Build notes written to: {notes_path}")

    # Write machine-readable verification data
    json_path = reports_dir / config.output.verification_data_file
    _write_verification_json(json_path, results)
    click.echo(f"📝 Verification data written to: {json_path}")


def _write_build_notes(
    path: Path,
    config: EvalConfig,
    results: list[dict],
) -> None:
    """Write the build/run verification report."""
    lines = [
        f"# Build & Run Verification Report",
        f"",
        f"**Evaluation:** {config.name}",
        f"**Date:** {datetime.now(timezone.utc).strftime('%Y-%m-%d %H:%M UTC')}",
        f"**Configurations:** {len(config.configurations)}",
        f"**Scenarios:** {len(config.scenarios)}",
        f"**Total projects:** {len(config.configurations) * len(config.scenarios)}",
        f"",
        f"## Results",
        f"",
        f"| Configuration | Run | Scenario | Build | Run | Format | Security | Notes |",
        f"|---|---|---|---|---|---|---|---|",
    ]

    for r in results:
        notes = r.get("notes", "").replace("\n", " ")[:100]
        fmt = r.get("format", "⏭️ Skipped")
        sec = r.get("security", "⏭️ Skipped")
        run_id = r.get("run_id", 1)
        lines.append(
            f"| {r['config']} | {run_id} | {r['scenario']} | {r['build']} | {r['run']} | {fmt} | {sec} | {notes} |"
        )

    lines.append("")

    # Automated Metrics section
    has_warnings = any(r.get("warnings", {}).get("total", 0) > 0 for r in results)
    if has_warnings:
        lines.extend([
            "## Automated Metrics",
            "",
            "### Build Warnings by Category",
            "",
            "| Configuration | Scenario | Total | Naming | Performance | Reliability | Security | Usage | Style | Other |",
            "|---|---|---|---|---|---|---|---|---|---|",
        ])
        for r in results:
            w = r.get("warnings", {})
            if w.get("total", 0) > 0:
                lines.append(
                    f"| {r['config']} | {r['scenario']} "
                    f"| {w.get('total', 0)} | {w.get('naming', 0)} | {w.get('performance', 0)} "
                    f"| {w.get('reliability', 0)} | {w.get('security', 0)} | {w.get('usage', 0)} "
                    f"| {w.get('style', 0)} | {w.get('other', 0)} |"
                )
        lines.append("")

    # Skill configuration summary
    lines.extend([
        "## Skill Configurations",
        "",
        "| Configuration | Label | Skills | Plugins |",
        "|---|---|---|---|",
    ])
    for cfg in config.configurations:
        skills = ", ".join(cfg.skills) if cfg.skills else "None"
        plugins = ", ".join(cfg.plugins) if cfg.plugins else "None"
        lines.append(f"| {cfg.name} | {cfg.label} | {skills} | {plugins} |")

    lines.append("")
    path.write_text("\n".join(lines), encoding="utf-8")


def _write_verification_json(path: Path, results: list[dict]) -> None:
    """Write machine-readable verification data as JSON."""
    json_results = []
    for r in results:
        json_results.append({
            "config": r["config"],
            "run_id": r.get("run_id", 1),
            "scenario": r["scenario"],
            "build_success": r.get("build_success", "Pass" in r.get("build", "")),
            "run_success": r.get("run_success", "Pass" in r.get("run", "")),
            "build_warnings": r.get("warnings", {}),
            "format_issues": r.get("format_issues", 0),
            "security_vulnerabilities": r.get("security_vulnerabilities", {}),
        })
    data = {"timestamp": datetime.now(timezone.utc).isoformat(), "results": json_results}
    path.write_text(json.dumps(data, indent=2), encoding="utf-8")
