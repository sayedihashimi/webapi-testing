# Plan: Auto-Improve Command

## Problem Statement

Users working on a specific skill/plugin want an automated way to iteratively improve it. Today the workflow is manual: run the evaluation pipeline, read the improvements file, apply changes, re-run, and repeat. We want to automate this into a single command that loops until the skill reaches a target quality level or stops improving.

## Proposed Approach

Add a new `auto-improve` CLI command that orchestrates an autonomous improvement loop:

```
skill-eval auto-improve \
  --configuration my-skill-config \
  --max-turns 5 \
  --min-improvement 0.5 \
  --target-score 9.0 \
  --runs-per-iteration 1 \
  --final-runs 3
```

Each iteration:
1. Run the full pipeline (generate → verify → analyze → suggest-improvements) for just 1 run (fast feedback)
2. Parse scores from `scores-data.json` for the target configuration
3. Compare against previous iteration scores — compute weighted average delta
4. Check stopping conditions
5. If not stopped: invoke Copilot CLI to apply the improvements file to the skill/plugin source files
6. Loop back to step 1

On the final iteration (when stopping), optionally re-run with `--final-runs` (e.g., 3) for a statistically robust final score.

## Stopping Conditions

Any one of these triggers exit:

| Condition | Default | Flag |
|-----------|---------|------|
| Weighted average score ≥ target | 9.0 | `--target-score` |
| Score delta < threshold for consecutive turns | 0.5 | `--min-improvement` |
| Max iterations reached | 5 | `--max-turns` |
| Score decreased for 2 consecutive turns | always on | (built-in regression guard) |

## Architecture

### New Files

- **`src/skill_eval/auto_improve.py`** — Core loop logic, score tracking, stopping condition evaluation, and Copilot-based improvement application.

### Modified Files

- **`src/skill_eval/cli.py`** — Add the `auto-improve` command with CLI options.

### Key Design Decisions

#### Score Tracking

After each iteration, read `scores-data.json` (already produced by the aggregator) and extract the weighted average for the target configuration. Maintain an iteration history in `reports/auto-improve-history.json`:

```json
{
  "configuration": "my-skill-config",
  "iterations": [
    {
      "turn": 1,
      "weighted_average": 6.2,
      "delta": null,
      "per_dimension": {"Code Quality": 7.0, "Error Handling": 5.5},
      "timestamp": "2026-04-16T03:00:00Z",
      "improvements_applied": true
    },
    {
      "turn": 2,
      "weighted_average": 7.1,
      "delta": 0.9,
      "per_dimension": {"Code Quality": 7.5, "Error Handling": 6.8},
      "timestamp": "2026-04-16T03:15:00Z",
      "improvements_applied": true
    }
  ],
  "stop_reason": "target_score_reached",
  "final_score": 9.1
}
```

#### Applying Improvements

The improvements file (`reports/improvements-{config}.md`) contains actionable suggestions. To apply them, invoke the Copilot CLI:

```python
prompt = (
    f"Read the improvement suggestions in {improvements_path} and apply them "
    f"to the skill/plugin source files at {skill_plugin_paths}. "
    f"Make only the changes described in the improvements file. "
    f"Do not modify any other files. Do not delete files."
)
cmd = ["copilot", "-p", prompt, "--yolo"]
```

This reuses the same Copilot CLI invocation pattern already used in `generate.py` and `suggest_improvements.py`.

#### Backup & Rollback

Before each "apply improvements" step:
1. Create a git commit (or snapshot copy) of the skill/plugin directory
2. If the next iteration's score regresses, restore from the backup
3. This ensures we never lose a known-good state

The preferred approach is git-based (if the skill source is in a git repo):
```python
# Before applying improvements
subprocess.run(["git", "add", "."], cwd=skill_dir)
subprocess.run(["git", "commit", "-m", f"auto-improve: snapshot before turn {turn}"], cwd=skill_dir)

# On rollback
subprocess.run(["git", "revert", "--no-commit", "HEAD"], cwd=skill_dir)
```

For non-git directories, use `shutil.copytree` to a temp backup.

#### Pipeline Configuration Per Iteration

During the loop, use `--runs 1` for fast iteration. The config object is mutated in-memory:
```python
config.runs = runs_per_iteration  # default: 1
```

On the final validation pass (optional), restore to `--final-runs` (default: 3) for statistical robustness.

