# Updates a Rhino.Compute server set up by bootstrap.ps1: Rhino, the compute build and its ASP.NET Core hosting bundle,
# and RhinoComputeUser's Hops. It asks which to update, unless a switch answers for it:
#   .\update.ps1 [-All] [-SkipRhino] [-SkipCompute] [-SkipHops] [-Force] [-RhinoVersion 7|8|9] [-EmailAddress <email>]
# A scheduled task should pass -All (or the -Skip switches it wants). IIS is stopped once, only when something is
# updated, and started again whatever happens.
# Exit code: 0 updated or already current, 1 something failed and was left as it was, 2 something needs attention
# (a restart, or a failure that couldn't be fully undone).
#Requires -RunAsAdministrator

param (
    # Update everything without asking.
    [switch] $All,
    [switch] $SkipRhino,
    [switch] $SkipCompute,
    [switch] $SkipHops,
    # Reinstall the compute build and Hops even when they're current.
    [switch] $Force,
    # Which Rhino this server runs; bootstrap.ps1 recorded it, so it's only needed on servers set up before that.
    [ValidateSet("7", "8", "9")][string] $RhinoVersion,
    # Rhino's download link asks for one; the bootstrap's is used when this isn't passed.
    [string] $EmailAddress
)
Set-ExecutionPolicy Bypass -Scope Process -Force

# Replaced with the commit when the script is published.
$scriptVersion = "dev"

# What differs between Rhino versions; the same table as bootstrap.ps1's.
$rhinoVersions = @{
    "7" = @{ DownloadSlugs = @("rhino-for-windows/7/latest"); BuildBranch = "7.x"; DotnetChannel = "5.0"; Status = "out of support, no security fixes"
             Warning = "Rhino 7 is out of support: it gets no more updates, and its Rhino.Compute runs on .NET 5, which gets no security fixes." }
    "8" = @{ DownloadSlugs = @("rhino-for-windows/8/latest"); BuildBranch = "8.x"; DotnetChannel = "10.0" }
    # The release link answers once Rhino 9 is released; until then the beta's does.
    "9" = @{ DownloadSlugs = @("rhino-for-windows/9/latest", "rhino-for-windows/9/beta"); BuildBranch = "9.x"; DotnetChannel = "10.0" }
}

# The bootstrap's folder. Earlier bootstraps named it differently.
$installFolders = @("C:\Rhino-Compute-Installation", "C:\Rhino_Compute_Installation", "C:\Rhino Compute Installation")
$physicalPathRoot = "C:\inetpub\wwwroot\aspnet_client\system_web\4_0_30319"
$rhinoComputePath = "$physicalPathRoot\rhino.compute"
$computeGeometryPath = "$physicalPathRoot\compute.geometry"
$backupDir = "$physicalPathRoot\rhino.compute-backup"
$olderBackupDir = "$physicalPathRoot\rhino.compute-backup-older"
$stagingDir = "$physicalPathRoot\rhino.compute-staging"
$components = @("rhino.compute", "compute.geometry")
$appPoolName = "RhinoComputeAppPool"
$websiteName = "Rhino.Compute"
# Nested Hops calls are made by the Hops installed for the user compute runs as, not by the client's Hops.
$computeUserName = "RhinoComputeUser"
$hopsVersionsUrl = "https://yak.rhino3d.com/versions/hops"
$hopsBackupName = "hops-backup"
$artifactsUrl = "https://api.github.com/repos/mcneel/compute.rhino3d/actions/artifacts"
$installerTimeoutSeconds = 1200
# Each part's outcome: Code 0 updated or current, 1 failed and left as it was, 2 needs attention.
$results = [ordered]@{}

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
function Set-Result {
    param (
        [Parameter(Mandatory=$true)][string] $part,
        [Parameter(Mandatory=$true)][int] $code,
        [Parameter(Mandatory=$true)][string] $note
    )
    $script:results[$part] = [pscustomobject]@{ Code = $code; Note = $note }
    if ($code -ne 0) { Write-Host $note -ForegroundColor $(if ($code -eq 1) { "Red" } else { "Yellow" }) }
}
# Through cmd, so the command's output (stderr too) reaches the log without stopping the script.
function Invoke-Cmd {
    param ([Parameter(Mandatory=$true)][string] $commandLine)
    $output = cmd /c "$commandLine 2>&1"
    foreach ($line in $output) { Write-Host "    $line" }
    return $LASTEXITCODE
}
# Scheduled tasks and -NonInteractive sessions can't answer questions, so they update everything.
function Test-Interactive {
    if (-not [Environment]::UserInteractive) { return $false }
    return -not ([Environment]::GetCommandLineArgs() | Where-Object { $_ -match '^-noni' })
}
function Read-YesNo {
    param ([Parameter(Mandatory=$true)][string] $prompt)
    while ($true) {
        $answer = (Read-Host "$prompt [Y/n]").Trim()
        if (-not $answer -or $answer -match '^y(es)?$') { return $true }
        if ($answer -match '^no?$') { return $false }
        Write-Host "Please answer y or n."
    }
}
function Save-AdminOnlyFile {
    param (
        [Parameter(Mandatory=$true)][string] $path,
        [Parameter(Mandatory=$true)][string[]] $lines
    )
    New-Item -ItemType File -Path $path -Force | Out-Null
    icacls $path /inheritance:r /grant:r "*S-1-5-32-544:(F)" "*S-1-5-18:(F)" | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Couldn't restrict $path to Administrators (icacls exit code $LASTEXITCODE)." }
    Set-Content -Path $path -Value $lines
}
# Process.Start rather than Start-Process -PassThru, whose ExitCode Windows PowerShell can leave empty.
function Invoke-Installer {
    param (
        [Parameter(Mandatory=$true)][string] $path,
        [Parameter(Mandatory=$true)][string[]] $arguments
    )
    $busyWaits = 0
    while ($true) {
        Write-Host "Running $(Split-Path $path -Leaf) $arguments"
        $startInfo = New-Object System.Diagnostics.ProcessStartInfo($path, ($arguments -join ' '))
        $startInfo.WorkingDirectory = Split-Path $path
        $process = [System.Diagnostics.Process]::Start($startInfo)
        # Stopping an installer that's still working would leave Windows Installer busy.
        if (-not $process.WaitForExit($installerTimeoutSeconds * 1000)) {
            throw "$(Split-Path $path -Leaf) was still running after $($installerTimeoutSeconds / 60) minutes. Let it finish, then run the update again."
        }
        $code = $process.ExitCode
        Write-Host "Installer exit code: $code"
        # Another installation is running (often Windows Update): wait for it.
        if ($code -ne 1618 -or $busyWaits -ge 10) { return $code }
        $busyWaits++
        Write-Host "Another installation is in progress; trying again in a minute"
        Start-Sleep -Seconds 60
    }
}
# Where a download link redirects, or $null when it doesn't; a link that isn't live yet answers 404 rather than throwing.
function Get-RedirectLocation {
    param ([Parameter(Mandatory=$true)][string] $url)
    Add-Type -AssemblyName System.Net.Http
    $handler = New-Object System.Net.Http.HttpClientHandler
    $handler.AllowAutoRedirect = $false
    $client = New-Object System.Net.Http.HttpClient($handler)
    try {
        $response = $client.GetAsync($url).GetAwaiter().GetResult()
        if ($response.Headers.Location) { return (New-Object System.Uri((New-Object System.Uri($url)), $response.Headers.Location)).AbsoluteUri }
        return $null
    }
    finally {
        $client.Dispose()
    }
}

