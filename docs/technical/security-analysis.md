# Security and code analysis (Docker Compose)

> Scope: local and periodic scans via `docker-compose.analyze.yml` (profiles `security` / `analyze`) — dependency CVEs, secrets, SAST, DAST, and SonarQube.
>
> Stack runtime and production hardening: [deployment.md](deployment.md). CI pipeline: [ci.md](ci.md).

## Prerequisites

- Docker engine (Docker Desktop on Windows/macOS).
- First run pulls tool images (`trivy`, `gitleaks`, `semgrep`, `zaproxy`, `sonarqube`, …).
- **SonarQube:** ~2–4 GB RAM free; credentials in `config/sonar.env` (local dev only).

Scans use `npm run compose:analyze -- --profile security run --rm <service>` (or individual `npm run analyze:*` wrappers) and **do not** start with `npm run docker:up` (except SonarQube server via `analyze:sonar:up`).

Compose merges `docker-compose.yml` (app stack / shared `app-network`) with `docker-compose.analyze.yml` (scan services).

## Artifact output

All scans write reports under **`artifacts/`** (gitignored except `artifacts/README.md`). See [artifacts/README.md](../../artifacts/README.md) for the full layout.

| Path                                                                  | Command                                 |
| --------------------------------------------------------------------- | --------------------------------------- |
| `artifacts/trivy/fs-report.json`, `fs-report.txt`                     | `analyze:deps`                          |
| `artifacts/trivy/images-*.json`, `images-*.txt`                       | `analyze:deps:images`                   |
| `artifacts/gitleaks/report.json`, `scan.log`                          | `analyze:secrets`                       |
| `artifacts/semgrep/report.json`, `report.sarif`, `scan.log`           | `analyze:sast`                          |
| `artifacts/zap/zap-report.html`, `zap-report.json`, `scan.log`        | `analyze:dast`                          |
| `artifacts/audit/npm-audit.json`, `dotnet-vulnerable.txt`             | `analyze:deps:audit`                    |
| `artifacts/sonar/token`, `summary.txt`, `*-report.json`, `*-scan.log` | `analyze:sonar`, `analyze:sonar:export` |

Console output is also tee'd to `scan.log` where useful.

## Quick reference

