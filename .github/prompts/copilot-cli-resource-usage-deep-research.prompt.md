---
description: Deep research prompt for GitHub Copilot CLI to investigate how to determine which skills and related customization resources are selected or injected for a given prompt, and how to stop execution before normal task processing.
---

# Deep Research: Evaluating Which Copilot CLI Skill Files and Resources Are Used for a Prompt

You are performing **deep research** on how to build a **reusable evaluation method** for **GitHub Copilot CLI** that determines, for a given **prompt + configuration**, which **skills and related resources** are:

1. **selected for inclusion**, and/or
2. **actually injected into model context**

My goal is to use this research to help build an **evaluator for a repository**. The evaluator will configure the skills/plugins being tested, run prompts, and produce a report showing which skills/resources were selected or used.

I am especially interested in approaches that can reveal this information **before Copilot finishes normal task processing**. Ideal outcome:

- **Best case:** a **dry-run mode** that reports the selected/injected resources without allowing the full request to proceed.
- **Second best:** allow selection/loading to happen, then **abort before full answer generation or file/tool work**.
- **Fallback:** if hard interception is not possible, identify the strongest available instrumentation method that can still reliably report selected/loaded resources.

Cross-platform is preferred, but it is acceptable to start with **Windows-only** if necessary.

---

## What to investigate

Focus on **GitHub Copilot CLI** first.

The evaluation target is not limited to one resource type. The evaluator may configure available skills/plugins, and we want to detect which relevant Copilot resources are used for a specific prompt.

Investigate detection of these resource categories where applicable:

- `SKILL.md`
- subsidiary skill files referenced by `SKILL.md`
- supporting Markdown docs in a skill folder
- scripts/tools bundled with a skill
- repo-level custom instructions
- path-specific or folder-level custom instructions
- agent-related configuration used by Copilot CLI
- any other prompt-shaping resources that affect selection or context loading
- any local repo files that are implicitly read as part of planning or context gathering, if detectable

### Important distinction

You must carefully distinguish among these different meanings of “used”:

- **discovered**: Copilot found the file as available configuration/resource
- **selected**: Copilot chose the resource as relevant for this prompt
- **loaded/injected**: the resource content was actually placed into prompt/model context
- **opened/read**: the CLI or agent accessed the file locally
- **influenced**: the resource materially changed behavior or output

My highest priority is:

1. **selected for inclusion**
2. **actually injected into model context**

If you cannot directly prove injection, identify the best available proxies and how trustworthy they are.

---

## Research goals

Produce implementation-grade guidance for how to determine resource usage for a given prompt.

Specifically answer:

1. **What officially supported observability exists today in Copilot CLI?**
   - logs
   - hooks
   - session data
   - programmatic invocation
   - any debug/verbose modes
   - plugin surfaces
   - environment variables
   - anything else documented

2. **Can Copilot CLI be instrumented to reveal which skill files/resources were selected or injected before full processing?**
   - directly
   - indirectly
   - with hooks
   - with logging
   - with wrapper scripts
   - with filesystem instrumentation
   - with network/proxy interception
   - with local process tracing
   - with plugin-level instrumentation
   - with extension/package patching if necessary

3. **What is the safest and most maintainable approach for a reusable evaluator?**
   - official APIs/hooks first
   - minimal-intrusion approaches second
   - fragile/reverse-engineering approaches last

4. **How can we stop or short-circuit execution?**
   - dry-run mode, if available
   - abort after selection/loading
   - abort before tool execution
   - abort before file mutation
   - abort before final answer generation
   - simulate or constrain execution enough to learn resource usage

5. **What evidence can be captured per prompt run?**
   - selected skills
   - selected skill folders/plugins
   - selected custom instruction files
   - loaded file paths
   - prompt-to-resource mapping confidence
   - timing / phase breakdown
   - whether the evidence is direct or inferred

6. **What would a practical evaluator architecture look like?**
   - runner
   - instrumentation layer
   - collection pipeline
   - normalization
   - report format
   - confidence scoring
   - portability strategy
   - CI suitability

---

## Source priorities

Prioritize sources in this order:

1. **Official GitHub documentation** for Copilot CLI, hooks, session data, logging, plugins, skills, and customization
2. Official GitHub repos or source code, if public and relevant
3. High-quality public investigations, writeups, or example repos
4. Community experiments only when they add concrete evidence

---

## Key research questions

### 1) Officially supported observability

Determine what GitHub Copilot CLI officially supports today for observing behavior.

Research:

- available log files and what they contain
- `--log-dir` and related CLI options
- hook triggers and what data hooks can access
- local session history / chronicle data and whether it includes resource selection details
- programmatic invocation and whether it exposes structured data
- whether there is any dry-run, trace, debug, inspect, or explain mode
- whether plugins can observe resource resolution or selection
- whether custom instructions / skills loading can be disabled selectively for A/B evaluation

For each supported capability, document:
- what it does
- what it does **not** reveal
- whether it can help identify selected/injected resources
- whether it can be used before normal completion

### 2) Resource resolution model

Research how Copilot CLI discovers and uses:

- skills
- skill support files
- custom instructions
- agents
- prompt-shaping repo files

Document what is known versus inferred about:
- discovery
- selection
- loading
- injection into model context
- precedence / ordering
- opt-out flags or controls

### 3) Candidate instrumentation strategies

Evaluate possible approaches, including but not limited to:

#### A. Official / low-risk
- hook-based tracing
- log parsing
- session-data analysis
- programmatic wrapper around `copilot -p`
- plugin-based observability if possible
- controlled A/B runs with flags such as disabling custom instructions where supported

