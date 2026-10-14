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

try { Stop-Transcript | Out-Null } catch { }
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

# Any error stops the bootstrap (the modules inherit this), so it never carries on past a failed step.
$ErrorActionPreference = "Stop"
$modules = @("module_rhino.ps1", "module_firewall.ps1", "module_iis_install.ps1", "module_compute.ps1", "module_hostingbundle.ps1", "module_iis_configure.ps1")
$currentModule = $null
$failure = $null
try {
    # check os is server
    $os = (Get-CimInstance -ClassName 'Win32_OperatingSystem').Caption
    if ($os -notlike '*server*') {
        throw "The script is intended for use on Windows Server. Detected '$os'"
    }

    Write-Host
    Write-Host "Root Script Path:" $PSScriptRoot

    # Run modules in order.
    foreach ($module in $modules) {
        $currentModule = $module
        & "$PSScriptRoot\$module"
    }
}
catch {
    $failure = $_
}
finally {
    $endTime = Get-Date -Format "dddd, MMMM dd yyyy hh:mm:ss tt"
    if ($failure) {
        Write-Host
        $where = if ($currentModule) { " in $currentModule" } else { "" }
        Write-Host "Bootstrap FAILED$($where): $($failure.Exception.Message)" -ForegroundColor Red
        Write-Host $failure.InvocationInfo.PositionMessage
        Write-Host "Log file: $logFullPath"
    }
    Write-Step "Bootstrap installation ended at: $endTime"
    Stop-Transcript
}
if ($failure) { exit 1 }

