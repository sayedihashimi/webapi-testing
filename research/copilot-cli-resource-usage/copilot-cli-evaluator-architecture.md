# Evaluator Architecture: Copilot CLI Resource Usage Detection

## Overview

This document describes a reusable evaluator that determines which Copilot CLI skills, custom instructions, and other resources are selected or injected for a given prompt. The evaluator is designed as a **Windows-first** implementation with a clear path to cross-platform support.

---

## Architecture Diagram (Textual)

```
┌─────────────────────────────────────────────────────────────────────┐
│                        EVALUATOR RUNNER                            │
│                                                                     │
│  ┌──────────────┐   ┌──────────────┐   ┌──────────────────────┐   │
│  │  Scenario     │   │  Config      │   │  Isolation           │   │
│  │  Loader       │   │  Manager     │   │  Manager             │   │
│  │              │   │              │   │                      │   │
│  │  - prompts   │   │  - skills    │   │  - temp directories  │   │
│  │  - expected  │   │  - toggles   │   │  - skill symlinks    │   │
│  │    outcomes  │   │  - A/B pairs │   │  - hooks setup       │   │
│  └──────┬───────┘   └──────┬───────┘   └──────────┬───────────┘   │
│         │                  │                       │               │
│         ▼                  ▼                       ▼               │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                    RUN ORCHESTRATOR                          │   │
│  │                                                             │   │
│  │  For each scenario:                                         │   │
│  │    1. Set up isolated environment (skills, instructions)    │   │
│  │    2. Install instrumentation (hooks, fs watcher)           │   │
│  │    3. Execute: copilot -p <PROMPT> [flags] --share <path>   │   │
│  │    4. Collect evidence from all sources                     │   │
│  │    5. Tear down environment                                 │   │
│  │    6. Repeat for A/B variant if configured                  │   │
│  └─────────────────────────┬───────────────────────────────────┘   │
│                            │                                       │
│                            ▼                                       │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                  INSTRUMENTATION LAYER                       │   │
│  │                                                             │   │
│  │  ┌──────────┐  ┌──────────┐  ┌───────────┐  ┌───────────┐ │   │
│  │  │  Hooks   │  │  FS      │  │  Session  │  │ Transcript│ │   │
│  │  │  Logger  │  │  Watcher │  │  Analyzer │  │ Parser    │ │   │
│  │  │          │  │          │  │           │  │           │ │   │
│  │  │ preTool  │  │ procmon  │  │ SQLite    │  │ --share   │ │   │
│  │  │ postTool │  │ fswatch  │  │ JSON      │  │ markdown  │ │   │
│  │  │ session  │  │ inotify  │  │ session   │  │ analysis  │ │   │
│  │  └────┬─────┘  └────┬─────┘  └─────┬─────┘  └─────┬─────┘ │   │
│  │       │             │              │              │         │   │
│  └───────┼─────────────┼──────────────┼──────────────┼─────────┘   │
│          │             │              │              │              │
│          ▼             ▼              ▼              ▼              │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                  EVIDENCE COLLECTOR                          │   │
│  │                                                             │   │
│  │  Merges evidence from all instrumentation sources           │   │
│  │  Applies confidence scoring (see evidence-model.md)         │   │
│  │  Classifies each resource: Confirmed/Probable/Possible/etc  │   │
│  │  Handles A/B differential analysis                          │   │
│  └─────────────────────────┬───────────────────────────────────┘   │
│                            │                                       │
│                            ▼                                       │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                  REPORT GENERATOR                            │   │
│  │                                                             │   │
│  │  Per-prompt resource usage report                           │   │
│  │  Cross-prompt summary matrix                                │   │
│  │  A/B comparison tables                                      │   │
│  │  Confidence distributions                                   │   │
│  │  Formats: JSON, Markdown, HTML                              │   │
│  └─────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Data Flow

```
Scenario Definition (YAML/JSON)
    │
    ▼
Environment Setup
    │ - Copy/symlink skills to .github/skills/
    │ - Install hooks.json with logging hooks
    │ - Start filesystem watcher (optional)
    │ - Set COPILOT_HOME to isolated directory
    │
    ▼
Copilot CLI Execution
    │ copilot -p "<prompt>" --share ./evidence/<run_id>.md
    │          --allow-all --no-ask-user --model <model>
    │
    ├──→ Hooks fire (preToolUse, postToolUse, sessionStart/End)
    │    └──→ Write to ./evidence/<run_id>/hooks.jsonl
    │
    ├──→ Filesystem watcher records
    │    └──→ Write to ./evidence/<run_id>/fs_events.jsonl
    │
    ├──→ Session transcript generated
    │    └──→ ./evidence/<run_id>/transcript.md
    │
    └──→ Session state files updated
         └──→ ~/.copilot/session-state/<session_id>/
    │
    ▼
