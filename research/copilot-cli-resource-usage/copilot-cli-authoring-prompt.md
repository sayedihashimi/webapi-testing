# Implementation Prompt: Copilot CLI Resource Usage Evaluator

Use this prompt to implement the evaluator designed in the research files.

---

## Prompt

Build a reusable **Copilot CLI Resource Usage Evaluator** for this repository. The evaluator determines which skills, custom instructions, and other resources Copilot CLI selects or injects for a given prompt.

### Source of Truth

The following research files in `research/` define the design. Read them first:

1. `copilot-cli-resource-usage-report.md` — Full findings and recommendations
2. `copilot-cli-evaluator-architecture.md` — Architecture, data flow, report schema, isolation strategy, early-stop strategy
3. `copilot-cli-evidence-model.md` — Evidence classes, confidence scoring, classification taxonomy
4. `copilot-cli-validation-plan.md` — Synthetic test skills, canary markers, experiment matrix
5. `copilot-cli-observability-matrix.md` — Approach comparison matrix

### What to Build

**Phase 1 — Canary Skills and Quick Test (implement first):**

1. Create 3-5 synthetic test skills in `.github/skills/` with unique canary markers per `copilot-cli-validation-plan.md`
2. Create a canary custom instruction in `.github/copilot-instructions.md`
3. Write a runner script (PowerShell or Python) that:
   - Runs `copilot -p <prompt> --share <path> --allow-all --no-ask-user --model <model>`
   - Parses the transcript for canary markers
   - Reports which markers were found

**Phase 2 — Scenario-Based Runner:**

1. Define scenario format (YAML): prompt, expected skills, enabled/disabled resources
2. Build the run orchestrator per the architecture doc:
   - Set up COPILOT_HOME isolation (temp directory)
   - Copy/symlink skills to `.github/skills/`
   - Install `hooks.json` with tool-logging hooks (per architecture doc)
   - Execute Copilot CLI programmatically
   - Collect evidence from hooks, transcript, and filesystem
   - Run A/B variants (with/without each resource)
3. Implement evidence scoring per the evidence model
4. Generate Markdown report per the report schema

**Phase 3 — Filesystem Watcher (Windows-first):**

1. Integrate Process Monitor or .NET FileSystemWatcher
2. Detect reads of SKILL.md and instruction files during runs
3. Correlate file-read events with prompts

### Constraints

- Windows-first (PowerShell + Python)
- Use official CLI flags only (no binary patching)
- Each run costs one Copilot premium request — minimize unnecessary runs
- Follow the evidence model for confidence scoring
- Validate with the experiment matrix from the validation plan

### Output

- Evaluator code in `src/` or `evaluator/`
- Scenario definitions in `scenarios/`
- Reports in `reports/`
- Canary skills in `.github/skills/` (for testing; can be removed after validation)