#### B. Moderate-risk
- filesystem watchers to detect reads of candidate files
- local process tracing
- environment manipulation
- tool-call interception
- sandboxing or controlled directories

#### C. High-risk / fragile
- HTTP proxy / MITM
- binary/package patching
- monkeypatching runtime code
- reverse engineering internal protocol details

For each strategy:
- explain how it would work
- whether it can distinguish “selected” from “injected”
- whether it can stop execution early
- reliability
- maintenance cost
- cross-platform support
- ethical/legal/tooling risks
- fit for a reusable evaluator

### 4) Early-stop / dry-run strategies

Research whether it is possible to:

- make Copilot resolve context and stop
- allow skill selection but block downstream tool use
- force a no-op or harmless termination after selection
- run with a constrained environment that reveals resource loading without allowing normal work
- intercept at a hook boundary and exit
- use dummy tools or fake sandboxes to trap execution

If no true dry-run exists, design the closest safe substitute.

### 5) Evidence model

Design a rigorous evidence model for a report that answers:
- which resources were definitely used
- which were probably used
- which were only available but not proven used

Define confidence levels such as:
- direct evidence
- strong inference
- weak inference

### 6) Evaluator architecture

Design a reusable evaluator for a repo.

Include:
- how prompts are supplied
- how skill/plugin configurations are varied
- how runs are isolated
- how evidence is collected
- how early-stop behavior is enforced
- how results are normalized
- how the report is generated

Address:
- Windows-first implementation options
- path to cross-platform support
- deterministic test harness design
- repeatability concerns

### 7) Validation methodology

Propose how to validate that the evaluator is correct.

Include:
- synthetic test skills with unique markers
- canary resources
- A/B experiments
- negative controls
- prompts that should match exactly one skill
- prompts that should match no skills
- prompts that intentionally overlap multiple skills

Explain how to tell whether the evaluator is missing selected resources or falsely attributing them.

### 8) Practical recommendations

At the end, recommend:
- the best official/low-risk path
- the best practical path even if not officially supported
- the minimum viable evaluator
- the strongest architecture for long-term maintainability

---

## Constraints and preferences

- Prefer **officially supported** methods when possible.
- But do **not** stop there if official support is insufficient.
- Explore unsupported methods too, as long as you clearly label:
  - support level
  - fragility
  - maintenance cost
  - safety/ethics concerns
- Focus on methods that can support a **reusable evaluator for a repository**, not one-off manual debugging.

---

## Deliverables

Create these files as separate Markdown files.

### 1. `research/copilot-cli-resource-usage-report.md`
Comprehensive report.

Requirements:
- clearly separate facts, inferences, and experimental proposals
- include a decision matrix across instrumentation approaches
- include a recommended path section
- include a section specifically titled:
  `Can we know what Copilot selected before it finishes?`

### 2. `research/copilot-cli-resource-usage-summary.md`
Short executive summary.

Requirements:
- high signal
- concise
- practical conclusions only

### 3. `research/copilot-cli-observability-matrix.md`
Matrix of observability options.

Rows should include:
- hooks
- logs
- session data
- programmatic CLI
- plugins
- filesystem monitoring
- process tracing
- proxy interception
- patching / reverse engineering
- any other meaningful option

Columns should include:
- support level
- can detect selection
- can detect injection
- can stop early
- cross-platform
- implementation difficulty
- confidence quality
- maintenance risk

### 4. `research/copilot-cli-evaluator-architecture.md`
Recommended evaluator design.

Requirements:
- component diagram or textual architecture
- data flow
- report schema
- isolation strategy
- early-stop strategy
- Windows-first implementation notes
- cross-platform notes

### 5. `research/copilot-cli-evidence-model.md`
Evidence and confidence model.

Requirements:
- define evidence classes
- define confidence scoring
- define how to classify direct vs inferred usage
- define how to represent uncertainty in reports

### 6. `research/copilot-cli-validation-plan.md`
Validation and test plan.

Requirements:
- include synthetic skills/resources
- include positive and negative controls
- include a repeatable experiment matrix

### 7. `research/copilot-cli-authoring-prompt.md`
A compact follow-up prompt I can later give Copilot to actually implement the evaluator.

Requirements:
- short
- implementation-oriented
- assumes the research files are the source of truth

### 8. `research/sources.md`
Source inventory.

For each source include:
- title
- URL
- source type
- date accessed
- trust level
- why it matters
- limitations / ambiguity notes

---

## Method requirements

Follow this process:

1. Start with official GitHub Copilot CLI documentation.
2. Determine what observability and interception features are officially supported.
3. Determine what those features are insufficient for.
4. Explore practical unsupported alternatives in order of increasing invasiveness.
5. Design a reusable evaluator architecture based on the best evidence.
6. Be explicit about what can be directly known versus only inferred.

---

## Output style requirements

- Be practical, not theoretical.
- Prefer direct recommendations over vague discussion.
- Use tables where useful.
- Avoid filler.
- Clearly label uncertainty.
- When something is undocumented, say so.
- Do not claim that resource injection can be directly observed unless you have solid evidence.

---

## Final section

At the end of the main report, include a section titled exactly:

## Recommended implementation sequence

That section should provide a phased plan:

1. quickest experiment to run first
2. minimum viable evaluator
3. stronger evaluator with better evidence
4. optional advanced instrumentation if needed

---

## Final constraint

This must produce a **research-backed implementation plan** for determining which Copilot CLI skill files and related resources are selected or injected for a given prompt, with a focus on **observability, interception, and early-stop behavior**.
