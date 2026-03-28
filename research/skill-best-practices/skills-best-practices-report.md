# Research Report: Latest Best Practices for GitHub Copilot AI Skills

**Research Date:** 2026-03-28
**Research Mode:** Deep
**Total Sources Consulted:** 22
**Scope:** GitHub Copilot skill authoring, customization surfaces, quality assurance, and repository architecture

---

## Executive Summary

GitHub Copilot Agent Skills, standardized through the agentskills.io open specification, represent the most powerful customization surface available for extending AI coding assistants with domain-specific workflows. Skills are modular instruction packages consisting of a `SKILL.md` file (YAML frontmatter + Markdown body) plus optional supporting assets, loaded into model context through a three-tier progressive disclosure mechanism that keeps token usage efficient.

This research identifies seven key findings:

1. **Skills follow a cross-platform standard** — the agentskills.io specification enables portability across Copilot, Claude, Codex, and Gemini with zero modification.
2. **Progressive disclosure is the core efficiency mechanism** — only metadata is loaded initially (~50-100 tokens per skill); full instructions load only when relevant.
3. **Five distinct customization surfaces exist** — skills, custom instructions, path-specific instructions, prompt files, and custom agents each serve different use cases with different tradeoffs.
4. **Description quality is the single most impactful authoring decision** — a behavior-driven, trigger-rich description determines whether the skill activates correctly.
5. **Context bloat is the primary failure mode** — embedding verbose documentation directly in `SKILL.md` degrades model performance across all skills.
6. **No formal testing framework exists yet** — quality assurance depends on manual review, golden examples, and prompt-based regression testing.
7. **Modular, single-purpose skills outperform monolithic ones** — composability and maintainability improve when each skill addresses one workflow.

**Primary Recommendation:** Adopt a modular skill architecture with trigger-focused descriptions, lean `SKILL.md` files that reference supporting assets, and a systematic review process using checklists and golden examples.

**Confidence Level:** High — based on official GitHub documentation, the agentskills.io specification, VS Code documentation, and corroborating community guidance from 22 sources.

---

## Introduction

### Research Question

What are the latest best practices for authoring GitHub Copilot skills, and how should a production-quality skills repository be structured to maximize reliability, maintainability, reusability, and testability?

This question matters because skills are the most powerful — but also the most complex — customization surface in the Copilot ecosystem. Poor skill design wastes model context, produces unreliable behavior, and creates maintenance burden. Well-designed skills dramatically improve the consistency and quality of AI-assisted coding workflows.

### Scope & Methodology

This research covers seven aspects of skill authoring: (1) the core skill model and how skills work, (2) comparison of all Copilot customization surfaces, (3) best practices for writing and organizing skills, (4) repository architecture for skills, (5) quality assurance and testing approaches, (6) research-to-implementation workflows, and (7) anti-patterns and failure modes.

Research was conducted using parallel web searches across official GitHub documentation, the agentskills.io specification, VS Code documentation, Microsoft Learn resources, and high-quality community guides. A total of 22 sources were consulted, with official documentation receiving highest priority. Community sources were used only when they provided concrete implementation value beyond what official docs cover.

The following were excluded from scope: general LLM prompt engineering not specific to skills, internal/proprietary GitHub tooling not publicly documented, and Copilot Studio enterprise skills (a separate product).

### Key Assumptions

- **Platform stability:** The agentskills.io specification and SKILL.md format are stable and will remain the primary skill definition mechanism.
- **Progressive disclosure behavior:** The three-tier loading model described in documentation accurately reflects runtime behavior across Copilot CLI, VS Code, and the coding agent.
- **Cross-platform portability:** Skills written for one platform (e.g., Copilot) function correctly on others (e.g., Claude, Codex) when placed in the appropriate directory.

---

## Main Analysis

### Finding 1: The Copilot Skill Model — Architecture and Mechanics

A GitHub Copilot Agent Skill is a modular, portable instruction package that teaches an AI coding agent how to handle a specific domain workflow or task. According to official GitHub documentation, each skill is a directory containing a `SKILL.md` file and optional supporting assets (scripts, templates, reference docs) [1][2].

The `SKILL.md` file has two parts. The YAML frontmatter declares metadata — currently three fields are supported:

| Field | Required | Constraints | Purpose |
|-------|----------|-------------|---------|
| `name` | Yes | 1-64 chars, lowercase, hyphens only | Unique identifier; must match folder name |
| `description` | Yes | 10-1024 chars | Tells the agent when to activate the skill |
| `license` | No | Free-form string | Licensing information for shared skills |

