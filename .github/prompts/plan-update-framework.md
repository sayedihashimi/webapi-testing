# Plan: Update Framework Based on AI Code Evaluation Research

## Problem Statement

The deep research report (`~/Documents/AI_Code_Eval_Methods_Research_20260327/research_report_20260327_ai_code_eval_methods.md`) identified five major gaps in the copilot-skill-eval framework:

1. **Missing quality pillars** — Security and Functional Correctness dimensions are completely absent
2. **No automated scoring** — All 30+ dimensions rely entirely on LLM judgment; ~15 could be automated via Roslyn analyzers and .NET tooling
3. **Minimal verification** — Only build + run + optional health check; no format checking, security scanning, or functional testing
4. **LLM-as-judge bias** — No randomization, no rubrics, qualitative ✅/❌ instead of numerical scores
5. **Single-run fragility** — No multi-run generation or statistical aggregation

This plan implements the research recommendations as concrete framework changes, including updates to both the `aspnet-webapi` and `aspnet-razor-pages` examples.

---

## Phase 1: Config Model Extensions (config.py)

### 1.1 Add `tier` and `weight` fields to Dimension model

Add optional fields to `Dimension` for priority weighting. Must be backward-compatible (defaults ensure existing eval.yaml files continue to work).

```python
class Dimension(BaseModel):
    name: str
    description: str
    what_to_look_for: str
    why_it_matters: str
    tier: Literal["critical", "high", "medium", "low"] = "medium"
    weight: float = 1.0
    evaluation_method: Literal["llm", "automated", "hybrid"] = "llm"
```

**Tier-to-weight mapping (used when weight not explicitly set):**
- `critical` → 3.0 (build failure, security vulnerabilities, missing endpoints)
- `high` → 2.0 (functional correctness, error handling, async patterns)
- `medium` → 1.0 (DTO design, service abstraction, file organization)
- `low` → 0.5 (naming conventions, collection syntax, global usings)

**Files to change:** `src/skill_eval/config.py`

### 1.2 Add new verification step models

Extend the `Verification` model with optional steps for format checking, security scanning, and metrics:

```python
class FormatVerification(BaseModel):
    command: str = "dotnet format --check"
    working_directory: str = "."

class SecurityVerification(BaseModel):
    vulnerability_scan: bool = True          # dotnet list package --vulnerable
    static_analysis: bool = False            # SecurityCodeScan NuGet
    analyzers: list[str] = []                # Additional Roslyn analyzer NuGet packages

class MetricsVerification(BaseModel):
    compute: bool = True
    metrics: list[str] = ["warnings", "errors"]  # What to capture from build output

class Verification(BaseModel):
    build: BuildVerification
    run: RunVerification | None = None
    format: FormatVerification | None = None
    security: SecurityVerification | None = None
    metrics: MetricsVerification | None = None
    analyzers: list[str] = []                    # Roslyn analyzer NuGet packages to inject
```

**Files to change:** `src/skill_eval/config.py`

### 1.3 Add multi-run support to EvalConfig

```python
class EvalConfig(BaseModel):
    # ... existing fields ...
    runs: int = 1  # Number of generation runs per configuration
```

**Files to change:** `src/skill_eval/config.py`

---

## Phase 2: Verification Pipeline Expansion (verify.py)

### 2.1 Inject Roslyn analyzers via Directory.Build.props

Before running `dotnet build`, inject a standardized `Directory.Build.props` file into each generated project that enables Roslyn analyzers. This captures hundreds of code quality warnings automatically.

**Create template file:** `templates/Directory.Build.props`

```xml
<Project>
  <PropertyGroup>
    <AnalysisLevel>latest</AnalysisLevel>
    <AnalysisMode>Recommended</AnalysisMode>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <!-- Additional analyzer packages injected here by the framework -->
</Project>
```

**Implementation:**
- Before `_run_build()`, check if `Directory.Build.props` already exists
- If not, copy/inject the template version
- Parse build output for warning/error counts by category (CA17xx naming, CA18xx performance, CA20xx reliability, CA21xx security, etc.)
- Store counts in results dict

**Files to change:** `src/skill_eval/verify.py`, create `templates/Directory.Build.props`

