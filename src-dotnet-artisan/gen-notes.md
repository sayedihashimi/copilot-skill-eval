# Code Generation Notes — dotnet-artisan Skills Usage

Generated on: 2026-03-19

## Overview

Three ASP.NET Core Web API applications were generated using `dotnet-artisan` skills. Each app was built in an **isolated context** with no shared knowledge between agents.

---

## Skills Used Per Application

### 1. Fitness Studio API (`src-dotnet-artisan/FitnessStudioApi/`)

| Skill | Purpose |
|-------|---------|
| **using-dotnet** | .NET intent detection and routing gateway |
| **dotnet-advisor** | Domain routing to correct API/tooling skills |
| **dotnet-csharp** | C# coding standards baseline (records, pattern matching, async/await, DI) |
| **dotnet-api** | ASP.NET Core API patterns, EF Core data access, OpenAPI configuration |
| **dotnet-tooling** | Project setup, SDK version detection, build optimization |

### 2. Library API (`src-dotnet-artisan/LibraryApi/`)

| Skill | Purpose |
|-------|---------|
| **using-dotnet** | .NET intent detection and routing gateway |
| **dotnet-advisor** | Domain routing to correct API/tooling skills |
| **dotnet-csharp** | C# coding standards baseline (records, pattern matching, async/await, DI) |
| **dotnet-api** | ASP.NET Core API patterns, EF Core data access, OpenAPI configuration |
| **dotnet-tooling** | Project setup, SDK version detection, build optimization |

### 3. Vet Clinic API (`src-dotnet-artisan/VetClinicApi/`)

| Skill | Purpose |
|-------|---------|
| **using-dotnet** | .NET intent detection and routing gateway |
| **dotnet-advisor** | Domain routing to correct domain skills |
| **dotnet-csharp** | C# coding standards baseline (records, pattern matching, async/await, DI) |
| **dotnet-api** | ASP.NET Core API patterns, EF Core data access, OpenAPI configuration |
| **dotnet-tooling** | Project setup, SDK version detection, build optimization |

---

## Skills Summary

All three applications used the same five `dotnet-artisan` skills:

1. **using-dotnet** — Entry point skill that detects .NET intent and loads version-specific coding standards
2. **dotnet-advisor** — Routes requests to the correct domain skills based on project signals
3. **dotnet-csharp** — Baseline C# skill providing language patterns, coding standards, async/await, DI, and LINQ guidance
4. **dotnet-api** — ASP.NET Core API construction including EF Core, middleware, OpenAPI, error handling, and architecture patterns
5. **dotnet-tooling** — .NET SDK management, project setup, build configuration, and version detection

**Note**: The `dotnet-webapi` skill was explicitly **not used** as per instructions.

---

## Build Verification

All three projects compiled successfully with **0 warnings and 0 errors**:

| Project | Build Result |
|---------|-------------|
| FitnessStudioApi | ✅ Build succeeded |
| LibraryApi | ✅ Build succeeded |
| VetClinicApi | ✅ Build succeeded |
