# AI Working Guide

> Scope: fast-start context for AI agents and contributors in **this solution**. Load this file first, then open the focused docs under `docs/` for the area you are changing.

## Repository shape

- `src/ChangeMe.Frontend` - Angular 21 frontend.
- `src/ChangeMe.Backend` - .NET backend solution.
<!--#if (PostgreSQL) -->
- `docker-compose.yml` - local full-stack environment with frontend, backend, PostgreSQL, and MailHog.
  <!--#endif-->
  <!--#if (SqlServer) -->
- `docker-compose.yml` - local full-stack environment with frontend, backend, SQL Server, and MailHog.
<!--#endif-->
- `.config/dotnet-tools.json` - pins **`dotnet-ef`** for `dotnet ef migrations add` (optional; see `docs/database-and-docker.md`).
- `docs/` - implementation, testing, and requirements guidance.
- Root `package.json` - optional npm scripts that run frontend and backend tasks from the repository root (see Commands).

## Start here by task

- Frontend change: read `docs/repo-map.md` and `docs/frontend-coding-guidelines.md`.
- Backend change: read `docs/repo-map.md` and `docs/backend-coding-guidelines.md`.
- Test work or bugfix verification: read `docs/testing-playbook.md`.
- Cross-stack feature: read all four docs above before editing.

## Commands

### Repository root (npm)

From the repository root, run `npm install` once to install root devDependencies (`concurrently` is required for `start:all` and `test:all`). Frontend packages still live under `src/ChangeMe.Frontend`; use `npm run install:frontend` after clone or when frontend dependencies change.

- Install frontend dependencies: `npm run install:frontend`
- Start dev servers: `npm run start:frontend`, `npm run start:backend`, or both in parallel with `npm run start:all`
- Build: `npm run build:frontend`, `npm run build:backend`, or `npm run build:all`
- Frontend quality: `npm run lint:frontend`, `npm run format:frontend`, `npm run test:frontend` (interactive watch when TTY), or `npm run test:frontend:ci` (single run, `--watch=false`)
- Backend tests: `npm run test:backend` (entire solution — unit and integration projects), `npm run test:backend:unit`, or `npm run test:backend:integration`
- Full automated check (frontend CI tests + full backend solution tests, parallel): `npm run test:all` — backend integration tests use Testcontainers and need a running Docker engine

### Frontend (in `src/ChangeMe.Frontend`)

- Install dependencies: `npm install`
- Run dev server: `npm start`
- Lint: `npm run lint`
- Format: `npm run format`
- Tests: `npm test`

### Backend (in `src/ChangeMe.Backend`)

- First-time migrations: add an EF migration from the solution root (`dotnet tool restore` then `dotnet ef migrations add ...`; see `docs/database-and-docker.md`).
- Restore/build solution: `dotnet build ChangeMe.Backend.sln`
- Run web app: `dotnet run --project src/ChangeMe.Backend.Web`
- All tests in the solution: `dotnet test ChangeMe.Backend.sln`
- Unit tests only: `dotnet test tests/ChangeMe.Backend.UnitTests`
- Integration tests only: `dotnet test tests/ChangeMe.Backend.IntegrationTests`

### Full stack

- Start dependencies and app containers: `docker compose up --build`
- Run all backend tests inside a container (bind-mounts the repo; integration tests need the host Docker socket): `docker compose --profile test run --rm backend-tests`

## Repo navigation rules

### Frontend

- Routes live in `src/app/app.routes.ts`.
- Feature code lives under `src/app/features/<feature>/`.
- Shared HTTP wrapper lives in `src/app/shared/api/services/api.service.ts`.
- Cross-cutting user/session concerns live under `src/app/core/` and `features/auth/`.
- Transient toast feedback uses `src/app/core/toast/services/toast.service.ts` with global `<p-toast>` in `app.component.ts`.
- Shared data models live under `src/app/shared/`.

### Backend

- HTTP endpoints live in `src/ChangeMe.Backend.Web`.
- Query and command contracts live in `src/ChangeMe.Backend.UseCases/<Feature>/`.
- Keep only `*Query.cs` and `*Command.cs` files at the top level of each `UseCases` feature folder.
- Place feature DTOs in `src/ChangeMe.Backend.UseCases/<Feature>/Dtos/`.
- Place feature services in `src/ChangeMe.Backend.UseCases/<Feature>/Services/`.
- Handlers still live with their matching command or query file.
- Domain rules and aggregates live in `src/ChangeMe.Backend.Domain`.
- EF Core, persistence, auth, and adapters live in `src/ChangeMe.Backend.Infrastructure`.
- Integration tests mirror API behavior under `tests/ChangeMe.Backend.IntegrationTests`.
- Unit tests cover domain/infrastructure helpers under `tests/ChangeMe.Backend.UnitTests`.

## Change coupling checklist

- If you change a backend request/response contract, check matching frontend models and services.
- If you add or change an endpoint, check validator, handler, and integration tests.
- If you change persistence shape, check EF configuration, migrations, and tests.
- If you change auth behavior, check backend auth config, frontend auth service, guards, and integration tests.
- If you change pagination or shared API result handling, check both backend shared models and frontend `ApiService`.

## Working agreements

- Follow the current code structure instead of inventing a new layer or folder layout.
- Prefer extending an existing feature slice over creating a parallel pattern.
- Keep docs current when introducing a new enforced convention.
- Do not assume files visible in the IDE are committed; verify against the filesystem first.
