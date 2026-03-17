# Code Generation Notes — Skills Used

This document describes the skills and reference materials used during the generation of the three ASP.NET Core Web API projects in this repository.

## Apps Generated

| App | Location | Description |
|-----|----------|-------------|
| Fitness Studio API | `src/FitnessStudioApi/` | Zenith Fitness Studio — membership, class scheduling, and booking management |
| Library API | `src/LibraryApi/` | Sunrise Community Library — books, loans, reservations, and fines management |
| Vet Clinic API | `src/VetClinicApi/` | Happy Paws Veterinary Clinic — pets, appointments, medical records, and vaccinations |

All three target **.NET 10** with **Entity Framework Core + SQLite**.

---

## Skills Invoked

### 1. `using-dotnet`
**Purpose:** .NET intent detection and version routing.

Detected .NET intent from the prompt keywords (ASP.NET Core, Web API, EF Core, .csproj) and repository signals. Verified the .NET 10 SDK was available and initiated the skill routing chain.

### 2. `dotnet-advisor`
**Purpose:** Domain skill routing.

Analyzed the task requirements (three backend API projects) and routed to the correct domain skills: `dotnet-csharp` for baseline C# coding standards and `dotnet-api` for ASP.NET Core API and EF Core patterns. This ensured the right guidance was loaded before any code was generated.

### 3. `dotnet-csharp`
**Purpose:** Baseline C# coding standards and language patterns.

Loaded and enforced the following standards across all three projects:

- **Naming conventions** — PascalCase for public members, `_camelCase` for private fields, `Async` suffix on async methods
- **File organization** — File-scoped namespaces (`namespace X;`), one type per file
- **Code style** — Explicit access modifiers, `var` when type is obvious, `is not null`/`is null` over `!=`/`==`, string interpolation, sealed classes
- **Async patterns** — Async all the way, `CancellationToken` forwarding to all downstream async calls, no `.Result`/`.Wait()` blocking
- **Code smell avoidance** — No empty catch blocks, no `throw ex;` (use `throw;`), guard clauses over deep nesting, no premature `.ToList()` in LINQ chains
- **.NET 10 / C# 14 awareness** — Used .NET 10-specific APIs and patterns where applicable

**Key reference files used:**
- `coding-standards.md`
- `async-patterns.md`
- `code-smells.md`
- `dotnet-releases.md`

### 4. `dotnet-api`
**Purpose:** ASP.NET Core API, EF Core, and backend service patterns.

Loaded and applied patterns for:

- **EF Core** — DbContext as scoped, `AsNoTracking()` for read-only queries, proper seeding with `EnsureCreated()`, Fluent API configuration, unique indexes, cascade delete behavior
- **Middleware** — Exception handler as outermost middleware, RFC 7807 ProblemDetails for error responses, correct pipeline ordering
- **Controllers** — Thin controllers delegating to service layer, interface + implementation pattern, DTOs for all API surfaces (never exposing raw entities)
- **OpenAPI** — `AddOpenApi()`/`MapOpenApi()` for .NET 10 built-in support plus Swashbuckle for Swagger UI
- **JSON configuration** — `ConfigureHttpJsonOptions` with `JsonStringEnumConverter` for enum serialization
- **Agent gotchas** — Avoided common mistakes: wrong SDK type, packages already in shared framework, hardcoded connection strings, singleton capturing scoped services

**Key reference files used:**
- `efcore-patterns.md`
- `minimal-apis.md`
- `middleware-patterns.md`
- `agent-gotchas.md`

---

## How Skills Influenced the Output

| Concern | Skill | What It Enforced |
|---------|-------|-----------------|
| Naming & style | `dotnet-csharp` | PascalCase, file-scoped namespaces, explicit access modifiers |
| Async correctness | `dotnet-csharp` | CancellationToken forwarding, no blocking on async |
| EF Core lifecycle | `dotnet-api` | Scoped DbContext, AsNoTracking for reads, proper seeding |
| Error handling | `dotnet-api` | Global exception middleware, ProblemDetails (RFC 7807) |
| API design | `dotnet-api` | DTOs over raw entities, pagination patterns, validation |
| Pipeline ordering | `dotnet-api` | Exception handler first, correct middleware sequence |
| Anti-pattern avoidance | `dotnet-csharp` + `dotnet-api` | No `.Result`, no empty catches, no wrong SDK type |
| .NET 10 awareness | `dotnet-csharp` | Correct TFM, C# 14 features, .NET 10 OpenAPI APIs |

---

## Generation Details

- **Date:** 2026-03-17
- **.NET SDK:** 10.0.200
- **Build status:** All three projects build with 0 warnings, 0 errors
- **Generation approach:** Three parallel background agents, each building one API independently with the same skill-loaded coding standards
