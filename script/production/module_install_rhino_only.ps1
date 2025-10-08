# Setup/Install script for installing Rhino
#Requires -RunAsAdministrator

param (
    [Parameter(Mandatory=$true)][string] $EmailAddress,
    [Parameter(Mandatory=$true)][string] $ApiKey,
    [Parameter(Mandatory=$true)][string] $RhinoToken,
    [switch] $install = $false
)
# * Make sure you run this script from a Powershel Admin Prompt!
# * Make sure Powershell Execution Policy is bypassed to run these scripts:
Set-ExecutionPolicy Bypass -Scope Process -Force

# Create a folder for all installation information
$installPath = "C:\Rhino Compute Installation"
$tempName = "Temp"
$logFileName = "bootstrap_step-1_log.txt"
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

#In case if $PSScriptRoot is empty (version of powershell V.2).  
if(!$PSScriptRoot){ $PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent } 

Write-Host @"

  # # # # # # # # # # # # # # # # # # # # #
  #                                       #
  #       R H I N O   I N S T A L L       #
  #                                       #
  #              S C R I P T              #
  #                                       #
  # # # # # # # # # # # # # # # # # # # # #

"@

# check os is server
$os = (Get-CimInstance -ClassName 'Win32_OperatingSystem').Caption
if ($os -notlike '*server*') {
    Write-Host "The script is intended for use on Windows Server. Detected '$os'" -ForegroundColor Red
    exit 1
}

Write-Host "Root Script Path:" $PSScriptRoot

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
Write-Step 'Download latest Rhino 8'
$rhinoDownloadUrl = "https://www.rhino3d.com/www-api/download/direct/?slug=rhino-for-windows/8/latest/?email=$EmailAddress" 
$rhinoSetup = "rhino_setup.exe"
$setupFullPath = Join-Path -Path $tmpFullPath -ChildPath $rhinoSetup
Download $rhinoDownloadUrl $setupFullPath

# Set firewall rule to allow installation
New-NetFirewallRule -DisplayName "Rhino 8 Installer" -Direction Inbound -Program $setupFullPath -Action Allow

Write-Step 'Installing Rhino'
# Automated install (https://wiki.mcneel.com/rhino/installingrhino/8)
$process = Start-Process -FilePath $setupFullPath -ArgumentList '-passive', '-norestart' -PassThru 
$process.WaitForExit()

if ($process.ExitCode -eq 0) {
    # delete installer
    #Remove-Item $rhinoSetup
    # Print installed version number
    $installedVersion = [Version] (get-itemproperty -Path HKLM:\SOFTWARE\McNeel\Rhinoceros\8.0\Install -name "version").Version
    Write-Host "Successfully installed Rhino $installedVersion"
} else {
    Write-Host "Process '$setupFullPath' finished with an error. Exit Code: $($process.ExitCode)"
}

Stop-Transcript