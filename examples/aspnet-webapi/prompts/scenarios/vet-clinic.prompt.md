---
mode: agent
description: "Create a Veterinary Clinic Management API using ASP.NET Core Web API and .NET 10"
tools: ["changes", "codebase", "fetch", "findTestFiles", "githubRepo", "problems", "runner", "selection", "terminalLastCommand", "terminalSelection", "usages", "visionScreenshot"]
---

# Veterinary Clinic Management API

## App Overview

Build a veterinary clinic management system API for **"Happy Paws Veterinary Clinic"**. This API manages pet owners, their pets, veterinarian staff, appointments, medical records, prescriptions, and vaccination tracking. The system helps the clinic manage day-to-day operations including scheduling, patient records, and treatment history.

## Technical Requirements

- **Framework**: ASP.NET Core Web API targeting **.NET 10**
- **ORM & Database**: Entity Framework Core with **SQLite**. The connection string must be configured in `appsettings.json` and the database file must reside in the project directory.
- **Authentication**: None — no authentication or authorization is required.
- **API Documentation**: OpenAPI/Swagger documentation must be enabled.
- **Project Structure**: Use a well-organized structure with clear separation of concerns:
  - **Models** — Entity classes
  - **DTOs** — Request and response data transfer objects
  - **Services** — Business logic behind interfaces and their implementations
  - **API Layer** — Endpoints/Controllers
- **Validation**: Use Data Annotations and/or FluentValidation for input validation on all DTOs.
- **Error Handling**: Implement global error handling middleware that returns consistent **ProblemDetails** responses (RFC 7807).
- **Project Location**: Create the project at `./src/VetClinicApi/`. It must be a standalone ASP.NET Core Web API project with no dependencies on other projects in this repository.

## Entities & Relationships

### Owner

| Field       | Type     | Constraints                               |
|-------------|----------|-------------------------------------------|
| Id          | int      | PK, auto-generated                        |
| FirstName   | string   | Required, max length 100                  |
| LastName    | string   | Required, max length 100                  |
| Email       | string   | Required, unique, valid email format      |
| Phone       | string   | Required                                  |
| Address     | string   | Optional                                  |
| City        | string   | Optional                                  |
| State       | string   | Optional, max length 2                    |
| ZipCode     | string   | Optional                                  |
| CreatedAt   | DateTime | Auto-set on creation                      |
| UpdatedAt   | DateTime | Auto-set on every modification            |

**Relationship**: An Owner has many Pets.

### Pet

| Field           | Type     | Constraints                                                  |
|-----------------|----------|--------------------------------------------------------------|
| Id              | int      | PK, auto-generated                                           |
| Name            | string   | Required, max length 100                                     |
| Species         | string   | Required (e.g., "Dog", "Cat", "Bird", "Rabbit")             |
| Breed           | string   | Optional, max length 100                                     |
| DateOfBirth     | DateOnly | Optional                                                     |
| Weight          | decimal  | Optional, must be positive                                   |
| Color           | string   | Optional                                                     |
| MicrochipNumber | string   | Optional, unique                                             |
| IsActive        | bool     | Default true — used for soft delete                          |
| OwnerId         | int      | FK → Owner, required                                         |
| CreatedAt       | DateTime | Auto-set on creation                                         |
| UpdatedAt       | DateTime | Auto-set on every modification                               |

**Relationships**: A Pet belongs to one Owner. A Pet has many Appointments, MedicalRecords, and Vaccinations.

### Veterinarian

| Field          | Type     | Constraints                                                               |
|----------------|----------|---------------------------------------------------------------------------|
| Id             | int      | PK, auto-generated                                                        |
| FirstName      | string   | Required, max length 100                                                  |
| LastName       | string   | Required, max length 100                                                  |
| Email          | string   | Required, unique                                                          |
| Phone          | string   | Required                                                                  |
| Specialization | string   | Optional (e.g., "General Practice", "Surgery", "Dentistry", "Dermatology")|
| LicenseNumber  | string   | Required, unique                                                          |
| IsAvailable    | bool     | Default true                                                              |
| HireDate       | DateOnly | Required                                                                  |

