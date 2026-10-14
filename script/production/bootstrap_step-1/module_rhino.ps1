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
function Install-ProcessWithTimeout {
    param (
        [string]$ExePath,
        [string[]]$Arguments,
        [System.Management.Automation.PSCredential]$Credential = $null,
        [int]$TimeoutSeconds = 600,   # default 10 min timeout
        [int]$MaxRetries = 2
    )

    for ($attempt = 1; $attempt -le $MaxRetries; $attempt++) {
        Write-Host "Starting attempt {$attempt}: $ExePath $Arguments"

        $params = @{
            FilePath = $ExePath
            ArgumentList = $Arguments
            WorkingDirectory = (Split-Path $ExePath)
            PassThru = $true
        }
        if ($Credential) { $params.Credential = $Credential }

        $process = Start-Process @params

        if ($process.WaitForExit($TimeoutSeconds * 1000)) {
            # Process ended, check exit code
            if ($process.ExitCode -eq 0) {
                Write-Host "Installer finished successfully on attempt $attempt"
                return $true
            } else {
                Write-Warning "Installer exited with code $($process.ExitCode) on attempt $attempt"
            }
        } else {
            Write-Warning "Installer timed out after $TimeoutSeconds seconds on attempt $attempt"
            try { Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue } catch {}
        }

        if ($attempt -lt $MaxRetries) {
            Write-Host "Retrying..."
            Start-Sleep -Seconds 5
        }
    }

    return $false
}
function Get-InstalledRhinoVersion {
    $key = Get-ItemProperty -Path "HKLM:\SOFTWARE\McNeel\Rhinoceros\8.0\Install" -Name "Version" -ErrorAction SilentlyContinue
    if (-not $key) { return $null }
    return [Version]$key.Version
}
# The download link redirects to the installer, whose file name ends in its version: rhino_en-us_8.24.25281.15001.exe.
function Resolve-RhinoInstaller {
    if ($PSVersionTable.PSVersion.Major -gt 5) {
        $response = Invoke-WebRequest -Method Get -MaximumRedirection 0 -Uri $rhinoDownloadUrl -ErrorAction Ignore -SkipHttpErrorCheck
    } else {
        $response = Invoke-WebRequest -Method Get -MaximumRedirection 0 -Uri $rhinoDownloadUrl -ErrorAction Ignore -UseBasicParsing
    }
    $location = $null
    if ($response) { $location = @($response.Headers.Location)[0] }
    if (-not $location) { throw "The Rhino download link didn't lead to an installer." }
    $fileName = [System.IO.Path]::GetFileName(([uri]$location).AbsolutePath)
    $version = $null
    if (-not [Version]::TryParse([System.IO.Path]::GetFileNameWithoutExtension($fileName).Split('_')[-1], [ref]$version)) {
        throw "Couldn't read a version from the installer's name, $fileName."
    }
    return [pscustomobject]@{ Url = $location; FileName = $fileName; Version = $version }
}
#EndRegion funcs

Write-Step 'Set environment variables'
SetEnvVar 'RHINO_TOKEN' $RhinoToken -secret
if ($PSBoundParameters.ContainsKey('ApiKey')) {
    SetEnvVar 'RHINO_COMPUTE_KEY' $ApiKey -secret
}
SetEnvVar 'RHINO_COMPUTE_URLS' 'http://+:80'

# Download and install Rhino
Write-Step 'Download latest Rhino 8'
$rhinoDownloadUrl = "https://www.rhino3d.com/www-api/download/direct/?slug=rhino-for-windows/8/latest/?email=$EmailAddress"
$package = Resolve-RhinoInstaller
$installedVersion = Get-InstalledRhinoVersion
# A second run (after fixing whatever stopped the first) doesn't install the same Rhino again.
if ($installedVersion -and $installedVersion -ge $package.Version) {
    Write-Host "Rhino $installedVersion is already installed; the latest is $($package.Version)."
    return
}
$setupFullPath = Join-Path -Path $tmpFullPath -ChildPath $package.FileName
Download $package.Url $setupFullPath

# Set firewall rule to allow installation
New-NetFirewallRule -DisplayName "Rhino 8 Installer" -Direction Inbound -Program $setupFullPath -Action Allow

Write-Step 'Installing Rhino'
# Automated install (https://wiki.mcneel.com/rhino/installingrhino/8)
$success = Install-ProcessWithTimeout -ExePath $setupFullPath -Arguments @('-passive','-norestart') -TimeoutSeconds 600 -MaxRetries 2

if (-not $success) {
    throw "Rhino didn't install; the installer's exit codes are above."
}
$installedVersion = Get-InstalledRhinoVersion
Write-Host "Successfully installed Rhino $installedVersion"
