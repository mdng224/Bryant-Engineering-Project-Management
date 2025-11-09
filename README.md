# App 🟦

A modern .NET 9 project structured with **Vertical Slice Architecture**, **Entity Framework Core**, and **ASP.NET Aspire** for orchestration.  

This solution demonstrates clean separation of concerns with four core projects:  

- **App.Api** – Minimal API host (endpoints, features, vertical slices)  
- **App.Application** – Application layer (interfaces, DTOs, validators, business rules)  
- **App.Domain** – Domain layer (pure entities, enums, value objects)  
- **App.Infrastructure** – Infrastructure layer (EF Core, ASP.NET Identity, JWT, persistence)  
- **App.AppHost** – Aspire orchestration project (runs services like Postgres + Api together)  
- **App.ServiceDefaults** – Default Aspire configuration (health checks, observability, etc.)

---

## 🏗️ Project Layout

```text
src/
├─ App.Api/ # Minimal API host (vertical slices)
│ └─ Features/
│ ├─ Auth/ # Register, Login, Me
│ ├─ Users/ # CRUD + pagination
│ ├─ Employees/ # CRUD + restore + soft delete
│ └─ Positions/ # CRUD + restore + soft delete
│ └─ Clients/ # CRUD + restore + soft delete
│ └─ Projects/ # CRUD + restore + soft delete
│
├─ App.Application/ # CQRS handlers, DTOs, validators, interfaces
│ ├─ Abstractions/ # ICommand, IQuery, IUnitOfWork, etc.
│ ├─ Common/ # Results, paging, exceptions
│ ├─ Employees/
│ ├─ Positions/
│ └─ Users/
│ └─ Clients/
│ └─ Projects/
│
├─ App.Domain/ # Entities, enums, events, value objects
│ ├─ Users/
│ ├─ Employees/
│ ├─ Positions/
│ └─ Clients/
│ └─ Projects/
│ └─ Common/
│
├─ App.Infrastructure/ # EF Core, Identity, Persistence, Outbox
│ ├─ Persistence/
│ │ ├─ AppDbContext.cs
│ │ ├─ Interceptors/ (AuditSaveChangesInterceptor, etc.)
│ │ └─ Seed/
│ ├─ Identity/
│ └─ Services/ # JWT, Email, etc.
│
├─ App.AppHost/ # Aspire orchestration (API + Postgres)
└─ App.ServiceDefaults/ # Health checks, logging, tracing
```


---

## 🧱 Architecture Highlights

- **Vertical Slice Design** – each feature folder owns its endpoint, DTOs, and logic.  
- **CQRS** – separates command and query responsibilities for clean scalability.  
- **DDD Patterns** – domain events, aggregates, and value objects maintain business integrity.  
- **Auditing & Soft Delete** – every entity tracks `CreatedAtUtc`, `UpdatedAtUtc`, `DeletedAtUtc`, and user IDs via EF Core interceptors.  
- **Outbox Pattern** – guarantees reliable event publication after successful transactions.  
- **Deterministic GUID v7** seeding ensures consistent IDs across environments.

---

## 🚀 Running Locally

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/)

### Start with Aspire
Run the full stack (API + Postgres + health checks):

```bash
dotnet run --project App.AppHost

This automatically:

* Spins up a PostgreSQL container
* Injects connection strings into the API
* Waits for DB health before starting services

Run API Only
(when a Postgres instance is already running)

```bash
dotnet run --project App.Api
```

---

## 🧑‍💻 Database & EF Core

Migrations are located in App.Infrastructure/Migrations.

Create a migration:
```bash
dotnet ef migrations add InitialCreate \
  --startup-project App.Api \
  --project App.Infrastructure
```

Apply migrations:
```bash
dotnet ef database update \
  --startup-project App.Api \
  --project App.Infrastructure
```

---

## 🔐 Authentication

* ASP.NET Identity manages user creation, roles, and passwords.
* JWT tokens generated via IJwtTokenService.
* Endpoints under /auth handle registration, login, and current user retrieval.

---

## 🌐 Frontend Overview

The Vue 3 client communicates with the backend via a clean Axios layer.
Core UI patterns include:
* Searchable and paginated tables
* Add/Edit/Delete/Restore dialogs
* Real-time status badges and filters
* Responsive layout using Tailwind and Grid utility classes

---

## Example Endpoints

| Method | Endpoint                  | Description                 |
| :----- | :------------------------ | :-------------------------- |
| GET    | `/health/db`              | Database connectivity check |
| POST   | `/auth/register`          | Register new user           |
| POST   | `/auth/login`             | Login and receive JWT       |
| GET    | `/auth/me`                | Current user info           |
| GET    | `/employees`              | Paginated employees         |
| POST   | `/positions`              | Create new position         |
| PATCH  | `/positions/{id}/restore` | Restore deleted position    |

---

## 🧰 Development Commands

```bash
dotnet build         # build all projects
dotnet test          # (optional) run tests
npm install && npm run dev  # run frontend (if separate repo)
```

---

## 🏛️ About Bryant Engineering, Inc.
Bryant Engineering, Inc.
 is a civil engineering and land surveying consulting firm based in Owensboro and Bowling Green, KY.
This internal system streamlines personnel management and operational oversight for engineering projects.

---

## Developed By

Daniel Ng – Full-Stack Software Engineer
