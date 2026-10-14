# Setup/Install script for installing IIS and various subsystems
#Requires -RunAsAdministrator

# Minimum IIS feature set required to host rhino.compute under IIS via the
# AspNetCoreModuleV2 (delivered by the .NET Hosting Bundle in bootstrap step 2).
$required_IIS_features = @(
    "IIS-WebServerRole",
    "IIS-WebServer",
    "IIS-CommonHttpFeatures",
    "IIS-DefaultDocument",
    "IIS-StaticContent",
    "IIS-RequestFiltering",
    "IIS-ApplicationInit",            # required for applicationDefaults.preloadEnabled
    "IIS-WebServerManagementTools",
    "IIS-ManagementConsole"           # WebAdministration PS module + IIS Manager GUI
)

#Region funcs
. "$PSScriptRoot\common.ps1"
function Install-IISPrerequisites {
    #Check to see if IIS components are installed
    Write-Host "Determining if all necessary IIS components have been installed" -ForegroundColor Green
    # One DISM call for all the missing features; each call takes 10-30 seconds.
    $missing = @(Get-WindowsOptionalFeature -Online | Where-Object { $_.FeatureName -in $required_IIS_features -and $_.State -ne "Enabled" } | ForEach-Object { $_.FeatureName })
    if ($missing.Count -gt 0) {
        Write-Host "Installing $($missing -join ', ')"
        $result = Enable-WindowsOptionalFeature -Online -FeatureName $missing -All -NoRestart
        if ($result.RestartNeeded) { Write-Warning "Windows needs a restart to finish installing IIS." }
    }
    Write-Step "All of the Necessary IIS Role Services have been installed"
}
#EndRegion funcs

# Install IIS and subsystems
Write-Step 'Installing IIS Role Services'
Install-IISPrerequisites