# Comparative Analysis: no-skills, dotnet-webapi, dotnet-artisan, managedcode-dotnet-skills, dotnet-skills

## Introduction

This report compares five Copilot configurations used to generate the same three ASP.NET Core Razor Pages applications:

| Configuration | Description |
|---------------|-------------|
| **no-skills** | Baseline — Copilot with no custom skills |
| **dotnet-webapi** | dotnet-webapi skill |
| **dotnet-artisan** | dotnet-artisan plugin chain (9 skills + 14 specialist agents) |
| **managedcode-dotnet-skills** | Community managed-code skills |
| **dotnet-skills** | Official .NET Skills (dotnet/skills) |

Each configuration generated three identical application scenarios:

- **SparkEvents** — Event registration portal with ticket types, capacity management, waitlists, and check-in
- **KeystoneProperties** — Property management with leases, tenants, maintenance requests, and rent tracking
- **HorizonHR** — Employee directory and HR portal with leave management, reviews, and skills

All applications target **.NET 10** with **SQLite** and follow Razor Pages conventions. The analysis covers 36 quality dimensions with quantitative metrics pulled directly from the generated source code.

---

## Executive Summary

| Dimension | no-skills | dotnet-webapi | dotnet-artisan | managedcode | dotnet-skills |
|-----------|-----------|---------------|----------------|-------------|---------------|
| **Page Model Design** | ✅ Thin | ✅ Thin | ✅ Thin | ✅ Thin | ✅ Thin |
| **Form Handling & Validation** | ✅ Good | ✅ Good | ✅ Good | ✅ Good | ✅ Good |
| **Input Model Separation** | ✅ Nested | ✅ Nested | ✅ Nested+IValidatable | ✅ Nested | ✅ Nested+IValidatable |
| **Named Handler Methods** | ✅ Some (7) | ✅ Some (8) | ❌ None (6 basic) | ❌ None (6 basic) | ✅ Some (8) |
| **Semantic HTML** | ✅ Good (59 `<section>`) | ⚠️ Mixed (30) | ⚠️ Mixed (30) | ✅ Good (44) | ✅ Good (52) |
| **Accessibility & ARIA** | ✅ Good | ✅ Good | ✅ Best | ✅ Best | ✅ Good |
| **View Components** | ❌ Partials only | ❌ Partials only | ❌ Partials only | ✅ ViewComponent | ❌ Partials only |
| **Null & Empty States** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |
| **CSS Organization** | ✅ Clean (15 inline) | ⚠️ OK (21 inline) | ✅ Clean (17 inline) | ✅ Best (11 inline) | ⚠️ OK (21 inline) |
| **Tag Helper Usage** | ✅ Modern | ✅ Modern | ✅ Modern | ✅ Modern | ✅ Modern |
| **Layout & Partials** | ✅ Complete | ✅ Complete | ✅ Complete | ✅ Complete | ✅ Complete+FlashPartial |
| **Bootstrap Integration** | ✅ Full | ✅ Full | ✅ Full | ✅ Full | ✅ Full |
| **Sealed Types** | ❌ None (0) | ✅ Extensive (187) | ✅ Extensive (136) | ❌ None (0) | ⚠️ Minimal (6) |
| **Primary Constructors** | ❌ Traditional (0) | ✅ Widespread (88) | ✅ Widespread (109) | ✅ Widespread (115) | ❌ Traditional (0) |
| **Service Abstraction** | ✅ Interface+Impl | ✅ Interface+Impl | ✅ Interface+Impl | ✅ Interface+Impl | ✅ Interface+Impl |
| **CancellationToken** | ❌ Absent (4) | ✅ Full propagation (420) | ⚠️ Partial (101) | ❌ Minimal (6) | ❌ Absent (4) |
| **AsNoTracking** | ❌ None (0) | ✅ Yes (53) | ✅ Yes (69) | ✅ Yes (72) | ✅ Yes (70) |
| **TempData & Flash** | ✅ Consistent | ✅ Consistent | ✅ Consistent | ✅ Consistent | ✅ Consistent |
| **Data Seeder Design** | ⚠️ Static sync | ⚠️ Static mixed | ✅ Injectable+static | ⚠️ Static async | ⚠️ Static async |
| **Exception Handling** | ❌ String returns | ✅ Middleware+IExceptionHandler | ❌ String returns | ❌ Tuple returns | ⚠️ Try-catch |
| **File Organization** | ✅ Feature-based | ✅ Feature-based | ✅ Feature-based | ✅ Feature-based | ✅ Feature-based |
| **Pagination** | ✅ Reusable | ✅ Reusable | ✅ Reusable | ✅ Reusable | ✅ Reusable |
| **Collection Init** | ❌ `new List<T>()` (46) | ✅ Mixed `[]` (71) | ✅ Mostly `[]` (76) | ✅ Mostly `[]` (95) | ❌ `new List<T>()` (41) |
| **Custom Tag Helpers** | ❌ None | ❌ None | ❌ None | ❌ None | ❌ None |
| **Structured Logging** | ✅ Templates (24) | ✅ Templates (22) | ✅ Templates (12) | ✅ Templates (18) | ✅ Templates (42) |
| **Nullable Reference Types** | ✅ Enabled | ✅ Enabled | ✅ Enabled | ✅ Enabled | ✅ Enabled |
| **Global Usings** | ⚠️ Implicit only | ⚠️ Implicit only | ⚠️ Implicit only | ⚠️ Implicit only | ⚠️ Implicit only |
| **Package Discipline** | ✅ Minimal | ✅ Minimal | ✅ Minimal | ✅ Minimal | ✅ Minimal |
| **EF Core Config** | ✅ Fluent API | ✅ Fluent+HasConversion(20) | ✅ Fluent+SplitQuery(4) | ✅ Fluent+Transactions | ✅ Fluent+SplitQuery(11) |
| **Naming Conventions** | ✅ Correct | ✅ Correct | ✅ Correct | ✅ Correct | ✅ Correct |
| **Enum Design** | ✅ Singular | ✅ Singular+HasConversion | ✅ Singular | ✅ Singular | ✅ Singular |
| **Guard Clauses** | ❌ Manual checks | ❌ Manual checks | ❌ Manual checks | ❌ Manual checks | ❌ Manual checks |
| **Async/Await** | ✅ Correct | ✅ Correct | ✅ Correct | ✅ Correct | ✅ Correct |
| **Access Modifiers** | ✅ Explicit | ✅ Explicit | ✅ Explicit | ✅ Explicit | ✅ Explicit |
| **Dispose & Resources** | ✅ DI-managed | ✅ DI-managed | ✅ DI-managed+using | ✅ DI-managed+transaction | ✅ DI-managed |