The Markdown body contains the actual instructions the agent follows when the skill is activated — procedures, examples, guidelines, and references to supporting files [1][3].

Skills can be placed at two levels. Project-level skills go in `.github/skills/<name>/SKILL.md` within a repository. User-level skills go in `~/.copilot/skills/<name>/SKILL.md` for personal use across projects [1][2]. The agentskills.io specification also recognizes `.claude/skills/` and `.agents/skills/` as equivalent directories for cross-platform compatibility [4][5].

**How Copilot decides when to use a skill** is governed by progressive disclosure — a three-tier context injection model [6][7]:

- **Tier 1 (Discovery):** At session start, only the YAML frontmatter (name + description) of every available skill is loaded into the agent's context. This costs approximately 50-100 tokens per skill and enables the agent to know what skills are available.
- **Tier 2 (Activation):** When a user's prompt matches a skill's description keywords or triggers, the full Markdown body of that skill is loaded into context (approximately 500-2000 tokens).
- **Tier 3 (Resource Access):** If the loaded instructions reference scripts, templates, or other files, those are loaded on-demand only when the agent needs them.

This staged approach means you can have many skills registered without degrading performance — only the relevant skill's content enters the context window at any given time.

**Current compatibility considerations:** Skills work across VS Code (via the agent skills extension), the Copilot CLI, and the GitHub coding agent. In VS Code, users can view and manage skills with the `/skills` command or through Chat Customizations settings. The `chat.agentSkillsLocations` setting controls where VS Code discovers skills [2].

**Sources:** [1], [2], [3], [4], [5], [6], [7]

---

### Finding 2: The Five Customization Surfaces — When to Use Each

GitHub Copilot offers five distinct customization mechanisms, each with different activation behavior, scope, and ideal use cases. Understanding when to use each is critical for avoiding the common mistake of putting the wrong type of guidance into the wrong surface [8][9][10].

| Surface | File Location | Activation | Scope | Best For |
|---------|--------------|------------|-------|----------|
| **Custom Instructions** | `.github/copilot-instructions.md` | Always-on | Repo-wide | Coding standards, naming conventions, project context |
| **Path-specific Instructions** | `.github/instructions/*.instructions.md` | Always-on for matching files | File/folder-scoped (via `applyTo` globs) | Language-specific rules, directory conventions |
| **Prompt Files** | `.github/prompts/*.prompt.md` | On-demand via `/` commands | Single task | Repeatable one-off tasks (test generation, reviews) |
| **Custom Agents** | `.github/agents/AGENT-NAME.md` | On-demand via `@agent` | Role/persona-scoped | Specialized roles with tool restrictions |
| **Agent Skills** | `.github/skills/<name>/SKILL.md` | Auto-activated when relevant | Task/workflow-scoped | Complex multi-step workflows with assets |

**Custom Instructions** (`.github/copilot-instructions.md`) are always injected into every Copilot interaction. They are ideal for universal project rules that should apply to every suggestion — coding style, preferred libraries, architectural principles. Their weakness is that they consume context tokens on every interaction regardless of relevance. Keep them concise (under 500 words is a good target) [8][11].

**Path-specific Instructions** (`.github/instructions/*.instructions.md`) use an `applyTo` field with glob patterns to target specific files or directories. For example, `applyTo: "**/*.py"` applies Python-specific rules only when editing Python files. This reduces context waste compared to repo-wide instructions. Multiple instruction files can overlap if their glob patterns match the same file [11][12].

**Prompt Files** (`.github/prompts/*.prompt.md`) are reusable task templates invoked manually via slash commands. They support input variables, model selection, and agent routing through YAML frontmatter fields (`description`, `name`, `argument-hint`, `agent`, `model`, `tools`). They are ideal for standardizing repeatable tasks like test generation, code review, or changelog creation. Unlike instructions, they are never automatically applied [13][14].

**Custom Agents** (`.github/agents/AGENT-NAME.md`) define specialized AI personas with specific roles, tool restrictions, and behavioral instructions. They are triggered with `@agent-name` syntax. Agents can be chained for multi-phase workflows. They are best for scenarios requiring strict persona boundaries — a security reviewer agent that only has read-only access, or a documentation agent that focuses exclusively on docs [9][15].

**Agent Skills** are the most powerful surface, designed for complex, multi-step workflows that include supporting scripts, templates, and reference materials. Unlike instructions (always-on) and prompts (manual), skills are auto-activated when the agent determines they are relevant based on the description. This makes them context-efficient for specialized workflows [1][8].

