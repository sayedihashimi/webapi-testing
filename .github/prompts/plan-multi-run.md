# Plan: Multi-Run Pipeline with Parallel Execution

## Problem Statement

The evaluation pipeline currently has a `runs` config field and `--runs` CLI flag, but they are **completely ignored** — every config/scenario generates, verifies, and analyzes exactly once. For statistically meaningful evaluation of Copilot skills, we need multiple runs with aggregation. Additionally, with 5 runs × 5 configs × 3 scenarios = 75 generation cycles, performance is a concern.

## Current Pipeline Flow (Single Run)

```
For each config (sequential):
    Register skills in ~/.copilot/config.json
    Create staging dir with symlinked skills
    For each scenario (sequential):
        Invoke Copilot CLI → output/{config}/{scenario}/
    Unregister skills
    Cleanup staging dir

For each config (sequential):
    For each scenario (sequential):
        Build, run, format check, security scan → build-notes.md

Render analysis prompt → Copilot CLI → analysis.md
```

**Current output structure:**
```
output/{config}/{scenario}/src/...
```

## Proposed Design

### 1. New Output Structure

```
output/
├── {config}/
│   ├── run-1/
│   │   ├── {scenario1}/src/...
│   │   ├── {scenario2}/src/...
│   │   └── gen-notes.md
│   ├── run-2/
│   │   └── ...
│   └── run-N/
│       └── ...
└── {other-config}/
    └── ...

reports/
├── build-notes.md          # Aggregated verification across all runs
├── analysis-run-1.md       # Per-run analysis
├── analysis-run-2.md
├── analysis-run-N.md
└── analysis.md             # Final aggregated report with statistics
```

### 2. CLI Changes

**Updated `run` command options:**

| Flag | Default | Description |
|------|---------|-------------|
| `--runs, -r` | 5 | Number of generation runs per configuration |
| `--configurations, -c` | all | Filter to specific configurations (repeatable) |
| `--scenarios, -s` | all | Filter to specific scenarios (repeatable) — **NEW** |
| `--parallel, -p` | 3 | Max concurrent operations — **NEW** |
| `--resume` | false | Resume from last completed run (skip existing) — **NEW** |
| `--skip-generate` | false | Skip generation step |
| `--skip-verify` | false | Skip verification step |
| `--analyze-only` | false | Only run analysis |
| `--run-ids` | all | Operate on specific run numbers (e.g., `--run-ids 3 4 5`) — **NEW** |

**Updated `generate` command:**
```
skill-eval generate --runs 5 -c dotnet-webapi -c no-skills -s FitnessStudioApi --parallel 3
```

**Updated `run` command:**
```
skill-eval run --runs 5 --parallel 3 --resume
```

### 3. Parallelization Strategy

#### What CAN be parallelized

| Step | Parallelizable? | Constraint |
|------|----------------|------------|
| Generation: different configs | ❌ No | Shared `~/.copilot/config.json` for skill registration |
| Generation: same config, different runs | ⚠️ Limited | Same skills registered, but need separate staging dirs. Copilot CLI may have rate limits |
| Generation: same config+run, different scenarios | ❌ No | Single Copilot invocation generates all scenarios |
| Verification: all configs × runs × scenarios | ✅ Yes | Each build/run is fully independent |
| Analysis: different runs | ✅ Yes | Each analysis is an independent Copilot CLI call |
| Aggregation | ❌ No | Needs all run results |

#### Proposed execution plan

```
Phase 1: GENERATE (partially parallel)
─────────────────────────────────────
For each config (SEQUENTIAL — skill registration is global):
    Register skills for this config
    For run 1..N (PARALLEL, up to --parallel limit):
        Create staging dir
        Invoke Copilot CLI → output/{config}/run-{i}/
        Cleanup staging dir
    Unregister skills

Phase 2: VERIFY (fully parallel)
────────────────────────────────
For each (config, run, scenario) combination (PARALLEL, up to --parallel limit):
    Build, run, format, security scan
Write aggregated build-notes.md

Phase 3: ANALYZE (parallel per run)
───────────────────────────────────
For run 1..N (PARALLEL, up to --parallel limit):
    Render analysis prompt pointing at output/*/run-{i}/
    Invoke Copilot CLI → reports/analysis-run-{i}.md

Phase 4: AGGREGATE (sequential)
──────────────────────────────
Parse scores from all analysis-run-{i}.md files
Compute statistics (mean, median, std dev, min, max)
Write reports/analysis.md with aggregated results
```