### 2.2 Add `dotnet format --check` step

After build succeeds, run `dotnet format --check` to validate code formatting:

- Parse output for file count with issues
- Store as `format_issues: int` in results
- Add "Format" column to build-notes.md table

**New function:** `_run_format(project_dir, working_directory) -> (bool, int, str)`

**Files to change:** `src/skill_eval/verify.py`

### 2.3 Add `dotnet list package --vulnerable` step

After build succeeds, run security vulnerability scanning:

- Parse output for vulnerability count and severities (Critical, High, Moderate, Low)
- Store as `vulnerabilities: dict[str, int]` in results
- Add "Security" column to build-notes.md table

**New function:** `_run_security_scan(project_dir, working_directory) -> (int, str)`

**Files to change:** `src/skill_eval/verify.py`

### 2.4 Parse and count Roslyn analyzer warnings from build output

Extract structured warning counts from `dotnet build` output:

- Regex parse `warning CA\d{4}:` patterns
- Group by category prefix (CA17xx=naming, CA18xx=performance, etc.)
- Compute total warning count and per-category counts
- Store in results dict for use in analysis

**New function:** `_parse_build_warnings(build_output: str) -> dict[str, int]`

**Files to change:** `src/skill_eval/verify.py`

### 2.5 Add .http file execution for functional correctness

After the app is started (run step), locate and execute .http files against the running API to verify endpoint correctness:

**New function:** `_execute_http_file(http_file_path, base_url) -> HttpFileResults`

**Implementation details:**
1. Parse the .http file to extract individual requests (method, URL, headers, body)
2. Resolve `@baseUrl` variable and any other file-level variables
3. Execute requests sequentially in document order (respects entity creation dependencies)
4. Classify each request:
   - **GET requests** → strict scoring: expect 2xx, any 4xx/5xx is a failure
   - **POST/PUT/DELETE/PATCH** → lenient scoring: 2xx = success, 4xx on re-run = acceptable (entity may already exist/be deleted), 5xx = failure
5. Compute metrics:
   - `get_success_rate` — percentage of GET requests returning 2xx (primary correctness signal)
   - `mutation_success_rate` — percentage of mutating requests returning 2xx or expected-conflict 4xx
   - `total_endpoints_tested` — count of requests in the .http file
   - `server_errors` — count of 5xx responses (always a failure regardless of method)
6. Handle edge cases:
   - **Fresh database:** The most reliable approach. Scenarios specify "seed on startup when DB empty" — so if the app starts with a clean DB, all seed data is present and .http file IDs match. Skip execution if app didn't start successfully.
   - **Re-execution tolerance:** POST creating duplicates (409), DELETE on already-deleted (404), PUT on non-existent after prior delete (404) should not be counted as failures — they indicate the request *would have* worked on first run.
   - **Timeout per request:** 10-second timeout per HTTP request to avoid hanging on slow endpoints.
   - **Connection errors:** If the app isn't responding, abort .http execution and report "app not reachable."

**Results structure:**
```python
{
    "http_file_found": True,
    "total_requests": 25,
    "get_success_rate": 0.92,      # 12/13 GETs returned 2xx
    "mutation_success_rate": 0.83,  # 10/12 mutations returned 2xx or expected 4xx
    "server_errors": 1,            # 1 request returned 500
    "details": [...]               # Per-request log
}
```

**Files to change:** `src/skill_eval/verify.py` (add function + call from run_verify after app start)

### 2.6 Write extended build-notes.md

Update `_write_build_notes()` to include new columns and a new "Automated Metrics" section:

```markdown
## Results
| Configuration | Scenario | Build | Warnings | Format | Security | Run | GET Rate | Mutation Rate | Notes |

## Automated Metrics
| Configuration | Scenario | Total Warnings | Naming | Performance | Security | Reliability |

## HTTP File Execution
| Configuration | Scenario | Requests | GET Success | Mutation Success | 5xx Errors |

## Skill Configurations
| Configuration | Label | Skills | Plugins |
```

**Files to change:** `src/skill_eval/verify.py`

---

## Phase 3: Analysis Improvements (analyze.md.j2, analyze.py)

### 3.1 Randomize configuration order in analysis prompt