**Decision guide — when a team should choose a skill instead of another surface:**
- Use **instructions** for universal "house rules" (always applies, minimal context cost)
- Use **path-specific instructions** for language- or directory-scoped rules
- Use **prompt files** for one-off repeatable tasks triggered manually
- Use **custom agents** for role-based personas with tool boundaries
- Use **skills** for domain-specific workflows that need supporting assets and should auto-activate

**Sources:** [1], [2], [8], [9], [10], [11], [12], [13], [14], [15]

---

### Finding 3: Best Practices for Skill Authoring

Synthesizing guidance from official documentation, the agentskills.io specification, and experienced practitioners, the following best practices emerge for writing effective skills [1][3][6][7][16].

**Writing descriptions that trigger reliably.** The description is the single most important field in a skill because it determines whether the agent activates the skill for a given task. A good description is behavior-driven, includes trigger keywords, and explicitly states when the skill should be used. According to community guidance and the agentskills.io spec, descriptions should encode usage triggers — specific keywords, file types, or task patterns that help the agent match the skill to user prompts [4][6].

Bad: `"Helps with testing."` — too vague, could match anything.
Good: `"Assists with web application E2E test strategies using Cypress or Playwright. Use when setting up integration tests, writing E2E test suites, or configuring test runners for React/Vue applications."` — specific frameworks, tasks, and contexts.

**Keeping SKILL.md lean.** The Markdown body should contain only the procedural instructions the agent needs to follow. Verbose background information, full API references, and extensive documentation belong in supporting files within the skill directory (e.g., `reference/`, `examples/`). The body should typically be 500-2000 tokens. Beyond that, the skill starts consuming disproportionate context relative to its value [7][16].

**What belongs in SKILL.md vs supporting files:**

| In SKILL.md | In Supporting Files |
|-------------|-------------------|
| Step-by-step procedure | Full API reference docs |
| When-to-use guidance | Lengthy code examples |
| Key decision rules | Template files |
| Short inline examples | Scripts to execute |
| References to supporting files | Configuration samples |

**Designing small, composable skills.** Rather than creating one giant "project management" skill, create focused skills like `create-pr-description`, `generate-changelog`, `review-security`. Each skill should do one thing well. This improves activation accuracy (the agent can match more precisely), reduces unnecessary context loading, and makes maintenance easier [3][16].

**Reducing ambiguity and hallucinations.** Use imperative, unambiguous instructions. Prefer "Create a file named X with content Y" over "You might want to consider creating a file." Specify exact outputs, file names, and formats. When the skill references external tools or APIs, specify the exact command or endpoint rather than leaving it to the agent to guess [6][7].

**Making skills robust when partially followed.** Structure instructions as numbered steps with clear success criteria for each step. If the agent skips a step, the remaining steps should still make sense. Include validation checks within the procedure (e.g., "After creating the file, verify it passes linting") [3].

**Improving repeatability and consistency.** Include at least one concrete example of expected input and output in the SKILL.md body. This anchors the agent's behavior and reduces variance across invocations. Golden examples are the most effective tool for consistent output [7][16].

**Sources:** [1], [3], [4], [6], [7], [16]

---

### Finding 4: Repository Architecture for a Production Skills Repository

A well-organized skills repository improves discoverability, maintainability, and team adoption. Based on the agentskills.io specification, official GitHub guidance, and patterns observed in high-quality public repositories, the following architecture is recommended [1][3][4][17].

```
.github/
  skills/
    skill-name/
      SKILL.md              # Primary skill definition
      reference/            # Background docs, API specs, guides
      examples/             # Golden input/output examples
      templates/            # File templates the skill generates
      scripts/              # Executable scripts the skill references
  instructions/
    python.instructions.md  # Path-specific instructions
    frontend.instructions.md
  prompts/
    review-code.prompt.md   # Reusable prompt templates
    generate-tests.prompt.md
  agents/
    security-reviewer.md    # Custom agent definitions
  copilot-instructions.md   # Repo-wide always-on instructions
```

**`SKILL.md`** — The single entry point for the skill. Contains YAML frontmatter (name, description, license) and Markdown instructions. Must match the folder name. Should be lean (under 2000 tokens in the body). References other files but does not duplicate their content [1][3].

**`reference/`** — Background information the agent may need during execution: API documentation, architectural decision records, specification excerpts, coding standards relevant to the skill's domain. These files are loaded on-demand (Tier 3) and should not be repeated in SKILL.md. What does NOT belong here: general project documentation unrelated to the skill's specific workflow [7].

**`examples/`** — Golden input/output pairs demonstrating the skill's expected behavior. These serve dual purposes: they improve the agent's output quality by providing concrete anchors, and they serve as regression test cases during manual review. Each example should be a complete, realistic scenario — not a trivial placeholder [16].