#### Estimated time savings

Assuming 5 runs, 5 configs, ~5 min per generation, ~2 min per verification, ~20 min per analysis:

| Step | Sequential | Parallel (3 workers) | Savings |
|------|-----------|---------------------|---------|
| Generate | 5 configs × 5 runs × 5 min = 125 min | 5 configs × ceil(5/3) runs × 5 min = 50 min | 60% |
| Verify | 5 × 5 × 3 × 2 min = 150 min | ceil(75/3) × 2 min = 50 min | 67% |
| Analyze | 5 × 20 min = 100 min | ceil(5/3) × 20 min = 40 min | 60% |
| **Total** | **~375 min (6.25 hr)** | **~140 min (2.3 hr)** | **~63%** |

### 4. Implementation Details

#### 4.1 `config.py` changes

```python
class EvalConfig(BaseModel):
    # ... existing fields ...
    runs: int = 5  # Change default from 1 to 5

class OutputSettings(BaseModel):
    # ... existing fields ...
    per_run_analysis_pattern: str = "analysis-run-{run}.md"  # NEW
```

#### 4.2 `generate.py` changes

```python
async def run_generate(config, project_root, configurations=None, parallel=3):
    for cfg in configs_to_run:
        # Register skills once for this config
        register_skills(cfg)
        
        # Generate N runs in parallel
        tasks = []
        for run_id in range(1, config.runs + 1):
            output_dir = output_base / cfg.name / f"run-{run_id}"
            if resume and output_dir.exists():
                continue  # Skip completed runs
            tasks.append(generate_single_run(cfg, run_id, output_dir, ...))
        
        await run_with_concurrency(tasks, max_concurrent=parallel)
        
        unregister_skills(cfg)
```

Key changes:
- Output goes to `output/{config}/run-{N}/{scenario}/` instead of `output/{config}/{scenario}/`
- Parallel execution within a config using asyncio semaphore
- Resume support: skip runs where output already exists
- Each run gets its own staging directory

#### 4.3 `verify.py` changes

```python
async def run_verify(config, project_root, parallel=3):
    tasks = []
    for cfg in config.configurations:
        for run_id in range(1, config.runs + 1):
            for scenario in config.scenarios:
                app_dir = output_base / cfg.name / f"run-{run_id}" / scenario.name
                tasks.append(verify_single(cfg, run_id, scenario, app_dir))
    
    results = await run_with_concurrency(tasks, max_concurrent=parallel)
    write_aggregated_build_notes(results)
```

Key changes:
- Iterate over all runs
- Full parallelization across all (config, run, scenario) triples
- Aggregated build-notes.md with per-run breakdown

#### 4.4 `analyze.py` changes

```python
async def run_analyze(config, project_root, parallel=3):
    # Phase 1: Per-run analysis (parallel)
    tasks = []
    for run_id in range(1, config.runs + 1):
        tasks.append(analyze_single_run(config, run_id, project_root))
    
    await run_with_concurrency(tasks, max_concurrent=parallel)
    
    # Phase 2: Aggregate (sequential, Python — no LLM)
    all_scores = parse_all_run_scores(config)
    verification_data = parse_all_verification_data(config)
    write_aggregated_analysis(all_scores, verification_data, config)
```

Key changes:
- Each run analyzed independently → `analysis-run-{N}.md`
- Score parsing from per-run analysis files
- Statistical aggregation into final `analysis.md`

#### 4.5 Per-run data storage

**Every run keeps its own complete record.** No data is discarded or summarized until the final aggregation step. The data we capture per run:

**a) Verification data** (structured, from verify step):
```python
@dataclass
class RunVerificationResult:
    run_id: int
    config_name: str
    scenario_name: str
    build_success: bool
    build_warnings: dict[str, int]  # {category: count}
    run_success: bool
    format_issues: int
    security_vulnerabilities: dict[str, int]  # {severity: count}
    timestamp: str
```

Stored as JSON at `reports/verification-data.json` for machine-readable access:
```json
{
  "runs": [
    {
      "run_id": 1,
      "config": "dotnet-webapi",
      "scenario": "FitnessStudioApi",
      "build_success": true,
      "build_warnings": {"naming": 2, "performance": 0, "security": 0},
      "run_success": true,
      "format_issues": 3,
      "security_vulnerabilities": {}
    }
  ]
}
```

**b) Analysis scores** (from LLM analysis of each run):

Each `analysis-run-{N}.md` contains the full LLM analysis with code examples and reasoning. We parse the Executive Summary table to extract structured scores.

