$appPoolName = "RhinoComputeAppPool"
$websiteName = "Rhino.Compute"
$physicalPathRoot = "C:\inetpub\wwwroot\aspnet_client\system_web\4_0_30319"
$rhinoComputePath = "$physicalPathRoot\rhino.compute"
$computeGeometryPath = "$physicalPathRoot\compute.geometry"
$rhinoPackagesPath = "C:\ProgramData\McNeel\Rhinoceros\packages"
$userPackagesPath = "C:\Users\$env:UserName\AppData\Roaming\McNeel\Rhinoceros\packages"
$appPoolPackagesPath = "C:\Users\RhinoComputeAppPool\AppData\Roaming\McNeel\Rhinoceros"
$localUserName = "RhinoComputeUser"

Import-Module WebAdministration

# Setup IIS to work with rhino.compute
#Requires -RunAsAdministrator

#Region funcs
function Write-Step { 
    Write-Host
    Write-Host "===> "$args[0] -ForegroundColor Green
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
function New-RandomPassword {
    param(
        [Parameter()]
        [int]$MinimumPasswordLength = 16,
        [Parameter()]
        [int]$MaximumPasswordLength = 20,
        [Parameter()]
        [int]$NumberOfAlphaNumericCharacters = 6,
        [Parameter()]
        [switch]$ConvertToSecureString
    )
    
    Add-Type -AssemblyName 'System.Web'
    $length = Get-Random -Minimum $MinimumPasswordLength -Maximum $MaximumPasswordLength
    $password = [System.Web.Security.Membership]::GeneratePassword($length,$NumberOfAlphaNumericCharacters)
    if ($ConvertToSecureString.IsPresent) {
        ConvertTo-SecureString -String $password -AsPlainText -Force
    } else {
        $password
    }
}
#EndRegion funcs

Write-Host "Generating new API Key"
$ApiKey = [guid]::NewGuid().Guid
SetEnvVar 'RHINO_COMPUTE_KEY' $ApiKey

Write-Step "Creating a new user identity"
$securePassword = New-RandomPassword -MinimumPasswordLength 16 -MaximumPasswordLength 20 -NumberOfAlphaNumericCharacters 6 -ConvertToSecureString
$localUserAccount = @{
   Name = $localUserName
   Password = $securePassword
   Description = 'User identity passed to IIS RhinoComputeAppPool'
   AccountNeverExpires = $true
   PasswordNeverExpires = $true
   Verbose = $true
}
New-LocalUser @localUserAccount

Write-Step "Adding user to RDP user group"
Add-LocalGroupMember -Group "Remote Desktop Users" -Member $localUserName

$localUserPassword = (New-Object PSCredential $localUserName,$securePassword).GetNetworkCredential().Password

Write-Step "Creating application pool"
CreateAppPool $appPoolName
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "processModel.loadUserProfile" -Value "True"
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "processModel.setProfileEnvironment" -Value "True"
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "processModel" -value @{userName = $localUserName; password = $localUserPassword; identitytype = 3}

$node = Select-XML -Path "$rhinoComputePath\web.config" -XPath "//aspNetCore" | Select -ExpandProperty Node
$arguments = $node.arguments

if($arguments.Contains('idlespan'))
{
    $params = $arguments -split "--"
    foreach($i in $params)
    {
        if($i.Contains('idlespan'))
        {
            $values = $i -split " "
            Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "processModel.idleTimeout" -Value ([TimeSpan]::FromMinutes(($values[1]/60) + 5))
        }
    }
}
else
{
    Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "processModel.idleTimeout" -Value ([TimeSpan]::FromMinutes(65))
}

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
        Set-ItemProperty "IIS:\Sites\$websiteName" -name applicationDefaults.preloadEnabled -value True
    }
    else {
        Write-Host "The IIS site $websiteName already exists"
    }
}

Write-Step "Granting application pool permissions on compute directories" 
cmd /c icacls $rhinoComputePath /grant ("IIS AppPool\$appPoolName" + ':(OI)(CI)F') /t /c /q 
cmd /c icacls $computeGeometryPath /grant ("IIS AppPool\$appPoolName"+ ':(OI)(CI)F') /t /c /q 

Write-Step "Setting environment variable for log paths"
SetEnvVar 'RHINO_COMPUTE_LOG_PATH' "$rhinoComputePath\logs"

Write-Step "Starting rhino.compute site" 
Start-IISSite -Name $websiteName

Start-Transcript -Path C:\Users\Administrator\Desktop\ReadMe.txt
Write-Host
Write-Host "Congratulations! All components have now been installed."
Write-Host
Write-Host "Your Rhino Compute API Key is: $ApiKey"
Write-Host "You need to send your API Key as a value for RhinoComputeKey header with every request"
Write-Host
Write-Host "To install third party plugins follow these steps"
Write-Host "  1) Log into your VM using these credentials (write these down)"
Write-Host "     User Name: $localUserName"
Write-Host "     Password: $localUserPassword"
Write-Host "  2) Install plugins using the Rhino package manager or"
Write-Host "  3) Copy/paste plugin files to C:\Users\$localUserName\AppData\Roaming\Grasshopper\Libraries"
Write-Host "  4) Restart the VM"
Write-Host
Write-Host "Please save the User Name and Password above for your records!"
Write-Host
Stop-Transcript
