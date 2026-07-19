# Security checks (local)

> Scope: vulnerability and exposure scanning via `docker-compose.security.yml` (profile `security`) — SCA, secrets, SAST, RESTler API fuzzing, ZAP SPA baseline.
>
> Design notes: [security-implementation-plan.md](security-implementation-plan.md). Code quality (SonarQube): [sonar-analysis.md](sonar-analysis.md). Production hardening: [deployment.md](deployment.md). CI: [ci.md](ci.md).

## Prerequisites

- Docker engine (Docker Desktop on Windows/macOS).
- First run pulls tool images (`trivy`, `gitleaks`, `semgrep`, `zaproxy`, `restlerfuzzer/restler`).
- **Runtime steps** (RESTler + ZAP): `npm run docker:up:detached` so backend (`:5000`) and frontend (`:4200`) respond.

Compose merges `docker-compose.yml` with `docker-compose.security.yml` via `npm run compose:security`. Does **not** start with `npm run docker:up`.

## Commands

| Command                                 | What runs                                                 |
| --------------------------------------- | --------------------------------------------------------- |
| `npm run security`                      | Offline steps + RESTler + ZAP when stack is reachable     |
| `npm run security:quick`                | Offline only — Trivy, npm/dotnet audit, Gitleaks, Semgrep |
| `npm run security -- --deep`            | Same as full; RESTler uses `fuzz` instead of `fuzz-lean`  |
| `npm run security -- --require-runtime` | Fail if backend or frontend is down (no silent skip)      |
| `npm run security:trivy`                | Trivy filesystem (HIGH/CRITICAL)                          |
| `npm run security:audit`                | `npm audit` + `dotnet list package --vulnerable`          |
| `npm run security:sca`                  | Trivy + audit                                             |
| `npm run security:secrets`              | Gitleaks                                                  |
| `npm run security:sast`                 | Semgrep                                                   |
| `npm run security:fuzz`                 | RESTler only (needs backend)                              |
| `npm run security:dast`                 | ZAP baseline on SPA (needs frontend)                      |
| `npm run security:images`               | Trivy on Docker images (needs `npm run docker:build`)     |

## Typical workflow

**Before a PR or release (offline, no app required):**

```powershell
npm run security:quick
```

Allow **15–30 minutes** on Windows (first Trivy run downloads the vulnerability DB ~100 MB, then scans the repo).

**Full local security stage (with API fuzzing):**

```powershell
npm run docker:up:detached
npm run security
```

Allow **30–60+ minutes** (offline steps + RESTler `fuzz-lean` + ZAP). Use `--deep` for a longer RESTler `fuzz` pass.

Start with **`artifacts/security-summary.txt`**, then open layer-specific reports listed there.

Run **one** `npm run security` / `security:quick` at a time — parallel runs contend on Trivy and overwrite artifacts.

## Layer → tool

| Layer         | Tool                                                | Needs running app? |
| ------------- | --------------------------------------------------- | ------------------ |
| SCA           | Trivy FS + npm/dotnet audit                         | No                 |
| Secrets       | Gitleaks                                            | No                 |
| SAST          | Semgrep                                             | No                 |
| Fuzzing (API) | RESTler (`fuzz-lean` default; `fuzz` with `--deep`) | Yes (backend)      |
| DAST (SPA)    | ZAP baseline                                        | Yes (frontend)     |
| SCA (images)  | Trivy images — `security:images`                    | Built images       |

RESTler is **API fuzzing** from OpenAPI (stateful requests), not browser DAST. ZAP baseline is **passive** SPA scanning (headers, cookies; no active SPA crawl).

## Artifacts

See [artifacts/README.md](../../artifacts/README.md).

| Path                                                                       | Tool                          |
| -------------------------------------------------------------------------- | ----------------------------- |
| `artifacts/sca/trivy-report.json`, `trivy-report.txt`                      | Trivy filesystem              |
| `artifacts/sca/npm-audit.json`, `dotnet-vulnerable.txt`                    | npm audit + dotnet vulnerable |
| `artifacts/secrets/report.json`, `scan.log`                                | Gitleaks                      |
| `artifacts/sast/report.json`, `report.sarif`, `scan.log`                   | Semgrep                       |
| `artifacts/fuzz/openapi.json`, `summary.txt`, `scan.log`, `bug_buckets/**` | RESTler                       |
| `artifacts/dast/frontend/report.html`, `report.json`, `scan.log`           | ZAP baseline                  |
| `artifacts/security-summary.json`, `security-summary.txt`                  | Orchestrator summary          |

## Configuration

| File                               | Purpose                                                                   |
| ---------------------------------- | ------------------------------------------------------------------------- |
| `config/gitleaks.toml`             | Allowlist for dev placeholders and test credentials                       |
| `config/restler.env`               | Fuzzing login user (matches `InitialAdministratorOptions` in Development) |
| `config/restler-settings.json`     | RESTler host, base path, JWT auth module                                  |
| `scripts/security/restler-auth.py` | Login → Bearer token for RESTler                                          |

Override RESTler when not using Compose networking:

```powershell
$env:RESTLER_API_BASE="http://host.docker.internal:5000/api/v1"
$env:RESTLER_OPENAPI_URL="http://host.docker.internal:5000/swagger/v1/swagger.json"
npm run security:fuzz
```

ZAP SPA override:

```powershell
$env:ZAP_TARGET_URL="http://host.docker.internal:4200"
npm run security:dast
```

## Severity and exit codes

| Step         | Fails when                                                 |
| ------------ | ---------------------------------------------------------- |
| Trivy        | HIGH or CRITICAL vulnerabilities (with `--ignore-unfixed`) |
| Gitleaks     | Secret detected                                            |
| Semgrep      | Blocking findings (`--error`)                              |
| RESTler      | Non-zero exit or bug buckets                               |
| ZAP baseline | Uses `-I` — informational alerts do not fail the step      |

`npm run security` runs all applicable steps and exits **`1`** if any **executed** step failed. Skipped runtime steps (stack down) do not fail unless `--require-runtime` is set. A non-zero exit often means **review findings**, not a broken toolchain.

## CI

Security checks are **local only** — not in GitHub Actions. Run `npm run security:quick` on PRs locally or add CI later. See [ci.md](ci.md).

## Troubleshooting

**First Trivy run is slow** — DB download + full repo scan on Windows bind mounts; 10–20 minutes is normal.

**`Found orphan containers` warning** — stale one-off scan containers from an older Compose layout. Safe to remove exited scanners only:

```powershell
docker rm -f changeme-sonar-backend-1 changeme-sonar-frontend-1 2>$null
```

Do **not** use `--remove-orphans` on `compose:security` if SonarQube (`sonar:up`) is running — Compose may stop `sonarqube` / `sonarqube-db` as orphans.

**Old artifact paths** — reports moved from `artifacts/trivy/` to `artifacts/sca/`; delete stale folders under `artifacts/` after upgrading.

## Related

- [sonar-analysis.md](sonar-analysis.md) — optional SonarQube (separate compose, not part of `npm run security`)
- [database-and-docker.md](database-and-docker.md) — Compose profiles
- [deployment.md](deployment.md) — production JWT, rate limiting, TLS