**`templates/`** — File templates that the skill generates or populates. For example, a test-generation skill might include a `test-template.ts` file showing the expected structure. Templates should be minimal and parameterized where possible [3].

**`scripts/`** — Executable scripts the skill references in its instructions. These should be self-contained, well-documented, and tested independently. The skill's SKILL.md should include exact commands for running each script [1][3].

**`tests/`** — If the skill includes scripts, this directory holds unit tests for those scripts. For the skill instructions themselves, "tests" take the form of golden examples and review checklists rather than automated test suites (see Finding 5).

**`evaluation/`** — Evaluation scenarios and rubrics for assessing skill quality. This is a recommended (not required) directory for teams that take skill quality seriously. It might contain prompt-response pairs, scoring rubrics, and regression test prompts.

**Changelog and versioning:** For shared skills, maintain a `CHANGELOG.md` at the skill level documenting changes to instructions, new examples, and breaking changes. Semantic versioning of skills is not yet standardized but is a reasonable practice for teams sharing skills across repositories.

**Sources:** [1], [3], [4], [7], [16], [17]

---

### Finding 5: Quality Assurance, Testing, and Validation

As of March 2026, GitHub does not provide a formal testing framework specifically for validating skill quality. This is a significant gap in the ecosystem. Quality assurance for skills currently depends on manual processes and emerging community practices [18][19].

**Manual review patterns.** The most common approach is peer review of SKILL.md content. Reviewers check that the description is trigger-accurate, instructions are unambiguous, examples are realistic, and supporting files are referenced correctly. This is analogous to code review but for AI instructions [18].

**Golden examples.** The most effective validation technique is maintaining input/output pairs that demonstrate expected behavior. To test a skill, you invoke it with a known input and compare the output against the golden example. This is not automated — it requires manual execution and comparison — but it catches regressions when skill instructions change [16][19].

**Prompt-based regression testing.** Teams can maintain a set of test prompts that exercise the skill under various conditions (simple case, edge case, ambiguous input). After modifying a skill, re-running these prompts and comparing outputs provides confidence that changes did not break existing behavior. Some teams document these as scenarios in an `evaluation/` directory [19].

**Review checklists.** A structured checklist for reviewing skills catches common issues systematically. Key checks include: description accuracy, instruction clarity, example completeness, context efficiency, and metadata validity. This research includes a dedicated checklist (see `copilot-skill-checklist.md`).

**Metadata validation.** The `name` and `description` fields have documented constraints (name: 1-64 chars, lowercase, hyphens; description: 10-1024 chars). A simple linting script can validate these constraints, verify that the folder name matches the `name` field, and check for required frontmatter fields [4][5].

**Script verification.** If skills include executable scripts, those scripts should have their own unit tests and be validated independently of the skill instructions. The skill's instructions should include exact commands for running scripts, which can be verified in CI.

**Freshness checks.** Skills that reference external APIs, tools, or documentation should be reviewed periodically (quarterly is reasonable) to ensure referenced resources still exist and the guidance remains accurate. Links can be checked automatically; content accuracy requires manual review.

**What is currently missing:** Automated skill execution sandboxes, formal pass/fail test harnesses for skill instructions, standardized quality scores, and CI-integrated skill validation pipelines. The community is developing practices in this area, but tooling is immature [18][19].

**Sources:** [1], [4], [5], [16], [18], [19]

---

### Finding 6: Research-to-Implementation Workflow

Converting research findings into usable skill authoring guidance requires a structured workflow that maintains provenance, enables updates, and integrates with the skill authoring process [1][3][8].

**Maintaining a best-practices reference pack.** A dedicated `research/` directory within a skills repository stores research artifacts: best practices reports, authoring rules, checklists, rubrics, and source inventories. These files are not skills themselves — they are reference material that skill authors consult when creating or reviewing skills.

**Updating over time.** Research artifacts should include a "last reviewed" date and a "what changed recently" section. When official documentation changes or new best practices emerge, authors update the relevant research files and propagate changes to affected skills. A quarterly review cadence is practical.

**Recording source provenance.** Every recommendation in the reference pack should cite its source (official docs, specification, community guide) with a trust level indicator. This enables future reviewers to verify whether guidance is still current and distinguish official requirements from community conventions.

**Distilling research into authoring rules.** The most actionable output of research is a compact rules file (`copilot-skill-authoring-rules.md`) that can be directly included in prompts when asking Copilot to generate or review skills. This file should be under 800 words, use imperative voice, and focus on durable guidance rather than platform-specific quirks.

