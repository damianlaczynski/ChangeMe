# Contributing to the ChangeMe template (maintainers)

> Scope: authors packaging **[changeme](https://www.nuget.org/packages/ChangeMe)** from **this GitHub repository**. If you only consume **`dotnet new changeme`** or fork a generated app, you do **not** need this file; see **`README.md`**, **`AGENTS.md`**, and **`docs/`** in your generated solution.

## Documentation split

| Audience                  | Files                                                                                                    |
| ------------------------- | -------------------------------------------------------------------------------------------------------- |
| Generated solution / fork | `README.md`, `AGENTS.md`, `docs/*`, `.config/dotnet-tools.json` (optional `dotnet-ef` pin)               |
| Template repo maintainers | `CONTRIBUTING.md` (this file), `.template.config/template.json`, `template-pack/*`, `template-content/*` |

`CONTRIBUTING.md` is excluded from the NuGet template payload (`dotnet new` output).

## Install and validate template locally

```powershell
dotnet new install .
dotnet new changeme -n Smoke -o %TEMP%\ChangeMeSmoke --Database PostgreSQL --force
```

Avoid **`dotnet new -o`** under **`artifacts/`** (or other nested outputs inside this repo) without cleaning stale folders; `artifacts/**` is excluded from packaged content but local installs can still recurse oddly.

## Publish NuGet package

```powershell
dotnet pack template-pack/ChangeMe.Templates.csproj -c Release
dotnet nuget push template-pack/bin/Release/ChangeMe.<version>.nupkg --source https://api.nuget.org/v3/index.json --api-key <your-api-key>
```

Package readme lives in **`template-pack/NuGetPackageREADME.md`**.

## Layout hints for editors

- `.template.config/template.json` – symbols, sources, `specialCustomOperations` for `docker-compose.yml`.
- `template-content/generated-readme/README.md` – becomes the generated solution’s root **`README.md`** (root **`README.md`** is excluded from the template payload and stays maintainer-facing on GitHub).
- `template-content/sqlserver-web/` – overlays **`appsettings.Development.json`** for `--Database SqlServer` (canonical PostgreSQL dev JSON lives on main tree).
- **`src/ChangeMe.Backend/.../Persistence/Migrations/*.cs`** – excluded from the template payload; consumers add migrations with **`dotnet ef`** (see **`docs/technical/database-and-docker.md`** in the generated tree).

When adjusting persistence across variants, keep **`README.md` / `AGENTS.md` / `docs/`** oriented toward the **generated product**, not this repo.
