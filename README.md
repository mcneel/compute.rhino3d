# Rhino Compute Server

[![Build status](https://ci.appveyor.com/api/projects/status/unmnwi57we5nvnfi/branch/master?svg=true)](https://ci.appveyor.com/project/mcneel/compute-rhino3d/branch/master) [![Discourse users](https://img.shields.io/discourse/https/discourse.mcneel.com/users.svg)](https://discourse.mcneel.com/c/serengeti/compute-rhino3d)

A REST api exposing Rhino's geometry core. This project has two web services: `compute.geometry` which provides the REST API, and `compute.frontend` that provides authentication, request stashing (saving POST data for diagnostics), logging, and configuration of request and response headers. `compute.frontend` creates the `compute.geometry` process, monitors its health, and restarts `compute.geometry` as necessary.


## Getting Started

1. Get the [latest build](https://ci.appveyor.com/project/mcneel/compute-rhino3d/branch/master/artifacts) from the `master` branch (or [build from source](#building-from-source)).
1. Create a Windows Server 2016 computer.
1. Remote desktop onto server.
1. Copy `src/bin/Release` to the server.
1. Install Rhino using PowerShell\* (as administrator):
    - In PowerShell: `cd _C:\Users\[USERNAME]\Desktop\Release\deployment\_`.
    - Run the admin script using: `.\headless_admin.ps1 -updaterhino`.  This will download the Rhino installer and place it in the _deployment_ directory.
    - Once downloaded, double-click on _rhinoinstaller.exe_ and install like you typically would.
1. Run and license Rhino. Be sure to validate your license.
1. Open _cmd.exe_, `cd` to the `Release` directory and run `compute.frontend.exe`.
1. For next steps, see [Configuration](#configuration) and [Running Compute as a service](#running-compute-as-a-service).

\* We include a PowerShell script to make easy to download Rhino on Windows Server with it's strict default internet security settings.


## Configuration

### URL reservation

Release builds of Compute listen on all available IP addreses by default. For this to work, you must:

1. Start PowerShell as Administrator.
1. For HTTP, `netsh http add urlacl url="http://+:80/" user="Everyone"`.
1. For HTTPS, `netsh http add urlacl url="https://+:443/" user="Everyone"`.

### Environment variables

All configuration of Compute is done via environment variables.

| Environment variable | Type | Default | Description |
| -------------------- | ---- | ------- | ----------- |
| `COMPUTE_HTTP_PORT` | `integer` | `80` (Release), `8888` (Debug) | Port to run HTTP server |
| `COMPUTE_HTTPS_PORT` | `integer` | `0` | Port to run HTTPS server |
| `COMPUTE_SPAWN_GEOMETRY_SERVER` | `bool` | `true` (Release), `false` (Debug) | When True, `compute.frontend` will spawn `compute.geometry` at http://localhost on port `COMPUTE_BACKEND_PORT`. Defaults to `false` in Debug so that you can run both `compute.geometry` and `compute.frontend` in the debugger. Configure this in `Solution > Properties > Startup Project`. |
| `COMPUTE_BACKEND_PORT` | `integer` | `8081` | Sets the TCP port where `compute.geometry` runs. |
| `COMPUTE_AUTH_METHOD` | `string` | | `RHINO_ACCOUNT`: Enables authentication via Rhino Accounts OAuth2 Token. Get your token at https://www.rhino3d.com/compute/login and pass it using a Bearer Authentication header in your HTTP request: `Authorization: Bearer <YOUR TOKEN>`.<br>`API_KEY`: Enables athentication via simple API key that looks like an email address. |
| `COMPUTE_LOG_RETAIN_DAYS` | `integer` | `10` | Delete log files after 10 days. |
| `COMPUTE_STASH_METHOD` | `string` | `TEMPFILE` | `TEMPFILE`: Enables stashing POST input data to a temp file.<br>`AMAZONS3`: Enables stashing POST input data to an Amazon S3 bucket. |
| `COMPUTE_STASH_S3_BUCKET` | `string` | | Name of the Amazon S3 bucket where POST input data should be stashed. Requires `COMPUTE_STASH_METHOD=AMAZONS3` |
| `AWS_ACCESS_KEY` | `string` | | Amazon Web Services Access Key for your account. If compute is running on EC2, consider using [EC2 Instance Profiles](https://docs.aws.amazon.com/IAM/latest/UserGuide/id_roles_use_switch-role-ec2_instance-profiles.html); Compute will find and use your credentials so they don't need to be on your instance. |
| `AWS_SECRET_ACCESS_KEY` | `string` | | Amazon Web Services Secrete Access Key for your account. If compute is running on EC2, consider using [EC2 Instance Profiles](https://docs.aws.amazon.com/IAM/latest/UserGuide/id_roles_use_switch-role-ec2_instance-profiles.html); Compute will find and use your credentials so they don't need to be on your instance. |
| `AWS_REGION_ENDPOINT` | `string` | `"us-east-1"` | Amazon Web Services [Region Endpoint](https://docs.aws.amazon.com/general/latest/gr/rande.html) |
<!-- | `COMPUTE_LOG_METHOD` | `string` | `TEMPFILE` | `TEMPFILE`: Enables logging to the temp directory. | -->

### SSL certificate (optional)

Add LetsEncrypt SSL Certificate for HTTPS support:

- Download from https://github.com/PKISharp/win-acme/releases/tag/v1.9.8.4
- Unzip download on the server.
- Start PowerShell as Administrator.
- cd to unzipped directory.
- `.\letsencrypt.exe`
- `N` create new certificate.
- `4` manually input host names.
- `compute.rhino3d.com` (or similar)
- `1` for default web site.
- `you@yourdomain.com` for the user to receive issues.
- `yes` to accept the license agreement.
- `Q` to Quit.


## Running Compute as a service

Compute uses [TopShelf](https://github.com/topshelf/topshelf) to make it easy to configure and run it as a service on Windows.

1. Start _cmd.exe_ as Administrator.
1. In _cmd_: `cd C:\Users\[USERNAME]\Desktop\Release\`
1. Run `compute.frontend install` to install as a service.
1. In the interactive menu, enter your username in the format `.\\[USERNAME]` (for example:`.\steve`) and use the administrator password for this account.


## Building from source

1. Install [Rhino WIP](https://www.rhino3d.com/download/rhino-for-windows/wip).
1. Start Rhino WIP to configure its license.
1. Load compute.sln and compile as `Debug`.
1. In `Solution Explorer`, right-click `Solution 'compute'`, then click `Properties`
1. In the `Startup Project` tab, select `Multiple Startup Projects`, then set both `compute.frontend` and `compute.geometry` to `Start`.
1. Start the application in the debugger.
1. Browse to http://localhost:8888/version or http://localhost:8888/sdk


## Notes

- There is a health check URL (`/healthcheck`) in case you want to set up a load balancer