---

## 1. Page Model Design

All five configurations produce **thin page models** that delegate to injected services rather than querying `DbContext` directly. This is a universal best practice.

```csharp
// no-skills: SparkEvents/Pages/Events/Index.cshtml.cs — traditional constructor
public class IndexModel : PageModel
{
    private readonly IEventService _eventService;
    public IndexModel(IEventService eventService) => _eventService = eventService;

    public async Task OnGetAsync()
    {
        Events = await _eventService.GetFilteredAsync(filter);
    }
}
```

```csharp
// dotnet-artisan: SparkEvents/Pages/Events/Create.cshtml.cs — primary constructor + sealed
public sealed class CreateModel(IEventService eventService, SparkEventsDbContext db) : PageModel
{
    public async Task<IActionResult> OnPostAsync()
    {
        await eventService.CreateEventAsync(evt);  // Delegates to service
        return RedirectToPage("Details", new { id = evt.Id });
    }
}
```

```csharp
// dotnet-webapi: KeystoneProperties/Pages/Properties/Create.cshtml.cs — primary constructor + sealed
public sealed class CreateModel(IPropertyService propertyService) : PageModel
{
    var created = await propertyService.CreateAsync(property, ct);
}
```

**Verdict**: All configurations equal. The skill-enhanced versions (dotnet-webapi, dotnet-artisan) add `sealed` and primary constructors to page models, but the core delegation pattern is identical.

---

## 2. Form Handling & Validation

All configurations use `[BindProperty]` on nested `InputModel` classes, `asp-validation-for` / `asp-validation-summary`, and the PRG (Post-Redirect-Get) pattern with `TempData`.

```html
<!-- Universal pattern across all configs -->
<form method="post">
    <div asp-validation-summary="ModelOnly" class="text-danger mb-3"></div>
    <div class="mb-3">
        <label asp-for="Input.Title" class="form-label"></label>
        <input asp-for="Input.Title" class="form-control" />
        <span asp-validation-for="Input.Title" class="text-danger"></span>
    </div>
</form>
```

The **dotnet-artisan** and **dotnet-skills** configurations additionally implement `IValidatableObject` for cross-field validation (2 occurrences each):

```csharp
// dotnet-artisan: SparkEvents/Pages/Events/Create.cshtml.cs
public sealed class InputModel : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate <= StartDate)
            yield return new ValidationResult("End date must be after start date.", [nameof(EndDate)]);
    }
}
```

Other configs use manual `ModelState.AddModelError()` for the same validation:

```csharp
// dotnet-skills: SparkEvents/Pages/Events/Create.cshtml.cs
if (Input.EndDate <= Input.StartDate)
    ModelState.AddModelError("Input.EndDate", "End date must be after start date.");
```

**Verdict**: **dotnet-artisan** wins marginally. `IValidatableObject` is the more idiomatic ASP.NET approach for cross-field validation, keeping validation with the model rather than scattered across handlers.

---

## 3. Input Model Separation

All five configurations use nested `InputModel` classes for form binding rather than binding directly to domain entities. This prevents over-posting attacks and keeps entities clean.

```csharp
// Universal pattern — all configs
[BindProperty]
public InputModel Input { get; set; } = new();

public class InputModel
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    // ... only form-relevant properties
}

// Mapping to entity in OnPost
var property = new Property
{
    Name = Input.Name,
    Address = Input.Address,
};
```

**Verdict**: Tie — all configurations follow this best practice consistently.

---

## 4. Named Handler Methods

| Config | Named Handler Count | Example |
|--------|:---:|---------|
| no-skills | 7 | `OnPostApproveAsync`, `OnPostRejectAsync` in HorizonHR |
| dotnet-webapi | 8 | `OnPostPublishAsync` in SparkEvents Events/Details |
| dotnet-artisan | 6 | Default `OnPostAsync` only; separate pages for each action |
| managedcode | 6 | Default `OnPostAsync` only |
| dotnet-skills | 8 | `OnPostApproveAsync`, `OnPostRejectAsync` in HorizonHR |

```csharp
// dotnet-webapi: SparkEvents/Pages/Events/Details.cshtml.cs — named handler
public async Task<IActionResult> OnPostPublishAsync(int id, CancellationToken ct)
{
    var success = await _eventService.PublishAsync(id, ct);
    TempData["SuccessMessage"] = "Event published!";
    return RedirectToPage();
}
```

