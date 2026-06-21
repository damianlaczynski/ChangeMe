# Changelog

All notable changes to the **ChangeMe** NuGet template package (`dotnet new changeme`) are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2026-06-21

### Added

- **API versioning** — HTTP endpoints under `/api/v1`; Swagger and frontend updated accordingly.
- **Same-origin API proxy (Docker / production)** — nginx proxies `/api/` and `/hubs/`; `runtime-config.js` written at container start from `CHANGE_ME_API_URL` (default `/api/v1`); `docker-entrypoint.sh` for the frontend image.
- **Root `package.json`** — unified scripts for install, start, build, lint/format, test, EF Core, Docker Compose, E2E, data generation, and requirements validation.
- **GitHub Actions CI** — parallel jobs for requirements, frontend, backend, and Playwright E2E.
- **Playwright E2E** — smoke suite under `src/ChangeMe.Frontend/e2e/`; `npm run test:e2e` / `test:e2e:ui`; `npm run install:frontend` installs Chromium.
- **Roles and permissions (RBAC)** — permission catalog, role CRUD, role–user assignments, effective-permissions preview, and permission-gated issue operations.
- **Users administration** — user list, create/edit, details, deactivate/activate, and admin session management.
- **Session auth** — refresh tokens, my-account profile, session list, per-session revoke, and logout-all (replacing single long-lived JWT from 1.0.0).
- **Password policy** — server-side validation and frontend helpers.
- **Initial administrator seed** — bootstrap admin user on first empty database.
- **Issue file attachments** — upload, download, and delete with local file storage and validation.
- **Issue details tabs** — separate comments, change history, and attachments tabs.
- **Data generator** — `npm run data:generate` for Development demo data (`docs/technical/data-generator.md`).
- **Template symbols** — derived `changeMe` (camelCase) and `CHANGE_ME` (SCREAMING_SNAKE) from the `-n` project name.
- **Documentation split** — `docs/guides/`, `docs/technical/`, `docs/requirements/` with validation via `npm run requirements:validate`; `docs/technical/deployment.md`, `database-and-docker.md`, `ci.md`, and `data-generator.md`.
- **Generated-project readme** — consumer-focused root `README.md` from `template-content/generated-readme/` (maintainer readme stays on GitHub only).
- **Frontend UI stack** — PrimeNG + Tailwind CSS; shared applied-filters chips and back-button components.

### Changed

- **Authentication** — admin-provisioned users instead of public self-registration; login returns refreshable sessions instead of a single access token.
- **Notifications UI** — notifications center replaced with header bell and dropdown panel (SignalR push for new notifications retained).
- **Mediator** — MediatR replaced with the Mediator source generator across UseCases and tests.
- **Solution format** — backend solution file is `ChangeMe.Backend.slnx` (was `.sln`).
- **Database migrations** — consolidated `InitialCreate` for the current PostgreSQL schema (roles, sessions, attachments, and related tables).
- **Configuration sections** — options classes bound with consistent `nameof` section names.
- **Docker Compose** — frontend service sets `CHANGE_ME_API_URL`; nginx serves SPA and proxies API/SignalR on one origin.
- **Maintainer docs** — `CONTRIBUTING.md` and this changelog excluded from `dotnet new` output.

### Removed

- **Public self-registration** — `Register` endpoint and UI; users are created by administrators.
- **Live issues-list refresh via SignalR** — `IssueRealtimeService` and list auto-reload on issue events (notification bell still uses SignalR).

### Migration notes for consumers upgrading from 1.0.0

If you generated a project from **1.0.0** and want to align with **2.0.0** patterns (not an automatic upgrade):

1. **API clients** — prefix routes with `/api/v1`.
2. **Auth** — replace public registration with admin user creation; adopt refresh/session endpoints if you extend auth.
3. **Production frontend** — serve SPA and API from one host with nginx proxy, or set `CHANGE_ME_API_URL` and `CorsOptions:AllowedOrigins` for split-host deployment (`docs/technical/deployment.md`).
4. **Scripts** — use root `package.json` for common tasks; run `npm run install:frontend` once before E2E.
5. **Permissions** — gate new endpoints and UI with the permission catalog pattern if you add features.

Fresh installs: `dotnet new install ChangeMe --force`, then `dotnet new changeme -n YourApp -o YourApp`.

## [1.0.0] - 2026-05-02

### Added

- Initial NuGet template package **`ChangeMe`** (`dotnet new changeme`).
- Full-stack starter: Angular frontend, layered ASP.NET backend (Web → UseCases → Domain → Infrastructure).
- Email/password login and public self-registration.
- Issue listing, details, comments, history, acceptance criteria, watch, and in-app notifications with SignalR.
- PostgreSQL, Docker Compose, MailHog, and Hangfire.
- Backend unit and integration tests (Testcontainers).
- Template token replacement for `ChangeMe` across solution, Docker, and docs.

[2.0.0]: https://github.com/damianlaczynski/ChangeMe/compare/v1.0.0...v2.0.0
[1.0.0]: https://github.com/damianlaczynski/ChangeMe/releases/tag/v1.0.0
