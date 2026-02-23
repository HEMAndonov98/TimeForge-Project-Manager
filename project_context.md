# TimeForge Project Context & REAPR Guide

## 🚀 Core Tech Stack
- **Framework**: ASP.NET Core 8.0+
- **Architecture**: Vertical Slice Architecture (VSA)
- **API Library**: [FastEndpoints](https://fast-endpoints.com/)
- **Testing**: Xunit v3, [FastEndpoints.Testing](https://fast-endpoints.com/docs/unit-testing), [Shouldly](https://shouldly.io/)
- **Database**: Entity Framework Core (Current: **InMemory**, Future: SQL Provider)
- **Authentication**: JWT Bearer Tokens (ASP.NET Identity)

---

## 🏗️ REAPR Pattern & Project Structure
Each feature is self-contained in a "Vertical Slice".
Location: `TimeForge.Api/Features/{FeatureName}/{ActionName}/`

### Components of a Slice:
1. **Request**: `*Request.cs` (DTO for incoming data, uses FluentValidation or DataAnnotations)
2. **Endpoint**: `*Endpoint.cs` (Inherits `Endpoint<TRequest, TResponse>`, handles routing and orchestration)
3. **Action**: Business logic (Implemented directly in Endpoint or Domain entities for simple cases)
4. **Persistence**: `TimeForgeDbContext` (EF Core access)
5. **Response**: `*Response.cs` or `*Dto.cs` (DTO for outgoing data)

### Folder Hierarchy:
```
TimeForge.Api/
├── Features/
│   ├── Auth/
│   │   ├── Login/
│   │   ├── Register/
│   │   └── GetMe/
│   ├── Projects/
│   │   ├── GetAll/
│   │   ├── Create/
│   │   └── ...
├── Common/ (Extensions, Shared Models)
├── Hubs/ (SignalR)
TimeForge.Tests/
├── Feature Tests/
│   ├── Auth/
│   ├── Projects/
```

---

## 🎨 FastEndpoints Coding Standards
- **Responses**: ALWAYS use the static `Send` class.
  - `await Send.OkAsync(response, ct);`
  - `await Send.CreatedAtAsync<TargetEndpoint>(routeValues, response, ct);`
  - `await Send.NotFoundAsync(ct);`
- **Error Handling**:
  - `ThrowError("Message", 404);` for immediate exits.
  - `AddError(r => r.Prop, "Msg"); ThrowIfAnyErrors();` for validation.
- **User Context**: Use `User.GetUserId()` extension to access the current authenticated user.
- **Configuration**: Use `Configure()` method for routing, permissions, and Swagger documentation.

---

## 🧪 Testing Strategy
**CRITICAL RULE: 100% Test Coverage for each feature before proceeding to the next.**

- **Framework**: Xunit v3
- **Base Class**: `TestBase<TimeForgeFixture>`
- **Assertions**: Shouldly (`result.ShouldBe(expected)`)
- **Integration**: Use `app.Client` from `TimeForgeFixture` to make real HTTP calls to the in-memory server.

---

## 📈 Feature Roadmap & Status (REAPR Order)

| Feature | Status | Description |
| :--- | :--- | :--- |
| **1. Auth** | ✅ DONE | Register, Login, GetMe. (FastEndpoints.Tests implemented) |
| **2. Projects** | ✅ DONE | CRUD for Projects. Soft delete support. (VSA slices & tests implemented) |
| **3. Tasks** | 🔜 NEXT | Update Task Status. |
| **4. Calendar**| 🔜 TODO | Get/Create Calendar Events. |
| **5. Teams** | 🔜 TODO | Team management, Roles (Manager/Member). |
| **6. Timer** | 🔜 TODO | Start/Stop tracking, One active timer per user. |
| **7. Chat** | 🔜 TODO | Conversations, DM vs Team, Friendships. |
| **8. Realtime**| 🔜 TODO | SignalR Hub for Chat & Presence. |

---

## 📝 Persistence & Domain Rules
- **Base Entity**: Most entities inherit from `BaseDeletableEntity<string>`.
- **Soft Delete**: Global query filter is active. `_db.Remove(entity)` triggers soft delete via interceptor.
- **Computed Properties**: Use domain properties like `Project.Progress` or `User.FullName` for logic.
- **Database**: Currently using `UseInMemoryDatabase("TimeForgeDb")` in `Program.cs`.

---

## 💡 Lessons Learned (Reworks)

During the implementation of Feature 2 (Projects), several architectural refinements were made to ensure strict adherence to VSA and FastEndpoints best practices:

1.  **Dedicated DTOs Over Anonymous Objects**:
    - **Principle**: Every endpoint MUST have a dedicated `Request` and `Response` object.
    - **Reasoning**: Avoids `EndpointWithoutRequest` even for GET requests. Standardizes how route parameters (e.g., `{id}`) are bound to the request object, improving maintainability and Swagger documentation.
2.  **Concrete Test Instantiations**:
    - **Principle**: In integration tests, always instantiate the concrete `Request` object instead of using anonymous types.
    - **Reasoning**: Ensures type safety and aligns the test code with the actual API contract.
3.  **Future-Proofing Responses**:
    - **Principle**: Even if a `Create` and `GetById` response look identical initially, they should use separate DTOs.
    - **Reasoning**: Allows each slice to evolve independently without unintended side effects on other features.