**Relationships**: A Veterinarian has many Appointments.

### Appointment

| Field              | Type     | Constraints                                                                 |
|--------------------|----------|-----------------------------------------------------------------------------|
| Id                 | int      | PK, auto-generated                                                          |
| PetId              | int      | FK → Pet, required                                                          |
| VeterinarianId     | int      | FK → Veterinarian, required                                                 |
| AppointmentDate    | DateTime | Required, must be in the future when creating                               |
| DurationMinutes    | int      | Required, default 30, valid range 15–120                                    |
| Status             | enum     | Scheduled, CheckedIn, InProgress, Completed, Cancelled, NoShow             |
| Reason             | string   | Required, max length 500 — reason for visit                                 |
| Notes              | string   | Optional, max length 2000                                                   |
| CancellationReason | string   | Optional — required when status is Cancelled                                |
| CreatedAt          | DateTime | Auto-set on creation                                                        |
| UpdatedAt          | DateTime | Auto-set on every modification                                              |

**Relationships**: Belongs to Pet and Veterinarian. Has one optional MedicalRecord.

### MedicalRecord

| Field           | Type     | Constraints                                |
|-----------------|----------|--------------------------------------------|
| Id              | int      | PK, auto-generated                         |
| AppointmentId   | int      | FK → Appointment, required, unique         |
| PetId           | int      | FK → Pet, required                         |
| VeterinarianId  | int      | FK → Veterinarian, required                |
| Diagnosis       | string   | Required, max length 1000                  |
| Treatment       | string   | Required, max length 2000                  |
| Notes           | string   | Optional, max length 2000                  |
| FollowUpDate    | DateOnly | Optional                                   |
| CreatedAt       | DateTime | Auto-set on creation                       |

**Relationships**: Belongs to Appointment, Pet, and Veterinarian. Has many Prescriptions.

### Prescription

| Field            | Type     | Constraints                                           |
|------------------|----------|-------------------------------------------------------|
| Id               | int      | PK, auto-generated                                    |
| MedicalRecordId  | int      | FK → MedicalRecord, required                          |
| MedicationName   | string   | Required, max length 200                              |
| Dosage           | string   | Required, max length 100 (e.g., "10mg twice daily")  |
| DurationDays     | int      | Required, must be positive                            |
| StartDate        | DateOnly | Required                                              |
| EndDate          | DateOnly | Computed or stored — StartDate + DurationDays         |
| Instructions     | string   | Optional, max length 500                              |
| IsActive         | bool     | True if EndDate >= today                              |
| CreatedAt        | DateTime | Auto-set on creation                                  |

### Vaccination

| Field              | Type     | Constraints                                                        |
|--------------------|----------|--------------------------------------------------------------------|
| Id                 | int      | PK, auto-generated                                                 |
| PetId              | int      | FK → Pet, required                                                 |
| VaccineName        | string   | Required, max length 200 (e.g., "Rabies", "DHPP", "Bordetella")  |
| DateAdministered   | DateOnly | Required                                                           |
| ExpirationDate     | DateOnly | Required, must be after DateAdministered                           |
| BatchNumber        | string   | Optional                                                           |
| AdministeredByVetId| int      | FK → Veterinarian, required                                        |
| Notes              | string   | Optional, max length 500                                           |
| CreatedAt          | DateTime | Auto-set on creation                                               |

**Computed Properties**:
- **IsExpired**: True if ExpirationDate < today
- **IsDueSoon**: True if expiration is within 30 days from today

## Business Rules

1. **Appointment Scheduling Conflict Detection**: A veterinarian cannot have overlapping appointments. When scheduling or rescheduling, verify that the vet has no existing appointment whose time range (AppointmentDate to AppointmentDate + DurationMinutes) overlaps with the proposed time range. Only non-cancelled and non-noshow appointments count toward conflicts.

