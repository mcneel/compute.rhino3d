Param(
  [switch]$initialize = $false,
  [switch]$updaterhino = $false,
  [string]$LICENSE_KEY
)

if ($initialize) {

    # Make http/https access available to Nancy
    netsh http add urlacl url="http://+:80/" user="Everyone"
    netsh http add urlacl url="https://+:443/" user="Everyone"

    # Install IIS
    Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process -Force
    Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole
    Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServer
    Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerManagementTools

    # TODO: Download/Install LetsEncrypt application for SSL
    # TODO: Download/Install StackDriver client application
}

if ($updaterhino) {
    # TODO: probably need to stop the RhinoCommon.Rest service

    # Download Rhino
    $url = "http://files.mcneel.com/dujour/exe/20180812/rhino_en-us_7.0.18224.20205.exe"
    $rhino_installer = $PSScriptRoot + "\rhinoinstaller.exe"
    (New-Object System.Net.WebClient).DownloadFile($url, $rhino_installer)
    #Invoke-WebRequest -Uri $url -OutFile $rhino_installer

    # Install Rhino
    if ($LICENSE_KEY) {
        & $rhino_installer LICENSE_METHOD=STANDALONE LICENSE_KEY=$LICENSE_KEY
    } else {
        & $rhino_installer
    }
}
