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

    Write-Error "Installer failed after $MaxRetries attempts."
    return $false
}
#EndRegion funcs

Write-Step 'Set environment variables'
SetEnvVar 'RHINO_TOKEN' $RhinoToken -secret
if ($PSBoundParameters.ContainsKey('ApiKey')) {
    SetEnvVar 'RHINO_COMPUTE_KEY' $ApiKey
}
SetEnvVar 'RHINO_COMPUTE_URLS' 'http://+:80'

# Download and install Rhino
Write-Step 'Download latest Rhino 8'
$rhinoDownloadUrl = "https://www.rhino3d.com/www-api/download/direct/?slug=rhino-for-windows/8/latest/?email=$EmailAddress" 
$rhinoSetup = "rhino_setup.exe"
$setupFullPath = Join-Path -Path $tmpFullPath -ChildPath $rhinoSetup
Download $rhinoDownloadUrl $setupFullPath

# Set firewall rule to allow installation
New-NetFirewallRule -DisplayName "Rhino 8 Installer" -Direction Inbound -Program $setupFullPath -Action Allow

Write-Step 'Installing Rhino'
# Automated install (https://wiki.mcneel.com/rhino/installingrhino/8)
$success = Install-ProcessWithTimeout -ExePath $setupFullPath -Arguments @('-passive','-norestart') -TimeoutSeconds 600 -MaxRetries 2

if ($success) {
    $installedVersion = [Version] (Get-ItemProperty -Path HKLM:\SOFTWARE\McNeel\Rhinoceros\8.0\Install -name "version").Version
    Write-Host "Successfully installed Rhino $installedVersion"
} else {
    Write-Host "Rhino installation failed after retries."
}
