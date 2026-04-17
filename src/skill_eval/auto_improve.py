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
# Lessons Learned
# ---------------------------------------------------------------------------

class LessonEntry:
    """A single lesson learned from a failed improvement attempt."""

    def __init__(
        self,
        turn: int,
        retry: int,
        attempted_changes: str,
        analysis: str,
        score_before: float,
        score_after: float,
        dimensions_affected: dict[str, float],
        confidence: float = 0.7,
        timestamp: str | None = None,
    ) -> None:
        self.turn = turn
        self.retry = retry
        self.attempted_changes = attempted_changes
        self.analysis = analysis
        self.score_before = score_before
        self.score_after = score_after
        self.dimensions_affected = dimensions_affected
        self.confidence = confidence
        self.timestamp = timestamp or datetime.now(timezone.utc).isoformat()

    def to_dict(self) -> dict:
        return {
            "turn": self.turn,
            "retry": self.retry,
            "attempted_changes": self.attempted_changes,
            "analysis": self.analysis,
            "score_before": self.score_before,
            "score_after": self.score_after,
            "dimensions_affected": self.dimensions_affected,
            "confidence": self.confidence,
            "timestamp": self.timestamp,
        }

    @classmethod
    def from_dict(cls, data: dict) -> LessonEntry:
        return cls(
            turn=data["turn"],
            retry=data["retry"],
            attempted_changes=data.get("attempted_changes", ""),
            analysis=data.get("analysis", ""),
            score_before=data.get("score_before", 0.0),
            score_after=data.get("score_after", 0.0),
            dimensions_affected=data.get("dimensions_affected", {}),
            confidence=data.get("confidence", 0.7),
            timestamp=data.get("timestamp"),
        )


class LessonsLearned:
    """Tracks lessons learned from failed improvement attempts.

    Lessons carry confidence scores (0.0–1.0) and are presented to
    the LLM as soft guidance, not hard rules.
    """

    # Confidence escalation for repeated failures
    CONFIDENCE_INITIAL = 0.7
    CONFIDENCE_SECOND = 0.85
    CONFIDENCE_THIRD = 0.95

    def __init__(self) -> None:
        self.lessons: list[LessonEntry] = []

    def add(self, lesson: LessonEntry) -> None:
        """Add a lesson, escalating confidence if similar attempts have failed before."""
        similar_count = sum(
            1 for l in self.lessons
            if l.turn == lesson.turn  # same turn = same plateau
        )
        if similar_count == 0:
            lesson.confidence = self.CONFIDENCE_INITIAL
        elif similar_count == 1:
            lesson.confidence = self.CONFIDENCE_SECOND
        else:
            lesson.confidence = self.CONFIDENCE_THIRD
        self.lessons.append(lesson)

    def format_for_prompt(self) -> str:
        """Render lessons as structured text for LLM consumption."""
        if not self.lessons:
            return ""

        lines = [
            "## Previous Attempts (Lessons Learned)",
            "",
            "The following approaches were previously tried and did not improve scores.",
            "Consider these as guidance, not absolute rules — they carry the noted",
            "confidence levels. Higher confidence means stronger evidence that the",
            "approach should be avoided.",
            "",
        ]

        for i, lesson in enumerate(self.lessons, 1):
            delta = lesson.score_after - lesson.score_before
            sign = "+" if delta >= 0 else ""
            lines.append(f"### Attempt {i} (Turn {lesson.turn}, Retry {lesson.retry}) "
                         f"— Confidence: {lesson.confidence:.0%}")
            lines.append("")
            lines.append(f"**Score impact:** {lesson.score_before:.2f} → "
                         f"{lesson.score_after:.2f} ({sign}{delta:.2f})")
            lines.append("")
            if lesson.dimensions_affected:
                lines.append("**Dimension changes:**")
                for dim, change in lesson.dimensions_affected.items():
                    d_sign = "+" if change >= 0 else ""
                    lines.append(f"  - {dim}: {d_sign}{change:.2f}")
                lines.append("")
            if lesson.attempted_changes:
                lines.append(f"**What was tried:** {lesson.attempted_changes}")
                lines.append("")
            if lesson.analysis:
                lines.append(f"**Why it didn't work:** {lesson.analysis}")
                lines.append("")

        return "\n".join(lines)

    def save(self, path: Path) -> None:
        """Persist lessons to a JSON file."""
        path.parent.mkdir(parents=True, exist_ok=True)
        data = {
            "lessons": [l.to_dict() for l in self.lessons],
            "updated_at": datetime.now(timezone.utc).isoformat(),
        }
        path.write_text(json.dumps(data, indent=2), encoding="utf-8")

    def load(self, path: Path) -> None:
        """Load lessons from a JSON file, merging with any existing in-memory lessons."""
        if not path.exists():
            return
        try:
            data = json.loads(path.read_text(encoding="utf-8"))
        except (json.JSONDecodeError, OSError):
            return
        for entry_data in data.get("lessons", []):
            try:
                self.lessons.append(LessonEntry.from_dict(entry_data))
            except (KeyError, TypeError):
                continue

    def to_dict_list(self) -> list[dict]:
        """Return all lessons as a list of dicts (for history embedding)."""
        return [l.to_dict() for l in self.lessons]


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


