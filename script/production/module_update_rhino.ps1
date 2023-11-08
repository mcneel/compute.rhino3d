# Setup/Install script for installing Rhino
#Requires -RunAsAdministrator

param (
    [Parameter(Mandatory=$true)][string] $EmailAddress,
    [switch] $install = $false
)

# This script installs the latest version of Rhino.
# * Make sure you run this script from a Powershell Admin Prompt!
# * Make sure Powershell Execution Policy is bypassed to run these scripts:
Set-ExecutionPolicy Bypass -Scope Process -Force

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
  #       U P D A T E    R H I N O        #
  #                                       #
  #             S C R I P T               #
  #                                       #
  # # # # # # # # # # # # # # # # # # # # #
"@

try {
    Write-Step 'Checking for update'
    $rhino7DownloadUrl = "https://www.rhino3d.com/download/rhino-for-windows/7/latest/direct/?email=$EmailAddress"
    if ((Get-Host).Version.Major -gt 5) {
        $uri = (Invoke-WebRequest -Method 'GET' -MaximumRedirection 0 -Uri $rhino7DownloadUrl -ErrorAction Ignore -SkipHttpErrorCheck).Headers.Location
    } else {
        $uri = (Invoke-WebRequest -Method 'GET' -MaximumRedirection 0 -Uri $rhino7DownloadUrl -ErrorAction Ignore).Headers.Location
    }
    $packageVersion = [Version][System.IO.Path]::GetFileNameWithoutExtension($uri).split('_')[-1]
    $installedVersion = [Version] (get-itemproperty -Path HKLM:\SOFTWARE\McNeel\Rhinoceros\7.0\Install -name "version").Version

    if ($installedVersion -ge $packageVersion) {
        Write-Host "Latest version avaliable is already installed! ($installedVersion >= $packageVersion)" -ForegroundColor Red
        exit 1
    }

    # Download and install Rhino
    Write-Step 'Download latest Rhino 7'
    $rhino7Setup = "rhino7_setup.exe"
    Download $rhino7DownloadUrl $rhino7Setup
  
    # Set firewall rule to allow installation
    New-NetFirewallRule -DisplayName "Rhino 7 Installer" -Direction Inbound -Program $rhino7Setup -Action Allow

    Write-Step 'Installing Rhino'
    # automated install (https://wiki.mcneel.com/rhino/installingrhino/7)
    Start-Process -FilePath $rhino7Setup -ArgumentList '-passive', '-norestart' -Wait
    # delete installer
    Remove-Item $rhino7Setup
    # Print installed version number
    $installedVersion = [Version] (get-itemproperty -Path HKLM:\SOFTWARE\McNeel\Rhinoceros\7.0\Install -name "version").Version
    Write-Step "Successfully installed $installedVersion"
}finally {}