# This script installs and configures everything needed to run rhino.compute under IIS:
# Rhino, firewall rules, the IIS feature set, the compute artifact, the ASP.NET Core
# Hosting Bundle, and IIS site/app-pool configuration.
#
# * Make sure you run this script from a Powershell Admin Prompt!
# * It asks for the email address, API key and Rhino token unless they're passed as parameters.
param (
    [string] $EmailAddress,
    [string] $ApiKey,
    [string] $RhinoToken,
    [switch] $SkipHopsInstall
)
# * Make sure Powershell Execution Policy is bypassed to run these scripts:
Set-ExecutionPolicy Bypass -Scope Process -Force

#Region funcs
. "$PSScriptRoot\common.ps1"
function Read-Required {
  param ([Parameter(Mandatory=$true)][string] $prompt)
  do { $value = (Read-Host $prompt).Trim() } while (-not $value)
  return $value
}
# The transcript's header repeats PowerShell's command line, so values passed as parameters end up in the log.
function Hide-Secrets {
  param ([Parameter(Mandatory=$true)][string] $path, [string[]] $secrets)
  $text = [System.IO.File]::ReadAllText($path)
  foreach ($secret in $secrets) {
    if ($secret) { $text = $text.Replace($secret, "***") }
  }
  [System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
}
#EndRegion funcs

# Create a folder for all installation information
$installPath = "C:\Rhino-Compute-Installation"
$tempName = "Temp"
$logFileName = "bootstrap_log.txt"
$tmpFullPath = Join-Path -Path $installPath -ChildPath $tempName
$logFullPath = Join-Path -Path $installPath -ChildPath $logFileName
$credentialFile = Join-Path -Path $installPath -ChildPath "RhinoComputeUser.txt"
if (-not (Test-Path -Path $tmpFullPath -PathType Container)) {
    # If the folder does not exist, create it as a Directory
    New-Item -ItemType Directory -Path $tmpFullPath
    Write-Host "'$tmpFullPath' created successfully."
} else {
    Write-Host "'$tmpFullPath' already exists."
}

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

# Asked before the transcript starts, so the API key and Rhino token stay out of the log.
if (-not $EmailAddress) { $EmailAddress = Read-Required "Email address" }
if (-not $ApiKey) { $ApiKey = Read-Required "API key" }
if (-not $RhinoToken) { $RhinoToken = Read-Required "Rhino token" }

try { Stop-Transcript | Out-Null } catch { }
Start-Transcript -path $logFullPath -append

$startTime = Get-Date -Format "dddd, MMMM dd yyyy hh:mm:ss tt"
Write-Step "Transcript started at: $startTime"

# Any error stops the bootstrap (the modules inherit this), so it never carries on past a failed step.
$ErrorActionPreference = "Stop"
# Windows PowerShell's progress bar slows Invoke-WebRequest and Expand-Archive down many times over.
$ProgressPreference = "SilentlyContinue"
$modules = @("module_rhino.ps1", "module_firewall.ps1", "module_iis_install.ps1", "module_compute.ps1", "module_hostingbundle.ps1", "module_iis_configure.ps1")
$moduleArguments = @{
    "module_rhino.ps1" = @{ EmailAddress = $EmailAddress; ApiKey = $ApiKey; RhinoToken = $RhinoToken; TempPath = $tmpFullPath }
    "module_hostingbundle.ps1" = @{ TempPath = $tmpFullPath }
    "module_iis_configure.ps1" = @{ TempPath = $tmpFullPath; CredentialFile = $credentialFile; SkipHopsInstall = $SkipHopsInstall }
}
$currentModule = $null
$failure = $null
try {
    Write-Host
    Write-Host "Root Script Path:" $PSScriptRoot

    # Run modules in order.
    foreach ($module in $modules) {
        $currentModule = $module
        $arguments = $moduleArguments[$module]
        if (-not $arguments) { $arguments = @{} }
        & "$PSScriptRoot\$module" @arguments
    }
}
catch {
    $failure = $_
}
finally {
    $endTime = Get-Date -Format "dddd, MMMM dd yyyy hh:mm:ss tt"
    Write-Host
    if ($failure) {
        $where = if ($currentModule) { " in $currentModule" } else { "" }
        Write-Host "Bootstrap FAILED$($where): $($failure.Exception.Message)" -ForegroundColor Red
        Write-Host $failure.InvocationInfo.PositionMessage
        Write-Host "Log file: $logFullPath"
    } else {
        Write-Host "Congratulations! All components have now been installed." -ForegroundColor Green
    }
    Write-Step "Bootstrap installation ended at: $endTime"
    Stop-Transcript
    try { Hide-Secrets $logFullPath @($ApiKey, $RhinoToken) }
    catch { Write-Warning "Couldn't remove the API key and Rhino token from ${logFullPath}: $($_.Exception.Message)" }
}

# Shown once the transcript has stopped, so the password isn't in the log.
if ($failure) {
    if (Test-Path $credentialFile) { Write-Host "The RhinoComputeUser password is in $credentialFile (Administrators only)." }
    exit 1
}
Write-Host
Write-Host "To install third party plugins follow these steps"
Write-Host "  1) Log into your VM using these credentials (write these down)"
Get-Content $credentialFile | ForEach-Object { Write-Host "     $_" }
Write-Host "  2) Install plugins using the Rhino package manager or"
Write-Host "  3) Copy/paste plugin files to C:\Users\RhinoComputeUser\AppData\Roaming\Grasshopper\Libraries"
Write-Host "  4) Restart the VM"
Write-Host
Write-Host "The User Name and Password are also saved in $credentialFile, which only Administrators can read. They're not in the log file."
