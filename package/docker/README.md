# Docker Packaging

This folder contains a simple Docker-based workflow for running and publishing `rhino-compute`.

The files here cover three related tasks:

- Building a local Docker image
- Running the container with Docker Compose
- Retagging and pushing the built image to Docker Hub

## Files

- `Dockerfile`
  Builds a `rhino-compute` image from Ubuntu and installs the `rhino-compute` package.

- `docker-compose.yml`
  Runs the `mcneel.com/rhino-compute:latest` image and optionally passes `RHINO_TOKEN` from your shell environment. The published host port can also be overridden with `HOST_PORT`.

- `docker-entrypoint.sh`
  Container entrypoint that prepares `/etc/rhino-compute/environment` and writes `RHINO_TOKEN` into it when that environment variable is provided.

## Scripts

### `build.sh`

Builds the local Docker image from the `Dockerfile` in this folder.

Usage:

```bash
./build.sh
```

Current image tag:

```bash
mcneel.com/rhino-compute:latest
```

### `compose.sh`

Starts the Compose stack in detached mode.

Usage:

```bash
./compose.sh
./compose.sh /full/path/to/rhino_token.txt
```

If you pass a file path, the script reads the token from that file and exports it as `RHINO_TOKEN` for the `docker compose` command. The file is not mounted into the container.

If you prefer, you can also set `RHINO_TOKEN` in your shell before running it:

```bash
RHINO_TOKEN=your_token_here ./compose.sh
```

You can override the port mapping per command:

```bash
HOST_PORT=8080 docker compose -p mcneel up -d
```

### `push.sh`

Retags the locally built image and pushes it to a Docker Hub repository.

Usage:

```bash
./push.sh yourdockerhubuser/rhino-compute:latest
```

You can also set the target image with `DOCKERHUB_IMAGE`:

```bash
DOCKERHUB_IMAGE=yourdockerhubuser/rhino-compute:latest ./push.sh
```

By default, the script expects the local source image to already exist as:

```bash
mcneel.com/rhino-compute:latest
```

If needed, you can override that with `SOURCE_IMAGE`.

## Typical Flow

```bash
cd package/docker
./build.sh
./compose.sh /full/path/to/rhino_token.txt
./push.sh yourdockerhubuser/rhino-compute:latest
```
