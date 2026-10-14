#Requires -RunAsAdministrator

param(
    [Parameter(Mandatory=$true)][string] $TempPath,
    # Where the RhinoComputeUser credentials are saved, readable only by Administrators.
    [Parameter(Mandatory=$true)][string] $CredentialFile,
    # When set, skip the Hops plugin install step. Hops is a convenience for users
    # whose workflows depend on Grasshopper-definition-as-a-service; compute.geometry's
    # core Rhino API works fine without it.
    [switch]$SkipHopsInstall
)

# Env-var bridge: lets callers that can set an environment variable but not pass
# -SkipHopsInstall to boostrap_step-1.ps1 opt out of the Hops install by setting
# BOOTSTRAP_SKIP_HOPS=1 before launching the orchestrator.
if (-not $SkipHopsInstall -and $env:BOOTSTRAP_SKIP_HOPS -eq '1') {
    $SkipHopsInstall = $true
}

$appPoolName = "RhinoComputeAppPool"
$websiteName = "Rhino.Compute"
$physicalPathRoot = "C:\inetpub\wwwroot\aspnet_client\system_web\4_0_30319"
$rhinoComputePath = "$physicalPathRoot\rhino.compute"
$computeGeometryPath = "$physicalPathRoot\compute.geometry"
$localUserName = "RhinoComputeUser"
$hopsVersionsUrl = "https://yak.rhino3d.com/versions/hops"
$rhinoRegistryKey = "HKLM:\SOFTWARE\McNeel\Rhinoceros\8.0\Install"

Import-Module WebAdministration

# Setup IIS to work with rhino.compute

