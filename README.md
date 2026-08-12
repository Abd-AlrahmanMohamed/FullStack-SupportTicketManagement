# Support Ticket Management System

A full-stack support ticket management system built as a technical assessment: ASP.NET Core 8 Web API backend (Clean Architecture + CQRS + MediatR + Generic Repository + AutoMapper + EF Core) and an Angular 17 frontend (Angular Material).

Three roles are supported end-to-end with backend-enforced authorization and customer data isolation: **Admin**, **Support Agent**, **Customer**.

## Tech Stack

**Backend**
- ASP.NET Core Web API on .NET 8, C#
- Entity Framework Core 8, SQL Server
- JWT Bearer authentication, role-based authorization
- MediatR 12 (CQRS), AutoMapper 13, FluentValidation 11
- Generic Repository + Unit of Work over EF Core
- Swagger / OpenAPI (Swashbuckle) with JWT support
- xUnit, FluentAssertions, `Microsoft.AspNetCore.Mvc.Testing`, SQLite (in-memory, tests only)

**Frontend**
- Angular 17 (standalone components, lazy-loaded routes)
- Angular Material, Reactive Forms, RxJS
- `ng2-charts` / Chart.js for the dashboard chart
- Karma/Jasmine for unit tests

## Architecture

The backend follows **Clean Architecture** with a strict one-way dependency rule: `Domain ← Application ← Infrastructure ← Api`.

- **`SupportTickets.Domain`** — entities (`User`, `Ticket`, `TicketComment`, `TicketActivity`, `TimeEntry`) and enums (`UserRole`, `TicketStatus`, `TicketPriority`). No dependencies on any other project.
- **`SupportTickets.Application`** — CQRS commands/queries and their MediatR handlers organized by feature folder (`Features/Auth`, `Features/Users`, `Features/Tickets`, `Features/Dashboard`), DTOs, AutoMapper profile, repository/unit-of-work/JWT/current-user *interfaces*, FluentValidation validators, and the two pieces of business logic that are genuinely shared: `TicketStatusRules` (the status state machine) and `TicketAccessGuard` (the customer/agent ownership check). References only `Domain` (plus the EF Core package purely for the async LINQ extensions used against `IQueryable<T>` — the actual `DbContext` still lives in Infrastructure).
- **`SupportTickets.Infrastructure`** — `ApplicationDbContext` and Fluent API configurations, the EF Core-backed `GenericRepository<T>` / `UnitOfWork`, the JWT token service, the ASP.NET Core Identity password hasher, and the seed data. References `Application` and `Domain`.
- **`SupportTickets.Api`** — thin controllers that only call `mediator.Send(...)`, JWT/authorization configuration, the `ICurrentUserService` implementation (reads claims off `HttpContext`), centralized exception-handling middleware, Swagger, and DI composition (`Program.cs`). References `Application` and `Infrastructure`.
- **`SupportTickets.Tests`** — unit tests against the Application layer (business rules) and integration tests that boot the real API pipeline via `WebApplicationFactory` against a private in-memory SQLite database.

### Why CQRS

Commands (`CreateTicketCommand`, `UpdateTicketStatusCommand`, ...) and queries (`GetTicketsQuery`, `GetTicketByIdQuery`, ...) are separated so each use case is an explicit, single-purpose class with its own handler. There's no generic "TicketService" god-class accumulating unrelated methods — each business operation is independently readable, testable, and diffable in a PR. This project deliberately stops there: no separate read/write databases, no event sourcing, no custom mediator — just MediatR wiring a request to exactly one handler.

### Repository Pattern

`Application` depends only on `IGenericRepository<T>` and `IUnitOfWork` (both tiny — six repository methods, one `SaveChangesAsync`). `Infrastructure` provides the EF Core-backed implementation. Application handlers compose LINQ against `Query()` (an `IQueryable<T>`) for filtering/sorting/pagination/projection, rather than the generic repository growing a bespoke method per query shape — this is the explicit trade-off called out in the assignment ("acceptable to use `Query()` and compose LINQ in handlers").

### AutoMapper

One `MappingProfile` maps entities to DTOs (`User → UserDto`, `Ticket → TicketDto` / `TicketDetailsDto`, `TicketComment → CommentDto`, `TicketActivity → ActivityDto`, `TimeEntry → TimeEntryDto`). EF entities are never returned from an endpoint. The only "logic" inside the profile is straightforward projection (enum-to-string, `Sum(DurationMinutes)` for the total-time field) — anything resembling a business rule (status transitions, ownership checks) lives in the Application layer instead.

### EF Core

