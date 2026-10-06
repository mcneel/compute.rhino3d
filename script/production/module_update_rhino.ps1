# Setup/Install script for installing Rhino
#Requires -RunAsAdministrator

param (
    [Parameter(Mandatory=$true)][string] $EmailAddress,
    # Unused; kept so existing command lines still work.
    [switch] $install = $false
)

$physicalPathRoot = "C:\inetpub\wwwroot\aspnet_client\system_web\4_0_30319"
$websiteName = "Rhino.Compute"
$installFolders = @("C:\Rhino-Compute-Installation", "C:\Rhino_Compute_Installation", "C:\Rhino Compute Installation")
$logFileName = "update_rhino_log.txt"
$rhinoRegistryKey = "HKLM:\SOFTWARE\McNeel\Rhinoceros\8.0\Install"
$rhinoDownloadUrl = "https://www.rhino3d.com/www-api/download/direct/?slug=rhino-for-windows/8/latest/?email=" + [uri]::EscapeDataString($EmailAddress)
$installerTimeoutSeconds = 1200

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

function Get-InstalledRhinoVersion {
    $key = Get-ItemProperty -Path $rhinoRegistryKey -Name "Version" -ErrorAction SilentlyContinue
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

# Process.Start rather than Start-Process -PassThru, whose ExitCode Windows PowerShell can leave empty.
function Invoke-Installer {
    param ([Parameter(Mandatory=$true)][string] $path)
    $process = [System.Diagnostics.Process]::Start($path, "-passive -norestart")
    if (-not $process.WaitForExit($installerTimeoutSeconds * 1000)) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
        throw "The installer didn't finish within $($installerTimeoutSeconds / 60) minutes."
    }
    return $process.ExitCode
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

# Rhino.Compute is stopped only while the installer runs, and started again whatever the result.
function Update-Rhino {
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
  #       U P D A T E    R H I N O        #
  #                                       #
  #             S C R I P T               #
  #                                       #
  # # # # # # # # # # # # # # # # # # # # #
"@
    Write-Host "Update started $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    Write-Host "Log file: $logFile"

    $installedBefore = $null
    $package = $null
    $upToDate = $false
    $iisStopped = $false
    $installStarted = $false
    $installerCode = $null
    $installer = $null
    $failure = $null
    try {
        $installedBefore = Get-InstalledRhinoVersion
        if (-not $installedBefore) { throw "Rhino 8 isn't installed. Please run the bootstrap script first!" }

        Write-Step "Checking for update"
        $package = Resolve-RhinoInstaller
        Write-Host "Installed: Rhino $installedBefore. Latest: Rhino $($package.Version)."
        if ($installedBefore -ge $package.Version) {
            $upToDate = $true
        }
        else {
            Write-Step "Downloading $($package.FileName)"
            $tempPath = Join-Path $installPath "Temp"
            New-Item -ItemType Directory -Force -Path $tempPath | Out-Null
            $installer = Join-Path $tempPath $package.FileName
            Write-Host $package.Url
            Download $package.Url $installer

            $running = @(Get-Process -Name "Rhino" -ErrorAction SilentlyContinue)
            if ($running.Count -gt 0) {
                throw "Rhino is open on this server (process $($running.Id -join ', ')). Close it and run the update again."
            }

            Write-Step "Stopping the IIS services"
            Invoke-Cmd "net stop was /y" | Out-Null
            $iisStopped = $true
            if (-not (Wait-ComputeExit)) { throw "compute.geometry or rhino.compute is still running." }

            Write-Step "Installing Rhino $($package.Version)"
            # Automated install (https://wiki.mcneel.com/rhino/installingrhino/8)
            $installStarted = $true
            $installerCode = Invoke-Installer $installer
            Write-Host "Installer exit code: $installerCode"
        }
    }
    catch {
        $failure = $_.Exception.Message
        Write-Host "ERROR: $failure" -ForegroundColor Red
    }
    finally {
        if ($iisStopped) { Start-Compute | Out-Null }
        if ($installer -and (Test-Path $installer)) { Remove-Item $installer -Force -ErrorAction SilentlyContinue }

        $installedAfter = Get-InstalledRhinoVersion
        Write-Host
        if ($upToDate) {
            $exitCode = 0
            Write-Host "Rhino $installedBefore is already the latest version; nothing was changed." -ForegroundColor Green
        }
        elseif (-not $installStarted) {
            $exitCode = 1
            Write-Host "Update FAILED: nothing was changed." -ForegroundColor Red
        }
        elseif (-not $failure -and $installerCode -eq 0 -and $installedAfter -eq $package.Version) {
            $exitCode = 0
            Write-Host "Update finished: Rhino $installedBefore is now Rhino $installedAfter." -ForegroundColor Green
        }
        elseif ($installerCode -eq 3010 -or $installerCode -eq 1641) {
            $exitCode = 2
            Write-Host "Rhino $installedAfter is installed, but Windows needs a restart to finish the update." -ForegroundColor Yellow
        }
        elseif ($installedAfter -eq $installedBefore) {
            $exitCode = 1
            Write-Host "Update FAILED: Rhino is still $installedBefore." -ForegroundColor Red
        }
        else {
            $exitCode = 2
            Write-Host "Update needs attention: Rhino reads $installedAfter, expected $($package.Version)." -ForegroundColor Yellow
        }
        Write-Host "Log file: $logFile"
        Stop-Transcript | Out-Null
    }
    return $exitCode
}
#EndRegion funcs

# Dot-sourcing the script loads the functions without updating, for testing.
if ($MyInvocation.InvocationName -ne '.') {
    exit (Update-Rhino)
}
