# Copilot Skill Evaluation Rubric

Use this rubric to score a GitHub Copilot agent skill across six dimensions.
Each dimension is scored on a 3-point scale: Excellent (3), Acceptable (2), Poor (1).

**Last Updated:** 2026-03-28

---

## Scoring Dimensions

### 1. Description Quality (Weight: High)

The description determines whether the skill activates correctly. This is the single most impactful dimension.

| Score | Criteria |
|-------|----------|
| **Excellent (3)** | Description is behavior-driven, includes 2+ specific trigger keywords (frameworks, tools, task types), states when to activate, and is 50-300 characters. A reader can immediately understand what prompts would activate this skill. |
| **Acceptable (2)** | Description mentions the general domain and includes at least one trigger keyword. Activation is reasonably predictable but could be more precise. |
| **Poor (1)** | Description is vague ("Helps with coding"), has no trigger keywords, or is either too short (<20 chars) or too long (>500 chars). Activation behavior is unpredictable. |

### 2. Instruction Clarity (Weight: High)

Clear, unambiguous instructions produce consistent, correct output.

| Score | Criteria |
|-------|----------|
| **Excellent (3)** | Numbered steps with clear success criteria. Imperative voice throughout. No ambiguity — a developer could follow these steps manually and get the same result. Includes at least one inline example. |
| **Acceptable (2)** | Steps are mostly clear but some are vague or missing success criteria. At least one example exists (inline or in `examples/`). |
| **Poor (1)** | Instructions are narrative prose without clear steps. Multiple ambiguous phrases ("consider", "maybe", "depending on"). No examples anywhere. |

### 3. Context Efficiency (Weight: High)

Efficient skills maximize instructional value per context token consumed.

| Score | Criteria |
|-------|----------|
| **Excellent (3)** | SKILL.md body is under 1000 words. Verbose content is in `reference/`. No duplicated information. Every paragraph directly contributes to the procedure. |
| **Acceptable (2)** | SKILL.md body is 1000-1500 words. Some content could be moved to `reference/` but overall token cost is reasonable. |
| **Poor (1)** | SKILL.md body exceeds 1500 words. Contains embedded documentation, full API references, or lengthy explanations that belong in supporting files. |

### 4. Metadata Validity (Weight: Medium)

Valid metadata ensures the skill loads correctly across platforms.

| Score | Criteria |
|-------|----------|
| **Excellent (3)** | `name` matches folder name, uses only lowercase/hyphens, is 1-64 chars. `description` is 10-1024 chars. No unsupported frontmatter fields. YAML parses cleanly. |
| **Acceptable (2)** | Metadata is functionally correct but has minor issues (e.g., description at boundary limits, name has unnecessary characters that still parse). |
| **Poor (1)** | `name` does not match folder, uses uppercase/spaces, or is missing. `description` is missing or outside valid range. Unsupported fields present. |

### 5. Example Coverage (Weight: Medium)

Examples anchor agent behavior and serve as regression tests.

| Score | Criteria |
|-------|----------|
| **Excellent (3)** | At least one inline example in SKILL.md body AND at least one golden example in `examples/` with complete input/output. Examples cover the primary use case and at least one edge case. |
| **Acceptable (2)** | At least one example exists (either inline or in `examples/`). Example is realistic but may not cover edge cases. |
| **Poor (1)** | No examples anywhere. Or examples are trivial stubs that don't demonstrate real behavior. |

### 6. Maintainability (Weight: Medium)

Maintainable skills remain correct over time with minimal effort.

| Score | Criteria |
|-------|----------|
| **Excellent (3)** | No hardcoded volatile data (versions, URLs) in SKILL.md body. Volatile data is in `reference/` with freshness dates. Supporting files are well-organized. Skill has a clear single responsibility. |
| **Acceptable (2)** | Minimal hardcoded volatile data. Organization is reasonable. Skill scope is mostly focused. |
| **Poor (1)** | Multiple hardcoded version numbers, URLs, or tool-specific assumptions embedded in SKILL.md. No organization of supporting files. Skill tries to cover too many workflows. |

---

## Scoring Summary

| Dimension | Weight | Score (1-3) |
|-----------|--------|-------------|
| Description Quality | High | ___ |
| Instruction Clarity | High | ___ |
| Context Efficiency | High | ___ |
| Metadata Validity | Medium | ___ |
| Example Coverage | Medium | ___ |
| Maintainability | Medium | ___ |

**Weighted Score Calculation:**
- High weight dimensions: multiply score × 2
- Medium weight dimensions: multiply score × 1
- **Maximum possible:** (3 × 2 × 3) + (3 × 1 × 3) = 18 + 9 = **27**

---

## Pass/Fail Recommendation Model

### Pass Criteria (ALL must be met)

- [ ] Total weighted score ≥ 18 (out of 27)
- [ ] No dimension scored "Poor" (1) on any High-weight dimension
- [ ] No more than one dimension scored "Poor" (1) on Medium-weight dimensions
- [ ] Metadata validates correctly (name matches folder, required fields present)

### Overall Rating

| Weighted Score | Rating | Recommendation |
|---------------|--------|----------------|
| 24-27 | **Exemplary** | Ready to share. Consider as a template for other skills. |
| 18-23 | **Pass** | Suitable for use. Address any "Acceptable" areas when convenient. |
| 12-17 | **Needs Work** | Address "Poor" dimensions before using in production. |
| 6-11 | **Fail** | Significant rework needed. Review against authoring rules and checklist. |

---

## Quick Red Flags

These issues should be flagged immediately during review, regardless of scoring:

- [ ] **Missing `name` or `description` frontmatter** → Skill will not load
- [ ] **Folder name ≠ `name` field** → Skill will not load
- [ ] **SKILL.md body > 2000 words with no `reference/`** → Context bloat
- [ ] **Description contains no specific technology or task keywords** → Unreliable activation
- [ ] **No examples of any kind** → Inconsistent output
- [ ] **Contains repo-wide policies** → Belongs in `copilot-instructions.md`
- [ ] **Hardcoded version numbers without freshness mechanism** → Will become stale
