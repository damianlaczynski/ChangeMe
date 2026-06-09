# ChangeMe

ChangeMe is a full-stack issue tracking application starter: Angular frontend, layered ASP.NET backend, authentication, persistence, Hangfire, local MailHog, and automated tests.

Users can register, sign in, browse issues, and authenticated users can create, edit, and delete issues.

## Purpose

This codebase gives you:

- a clean full-stack starting point for product work
- separation between frontend, API, domain, and infrastructure
- patterns for auth, CRUD flows, validation, and integration testing
- documentation that is easy for developers and AI assistants to follow

## Tech Stack

- Frontend: Angular 21, TypeScript, RxJS
- Backend: ASP.NET Core, FastEndpoints, MediatR-style use case flow
<!--#if (PostgreSQL) -->
- Database: PostgreSQL
  <!--#endif-->
  <!--#if (SqlServer) -->
- Database: SQL Server
<!--#endif-->
- Background jobs: Hangfire
- Local email testing: MailHog
- Testing: Angular test runner, .NET unit tests, .NET integration tests with Testcontainers
- Local orchestration: Docker Compose

## Repository Structure

- `src/ChangeMe.Frontend` - Angular application
- `src/ChangeMe.Backend` - .NET solution with source projects and tests
- `docs/` - implementation and testing guidance
<!--#if (PostgreSQL) -->
- `docker-compose.yml` - local full-stack environment (frontend, backend, PostgreSQL, MailHog)
  <!--#endif-->
  <!--#if (SqlServer) -->
- `docker-compose.yml` - local full-stack environment (frontend, backend, SQL Server, MailHog)
<!--#endif-->
- `AGENTS.md` - working guide for AI agents and contributors

## Main Features

- user registration and login
- authenticated issue creation, editing, and deletion
- issue listing and issue details views
- layered backend architecture with separate Web, UseCases, Domain, and Infrastructure projects
- integration-ready local development stack

## Getting Started

### Frontend

From `src/ChangeMe.Frontend`:

```powershell
npm install
npm start
```

Useful commands:

```powershell
npm run lint
npm run format
npm test
```

### Backend

Create the first EF Core migration before running the API or integration tests that apply the database (see `docs/database-and-docker.md`). From the **solution root**:

```powershell
dotnet tool restore
dotnet ef migrations add InitialCreate --project src/ChangeMe.Backend/src/ChangeMe.Backend.Infrastructure/ChangeMe.Backend.Infrastructure.csproj --startup-project src/ChangeMe.Backend/src/ChangeMe.Backend.Web/ChangeMe.Backend.Web.csproj --output-dir Persistence/Migrations
```

From `src/ChangeMe.Backend`:

```powershell
dotnet build ChangeMe.Backend.sln
dotnet run --project src/ChangeMe.Backend.Web
```

### Sample data (optional)

After migrations are applied, from the **repository root**:

```powershell
npm run data:generate
```

Creates demo users (`user1@demo.local`, password in `DataGenerator:DefaultPassword`), issues, comments, and notifications. Use `npm run data:generate -- --reset` to refresh. See `docs/data-generator.md`.

Useful commands:

```powershell
dotnet test tests/ChangeMe.Backend.UnitTests
dotnet test tests/ChangeMe.Backend.IntegrationTests
```

### Full Stack with Docker

From the repository root:

```powershell
docker compose up --build
```

This starts the frontend, backend, MailHog, and the database service defined in Compose.

## Documentation

- `docs/database-and-docker.md` - persistence, Compose, EF migration notes
- `docs/data-generator.md` - optional demo data for local development
- `docs/repo-map.md` - where code lives and which layer owns what
- `docs/frontend-coding-guidelines.md` - frontend conventions
- `docs/backend-coding-guidelines.md` - backend conventions
- `docs/testing-playbook.md` - how to verify changes
- `docs/feature-recipes.md` - implementation recipes for common feature work
- `docs/requirements/` - functional specifications (`FR-*`), non-functional requirements (`NFR-*`), and shared reference docs
- `docs/templates/` - reusable document templates (for example `functional-specification-template.md` for new `FR-*` files)

## About `AGENTS.md`

[`AGENTS.md`](AGENTS.md) is the fast-start guide for AI agents and contributors: which docs to open first, repo layout, commands, folder navigation, and cross-stack coupling checks.

## Development Notes

- Frontend routes: `src/app/app.routes.ts`.
- Frontend features: `src/app/features/<feature>/`.
- Backend endpoints: `src/ChangeMe.Backend.Web`.
- Use cases: `src/ChangeMe.Backend.UseCases`.
- Domain: `src/ChangeMe.Backend.Domain`.
- Persistence and integrations: `src/ChangeMe.Backend.Infrastructure`.

## Testing

Use the smallest relevant test scope for the change:

- frontend UI or service change: `npm run lint` and `npm test`
- backend domain change: unit tests
- backend endpoint or auth change: integration tests

See `docs/testing-playbook.md`.

## License

This project is licensed under the MIT License. See [`LICENSE`](LICENSE) for details.
