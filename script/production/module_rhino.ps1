# Setup/Install script for installing Rhino
#Requires -RunAsAdministrator

param (
    [Parameter(Mandatory=$true)][string] $EmailAddress,
    [Parameter(Mandatory=$true)][string] $ApiKey,
    [Parameter(Mandatory=$true)][string] $RhinoToken,
    [switch] $install = $false
)

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

# Download and install Rhino
Write-Step 'Download latest Rhino 7'
$rhino7DownloadUrl = "https://www.rhino3d.com/download/rhino-for-windows/7/latest/direct?email=$EmailAddress"
$rhino7Setup = "rhino7_setup.exe"
Download $rhino7DownloadUrl $rhino7Setup
# TODO: print rhino version

Write-Step 'Installing Rhino'
# automated install (https://wiki.mcneel.com/rhino/installingrhino/6)
Start-Process -FilePath $rhino7Setup -ArgumentList '-passive', '-norestart' -Wait
# delete installer
Remove-Item $rhino7Setup