# FitnessStudioApi — Generation Notes

## Skills Used

### 1. `analyzing-dotnet-performance`

**What it does:** Scans .NET code for ~50 performance anti-patterns across async, memory, strings, collections, LINQ, regex, serialization, and I/O with tiered severity classification.

**How it influenced the code:**

- **Sealed all classes** — The scan found 0 of 60 classes were sealed. All non-inherited classes were sealed for JIT devirtualization benefits (services, controllers, DTOs, validators, models, middleware).
  - Exception: `MemberDto` left unsealed as it's the base class for `MemberDetailDto`; `FitnessDbContext` left unsealed as EF Core requires it.

- **Replaced `.ToLower().Contains()` with `EF.Functions.Like()`** — Found 5 instances of `.ToLower()` in EF Core LINQ queries across `MemberService` (search by name/email) and `InstructorService` (filter by specialization). These were replaced with `EF.Functions.Like()` which:
  - Translates to native SQL `LIKE` (SQLite is case-insensitive for ASCII by default)
  - Avoids string allocation from `.ToLower()` on every row evaluation
  - Produces more efficient SQL without per-row function calls

### 2. `optimizing-ef-core-queries`

**What it does:** Optimizes Entity Framework Core queries by fixing N+1 problems, choosing correct tracking modes, using compiled queries, and avoiding common performance traps.

**How it influenced the code:**

- **Replaced `Include()` with projection in `MemberService.GetByIdAsync()`** — The original code used `.Include(m => m.Memberships).ThenInclude(...).Include(m => m.Bookings)` to load ALL membership and booking records into memory, then counted them in C#. This was replaced with a `.Select()` projection that computes `TotalBookings`, `AttendedClasses`, and `ActiveMembership` at the database level. This:
  - Avoids loading potentially hundreds of booking records into memory
  - Computes counts via SQL `COUNT()` instead of in-memory LINQ
  - Reduces data transfer from the database

- **Validated existing patterns** — The scan confirmed several good practices were already in place:
  - ✅ `AsNoTracking()` used consistently on all read-only queries (19 instances across all services)
  - ✅ `Include()` used properly to prevent N+1 problems on queries that need related data
  - ✅ `FindAsync()` used for primary-key lookups (leverages identity map cache)
  - ✅ Tracked queries used correctly where entities are modified (cancel, freeze, etc.)
  - ✅ Pagination with `Skip()`/`Take()` prevents loading entire tables

## Project Architecture

- **Framework:** ASP.NET Core Web API, .NET 10
- **Database:** EF Core with SQLite
- **Validation:** FluentValidation with auto-validation
- **Error Handling:** `IExceptionHandler` with RFC 7807 `ProblemDetails`
- **Structure:** Models → DTOs → Services (interface + implementation) → Controllers
- **Seed Data:** 3 plans, 8 members, 9 memberships, 4 instructors, 6 class types, 12 schedules, 20 bookings

## Business Rules Implemented

All 12 business rules from the spec are implemented in `BookingService` and `MembershipService`:
1. Booking window (7 days advance, 30 min minimum)
2. Capacity management with automatic waitlist promotion
3. Cancellation policy (2-hour late cancellation marking)
4. Membership tier access (premium class blocking for Basic members)
5. Weekly booking limits per plan
6. Active membership requirement
7. No double-booking (time overlap detection)
8. Instructor schedule conflict detection
9. Membership freeze (7-30 days, once per term, extends end date)
10. Class cancellation cascade
11. Check-in window (15 min before/after)
12. No-show flagging (after 15 min past start)