Add randomization to `analyze.py` before rendering the template to mitigate LLM position bias:

```python
import random
shuffled_configs = list(config.configurations)
random.shuffle(shuffled_configs)
```

Pass `shuffled_configs` to the template instead of `config.configurations`.

**Files to change:** `src/skill_eval/analyze.py`

### 3.2 Add numerical scoring (1–5) to analysis template

Update `analyze.md.j2` to request explicit 1–5 numerical scores per dimension per configuration, with a rubric:

```markdown
For each dimension, score each configuration on a 1–5 scale:
- **1** — Missing or fundamentally wrong
- **2** — Present but poorly implemented
- **3** — Adequate, follows basic conventions
- **4** — Good, follows most best practices
- **5** — Excellent, follows all best practices with modern patterns

## Executive Summary
Include a table with numerical scores:
| Dimension | Config A | Config B | Config C | ... |
```

Replace the current `✅/❌/mixed` qualitative system.

**Files to change:** `templates/analyze.md.j2`

### 3.3 Include automated metrics in analysis prompt

Feed the verification results (warning counts, format issues, vulnerability counts) into the analysis prompt so the LLM has objective data alongside its code review:

```jinja2
## Automated Verification Results
The following metrics were captured automatically during verification:
{% for config in configurations %}
### {{ config.name }}
{% for scenario in scenarios %}
- {{ scenario.name }}: {{ metrics[config.name][scenario.name] }}
{% endfor %}
{% endfor %}

Use these metrics as objective anchors when scoring dimensions.
```

This requires passing verification results from verify step to analyze step.

**Files to change:** `templates/analyze.md.j2`, `src/skill_eval/analyze.py`, `src/skill_eval/verify.py` (write results as JSON)

### 3.4 Add dimension tier/weight info to analysis template

Show the LLM which dimensions are Critical/High/Medium/Low so it can calibrate its analysis appropriately:

```jinja2
{% for dim in dimensions %}
   - **{{ dim.name }}** [{{ dim.tier | upper }}]: {{ dim.description }}
{% endfor %}
```

**Files to change:** `templates/analyze.md.j2`

---

## Phase 4: Multi-Run Support (generate.py, cli.py)

### 4.1 Add `--runs N` CLI flag

Add to both `generate` and `run` commands:

```python
@click.option("--runs", "-r", type=int, default=None,
              help="Number of generation runs per configuration (overrides eval.yaml)")
```

If not specified, falls back to `config.runs` (default 1).

**Files to change:** `src/skill_eval/cli.py`

### 4.2 Update generation to support multiple runs

Modify `run_generate()` to loop N times per configuration:

- Output structure changes from `output/{config}/{scenario}/` to `output/{config}/run_{n}/{scenario}/`
- When runs=1 (default), maintain current flat structure for backward compatibility
- Each run uses a fresh Copilot invocation with isolated context

**Files to change:** `src/skill_eval/generate.py`

### 4.3 Update verification to handle multi-run outputs

Modify `run_verify()` to iterate across runs:

- Detect whether output uses `run_N/` subdirectories
- Run verification for each run independently
- Aggregate results across runs (mean, min, max, stddev for numerical metrics)

**Files to change:** `src/skill_eval/verify.py`

### 4.4 Update analysis to aggregate multi-run results

Modify analysis prompt to present aggregated data when multiple runs exist:

- Show per-run scores and mean/variance
- Ask LLM to consider consistency across runs as an additional quality signal

**Files to change:** `templates/analyze.md.j2`, `src/skill_eval/analyze.py`

---

## Phase 5: New Dimensions — Security & Functional Correctness

### 5.1 Define new shared dimensions (both examples)

These dimensions apply to both Web API and Razor Pages apps:

**Security (tier: critical):**

1. **Security Vulnerability Scan**
   - description: Known NuGet vulnerabilities and insecure package versions
   - what_to_look_for: Run `dotnet list package --vulnerable`. Check for packages with known CVEs. Check for deprecated packages. Verify no development-only packages (e.g., Swashbuckle in .NET 9+) are included.
   - why_it_matters: Vulnerable dependencies are the most common attack vector in production .NET apps
   - tier: critical
   - evaluation_method: automated

