# HorizonHR - Generation Notes

## Summary

HorizonHR is a full-featured Employee Directory & HR Portal built with ASP.NET Core Razor Pages targeting .NET 10. It provides HR administrators with tools to manage departments, employees, leave requests, performance reviews, and employee skills/competencies.

## What Was Generated

### Project Structure
- **Framework**: ASP.NET Core Razor Pages (.NET 10)
- **Database**: Entity Framework Core with SQLite (`HorizonHR.db`)
- **Styling**: Bootstrap 5 (default theme)
- **Location**: `./src/HorizonHR/`

### Models (8 entity classes + enums)
- `Department` - with self-referencing hierarchy (parent/child)
- `Employee` - with self-referencing manager relationship
- `LeaveType` - configurable leave categories
- `LeaveBalance` - per-employee, per-year leave tracking
- `LeaveRequest` - leave workflow with status transitions
- `PerformanceReview` - multi-step review workflow
- `Skill` - skill catalog with categories
- `EmployeeSkill` - many-to-many join with proficiency levels
- `PaginatedList<T>` - reusable pagination helper
- Enums: `EmployeeStatus`, `EmploymentType`, `LeaveRequestStatus`, `ReviewStatus`, `OverallRating`, `ProficiencyLevel`

### Services (5 service interfaces + implementations)
- `IDepartmentService` / `DepartmentService` - CRUD, hierarchy, circular reference detection
- `IEmployeeService` / `EmployeeService` - CRUD, search/filter, termination cascading, auto-numbering, leave balance initialization
- `ILeaveService` / `LeaveService` - request workflow (submit/approve/reject/cancel), balance enforcement, overlap detection, business day calculation
- `IReviewService` / `ReviewService` - review workflow (draft → self-assessment → manager review → completed), overlap validation
- `ISkillService` / `SkillService` - skill catalog CRUD, employee skill management, skill-based search

### Pages (30+ Razor Pages)
- **Dashboard** (`/`) - Overview with stats cards, recent hires, on-leave employees, headcount by department
- **Departments** (`/Departments`) - List (hierarchical), Details, Create, Edit
- **Employees** (`/Employees`) - Directory (searchable/filterable/paginated), Details (tabbed profile), Create, Edit, Terminate, Direct Reports
- **Leave** (`/Leave`) - Requests list, Details, Submit Request, Approve/Reject, Cancel, Balances, Employee Summary
- **Reviews** (`/Reviews`) - List, Details, Create, Self-Assessment, Manager Review
- **Skills** (`/Skills`) - List (grouped by category), Create, Edit, Search by skill; Employee Skills management

### Shared Components
- `_Layout.cshtml` - Bootstrap 5 dark navbar with all navigation links
- `_StatusBadge.cshtml` - Reusable status/type badge partial with color-coding
- `_Pagination.cshtml` - Reusable pagination partial

### Data Layer
- `ApplicationDbContext` - Full EF Core configuration with relationships, indexes, unique constraints, automatic timestamp updates
- `DataSeeder` - Seeds database with realistic demo data:
  - 5 departments with hierarchy
  - 4 leave types
  - 13 employees across departments (including terminated and on-leave)
  - Leave balances for all active employees
  - 8 leave requests in various statuses
  - 6 performance reviews in various statuses
  - 12 skills across categories
  - 24 employee-skill records

### Business Rules Implemented
1. Leave balance enforcement on request submission
2. Leave request workflow with status transitions
3. Automatic balance deduction on approval and restoration on cancellation
4. Overlapping leave detection
5. Department manager constraints
6. Employee termination cascading (cancel leaves, remove from manager roles)
7. Performance review workflow with status transitions
8. No duplicate reviews for overlapping periods
9. Leave balance auto-initialization for new employees
10. Auto-generated employee numbers (EMP-NNNN format)
11. Department hierarchy circular reference prevention

### Cross-Cutting Concerns
- Global error handling with custom Error page
- TempData flash messages (success/error) with Bootstrap alerts
- Post-Redirect-Get (PRG) pattern on all forms
- Data Annotations validation with client-side validation support
- Semantic HTML with accessibility attributes (aria-label, aria-current, role="alert")
- Input model pattern with [BindProperty] for form binding
- Named handler methods for multi-action pages (Approve/Reject)
- ILogger usage for key operations

## How to Run

```bash
cd src/HorizonHR
dotnet run
```

The database is automatically created and seeded on first run.