Stored as JSON at `reports/scores-data.json`:
```json
{
  "runs": [
    {
      "run_id": 1,
      "scores": {
        "Build & Run Success": {"dotnet-webapi": 5, "no-skills": 4, "dotnet-artisan": 3},
        "Minimal API Architecture": {"dotnet-webapi": 5, "no-skills": 1, "dotnet-artisan": 4},
        ...
      },
      "weighted_totals": {"dotnet-webapi": 194.5, "no-skills": 118.0, "dotnet-artisan": 155.5}
    }
  ]
}
```

**c) Per-run analysis reports** (qualitative, from LLM):

Each `analysis-run-{N}.md` is kept in full — these contain the code examples, reasoning, and per-dimension verdicts that justify the numerical scores. The aggregated `analysis.md` references these for traceability.

#### 4.6 Score parsing

The per-run analysis files contain an Executive Summary table like:

```markdown
| Dimension [Tier] | config-a | config-b | config-c |
|---|---|---|---|
| Build & Run [CRITICAL] | 4 | 5 | 3 |
| ... | ... | ... | ... |
```

We need a robust parser that:
1. Reads each `analysis-run-{N}.md`
2. Extracts the Executive Summary table
3. Parses dimension names (with tier tags) and scores per config
4. Handles edge cases (missing scores, non-numeric values, extra whitespace)
5. Returns structured data for aggregation
6. Writes parsed data to `reports/scores-data.json` for downstream use

```python
@dataclass
class RunScores:
    run_id: int
    dimension_scores: dict[str, dict[str, int]]  # {dimension: {config: score}}
    weighted_totals: dict[str, float]  # {config: weighted_total}

def parse_run_scores(analysis_file: Path, dimensions: list[Dimension]) -> RunScores:
    """Parse scores from an analysis-run-N.md file."""
    ...

def parse_all_run_scores(config: EvalConfig) -> list[RunScores]:
    """Parse scores from all run analysis files, write to scores-data.json."""
    ...
```

#### 4.7 Aggregated analysis.md — the final report

The final `analysis.md` is generated **entirely by Python** (no LLM call needed). It reads all per-run data and computes statistics. This is fast, deterministic, and reproducible.

**Report structure:**

```markdown
# Aggregated Analysis: {eval_name}

**Runs:** {N} | **Configurations:** {C} | **Scenarios:** {S} | **Dimensions:** {D}
**Date:** {timestamp}

---

## Executive Summary

| Dimension [Tier] | config-a | config-b | config-c |
|---|---|---|---|
| Build & Run [CRITICAL] | 4.2 ± 0.4 | 4.8 ± 0.4 | 3.4 ± 0.9 |
| Modern C# [HIGH] | 4.6 ± 0.5 | 3.2 ± 0.8 | 2.8 ± 0.4 |
| ... | ... | ... | ... |

## Final Rankings

| Rank | Configuration | Mean Weighted Score | Std Dev | Min | Max | 95% CI |
|---|---|---|---|---|---|---|
| 🥇 | config-b | 182.1 | 2.1 | 179.5 | 185.0 | [179.5, 184.7] |
| 🥈 | config-a | 154.6 | 4.7 | 148.0 | 161.0 | [148.8, 160.4] |
| 🥉 | config-c | 141.8 | 2.7 | 138.5 | 145.0 | [138.5, 145.1] |

## Weighted Score per Run

Shows the trajectory and consistency of each configuration across runs.

| Run | config-a | config-b | config-c |
|---|---|---|---|
| 1 | 156.5 | 183.0 | 142.0 |
| 2 | 148.0 | 179.5 | 138.5 |
| 3 | 161.0 | 185.0 | 145.0 |
| 4 | 152.5 | 181.0 | 140.0 |
| 5 | 155.0 | 182.0 | 143.5 |

## Verification Summary (All Runs)

Hard data from automated checks — not LLM-judged.

| Configuration | Build Pass Rate | Run Pass Rate | Avg Warnings | Format Issues | Security Vulns |
|---|---|---|---|---|---|
| config-a | 14/15 (93%) | 13/15 (87%) | 12.3 | 4.2 | 0 |
| config-b | 15/15 (100%) | 15/15 (100%) | 3.1 | 0.8 | 0 |
| config-c | 12/15 (80%) | 10/15 (67%) | 18.7 | 7.5 | 2 |

### Build Failures Detail

| Configuration | Run | Scenario | Error Summary |
|---|---|---|---|
| config-a | 3 | VetClinicApi | CS1061: 'Pet' does not contain 'Vaccinations' |
| config-c | 2 | LibraryApi | CS0246: The type 'FineService' could not be found |

## Consistency Analysis

Which configs produce reliable results vs flaky ones?

| Configuration | Score σ | Verdict | Most Consistent Dim (σ) | Most Variable Dim (σ) |
|---|---|---|---|---|
| config-b | 2.1 | Very consistent | Service Abstraction (0.0) | EF Migration (1.3) |
| config-a | 4.7 | Moderately consistent | Modern C# (0.4) | Error Handling (1.1) |
| config-c | 8.2 | Inconsistent | NRT (0.0) | Build & Run (1.5) |

## Per-Dimension Breakdown

### 1. Build & Run Success [CRITICAL × 3]

| Run | config-a | config-b | config-c |
|---|---|---|---|
| 1 | 4 | 5 | 3 |
| 2 | 5 | 5 | 4 |
| 3 | 3 | 5 | 2 |
| 4 | 5 | 5 | 4 |
| 5 | 4 | 4 | 4 |
| **Mean** | **4.2** | **4.8** | **3.4** |
| **σ** | **0.8** | **0.4** | **0.9** |

**Observations:**
- config-b: Most reliable builds (σ=0.4)
- config-c: Inconsistent — 2 build failures across 5 runs
- Weighted impact: (4.8 - 3.4) × 3 = 4.2 points advantage for config-b

### 2. Minimal API Architecture [CRITICAL × 3]
[Same table format...]

[Repeat for all dimensions]

## Statistical Significance

For configurations with overlapping confidence intervals, we note that
the difference may not be statistically significant at N=5 runs.

| Comparison | Δ Mean | Overlapping CI? | Conclusion |
|---|---|---|---|
| config-b vs config-a | +27.5 | No | config-b significantly better |
| config-a vs config-c | +12.8 | Yes (partial) | Likely better, more runs needed |

## Raw Data References

- Per-run analysis: `reports/analysis-run-{1..N}.md`
- Verification data: `reports/verification-data.json`
- Score data: `reports/scores-data.json`
- Build notes: `reports/build-notes.md`
```

