---
mode: agent
description: "Create a Community Library Management API using ASP.NET Core Web API and .NET 10"
tools: ["changes", "codebase", "fetch", "findTestFiles", "githubRepo", "problems", "runner", "selection", "terminalLastCommand", "terminalSelection", "usages", "visionScreenshot"]
---

# Community Library Management API — Sunrise Community Library

## App Overview

Build a **community library management system API** for **"Sunrise Community Library"**. This API manages books, authors, categories, library patrons, book loans, reservations, and fines. The system helps librarians manage the book collection, handle borrowing and returns, track overdue items, manage a reservation queue, and enforce borrowing policies.

The API is the backend for a library system used by library staff. It does not include a frontend — it exposes a RESTful JSON API consumed by client applications.

## Technical Requirements

- **Framework**: ASP.NET Core Web API targeting **.NET 10**
- **ORM & Database**: Entity Framework Core with **SQLite**. The connection string should be configured in `appsettings.json`, and the database file should live in the project directory.
- **Authentication**: None. No authentication or authorization is required.
- **API Documentation**: OpenAPI/Swagger documentation must be enabled and accessible.
- **Project Structure**: Use a well-organized project with clear separation of concerns:
  - **Models** — Entity/domain classes and enums
  - **DTOs** — Request and response data transfer objects (never expose raw entities directly in API responses)
  - **Services** — Business logic behind interfaces (interface + implementation pattern)
- **Validation**: Use Data Annotations and/or FluentValidation for all input validation.
- **Error Handling**: Global error handling middleware that returns consistent **ProblemDetails** responses (RFC 7807).
- **Project Location**: Create the project at `./src/LibraryApi/`

## Entities & Relationships

### Author

| Field       | Type     | Constraints                          |
|-------------|----------|--------------------------------------|
| Id          | int      | Primary key, auto-generated          |
| FirstName   | string   | Required, max length 100             |
| LastName    | string   | Required, max length 100             |
| Biography   | string   | Optional, max length 2000            |
| BirthDate   | DateOnly | Optional                             |
| Country     | string   | Optional, max length 100             |
| CreatedAt   | DateTime | Auto-set on creation                 |

**Relationships**: An Author has many Books through a many-to-many relationship (via a `BookAuthor` join entity).

### Category

| Field       | Type   | Constraints                            |
|-------------|--------|----------------------------------------|
| Id          | int    | Primary key, auto-generated            |
| Name        | string | Required, max length 100, unique       |
| Description | string | Optional, max length 500               |

**Relationships**: A Category has many Books through a many-to-many relationship (via a `BookCategory` join entity).

### Book

| Field           | Type     | Constraints                                                        |
|-----------------|----------|--------------------------------------------------------------------|
| Id              | int      | Primary key, auto-generated                                        |
| Title           | string   | Required, max length 300                                           |
| ISBN            | string   | Required, unique, must be a valid ISBN format                      |
| Publisher       | string   | Optional, max length 200                                           |
| PublicationYear | int      | Optional                                                           |
| Description     | string   | Optional, max length 2000                                          |
| PageCount       | int      | Optional, must be positive if provided                             |
| Language        | string   | Optional, default "English"                                        |
| TotalCopies     | int      | Required, must be >= 1                                             |
| AvailableCopies | int      | Computed or tracked — equals TotalCopies minus currently checked-out copies |
| CreatedAt       | DateTime | Auto-set on creation                                               |
| UpdatedAt       | DateTime | Auto-set on creation and update                                    |

**Relationships**:
- Has many Authors (many-to-many via `BookAuthor`)
- Has many Categories (many-to-many via `BookCategory`)
- Has many Loans
- Has many Reservations

### Patron

| Field          | Type           | Constraints                                              |
|----------------|----------------|----------------------------------------------------------|
| Id             | int            | Primary key, auto-generated                              |
| FirstName      | string         | Required, max length 100                                 |
| LastName       | string         | Required, max length 100                                 |
| Email          | string         | Required, unique, valid email format                     |
| Phone          | string         | Optional                                                 |
| Address        | string         | Optional                                                 |
| MembershipDate | DateOnly       | Required, auto-set to today on creation                  |
| MembershipType | enum           | Standard, Premium, or Student — affects borrowing limits |
| IsActive       | bool           | Default true                                             |
| CreatedAt      | DateTime       | Auto-set on creation                                     |
| UpdatedAt      | DateTime       | Auto-set on creation and update                          |

