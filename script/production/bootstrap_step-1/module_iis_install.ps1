# Setup/Install script for installing IIS and various subsystems
#Requires -RunAsAdministrator

# Minimum IIS feature set required to host rhino.compute under IIS via the
# AspNetCoreModuleV2 (delivered by the .NET Hosting Bundle). The historical list
# contained ~35 features carried over from the ASP.NET 4.x Framework era, but
# ASP.NET Core has a much smaller IIS surface area. Removed:
#   - ASP.NET 4.5 Framework features (IIS-ASPNET45, IIS-NetFxExtensibility45,
#     NetFx4Extended-ASPNET45) — rhino.compute targets ASP.NET Core, not Framework
#   - WCF-* features — no WCF in the stack
#   - WAS-* features — not needed for ASP.NET Core InProcess hosting
#   - IIS-IIS6ManagementCompatibility — legacy
#   - IIS-BasicAuthentication / IIS-WindowsAuthentication — site uses neither
#   - IIS-ISAPIExtensions / IIS-ISAPIFilter — AspNetCoreModuleV2 is native, not ISAPI
#   - IIS-HttpCompressionStatic — no static content to compress
#   - IIS-HttpRedirect, IIS-HttpErrors — not used
#   - Diagnostic features (HealthAndDiagnostics, HttpLogging, LoggingLibraries,
#     RequestMonitor, HttpTracing) — rhino.compute uses Serilog; IIS-level logging
#     is redundant. Re-enable if you ever need IIS request logs for troubleshooting.
# Result: dramatically faster VM provisioning (each DISM call takes ~10-30 seconds).
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