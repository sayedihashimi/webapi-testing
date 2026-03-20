---
mode: agent
description: "Create a Fitness & Wellness Studio Booking API using ASP.NET Core Web API and .NET 10"
tools: ["changes", "codebase", "fetch", "findTestFiles", "githubRepo", "problems", "runner", "selection", "terminalLastCommand", "terminalSelection", "usages", "visionScreenshot"]
---

# Fitness & Wellness Studio Booking API

## App Overview

Build a fitness and wellness studio booking API for **"Zenith Fitness Studio"**. This API manages members, membership plans, memberships, instructors, class types, class schedules, bookings, and a waitlist system. The system helps the studio manage membership sales, class scheduling, instructor assignments, and member bookings with capacity management.

## Technical Requirements

- **Framework**: ASP.NET Core Web API targeting **.NET 10**
- **Database**: Entity Framework Core with **SQLite** database (connection string configured in `appsettings.json`, database file stored in the project directory)
- **Authentication**: None — no authentication or authorization required
- **Documentation**: OpenAPI/Swagger documentation enabled
- **Project Structure**: Structured with separation of concerns — **Models**, **DTOs**, **Services** (interface + implementation), and **API layer**
- **Validation**: Use Data Annotations and/or FluentValidation for input validation
- **Error Handling**: Global error handling middleware that returns consistent **ProblemDetails** responses
- **Project Location**: Create the project at `./src/FitnessStudioApi/`

## Entities & Relationships

### MembershipPlan

- **Id** (int, PK, auto-generated)
- **Name** (string, required, max 100, unique — e.g., "Basic", "Premium", "Elite")
- **Description** (string, optional, max 500)
- **DurationMonths** (int, required, range 1–24)
- **Price** (decimal, required, must be positive)
- **MaxClassBookingsPerWeek** (int, required — e.g., Basic=3, Premium=5, Elite=unlimited represented as -1)
- **AllowsPremiumClasses** (bool — Elite and Premium can access premium classes, Basic cannot)
- **IsActive** (bool, default true)
- **CreatedAt**, **UpdatedAt**

### Member

- **Id** (int, PK, auto-generated)
- **FirstName** (string, required, max 100)
- **LastName** (string, required, max 100)
- **Email** (string, required, unique)
- **Phone** (string, required)
- **DateOfBirth** (DateOnly, required, must be at least 16 years old)
- **EmergencyContactName** (string, required, max 200)
- **EmergencyContactPhone** (string, required)
- **JoinDate** (DateOnly, auto-set to today)
- **IsActive** (bool, default true)
- **CreatedAt**, **UpdatedAt**
- **Relationships**: Has many Memberships, Bookings

### Membership

- **Id** (int, PK, auto-generated)
- **MemberId** (int, FK → Member, required)
- **MembershipPlanId** (int, FK → MembershipPlan, required)
- **StartDate** (DateOnly, required)
- **EndDate** (DateOnly, computed — StartDate + Plan.DurationMonths)
- **Status** (enum: Active, Expired, Cancelled, Frozen)
- **PaymentStatus** (enum: Paid, Pending, Refunded)
- **FreezeStartDate** (DateOnly, nullable — set when membership is frozen)
- **FreezeEndDate** (DateOnly, nullable — when freeze ends, membership EndDate is extended by freeze duration)
- **CreatedAt**, **UpdatedAt**
- **Relationships**: Belongs to Member and MembershipPlan
- A member can have multiple memberships (historical), but only one Active or Frozen at a time

### Instructor

- **Id** (int, PK, auto-generated)
- **FirstName** (string, required, max 100)
- **LastName** (string, required, max 100)
- **Email** (string, required, unique)
- **Phone** (string, required)
- **Bio** (string, optional, max 1000)
- **Specializations** (string, optional — comma-separated list, e.g., "Yoga, Pilates, Meditation")
- **HireDate** (DateOnly, required)
- **IsActive** (bool, default true)
- **CreatedAt**, **UpdatedAt**
- **Relationships**: Has many ClassSchedules

### ClassType

- **Id** (int, PK, auto-generated)
- **Name** (string, required, max 100, unique — e.g., "Yoga", "HIIT", "Spin", "Pilates", "Boxing", "Meditation")
- **Description** (string, optional, max 500)
- **DefaultDurationMinutes** (int, required, range 30–120)
- **DefaultCapacity** (int, required, range 1–50)
- **IsPremium** (bool — marks classes only available to Premium/Elite members)
- **CaloriesPerSession** (int, optional — estimated calories burned)
- **DifficultyLevel** (enum: Beginner, Intermediate, Advanced, AllLevels)
- **IsActive** (bool, default true)
- **CreatedAt**, **UpdatedAt**

