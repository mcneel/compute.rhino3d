# RhinoCommon.Rest
REST geometry server based on RhinoCommon and headless Rhino


## Set up for Google Compute Engine
- Compile RhinoCommon.REST in Release
- Create a Windows Server 2016 VM
- Remote desktop onto server
- copy x64/Release directory to desktop
- Make http/https access available to Nancy
    - Start PowerShell as admin
    - `netsh http add urlacl url="http://+:80/" user="Everyone"`
    - `netsh http add urlacl url="https://+:443/" user="Everyone"`
- Install IIS
    - Start PowerShell as admin
    - `Install-WindowsFeature -name Web-Server -IncludeManagementTools`
- Install Rhino
    - Go to Release/deployment directory
    - Run PowerShell with the `downloadrhino.ps1` script
    - This will download the Rhino installer and place it in the deployment directory
    - Double click on Rhino installer and install like you typically would
- Run Rhino and set up a stand alone license key
- Copy `RhinoLibrary.dll` in the deployment directory to the installed Rhino system directory.
- Add LetsEncrypt SSL Certificate for HTTPS support
    - Download from https://github.com/PKISharp/win-acme/releases/tag/v1.9.8.4
    - Unzip download on server
    - Start PowerShell as admin
    - cd to unzipped directory
    - `.\letsencrypt.exe`
    - `N` create new certificate
    - `4` manually input host names
    - `compute.rhino3d.com`
    - `1` default web site
    - `steve@mcneel.com`
    - `yes`
    - `Q` quit
- Double click on `RhinoCommon.Rest.exe` to start the server
- Install StackDriver client application
    - https://cloud.google.com/logging/docs/agent/installation
    - PowerShell `cd C:\Users\[USERNAME]
invoke-webrequest https://dl.google.com/cloudagents/windows/StackdriverLogging-v1-8.exe -OutFile StackdriverLogging-v1-8.exe;
.\StackdriverLogging-v1-8.exe`


## Notes for future work
- There is a health check URL in case we want to set up a load balancer
    - On the Compute Engine web page, click on "Health Checks"
    - Click "create a new health check"
    - Set request path to "/healthcheck"