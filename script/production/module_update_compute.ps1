# Download/Install compute
#Requires -RunAsAdministrator

param (
    # Reinstall even when the latest build and Hops are already installed.
    [switch] $Force = $false,
    # Leave the compute user's Hops as it is.
    [switch] $SkipHops = $false
)

$physicalPathRoot = "C:\inetpub\wwwroot\aspnet_client\system_web\4_0_30319"
$rhinoComputePath = "$physicalPathRoot\rhino.compute"
$computeGeometryPath = "$physicalPathRoot\compute.geometry"
$rhinoComputeExe = "$rhinoComputePath\rhino.compute.exe"
$computeGeometryExe = "$computeGeometryPath\compute.geometry.exe"
$appPoolName = "RhinoComputeAppPool"
$websiteName = "Rhino.Compute"
$backupDir = "$physicalPathRoot\rhino.compute-backup"
$olderBackupDir = "$physicalPathRoot\rhino.compute-backup-older"
$stagingDir = "$physicalPathRoot\rhino.compute-staging"
$installFolders = @("C:\Rhino-Compute-Installation", "C:\Rhino_Compute_Installation", "C:\Rhino Compute Installation")
$logFileName = "update_compute_log.txt"
$matchingBranch = "9.x"
$artifactName = "rhino.compute"
$components = @("rhino.compute", "compute.geometry")
# Nested Hops calls are made by the Hops installed for the user compute runs as, not by the client's Hops.
$computeUserName = "RhinoComputeUser"
$hopsVersionsUrl = "https://yak.rhino3d.com/versions/hops"
$hopsPackagesFolder = "9.0"
$rhinoRegistryKey = "HKLM:\SOFTWARE\McNeel\Rhinoceros\$hopsPackagesFolder\Install"
$hopsBackupName = "hops-backup"

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

# Through cmd, so the command's output (stderr too) reaches the log without stopping the script.
function Invoke-Cmd {
    param ([Parameter(Mandatory=$true)][string] $commandLine)
    $output = cmd /c "$commandLine 2>&1"
    foreach ($line in $output) { Write-Host "    $line" }
    return $LASTEXITCODE
}

# Newest first. Only builds of the matching branch: other branches' builds target a different Rhino.
function Find-LatestArtifact {
    $response = Invoke-RestMethod -Method Get -Uri "https://api.github.com/repos/mcneel/compute.rhino3d/actions/artifacts?name=$artifactName&per_page=100"
    foreach ($artifact in $response.artifacts) {
        if ($artifact.expired) { continue }
        if ($artifact.workflow_run.head_branch -ne $matchingBranch) { continue }
        return $artifact
    }
    return $null
}

