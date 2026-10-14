# Functions the bootstrap's scripts share; each script dot-sources this file.

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