**Turning research into checklists and rubrics.** Checklists provide systematic review coverage. Rubrics provide qualitative scoring. Both should be organized by phase (design → authoring → validation → maintenance) to match the natural skill lifecycle.

**Feeding research back into Copilot.** The most powerful integration point is referencing the authoring rules file from a prompt file or skill. For example, a `create-skill.prompt.md` can reference `research/copilot-skill-authoring-rules.md` to ensure generated skills follow best practices. Similarly, a `review-skill.prompt.md` can reference the checklist and rubric.

**Sources:** [1], [3], [8]

---

### Finding 7: Anti-Patterns and Failure Modes

The following anti-patterns are drawn from official guidance, community reports, and inference from the progressive disclosure model [7][16][17][20].

**Anti-pattern 1: Putting too much content into SKILL.md**

Why harmful: When a skill activates (Tier 2), the entire Markdown body loads into context. A 5000-word SKILL.md consumes context that could be used for the user's actual code. If multiple skills activate, context bloat compounds rapidly.

Better alternative: Keep SKILL.md under 2000 tokens. Move verbose content to `reference/` files that load on-demand (Tier 3).

How to detect: Review SKILL.md word count. If the body exceeds 1500 words, it likely needs refactoring.

**Anti-pattern 2: Making the description too vague or too broad**

Why harmful: The description is the primary mechanism for skill activation. A vague description like "Helps with coding tasks" will either never activate (because it matches nothing specifically) or always activate (wasting context on irrelevant interactions).

Better alternative: Write behavior-driven descriptions with specific trigger keywords: "Generates Playwright E2E tests for React components. Use when asked to create integration tests, browser tests, or E2E test suites."

How to detect: If the description does not mention at least one specific technology, file type, or task action, it is probably too vague.

**Anti-pattern 3: Encoding unstable facts directly into the skill**

Why harmful: API endpoints, version numbers, configuration defaults, and tool-specific syntax change frequently. Embedding these directly in SKILL.md creates a maintenance burden and risks incorrect guidance.

Better alternative: Place volatile information in `reference/` files with explicit "last verified" dates. The SKILL.md instructions should say "Refer to `reference/api-endpoints.md` for current endpoints" rather than hardcoding them.

How to detect: Search SKILL.md for version numbers, URLs, specific API paths, or date-dependent claims.

**Anti-pattern 4: Mixing repo policy with narrow task instructions**

Why harmful: Repo-wide policies (e.g., "always use TypeScript strict mode") belong in `copilot-instructions.md`, not in individual skills. Putting them in skills means they only apply when that skill activates, creating inconsistency.

Better alternative: Use custom instructions for repo-wide policies. Use skills for task-specific procedures.

How to detect: If a skill contains rules that should apply to all code regardless of the task, those rules belong in instructions instead.

**Anti-pattern 5: Using unsupported metadata**

Why harmful: Only `name`, `description`, and `license` are officially documented YAML frontmatter fields. Adding fields like `triggers`, `version`, `author`, or `tags` may be silently ignored or cause parsing issues. Relying on undocumented behavior creates fragility.

Better alternative: Stick to the three documented fields. Encode trigger information in the description text. Place metadata like author and version in comments or a separate `CHANGELOG.md`.

How to detect: Check frontmatter for any fields other than `name`, `description`, and `license`.

**Anti-pattern 6: Having no examples**

Why harmful: Without concrete examples, the agent has no anchor for expected output quality and format. Outputs become inconsistent across invocations, and subtle misinterpretations of instructions go undetected.

Better alternative: Include at least one inline example in SKILL.md and maintain golden examples in `examples/`.

How to detect: If SKILL.md contains no code blocks, no sample inputs, and no expected outputs, it lacks examples.

**Anti-pattern 7: Creating one giant skill instead of focused skills**

Why harmful: A "full stack development" skill tries to cover frontend, backend, testing, deployment, and documentation. Its description is necessarily vague, its instructions are long, and it activates for too many prompts. Context efficiency drops, and the agent frequently applies irrelevant parts of the instructions.

Better alternative: Decompose into focused skills: `frontend-component-creation`, `api-endpoint-scaffolding`, `e2e-test-generation`, each with precise descriptions and lean instructions.

How to detect: If a skill name contains "full", "complete", "all", or generic terms, it may be too broad. If SKILL.md has more than 5 major workflow sections, consider splitting.

**Anti-pattern 8: Including outdated implementation assumptions**

Why harmful: Skills that assume specific tool versions, deprecated APIs, or outdated patterns produce incorrect guidance. The agent follows instructions literally, so outdated commands will generate code that fails.