```csharp
// dotnet-artisan: SparkEvents/Pages/Events/Cancel.cshtml.cs — separate page instead
public async Task<IActionResult> OnPostAsync()
{
    // Single handler; uses a separate Cancel page
    await eventService.CancelEventAsync(id);
}
```

**Verdict**: **dotnet-webapi** and **dotnet-skills** handle this best. Named handlers like `OnPostPublishAsync` with `asp-page-handler="Publish"` are cleaner for multi-action pages than creating separate pages per action.

---

## 5. Semantic HTML

Measured by usage of `<section>`, `<header>`, `<nav>`, `<main>`, `<footer>`, and `<article>` elements:

| Config | `<section>` count | Layout semantics |
|--------|:-:|---|
| no-skills | 59 | `<header>`, `<nav>`, `<main>`, `<footer>` in layout; `<section>` wrapping page content |
| dotnet-webapi | 30 | Layout semantic elements; fewer `<section>` in content pages |
| dotnet-artisan | 30 | Layout semantic elements; detail pages use `<section class="card">` |
| managedcode | 44 | Layout semantic elements; good `<section>` usage |
| dotnet-skills | 52 | Layout semantic elements; strong `<section>` usage |

```html
<!-- no-skills: SparkEvents/Pages/Events/Index.cshtml — section wrapping content -->
<section>
    <div class="d-flex justify-content-between align-items-center mb-3">
        <h1>Events</h1>
    </div>
    <table class="table table-striped">...</table>
</section>
```

```html
<!-- dotnet-webapi: SparkEvents/Pages/Events/Details.cshtml — card as section -->
<section class="card mb-4">
    <header class="card-header"><h2>@evt.Title</h2></header>
    <div class="card-body">
        <dl class="row">
            <dt class="col-sm-4">Category</dt>
            <dd class="col-sm-8">@evt.EventCategory.Name</dd>
        </dl>
    </div>
</section>
```

**Verdict**: **no-skills** and **dotnet-skills** produce the most semantic HTML. The `<section>` element is used liberally to group related content, while dotnet-webapi and dotnet-artisan tend toward div-heavy Bootstrap structures.

---

## 6. Accessibility & ARIA

| Config | `aria-label` | `aria-current` | `role="alert"` | Total ARIA coverage |
|--------|:-:|:-:|:-:|---|
| no-skills | 32 | 20 | 13 | Good — standard Bootstrap ARIA |
| dotnet-webapi | 36 | 19 | 13 | Good — adds `aria-valuenow/min/max` on progress bars |
| dotnet-artisan | 42 | 38 | 11 | Best — most `aria-current="page"` usage |
| managedcode | 51 | 43 | 22 | Best — highest ARIA attribute density |
| dotnet-skills | 37 | 12 | 20 | Good |

```html
<!-- dotnet-webapi: KeystoneProperties/Pages/Properties/Index.cshtml — progress bar ARIA -->
<div class="progress" title="@occupancyRate.ToString("F0")% occupied">
    <div class="progress-bar" role="progressbar"
         aria-valuenow="@occupancyRate.ToString("F0")"
         aria-valuemin="0" aria-valuemax="100">
        @occupancyRate.ToString("F0")%
    </div>
</div>
```

```html
<!-- managedcode: _Layout.cshtml — highest ARIA coverage -->
<nav class="navbar navbar-expand-lg navbar-dark bg-dark" aria-label="Main navigation">
    <a class="nav-link" asp-page="/Index"
       aria-current="@(currentPage == "/Index" ? "page" : null)">Dashboard</a>
</nav>
```

**Verdict**: **managedcode-dotnet-skills** leads with the most ARIA attributes (51 `aria-label`, 43 `aria-current`, 22 `role="alert"`). All configs lack `aria-describedby` and `sr-only` classes — no configuration achieves full WCAG compliance.

---

## 7. View Components & Reusable UI

| Config | ViewComponent classes | Partial views | Status badge reuse |
|--------|:-:|:-:|---|
| no-skills | 0 | 93 | `_StatusBadge.cshtml` partial |
| dotnet-webapi | 0 | 62 | `_StatusBadge.cshtml` partial |
| dotnet-artisan | 0 | 89 | `_StatusBadge.cshtml` partial |
| managedcode | **1** | 72 | `StatusBadgeViewComponent` class |
| dotnet-skills | 0 | 91 | `_StatusBadge.cshtml` partial |

Only **managedcode-dotnet-skills** creates a proper `ViewComponent` class:

```csharp
// managedcode: KeystoneProperties/Pages/Shared/Components/StatusBadge/StatusBadgeViewComponent.cs
public class StatusBadgeViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string status, string? type = null)
    {
        var badgeClass = GetBadgeClass(status, type);
        ViewBag.Status = status;
        ViewBag.BadgeClass = badgeClass;
        return View("Default");
    }
}
```

All other configs use partial views, which are simpler but less encapsulated:

```html
<!-- All other configs: partial view approach -->
<partial name="_StatusBadge" model="@evt.Status.ToString()" />
```

**Verdict**: **managedcode** is the only one using the component-based pattern. ViewComponents are more testable and encapsulated, but the partial approach is pragmatic and sufficient for simple cases.

---

## 8. Null & Empty State Handling

All configurations implement empty collection guards with user-friendly messages:

```html
<!-- Universal pattern across all configs -->
@if (Model.Events.Count == 0)
{
    <div class="alert alert-info">No events found matching your criteria.</div>
}
else
{
    <table class="table table-striped">...</table>
}
```

Service-level null handling varies slightly:

