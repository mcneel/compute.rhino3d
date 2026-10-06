# Download/Install compute
#Requires -RunAsAdministrator

$physicalPathRoot = "C:\inetpub\wwwroot\aspnet_client\system_web\4_0_30319"
$rhinoComputePath = "$physicalPathRoot\rhino.compute"
$computeGeometryPath = "$physicalPathRoot\compute.geometry"
$rhinoComputeExe = "$rhinoComputePath\rhino.compute.exe"
$computeGeometryExe = "$computeGeometryPath\compute.geometry.exe"
$appPoolName = "RhinoComputeAppPool"
$websiteName = "Rhino.Compute"
$backupDir = "$physicalPathRoot\rhino.compute-backup"
$stagingDir = "$physicalPathRoot\rhino.compute-staging"
$installFolders = @("C:\Rhino-Compute-Installation", "C:\Rhino_Compute_Installation", "C:\Rhino Compute Installation")
$logFileName = "update_compute_log.txt"
$matchingBranch = "9.x"
$artifactName = "rhino.compute"
$components = @("rhino.compute", "compute.geometry")

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

function Get-ComputeProcesses {
    Get-Process -Name "compute.geometry", "rhino.compute" -ErrorAction SilentlyContinue | Where-Object {
        $path = $null
        try { $path = $_.Path } catch { }
        (-not $path) -or $path.StartsWith($physicalPathRoot, [System.StringComparison]::OrdinalIgnoreCase)
    }
}

# Children exit once they notice IIS has stopped; any still running after a minute are stopped.
function Wait-ComputeExit {
    $deadline = (Get-Date).AddSeconds(60)
    while ((Get-Date) -lt $deadline) {
        $busy = @(Get-ComputeProcesses).Count + @(Get-Process -Name "w3wp" -ErrorAction SilentlyContinue).Count
        if ($busy -eq 0) { return $true }
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
        throw "Could not move $from to $to. A file in it may be open in another program. ($($_.Exception.InnerException.Message))"
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

function Start-Compute {
    Write-Step "Starting the IIS Service"
    try {
        Invoke-Cmd "net start w3svc" | Out-Null
        Start-IISSite -Name $websiteName
    }
    catch {
        Write-Host "Could not start IIS: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Either both folders become the new build or both stay as they were; IIS is restarted either way.
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
    $moved = @{}
    $artifact = $null
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

        Write-Step "Create backup"
        if (Test-Path $backupDir) {
            Write-Host "Deleting '$backupDir'"
            Remove-Item -Recurse -Force $backupDir
        }
        New-Item -ItemType Directory -Path $backupDir | Out-Null

        Write-Step "Stopping the IIS services"
        Invoke-Cmd "net stop was /y" | Out-Null
        $iisStopped = $true
        if (-not (Wait-ComputeExit)) { throw "compute.geometry or rhino.compute is still running." }

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
        $exitCode = 0
    }
    catch {
        Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
        if ($moved.Count -gt 0) { $exitCode = Restore-Backup $moved }
    }
    finally {
        if ($iisStopped) { Start-Compute | Out-Null }
        if (Test-Path $stagingDir) { Remove-Item -Recurse -Force $stagingDir -ErrorAction SilentlyContinue }

        Write-Host
        if ($exitCode -eq 0) {
            Write-Host "Update finished: rhino.compute and compute.geometry are $matchingBranch build $($artifact.workflow_run.head_sha.Substring(0, 7)) (artifact $($artifact.id))." -ForegroundColor Green
        }
        elseif ($exitCode -eq 2) {
            Write-Host "Update FAILED and the previous version could not be fully restored. Its folders are in $backupDir." -ForegroundColor Red
        }
        elseif ($moved.Count -gt 0) {
            Write-Host "Update FAILED: the previous version was restored." -ForegroundColor Red
        }
        else {
            Write-Host "Update FAILED: nothing was changed." -ForegroundColor Red
        }
        Write-Host "Log file: $logFile"
        Stop-Transcript | Out-Null
    }
    return $exitCode
}
#EndRegion funcs

# Dot-sourcing the script loads the functions without updating, for testing.
if ($MyInvocation.InvocationName -ne '.') {
    exit (Update-Compute)
}
