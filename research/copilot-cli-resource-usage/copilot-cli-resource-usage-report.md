# Comprehensive Report: Evaluating Which Copilot CLI Skill Files and Resources Are Used for a Prompt

**Date:** 2026-03-28  
**Mode:** Deep Research  
**Sources Consulted:** 22 (see sources.md)

---

## Executive Summary

GitHub Copilot CLI provides several officially supported observability mechanisms—hooks, session data, slash commands (`/skills`, `/instructions`), programmatic invocation flags, and session transcripts—but **none of them directly report which skills or resources were selected or injected into model context for a given prompt**. The internal resource resolution process (discovery → selection → injection) happens within the CLI's opaque internal logic, and no hook, log, or API event fires at the selection boundary.

The strongest officially supported approach is **A/B testing**: running the same prompt with a resource enabled versus disabled, and comparing behavioral differences. Combined with **filesystem watchers** (a moderate-risk, OS-level technique) to detect which SKILL.md files Copilot reads during processing, this produces high-confidence evidence of resource selection. Together, these two approaches form the foundation of a practical evaluator.

No true **dry-run mode** exists. The closest safe substitute is running with `--deny-tool=shell --deny-tool=write --no-ask-user`, which allows Copilot to select resources and plan, but prevents any file mutations or command execution. The session transcript captured with `--share` reveals the agent's planning and attempted tool usage, which indirectly indicates skill influence.

**Primary Recommendation:** Build a reusable evaluator using (1) A/B testing with skill toggles and `--no-custom-instructions`, (2) filesystem watchers for file-read detection, (3) hooks for tool-usage logging, and (4) `--share` transcript analysis. Start with Windows-first implementation using Process Monitor for file-read tracking.

---

## Introduction

### Research Question

How can we build a reusable evaluation method for GitHub Copilot CLI that determines, for a given prompt and configuration, which skills and related resources are selected for inclusion and/or actually injected into model context?

### Scope

This research focuses exclusively on GitHub Copilot CLI (the terminal agent). It covers skills, custom instructions, path-specific instructions, agent configurations, plugins, and any other prompt-shaping resources. It does not cover Copilot in VS Code, GitHub.com, or JetBrains IDEs except where their mechanisms inform CLI understanding.

### Methodology

Research proceeded through official GitHub documentation, the CLI's built-in help system, the hooks configuration reference, community investigations, and third-party tools that parse Copilot session data. Twenty-two sources were consulted, with trust levels ranging from 55 to 95 out of 100.

---

## Finding 1: Officially Supported Observability Is Tool-Centric, Not Resource-Centric

GitHub Copilot CLI offers a robust hooks system with six core event types: `sessionStart`, `sessionEnd`, `userPromptSubmitted`, `preToolUse`, `postToolUse`, and `errorOccurred`, plus `agentStop` and `subagentStop` [3][4][5]. These hooks receive structured JSON input and execute custom shell commands synchronously during agent operation.