Code-first with an initial migration (`Persistence/Migrations`), Fluent API configuration per entity (required fields, string lengths, unique index on `Users.Email`, unique index on `Tickets.TicketNumber`, indexes on `Status`/`Priority`/`CreatedAt`), `AsNoTracking()` on read queries, and server-side pagination/filtering/searching/sorting/aggregation (dashboard counts are five scoped `COUNT` queries plus one narrow two-column projection for the average-resolution-time calculation — never a full table load).

## API

All endpoints are under `/api` and (except `/api/auth/login`) require `Authorization: Bearer <token>`.

| Endpoint | Roles | Notes |
|---|---|---|
| `POST /api/auth/login` | anyone | returns JWT + user |
| `GET /api/users` | Admin | |
| `GET /api/users/support-agents` | Admin | for the assign-ticket dropdown |
| `POST /api/users` | Admin | create Admin/SupportAgent/Customer |
| `POST /api/tickets` | Customer | `CustomerId` always comes from the JWT |
| `GET /api/tickets` | any | paginated, filtered, searched, sorted; results scoped by role |
| `GET /api/tickets/{id}` | any | 404 if not yours to see |
| `PATCH /api/tickets/{id}/assign` | Admin | |
| `PATCH /api/tickets/{id}/status` | Admin, SupportAgent | validated state machine |
| `PATCH /api/tickets/{id}/priority` | Admin | |
| `POST /api/tickets/{id}/comments` | any | ownership enforced |
| `POST /api/tickets/{id}/time-entries` | SupportAgent | only on tickets assigned to you |
| `GET /api/tickets/{id}/time-entries` | Admin, SupportAgent | |
| `GET /api/tickets/{id}/timeline` | any | ownership enforced |
| `POST /api/tickets/{id}/close` | Admin, Customer | only from `Resolved` |
| `GET /api/dashboard` | Admin | |

## Security

- **Password hashing**: ASP.NET Core Identity's `PasswordHasher<T>` (PBKDF2). `PasswordHash` is never mapped onto a DTO.
- **JWT**: contains the user's id, email, and role; validated (issuer, audience, lifetime, signing key) on every request via `AddJwtBearer`.
- **Role authorization**: enforced with `[Authorize(Roles = "...")]` on controller actions — never checked "by hand" inside a controller.
- **Customer/agent data isolation** (the most important requirement here): enforced in the Application layer, not the UI.
  - `CreateTicketCommandHandler` takes `CustomerId` from `ICurrentUserService`, never from the request body — a spoofed `customerId` in the JSON body is silently ignored.
  - `TicketAccessGuard.EnsureCanAccess` is the single choke point used by every ticket-scoped handler (`GetTicketById`, `AddComment`, `AddTimeEntry`, `CloseTicket`, `GetTicketTimeline`, `GetTicketTimeEntries`, `UpdateTicketStatus`): Admin passes unconditionally, a Customer must own the ticket, an Agent must be its assigned agent — anyone else gets a **404**, not a 403, so an unauthorized caller can't even confirm the ticket exists.
  - `TicketAccessGuard.ScopeToCurrentUser` applies the same rule to the ticket *list* query, as a `WHERE` clause applied before any other filter — every other filter/search/sort param can only narrow the result set further, never widen it past what the role is allowed to see.
  - Angular route guards (`authGuard`, `roleGuard`) only hide navigation/UI; they are explicitly documented in code as **not** a security boundary.
- **Ticket status state machine**: centralized in `TicketStatusRules` (`Open → InProgress → Resolved → Closed`, with `InProgress ↔ Open` and `Resolved ↔ InProgress` corrections allowed). Closing is deliberately its own use case (`CloseTicketCommand`), only reachable from `Resolved`, callable by Admin or the owning Customer — not Support Agents.
- **Centralized exception handling**: one middleware maps `NotFoundException → 404`, `ForbiddenException → 403`, `ValidationException`/`BusinessRuleException → 400`, `UnauthorizedAccessException → 401`, anything else → `500` with a generic message (the real exception is logged server-side via `ILogger`, never returned to the client).
- **Logging**: login failures, ticket creation/assignment/status changes, and unhandled exceptions are logged via `ILogger<T>`. Passwords, JWTs, and connection strings are never logged.

## Setup

### Prerequisites
- .NET 8 SDK
- Node.js 18+ and npm
- A SQL Server instance reachable from your machine (a local SQL Server, LocalDB, or the Docker command below all work)

### Backend