Better alternative: Date-stamp implementation-specific guidance. Use `reference/` files for version-specific details. Include freshness checks in the maintenance cycle.

How to detect: During quarterly review, verify that all commands, APIs, and tool references in the skill still work.

**Sources:** [4], [7], [16], [17], [20]

---

## Synthesis & Insights

### Patterns Identified

**Pattern 1: Context efficiency as first-class concern.** Every aspect of skill design — from the three-tier progressive disclosure model to the recommendation to keep SKILL.md lean — reflects a core constraint: model context windows are finite and expensive. Unlike traditional documentation where more detail is generally better, skill authoring requires deliberate economy. The best skills maximize instructional value per token.

**Pattern 2: Description-driven activation as implicit contract.** The description field functions as a contract between the skill author and the runtime. It is simultaneously a human-readable summary and a machine-readable trigger. This dual purpose means descriptions must be written with both audiences in mind — clear enough for humans to understand and keyword-rich enough for the agent to match reliably.

**Pattern 3: Cross-platform standardization accelerating adoption.** The agentskills.io specification's adoption by Copilot, Claude, Codex, and Gemini means skills written once work across all major AI coding tools. This dramatically increases the ROI of investing in high-quality skill authoring. It also means best practices are converging across ecosystems rather than fragmenting.

### Novel Insights

**Insight 1: The customization surface hierarchy.** The five surfaces form a natural hierarchy of specificity and activation cost: instructions (always-on, low specificity) → path-specific instructions (always-on for matching files) → skills (auto-activated, high specificity) → prompt files (manual, task-scoped) → custom agents (manual, persona-scoped). Teams should place guidance at the lowest-cost surface that achieves the desired effect.

**Insight 2: Skills as executable documentation.** Well-written skills function as living documentation of team workflows. Unlike static wiki pages, skills are directly executed by AI agents, which means they must be precise, current, and unambiguous. This creates a natural incentive to keep workflow documentation up to date — because stale skills produce incorrect code.

### Implications

**For teams adopting skills:** Start with 3-5 focused skills covering your most repeated workflows. Invest heavily in description quality and golden examples. Establish a quarterly review cadence.

**For the ecosystem:** As formal testing tooling matures, expect skill quality to improve rapidly. The current gap between skill authoring ease and skill validation rigor represents the biggest bottleneck in the ecosystem.

---

## Limitations & Caveats

### Known Gaps

**Gap 1: Internal context injection mechanics.** Exactly how Copilot's runtime decides when to activate a skill (beyond matching the description) is not fully documented. The progressive disclosure model is described at a conceptual level, but implementation details — such as scoring algorithms, activation thresholds, and behavior when multiple skills match — are not publicly available.

**Gap 2: Formal testing infrastructure.** No automated framework exists for testing skill quality. This research recommends manual processes, but acknowledges this limits scalability for teams maintaining large skill repositories.

**Gap 3: Cross-platform behavioral differences.** While the agentskills.io specification ensures structural compatibility, runtime behavior may differ across platforms. A skill that activates reliably in VS Code may behave differently in the Copilot CLI or GitHub coding agent. Systematic cross-platform testing data is not available.

### Areas of Uncertainty

**Uncertainty 1: Unsupported metadata behavior.** Whether custom YAML fields are silently ignored, cause warnings, or produce errors may vary across platforms and versions. Official documentation says only `name`, `description`, and `license` are supported, but some community sources mention additional fields. This research recommends sticking to documented fields.

**Uncertainty 2: Token budgets for skills.** The approximate token costs cited (50-100 for Tier 1, 500-2000 for Tier 2) come from community sources, not official documentation. Actual token consumption may vary by model and platform.

---

## Recommendations

### Immediate Actions

1. **Audit existing skills against the checklist.** Use `copilot-skill-checklist.md` to review all current skills for anti-patterns, description quality, and context efficiency.

2. **Establish a `research/` directory.** Maintain best practices, authoring rules, and source provenance in a dedicated directory within your skills repository.

3. **Create golden examples for every skill.** Each skill should have at least one complete input/output example in `examples/` that demonstrates correct behavior.

4. **Write a metadata linting script.** Validate `name` format (1-64 chars, lowercase, hyphens), `description` length (10-1024 chars), and folder name matching.

### Next Steps

1. **Build prompt files that reference authoring rules.** Create `create-skill.prompt.md` and `review-skill.prompt.md` that include `copilot-skill-authoring-rules.md` as context.

2. **Establish a quarterly skill review cadence.** Check freshness of referenced URLs, verify commands still work, and update examples for current API versions.

