# Analysis artifacts (local, gitignored)

Reports from `npm run analyze:*` are written here. See [docs/technical/security-analysis.md](../docs/technical/security-analysis.md).

| Path                                                                        | Tool                                                    |
| --------------------------------------------------------------------------- | ------------------------------------------------------- |
| `trivy/fs-report.json`                                                      | Trivy filesystem (SCA)                                  |
| `trivy/images-*.json`                                                       | Trivy Docker image scans                                |
| `gitleaks/report.json`                                                      | Gitleaks secrets                                        |
| `semgrep/report.json` / `report.sarif`                                      | Semgrep SAST                                            |
| `zap/zap-report.html` / `zap-report.json`                                   | OWASP ZAP baseline                                      |
| `audit/npm-audit.json`                                                      | npm audit                                               |
| `audit/dotnet-vulnerable.txt`                                               | dotnet vulnerable packages                              |
| `sonar/token`, `*-scan.log`, `summary.txt`, `summary.json`, `*-report.json` | SonarQube (API export; dashboard optional)              |
| FE `coverage/**/lcov.info`                                                  | Vitest coverage (before `analyze:sonar:frontend`)       |
| BE `TestResults/**/coverage.cobertura.xml`                                  | Coverlet from unit tests during `analyze:sonar:backend` |
