# Comparative Analysis: no-skills, dotnet-webapi, dotnet-artisan, managedcode-dotnet-skills

## Introduction

This report compares four Copilot skill configurations used to generate the same three ASP.NET Core Razor Pages applications on .NET 10 with SQLite and Bootstrap 5:

| Configuration | Description | Skills Invoked |
|---|---|---|
| **no-skills** | Baseline — no custom skills | None (model's default behavior) |
| **dotnet-webapi** | Single `dotnet-webapi` skill | `dotnet-webapi` (Web API–oriented patterns adapted for Razor Pages) |
| **dotnet-artisan** | Plugin chain with 9 skills | `using-dotnet` → `dotnet-advisor` → `dotnet-csharp` → `dotnet-api` (+ tooling, releases) |
| **managedcode-dotnet-skills** | Community managed-code skills | `dotnet` → `dotnet-aspnet-core` + `dotnet-entity-framework-core` + `dotnet-project-setup` + `dotnet-modern-csharp` |

Each configuration generated the same three apps:
- **SparkEvents** — Event registration portal with ticket types, capacity, waitlists, and check-in
- **KeystoneProperties** — Property management with leases, tenants, maintenance, and rent tracking
- **HorizonHR** — HR portal with employees, leave management, reviews, and skills

All four configurations produced fully-featured, structurally similar applications with service layers, InputModel patterns, and PRG form handling. The differences lie in code-quality details, modern C# adoption, and production-readiness patterns.

---

## Executive Summary

| Dimension | no-skills | dotnet-webapi | dotnet-artisan | managedcode |
|---|---|---|---|---|
| **Sealed Types** | ❌ None | ✅ All classes | ✅ All classes | ❌ None |
| **Primary Constructors** | ❌ Traditional | ✅ All services + pages | ✅ All services + pages | ✅ All services + pages |
| **CancellationToken** | ❌ Not propagated | ✅ All service methods | ❌ Not propagated | ❌ Not propagated |
| **AsNoTracking()** | ❌ Not used | ✅ All read queries | ✅ All read queries | ✅ All read queries |
| **InputModel Separation** | ✅ All forms | ✅ All forms | ✅ All forms | ✅ All forms |
| **Named Handlers** | ✅ Some pages | ✅ Some pages | ✅ More pages | ✅ More pages |
| **IExceptionHandler** | ❌ None | ✅ GlobalExceptionHandler | ❌ None | ❌ None |
| **Collection Expressions** | ❌ `new List<T>()` | Mixed | ✅ `= []` throughout | ✅ `= []` throughout |
| **Service Abstraction** | ✅ Interfaces | ✅ Interfaces (more granular) | ✅ Interfaces | ✅ Interfaces |
| **View Components / Tag Helpers** | ❌ None | ❌ None | ✅ StatusBadgeTagHelper | ✅ ViewComponents (2) |
| **Semantic HTML & ARIA** | ✅ Basic (~75 refs) | ✅ Rich (~170 refs) | ✅ Rich (~160 refs) | ✅ Moderate (~110 refs) |
| **TempData Flash Messages** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |
| **Pagination** | ✅ Partial view | ✅ PaginatedList<T> + partial | ✅ PaginatedList<T> + partial | ✅ ViewComponent |
| **IEntityTypeConfiguration** | ❌ Inline | ❌ Inline | ❌ Inline | ✅ Separate classes |
| **Bootstrap Integration** | ✅ Good | ✅ Comprehensive | ✅ Comprehensive | ✅ Good |
| **CSS Organization** | Mixed (some inline) | Mixed (some inline) | ✅ Clean (no inline) | ✅ Clean (no inline) |
| **Data Seeder** | Static sync method | Static async method | Static async method | Static method |
| **File Organization** | ✅ Feature-based | ✅ Feature-based + Middleware/ | ✅ Feature-based | ✅ Feature-based + Components/ |

---

## 1. Page Model Design

### no-skills — Mixed thin/fat models

Page models use traditional constructor injection. Some pages inject both a service AND the DbContext directly, creating a mixed responsibility pattern:

```csharp
// no-skills/SparkEvents/Pages/Events/Index.cshtml.cs
public class IndexModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly SparkEventsDbContext _db;

    public IndexModel(IEventService eventService, SparkEventsDbContext db)
    {
        _eventService = eventService;
        _db = db;
    }
}
```

### dotnet-webapi — Strict thin models with private fields

All page models are `sealed`, use traditional constructor DI, and **never** inject DbContext directly. All data access goes through service interfaces:

```csharp
// dotnet-webapi/SparkEvents/Pages/Events/Index.cshtml.cs
public sealed class IndexModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IEventCategoryService _categoryService;

    public IndexModel(IEventService eventService, IEventCategoryService categoryService)
    {
        _eventService = eventService;
        _categoryService = categoryService;
    }
}
```

### dotnet-artisan — Primary constructors, sealed, thin by principle

Page models use C# 12 primary constructors, eliminating field boilerplate. The `using-dotnet` KISS principle selectively allows direct DbContext access for simple CRUD pages while routing complex logic through services:

```csharp
// dotnet-artisan/SparkEvents/Pages/Events/Index.cshtml.cs
public sealed class IndexModel(IEventService eventService, SparkEventsDbContext db) : PageModel
{
    private const int PageSize = 10;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }
}
```

### managedcode-dotnet-skills — Primary constructors, non-sealed

Uses primary constructors like dotnet-artisan, but classes are **not** sealed:

```csharp
// managedcode/SparkEvents/Pages/Events/Index.cshtml.cs
public class IndexModel(IEventService eventService, ICategoryService categoryService) : PageModel
{
    public List<Event> Events { get; set; } = [];
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; } = 1;
}
```

**Verdict:** **dotnet-webapi** has the strictest separation (no DbContext in page models at all). **dotnet-artisan** achieves the best balance with primary constructors and pragmatic KISS-driven DbContext use. Both are superior to no-skills, which leaks DbContext into page models without a clear philosophy.

---

## 2. Form Handling & Validation

All four configurations implement the Post-Redirect-Get (PRG) pattern, use `[BindProperty]` with nested `InputModel` classes, and include `_ValidationScriptsPartial` for client-side validation. The differences are minor but meaningful.

### Shared across all

```csharp
// Common pattern in all four configs
[BindProperty]
public InputModel Input { get; set; } = new();

public async Task<IActionResult> OnPostAsync()
{
    if (!ModelState.IsValid)
        return Page();

    // ... create entity ...
    TempData["SuccessMessage"] = "Created successfully.";
    return RedirectToPage("Index");
}
```

### dotnet-webapi — CancellationToken in POST handlers

Only dotnet-webapi propagates CancellationToken through POST handlers:

```csharp
// dotnet-webapi/SparkEvents/Pages/Events/Create.cshtml.cs
public async Task<IActionResult> OnPostAsync(CancellationToken ct)
{
    if (!ModelState.IsValid) { /* ... */ return Page(); }
    var evt = await _eventService.CreateAsync(newEvent, ct);
    TempData["Success"] = $"Event '{evt.Title}' created.";
    return RedirectToPage("Details", new { id = evt.Id });
}
```

**Verdict:** All configs handle forms correctly. **dotnet-webapi** is the most production-ready with CancellationToken propagation in handlers.

---

## 3. Input Model Separation

All four configurations use dedicated `InputModel` classes inside page models rather than binding directly to entities. This prevents over-posting attacks and is a Razor Pages best practice.

```csharp
// Pattern used consistently across all four configs
public sealed class InputModel  // sealed in dotnet-webapi/dotnet-artisan
{
    [Required, MaxLength(300)]
    [Display(Name = "Event Title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Category")]
    public int EventCategoryId { get; set; }
}
```

**Verdict:** All configurations do this correctly. **dotnet-webapi** and **dotnet-artisan** additionally seal their InputModel classes.

---

## 4. Named Handler Methods

Named handlers like `OnPostDeleteAsync` and `OnPostApproveAsync` improve intent clarity and routing on multi-action pages.

| Config | Named Handler Files | Example Pages |
|---|---|---|
| **no-skills** | ~5 | TicketTypes, ManageEmployeeSkills, LeaveReview |
| **dotnet-webapi** | ~5 | TicketTypes, ManageEmployeeSkills, LeaveReview |
| **dotnet-artisan** | ~9 | TicketTypes, Skills, LeaveReview, EventDetails, Categories + more |
| **managedcode** | ~9 | TicketTypes, Skills, LeaveReview, EventDetails + more |

```csharp
// dotnet-artisan/SparkEvents/Pages/Events/TicketTypes.cshtml.cs
public sealed class TicketTypesModel(...) : PageModel
{
    public async Task<IActionResult> OnPostCreateAsync(int eventId) { /* ... */ }
    public async Task<IActionResult> OnPostDeleteAsync(int eventId, int ticketTypeId) { /* ... */ }
}
```

**Verdict:** **dotnet-artisan** and **managedcode** use named handlers more broadly, which is the better pattern for multi-action pages. All configs use them where most obviously needed (TicketTypes).

---

## 5. Semantic HTML

All configurations use `<nav>`, `<main>`, `<table>` with `<thead>/<tbody>`, and `<label>` elements. The skill-enhanced configs produce noticeably richer semantic markup.

```html
<!-- dotnet-webapi _Layout.cshtml — rich semantic structure -->
<nav class="navbar navbar-expand-lg navbar-dark bg-primary" aria-label="Main navigation">
    <!-- ... -->
</nav>
<main role="main" class="container mt-4">
    @RenderBody()
</main>
<footer class="border-top footer text-muted">
    <!-- ... -->
</footer>
```

```html
<!-- no-skills _Layout.cshtml — basic but adequate -->
<nav class="navbar navbar-expand-lg navbar-dark bg-primary">
    <!-- ... (no aria-label) -->
</nav>
<div class="container">
    <main role="main" class="pb-3">
        @RenderBody()
    </main>
</div>
```

**Verdict:** **dotnet-webapi** and **dotnet-artisan** produce the most semantic HTML. All configs are adequate, but the skill-enhanced versions include more `<section>`, `<article>`, and `<footer>` elements.

---

## 6. Accessibility & ARIA

ARIA attribute usage was measured across all `.cshtml` files:

| Config | ARIA/role Occurrences | Files with ARIA |
|---|---|---|
| **no-skills** | ~75 | 29 |
| **dotnet-webapi** | ~170 | 65 |
| **dotnet-artisan** | ~160 | 55 |
| **managedcode** | ~110 | 68 |

### dotnet-webapi — Most comprehensive ARIA

```html
<!-- dotnet-webapi: rich accessibility on form pages -->
<div class="alert alert-success alert-dismissible fade show" role="alert">
    @TempData["Success"]
    <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
</div>

<!-- Navigation with aria-label and aria-current -->
<nav aria-label="Main navigation">
    <a class="nav-link active" aria-current="page" asp-page="/Index">Dashboard</a>
</nav>

<!-- Pagination with aria-label -->
<nav aria-label="Page navigation">
    <ul class="pagination justify-content-center">
```

### no-skills — Basic ARIA

```html
<!-- no-skills: minimal but present on key elements -->
<div class="alert alert-success" role="alert">
    @TempData["SuccessMessage"]
</div>
```

**Verdict:** **dotnet-webapi** leads with 2× the ARIA usage of no-skills, including `aria-current`, `aria-label` on nav/pagination, and `aria-describedby` on form inputs. **dotnet-artisan** is close behind.

---

## 7. View Components & Reusable UI

| Config | Custom Components | Type |
|---|---|---|
| **no-skills** | None | Partial views only (_Pagination, _StatusBadge) |
| **dotnet-webapi** | None | Partial views only (_Pagination, _StatusBadge) |
| **dotnet-artisan** | `StatusBadgeTagHelper` | Custom TagHelper class |
| **managedcode** | `StatusBadgeViewComponent`, `PaginationViewComponent` | ViewComponent classes |

### dotnet-artisan — TagHelper approach

```csharp
// dotnet-artisan/HorizonHR/ViewComponents/StatusBadgeTagHelper.cs
public sealed class StatusBadgeTagHelper : TagHelper
{
    [HtmlAttributeName("status")]
    public string Status { get; set; } = string.Empty;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.Attributes.SetAttribute("class", $"badge bg-{GetBadgeColor(Status)}");
        output.Content.SetContent(Status);
    }
}
```

### managedcode — ViewComponent approach

```csharp
// managedcode/KeystoneProperties/Pages/Shared/Components/StatusBadge/StatusBadgeViewComponent.cs
public class StatusBadgeViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string status, string? type = null)
    {
        ViewBag.Status = status;
        ViewBag.BadgeClass = GetBadgeClass(status, type);
        return View();
    }
}
```

**Verdict:** **managedcode** has the richest component model with two ViewComponents. **dotnet-artisan** uses a TagHelper, which is more idiomatic for simple rendering. Both approaches are superior to the partial-view-only approach of no-skills and dotnet-webapi, reducing duplication and improving testability.

---

## 8. Null & Empty State Handling

All configurations handle empty collections with conditional rendering:

```html
<!-- Common pattern across all configs -->
@if (Model.Events.Any())
{
    <table class="table table-striped">...</table>
}
else
{
    <div class="alert alert-info">
        <p>No events found matching your criteria.</p>
    </div>
}
```

The skill-enhanced configs tend to use slightly more polished empty state messages and icons:

```html
<!-- dotnet-artisan: richer empty states -->
<div class="text-center py-5">
    <i class="bi bi-calendar-x fs-1 text-muted"></i>
    <p class="text-muted mt-2">No events found. Try adjusting your filters.</p>
</div>
```

**Verdict:** All configs handle empty states. **dotnet-artisan** and **dotnet-webapi** produce slightly more polished UI with icons and centered layouts.

---

## 9. CSS Organization

Inline `style=""` attribute usage in `.cshtml` files:

| Config | Inline style= Occurrences |
|---|---|
| **no-skills** | ~18 |
| **dotnet-webapi** | ~17 |
| **dotnet-artisan** | ~0 |
| **managedcode** | ~0 |

### dotnet-artisan / managedcode — Clean CSS

Both skill-chain configs rely entirely on Bootstrap CSS classes with no inline styles, keeping presentational concerns in CSS files.

### no-skills / dotnet-webapi — Some inline styles

```html
<!-- no-skills: inline styles for quick positioning -->
<div style="max-width: 600px; margin: 0 auto;">
    <div class="progress" style="height: 20px;">
```

**Verdict:** **dotnet-artisan** and **managedcode** are cleanest, relying on CSS classes. **no-skills** and **dotnet-webapi** use inline styles sparingly — not a major issue but less maintainable.

---

## 10. Tag Helper Usage

All configurations use modern ASP.NET Core tag helpers (`asp-for`, `asp-page`, `asp-route-*`, `asp-items`, `asp-validation-for`, `asp-validation-summary`). None use legacy `@Html.TextBoxFor()` or `@Html.ActionLink()` patterns.

```html
<!-- Universal across all configs -->
<form method="post">
    <div class="mb-3">
        <label asp-for="Input.Title" class="form-label"></label>
        <input asp-for="Input.Title" class="form-control" />
        <span asp-validation-for="Input.Title" class="text-danger"></span>
    </div>
    <button type="submit" class="btn btn-primary">Save</button>
</form>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

All `_ViewImports.cshtml` files include:
```csharp
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

The **dotnet-artisan** HorizonHR app additionally registers its custom tag helper:
```csharp
@addTagHelper *, HorizonHR
```

**Verdict:** All configurations use modern tag helpers correctly. No meaningful differences.

---

## 11. Layout & Partial Views

All configurations use `_Layout.cshtml`, `_ViewStart.cshtml`, `_ViewImports.cshtml`, and `_ValidationScriptsPartial`. All extract pagination and status badges into reusable partials.

| Config | Reusable Partials | Layout Features |
|---|---|---|
| **no-skills** | _Pagination, _StatusBadge | Navbar, TempData alerts, @RenderBody |
| **dotnet-webapi** | _Pagination, _StatusBadge | Same + dismissible alerts |
| **dotnet-artisan** | _Pagination, _StatusBadge, _EventStatusBadge, _RegistrationStatusBadge | Same + `@section Scripts` |
| **managedcode** | ViewComponents for Pagination + StatusBadge | Same |

### dotnet-artisan — Most partials

dotnet-artisan's SparkEvents has separate badge partials per entity type, providing type-specific color mapping:

```html
<!-- dotnet-artisan: _EventStatusBadge.cshtml -->
@model EventStatus
@{
    var (css, label) = Model switch
    {
        EventStatus.Draft => ("secondary", "Draft"),
        EventStatus.Published => ("success", "Published"),
        EventStatus.SoldOut => ("warning", "Sold Out"),
        EventStatus.Cancelled => ("danger", "Cancelled"),
        EventStatus.Completed => ("info", "Completed"),
        _ => ("secondary", Model.ToString())
    };
}
<span class="badge bg-@css">@label</span>
```

**Verdict:** **dotnet-artisan** has the richest partial view hierarchy with type-specific badge partials. **managedcode** uses ViewComponents for the same purpose, which is more encapsulated but requires more boilerplate.

---

## 12. Bootstrap Integration

All configurations use Bootstrap 5 classes competently. The skill-enhanced configs tend to use a wider range of Bootstrap features:

| Feature | no-skills | dotnet-webapi | dotnet-artisan | managedcode |
|---|---|---|---|---|
| Grid system (container, row, col-*) | ✅ | ✅ | ✅ | ✅ |
| Form classes (form-control, form-label) | ✅ | ✅ | ✅ | ✅ |
| Table classes (table-striped, table-hover) | ✅ | ✅ | ✅ | ✅ |
| Badges for status | ✅ | ✅ | ✅ | ✅ |
| Alert dismissible with data-bs-dismiss | Basic | ✅ | ✅ | ✅ |
| Cards for dashboard | ✅ | ✅ | ✅ | ✅ |
| Bootstrap Icons | Some | ✅ | ✅ | ✅ |
| Progress bars | ✅ | ✅ | ✅ | ✅ |
| btn-outline-* variants | Some | ✅ | ✅ | ✅ |

**Verdict:** **dotnet-webapi** and **dotnet-artisan** use the most comprehensive Bootstrap vocabulary. **no-skills** is slightly behind on dismissible alerts and icon usage.

---

## 13. Sealed Types

This is one of the most significant differentiators:

| Config | Sealed Classes | Non-Sealed Classes |
|---|---|---|
| **no-skills** | 0 | All |
| **dotnet-webapi** | ~120+ (all classes) | 0 |
| **dotnet-artisan** | ~120+ (all classes) | 0 |
| **managedcode** | 0 | All |

### dotnet-webapi / dotnet-artisan — Everything sealed

```csharp
// Both configs seal all types
public sealed class Event { /* ... */ }
public sealed class EventService(SparkEventsDbContext db, ILogger<EventService> logger) : IEventService { }
public sealed class IndexModel : PageModel { /* ... */ }
public sealed class InputModel { /* ... */ }
internal sealed class GlobalExceptionHandler : IExceptionHandler { /* ... */ }  // dotnet-webapi only
```

### no-skills / managedcode — Nothing sealed

```csharp
// Both use unsealed classes
public class Event { /* ... */ }
public class EventService : IEventService { /* ... */ }
public class IndexModel : PageModel { /* ... */ }
```

**Verdict:** **dotnet-webapi** and **dotnet-artisan** correctly seal all types, enabling JIT devirtualization (CA1852) and signaling design intent. This is a clear win for performance and maintainability. The fact that **managedcode** (which claims modern C# patterns) misses `sealed` is a notable gap.

---

## 14. Primary Constructors

| Config | Uses Primary Constructors | Traditional DI Constructors |
|---|---|---|
| **no-skills** | ❌ | All classes |
| **dotnet-webapi** | ✅ Services only | Page models (traditional) |
| **dotnet-artisan** | ✅ All services + page models | None |
| **managedcode** | ✅ All services + page models | None |

### no-skills — Traditional boilerplate

```csharp
// no-skills: 6+ lines per constructor
public class EventService : IEventService
{
    private readonly SparkEventsDbContext _db;
    private readonly ILogger<EventService> _logger;

    public EventService(SparkEventsDbContext db, ILogger<EventService> logger)
    {
        _db = db;
        _logger = logger;
    }
}
```

### dotnet-artisan / managedcode — C# 12 conciseness

```csharp
// dotnet-artisan: 1 line
public sealed class EventService(SparkEventsDbContext db, ILogger<EventService> logger) : IEventService
{
    // db and logger are directly available as parameters
}
```

**Verdict:** **dotnet-artisan** and **managedcode** make the most of primary constructors, applying them to both services and page models. **dotnet-webapi** uses them in services but not consistently in page models. **no-skills** doesn't use them at all, producing significantly more boilerplate.

---

## 15. Service Abstraction

All four configurations use the interface + implementation pattern with `AddScoped<IService, Service>()` registration:

```csharp
// Universal pattern across all configs
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
```

### dotnet-webapi — Most granular service decomposition

dotnet-webapi creates more fine-grained services (separate `IEventCategoryService`, `IVenueService`, `ITicketTypeService`, `IAttendeeService`) rather than having one monolithic service per domain area. This results in 7+ services in SparkEvents vs 3 in no-skills.

### dotnet-artisan — KISS-driven selectivity

Following the `using-dotnet` KISS principle, dotnet-artisan only creates service interfaces where genuine business logic exists. Simple CRUD (categories, venues, attendees) uses DbContext directly:

```csharp
// dotnet-artisan: only 3 services in SparkEvents
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<ICheckInService, CheckInService>();
// Categories, Venues, Attendees: direct DbContext in page models
```

**Verdict:** **dotnet-webapi** has the strictest dependency inversion but may over-abstract simple CRUD. **dotnet-artisan's** KISS approach is pragmatic — abstracting only where business logic justifies it. Both are valid strategies depending on team preferences.

---

## 16. CancellationToken Propagation

This is one of the sharpest differentiators between the configurations:

| Config | Service Interface Methods | Page Model Handlers | DbContext Calls |
|---|---|---|---|
| **no-skills** | ❌ No CT parameter | ❌ No CT parameter | SaveChangesAsync(CT) in override only |
| **dotnet-webapi** | ✅ All methods accept `CancellationToken ct = default` | ✅ `OnGetAsync(CancellationToken ct)` | ✅ Forwarded to all EF Core calls |
| **dotnet-artisan** | ❌ No CT parameter | ❌ No CT parameter | SaveChangesAsync(CT) in override only |
| **managedcode** | ❌ No CT parameter | ❌ No CT parameter | SaveChangesAsync(CT) in override only |

### dotnet-webapi — Full propagation chain

```csharp
// dotnet-webapi/SparkEvents/Services/IEventService.cs
public interface IEventService
{
    Task<PaginatedList<Event>> GetAllAsync(string? search, int? categoryId,
        EventStatus? status, DateTime? fromDate, DateTime? toDate,
        int pageNumber, int pageSize, CancellationToken ct = default);
    Task<Event?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Event> CreateAsync(Event evt, CancellationToken ct = default);
    Task CancelEventAsync(int id, string reason, CancellationToken ct = default);
}
```

```csharp
// dotnet-webapi page model forwarding token
public async Task<IActionResult> OnGetAsync(CancellationToken ct)
{
    Events = await _eventService.GetAllAsync(Search, CategoryId, Status,
        FromDate, ToDate, PageNumber, PageSize, ct);
    return Page();
}
```

**Verdict:** **dotnet-webapi** is the only configuration that properly propagates CancellationToken. This is **critical for production** — without it, server resources are wasted on cancelled requests (user navigates away, request times out). The other three configs all miss this, which is a significant gap.

---

## 17. AsNoTracking Usage

| Config | Uses AsNoTracking() | Read-Only Query Coverage |
|---|---|---|
| **no-skills** | ❌ ~2 occurrences total | Minimal — only in HorizonHR and KeystoneProperties DbContext |
| **dotnet-webapi** | ✅ All read queries | Complete — every service read method |
| **dotnet-artisan** | ✅ All read queries | Complete — every service read method |
| **managedcode** | ✅ All read queries | Complete — every service read method |

### Skill-enhanced configs — Consistent optimization

```csharp
// dotnet-artisan/SparkEvents/Services/EventService.cs
public async Task<(List<Event> Items, int TotalCount)> GetEventsAsync(...)
{
    var query = db.Events
        .AsNoTracking()
        .Include(e => e.Category)
        .Include(e => e.Venue)
        .AsQueryable();
    // ...
}
```

### no-skills — Missing optimization

```csharp
// no-skills: tracked queries everywhere
public async Task<List<Event>> GetEventsAsync(...)
{
    var query = _db.Events
        .Include(e => e.EventCategory)
        .Include(e => e.Venue)
        .AsQueryable();
    // no AsNoTracking() — entities tracked unnecessarily
}
```

**Verdict:** All three skill-enhanced configs consistently apply `AsNoTracking()` on read-only queries. **no-skills** almost entirely misses this optimization, which doubles memory usage and slows queries on list pages. This is one of the clearest skill-driven improvements.

---

## 18. TempData & Flash Messages

All configurations use the same pattern for user feedback after form submissions:

```csharp
// Universal pattern
TempData["SuccessMessage"] = "Event created successfully.";
return RedirectToPage("Index");
```

Layout rendering is consistent:

```html
<!-- Common _Layout.cshtml pattern -->
@if (TempData["SuccessMessage"] is string successMsg)
{
    <div class="alert alert-success alert-dismissible fade show" role="alert">
        @successMsg
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    </div>
}
@if (TempData["ErrorMessage"] is string errorMsg)
{
    <div class="alert alert-danger alert-dismissible fade show" role="alert">
        @errorMsg
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    </div>
}
```

### Minor variations

- **dotnet-webapi** uses shorter keys: `TempData["Success"]` / `TempData["Error"]`
- **no-skills** and **dotnet-artisan** use: `TempData["SuccessMessage"]` / `TempData["ErrorMessage"]`
- **dotnet-webapi** and **dotnet-artisan** use `role="alert"` on all flash messages consistently

**Verdict:** All configs implement flash messages well. **dotnet-webapi** and **dotnet-artisan** are slightly more accessible with consistent `role="alert"` usage.

---

## 19. Data Seeder Design

| Config | Seeder Pattern | Async | Idempotency |
|---|---|---|---|
| **no-skills** | `DataSeeder.Seed(db)` (static sync) | ❌ | ✅ Checks `AnyAsync()` |
| **dotnet-webapi** | `await DataSeeder.SeedAsync(db)` (static async) | ✅ | ✅ Checks `AnyAsync()` |
| **dotnet-artisan** | `await DataSeeder.SeedAsync(db)` (static async) | ✅ | ✅ Checks `AnyAsync()` |
| **managedcode** | `DataSeeder.Seed(db)` (static sync) | ❌ | ✅ Checks `Any()` |

```csharp
// dotnet-webapi/dotnet-artisan: async seeding
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SparkEventsDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DataSeeder.SeedAsync(db);
}

// no-skills: synchronous seeding
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SparkEventsDbContext>();
    db.Database.EnsureCreated();
    DataSeeder.Seed(db);
}
```

**Verdict:** **dotnet-webapi** and **dotnet-artisan** use async seeding, which is consistent with the async-everywhere approach. All configs implement idempotency guards correctly.

---

## 20. Exception Handling Strategy

| Config | Strategy | Implementation |
|---|---|---|
| **no-skills** | Default error page | `app.UseExceptionHandler("/Error")` only |
| **dotnet-webapi** | `IExceptionHandler` middleware | `GlobalExceptionHandler` in Middleware/ folder |
| **dotnet-artisan** | Default error page | `app.UseExceptionHandler("/Error")` only |
| **managedcode** | Default error page | `app.UseExceptionHandler("/Error")` only |

### dotnet-webapi — Structured exception handling

```csharp
// dotnet-webapi/SparkEvents/Middleware/GlobalExceptionHandler.cs
internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
        CancellationToken ct)
    {
        logger.LogError(exception, "Unhandled exception");
        // Maps exception types to HTTP status codes
        // KeyNotFoundException → 404, InvalidOperationException → 400
    }
}
```

Registration in Program.cs:
```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
```

**Verdict:** **dotnet-webapi** is the only config with structured exception handling via `IExceptionHandler`, which is the modern .NET pattern. The others rely on the default exception handler, which is adequate for development but less robust for production.

---

## 21. File Organization

All configurations organize files by feature area under `Pages/`:

```
Pages/
├── Index.cshtml(.cs)          # Dashboard
├── Events/                    # All event-related pages
├── Attendees/                 # All attendee pages
├── Registrations/             # Registration details/cancel
├── CheckIn/                   # Check-in workflow
├── Categories/                # Category CRUD
├── Venues/                    # Venue CRUD
└── Shared/                    # Layout, partials, validation scripts
```

### Distinctive organizational patterns

| Config | Unique Folder | Purpose |
|---|---|---|
| **dotnet-webapi** | `Middleware/` | GlobalExceptionHandler |
| **dotnet-artisan** | `ViewComponents/` (HorizonHR) | StatusBadgeTagHelper |
| **managedcode** | `Pages/Shared/Components/` | ViewComponent views |
| **managedcode** | `Data/Configurations/` (SparkEvents) | IEntityTypeConfiguration classes |

**Verdict:** All configs use sensible feature-based organization. **dotnet-webapi** and **managedcode** have slightly richer folder structures for middleware and components respectively.

---

## 22. Pagination

All configurations implement pagination, but with different levels of reusability:

### dotnet-webapi / dotnet-artisan — PaginatedList<T> helper class

```csharp
// Reusable generic pagination
public sealed class PaginatedList<T>
{
    public IReadOnlyList<T> Items { get; }
    public int PageNumber { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
```

### managedcode — ViewComponent for pagination UI

```csharp
// managedcode: Pagination as a ViewComponent
public class PaginationViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(int currentPage, int totalPages,
        string pagePath, IDictionary<string, string>? routeValues = null)
    {
        // Renders pagination UI with route value preservation
    }
}
```

### no-skills — Simpler tuple-based approach

```csharp
// no-skills: returns tuple, manual pagination in page model
public async Task<(List<Event> Items, int TotalCount)> GetEventsAsync(
    string? search, int page, int pageSize)
```

**Verdict:** **dotnet-webapi** and **dotnet-artisan** have the most professional `PaginatedList<T>` with metadata properties. **managedcode's** ViewComponent approach eliminates UI duplication. **no-skills** works but is less reusable.

---

## 23. Collection Initialization

| Config | Syntax | Example |
|---|---|---|
| **no-skills** | `new List<T>()` | `public ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();` |
| **dotnet-webapi** | Mixed | `new List<T>()` on entities, `= []` on page models |
| **dotnet-artisan** | `= []` throughout | `public ICollection<TicketType> TicketTypes { get; set; } = [];` |
| **managedcode** | `= []` throughout | `public ICollection<TicketType> TicketTypes { get; set; } = [];` |

**Verdict:** **dotnet-artisan** and **managedcode** consistently use C# 12 collection expressions, which are more concise and signal modern C# awareness. **no-skills** uses the pre-C# 12 pattern exclusively.

---

## 24. Custom Tag Helpers

| Config | Custom TagHelpers | Registration |
|---|---|---|
| **no-skills** | ❌ None | Standard only |
| **dotnet-webapi** | ❌ None | Standard only |
| **dotnet-artisan** | ✅ `StatusBadgeTagHelper` (HorizonHR) | `@addTagHelper *, HorizonHR` in _ViewImports |
| **managedcode** | ❌ (uses ViewComponents instead) | Standard only |

The **dotnet-artisan** `StatusBadgeTagHelper` allows usage like:

```html
<status-badge status="@item.Status"></status-badge>
```

Which is more Razor-idiomatic than partial views:

```html
<!-- Partial view approach (other configs) -->
<partial name="_StatusBadge" model="@item.Status" />
```

**Verdict:** **dotnet-artisan's** TagHelper is the most idiomatic Razor approach for simple rendering components. **managedcode's** ViewComponents are heavier but more appropriate for complex components with their own views.

---

## 25. EF Core Configuration Style

| Config | Configuration Approach |
|---|---|
| **no-skills** | Inline in `OnModelCreating` |
| **dotnet-webapi** | Inline in `OnModelCreating` |
| **dotnet-artisan** | Inline in `OnModelCreating` |
| **managedcode** | `IEntityTypeConfiguration<T>` + `ApplyConfigurationsFromAssembly` (SparkEvents) |

### managedcode — Separated configuration classes

```csharp
// managedcode/SparkEvents/Data/Configurations/EventConfiguration.cs
public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasIndex(e => e.Title);
        builder.Property(e => e.Status).HasConversion<string>();
        // ...
    }
}

// In DbContext:
modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
```

**Verdict:** **managedcode's** `IEntityTypeConfiguration` pattern scales better for large models by keeping DbContext small. For these app sizes, both approaches work fine — but the separated pattern is a best practice for production codebases.

---

## What All Versions Get Right

- ✅ **InputModel pattern** — All four configs use dedicated InputModel classes for form binding, preventing over-posting
- ✅ **Post-Redirect-Get (PRG)** — All POST handlers redirect on success, preventing duplicate submissions
- ✅ **Service layer with interfaces** — All use `AddScoped<IService, Service>()` for dependency injection
- ✅ **TempData flash messages** — All provide success/error feedback after form actions
- ✅ **Feature-based page organization** — All group pages by domain area (Events/, Attendees/, etc.)
- ✅ **Bootstrap 5 responsive layout** — All produce professional, mobile-friendly UI
- ✅ **Client-side + server-side validation** — All include `_ValidationScriptsPartial` and Data Annotations
- ✅ **EF Core Fluent API** — All configure relationships, constraints, and column types in `OnModelCreating`
- ✅ **Async/await throughout** — All service methods and page handlers are async
- ✅ **Reusable pagination** — All extract pagination into a shared partial or component
- ✅ **Status badge partials** — All have reusable status badge rendering
- ✅ **Error page** — All configure `app.UseExceptionHandler("/Error")` with a custom Error page
- ✅ **File-scoped namespaces** — All use `namespace X;` (C# 10+) syntax
- ✅ **Nullable reference types** — All enable nullable context and annotate optional properties

---

## Summary: Impact of Skills

### Ranking by Production-Readiness

| Rank | Config | Score | Key Strengths |
|---|---|---|---|
| 🥇 | **dotnet-webapi** | ⭐⭐⭐⭐⭐ | CancellationToken propagation, sealed types, IExceptionHandler, comprehensive ARIA, AsNoTracking |
| 🥈 | **dotnet-artisan** | ⭐⭐⭐⭐½ | Sealed types, primary constructors everywhere, AsNoTracking, collection expressions, custom TagHelper, clean CSS |
| 🥉 | **managedcode** | ⭐⭐⭐⭐ | Primary constructors, AsNoTracking, ViewComponents, IEntityTypeConfiguration, collection expressions |
| 4th | **no-skills** | ⭐⭐⭐ | Solid fundamentals but misses all performance/modern optimizations |

### Most Impactful Skill-Driven Differences

1. **CancellationToken propagation** (dotnet-webapi only) — The single most production-critical pattern. Without it, server resources are wasted on abandoned requests. Only `dotnet-webapi` achieves this, making it the clear leader for production deployments.

2. **AsNoTracking() on read queries** (all skill configs) — All three skill configurations consistently apply this optimization, while no-skills misses it entirely. This can halve memory usage on list pages.

3. **Sealed types** (dotnet-webapi, dotnet-artisan) — Both enable JIT devirtualization and communicate design intent. Notably, managedcode — despite emphasizing "modern C#" — omits `sealed` entirely.

4. **IExceptionHandler middleware** (dotnet-webapi only) — Structured exception-to-HTTP-status mapping with RFC 7807 ProblemDetails support is a production necessity that only dotnet-webapi provides.

5. **Primary constructors** (all skill configs) — All three skill configurations use C# 12 primary constructors, reducing boilerplate by 50–70% in service and page model classes compared to no-skills.

6. **Custom TagHelper / ViewComponent** (dotnet-artisan, managedcode) — These configs go beyond partial views to create truly reusable, testable UI components.

### Overall Assessment

**Skills make a measurable difference.** The baseline (no-skills) produces functional, well-structured code — but misses performance optimizations (AsNoTracking, sealed), production patterns (CancellationToken, IExceptionHandler), and modern C# features (primary constructors, collection expressions) that the skill-enhanced configs consistently deliver.

**dotnet-webapi** is the most production-ready due to CancellationToken propagation and structured exception handling — patterns that directly impact reliability under load. **dotnet-artisan** produces the cleanest, most idiomatic code with its KISS philosophy, sealed types, and zero inline styles. **managedcode** excels at architectural patterns (IEntityTypeConfiguration, ViewComponents) but surprisingly omits sealed types despite its modern C# focus.

The ideal configuration would combine **dotnet-webapi's** CancellationToken and IExceptionHandler patterns with **dotnet-artisan's** sealed types, primary constructors, and clean CSS approach, plus **managedcode's** IEntityTypeConfiguration and ViewComponent patterns.
