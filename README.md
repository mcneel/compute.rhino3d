# RhinoCommon.Rest

[![Build status](https://ci.appveyor.com/api/projects/status/unmnwi57we5nvnfi/branch/master?svg=true)](https://ci.appveyor.com/project/mcneel/compute-rhino3d/branch/master)

REST geometry server based on RhinoCommon and headless Rhino

## Local Debug Builds

1. You need to have the Rhino WIP (V7) installed and run at least once.
1. Load RhinoCommon.Rest.sln and compile as debug
1. Start the application in the debugger.
1. You should be able to go to http://localhost to see the server working.

## Set up for Google Compute Engine

1. Build RhinoCommon.REST project in _Release_.
1. Create a Windows Server 2016 VM.
1. Remote desktop onto server.
1. Copy _x64/Release_ directory to desktop of the server.
1. Make http/https access available to Nancy:
    - Start PowerShell as Administrator.
    - `netsh http add urlacl url="http://+:80/" user="Everyone"`.
    - `netsh http add urlacl url="https://+:443/" user="Everyone"`.
1. Install IIS:
    - Start PowerShell as Administrator.
    - In PowerShell: `Install-WindowsFeature -name Web-Server -IncludeManagementTools`
1. Install Rhino using PowerShell (as administrator):
    - In PowerShell: `cd _C:\Users\[USERNAME]\Desktop\Release\deployment\_`.
    - Run the admin script using: `.\headless_admin.ps1 -updaterhino`.  This will download the Rhino installer and place it in the _deployment_ directory.
    - Once downloaded, double-click on _rhinoinstaller.exe_ and install like you typically would.
1. Run Rhino and set up a stand alone license key.  Validate your license.
1. Add LetsEncrypt SSL Certificate for HTTPS support:
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
1. Start _RhinoCommon.Rest_ as a service:
    - Start _cmd.exe_ as Administrator.
    - In _cmd_: `cd C:\Users\[USERNAME]\Desktop\Release\`
    - Run `RhinoCommon.Rest.exe install` to install as a service.
    - In the interactive menu, enter your username in the format `.\\[USERNAME]` (for example:`.\steve`) and use the administrator password for this account (this should be the Windows password created on the Google Compute Engine dashboard).
1. _(Optional)_ Install StackDriver client application
    - https://cloud.google.com/logging/docs/agent/installation
    - PowerShell `cd C:\Users\[USERNAME]
invoke-webrequest https://dl.google.com/cloudagents/windows/StackdriverLogging-v1-8.exe -OutFile StackdriverLogging-v1-8.exe;
.\StackdriverLogging-v1-8.exe`
1. _(Optional)_ Add private logging key
    - Create a logging account key by
    - going to https://console.cloud.google.com/apis/credentials
    - Click "Create Credentials" drop down and select "Service account key"
    - Make the service account stackdriver
    - This will download a "key" json file (the file name will match the account key id)
    - Place this json file in the deployment directory on your server.  compute.rhino3d will notice this file when it starts and use it to perform logging to stackdriver

## Configuration Options ##
All configuration of Compute is done via environment varibles.

**COMPUTE_HTTP_PORT**: integer, Default=80 (release builds) or 8888 (debug builds)

Port to run HTTP server. 

**COMPUTE_HTTPS_PORT**: integer, default=0. 

Port to run HTTPS server

**COMPUTE_AUTH_APIKEY**: bool, default = 0

Enables athentication via simple API key that looks like an email address.

**COMPUTE_AUTH_RHINOACCOUNT**: bool, default=0

Enables authentication via Rhino Accounts OAuth2 Token.

**COMPUTE_LOG_TEMPFILE**: bool, default=1

Enables logging to the temp directory.

**COMPUTE_LOG_RETAIN_DAYS**: integer, default=10

Delete log files after 10 days.

**COMPUTE_STASH_TEMPFILE**: bool, default=0

Enables stashing POST input data to a temp file.

**COMPUTE_STASH_AMAZONS3**: bool, default=0

Enables stashing POST input data to an Amazon S3 bucket

**COMPUTE_STASH_S3_BUCKET**: string

Name of the bucket where POST input data should be stashed.

**AWS_ACCESS_KEY**: string

Amazon Web Services Access Key for your account. If compute is running on EC2, consider using [EC2 Instance Profiles](https://docs.aws.amazon.com/IAM/latest/UserGuide/id_roles_use_switch-role-ec2_instance-profiles.html); Compute will find and use your credentials so they don't need to be on your instance.

**AWS_SECRET_ACCESS_KEY**: string

Amazon Web Services Secrete Access Key for your account. If compute is running on EC2, consider using [EC2 Instance Profiles](https://docs.aws.amazon.com/IAM/latest/UserGuide/id_roles_use_switch-role-ec2_instance-profiles.html); Compute will find and use your credentials so they don't need to be on your instance.

**AWS_REGION_ENDPOINT**: string, default="us-east-1""

Amazon Web Services [Region Endpoint](https://docs.aws.amazon.com/general/latest/gr/rande.html)

## Notes for future work
- There is a health check URL in case we want to set up a load balancer
    - On the Compute Engine web page, click on "Health Checks"
    - Click "create a new health check"
    - Set request path to "/healthcheck"
