# Validation and Test Plan: Copilot CLI Resource Usage Evaluator

## Purpose

This document defines how to validate that the evaluator correctly detects which resources Copilot CLI selects and uses. It includes synthetic test skills, canary resources, positive and negative controls, and a repeatable experiment matrix.

---

## Synthetic Test Skills

### Design Principles

Each synthetic skill must contain a **unique behavioral marker** that makes its influence detectable in Copilot's output. Markers should be:

1. **Unambiguous**: Something the model would never produce without the skill
2. **Verifiable**: Programmatically checkable in output
3. **Non-conflicting**: Each marker is unique across all test skills

### Skill 1: Canary Comment Skill

```markdown
---
name: canary-comment
description: Adds a specific canary comment to all generated code. Use when writing any code.
---

## Instructions

When generating ANY code, always include this exact comment as the first line:

<!-- CANARY-SKILL-ALPHA-7X9 -->

For non-HTML code, use the language's comment syntax:
- Python: `# CANARY-SKILL-ALPHA-7X9`
- JavaScript/TypeScript: `// CANARY-SKILL-ALPHA-7X9`
- Shell: `# CANARY-SKILL-ALPHA-7X9`
```

**Detection**: Grep output for `CANARY-SKILL-ALPHA-7X9`. If present, skill was loaded and influential.

### Skill 2: Specific Format Skill

```markdown
---
name: format-enforcer
description: Enforces a specific output format with a header. Use when generating any documentation or text output.
---

## Instructions

All text output MUST begin with this exact header:

```
=== EVAL-FORMAT-BRAVO-3K2 ===
```

And end with:

```
=== END-EVAL-FORMAT-BRAVO-3K2 ===
```
```

**Detection**: Check for `EVAL-FORMAT-BRAVO-3K2` wrapper in output.

### Skill 3: Tool Preference Skill

```markdown
---
name: tool-preference
description: Specifies a required tool workflow. Use when asked to analyze files.
---

## Instructions

When analyzing any file:
1. ALWAYS run `echo "EVAL-TOOL-CHARLIE-5P8"` as the very first shell command
2. Then proceed with analysis
```

**Detection**: Check postToolUse hook logs or transcript for `echo "EVAL-TOOL-CHARLIE-5P8"`.

### Skill 4: Narrow-Scope Skill (Should NOT Match Broad Prompts)

```markdown
---
name: kubernetes-deployment
description: Guide for Kubernetes deployment YAML generation. ONLY use when explicitly asked about Kubernetes deployments.
---

## Instructions

When generating Kubernetes deployment manifests, always include the annotation:
```yaml
metadata:
  annotations:
    eval-marker: "EVAL-K8S-DELTA-9R1"
```
```

**Detection**: Should only appear for Kubernetes-specific prompts. Presence in unrelated prompts = false selection.

### Skill 5: Overlapping Scope Skill

```markdown
---
name: testing-overlap
description: Testing helper for all test-related tasks. Use when writing tests, test plans, or test strategies.
---

## Instructions

Always output `[EVAL-TEST-ECHO-2M6]` as the last line of any test-related response.
```

**Detection**: Competes with `canary-comment` skill on testing prompts. Tests overlap resolution.

---

## Canary Resources

### Canary Custom Instruction

Create `.github/copilot-instructions.md`:

```markdown
When greeting the user or starting a response, always include the phrase "CANARY-INSTRUCTION-FOXTROT-4J7" in your first sentence.
```

**Detection**: If `CANARY-INSTRUCTION-FOXTROT-4J7` appears in output, custom instructions were loaded.

### Canary Path-Specific Instruction

Create `.github/instructions/python.instructions.md`:

```markdown
---
applyTo: "**/*.py"
---

