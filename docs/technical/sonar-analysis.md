# SonarQube code quality (optional)

> Scope: local code quality and coverage dashboard via `docker-compose.sonar.yml` (profile `sonar`) — **separate** from the security pipeline (`npm run security`).
>
> Security checks: [security-checks.md](security-checks.md).

## Prerequisites

- Docker engine; ~2–4 GB RAM free for SonarQube.
- Credentials in `config/sonar.env` (local dev only).
- Does **not** start with `npm run docker:up` — use `npm run sonar:up` or `npm run sonar`.

Compose merges `docker-compose.yml` with `docker-compose.sonar.yml` via `npm run compose:sonar`.

## Commands

| Command                  | What it does                                                                                      |
| ------------------------ | ------------------------------------------------------------------------------------------------- |
| `npm run sonar`          | Start server (if needed), bootstrap token, FE+BE scan with coverage, export to `artifacts/sonar/` |
| `npm run sonar:up`       | Start SonarQube + DB only (http://localhost:9000)                                                 |
| `npm run sonar:down`     | Stop SonarQube containers                                                                         |
| `npm run sonar:export`   | Download metrics + issues JSON/TXT (server must be up; prior scan recommended)                    |
| `npm run sonar:frontend` | Scan frontend only                                                                                |
| `npm run sonar:backend`  | Scan backend only                                                                                 |

## Typical workflow

```powershell
npm run sonar
```

Expect **20–40+ minutes** (backend runs unit + integration tests with coverage inside the scanner container).

**First run:** `scripts/sonar/bootstrap.mjs` waits until SonarQube is `UP`, logs in as `admin` with factory password `admin`, changes it to `SONAR_ADMIN_PASSWORD` from `config/sonar.env`, generates a token cached in `artifacts/sonar/token`. No manual UI step required.

**Dashboard (optional):** http://localhost:9000 (`admin` / password from `config/sonar.env`)

## Artifacts

| Path                                            | Description                           |
| ----------------------------------------------- | ------------------------------------- |
| `artifacts/sonar/summary.txt`                   | Human-readable metrics + quality gate |
| `artifacts/sonar/summary.json`                  | Combined export                       |
| `artifacts/sonar/changeme-backend-report.json`  | Backend measures, gate, issues        |
| `artifacts/sonar/changeme-frontend-report.json` | Frontend measures, gate, issues       |
| `artifacts/sonar/*-scan.log`                    | Scanner logs                          |

Project keys: `changeme-frontend` (`sonar-project.properties`), `changeme-backend` (parameters in `scripts/sonar/backend.sh`).

## Relation to `npm run security`

|                              | `npm run security`                      | `npm run sonar`                     |
| ---------------------------- | --------------------------------------- | ----------------------------------- |
| Purpose                      | Vulnerabilities, secrets, fuzzing, DAST | Code smells, coverage, quality gate |
| Tool                         | Trivy, Gitleaks, Semgrep, RESTler, ZAP  | SonarQube CE                        |
| Compose file                 | `docker-compose.security.yml`           | `docker-compose.sonar.yml`          |
| In default template pipeline | Yes                                     | No — run separately when needed     |

## CI

SonarQube is **not** in GitHub Actions. Run locally before releases or periodically.

## Related

- [security-checks.md](security-checks.md) — security pipeline
- [database-and-docker.md](database-and-docker.md) — Compose profiles
