#!/bin/bash
set -euo pipefail

TOKEN_FILE="${1:-}"

if [[ -n "${TOKEN_FILE}" ]]; then
  if [[ ! -f "${TOKEN_FILE}" ]]; then
    echo "Error: token file not found: ${TOKEN_FILE}" >&2
    exit 1
  fi

  RHINO_TOKEN="$(tr -d '\r\n' < "${TOKEN_FILE}")"

  if [[ -z "${RHINO_TOKEN}" ]]; then
    echo "Error: token file is empty: ${TOKEN_FILE}" >&2
    exit 1
  fi

  export RHINO_TOKEN
  echo "Using Rhino token from file: ${TOKEN_FILE}"
else
  RHINO_TOKEN="${RHINO_TOKEN:-}"
  export RHINO_TOKEN
  echo "No token file provided. Set RHINO_TOKEN in your shell if the container needs a token."
fi

docker run -d \
  --name rhino-compute \
  -p "${HOST_PORT:-5001}:5000" \
  -e RHINO_TOKEN="$RHINO_TOKEN" \
  mcneel.com/rhino-compute:latest
