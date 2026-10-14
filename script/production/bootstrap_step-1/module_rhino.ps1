# Setup/Install script for installing Rhino
#Requires -RunAsAdministrator

param (
    [Parameter(Mandatory=$true)][string] $EmailAddress,
    [Parameter(Mandatory=$true)][string] $ApiKey,
    [Parameter(Mandatory=$true)][string] $RhinoToken,
    [Parameter(Mandatory=$true)][string] $TempPath,
    [switch] $install = $false
)

#Region funcs
. "$PSScriptRoot\common.ps1"
function Get-InstalledRhinoVersion {
    $key = Get-ItemProperty -Path "HKLM:\SOFTWARE\McNeel\Rhinoceros\9.0\Install" -Name "Version" -ErrorAction SilentlyContinue
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
Write-Step 'Download latest Rhino 9'
$rhinoDownloadUrl = "https://www.rhino3d.com/www-api/download/direct/?slug=rhino-for-windows/9/wip/?email=$EmailAddress"
$package = Resolve-RhinoInstaller
$installedVersion = Get-InstalledRhinoVersion
# A second run (after fixing whatever stopped the first) doesn't install the same Rhino again.
if ($installedVersion -and $installedVersion -ge $package.Version) {
    Write-Host "Rhino $installedVersion is already installed; the latest is $($package.Version)."
    return
}
$setupFullPath = Join-Path -Path $TempPath -ChildPath $package.FileName
Download $package.Url $setupFullPath

# Only run an installer McNeel signed.
$signature = Get-AuthenticodeSignature -FilePath $setupFullPath
if ($signature.Status -ne "Valid" -or $signature.SignerCertificate.Subject -notmatch 'O="?ROBERT MCNEEL & ASSOCIATES') {
    Remove-Item $setupFullPath -Force -ErrorAction SilentlyContinue
    $signer = if ($signature.SignerCertificate) { $signature.SignerCertificate.GetNameInfo("SimpleName", $false) } else { "nobody" }
    throw "The downloaded $($package.FileName) isn't signed by Robert McNeel & Associates (signature $($signature.Status), signed by $signer); not running it."
}

# Set firewall rule to allow installation
New-NetFirewallRule -DisplayName "Rhino 9 Installer" -Direction Inbound -Program $setupFullPath -Action Allow

Write-Step 'Installing Rhino'
# Automated install (https://wiki.mcneel.com/rhino/installingrhino/9)
$success = Install-ProcessWithTimeout -ExePath $setupFullPath -Arguments @('-passive','-norestart') -TimeoutSeconds 1200 -MaxRetries 2

if (-not $success) {
    throw "Rhino didn't install; the installer's exit codes are above."
}
$installedVersion = Get-InstalledRhinoVersion
Write-Host "Successfully installed Rhino $installedVersion"
