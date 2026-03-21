# Scenario Prompt Authoring Guide

This guide explains how to write **scenario prompt files** — the detailed app specifications that drive the [Copilot Skill Evaluation Framework](../README.md). A well-written prompt produces realistic, comparable code across different skill configurations, making your evaluation results meaningful.

The patterns documented here are derived from the three reference prompts in `examples/aspnet-webapi/prompts/scenarios/`:

| Prompt | Domain | Complexity |
|--------|--------|------------|
| `fitness-studio.prompt.md` | Booking/membership | Waitlists, capacity management, membership tiers, freeze/unfreeze |
| `library.prompt.md` | Resource management | Loans, reservations, overdue fines, renewal limits |
| `vet-clinic.prompt.md` | Healthcare/service | Appointment workflows, medical records, prescriptions, vaccinations |

---

## Core Principles

These six principles were the guiding instructions behind the original prompts. Every decision in a scenario prompt should trace back to one of them.

### 1. Describe WHAT, Never HOW

The prompt defines **entities, relationships, business rules, and endpoints** — but never prescribes code patterns, class hierarchies, architecture decisions, or specific implementation techniques.

This is the most important principle. The entire point of a skill evaluation is to see how different skill configurations influence the *implementation*. If the prompt dictates the implementation, all configurations produce the same code and the evaluation is meaningless.

**Do this:**
> The system manages members, membership plans, memberships, instructors, class types, class schedules, bookings, and a waitlist system.

**Don't do this:**
> Create a `BookingService` class that implements `IBookingService`. Use the repository pattern with a `GenericRepository<T>`. Register services using `AddScoped<IBookingService, BookingService>()`.

**Where the line is:**
- ✅ "Use Entity Framework Core with SQLite" — this is a *technology choice*, not an implementation pattern
- ✅ "Global error handling middleware that returns ProblemDetails" — this is a *requirement*, not a code structure
- ❌ "Use the `IExceptionHandler` interface for error handling" — this prescribes a *specific implementation*
- ❌ "Use Minimal APIs instead of controllers" — this prescribes an *architectural approach*

### 2. Choose Realistic, Non-Trivial Domains

Each app should model a real business scenario with enough complexity to exercise advanced patterns. Trivial apps (TODO lists, simple CRUDs) don't differentiate skill quality.

Good domains have:
- **Multiple related entities** (at least 5–7) with foreign key relationships
- **Business rules that interact** (e.g., booking a class requires checking capacity, membership tier, weekly limits, and time overlap — all at once)
- **State machines** (appointment workflows, membership statuses, booking lifecycle)
- **Computed values** (available copies, waitlist positions, overdue fines)
- **Edge cases** (what happens when a class is cancelled? when a reservation expires? when a membership is frozen?)

Give each app a **fictional business name** (e.g., "Zenith Fitness Studio", "Sunrise Community Library", "Happy Paws Veterinary Clinic"). This makes the prompts feel grounded and gives the generated code a realistic context.

### 3. Omit Authentication

These are evaluation and demo apps. Authentication adds complexity without contributing to skill evaluation — it increases generation time, introduces auth-framework-specific code, and makes the generated apps harder to exercise.

State this explicitly:
> **Authentication**: None — no authentication or authorization required.

### 4. Keep Apps Standalone and Independent

Each scenario app must be fully self-contained. No shared libraries, no cross-project references, no shared database. This ensures:
- Each app can be generated, built, and run in isolation
- Different skill configurations don't accidentally share artifacts
- Apps can be evaluated independently or in parallel

### 5. Include Exercisable Artifacts

Every generated app must be immediately testable without external setup. This requires two things:

**Seed Data** — Realistic dummy data seeded on startup so the API has something to return. Specify minimum quantities and ensure internal consistency (e.g., `AvailableCopies` values must match the number of active loans).

**HTTP File** — A `.http` file with sample requests for every endpoint, using IDs that match the seed data. This lets evaluators exercise the entire API within seconds of running it.

### 6. Maintain Consistent Structure

All scenario prompts in an evaluation should follow the same section ordering and detail level. This ensures fair comparison — if one prompt is more detailed than another, the generated code will vary for reasons unrelated to skill quality.

