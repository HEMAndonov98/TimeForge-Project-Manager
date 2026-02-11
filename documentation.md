# TimeForge Project Documentation

This document provides a detailed overview of the TimeForge application architecture, business logic implementation, and guidance for migrating from an ASP.NET Core Web App to a Web API.

---

## Architecture Overview

TimeForge follows a N-Layer architecture pattern, separating concerns into distinct projects:

- **TimeForge.Common**: Shared utilities, constants, and enums.
- **TimeForge.Infrastructure**: Data access layer, EF Core configurations, migrations, and repositories.
- **TimeForge.Models**: Domain entities and base models.
- **TimeForge.Services**: Core business logic.
- **TimeForge.ViewModels**: User-facing data structures (to be migrated to DTOs).
- **TimeForge.Web**: Current MVC Web App project (to be migrated to Web API).
- **TimeForge.Tests**: Unit and integration tests.

---

## 1. TimeForge.Common
**Purpose**: Contains shared logic and definitions used across all layers.

- **Constants**: Centralized strings for validation limits, error messages, and system settings (e.g., `ProjectValidationConstants.cs`).
- **Enums**: Shared enumerables like `TimeEntryState` (Running, Paused, Completed).
- **GlobalErrorMessages**: Standardized error strings to ensure consistency across the UI and API.
- **Dto Validation**: Common validation logic for data transfer objects.

---

## 2. TimeForge.Infrastructure
**Purpose**: Manages data persistence and database-related infrastructure.

- **Configurations**: Fluent API configurations for entity relationships (e.g., One-to-Many between Project and Tasks).
- **Interceptors**: 
    - `SoftDeleteInterceptor`: Automatically intercepts `Delete` operations and converts them to soft deletes (`IsDeleted = true`), setting a `DeletedAt` timestamp.
- **Migrations**: Standard EF Core migration files tracking database schema evolution.
- **Repositories**:
    - **Common**: `BaseRepository<TContext>` provides a generic implementation of standard CRUD operations (`AddAsync`, `Update`, `Delete`, `GetByIdAsync`, `All`).
    - **Interfaces**: `IRepository` and project-specific interfaces like `ITimeForgeRepository`.
- **Seeders**: Classes like `TagSeeder` and `ProjectSeeder` populate the database with initial/test data.
- **DbInitializer**: Logic to trigger seeders in the correct order (Tags -> Projects -> Tasks).
- **TimeForgeDbContext**: The core EF Core context, inheriting from `IdentityDbContext<User>` for built-in authentication support.

---

## 3. TimeForge.Models
**Purpose**: Defines the domain entities.

- **Entity Models**: `Project`, `Tag`, `ProjectTask`, `TimeEntry`, `User`.
- **Base Models**: Located in `Common/`, these include `BaseModel` (Id, CreatedAt, LastModified) and `BaseDeletableModel` (IsDeleted, DeletedAt) for audit and soft-delete capabilities.

---

## 4. TimeForge.Services
**Purpose**: Implements core business logic.

### Detailed Service Descriptions:

- **ProjectService**: 
    - **Purpose**: Manages the lifecycle of projects and their assignments.
    - **Implementation**: Uses `ITimeForgeRepository` for DB operations. Includes logic for paging, filtering by tags, and assigning users.
    - **Improvement Tip**: Moving projection logic (`Select(p => new ProjectViewModel...)`) into a dedicated mapping layer (e.g., AutoMapper) would keep the service cleaner.

- **TaskService**: 
    - **Purpose**: Handles task creation, completion status, and retrieval.
    - **Implementation**: Manages state changes for tasks and links them to projects.
    - **Improvement Tip**: Implement "Task Reordering" or "State Transition" logic to restrict completion if pre-requisites aren't met.

- **TagService**: 
    - **Purpose**: Manages reusable tags for project categorization.
    - **Implementation**: Simple CRUD operations with user-specific ownership checks.
    - **Improvement Tip**: Add "Tag Merging" functionality to clean up duplicate tags.

- **TimeEntryService**: 
    - **Purpose**: The "Stopwatch" logic of the app.
    - **Implementation**: Complex logic for pausing/resuming. It automatically pauses any existing running timer for a user when a new one starts to prevent overlaps.
    - **Improvement Tip**: Implement a "Time Gap" analysis to alert users if they have large unrecorded gaps in their workday. Use a formal "State Pattern" if the timer logic becomes more complex.

- **ConnectionService**: Manages social/team connections between users.

---

## 5. TimeForge.Tests
**Purpose**: Ensures correctness through unit tests.

- **Current State**: Uses NUnit, Moq, and EF Core `UseInMemoryDatabase`.
- **Improvement for API**:
    - Switch to `WebApplicationFactory` for integration testing that covers the HTTP pipeline, middleware, and authentication filters.
    - Ensure tests validate HTTP status codes (200 OK, 201 Created, 400 BadRequest) rather than just service results.

---

## 6. TimeForge.ViewModels
**Purpose**: Data structures for the MVC Views.

- **Current Pattern**: Heavy use of `InputModel` and `ViewModel`.
- **Improvement for API**:
    - Convert ViewModels to **DTOs (Data Transfer Objects)**. 
    - Remove UI-specific properties (like `IsLoggedIn` or `Layout` flags).
    - Use System.Text.Json attributes for better control over JSON serialization.

---

## 7. TimeForge.Web (Migration to Web API)

### Converting Program.cs
1. **Remove**: `builder.Services.AddControllersWithViews()` and `app.MapRazorPages()`.
2. **Add**: `builder.Services.AddControllers()` and `builder.Services.AddEndpointsApiExplorer()` with `AddSwaggerGen()`.
3. **Identity**: Use JWT Bearer authentication instead of cookie-based authentication. Use `builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)`.
4. **Middleware**: Remove `app.UseStaticFiles()` if the API is headless.

### Converting Controllers
1. **Inheritance**: Change `Controller` to `ControllerBase`.
2. **Attributes**: Add `[ApiController]` to all controllers for automatic model validation and binding.
3. **Actions**: Replace `IActionResult` with specific `ActionResult<T>` and use standard HTTP verbs (`[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`).
4. **Responses**: Use `CreatedAtAction`, `Ok()`, `NoContent()`, and `BadRequest(ModelState)` instead of `View()`.

### Files for Removal
- `Views/` directory.
- `wwwroot/` directory (css, js, lib).
- `ViewComponents/`.
- `Areas/` (if they were strictly for SSR).
- `_Layout.cshtml` and other Razor partials.