```csharp
// dotnet-artisan: Uses 'is null' pattern matching
if (category is null) { return NotFound(); }

// no-skills: Uses '== null' comparison
if (Event == null) return NotFound();
```

**Verdict**: Tie — all configurations handle this consistently. The `is null` pattern (used by dotnet-artisan, managedcode) is marginally preferred per C# style guidelines.

---

## 9. CSS Organization

| Config | Inline `style=""` attributes | CSS files | Approach |
|--------|:-:|:-:|---|
| no-skills | 15 | `site.css` + `_Layout.cshtml.css` | Local Bootstrap + scoped CSS |
| dotnet-webapi | 21 | `site.css` | Local or CDN Bootstrap |
| dotnet-artisan | 17 | `site.css` + `_Layout.cshtml.css` | Local Bootstrap + scoped CSS |
| managedcode | **11** | CDN only | CDN Bootstrap, minimal custom CSS |
| dotnet-skills | 21 | `site.css` | CDN or local Bootstrap |

**Verdict**: **managedcode** has the fewest inline styles (11) and the cleanest CSS approach. All configs keep styling in Bootstrap classes, but dotnet-webapi and dotnet-skills have the most inline styles (21 each).

---

## 10. Tag Helper Usage

All five configurations use **100% modern tag helpers** with zero legacy `@Html.*` helpers:

```html
<!-- Universal across all configs -->
<label asp-for="Input.Title" class="form-label"></label>
<input asp-for="Input.Title" class="form-control" />
<select asp-for="Input.VenueId" asp-items="Model.VenueList" class="form-select"></select>
<a asp-page="Details" asp-route-id="@evt.Id" class="btn btn-primary">Details</a>
<span asp-validation-for="Input.Title" class="text-danger"></span>
<link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
```

**Verdict**: Tie — all configurations use modern tag helpers exclusively.

---

## 11. Layout & Partial Views

All configurations implement the standard Razor Pages layout hierarchy:

| Component | no-skills | dotnet-webapi | dotnet-artisan | managedcode | dotnet-skills |
|-----------|:-:|:-:|:-:|:-:|:-:|
| `_Layout.cshtml` | ✅ | ✅ | ✅ | ✅ | ✅ |
| `_ViewImports.cshtml` | ✅ | ✅ | ✅ | ✅ | ✅ |
| `_ViewStart.cshtml` | ⚠️ | ✅ | ✅ | ✅ | ✅ |
| `_ValidationScriptsPartial` | ✅ | ✅ | ✅ | ✅ | ✅ |
| `@section Scripts` | ✅ | ✅ | ✅ | ✅ | ✅ |
| `_FlashMessages.cshtml` | ❌ | ❌ | ❌ | ❌ | ✅ (1 app) |
| `_StatusBadge.cshtml` | ✅ | ✅ | ✅ | ✅ | ✅ |
| `_Pagination.cshtml` | ✅ | ✅ | ✅ | ✅ | ✅ |

The **dotnet-skills** HorizonHR app uniquely extracts flash messages into a reusable `_FlashMessages.cshtml` partial:

```html
<!-- dotnet-skills: Pages/Shared/_FlashMessages.cshtml -->
@if (TempData["SuccessMessage"] != null)
{
    <div class="alert alert-success alert-dismissible fade show" role="alert">
        @TempData["SuccessMessage"]
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    </div>
}
```

**Verdict**: **dotnet-skills** edges ahead by extracting flash messages into a partial. Most configs inline TempData rendering in `_Layout.cshtml`.

---

## 12. Bootstrap Integration

All five configurations deliver comprehensive Bootstrap 5 integration: grid system, form classes, table styling, cards, badges, alerts, and navbar. No meaningful differences exist in Bootstrap coverage.

**Verdict**: Tie — all configurations produce professional Bootstrap 5 UIs.

---

## 13. Sealed Types

This is one of the **most dramatic differentiators** between configurations:

| Config | Sealed class declarations | Scope |
|--------|:-:|---|
| no-skills | **0** | Nothing sealed |
| dotnet-webapi | **187** | DbContext, services, page models, entity models all sealed |
| dotnet-artisan | **136** | DbContext, services, page models, InputModels all sealed |
| managedcode | **0** | Nothing sealed |
| dotnet-skills | **6** | Only services sealed (HorizonHR) |

```csharp
// dotnet-webapi: Pervasive sealing
public sealed class SparkEventsDbContext : DbContext { }
public sealed class EventService(SparkEventsDbContext db, ...) : IEventService { }
public sealed class CreateModel(IEventService svc) : PageModel { }
public sealed class Event { }  // Even entity models sealed
```

```csharp
// dotnet-skills: Selective sealing
public sealed class EmployeeService : IEmployeeService { }  // Only services
public class Employee { }  // Entities NOT sealed
```

```csharp
// no-skills & managedcode: No sealing at all
public class EventService : IEventService { }
public class CreateModel : PageModel { }
```

**Verdict**: **dotnet-webapi** wins definitively. Sealing all non-extensible types enables JIT devirtualization and signals design intent. The dotnet-artisan config is close behind. Sealing entities (as dotnet-webapi does) is debatable for EF Core proxy scenarios, but for SQLite apps without lazy loading, it's a solid optimization.

---

## 14. Primary Constructors

Another **major differentiator**:

| Config | Primary constructor usage | Coverage |
|--------|:-:|---|
| no-skills | **0** | All traditional `private readonly` + constructor body |
| dotnet-webapi | **88** | Mixed — KeystoneProperties/HorizonHR use them; SparkEvents traditional |
| dotnet-artisan | **109** | Consistent — all services, page models, DbContext |
| managedcode | **115** | Consistent — all services, page models |
| dotnet-skills | **0** | All traditional constructor injection |

