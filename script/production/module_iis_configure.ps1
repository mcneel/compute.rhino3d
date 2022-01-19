$userName = Read-Host 'Login user name'
$passwordSecure = Read-Host 'Login password' -AsSecureString
$password = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($passwordSecure))

$appPoolName = "RhinoComputeAppPool"
$websiteName = "Rhino.Compute"
$physicalPathRoot = "C:\inetpub\wwwroot\aspnet_client\system_web\4_0_30319"
$rhinoComputePath = "$physicalPathRoot\rhino.compute"
$computeGeometryPath = "$physicalPathRoot\compute.geometry"

Import-Module WebAdministration

# Setup IIS to work with rhino.compute
#Requires -RunAsAdministrator

#Region funcs
function Write-Step { 
    Write-Host
    Write-Host "===> "$args[0] -ForegroundColor Darkgreen
    Write-Host
}
function CreateAppPool {
    Param([string] $appPoolName)
     if(Test-Path ("IIS:\AppPools\$appPoolName")) {
         Write-Host "The App Pool $appPoolName already exists"
         return
     }
     $appPool = New-WebAppPool -Name $appPoolName
}
#EndRegion funcs


Write-Step "Creating application pool"
CreateAppPool $appPoolName
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "processModel.loadUserProfile" -Value "True"

If((Test-Path "IIS:\Sites\Default Web Site"))
{
    Write-Step "Removing default website"
    Get-WebSite -Name "Default Web Site" | Remove-WebSite -Confirm:$false 
}

Write-Step "Creating new rhino.compute site"
If((Test-Path $rhinoComputePath))
{
    If(!(Test-Path "IIS:\Sites\$websiteName"))
    {
        New-WebSite -Name $websiteName -Id 2 -PhysicalPath $rhinoComputePath -ApplicationPool $appPoolName -Port 80 
        $website = Get-Item "IIS:\Sites\$websiteName"
        $website.virtualDirectoryDefaults.userName = $userName
        $website.virtualDirectoryDefaults.password = $password
        $website | set-item
        Set-ItemProperty "IIS:\Sites\$websiteName" -name applicationDefaults.preloadEnabled -value True
    }
    else {
        Write-Host "The IIS site $websiteName already exists"
    }
}

Write-Step "Granting application pool permissions on compute directories" 
cmd /c icacls $rhinoComputePath /grant ("IIS AppPool\$appPoolName" + ':(OI)(CI)M') /t /c /q
cmd /c icacls $computeGeometryPath /grant ("IIS AppPool\$appPoolName"+ ':(OI)(CI)M') /t /c /q

Write-Step "Starting rhino.compute site" 
Start-IISSite -Name $websiteName