| Command                        | What it runs                                                                                           | Needs running app?                             |
| ------------------------------ | ------------------------------------------------------------------------------------------------------ | ---------------------------------------------- |
| `npm run analyze:deps`         | Trivy filesystem (lockfiles, configs)                                                                  | No                                             |
| `npm run analyze:deps:images`  | Trivy on `changeme-frontend` / `changeme-backend` images                                               | Images built (`npm run docker:build`)          |
| `npm run analyze:deps:audit`   | `npm audit` + `dotnet list package --vulnerable` → `artifacts/audit/`                                  | No                                             |
| `npm run analyze:secrets`      | Gitleaks (git tree + history)                                                                          | No                                             |
| `npm run analyze:sast`         | Semgrep (`p/default`, `p/csharp`, `p/typescript`)                                                      | No                                             |
| `npm run analyze:dast`         | OWASP ZAP baseline → `http://host.docker.internal:4200`                                                | Yes (`npm run docker:up` or dev servers)       |
| `npm run analyze:sonar:up`     | Start SonarQube + DB (http://localhost:9000)                                                           | No                                             |
| `npm run analyze:sonar`        | Start SonarQube, bootstrap token, scan FE + BE, export reports                                         | No (automatic)                                 |
| `npm run analyze:sonar:export` | Download metrics + issues JSON/TXT from SonarQube API (no UI)                                          | SonarQube up, prior scan recommended           |
| `npm run analyze:sonar:down`   | Stop SonarQube containers                                                                              | No                                             |
| `npm run analyze:quick`        | `run-steps.mjs quick` — `deps`, `secrets`, `audit`, `sast` (each step runs even if a prior one failed) | No                                             |
| `npm run analyze:all`          | `run-steps.mjs all` — quick steps + `deps:images`, `dast`, `sonar` (same continue-on-failure behavior) | Yes — see [Full suite](#full-suite-analyzeall) |

Equivalent Compose invocations:

```powershell
npm run compose:analyze -- --profile security run --rm trivy-fs
npm run compose:analyze -- --profile security run --rm gitleaks
```

Profile alias `analyze` works the same: `npm run compose:analyze -- --profile analyze run --rm semgrep`.

## Tool choices (template defaults)

| Layer                    | Tool                   | Notes                                                                   |
| ------------------------ | ---------------------- | ----------------------------------------------------------------------- |
| SCA (deps)               | **Trivy**              | HIGH+CRITICAL, `--scanners vuln`, JSON + table in `artifacts/trivy/`    |
| SCA (images)             | **Trivy**              | Requires built images tagged `changeme-frontend`, `changeme-backend`    |
| Secrets                  | **Gitleaks**           | Config: `config/gitleaks.toml`; report `artifacts/gitleaks/report.json` |
| SAST                     | **Semgrep**            | Fast local rules; JSON + SARIF in `artifacts/semgrep/`                  |
| SAST / quality dashboard | **SonarQube CE**       | UI at :9000; auto token via API; admin password in `config/sonar.env`   |
| DAST                     | **OWASP ZAP baseline** | HTML + JSON in `artifacts/zap/` (`-I` = warn only, not fail on medium)  |

Native registry checks (`analyze:deps:audit`) complement Trivy without extra images.

## Typical workflows

### Quick scan (no running app, ~few minutes)

```powershell
npm run analyze:quick
```

### Full suite (`analyze:all`)

Runs every local scan in sequence: Trivy (filesystem + images), Gitleaks, Semgrep, registry audits, ZAP baseline, and SonarQube (with FE/BE coverage). Expect **30–60+ minutes** and ~4 GB RAM free for SonarQube.

**Before you run:**

1. `npm run docker:build` — so `analyze:deps:images` can scan `changeme-frontend` / `changeme-backend` (missing images are skipped with a note in `artifacts/trivy/`).
2. `npm run docker:up` or `docker:up:detached` — stack must respond on http://localhost:4200 for ZAP (`analyze:dast`).

```powershell
npm run docker:build
npm run docker:up:detached
npm run analyze:all
```

For only the lightweight scans (no DAST, images, or Sonar), use `npm run analyze:quick` instead.

**Continue on findings:** `analyze:quick` and `analyze:all` use `scripts/analyze/run-steps.mjs`. Each tool runs as its own step — if Trivy, Semgrep, Gitleaks, or another scanner exits non-zero because it found issues, the next tools still run and write their reports under `artifacts/`. The npm script exits `1` at the end when any step failed so you know to review the reports; it does **not** stop mid-pipeline.

`analyze:sonar` uses the same idea internally: frontend, backend, and export are separate steps (export retries up to 3 times). Standalone `npm run analyze:deps` (etc.) still exits non-zero on findings when run alone.

### SonarQube

```powershell
npm run analyze:sonar
```

One command: starts SonarQube (if needed), bootstraps token, runs **unit + integration tests with coverage** (FE: Vitest lcov; BE: Coverlet cobertura), uploads to SonarQube, then exports reports to `artifacts/sonar/`.

**First run — no manual UI step:** `scripts/analyze/sonar-bootstrap.mjs` (called by `analyze:sonar` / `analyze:all`) waits until SonarQube is `UP`, logs in as `admin` with the factory default password `admin`, changes it to `SONAR_ADMIN_PASSWORD` from `config/sonar.env` (default `StrongPass123!`), generates an analysis token, and caches it in `artifacts/sonar/token`. You do **not** need to open http://localhost:9000 to set a password on first start. On later runs it reuses the cached token or logs in with the password already stored in SonarQube.

**Without the web UI** (after at least one scan):

```powershell
npm run analyze:sonar:export
```

Files:

- `artifacts/sonar/summary.txt` — human-readable metrics + quality gate
- `artifacts/sonar/summary.json` — combined export
- `artifacts/sonar/changeme-backend-report.json` / `changeme-frontend-report.json` — measures, quality gate, all issues

Community Edition has no built-in PDF/HTML report (unlike ZAP); export uses the SonarQube Web API.

- Dashboard (optional): http://localhost:9000 (`admin` / password from `config/sonar.env`)
- Optional: `npm run analyze:sonar:down` when finished
- Project keys: `changeme-frontend` (`sonar-project.properties`), `changeme-backend` (parameters in `scripts/analyze/sonar-backend.sh` — dotnet-sonarscanner does not read `sonar-project.properties`)

### DAST (baseline)

1. Start the stack: `npm run docker:up` (or `docker:up:detached`) and wait until http://localhost:4200 responds.
2. Run `npm run analyze:dast` — ZAP auto-picks `http://frontend` (Compose) or `http://host.docker.internal:4200` (local dev servers).
3. Open `artifacts/zap/zap-report.html`.

**Local `npm run start:all` without Compose:** if ZAP cannot reach the host, override the target:

```powershell
$env:ZAP_TARGET_URL="http://host.docker.internal:4200"
npm run analyze:dast
```

If that still fails, use `npm run docker:up` for DAST (recommended).

ZAP targets `host.docker.internal:4200` from inside the container (same pattern as backend integration tests).

### Adjusting Gitleaks allowlist

Edit `config/gitleaks.toml` when you add documented placeholders (for example new `*.example` config files). Do **not** allowlist real secret paths.

## Severity and exit codes

When run **individually** (`npm run analyze:deps`, `analyze:sast`, …):

- **Trivy** and **Semgrep** fail the command on HIGH/CRITICAL (Trivy) or any Semgrep finding (`--error`).
- **Gitleaks** exits non-zero when secrets are detected.
- **ZAP baseline** uses `-I` so informational/medium items do not fail the run; inspect the HTML report for actionable items.
- **SonarQube** quality gate is configured in the server UI (not enforced by npm scripts by default).

When run via **`analyze:quick` or `analyze:all`**, each tool still writes its report and the orchestrator **continues** to the next tool even when one exits non-zero. The npm script exits `1` at the end if any step failed (findings or errors) — check `artifacts/` for every tool.

Tighten or loosen policies in `docker-compose.analyze.yml` service `command` blocks.

## CI

Security and code analysis runs **locally only** (`npm run analyze:*`). It is intentionally **not** part of the GitHub Actions pipeline — run before releases or periodic reviews. See [template-improvements-backlog.md](template-improvements-backlog.md).

## Related

- [database-and-docker.md](database-and-docker.md) — Compose profiles (`test` in `docker-compose.yml`; analysis in `docker-compose.analyze.yml`).
- [deployment.md](deployment.md) — production JWT, CORS, Hangfire hardening.
- Backend tests in container: `npm run docker:test:backend`.
