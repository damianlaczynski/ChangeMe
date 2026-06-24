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
  echo "Cannot reach the frontend."
  echo ""
  echo "Start one of:"
  echo "  npm run docker:up:detached   (ZAP uses http://frontend on the Compose network)"
  echo "  npm run start:all            (then set ZAP_TARGET_URL if needed)"
  echo ""
  echo "Override: ZAP_TARGET_URL=http://host.docker.internal:4200"
  exit 1
fi

echo "ZAP baseline target: $TARGET"

cd /zap/wrk
zap-baseline.py \
  -t "$TARGET" \
  -r report.html \
  -J report.json \
  -I 2>&1 | tee scan.log
