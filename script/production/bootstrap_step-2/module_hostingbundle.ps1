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
#EndRegion funcs

$temp_path = "C:\temp\"

if( ![System.IO.Directory]::Exists( $temp_path ) )
{
    New-Item $temp_path -ItemType Directory
}

# Download and install .NET Hosting Bundle
Write-Step 'Download ASP.NET Core 5.0 Hosting Bundle'
$hb_installer_url = "https://download.visualstudio.microsoft.com/download/pr/08a0bb65-6004-4099-8289-945f1681d946/f77d62d5e200693bec1416d4aefa97f7/dotnet-hosting-5.0.13-win.exe"
$hb_intaller_filename = [System.IO.Path]::GetFileName( $hb_installer_url )
$hb_installer_filepath = $temp_path + $hb_intaller_filename
Download $hb_installer_url $hb_installer_filepath
Write-Output ""
Write-Output "$hb_intaller_filename downloaded"
Write-Output ""
Write-Step 'Installing ASP.NET Core 5.0 Hosting Bundle'
$result = Start-Process -FilePath $hb_installer_filepath -ArgumentList '/repair', '/quiet', '/norestart' -NoNewWindow -Wait -PassThru
If($result.Exitcode -Eq 0)
{
    Write-Output "$hb_intaller_filename installed"
    Write-Step 'Restarting IIS services'
    net stop was /y
    net start w3svc
}
else {
    Write-Output "Something went wrong with the installation. Errorlevel: ${result.ExitCode}"
}