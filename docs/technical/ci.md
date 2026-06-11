# Continuous integration

> Scope: GitHub Actions workflow for this repository.
> **Source of truth:** [`.github/workflows/ci.yml`](../../.github/workflows/ci.yml) — update this file when CI changes; keep `ci.md` in sync.

## Triggers

Runs on **push** and **pull_request** to `main` or `master`.

Concurrent runs for the same branch are cancelled (`cancel-in-progress: true`) when a newer commit is pushed.

## Jobs

Three jobs run **in parallel** (no job depends on another):

| Job              | What it runs                                                              | Working directory       |
| ---------------- | ------------------------------------------------------------------------- | ----------------------- |
| **Requirements** | `npm ci` → `npm run requirements:validate`                                | Repository root         |
| **Frontend**     | `npm ci` → `npm test -- --watch=false` → `npm run build`                  | `src/ChangeMe.Frontend` |
| **Backend**      | `dotnet restore` → optional EF migration → `dotnet test` → `dotnet build` | Repository root         |

### Requirements

Validates `docs/requirements/` structure: `FR-*` / `NFR-*` frontmatter, cross-references, change records, and regenerates `docs/requirements/README.md`. See `docs/requirements/requirements-change-process.md`.

Run locally before pushing specification changes:

```powershell
npm run requirements:validate
```

### Frontend

- Node.js **22**
- Tests run once (no watch), then production **build**
- Does **not** run `npm run lint` or Prettier checks — run those locally when you touch frontend code (`npm run lint:frontend`, `npm run format:check:frontend`)

### Backend

- .NET **10**
- `dotnet test` and `dotnet build` on `ChangeMe.Backend.slnx` in **Release**
- **Integration tests** use Testcontainers (Docker). GitHub-hosted `ubuntu-latest` runners provide Docker; local runs need a running Docker engine too.

#### EF migrations in CI

Migration `.cs` files are **not** committed in the template repository (see `CONTRIBUTING.md`). The backend job creates `InitialCreate` with `dotnet ef` when `Persistence/Migrations/*.cs` is missing.

Generated solutions that commit their own migrations skip this step automatically.

Locally, add migrations before integration tests if the folder is empty — see `docs/technical/database-and-docker.md`.

## What CI does not cover

| Check                         | Local command                 |
| ----------------------------- | ----------------------------- |
| Frontend ESLint               | `npm run lint:frontend`       |
| Frontend / backend formatting | `npm run format:check:all`    |
| Full stack in Docker          | `npm run docker:up`           |
| Backend tests only in Compose | `npm run docker:test:backend` |

For test scope and project layout, see `docs/guides/testing-guidelines.md`.

## Reproduce CI locally

From the repository root after `npm install`:

```powershell
npm run requirements:validate
npm run test:frontend:ci
npm run build:frontend
npm run test:backend
npm run build:backend
```

Or approximate the full automated check:

```powershell
npm run test:all
npm run build:all
```

(`test:all` does not include `requirements:validate` or frontend build.)