def _read_focused_score(
    scores_path: Path,
    target_config: str,
    focus_dimensions: list[str],
) -> float | None:
    """Compute the mean score across only the *focus_dimensions*.

    Returns the simple average of per-dimension means for the focused
    dimensions, or None if scores can't be read.
    """
    _, per_dim = _read_weighted_average(scores_path, target_config)
    if not per_dim:
        return None

    focused_vals = [per_dim[d] for d in focus_dimensions if d in per_dim]
    if not focused_vals:
        return None

    return sum(focused_vals) / len(focused_vals)


def _auto_detect_lowest_dims(
    per_dim_means: dict[str, float],
    n: int,
) -> list[str]:
    """Return the *n* lowest-scoring dimension names."""
    sorted_dims = sorted(per_dim_means.items(), key=lambda x: x[1])
    return [name for name, _ in sorted_dims[:n]]


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


# ---------------------------------------------------------------------------
# Patch generation
# ---------------------------------------------------------------------------

def _generate_patch(
    skill_paths: list[Path],
    plugin_paths: list[Path],
    backup_root: Path,
    patch_path: Path,
) -> bool:
    """Generate a unified diff patch comparing turn-1 backup to current state.

    Returns True if a non-empty patch was written.
    """
    original_backup = backup_root / "turn-1"
    if not original_backup.exists():
        return False

    import difflib

    patch_lines: list[str] = []

    all_entries: list[tuple[str, int, Path]] = []
    for i, p in enumerate(skill_paths):
        all_entries.append(("skill", i, p))
    for i, p in enumerate(plugin_paths):
        all_entries.append(("plugin", i, p))

    for kind, idx, current_path in all_entries:
        backup_dir = original_backup / f"{kind}-{idx}-{current_path.name}"
        if not backup_dir.is_dir() or not current_path.is_dir():
            continue

        # Collect all files from both original and current
        original_files: set[Path] = set()
        current_files: set[Path] = set()

        for f in backup_dir.rglob("*"):
            if f.is_file():
                original_files.add(f.relative_to(backup_dir))
        for f in current_path.rglob("*"):
            if f.is_file():
                current_files.add(f.relative_to(current_path))

        all_files = sorted(original_files | current_files)

        for rel_file in all_files:
            orig_file = backup_dir / rel_file
            curr_file = current_path / rel_file

            # Use the skill/plugin directory name as the path prefix
            label = f"{current_path.name}/{rel_file}"

            orig_lines = _read_file_lines(orig_file)
            curr_lines = _read_file_lines(curr_file)

            if orig_lines == curr_lines:
                continue

            orig_label = f"a/{label}"
            curr_label = f"b/{label}"

            diff = difflib.unified_diff(
                orig_lines, curr_lines,
                fromfile=orig_label, tofile=curr_label,
                lineterm="",
            )
            diff_lines = list(diff)
            if diff_lines:
                patch_lines.extend(diff_lines)
                patch_lines.append("")  # blank line between file diffs

    if not patch_lines:
        return False

    patch_path.parent.mkdir(parents=True, exist_ok=True)
    patch_path.write_text("\n".join(patch_lines) + "\n", encoding="utf-8")
    return True