**Relationships**:
- Has many Loans
- Has many Reservations
- Has many Fines

### Loan

| Field        | Type     | Constraints                                                                 |
|--------------|----------|-----------------------------------------------------------------------------|
| Id           | int      | Primary key, auto-generated                                                 |
| BookId       | int      | Foreign key → Book, required                                                |
| PatronId     | int      | Foreign key → Patron, required                                              |
| LoanDate     | DateTime | Required, auto-set to now on creation                                       |
| DueDate      | DateTime | Required, calculated as LoanDate + loan period based on patron's membership type |
| ReturnDate   | DateTime | Nullable — set when the book is returned                                    |
| Status       | enum     | Active, Returned, or Overdue                                                |
| RenewalCount | int      | Default 0, maximum of 2 renewals allowed                                    |
| CreatedAt    | DateTime | Auto-set on creation                                                        |

**Relationships**:
- Belongs to a Book
- Belongs to a Patron

### Reservation

| Field           | Type     | Constraints                                                                  |
|-----------------|----------|------------------------------------------------------------------------------|
| Id              | int      | Primary key, auto-generated                                                  |
| BookId          | int      | Foreign key → Book, required                                                 |
| PatronId        | int      | Foreign key → Patron, required                                               |
| ReservationDate | DateTime | Auto-set to now on creation                                                  |
| ExpirationDate  | DateTime | Set when the book becomes available; patron has 3 days to pick it up         |
| Status          | enum     | Pending, Ready, Fulfilled, Cancelled, or Expired                             |
| QueuePosition   | int      | Position in the reservation queue for this book                              |
| CreatedAt       | DateTime | Auto-set on creation                                                         |

**Relationships**:
- Belongs to a Book
- Belongs to a Patron

### Fine

| Field      | Type     | Constraints                                        |
|------------|----------|----------------------------------------------------|
| Id         | int      | Primary key, auto-generated                        |
| PatronId   | int      | Foreign key → Patron, required                     |
| LoanId     | int      | Foreign key → Loan, required                       |
| Amount     | decimal  | Required, must be positive                         |
| Reason     | string   | Required (e.g., "Overdue return", "Damaged book")  |
| IssuedDate | DateTime | Auto-set on creation                               |
| PaidDate   | DateTime | Nullable — set when the fine is paid               |
| Status     | enum     | Unpaid, Paid, or Waived                            |
| CreatedAt  | DateTime | Auto-set on creation                               |

**Relationships**:
- Belongs to a Patron
- Belongs to a Loan

## Business Rules

### 1. Borrowing Limits by Membership Type

| Membership Type | Max Active Loans | Loan Period |
|-----------------|------------------|-------------|
| Standard        | 5                | 14 days     |
| Premium         | 10               | 21 days     |
| Student         | 3                | 7 days      |

### 2. Checkout Rules

When a patron attempts to check out a book, enforce ALL of the following:

- The book must have at least 1 available copy (`AvailableCopies > 0`).
- The patron must not have unpaid fines totaling **$10.00 or more**.
- The patron must not have exceeded their membership type's borrowing limit (active loans count < max active loans).
- The patron's membership must be active (`IsActive = true`).

If any rule is violated, reject the checkout with a descriptive error message.

### 3. Return Processing

When a book is returned:

1. Set the `ReturnDate` on the loan to now and update the loan status to **Returned**.
2. Increment the book's `AvailableCopies`.
3. If the book is returned **after** its `DueDate`, automatically generate a **Fine** of **$0.25 per day overdue**.
4. After the return, check if there are **pending reservations** for the book. If so, transition the first reservation in the queue to **"Ready"** status and set its `ExpirationDate` to 3 days from now.

### 4. Overdue Detection

- Any loan past its `DueDate` that has no `ReturnDate` should be considered **Overdue**.
- Provide an endpoint or background mechanism to detect and flag overdue loans (update their status to Overdue).

### 5. Reservation Queue

- A patron can reserve a book even if copies are currently available — the reservation goes to the back of the queue.
- When a copy becomes available (via return) and there are pending reservations, the **first reservation** in the queue transitions to **"Ready"** status.
- If a "Ready" reservation is not fulfilled within **3 days** (its `ExpirationDate` passes), it transitions to **"Expired"** and the **next** reservation in the queue is moved to "Ready".
- A patron **cannot** reserve a book they already have on an active loan.

