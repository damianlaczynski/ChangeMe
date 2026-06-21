#!/bin/sh
set -e

if [ -z "$SONAR_TOKEN" ]; then
  echo "SONAR_TOKEN is missing. Run npm run analyze:sonar (token is created automatically)."
  exit 1
fi

mkdir -p /repo/artifacts/sonar
LOG=/repo/artifacts/sonar/backend-scan.log
COVERAGE_DIR=./TestResults/sonar-coverage

cd /repo/src/ChangeMe.Backend
rm -rf .sonarqube "$COVERAGE_DIR"
mkdir -p "$COVERAGE_DIR"

dotnet tool install --global dotnet-sonarscanner >/dev/null 2>&1 || true
export PATH="$PATH:/root/.dotnet/tools"

{
  echo "=== dotnet sonarscanner begin ==="
  dotnet sonarscanner begin \
    /k:"changeme-backend" \
    /n:"ChangeMe Backend" \
    /v:"1.0" \
    /d:sonar.host.url="$SONAR_HOST_URL" \
    /d:sonar.token="$SONAR_TOKEN" \
    /d:sonar.exclusions="**/Persistence/Migrations/**" \
    /d:sonar.coverage.exclusions="**/Persistence/Migrations/**,**/Migrations/**" \
    /d:sonar.cs.cobertura.reportsPaths="TestResults/**/coverage.cobertura.xml"

  echo "=== dotnet restore ==="
  dotnet restore ChangeMe.Backend.slnx

  echo "=== dotnet build ==="
  dotnet build ChangeMe.Backend.slnx -c Release --no-incremental

  echo "=== dotnet test (unit + integration, coverage) ==="
  dotnet test ChangeMe.Backend.slnx \
    -c Release --no-build \
    --collect:"XPlat Code Coverage" \
    --results-directory "$COVERAGE_DIR"

  echo "=== dotnet sonarscanner end ==="
  dotnet sonarscanner end /d:sonar.token="$SONAR_TOKEN"
} 2>&1 | tee "$LOG"
