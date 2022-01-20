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

$gitPrefix = 'https://api.github.com/repos'
$nightlyPrefix = 'https://nightly.link'
$actionurl = 'mcneel/compute.rhino3d/actions/artifacts'
$giturl = "$gitPrefix/$actionurl"

$response = Invoke-RestMethod -Method Get -Uri $giturl
$artifacts = $response.artifacts
$latest = $artifacts[0]
$artifactID = $latest.id
$downloadurl = "$nightlyPrefix/$actionurl/$artifactID.zip"

if (-Not (Test-Path -Path $appDirectory)){
    New-Item $appDirectory -ItemType Directory
}

if ((Test-Path $appDirectory)) {
    Write-Step 'Download and unzip latest build of compute from $downloadurl'
    Download $downloadurl "$appDirectory/compute.zip"
    Expand-Archive "$appDirectory/compute.zip" -DestinationPath $appDirectory
    Remove-Item "$appDirectory/compute.zip"
}