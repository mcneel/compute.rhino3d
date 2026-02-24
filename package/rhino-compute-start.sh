#!/bin/bash
# Rhino Compute startup script for non-systemd environments (Docker, etc.)

# If running as root, try to re-exec as rhino-compute user
if [ "$(id -u)" -eq 0 ]; then
    # Try various methods to switch user
    if command -v runuser >/dev/null 2>&1; then
        exec runuser -u rhino-compute -- "$0" "$@"
    elif command -v su >/dev/null 2>&1; then
        exec su rhino-compute -s /bin/bash -- "$0" "$@"
    elif command -v sudo >/dev/null 2>&1; then
        exec sudo -u rhino-compute "$0" "$@"
    else
        # No user-switching tools available - warn and continue as root
        echo "Warning: No user-switching tool available (runuser/su/sudo)" >&2
        echo "Warning: Running rhino-compute as root (not recommended for production)" >&2
    fi
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