**Key principles for the aggregated report:**
1. **Keep all raw data** — every per-run analysis file is preserved with full LLM reasoning and code examples
2. **Statistics computed by Python** — mean, std dev, min, max, CI are deterministic, not LLM-judged
3. **Verification data is separate from analysis scores** — build/run pass rates are hard facts; dimension scores are LLM assessments
4. **Per-dimension breakdown shows every run** — the reader can see individual scores, not just averages
5. **Flag inconsistency** — high variance dimensions and configs are highlighted
6. **Statistical significance** — note when confidence intervals overlap (differences may not be real)
7. **Traceability** — the final report references per-run files and JSON data so anyone can audit the numbers

### 5. Git Cleanup & Output Gitignore

The `output/` directories in both examples contain committed generated code files. These should be removed from git and ignored going forward since multi-run will generate large amounts of output. The `reports/` directory stays in git — the analysis reports are the deliverables people want to review.

**Steps:**

1. Add `.gitignore` to each example's `output/` directory:
   ```
   # Generated output — not tracked in git
   # Re-generate with: skill-eval run
   *
   !.gitignore
   ```

2. Remove tracked output files from git:
   ```bash
   git rm -r --cached examples/aspnet-webapi/output/
   git rm -r --cached examples/aspnet-razor-pages/output/
   ```

3. Also add to the root `.gitignore` (if one exists) or create one:
   ```
   # Generated evaluation output (large, regenerable)
   examples/*/output/
   ```

**Note:** `reports/` stays tracked in git. The analysis reports, build notes, and JSON data files are the evaluation deliverables that should be committed and reviewed in PRs.

**Output directory structure** (designed for easy navigation):

