#!/usr/bin/env bash
set -euo pipefail

ENV_FILE="/etc/rhino-compute/environment"

if [[ ! -f "$ENV_FILE" ]]; then
  cp /etc/rhino-compute/environment.example "$ENV_FILE"
fi

tmp_file="$(mktemp)"

awk -v token="${RHINO_TOKEN:-}" '
  BEGIN { replaced = 0 }
  /^RHINO_TOKEN=/ {
    if (length(token) > 0) {
      print "RHINO_TOKEN=" token
    } else {
      print "RHINO_TOKEN="
    }
    replaced = 1
    next
  }
  { print }
  END {
    if (!replaced && length(token) > 0) {
      print "RHINO_TOKEN=" token
    }
  }
' "$ENV_FILE" > "$tmp_file"

cat "$tmp_file" > "$ENV_FILE"
rm -f "$tmp_file"

unset RHINO_TOKEN

exec "$@"
