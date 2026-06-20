# ChangeMe

ChangeMe is a template full-stack issue tracking application. It is intended as a practical starter repository for building and extending a modern web app with a typed frontend, a layered .NET backend, JWT session authentication, persistence, and automated testing already in place.

The current feature set centers around authenticated issue management with role-based access control: administrators create users and assign roles; signed-in users browse issues and, when permitted, create, edit, and delete them.

## Purpose

This repository is meant to provide:

- a clean full-stack starting point for product work
- a clear separation between frontend, API, domain, and infrastructure concerns
- a place to practice or extend real-world patterns such as login sessions, permissions, CRUD flows, validation, and integration testing
- a documented structure that is easy for both developers and AI agents to navigate

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
- `.template.config/` - `dotnet new` template manifest (`changeme`, `sourceName` token `ChangeMe`)
- `template-pack/` - NuGet packaging project for the template
- `template-content/` - overlays and **generated-project** `README.md` (consumer readme for `dotnet new` output)

## Main Features

- email/password login, session refresh, and logout
- admin-managed users, roles, and permission-based access
- issue listing, details, comments, attachments, and notifications
- authenticated issue create, edit, and delete (permission-gated)
- layered backend architecture with separate Web, UseCases, Domain, and Infrastructure projects
- integration-ready local development stack

## Getting Started

### Install as a `dotnet new` template

Install the template from NuGet:

```powershell
dotnet new install ChangeMe
```

Or install from this repository root during local development:

```powershell
dotnet new install .
```

Create a new solution from the installed template:

```powershell
dotnet new changeme -n IssuesDemo -o IssuesDemo
```

PostgreSQL persistence, Hangfire storage, Docker Compose, and integration tests.

The installed short name appears in the `Short Name` column of `dotnet new list`.

This replaces `ChangeMe` across the solution, project names, folders, Docker configuration, docs, and frontend package metadata. Use a .NET-friendly project name such as `IssuesDemo` so generated solution and namespace names stay valid. Avoid embedding the substring `ChangeMe` in secrets you expect to stay literal after generation (the template renames that token everywhere).

**Generated projects** receive a different root `README.md` (product-focused, no template packaging steps); that file is maintained under `template-content/generated-readme/README.md`.

### Publish the NuGet package

```powershell
dotnet pack template-pack/ChangeMe.Templates.csproj -c Release
```

```powershell
dotnet nuget push template-pack/bin/Release/ChangeMe.<version>.nupkg --source https://api.nuget.org/v3/index.json --api-key <your-api-key>
```

The packaging project targets `net10.0` only as a carrier for NuGet metadata and `dotnet pack`. It does not affect the generated solution structure or the target frameworks used by the projects created from the template.

### Frontend (this repository)

From the **repository root** (installs npm packages and Playwright Chromium for E2E):

```powershell
npm install
npm run install:frontend
npm run start:frontend
```

Or from `src/ChangeMe.Frontend` (npm packages only; E2E needs Chromium from `npm run install:frontend` at the root):

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

### Backend (this repository)

Includes `InitialCreate` — apply once after clone: `npm run ef:database:update` (see `docs/technical/database-and-docker.md`).

From `src/ChangeMe.Backend`:

```powershell
dotnet build ChangeMe.Backend.slnx
dotnet run --project src/ChangeMe.Backend.Web
```

Useful commands:

```powershell
dotnet test tests/ChangeMe.Backend.UnitTests
dotnet test tests/ChangeMe.Backend.IntegrationTests
```

### Full stack with Docker

From the repository root:

```powershell
docker compose up --build
```

This starts the frontend, backend, MailHog, and the database service defined in this solution's Compose file.

## Documentation

The `docs/` directory contains guidance that is also shipped into generated solutions. See [`docs/README.md`](docs/README.md) for the full index.

- `docs/guides/` - implementation conventions (start at `docs/guides/README.md`)
- `docs/technical/` - run and configure the stack (start at `docs/technical/README.md`)
- `docs/requirements/` - product specs and change workflow (start at `docs/requirements/requirements-change-process.md`; templates `_functional-specification-template.md`, `_changes-template.md`)

Maintainers of the template package: see [`CONTRIBUTING.md`](CONTRIBUTING.md).

## About `AGENTS.md`

[`AGENTS.md`](AGENTS.md) is the fast-start guide for AI agents and contributors working in this repository. It explains:

- which docs to read first depending on the task
- the main repo structure
- standard frontend, backend, and Docker commands
- navigation rules for key folders
- change-coupling checks across frontend and backend
- working agreements for keeping changes aligned with the existing architecture

If you are making code changes, `AGENTS.md` should be treated as the first orientation document before opening area-specific docs in `docs/`.

## Development Notes

- Frontend routes are defined in `src/app/app.routes.ts`.
- Frontend feature code lives under `src/app/features/<feature>/`.
- Backend endpoints live in `src/ChangeMe.Backend.Web`.
- Use case contracts and handlers live in `src/ChangeMe.Backend.UseCases`.
- Domain rules live in `src/ChangeMe.Backend.Domain`.
- Persistence and integrations live in `src/ChangeMe.Backend.Infrastructure`.

## Testing

Use the smallest relevant test scope for the change:

- frontend UI or service change: `npm run lint` and `npm test`
- backend domain change: unit tests
- backend endpoint or auth change: integration tests

For more detail, see `docs/guides/testing-guidelines.md`.

## License

This project is licensed under the MIT License. See [`LICENSE`](LICENSE) for details.