def _read_file_lines(path: Path) -> list[str]:
    """Read a file as a list of lines, returning [] if it doesn't exist or is binary."""
    if not path.exists():
        return []
    try:
        return path.read_text(encoding="utf-8").splitlines()
    except (UnicodeDecodeError, OSError):
        return [f"<binary file: {path.name}>"]


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
    focus_dimensions: list[str] | None = None,
    idle_timeout: int = 600,
    lessons_context: str | None = None,
) -> bool:
    """Invoke Copilot CLI to apply the improvements file to skill/plugin sources.

    When *focus_dimensions* is set, the prompt prioritises changes that
    improve those specific dimensions.

    Returns True if the process completed without timing out.
    """
    from skill_eval.generate import _kill_process_tree, _watchdog_wait

    all_paths = skill_paths + plugin_paths
    paths_str = "\n".join(f"  - {p}" for p in all_paths)

    focus_directive = ""
    if focus_dimensions:
        dims_str = ", ".join(focus_dimensions)
        focus_directive = (
            f"\n\nPRIORITY FOCUS: Concentrate on suggestions that improve these "
            f"dimensions: {dims_str}. Apply those suggestions first. "
            f"Skip suggestions for other dimensions unless they are trivial to apply "
            f"and do not risk regressing the focused dimensions.\n"
        )

    # Determine if any paths are plugins (have plugin.json manifests)
    has_plugins = any(
        any((p / loc).is_file() for loc in [
            "plugin.json", ".plugin/plugin.json",
            ".github/plugin/plugin.json", ".claude-plugin/plugin.json",
        ])
        for p in plugin_paths
        if p.is_dir()
    )

    if has_plugins:
        file_rules = (
            f"Rules:\n"
            f"- You may modify existing files within the directories listed above.\n"
            f"- You may CREATE new files and subdirectories within those directories\n"
            f"  (e.g., new skills in skills/, new agents in agents/, hooks.json, .mcp.json).\n"
            f"- Do NOT delete any existing files.\n"
            f"- Do NOT create files outside those directories.\n"
            f"- When creating new SKILL.md files, include proper YAML frontmatter with\n"
            f"  `name` and `description` fields.\n"
            f"- When creating new agent files, include proper YAML frontmatter with\n"
            f"  `name`, `description`, and appropriate tool restrictions.\n"
            f"- When updating plugin.json, preserve existing fields and only add/modify\n"
            f"  what the improvement suggestions specify.\n"
            f"- Make the concrete changes described in the improvements file.\n"
            f"- If a suggestion is vague or unclear, skip it rather than guessing.\n"
            f"- After applying changes, briefly summarize what you changed\n"
            f"  (including any new files created)."
        )
    else:
        file_rules = (
            f"Rules:\n"
            f"- Only modify files within the directories listed above.\n"
            f"- Do NOT delete any files.\n"
            f"- Do NOT create files outside those directories.\n"
            f"- Make the concrete changes described in the improvements file.\n"
            f"- If a suggestion is vague or unclear, skip it rather than guessing.\n"
            f"- After applying changes, briefly summarize what you changed."
        )

    lessons_directive = ""
    if lessons_context:
        lessons_directive = (
            f"\n\nLESSONS FROM PREVIOUS ATTEMPTS:\n"
            f"These approaches were previously tried and didn't help. The confidence "
            f"scores indicate how certain we are about each lesson. Avoid repeating "
            f"these approaches unless you have a clearly different angle.\n\n"
            f"{lessons_context}\n"
        )

    prompt = (
        f"Read the improvement suggestions in the file at {improvements_path}. "
        f"Apply the suggested changes to the skill/plugin source files located at:\n"
        f"{paths_str}\n\n"
        f"{file_rules}"
        f"{focus_directive}"
        f"{lessons_directive}"
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
# Lesson generation
# ---------------------------------------------------------------------------

def _generate_lesson(
    improvements_path: Path,
    score_before: float,
    score_after: float,
    per_dim_before: dict[str, float],
    per_dim_after: dict[str, float],
    turn: int,
    retry: int,
    model: str | None,
    idle_timeout: int = 300,
) -> LessonEntry:
    """Ask the LLM to analyze why the attempted improvements didn't help.

    Returns a LessonEntry with the LLM's analysis.
    """
    from skill_eval.generate import _kill_process_tree, _watchdog_wait

    delta = score_after - score_before
    sign = "+" if delta >= 0 else ""

    # Compute per-dimension changes
    dim_changes: dict[str, float] = {}
    all_dims = set(per_dim_before.keys()) | set(per_dim_after.keys())
    for dim in all_dims:
        before = per_dim_before.get(dim, 0.0)
        after = per_dim_after.get(dim, 0.0)
        dim_changes[dim] = round(after - before, 2)

    # Build dimension change summary
    dim_lines = []
    for dim, change in sorted(dim_changes.items(), key=lambda x: x[1]):
        d_sign = "+" if change >= 0 else ""
        dim_lines.append(f"  - {dim}: {d_sign}{change:.2f}")
    dim_summary = "\n".join(dim_lines)

    prompt = (
        f"Analyze why the improvement suggestions did not work.\n\n"
        f"The improvements file is at: {improvements_path}\n"
        f"Read it to understand what changes were suggested.\n\n"
        f"Score before applying: {score_before:.2f}\n"
        f"Score after applying:  {score_after:.2f} ({sign}{delta:.2f})\n\n"
        f"Per-dimension changes:\n{dim_summary}\n\n"
        f"Please provide a concise analysis (under 200 words) answering:\n"
        f"1. What changes were attempted (brief summary)\n"
        f"2. Why they likely didn't improve the score (or made it worse)\n"
        f"3. What kind of different approach might work better\n\n"
        f"Write your analysis as plain text to stdout. Do NOT modify any files.\n"
        f"Do NOT create any files. Just output your analysis text."
    )

    # Try to capture LLM output
    cmd = ["copilot", "-p", prompt, "--yolo"]
    if model:
        cmd.extend(["--model", model])

    env = {**os.environ, "NODE_OPTIONS": "--max-old-space-size=8192"}

    analysis = ""
    attempted_changes = ""

    try:
        proc = subprocess.Popen(
            cmd, env=env,
            stdout=subprocess.PIPE, stderr=subprocess.PIPE,
        )
        timed_out = _watchdog_wait(proc, idle_timeout)
        if timed_out:
            _kill_process_tree(proc)
            analysis = "Lesson generation timed out."
        elif proc.stdout:
            raw = proc.stdout.read()
            if raw:
                analysis = raw.decode("utf-8", errors="replace").strip()
    except Exception as e:
        analysis = f"Lesson generation failed: {e}"

    # Read the improvements file for a summary of what was attempted
    if improvements_path.exists():
        try:
            content = improvements_path.read_text(encoding="utf-8")
            # Extract the executive summary if present
            for marker in ["## Executive Summary", "## Summary"]:
                idx = content.find(marker)
                if idx >= 0:
                    end = content.find("\n## ", idx + len(marker))
                    if end < 0:
                        end = min(len(content), idx + 1000)
                    attempted_changes = content[idx:end].strip()
                    break
            if not attempted_changes:
                attempted_changes = content[:500].strip()
        except OSError:
            pass

    if not analysis:
        analysis = (
            f"Score changed from {score_before:.2f} to {score_after:.2f} "
            f"({sign}{delta:.2f}). The attempted improvements did not "
            f"meet the minimum improvement threshold."
        )

    return LessonEntry(
        turn=turn,
        retry=retry,
        attempted_changes=attempted_changes,
        analysis=analysis,
        score_before=score_before,
        score_after=score_after,
        dimensions_affected=dim_changes,
    )


# ---------------------------------------------------------------------------
# Stopping conditions
# ---------------------------------------------------------------------------

class StopReason:
    TARGET_REACHED = "target_score_reached"
    PLATEAU = "score_plateau"
    PLATEAU_EXHAUSTED = "plateau_retries_exhausted"
    MAX_TURNS = "max_turns_reached"
    REGRESSION = "score_regression"
    APPLY_FAILED = "apply_failed"
    PIPELINE_FAILED = "pipeline_failed"


def _check_stop(
    history: list[dict],
    target_score: float,
    min_improvement: float,
    max_turns: int,
    use_focused: bool = False,
) -> str | None:
    """Evaluate stopping conditions. Returns a StopReason or None to continue.

    When *use_focused* is True, target and plateau checks use ``focused_score``
    instead of ``weighted_average``.  The regression guard always uses the
    overall ``weighted_average``.
    """
    if not history:
        return None

    latest = history[-1]
    score_key = "focused_score" if use_focused else "weighted_average"
    delta_key = "focused_delta" if use_focused else "delta"
    score = latest.get(score_key)

    # Fall back to weighted_average if focused_score is missing
    if score is None:
        score = latest.get("weighted_average")

    if score is not None and score >= target_score:
        return StopReason.TARGET_REACHED

    if len(history) >= max_turns:
        return StopReason.MAX_TURNS

    # Check for plateau (score delta below threshold)
    if len(history) >= 2:
        delta = latest.get(delta_key)
        if delta is None:
            delta = latest.get("delta")
        if delta is not None and delta < min_improvement:
            return StopReason.PLATEAU

    # Check for regression (2 consecutive decreases) — always uses overall score
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
    StopReason.PLATEAU_EXHAUSTED: "📊 Plateau — all retry attempts exhausted",
    StopReason.MAX_TURNS: "🔄 Maximum turns reached",
    StopReason.REGRESSION: "📉 Score regression — 2 consecutive decreases",
    StopReason.APPLY_FAILED: "❌ Failed to apply improvements",
    StopReason.PIPELINE_FAILED: "❌ Pipeline failed to produce scores",
}