2. **Input Validation Coverage**
   - description: Whether all user inputs are validated before processing
   - what_to_look_for: Check all POST/PUT endpoint parameters and request DTOs for validation attributes ([Required], [StringLength], [Range], [RegularExpression]). Look for FluentValidation usage. Check that validation errors return ProblemDetails (400). Verify no raw user input reaches database queries.
   - why_it_matters: Missing input validation enables injection attacks, data corruption, and unexpected behavior
   - tier: critical
   - evaluation_method: hybrid

**Functional Correctness (tier: critical):**

3. **Endpoint Completeness**
   - description: Whether all specified API endpoints or pages are implemented
   - what_to_look_for: Compare the list of endpoints/pages specified in the scenario prompt against the actual routes registered in Program.cs or mapped via controllers/page models. Count missing endpoints. Check HTTP methods match spec.
   - why_it_matters: Missing endpoints mean the generated app doesn't meet the specification — the most basic correctness requirement
   - tier: critical
   - evaluation_method: hybrid

4. **Business Rule Implementation**
   - description: Whether business rules from the specification are enforced in code
   - what_to_look_for: For each business rule in the scenario prompt, check if there is corresponding validation/logic in the service or endpoint layer. Look for boundary checks, state transition guards, capacity limits, and constraint enforcement. Note any rules that are completely missing from the implementation.
   - why_it_matters: Business rules define the app's purpose. Code that compiles but doesn't enforce rules is functionally incorrect.
   - tier: critical
   - evaluation_method: llm

**Reliability (tier: high):**

5. **Error Response Conformance**
   - description: Consistent ProblemDetails responses for error conditions
   - what_to_look_for: Check that 400 (validation), 404 (not found), and 409 (conflict/business rule violation) responses use ProblemDetails format. Look for consistent error response structure. Verify error messages are informative but don't leak internal details.
   - why_it_matters: Consistent error responses enable clients to handle failures reliably
   - tier: high
   - evaluation_method: hybrid

### 5.2 Define Web API-specific new dimensions

6. **HTTP Test File Executability** (Web API only)
   - description: Whether the .http test file can be executed and gets correct responses
   - what_to_look_for: Check that .http file request URLs match actual routes. Verify request bodies use correct property names and types. Check that referenced IDs in URLs are consistent with seed data or creation order. Verify the file covers all CRUD operations.
   - why_it_matters: A correct .http file serves as executable documentation and validates the API works end-to-end
   - tier: high
   - evaluation_method: hybrid

   **Important: Non-idempotent request handling.** When executing .http files automatically, mutating requests (POST, PUT, DELETE, PATCH) may fail on second runs or when seed data has already established state. Common failure modes include:
   - POST creating a duplicate entity (409 Conflict or unique constraint violation)
   - DELETE on an already-deleted entity (404 Not Found)
   - PUT/PATCH on a non-existent entity after prior deletion (404 Not Found)
   - POST with an ID that already exists in seed data (duplicate key)

   **Mitigation strategy for automated .http execution (Phase 2):**
   1. **Classify requests by type:** GET requests are idempotent — expect 200. Mutating requests (POST/PUT/DELETE/PATCH) may legitimately return 4xx on re-execution.
   2. **Scoring approach — two tiers:**
      - **GET endpoints:** Must return 2xx. A 404/500 on a GET against seed data is a real bug. These are the primary correctness signal.
      - **Mutating endpoints:** Score on _first-run success_ only. On re-runs, accept 4xx as "previously succeeded" (not a failure). Alternatively, count 2xx OR expected-conflict-4xx as acceptable.
   3. **Fresh database per run:** The most reliable approach is to ensure each verification run starts with a clean database (the app should seed on startup when DB is empty, per the scenario spec). This makes all requests idempotent across runs.
   4. **Request ordering matters:** Execute .http file requests in document order (top to bottom). Well-authored .http files create parent entities before children, so respecting order maximizes first-run success.
   5. **Metric:** Report `GET success rate` (strict — 2xx only) and `mutation success rate` (lenient — 2xx or expected 4xx on re-run) separately, rather than a single pass/fail.

### 5.3 Define Razor Pages-specific new dimensions

