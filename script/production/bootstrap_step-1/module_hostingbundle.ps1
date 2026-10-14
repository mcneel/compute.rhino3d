# Setup/Install script for installing .NET Core Hosting Bundle
#Requires -RunAsAdministrator

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
function Install-ProcessWithTimeout {
    param (
        [Parameter(Mandatory=$true)][string]$ExePath,
        [string[]]$Arguments,
        [int]$TimeoutSeconds = 600,   # default 10 min timeout
        [int]$MaxRetries = 2
    )
    $busyWaits = 0
    $attempt = 0
    while ($attempt -lt $MaxRetries) {
        $attempt++
        Write-Host "Starting attempt {$attempt}: $ExePath $Arguments"

        # Process.Start rather than Start-Process -PassThru, whose ExitCode Windows PowerShell can leave empty.
        $startInfo = New-Object System.Diagnostics.ProcessStartInfo($ExePath, ($Arguments -join ' '))
        $startInfo.WorkingDirectory = Split-Path $ExePath
        $process = [System.Diagnostics.Process]::Start($startInfo)

        # Stopping an installer that's still working would leave Windows Installer busy for the next try.
        if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
            throw "$(Split-Path $ExePath -Leaf) was still running after $($TimeoutSeconds / 60) minutes. Let it finish, then run the bootstrap again."
        }
        $exitCode = $process.ExitCode
        if ($exitCode -eq 0) {
            Write-Host "Installer finished successfully on attempt $attempt"
            return $true
        }
        if ($exitCode -eq 3010 -or $exitCode -eq 1641) {
            Write-Warning "Installer finished, but Windows needs a restart to complete it (exit code $exitCode)"
            return $true
        }
        # Another installation is running (often Windows Update on a new server): wait for it without using up a try.
        if ($exitCode -eq 1618 -and $busyWaits -lt 10) {
            $busyWaits++
            $attempt--
            Write-Host "Another installation is in progress (exit code 1618); trying again in a minute"
            Start-Sleep -Seconds 60
            continue
        }
        Write-Warning "Installer exited with code $exitCode on attempt $attempt"
        if ($attempt -lt $MaxRetries) {
            Write-Host "Retrying..."
            Start-Sleep -Seconds 5
        }
    }

    return $false
}
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

# From 8.36 on, Rhino 8 installs the .NET 10 runtimes, and compute.geometry needs all of its runtimes from one .NET version.
$dotnetChannel = "10.0"

# Download and install .NET Hosting Bundle
Write-Step "Download ASP.NET Core $dotnetChannel Hosting Bundle"

$bundle = Find-HostingBundle $dotnetChannel
$hbInstallerURL = $bundle.Url
$hbInstallerFilename = "dotnet-hosting-$($bundle.Version)-win.exe"
$hbInstallerFilepath = Join-Path -Path $tmpFullPath -ChildPath $hbInstallerFilename

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