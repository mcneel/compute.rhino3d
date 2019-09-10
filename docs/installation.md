## Getting Started

1. Get the [latest build](https://ci.appveyor.com/project/mcneel/compute-rhino3d/branch/master/artifacts) from the `master` branch (or [build from source](#building-from-source)).
1. Create a Windows Server 2016 computer. (Also a Windows 10 desktop computer with Rhino installed can be used, typically for developers sending debug requests to localhost).
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
See [environment variables](environment_variables.md) for details

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

## Licensing Rhino and Scaling
Rhino WIP (7) encrypts license information by default, and gets a never-expiring login token from Rhino Accounts. In order to scale your compute service on Amazon Web Services or other PaaS services, you'll need to disable encryption of the license information before creating your machine image.

1. Open Rhino on the template host machine.
1. From the **Tools** menu, click **Options** then click **Advanced**.
1. Search for **Rhino.LicensingSettings.CloudZooPlainText**
1. Select the checkbox to enable the plain text setting.
1. **Important!** Close all instances of Rhino (Changes do not take effect until Rhino is restarted)
1. Start Rhino.
1. Login to the respective Rhino account.
1. Close Rhino.
1. Create your machine instance.

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
