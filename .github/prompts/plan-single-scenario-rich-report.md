# Plan: Single Random Scenario + Rich Analysis Report

## Problem Statement

Two issues with the current multi-run pipeline:

1. **Generation is too slow** — Each run generates all 3 scenarios (~30 min per run). With 3 runs × 5 configs = 15 cycles × 3 scenarios = 45 Copilot invocations. We can cut this to 15 by randomly picking one scenario per run.

2. **Aggregated analysis.md lacks detail** — The Python-generated `analysis.md` has only score tables and statistics. The previous single-run reports had rich code snippets, qualitative verdicts, and side-by-side comparisons that made them actionable. We need to bring that detail back.

## Change 1: Random Single Scenario per Run

### Current behavior
```
For each run:
    For each scenario (3):
        Invoke Copilot CLI → generate app
```
**Total per config:** 3 runs × 3 scenarios = 9 Copilot invocations

### Proposed behavior
```
For each run:
    Randomly pick 1 scenario (from however many are defined)
    Invoke Copilot CLI → generate app
```
**Total per config:** 3 runs × 1 scenario = 3 Copilot invocations (**67% reduction**)

### Design details

- Use `random.seed(run_id)` for reproducibility — same run_id always picks the same scenario
- The scenario pool is dynamic — works with any number of scenarios (not hardcoded to 3)
- Each run's selected scenario is recorded in the output directory (e.g., `run-1/selected-scenario.txt`)
- Across N runs, scenarios are distributed to maximize coverage (round-robin with shuffle)
- The verify and analyze steps already work per-scenario, so they naturally handle partial runs

### Scenario distribution strategy

Rather than pure random (which could pick the same scenario 3 times), use a distribution that ensures variety:

```python
scenarios = config.scenarios
# Distribute scenarios across runs to maximize coverage
for run_id in range(1, num_runs + 1):
    scenario = scenarios[(run_id - 1) % len(scenarios)]
```

For 3 runs × 3 scenarios: run 1 → scenario 1, run 2 → scenario 2, run 3 → scenario 3.
For 5 runs × 3 scenarios: run 1-3 → each scenario once, run 4-5 → scenarios 1-2 again.

This guarantees every scenario is covered at least once in N >= len(scenarios) runs.

### Files to change

- **`generate.py`**: Select one scenario per run instead of looping all
- **`verify.py`**: Already handles missing scenario dirs (skips them) — no change needed
- **`analyze.py`**: Already handles partial runs — no change needed
- **`aggregator.py`**: Already handles missing scores — no change needed

## Change 2: Rich Analysis Report with Code Snippets

### Current aggregated analysis.md
- Statistical tables only (mean ± std, per-run scores)
- No code snippets
- No qualitative verdicts
- No side-by-side comparisons

### Proposed approach

The per-run `analysis-run-{N}.md` files already contain rich content (code snippets, verdicts, reasoning). The problem is the aggregation step discards everything except scores.

**Solution:** Extract code snippets and qualitative content from per-run analysis files and weave them into the aggregated report.

#### Implementation

1. **Enhance the score parser** to also extract per-dimension content sections (not just the Executive Summary table). Each per-run analysis has sections like:

   ```markdown
   ## 3. Minimal API Architecture [CRITICAL]
   
   [rich content with code snippets, comparisons, verdicts]
   
   **Scores:** ...
   ```

2. **New function `parse_run_content()`** — extracts the full markdown section for each dimension from a per-run analysis file. Returns `{dimension_name: section_markdown}`.

3. **Pick the best per-run content** for the aggregated report — from the run with the most complete scores (or the median-scoring run to avoid outliers).

4. **Updated aggregated report structure:**

   ```markdown
   # Aggregated Analysis: {name}
   
   ## Executive Summary
   [mean ± std table — keep as-is]
   
   ## Final Rankings
   [rankings table — keep as-is]
   
   ## Weighted Score per Run
   [per-run table — keep as-is]
   
   ## Verification Summary
   [verification data — keep as-is]
   
   ## Per-Dimension Analysis
   
   ### 1. Build & Run Success [CRITICAL × 3]
   
   #### Scores Across Runs
   | Run | config-a | config-b | ... |
   [per-run score table — keep as-is]
   
   #### Analysis
   [Rich content from the representative per-run analysis — 
    code snippets, comparisons, verdicts]
   
   ### 2. Minimal API Architecture [CRITICAL × 3]
   [Same pattern]
   ```

### Files to change

- **`aggregator.py`**: Add `parse_run_content()`, update `_write_aggregated_report()` to include rich content
- No template changes needed (aggregated report is Python-generated)

## Implementation Order

| Step | Work | Files |
|------|------|-------|
| 1 | Single random scenario per run | `generate.py` |
| 2 | Parse rich content from per-run analyses | `aggregator.py` |
| 3 | Weave rich content into aggregated report | `aggregator.py` |
| 4 | Test with existing data | verify imports, check output |
| 5 | Commit | all changed files |

## Estimated Impact

| Metric | Before | After |
|--------|--------|-------|
| Copilot invocations per eval (3 runs × 5 configs) | 45 generation + 3 analysis = 48 | 15 generation + 3 analysis = 18 |
| Generation time (est.) | ~7.5 hours | ~2.5 hours |
| Analysis report quality | Statistics only | Statistics + code snippets + verdicts |