7. **Anti-Forgery Token Coverage** (Razor Pages only)
   - description: Whether all forms include CSRF protection
   - what_to_look_for: Check that all POST form handlers validate anti-forgery tokens. Look for [ValidateAntiForgeryToken] attribute or global auto-validation. Verify <form> elements include @Html.AntiForgeryToken() or use the asp-antiforgery tag helper.
   - why_it_matters: Missing CSRF tokens is a critical security vulnerability (OWASP Top 10)
   - tier: critical
   - evaluation_method: hybrid

---

## Phase 6: Dimension Consolidation

### 6.1 Consolidate redundant dimensions in aspnet-webapi

| Current Dimensions | Consolidated Into | Rationale |
|---|---|---|
| API Style + Route Group Organization | **API Architecture** | Both evaluate Minimal API patterns; Route Groups are N/A with Controllers |
| Naming Conventions + Access Modifier Explicitness | **Code Standards Compliance** | Both measurable by `dotnet format`; same underlying quality signal |
| Collection Initialization + Primary Constructors + Global Using Directives | **Modern C# Adoption** | All measure adoption of latest C# features; can be scored collectively |

This reduces the Web API example from 31 → 28 dimensions, then adding ~5 new dimensions brings it to ~33.

### 6.2 Consolidate redundant dimensions in aspnet-razor-pages

| Current Dimensions | Consolidated Into | Rationale |
|---|---|---|
| Naming Conventions + Access Modifier Explicitness | **Code Standards Compliance** | Same as Web API rationale |
| Collection Initialization + Primary Constructors + Global Using Directives | **Modern C# Adoption** | Same as Web API rationale |
| CSS Organization (remove or merge into Bootstrap Integration) | **UI Styling** | CSS Organization is too granular for evaluation; Bootstrap Integration already covers styling quality |

This reduces Razor Pages from 35 → 31, then adding ~6 new dimensions brings it to ~37.

### 6.3 Assign tiers to all existing dimensions

**Both examples — assign tiers:**

| Tier | Dimensions |
|------|-----------|
| **critical** | Security Vulnerability Scan, Input Validation Coverage, Endpoint Completeness, Business Rule Implementation |
| **high** | Exception Handling Strategy, Middleware Style, CancellationToken Propagation, Async/Await Best Practices, EF Core Relationship Configuration, Error Response Conformance, DTO Design, Service Abstraction, Modern C# Adoption (was Collection Init + Primary Constructors + Global Usings) |
| **medium** | API Architecture (was API Style), AsNoTracking Usage, Return Type Precision, Data Seeder Design, File Organization, Pagination, Structured Logging, Nullable Reference Types, Enum Design, Guard Clauses, Dispose & Resource Management, TypedResults Usage, OpenAPI Metadata, HTTP Test File Quality, Sealed Types, Package Discipline |
| **low** | Code Standards Compliance (was Naming + Access Modifiers) |

---

## Phase 7: Update Both Examples

### 7.1 Update `examples/aspnet-webapi/eval.yaml`

Changes:
- Add `runs: 1` at top level (explicit default)
- Add new dimensions (Security Vulnerability Scan, Input Validation Coverage, Endpoint Completeness, Business Rule Implementation, Error Response Conformance, HTTP Test File Executability)
- Consolidate redundant dimensions per Phase 6.1
- Add `tier` field to all dimensions
- Add `evaluation_method` field where applicable
- Add expanded verification config:
  ```yaml
  verification:
    build:
      command: "dotnet build"
      success_pattern: "Build succeeded"
    format:
      command: "dotnet format --check"
    security:
      vulnerability_scan: true
    analyzers:
      - "Meziantou.Analyzer"
    run:
      command: "dotnet run"
      timeout_seconds: 15
  ```

### 7.2 Update `examples/aspnet-razor-pages/eval.yaml`

Same structural changes as Web API, plus Razor Pages-specific:
- Add Anti-Forgery Token Coverage dimension
- Consolidate CSS Organization into UI Styling
- Add tiers to all dimensions (some differ from Web API — e.g., Form Handling & Validation is `high` tier)

### 7.3 Update init_cmd.py suggested dimensions

Update `_suggest_dimensions()` to include new dimension categories:

