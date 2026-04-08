# Evidence and Confidence Model for Copilot CLI Resource Usage Evaluation

## Purpose

This document defines how to classify, score, and report evidence about which Copilot CLI resources (skills, custom instructions, prompt-shaping files) were used for a given prompt. It establishes a rigorous framework for distinguishing what can be directly observed from what must be inferred.

---

## Evidence Classes

### Class 1: Direct Observation

Evidence obtained from an officially supported mechanism that explicitly reports the resource.

| Property | Value |
|----------|-------|
| **Definition** | The evaluator can point to a specific data artifact (log entry, command output, hook payload, session record) that names or identifies the resource. |
| **Examples** | `/skills list` output showing available skills; `/instructions` output showing active instruction files; `--share` session transcript mentioning a skill invocation; tool-use hook payload showing `toolName` and `toolArgs`. |
| **Confidence Score** | 90–100 |
| **Limitation** | Direct observation can confirm a resource was *available* or a tool was *used*, but may not confirm the resource was *injected into model context*. |

### Class 2: Strong Inference

Evidence obtained from a combination of officially supported mechanisms, where the conclusion follows with high probability but is not directly stated.

| Property | Value |
|----------|-------|
| **Definition** | The evaluator observes behavior consistent with resource usage and can rule out most alternative explanations. |
| **Examples** | A/B comparison: running the same prompt with and without a specific skill enabled, observing materially different output consistent with the skill's instructions; filesystem watcher detecting a read of SKILL.md during prompt processing; session data showing tools used that match a skill's prescribed workflow. |
| **Confidence Score** | 70–89 |
| **Limitation** | Cannot distinguish between "loaded but not influential" and "loaded and influential." Correlation is not causation. |

### Class 3: Weak Inference

Evidence based on circumstantial indicators where multiple alternative explanations exist.

| Property | Value |
|----------|-------|
| **Definition** | The evaluator observes patterns that are *consistent with* resource usage but cannot rule out other explanations. |
| **Examples** | Output style matches a skill's instructions (but the model might have done this without the skill); a file exists in a skill directory (but may not have been loaded); timing correlation between prompt and file access. |
| **Confidence Score** | 30–69 |
| **Limitation** | High false-positive risk. Should not be used alone for definitive claims. |

### Class 4: Structural Availability

The resource exists in a location where Copilot could discover it, but there is no evidence it was selected or loaded.

| Property | Value |
|----------|-------|
| **Definition** | The evaluator can confirm the resource is present and discoverable, but has no evidence of selection or injection. |
| **Examples** | A SKILL.md file exists in `.github/skills/my-skill/`; a `.github/copilot-instructions.md` file is present; a plugin is installed. |
| **Confidence Score** | 10–29 |
| **Limitation** | Presence does not imply usage. |

---

## Confidence Scoring

### Scoring Formula

Each resource gets a composite confidence score based on the strongest evidence class available, with bonuses for corroborating evidence:

```
Score = BaseScore(strongest_evidence_class) + Corroboration_Bonus
```

| Evidence Class | Base Score Range |
|---------------|-----------------|
| Direct Observation | 90–100 |
| Strong Inference | 70–89 |
| Weak Inference | 30–69 |
| Structural Availability | 10–29 |

### Corroboration Bonus

| Condition | Bonus |
|-----------|-------|
| Two independent evidence sources agree | +5 |
| Three or more independent evidence sources agree | +10 |
| A/B comparison confirms behavioral difference | +15 |
| Evidence from official mechanism (hooks, session data) | +5 |

### Score Caps

- Maximum score: 100
- If only Weak Inference evidence exists: cap at 69 regardless of corroboration
- If only Structural Availability evidence exists: cap at 40 regardless of corroboration

---

## Usage Classification Taxonomy

For each resource evaluated, classify it into exactly one category:

| Classification | Confidence Required | Meaning |
|---------------|-------------------|---------|
| **Confirmed Used** | ≥ 85 | Strong evidence the resource was selected and injected. |
| **Probably Used** | 70–84 | Evidence strongly suggests usage but cannot be directly confirmed. |
| **Possibly Used** | 50–69 | Circumstantial evidence exists; usage is plausible but uncertain. |
| **Available But Unconfirmed** | 30–49 | Resource was discoverable; no evidence of usage beyond availability. |
| **Not Used** | < 30 | Resource was either absent or evidence indicates it was not selected. |
| **Confirmed Not Used** | Special | Explicit evidence of exclusion (e.g., skill disabled via `/skills`, `--no-custom-instructions` flag active). |

