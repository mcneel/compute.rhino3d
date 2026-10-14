# Download/Install compute
#Requires -RunAsAdministrator

$appDirectory = "C:\inetpub\wwwroot\aspnet_client\system_web\4_0_30319"

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

# A second run keeps the build already installed; module_update_compute.ps1 updates it.
if ((Test-Path "$appDirectory\rhino.compute\rhino.compute.exe") -and (Test-Path "$appDirectory\compute.geometry\compute.geometry.exe")) {
    Write-Step "rhino.compute and compute.geometry are already installed in $appDirectory"
    return
}

$gitPrefix = 'https://api.github.com/repos'
$nightlyPrefix = 'https://nightly.link'
$actionurl = 'mcneel/compute.rhino3d/actions/artifacts'
$giturl = "$gitPrefix/$actionurl"

$artifactID = -1
$matchingBranch = "9.x"
$artifactName = "rhino.compute"

# Newest first, all branches; skip other branches' builds and expired ones rather than settle for them.
$response = Invoke-RestMethod -Method Get -Uri "$($giturl)?name=$artifactName&per_page=100"
foreach ($artifact in $response.artifacts) {
    if ($artifact.expired -or $artifact.workflow_run.head_branch -ne $matchingBranch) { continue }
    $artifactID = $artifact.id
    break
}

if ($artifactID -lt 0){
    throw "Unable to find the latest $matchingBranch build artifact."
}

$downloadurl = "$nightlyPrefix/$actionurl/$artifactID.zip"

if (-Not (Test-Path -Path $appDirectory)){
    New-Item $appDirectory -ItemType Directory
}

if ((Test-Path $appDirectory)) {
    Write-Step "Download and unzip latest build of compute from $downloadurl"
    # What an interrupted run left behind.
    foreach ($folder in "rhino.compute", "compute.geometry") {
        if (Test-Path "$appDirectory\$folder") { Remove-Item -Recurse -Force "$appDirectory\$folder" }
    }
    Download $downloadurl "$appDirectory/compute.zip"
    Expand-Archive "$appDirectory/compute.zip" -DestinationPath $appDirectory
    Remove-Item "$appDirectory/compute.zip"
}