3. **Monitor agentskills.io specification changes.** As the standard evolves, new metadata fields or loading behaviors may be introduced.

### Further Research Needs

1. **Cross-platform activation testing.** Systematically compare how the same skill activates across Copilot CLI, VS Code, and the GitHub coding agent.

2. **Optimal description patterns.** Empirically test different description styles to identify which patterns produce the most reliable activation.

3. **Automated skill evaluation.** Develop tooling for automated prompt-response testing of skills, analogous to unit testing for code.

---

## How I should prompt Copilot in the future

### 1. Create a skill

```
Create a new Copilot agent skill following the rules in @research/copilot-skill-authoring-rules.md.

The skill should: [describe what the skill does]
It should activate when: [describe trigger conditions]

Place it in .github/skills/[skill-name]/ with SKILL.md and at least one golden example in examples/.
Review the result against @research/copilot-skill-checklist.md before finishing.
```

### 2. Review this skill

```
Review the skill at [path to SKILL.md] against the checklist in @research/copilot-skill-checklist.md and the rubric in @research/copilot-skill-evaluation-rubric.md.

Check for: description quality, context efficiency, instruction clarity, example coverage, anti-patterns, and metadata validity.
Report findings with severity (critical/warning/info) and specific fix recommendations.
```

### 3. Update this skill to the latest best practices

```
Update the skill at [path to SKILL.md] to follow the latest best practices in @research/copilot-skill-authoring-rules.md.

Specifically check for:
- Description uses trigger keywords and is behavior-driven
- SKILL.md body is under 1500 words; verbose content moved to reference/
- At least one golden example exists in examples/
- No unstable facts (version numbers, URLs) hardcoded in SKILL.md
- Metadata uses only supported fields (name, description, license)
```

---

## Bibliography

[1] GitHub. "Creating agent skills for GitHub Copilot." GitHub Docs. https://docs.github.com/en/copilot/how-tos/use-copilot-agents/coding-agent/create-skills (Retrieved: 2026-03-28)

[2] Microsoft. "Use Agent Skills in VS Code." Visual Studio Code Documentation. https://code.visualstudio.com/docs/copilot/customization/agent-skills (Retrieved: 2026-03-28)

[3] SmartScope. "GitHub Copilot Agent Skills Guide [Latest Feb 2026]." SmartScope Blog. https://smartscope.blog/en/generative-ai/github-copilot/github-copilot-skills-guide/ (Retrieved: 2026-03-28)

[4] Anthropic. "Agent Skills Specification." agentskills.io. https://agentskills.io/specification (Retrieved: 2026-03-28)

[5] DeepWiki. "Agent Skills Specification." DeepWiki Analysis of anthropics/skills. https://deepwiki.com/anthropics/skills/5.1-agent-skills-specification (Retrieved: 2026-03-28)

[6] DeepWiki. "Progressive Disclosure Pattern." DeepWiki Analysis of microsoft/agent-skills. https://deepwiki.com/microsoft/agent-skills/5.3-progressive-disclosure-pattern (Retrieved: 2026-03-28)

[7] CloudProInc. "Use GitHub Copilot Agent Skills Without Blowing Your Context Window." CloudProInc Blog. https://www.cloudproinc.com.au/index.php/2026/02/02/use-github-copilot-agent-skills-without-blowing-your-context-window/ (Retrieved: 2026-03-28)

[8] Blog.cloud-eng.nl. "GitHub Copilot Customization: Instructions, Prompts, Agents and Skills." Cloud Engineering Blog. https://blog.cloud-eng.nl/2025/12/22/copilot-customization/ (Retrieved: 2026-03-28)

[9] Dev.to/pwd9000. "GitHub Copilot Instructions vs Prompts vs Custom Agents vs Skills." DEV Community. https://dev.to/pwd9000/github-copilot-instructions-vs-prompts-vs-custom-agents-vs-skills-vs-x-vs-why-339l (Retrieved: 2026-03-28)

[10] GitHub. "Copilot customization cheat sheet." GitHub Docs. https://docs.github.com/en/copilot/reference/customization-cheat-sheet (Retrieved: 2026-03-28)

[11] GitHub. "Adding custom instructions for GitHub Copilot CLI." GitHub Docs. https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/add-custom-instructions (Retrieved: 2026-03-28)

[12] DeepWiki. "Custom Instructions and Memory System." DeepWiki Analysis of github/awesome-copilot. https://deepwiki.com/github/awesome-copilot/4-custom-instructions-and-memory-system (Retrieved: 2026-03-28)