- Add "Security" category to universal suggestions (Security Vulnerability Scan, Input Validation Coverage)
- Add "Functional Correctness" category (Endpoint Completeness, Business Rule Implementation)
- Include `tier` in suggested dimensions
- Update .NET-specific suggestions with expanded list

**Files to change:** `src/skill_eval/init_cmd.py`

---

## Phase 8: Template Updates

### 8.1 Update scenario.prompt.md.j2

Add a section reminding scenario authors to include testable business rules and explicit endpoint lists, since the new Functional Correctness dimensions depend on clear specifications.

**Files to change:** `templates/scenario.prompt.md.j2`

### 8.2 Update create-all-apps.md.j2

No structural changes needed for single-run mode. For multi-run mode, the generation loop in `generate.py` handles iteration — the template stays the same.

### 8.3 Create .editorconfig template

Create a standardized `.editorconfig` that is optionally injected alongside `Directory.Build.props` for consistent code style evaluation:

**Create file:** `templates/.editorconfig`

---

## Phase 9: Documentation Updates

### 9.1 Update README.md

- Document new verification steps (format, security, analyzers)
- Document dimension tiers and weighting
- Document multi-run support (`--runs N` flag or `runs:` in eval.yaml)
- Update the example `eval.yaml` in README to show new fields
- Add a "Verification Pipeline" section explaining the full pipeline
- Document the numerical scoring system (1–5)

### 9.2 Update docs/authoring-guide.md

- Add guidance on writing business rules that are testable/verifiable
- Add guidance on specifying endpoints precisely (for Endpoint Completeness dimension)
- Note that security dimensions will be evaluated
- Update the pre-submission checklist with new items

---

## Implementation Order (Dependencies)

```
Phase 1 (Config models)  ──┐
                            ├── Phase 2 (Verification) ──┐
                            │                             ├── Phase 3 (Analysis)
Phase 5 (New dimensions) ──┤                             │
                            │                             ├── Phase 7 (Examples)
Phase 6 (Consolidation)  ──┘                             │
                                                          ├── Phase 9 (Docs)
Phase 4 (Multi-run)  ────────────────────────────────────┘

Phase 8 (Templates) — independent, can be done anytime
```

**Recommended order:**
1. Phase 1 → foundation, everything depends on it
2. Phase 5 + 6 → define new dimensions and consolidations (content decisions)
3. Phase 2 → verification pipeline (biggest code change)
4. Phase 3 → analysis improvements
5. Phase 7 → update both examples with all new features
6. Phase 8 → template updates
7. Phase 4 → multi-run support (most complex, can ship as separate PR)
8. Phase 9 → documentation (last, after all features stabilized)

---

## Files Changed Summary

| Phase | Files | Type |
|-------|-------|------|
| 1 | `src/skill_eval/config.py` | Edit |
| 2 | `src/skill_eval/verify.py` | Edit |
| 2 | `templates/Directory.Build.props` | Create |
| 3 | `src/skill_eval/analyze.py` | Edit |
| 3 | `templates/analyze.md.j2` | Edit |
| 4 | `src/skill_eval/cli.py` | Edit |
| 4 | `src/skill_eval/generate.py` | Edit |
| 7 | `examples/aspnet-webapi/eval.yaml` | Edit |
| 7 | `examples/aspnet-razor-pages/eval.yaml` | Edit |
| 7 | `src/skill_eval/init_cmd.py` | Edit |
| 8 | `templates/scenario.prompt.md.j2` | Edit |
| 8 | `templates/.editorconfig` | Create |
| 9 | `README.md` | Edit |
| 9 | `docs/authoring-guide.md` | Edit |

---

## Risk Assessment

| Risk | Impact | Mitigation |
|------|--------|------------|
| Backward compatibility — existing eval.yaml files break | High | All new fields have defaults; existing configs work unchanged |
| Multi-run cost — 3× compute for generation | Medium | Default `runs: 1`; multi-run is opt-in |
| Roslyn analyzer injection — may conflict with project config | Medium | Check for existing Directory.Build.props before injecting; skip if present |
| Numerical scoring may reduce analysis nuance | Low | Keep qualitative verdict alongside numerical scores |
| Dimension consolidation may lose signal | Low | Consolidated dimensions cover same ground with broader scope |