### 6. Renewals

- A loan can be renewed a maximum of **2 times** (`RenewalCount` tracks this).
- Renewing a loan extends the `DueDate` by the patron's membership type loan period, calculated from **today** (not from the current due date).
- A loan **cannot** be renewed if there are pending reservations for the book.
- A loan **cannot** be renewed if it is already overdue.

### 7. Fine Threshold

- Patrons with **$10.00 or more** in total unpaid fines are blocked from checking out or renewing books.

### 8. Book Availability

- `AvailableCopies` must always be kept accurate:
  - **Decremented** when a book is checked out.
  - **Incremented** when a book is returned.

## API Endpoints

### Authors

| Method | Endpoint             | Description                                          |
|--------|----------------------|------------------------------------------------------|
| GET    | `/api/authors`       | List authors with search by name and pagination      |
| GET    | `/api/authors/{id}`  | Get author details including their books             |
| POST   | `/api/authors`       | Create a new author                                  |
| PUT    | `/api/authors/{id}`  | Update an existing author                            |
| DELETE | `/api/authors/{id}`  | Delete an author (fail if the author has any books)  |

### Categories

| Method | Endpoint                | Description                                              |
|--------|-------------------------|----------------------------------------------------------|
| GET    | `/api/categories`       | List all categories                                      |
| GET    | `/api/categories/{id}`  | Get category details with count of books in the category |
| POST   | `/api/categories`       | Create a new category                                    |
| PUT    | `/api/categories/{id}`  | Update an existing category                              |
| DELETE | `/api/categories/{id}`  | Delete a category (fail if category has any books)       |

### Books

| Method | Endpoint                       | Description                                                                                   |
|--------|--------------------------------|-----------------------------------------------------------------------------------------------|
| GET    | `/api/books`                   | List books with search (by title, author, ISBN, category), filter by availability, pagination, sorting |
| GET    | `/api/books/{id}`              | Get book details including authors, categories, and availability info                         |
| POST   | `/api/books`                   | Create a new book (accepts author IDs and category IDs in the request body)                   |
| PUT    | `/api/books/{id}`              | Update an existing book                                                                       |
| DELETE | `/api/books/{id}`              | Delete a book (fail if the book has any active loans)                                         |
| GET    | `/api/books/{id}/loans`        | Get the loan history for a specific book                                                      |
| GET    | `/api/books/{id}/reservations` | Get the active reservations queue for a specific book                                         |

### Patrons

| Method | Endpoint                         | Description                                                                        |
|--------|----------------------------------|------------------------------------------------------------------------------------|
| GET    | `/api/patrons`                   | List patrons with search (by name, email), filter by membership type, pagination   |
| GET    | `/api/patrons/{id}`              | Get patron details with summary (active loans count, total unpaid fines balance)   |
| POST   | `/api/patrons`                   | Create a new patron                                                                |
| PUT    | `/api/patrons/{id}`              | Update an existing patron                                                          |
| DELETE | `/api/patrons/{id}`              | Deactivate patron (set IsActive = false; fail if patron has active loans)          |
| GET    | `/api/patrons/{id}/loans`        | Get patron's loans (filter by status: active, returned, overdue)                   |
| GET    | `/api/patrons/{id}/reservations` | Get patron's reservations                                                          |
| GET    | `/api/patrons/{id}/fines`        | Get patron's fines (filter by status: unpaid, paid, waived)                        |

### Loans

| Method | Endpoint                  | Description                                                        |
|--------|---------------------------|--------------------------------------------------------------------|
| GET    | `/api/loans`              | List loans with filter by status, overdue flag, date range; pagination |
| GET    | `/api/loans/{id}`         | Get loan details                                                   |
| POST   | `/api/loans`              | Check out a book — create a loan enforcing all checkout rules      |
| POST   | `/api/loans/{id}/return`  | Return a book — enforce all return processing rules                |
| POST   | `/api/loans/{id}/renew`   | Renew a loan — enforce all renewal rules                           |
| GET    | `/api/loans/overdue`      | Get all currently overdue loans                                    |

### Reservations

