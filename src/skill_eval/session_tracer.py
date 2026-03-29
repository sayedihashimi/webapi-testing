"""Parse Copilot CLI session events to extract loaded skills/plugins.

After each Copilot invocation the CLI writes telemetry to an events.jsonl
file under ``~/.copilot/session-state/{session-id}/``.  This module reads
that file and returns a structured summary of the session metadata and
every skill that was loaded (``skill.invoked`` events).
"""

from __future__ import annotations

import json
import shutil
from dataclasses import asdict, dataclass, field
from pathlib import Path


# Event types we care about --------------------------------------------------

_RESOURCE_EVENT_TYPES = frozenset({"skill.invoked"})
_META_EVENT_TYPES = frozenset({
    "session.start",
    "session.model_change",
    "user.message",
})


# Data models -----------------------------------------------------------------

@dataclass
class LoadedResource:
    """A skill or plugin that Copilot loaded during the session."""

    resource_type: str = "skill"
    name: str = ""
    path: str | None = None
    plugin_name: str | None = None
    description: str | None = None
    content_length: int = 0
    timestamp: str | None = None


@dataclass
class SessionTrace:
    """Summary of a Copilot CLI session parsed from events.jsonl."""

    session_id: str | None = None
    model: str | None = None
    copilot_version: str | None = None
    start_time: str | None = None
    cwd: str | None = None
    prompt: str | None = None
    resources: list[LoadedResource] = field(default_factory=list)
    total_events: int = 0
    events_file: str | None = None

    # Convenience helpers -----------------------------------------------------

    @property
    def skill_names(self) -> list[str]:
        return [r.name for r in self.resources if r.resource_type == "skill"]

    @property
    def plugin_names(self) -> list[str]:
        names: list[str] = []
        for r in self.resources:
            if r.plugin_name and r.plugin_name not in names:
                names.append(r.plugin_name)
        return names

    def to_dict(self) -> dict:
        """Serialise to a plain dict (for JSON output)."""
        return asdict(self)


# Parsing ---------------------------------------------------------------------

def _truncate(text: str | None, length: int = 200) -> str | None:
    if text is None:
        return None
    return text[:length] + "…" if len(text) > length else text


def parse_events_file(events_path: Path) -> SessionTrace:
    """Parse an ``events.jsonl`` file and return a :class:`SessionTrace`."""
    trace = SessionTrace(events_file=str(events_path))
    first_user_seen = False

    with open(events_path, encoding="utf-8") as fh:
        for line in fh:
            line = line.strip()
            if not line:
                continue
            try:
                event = json.loads(line)
            except json.JSONDecodeError:
                continue

            trace.total_events += 1
            etype = event.get("type", "")
            data = event.get("data", {})

            if etype == "session.start":
                trace.session_id = data.get("sessionId")
                trace.model = data.get("selectedModel")
                trace.copilot_version = data.get("copilotVersion")
                trace.start_time = data.get("startTime")
                ctx = data.get("context", {})
                trace.cwd = ctx.get("cwd")

            elif etype == "session.model_change":
                trace.model = data.get("newModel") or trace.model

            elif etype == "user.message" and not first_user_seen:
                first_user_seen = True
                trace.prompt = _truncate(data.get("content"))

            elif etype == "skill.invoked":
                content = data.get("content", "")
                trace.resources.append(LoadedResource(
                    resource_type="skill",
                    name=data.get("name", "unknown"),
                    path=data.get("path"),
                    plugin_name=data.get("pluginName") or None,
                    description=_truncate(data.get("description"), 120),
                    content_length=len(content) if content else 0,
                    timestamp=event.get("timestamp"),
                ))

    return trace


# Session discovery -----------------------------------------------------------

def _find_latest_events_file(
    copilot_home: Path,
    created_after: float | None = None,
) -> Path | None:
    """Return the most recently modified ``events.jsonl`` under *copilot_home*.

    Parameters
    ----------
    created_after:
        If provided, only consider session directories whose ``events.jsonl``
        was modified after this Unix timestamp.  This helps locate the correct
        session when multiple Copilot invocations run close together.
    """
    session_state = copilot_home / "session-state"
    if not session_state.is_dir():
        return None

    candidates: list[tuple[float, Path]] = []
    for d in session_state.iterdir():
        ef = d / "events.jsonl"
        if ef.is_file():
            mtime = ef.stat().st_mtime
            if created_after is not None and mtime < created_after:
                continue
            candidates.append((mtime, ef))

    if not candidates:
        return None

    candidates.sort(reverse=True)
    return candidates[0][1]


def trace_session(
    copilot_home: Path | None = None,
    created_after: float | None = None,
) -> SessionTrace | None:
    """Find the most recent ``events.jsonl`` and parse it.

    Parameters
    ----------
    copilot_home:
        Root of the Copilot home directory (contains ``session-state/``).
        Defaults to ``~/.copilot``.

    Returns ``None`` if no events file can be found.
    """
    if copilot_home is None:
        copilot_home = Path.home() / ".copilot"

    events_path = _find_latest_events_file(copilot_home, created_after)
    if events_path is None:
        return None

    return parse_events_file(events_path)


# Comparison ------------------------------------------------------------------

def compare_resources(
    trace: SessionTrace,
    expected_skills: list[str],
    expected_plugins: list[str],
) -> dict:
    """Compare loaded resources against expected configuration.

    Returns a dict describing matches, unexpected loads, and missing assets.
    """
    actual_skills = list(dict.fromkeys(trace.skill_names))  # dedupe, preserve order
    actual_plugins = list(dict.fromkeys(trace.plugin_names))

    expected_skills_set = set(expected_skills)
    actual_skills_set = set(actual_skills)
    expected_plugins_set = set(expected_plugins)
    actual_plugins_set = set(actual_plugins)

    unexpected_skills = sorted(actual_skills_set - expected_skills_set)
    missing_skills = sorted(expected_skills_set - actual_skills_set)
    unexpected_plugins = sorted(actual_plugins_set - expected_plugins_set)
    missing_plugins = sorted(expected_plugins_set - actual_plugins_set)

    match = (not unexpected_skills and not missing_skills
             and not unexpected_plugins and not missing_plugins)

    return {
        "expected_skills": sorted(expected_skills_set),
        "actual_skills": actual_skills,
        "unexpected_skills": unexpected_skills,
        "missing_skills": missing_skills,
        "expected_plugins": sorted(expected_plugins_set),
        "actual_plugins": actual_plugins,
        "unexpected_plugins": unexpected_plugins,
        "missing_plugins": missing_plugins,
        "match": match,
    }


# Events file preservation ----------------------------------------------------

def preserve_events_file(
    copilot_home: Path,
    destination: Path,
    created_after: float | None = None,
) -> Path | None:
    """Copy the most recent ``events.jsonl`` to *destination*.

    Returns the destination path on success, or ``None``.
    """
    events_path = _find_latest_events_file(copilot_home, created_after)
    if events_path is None:
        return None

    destination.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(events_path, destination)
    return destination
