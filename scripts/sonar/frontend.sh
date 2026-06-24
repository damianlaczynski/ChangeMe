#!/bin/sh
set -e

if [ -z "$SONAR_TOKEN" ]; then
  echo "SONAR_TOKEN is missing. Run npm run sonar (token is created automatically)."
  exit 1
fi

mkdir -p /repo/artifacts/sonar
LOG=/repo/artifacts/sonar/frontend-scan.log
LCOV=/repo/src/ChangeMe.Frontend/coverage/ChangeMe.Frontend/lcov.info
if [ ! -f "$LCOV" ]; then
  LCOV=$(find /repo/src/ChangeMe.Frontend/coverage -name lcov.info -print -quit)
fi

if [ -z "$LCOV" ] || [ ! -f "$LCOV" ]; then
  echo "Missing lcov.info — run npm run test:frontend:coverage before SonarScanner."
  exit 1
fi

echo "Using coverage report: $LCOV"

cd /repo/src/ChangeMe.Frontend

sonar-scanner \
  -Dsonar.host.url="$SONAR_HOST_URL" \
  -Dsonar.token="$SONAR_TOKEN" \
  2>&1 | tee "$LOG"
