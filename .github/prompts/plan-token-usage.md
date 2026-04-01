# Plan: Add Token Usage to analysis.md Reports

## Problem

Token-based billing is rolling out and users need to understand the token cost of each skill configuration. The `generation-usage.json` file already captures detailed per-run token data (`input_tokens`, `output_tokens`, `cache_read_tokens`, `api_calls`, `api_duration_ms`, `wall_time_seconds`), but **this data is never surfaced in the `analysis.md` report**. The only usage data currently shown is the "Asset Usage Summary" table which lists loaded skills/plugins — no token counts.

## Proposed Approach

Add a new **"Token Usage"** section to the aggregated `analysis.md` report and a new **"Token Efficiency"** scoring dimension. Changes are confined to `src/skill_eval/aggregator.py` with no changes to data collection (already complete).

### What Gets Added to analysis.md

1. **Token Usage Summary table** — per-configuration averages across runs, with delta columns vs "no-skills" baseline
2. **Token Usage Per Run table** — per-config/per-run detail so readers can assess variance
3. **Token Efficiency dimension** — an automatically-scored dimension (MEDIUM tier, ×1.0 weight) based on the ratio of tokens used vs the baseline

### Report Section Design

#### Section: "Token Usage Summary" (placed after Verification Summary)

| Configuration | Avg Input Tokens | Avg Output Tokens | Avg Cache Read | Avg API Calls | Avg Wall Time | Δ Input vs Baseline |
|---|---|---|---|---|---|---|
| no-skills | 2,302,594 | 50,516 | 2,152,268 | 35 | 10m 54s | — (baseline) |
| dotnet-webapi | 2,375,147 | 52,735 | 2,199,089 | 33 | 11m 18s | +3.2% |
| dotnet-artisan | 3,717,309 | 54,367 | 3,518,324 | 36 | 17m 1s | +61.4% |

#### Section: "Token Usage Per Run" (collapsible detail)

| Configuration | Run | Scenario | Input Tokens | Output Tokens | Cache Read | API Calls | Wall Time |
|---|---|---|---|---|---|---|---|
| no-skills | 1 | FitnessStudioApi | 2,348,300 | 53,225 | 2,201,793 | 34 | 11m 22s |
| ... | ... | ... | ... | ... | ... | ... | ... |

#### Dimension: "Token Efficiency" (automated, MEDIUM tier)

Scoring rubric (automated, based on total input+output vs baseline):
- **5** — Uses ≤100% of baseline tokens (same or fewer)
- **4** — Uses 101–125% of baseline tokens
- **3** — Uses 126–175% of baseline tokens
- **2** — Uses 176–250% of baseline tokens
- **1** — Uses >250% of baseline tokens

## Implementation Details

### Functions Added to `src/skill_eval/aggregator.py`

#### `_compute_token_efficiency_scores(generation_usage, config_names, run_ids)`

- Groups total tokens (input + output) by (config, run_id)
- Computes baseline mean from "no-skills" config
- Maps each config's per-run ratio vs baseline to a 1–5 score
- Returns per-run score dicts in the same shape as parsed LLM scores

#### `_write_token_usage_sections(lines, generation_usage, config_names)`

- Computes per-config averages across all runs
- Renders Token Usage Summary table with delta % vs baseline
- Renders Token Usage Per Run detail table sorted by config then run

### Integration Points

Token Efficiency scores are injected into `all_run_scores` before dimension stats are computed, so they automatically appear in:
- Executive Summary table
- Final Rankings weighted score
- Per-Dimension Analysis section
- Consistency Analysis
- Weighted Score per Run table

### Constants

```python
_TOKEN_EFFICIENCY_DIM = "Token Efficiency"
_TOKEN_EFFICIENCY_TIER = "MEDIUM"
_TOKEN_EFFICIENCY_WEIGHT = 1.0
_BASELINE_CONFIG = "no-skills"
```

## Key Design Decisions

- **Baseline is always "no-skills"** — hardcoded config name; if no "no-skills" config exists, deltas are omitted and Token Efficiency scores are skipped
- **No cost estimation** — only raw token counts; users calculate costs externally
- **Token Efficiency is MEDIUM tier (×1.0)** — standard influence on weighted rankings
- **Automated scoring** — Token Efficiency is computed from data, not by the LLM judge
- **No changes to data collection** — `generate.py` and `_parse_copilot_log_usage()` are untouched; all data already exists in `generation-usage.json`
- **No changes to per-run analysis** — Token usage sections only appear in the aggregated `analysis.md`, not in `analysis-run-N.md` files
- **Graceful degradation** — If `generation-usage.json` is missing or empty, token sections are silently omitted

## Files Modified

| File | Change |
|---|---|
| `src/skill_eval/aggregator.py` | Add token usage sections + automated Token Efficiency dimension |
| `.github/prompts/plan-token-usage.md` | This plan file |
