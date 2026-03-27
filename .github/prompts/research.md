# Deep Research: AI Code Evaluation Methods for .NET App Generation

## Research Objective

I have built an open-source framework called **copilot-skill-eval** that evaluates how different GitHub Copilot custom skills and plugins affect the quality of AI-generated code. The framework uses a 3-step pipeline — **generate → verify → analyze** — to compare multiple skill configurations across identical app specifications.

I need comprehensive research on **AI code evaluation methodologies** to determine whether my approach is optimal, what I'm missing, and what concrete improvements I should make. The research should focus on evaluating AI-generated **C# .NET 10 applications**, specifically ASP.NET Core Web API and Razor Pages apps.

---

## Current Framework Architecture

### Pipeline Overview

```
1. GENERATE — Copilot CLI generates the same apps under different skill configs
2. VERIFY   — Each generated project is built and run (dotnet build + dotnet run + health check)
3. ANALYZE  — Copilot (LLM) performs comparative analysis across all configs using defined quality dimensions
```

### How It Works

- **Scenarios**: Detailed app specifications (prompt files) describing realistic .NET apps with 5–7 entities, 8–12 business rules, 25–30+ API endpoints, seed data, and .http test files. Three example scenarios exist: a fitness studio booking API, a community library API, and a veterinary clinic API.

- **Configurations**: Multiple skill sets to compare (e.g., baseline with no skills, a single custom skill, a plugin chain with 9 skills, community skills, official .NET skills from dotnet/skills). Currently testing 5 configurations.

- **Dimensions**: 30 quality criteria the LLM evaluates when comparing generated code. These cover C# language features, .NET patterns, EF Core usage, API design, error handling, and code organization.

- **Verification**: Currently limited to:
  - `dotnet build` — checks if the project compiles (success pattern: "Build succeeded")
  - `dotnet run` — starts the app with a 15-second timeout, checks if process stays alive
  - Optional HTTP health check (GET request, expect 200)

- **Analysis**: A Jinja2 template generates a prompt that is sent to Copilot CLI. Copilot reads all generated code across all configurations, then produces a comparative analysis report with:
  - Executive summary table (✅/❌/mixed per dimension per config)
  - Per-dimension sections with inline code examples from each config
  - Verdict/ranking per dimension citing best practices
  - Summary of overall skill impact

### Current Evaluation Dimensions (30 total)

These are the quality criteria currently defined for ASP.NET Core Web API evaluation:

**Core Architecture & Code Quality:**
1. API Style — Controllers vs Minimal APIs
2. Sealed Types — Use of `sealed` modifier on classes/records
3. Primary Constructors — C# 12 primary constructors vs traditional injection
4. DTO Design — Records vs classes, immutability, naming (*Request/*Response vs *Dto)
5. Service Abstraction — Interface + implementation vs concrete-only DI registration

**Database & Query Performance:**
6. CancellationToken Propagation — Forwarding through endpoint → service → EF Core
7. AsNoTracking Usage — Read-only queries using AsNoTracking()
8. Return Type Precision — IReadOnlyList<T> vs List<T> vs IEnumerable<T>
9. Data Seeder Design — HasData() vs static methods vs injectable services
10. EF Core Relationship Configuration — Fluent API vs convention-only

**API Contracts & Documentation:**
11. TypedResults Usage — TypedResults vs Results for Minimal API returns
12. Route Group Organization — MapGroup for endpoint grouping
13. OpenAPI Metadata — WithName(), WithSummary(), Produces<T>(), etc.
14. HTTP Test File Quality — Coverage and correctness of .http files

**Error Handling & Validation:**
15. Middleware Style — IExceptionHandler vs convention-based vs try/catch
16. Exception Handling Strategy — Custom exceptions vs built-in vs result patterns
17. Guard Clauses & Argument Validation — Modern throw helpers

**Code Organization & Standards:**
18. File Organization — Per-entity vs per-concern layout
19. Pagination — Immutable types with computed properties
20. Naming Conventions — .NET naming guidelines compliance
21. Access Modifier Explicitness — Explicit public/internal vs defaults
22. Enum Design — Enums vs magic strings, naming, [Flags]

**Modern C# Features:**
23. Collection Initialization — C# 12 [] syntax vs new List<T>()
24. Structured Logging — ILogger<T> with templates vs string interpolation
25. Nullable Reference Types — NRT enforcement and annotations
26. Global Using Directives — C# 10+ global usings
27. Package Discipline — Minimal NuGet dependencies

**Async & Resource Management:**
28. Async/Await Best Practices — Correct patterns, no async void, no sync-over-async
29. Dispose & Resource Management — IDisposable/IAsyncDisposable, using declarations

---

## Research Questions

### Area 1: AI Code Evaluation Methodology — State of the Art

- What are the established methodologies for evaluating AI-generated code quality? (e.g., HumanEval, MBPP, SWE-bench, CodeBenchmark, BigCodeBench, LiveCodeBench, and others)
- How do industry leaders (Google, Microsoft, Meta, Anthropic, OpenAI) evaluate code generation quality in their LLM products?
- What academic research exists on evaluating LLM code generation beyond simple pass/fail correctness?
- What are the key differences between evaluating code **correctness** vs code **quality** vs code **style** — and which matters most for production applications?
- What evaluation frameworks exist specifically for comparing different LLM configurations or prompting strategies?
- What are best practices for **reproducibility** in AI code evaluation? How should I handle the inherent non-determinism of LLM outputs?
- Are there established methodologies for evaluating code generation in the context of **enterprise/production applications** (as opposed to competitive programming or small function generation)?

### Area 2: Dimension Assessment — Are We Measuring the Right Things?

- Given the 30 dimensions listed above, are there significant **gaps** — important code quality signals we're missing?
- Are any of the current dimensions **redundant** or measuring the same underlying quality?
- Should there be dimensions that assess higher-level architectural concerns (e.g., SOLID principles, separation of concerns, testability, maintainability scores)?
- Should we measure **functional correctness** — does the generated code actually implement the business rules from the spec?
- Should we measure **security** — SQL injection, XSS, insecure defaults, secrets in code, OWASP Top 10 compliance?
- Should we measure **performance characteristics** — query complexity, N+1 query patterns, memory allocation patterns?
- Should we measure **test generation** — does the generated code include unit tests, integration tests, and are they meaningful?
- How should dimensions be **weighted or prioritized**? A compile error matters more than a naming convention violation — how do leading frameworks handle this?
- Are there established **taxonomies** for code quality dimensions that we should align with (e.g., ISO 25010, CISQ, maintainability indices)?

### Area 3: Verification Improvements — Beyond Build + Run

- Our current verification is minimal: compile → run → health check. What additional automated verification should we add?
- Should we run the generated `.http` test files and verify responses against expected behavior?
- Should we run **Roslyn analyzers** (e.g., Microsoft.CodeAnalysis.NetAnalyzers, StyleCop.Analyzers, SonarAnalyzer) and incorporate warnings/errors into scoring?
- Should we measure **code coverage** by generating and running tests, then checking coverage percentages?
- Should we use **mutation testing** to evaluate test quality?
- Should we run **security scanning** tools (e.g., `dotnet list package --vulnerable`, OWASP dependency check)?
- Should we compute **code metrics** like cyclomatic complexity, maintainability index, coupling, or lines of code?
- What .NET-specific verification tools exist that could provide deterministic quality signals? Consider:
  - `dotnet format` — code formatting compliance
  - `dotnet test` — if tests are generated, run them
  - NuGet audit (`dotnet list package --vulnerable`)
  - Binary size analysis
  - Startup time benchmarking
- Should we validate that the API actually conforms to the specification (e.g., all endpoints exist, correct HTTP methods, correct response shapes)?

### Area 4: Analysis Methodology — LLM-as-Judge vs Automated Scoring

- Our analysis step uses **Copilot (an LLM) as the judge** to compare code across configurations. What does the research say about LLM-as-judge reliability for code evaluation?
- What are the known **biases** of LLM-as-judge approaches? (e.g., position bias, verbosity bias, self-preference bias)
- Should we replace LLM-based analysis with **deterministic scoring** (automated tools + metrics), or use a **hybrid approach**?
- If using LLM-as-judge, what are best practices for prompt design to maximize evaluation accuracy? (e.g., rubrics, chain-of-thought, calibration, multi-judge panels)
- How should we **score** results? Our current approach is qualitative (✅/❌/mixed per dimension). Should we use:
  - Numerical scores (1–5 or 1–10 per dimension)?
  - Weighted composite scores?
  - Elo/ranking systems across configs?
  - Win-rate comparisons (pairwise tournament)?
- Should we use **multiple LLM judges** (e.g., Copilot + Claude + GPT) and aggregate their assessments?
- How do we validate that our evaluation is **reliable** — if we run the same eval twice, do we get consistent results?

### Area 5: .NET-Specific Code Quality Signals & Tooling

- What .NET-specific static analysis tools should we integrate into our verification pipeline?
- Are there **Roslyn analyzer rulesets** specifically designed for evaluating ASP.NET Core Web API best practices?
- Should we use **BenchmarkDotNet** or similar tools to measure runtime performance of generated code?
- What **.NET Aspire** or **OpenTelemetry** instrumentation could we use to assess observability quality?
- Are there tools that can automatically verify EF Core patterns (N+1 detection, migration correctness)?
- What .NET code quality tools exist beyond what we're already considering? (e.g., NDepend, JetBrains ReSharper/Rider inspections, Meziantou.Analyzer, Roslynator)
- Should we evaluate whether the generated code uses the **latest .NET 10 features** appropriately (e.g., new BCL APIs, new EF Core features, new ASP.NET Core features)?

---

## Expected Research Output

Please produce a structured report with these sections:

### 1. Executive Summary
A brief (1-page) overview of key findings and the top 5 most impactful recommendations.

### 2. State of the Art in AI Code Evaluation
Survey of current methodologies, frameworks, and benchmarks. Focus on what's applicable to evaluating AI-generated production applications (not competitive programming).

### 3. Dimension Gap Analysis
Specific dimensions to **add**, **remove**, or **modify** in our framework. For each recommendation, explain:
- What the dimension measures
- Why it matters for .NET app quality
- How it could be evaluated (LLM-judged vs automated)
- Priority level (must-have, should-have, nice-to-have)

### 4. Verification Pipeline Recommendations
Concrete tools and checks to add to the verification step, with:
- Tool name and what it checks
- How to integrate it (command, config, scoring)
- Expected impact on evaluation quality
- Priority and implementation complexity

### 5. Analysis Methodology Recommendations
How to improve the analysis step — scoring approaches, LLM-as-judge best practices, hybrid strategies. Include:
- Recommended scoring system with rationale
- Specific prompt engineering improvements
- Whether to add automated scoring components
- How to measure evaluation reliability

### 6. .NET-Specific Tooling Recommendations
.NET tools and analyzers to integrate, with concrete integration steps.

### 7. Prioritized Action Plan
A ranked list of all recommendations ordered by impact-to-effort ratio, grouped into:
- **Quick wins** — high impact, low effort
- **Strategic investments** — high impact, requires significant work
- **Nice-to-haves** — lower impact improvements

---

## Context Files

For deeper understanding of the framework, reference these files in the repository:

- `README.md` — Framework overview and usage
- `examples/aspnet-webapi/eval.yaml` — Full configuration with all 30 dimensions
- `examples/aspnet-webapi/prompts/scenarios/` — Example scenario prompt files
- `templates/analyze.md.j2` — The analysis prompt template
- `templates/create-all-apps.md.j2` — The generation prompt template
- `src/skill_eval/verify.py` — Verification implementation
- `src/skill_eval/analyze.py` — Analysis orchestration
- `src/skill_eval/config.py` — Configuration model
- `docs/authoring-guide.md` — Scenario prompt authoring guide