2. **Appointment Status Workflow**: Status transitions must follow these valid paths:
   - **Scheduled** → CheckedIn, Cancelled, NoShow
   - **CheckedIn** → InProgress, Cancelled
   - **InProgress** → Completed
   - **Completed**, **Cancelled**, and **NoShow** are terminal states — no further transitions allowed.

3. **Cancellation Rules**: Setting an appointment's status to Cancelled requires a `CancellationReason` to be provided. Appointments in the past (based on AppointmentDate) cannot be cancelled.

4. **Medical Record Creation**: A MedicalRecord can only be created for an appointment with status **Completed** or **InProgress**. Each appointment can have at most one medical record (enforced by the unique constraint on AppointmentId).

5. **Prescription Date Calculations**: `EndDate` is calculated as `StartDate + DurationDays`. `IsActive` is determined dynamically by checking whether `EndDate >= today`.

6. **Vaccination Due Dates**: The API must be able to return upcoming and overdue vaccinations for a pet. A vaccination is "due soon" if it expires within 30 days, and "overdue" if the expiration date has already passed.

7. **Pet Ownership**: A pet must always belong to a valid owner. Updating a pet's `OwnerId` effectively transfers ownership to a different owner.

8. **Soft Delete for Pets**: Setting `IsActive` to false marks a pet as inactive rather than deleting it. Default list queries must exclude inactive pets, but a query parameter or filter should allow retrieving inactive pets when explicitly requested.

## API Endpoints

### Owners

| Method | Route                            | Description                                              |
|--------|----------------------------------|----------------------------------------------------------|
| GET    | /api/owners                      | List all owners (support search by name, email; pagination) |
| GET    | /api/owners/{id}                 | Get owner by ID (include their pets)                     |
| POST   | /api/owners                      | Create a new owner                                       |
| PUT    | /api/owners/{id}                 | Update an existing owner                                 |
| DELETE | /api/owners/{id}                 | Delete owner (fail if owner has active pets)             |
| GET    | /api/owners/{id}/pets            | Get all pets for an owner                                |
| GET    | /api/owners/{id}/appointments    | Get appointment history for all of an owner's pets       |

### Pets

| Method | Route                                    | Description                                                      |
|--------|------------------------------------------|------------------------------------------------------------------|
| GET    | /api/pets                                | List all active pets (search by name, species; pagination; optional include inactive) |
| GET    | /api/pets/{id}                           | Get pet by ID (include owner info)                               |
| POST   | /api/pets                                | Create a new pet                                                 |
| PUT    | /api/pets/{id}                           | Update pet (including owner transfer)                            |
| DELETE | /api/pets/{id}                           | Soft-delete (set IsActive = false)                               |
| GET    | /api/pets/{id}/medical-records           | Get all medical records for a pet                                |
| GET    | /api/pets/{id}/vaccinations              | Get all vaccinations for a pet                                   |
| GET    | /api/pets/{id}/vaccinations/upcoming     | Get vaccinations that are due soon or overdue                    |

### Veterinarians

| Method | Route                                             | Description                                                  |
|--------|----------------------------------------------------|--------------------------------------------------------------|
| GET    | /api/veterinarians                                 | List all veterinarians (filter by specialization, availability) |
| GET    | /api/veterinarians/{id}                            | Get vet details                                              |
| POST   | /api/veterinarians                                 | Create a new vet                                             |
| PUT    | /api/veterinarians/{id}                            | Update vet info                                              |
| GET    | /api/veterinarians/{id}/schedule?date={date}       | Get vet's appointments for a specific date                   |
| GET    | /api/veterinarians/{id}/appointments               | Get all appointments for a vet (pagination, filter by status)|

### Appointments