#Region funcs
. "$PSScriptRoot\common.ps1"
function CreateAppPool {
    Param([string] $appPoolName)
     if(Test-Path ("IIS:\AppPools\$appPoolName")) {
         Write-Host "The App Pool $appPoolName already exists"
         return
     }
     $appPool = New-WebAppPool -Name $appPoolName
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

# Creates the user's Windows profile without signing in as them (which fails in headless sessions), so files can
# go into it before IIS first loads it. Returns the profile's folder.
function New-UserProfile {
    param ([Parameter(Mandatory=$true)][string] $userName)
    $sid = (Get-LocalUser -Name $userName).SID.Value
    $existing = Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\$sid" -ErrorAction SilentlyContinue
    if ($existing) { return $existing.ProfileImagePath }
    if (-not ('UserEnv.NativeMethods' -as [type])) {
        Add-Type -Namespace UserEnv -Name NativeMethods -MemberDefinition '[DllImport("userenv.dll", CharSet = CharSet.Unicode)] public static extern int CreateProfile(string pszUserSid, string pszUserName, System.Text.StringBuilder pszProfilePath, uint cchProfilePath);'
    }
    $path = New-Object System.Text.StringBuilder 260
    $result = [UserEnv.NativeMethods]::CreateProfile($sid, $userName, $path, 260)
    if ($result -ne 0) { throw ("Couldn't create the {0} profile (0x{1:X8})." -f $userName, $result) }
    return $path.ToString()
}
# Packages name the oldest Rhino they load in: rh8_0 is any Rhino 8, rh8_36 needs 8.36 or later.
function Test-HopsPackageFits {
    param (
        [Parameter(Mandatory=$true)][string] $rhinoLabel,
        [Parameter(Mandatory=$true)][System.Version] $rhinoVersion
    )
    if ($rhinoLabel -notmatch '^rh(\d+)_(\d+)$') { return $false }
    return ([int]$Matches[1] -eq $rhinoVersion.Major) -and ([int]$Matches[2] -le $rhinoVersion.Minor)
}
# The newest release of Hops this Rhino can load, from the package server, or $null.
function Find-LatestHops {
    param ([Parameter(Mandatory=$true)][System.Version] $rhinoVersion)
    $latest = $null
    foreach ($release in (Invoke-RestMethod -Method Get -Uri $hopsVersionsUrl)) {
        if ($release.prerelease) { continue }
        $version = $null
        if (-not [System.Version]::TryParse($release.version, [ref]$version)) { continue }
        $package = $release.distributions | Where-Object { $_.platform -in "any", "win" -and (Test-HopsPackageFits $_.rhino_version $rhinoVersion) } | Select-Object -First 1
        if (-not $package) { continue }
        if (-not $latest -or $version -gt $latest.Version) {
            $latest = [pscustomobject]@{ Version = $version; Name = $release.version; Url = $package.url }
        }
    }
    return $latest
}
# The version the package manager installed (named in manifest.txt), or $null when Hops isn't installed.
function Get-InstalledHops {
    param ([Parameter(Mandatory=$true)][string] $packagePath)
    $manifest = "$packagePath\manifest.txt"
    if (-not (Test-Path $manifest)) { return $null }
    $name = ([System.IO.File]::ReadAllText($manifest)).Trim()
    if (-not $name -or -not (Test-Path "$packagePath\$name\Hops.gha")) { return $null }
    return $name
}
# Unpacks the newest Hops the installed Rhino can load into the user's packages folder, as the package manager
# would, rather than running Yak.exe as the user. Returns the version installed.
function Install-Hops {
    param ([Parameter(Mandatory=$true)][string] $packagePath)
    $rhinoVersion = [System.Version](Get-ItemProperty -Path $rhinoRegistryKey -Name "Version").Version
    $latest = Find-LatestHops $rhinoVersion
    if (-not $latest) { throw "The package server has no Hops release for Rhino $rhinoVersion." }
    $target = "$packagePath\$($latest.Name)"
    if (Test-Path $target) { Remove-Item -Recurse -Force $target }
    New-Item -ItemType Directory -Path $target | Out-Null
    # Expand-Archive only takes .zip files; a .yak is a zip.
    $zip = Join-Path $TempPath "Hops-$($latest.Name).zip"
    Download $latest.Url $zip
    Expand-Archive $zip -DestinationPath $target
    Remove-Item $zip -Force
    if (-not (Test-Path "$target\Hops.gha")) { throw "The Hops $($latest.Name) package has no Hops.gha." }
    [System.IO.File]::WriteAllText("$packagePath\manifest.txt", "$($latest.Name)`n")
    return $latest.Name
}
#EndRegion funcs

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
if (Get-LocalUser -Name $localUserName -ErrorAction SilentlyContinue) {
    # A second run: the app pool and the credentials file get the new password below.
    Write-Host "$localUserName already exists; giving it a new password"
    Set-LocalUser -Name $localUserName -Password $securePassword
} else {
    New-LocalUser @localUserAccount
}

Write-Step "Adding user to RDP user group"
# By SID: the group's name is translated on non-English Windows.
$remoteDesktopUsers = "S-1-5-32-555"
if (-not (Get-LocalGroupMember -SID $remoteDesktopUsers -Member $localUserName -ErrorAction SilentlyContinue)) {
    Add-LocalGroupMember -SID $remoteDesktopUsers -Member $localUserName
}

$localUserPassword = (New-Object PSCredential $localUserName,$securePassword).GetNetworkCredential().Password

# The main script shows these after its transcript stops; restricted before the password is written.
Write-Step "Saving the $localUserName credentials to $CredentialFile (Administrators only)"
New-Item -ItemType File -Path $CredentialFile -Force | Out-Null
icacls $CredentialFile /inheritance:r /grant:r "*S-1-5-32-544:(F)" "*S-1-5-18:(F)" | Out-Null
if ($LASTEXITCODE -ne 0) { throw "Couldn't restrict $CredentialFile to Administrators (icacls exit code $LASTEXITCODE)." }
Set-Content -Path $CredentialFile -Value @("User Name: $localUserName", "Password: $localUserPassword")

if ($SkipHopsInstall) {
    Write-Step "Skipping Hops plugin install (set by -SkipHopsInstall switch or BOOTSTRAP_SKIP_HOPS=1)"
    Write-Host "  Users who need Hops can install it after deployment from the Package Manager, signed in as $localUserName"
} else {
    Write-Step "Installing the Hops plugin for $localUserName"
    # Hops is optional, so a failure here doesn't stop the rest of the server being set up.
    try {
        $packagePath = "$(New-UserProfile $localUserName)\AppData\Roaming\McNeel\Rhinoceros\packages\8.0\Hops"
        $installedHops = Get-InstalledHops $packagePath
        if ($installedHops) {
            Write-Host "Hops $installedHops is already installed in $packagePath"
        } else {
            Write-Host "Installed Hops $(Install-Hops $packagePath) in $packagePath"
        }
    }
    catch {
        Write-Warning "Hops wasn't installed: $($_.Exception.Message) Install it from the Package Manager, signed in as $localUserName."
    }
}

Write-Step "Creating application pool"
CreateAppPool $appPoolName
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "processModel.loadUserProfile" -Value "True"
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "processModel.setProfileEnvironment" -Value "True"
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "processModel" -value @{userName = $localUserName; password = $localUserPassword; identitytype = 3}
# rhino.compute restarts its children when idle; IIS's default restart every 29 hours would stop solves in progress,
# and an overlapping restart would run a second rhino.compute, with its own children, until the first one exits.
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "recycling.periodicRestart.time" -Value ([TimeSpan]::Zero)
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "recycling.disallowOverlappingRotation" -Value $true

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
# Modify covers writing logs; the app pool has no need to change permissions or take ownership.
foreach ($path in $rhinoComputePath, $computeGeometryPath) {
    icacls $path /grant ("IIS AppPool\$appPoolName" + ':(OI)(CI)M') /t /c /q | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Couldn't grant $appPoolName access to $path (icacls exit code $LASTEXITCODE)." }
}

Write-Step "Setting environment variable for log paths"
SetEnvVar 'RHINO_COMPUTE_LOG_PATH' "$rhinoComputePath\logs"

Write-Step "Starting rhino.compute site" 
Start-IISSite -Name $websiteName