```csharp
// dotnet-artisan & managedcode: C# 12 primary constructors
public sealed class EventService(SparkEventsDbContext db, ILogger<EventService> logger) : IEventService
{
    public async Task<Event?> GetByIdAsync(int id) =>
        await db.Events.FirstOrDefaultAsync(e => e.Id == id);
}
```

```csharp
// no-skills & dotnet-skills: Traditional constructors
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

**Verdict**: **managedcode** and **dotnet-artisan** win. Primary constructors eliminate 3+ lines of boilerplate per class. The traditional pattern used by no-skills and dotnet-skills is verbose but universally understood.

---

## 15. Service Abstraction

All five configurations implement interface-based service abstractions with `AddScoped<IService, Service>()` registration:

```csharp
// Universal pattern across all configs
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IVenueService, VenueService>();
```

**Verdict**: Tie — all configurations follow this best practice. Interface file organization varies (single `Interfaces.cs` vs separate files per interface), but the pattern is consistent.

---

## 16. CancellationToken Propagation

This is the **single most impactful quality differentiator**:

| Config | CT occurrences | Scope |
|--------|:-:|---|
| no-skills | **4** | Only in `DbContext.SaveChangesAsync` override |
| dotnet-webapi | **420** | Full propagation: page handlers → services → EF Core |
| dotnet-artisan | **101** | Good — services accept CT with `= default`; page handlers pass CT |
| managedcode | **6** | Only in `DbContext.SaveChangesAsync` override |
| dotnet-skills | **4** | Only in `DbContext.SaveChangesAsync` override |

```csharp
// dotnet-webapi: Full CancellationToken propagation
// Page handler receives CT from framework
public async Task<IActionResult> OnPostAsync(CancellationToken ct)
{
    await _eventService.CreateAsync(evt, ct);
}

// Service propagates to EF Core
public async Task<Event> CreateAsync(Event evt, CancellationToken ct = default)
{
    db.Events.Add(evt);
    await db.SaveChangesAsync(ct);
    return evt;
}
```

```csharp
// no-skills: No CancellationToken at all
public async Task<IActionResult> OnPostAsync()  // No CancellationToken
{
    await _eventService.CreateAsync(evt);  // No CancellationToken
}
```

**Verdict**: **dotnet-webapi** wins decisively with 420 CancellationToken uses (full propagation through every layer). This is critical for production — cancelled HTTP requests will properly cancel database queries, preventing wasted server resources. The dotnet-artisan config is decent at 101. Three configs (no-skills, managedcode, dotnet-skills) essentially ignore cancellation.

---

## 17. AsNoTracking Usage

| Config | `.AsNoTracking()` count | Coverage |
|--------|:-:|---|
| no-skills | **0** | Never used — all queries tracked |
| dotnet-webapi | **53** | Consistent on read-only queries |
| dotnet-artisan | **69** | Consistent on read-only queries |
| managedcode | **72** | Most consistent |
| dotnet-skills | **70** | Consistent on read-only queries |

```csharp
// dotnet-artisan: AsNoTracking on read queries
public async Task<PaginatedList<Event>> GetEventsAsync(...)
{
    var query = db.Events
        .Include(e => e.EventCategory)
        .Include(e => e.Venue)
        .AsNoTracking()  // Halves memory usage for read queries
        .AsQueryable();
}
```

```csharp
// no-skills: No AsNoTracking — all queries tracked
return await _db.Events
    .Include(e => e.EventCategory)
    .Include(e => e.Venue)
    .FirstOrDefaultAsync(e => e.Id == id);
    // Tracking enabled — unnecessary for read-only display
```

**Verdict**: All skill-enhanced configs (dotnet-webapi, dotnet-artisan, managedcode, dotnet-skills) use `AsNoTracking()` consistently. **no-skills** is the outlier — every query tracks entities unnecessarily, doubling memory usage on list pages.

---

## 18. TempData & Flash Messages

All configurations implement TempData-based flash messages with the PRG pattern. The approach is essentially identical:

```csharp
// Universal pattern
TempData["SuccessMessage"] = "Event created successfully.";
return RedirectToPage("Details", new { id = evt.Id });
```

```html
<!-- Layout rendering — identical structure, minor variations in key names -->
@if (TempData["SuccessMessage"] is string successMsg)
{
    <div class="alert alert-success alert-dismissible fade show" role="alert">
        @successMsg
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    </div>
}
```

The **dotnet-webapi** config uses type-safe pattern matching (`TempData["SuccessMessage"] is string successMsg`) in all apps, while **no-skills** uses null check (`TempData["SuccessMessage"] != null`).

**Verdict**: Marginal win for configs using `is string` pattern matching (type-safe cast + null check in one step).

---

## 19. Data Seeder Design

| Config | Pattern | Async? | Idempotent? | Injectable? |
|--------|---------|:-:|:-:|:-:|
| no-skills | `static DataSeeder.Seed(DbContext)` | ❌ Sync | ✅ `if (Any()) return` | ❌ |
| dotnet-webapi | Static mixed (async in SparkEvents/HorizonHR, sync in KeystoneProperties) | ⚠️ Mixed | ✅ | ❌ |
| dotnet-artisan | Injectable in HorizonHR, static in others | ✅ | ✅ | ✅ (1 app) |
| managedcode | `static DataSeeder.SeedAsync(DbContext)` | ✅ | ✅ | ❌ |
| dotnet-skills | `static DataSeeder.SeedAsync(DbContext)` | ✅ | ✅ | ❌ |

```csharp
// dotnet-artisan: Injectable seeder (HorizonHR)
public sealed class DataSeeder(ApplicationDbContext db)
{
    public async Task SeedAsync()
    {
        if (await db.Departments.AnyAsync()) return;
        // ... seed data
    }
}

