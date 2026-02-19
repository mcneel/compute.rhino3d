# Rhino Compute Systemd Service Setup

This document describes how to configure rhino-compute to start automatically on system boot using systemd.

## Installation

When you install rhino-compute via package manager:
```bash
# On AmazonLinux2023
sudo dnf install rhino-compute

# On Ubuntu
sudo apt install rhino-compute
```

The systemd service file is automatically installed, but not enabled by default.

## Configuration

### Setting the API Key

Rhino Compute requires a valid Rhino license token to run. Configure it using an environment file:

1. **Copy the example configuration:**
   ```bash
   sudo cp /etc/rhino-compute/environment.example /etc/rhino-compute/environment
   ```

2. **Edit the configuration file:**
   ```bash
   sudo nano /etc/rhino-compute/environment
   ```

3. **Set your API key:**
   ```bash
   RHINO_TOKEN=your_actual_api_key_here
   ```

4. **Restart the service to apply changes:**
   ```bash
   sudo systemctl restart rhino-compute
   ```

The environment file (`/etc/rhino-compute/environment`) can contain any environment variables needed by the service. It's secured with root-only write access.

## Quick Setup

```bash
# Enable service to start on boot
sudo systemctl enable rhino-compute

# Start the service now
sudo systemctl start rhino-compute
```

## Managing the Service

### Check Service Status
```bash
sudo systemctl status rhino-compute
```

### View Logs
```bash
# Follow logs in real-time
sudo journalctl -u rhino-compute -f

# View recent logs
sudo journalctl -u rhino-compute -n 100
```

### Stop the Service
```bash
sudo systemctl stop rhino-compute
```

### Restart the Service
```bash
sudo systemctl restart rhino-compute
```

### Disable Automatic Startup
```bash
sudo systemctl disable rhino-compute
sudo systemctl stop rhino-compute
```

## Service Configuration

The systemd service file is located at:
- `/usr/lib/systemd/system/rhino-compute.service` (RPM-based systems)
- `/lib/systemd/system/rhino-compute.service` (Debian-based systems)

### Default Configuration

The service runs with these defaults:
- **URL**: `http://0.0.0.0:5000`
- **User**: `rhino-compute` (dedicated system user)
- **Working Directory**: `/usr/lib/rhino-compute`
- **Restart Policy**: Automatic restart on failure

### Customizing the Service

To customize the service (e.g., change port, add API key, adjust child count):

1. Create a systemd override file:
```bash
sudo systemctl edit rhino-compute
```

2. Add your custom configuration:
```ini
[Service]
ExecStart=
ExecStart=/usr/bin/rhino-compute --urls http://0.0.0.0:8080 --childcount 8
```

3. Reload and restart:
```bash
sudo systemctl daemon-reload
sudo systemctl restart rhino-compute
```

### Available Command-Line Options

All rhino-compute command-line options can be used in the systemd service:

- `--urls <url>`: Set listening URL 
- `--port <port>`: Set port number
- `--childcount <n>`: Number of worker processes (default: 4)
- `--spawn-on-startup`: Launch workers immediately
- `--idlespan <seconds>`: Worker idle timeout (default: 3600)
- `--apikey <key>`: Enable API key authentication
- `--timeout <seconds>`: Request timeout (default: 100)
- `--max-request-size <bytes>`: Max request body size (default: 52428800)
- `--load-grasshopper <true|false>`: Load Grasshopper plugin (default: true)

## Security Considerations

The service runs as a dedicated `rhino-compute` user with limited privileges:
- No login shell (`/sbin/nologin`)
- Restricted to `/usr/lib/rhino-compute` directory
- Private temporary directory (`PrivateTmp=true`)
- Cannot escalate privileges (`NoNewPrivileges=true`)

## Troubleshooting

### Service fails to start

1. Check the logs:
```bash
sudo journalctl -u rhino-compute -n 50
```

2. Verify rhino3d is installed:
```bash
rpm -q rhino3d  # On RPM-based systems
dpkg -l rhino3d  # On Debian-based systems
```

3. Test manual startup:
```bash
sudo -u rhino-compute /usr/bin/rhino-compute --urls http://0.0.0.0:5000
```

### Port already in use

If port 5000 is already in use, customize the service to use a different port (see "Customizing the Service" above).

### Permission issues

Ensure the rhino-compute user has access to required files:
```bash
sudo ls -la /usr/lib/rhino-compute
```

## Uninstalling

To completely remove rhino-compute:

```bash
# Stop and disable the service
sudo systemctl disable rhino-compute
sudo systemctl stop rhino-compute

# Remove the package
sudo dnf remove rhino-compute  # RPM-based
sudo apt remove rhino-compute  # Debian-based
```

The service user and service file will be cleaned up automatically during package removal.
