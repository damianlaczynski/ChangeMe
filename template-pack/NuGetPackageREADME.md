# ChangeMe Full Stack Templates

`ChangeMe` is a full-stack starter template for `dotnet new`.

It generates:

- an Angular frontend
- a layered ASP.NET backend
- **PostgreSQL** for EF Core, Hangfire, integration tests, and Docker Compose
- MailHog for local email capture
- backend unit and integration test projects
- `docs/` with guides, technical notes (`docs/technical/database-and-docker.md`), and requirements

## Install

```powershell
dotnet new install ChangeMe
```

## Create a project

```powershell
dotnet new changeme -n IssuesDemo -o IssuesDemo
```

## After generation

- Start the API once in Development (migrations apply on startup), then see `docs/technical/database-and-docker.md` for Compose and production notes.

## Verify the install

```powershell
dotnet new list changeme
```

## Update

```powershell
dotnet new install ChangeMe --force
```

## Uninstall

```powershell
dotnet new uninstall ChangeMe
```

## Source repository

https://github.com/damianlaczynski/ChangeMe