However, every hook type operates at the **tool execution** level, not the **resource selection** level. The `preToolUse` hook receives `toolName` (e.g., "bash", "edit", "view") and `toolArgs` (the tool's parameters), but does not receive any information about which skill triggered the tool call, which custom instructions are active, or what resources are in the model's context [4]. The `postToolUse` hook adds `toolResult` (success/failure and output text) but still lacks resource-level metadata [4].

There is no hook event that fires when Copilot selects a skill, loads a SKILL.md file, or injects custom instructions into context. The gap is fundamental: the hooks system was designed for tool-level policy enforcement and logging, not for resource-level observability [5][8].

Session data stored in `~/.copilot/session-state/` and the SQLite session store records prompts, responses, tool calls, and file modifications [6]. The `/chronicle` experimental command provides session-level insights like standup reports and usage tips [6]. However, neither the session data nor `/chronicle` appears to record which skills were selected or which instruction files were loaded for a given prompt. The session timeline tracks conversation turns and tool executions, but skill selection is an internal planning step that precedes visible tool use.

The `/skills list` and `/skills info` commands report which skills are *available* and their metadata, but not which were *used* for a specific prompt [2]. The `/instructions` command shows which instruction files are *active* (i.e., discovered and eligible for loading), but not whether they materially influenced a particular response [22].

**What officially exists:**
- Tool-level hooks with structured JSON input/output
- Session data with conversation and tool logs
- `/skills` and `/instructions` commands for availability enumeration
- `--share` for session transcript export
- `--no-custom-instructions` for disabling all custom instructions

**What does not exist:**
- A "resource selected" or "skill loaded" hook event
- A dry-run mode that reports planned resource usage
- An API to query "what resources are in the current context?"
- Any log entry identifying which skills were selected for a prompt

---

## Finding 2: The Resource Resolution Model Is a Three-Stage Pipeline

Based on official documentation [2] and community analysis [16], Copilot CLI uses a progressive disclosure model for skills:

**Stage 1 — Discovery:** At startup (or when `/skills reload` is called), Copilot scans configured directories for SKILL.md files and reads their YAML frontmatter (name and description). This is lightweight and establishes the set of *available* skills. Skill directories include `.github/skills/`, `.claude/skills/`, `.agents/skills/` (project-level) and `~/.copilot/skills/`, `~/.claude/skills/`, `~/.agents/skills/` (personal-level) [2][9].

**Stage 2 — Selection:** When a user submits a prompt, Copilot evaluates the prompt against skill descriptions to determine relevance. The selection algorithm is internal and undocumented. Skills can also be explicitly invoked with `/skill-name` syntax [2].

**Stage 3 — Loading/Injection:** For selected skills, Copilot loads the full SKILL.md content (and potentially referenced subsidiary files) into the agent's context window. This happens before the model generates a response [2][16].

Custom instructions follow a simpler model: they are *always loaded* when present, unless `--no-custom-instructions` is used [10]. Path-specific instructions (`.github/instructions/**/*.instructions.md`) are loaded when the prompt involves files matching the instruction's `applyTo` glob pattern [10].

**What is known:** The three stages exist and the official documentation confirms the progressive disclosure approach.

**What is inferred:** The selection algorithm likely uses semantic similarity between the prompt and skill descriptions. This inference comes from community sources [16] and the official statement that skills are loaded "when relevant" [2].

**What is unknown:** The exact selection algorithm, threshold for relevance, precedence when multiple skills match, whether all matching skills are loaded or only the best match, and at what point in the pipeline the model context is assembled.

---

## Finding 3: A/B Testing Is the Highest-Confidence Official Method

The most reliable officially supported method for determining resource usage is controlled A/B testing. The CLI provides all the flags needed:

- `--no-custom-instructions` disables all custom instruction files [10]
- `/skills` allows toggling individual skills on/off interactively [2]
- `-p "<prompt>"` enables programmatic, non-interactive execution [7]
- `--share <path>` exports a session transcript for analysis [7]
- `--model <model>` pins the model for reproducibility [7]
- `--no-ask-user` prevents the agent from pausing for input [7]
- `--allow-all` or specific `--allow-tool`/`--deny-tool` controls permissions [7]

**A/B test protocol:**

1. **Variant A (treatment):** Run with the target skill/instruction enabled
2. **Variant B (control):** Run with the target skill/instruction disabled
3. **Compare:** Analyze transcripts for behavioral differences

If the output differs materially between variants—particularly if the treatment variant follows procedures described in the skill—this is strong evidence (70-89 confidence) that the resource was selected, loaded, and influential.

**Strengths:** Fully official, cross-platform, high confidence, requires no OS-level instrumentation.

**Weaknesses:** Requires two runs per resource per prompt (2x cost in API calls). Output variance from model non-determinism can create noise. Cannot distinguish "selected but not influential" from "not selected."

**Important operational detail:** In programmatic mode (`-p`), skill toggling via `/skills` is not available. To control which skills are available, the evaluator must manipulate the filesystem—placing or removing SKILL.md files from the skills directories before each run [2].

---

## Finding 4: Filesystem Watchers Provide the Strongest Selection Evidence

Since Copilot CLI runs as a local process and reads skill files from the filesystem, an OS-level filesystem watcher can detect exactly which files the CLI reads during prompt processing. If Copilot reads `.github/skills/webapp-testing/SKILL.md` during a run, this is strong evidence that the skill was selected [16].

**Windows implementation:** Sysinternals Process Monitor (procmon) can log all `ReadFile` and `CreateFile` operations filtered to the Copilot process. It provides process-level filtering, timestamp precision, and complete file path logging. ETW (Event Tracing for Windows) is an alternative for programmatic integration.

**macOS implementation:** `fs_usage` or `dtrace` can trace file system calls. `fswatch` can detect writes but may not capture reads.

**Linux implementation:** `inotifywait` (from inotify-tools) can watch for `ACCESS` events (reads). `strace` can capture all system calls including `open` and `read`.

**Confidence level:** If the filesystem watcher detects a read of SKILL.md during the prompt processing window, this is Strong Inference (confidence 70-89) that the skill was selected. It cannot confirm *injection into model context* because the CLI could theoretically read a file without including it in the prompt, but this scenario is unlikely given the progressive disclosure model.

**Risks:** Filesystem watchers are OS-specific and add implementation complexity. Process Monitor on Windows requires administrator privileges for some features. The Copilot CLI process name and tree must be correctly identified (it may spawn child processes). Performance overhead is minimal for file-event monitoring.

---

## Finding 5: Hooks Enable Tool-Level Correlation But Not Direct Resource Detection

While hooks cannot identify which skills were loaded, they can detect **tool usage patterns** that correlate with specific skills. A skill that instructs Copilot to "always run `npm test` first" will produce a `preToolUse` event with `toolName: "bash"` and `toolArgs` containing `npm test`. By matching observed tool patterns against known skill instructions, the evaluator can infer skill influence.

This is Weak Inference (confidence 30-69) when used alone, but becomes Strong Inference when combined with other evidence sources (filesystem watcher + tool pattern matching + output analysis).

The `preToolUse` hook's ability to return `{"permissionDecision": "deny"}` is valuable for early-stop strategies. A hook that logs all tool attempts and then denies them creates a record of what Copilot *planned to do* without actually executing [4]. This "log and deny" pattern is the closest available substitute for a dry-run mode.

The `subagentStop` hook fires when a subagent completes, providing visibility into delegated work. If Copilot delegates a task to a custom agent (which may use different skills), the subagentStop hook can capture the subagent's results before they return to the main agent [5].

---

## Finding 6: No True Dry-Run Mode Exists

The research conclusively found that no official dry-run, inspect, trace, or explain mode exists for Copilot CLI that would report "for this prompt, I would select these resources" without proceeding to full execution [7].

**Closest substitutes:**

1. **Constrained execution with `--deny-tool`:** Running with all mutation tools denied forces Copilot to plan but prevents action. The session transcript reveals the plan, including any skill-influenced procedures. This costs one API call but prevents file changes.

2. **Hook-based abort:** A `preToolUse` hook that logs and denies all tools. The agent will attempt tool calls (revealing its plan) but none will execute. The agent may still produce a text response that shows skill influence.

3. **Extremely restrictive permissions:** `--deny-tool=shell --deny-tool=write --deny-tool=read --no-ask-user` denies nearly everything, forcing the agent to respond from context alone. The response may still reflect loaded skills and instructions.

None of these substitutes can stop execution *between* skill selection and context loading. By the time any hook fires, skills have already been selected and loaded into context. The hooks fire at tool *execution* time, which is after context assembly.

---

## Can We Know What Copilot Selected Before It Finishes?

**Short answer: Partially, with significant caveats.**

**What we can know before finish:**
- Which tools Copilot *attempts* to use (via `preToolUse` hook, which fires before the tool executes)
- That we can *deny* those tools and capture the plan without execution
- Which files Copilot *reads* from disk (via filesystem watcher, which runs concurrently)

**What we cannot know before finish:**
- The complete list of skills that were selected (no selection event)
- The contents of the model context window (assembled internally)
- Whether a skill was loaded but not influential (context was loaded but model ignored it)

**What we can know after finish:**
- The full session transcript (via `--share`)
- All tool calls and results (via hooks + session data)
- Behavioral differences via A/B comparison
- Filesystem access log (via watcher)

**Net assessment:** We can obtain *strong inference* about resource selection through a combination of filesystem watching, constrained execution, and A/B testing. We cannot obtain *direct observation* of context injection through any available mechanism. The evidence model (see copilot-cli-evidence-model.md) provides a rigorous framework for scoring and classifying the resulting evidence.

---

## Finding 7: Session Data and Transcript Provide Post-Hoc Evidence

The `--share` flag exports a session transcript to a Markdown file containing the full conversation, including tool calls and their results [7]. This transcript can be parsed for:

- Skill name mentions (e.g., "Using the webapp-testing skill...")
- Tool usage patterns that match skill instructions
- Output format markers consistent with skill formatting rules
- References to skill resources ("As described in the skill's procedure...")

Session state files in `~/.copilot/session-state/` contain structured JSON with a timeline of events, message log, and tool log [6][11]. The SQLite session store provides queryable access via tools like copilot-session-tools [14] and Dispatch [13].

These are post-hoc evidence sources—they cannot stop execution early—but they provide the richest data for analysis. Combined with filesystem watcher data (which captures the "when" of file reads) and hook data (which captures tool-level events), session data enables comprehensive post-run analysis.

---

## Finding 8: The Copilot SDK Offers an Alternative Evaluation Path

The official Copilot SDK (available for Python, Node.js, Go, and .NET) provides programmatic access to Copilot sessions with event streaming, permission hooks, and session management [20]. The Python SDK offers async support and real-time event callbacks.

If the SDK exposes resource-selection events that the CLI does not surface through hooks, it could provide a superior instrumentation path. However, the SDK's event model appears to mirror the CLI's—tool-level events, not resource-level events. This needs empirical validation.

The SDK's main advantage for an evaluator is tighter programmatic control: session creation, prompt submission, event handling, and session teardown can all be managed from a test harness without shell-level orchestration. The trade-off is that the SDK's resource resolution may differ from the CLI's.

---

## Synthesis & Insights

### Pattern: Observability Gap at the Selection Boundary

Across all mechanisms examined, there is a consistent gap: no event fires between "Copilot decides which skills are relevant" and "Copilot starts executing tools." The hooks system provides excellent observability at the tool level, and session data provides excellent observability at the conversation level, but the middle layer—resource selection and context assembly—is opaque.

This gap is likely by design: resource selection is part of the model's reasoning process, not a discrete, hookable event in the CLI's execution pipeline. Skills are loaded into the model's context as part of prompt assembly, which happens before the first tool call.

### Insight: Filesystem Watching Bridges the Gap

The filesystem watcher approach is uniquely positioned to bridge this gap because skill loading *requires* reading files from disk. Even though the CLI doesn't emit a "skill selected" event, the act of reading SKILL.md from a skill directory is a detectable side effect of selection. This makes filesystem watching the single most valuable instrumentation technique for an evaluator, despite being unofficial and OS-specific.

### Insight: Canary Markers Transform Weak Inference into Strong Inference

By designing skills with unique, unambiguous markers (see copilot-cli-validation-plan.md), the evaluator can transform output analysis from Weak Inference (30-69 confidence) into Strong Inference (70-89 confidence). If a skill says "always include CANARY-XYZ in output" and CANARY-XYZ appears, the skill was not merely selected—it was loaded, injected, read by the model, and influenced the output.

---

## Limitations & Caveats

### Model Non-Determinism

LLM outputs are non-deterministic. A skill may be loaded but the model might not follow its instructions consistently. Canary marker detection may fail 5-20% of the time even when the skill is loaded. Multiple runs per experiment mitigate this.

### Undocumented Internals

The skill selection algorithm, context assembly process, and session data schema are all undocumented. This research relies on official documentation for external behavior and community analysis for internal structure. Internal details may change without notice.

### Resource vs. Influence

Even the strongest evidence approach (A/B + filesystem watcher + canary markers) cannot distinguish between "loaded and influential" and "loaded and coincidentally matching." The model might produce canary-marker-like output by chance, though this is extremely unlikely with well-designed markers.

### Cost

Each evaluation run costs one premium Copilot request. A/B testing doubles the cost. Running the full experiment matrix (12+ experiments × 3 repeats × 2 A/B variants) could consume 72+ requests.

---

## Recommendations

### Best Official/Low-Risk Path

1. Enumerate available resources with `/skills list`, `/skills info`, `/instructions`
2. Use A/B testing with `--no-custom-instructions` and skill directory manipulation
3. Capture evidence with `--share` and hooks
4. Analyze transcripts and tool patterns post-hoc
5. Use canary-marker skills for high-confidence validation

### Best Practical Path

All of the above, plus:
1. Add filesystem watcher (Process Monitor on Windows) for file-read detection
2. Implement "log and deny" preToolUse hook for early-stop behavior
3. Build automated scenario runner with COPILOT_HOME isolation

### Minimum Viable Evaluator

1. A set of canary-marker test skills
2. A script that runs `copilot -p <prompt> --share <path> --no-ask-user --allow-all`
3. A parser that greps the transcript for canary markers
4. A simple A/B comparison (with/without each skill)

### Strongest Long-Term Architecture

See copilot-cli-evaluator-architecture.md for the full design. Key components:
- Automated scenario runner with COPILOT_HOME isolation
- Filesystem watcher integration
- Hook-based tool logging
- A/B differential analysis
- Evidence scoring per copilot-cli-evidence-model.md
- Validation via copilot-cli-validation-plan.md

---

## Recommended Implementation Sequence

### Phase 1: Quick Experiment (Hours)

1. Create 2-3 synthetic test skills with unique canary markers
2. Place them in `.github/skills/` in a test repository
3. Run `copilot -p "prompt matching skill" --share ./transcript.md --allow-all --no-ask-user`
4. Check transcript for canary markers
5. Repeat with skills removed to confirm A/B difference

**Goal:** Validate that canary markers work and A/B testing produces detectable differences.

### Phase 2: Minimum Viable Evaluator (Days)

1. Create scenario definition format (YAML: prompt, expected skills, canary markers)
2. Build runner script (PowerShell or Python) that:
   - Sets up COPILOT_HOME in a temp directory
   - Copies/symlinks target skills to .github/skills/
   - Installs hooks.json with tool logging
   - Runs `copilot -p` with --share
   - Parses transcript for markers
   - Compares A/B variants
3. Generate simple Markdown report

### Phase 3: Stronger Evaluator (Weeks)

1. Add filesystem watcher (Process Monitor on Windows)
2. Add evidence scoring model
3. Add session store analysis
4. Build HTML report generator
5. Add support for custom instruction testing
6. Add experiment matrix runner
7. Add confidence scoring and classification

### Phase 4: Advanced Instrumentation (Optional)

1. Copilot SDK-based runner (if SDK provides better events)
2. Cross-platform filesystem watcher support
3. CI integration (GitHub Actions workflow)
4. Statistical analysis of multi-run confidence variance
5. Integration with skill development workflow

---

## Bibliography

[1] GitHub (2026). "GitHub Copilot CLI README." GitHub Repository. https://github.com/github/copilot-cli (Retrieved: 2026-03-28)

[2] GitHub (2026). "Creating agent skills for GitHub Copilot CLI." GitHub Docs. https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/create-skills (Retrieved: 2026-03-28)

[3] GitHub (2026). "Using hooks with GitHub Copilot CLI." GitHub Docs. https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/use-hooks (Retrieved: 2026-03-28)

[4] GitHub (2026). "Hooks configuration." GitHub Docs. https://docs.github.com/en/copilot/reference/hooks-configuration (Retrieved: 2026-03-28)

[5] GitHub (2026). "About hooks." GitHub Docs. https://docs.github.com/en/copilot/concepts/agents/coding-agent/about-hooks (Retrieved: 2026-03-28)

[6] GitHub (2026). "About GitHub Copilot CLI session data." GitHub Docs. https://docs.github.com/en/copilot/concepts/agents/copilot-cli/chronicle (Retrieved: 2026-03-28)

[7] GitHub (2026). "GitHub Copilot CLI programmatic reference." GitHub Docs. https://docs.github.com/en/copilot/reference/copilot-cli-reference/cli-programmatic-reference (Retrieved: 2026-03-28)

[8] GitHub (2026). "Comparing GitHub Copilot CLI customization features." GitHub Docs. https://docs.github.com/en/copilot/concepts/agents/copilot-cli/comparing-cli-features (Retrieved: 2026-03-28)

[9] GitHub (2026). "About agent skills." GitHub Docs. https://docs.github.com/en/copilot/concepts/agents/about-agent-skills (Retrieved: 2026-03-28)

[10] GitHub (2026). "Adding custom instructions for GitHub Copilot CLI." GitHub Docs. https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/add-custom-instructions (Retrieved: 2026-03-28)

[11] DeepWiki (2026). "Session Management & History." DeepWiki. https://deepwiki.com/github/copilot-cli/3.3-session-management-and-history (Retrieved: 2026-03-28)

[12] DeepWiki (2026). "Session State & Lifecycle Management." DeepWiki. https://deepwiki.com/github/copilot-cli/6.2-session-state-and-lifecycle-management (Retrieved: 2026-03-28)

[13] Gallant, J. (2026). "Dispatch — A GitHub Copilot CLI Session Explorer." Blog. https://blog.jongallant.com/2026/03/dispatch-github-copilot-cli-session-explorer/ (Retrieved: 2026-03-28)

[14] Arithmomaniac (2026). "copilot-session-tools." GitHub Repository. https://github.com/Arithmomaniac/copilot-session-tools/ (Retrieved: 2026-03-28)

[15] SmartScope (2026). "GitHub Copilot Hooks Complete Guide." SmartScope Blog. https://smartscope.blog/en/generative-ai/github-copilot/github-copilot-hooks-guide/ (Retrieved: 2026-03-28)

[16] mkabumattar (2026). "Setting Up GitHub Copilot Agent Skills in Your Repository." DevTips. https://mkabumattar.com/devtips/post/github-copilot-agent-skills-setup/ (Retrieved: 2026-03-28)

[17] DXRF (2026). "Copilot CLI Skills: A Practical Guide." Blog. https://dxrf.com/blog/2026/03/03/copilot-cli-skills-practical-guide/ (Retrieved: 2026-03-28)

[18] cloud-eng.nl (2025). "GitHub Copilot Customization." Blog. https://blog.cloud-eng.nl/2025/12/22/copilot-customization/ (Retrieved: 2026-03-28)

[19] htekdev (2026). "GitHub Copilot CLI — Complete Reference." GitHub Pages. https://htekdev.github.io/copilot-cli-reference/ (Retrieved: 2026-03-28)

[20] PyPI (2026). "github-copilot-sdk." Python Package Index. https://pypi.org/project/github-copilot-sdk/ (Retrieved: 2026-03-28)

[21] GitHub (2026). "PreToolUseHookInput (Copilot SDK Java API)." API Documentation. https://github.github.io/copilot-sdk-java/latest/apidocs/com/github/copilot/sdk/json/PreToolUseHookInput.html (Retrieved: 2026-03-28)

[22] GitHub Copilot CLI (2026). Built-in help output (`/help` command). Local CLI. (Retrieved: 2026-03-28)

---

## Appendix: Methodology

### Research Process

- **Phase 1 (SCOPE):** Decomposed the research question into 8 sub-questions covering observability, resource resolution, instrumentation strategies, early-stop mechanisms, evidence modeling, evaluator architecture, validation, and practical recommendations.
- **Phase 2 (PLAN):** Identified 10+ search angles and prioritized official GitHub documentation.
- **Phase 3 (RETRIEVE):** Executed 12 parallel web searches and fetched 8 official documentation pages. Reviewed the CLI's built-in help output. Checked existing research in the repository.
- **Phase 4 (TRIANGULATE):** Cross-referenced hook capabilities across 4 official docs pages. Verified session data structure through official docs and 2 community tools. Confirmed programmatic flags through official reference.
- **Phase 5 (SYNTHESIZE):** Connected findings across sources to identify the observability gap at the selection boundary. Designed the evaluator architecture to work around this gap.
- **Phase 6 (CRITIQUE):** Identified limitations of each approach. Noted that no approach can directly confirm context injection.
- **Phase 7 (REFINE):** Added filesystem watcher strategy details. Strengthened A/B testing methodology.
- **Phase 8 (PACKAGE):** Generated 8 deliverable files.

### Sources Consulted

**Total Sources:** 22  
**Source Types:** Official documentation (12), Community documentation (4), Open-source tools (2), Community blogs (3), Official SDK (1)  
**Trust Level Distribution:** High (>80): 13 sources; Medium (55-80): 9 sources  
**Temporal Coverage:** 2025–2026

### Claims-Evidence Table

| Claim | Evidence Type | Sources | Confidence |
|-------|--------------|---------|------------|
| No hook fires at skill selection | Official documentation review | [3][4][5] | High |
| Progressive disclosure model exists | Official + community docs | [2][16] | High |
| A/B testing is most reliable official method | Official flag documentation | [7][10] | High |
| Filesystem watcher can detect skill reads | Logical inference from architecture | [16], architecture analysis | Medium-High |
| No dry-run mode exists | Exhaustive documentation search | [7], all docs | High |
| Session data records tool calls | Official documentation | [6][11] | High |
| preToolUse hook can deny tools | Official documentation with examples | [3][4] | High |
| Canary markers can detect skill influence | Experimental design (proposed) | Design inference | Medium |