// Program.cs registration
builder.Services.AddScoped<DataSeeder>();
var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
await seeder.SeedAsync();
```

**Verdict**: **dotnet-artisan** shows the most mature approach with injectable seeders that follow DI conventions. The static pattern used by most configs is pragmatic but less testable.

---

## 20. Exception Handling Strategy

| Config | Approach | Custom middleware? | IExceptionHandler? |
|--------|----------|:-:|:-:|
| no-skills | `string?` error returns from services | ❌ | ❌ |
| dotnet-webapi | Custom middleware + `IExceptionHandler` + `InvalidOperationException` | ✅ (2 apps) | ✅ (HorizonHR) |
| dotnet-artisan | `string?` error returns from services | ❌ | ❌ |
| managedcode | `(bool, string?)` tuple returns from services | ❌ | ❌ |
| dotnet-skills | Try-catch with `InvalidOperationException` | ❌ | ❌ |

```csharp
// dotnet-webapi: Modern IExceptionHandler (HorizonHR)
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
        ctx.Response.StatusCode = ex switch
        {
            KeyNotFoundException => StatusCodes.Status404NotFound,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
        return false;
    }
}
```

```csharp
// no-skills: String-based error pattern
public async Task<string?> PublishAsync(int id)
{
    var evt = await _db.Events.FindAsync(id);
    if (evt == null) return "Event not found.";
    if (evt.Status != EventStatus.Draft) return "Only draft events can be published.";
    return null; // Success
}
```

**Verdict**: **dotnet-webapi** wins with the most sophisticated approach — `IExceptionHandler` for global error handling, custom exception middleware, and structured error categorization. The string-based pattern (no-skills, dotnet-artisan) is simple but loses type safety.

---

## 21. File Organization

All configurations use **feature-based folder structure** under `Pages/`:

```
Pages/
├── Events/        (or Employees/, Properties/, etc.)
├── Categories/    (or Departments/, Units/, etc.)
├── Registrations/ (or Leave/, Leases/, etc.)
└── Shared/        (_Layout, _StatusBadge, _Pagination)
```

The **dotnet-skills** HorizonHR app additionally organizes enums into a dedicated `Models/Enums/` subfolder and interfaces into `Services/Interfaces/`.

**Verdict**: Tie on the basic structure. **dotnet-skills** has the most organized model and service folders.

---

## 22. Pagination

All configurations implement a reusable `PaginatedList<T>` class and dedicated pagination partial views. The implementations are functionally identical.

```csharp
// Universal pattern — slight naming variations
public class PaginatedList<T> : List<T>
{
    public int PageIndex { get; }
    public int TotalPages { get; }
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;

    public static async Task<PaginatedList<T>> CreateAsync(
        IQueryable<T> source, int pageIndex, int pageSize)
    { /* Skip/Take with Count */ }
}
```

**Verdict**: Tie — all configurations provide reusable pagination.

---

## 23. Collection Initialization

| Config | `= []` expressions | `new List<T>()` | Dominant pattern |
|--------|:-:|:-:|---|
| no-skills | **0** | **46** | 100% traditional |
| dotnet-webapi | **71** | 41 | Mixed, leaning modern |
| dotnet-artisan | **76** | 16 | Mostly modern `[]` |
| managedcode | **95** | 14 | Mostly modern `[]` |
| dotnet-skills | **14** | 41 | Mostly traditional |

```csharp
// dotnet-artisan & managedcode: Modern C# 12 collection expressions
public List<SelectListItem> Categories { get; set; } = [];
public List<Event> Events { get; set; } = [];
```

```csharp
// no-skills & dotnet-skills: Traditional initialization
public List<Event> Events { get; set; } = new();
public ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();
```

**Verdict**: **managedcode** leads with 95 collection expressions. The `[]` syntax is more concise and recommended in C# 12+.

---

## 24. Custom Tag Helpers

**No configuration produces custom `TagHelper` classes.** All rely exclusively on built-in ASP.NET Core tag helpers and partial views for reusable UI.

**Verdict**: Tie at zero. This is a missed opportunity across all configurations — status badges and pagination could benefit from custom tag helpers.

---

## 25. Structured Logging

All configurations inject `ILogger<T>` and use structured message templates (not string interpolation):

| Config | `ILogger<T>` injection points |
|--------|:-:|
| no-skills | 24 |
| dotnet-webapi | 22 |
| dotnet-artisan | 12 |
| managedcode | 18 |
| dotnet-skills | **42** |

```csharp
// Universal structured logging pattern
logger.LogInformation("Created event: {EventTitle} (ID: {EventId})", evt.Title, evt.Id);
logger.LogInformation("Cancelled event {EventId}. Reason: {Reason}", id, reason);
```

No configuration uses `[LoggerMessage]` source generators (high-performance logging).

**Verdict**: **dotnet-skills** has the most logging points (42 injections). All configs correctly use structured templates, but none use source-generated `[LoggerMessage]`, which would eliminate allocations.

---

## 26. Nullable Reference Types

All configurations enable `<Nullable>enable</Nullable>` in `.csproj` and use proper `?` annotations:

```csharp
// Universal pattern
public string? CancellationReason { get; set; }      // Optional
public EventCategory EventCategory { get; set; } = null!;  // Required non-nullable
public Employee? Manager { get; set; }                 // Optional navigation
```

**Verdict**: Tie — all configurations handle nullable references correctly.

---

## 27. Global Using Directives

All configurations enable `<ImplicitUsings>enable</ImplicitUsings>` in `.csproj` but none create a `GlobalUsings.cs` file. This means each file has explicit `using` statements for project-specific namespaces.

**Verdict**: Tie — none create `GlobalUsings.cs`, all rely on implicit usings for framework namespaces.

---

## 28. Package Discipline

All configurations maintain minimal NuGet dependencies:

```xml
<!-- Universal — only EF Core packages needed -->
<ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*-*" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.*-*" />
</ItemGroup>
```

No configuration adds unnecessary packages (no Swashbuckle, no RuntimeCompilation, no extra ORM tools).

**Verdict**: Tie — all configurations are disciplined about dependencies.

---

## 29. EF Core Relationship Configuration

| Config | Fluent API | HasConversion | AsSplitQuery | Transactions | IEntityTypeConfiguration |
|--------|:-:|:-:|:-:|:-:|:-:|
| no-skills | ✅ | ❌ (0) | ❌ (0) | ❌ | ❌ |
| dotnet-webapi | ✅ | ✅ (20) | ❌ (0) | ❌ | ❌ |
| dotnet-artisan | ✅ | ⚠️ (2) | ✅ (4) | ❌ | ❌ |
| managedcode | ✅ | ⚠️ (2) | ⚠️ (1) | ✅ (1) | ❌ |
| dotnet-skills | ✅ | ❌ (0) | ✅ (11) | ❌ | ❌ |

```csharp
// dotnet-webapi: Enum-to-string conversion (20 usages!)
entity.Property(e => e.Status).HasConversion<string>();
entity.Property(p => p.PropertyType).HasConversion<string>();
```

```csharp
// dotnet-skills: AsSplitQuery for complex multi-Include queries (11 usages)
return await _db.Employees
    .Include(e => e.Department)
    .Include(e => e.Manager)
    .Include(e => e.DirectReports)
    .Include(e => e.EmployeeSkills).ThenInclude(es => es.Skill)
    .AsSplitQuery()  // Prevents cartesian explosion
    .FirstOrDefaultAsync(e => e.Id == id);
