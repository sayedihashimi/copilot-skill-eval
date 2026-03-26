# FitnessStudioApi - Generation Notes

## Overview
A complete ASP.NET Core Web API for **Zenith Fitness Studio**, implementing a booking/membership system with class scheduling, waitlists, and instructor management.

## Technical Stack
- **Framework**: ASP.NET Core Web API (.NET 10)
- **Database**: Entity Framework Core with SQLite
- **Documentation**: Swagger/OpenAPI via Swashbuckle
- **Architecture**: Controllers → Services (Interface + Implementation) → EF Core DbContext

## Project Structure
```
src/FitnessStudioApi/
├── Controllers/          # 7 API controllers
│   ├── BookingsController.cs
│   ├── ClassSchedulesController.cs
│   ├── ClassTypesController.cs
│   ├── InstructorsController.cs
│   ├── MembersController.cs
│   ├── MembershipsController.cs
│   └── MembershipPlansController.cs
├── Data/
│   ├── FitnessDbContext.cs    # EF Core context with relationships
│   └── DataSeeder.cs          # Seed data for demo/testing
├── DTOs/
│   └── Dtos.cs                # All request/response DTOs
├── Middleware/
│   └── GlobalExceptionHandlerMiddleware.cs  # RFC 7807 ProblemDetails
├── Models/
│   ├── Enums.cs               # Status enums
│   ├── Booking.cs
│   ├── ClassSchedule.cs
│   ├── ClassType.cs
│   ├── Instructor.cs
│   ├── Member.cs
│   ├── Membership.cs
│   └── MembershipPlan.cs
├── Services/
│   ├── Interfaces.cs          # All service interfaces
│   ├── BookingService.cs
│   ├── ClassScheduleService.cs
│   ├── ClassTypeService.cs
│   ├── InstructorService.cs
│   ├── MemberService.cs
│   ├── MembershipPlanService.cs
│   └── MembershipService.cs
├── Program.cs
├── appsettings.json
└── FitnessStudioApi.http      # Sample HTTP requests for all endpoints
```

## Entities
- **MembershipPlan** – Basic ($29.99), Premium ($49.99), Elite ($79.99)
- **Member** – 8 seeded members with realistic data
- **Membership** – Links members to plans with status tracking (Active/Expired/Cancelled/Frozen)
- **Instructor** – 4 instructors with specializations
- **ClassType** – 6 types (Yoga, HIIT, Spin, Pilates, Boxing, Meditation); Boxing & Meditation are premium
- **ClassSchedule** – 12 scheduled classes across 6 days; includes full/cancelled scenarios
- **Booking** – 15+ bookings in various states (Confirmed, Waitlisted, Cancelled)

## Business Rules Implemented
1. Booking window: 7 days advance, ≥30 min before class
2. Capacity management with automatic waitlist promotion
3. Cancellation policy (free >2hrs, late <2hrs)
4. Membership tier access (premium classes restricted)
5. Weekly booking limits per plan
6. Active membership required for booking
7. No double-booking (time overlap check)
8. Instructor schedule conflict prevention
9. Membership freeze/unfreeze with end-date extension
10. Class cancellation cascades to all bookings
11. Check-in window (15 min before to 15 min after)
12. No-show flagging (after 15 min past start)

## API Endpoints (30+ endpoints)
- Membership Plans: CRUD + deactivate
- Members: CRUD + bookings/memberships history
- Memberships: Create, cancel, freeze, unfreeze, renew
- Instructors: CRUD + schedule view
- Class Types: CRUD with filters
- Class Schedules: CRUD + cancel + roster/waitlist + available
- Bookings: Create, cancel, check-in, no-show

## Seed Data
Database is automatically seeded on first run with comprehensive demo data including members, plans, instructors, class types, schedules (some full, some cancelled), and bookings in all states.
