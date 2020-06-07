# Setup/Install script for compute on a web server

#Requires -RunAsAdministrator

param (
    [Parameter(Mandatory=$true)][string] $EmailAddress,
    [Parameter(Mandatory=$true)][string] $ApiKey,
    [Parameter(Mandatory=$true)][string] $PublicDns
)

if ($PSBoundParameters.ContainsKey('ApiKey')) {
    [System.Environment]::SetEnvironmentVariable("RHINO_COMPUTE_KEY", $ApiKey, "Machine")
}

# Install IIS
Install-WindowsFeature -name Web-Server -IncludeManagementTools

# Configure URL reservation
Start-Process "netsh" -ArgumentList "http", "add", "urlacl", "url='http://+:80/'", "user='Everyone'"
Start-Process "netsh" -ArgumentList "http", "add", "urlacl", "url='http://+:443/'", "user='Everyone'"

# Set up HTTPS support
if ((Test-Path "C:\Users/Administrator\Desktop\win-acme") -eq $false) {
    Invoke-WebRequest "https://github.com/win-acme/win-acme/releases/download/v2.1.8.838/win-acme.v2.1.8.838.x64.pluggable.zip" -OutFile "win-acme.zip"
    Expand-Archive "win-acme.zip" -DestinationPath "C:\Users\Administrator\Desktop\win-acme"
    Remove-Item "win-acme.zip"

    # IN PROGRESS - run wacs to set up HTTPS support
    # $winacme = "C:\Users\Administrator\Desktop\win-acme\wacs.exe"

    # TODO: Figure out how to automatically determine the public DNS for this server
    # $PublicDns = "ec2-54-159-4-143.compute-1.amazonaws.com"
}

# Download and unzip latest build of compute
if ((Test-Path "C:\Users\Administrator\Desktop\compute") -eq $false) {
    Invoke-WebRequest "https://ci.appveyor.com/api/projects/mcneel/compute-rhino3d/artifacts/compute.zip?branch=master" -OutFile "compute.zip"
    Expand-Archive "compute.zip" -DestinationPath "C:\Users\Administrator\Desktop\compute"
    Remove-Item "compute.zip"
}

# Download latest Rhino 7
$rhino7DownloadUrl = "https://www.rhino3d.com/download/rhino-for-windows/7/wip/direct?email='$EmailAddress'"
$rhino7Setup = "rhino7_setup.exe"
Invoke-WebRequest $rhino7DownloadUrl -OutFile $rhino7Setup

# Automated install
# https://wiki.mcneel.com/rhino/installingrhino/6
Start-Process -FilePath $rhino7Setup -ArgumentList "-passive" -Wait

# delete installer
Remove-Item $rhino7Setup
