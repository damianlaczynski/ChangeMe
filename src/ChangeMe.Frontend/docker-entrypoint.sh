#!/bin/sh
set -e

API_URL="${CHANGE_ME_API_URL:-/api/v1}"
CONFIG_PATH=/usr/share/nginx/html/runtime-config.js
escaped_url=$(printf '%s' "$API_URL" | sed 's/\\/\\\\/g; s/"/\\"/g')

cat > "$CONFIG_PATH" <<EOF
window.__CHANGE_ME_CONFIG__ = {
  apiUrl: "${escaped_url}"
};
EOF

exec nginx -g 'daemon off;'
