# Setup/Install script for installing IIS and various subsystems
#Requires -RunAsAdministrator

$required_IIS_features = @(
    "IIS-WebServerRole",
    "IIS-WebServer",
    "IIS-CommonHttpFeatures",
    "IIS-HttpErrors",
    "IIS-HttpRedirect",
    "IIS-ApplicationDevelopment",
    "NetFx4Extended-ASPNET45",
    "IIS-NetFxExtensibility45",
    "IIS-HealthAndDiagnostics",
    "IIS-HttpLogging",
    "IIS-LoggingLibraries",
    "IIS-RequestMonitor",
    "IIS-HttpTracing",
    "IIS-Security",
    "IIS-RequestFiltering",
    "IIS-Performance",
    "IIS-WebServerManagementTools",
    "IIS-IIS6ManagementCompatibility",
    "IIS-ManagementConsole",
    "IIS-BasicAuthentication",
    "IIS-WindowsAuthentication",
    "IIS-StaticContent",
    "IIS-ApplicationInit",
    "IIS-ISAPIExtensions",
    "IIS-ISAPIFilter",
    "IIS-HttpCompressionStatic",
    "IIS-ASPNET45",
    "WAS-WindowsActivationService",
    "WAS-ProcessModel",
    "WAS-ConfigurationAPI",
    "WCF-Services45",
    "WCF-HTTP-Activation45",
    "WCF-TCP-Activation45",
    "WCF-Pipe-Activation45",
    "WCF-TCP-PortSharing45"
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
    Install-WindowsFeature WAS
    Install-WindowsFeature NET-Framework-45-Features
    Write-Step "All of the Necessary IIS Role Services have been installed"
}
#EndRegion funcs

# Install IIS and subsystems
Write-Step 'Installing IIS Role Services'
Install-IISPrerequisites