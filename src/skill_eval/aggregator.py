"""Parse per-run analysis scores and produce aggregated statistics."""

from __future__ import annotations

import json
import re
import statistics
from datetime import datetime, timezone
from pathlib import Path

from skill_eval.config import EvalConfig


def parse_run_scores(
    analysis_file: Path,
    config_names: list[str],
) -> dict[str, dict[str, float]] | None:
    """Parse scores from an analysis-run-N.md Executive Summary table.

    Returns {dimension_name: {config_name: score}} or None if parsing fails.
    """
    if not analysis_file.exists():
        return None

    text = analysis_file.read_text(encoding="utf-8")

    # Find the Executive Summary table
    # Pattern: | Dimension [Tier] | config1 | config2 | ... |
    lines = text.split("\n")
    table_lines: list[str] = []
    in_summary = False

    for line in lines:
        stripped = line.strip()
        if "Executive Summary" in stripped:
            in_summary = True
            continue
        if in_summary:
            if stripped.startswith("|") and "---" not in stripped:
                table_lines.append(stripped)
            elif stripped.startswith("|") and "---" in stripped:
                continue  # separator row
            elif table_lines and not stripped.startswith("|"):
                break  # end of table

    if len(table_lines) < 2:
        return None

    # Parse header to find config column indices
    header = table_lines[0]
    header_cells = [c.strip().strip("*") for c in header.split("|")[1:-1]]

    # Map config names to column indices
    col_map: dict[str, int] = {}

    # Pass 1: exact matches
    for i, cell in enumerate(header_cells):
        cell_lower = cell.lower().strip()
        for cfg_name in config_names:
            if cfg_name.lower() == cell_lower:
                col_map[cfg_name] = i
                break

    # Pass 2: substring matches for configs not yet matched, preferring
    # the longest matching config name to avoid "dotnet-skills" stealing
    # the column that belongs to "managedcode-dotnet-skills".
    unmatched = [c for c in config_names if c not in col_map]
    for i, cell in enumerate(header_cells):
        if i in col_map.values():
            continue  # column already claimed
        cell_lower = cell.lower().strip()
        best_name: str | None = None
        best_len = 0
        for cfg_name in unmatched:
            if cfg_name.lower() in cell_lower or cell_lower in cfg_name.lower():
                # Prefer the longest matching name to avoid substring collisions
                if len(cfg_name) > best_len:
                    best_name = cfg_name
                    best_len = len(cfg_name)
        if best_name is not None:
            col_map[best_name] = i
            unmatched.remove(best_name)

    if not col_map:
        return None

    # Parse score rows
    scores: dict[str, dict[str, float]] = {}
    for row in table_lines[1:]:
        cells = [c.strip() for c in row.split("|")[1:-1]]
        if len(cells) < 2:
            continue

        dim_cell = cells[0].strip()
        # Extract dimension name (remove [TIER] suffix)
        dim_match = re.match(r"(.+?)\s*\[", dim_cell)
        dim_name = dim_match.group(1).strip() if dim_match else dim_cell.strip()
        dim_name = dim_name.strip("*").strip()

        if not dim_name:
            continue

        scores[dim_name] = {}
        for cfg_name, col_idx in col_map.items():
            if col_idx < len(cells):
                score_text = cells[col_idx].strip().strip("*")
                try:
                    scores[dim_name][cfg_name] = float(score_text)
                except ValueError:
                    pass  # Skip unparseable scores

    return scores if scores else None


def parse_run_content(analysis_file: Path) -> dict[str, str]:
    """Extract per-dimension rich content sections from an analysis-run-N.md.

    Returns {dimension_name: markdown_content} where content includes
    code snippets, verdicts, and qualitative analysis.
    """
    if not analysis_file.exists():
        return {}

    text = analysis_file.read_text(encoding="utf-8")
    lines = text.split("\n")
    sections: dict[str, str] = {}

    current_dim = ""
    current_lines: list[str] = []
    in_dimension = False

    for line in lines:
        # Match dimension headers like "## 3. Minimal API Architecture [CRITICAL]"
        header_match = re.match(
            r"^##\s+\d+\.\s+(.+?)\s*\[", line,
        )
        if header_match:
            # Save previous section
            if current_dim and current_lines:
                sections[current_dim] = "\n".join(current_lines).strip()
            current_dim = header_match.group(1).strip()
            current_lines = []
            in_dimension = True
            continue

        # End of dimension section (next ## header or major section)
        if in_dimension and re.match(r"^##\s+", line) and not re.match(r"^##\s+\d+\.", line):
            if current_dim and current_lines:
                sections[current_dim] = "\n".join(current_lines).strip()
            current_dim = ""
            current_lines = []
            in_dimension = False
            continue

        if in_dimension:
            current_lines.append(line)

    # Save last section
    if current_dim and current_lines:
        sections[current_dim] = "\n".join(current_lines).strip()

    return sections