The reference structure is documented in the [Prompt Structure Reference](#prompt-structure-reference) below.

---

## Prompt Structure Reference

Every scenario prompt follows this section order. Each section is required unless noted as optional.

### YAML Front Matter

```yaml
---
mode: agent
description: "Create a [App Name] using [Technology Stack]"
tools: ["changes", "codebase", "fetch", "findTestFiles", "githubRepo", "problems", "runner", "selection", "terminalLastCommand", "terminalSelection", "usages", "visionScreenshot"]
---
```

- `mode: agent` tells Copilot to run this as an agentic task (multi-step, tool-using)
- `description` is a one-line summary — include the app name and tech stack
- `tools` lists the Copilot tools the agent can use during generation

### App Overview

A 2–4 sentence narrative description of what the app does, who uses it, and its core purpose. Include the **fictional business name**.

> Build a fitness and wellness studio booking API for **"Zenith Fitness Studio"**. This API manages members, membership plans, memberships, instructors, class types, class schedules, bookings, and a waitlist system. The system helps the studio manage membership sales, class scheduling, instructor assignments, and member bookings with capacity management.

**Tips:**
- Name every entity the system manages — this sets scope expectations
- Describe the user persona (e.g., "used by library staff", "used by clinic administrators")
- If the app is API-only (no frontend), state that explicitly

### Technical Requirements

A bulleted list of technology choices and architectural constraints. Be specific about versions.

```markdown
- **Framework**: ASP.NET Core Web API targeting **.NET 10**
- **Database**: Entity Framework Core with **SQLite**
- **Authentication**: None
- **Documentation**: OpenAPI/Swagger documentation enabled
- **Project Structure**: Separation of concerns — Models, DTOs, Services, API layer
- **Validation**: Data Annotations and/or FluentValidation
- **Error Handling**: Global error handling with ProblemDetails responses
- **Project Location**: Create the project at `./src/MyApp/`
```

**What to include:**
- Framework and version
- ORM/database technology and where the database file lives
- Auth stance (always "None" for evaluation prompts)
- API documentation approach
- High-level structural expectations (Models/DTOs/Services — but NOT how to implement them)
- Validation approach
- Error handling approach (ProblemDetails, not specific middleware classes)
- Exact project output location

**What NOT to include:**
- Specific patterns like "use repository pattern" or "use Minimal APIs"
- Package names beyond the ORM/framework
- Code snippets or class structures

### Entities & Relationships

The most detailed section. Define every entity with:
- Field name, type, and constraints (required, max length, unique, ranges, defaults)
- Primary keys and auto-generation
- Foreign keys with explicit target entity
- Computed fields and how they're derived
- Enum definitions with all valid values
- Relationship descriptions (one-to-many, many-to-many, join entities)
- Timestamp fields (CreatedAt, UpdatedAt)

**Use tables for field definitions** — they're scannable and unambiguous:

```markdown
| Field           | Type     | Constraints                                        |
|-----------------|----------|----------------------------------------------------|
| Id              | int      | PK, auto-generated                                 |
| Name            | string   | Required, max length 100, unique                   |
| DurationMonths  | int      | Required, range 1–24                               |
| Price           | decimal  | Required, must be positive                         |
| IsActive        | bool     | Default true                                       |
| CreatedAt       | DateTime | Auto-set on creation                               |
```

**Or use a list format with inline constraints:**

```markdown
- **Id** (int, PK, auto-generated)
- **Name** (string, required, max 100, unique)
- **DurationMonths** (int, required, range 1–24)
- **Price** (decimal, required, must be positive)
```

Either format works — just be consistent across all entities in a prompt.

**Relationships section** below each entity:

> **Relationships**: A Pet belongs to one Owner. A Pet has many Appointments, MedicalRecords, and Vaccinations.

**Tips:**
- Define enum values inline (e.g., `Status (enum: Active, Expired, Cancelled, Frozen)`)
- Call out computed fields explicitly: "AvailableCopies — equals TotalCopies minus currently checked-out copies"
- Note multiplicity constraints: "A member can have multiple memberships (historical), but only one Active or Frozen at a time"
- For many-to-many relationships, name the join entity: "via a `BookAuthor` join entity"

### Business Rules

Numbered rules that define the domain logic. Each rule should be:
- **Specific** — not "validate bookings" but "members can only book classes up to 7 days in advance and no less than 30 minutes before class start time"
- **Enforceable** — the rule must map to a concrete validation or computation
- **Independent** — each rule should be understandable without reading the others (though rules can reference each other by number)

Good rules cover:
- **Access control** (membership tiers, booking limits)
- **State transitions** (appointment workflow, booking lifecycle)
- **Capacity/resource management** (available copies, class enrollment, waitlist promotion)
- **Time-based constraints** (cancellation windows, check-in windows, booking advance limits)
- **Cascade effects** (cancelling a class cancels all bookings; returning a book checks reservation queue)
- **Computed values** (overdue fines = $0.25/day, freeze extends end date)

Example of a well-written rule:

> **Membership Freeze**: A membership can be frozen for 7–30 days. When frozen, the EndDate is extended by the freeze duration when unfreezing. Only Active memberships can be frozen. A membership can only be frozen once per membership term.

This is specific (7–30 days), defines the computation (extend EndDate), states preconditions (Active only), and includes a constraint (once per term).

**Aim for 8–12 business rules per app.** Fewer than that means the domain is too simple; more than that and you risk overwhelming the generator with interacting constraints.

### API Endpoints

A complete list of every endpoint the app should expose. Use tables grouped by resource:

```markdown
### Members

| Method | Endpoint                          | Description                                           |
|--------|-----------------------------------|-------------------------------------------------------|
| GET    | /api/members                      | List members (search by name, email; pagination)      |
| GET    | /api/members/{id}                 | Get member details with active membership info        |
| POST   | /api/members                      | Register a new member                                 |
| PUT    | /api/members/{id}                 | Update member profile                                 |
| DELETE | /api/members/{id}                 | Deactivate (fail if future bookings exist)            |
| GET    | /api/members/{id}/bookings        | Member's bookings (filter by status, date; pagination)|
```

**Tips:**
- Include **filter, search, and pagination** notes in the description (e.g., "search by name, email; filter by active status; pagination")
- Note **side effects** in the description (e.g., "cascade cancel all bookings", "promote from waitlist")
- Note **failure conditions** (e.g., "fail if the author has any books")
- Use **action endpoints** for state changes: `POST /api/bookings/{id}/cancel`, not `PATCH /api/bookings/{id}` with a status field
- Include **convenience endpoints** that aggregate data: `GET /api/classes/available`, `GET /api/loans/overdue`
- Use **consistent URL patterns**: plural nouns for resources, nested routes for sub-resources

### Seed Data

Specify minimum quantities and variety requirements for each entity. Be explicit about the states and relationships the seed data should demonstrate.

```markdown
The application **MUST** seed the database on startup with realistic dummy data:

- At least **3 membership plans** (Basic at $29.99, Premium at $49.99, Elite at $79.99)
- At least **8 members** with realistic names, emails, dates of birth
- At least **6 active memberships** across different plans (some expired too)
- At least **4 instructors** with different specializations
- At least **12 class schedules** over the next 7 days (some full, some with availability, one cancelled)
- At least **15 bookings** in various states (confirmed, waitlisted, attended, cancelled, no-show)
- Ensure some classes are at capacity with waitlisted members to demonstrate waitlist functionality
```

**Tips:**
- Specify **exact prices and values** for plans/tiers — this makes the seed data deterministic
- Require **state variety** — bookings in every status, some overdue loans, some expired memberships
- Require **consistency** — "AvailableCopies values should match the number of active loans"
- Specify that seed data **only runs when the database is empty** to avoid duplicates
- Call out **scenario coverage** — "ensure some classes are at capacity" so the waitlist can be demonstrated

### HTTP File

Instructions for generating a `.http` file that exercises the API.

```markdown
Create a `.http` file (e.g., `MyApp.http`) in the same folder as `Program.cs`. This file should:

- Use a `@baseUrl` variable set to `http://localhost:{{port}}`
- Include sample requests for **ALL** endpoints listed above
- Group requests by resource with comment headers
- Include realistic request bodies for POST and PUT operations
- Include query parameter examples for search/filter/pagination endpoints
- Use IDs that correspond to the seed data so requests work out of the box
- Include examples that test business rules (e.g., booking when at weekly limit)
```

The key instruction is that **IDs must match seed data** — the HTTP file should work against a freshly seeded database without modification.

### Cross-Cutting Concerns

Address these four areas consistently across all prompts:

**Error Handling:**
- Global exception handler returning RFC 7807 ProblemDetails
- Business rule violations → 400/409 with descriptive messages
- Not-found → 404

**Validation:**
- Validate all input DTOs
- Return 400 with structured validation error details

**Logging:**
- Use the framework's built-in logger
- Log key business operations at Information level
- Log errors at Error level

**OpenAPI/Swagger:**
- Swagger UI accessible at root or `/swagger`
- Descriptive operation summaries and response types

**Pagination:**
- Consistent pattern across all list endpoints
- Accept page number and page size with sensible defaults
- Return total count and page metadata

### Project Location

Explicitly state where the project should be created and that it must be standalone:

> Create the project at: `./src/MyApp/`
>
> The project should be a standalone [framework] project with no dependencies on other projects in this repository.

---

## Anti-Patterns

| Anti-Pattern | Why It's Bad | Do This Instead |
|-------------|-------------|-----------------|
| Prescribing code patterns ("use repository pattern") | Prevents skills from influencing architecture | Describe requirements, not implementation |
| Vague business rules ("validate bookings properly") | Generator can't implement what isn't defined | Be specific: "up to 7 days in advance, no less than 30 minutes before start" |
| Missing relationship details | Generator invents or omits foreign keys | Explicitly define every FK and join entity |
| Inconsistent entity detail levels | Some entities get rich specs, others are bare | Every entity gets the same field/constraint/relationship treatment |
| No enum values listed | Generator picks arbitrary strings | List all valid values inline: `Status (enum: Active, Expired, Cancelled)` |
| Seed data without state variety | Can't exercise business rules | Require entities in every status (some overdue, some cancelled, etc.) |
| HTTP file without business rule tests | Misses the most interesting evaluation surface | Include requests that trigger rule violations |
| No "what happens when" rules | Missing cascade/side-effect logic | Define cascades explicitly: "cancel class → cancel all bookings" |

---

## Pre-Submission Checklist

Use this checklist before finalizing a scenario prompt:

- [ ] **WHAT not HOW** — No code patterns, class names, or architectural prescriptions
- [ ] **Fictional business name** — App has a named identity (e.g., "Zenith Fitness Studio")
- [ ] **5+ entities** with full field/type/constraint definitions
- [ ] **All relationships** explicitly defined (FKs, join entities, multiplicities)
- [ ] **All enums** have their valid values listed
- [ ] **8+ business rules** — specific, enforceable, covering interactions and edge cases
- [ ] **State transitions** defined for any entity with a Status/Lifecycle field
- [ ] **All endpoints** listed with method, route, description, filters, side effects
- [ ] **Seed data** specifies quantities, variety, state coverage, and consistency requirements
- [ ] **HTTP file** instructions reference seed data IDs and business rule test cases
- [ ] **Cross-cutting concerns** cover error handling, validation, logging, OpenAPI, pagination
- [ ] **Project location** is explicit and states standalone requirement
- [ ] **No auth** — explicitly stated in Technical Requirements
- [ ] **Consistent** with other prompts in the evaluation (same section order, similar detail level)

---

## Reference Examples

The three prompts in `examples/aspnet-webapi/prompts/scenarios/` are the canonical examples:

- **[fitness-studio.prompt.md](../examples/aspnet-webapi/prompts/scenarios/fitness-studio.prompt.md)** — 7 entities, 12 business rules, 30+ endpoints. Best example of complex interactions (waitlist promotion, membership freeze, weekly booking limits, premium tier access).

- **[library.prompt.md](../examples/aspnet-webapi/prompts/scenarios/library.prompt.md)** — 7 entities, 8 business rules, 30+ endpoints. Best example of computed values (available copies, overdue fines) and reservation queues with expiration.

- **[vet-clinic.prompt.md](../examples/aspnet-webapi/prompts/scenarios/vet-clinic.prompt.md)** — 7 entities, 8 business rules, 25+ endpoints. Best example of appointment state machine workflows and one-to-one relationships (appointment → medical record).

Each follows the exact structure documented in this guide.
