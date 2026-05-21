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

    Write-Error "Installer failed after $MaxRetries attempts."
    return $false
}
#EndRegion funcs

# Download and install .NET Hosting Bundle
Write-Step 'Download ASP.NET Core 8.0 Hosting Bundle'

$hbInstallerURL = "https://builds.dotnet.microsoft.com/dotnet/aspnetcore/Runtime/8.0.14/dotnet-hosting-8.0.14-win.exe"
$hbInstallerFilename = "dotnet-hosting-8.0.14-win.exe"
$hbInstallerFilepath = Join-Path -Path $tmpFullPath -ChildPath $hbInstallerFilename

Download $hbInstallerURL $hbInstallerFilepath

Write-Step 'Installing ASP.NET Core 8.0 Hosting Bundle'

$success = Install-ProcessWithTimeout -ExePath $hbInstallerFilepath -Arguments @('/repair','/quiet','/norestart') -TimeoutSeconds 600 -MaxRetries 2

if ($success) {
    Write-Output "$hbInstallerFilename successfully installed"
    Write-Step 'Restarting IIS services'
    net stop was /y
    net start w3svc
} else {
    Write-Output "Something went wrong with the hosting bundle installation after retries."
}