| Method | Endpoint                          | Description                                                      |
|--------|-----------------------------------|------------------------------------------------------------------|
| GET    | `/api/reservations`               | List reservations with filter by status and pagination           |
| GET    | `/api/reservations/{id}`          | Get reservation details                                          |
| POST   | `/api/reservations`               | Create a reservation enforcing all reservation rules             |
| POST   | `/api/reservations/{id}/cancel`   | Cancel a reservation                                             |
| POST   | `/api/reservations/{id}/fulfill`  | Fulfill a "Ready" reservation (creates a loan for the patron)    |

### Fines

| Method | Endpoint                 | Description                    |
|--------|--------------------------|--------------------------------|
| GET    | `/api/fines`             | List fines with filter by status and pagination |
| GET    | `/api/fines/{id}`        | Get fine details               |
| POST   | `/api/fines/{id}/pay`    | Pay a fine (set PaidDate, update status to Paid) |
| POST   | `/api/fines/{id}/waive`  | Waive a fine (update status to Waived)           |

## Seed Data

The application **MUST** seed the database on startup with realistic dummy data for demo and testing purposes. The seed data should include:

- **At least 5 authors** with realistic names, biographies, birth dates, and countries.
- **At least 5 categories** (e.g., Fiction, Science Fiction, History, Science, Biography).
- **At least 12 books** spread across multiple authors and categories. Some books should have multiple copies, some should have only 1 copy. Some should be fully available, some should be partially checked out.
- **At least 6 patrons** with a mix of membership types (Standard, Premium, Student). Include a variety of active and at least one inactive patron.
- **At least 8 loans** in various states: some Active, some Returned, some Overdue.
- **At least 3 reservations**: some Pending, at least one Ready.
- **At least 3 fines**: some Unpaid, some Paid.

Use the EF Core seeding mechanism or a dedicated data seeder service. Ensure the seed data **only runs when the database is empty** to avoid duplicates on subsequent application starts.

The seed data should be internally consistent — `AvailableCopies` values should match the number of active loans against each book, fine amounts should correspond to overdue days, reservation queue positions should be sequential, etc.

## HTTP File

Create a `.http` file (e.g., `LibraryApi.http`) in the **same folder as `Program.cs`**. This file should:

- Define a `@baseUrl` variable set to `http://localhost:{{port}}` (use whatever port the app is configured to run on).
- Include **sample requests for ALL endpoints** listed above.
- **Group requests by resource** with comment section headers (e.g., `### Authors`, `### Books`, `### Patrons`, `### Loans`, `### Reservations`, `### Fines`).
- Include **realistic request bodies** for all POST and PUT operations.
- Include **query parameter examples** for search, filter, and pagination endpoints.
- Use **IDs that correspond to the seed data** so that requests work out of the box against a freshly seeded database.
- Include **examples that test business rules**, such as:
  - Attempting to check out a book for a patron who has too many unpaid fines.
  - Attempting to renew a loan when there are pending reservations for the book.
  - Attempting to check out when a patron has exceeded their borrowing limit.
  - Attempting to reserve a book the patron already has on active loan.

## Cross-Cutting Concerns

### Error Handling
- Implement a **global exception handler** middleware that catches unhandled exceptions and returns RFC 7807 **ProblemDetails** responses.
- Business rule violations (e.g., checkout denied, renewal denied) should return **400 Bad Request** or **409 Conflict** with a descriptive error message in the ProblemDetails body.
- Not-found scenarios should return **404 Not Found**.

### Validation
- Validate **all input DTOs** using Data Annotations and/or FluentValidation.
- Return **400 Bad Request** with detailed validation error information when input is invalid.

### Logging
- Use the built-in `ILogger` throughout the application.
- Log key business operations at **Information** level: book checked out, book returned, fine issued, reservation status change, etc.
- Log errors and exceptions at **Error** level.

### OpenAPI / Swagger
- Configure Swagger UI to be accessible at the root (`/`) or `/swagger`.
- Include descriptive **operation summaries** and **response type annotations** on all endpoints so the Swagger documentation is useful and complete.

### Pagination
- Use a **consistent pagination pattern** across all list endpoints.
- Accept `page` (page number, default 1) and `pageSize` (items per page, default 10) query parameters.
- Return pagination metadata in the response (total count, current page, page size, total pages).

## Project Location

Create the project at: `./src/LibraryApi/`

The project should be a **standalone ASP.NET Core Web API project** with no dependencies on other projects in this repository. It should be fully self-contained and runnable with `dotnet run`.
