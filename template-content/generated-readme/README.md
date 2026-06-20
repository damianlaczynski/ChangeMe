# ChangeMe

ChangeMe is a full-stack issue tracking application starter: Angular frontend, layered ASP.NET backend, JWT session authentication, persistence, Hangfire, local MailHog, and automated tests.

Administrators create users and assign roles; signed-in users browse issues and, when permitted, create, edit, and delete them.

## Purpose

This codebase gives you:

- a clean full-stack starting point for product work
- separation between frontend, API, domain, and infrastructure
- patterns for login sessions, permissions, CRUD flows, validation, and integration testing
- documentation that is easy for developers and AI assistants to follow

## Tech Stack

- Frontend: Angular 21, TypeScript, RxJS
- Backend: ASP.NET Core, FastEndpoints, Mediator source generator use case flow
- Database: PostgreSQL
- Background jobs: Hangfire
- Local email testing: MailHog
- Testing: Angular test runner, .NET unit tests, .NET integration tests with Testcontainers
- Local orchestration: Docker Compose

## Repository Structure

- `src/ChangeMe.Frontend` - Angular application
- `src/ChangeMe.Backend` - .NET solution with source projects and tests
- `docs/` - implementation and testing guidance
- `docker-compose.yml` - local full-stack environment (frontend, backend, PostgreSQL, MailHog)
- `AGENTS.md` - working guide for AI agents and contributors

## Main Features

- email/password login, session refresh, and logout
- admin-managed users, roles, and permission-based access
- issue listing, details, comments, attachments, and notifications
- authenticated issue create, edit, and delete (permission-gated)
- layered backend architecture with separate Web, UseCases, Domain, and Infrastructure projects
- integration-ready local development stack

## Getting Started

### Frontend

From the **repository root** (recommended — includes Playwright Chromium for E2E):

```powershell
npm install
npm run install:frontend
npm run start:frontend
```

Or from `src/ChangeMe.Frontend` (npm packages only):

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

Includes `InitialCreate` — in Development, migrations apply on API startup (`DatabaseOptions:ApplyMigrationsOnStartup` is `true` in `appsettings.Development.json`; see `docs/technical/database-and-docker.md`).

From `src/ChangeMe.Backend`:

```powershell
dotnet build ChangeMe.Backend.slnx
dotnet run --project src/ChangeMe.Backend.Web
```

### Sample data (optional)

After migrations are applied, from the **repository root**:

```powershell
npm run data:generate
```

Creates demo users (`user1@demo.local`, password in `DataGenerator:DefaultPassword`), issues, comments, and notifications. Use `npm run data:generate -- --reset` to refresh. See `docs/technical/data-generator.md`.

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

See [`docs/README.md`](docs/README.md) for the full index.

- `docs/guides/` - implementation conventions (start at `docs/guides/README.md`)
- `docs/technical/` - run and configure the stack (start at `docs/technical/README.md`)
- `docs/requirements/` - product specs and change workflow (start at `docs/requirements/requirements-change-process.md`; templates `_functional-specification-template.md`, `_changes-template.md`)

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

See `docs/guides/testing-guidelines.md`.

## License

This project is licensed under the MIT License. See [`LICENSE`](LICENSE) for details.
