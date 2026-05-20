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
function Write-Step {
    Write-Host
    Write-Host "===> "$args[0] -ForegroundColor Green
    Write-Host
}
function Install-IISPrerequisites {
    #Check to see if IIS components are installed
    Write-Host "Determining if all necessary IIS components have been installed" -ForegroundColor Green
    ForEach ($feature in $required_IIS_features) {
        IF ((Get-WindowsOptionalFeature -Online -FeatureName $feature).State -eq "Disabled"){
           Write-Host "$($feature) missing - installing"
           Enable-WindowsOptionalFeature -Online -FeatureName $feature -NoRestart
        }
    }
    Write-Step "All of the Necessary IIS Role Services have been installed"
}
#EndRegion funcs

# Install IIS and subsystems
Write-Step 'Installing IIS Role Services'
Install-IISPrerequisites