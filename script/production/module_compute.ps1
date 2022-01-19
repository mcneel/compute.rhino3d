# Download/Install compute
#Requires -RunAsAdministrator

$appDirectory = "C:\inetpub\wwwroot\aspnet_client\system_web\4_0_30319"

#Region funcs
function Write-Step { 
    Write-Host
    Write-Host "===> "$args[0] -ForegroundColor Darkgreen
    Write-Host
}
function Download {
    param (
        [Parameter(Mandatory=$true)][string] $url,
        [Parameter(Mandatory=$true)][string] $output
    )
    (New-Object System.Net.WebClient).DownloadFile($url, $output)
}
#EndRegion funcs

# Download and install compute
Write-Step 'Download and unzip latest build of compute'
if (-Not (Test-Path -Path "$appDirectory\compute.geometry")){
    New-Item "$appDirectory\compute.geometry" -ItemType Directory
}

if ((Test-Path "$appDirectory\compute.geometry")) {
    Download "https://ci.appveyor.com/api/projects/mcneel/compute-rhino3d/artifacts/compute.zip?branch=master" "compute.zip"
    Expand-Archive "compute.zip" -DestinationPath "$appDirectory\compute.geometry"
    Remove-Item "compute.zip"
}