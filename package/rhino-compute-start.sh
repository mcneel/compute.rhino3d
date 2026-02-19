#!/bin/bash
# Rhino Compute startup script for non-systemd environments (Docker, etc.)

# If running as root, re-exec as rhino-compute user
if [ "$(id -u)" -eq 0 ]; then
    exec sudo -u rhino-compute "$0" "$@"
fi

# Load environment variables if the file exists
if [ -f /etc/rhino-compute/environment ]; then
    set -a
    source /etc/rhino-compute/environment
    set +a
fi

# Set defaults
URLS="${RHINO_COMPUTE_URLS:-http://0.0.0.0:5000}"

# Run rhino-compute
exec /usr/bin/rhino-compute \
    --urls "$URLS" \
    "$@"