Evidence Collection
    │ - Parse hooks.jsonl for tool usage
    │ - Parse fs_events.jsonl for file reads
    │ - Parse transcript.md for skill references
    │ - Query session store for tool/turn data
    │ - If A/B: compare with variant run
    │
    ▼
Scoring & Classification
    │ - Apply evidence model
    │ - Compute confidence scores
    │ - Classify resources
    │
    ▼
Report Generation
    └──→ ./reports/<scenario_id>/report.json
    └──→ ./reports/<scenario_id>/report.md
    └──→ ./reports/summary.md
```

---

## Report Schema

### Per-Run Report (JSON)

```json
{
  "run_id": "run-2026-03-28-001",
  "scenario_id": "test-webapp-skill",
  "prompt": "Create unit tests for the user authentication module",
  "model": "claude-sonnet-4.6",
  "timestamp": "2026-03-28T14:30:00Z",
  "duration_seconds": 45,
  "variant": "A",
  "environment": {
    "skills_available": [
      {"name": "webapp-testing", "path": ".github/skills/webapp-testing/SKILL.md"},
      {"name": "code-review", "path": ".github/skills/code-review/SKILL.md"}
    ],
    "instructions_active": [
      ".github/copilot-instructions.md",
      "$HOME/.copilot/copilot-instructions.md"
    ],
    "custom_instructions_disabled": false,
    "plugins_installed": [],
    "mcp_servers": ["github"]
  },
  "resources": [
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
          "detail": "File read detected at T+2.3s"
        },
        {
          "source": "tool_pattern_match",
          "class": "Weak Inference",
          "detail": "Agent followed 5-step testing procedure from skill"
        }
      ],
      "negative_evidence": []
    },
    {
      "resource_path": ".github/copilot-instructions.md",
      "resource_type": "custom_instruction",
      "resource_name": "repo-instructions",
      "classification": "Confirmed Used",
      "confidence_score": 95,
      "evidence": [
        {
          "source": "always_on_rule",
          "class": "Direct Observation",
          "detail": "Custom instructions are always loaded when present and --no-custom-instructions not set"
        }
      ],
      "negative_evidence": []
    }
  ],
  "tools_used": [
    {"tool": "bash", "count": 3, "args_summary": ["npm test", "node --version", "cat package.json"]},
    {"tool": "edit", "count": 2, "args_summary": ["src/auth.test.ts", "src/auth.test.ts"]},
    {"tool": "view", "count": 5}
  ],
  "ab_comparison": null
}
```

### A/B Comparison (when applicable)

```json
{
  "scenario_id": "test-webapp-skill-ab",
  "variant_a": { "run_id": "...", "skills_enabled": ["webapp-testing"] },
  "variant_b": { "run_id": "...", "skills_enabled": [] },
  "behavioral_diff": {
    "output_similarity": 0.35,
    "tool_usage_diff": ["variant_a used 'npm test'; variant_b did not"],
    "procedure_match": "variant_a followed skill's 5-step procedure; variant_b used ad-hoc approach"
  },
  "conclusion": "webapp-testing skill was loaded and influenced behavior",
  "confidence": 88
}
```

---

## Isolation Strategy

### Per-Run Isolation

Each evaluation run must be isolated to prevent cross-contamination:

1. **COPILOT_HOME Override**: Set `COPILOT_HOME` to a temporary directory per run. This isolates session state, config, and personal skills.

2. **Working Directory**: Execute each run in a dedicated temporary clone/copy of the repo under test, or use `--add-dir` to control scope.

3. **Skill Configuration**:
   - Create a temporary `.github/skills/` directory with only the skills being tested
   - Use `/skills` toggle programmatically (in interactive mode) or control via directory presence (in `-p` mode)

4. **Instruction Control**: Use `--no-custom-instructions` for the control variant in A/B tests.

5. **Clean Session State**: Each run should start with a fresh session (no `--resume`).

### Concurrency

Runs can execute concurrently if each has its own:
- Working directory
- COPILOT_HOME
- Hooks output directory
- Filesystem watcher scope

---

## Early-Stop Strategy

Since no official dry-run mode exists, the evaluator uses a **constrained execution** approach:

### Strategy 1: Deny All Tools (Strongest Early-Stop)

```
copilot -p "<prompt>" --deny-tool=shell --deny-tool=write --no-ask-user --share <path>
```

This allows Copilot to:
- Parse the prompt
- Select relevant skills and instructions
- Plan a response
- Attempt to use tools (which will be denied)

The session transcript will show which tools Copilot *tried* to use, which may reveal skill influence.

**Limitation**: Copilot may still read files (the `read` tool is not denied here). To deny reads too: `--deny-tool=read`.

### Strategy 2: Hook-Based Abort

Use a `preToolUse` hook that denies all tool executions after logging:

```json
{
  "version": 1,
  "hooks": {
    "preToolUse": [
      {
        "type": "command",
        "powershell": "./scripts/log-and-deny.ps1"
      }
    ]
  }
}
```

```powershell
# log-and-deny.ps1
$input = [Console]::In.ReadToEnd() | ConvertFrom-Json
$entry = @{
    timestamp = $input.timestamp
    toolName = $input.toolName
    toolArgs = $input.toolArgs
} | ConvertTo-Json -Compress
Add-Content -Path "evidence/hooks.jsonl" -Value $entry