---

## Evidence Sources and Their Strength

| Evidence Source | Best-Case Class | What It Proves | What It Cannot Prove |
|----------------|----------------|----------------|---------------------|
| `/skills list` | Direct Observation | Skill is available/enabled | Skill was selected for this prompt |
| `/skills info` | Direct Observation | Skill metadata and location | Skill was injected into context |
| `/instructions` | Direct Observation | Instruction files active | Instructions affected output |
| `preToolUse` hook | Direct Observation | Tool was about to execute | Which skill triggered the tool |
| `postToolUse` hook | Direct Observation | Tool executed with result | Which skill triggered the tool |
| `--share` transcript | Strong Inference | Session conversation and tools | Internal context loading |
| Session state files | Strong Inference | Conversation turns and tool calls | Skill selection events |
| Filesystem watcher | Strong Inference | File was read by process | File content was used in context |
| A/B comparison | Strong Inference | Behavioral difference with/without | Exact injection mechanism |
| Output analysis | Weak Inference | Output consistent with skill | Model may produce similar output without skill |
| Timing correlation | Weak Inference | File access during prompt processing | Causal relationship |
| File existence | Structural Availability | Resource is discoverable | Nothing about actual usage |

---

## Report Schema

Each resource in the evaluator report should include:

```json
{
  "resource_path": ".github/skills/webapp-testing/SKILL.md",
  "resource_type": "skill",
  "resource_name": "webapp-testing",
  "classification": "Probably Used",
  "confidence_score": 78,
  "evidence": [
    {
      "source": "filesystem_watcher",
      "class": "Strong Inference",
      "detail": "SKILL.md was read at T+2.3s during prompt processing",
      "timestamp": "2026-03-28T14:30:02.300Z"
    },
    {
      "source": "output_analysis",
      "class": "Weak Inference",
      "detail": "Output followed the skill's prescribed 5-step testing procedure"
    }
  ],
  "negative_evidence": [],
  "notes": "Filesystem read confirmed; cannot confirm injection into model context."
}
```

---

## Representing Uncertainty

### In Tabular Reports

Use the following visual indicators:

| Symbol | Meaning |
|--------|---------|
| ✅ | Confirmed Used (≥85) |
| 🟢 | Probably Used (70–84) |
| 🟡 | Possibly Used (50–69) |
| ⚪ | Available But Unconfirmed (30–49) |
| ❌ | Not Used (<30) |
| 🚫 | Confirmed Not Used |

### In Prose Reports

- Always state the evidence class when making claims
- Use hedging language proportional to uncertainty:
  - Confirmed: "The skill was loaded" 
  - Probable: "The skill was very likely loaded, based on..."
  - Possible: "The skill may have been loaded; evidence suggests..."
  - Unconfirmed: "The skill was available but no evidence of selection was found"

### Error Bars

For quantitative analysis across multiple runs, report:
- Mean confidence score across runs
- Standard deviation
- Whether classification was consistent across runs (stability indicator)

---

## Special Cases

### Custom Instructions (Always-On Resources)

Custom instructions (e.g., `.github/copilot-instructions.md`) are designed to be *always loaded* when present, unless explicitly disabled with `--no-custom-instructions`. For these:

- If file exists and `--no-custom-instructions` was NOT used → classify as **Confirmed Used** (score 95)
- If file exists and `--no-custom-instructions` WAS used → classify as **Confirmed Not Used**
- If file does not exist → classify as **Not Applicable**

### Skills (On-Demand Resources)

Skills are loaded *only when relevant*. The selection algorithm is internal and undocumented. For these:

- Filesystem read is Strong Inference of selection
- Matching output behavior is Weak Inference
- Availability alone is only Structural Availability

### Path-Specific Instructions

Files matching `.github/instructions/**/*.instructions.md` with `applyTo` globs:

- If the prompt operates on files matching the glob → classify as **Probably Used** if file exists
- If the prompt operates on unrelated files → classify as **Available But Unconfirmed**
