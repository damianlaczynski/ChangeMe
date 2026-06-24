#!/bin/sh
set -e

FUZZ_DIR=/repo/artifacts/fuzz
COMPILE_DIR="$FUZZ_DIR/compile"
RESTLER="dotnet /RESTler/restler/Restler.dll"
MODE="${RESTLER_MODE:-fuzz-lean}"

mkdir -p "$FUZZ_DIR" "$COMPILE_DIR"

if [ -f /repo/config/restler.env ]; then
  set -a
  # shellcheck disable=SC1091
  . /repo/config/restler.env
  set +a
fi

pick_openapi_url() {
  if [ -n "$RESTLER_OPENAPI_URL" ]; then
    echo "$RESTLER_OPENAPI_URL"
    return
  fi

  if curl -sf -o /dev/null --max-time 5 http://backend:8080/swagger/v1/swagger.json; then
    echo "http://backend:8080/swagger/v1/swagger.json"
    return
  fi

  if curl -sf -o /dev/null --max-time 5 http://host.docker.internal:5000/swagger/v1/swagger.json; then
    echo "http://host.docker.internal:5000/swagger/v1/swagger.json"
    return
  fi

  echo ""
}

OPENAPI_URL=$(pick_openapi_url)

if [ -z "$OPENAPI_URL" ]; then
  echo "Cannot reach the API OpenAPI document."
  echo "Start: npm run docker:up:detached"
  exit 1
fi

echo "RESTler mode: $MODE"
echo "OpenAPI: $OPENAPI_URL"

curl -sf "$OPENAPI_URL" -o "$FUZZ_DIR/openapi.json"

echo "Compiling OpenAPI grammar..."
$RESTLER compile \
  --api_spec "$FUZZ_DIR/openapi.json" \
  --output_dir "$COMPILE_DIR" \
  2>&1 | tee "$FUZZ_DIR/compile.log"

GRAMMAR="$COMPILE_DIR/grammar.py"
DICT="$COMPILE_DIR/dict.json"

if [ ! -f "$GRAMMAR" ] || [ ! -f "$DICT" ]; then
  echo "RESTler compile did not produce grammar.py / dict.json under $COMPILE_DIR"
  exit 1
fi

echo "Running RESTler $MODE..."
set +e
$RESTLER "$MODE" \
  --grammar_file "$GRAMMAR" \
  --dictionary_file "$DICT" \
  --settings /repo/config/restler-settings.json \
  --no_ssl \
  2>&1 | tee "$FUZZ_DIR/scan.log"
RESTLER_EXIT=$?
set -e

# Summarize bug buckets when present.
SUMMARY="$FUZZ_DIR/summary.txt"
{
  echo "RESTler $MODE finished with exit code $RESTLER_EXIT"
  echo "OpenAPI: $OPENAPI_URL"
  echo ""
  find "$FUZZ_DIR" -name bug_buckets.txt 2>/dev/null | while read -r bucket; do
    echo "=== $bucket ==="
    cat "$bucket"
    echo ""
  done
} > "$SUMMARY"

if [ "$RESTLER_EXIT" -ne 0 ]; then
  echo "RESTler reported findings or errors — see $FUZZ_DIR/scan.log and bug_buckets."
  exit "$RESTLER_EXIT"
fi

echo "RESTler completed — see $SUMMARY"
