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

# Download and install .NET Hosting Bundle
Write-Step 'Download ASP.NET Core 8.0 Hosting Bundle'

$hbInstallerURL = "https://builds.dotnet.microsoft.com/dotnet/aspnetcore/Runtime/8.0.14/dotnet-hosting-8.0.14-win.exe"
$hbIntallerFilename = [System.IO.Path]::GetFileName( $hbInstallerURL )
$hbInstallerFilepath = Join-Path -Path $tmpFullPath -ChildPath $hbIntallerFilename
Download $hbInstallerURL $hbInstallerFilepath
Write-Output ""
Write-Output "$hbIntallerFilename downloaded"
Write-Output ""
Write-Step 'Installing ASP.NET Core 8.0 Hosting Bundle'
$result = Start-Process -FilePath $hbInstallerFilepath -ArgumentList '/repair', '/quiet', '/norestart' -NoNewWindow -Wait -PassThru
If($result.Exitcode -Eq 0)
{
    Write-Output "$hbIntallerFilename successfully installed"
    Write-Step 'Restarting IIS services'
    net stop was /y
    net start w3svc
}
else {
    Write-Output "Something went wrong with the hosting bundle installation. Errorlevel: ${result.ExitCode}"
}