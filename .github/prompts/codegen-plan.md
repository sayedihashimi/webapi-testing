# Plan: Create 3 ASP.NET Core Web API Prompt Files

## Problem Statement
Create 3 detailed `.prompt.md` files in `.github/prompts/` that can be used with GitHub Copilot to generate realistic ASP.NET Core Web API applications. These will be used to evaluate AI code generation quality — with and without custom skills.

## Constraints & Decisions
- **Framework**: ASP.NET Core Web API, .NET 10
- **API style**: Not prescribed — let the AI decide (controllers vs minimal APIs)
- **Data layer**: EF Core with SQLite
- **Auth**: None (demo/evaluation apps)
- **Tests**: Out of scope
- **Complexity**: Higher — multiple related entities, DTOs, service layer, business rules, validation, error handling
- **Output folders**: Each app under `./src/<app-name>/`
- **HTTP file**: Each app must include a `.http` file in the same folder as `Program.cs` that exercises all API endpoints (for use with VS / VS Code REST Client)
- **Seed data**: Each app must seed the database with realistic dummy data on startup so it's immediately usable for demos and testing
- **Prompts must NOT define the code** — they should describe *what* the app does, its entities, relationships, business rules, and API surface. The actual implementation is left to the AI.

## The 3 App Scenarios

### 1. Veterinary Clinic Management API (`src/VetClinicApi/`)
**Domain**: Healthcare / service management
**Prompt file**: `.github/prompts/create-vet-clinic-api.prompt.md`

A veterinary clinic system for managing pets, owners, appointments, medical records, and prescriptions.

**Key entities**: Owners, Pets (with species/breed), Veterinarians, Appointments, MedicalRecords, Prescriptions, Vaccinations
**Business rules**:
- Appointment scheduling with conflict detection (no double-booking a vet)
- Pets must belong to an owner
- Prescriptions linked to medical records, with dosage and duration
- Vaccination tracking with next-due-date calculations
- Appointment status workflow (Scheduled → Checked-In → In-Progress → Completed / Cancelled / No-Show)
- Cancellation rules (e.g., cannot cancel past appointments)

**API surface**: Full CRUD for core entities + specialized endpoints (e.g., upcoming vaccinations for a pet, appointment history for an owner, vet's schedule for a given day).

---

### 2. Community Library Management API (`src/LibraryApi/`)
**Domain**: Resource management / public service
**Prompt file**: `.github/prompts/create-library-api.prompt.md`

A community library system for managing books, patrons, loans, reservations, and fines.

**Key entities**: Books, Authors, Categories, Patrons, Loans, Reservations, Fines
**Business rules**:
- Borrowing limits (e.g., max 5 active loans per patron)
- Loan duration with due dates and overdue detection
- Fine calculation based on overdue days
- Reservation queue when a book is checked out (first-come-first-served)
- Book availability tracking (total copies vs checked-out copies)
- Loan status workflow (Active → Returned / Overdue)
- Cannot check out a book if patron has unpaid fines above a threshold

**API surface**: Full CRUD for core entities + specialized endpoints (e.g., search books by title/author/category, patron's active loans, overdue loans report, reservation queue for a book).

---

### 3. Fitness & Wellness Studio API (`src/FitnessStudioApi/`)
**Domain**: Booking / membership management
**Prompt file**: `.github/prompts/create-fitness-studio-api.prompt.md`

A fitness studio system for managing classes, instructors, members, memberships, and bookings.

**Key entities**: Members, MembershipPlans, Memberships, Instructors, ClassTypes, ClassSchedules, Bookings
**Business rules**:
- Class capacity limits with waitlist support
- Booking window (e.g., can only book up to 7 days in advance)
- Cancellation policy (e.g., free cancellation up to 2 hours before class)
- Membership tier access (some classes restricted to premium members)
- Membership expiration and renewal tracking
- Instructor assignment and schedule conflict detection
- No double-booking a member for overlapping class times

**API surface**: Full CRUD for core entities + specialized endpoints (e.g., available classes for a date range, member's booking history, class roster, instructor's weekly schedule, waitlist position).

---

## Prompt File Structure
Each `.prompt.md` file will follow this structure:
1. **Header / Metadata** — mode, tools (if applicable)
2. **App Overview** — what the app is and its purpose
3. **Technical Requirements** — framework, data layer, patterns
4. **Entity Descriptions & Relationships** — what the data model look like
5. **Business Rules** — the logic the app must enforce
6. **API Endpoints** — the operations the API must support
7. **Seed Data** — instruct the AI to seed the database with realistic dummy data on startup
8. **HTTP File** — instruct the AI to create a `.http` file alongside `Program.cs` that exercises all endpoints
9. **Cross-Cutting Concerns** — error handling, validation, OpenAPI/Swagger, logging
10. **Project Structure Guidance** — where the app should be created (folder path)

## Todos

1. **create-folders** — Create `src/` directory
2. **create-vet-clinic-prompt** — Write `.github/prompts/create-vet-clinic-api.prompt.md`
3. **create-library-prompt** — Write `.github/prompts/create-library-api.prompt.md`
4. **create-fitness-studio-prompt** — Write `.github/prompts/create-fitness-studio-api.prompt.md`