[13] Microsoft. "Use prompt files in VS Code." Visual Studio Code Documentation. https://code.visualstudio.com/docs/copilot/customization/prompt-files (Retrieved: 2026-03-28)

[14] Microsoft. "Prompt Files and Instructions Files Explained." .NET Blog. https://devblogs.microsoft.com/dotnet/prompt-files-and-instructions-files-explained/ (Retrieved: 2026-03-28)

[15] Microsoft. "Custom agents in VS Code." Visual Studio Code Documentation. https://code.visualstudio.com/docs/copilot/customization/custom-agents (Retrieved: 2026-03-28)

[16] mkabumattar. "Setting Up GitHub Copilot Agent Skills in Your Repository." DevTips. https://mkabumattar.com/devtips/post/github-copilot-agent-skills-setup/ (Retrieved: 2026-03-28)

[17] GitHub/duskmoon314. "Agent Skills Anti-Patterns." GitHub. https://github.com/duskmoon314/keine/blob/main/docs/2026-03-11-agent-skills-anti-patterns.md (Retrieved: 2026-03-28)

[18] Microsoft. "Test with GitHub Copilot." Visual Studio Code Documentation. https://code.visualstudio.com/docs/copilot/guides/test-with-copilot (Retrieved: 2026-03-28)

[19] Dev.to/qa-leaders. "GitHub Copilot Agent Skills: Teaching AI Your Repository Patterns." DEV Community. https://dev.to/qa-leaders/github-copilot-agent-skills-teaching-ai-your-repository-patterns-1oa8 (Retrieved: 2026-03-28)

[20] aridanemartin. "Agent Skills Part 1: Getting Started with SKILL.md." aridanemartin.dev. https://aridanemartin.dev/blog/agent-skills-getting-started/ (Retrieved: 2026-03-28)

[21] Code Inside Blog. "How to create a Skill for GitHub Copilot." https://blog.codeinside.eu/2026/03/17/how-to-create-a-skill-for-github-copilot/ (Retrieved: 2026-03-28)

[22] Chris Ayers. "Agent Skills, Plugins and Marketplace: The Complete Guide." https://chris-ayers.com/posts/agent-skills-plugins-marketplace/ (Retrieved: 2026-03-28)

---

## Appendix: Methodology

### Research Process

**Phase 1 (SCOPE):** Decomposed the research question into seven components matching the prompt's research questions. Defined scope boundaries (GitHub Copilot skills ecosystem, excluding Copilot Studio enterprise features).

**Phase 3 (RETRIEVE):** Conducted 12 parallel web searches covering official documentation, the agentskills.io specification, customization surface comparisons, anti-patterns, testing approaches, progressive disclosure mechanics, and community best practices.

**Phase 4 (TRIANGULATE):** Cross-referenced findings across official GitHub docs [1][10][11], VS Code docs [2][13][15], the agentskills.io spec [4][5], and community guides [3][7][8][9]. Core claims verified across 3+ sources.

**Phase 5 (SYNTHESIZE):** Identified patterns (context efficiency, description-driven activation, cross-platform convergence) and generated insights (customization hierarchy, skills as executable documentation).

**Phase 8 (PACKAGE):** Produced seven output artifacts per the research prompt's requirements.

### Sources Consulted

**Total Sources:** 22

**Source Types:**
- Official GitHub documentation: 4
- Official VS Code documentation: 4
- Specification documents: 2
- Community blogs and guides: 10
- Code repositories: 2

**Temporal Coverage:** Sources range from December 2025 to March 2026, with official documentation reflecting the most current state.

### Claims-Evidence Table

| Claim ID | Major Claim | Evidence Type | Supporting Sources | Confidence |
|----------|-------------|---------------|-------------------|------------|
| C1 | Skills use YAML frontmatter with name + description | Official documentation | [1], [2], [4] | High |
| C2 | Progressive disclosure has three tiers | Official + community docs | [2], [6], [7] | High |
| C3 | Five customization surfaces exist | Official documentation | [8], [9], [10] | High |
| C4 | Description quality determines activation reliability | Community consensus + spec | [3], [4], [6] | High |
| C5 | No formal skill testing framework exists | Documentation absence + community reports | [18], [19] | High |
| C6 | Context bloat is the primary failure mode | Community reports | [7], [17], [20] | Medium-High |
| C7 | Cross-platform portability via agentskills.io | Specification + community reports | [4], [5], [22] | High |

---

**Research Mode:** Deep
**Total Sources:** 22
**Word Count:** ~5,500
**Generated:** 2026-03-28
**Validation Status:** Manual review completed
