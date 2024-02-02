# Download/Install compute
#Requires -RunAsAdministrator

$physicalPathRoot = "C:\inetpub\wwwroot\aspnet_client\system_web\4_0_30319"
$rhinoComputePath = "$physicalPathRoot\rhino.compute"
$computeGeometryPath = "$physicalPathRoot\compute.geometry"
$rhinoComputeExe = "$rhinoComputePath\rhino.compute.exe"
$computeGeometryExe = "$computeGeometryPath\compute.geometry.exe"
$appPoolName = "RhinoComputeAppPool"
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

# check that compute exists
if (((Test-Path "$computeGeometryExe") -eq $false) -or ((Test-Path "$rhinoComputeExe") -eq $false)) {
    Write-Host "The rhino.compute or compute.geometry executable file could not be found." -ForegroundColor Red
    Write-Host "Please run the bootstrap script first!" -ForegroundColor Red
    exit 1
}

Write-Step "Stopping any IIS services that are running"
net stop was /y

Write-Step 'Create backup'
$backupDir = "$physicalPathRoot\rhino.compute-backup"
if ((Test-Path "$backupDir") -eq $true) {
    Write-Host "Deleting '$backupDir'"
    Remove-Item -Recurse -Force "$backupDir"
}
New-Item -ItemType Directory -Force -Path $backupDir
Write-Host "Moving $rhinoComputePath to $backupDir\rhino.compute"
Move-Item -Path $rhinoComputePath -Destination $backupDir 
Write-Host "Moving $computeGeometryPath to $backupDir\compute.geometry"
Move-Item -Path $computeGeometryPath -Destination $backupDir 

$gitPrefix = 'https://api.github.com/repos'
$nightlyPrefix = 'https://nightly.link'
$actionurl = 'mcneel/compute.rhino3d/actions/artifacts'
$giturl = "$gitPrefix/$actionurl"

$response = Invoke-RestMethod -Method Get -Uri $giturl
$artifacts = $response.artifacts
$artifactID = -1
$matchingBranch = "7.x"

for($i=0; $i -lt $artifacts.Length; $i++){
    $latest = $artifacts[$i]
    $artifactID = $latest.id
    $artifactBranch = $latest.workflow_run.head_branch 
    if ($artifactBranch -eq $matchingBranch) {
        break
    }
}

if ($artifactID -lt 0){
    Write-Host "Unable to find the latest $matchingBranch build artifact." -ForegroundColor Red
    exit 1
}

$downloadurl = "$nightlyPrefix/$actionurl/$artifactID.zip"

if (-Not (Test-Path -Path $physicalPathRoot)){
    New-Item $physicalPathRoot -ItemType Directory
}

if ((Test-Path $physicalPathRoot)) {
    Write-Step "Download and unzip latest build of compute from $downloadurl"
    Download $downloadurl "$physicalPathRoot/compute.zip"
    Expand-Archive "$physicalPathRoot/compute.zip" -DestinationPath $physicalPathRoot
    Remove-Item "$physicalPathRoot/compute.zip"
}

Write-Step "Granting application pool permissions on compute directories" 
cmd /c icacls $rhinoComputePath /grant ("IIS AppPool\$appPoolName" + ':(OI)(CI)F') /t /c /q 
cmd /c icacls $computeGeometryPath /grant ("IIS AppPool\$appPoolName"+ ':(OI)(CI)F') /t /c /q 

Write-Step "Starting the IIS Service"
net start w3svc
Start-IISSite -Name $websiteName