When working with Python files, always include `# CANARY-PATH-GOLF-8W3` as a comment in generated code.
```

**Detection**: Should appear only when prompt involves Python files.

### Canary Personal Instruction

Create `~/.copilot/copilot-instructions.md` (in COPILOT_HOME for test):

```markdown
Always end your response with the phrase "CANARY-PERSONAL-HOTEL-6N5".
```

**Detection**: Should appear in all runs using this COPILOT_HOME.

---

## Control Experiments

### Positive Controls (Should Detect Usage)

| ID | Prompt | Expected Skill | Expected Marker | Why |
|----|--------|---------------|-----------------|-----|
| PC-1 | "Write a Python function to sort a list" | canary-comment | CANARY-SKILL-ALPHA-7X9 | Broad coding prompt should trigger code-writing skill |
| PC-2 | "Generate documentation for this API" | format-enforcer | EVAL-FORMAT-BRAVO-3K2 | Documentation prompt matches format-enforcer |
| PC-3 | "Analyze the file src/main.py" | tool-preference | EVAL-TOOL-CHARLIE-5P8 | File analysis prompt matches tool-preference |
| PC-4 | "Create a Kubernetes deployment manifest" | kubernetes-deployment | EVAL-K8S-DELTA-9R1 | Direct match to narrow-scope skill |
| PC-5 | "Write unit tests for the auth module" | testing-overlap | EVAL-TEST-ECHO-2M6 | Testing prompt matches testing-overlap |
| PC-CI | Any prompt (no --no-custom-instructions) | repo instructions | CANARY-INSTRUCTION-FOXTROT-4J7 | Custom instructions always loaded |

### Negative Controls (Should NOT Detect Usage)

| ID | Prompt | Should NOT Use | Why |
|----|--------|---------------|-----|
| NC-1 | "What time is it?" | Any skill | Trivial question; no skill should be relevant |
| NC-2 | "Explain recursion" | kubernetes-deployment | Completely unrelated to K8s |
| NC-3 | "Write a Python function" (with --no-custom-instructions) | repo instructions | Flag explicitly disables custom instructions |
| NC-4 | "Write a Python function" (with canary-comment skill disabled via /skills toggle) | canary-comment | Skill explicitly disabled |
| NC-5 | "Explain this JSON file" | testing-overlap | Not a testing prompt |

### A/B Experiments

| ID | Variant A | Variant B | Expected Difference |
|----|-----------|-----------|-------------------|
| AB-1 | canary-comment enabled | canary-comment disabled | CANARY-SKILL-ALPHA-7X9 present in A, absent in B |
| AB-2 | custom instructions on | --no-custom-instructions | CANARY-INSTRUCTION-FOXTROT-4J7 present in A, absent in B |
| AB-3 | format-enforcer enabled | format-enforcer disabled | EVAL-FORMAT-BRAVO-3K2 wrapper in A, absent in B |
| AB-4 | Both canary-comment + testing-overlap | Only testing-overlap | Reveals overlap resolution behavior |

---

## Experiment Matrix

### Full Matrix (Prompt × Skill Configuration × Instrumentation)

| Experiment | Prompt | Skills Enabled | Custom Instructions | Instrumentation | Expected Outcome |
|-----------|--------|---------------|-------------------|-----------------|-----------------|
| E-01 | PC-1 | All | On | Hooks + FS watcher | canary-comment detected |
| E-02 | PC-1 | All except canary-comment | On | Hooks + FS watcher | canary-comment NOT detected |
| E-03 | PC-1 | None | On | Hooks + FS watcher | No skill markers; instruction marker present |
| E-04 | PC-1 | All | Off (--no-custom-instructions) | Hooks + FS watcher | canary-comment detected; instruction marker absent |
| E-05 | NC-1 | All | On | Hooks + FS watcher | No skill markers (trivial question) |
| E-06 | PC-4 | All | On | Hooks + FS watcher | kubernetes-deployment detected; others NOT detected |
| E-07 | PC-4 | All except kubernetes-deployment | On | Hooks + FS watcher | No K8s marker in output |
| E-08 | PC-5 | canary-comment + testing-overlap | On | Hooks + FS watcher | Both skills may be loaded; test overlap |
| E-09 | PC-1 | All | On | Hooks only (no FS watcher) | Test hooks-only evidence quality |
| E-10 | PC-1 | All | On | FS watcher only (no hooks) | Test FS watcher-only evidence quality |
| E-11 | PC-1 | All | On | --share transcript only | Test transcript-only evidence quality |
| E-12 | PC-1 | All | On | All instrumentation | Maximum evidence; baseline comparison |

### Repeatability Sub-Matrix

Run E-01, E-05, and E-06 three times each to measure:
- Confidence score variance across runs
- Classification stability (same classification every time?)
- Marker presence consistency

---

## Validation Criteria

### True Positive

The evaluator reports a resource as "Confirmed Used" or "Probably Used," AND:
- The expected canary marker appears in the output, OR
- The A/B comparison shows clear behavioral difference when the resource is toggled

### True Negative

The evaluator reports a resource as "Not Used" or "Confirmed Not Used," AND:
- The canary marker does NOT appear in the output, AND
- Toggling the resource on/off produces no behavioral difference

### False Positive

The evaluator reports a resource as used, BUT:
- The canary marker does NOT appear in the output, AND
- A/B comparison shows no behavioral difference

**Acceptable false positive rate**: < 10% across all experiments

### False Negative

The evaluator reports a resource as not used, BUT:
- The canary marker DOES appear in the output

**Acceptable false negative rate**: < 5% across all experiments

---

## Diagnosing Evaluator Errors

### If False Positives Are High

1. Check if filesystem watcher is detecting reads by processes other than Copilot
2. Verify hook scripts are correctly filtering by relevant tool names
3. Tighten confidence scoring thresholds
4. Add process-level filtering to FS watcher

### If False Negatives Are High

1. Check if skills are loading from an unexpected location (personal vs project)
2. Verify FS watcher is monitoring all skill directories
3. Check if Copilot is reading skills before the watcher starts
4. Increase A/B test sensitivity (more runs, different prompts)
5. Check if canary markers are being suppressed by other instructions

### If Results Are Inconsistent Across Runs

1. Pin the model with `--model`
2. Reduce temperature/reasoning effort
3. Use longer, more specific prompts that unambiguously match skills
4. Increase run count for statistical significance (5+ runs per experiment)