# The installed Rhino's version, or this major version alone when the registry doesn't say.
function Get-RhinoVersion {
    $key = Get-ItemProperty -Path $rhinoRegistryKey -Name "Version" -ErrorAction SilentlyContinue
    $version = $null
    if ($key -and [System.Version]::TryParse($key.Version, [ref]$version)) { return $version }
    return [System.Version]$hopsPackagesFolder
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
            $latest = [pscustomobject]@{ Version = $version; Name = $release.version; Url = $package.url }
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

function Get-HopsPackagePath {
    $user = Get-ComputeUser
    $profilePath = "C:\Users\$(Split-Path $user -Leaf)"
    try {
        $sid = (New-Object System.Security.Principal.NTAccount($user)).Translate([System.Security.Principal.SecurityIdentifier]).Value
        $profilePath = (Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\$sid" -ErrorAction Stop).ProfileImagePath
    }
    catch { }
    return "$profilePath\AppData\Roaming\McNeel\Rhinoceros\packages\$hopsPackagesFolder\Hops"
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

# Swaps the staged Hops in for the installed one; on failure puts the installed one back. Returns 0, 1 or 2 like Restore-Backup.
function Install-Hops {
    param (
        [Parameter(Mandatory=$true)] $hops,
        [Parameter(Mandatory=$true)][string] $installPath
    )
    Write-Step "Replacing Hops $($hops.Installed.Name) with $($hops.Latest.Name)"
    $packagePath = $hops.PackagePath
    $manifest = "$packagePath\manifest.txt"
    $previousManifest = [System.IO.File]::ReadAllText($manifest)
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
        Move-Folder $hops.Staged "$packagePath\$($hops.Latest.Name)"
        $placed = $true
        [System.IO.File]::WriteAllText($manifest, "$($hops.Latest.Name)`n")
        $installed = Get-InstalledHops $packagePath
        if (-not $installed -or $installed.Name -ne $hops.Latest.Name) { throw "Hops $($hops.Latest.Name) is not installed after the swap." }
        [System.IO.File]::WriteAllText("$newBackup\manifest.txt", $previousManifest)
        Remove-Item -Recurse -Force "$installPath\$hopsBackupName" -ErrorAction SilentlyContinue
        Move-Folder $newBackup "$installPath\$hopsBackupName"
        Write-Host "The previous Hops is in $installPath\$hopsBackupName"
        return 0
    }
    catch {
        Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
        Write-Step "Restoring the previous Hops"
        $restored = $true
        try {
            if ($placed) { Remove-Item -Recurse -Force "$packagePath\$($hops.Latest.Name)" }
            foreach ($name in $moved) {
                Move-Folder "$newBackup\$name" "$packagePath\$name"
                Write-Host "Restored $packagePath\$name"
            }
            [System.IO.File]::WriteAllText($manifest, $previousManifest)
            Remove-Item -Recurse -Force $newBackup -ErrorAction SilentlyContinue
        }
        catch {
            Write-Host "Could not restore Hops: $($_.Exception.Message)" -ForegroundColor Red
            $restored = $false
        }
        if ($restored) { return 1 }
        return 2
    }
}

function Start-Compute {
    param ([string[]] $services = @())
    Write-Step "Starting the IIS Service"
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

# Either both folders become the new build or both stay as they were, and the same for Hops; IIS is restarted either way.
function Update-Compute {
    # The bootstrap's folder. Earlier bootstraps named it differently, and older servers have none.
    $installPath = $installFolders | Where-Object { Test-Path $_ -PathType Container } | Select-Object -First 1
    if (-not $installPath) {
        $installPath = $installFolders[0]
        New-Item -ItemType Directory -Force -Path $installPath | Out-Null
    }
    $logFile = Join-Path $installPath $logFileName
    Start-Transcript -Path $logFile -Append | Out-Null
    $ErrorActionPreference = "Stop"

Write-Host @"
  # # # # # # # # # # # # # # # # # # # # #
  #                                       #
  #             U P D A T E               #
  #                                       #
  #       R H I N O . C O M P U T E       #
  #                                       #
  #             S C R I P T               #
  #                                       #
  # # # # # # # # # # # # # # # # # # # # #
"@
    Write-Host "Update started $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    Write-Host "Log file: $logFile"

    $exitCode = 1
    $iisStopped = $false
    $dependentServices = @()
    $olderBackupSetAside = $false
    $upToDate = $false
    $computeNeeded = $false
    $moved = @{}
    $artifact = $null
    # Hops: what was found, and how its update ended (0 updated or nothing to do, 1 failed and unchanged, 2 not fully restored).
    $hops = $null
    $hopsNote = $null
    $hopsCode = 0
    try {
        if (((Test-Path "$computeGeometryExe") -eq $false) -or ((Test-Path "$rhinoComputeExe") -eq $false)) {
            throw "The rhino.compute or compute.geometry executable file could not be found. Please run the bootstrap script first!"
        }

        Write-Step "Finding the latest $matchingBranch build"
        $artifact = Find-LatestArtifact
        if (-not $artifact) { throw "Unable to find the latest $matchingBranch build artifact." }
        Write-Host "Artifact $($artifact.id), commit $($artifact.workflow_run.head_sha), built $($artifact.created_at)"

        Write-Step "Downloading and unpacking it"
        if (Test-Path $stagingDir) { Remove-Item -Recurse -Force $stagingDir }
        New-Item -ItemType Directory -Path $stagingDir | Out-Null
        $zip = "$stagingDir\compute.zip"
        Download "https://nightly.link/mcneel/compute.rhino3d/actions/artifacts/$($artifact.id).zip" $zip
        Expand-Archive $zip -DestinationPath $stagingDir
        Remove-Item $zip
        $stagedHashes = @{}
        foreach ($file in @("rhino.compute\rhino.compute.exe", "compute.geometry\compute.geometry.exe", "rhino.compute\rhino.compute.dll", "compute.geometry\compute.geometry.dll")) {
            if (-not (Test-Path "$stagingDir\$file")) { throw "The download has no $file." }
            $stagedHashes[$file] = (Get-FileHash "$stagingDir\$file").Hash
        }

        # Compared by content, so a server with one folder on an older build still updates.
        $installedMatches = $true
        foreach ($file in $stagedHashes.Keys) {
            if (-not (Test-Path "$physicalPathRoot\$file") -or (Get-FileHash "$physicalPathRoot\$file").Hash -ne $stagedHashes[$file]) { $installedMatches = $false }
        }
        $upToDate = $installedMatches -and -not $Force
        $computeNeeded = -not $upToDate

        # A problem with Hops never stops the compute update; it is reported on its own.
        if ($SkipHops) {
            $hopsNote = "Hops was not checked (-SkipHops)."
        }
        else {
            Write-Step "Checking Hops"
            try {
                $packagePath = Get-HopsPackagePath
                $installed = Get-InstalledHops $packagePath
                if (-not $installed) {
                    $hopsNote = "Hops is not installed in $packagePath, so it was left alone."
                }
                else {
                    $rhinoVersion = Get-RhinoVersion
                    Write-Host "Installed: Hops $($installed.Name) in $packagePath, Rhino $rhinoVersion"
                    $latest = Find-LatestHops $rhinoVersion
                    if (-not $latest) {
                        $hopsNote = "No Hops release for Rhino $rhinoVersion was found, so Hops $($installed.Name) was left alone."
                    }
                    elseif ($installed.Version -and $installed.Version -ge $latest.Version -and -not $Force) {
                        $hopsNote = "Hops is already $($installed.Name), the latest release for Rhino $rhinoVersion."
                    }
                    else {
                        Write-Host "Downloading Hops $($latest.Name) from $($latest.Url)"
                        $staged = Get-HopsStaged $latest $packagePath
                        $hops = [pscustomobject]@{ PackagePath = $packagePath; Installed = $installed; Latest = $latest; Staged = $staged }
                    }
                }
            }
            catch {
                Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
                $hopsNote = "Hops update FAILED: $($_.Exception.Message) Nothing was changed."
                $hopsCode = 1
            }
            if ($hopsNote) { Write-Host $hopsNote }
        }

        if (-not $computeNeeded -and -not $hops) {
            $exitCode = 0
            return
        }

        if ($computeNeeded) {
            # The download carries the default web.config; the server's own settings (child count, timeouts) stay.
            $liveConfig = "$rhinoComputePath\web.config"
            $stagedConfig = "$stagingDir\rhino.compute\web.config"
            if (Test-Path $liveConfig) {
                Write-Step "Keeping this server's web.config"
                if ((Test-Path $stagedConfig) -and (Get-FileHash $liveConfig).Hash -ne (Get-FileHash $stagedConfig).Hash) {
                    Write-Host "It differs from the new build's default web.config, which is:"
                    Get-Content $stagedConfig | ForEach-Object { Write-Host "    $_" }
                }
                Copy-Item $liveConfig $stagedConfig -Force
            }

            Write-Step "Create backup"
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
        }

        Write-Step "Stopping the IIS services"
        # Stopping WAS stops every service that depends on it; the ones running now are started again.
        $dependentServices = @((Get-Service -Name "WAS" -ErrorAction SilentlyContinue).DependentServices | Where-Object { $_.Status -eq "Running" } | ForEach-Object { $_.Name })
        Invoke-Cmd "net stop was /y" | Out-Null
        $iisStopped = $true
        if ($hops) { Stop-HopsProcesses $hops.PackagePath }
        if (-not (Wait-ComputeExit)) { throw "compute.geometry or rhino.compute is still running." }

        if ($computeNeeded) {
            Write-Step "Replacing rhino.compute and compute.geometry"
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

            Write-Step "Granting application pool permissions on compute directories"
            foreach ($component in $components) {
                $code = Invoke-Cmd "icacls `"$physicalPathRoot\$component`" /grant `"IIS AppPool\${appPoolName}:(OI)(CI)F`" /t /c /q"
                if ($code -ne 0) { throw "icacls failed on $component (exit code $code)." }
            }

            Write-Step "Checking the installed files"
            foreach ($file in $stagedHashes.Keys) {
                if ((Get-FileHash "$physicalPathRoot\$file").Hash -ne $stagedHashes[$file]) { throw "$file is not the downloaded file." }
            }
            $productVersion = (Get-Item "$rhinoComputePath\rhino.compute.dll").VersionInfo.ProductVersion
            Write-Host "rhino.compute product version: $productVersion"
            if ($productVersion -notlike "*+$($artifact.workflow_run.head_sha)*") {
                Write-Host "Warning: the product version does not name commit $($artifact.workflow_run.head_sha)." -ForegroundColor Yellow
            }
            if ($olderBackupSetAside) {
                Remove-Item -Recurse -Force $olderBackupDir -ErrorAction SilentlyContinue
                if (Test-Path $olderBackupDir) { Write-Host "Could not delete $olderBackupDir; the next update will." -ForegroundColor Yellow }
                else { Write-Host "Deleted the older backup, $olderBackupDir" }
            }
        }
        $exitCode = 0
    }
    catch {
        Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
        if ($moved.Count -gt 0) { $exitCode = Restore-Backup $moved }
        if ($olderBackupSetAside -and $exitCode -ne 2) { Restore-OlderBackup }
        if ($hops -and -not $iisStopped) { $hopsNote = "Hops was not updated, because the compute update failed first."; $hopsCode = 1 }
    }
    finally {
        # Hops is swapped last, while IIS is still stopped, whether or not the compute update succeeded.
        if ($hops -and $iisStopped) {
            $hopsCode = Install-Hops $hops $installPath
            if ($hopsCode -eq 0 -and $hops.Installed.Name -eq $hops.Latest.Name) { $hopsNote = "Hops $($hops.Latest.Name) was reinstalled." }
            elseif ($hopsCode -eq 0) { $hopsNote = "Hops updated from $($hops.Installed.Name) to $($hops.Latest.Name)." }
            elseif ($hopsCode -eq 1) { $hopsNote = "Hops update FAILED: Hops $($hops.Installed.Name) was restored." }
            else { $hopsNote = "Hops update FAILED and Hops $($hops.Installed.Name) could not be fully restored. Its folders are in $installPath\$hopsBackupName-new." }
        }
        if ($hops -and (Test-Path $hops.Staged)) { Remove-Item -Recurse -Force $hops.Staged -ErrorAction SilentlyContinue }
        if ($iisStopped) { Start-Compute -services $dependentServices | Out-Null }
        if (Test-Path $stagingDir) { Remove-Item -Recurse -Force $stagingDir -ErrorAction SilentlyContinue }

        Write-Host
        if ($upToDate -and $exitCode -eq 0) {
            Write-Host "rhino.compute and compute.geometry are already $matchingBranch build $($artifact.workflow_run.head_sha.Substring(0, 7)); nothing was changed. Run the script with -Force to reinstall it." -ForegroundColor Green
        }
        elseif ($exitCode -eq 0) {
            Write-Host "Update finished: rhino.compute and compute.geometry are $matchingBranch build $($artifact.workflow_run.head_sha.Substring(0, 7)) (artifact $($artifact.id))." -ForegroundColor Green
        }
        elseif ($exitCode -eq 2) {
            Write-Host "Update FAILED and the previous rhino.compute and compute.geometry could not be fully restored. Their folders are in $backupDir." -ForegroundColor Red
        }
        elseif ($moved.Count -gt 0) {
            Write-Host "Update FAILED: the previous rhino.compute and compute.geometry were restored." -ForegroundColor Red
        }
        else {
            Write-Host "Update FAILED: rhino.compute and compute.geometry were not changed." -ForegroundColor Red
        }
        if ($hopsNote) { Write-Host $hopsNote -ForegroundColor $(if ($hopsCode -eq 0) { "Green" } else { "Red" }) }
        Write-Host "Log file: $logFile"
        Stop-Transcript | Out-Null
        # The worse of the two outcomes: 0 updated or current, 1 failed with nothing changed or restored, 2 not fully restored.
        [Math]::Max($exitCode, $hopsCode)
    }
}
#EndRegion funcs

# Dot-sourcing the script loads the functions without updating, for testing.
if ($MyInvocation.InvocationName -ne '.') {
    exit (Update-Compute)
}
