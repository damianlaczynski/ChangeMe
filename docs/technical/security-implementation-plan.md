# Security pipeline — implementation plan

> Status: implemented. Operational guide: [security-checks.md](security-checks.md). SonarQube: [sonar-analysis.md](sonar-analysis.md).

## Goals

- One command for the full security stage of the SDLC: `npm run security`
- Minimum tooling per layer — no SonarQube, no multi-variant ZAP
- **RESTler** for API fuzzing (stateful OpenAPI)
- **ZAP baseline** only for passive SPA DAST (headers, cookies)
- Reports under `artifacts/` + `security-summary.json`

## Layer → tool

| Layer         | Tool                                                | Needs running app?                                           |
| ------------- | --------------------------------------------------- | ------------------------------------------------------------ |
| SCA           | Trivy FS + npm audit + dotnet vulnerable            | No                                                           |
| Secrets       | Gitleaks                                            | No                                                           |
| SAST          | Semgrep                                             | No                                                           |
| Fuzzing (API) | RESTler (`fuzz-lean` default, `fuzz` with `--deep`) | Yes (backend)                                                |
| DAST (SPA)    | ZAP baseline                                        | Yes (frontend)                                               |
| SCA (images)  | Trivy images — optional `npm run security:images`   | Built images                                                 |
| Code quality  | SonarQube CE — optional `npm run sonar`             | Separate compose; see [sonar-analysis.md](sonar-analysis.md) |

## Commands

| Command                                 | What runs                                                  |
| --------------------------------------- | ---------------------------------------------------------- |
| `npm run security`                      | Offline steps + RESTler + ZAP when stack reachable         |
| `npm run security:quick`                | Offline only (SCA, secrets, SAST)                          |
| `npm run security -- --deep`            | RESTler `fuzz` instead of `fuzz-lean`                      |
| `npm run security -- --require-runtime` | Exit 1 if backend/frontend unreachable (DAST/fuzz skipped) |
| `npm run security:images`               | Trivy on Docker images                                     |

## Compose

File: `docker-compose.security.yml` — profile `security`, services:

- `trivy-fs`, `trivy-images` (optional)
- `gitleaks`, `semgrep`
- `restler`, `zap-frontend`

Merged with app stack: `npm run compose:security`.

## Artifacts

```text
artifacts/
  sca/           Trivy, npm-audit, dotnet-vulnerable
  secrets/       Gitleaks
  sast/          Semgrep
  fuzz/          RESTler (openapi, compile, bug_buckets, scan.log)
  dast/frontend/ ZAP baseline HTML/JSON
  security-summary.json
  security-summary.txt
```

## Removed (from security pipeline)

- SonarQube — moved to optional `docker-compose.sonar.yml` (`npm run sonar`; see [sonar-analysis.md](sonar-analysis.md))
- ZAP API / API smart / full scans
- `zap-bootstrap.mjs`, `zap_api_hook.py`, `config/zap*.env`, `config/zap-api.conf`
- `npm run analyze:*` (replaced by `security:*`)

## Runtime behaviour

Orchestrator (`scripts/security/run.mjs`) probes from the host:

- Backend: `http://localhost:5000/swagger/v1/swagger.json`
- Frontend: `http://localhost:4200/`

If unreachable, fuzz/DAST steps are **skipped** and recorded in the summary (unless `--require-runtime`).

## Maintenance surface

| Component          | Files to touch when auth/API changes                     |
| ------------------ | -------------------------------------------------------- |
| RESTler JWT        | `config/restler.env`, `scripts/security/restler-auth.py` |
| RESTler target     | `config/restler-settings.json` (`host`, `basepath`)      |
| ZAP SPA            | `scripts/security/zap-frontend.sh` (usually unchanged)   |
| Gitleaks allowlist | `config/gitleaks.toml`                                   |

## Optional extensions (not in template)

- RESTler `test` smoke mode
- Schemathesis / Nuclei
- Security checks in GitHub Actions (`security:quick` only)

## Verification checklist

| Check               | Command                                                     |
| ------------------- | ----------------------------------------------------------- |
| Offline pipeline    | `npm run security:quick` → `artifacts/security-summary.txt` |
| Runtime fuzz + DAST | `npm run docker:up:detached` then `npm run security`        |
| SonarQube stack     | `npm run sonar:up` → http://localhost:9000 UP               |
| Full Sonar scan     | `npm run sonar` (long; optional)                            |
