#!/bin/bash
set -euo pipefail

SOURCE_IMAGE="${SOURCE_IMAGE:-mcneel.com/rhino-compute:latest}"
TARGET_IMAGE="${1:-${DOCKERHUB_IMAGE:-}}"

if [[ -z "${TARGET_IMAGE}" ]]; then
  echo "Usage: $0 <dockerhub-user-or-org/repo[:tag]>" >&2
  echo "Example: $0 mydockerhubuser/rhino-compute:latest" >&2
  exit 1
fi

docker image inspect "${SOURCE_IMAGE}" >/dev/null
docker tag "${SOURCE_IMAGE}" "${TARGET_IMAGE}"
docker push "${TARGET_IMAGE}"
