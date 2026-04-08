# Competitive Analysis: copilot-skill-eval vs Waza

> **Date:** April 2026
> **Repos:** [sayedihashimi/copilot-skill-eval](https://github.com/sayedihashimi/copilot-skill-eval) · [tw93/Waza](https://github.com/tw93/Waza)

---

## Executive Summary

**copilot-skill-eval** and **Waza** both operate in the AI-assisted development tooling space, but they solve fundamentally different problems and target different audiences. They are complementary rather than directly competing.

| | copilot-skill-eval | Waza |
|---|---|---|
| **One-liner** | Evaluate how custom skills impact Copilot code generation quality | Engineering habits packaged as skills for Claude Code |
| **Category** | Evaluation / benchmarking framework | Skill collection / developer workflow |
| **Primary question** | "Does my skill actually make generated code better?" | "How do I make my AI-assisted workflow more disciplined?" |
| **AI platform** | GitHub Copilot (platform-agnostic model selection) | Claude Code (Anthropic), partial multi-agent support |

---

## 1. Purpose & Philosophy

### copilot-skill-eval

A **measurement framework** for skill authors. It answers the question: *"Does adding custom skills to Copilot actually improve the quality of generated code?"* The tool treats skill evaluation as a scientific process with controlled variables, multiple runs for statistical reliability, and weighted scoring across configurable quality dimensions.

**Core philosophy:** You can't improve what you can't measure. Skill quality should be evaluated empirically, not assumed.

### Waza

A **curated collection of 8 engineering discipline skills** for Claude Code. The name comes from the Japanese martial arts term for technique (技, わざ) — a move practiced until it becomes instinct. Waza codifies habits like "think before coding," "review before merging," and "diagnose before fixing" into executable AI playbooks.

**Core philosophy:** AI makes you faster, but it doesn't make you think more clearly. Good engineering habits should be encoded into the AI's workflow.

---

## 2. Target Audience

| Audience | copilot-skill-eval | Waza |
|---|---|---|
| Skill/plugin authors | ✅ Primary audience | ❌ |
| Teams evaluating skill ROI | ✅ Primary audience | ❌ |
| Individual developers | ⚠️ Can use, but overkill for personal use | ✅ Primary audience |
| CI/CD pipeline operators | ✅ Built-in GitHub Actions support | ❌ |
| Claude Code users | ❌ Copilot-only | ✅ Primary audience |
| GitHub Copilot users | ✅ Native support | ⚠️ Core skills work, platform features lost |

---

## 3. Feature Comparison

### 3.1 Core Capabilities

| Feature | copilot-skill-eval | Waza |
|---|---|---|
| **Automated code generation** | ✅ Generates full apps via Copilot CLI | ❌ Not a generation tool |
| **Build verification** | ✅ Compile, format-check, security scan, runtime health | ❌ |
| **Comparative analysis** | ✅ Weighted multi-dimensional scoring | ❌ |
| **Multi-run reliability** | ✅ Configurable N runs with aggregation | ❌ |
| **Skill isolation** | ✅ Staging directory isolation per config | ❌ |
| **Session tracing** | ✅ Verifies which skills were actually loaded | ❌ |
| **Token usage tracking** | ✅ Per-config/run generation usage | ❌ |
| **Pre-coding discipline** | ❌ | ✅ `/think` skill |
| **Code review workflow** | ❌ | ✅ `/check` skill with sub-agent reviewers |
| **Debugging methodology** | ❌ | ✅ `/hunt` skill |
| **UI/design guidance** | ❌ | ✅ `/design` skill |
| **Research workflow** | ❌ | ✅ `/learn` skill (6-phase pipeline) |
| **Prose writing/editing** | ❌ | ✅ `/write` skill (Chinese + English) |
| **URL/PDF reading** | ❌ | ✅ `/read` skill with proxy cascade |
| **Config health audit** | ❌ | ✅ `/health` skill (6-layer audit) |

### 3.2 Configuration & Extensibility

| Aspect | copilot-skill-eval | Waza |
|---|---|---|
| **Config format** | `eval.yaml` + `skill-sources.yaml` | `CLAUDE.md` + per-skill `SKILL.md` frontmatter |
| **Skill source management** | Git repos + local paths, cached in `~/.skill-eval/cache/` | `npx skills add` package manager |
| **Custom dimensions** | ✅ User-defined with tiers (critical/high/medium/low) | ❌ Fixed set of 8 skills |
| **Custom scenarios** | ✅ User-authored prompts with Jinja2 rendering | ❌ |
| **Tech stack agnostic** | ✅ Any stack with configurable verification commands | ⚠️ General but frontend-leaning (design references) |
| **Plugin support** | ✅ Supports Copilot plugins alongside skills | ❌ N/A (different ecosystem) |
| **Interactive setup** | ✅ `skill-eval init` wizard | ✅ `npx skills add` interactive agent picker |

### 3.3 CI/CD Integration

| Feature | copilot-skill-eval | Waza |
|---|---|---|
| **GitHub Actions workflow** | ✅ `skill-eval ci-setup` generates complete workflow | ❌ No CI integration |
| **Manual dispatch** | ✅ `workflow_dispatch` with skip/analyze-only | ❌ |
| **Scheduled runs** | ✅ Configurable cron schedule | ❌ |
| **Artifact upload** | ✅ Reports + output as workflow artifacts | ❌ |

### 3.4 Reporting & Output

| Output | copilot-skill-eval | Waza |
|---|---|---|
| **Analysis reports** | ✅ `reports/analysis.md` with weighted scores | ❌ |
| **Per-run reports** | ✅ `analysis-run-{N}.md` | ❌ |
| **Build notes** | ✅ `build-notes.md` with Roslyn warnings | ❌ |
| **Machine-readable data** | ✅ `scores-data.json`, `verification-data.json`, `generation-usage.json` | ❌ |
| **Health audit report** | ❌ | ✅ Structured health report with severity tiers |
| **Code review sign-off** | ❌ | ✅ Structured sign-off with file/scope/test counts |
| **Debug trace output** | ❌ | ✅ Root cause + fix + evidence format |

---

## 4. Architecture

### copilot-skill-eval

```
Python CLI (Click + Pydantic)
│
├── 3-stage pipeline: Generate → Verify → Analyze
├── Jinja2 template rendering for prompts
├── Source resolution (git clone/cache)
├── Staging directory isolation (symlinks/junctions)
├── Session tracing (skill/plugin activation verification)
├── Aggregator (cross-run score statistics)
└── GitHub Actions workflow generation
```

- **Language:** Python 3.10+
- **Dependencies:** PyYAML, Click, Jinja2, Pydantic
- **Install:** `pipx install` from git
- **Entry point:** `skill-eval` CLI

### Waza

```
Markdown skill definitions (SKILL.md frontmatter)
│
├── 8 skill folders, each with:
│   ├── SKILL.md (loaded on-demand by Claude)
│   ├── references/ (supporting docs)
│   ├── agents/ (sub-agent definitions)
│   └── scripts/ (shell helpers)
├── .claude-plugin/marketplace.json (registry)
├── .agents/skills/ (auto-discovery path)
└── scripts/ (statusline, install helpers)
```

- **Language:** Shell scripts + Markdown
- **Dependencies:** Node 18+ (for `npx skills add`)
- **Install:** `npx skills add tw93/Waza`
- **Entry point:** Slash commands (`/think`, `/check`, etc.)

---

## 5. Strengths & Weaknesses

### copilot-skill-eval

| Strengths | Weaknesses |
|---|---|
| Rigorous, data-driven skill evaluation | Requires Copilot CLI setup and build tooling |
| Multi-run statistical reliability | Complex configuration for simple use cases |
| Tech-stack agnostic with configurable verification | No built-in engineering workflow skills |
| CI/CD integration out of the box | Newer project with less community traction (1 star) |
| Machine-readable output for automation | Learning curve for first-time configuration |
| Skill isolation prevents contamination | Focused only on code generation quality measurement |

### Waza

| Strengths | Weaknesses |
|---|---|
| Opinionated, battle-tested engineering habits | No measurement or benchmarking capability |
| Low friction: install and use via slash commands | Claude Code-centric; reduced features on other platforms |
| Strong community adoption (1,600+ stars, 80 forks) | Fixed set of 8 skills; no custom skill authoring framework |
| Rich "gotchas" from real project failures | No CI/CD integration |
| Multi-agent architecture (/check uses sub-agent reviewers) | No programmatic/machine-readable output |
| Bilingual (Chinese + English) support | No way to quantify whether skills improve output quality |

---

## 6. Overlap & Differentiation

### Where they overlap

Both projects are concerned with **AI skill quality** but from opposite ends:

- **copilot-skill-eval** asks: "Given a skill, how good is the code it helps produce?"
- **Waza** asks: "Given an engineering habit, how do I make the AI follow it?"

Both use YAML/Markdown-based configuration and support git-based skill distribution.

### Where they diverge

| Dimension | copilot-skill-eval | Waza |
|---|---|---|
| **Evaluation vs. Execution** | Evaluates skills after the fact | Executes skills in real-time |
| **Quantitative vs. Qualitative** | Produces numerical scores and weighted rankings | Produces structured but qualitative output |
| **Platform** | GitHub Copilot ecosystem | Claude Code ecosystem |
| **Scope** | Measures code generation quality specifically | Covers the full engineering lifecycle (think → design → code → review → debug → learn → write) |
| **Automation** | Fully automated pipeline with CI | Human-in-the-loop workflow via slash commands |

---

## 7. Complementary Use Cases

These tools could be used together in the following workflows:

1. **Skill development cycle:** Author a Copilot custom skill → use copilot-skill-eval to measure its impact → iterate on skill content → re-evaluate.

2. **Cross-platform skill porting:** Port a Waza-style engineering habit into a Copilot custom skill → use copilot-skill-eval to verify the ported skill actually improves code quality.

3. **Benchmark inspiration:** Use Waza's detailed "gotchas" tables as source material for copilot-skill-eval analysis dimensions (e.g., a "debugging discipline" dimension inspired by `/hunt`).

4. **Quality validation for Waza skills:** Adapt copilot-skill-eval's evaluation methodology to measure whether Waza's `/think` or `/design` skills produce measurably better code than unguided Claude Code.

---

## 8. Community & Maturity

| Metric | copilot-skill-eval | Waza |
|---|---|---|
| **GitHub Stars** | 1 | 1,609 |
| **Forks** | 0 | 80 |
| **Created** | March 17, 2026 | March 12, 2026 |
| **Language** | Python | Shell/Markdown |
| **License** | MIT | MIT |
| **Open Issues** | 0 | 0 |
| **Examples** | 2 complete evaluation projects (ASP.NET WebAPI, Razor Pages) | 8 skill playbooks |
| **Documentation** | README + architecture doc + authoring guide + agent guide | README + CLAUDE.md + per-skill docs |
| **Maintainer** | sayedihashimi | tw93 (Tw93) |

Waza has significantly more community traction, likely because it provides immediate, tangible value to individual developers (install and use in seconds). copilot-skill-eval targets a narrower audience (skill authors and teams doing systematic evaluation) and requires more setup investment.

---

## 9. Key Takeaways

1. **They solve different problems.** copilot-skill-eval is a measuring stick; Waza is a toolkit. One tells you *how good* your skills are; the other *gives you* skills to use.

2. **Different AI ecosystems.** copilot-skill-eval is GitHub Copilot-native; Waza is Claude Code-native. This is the biggest practical barrier to direct comparison.

3. **copilot-skill-eval's unique value** is its rigorous, automated, multi-run evaluation pipeline with weighted scoring. No other tool in this space provides comparable quantitative skill assessment.

4. **Waza's unique value** is its opinionated, battle-tested engineering discipline encoded into actionable AI skills, with deep "gotchas" from real failures.

5. **Growth opportunity for copilot-skill-eval:** Consider adding support for evaluating skills on additional AI platforms (Claude Code, Cursor, etc.) to broaden the addressable market. Waza's popularity demonstrates strong demand for engineering-discipline skills.

6. **Growth opportunity for Waza:** Adding measurability to skills — even lightweight before/after comparisons — would help users justify skill adoption and identify which skills provide the most value.