```
output/
├── dotnet-webapi/                    # ← config name
│   ├── run-1/                        # ← run number
│   │   ├── FitnessStudioApi/         # ← scenario name
│   │   │   └── src/
│   │   │       └── FitnessStudioApi/
│   │   │           ├── Program.cs
│   │   │           ├── FitnessStudioApi.csproj
│   │   │           └── ...
│   │   ├── LibraryApi/
│   │   │   └── src/...
│   │   ├── VetClinicApi/
│   │   │   └── src/...
│   │   └── gen-notes.md
│   ├── run-2/
│   │   └── [same structure]
│   └── run-5/
│       └── [same structure]
├── dotnet-artisan/
│   ├── run-1/
│   └── ...
├── no-skills/
│   └── ...
└── README.md                         # ← auto-generated index file

reports/
├── analysis-run-1.md                 # ← per-run LLM analysis (full)
├── analysis-run-2.md
├── analysis-run-3.md
├── analysis-run-4.md
├── analysis-run-5.md
├── analysis.md                       # ← aggregated report (Python-generated)
├── build-notes.md                    # ← verification results (all runs)
├── verification-data.json            # ← machine-readable verification data
├── scores-data.json                  # ← machine-readable parsed scores
└── README.md                         # ← auto-generated index file
```

The `output/README.md` and `reports/README.md` are auto-generated index files listing what each directory contains, when the evaluation was run, and how to navigate the results. These help people who browse the folders understand the structure.

### 6. Backward Compatibility

- `runs: 1` in eval.yaml still works (single run, no run-N directory — OR use run-1/ for consistency)
- Decision: **always use `run-{N}` directories**, even for `runs: 1`. This keeps the code path consistent.
- Old `output/{config}/{scenario}` directories are not compatible — users must regenerate.

### 6. Resume Support

For long-running evaluations (5 runs × 5 configs = 25 generation cycles), failures are likely. The `--resume` flag:

1. **Generation**: Skip `output/{config}/run-{N}/` if directory exists and contains files
2. **Verification**: Skip verification for runs already in build-notes.md
3. **Analysis**: Skip `analysis-run-{N}.md` if file exists
4. **Aggregation**: Always re-run (fast, Python-only)

### 7. Additional CLI Parameters to Consider

| Parameter | Purpose | Rationale |
|-----------|---------|-----------|
| `--scenarios, -s` | Filter to specific scenarios | Useful for debugging a single scenario across configs |
| `--parallel, -p` | Max concurrent workers | Control resource usage; default 3 |
| `--resume` | Skip completed work | Essential for recovering from failures in long runs |
| `--run-ids` | Operate on specific runs | Re-run only failed runs: `--run-ids 3 5` |
| `--seed` | Random seed for config ordering | Reproducible analysis ordering across runs |
| `--dry-run` | Show what would run without executing | Preview the execution plan |
| `--timeout` | Per-generation timeout | Cap runaway Copilot sessions |

### 8. Implementation Order

| Phase | Work | Depends On |
|-------|------|------------|
| **A** | Git cleanup: remove output/reports from git, add .gitignore | — |
| **B** | Config model: change default runs to 5, add new output settings | — |
| **C** | Output structure: update generate.py for `run-{N}` directories | B |
| **D** | CLI updates: add `--scenarios`, `--parallel`, `--resume`, `--run-ids` | B |
| **E** | Parallel infrastructure: asyncio semaphore utility | — |
| **F** | Generate: parallel runs within config, resume support | C, D, E |
| **G** | Verify: parallel verification across all runs, aggregated build-notes | C, E |
| **H** | Analyze: per-run analysis, parallel execution | C, E |
| **I** | Score parser: extract scores from analysis markdown | H |
| **J** | Aggregation: statistical summary, final analysis.md | I |
| **K** | Templates: update analyze.md.j2, create aggregate template | J |
| **L** | Documentation: update README, authoring guide | All |

### 9. Open Questions

1. ~~**Should aggregation be LLM or Python?**~~ **DECIDED: Python.** The statistical tables (mean, std dev, CI) are computed deterministically by Python. Per-run LLM analysis files are kept for qualitative reference. The aggregated report is generated without an LLM call — it's fast, reproducible, and auditable.

2. **Copilot CLI rate limits**: How many concurrent Copilot sessions can we run? Need to test with `--parallel 2` vs `3` vs `5`. **Recommendation:** Default to 3, let users increase.

3. **Deterministic config ordering per run**: Should each run use a different random order for bias mitigation? **Recommendation:** Yes — use `random.seed(run_id)` so each run has a different (but reproducible) config ordering.

4. **Storage impact**: 5 runs × 5 configs × 3 scenarios = 75 generated apps. Could be significant disk usage. **Recommendation:** Add a `--clean` flag to remove intermediate output after analysis.

5. **Score parsing robustness**: The LLM-generated analysis tables may have formatting inconsistencies across runs. Need a fault-tolerant parser that handles missing scores, non-numeric values, and variant table formats. **Recommendation:** Log warnings for unparseable scores rather than failing; require a minimum number of parseable runs to produce aggregation.