### ClassSchedule

- **Id** (int, PK, auto-generated)
- **ClassTypeId** (int, FK → ClassType, required)
- **InstructorId** (int, FK → Instructor, required)
- **StartTime** (DateTime, required)
- **EndTime** (DateTime, required — StartTime + duration)
- **Capacity** (int, required, overrides ClassType default if specified)
- **CurrentEnrollment** (int, tracked — count of confirmed bookings)
- **WaitlistCount** (int, tracked — count of waitlisted bookings)
- **Room** (string, required, max 50 — e.g., "Studio A", "Studio B", "Main Floor")
- **Status** (enum: Scheduled, InProgress, Completed, Cancelled)
- **CancellationReason** (string, optional)
- **CreatedAt**, **UpdatedAt**
- **Relationships**: Belongs to ClassType and Instructor. Has many Bookings.

### Booking

- **Id** (int, PK, auto-generated)
- **ClassScheduleId** (int, FK → ClassSchedule, required)
- **MemberId** (int, FK → Member, required)
- **BookingDate** (DateTime, auto-set to now)
- **Status** (enum: Confirmed, Waitlisted, Cancelled, Attended, NoShow)
- **WaitlistPosition** (int, nullable — only set when Status is Waitlisted)
- **CheckInTime** (DateTime, nullable — set when member checks in)
- **CancellationDate** (DateTime, nullable)
- **CancellationReason** (string, optional)
- **CreatedAt**, **UpdatedAt**
- **Relationships**: Belongs to ClassSchedule and Member

## Business Rules

1. **Booking Window**: Members can only book classes up to 7 days in advance and no less than 30 minutes before class start time.

2. **Capacity Management**:
   - When a class is full (CurrentEnrollment == Capacity), new bookings go to the waitlist.
   - When a confirmed booking is cancelled, the first person on the waitlist is automatically promoted to Confirmed and CurrentEnrollment is maintained.
   - WaitlistPosition is maintained in order.

3. **Cancellation Policy**:
   - Free cancellation up to 2 hours before class start time.
   - Late cancellation (less than 2 hours before): allowed but marked as "late cancellation" in notes.
   - Cannot cancel a class that has already started or completed.

4. **Membership Tier Access**:
   - Premium classes (IsPremium = true) can only be booked by members with a plan where AllowsPremiumClasses = true.
   - Basic members attempting to book premium classes should get a clear error.

5. **Weekly Booking Limits**:
   - Enforce MaxClassBookingsPerWeek from the member's active plan.
   - Count only Confirmed and Attended bookings for the current ISO week.
   - -1 means unlimited.

6. **Active Membership Required**:
   - Only members with an Active (not Expired, Cancelled, or Frozen) membership can book classes.
   - Frozen memberships cannot book but retain their membership.

7. **No Double Booking**: A member cannot book two classes that overlap in time (check StartTime–EndTime ranges).

8. **Instructor Schedule Conflicts**: An instructor cannot be assigned to two classes that overlap in time.

9. **Membership Freeze**:
   - A membership can be frozen for 7–30 days.
   - When frozen, the EndDate is extended by the freeze duration when unfreezing.
   - Only Active memberships can be frozen.
   - A membership can only be frozen once per membership term.

10. **Class Cancellation by Studio**: When a class is cancelled (status → Cancelled), all bookings for that class should be automatically cancelled with reason "Class cancelled by studio".

11. **Check-In**: Members can check in starting 15 minutes before class start time and up to 15 minutes after. Check-in transitions the booking from Confirmed to Attended.

12. **No-Show**: Bookings with status Confirmed that are not checked in within 15 minutes of class start should be flaggable as NoShow.

## API Endpoints

### Membership Plans

- `GET /api/membership-plans` — List all active plans
- `GET /api/membership-plans/{id}` — Get plan details
- `POST /api/membership-plans` — Create plan
- `PUT /api/membership-plans/{id}` — Update plan
- `DELETE /api/membership-plans/{id}` — Deactivate plan (set IsActive = false)

### Members

- `GET /api/members` — List members (search by name, email; filter by active status; pagination)
- `GET /api/members/{id}` — Get member details (include active membership info, booking stats)
- `POST /api/members` — Register a new member
- `PUT /api/members/{id}` — Update member profile
- `DELETE /api/members/{id}` — Deactivate member (fail if they have future bookings)
- `GET /api/members/{id}/bookings` — Get member's bookings (filter by status, date range; pagination)
- `GET /api/members/{id}/bookings/upcoming` — Get member's upcoming confirmed bookings
- `GET /api/members/{id}/memberships` — Get membership history for a member

### Memberships