def aggregate_results(config: EvalConfig, project_root: Path) -> None:
    """Parse all per-run analysis files and write the aggregated report."""
    reports_dir = project_root / config.output.reports_directory
    config_names = [c.name for c in config.configurations]
    num_runs = config.runs

    # Collect scores from all runs
    all_run_scores: list[dict[str, dict[str, float]]] = []
    parsed_run_ids: list[int] = []

    for run_id in range(1, num_runs + 1):
        run_file = reports_dir / config.output.per_run_analysis_pattern.format(run=run_id)
        scores = parse_run_scores(run_file, config_names)
        if scores:
            all_run_scores.append(scores)
            parsed_run_ids.append(run_id)

    # Filter config_names to only configs that have actual output on disk.
    # This handles the case where a config was excluded from the run via -c flags.
    # The AI judge may still score excluded configs (it sees eval.yaml), so we
    # filter based on output directories rather than score data alone.
    output_dir = project_root / config.output.directory
    configs_with_output = set()
    for c in config_names:
        cfg_dir = output_dir / c
        if cfg_dir.exists() and any(cfg_dir.iterdir()):
            configs_with_output.add(c)
    config_names = [c for c in config_names if c in configs_with_output]

    # Also strip excluded configs from the parsed scores
    for run_scores in all_run_scores:
        for dim in run_scores:
            run_scores[dim] = {
                cfg: score for cfg, score in run_scores[dim].items()
                if cfg in configs_with_output
            }

    if not all_run_scores:
        # No scores parsed — write a minimal report
        analysis_path = reports_dir / config.output.analysis_file
        analysis_path.write_text(
            "# Aggregated Analysis\n\n"
            "No per-run analysis scores could be parsed.\n"
            "Check the individual analysis-run-N.md files.\n",
            encoding="utf-8",
        )
        return

    # Load verification data if available
    verif_path = reports_dir / config.output.verification_data_file
    verif_data = None
    if verif_path.exists():
        verif_data = json.loads(verif_path.read_text(encoding="utf-8"))

    # Load generation usage data for session trace info
    usage_path = reports_dir / "generation-usage.json"
    generation_usage: list[dict] | None = None
    if usage_path.exists():
        try:
            generation_usage = json.loads(usage_path.read_text(encoding="utf-8"))
        except (json.JSONDecodeError, OSError):
            pass

    # Collect all dimension names across runs
    all_dims: list[str] = []
    seen = set()
    for run_scores in all_run_scores:
        for dim in run_scores:
            if dim not in seen:
                all_dims.append(dim)
                seen.add(dim)

    # Build dimension weight lookup
    dim_weights: dict[str, float] = {}
    dim_tiers: dict[str, str] = {}
    for d in config.dimensions:
        dim_weights[d.name] = d.effective_weight
        dim_tiers[d.name] = d.tier.upper()

    # Compute per-dimension stats
    dim_stats: dict[str, dict[str, dict[str, float]]] = {}
    for dim in all_dims:
        dim_stats[dim] = {}
        for cfg in config_names:
            values = [rs[dim][cfg] for rs in all_run_scores if dim in rs and cfg in rs[dim]]
            if values:
                dim_stats[dim][cfg] = {
                    "mean": statistics.mean(values),
                    "stdev": statistics.stdev(values) if len(values) > 1 else 0.0,
                    "min": min(values),
                    "max": max(values),
                    "values": values,
                }

    # Compute weighted totals per run per config
    weighted_per_run: dict[str, list[float]] = {cfg: [] for cfg in config_names}
    for run_scores in all_run_scores:
        for cfg in config_names:
            total = 0.0
            for dim, scores in run_scores.items():
                if cfg in scores:
                    weight = dim_weights.get(dim, 1.0)
                    total += scores[cfg] * weight
            weighted_per_run[cfg].append(total)

    # Write scores-data.json
    scores_json_path = reports_dir / config.output.scores_data_file
    _write_scores_json(scores_json_path, all_run_scores, parsed_run_ids, weighted_per_run)

    # Collect rich content from per-run analyses (pick the median-scoring run)
    rich_content = _select_best_run_content(reports_dir, config, parsed_run_ids, weighted_per_run, config_names)

    # Strip references to excluded configs from rich content
    excluded_configs = {c.name for c in config.configurations} - set(config_names)
    if excluded_configs and rich_content:
        for dim_key in list(rich_content.keys()):
            content = rich_content[dim_key]
            filtered_lines = []
            for line in content.split("\n"):
                if any(exc in line for exc in excluded_configs):
                    continue
                filtered_lines.append(line)
            rich_content[dim_key] = "\n".join(filtered_lines)

    # Write aggregated analysis.md
    analysis_path = reports_dir / config.output.analysis_file
    _write_aggregated_report(
        analysis_path, config, config_names, all_dims,
        dim_stats, dim_tiers, dim_weights,
        weighted_per_run, parsed_run_ids, all_run_scores,
        verif_data, rich_content, generation_usage,
    )


