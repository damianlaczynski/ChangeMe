# Repository Map

> Scope: where things live and which layer owns what. This is the quickest orientation document after `AGENTS.md`.

## Top level

<!--#if (PostgreSQL) -->

- `docker-compose.yml` starts the local stack: Angular frontend, ASP.NET backend, PostgreSQL, and MailHog. The same file defines `backend-tests` (Compose profile `test`): an SDK container that runs `dotnet test` on the backend solution with the repository mounted from the host.
  <!--#endif-->
  <!--#if (SqlServer) -->
- `docker-compose.yml` starts the local stack: Angular frontend, ASP.NET backend, SQL Server (with `sqlserver-init` creating the application database), and MailHog. The same file defines `backend-tests` (Compose profile `test`): an SDK container that runs `dotnet test` on the backend solution with the repository mounted from the host.
<!--#endif-->
- Root `package.json` defines optional npm scripts (`start:*`, `build:*`, `test:*`, `install:frontend`, and frontend `lint`/`format`) so you can run common frontend and `dotnet` backend tasks from the repository root. Run `npm install` in the repository root to install root devDependencies such as `concurrently` (used by `start:all` and `test:all`). Frontend `node_modules` still live under `src/ChangeMe.Frontend` — refresh them with `npm run install:frontend` from the root or `npm install` inside that folder.
- `src/ChangeMe.Frontend` contains the Angular application.
- `src/ChangeMe.Backend` contains the .NET solution and tests.

## Frontend map

### Tooling and entry points

- `package.json` defines `start`, `build`, `lint`, `format`, and `test`.
- `src/main.ts` bootstraps the Angular app.
- `src/app/app.config.ts` configures providers.
- `src/app/app.routes.ts` defines route-to-component mapping.
- `tsconfig.json` defines strict TypeScript settings and path aliases:
  - `@core/*`
  - `@features/*`
  - `@shared/*`
  - `@styles/*`
  - `@environments/*`

### Runtime structure

- `src/app/core/` holds app-wide services and models that are not specific to one feature (layout shell, navigation history, toasts).
- `src/app/features/` holds feature slices such as `auth` and `issues`.
- `src/app/shared/` holds reusable API wrappers and shared data contracts.

### Feature layout

Each current feature follows a simple slice structure:

- `components/` - standalone Angular components bound directly from routes or nested views.
- `models/` - feature-specific TypeScript contracts (DTOs, enums, request/response shapes).
- `utils/<feature>.utils.ts` - one file per feature for validation limits, labeled select options, and other UI constants.
- `services/` - feature-specific data access and orchestration.
- `guards/` or `interceptors/` - feature-specific Angular infrastructure where needed.

## Backend map

### Solution shape

- `ChangeMe.Backend.sln` is the backend solution entry point.
- `Directory.Packages.props` manages package versions centrally.
- `Directory.Build.props` enables central package version management.

### Layer responsibilities

- `src/ChangeMe.Backend.Web`
  - ASP.NET host startup in `Program.cs`
  - endpoint definitions
  - transport-level configuration
  - common endpoint base types and pipeline behavior
- `src/ChangeMe.Backend.UseCases`
  - top level of each feature folder contains only command and query files
  - handlers stay in the same files as their commands and queries
  - `Dtos/` contains API-facing DTOs for the feature
  - `Services/` contains feature-scoped orchestration services
  - request/response orchestration
- `src/ChangeMe.Backend.Domain`
  - aggregates and entities
  - invariants and domain rules
  - domain interfaces and shared primitives
- `src/ChangeMe.Backend.Infrastructure`
  - EF Core `ApplicationDbContext`
  - entity configuration and migrations
  - auth and email adapters
  - persistence and infrastructure registrations

### Endpoint flow

Current issue endpoints illustrate the standard flow:

1. HTTP endpoint class in `Web/Issues/*.cs`
2. validation class near the endpoint
3. command/query contract in `UseCases/Issues/*.cs`
4. handler in the same command/query file
5. domain calls in `Domain/Aggregates/*`
6. persistence through `ApplicationDbContext`

## Test map

- `tests/ChangeMe.Backend.UnitTests`
  - domain and infrastructure helper tests
- `tests/ChangeMe.Backend.IntegrationTests`
  - endpoint-level tests through real HTTP
  - `Fixtures/` for application factories and container-backed setup
  - `Support/` for reusable auth and test helpers

<!--#if (PostgreSQL) -->

`BackendWebApplicationFactory` starts disposable PostgreSQL via Testcontainers, applies test environment overrides, and replaces `IEmailService` with a fake implementation for integration tests.

<!--#endif-->
<!--#if (SqlServer) -->

`BackendWebApplicationFactory` starts disposable SQL Server via Testcontainers, applies test environment overrides, and replaces `IEmailService` with a fake implementation for integration tests.

<!--#endif-->