```bash
# from the repo root
dotnet restore

# local, version-pinned dotnet-ef (avoids picking up an incompatible global version)
dotnet tool restore

# point the API at your SQL Server instance and a real JWT secret (never commit either)
dotnet user-secrets init --project backend/SupportTickets.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=SupportTicketManagement;User Id=sa;Password=<yours>;TrustServerCertificate=True;" --project backend/SupportTickets.Api
dotnet user-secrets set "Jwt:Secret" "<a random string, at least 32 characters>" --project backend/SupportTickets.Api

# apply the EF Core migration (creates the database)
dotnet ef database update --project backend/SupportTickets.Infrastructure --startup-project backend/SupportTickets.Api

# run the API (also seeds the accounts/tickets below on first run)
dotnet run --project backend/SupportTickets.Api
```

The API listens on the URL printed in the console (Swagger UI at `/swagger`). Don't have SQL Server installed locally? This is exactly what was used to develop and verify this project end-to-end:

```bash
docker run -d --name support-tickets-sql -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=<yours>" -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
```

### Frontend

```bash
cd frontend/support-tickets-ui
npm install
ng serve
```

The app opens at `http://localhost:4200` and expects the API at `http://localhost:5199/api` (see `src/environments/environment.ts` — change it if your API runs on a different port).

## Test Accounts

Seeded automatically on first run (development-only passwords — change them before ever pointing this at a real deployment):

| Role | Email | Password |
|---|---|---|
| Admin | `admin@support.local` | `Admin@123` |
| Support Agent | `agent@support.local` | `Agent@123` |
| Support Agent | `agent2@support.local` | `Agent@123` |
| Customer | `customer@support.local` | `Customer@123` |
| Customer | `customer2@support.local` | `Customer@123` |

Two customers are seeded specifically so the data-isolation behavior is easy to demonstrate: log in as `customer@support.local`, note a ticket ID, then log in as `customer2@support.local` and try to fetch it — the API returns `404`.

## Running Tests

```bash
# Backend: 35 unit tests + 8 integration tests
dotnet test

# Frontend
cd frontend/support-tickets-ui
ng test --watch=false --browsers=ChromeHeadless
```

Both suites passed as of the last run in this environment (43/43 backend, 11/11 frontend).

## Assumptions

- **Ticket number generation** (`TKT-000001`, ...) is `COUNT(*) + 1` at creation time — simple and adequate for this assessment, but not collision-proof under heavy concurrent writes; a production system would use a database sequence instead.
- **Time entries are hidden from customers**; only the computed `totalTimeMinutes` is shown to them on the ticket details view. The requirement only explicitly restricts this to "Admin may view time entries," so this is a reasonable interpretation, not a stated rule.
- **`Open → Closed` and `Resolved → Closed` are only reachable via the dedicated `POST /tickets/{id}/close` endpoint**, not the general status-update endpoint — this keeps the state machine and its authorization story in one place instead of two.
- **Support Agents cannot close tickets** — only Admin or the owning Customer can, and only when the ticket is `Resolved`.
- **Admin does not log time** (per "Support Agents can log time... Admin may *view*"), enforced with `[Authorize(Roles = "SupportAgent")]` on that endpoint.
- **`GET /api/users`** returns the full list unpaginated — acceptable for the small user counts in this assessment; ticket listing (the endpoint that actually needs it) has full pagination/filter/search/sort.
- The `AutoMapper` NuGet package triggers an `NU1903` build warning. It is a licensing-model advisory from the maintainer (their >free-tier commercial license terms), not a code security vulnerability — noted here so it isn't mistaken for one during review.

## Limitations / Incomplete Requirements

- **The UI was not visually clicked through in a browser during this session** — no browser automation tool was available in this environment. It was instead verified by: a successful `ng build` and `ng test` run, passing Angular unit tests against the real services, and a full pass of the backend integration test suite (including login, ticket CRUD, and the customer-isolation scenario) driven through the actual HTTP pipeline. Please do a manual click-through before relying on this for a live demo.
- **Refresh token rotation, Docker Compose, SignalR, optimistic concurrency, a CI pipeline, caching, and rate limiting** were intentionally not implemented — they're listed as bonus-only in the assessment brief. See the recommendation below.
- Everything in the P0–P3 priority list in the assessment brief is implemented and tested.

### Recommended bonus (if there's time)

**Docker Compose** for the best value-to-effort ratio here: one `docker-compose up` bringing up SQL Server, the API, and the built Angular app would remove the only real friction in a live review (getting SQL Server running on the reviewer's machine) for comparatively little implementation time — a Dockerfile per app plus a compose file wiring them together with the existing connection string/JWT configuration.

Clone the project:

```bash
git clone https://github.com/Abd-AlrahmanMohamed/FullStack-SupportTicketManagement.git