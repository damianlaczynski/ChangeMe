# ChangeMe.Backend.DataGenerator

Demo/test data generator for local ChangeMe development.

## Purpose

This tool fills the database with sample users, projects, issues, time entries, billing employment data, availability calendars, comments, acceptance criteria, watchers, and notifications so you can exercise the frontend and API without entering data manually.

**Run only in Development.** Do not use in production.

## Prerequisites

1. A running database (Docker Compose or local PostgreSQL / SQL Server, depending on how the template was generated).
2. EF Core migrations applied (`npm run ef:database:update` from the repository root).
3. A valid connection string in the Web project `appsettings.Development.json` (those files are linked into this tool at build time).

## Running

From the repository root:

```powershell
npm run data:generate
```

Refresh demo data (removes existing `@demo.local` accounts and related records, then regenerates):

```powershell
npm run data:generate -- --reset
```

Directly via `dotnet`:

```powershell
dotnet run --project src/ChangeMe.Backend/tools/ChangeMe.Backend.DataGenerator/ChangeMe.Backend.DataGenerator.csproj
```

## Behavior

1. Applies pending migrations and system seed (roles, optional administrator from `InitialAdministrator`).
2. If demo users (`*@demo.local`) already exist — exits without changes (unless you pass `--reset`).
3. Generates data according to the `DataGenerator` section in the Web `appsettings.Development.json`.

## Demo accounts

By default, accounts `user1@demo.local` through `user8@demo.local` are created with the password from `DataGenerator:DefaultPassword` (default: `Demo123!`).

The seeded system administrator (`admin@example.local` / `admin`) is left unchanged.

## Configuration

`DataGenerator` section in `src/ChangeMe.Backend/src/ChangeMe.Backend.Web/appsettings.Development.json`:

| Key                               | Description                                          |
| --------------------------------- | ---------------------------------------------------- |
| `Seed`                            | Bogus seed (repeatable data)                         |
| `Users`                           | Number of demo accounts                              |
| `Projects`                        | Number of additional demo projects                   |
| `Issues`                          | Number of issues                                     |
| `TimeEntries`                     | Number of logged time entries                        |
| `CommentsPerIssueMin` / `Max`     | Comment count range per issue                        |
| `NotificationsPerUserMin` / `Max` | Notification count range per demo user               |
| `GenerateBilling`                 | Create employment, availability, and leave demo data |
| `DefaultPassword`                 | Shared password for demo accounts                    |
| `EmailDomain`                     | Email domain (e.g. `demo.local`)                     |

## More information

Full documentation: `docs/data-generator.md` at the repository root.
