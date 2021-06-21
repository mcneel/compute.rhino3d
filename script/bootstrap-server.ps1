# Setup/Install script for compute on a web server
#Requires -RunAsAdministrator

param (
    [Parameter(Mandatory=$true)][string] $EmailAddress,
    [Parameter(Mandatory=$true)][string] $ApiKey,
    [Parameter(Mandatory=$true)][string] $RhinoToken,
    [switch] $install = $false
)

Write-Host @"

  # # # # # # # # # # # # # # # # # # # # #
  #                                       #
  #       R H I N O   C O M P U T E       #
  #                                       #
  #    B O O T S T R A P   S C R I P T    #
  #                                       #
  # # # # # # # # # # # # # # # # # # # # #

"@

# check os is server
$os = (Get-CimInstance -ClassName 'Win32_OperatingSystem').Caption
if ($os -notlike '*server*') {
    Write-Host "The script is intended for use on Windows Server. Detected '$os'" -ForegroundColor Red
    exit 1
}

$appDirectory = "$home\Desktop"

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
function SetEnvVar {
    param (
        [Parameter(Mandatory=$true)][string] $name,
        [Parameter(Mandatory=$true)][string] $value,
        [switch] $secret = $false
    )
    $print = if ($secret) {"***"} else {$value}
    Write-Host "Setting environment variable: $name=$print"
    [System.Environment]::SetEnvironmentVariable($name, $value, "Machine")
}
#EndRegion funcs

Write-Step 'Set environment variables'
SetEnvVar 'RHINO_TOKEN' $RhinoToken -secret
if ($PSBoundParameters.ContainsKey('ApiKey')) {
    SetEnvVar 'RHINO_COMPUTE_KEY' $ApiKey
}
SetEnvVar 'RHINO_COMPUTE_URLS' 'http://+:80'

Write-Step 'Install IIS'
Install-WindowsFeature -name Web-Server -IncludeManagementTools

Write-Step 'Configure URL reservation (80 and 443)'
Start-Process "netsh" -ArgumentList "http", "add", "urlacl", "url='http://+:80/'", "user='Everyone'"
Start-Process "netsh" -ArgumentList "http", "add", "urlacl", "url='https://+:443/'", "user='Everyone'"

Write-Step 'Download and unzip latest build of compute'
if ((Test-Path "$appDirectory\compute") -eq $false) {
    Download "https://ci.appveyor.com/api/projects/mcneel/compute-rhino3d/artifacts/compute.zip?branch=master" "compute.zip"
    Expand-Archive "compute.zip" -DestinationPath "$appDirectory\compute"
    Remove-Item "compute.zip"
}

Write-Step 'Download latest Rhino 7'
$rhino7DownloadUrl = "https://www.rhino3d.com/download/rhino-for-windows/7/latest/direct?email=$EmailAddress"
$rhino7Setup = "rhino7_setup.exe"
Download $rhino7DownloadUrl $rhino7Setup
# TODO: print rhino version

Write-Step 'Install Rhino'
# automated install (https://wiki.mcneel.com/rhino/installingrhino/6)
Start-Process -FilePath $rhino7Setup -ArgumentList '-passive', '-norestart' -Wait
# delete installer
Remove-Item $rhino7Setup


Write-Step 'Install compute.geometry service'
if ($install) {
    Write-Host 'Installing compute.geometry as a service...'
    Write-Host 'Enter the username (with domain) and password of the Windows user to run as'
    Write-Host "(e.g. to run as the current user, use '.\$env:UserName' as the username)"
    Start-Process "$appDirectory\compute\compute.geometry.exe" -ArgumentList "install" -Wait
} else {
    Write-Host "No '-install' flag. Skipping..."
}

Write-Step 'Restart Windows to complete setup!'
Write-Host 'Rebooting in 5 seconds...'
shutdown /r /t 5