def _format_summary_table(iterations: list[dict], stop_reason: str | None) -> list[str]:
    """Build the score progression table as a list of lines (reused for console and report)."""
    has_focused = any(it.get("focused_score") is not None for it in iterations)
    has_retries = any(it.get("is_retry") for it in iterations)
    lines: list[str] = []

    if has_focused:
        header = "| Turn | Overall | Delta | Focused | F.Delta |"
        sep = "|-----:|--------:|------:|--------:|--------:|"
    else:
        header = "| Turn | Score | Delta |"
        sep = "|-----:|------:|------:|"

    if has_retries:
        header += " Retry |"
        sep += "------:|"

    header += " Status |"
    sep += "--------|"

    lines.append(header)
    lines.append(sep)

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
        elif it.get("is_retry") and it.get("improvements_applied"):
            retry_num = it.get("retry_attempt", "?")
            status = f"✅ Retry {retry_num} succeeded"
        elif it.get("improvements_applied"):
            status = "✅ Improvements applied"
        elif it == iterations[-1] and stop_reason:
            status = _STOP_LABELS.get(stop_reason, stop_reason)
        else:
            status = "—"

        row = f"| {turn} | {score_str} | {delta_str} |"

        if has_focused:
            f_score = it.get("focused_score")
            f_delta = it.get("focused_delta")
            f_score_str = f"{f_score:.2f}" if f_score is not None else "—"
            f_delta_str = f"+{f_delta:.2f}" if f_delta is not None and f_delta >= 0 else (
                f"{f_delta:.2f}" if f_delta is not None else "—"
            )
            row = f"| {turn} | {score_str} | {delta_str} | {f_score_str} | {f_delta_str} |"

        if has_retries:
            retry_attempt = it.get("retry_attempt", 0)
            retry_str = f"{retry_attempt}" if retry_attempt else "—"
            row += f" {retry_str} |"

        row += f" {status} |"
        lines.append(row)

    return lines


