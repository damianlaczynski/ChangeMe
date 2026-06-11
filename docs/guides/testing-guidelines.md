# Testing Guidelines

> Scope: how to verify changes in this repository and where each kind of test belongs.

## Test projects

- Backend unit tests: `src/ChangeMe.Backend/tests/ChangeMe.Backend.UnitTests`
- Backend integration tests: `src/ChangeMe.Backend/tests/ChangeMe.Backend.IntegrationTests`
- Frontend tests: run through Angular with `npm test` in `src/ChangeMe.Frontend`

**First-time EF migrations:** migration `.cs` files are not shipped. Ensure `Infrastructure/Persistence/Migrations` exists by adding a migration from the solution root before integration tests that call `MigrateAsync()` (see `docs/technical/database-and-docker.md`).

**DataGenerator:** integration and unit tests do not use `ChangeMe.Backend.DataGenerator`; they seed data via `IssueTestHelper`, `TestAuthHelper`, and Testcontainers (see `docs/technical/data-generator.md`).

## From repository root

After `npm install` in the repository root (for `concurrently`), you can run:

- `npm run test:all` — in parallel: frontend tests once (`test:frontend:ci`, no watch) and **all** backend tests via `dotnet test` on `ChangeMe.Backend.slnx`. Integration tests need Docker (Testcontainers).
- `npm run test:backend` — same as `dotnet test` on the whole backend solution (unit + integration projects).
- `npm run test:backend:unit` — unit project only.
- `npm run test:backend:integration` — integration project only.

## Backend tests in Docker

The `backend-tests` service in `docker-compose.yml` uses the .NET SDK image, mounts the repository at `/repo`, and runs `dotnet test ChangeMe.Backend.slnx -c Release` from `src/ChangeMe.Backend`. The host Docker socket is mounted so Testcontainers can start the database for integration tests.

```powershell
docker compose --profile test run --rm backend-tests
```

## Backend unit tests

Use unit tests for:

- domain invariants
- aggregate/entity behavior
- small infrastructure helpers that do not need full app startup

Command (from `src/ChangeMe.Backend`):

```powershell
dotnet test tests/ChangeMe.Backend.UnitTests
```

From repository root:

```powershell
npm run test:backend:unit
```

## Backend integration tests

Use integration tests for:

- endpoint routes and status codes
- auth behavior
- validation behavior visible through HTTP
- persistence side effects
- API contract behavior

Current setup:

<!--#if (PostgreSQL) -->

- `BackendWebApplicationFactory` starts PostgreSQL via Testcontainers.
  <!--#endif-->
  <!--#if (SqlServer) -->
- `BackendWebApplicationFactory` starts SQL Server via Testcontainers.
<!--#endif-->
- Test environment variables override connection string, JWT settings, and email settings.
- `IEmailService` is replaced with `FakeEmailService`.
- `TestAuthHelper` creates a registered and authenticated client through real API calls.

Command (from `src/ChangeMe.Backend`):

```powershell
dotnet test tests/ChangeMe.Backend.IntegrationTests
```

From repository root:

```powershell
npm run test:backend:integration
```

## Frontend checks

Minimum checks for frontend work (from `src/ChangeMe.Frontend`, or use the `npm run …:frontend` equivalents from the repository root — see `AGENTS.md`):

- `npm run lint`
- `npm test` when component or service behavior changes

Useful commands (from `src/ChangeMe.Frontend`):

```powershell
npm run lint
npm test
npm run format:check
```

From repository root (delegates into the frontend folder):

```powershell
npm run lint:frontend
npm run test:frontend
npm run test:frontend:ci
```

## Change-based checklist

### Backend endpoint change

- update or add integration tests
- verify auth expectation
- verify status code and response shape
- verify persistence side effects if data is written

### Domain or persistence change

- add or update unit tests for domain rules
- add or update integration tests if HTTP behavior or saved data changes
- verify migrations/configuration if schema changed

### Frontend API contract change

- update frontend model and service
- check auth flow if token use changed
- run lint and relevant tests

## AI verification rule

For any non-trivial change, prefer running the smallest relevant automated check before finishing:

- frontend-only UI/service change: lint plus affected frontend tests when available
- backend domain change: unit tests first
- backend endpoint change: integration tests for the affected area, then broader tests if needed
- cross-stack or wide regression: from repository root, `npm run test:all` when Docker is available; otherwise `npm run test:frontend:ci` plus `npm run test:backend:unit`, then integration tests separately when the stack is up

Verify behavior against **Functional requirements** in the touched `FR-*` files and inherited `FR-UI-001` / `_shared/` docs — not a separate acceptance-scenarios table.