function Get-InstalledRhinoVersion {
    param ([Parameter(Mandatory=$true)][string] $major)
    $key = Get-ItemProperty -Path "HKLM:\SOFTWARE\McNeel\Rhinoceros\$major.0\Install" -Name "Version" -ErrorAction SilentlyContinue
    $version = $null
    if ($key -and [System.Version]::TryParse($key.Version, [ref]$version)) { return $version }
    return $null
}
# For servers set up before bootstrap.json: the one Rhino 7, 8 or 9 that's installed.
function Find-RhinoVersion {
    $installed = @($rhinoVersions.Keys | Sort-Object | Where-Object { Get-InstalledRhinoVersion $_ })
    if ($installed.Count -eq 1) { return $installed[0] }
    if ($installed.Count -eq 0) { throw "Rhino 7, 8 or 9 isn't installed. Please run the bootstrap script first!" }
    if (-not (Test-Interactive)) { throw "Rhino $($installed -join ' and ') are installed; pass -RhinoVersion to say which one Rhino.Compute runs." }
    while ($true) {
        $value = (Read-Host "Rhino $($installed -join ' and ') are installed. Which one does Rhino.Compute run?").Trim()
        if ($value -in $installed) { return $value }
    }
}
# The installer's file name ends in its version: rhino_en-us_8.36.26290.12001.exe, rhino_9.0.26286.06303.exe.
function Resolve-RhinoInstaller {
    foreach ($slug in $rhinoVersions[$RhinoVersion].DownloadSlugs) {
        $location = Get-RedirectLocation ("https://www.rhino3d.com/www-api/download/direct/?slug=$slug/?email=" + [uri]::EscapeDataString($EmailAddress))
        if (-not $location) {
            Write-Host "No installer at $slug yet"
            continue
        }
        Write-Host "Download link: $slug"
        $fileName = [System.IO.Path]::GetFileName(([uri]$location).AbsolutePath)
        $version = $null
        if (-not [Version]::TryParse([System.IO.Path]::GetFileNameWithoutExtension($fileName).Split('_')[-1], [ref]$version)) {
            throw "Couldn't read a version from the installer's name, $fileName."
        }
        return [pscustomobject]@{ Url = $location; FileName = $fileName; Version = $version }
    }
    throw "None of the Rhino $RhinoVersion download links ($($rhinoVersions[$RhinoVersion].DownloadSlugs -join ', ')) led to an installer."
}
function Assert-McNeelSignature {
    param ([Parameter(Mandatory=$true)][string] $path)
    $signature = Get-AuthenticodeSignature -FilePath $path
    if ($signature.Status -ne "Valid" -or $signature.SignerCertificate.Subject -notmatch 'O="?ROBERT MCNEEL & ASSOCIATES') {
        Remove-Item $path -Force -ErrorAction SilentlyContinue
        $signer = if ($signature.SignerCertificate) { $signature.SignerCertificate.GetNameInfo("SimpleName", $false) } else { "nobody" }
        throw "The downloaded $(Split-Path $path -Leaf) isn't signed by Robert McNeel & Associates (signature $($signature.Status), signed by $signer); not running it."
    }
}

# The newest hosting bundle of a .NET version, from Microsoft's release list.
function Find-HostingBundle {
    param ([Parameter(Mandatory=$true)][string] $channel)
    $releases = Invoke-RestMethod -Uri "https://builds.dotnet.microsoft.com/dotnet/release-metadata/$channel/releases.json"
    $release = $releases.releases | Where-Object { $_.'release-version' -eq $releases.'latest-release' } | Select-Object -First 1
    $file = $release.'aspnetcore-runtime'.files | Where-Object { $_.name -eq 'dotnet-hosting-win.exe' } | Select-Object -First 1
    if (-not $file) { throw "The .NET $channel release list has no hosting bundle." }
    return [pscustomobject]@{ Version = [System.Version]$release.'aspnetcore-runtime'.version; Url = $file.url; Hash = $file.hash; SupportPhase = $releases.'support-phase'; EndOfSupport = $releases.'eol-date' }
}
# The newest ASP.NET Core runtime of a .NET version, or $null.
function Get-InstalledAspNetCore {
    param ([Parameter(Mandatory=$true)][string] $channel)
    $newest = $null
    foreach ($folder in @(Get-ChildItem "$env:ProgramFiles\dotnet\shared\Microsoft.AspNetCore.App" -Directory -ErrorAction SilentlyContinue)) {
        $version = $null
        if (-not [System.Version]::TryParse($folder.Name, [ref]$version)) { continue }
        if ("$($version.Major).$($version.Minor)" -ne $channel) { continue }
        if (-not $newest -or $version -gt $newest) { $newest = $version }
    }
    return $newest
}
# The IIS module the hosting bundle adds; an ASP.NET Core runtime alone (as Rhino installs) doesn't have it.
function Test-AspNetCoreModule {
    return (Test-Path "$env:ProgramFiles\IIS\Asp.Net Core Module\V2\aspnetcorev2.dll")
}

