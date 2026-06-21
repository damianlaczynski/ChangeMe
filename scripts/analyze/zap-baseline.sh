#!/bin/sh
set -e

mkdir -p /zap/wrk

pick_target() {
  if [ -n "$ZAP_TARGET_URL" ]; then
    echo "$ZAP_TARGET_URL"
    return
  fi

  if curl -sf -o /dev/null --max-time 5 http://frontend/; then
    echo "http://frontend"
    return
  fi

  if curl -sf -o /dev/null --max-time 5 http://host.docker.internal:4200/; then
    echo "http://host.docker.internal:4200"
    return
  fi

  echo ""
}

TARGET=$(pick_target)

if [ -z "$TARGET" ]; then
  echo "Cannot reach the application."
  echo ""
  echo "Start one of:"
  echo "  npm run docker:up          (ZAP will use http://frontend on the Compose network)"
  echo "  npm run start:all          (then set ZAP_TARGET_URL if host.docker.internal fails)"
  echo ""
  echo "Override: ZAP_TARGET_URL=http://host.docker.internal:4200 npm run analyze:dast"
  exit 1
fi

echo "ZAP target: $TARGET"

cd /zap/wrk
# Paths must be relative to /zap/wrk — absolute paths get doubled by the ZAP report job.
zap-baseline.py \
  -t "$TARGET" \
  -r zap-report.html \
  -J zap-report.json \
  -I 2>&1 | tee scan.log
