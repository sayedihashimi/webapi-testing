# Executive Summary: Copilot CLI Resource Usage Evaluation

**Date:** 2026-03-28

---

## The Question

Can we determine which skills, custom instructions, and other resources GitHub Copilot CLI selects and injects into model context for a given prompt?

## The Answer

**Partially.** No single mechanism directly reports resource selection or context injection. However, a combination of officially supported features and practical instrumentation produces high-confidence evidence.

---

## Key Findings

1. **No resource-level observability exists.** Hooks fire at the tool level (bash, edit, view), not the skill-selection level. No hook, log, or API event fires when Copilot selects a skill or loads custom instructions into context.

2. **No dry-run mode exists.** There is no way to ask Copilot "what would you load for this prompt?" without running the prompt. The closest substitute is running with `--deny-tool` flags to prevent mutations while capturing the plan.

3. **A/B testing is the best official method.** Run the same prompt with a resource enabled vs. disabled. Use `--no-custom-instructions` to toggle instructions. Manipulate `.github/skills/` to toggle skills. Compare `--share` transcripts for behavioral differences.

4. **Filesystem watchers provide the strongest selection evidence.** When Copilot reads a SKILL.md file, it was almost certainly selected. Process Monitor (Windows), fs_usage/dtrace (macOS), or inotifywait/strace (Linux) can detect these reads.

5. **Canary markers make skill influence detectable.** Skills designed with unique output markers (e.g., `CANARY-SKILL-ALPHA-7X9`) transform output analysis from weak inference into strong evidence of both loading and influence.

---

## Recommended Path

| Phase | What | Effort | Confidence Gained |
|-------|------|--------|-------------------|
| 1. Quick Experiment | Create canary skills, run A/B with `--share`, grep for markers | Hours | Medium-High |
| 2. Minimum Viable Evaluator | Scenario runner + hooks + transcript parser + A/B comparison | Days | High |
| 3. Strong Evaluator | Add filesystem watcher + evidence scoring + session analysis | Weeks | Very High |
| 4. Advanced (Optional) | SDK-based runner + CI integration + cross-platform support | Weeks | Maximum |

---

## Critical Gaps

- **Context injection is never directly observable.** All approaches infer injection from side effects.
- **Skill selection algorithm is undocumented.** We can observe what was selected but not why.
- **Model non-determinism adds noise.** Multiple runs per experiment are required for statistical confidence.
- **Each run costs one premium Copilot request.** Full experiment matrix can be expensive.

---

## Deliverables Produced

| File | Purpose |
|------|---------|
| `copilot-cli-resource-usage-report.md` | Comprehensive research report |
| `copilot-cli-resource-usage-summary.md` | This file — executive summary |
| `copilot-cli-observability-matrix.md` | Comparison matrix of all approaches |
| `copilot-cli-evaluator-architecture.md` | Evaluator design with data flow and schemas |
| `copilot-cli-evidence-model.md` | Evidence classification and confidence scoring |
| `copilot-cli-validation-plan.md` | Synthetic skills, controls, and experiment matrix |
| `copilot-cli-authoring-prompt.md` | Follow-up prompt to implement the evaluator |
| `sources.md` | Source inventory with trust levels |
