# RhinoCommon.Rest
REST geometry server based on RhinoCommon and headless Rhino


## Set up for Google Compute Engine
- Compile RhinoCommon.REST in release
- Create a Windows Server 2016 VM
- Remote desktop onto server
- copy x64/Release directory to desktop
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
- Double click on `RhinoCommon.Rest.exe` to start the server
- Install StackDriver client application
    - https://cloud.google.com/logging/docs/agent/installation
    - PowerShell `cd C:\Users\[USERNAME]
invoke-webrequest https://dl.google.com/cloudagents/windows/StackdriverLogging-v1-8.exe -OutFile StackdriverLogging-v1-8.exe;
.\StackdriverLogging-v1-8.exe`


## Optional - set up for load balancer (in progress)
- Add health check
    - On the Compute Engine web page, click on "Health Checks"
    - Click "create a new health check"
    - Set request path to "/healthcheck"