# KeystoneProperties — Generation Notes

## What Was Generated

A full-featured **Residential Property Management** web application built with **ASP.NET Core Razor Pages** targeting **.NET 10**, using **Entity Framework Core with SQLite** and **Bootstrap 5** styling.

## Project Structure

```
src/KeystoneProperties/
├── Models/              # Entity classes (Property, Unit, Tenant, Lease, Payment, MaintenanceRequest, Inspection)
│   └── Enums/           # 12 enum types (PropertyType, UnitStatus, LeaseStatus, etc.)
├── Data/
│   ├── AppDbContext.cs  # EF Core DbContext with relationships and timestamp tracking
│   └── DataSeeder.cs    # Realistic seed data (3 properties, 15 units, 10 tenants, 13 leases, 20+ payments, 8 maintenance requests, 5 inspections)
├── Services/
│   ├── Interfaces/      # Service interfaces with PaginatedList<T>, DashboardStats, OverdueLeaseInfo
│   ├── PropertyService, UnitService, TenantService, LeaseService
│   ├── PaymentService, MaintenanceService, InspectionService
│   └── DashboardService
├── Pages/
│   ├── Index (Dashboard)
│   ├── Properties/      # List, Details, Create, Edit, Deactivate
│   ├── Units/           # List, Details, Create, Edit
│   ├── Tenants/         # List, Details, Create, Edit, Deactivate
│   ├── Leases/          # List, Details, Create, Edit, Terminate, Renew
│   ├── Payments/        # List, Details, Create, Overdue
│   ├── Maintenance/     # List, Details, Create, UpdateStatus
│   ├── Inspections/     # List, Details, Create, Complete
│   └── Shared/          # _Layout, _StatusBadgePartial, _PaginationPartial
└── Program.cs           # DI configuration, middleware, database seeding
```

## Key Features Implemented

- **Dashboard** with occupancy rate, rent collected, overdue payments count, open maintenance requests, and upcoming lease expirations
- **Property management** with CRUD, deactivation guard (no active leases), occupancy tracking
- **Unit management** with filtering by property, status, bedrooms, rent range; unique (PropertyId, UnitNumber) constraint
- **Tenant management** with age validation (18+), deactivation guard (no active leases), search by name/email
- **Lease management** with overlap validation, status workflow (Pending→Active→Expired/Renewed/Terminated), renewal chains, deposit tracking
- **Payment recording** with automatic late fee calculation ($50 + $5/day, capped at $200), overdue payment tracking
- **Maintenance requests** with priority-based workflow, emergency handling (auto-assign, unit status change), status transitions
- **Inspection scheduling** with completion workflow, condition assessment, follow-up tracking

## Business Rules Implemented

1. No overlapping Active/Pending leases for the same unit
2. Unit status syncs with lease lifecycle (Occupied/Available)
3. Lease status workflow with proper transition guards
4. Late fee auto-calculation (>5 days past due)
5. Lease renewal creates new lease, marks original as Renewed
6. Maintenance request workflow with valid state transitions
7. Emergency maintenance requires AssignedTo, changes unit to Maintenance status
8. Tenant deactivation blocked by active leases
9. Deposit tracking with DepositReturn payment on termination
10. Property deactivation blocked by active leases

## Cross-Cutting Concerns

- **Validation**: Data Annotations on all input models, inline validation with `asp-validation-for`
- **Error Handling**: Global exception handler, TempData flash messages (success/error), PRG pattern
- **Pagination**: Inline pagination on all list pages with Bootstrap controls
- **Status Badges**: Reusable `_StatusBadgePartial` with color-coded Bootstrap badges
- **Navigation**: Bootstrap 5 dark navbar with `aria-current` for active page
- **Semantic HTML**: `<nav>`, `<main>`, `<section>`, `<table>` with `<thead>`/`<tbody>`, `role="alert"` on alerts
- **Logging**: ILogger used for key operations (lease created, payment recorded, etc.)

## Technology Stack

- ASP.NET Core Razor Pages (.NET 10)
- Entity Framework Core with SQLite
- Bootstrap 5 (default theme)
- jQuery Validation Unobtrusive (client-side validation)

## How to Run

```bash
cd src/KeystoneProperties
dotnet run
```

The database is automatically created and seeded on first run.
