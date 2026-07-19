# Security artifacts (local, gitignored)

Reports from `npm run security` / `security:*`. See [docs/technical/security-checks.md](../docs/technical/security-checks.md).

| Path                                                                        | Tool                                                |
| --------------------------------------------------------------------------- | --------------------------------------------------- |
| `sca/trivy-report.json`, `trivy-report.txt`                                 | Trivy filesystem (SCA)                              |
| `sca/images-*.json`, `images-*.txt`                                         | Trivy Docker image scans (`security:images`)        |
| `sca/npm-audit.json`, `dotnet-vulnerable.txt`                               | npm audit + dotnet vulnerable                       |
| `secrets/report.json`, `scan.log`                                           | Gitleaks                                            |
| `sast/report.json`, `report.sarif`, `scan.log`                              | Semgrep SAST                                        |
| `fuzz/openapi.json`, `summary.txt`, `scan.log`, `bug_buckets/**`            | RESTler API fuzzing                                 |
| `dast/frontend/report.html`, `report.json`, `scan.log`                      | OWASP ZAP baseline (SPA)                            |
| `security-summary.json`, `security-summary.txt`                             | Orchestrator summary                                |
| `sonar/token`, `*-scan.log`, `summary.txt`, `summary.json`, `*-report.json` | SonarQube (`npm run sonar`)                         |
| FE `coverage/**/lcov.info`                                                  | Vitest coverage (before `sonar` / `sonar:frontend`) |
| BE `TestResults/**/coverage.cobertura.xml`                                  | Coverlet from backend scan during `sonar`           |