- `POST /api/memberships` — Purchase/create a membership for a member
- `GET /api/memberships/{id}` — Get membership details
- `POST /api/memberships/{id}/cancel` — Cancel a membership
- `POST /api/memberships/{id}/freeze` — Freeze a membership (provide freeze duration)
- `POST /api/memberships/{id}/unfreeze` — Unfreeze a membership (extends EndDate)
- `POST /api/memberships/{id}/renew` — Renew an expired membership (creates a new Active membership)

### Instructors

- `GET /api/instructors` — List instructors (filter by specialization, active status)
- `GET /api/instructors/{id}` — Get instructor details
- `POST /api/instructors` — Create instructor
- `PUT /api/instructors/{id}` — Update instructor
- `GET /api/instructors/{id}/schedule` — Get instructor's class schedule (filter by date range)

### Class Types

- `GET /api/class-types` — List class types (filter by difficulty, premium status)
- `GET /api/class-types/{id}` — Get class type details
- `POST /api/class-types` — Create class type
- `PUT /api/class-types/{id}` — Update class type

### Class Schedules

- `GET /api/classes` — List scheduled classes (filter by date range, class type, instructor, availability; pagination)
- `GET /api/classes/{id}` — Get class details (include roster count, waitlist count, availability)
- `POST /api/classes` — Schedule a new class (enforce instructor conflicts)
- `PUT /api/classes/{id}` — Update class details (check for instructor conflicts if instructor/time changed)
- `PATCH /api/classes/{id}/cancel` — Cancel a class (cascade cancel all bookings)
- `GET /api/classes/{id}/roster` — Get the list of confirmed members for a class
- `GET /api/classes/{id}/waitlist` — Get the waitlist for a class
- `GET /api/classes/available` — Get classes with available spots in the next 7 days

### Bookings

- `POST /api/bookings` — Book a class (enforce all booking rules: capacity, membership, tier, weekly limit, overlap, booking window)
- `GET /api/bookings/{id}` — Get booking details
- `POST /api/bookings/{id}/cancel` — Cancel a booking (enforce cancellation policy, promote from waitlist)
- `POST /api/bookings/{id}/check-in` — Check in for a class (enforce check-in window)
- `POST /api/bookings/{id}/no-show` — Mark booking as no-show

## Seed Data

The application **MUST** seed the database on startup with realistic dummy data for demo and testing purposes. Include:

- At least **3 membership plans** (Basic at $29.99/month, Premium at $49.99/month, Elite at $79.99/month)
- At least **8 members** with realistic names, emails, dates of birth, emergency contacts
- At least **6 active memberships** across different plans (some members with expired memberships too)
- At least **4 instructors** with different specializations
- At least **6 class types** (Yoga, HIIT, Spin, Pilates, Boxing, Meditation — make 2 of them premium)
- At least **12 class schedules** over the next 7 days (various times, rooms, instructors — some full, some with availability, one cancelled)
- At least **15 bookings** in various states (confirmed, waitlisted, attended, cancelled, no-show)
- Ensure some classes are at capacity with waitlisted members to demonstrate waitlist functionality

Use the EF Core seeding mechanism or a data seeder service. Ensure the seed data only runs when the database is empty to avoid duplicates.

## HTTP File

Create a `.http` file (e.g., `FitnessStudioApi.http`) in the same folder as `Program.cs`. This file should:

- Use a `@baseUrl` variable set to `http://localhost:{{port}}` (use whatever port the app is configured to run on)
- Include sample requests for **ALL** endpoints listed above
- Group requests by resource with comment headers (`### Membership Plans`, `### Members`, `### Bookings`, etc.)
- Include realistic request bodies for POST and PUT operations
- Include query parameter examples for search/filter/pagination endpoints
- Use IDs that correspond to the seed data so requests work out of the box
- Include examples that test business rules (e.g., booking a premium class as a basic member, booking when at weekly limit, cancellation within 2 hours)

## Cross-Cutting Concerns

- **Error Handling**: Global exception handler returning RFC 7807 ProblemDetails. Business rule violations should return 400/409 with descriptive error messages.
- **Validation**: Validate all input DTOs. Return 400 with validation error details.
- **Logging**: Use the built-in `ILogger`. Log key operations (booking created, class cancelled, membership frozen, etc.) at Information level. Log errors at Error level.
- **OpenAPI/Swagger**: Configure Swagger UI at the root or `/swagger`. Include descriptive operation summaries and response types.
- **Pagination**: Use consistent pagination across list endpoints (page number + page size, with defaults).

## Project Location

Create the project at: `./src/FitnessStudioApi/`

The project should be a standalone ASP.NET Core Web API project with no dependencies on other projects in this repository.
