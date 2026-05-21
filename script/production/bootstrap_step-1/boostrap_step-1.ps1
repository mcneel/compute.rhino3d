# This script installs and configures everything needed to run rhino.compute under IIS:
# Rhino, firewall/URL ACLs, the IIS feature set, the compute artifact, the ASP.NET Core
# Hosting Bundle, and IIS site/app-pool configuration.
#
# * Make sure you run this script from a Powershell Admin Prompt!
# * Make sure Powershell Execution Policy is bypassed to run these scripts:
Set-ExecutionPolicy Bypass -Scope Process -Force

#Region funcs
function Write-Step {
  Write-Host
  Write-Host "===> "$args[0] -ForegroundColor Green
  Write-Host
}
#EndRegion funcs

# Create a folder for all installation information
$installPath = "C:\Rhino-Compute-Installation"
$tempName = "Temp"
$logFileName = "bootstrap_log.txt"
$tmpFullPath = Join-Path -Path $installPath -ChildPath $tempName
$logFullPath = Join-Path -Path $installPath -ChildPath $logFileName
if (-not (Test-Path -Path $tmpFullPath -PathType Container)) {
    # If the folder does not exist, create it as a Directory
    New-Item -ItemType Directory -Path $tmpFullPath
    Write-Host "'$tmpFullPath' created successfully."
} else {
    Write-Host "'$tmpFullPath' already exists."
}

$ErrorActionPreference="SilentlyContinue"
Stop-Transcript | out-null
$ErrorActionPreference = "Continue"
Start-Transcript -path $logFullPath -append

$startTime = Get-Date -Format "dddd, MMMM dd yyyy hh:mm:ss tt"
Write-Step "Transcript started at: $startTime"

#In case $PSScriptRoot is empty (version of powershell V.2).
if(!$PSScriptRoot){ $PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent }

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

Write-Host
Write-Host "Root Script Path:" $PSScriptRoot

# Run modules in order. 
& "$PSScriptRoot\module_rhino.ps1"
& "$PSScriptRoot\module_firewall.ps1"
& "$PSScriptRoot\module_iis_install.ps1"
& "$PSScriptRoot\module_compute.ps1"
& "$PSScriptRoot\module_hostingbundle.ps1"
& "$PSScriptRoot\module_iis_configure.ps1"

$endTime = Get-Date -Format "dddd, MMMM dd yyyy hh:mm:ss tt"
Write-Step "Bootstrap installation ended at: $endTime"

Stop-Transcript