```

```csharp
// managedcode: Database transactions for complex seeding
await using var transaction = await context.Database.BeginTransactionAsync();
// ... seeding operations
await transaction.CommitAsync();
```

**Verdict**: **dotnet-webapi** leads with 20 `HasConversion<string>()` calls (making enum values human-readable in the database). **dotnet-skills** best handles N+1 query risks with 11 `AsSplitQuery()` calls. No configuration uses `IEntityTypeConfiguration<T>` for clean separation.

---

## 30. Naming Conventions

All configurations follow .NET naming guidelines correctly:
- ✅ PascalCasing for public types and members
- ✅ `_camelCase` for private fields
- ✅ `Async` suffix on all async methods
- ✅ `I` prefix on all interfaces
- ✅ Singular enum names (`EventStatus`, not `EventStatuses`)

**Verdict**: Tie — all configurations follow standard .NET naming conventions.

---

## 31. Enum Design

All configurations use enums for domain status fields with singular names:

```csharp
// Universal pattern
public enum EventStatus { Draft, Published, SoldOut, Completed, Cancelled }
public enum LeaseStatus { Pending, Active, Expired, Renewed, Terminated }
public enum EmploymentType { FullTime, PartTime, Contract, Intern }
```

The key difference is **persistence**: dotnet-webapi stores enums as strings via `HasConversion<string>()` (20 occurrences), while others default to integer storage.

**Verdict**: **dotnet-webapi** wins — string storage makes database values human-readable and migration-safe.

---

## 32. Guard Clauses & Argument Validation

**No configuration uses modern guard clause helpers** (`ArgumentNullException.ThrowIfNull()`, `ArgumentException.ThrowIfNullOrEmpty()`). All use manual null checks:

```csharp
// Universal pattern — manual null checks
var evt = await _eventService.GetByIdAsync(id);
if (evt is null) return NotFound();

if (evt.Status != EventStatus.Draft)
    return "Only draft events can be published.";
```

**Verdict**: Tie at zero — all configurations miss this modern .NET pattern. This represents a gap in all skill configurations.

---

## 33. Async/Await Best Practices

All configurations follow async/await best practices:
- ✅ All async methods have `Async` suffix
- ✅ No `async void` methods
- ✅ No sync-over-async patterns (`.Result`, `.Wait()`)
- ✅ `await` used consistently through the call chain

**Verdict**: Tie — all configurations handle async correctly.

---

## 34. Access Modifier Explicitness

All configurations use explicit access modifiers and file-scoped namespaces:

```csharp
namespace SparkEvents.Models;  // File-scoped (C# 10+)