# Deny the tool
@{
    permissionDecision = "deny"
    permissionDecisionReason = "Evaluator: logging only, no execution"
} | ConvertTo-Json -Compress
```

This logs every tool the agent tries to use, then denies it. The agent will eventually give up or produce a response without tools.

### Strategy 3: Deny Only Mutation Tools

Allow read-only tools but deny writes and shell:

```
copilot -p "<prompt>" --allow-tool=read --deny-tool=shell --deny-tool=write --no-ask-user --share <path>
```

This allows Copilot to explore the codebase (read files, search) but prevents mutations. The transcript reveals planning and resource usage.

---

## Windows-First Implementation Notes

### Filesystem Watcher: Process Monitor (procmon)

On Windows, use Sysinternals Process Monitor for file-read detection:

```powershell
# Start procmon with filter for copilot process reading .md files
$procmonPath = "C:\Tools\Sysinternals\Procmon.exe"
$configPath = "$PSScriptRoot\procmon-filter.pmc"
& $procmonPath /Quiet /Minimized /BackingFile "$evidencePath\procmon.pml" /LoadConfig $configPath

# ... run copilot ...

# Stop and convert
& $procmonPath /Terminate
& $procmonPath /OpenLog "$evidencePath\procmon.pml" /SaveAs "$evidencePath\procmon.csv"
```

Filter configuration should match:
- Process name contains "copilot" or "node"
- Operation is "ReadFile" or "CreateFile"
- Path ends with `.md` or is in `.github/skills/` or `.github/instructions/`

### Alternative: .NET FileSystemWatcher

For a lighter-weight approach on Windows:

```powershell
$watcher = New-Object System.IO.FileSystemWatcher
$watcher.Path = ".github\skills"
$watcher.Filter = "*.md"
$watcher.IncludeSubdirectories = $true
$watcher.EnableRaisingEvents = $true

Register-ObjectEvent $watcher "Changed" -Action {
    $event = $EventArgs
    "$([DateTime]::Now.ToString('o')),$($event.FullPath),$($event.ChangeType)" | 
        Add-Content "evidence\fs_events.csv"
}
```

**Limitation**: FileSystemWatcher detects writes/changes but may not detect reads. For read detection, Process Monitor or ETW tracing is required on Windows.

### Hooks: PowerShell Scripts

All hook scripts should use PowerShell for Windows-first:

```powershell
# Generic hook logger (works for all hook types)
$raw = [Console]::In.ReadToEnd()
$input = $raw | ConvertFrom-Json
$entry = @{
    hookType = $MyInvocation.ScriptName
    data = $input
    timestamp = Get-Date -Format "o"
} | ConvertTo-Json -Compress
Add-Content -Path "evidence\hooks.jsonl" -Value $entry
```

---

## Cross-Platform Notes

| Component | Windows | macOS | Linux |
|-----------|---------|-------|-------|
| Filesystem watcher (reads) | Process Monitor, ETW | dtrace, fs_usage | inotifywait, strace |
| Filesystem watcher (writes) | FileSystemWatcher | fswatch | inotifywait |
| Hook scripts | PowerShell | Bash | Bash |
| Process tracing | procmon | dtrace | strace |
| SQLite analysis | sqlite3 CLI or PowerShell | sqlite3 | sqlite3 |

To support cross-platform:
- Use both `bash` and `powershell` keys in hooks.json
- Abstract filesystem watcher behind a platform adapter interface
- Use SQLite libraries available on all platforms (Python sqlite3 module)

---

## Deterministic Test Harness Design

### Repeatability Controls

1. **Pin the model**: Always use `--model=<specific_model>` to reduce variance
2. **Set reasoning effort**: Use `reasoning_effort: "low"` in config for faster, more deterministic responses
3. **Fixed prompts**: Use exact prompt text from scenario definitions
4. **Seed control**: If the API supports temperature/seed, pin these values
5. **Clean environment**: Fresh COPILOT_HOME per run
6. **Multiple runs**: Run each scenario 3+ times and compute confidence variance

### CI Integration

```yaml
# GitHub Actions example
jobs:
  evaluate-skills:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - name: Install Copilot CLI
        run: winget install GitHub.Copilot --accept-package-agreements
      - name: Run evaluator
        env:
          COPILOT_GITHUB_TOKEN: ${{ secrets.COPILOT_TOKEN }}
          COPILOT_HOME: ${{ runner.temp }}/copilot-eval
        run: python evaluator/run.py --scenarios scenarios/ --output reports/
      - name: Upload reports
        uses: actions/upload-artifact@v4
        with:
          name: skill-evaluation-reports
          path: reports/
```
