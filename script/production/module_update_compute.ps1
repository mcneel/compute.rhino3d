# Download/Install compute
#Requires -RunAsAdministrator

$physicalPathRoot = "C:\inetpub\wwwroot\aspnet_client\system_web\4_0_30319"
$rhinoComputePath = "$physicalPathRoot\rhino.compute"
$computeGeometryPath = "$physicalPathRoot\compute.geometry"
$websiteName = "Rhino.Compute"

#Region funcs
function Write-Step { 
    Write-Host
    Write-Host "===> "$args[0] -ForegroundColor Green
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

Write-Host @"
  # # # # # # # # # # # # # # # # # # # # #
  #                                       #
  #             U P D A T E               #
  #                                       #
  #       R H I N O . C O M P U T E       #
  #                                       #
  #             S C R I P T               #
  #                                       #
  # # # # # # # # # # # # # # # # # # # # #
"@

Write-Step "Stopping any IIS services that are running"
net stop was /y

if ((Test-Path -Path $physicalPathRoot)) {
    Write-Step "Removing any existing Rhino.Compute build directories"
    Remove-Item -LiteralPath $physicalPathRoot -Force -Recurse
}

$gitPrefix = 'https://api.github.com/repos'
$nightlyPrefix = 'https://nightly.link'
$actionurl = 'mcneel/compute.rhino3d/actions/artifacts'
$giturl = "$gitPrefix/$actionurl"

$response = Invoke-RestMethod -Method Get -Uri $giturl
$artifacts = $response.artifacts
$latest = $artifacts[0]
$artifactID = $latest.id
$downloadurl = "$nightlyPrefix/$actionurl/$artifactID.zip"

if (-Not (Test-Path -Path $physicalPathRoot)){
    New-Item $physicalPathRoot -ItemType Directory
}

if ((Test-Path -Path $physicalPathRoot)) {
    Write-Step "Download and unzip latest build of compute from $downloadurl"
    Download $downloadurl "$physicalPathRoot/compute.zip"
    Expand-Archive "$physicalPathRoot/compute.zip" -DestinationPath $physicalPathRoot
    Remove-Item "$physicalPathRoot/compute.zip"
}

Write-Step "Starting the IIS Service"
net start w3svc
Start-IISSite -Name $websiteName