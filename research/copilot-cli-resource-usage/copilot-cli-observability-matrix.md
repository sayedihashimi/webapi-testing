# Observability Matrix: Copilot CLI Resource Usage Detection

## Overview

This matrix evaluates all known approaches for detecting which skills, custom instructions, and other resources are selected or injected by GitHub Copilot CLI for a given prompt. Approaches are ranked from most officially supported to most fragile.

---

## Observability Matrix

| Approach | Support Level | Can Detect Selection | Can Detect Injection | Can Stop Early | Cross-Platform | Implementation Difficulty | Confidence Quality | Maintenance Risk |
|----------|--------------|---------------------|---------------------|---------------|---------------|--------------------------|-------------------|-----------------|
| **`/skills list` + `/skills info`** | Official | Available skills only | No | N/A (interactive) | Yes | Low | Low (availability only) | Low |
| **`/instructions` command** | Official | Active instruction files | Partial (always-on files confirmed) | N/A (interactive) | Yes | Low | Medium (confirms always-on) | Low |
| **Hooks (preToolUse/postToolUse)** | Official | No (tool-level only) | No | Yes (deny tool) | Yes | Low–Medium | Medium (tool usage only) | Low |
| **Hooks (userPromptSubmitted)** | Official | No | No | No | Yes | Low | Low (prompt logging only) | Low |
| **Hooks (sessionStart/End)** | Official | No | No | No | Yes | Low | Low (session lifecycle only) | Low |
| **`--share` session transcript** | Official | Indirect (mentions) | Indirect (skill references in output) | No | Yes | Low | Medium (post-hoc analysis) | Low |
| **Session state files (JSON)** | Official (data format undocumented) | Indirect (tool calls logged) | Indirect (possible context clues) | No | Yes | Medium | Medium | Medium |
| **Session store (SQLite)** | Official (schema undocumented) | Indirect (turn/tool data) | No | No | Yes | Medium | Medium | Medium |
| **`/chronicle` analysis** | Experimental | Indirect (pattern analysis) | No | No | Yes | Low | Low–Medium | Medium |
| **`--no-custom-instructions` A/B** | Official flag | Yes (by diff) | Yes (by behavioral diff) | No | Yes | Medium | High (controlled experiment) | Low |
| **`/skills` toggle on/off + A/B** | Official mechanism | Yes (by diff) | Yes (by behavioral diff) | No | Yes | Medium | High (controlled experiment) | Low |
| **`--deny-tool=*` constrained run** | Official flags | Partial (forces abort before tool use) | No | Yes | Yes | Medium | Medium | Low |
| **Programmatic `-p` + `--share`** | Official | Same as transcript | Same as transcript | No | Yes | Low–Medium | Medium | Low |
| **Filesystem watcher (procmon/fswatch)** | Unsupported (OS-level) | Yes (file reads detected) | Strong inference | Yes (can halt) | Platform-specific | Medium–High | High (direct file access) | Medium–High |
| **Process tracing (strace/dtrace)** | Unsupported (OS-level) | Yes (system calls) | Strong inference | Yes (can halt) | Platform-specific | High | High | High |
| **Controlled directory manipulation** | Unsupported (workaround) | Yes (by absence/presence) | Yes (by behavioral diff) | No | Yes | Medium | Medium–High | Medium |
| **HTTP proxy / MITM** | Unsupported (fragile) | Potentially (API payload) | Yes (if context visible) | Yes (can block) | Yes (with setup) | High | Potentially very high | Very High |
| **Binary/package patching** | Unsupported (fragile) | Yes (intercept internals) | Yes | Yes | No | Very High | High | Very High |
| **Copilot SDK programmatic** | Official SDK | Depends on SDK events | Depends on SDK events | Yes (session control) | Yes | Medium–High | Unknown (SDK maturity) | Medium |

---

## Column Definitions

| Column | Definition |
|--------|-----------|
| **Support Level** | Official = documented by GitHub; Experimental = behind feature flag; Unsupported = not intended for this use |
| **Can Detect Selection** | Whether this approach can determine that Copilot chose a specific resource as relevant for the prompt |
| **Can Detect Injection** | Whether this approach can confirm the resource content was placed into the model's context window |
| **Can Stop Early** | Whether execution can be halted after resource selection but before full task processing |
| **Cross-Platform** | Works on Windows, macOS, and Linux without modification |
| **Implementation Difficulty** | Low = hours; Medium = days; High = weeks; Very High = significant R&D |
| **Confidence Quality** | How trustworthy the evidence from this approach is (Low/Medium/High) |
| **Maintenance Risk** | Risk that CLI updates will break this approach (Low/Medium/High/Very High) |

---

## Key Insights

### Best Official Approaches

1. **A/B Testing via `/skills` toggle + `--no-custom-instructions`**: The highest-confidence officially supported method. Run the same prompt twice—once with a resource enabled, once disabled. Compare outputs. If behavior changes materially, the resource was both selected and influential.

2. **Hooks (preToolUse/postToolUse)**: Best for observing *what the agent does* after resource loading, but cannot identify *which resources were loaded*. Useful for detecting skill-prescribed tool patterns.

3. **`--share` + session analysis**: Post-hoc analysis of the session transcript can reveal skill references, tool usage patterns, and behavioral indicators.

### Best Practical Approach (Moderate Risk)

4. **Filesystem watcher**: The strongest single approach for detecting resource *selection*. If Copilot reads a SKILL.md file during prompt processing, it was almost certainly selected. Platform-specific but very high confidence.

### What No Approach Can Do Today

- **No approach can directly confirm context injection.** The model's context window is assembled server-side (or in the CLI's internal logic) and is not exposed through any official or unofficial mechanism.
- **No hook type fires when a skill is selected.** The preToolUse hook fires for tools like `bash`, `edit`, `view`, etc.—not for the internal act of selecting and loading a skill.
- **No official dry-run mode exists** that reports "I would load these resources" without proceeding.

---

## Recommended Combinations

### Minimum Viable Evaluator
- `/skills list` + `/instructions` (enumerate available resources)
- A/B testing with skill toggle (detect selection by behavioral difference)
- `--share` transcript analysis (capture evidence)

### Strong Evaluator
- All of the above, plus:
- Filesystem watcher during prompt runs (detect file reads)
- Hooks for tool-level logging (correlate tool usage to skills)
- Session store analysis (cross-session patterns)

### Maximum Evidence Evaluator
- All of the above, plus:
- Controlled directory manipulation (symlinks, missing files)
- HTTP proxy for API payload inspection (if legal and ethical for your context)
