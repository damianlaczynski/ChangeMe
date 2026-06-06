# Data generator (demo data)

> Scope: local development tool that fills the database with sample data.

## Overview

`ChangeMe.Backend.DataGenerator` is a console project under `src/ChangeMe.Backend/tools/ChangeMe.Backend.DataGenerator/`. It is **not** part of the production API host and is **not** used by automated tests.

| Mechanism               | Purpose                                                                  |
| ----------------------- | ------------------------------------------------------------------------ |
| `ApplicationDataSeeder` | System roles and optional initial administrator (always-on product seed) |
| **DataGenerator**       | Optional demo dataset for UI and manual API exploration                  |

## Prerequisites

1. Database is reachable (see `docs/database-and-docker.md`).
2. EF Core migrations exist and are applied:

   ```powershell
   npm run ef:restore
   npm run ef:database:update
   ```

3. `ConnectionStrings:DefaultConnection` in `src/ChangeMe.Backend/src/ChangeMe.Backend.Web/appsettings.Development.json` points at your database.

## Commands

From the repository root:

```powershell
npm run data:generate
npm run data:generate -- --reset
```

`--reset` removes existing demo users (emails ending with `@<EmailDomain>`) and related issues/notifications, then regenerates.

If demo data already exists and you omit `--reset`, the tool exits successfully without changes.

## What gets generated

- **Users** — `user1@demo.local`, `user2@demo.local`, … with the default `User` role
- **Projects** — additional non-system projects (default project is ensured separately) with demo users as members
- **Issues** — varied title, description, status, priority, optional assignee, distributed across all projects
- **Issue children** — acceptance criteria, comments, watchers (via domain methods)
- **Time entries** — logged work spread across demo users, projects, and issues with audit log entries
- **Notifications** — linked to issues and demo users
- **Billing / availability** — demo positions, employment profiles and contracts, weekly recurring patterns with generated entries, manual availability exceptions, and approved leave requests with synced leave entries

All inserts go through domain factories (`User.Create`, `Issue.Create`, etc.) and `ApplicationDbContext`, matching the integration-test pattern.

## Configuration

Settings live in the Web project `appsettings.Development.json` under `DataGenerator` (also documented in the tool [README](../src/ChangeMe.Backend/tools/ChangeMe.Backend.DataGenerator/README.md)).

The generator copies `appsettings.json` and `appsettings.Development.json` from `ChangeMe.Backend.Web` at build output time.

## Architecture

```text
npm run data:generate
  → ChangeMe.Backend.DataGenerator (console)
  → DatabaseConfig.InitializeDatabaseAsync (migrate + ApplicationDataSeeder)
  → DemoDataExistsChecker (skip or DemoDataCleaner on --reset)
  → UsersGenerator → BillingGenerator → ProjectsGenerator → IssuesGenerator → TimeEntriesGenerator → NotificationsGenerator
  → ApplicationDbContext.SaveChanges
```

## Troubleshooting

| Problem                  | Action                                                                                                              |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------- |
| No migrations found      | Add and apply migrations (`docs/database-and-docker.md`)                                                            |
| Connection refused       | Start Docker Compose or local DB; verify connection string                                                          |
| Demo data already exists | Run with `--reset` or delete demo users manually                                                                    |
| Wrong provider           | Regenerate template with the intended `--Database` option; do not mix PostgreSQL and SQL Server migration histories |

## Tests

Integration tests use their own helpers (`IssueTestHelper`, `TestAuthHelper`) and Testcontainers — they do **not** invoke DataGenerator. See `docs/testing-playbook.md`.
