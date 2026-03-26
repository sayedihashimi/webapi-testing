# Keystone Properties — Generation Notes

## Summary

A complete residential property management web application built with ASP.NET Core Razor Pages targeting .NET 10, using Entity Framework Core with SQLite.

## What Was Generated

### Models & Enums (7 entities, 12 enums)
- **Property**, **Unit**, **Tenant**, **Lease**, **Payment**, **MaintenanceRequest**, **Inspection**
- Enums: PropertyType, UnitStatus, LeaseStatus, DepositStatus, PaymentMethod, PaymentType, PaymentStatus, MaintenancePriority, MaintenanceStatus, MaintenanceCategory, InspectionType, OverallCondition

### Data Layer
- `ApplicationDbContext` with full EF Core configuration (unique constraints, foreign keys, decimal precision, auto-timestamps)
- `DataSeeder` with realistic seed data: 3 properties, 17 units, 11 tenants, 15 leases (including renewal chain, terminated, pending), 27+ payments, 8 maintenance requests, 6 inspections

### Services (8 service interfaces + implementations)
- `IPropertyService` / `PropertyService` — CRUD, deactivation with active lease check
- `IUnitService` / `UnitService` — CRUD, filtering, available units
- `ITenantService` / `TenantService` — CRUD, deactivation with active lease check, email uniqueness
- `ILeaseService` / `LeaseService` — CRUD, overlap validation, terminate, renew (with unit status sync)
- `IPaymentService` / `PaymentService` — Record payments, late fee auto-calculation ($50 + $5/day, capped at $200), overdue detection
- `IMaintenanceService` / `MaintenanceService` — Status workflow enforcement, emergency handling
- `IInspectionService` / `InspectionService` — Schedule and complete inspections
- `IDashboardService` / `DashboardService` — Aggregated dashboard metrics

### Pages (30+ Razor Pages across 8 feature areas)
- **Dashboard** (`/`) — Overview with metrics cards and upcoming lease expirations
- **Properties** — Index (search/filter), Details, Create, Edit, Deactivate
- **Units** — Index (filter by property/status/bedrooms/rent), Details, Create, Edit
- **Tenants** — Index (search/filter), Details, Create, Edit, Deactivate
- **Leases** — Index (filter), Details, Create, Edit, Terminate, Renew
- **Payments** — Index (filter/sort), Details, Create (with auto late fee), Overdue list
- **Maintenance** — Index (filter), Details, Create, UpdateStatus
- **Inspections** — Index (filter), Details, Create, Complete

### Shared Components
- `_Layout.cshtml` — Bootstrap 5 dark navbar with all navigation links, TempData alerts, footer
- `_PaginationPartial.cshtml` — Reusable pagination control
- `_StatusBadge.cshtml` — Reusable color-coded status badge partial (supports unit, lease, payment, maintenance, priority, condition, deposit types)

### Business Rules Implemented
1. No overlapping leases (validated on create/edit)
2. Unit status sync (Occupied ↔ Available based on lease status)
3. Lease status workflow (Pending → Active → Expired/Renewed/Terminated)
4. Late fee calculation ($50 base + $5/day after 5-day grace, capped at $200)
5. Lease renewal creates new lease with RenewalOfLeaseId link
6. Maintenance request workflow with valid state transitions
7. Emergency maintenance requires assignment and sets unit to Maintenance status
8. Tenant deactivation blocked by active leases
9. Deposit tracking with DepositReturn payment on termination
10. Property deactivation blocked by active leases

### Cross-Cutting Concerns
- Data Annotations validation with inline error display
- Post-Redirect-Get pattern on all forms
- TempData flash messages (success/error)
- InputModel pattern (dedicated DTOs for form binding)
- ILogger integration for key operations
- Global error handling via `/Error` page
- Bootstrap 5 styling throughout
- Semantic HTML with aria attributes

## How to Run

```bash
cd src/KeystoneProperties
dotnet run
```

The database is auto-created and seeded on first run.