| Method | Route                                  | Description                                                            |
|--------|----------------------------------------|------------------------------------------------------------------------|
| GET    | /api/appointments                      | List appointments (filter by date range, status, vet, pet; pagination) |
| GET    | /api/appointments/{id}                 | Get appointment details (include pet, vet, medical record)             |
| POST   | /api/appointments                      | Schedule a new appointment (enforce conflict detection)                |
| PUT    | /api/appointments/{id}                 | Update appointment details (date/time changes re-check conflicts)      |
| PATCH  | /api/appointments/{id}/status          | Update appointment status (enforce workflow rules)                     |
| GET    | /api/appointments/today                | Get all of today's appointments                                        |

### Medical Records

| Method | Route                        | Description                                                  |
|--------|------------------------------|--------------------------------------------------------------|
| GET    | /api/medical-records/{id}    | Get medical record with prescriptions                        |
| POST   | /api/medical-records         | Create medical record (enforce appointment status rule)      |
| PUT    | /api/medical-records/{id}    | Update medical record                                        |

### Prescriptions

| Method | Route                                  | Description                              |
|--------|----------------------------------------|------------------------------------------|
| GET    | /api/prescriptions/{id}               | Get prescription details                 |
| POST   | /api/prescriptions                     | Create prescription for a medical record |
| GET    | /api/pets/{id}/prescriptions/active    | Get active prescriptions for a pet       |

### Vaccinations

| Method | Route                     | Description                   |
|--------|---------------------------|-------------------------------|
| POST   | /api/vaccinations         | Record a new vaccination      |
| GET    | /api/vaccinations/{id}    | Get vaccination details       |

## Seed Data

The application **must** seed the database on startup with realistic dummy data for demo and testing purposes. Include at minimum:

- **5 owners** with realistic names, emails, phone numbers, and addresses
- **8 pets** across those owners (a mix of dogs, cats, and at least one other species with realistic breed names)
- **3 veterinarians** with different specializations
- **10 appointments** in various statuses (some completed, some scheduled in the future, at least one cancelled)
- **4 medical records** linked to completed appointments
- **5 prescriptions** across those medical records (mix of active and expired)
- **6 vaccinations** (some current, some expiring soon, some expired)

Use the EF Core seeding mechanism or a data seeder service. Ensure the seed data only runs when the database is empty to avoid duplicates on subsequent startups.

## HTTP File

Create a `.http` file (e.g., `VetClinicApi.http`) in the same folder as `Program.cs`. This file must:

- Define a `@baseUrl` variable set to `http://localhost:{{port}}` (use whichever port the application is configured to listen on)
- Include sample requests for **all** endpoints listed above
- Group requests by resource with comment headers (e.g., `### Owners`, `### Pets`, etc.)
- Include realistic request bodies for all POST and PUT operations
- Include query parameter examples for search and filter endpoints
- Use IDs that correspond to the seed data so that requests work out of the box against a freshly seeded database

## Cross-Cutting Concerns

### Error Handling
Implement a global exception handler that returns **RFC 7807 ProblemDetails** responses. Business rule violations (e.g., scheduling conflicts, invalid status transitions) should return **400 Bad Request** or **409 Conflict** with descriptive error messages.

### Validation
Validate all input DTOs. Return **400 Bad Request** with structured validation error details when input is invalid.

### Logging
Use the built-in `ILogger`. Log key operations (appointment created, status changed, medical record created, etc.) at **Information** level. Log errors and exceptions at **Error** level.

### OpenAPI / Swagger
Configure Swagger UI to be accessible at the root or `/swagger`. Include descriptive operation summaries and document response types for all endpoints.

### Pagination
Use a consistent pagination pattern across all list endpoints. Accept `page` (page number) and `pageSize` (items per page) query parameters with sensible defaults. Return pagination metadata (total count, total pages, current page) in responses.

## Project Location

Create the project at: `./src/VetClinicApi/`

The project must be a standalone ASP.NET Core Web API project with no dependencies on other projects in this repository.