# Newest first, all branches; skip other branches' builds, which target a different Rhino, and expired ones.
function Find-LatestArtifact {
    param ([Parameter(Mandatory=$true)][string] $branch)
    $response = Invoke-RestMethod -Method Get -Uri "$($artifactsUrl)?name=rhino.compute&per_page=100"
    return $response.artifacts | Where-Object { -not $_.expired -and $_.workflow_run.head_branch -eq $branch } | Select-Object -First 1
}
# The commit the installed rhino.compute was built from, from its product version (1.0.0+<commit>).
function Get-InstalledComputeCommit {
    $dll = "$rhinoComputePath\rhino.compute.dll"
    if (-not (Test-Path $dll)) { return $null }
    $productVersion = (Get-Item $dll).VersionInfo.ProductVersion
    if ($productVersion -match '\+([0-9a-f]{7,40})') { return $Matches[1].Substring(0, 7) }
    return $productVersion
}

# Packages name the oldest Rhino they load in: rh8_0 is any Rhino 8, rh8_36 needs 8.36 or later.
function Test-HopsPackageFits {
    param (
        [Parameter(Mandatory=$true)][string] $rhinoLabel,
        [Parameter(Mandatory=$true)][System.Version] $rhinoVersion
    )
    if ($rhinoLabel -notmatch '^rh(\d+)_(\d+)$') { return $false }
    return ([int]$Matches[1] -eq $rhinoVersion.Major) -and ([int]$Matches[2] -le $rhinoVersion.Minor)
}
# The newest release of Hops this Rhino can load, from the package server, or $null.
function Find-LatestHops {
    param ([Parameter(Mandatory=$true)][System.Version] $rhinoVersion)
    $latest = $null
    foreach ($release in (Invoke-RestMethod -Method Get -Uri $hopsVersionsUrl)) {
        if ($release.prerelease) { continue }
        $version = $null
        if (-not [System.Version]::TryParse($release.version, [ref]$version)) { continue }
        $package = $release.distributions | Where-Object { $_.platform -in "any", "win" -and (Test-HopsPackageFits $_.rhino_version $rhinoVersion) } | Select-Object -First 1
        if (-not $package) { continue }
        if (-not $latest -or $version -gt $latest.Version) {
            $latest = [pscustomobject]@{ Version = $version; Name = $release.version; Url = $package.url; RhinoLabel = $package.rhino_version }
        }
    }
    return $latest
}
# The user the app pool runs as, so the one whose Hops compute.geometry loads.
function Get-ComputeUser {
    try {
        Import-Module WebAdministration -ErrorAction Stop
        $name = (Get-ItemProperty "IIS:\AppPools\$appPoolName" -Name processModel).userName
        if ($name) { return $name }
    }
    catch { }
    return $computeUserName
}
# Hops's folder in the compute user's packages, or $null when the user has no Windows profile yet.
function Get-HopsPackagePath {
    $user = Get-ComputeUser
    try {
        $sid = (New-Object System.Security.Principal.NTAccount($user)).Translate([System.Security.Principal.SecurityIdentifier]).Value
        $profilePath = (Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\$sid" -ErrorAction Stop).ProfileImagePath
    }
    catch {
        return $null
    }
    return "$profilePath\AppData\Roaming\McNeel\Rhinoceros\packages\$RhinoVersion.0\Hops"
}
# The version the package manager installed (named in manifest.txt), or $null when Hops isn't installed.
function Get-InstalledHops {
    param ([Parameter(Mandatory=$true)][string] $packagePath)
    $manifest = "$packagePath\manifest.txt"
    if (-not (Test-Path $manifest)) { return $null }
    $name = ([System.IO.File]::ReadAllText($manifest)).Trim()
    if (-not $name -or -not (Test-Path "$packagePath\$name\Hops.gha")) { return $null }
    $version = $null
    [void][System.Version]::TryParse($name, [ref]$version)
    return [pscustomobject]@{ Version = $version; Name = $name }
}
# Downloads and unpacks a Hops release next to the installed one, so it takes the package folder's permissions.
function Get-HopsStaged {
    param (
        [Parameter(Mandatory=$true)] $latest,
        [Parameter(Mandatory=$true)][string] $packagePath
    )
    $staged = "$packagePath\$($latest.Name).staging"
    if (Test-Path $staged) { Remove-Item -Recurse -Force $staged }
    New-Item -ItemType Directory -Path $staged | Out-Null
    # Expand-Archive only takes .zip files; a .yak is a zip.
    $zip = "$staged.zip"
    try {
        Download $latest.Url $zip
        Expand-Archive $zip -DestinationPath $staged
        if (-not (Test-Path "$staged\Hops.gha")) { throw "The Hops $($latest.Name) package has no Hops.gha." }
        $label = $null
        foreach ($line in @(Get-Content "$staged\manifest.yml" -ErrorAction SilentlyContinue)) {
            if ($line -match '^version:\s*(\S+)') { $label = $Matches[1]; break }
        }
        if ($label -ne $latest.Name) { throw "The Hops $($latest.Name) package is labelled $(if ($label) { $label } else { 'with no version' })." }
    }
    catch {
        Remove-Item -Recurse -Force $staged -ErrorAction SilentlyContinue
        throw
    }
    finally {
        Remove-Item $zip -Force -ErrorAction SilentlyContinue
    }
    return $staged
}

function Show-Installed {
    $channel = $rhinoVersions[$RhinoVersion].DotnetChannel
    $rhino = Get-InstalledRhinoVersion $RhinoVersion
    $commit = Get-InstalledComputeCommit
    $aspNetCore = Get-InstalledAspNetCore $channel
    $hopsPath = Get-HopsPackagePath
    $hops = if ($hopsPath) { Get-InstalledHops $hopsPath } else { $null }
    Write-Host "Installed now:"
    Write-Host "  Rhino            $(if ($rhino) { $rhino } else { "no Rhino $RhinoVersion" })"
    Write-Host "  Rhino.Compute    $(if ($commit) { "$($rhinoVersions[$RhinoVersion].BuildBranch) build $commit" } else { 'not installed' })"
    Write-Host "  ASP.NET Core     $(if ($aspNetCore) { $aspNetCore } else { "no $channel runtime" })$(if (-not (Test-AspNetCoreModule)) { ', no IIS module' })"
    Write-Host "  Hops             $(if ($hops) { $hops.Name } else { 'not installed' })"
    Write-Host
}

function Get-ComputeProcesses {
    Get-Process -Name "compute.geometry", "rhino.compute" -ErrorAction SilentlyContinue | Where-Object {
        $path = $null
        try { $path = $_.Path } catch { }
        (-not $path) -or $path.StartsWith($physicalPathRoot, [System.StringComparison]::OrdinalIgnoreCase)
    }
}
# Hops before 0.17.1 starts its own rhino.compute from its package folder for nested calls; that one ignores IIS.
function Stop-HopsProcesses {
    param ([Parameter(Mandatory=$true)][string] $packagePath)
    Get-Process -Name "compute.geometry", "rhino.compute" -ErrorAction SilentlyContinue | ForEach-Object {
        $path = $null
        try { $path = $_.Path } catch { }
        if ($path -and $path.StartsWith($packagePath, [System.StringComparison]::OrdinalIgnoreCase)) {
            Write-Host "Stopping $($_.Name) ($($_.Id)), started from the Hops package"
            Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
        }
    }
}
# Children exit once they notice IIS has stopped; any still running after a minute are stopped.
function Wait-ComputeExit {
    $started = Get-Date
    $deadline = $started.AddSeconds(60)
    $computeCount = @(Get-ComputeProcesses).Count
    $workerCount = @(Get-Process -Name "w3wp" -ErrorAction SilentlyContinue).Count
    Write-Host "Waiting for $computeCount rhino.compute/compute.geometry and $workerCount w3wp processes to exit"
    while ((Get-Date) -lt $deadline) {
        $busy = @(Get-ComputeProcesses).Count + @(Get-Process -Name "w3wp" -ErrorAction SilentlyContinue).Count
        if ($busy -eq 0) {
            Write-Host ("All exited after {0:N0} s" -f ((Get-Date) - $started).TotalSeconds)
            return $true
        }
        Start-Sleep -Seconds 1
    }
    $remaining = @(Get-ComputeProcesses)
    foreach ($process in $remaining) {
        Write-Host "Stopping $($process.Name) ($($process.Id))"
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    }
    if ($remaining.Count -gt 0) { Start-Sleep -Seconds 2 }
    return (@(Get-ComputeProcesses).Count -eq 0)
}
# One rename: all of the folder moves or none of it. Move-Item can move part of a folder, then fail.
function Move-Folder {
    param (
        [Parameter(Mandatory=$true)][string] $from,
        [Parameter(Mandatory=$true)][string] $to
    )
    try {
        [System.IO.Directory]::Move($from, $to)
    }
    catch {
        throw "Could not move $from to $to. The folder or a file in it may be open in another program, such as a File Explorer window. ($($_.Exception.InnerException.Message))"
    }
}
# Puts each folder that was moved back where it was. Returns 1 when everything is back, else 2.
function Restore-Backup {
    param ([Parameter(Mandatory=$true)][hashtable] $moved)
    Write-Step "Restoring the previous version"
    $restored = $true
    foreach ($component in $components) {
        if (-not $moved.ContainsKey($component)) { continue }
        try {
            if (Test-Path "$physicalPathRoot\$component") {
                Move-Folder "$physicalPathRoot\$component" "$stagingDir\$component-failed"
            }
            Move-Folder "$backupDir\$component" "$physicalPathRoot\$component"
            Write-Host "Restored $component"
        }
        catch {
            Write-Host "Could not restore ${component}: $($_.Exception.Message)" -ForegroundColor Red
            $restored = $false
        }
    }
    if ($restored) { return 1 }
    return 2
}
# A failed update puts back the backup it set aside, so it costs no backup.
function Restore-OlderBackup {
    try {
        if ((Test-Path $backupDir) -and -not (Get-ChildItem $backupDir -Force | Select-Object -First 1)) {
            Remove-Item -Force $backupDir
        }
        Move-Folder $olderBackupDir $backupDir
        Write-Host "Put the previous backup back in $backupDir"
    }
    catch {
        Write-Host "The previous backup is still in ${olderBackupDir}: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

# Downloads what each chosen part needs while Rhino.Compute keeps running; returns what's to be installed.
function Get-Updates {
    $branch = $rhinoVersions[$RhinoVersion].BuildBranch
    $channel = $rhinoVersions[$RhinoVersion].DotnetChannel
    $pending = [pscustomobject]@{ Rhino = $null; Bundle = $null; Compute = $null; Hops = $null }

    if (-not $updateRhino) { Set-Result "Rhino" 0 "Rhino was not checked." }
    else {
        Write-Step "Checking for a newer Rhino $RhinoVersion"
        try {
            $before = Get-InstalledRhinoVersion $RhinoVersion
            if (-not $before) { throw "Rhino $RhinoVersion isn't installed. Please run the bootstrap script first!" }
            if (-not $EmailAddress) { throw "Rhino's download link needs an email address; pass -EmailAddress." }
            $package = Resolve-RhinoInstaller
            Write-Host "Installed: Rhino $before. Latest: Rhino $($package.Version)."
            if ($before -ge $package.Version) {
                Set-Result "Rhino" 0 "Rhino $before is already the latest version."
            }
            else {
                $installer = Join-Path $tempPath $package.FileName
                Write-Host "Downloading $($package.Url)"
                Download $package.Url $installer
                Assert-McNeelSignature $installer
                $pending.Rhino = [pscustomobject]@{ Before = $before; Package = $package; Installer = $installer }
            }
        }
        catch {
            Set-Result "Rhino" 1 "Rhino update FAILED: $($_.Exception.Message) Nothing was changed."
        }
    }

    if (-not $updateCompute) {
        Set-Result "Hosting bundle" 0 "The hosting bundle was not checked."
        Set-Result "Rhino.Compute" 0 "Rhino.Compute was not checked."
    }
    else {
        Write-Step "Checking for a newer ASP.NET Core $channel Hosting Bundle"
        try {
            $latest = Find-HostingBundle $channel
            if ($latest.SupportPhase -eq "eol") {
                Write-Warning "Rhino $RhinoVersion's Rhino.Compute runs on .NET $channel, which Microsoft stopped supporting on $($latest.EndOfSupport); it gets no more security fixes."
            }
            $before = Get-InstalledAspNetCore $channel
            $module = Test-AspNetCoreModule
            Write-Host "Installed: ASP.NET Core $(if ($before) { $before } else { 'none' })$(if (-not $module) { ' without the IIS module' }). Latest: $($latest.Version)."
            if ($before -and $module -and $before -ge $latest.Version) {
                Set-Result "Hosting bundle" 0 "The ASP.NET Core $before hosting bundle is already the latest."
            }
            else {
                $installer = Join-Path $tempPath "dotnet-hosting-$($latest.Version)-win.exe"
                Write-Host "Downloading $($latest.Url)"
                Download $latest.Url $installer
                if ((Get-FileHash -Path $installer -Algorithm SHA512).Hash -ne $latest.Hash) {
                    Remove-Item $installer -Force -ErrorAction SilentlyContinue
                    throw "The downloaded hosting bundle doesn't match Microsoft's checksum."
                }
                $pending.Bundle = [pscustomobject]@{ Before = $before; Latest = $latest; Installer = $installer }
            }
        }
        catch {
            Set-Result "Hosting bundle" 1 "Hosting bundle update FAILED: $($_.Exception.Message) Nothing was changed."
        }

        Write-Step "Checking for a newer $branch build of Rhino.Compute"
        try {
            if (-not (Test-Path "$rhinoComputePath\rhino.compute.exe") -or -not (Test-Path "$computeGeometryPath\compute.geometry.exe")) {
                throw "rhino.compute or compute.geometry isn't installed in $physicalPathRoot. Please run the bootstrap script first!"
            }
            $artifact = Find-LatestArtifact $branch
            if (-not $artifact) { throw "There's no current $branch build of Rhino.Compute to download (GitHub keeps builds for 90 days)." }
            $commit = $artifact.workflow_run.head_sha.Substring(0, 7)
            Write-Host "Build $($artifact.id), commit $commit, built $($artifact.created_at)"

            if (Test-Path $stagingDir) { Remove-Item -Recurse -Force $stagingDir }
            New-Item -ItemType Directory -Path $stagingDir | Out-Null
            $zip = "$stagingDir\compute.zip"
            Download "https://nightly.link/mcneel/compute.rhino3d/actions/artifacts/$($artifact.id).zip" $zip
            # nightly.link passes on GitHub's artifact; check it's the file GitHub lists (older artifacts have no digest).
            if ($artifact.digest -and ("sha256:" + (Get-FileHash $zip -Algorithm SHA256).Hash) -ne $artifact.digest) {
                throw "The downloaded build doesn't match GitHub's checksum for build $($artifact.id)."
            }
            Expand-Archive $zip -DestinationPath $stagingDir
            Remove-Item $zip
            # Rhino 7's compute.geometry is a .NET Framework program, with no .dll beside its .exe.
            $stagedHashes = @{}
            foreach ($file in @("rhino.compute\rhino.compute.exe", "compute.geometry\compute.geometry.exe", "rhino.compute\rhino.compute.dll", "compute.geometry\compute.geometry.dll")) {
                if (Test-Path "$stagingDir\$file") { $stagedHashes[$file] = (Get-FileHash "$stagingDir\$file").Hash }
                elseif ($file -like "*.exe") { throw "The download has no $file." }
            }
            # Compared by content, so a server with one folder on an older build still updates.
            $installedMatches = $true
            foreach ($file in $stagedHashes.Keys) {
                if (-not (Test-Path "$physicalPathRoot\$file") -or (Get-FileHash "$physicalPathRoot\$file").Hash -ne $stagedHashes[$file]) { $installedMatches = $false }
            }
            if ($installedMatches -and -not $Force) {
                Remove-Item -Recurse -Force $stagingDir -ErrorAction SilentlyContinue
                Set-Result "Rhino.Compute" 0 "Rhino.Compute is already $branch build $commit. Run the update with -Force to reinstall it."
            }
            else {
                # The download carries the default web.config; the server's own settings (child count, timeouts) stay.
                $liveConfig = "$rhinoComputePath\web.config"
                $stagedConfig = "$stagingDir\rhino.compute\web.config"
                if (Test-Path $liveConfig) {
                    if ((Test-Path $stagedConfig) -and (Get-FileHash $liveConfig).Hash -ne (Get-FileHash $stagedConfig).Hash) {
                        Write-Host "Keeping this server's web.config. The new build's default web.config is:"
                        Get-Content $stagedConfig | ForEach-Object { Write-Host "    $_" }
                    }
                    Copy-Item $liveConfig $stagedConfig -Force
                }
                $pending.Compute = [pscustomobject]@{ Artifact = $artifact; Commit = $commit; Hashes = $stagedHashes }
            }
        }
        catch {
            Remove-Item -Recurse -Force $stagingDir -ErrorAction SilentlyContinue
            Set-Result "Rhino.Compute" 1 "Rhino.Compute update FAILED: $($_.Exception.Message) Nothing was changed."
        }
    }

    if (-not $updateHops) { Set-Result "Hops" 0 "Hops was not checked." }
    else {
        Write-Step "Checking for a newer Hops"
        try {
            $packagePath = Get-HopsPackagePath
            $installed = if ($packagePath) { Get-InstalledHops $packagePath } else { $null }
            # The bootstrap records whether the server should have Hops, so one it couldn't install yet is installed later.
            $wanted = $installed -or ($settings -and $settings.hops -eq $true)
            if (-not $packagePath) {
                Set-Result "Hops" 0 "$computeUserName has no Windows profile yet, so Hops was left alone."
            }
            elseif (-not $wanted) {
                Set-Result "Hops" 0 "Hops isn't installed in $packagePath, so it was left alone."
            }
            else {
                # The Rhino it'll run in once Rhino is updated.
                $rhino = if ($pending.Rhino) { $pending.Rhino.Package.Version } else { Get-InstalledRhinoVersion $RhinoVersion }
                if (-not $rhino) { throw "Rhino $RhinoVersion isn't installed." }
                Write-Host "Installed: $(if ($installed) { "Hops $($installed.Name)" } else { 'no Hops' }) in $packagePath, for Rhino $rhino"
                $latest = Find-LatestHops $rhino
                if (-not $latest) {
                    Set-Result "Hops" 0 "The package server has no Hops release for Rhino $rhino yet, so Hops was left alone."
                }
                elseif ($installed -and $installed.Version -and $installed.Version -ge $latest.Version -and -not $Force) {
                    Set-Result "Hops" 0 "Hops is already $($installed.Name), the latest release for Rhino $rhino."
                }
                else {
                    Write-Host "Downloading Hops $($latest.Name) from $($latest.Url)"
                    $staged = Get-HopsStaged $latest $packagePath
                    $pending.Hops = [pscustomobject]@{ PackagePath = $packagePath; Installed = $installed; Latest = $latest; Staged = $staged }
                }
            }
        }
        catch {
            Set-Result "Hops" 1 "Hops update FAILED: $($_.Exception.Message) Nothing was changed."
        }
    }
    return $pending
}

function Install-HostingBundle {
    param ([Parameter(Mandatory=$true)] $bundle)
    Write-Step "Installing the ASP.NET Core $($bundle.Latest.Version) Hosting Bundle"
    try {
        $code = Invoke-Installer $bundle.Installer @('/repair', '/quiet', '/norestart')
        $from = if ($bundle.Before) { "from $($bundle.Before) " } else { "" }
        if ($code -eq 0) { Set-Result "Hosting bundle" 0 "Hosting bundle updated $($from)to ASP.NET Core $($bundle.Latest.Version)." }
        elseif ($code -eq 3010 -or $code -eq 1641) { Set-Result "Hosting bundle" 2 "The ASP.NET Core $($bundle.Latest.Version) hosting bundle is installed, but Windows needs a restart to finish it." }
        else { Set-Result "Hosting bundle" 1 "Hosting bundle update FAILED: the installer's exit code was $code." }
    }
    catch {
        Set-Result "Hosting bundle" 2 "Hosting bundle update needs attention: $($_.Exception.Message)"
    }
}

function Install-Rhino {
    param ([Parameter(Mandatory=$true)] $rhino)
    Write-Step "Installing Rhino $($rhino.Package.Version)"
    $code = $null
    $failure = $null
    # Automated install (https://wiki.mcneel.com/rhino/installingrhino/8)
    try { $code = Invoke-Installer $rhino.Installer @('-passive', '-norestart') }
    catch { $failure = $_.Exception.Message }
    $after = Get-InstalledRhinoVersion $RhinoVersion
    if ($failure) { Set-Result "Rhino" 2 "Rhino update needs attention: $failure Rhino reads $after." }
    elseif ($code -eq 0 -and $after -eq $rhino.Package.Version) { Set-Result "Rhino" 0 "Rhino $($rhino.Before) is now Rhino $after." }
    elseif ($code -eq 3010 -or $code -eq 1641) { Set-Result "Rhino" 2 "Rhino $after is installed, but Windows needs a restart to finish the update." }
    elseif ($after -eq $rhino.Before) { Set-Result "Rhino" 1 "Rhino update FAILED (installer exit code $code): Rhino is still $($rhino.Before)." }
    else { Set-Result "Rhino" 2 "Rhino update needs attention: the installer's exit code was $code, and Rhino reads $after where $($rhino.Package.Version) was expected." }
}

# Either both folders become the new build or both stay as they were.
function Install-Compute {
    param ([Parameter(Mandatory=$true)] $compute)
    $branch = $rhinoVersions[$RhinoVersion].BuildBranch
    Write-Step "Replacing rhino.compute and compute.geometry with $branch build $($compute.Commit)"
    $moved = @{}
    $olderBackupSetAside = $false
    try {
        # A run stopped part way can leave both backups; keep the newer one that still holds a version.
        if (Test-Path $olderBackupDir) {
            if ((Test-Path $backupDir) -and (Get-ChildItem $backupDir -Force | Select-Object -First 1)) {
                Remove-Item -Recurse -Force $olderBackupDir
            }
            else {
                if (Test-Path $backupDir) { Remove-Item -Recurse -Force $backupDir }
                Move-Folder $olderBackupDir $backupDir
            }
        }
        if (Test-Path $backupDir) {
            Write-Host "Setting the previous backup aside in $olderBackupDir until the update succeeds"
            Move-Folder $backupDir $olderBackupDir
            $olderBackupSetAside = $true
        }
        New-Item -ItemType Directory -Path $backupDir | Out-Null

        foreach ($component in $components) {
            Write-Host "Moving $physicalPathRoot\$component to $backupDir\$component"
            Move-Folder "$physicalPathRoot\$component" "$backupDir\$component"
            $moved[$component] = "backup"
        }
        foreach ($component in $components) {
            Write-Host "Moving the new $component into place"
            Move-Folder "$stagingDir\$component" "$physicalPathRoot\$component"
            $moved[$component] = "live"
        }

        Write-Host "Granting $appPoolName access to the new folders"
        # Modify covers writing logs; the app pool has no need to change permissions or take ownership.
        foreach ($component in $components) {
            $code = Invoke-Cmd "icacls `"$physicalPathRoot\$component`" /grant `"IIS AppPool\${appPoolName}:(OI)(CI)M`" /t /c /q"
            if ($code -ne 0) { throw "icacls failed on $component (exit code $code)." }
        }

        foreach ($file in $compute.Hashes.Keys) {
            if ((Get-FileHash "$physicalPathRoot\$file").Hash -ne $compute.Hashes[$file]) { throw "$file is not the downloaded file." }
        }
        $productVersion = (Get-Item "$rhinoComputePath\rhino.compute.dll").VersionInfo.ProductVersion
        Write-Host "rhino.compute product version: $productVersion"
        if ($productVersion -notlike "*+$($compute.Artifact.workflow_run.head_sha)*") {
            Write-Warning "The product version doesn't name commit $($compute.Artifact.workflow_run.head_sha)."
        }
        if ($olderBackupSetAside) {
            Remove-Item -Recurse -Force $olderBackupDir -ErrorAction SilentlyContinue
            if (Test-Path $olderBackupDir) { Write-Warning "Couldn't delete $olderBackupDir; the next update will." }
        }
        Write-Host "The previous version is in $backupDir"
        Set-Result "Rhino.Compute" 0 "Rhino.Compute updated to $branch build $($compute.Commit) (build $($compute.Artifact.id))."
    }
    catch {
        Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
        $code = 1
        if ($moved.Count -gt 0) { $code = Restore-Backup $moved }
        if ($olderBackupSetAside -and $code -ne 2) { Restore-OlderBackup }
        if ($code -eq 2) { Set-Result "Rhino.Compute" 2 "Rhino.Compute update FAILED, and the previous rhino.compute and compute.geometry couldn't be fully restored. Their folders are in $backupDir." }
        elseif ($moved.Count -gt 0) { Set-Result "Rhino.Compute" 1 "Rhino.Compute update FAILED: $($_.Exception.Message) The previous version was restored." }
        else { Set-Result "Rhino.Compute" 1 "Rhino.Compute update FAILED: $($_.Exception.Message) Nothing was changed." }
    }
}

# Swaps the staged Hops in for the installed one (or installs it, when there's none); on failure puts the installed one back.
function Install-Hops {
    param ([Parameter(Mandatory=$true)] $hops)
    $packagePath = $hops.PackagePath
    $name = $hops.Latest.Name
    Write-Step $(if ($hops.Installed) { "Replacing Hops $($hops.Installed.Name) with $name" } else { "Installing Hops $name" })
    $manifest = "$packagePath\manifest.txt"
    $previousManifest = if (Test-Path $manifest) { [System.IO.File]::ReadAllText($manifest) } else { $null }
    $newBackup = "$installPath\$hopsBackupName-new"
    $moved = @()
    $placed = $false
    try {
        if (Test-Path $newBackup) { Remove-Item -Recurse -Force $newBackup }
        New-Item -ItemType Directory -Path $newBackup | Out-Null
        foreach ($folder in @(Get-ChildItem $packagePath -Directory | Where-Object { $_.Name -notlike "*.staging" })) {
            Write-Host "Moving $($folder.FullName) to $newBackup\$($folder.Name)"
            Move-Folder $folder.FullName "$newBackup\$($folder.Name)"
            $moved += $folder.Name
        }
        Move-Folder $hops.Staged "$packagePath\$name"
        $placed = $true
        [System.IO.File]::WriteAllText($manifest, "$name`n")
        $installed = Get-InstalledHops $packagePath
        if (-not $installed -or $installed.Name -ne $name) { throw "Hops $name is not installed after the swap." }
        if ($hops.Installed) {
            [System.IO.File]::WriteAllText("$newBackup\manifest.txt", $previousManifest)
            Remove-Item -Recurse -Force "$installPath\$hopsBackupName" -ErrorAction SilentlyContinue
            Move-Folder $newBackup "$installPath\$hopsBackupName"
            Write-Host "The previous Hops is in $installPath\$hopsBackupName"
        }
        else {
            Remove-Item -Recurse -Force $newBackup -ErrorAction SilentlyContinue
        }
        if (-not $hops.Installed) { Set-Result "Hops" 0 "Hops $name installed." }
        elseif ($hops.Installed.Name -eq $name) { Set-Result "Hops" 0 "Hops $name was reinstalled." }
        else { Set-Result "Hops" 0 "Hops updated from $($hops.Installed.Name) to $name." }
    }
    catch {
        Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
        Write-Step "Restoring the previous Hops"
        try {
            if ($placed) { Remove-Item -Recurse -Force "$packagePath\$name" }
            foreach ($folder in $moved) {
                Move-Folder "$newBackup\$folder" "$packagePath\$folder"
                Write-Host "Restored $packagePath\$folder"
            }
            if ($null -ne $previousManifest) { [System.IO.File]::WriteAllText($manifest, $previousManifest) }
            elseif (Test-Path $manifest) { Remove-Item $manifest -Force }
            Remove-Item -Recurse -Force $newBackup -ErrorAction SilentlyContinue
            Set-Result "Hops" 1 "Hops update FAILED: $($_.Exception.Message) $(if ($hops.Installed) { "Hops $($hops.Installed.Name) was restored." } else { 'Nothing was changed.' })"
        }
        catch {
            Write-Host "Could not restore Hops: $($_.Exception.Message)" -ForegroundColor Red
            Set-Result "Hops" 2 "Hops update FAILED, and the previous Hops couldn't be fully restored. Its folders are in $newBackup."
        }
    }
}

function Start-Compute {
    param ([string[]] $services = @())
    Write-Step "Starting the IIS services"
    try {
        Invoke-Cmd "net start w3svc" | Out-Null
        foreach ($service in $services) {
            if ($service -ne "W3SVC") { Invoke-Cmd "net start $service" | Out-Null }
        }
        Start-IISSite -Name $websiteName
    }
    catch {
        Write-Host "Could not start IIS: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Rhino.Compute stops once, for everything that's to be installed: the hosting bundle, Rhino, the compute build, then
# Hops (whose release depends on the Rhino). IIS is started again whatever happens.
function Update-Server {
    $pending = Get-Updates
    if ($pending.Rhino) {
        $running = @(Get-Process -Name "Rhino" -ErrorAction SilentlyContinue)
        if ($running.Count -gt 0) {
            Set-Result "Rhino" 1 "Rhino update FAILED: Rhino is open on this server (process $($running.Id -join ', ')). Close it and run the update again."
            Remove-Item $pending.Rhino.Installer -Force -ErrorAction SilentlyContinue
            $pending.Rhino = $null
        }
    }
    if (-not ($pending.Rhino -or $pending.Bundle -or $pending.Compute -or $pending.Hops)) { return }

    $iisStopped = $false
    $dependentServices = @()
    try {
        Write-Step "Stopping the IIS services"
        # Stopping WAS stops every service that depends on it; the ones running now are started again.
        $dependentServices = @((Get-Service -Name "WAS" -ErrorAction SilentlyContinue).DependentServices | Where-Object { $_.Status -eq "Running" } | ForEach-Object { $_.Name })
        Invoke-Cmd "net stop was /y" | Out-Null
        $iisStopped = $true
        if ($pending.Hops) { Stop-HopsProcesses $pending.Hops.PackagePath }
        if (-not (Wait-ComputeExit)) {
            $parts = [ordered]@{ Rhino = "Rhino"; Bundle = "Hosting bundle"; Compute = "Rhino.Compute"; Hops = "Hops" }
            foreach ($key in $parts.Keys) {
                if ($pending.$key) { Set-Result $parts[$key] 1 "$($parts[$key]) update FAILED: compute.geometry or rhino.compute is still running. Nothing was changed." }
            }
            return
        }

        if ($pending.Bundle) { Install-HostingBundle $pending.Bundle }
        if ($pending.Rhino) { Install-Rhino $pending.Rhino }
        if ($pending.Compute) { Install-Compute $pending.Compute }
        if ($pending.Hops) {
            $rhino = Get-InstalledRhinoVersion $RhinoVersion
            if ($rhino -and -not (Test-HopsPackageFits $pending.Hops.Latest.RhinoLabel $rhino)) {
                Set-Result "Hops" 1 "Hops $($pending.Hops.Latest.Name) needs a newer Rhino than $rhino, so Hops was left as it was."
            }
            else {
                Install-Hops $pending.Hops
            }
        }
    }
    finally {
        if ($iisStopped) { Start-Compute -services $dependentServices }
        if ($pending.Hops -and (Test-Path $pending.Hops.Staged)) { Remove-Item -Recurse -Force $pending.Hops.Staged -ErrorAction SilentlyContinue }
        if (Test-Path $stagingDir) { Remove-Item -Recurse -Force $stagingDir -ErrorAction SilentlyContinue }
        foreach ($installer in @($pending.Rhino, $pending.Bundle) | Where-Object { $_ } | ForEach-Object { $_.Installer }) {
            Remove-Item $installer -Force -ErrorAction SilentlyContinue
        }
    }
}
#EndRegion funcs

# Dot-sourcing the script loads the functions without updating anything, for testing.
if ($MyInvocation.InvocationName -eq '.') { return }

$installPath = $installFolders | Where-Object { Test-Path $_ -PathType Container } | Select-Object -First 1
if (-not $installPath) { $installPath = $installFolders[0] }
$tempPath = "$installPath\Temp"
$settingsFile = "$installPath\bootstrap.json"
$logFile = "$installPath\update_log.txt"
New-Item -ItemType Directory -Force -Path $tempPath | Out-Null
try { Stop-Transcript | Out-Null } catch { }
Start-Transcript -Path $logFile -Append | Out-Null
$ErrorActionPreference = "Stop"
# Windows PowerShell's progress bar slows Invoke-WebRequest and Expand-Archive down many times over.
$ProgressPreference = "SilentlyContinue"

Write-Host @"

  # # # # # # # # # # # # # # # # # # # # #
  #                                       #
  #       R H I N O   C O M P U T E       #
  #                                       #
  #      U P D A T E   S C R I P T        #
  #                                       #
  # # # # # # # # # # # # # # # # # # # # #

"@
$scriptHash = if ($PSCommandPath) { (Get-FileHash -Path $PSCommandPath -Algorithm SHA256).Hash.Substring(0, 12).ToLower() } else { "not run from a file" }
Write-Host "update.ps1 version $scriptVersion ($scriptHash)"
Write-Host "Update started $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
Write-Host "Log file: $logFile"
Write-Host

$failure = $null
try {
    $settings = $null
    if (Test-Path $settingsFile) {
        try { $settings = Get-Content $settingsFile -Raw | ConvertFrom-Json }
        catch { Write-Warning "Couldn't read ${settingsFile}: $($_.Exception.Message)" }
    }
    if (-not $RhinoVersion -and $settings -and $rhinoVersions.ContainsKey("$($settings.rhinoVersion)")) { $RhinoVersion = "$($settings.rhinoVersion)" }
    if (-not $RhinoVersion) { $RhinoVersion = Find-RhinoVersion }
    Write-Host "This server runs Rhino $RhinoVersion."
    if ($rhinoVersions[$RhinoVersion].Warning) { Write-Warning $rhinoVersions[$RhinoVersion].Warning }
    Show-Installed

    $updateRhino = -not $SkipRhino
    $updateCompute = -not $SkipCompute
    $updateHops = -not $SkipHops
    if (-not ($All -or $SkipRhino -or $SkipCompute -or $SkipHops) -and (Test-Interactive)) {
        $updateRhino = Read-YesNo "Update Rhino $RhinoVersion (recommended)?"
        $updateCompute = Read-YesNo "Update Rhino.Compute and its .NET hosting bundle (recommended)?"
        $updateHops = Read-YesNo "Update Hops (recommended)?"
    }
    $chosen = @(if ($updateRhino) { 'Rhino' }; if ($updateCompute) { 'Rhino.Compute' }; if ($updateHops) { 'Hops' })
    Write-Host "Updating: $(if ($chosen) { $chosen -join ', ' } else { 'nothing' })"
    if ($updateRhino -and -not $EmailAddress) {
        if ($settings -and $settings.emailAddress) { $EmailAddress = $settings.emailAddress }
        elseif (Test-Interactive) { do { $EmailAddress = (Read-Host "Email address (for Rhino's download link)").Trim() } while (-not $EmailAddress) }
    }
    # Servers set up before bootstrap.json get one, so the next update doesn't need to ask.
    if (-not $settings) {
        $recorded = [ordered]@{ rhinoVersion = [int]$RhinoVersion; emailAddress = $EmailAddress; written = (Get-Date).ToUniversalTime().ToString("o"); script = "update.ps1 $scriptVersion ($scriptHash)" }
        Save-AdminOnlyFile $settingsFile (($recorded | ConvertTo-Json) -split "`r?`n")
        Write-Host "Recorded Rhino $RhinoVersion in $settingsFile"
    }

    Update-Server
}
catch {
    $failure = $_
}
finally {
    Write-Host
    if ($failure) {
        Write-Host "Update FAILED: $($failure.Exception.Message)" -ForegroundColor Red
        Write-Host $failure.InvocationInfo.PositionMessage
    }
    foreach ($part in "Rhino", "Hosting bundle", "Rhino.Compute", "Hops") {
        if (-not $results.Contains($part)) { continue }
        Write-Host $results[$part].Note -ForegroundColor $(switch ($results[$part].Code) { 0 { "Green" } 1 { "Red" } default { "Yellow" } })
    }
    Write-Step "Update ended at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss'). Log file: $logFile"
    Stop-Transcript | Out-Null
}

$exitCode = if ($failure) { 1 } else { 0 }
foreach ($result in $results.Values) { $exitCode = [Math]::Max($exitCode, $result.Code) }
exit $exitCode
