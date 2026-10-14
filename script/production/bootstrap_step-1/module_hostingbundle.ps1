# Setup/Install script for installing .NET Core Hosting Bundle
#Requires -RunAsAdministrator

param ([Parameter(Mandatory=$true)][string] $TempPath)

#Region funcs
. "$PSScriptRoot\common.ps1"
# The newest hosting bundle of a .NET version, from Microsoft's release list, so new servers get its security fixes.
function Find-HostingBundle {
    param ([Parameter(Mandatory=$true)][string] $channel)
    $releases = Invoke-RestMethod -Uri "https://builds.dotnet.microsoft.com/dotnet/release-metadata/$channel/releases.json"
    $release = $releases.releases | Where-Object { $_.'release-version' -eq $releases.'latest-release' } | Select-Object -First 1
    $file = $release.'aspnetcore-runtime'.files | Where-Object { $_.name -eq 'dotnet-hosting-win.exe' } | Select-Object -First 1
    if (-not $file) { throw "The .NET $channel release list has no hosting bundle." }
    return [pscustomobject]@{ Version = $release.'aspnetcore-runtime'.version; Url = $file.url; Hash = $file.hash }
}
#EndRegion funcs

# compute.geometry, like Rhino 9, runs on .NET 10, and needs all of its runtimes from one .NET version.
$dotnetChannel = "10.0"

# Download and install .NET Hosting Bundle
Write-Step "Download ASP.NET Core $dotnetChannel Hosting Bundle"

$bundle = Find-HostingBundle $dotnetChannel
$hbInstallerURL = $bundle.Url
$hbInstallerFilename = "dotnet-hosting-$($bundle.Version)-win.exe"
$hbInstallerFilepath = Join-Path -Path $TempPath -ChildPath $hbInstallerFilename

Write-Host $hbInstallerURL
Download $hbInstallerURL $hbInstallerFilepath

if ((Get-FileHash -Path $hbInstallerFilepath -Algorithm SHA512).Hash -ne $bundle.Hash) {
    Remove-Item $hbInstallerFilepath -Force -ErrorAction SilentlyContinue
    throw "The downloaded $hbInstallerFilename doesn't match Microsoft's checksum; not installing it."
}

Write-Step "Installing ASP.NET Core $($bundle.Version) Hosting Bundle"

$success = Install-ProcessWithTimeout -ExePath $hbInstallerFilepath -Arguments @('/repair','/quiet','/norestart') -TimeoutSeconds 600 -MaxRetries 2

if (-not $success) {
    throw "The hosting bundle didn't install; the installer's exit codes are above."
}
Write-Output "$hbInstallerFilename successfully installed"
Write-Step 'Restarting IIS services'
net stop was /y
net start w3svc