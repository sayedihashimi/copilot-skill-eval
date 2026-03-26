# SparkEvents — Generation Notes

## What Was Generated

A full-featured **Community Event Registration Portal** built with ASP.NET Core Razor Pages targeting .NET 10 with Entity Framework Core and SQLite.

### Architecture

- **Models** (8 files): EventCategory, Venue, Event, TicketType, Attendee, Registration, CheckIn, plus EventStatus and RegistrationStatus enums
- **Data** (2 files): SparkEventsDbContext with full relationship configuration; DataSeeder with realistic demo data
- **Services** (12 files): 6 service interfaces + 6 implementations covering categories, venues, events, attendees, registrations, and check-in
- **Pages** (40+ files): Complete Razor Pages organized by feature area

### Feature Areas

| Area | Pages | Key Features |
|------|-------|-------------|
| Dashboard | 1 | Quick stats, today's events with check-in progress bars, upcoming events, recent registrations |
| Categories | 4 | CRUD with delete protection (prevents deleting categories with events) |
| Venues | 5 | CRUD + details page showing upcoming events at venue |
| Events | 12 | List (filterable/paginated), details, create, edit, cancel, complete, ticket type management, registration, roster, waitlist, check-in dashboard, check-in processing |
| Attendees | 4 | CRUD with search, details page showing registration history |
| Registrations | 2 | Details with full info, cancellation with policy enforcement |

### Business Rules Implemented

- Registration window enforcement (open/close dates)
- Capacity management with automatic sold-out status transitions
- Ticket type capacity tracking
- Early-bird pricing (automatic price selection based on deadline)
- Duplicate registration prevention
- 24-hour cancellation policy
- Waitlist with automatic promotion on cancellation
- Event cancellation cascade (cancels all registrations)
- Event status workflow (Draft → Published → SoldOut/Completed/Cancelled)
- Check-in window enforcement (StartDate - 1 hour to EndDate)
- Venue capacity constraint validation
- Confirmation number format: SPK-YYYYMMDD-NNNN

### Cross-Cutting Concerns

- **Bootstrap 5** styling throughout with responsive layout
- **Reusable partial views**: Status badge component (`_StatusBadge.cshtml`), Pagination component (`_Pagination.cshtml`)
- **Data annotations** validation with client-side jQuery Validation Unobtrusive
- **TempData** flash messages for success/error notifications
- **Post-Redirect-Get** pattern on all form submissions
- **Input models** (nested InputModel classes) for form binding
- **Named handlers** for pages with multiple actions
- **Semantic HTML** with proper `<nav>`, `<main>`, `<section>`, ARIA labels
- **ILogger** integration in services

### Seed Data

- 4 event categories (Technology, Business, Creative Arts, Health & Wellness)
- 3 venues with varying capacities (50, 200, 500)
- 6 events in different states (2 Published, 1 SoldOut, 1 today, 1 Completed, 1 Draft)
- 3 ticket types per event with early-bird pricing
- 12 attendees with realistic data
- 20 registrations across all statuses
- 5 check-ins

### How to Run

```bash
cd src/SparkEvents
dotnet run
```

The application will create and seed the SQLite database on first startup.