def _write_scores_json(
    path: Path,
    all_run_scores: list[dict],
    run_ids: list[int],
    weighted_per_run: dict[str, list[float]],
) -> None:
    """Write machine-readable scores data."""
    runs = []
    for i, run_id in enumerate(run_ids):
        runs.append({
            "run_id": run_id,
            "scores": all_run_scores[i],
            "weighted_totals": {cfg: weighted_per_run[cfg][i] for cfg in weighted_per_run},
        })
    data = {"timestamp": datetime.now(timezone.utc).isoformat(), "runs": runs}
    path.write_text(json.dumps(data, indent=2), encoding="utf-8")


def _select_best_run_content(
    reports_dir: Path,
    config: EvalConfig,
    run_ids: list[int],
    weighted_per_run: dict[str, list[float]],
    config_names: list[str],
) -> dict[str, str]:
    """Select rich content from the most representative per-run analysis.

    Picks the run closest to the median total score to avoid outliers.
    Returns {dimension_name: rich_markdown_content}.
    """
    if not run_ids:
        return {}

    # Find the median-scoring run (by average across configs)
    run_avg_scores: list[tuple[int, int, float]] = []
    for i, run_id in enumerate(run_ids):
        cfg_scores = [weighted_per_run[cfg][i] for cfg in config_names if weighted_per_run[cfg][i] > 0]
        avg = statistics.mean(cfg_scores) if cfg_scores else 0.0
        run_avg_scores.append((i, run_id, avg))

    run_avg_scores.sort(key=lambda x: x[2])
    # Pick the middle run (median)
    median_idx, median_run_id, _ = run_avg_scores[len(run_avg_scores) // 2]

    run_file = reports_dir / config.output.per_run_analysis_pattern.format(run=median_run_id)
    return parse_run_content(run_file)


def _write_aggregated_report(
    path: Path,
    config: EvalConfig,
    config_names: list[str],
    all_dims: list[str],
    dim_stats: dict,
    dim_tiers: dict,
    dim_weights: dict,
    weighted_per_run: dict[str, list[float]],
    run_ids: list[int],
    all_run_scores: list[dict],
    verif_data: dict | None,
    rich_content: dict[str, str] | None = None,
    generation_usage: list[dict] | None = None,
) -> None:
    """Write the final aggregated analysis markdown report."""
    n_runs = len(run_ids)
    timestamp = datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M UTC")

    lines = [
        f"# Aggregated Analysis: {config.name}",
        "",
        f"**Runs:** {n_runs} | **Configurations:** {len(config_names)} "
        f"| **Scenarios:** {len(config.scenarios)} | **Dimensions:** {len(all_dims)}",
        f"**Date:** {timestamp}",
        "",
        "---",
        "",
    ]

    # Overview
    if config.description:
        lines.extend([
            "## Overview",
            "",
            config.description.strip(),
            "",
            "---",
            "",
        ])

    # What Was Tested
    lines.extend([
        "## What Was Tested",
        "",
    ])

    # Scenarios
    lines.extend([
        "### Scenarios",
        "",
        "Each run generates one of the following application scenarios "
        "(randomly selected per run):",
        "",
        "| Scenario | Description |",
        "|---|---|",
    ])
    for s in config.scenarios:
        lines.append(f"| {s.name} | {s.description} |")
    lines.append("")

    # Configurations
    cfg_lookup = {c.name: c for c in config.configurations}
    lines.extend([
        "### Configurations",
        "",
        "Each configuration gives Copilot different custom skills or plugins. "
        "The **no-skills** baseline uses default Copilot with no custom instructions.",
        "",
        "| Configuration | Description | Skills | Plugins |",
        "|---|---|---|---|",
    ])
    for name in config_names:
        cfg = cfg_lookup.get(name)
        if not cfg:
            continue
        label = cfg.label or name
        skills = ", ".join(cfg.skills) if cfg.skills else "—"
        plugins = ", ".join(cfg.plugins) if cfg.plugins else "—"
        lines.append(f"| {name} | {label} | {skills} | {plugins} |")
    lines.append("")

    # How It Works
    # Detect the model used from generation_usage
    model_name = "—"
    if generation_usage:
        models = {u.get("model") for u in generation_usage if u.get("model")}
        model_name = ", ".join(sorted(models)) if models else "—"

    lines.extend([
        "### How It Works",
        "",
        "1. **Generate** — For each configuration, Copilot CLI (`copilot --yolo`) "
        "is given a scenario prompt and generates a complete project from scratch. "
        "One scenario is randomly selected per run.",
        "2. **Verify** — Each generated project is built (`dotnet build`), "
        "run, format-checked, and scanned for NuGet vulnerabilities.",
        "3. **Analyze** — An AI judge reviews the source code of all "
        "configurations side-by-side and scores each across "
        f"{len(all_dims)} quality dimensions.",
        "",
        f"Generation model: **{model_name}**",
        f"Analysis model: **{config.analysis_model}**",
        "",
        "---",
        "",
    ])

    # Scoring Methodology
    tier_weights = {"critical": 3.0, "high": 2.0, "medium": 1.0, "low": 0.5}
    tier_counts: dict[str, int] = {}
    for d in config.dimensions:
        t = d.tier.lower()
        tier_counts[t] = tier_counts.get(t, 0) + 1
    max_weighted = sum(d.effective_weight * 5 for d in config.dimensions)

    lines.extend([
        "## Scoring Methodology",
        "",
        "Each dimension is scored on a **1–5 scale**:",
        "",
        "| Score | Meaning |",
        "|:---:|---|",
        "| 5 | Excellent — follows all best practices |",
        "| 4 | Good — minor gaps only |",
        "| 3 | Acceptable — some issues present |",
        "| 2 | Below average — significant gaps |",
        "| 1 | Poor — missing or fundamentally wrong |",
        "",
        "Dimensions are grouped into **tiers** that determine their weight "
        "in the final weighted score:",
        "",
        "| Tier | Weight | Dimensions |",
        "|---|:---:|:---:|",
    ])
    for tier_name in ["critical", "high", "medium", "low"]:
        count = tier_counts.get(tier_name, 0)
        if count > 0:
            w = tier_weights[tier_name]
            w_str = f"{w:.0f}" if w == int(w) else f"{w:.1f}"
            lines.append(f"| {tier_name.upper()} | ×{w_str} | {count} |")
    lines.extend([
        "",
        f"**Maximum possible weighted score: {max_weighted:.1f}** "
        f"(all dimensions scoring 5).",
        "Scores shown as **mean ± standard deviation** across runs.",
        "",
        "---",
        "",
    ])

    # Executive Summary
    lines.append("## Executive Summary")
    lines.append("")
    header = "| Dimension [Tier] | " + " | ".join(config_names) + " |"
    sep = "|---|" + "|".join(["---"] * len(config_names)) + "|"
    lines.append(header)
    lines.append(sep)

    for dim in all_dims:
        tier = dim_tiers.get(dim, "MEDIUM")
        cells = []
        for cfg in config_names:
            if cfg in dim_stats.get(dim, {}):
                s = dim_stats[dim][cfg]
                if s["stdev"] > 0:
                    cells.append(f"{s['mean']:.1f} ± {s['stdev']:.1f}")
                else:
                    cells.append(f"{s['mean']:.1f}")
            else:
                cells.append("—")
        lines.append(f"| {dim} [{tier}] | " + " | ".join(cells) + " |")

    lines.extend(["", "---", ""])

    # Final Rankings
    lines.append("## Final Rankings")
    lines.append("")
    max_score_label = f"{max_weighted:.0f}" if max_weighted == int(max_weighted) else f"{max_weighted:.1f}"
    lines.append(f"| Rank | Configuration | Mean Score | % of Max ({max_score_label}) | Std Dev | Min | Max |")
    lines.append("|---|---|---|---|---|---|---|")

    rankings = []
    for cfg in config_names:
        values = weighted_per_run[cfg]
        if values:
            mean = statistics.mean(values)
            stdev = statistics.stdev(values) if len(values) > 1 else 0.0
            rankings.append((cfg, mean, stdev, min(values), max(values)))

    rankings.sort(key=lambda x: x[1], reverse=True)
    medals = ["🥇", "🥈", "🥉"]
    for i, (cfg, mean, stdev, mn, mx) in enumerate(rankings):
        medal = medals[i] if i < len(medals) else f"{i+1}th"
        pct = mean / max_weighted * 100
        lines.append(
            f"| {medal} | {cfg} | {mean:.1f} | {pct:.0f}% | {stdev:.1f} | {mn:.1f} | {mx:.1f} |"
        )

    lines.extend(["", "---", ""])

    # Weighted Score per Run
    lines.append("## Weighted Score per Run")
    lines.append("")
    header = "| Run | " + " | ".join(config_names) + " |"
    sep = "|---|" + "|".join(["---"] * len(config_names)) + "|"
    lines.append(header)
    lines.append(sep)

    for i, run_id in enumerate(run_ids):
        cells = [f"{weighted_per_run[cfg][i]:.1f}" for cfg in config_names]
        lines.append(f"| {run_id} | " + " | ".join(cells) + " |")

    # Add summary row
    mean_cells = []
    for cfg in config_names:
        values = weighted_per_run[cfg]
        mean_cells.append(f"**{statistics.mean(values):.1f}**")
    lines.append(f"| **Mean** | " + " | ".join(mean_cells) + " |")

    lines.extend(["", "---", ""])

    # Verification Summary
    if verif_data and verif_data.get("results"):
        lines.append("## Verification Summary (All Runs)")
        lines.append("")
        lines.append("| Configuration | Build Pass Rate | Run Pass Rate | Avg Warnings |")
        lines.append("|---|---|---|---|")

        for cfg in config_names:
            cfg_results = [r for r in verif_data["results"] if r["config"] == cfg]
            if not cfg_results:
                continue
            total = len(cfg_results)
            build_pass = sum(1 for r in cfg_results if r.get("build_success", False))
            run_pass = sum(1 for r in cfg_results if r.get("run_success", False))
            avg_warnings = statistics.mean(
                [r.get("build_warnings", {}).get("total", 0) for r in cfg_results]
            )
            lines.append(
                f"| {cfg} | {build_pass}/{total} ({build_pass*100//total}%) "
                f"| {run_pass}/{total} ({run_pass*100//total}%) "
                f"| {avg_warnings:.1f} |"
            )

        lines.extend(["", "---", ""])

    # Consistency Analysis
    lines.append("## Consistency Analysis")
    lines.append("")
    lines.append("| Configuration | Score σ | Most Consistent Dim (σ) | Most Variable Dim (σ) |")
    lines.append("|---|---|---|---|")

    for cfg in config_names:
        values = weighted_per_run[cfg]
        if not values or len(values) < 2:
            continue
        stdev = statistics.stdev(values)

        # Find most/least consistent dimension
        best_dim, best_std = "", float("inf")
        worst_dim, worst_std = "", 0.0
        for dim in all_dims:
            if cfg in dim_stats.get(dim, {}):
                s = dim_stats[dim][cfg]
                if s["stdev"] < best_std:
                    best_std = s["stdev"]
                    best_dim = dim
                if s["stdev"] > worst_std:
                    worst_std = s["stdev"]
                    worst_dim = dim

        lines.append(
            f"| {cfg} | {stdev:.1f} "
            f"| {best_dim} ({best_std:.1f}) "
            f"| {worst_dim} ({worst_std:.1f}) |"
        )

    lines.extend(["", "---", ""])

    # Per-Dimension Breakdown
    lines.append("## Per-Dimension Analysis")
    lines.append("")

    for dim_idx, dim in enumerate(all_dims, 1):
        tier = dim_tiers.get(dim, "MEDIUM")
        weight = dim_weights.get(dim, 1.0)
        lines.append(f"### {dim_idx}. {dim} [{tier} × {weight:.0f}]")
        lines.append("")

        # Score table
        lines.append("#### Scores Across Runs")
        lines.append("")
        header = "| Run | " + " | ".join(config_names) + " |"
        sep = "|---|" + "|".join(["---"] * len(config_names)) + "|"
        lines.append(header)
        lines.append(sep)

        for i, run_id in enumerate(run_ids):
            cells = []
            for cfg in config_names:
                run_scores = all_run_scores[i]
                if dim in run_scores and cfg in run_scores[dim]:
                    cells.append(str(int(run_scores[dim][cfg])))
                else:
                    cells.append("—")
            lines.append(f"| {run_id} | " + " | ".join(cells) + " |")

        # Mean row
        mean_cells = []
        for cfg in config_names:
            if cfg in dim_stats.get(dim, {}):
                mean_cells.append(f"**{dim_stats[dim][cfg]['mean']:.1f}**")
            else:
                mean_cells.append("—")
        lines.append(f"| **Mean** | " + " | ".join(mean_cells) + " |")
        lines.append("")

        # Rich content from the representative per-run analysis
        if rich_content:
            # Try exact match first, then fuzzy
            content = rich_content.get(dim)
            if not content:
                for key, val in rich_content.items():
                    if dim.lower() in key.lower() or key.lower() in dim.lower():
                        content = val
                        break
            if content:
                lines.append("#### Analysis")
                lines.append("")
                lines.append(content)
                lines.append("")

    # Asset Usage Summary (from session tracing) — operational detail at end
    if generation_usage:
        has_traces = any(u.get("session_id") for u in generation_usage)
        if has_traces:
            lines.extend([
                "---",
                "",
                "## Asset Usage Summary",
                "",
                "| Configuration | Run | Session ID | Model | Skills Loaded | Plugins | Match? |",
                "|---|---|---|---|---|---|---|",
            ])

            any_mismatch = False
            for u in generation_usage:
                if u.get("config") not in config_names:
                    continue
                sid = u.get("session_id", "—")
                if sid and len(sid) > 12:
                    sid_short = sid[:8] + "…" + sid[-4:]
                else:
                    sid_short = sid or "—"
                model = u.get("model", "—")
                resources = u.get("loaded_resources", [])
                skill_names = [r["name"] for r in resources if r.get("resource_type") == "skill"]
                plugin_names = list(dict.fromkeys(
                    r["plugin_name"] for r in resources if r.get("plugin_name")
                ))
                skills_str = ", ".join(skill_names) if skill_names else "—"
                plugins_str = ", ".join(plugin_names) if plugin_names else "—"
                comp = u.get("resource_comparison", {})
                match = comp.get("match", True) if comp else True
                if not match:
                    any_mismatch = True
                match_str = "✅" if match else "⚠️ Mismatch"
                lines.append(
                    f"| {u.get('config', '?')} | {u.get('run_id', '?')} "
                    f"| {sid_short} | {model} | {skills_str} | {plugins_str} | {match_str} |"
                )

            lines.append("")

            if any_mismatch:
                contaminated_runs = [
                    u for u in generation_usage
                    if u.get("resource_comparison", {}).get("contaminated")
                ]
                if contaminated_runs:
                    lines.extend([
                        "### 🚨 Skill Contamination Detected",
                        "",
                        "The following runs loaded skills from outside their configured "
                        "directories. **Scores for these configurations may be inflated "
                        "or deflated** because the model had access to skills it should "
                        "not have seen.",
                        "",
                        "| Configuration | Run | Contaminating Skill | Loaded From |",
                        "|---|---|---|---|",
                    ])
                    for u in contaminated_runs:
                        comp = u.get("resource_comparison", {})
                        for c in comp.get("contaminated", []):
                            lines.append(
                                f"| {u.get('config', '?')} | {u.get('run_id', '?')} "
                                f"| {c['name']} | {c.get('path', '?')} |"
                            )
                    lines.append("")
                else:
                    lines.extend([
                        "### ⚠️ Asset Notes",
                        "",
                        "Some runs had missing expected skills or plugins. "
                        "Review the session events.jsonl files for details.",
                        "",
                    ])

    # Data references
    lines.extend([
        "---",
        "",
        "## Raw Data References",
        "",
    ])
    for run_id in run_ids:
        run_file = config.output.per_run_analysis_pattern.format(run=run_id)
        lines.append(f"- Per-run analysis: `{config.output.reports_directory}/{run_file}`")
    lines.extend([
        f"- Verification data: `{config.output.reports_directory}/{config.output.verification_data_file}`",
        f"- Score data: `{config.output.reports_directory}/{config.output.scores_data_file}`",
        f"- Build notes: `{config.output.reports_directory}/{config.output.notes_file}`",
        "",
    ])

    path.write_text("\n".join(lines), encoding="utf-8")