public class Event  // Explicitly public
{
    public int Id { get; set; }
    private readonly SparkEventsDbContext _db;  // Explicitly private
}
```

**Verdict**: Tie — all configurations use explicit modifiers and file-scoped namespaces.

---

## 35. Dispose & Resource Management

All configurations rely on DI for `DbContext` lifecycle management. Explicit `using` blocks appear only for startup scope creation:

```csharp
// Universal pattern
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SparkEventsDbContext>();
    db.Database.EnsureCreated();
}
```

The **managedcode** config additionally uses `await using` for transactions:

```csharp
// managedcode: Async disposable transaction
await using var transaction = await context.Database.BeginTransactionAsync();
```

**Verdict**: **managedcode** demonstrates the best resource management with `await using` for transactions.

---

## What All Versions Get Right

- **Thin page models** — all delegate to injected services, never query `DbContext` directly
- **Interface-based service abstraction** — all use `AddScoped<IService, Service>()`
- **Input model separation** — nested `InputModel` classes prevent over-posting
- **PRG pattern** — all forms POST, set TempData, and redirect
- **Tag helpers** — 100% modern tag helpers, zero legacy `@Html.*` patterns
- **Feature-based organization** — Pages grouped by domain area
- **Bootstrap 5 integration** — comprehensive, responsive UIs
- **Nullable reference types** — `<Nullable>enable</Nullable>` with proper annotations
- **Proper async/await** — correct suffix naming, no async void, no sync-over-async
- **Minimal package discipline** — only EF Core SQLite dependencies
- **Standard .NET naming** — PascalCase, `I` prefix, `Async` suffix
- **Reusable pagination** — `PaginatedList<T>` class + partial views
- **Structured logging** — `ILogger<T>` with message templates, no string interpolation
- **Empty state handling** — "No items found" messages on all list pages
- **TempData flash messages** — success/error feedback after form submissions

---

## Summary: Impact of Skills

### Configuration Rankings

#### 1. 🥇 dotnet-webapi (Best Overall)
The **dotnet-webapi** skill produces the highest-quality code across the most impactful dimensions:
- **CancellationToken propagation** (420 uses) — the single most production-critical improvement
- **Sealed types** (187 declarations) — JIT optimization and design intent
- **Exception handling** — only config with `IExceptionHandler` middleware
- **HasConversion&lt;string&gt;** (20 uses) — human-readable enum storage
- **Primary constructors** — reduces boilerplate (88 uses)

#### 2. 🥈 dotnet-artisan (Strong Runner-Up)
The **dotnet-artisan** plugin chain excels in modern C# idioms:
- **Primary constructors** (109 uses) — most consistent adoption
- **Sealed types** (136 declarations) — strong design intent signaling
- **AsNoTracking** (69 uses) — excellent read-query optimization
- **CancellationToken** (101 uses) — partial propagation (service layer but not all page handlers)
- **IValidatableObject** — proper cross-field validation pattern
- **AsSplitQuery** (4 uses) — prevents cartesian explosion

#### 3. 🥉 managedcode-dotnet-skills (Modern C# Champion)
The **managedcode** skills excel at modern language features but miss performance-critical patterns:
- **Primary constructors** (115 uses) — highest adoption
- **Collection expressions** (95 `[]` uses) — most modern syntax
- **AsNoTracking** (72 uses) — best read-query coverage
- **ViewComponent** — only config to create proper ViewComponents
- **ARIA accessibility** — highest attribute density (51 `aria-label`)
- **Weakness**: no CancellationToken propagation (6 uses), no sealed types

#### 4. dotnet-skills (Solid Foundation)
The **dotnet-skills** configuration is thorough but conservative:
- **Structured logging** (42 ILogger injections) — most comprehensive logging
- **AsSplitQuery** (11 uses) — best N+1 query prevention
- **Semantic HTML** (52 `<section>` elements) — strong document structure
- **Named handlers** (8) — proper multi-action page support
- **Weakness**: no primary constructors, no sealed types, no CancellationToken

#### 5. no-skills (Baseline)
The **no-skills** baseline delivers surprisingly solid code with clean architecture, but misses every modern optimization:
- **Zero AsNoTracking** — every read query tracks entities
- **Zero CancellationToken** — cancelled requests waste server resources
- **Zero sealed types** — misses JIT optimization opportunities
- **Zero collection expressions** — exclusively traditional `new List<T>()`
- **Strength**: most semantic HTML (59 `<section>` elements) and clean separation of concerns

### Most Impactful Differences (Ranked)

1. **CancellationToken propagation** — dotnet-webapi (420) vs no-skills (4). This is the most production-critical gap. Without CancellationToken, cancelled HTTP requests continue executing database queries, wasting server resources and database connections.

2. **AsNoTracking** — All skill configs (53–72) vs no-skills (0). The baseline tracks every entity on every read, doubling memory usage on list pages. This is the most common EF Core performance mistake.

3. **Sealed types** — dotnet-webapi (187) and dotnet-artisan (136) vs no-skills (0). Sealing enables JIT devirtualization, particularly impactful for hot-path service methods.

4. **Primary constructors** — managedcode (115) and dotnet-artisan (109) vs no-skills (0). This eliminates ~3 lines of boilerplate per class, affecting code readability and maintenance cost.

5. **Exception handling** — dotnet-webapi is the only config with proper `IExceptionHandler` middleware. All others leave unhandled exceptions to the default error page.

6. **HasConversion&lt;string&gt;** — dotnet-webapi (20) makes enum values human-readable in the database. Other configs store enums as integers, which are harder to debug and migrate.

### Overall Assessment

Skills make a significant, measurable difference in code quality. The **dotnet-webapi** skill produces the most production-ready code by addressing performance-critical patterns (CancellationToken, sealed types) and operational concerns (exception handling, enum storage). The **dotnet-artisan** plugin chain is the strongest for modern C# idioms (primary constructors, sealed types, IValidatableObject). The **managedcode** skills excel at modern syntax but miss performance patterns. The **no-skills** baseline produces functional, well-structured code but misses every optimization opportunity that distinguishes hobby projects from production applications.