def _print_summary(iterations: list[dict], stop_reason: str | None) -> None:
    """Print a score progression table."""
    has_retries = any(it.get("is_retry") for it in iterations)

    click.echo(f"\n{'=' * 60}")
    click.echo("Auto-Improve Summary")
    click.echo(f"{'=' * 60}")

    if has_retries:
        click.echo(f"\n  {'Turn':>4s}  {'Score':>8s}  {'Delta':>8s}  {'Retry':>5s}  Status")
        click.echo(f"  {'----':>4s}  {'-----':>8s}  {'-----':>8s}  {'-----':>5s}  ------")
    else:
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
        elif it.get("is_retry") and it.get("improvements_applied"):
            retry_num = it.get("retry_attempt", "?")
            status = f"✅ Retry {retry_num} succeeded"
        elif it.get("improvements_applied"):
            status = "✅ Improvements applied"
        elif it == iterations[-1] and stop_reason:
            status = _STOP_LABELS.get(stop_reason, stop_reason)
        else:
            status = "—"

        if has_retries:
            retry_attempt = it.get("retry_attempt", 0)
            retry_str = f"{retry_attempt}" if retry_attempt else "—"
            click.echo(f"  {turn:>4}  {score_str:>8s}  {delta_str:>8s}  {retry_str:>5s}  {status}")
        else:
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
    lessons: LessonsLearned | None = None,
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

    # Show focus dimensions if set
    focus_dims = None
    for it in iterations:
        fd = it.get("focus_dimensions")
        if fd:
            focus_dims = fd
            break
    if focus_dims:
        lines.append(f"**Focus dimensions:** {', '.join(focus_dims)}  ")

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

        # Focused score change (if applicable)
        first_focused = eval_iterations[0].get("focused_score")
        last_focused = eval_iterations[-1].get("focused_score")
        if first_focused is not None and last_focused is not None:
            f_delta = last_focused - first_focused
            f_sign = "+" if f_delta >= 0 else ""
            lines.append("### Focused Dimensions Score Change")
            lines.append("")
            lines.append(f"| Metric | Value |")
            lines.append(f"|--------|------:|")
            lines.append(f"| Starting focused score | {first_focused:.2f} |")
            lines.append(f"| Final focused score | {last_focused:.2f} |")
            lines.append(f"| Net change | {f_sign}{f_delta:.2f} |")
            f_pct = (f_delta / first_focused * 100) if first_focused != 0 else 0
            f_sign_pct = "+" if f_pct >= 0 else ""
            lines.append(f"| Percent change | {f_sign_pct}{f_pct:.1f}% |")
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

    # Lessons learned
    if lessons and lessons.lessons:
        lines.append("## Lessons Learned")
        lines.append("")
        lines.append(f"*{len(lessons.lessons)} lesson(s) recorded during this session.*")
        lines.append("")
        for i, lesson in enumerate(lessons.lessons, 1):
            ldelta = lesson.score_after - lesson.score_before
            l_sign = "+" if ldelta >= 0 else ""
            lines.append(f"### Lesson {i} (Turn {lesson.turn}, Retry {lesson.retry}) "
                         f"— Confidence: {lesson.confidence:.0%}")
            lines.append("")
            lines.append(f"**Score impact:** {lesson.score_before:.2f} → "
                         f"{lesson.score_after:.2f} ({l_sign}{ldelta:.2f})")
            lines.append("")
            if lesson.analysis:
                lines.append(f"**Analysis:** {lesson.analysis}")
                lines.append("")
        lines.append("")

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

    # Get focus dimensions if set
    focus_dims: set[str] = set()
    for it in eval_iters:
        fd = it.get("focus_dimensions")
        if fd:
            focus_dims = set(fd)
            break

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

    def _dim_label(name: str) -> str:
        return f"**{name}** 🎯" if name in focus_dims else name

    lines.append("## Per-Dimension Analysis")
    lines.append("")
    if focus_dims:
        lines.append(f"*Dimensions marked with 🎯 were the focus of improvement.*")
        lines.append("")

    if improved:
        improved.sort(key=lambda x: x[3], reverse=True)
        lines.append("### ✅ Improved Dimensions")
        lines.append("")
        lines.append("| Dimension | Start | End | Change |")
        lines.append("|-----------|------:|----:|-------:|")
        for dim, start, end, delta in improved:
            lines.append(f"| {_dim_label(dim)} | {start:.2f} | {end:.2f} | +{delta:.2f} |")
        lines.append("")

    if regressed:
        regressed.sort(key=lambda x: x[3])
        lines.append("### ⚠️ Regressed Dimensions")
        lines.append("")
        lines.append("| Dimension | Start | End | Change |")
        lines.append("|-----------|------:|----:|-------:|")
        for dim, start, end, delta in regressed:
            lines.append(f"| {_dim_label(dim)} | {start:.2f} | {end:.2f} | {delta:.2f} |")
        lines.append("")

    if unchanged:
        lines.append("### ➡️ Unchanged Dimensions")
        lines.append("")
        lines.append("| Dimension | Start | End | Change |")
        lines.append("|-----------|------:|----:|-------:|")
        for dim, start, end, delta in unchanged:
            sign = "+" if delta >= 0 else ""
            lines.append(f"| {_dim_label(dim)} | {start:.2f} | {end:.2f} | {sign}{delta:.2f} |")
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
    focus_dimensions: list[str] | None = None,
    focus_lowest: int | None = None,
    research_dir: Path | None = None,
    max_retries: int = 3,
    lessons_file: Path | None = None,
    no_lessons: bool = False,
) -> None:
    """Run the autonomous improvement loop.

    When *focus_dimensions* is provided, improvement suggestions and stopping
    conditions target those specific dimensions.  When *focus_lowest* is
    provided instead, the N lowest-scoring dimensions are auto-selected
    after the first evaluation turn.

    When *research_dir* is provided, best-practice files from that directory
    are included in the improvement prompt context.

    When *max_retries* > 0, plateau/regression triggers a retry sub-loop
    that rolls back, generates lessons learned, and retries with different
    improvement suggestions.  Retries do not count toward *max_turns*.

    When *no_lessons* is True, lessons learned are disabled entirely.
    """
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

    # Scope the pipeline to only the target configuration so that
    # generate/verify/analyze don't waste time on other configs.
    config.configurations = [target_cfg]

    # Apply model overrides
    if generation_model:
        config.generation_model = generation_model
    if analysis_model:
        config.analysis_model = analysis_model
    if improvement_model:
        config.improvement_model = improvement_model

    # Validate focus dimensions
    if focus_dimensions and focus_lowest:
        raise click.ClickException(
            "--dimensions and --focus-lowest are mutually exclusive."
        )

    if focus_dimensions:
        known_dims = {d.name for d in config.dimensions}
        unknown = [d for d in focus_dimensions if d not in known_dims]
        if unknown:
            available = ", ".join(sorted(known_dims))
            raise click.ClickException(
                f"Unknown dimension(s): {', '.join(unknown)}\n"
                f"Available: {available}"
            )

    # active_focus will be set after the first turn if focus_lowest is used
    active_focus: list[str] | None = focus_dimensions

    reports_dir = project_root / config.output.reports_directory
    reports_dir.mkdir(parents=True, exist_ok=True)
    scores_path = reports_dir / config.output.scores_data_file
    history_path = reports_dir / "auto-improve-history.json"
    backup_root = project_root / ".auto-improve-backups"

    imp_model = improvement_model or config.effective_improvement_model

    # Initialize lessons learned
    lessons = LessonsLearned()
    if not no_lessons:
        default_lessons_path = reports_dir / f"lessons-learned-{target_config_name}.json"
        lessons_path = lessons_file or default_lessons_path
        lessons.load(lessons_path)
        if lessons.lessons:
            click.echo(f"  📚 Loaded {len(lessons.lessons)} lesson(s) from {lessons_path}")
    else:
        lessons_path = None

    click.echo(f"\n{'=' * 60}")
    click.echo("Auto-Improve Loop")
    click.echo(f"{'=' * 60}")
    click.echo(f"  Configuration:     {target_config_name}")
    click.echo(f"  Max turns:         {max_turns}")
    click.echo(f"  Target score:      {target_score}")
    click.echo(f"  Min improvement:   {min_improvement}")
    click.echo(f"  Max retries:       {max_retries}")
    click.echo(f"  Lessons learned:   {'disabled' if no_lessons else 'enabled'}")
    click.echo(f"  Runs/iteration:    {runs_per_iteration}")
    click.echo(f"  Generation model:  {config.generation_model}")
    click.echo(f"  Analysis model:    {config.analysis_model}")
    click.echo(f"  Improvement model: {imp_model}")
    click.echo(f"  Skill paths:       {', '.join(str(p) for p in skill_paths)}")
    click.echo(f"  Plugin paths:      {', '.join(str(p) for p in plugin_paths) or '(none)'}")
    if active_focus:
        click.echo(f"  Focus dimensions:  {', '.join(active_focus)}")
    elif focus_lowest:
        click.echo(f"  Focus lowest:      {focus_lowest} (auto-detect after first turn)")

    iterations: list[dict] = []
    stop_reason: str | None = None

    # Get lessons context for prompts
    lessons_ctx = lessons.format_for_prompt() if not no_lessons else ""

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
            run_generate(config, project_root, resume=False, resolver=resolver,
                        scenario_offset=turn - 1)
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

        # Auto-detect focus dimensions after first turn
        if turn == 1 and focus_lowest and not active_focus:
            active_focus = _auto_detect_lowest_dims(per_dim, focus_lowest)
            if active_focus:
                click.echo(f"  🎯 Auto-selected focus dimensions (lowest {focus_lowest}):")
                for dim_name in active_focus:
                    score_val = per_dim.get(dim_name, 0)
                    click.echo(f"       • {dim_name}: {score_val:.2f}")
            else:
                click.echo("  ⚠️  Could not auto-detect focus dimensions, using all")

        # Compute focused score if dimensions are selected
        focused_score: float | None = None
        focused_delta: float | None = None
        if active_focus:
            focused_score = _read_focused_score(scores_path, target_config_name, active_focus)
            if iterations and focused_score is not None:
                prev_focused = iterations[-1].get("focused_score")
                if prev_focused is not None:
                    focused_delta = focused_score - prev_focused

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
            "focused_score": round(focused_score, 2) if focused_score is not None else None,
            "focused_delta": round(focused_delta, 2) if focused_delta is not None else None,
            "focus_dimensions": active_focus,
            "timestamp": datetime.now(timezone.utc).isoformat(),
            "elapsed_seconds": round(turn_elapsed, 1),
            "improvements_applied": False,
            "retry_attempt": 0,
            "is_retry": False,
        }

        delta_str = f" (delta: {'+' if delta >= 0 else ''}{delta:.2f})" if delta is not None else ""
        click.echo(f"  📈 Overall score: {weighted_avg:.2f}{delta_str}  [{mins}m {secs}s]")
        if focused_score is not None:
            f_delta_str = f" (delta: {'+' if focused_delta >= 0 else ''}{focused_delta:.2f})" if focused_delta is not None else ""
            click.echo(f"  🎯 Focused score: {focused_score:.2f}{f_delta_str}")

        iterations.append(iteration_record)

        # --- Step 3: Check for plateau/regression and retry ---
        check_result = _check_stop(
            iterations, target_score, min_improvement, max_turns,
            use_focused=bool(active_focus),
        )

        # Handle plateau or regression with backtracking retries
        needs_retry = (
            check_result in (StopReason.PLATEAU, StopReason.REGRESSION)
            and max_retries > 0
            and turn > 1  # can't retry the very first turn
        )

        if needs_retry:
            # Save the score/dims before the failed attempt for lesson generation
            prev_iter = iterations[-2] if len(iterations) >= 2 else None
            score_before = prev_iter["weighted_average"] if prev_iter else weighted_avg
            per_dim_before = prev_iter.get("per_dimension", {}) if prev_iter else per_dim

            # Rollback the failed attempt before entering retry loop
            if not no_rollback and delta is not None and delta < 0 and len(iterations) >= 2:
                prev_backup = backup_root / f"turn-{turn - 1}"
                if prev_backup.exists():
                    click.echo(f"  ⚠️  Score decreased — rolling back skill changes")
                    _rollback_skill_dirs(skill_paths, plugin_paths, prev_backup)

            retry_succeeded = False
            for retry_num in range(1, max_retries + 1):
                click.echo(f"\n  {'·' * 40}")
                click.echo(f"  🔄 Retry {retry_num}/{max_retries} (turn {turn})")
                click.echo(f"  {'·' * 40}")

                # Generate a lesson from the failed attempt
                if not no_lessons:
                    click.echo("  📚 Generating lesson from failed attempt...")
                    improvements_file = config.output.improvements_file_pattern.format(
                        config=target_config_name
                    )
                    improvements_path = reports_dir / improvements_file
                    lesson = _generate_lesson(
                        improvements_path=improvements_path,
                        score_before=score_before,
                        score_after=weighted_avg,
                        per_dim_before=per_dim_before,
                        per_dim_after=per_dim,
                        turn=turn,
                        retry=retry_num,
                        model=imp_model,
                    )
                    lessons.add(lesson)
                    lessons_ctx = lessons.format_for_prompt()
                    click.echo(f"  📚 Lesson recorded (confidence: {lesson.confidence:.0%})")

                    # Persist lessons after each one
                    if lessons_path:
                        lessons.save(lessons_path)

                # Rollback to pre-turn state
                pre_turn_backup = backup_root / f"turn-{turn}"
                if not pre_turn_backup.exists() and turn > 1:
                    pre_turn_backup = backup_root / f"turn-{turn - 1}"
                if pre_turn_backup.exists():
                    click.echo(f"  ↩️  Rolling back to pre-attempt state...")
                    _rollback_skill_dirs(skill_paths, plugin_paths, pre_turn_backup)

                # Re-generate improvement suggestions with lessons context
                click.echo("  💡 Generating new improvement suggestions (with lessons)...")
                _clean_previous_outputs(config, project_root)
                try:
                    run_suggest_improvements(
                        config, project_root, resolver, model_override=imp_model,
                        focus_dimensions=active_focus,
                        research_dir=research_dir,
                        lessons_context=lessons_ctx,
                    )
                except Exception as e:
                    click.echo(f"  ❌ Improvement suggestions failed: {e}")
                    continue

                improvements_file = config.output.improvements_file_pattern.format(
                    config=target_config_name
                )
                improvements_path = reports_dir / improvements_file
                if not improvements_path.exists():
                    click.echo(f"  ❌ Improvements file not generated")
                    continue

                # Snapshot and apply
                retry_backup_label = f"turn-{turn}-retry-{retry_num}"
                backup_dir = backup_root / retry_backup_label
                backup_dir.mkdir(parents=True, exist_ok=True)
                _snapshot_skill_dirs(skill_paths, plugin_paths, turn, backup_root)

                click.echo("  🔧 Applying improvements via Copilot CLI...")
                apply_success = _apply_improvements(
                    improvements_path, skill_paths, plugin_paths, model=imp_model,
                    focus_dimensions=active_focus,
                    lessons_context=lessons_ctx,
                )
                if not apply_success:
                    click.echo("  ❌ Copilot CLI failed or timed out")
                    continue

                # Re-run pipeline to evaluate the retry
                click.echo("  📦 Re-running pipeline to evaluate retry...")
                _clean_previous_outputs(config, project_root)
                config.runs = runs_per_iteration
                try:
                    run_generate(config, project_root, resume=False, resolver=resolver,
                                scenario_offset=turn - 1)
                    if config.verification is not None:
                        run_verify(config, project_root)
                    run_analyze(config, project_root)
                except Exception as e:
                    click.echo(f"  ❌ Retry pipeline failed: {e}")
                    continue

                retry_avg, retry_dim = _read_weighted_average(scores_path, target_config_name)
                if retry_avg is None:
                    click.echo("  ❌ Could not parse retry scores")
                    continue

                retry_delta = retry_avg - score_before
                retry_elapsed = time.monotonic() - turn_start
                r_mins, r_secs = divmod(int(retry_elapsed), 60)

                r_sign = "+" if retry_delta >= 0 else ""
                click.echo(f"  📈 Retry score: {retry_avg:.2f} ({r_sign}{retry_delta:.2f} vs pre-turn)  [{r_mins}m {r_secs}s]")

                # Check if the retry met the minimum improvement
                if retry_delta >= min_improvement:
                    click.echo(f"  ✅ Retry {retry_num} succeeded!")

                    # Update the iteration record with retry results
                    retry_record = {
                        "turn": turn,
                        "weighted_average": round(retry_avg, 2),
                        "delta": round(retry_delta, 2),
                        "per_dimension": {k: round(v, 2) for k, v in retry_dim.items()},
                        "focused_score": None,
                        "focused_delta": None,
                        "focus_dimensions": active_focus,
                        "timestamp": datetime.now(timezone.utc).isoformat(),
                        "elapsed_seconds": round(retry_elapsed, 1),
                        "improvements_applied": True,
                        "retry_attempt": retry_num,
                        "is_retry": True,
                    }

                    # Compute focused score for retry
                    if active_focus:
                        retry_focused = _read_focused_score(scores_path, target_config_name, active_focus)
                        if retry_focused is not None:
                            retry_record["focused_score"] = round(retry_focused, 2)

                    # Replace the failed iteration with the successful retry
                    iterations[-1] = retry_record

                    # Update state for next turn
                    weighted_avg = retry_avg
                    per_dim = retry_dim
                    retry_succeeded = True
                    break
                else:
                    click.echo(f"  ⚠️  Retry {retry_num} didn't meet threshold ({retry_delta:.2f} < {min_improvement})")
                    # Update score state for next lesson
                    weighted_avg = retry_avg
                    per_dim = retry_dim

            if not retry_succeeded:
                click.echo(f"  ❌ All {max_retries} retries exhausted")
                stop_reason = StopReason.PLATEAU_EXHAUSTED
                _write_history(history_path, target_config_name, iterations, stop_reason)
                break

            # Retry succeeded — continue to next turn
            _write_history(history_path, target_config_name, iterations, stop_reason)
            continue

        # Handle non-retryable stops (target reached, max turns, etc.)
        if check_result and check_result not in (StopReason.PLATEAU, StopReason.REGRESSION):
            stop_reason = check_result
            _write_history(history_path, target_config_name, iterations, stop_reason)
            break

        # Handle plateau/regression when retries are disabled (max_retries=0 or turn==1)
        if check_result in (StopReason.PLATEAU, StopReason.REGRESSION) and not needs_retry:
            # Rollback on regression
            if not no_rollback and delta is not None and delta < 0 and len(iterations) >= 2:
                prev_backup = backup_root / f"turn-{turn - 1}"
                if prev_backup.exists():
                    click.echo(f"  ⚠️  Score decreased — rolling back skill changes from turn {turn - 1}")
                    _rollback_skill_dirs(skill_paths, plugin_paths, prev_backup)
            stop_reason = check_result
            _write_history(history_path, target_config_name, iterations, stop_reason)
            break

        # --- Step 5: Generate improvement suggestions ---
        click.echo("  💡 Generating improvement suggestions...")
        try:
            run_suggest_improvements(
                config, project_root, resolver, model_override=imp_model,
                focus_dimensions=active_focus,
                research_dir=research_dir,
                lessons_context=lessons_ctx,
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
            improvements_path, skill_paths, plugin_paths, model=imp_model,
            focus_dimensions=active_focus,
            lessons_context=lessons_ctx,
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
        StopReason.TARGET_REACHED, StopReason.PLATEAU, StopReason.PLATEAU_EXHAUSTED,
        StopReason.MAX_TURNS, None
    ):
        click.echo(f"\n{'─' * 60}")
        click.echo(f"  Final validation pass ({final_runs} runs)")
        click.echo(f"{'─' * 60}")

        _clean_previous_outputs(config, project_root)
        config.runs = final_runs

        try:
            run_generate(config, project_root, resume=False, resolver=resolver,
                        scenario_offset=turn)
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
            "Max retries": max_retries,
            "Lessons learned": "disabled" if no_lessons else "enabled",
            "Runs per iteration": runs_per_iteration,
            "Final runs": final_runs or runs_per_iteration,
            "Generation model": config.generation_model,
            "Analysis model": config.analysis_model,
            "Improvement model": imp_model,
            "Rollback enabled": not no_rollback,
            "Focus dimensions": ", ".join(active_focus) if active_focus else "All",
            "Focus mode": "explicit" if focus_dimensions else ("auto-detect lowest " + str(focus_lowest) if focus_lowest else "none"),
        },
        lessons=lessons if not no_lessons else None,
    )
    click.echo(f"\n  📄 Results report: {results_path}")

    # Generate patch file (always, for both local and git sources)
    patch_path = reports_dir / f"auto-improve-{target_config_name}.patch"
    if backup_root.exists():
        patch_written = _generate_patch(
            skill_paths, plugin_paths, backup_root, patch_path
        )
        if patch_written:
            click.echo(f"  📋 Patch file: {patch_path}")
        else:
            click.echo(f"  ℹ️  No changes detected — patch file not written")

    if hrs:
        click.echo(f"  Total time: {hrs}h {mins}m {secs}s")
    else:
        click.echo(f"  Total time: {mins}m {secs}s")

    click.echo(f"  History: {history_path}")

    # Save lessons learned
    if not no_lessons and lessons_path and lessons.lessons:
        lessons.save(lessons_path)
        click.echo(f"  📚 Lessons learned: {lessons_path} ({len(lessons.lessons)} lesson(s))")

    # Cleanup backups
    if backup_root.exists():
        click.echo(f"  Backups: {backup_root}")