#### Isolation from Existing Pipeline

The auto-improve loop reuses existing functions directly:
- `run_generate()` from `generate.py`
- `run_verify()` from `verify.py`
- `run_analyze()` from `analyze.py`
- `run_suggest_improvements()` from `suggest_improvements.py`

No changes needed to these modules. The loop just calls them in sequence, same as the `run` command does.

## CLI Interface

```python
@main.command("auto-improve")
@click.option("--configuration", "-c", required=True,
              help="Name of the configuration to improve (must have suggest_improvements: true)")
@click.option("--max-turns", type=int, default=5,
              help="Maximum number of improvement iterations (default: 5)")
@click.option("--target-score", type=float, default=9.0,
              help="Stop when weighted average score reaches this value (default: 9.0)")
@click.option("--min-improvement", type=float, default=0.5,
              help="Stop if score improvement is below this threshold (default: 0.5)")
@click.option("--runs-per-iteration", type=int, default=1,
              help="Number of generation runs per iteration (default: 1, use 1 for fast feedback)")
@click.option("--final-runs", type=int, default=None,
              help="Number of runs for the final validation pass (default: same as --runs-per-iteration)")
@click.option("--generation-model", type=str, default=None,
              help="AI model for code generation (overrides eval.yaml)")
@click.option("--analysis-model", type=str, default=None,
              help="AI model for analysis (overrides eval.yaml)")
@click.option("--improvement-model", type=str, default=None,
              help="AI model for improvement suggestions (overrides eval.yaml)")
@click.option("--no-rollback", is_flag=True,
              help="Disable automatic rollback on score regression")
```

## Implementation Todos

### 1. Create `auto_improve.py` module
Core module with the improvement loop. Key functions:
- `run_auto_improve()` — main entry point, orchestrates the loop
- `_extract_weighted_average()` — read scores-data.json and compute weighted average for a config
- `_apply_improvements()` — invoke Copilot CLI to apply improvements from the markdown file
- `_snapshot_skill_dir()` — git commit or file copy before applying changes
- `_rollback_skill_dir()` — revert to previous snapshot on regression
- `_check_stopping_conditions()` — evaluate all stopping criteria
- `_write_iteration_history()` — persist iteration history to JSON

### 2. Add CLI command to `cli.py`
Add the `auto-improve` command with all options listed above. Wire it to `run_auto_improve()`.

### 3. Create the improvement application prompt
Write a focused prompt template that tells Copilot exactly how to apply the improvements from the markdown file to the skill/plugin source files. This could be a Jinja2 template in `templates/` or an inline prompt string. Key requirements:
- Reference the improvements file path
- List the skill/plugin source file paths
- Instruct Copilot to only modify the skill/plugin files
- Instruct Copilot not to delete files or change unrelated code

### 4. Add iteration history tracking
Write `auto-improve-history.json` to the reports directory after each iteration. Include per-dimension scores, weighted averages, deltas, timestamps, and the final stop reason.

### 5. Add summary output
At the end of the loop, print a summary table showing the score progression across iterations:
```
Turn  Score  Delta  Status
  1    6.2     —    ✅ Improvements applied
  2    7.1   +0.9   ✅ Improvements applied
  3    8.5   +1.4   ✅ Improvements applied
  4    9.1   +0.6   🎯 Target score reached
```

## Risks & Mitigations

| Risk | Mitigation |
|------|-----------|
| Copilot may not correctly apply improvements | Validate skill files still exist after apply; rollback on failure |
| Cost — each iteration runs the full pipeline | Default to `--runs 1` during iteration; multi-run only for final validation |
| Improvements file may be vague or contradictory | The improvement prompt template already produces concrete, actionable suggestions |
| Infinite loop if scores oscillate | Regression guard (2 consecutive decreases) + max-turns cap |
| Git repo may have uncommitted changes | Warn user and require clean working tree before starting |
| Non-git skill sources (remote repos) | Only support auto-improve for local sources initially; error for git source refs |

## Future Enhancements

- **Dry-run mode** — Show what would be done without applying changes
- **Selective dimensions** — Focus improvement on specific low-scoring dimensions
- **A/B branching** — Create a git branch for the improved version
- **Cost estimation** — Estimate API cost before starting based on runs × iterations
- **Resume support** — Resume an interrupted auto-improve session from the history file
