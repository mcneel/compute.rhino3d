# Sets up Rhino.Compute under IIS on a new Windows Server, for Rhino 7, 8 or 9: Rhino, IIS, the latest compute
# build, the ASP.NET Core hosting bundle, the RhinoComputeUser account and app pool, and Hops.
#
# Run it in an administrator PowerShell. It asks for anything that isn't passed as a parameter:
#   .\bootstrap.ps1 [-RhinoVersion 7|8|9] [-EmailAddress <email>] [-ApiKey <key>] [-RhinoToken <token>] [-SkipHopsInstall]
# Running it again after a failure picks up where it stopped. update.ps1 keeps the server up to date afterwards.
#Requires -RunAsAdministrator

param (
    [ValidateSet("7", "8", "9")][string] $RhinoVersion,
    [string] $EmailAddress,
    [string] $ApiKey,
    [string] $RhinoToken,
    # Hops is a convenience for Grasshopper-definition-as-a-service workflows; compute.geometry works without it.
    [switch] $SkipHopsInstall
)
Set-ExecutionPolicy Bypass -Scope Process -Force

# Replaced with the commit when the script is published.
$scriptVersion = "dev"

# What differs between Rhino versions. The first download link that leads to an installer is used.
$rhinoVersions = @{
    "7" = @{ DownloadSlugs = @("rhino-for-windows/7/latest"); BuildBranch = "7.x"; DotnetChannel = "5.0"; Status = "out of support, no security fixes"
             Warning = "Rhino 7 is out of support: it gets no more updates, and its Rhino.Compute runs on .NET 5, which gets no security fixes." }
    "8" = @{ DownloadSlugs = @("rhino-for-windows/8/latest"); BuildBranch = "8.x"; DotnetChannel = "10.0" }
    # The release link answers once Rhino 9 is released; until then the beta's does.
    "9" = @{ DownloadSlugs = @("rhino-for-windows/9/latest", "rhino-for-windows/9/beta"); BuildBranch = "9.x"; DotnetChannel = "10.0" }
}
$defaultRhinoVersion = "8"

$installPath = "C:\Rhino-Compute-Installation"
$tempPath = "$installPath\Temp"
$logFile = "$installPath\bootstrap_log.txt"
$credentialFile = "$installPath\RhinoComputeUser.txt"
$settingsFile = "$installPath\bootstrap.json"
$physicalPathRoot = "C:\inetpub\wwwroot\aspnet_client\system_web\4_0_30319"
$rhinoComputePath = "$physicalPathRoot\rhino.compute"
$computeGeometryPath = "$physicalPathRoot\compute.geometry"
$appPoolName = "RhinoComputeAppPool"
$websiteName = "Rhino.Compute"
$localUserName = "RhinoComputeUser"
$hopsVersionsUrl = "https://yak.rhino3d.com/versions/hops"
$artifactsUrl = "https://api.github.com/repos/mcneel/compute.rhino3d/actions/artifacts"
# Minimum IIS feature set to host rhino.compute through the AspNetCoreModuleV2 the hosting bundle brings.
$iisFeatures = @(
    "IIS-WebServerRole", "IIS-WebServer", "IIS-CommonHttpFeatures", "IIS-DefaultDocument", "IIS-StaticContent",
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
function Download {
    param (
        [Parameter(Mandatory=$true)][string] $url,
        [Parameter(Mandatory=$true)][string] $output
    )
    (New-Object System.Net.WebClient).DownloadFile($url, $output)
}
function Set-MachineVariable {
    param (
        [Parameter(Mandatory=$true)][string] $name,
        [Parameter(Mandatory=$true)][string] $value,
        [switch] $secret
    )
    $print = if ($secret) { "***" } else { $value }
    Write-Host "Setting environment variable: $name=$print"
    [System.Environment]::SetEnvironmentVariable($name, $value, "Machine")
}
function Read-Required {
    param ([Parameter(Mandatory=$true)][string] $prompt)
    do { $value = (Read-Host $prompt).Trim() } while (-not $value)
    return $value
}
function Read-RhinoVersion {
    param ([Parameter(Mandatory=$true)][string] $default)
    Write-Host
    Write-Host "Which version of Rhino should this server run?"
    Write-Host
    foreach ($version in $rhinoVersions.Keys | Sort-Object) {
        Write-Host "  [$version] " -ForegroundColor Cyan -NoNewline
        Write-Host "Rhino $version" -NoNewline
        if ($version -eq $default) { Write-Host "  (default)" -NoNewline }
        if ($rhinoVersions[$version].Status) { Write-Host "  $($rhinoVersions[$version].Status)" -ForegroundColor Yellow -NoNewline }
        Write-Host
    }
    Write-Host
    while ($true) {
        $value = (Read-Host "Type 7, 8 or 9, or press Enter for $default").Trim()
        if (-not $value) { return $default }
        if ($rhinoVersions.ContainsKey($value)) { return $value }
        Write-Host "Please type 7, 8 or 9." -ForegroundColor Red
    }
}
# Writes a file only Administrators and SYSTEM can read; restricted before anything is written to it.
function Save-AdminOnlyFile {
    param (
        [Parameter(Mandatory=$true)][string] $path,
        [Parameter(Mandatory=$true)][string[]] $lines
    )
    New-Item -ItemType File -Path $path -Force | Out-Null
    icacls $path /inheritance:r /grant:r "*S-1-5-32-544:(F)" "*S-1-5-18:(F)" | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Couldn't restrict $path to Administrators (icacls exit code $LASTEXITCODE)." }
    Set-Content -Path $path -Value $lines
}
# The transcript's header repeats PowerShell's command line, so values passed as parameters end up in the log.
function Hide-Secrets {
    param ([Parameter(Mandatory=$true)][string] $path, [string[]] $secrets)
    $text = [System.IO.File]::ReadAllText($path)
    foreach ($secret in $secrets) {
        if ($secret) { $text = $text.Replace($secret, "***") }
    }
    [System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
}
function Install-ProcessWithTimeout {
    param (
        [Parameter(Mandatory=$true)][string]$ExePath,
        [string[]]$Arguments,
        [int]$TimeoutSeconds = 600,
        [int]$MaxRetries = 2
    )
    $busyWaits = 0
    $attempt = 0
    while ($attempt -lt $MaxRetries) {
        $attempt++
        Write-Host "Starting attempt {$attempt}: $ExePath $Arguments"
        # Process.Start rather than Start-Process -PassThru, whose ExitCode Windows PowerShell can leave empty.
        $startInfo = New-Object System.Diagnostics.ProcessStartInfo($ExePath, ($Arguments -join ' '))
        $startInfo.WorkingDirectory = Split-Path $ExePath
        $process = [System.Diagnostics.Process]::Start($startInfo)
        # Stopping an installer that's still working would leave Windows Installer busy for the next try.
        if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
            throw "$(Split-Path $ExePath -Leaf) was still running after $($TimeoutSeconds / 60) minutes. Let it finish, then run the bootstrap again."
        }
        $exitCode = $process.ExitCode
        if ($exitCode -eq 0) {
            Write-Host "Installer finished successfully on attempt $attempt"
            return $true
        }
        if ($exitCode -eq 3010 -or $exitCode -eq 1641) {
            Write-Warning "Installer finished, but Windows needs a restart to complete it (exit code $exitCode)"
            return $true
        }
        # Another installation is running (often Windows Update on a new server): wait for it without using up a try.
        if ($exitCode -eq 1618 -and $busyWaits -lt 10) {
            $busyWaits++
            $attempt--
            Write-Host "Another installation is in progress (exit code 1618); trying again in a minute"
            Start-Sleep -Seconds 60
            continue
        }
        Write-Warning "Installer exited with code $exitCode on attempt $attempt"
        if ($attempt -lt $MaxRetries) {
            Write-Host "Retrying..."
            Start-Sleep -Seconds 5
        }
    }
    return $false
}
# Where a download link redirects, or $null when it doesn't; a link that isn't live yet answers 404 rather than throwing.
function Get-RedirectLocation {
    param ([Parameter(Mandatory=$true)][string] $url)
    Add-Type -AssemblyName System.Net.Http
    $handler = New-Object System.Net.Http.HttpClientHandler
    $handler.AllowAutoRedirect = $false
    $client = New-Object System.Net.Http.HttpClient($handler)
    try {
        $response = $client.GetAsync($url).GetAwaiter().GetResult()
        if ($response.Headers.Location) { return (New-Object System.Uri((New-Object System.Uri($url)), $response.Headers.Location)).AbsoluteUri }
        return $null
    }
    finally {
        $client.Dispose()
    }
}

function Get-InstalledRhinoVersion {
    $key = Get-ItemProperty -Path "HKLM:\SOFTWARE\McNeel\Rhinoceros\$RhinoVersion.0\Install" -Name "Version" -ErrorAction SilentlyContinue
    if (-not $key) { return $null }
    return [Version]$key.Version
}
# The installer's file name ends in its version: rhino_en-us_8.36.26290.12001.exe, rhino_9.0.26286.06303.exe.
function Resolve-RhinoInstaller {
    foreach ($slug in $rhinoVersions[$RhinoVersion].DownloadSlugs) {
        $location = Get-RedirectLocation ("https://www.rhino3d.com/www-api/download/direct/?slug=$slug/?email=" + [uri]::EscapeDataString($EmailAddress))
        if (-not $location) {
            Write-Host "No installer at $slug yet"
            continue
        }
        Write-Host "Download link: $slug"
        $fileName = [System.IO.Path]::GetFileName(([uri]$location).AbsolutePath)
        $version = $null
        if (-not [Version]::TryParse([System.IO.Path]::GetFileNameWithoutExtension($fileName).Split('_')[-1], [ref]$version)) {
            throw "Couldn't read a version from the installer's name, $fileName."
        }
        return [pscustomobject]@{ Url = $location; FileName = $fileName; Version = $version }
    }
    throw "None of the Rhino $RhinoVersion download links ($($rhinoVersions[$RhinoVersion].DownloadSlugs -join ', ')) led to an installer."
}
function Install-Rhino {
    Write-Step "Setting environment variables"
    Set-MachineVariable 'RHINO_TOKEN' $RhinoToken -secret
    Set-MachineVariable 'RHINO_COMPUTE_KEY' $ApiKey -secret
    Set-MachineVariable 'RHINO_COMPUTE_URLS' 'http://+:80'

    Write-Step "Downloading the latest Rhino $RhinoVersion"
    $package = Resolve-RhinoInstaller
    $installedVersion = Get-InstalledRhinoVersion
    # A second run (after fixing whatever stopped the first) doesn't install the same Rhino again.
    if ($installedVersion -and $installedVersion -ge $package.Version) {
        Write-Host "Rhino $installedVersion is already installed; the latest is $($package.Version)."
        return
    }
    $installer = Join-Path $tempPath $package.FileName
    Download $package.Url $installer
    $signature = Get-AuthenticodeSignature -FilePath $installer
    if ($signature.Status -ne "Valid" -or $signature.SignerCertificate.Subject -notmatch 'O="?ROBERT MCNEEL & ASSOCIATES') {
        Remove-Item $installer -Force -ErrorAction SilentlyContinue
        $signer = if ($signature.SignerCertificate) { $signature.SignerCertificate.GetNameInfo("SimpleName", $false) } else { "nobody" }
        throw "The downloaded $($package.FileName) isn't signed by Robert McNeel & Associates (signature $($signature.Status), signed by $signer); not running it."
    }

    Write-Step "Installing Rhino $($package.Version)"
    # Automated install (https://wiki.mcneel.com/rhino/installingrhino/8)
    if (-not (Install-ProcessWithTimeout -ExePath $installer -Arguments @('-passive', '-norestart') -TimeoutSeconds 1200)) {
        throw "Rhino didn't install; the installer's exit codes are above."
    }
    Write-Host "Installed Rhino $(Get-InstalledRhinoVersion)"
}

function Enable-PingRules {
    Write-Step "Allowing ping"
    $rules = @(
        @{ DisplayName = "ICMP Allow incoming V4 echo request"; Protocol = "ICMPv4"; IcmpType = 8 },
        @{ DisplayName = "ICMP Allow incoming V6 echo request"; Protocol = "ICMPv6"; IcmpType = 128 }
    )
    foreach ($rule in $rules) {
        if (-not (Get-NetFirewallRule -DisplayName $rule.DisplayName -ErrorAction SilentlyContinue)) {
            New-NetFirewallRule @rule -Direction Inbound -Action Allow | Out-Null
        }
    }
}

function Install-IISFeatures {
    Write-Step "Installing IIS"
    # One DISM call for all the missing features; each call takes 10-30 seconds.
    $missing = @(Get-WindowsOptionalFeature -Online | Where-Object { $_.FeatureName -in $iisFeatures -and $_.State -ne "Enabled" } | ForEach-Object { $_.FeatureName })
    if ($missing.Count -eq 0) {
        Write-Host "IIS is already installed"
        return
    }
    Write-Host "Installing $($missing -join ', ')"
    $result = Enable-WindowsOptionalFeature -Online -FeatureName $missing -All -NoRestart
    if ($result.RestartNeeded) { Write-Warning "Windows needs a restart to finish installing IIS." }
}

function Install-ComputeBuild {
    $branch = $rhinoVersions[$RhinoVersion].BuildBranch
    Write-Step "Downloading the latest $branch build of Rhino.Compute"
    # A second run keeps the build already installed; update.ps1 updates it.
    if ((Test-Path "$rhinoComputePath\rhino.compute.exe") -and (Test-Path "$computeGeometryPath\compute.geometry.exe")) {
        Write-Host "rhino.compute and compute.geometry are already installed in $physicalPathRoot"
        return
    }
    # Newest first, all branches; skip other branches' builds and expired ones rather than settle for them.
    $artifact = (Invoke-RestMethod -Method Get -Uri "$($artifactsUrl)?name=rhino.compute&per_page=100").artifacts |
        Where-Object { -not $_.expired -and $_.workflow_run.head_branch -eq $branch } | Select-Object -First 1
    if (-not $artifact) { throw "There's no current $branch build of Rhino.Compute to download (GitHub keeps builds for 90 days)." }
    Write-Host "Build $($artifact.id), commit $($artifact.workflow_run.head_sha.Substring(0, 7)), built $($artifact.created_at)"

    New-Item -ItemType Directory -Force -Path $physicalPathRoot | Out-Null
    # What an interrupted run left behind.
    foreach ($folder in $rhinoComputePath, $computeGeometryPath) {
        if (Test-Path $folder) { Remove-Item -Recurse -Force $folder }
    }
    $zip = "$physicalPathRoot\compute.zip"
    Download "https://nightly.link/mcneel/compute.rhino3d/actions/artifacts/$($artifact.id).zip" $zip
    # nightly.link passes on GitHub's artifact; check it's the file GitHub lists (older artifacts have no digest).
    if ($artifact.digest -and ("sha256:" + (Get-FileHash $zip -Algorithm SHA256).Hash) -ne $artifact.digest) {
        Remove-Item $zip -Force -ErrorAction SilentlyContinue
        throw "The downloaded build doesn't match GitHub's checksum for build $($artifact.id); not installing it."
    }
    Expand-Archive $zip -DestinationPath $physicalPathRoot
    Remove-Item $zip
}

# The newest hosting bundle of a .NET version, from Microsoft's release list, so new servers get its security fixes.
function Find-HostingBundle {
    param ([Parameter(Mandatory=$true)][string] $channel)
    $releases = Invoke-RestMethod -Uri "https://builds.dotnet.microsoft.com/dotnet/release-metadata/$channel/releases.json"
    $release = $releases.releases | Where-Object { $_.'release-version' -eq $releases.'latest-release' } | Select-Object -First 1
    $file = $release.'aspnetcore-runtime'.files | Where-Object { $_.name -eq 'dotnet-hosting-win.exe' } | Select-Object -First 1
    if (-not $file) { throw "The .NET $channel release list has no hosting bundle." }
    return [pscustomobject]@{ Version = $release.'aspnetcore-runtime'.version; Url = $file.url; Hash = $file.hash; SupportPhase = $releases.'support-phase'; EndOfSupport = $releases.'eol-date' }
}
function Install-HostingBundle {
    # compute.geometry needs all of its .NET runtimes from one version; for Rhino 8 and 9 that's .NET 10, as Rhino installs.
    $channel = $rhinoVersions[$RhinoVersion].DotnetChannel
    Write-Step "Downloading the ASP.NET Core $channel Hosting Bundle"
    $bundle = Find-HostingBundle $channel
    if ($bundle.SupportPhase -eq "eol") {
        Write-Warning "Rhino $RhinoVersion's Rhino.Compute runs on .NET $channel, which Microsoft stopped supporting on $($bundle.EndOfSupport); it gets no more security fixes."
    }
    $installer = Join-Path $tempPath "dotnet-hosting-$($bundle.Version)-win.exe"
    Write-Host $bundle.Url
    Download $bundle.Url $installer
    if ((Get-FileHash -Path $installer -Algorithm SHA512).Hash -ne $bundle.Hash) {
        Remove-Item $installer -Force -ErrorAction SilentlyContinue
        throw "The downloaded hosting bundle doesn't match Microsoft's checksum; not installing it."
    }

    Write-Step "Installing the ASP.NET Core $($bundle.Version) Hosting Bundle"
    if (-not (Install-ProcessWithTimeout -ExePath $installer -Arguments @('/repair', '/quiet', '/norestart'))) {
        throw "The hosting bundle didn't install; the installer's exit codes are above."
    }
    Write-Host "Restarting IIS"
    net stop was /y | Out-Null
    net start w3svc | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "IIS didn't start again after the hosting bundle install (net start w3svc exit code $LASTEXITCODE)." }
}

function New-RandomPassword {
    Add-Type -AssemblyName 'System.Web'
    return [System.Web.Security.Membership]::GeneratePassword((Get-Random -Minimum 16 -Maximum 21), 6)
}
# Returns the account's new password; a second run gives an existing account a new one.
function Set-ComputeUser {
    Write-Step "Creating the $localUserName account"
    $password = New-RandomPassword
    $securePassword = ConvertTo-SecureString $password -AsPlainText -Force
    if (Get-LocalUser -Name $localUserName -ErrorAction SilentlyContinue) {
        Write-Host "$localUserName already exists; giving it a new password"
        Set-LocalUser -Name $localUserName -Password $securePassword
    } else {
        New-LocalUser -Name $localUserName -Password $securePassword -Description "User identity passed to IIS $appPoolName" -AccountNeverExpires -PasswordNeverExpires | Out-Null
    }
    # New-LocalUser leaves "password required" off, which would let the password be blanked later.
    net user $localUserName /passwordreq:yes | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Couldn't require a password for $localUserName (net user exit code $LASTEXITCODE)." }

    # By SID: the group's name is translated on non-English Windows.
    $remoteDesktopUsers = "S-1-5-32-555"
    if (-not (Get-LocalGroupMember -SID $remoteDesktopUsers -Member $localUserName -ErrorAction SilentlyContinue)) {
        Add-LocalGroupMember -SID $remoteDesktopUsers -Member $localUserName
    }

    # Shown once the transcript has stopped; restricted before the password is written.
    Write-Host "Saving the $localUserName credentials to $credentialFile (Administrators only)"
    Save-AdminOnlyFile $credentialFile @("User Name: $localUserName", "Password: $password")
    return $password
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
# Unpacks the newest Hops the installed Rhino can load into RhinoComputeUser's packages folder, as the package manager
# would, rather than running Yak.exe as that user. Hops is optional, so problems are warnings.
function Install-Hops {
    Write-Step "Installing Hops for $localUserName"
    try {
        $packagePath = "$(New-UserProfile $localUserName)\AppData\Roaming\McNeel\Rhinoceros\packages\$RhinoVersion.0\Hops"
        $installed = Get-InstalledHops $packagePath
        if ($installed) {
            Write-Host "Hops $installed is already installed in $packagePath"
            return
        }
        $rhino = Get-InstalledRhinoVersion
        $latest = Find-LatestHops $rhino
        if (-not $latest) {
            Write-Warning "The package server has no Hops release for Rhino $rhino yet, so Hops wasn't installed."
            return
        }
        $target = "$packagePath\$($latest.Name)"
        if (Test-Path $target) { Remove-Item -Recurse -Force $target }
        New-Item -ItemType Directory -Path $target | Out-Null
        # Expand-Archive only takes .zip files; a .yak is a zip.
        $zip = Join-Path $tempPath "Hops-$($latest.Name).zip"
        Download $latest.Url $zip
        Expand-Archive $zip -DestinationPath $target
        Remove-Item $zip -Force
        if (-not (Test-Path "$target\Hops.gha")) { throw "The Hops $($latest.Name) package has no Hops.gha." }
        [System.IO.File]::WriteAllText("$packagePath\manifest.txt", "$($latest.Name)`n")
        Write-Host "Installed Hops $($latest.Name) in $packagePath"
    }
    catch {
        Write-Warning "Hops wasn't installed: $($_.Exception.Message) Install it from the Package Manager, signed in as $localUserName."
    }
}

function Set-ComputeSite {
    param ([Parameter(Mandatory=$true)][string] $password)
    Import-Module WebAdministration

    Write-Step "Creating the $appPoolName application pool"
    if (-not (Test-Path "IIS:\AppPools\$appPoolName")) { New-WebAppPool -Name $appPoolName | Out-Null }
    $pool = "IIS:\AppPools\$appPoolName"
    Set-ItemProperty $pool -Name "managedRuntimeVersion" -Value ""
    Set-ItemProperty $pool -Name "processModel.loadUserProfile" -Value "True"
    Set-ItemProperty $pool -Name "processModel.setProfileEnvironment" -Value "True"
    Set-ItemProperty $pool -Name "processModel" -Value @{ userName = $localUserName; password = $password; identitytype = 3 }
    # rhino.compute restarts its children when idle; IIS's default restart every 29 hours would stop solves in progress,
    # and an overlapping restart would run a second rhino.compute, with its own children, until the first one exits.
    Set-ItemProperty $pool -Name "recycling.periodicRestart.time" -Value ([TimeSpan]::Zero)
    Set-ItemProperty $pool -Name "recycling.disallowOverlappingRotation" -Value $true
    # A little longer than rhino.compute's own idle span, which shuts its children down first.
    $idleMinutes = 65
    $arguments = (Select-Xml -Path "$rhinoComputePath\web.config" -XPath "//aspNetCore" | Select-Object -ExpandProperty Node).arguments
    if ($arguments -match '--idlespan\s+(\d+)') { $idleMinutes = [int]$Matches[1] / 60 + 5 }
    Set-ItemProperty $pool -Name "processModel.idleTimeout" -Value ([TimeSpan]::FromMinutes($idleMinutes))

    if (Test-Path "IIS:\Sites\Default Web Site") {
        Write-Step "Removing the default website"
        Remove-Website -Name "Default Web Site"
    }
    Write-Step "Creating the $websiteName site"
    if (Test-Path "IIS:\Sites\$websiteName") {
        Write-Host "The $websiteName site already exists"
    } else {
        New-Website -Name $websiteName -Id 2 -PhysicalPath $rhinoComputePath -ApplicationPool $appPoolName -Port 80 | Out-Null
        Set-ItemProperty "IIS:\Sites\$websiteName" -Name applicationDefaults.preloadEnabled -Value True
    }

    Write-Step "Granting the application pool access to the compute folders"
    # Modify covers writing logs; the app pool has no need to change permissions or take ownership.
    foreach ($path in $rhinoComputePath, $computeGeometryPath) {
        icacls $path /grant ("IIS AppPool\$appPoolName" + ':(OI)(CI)M') /t /c /q | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "Couldn't grant $appPoolName access to $path (icacls exit code $LASTEXITCODE)." }
    }

    Write-Step "Setting environment variables for logging"
    Set-MachineVariable 'RHINO_COMPUTE_LOG_PATH' "$rhinoComputePath\logs"
    # Debug-level logs, so the detail support asks for is already there; unlike RHINO_COMPUTE_DEBUG it doesn't change answers.
    # Rhino 7's compute doesn't read it.
    if ($RhinoVersion -ne "7") { Set-MachineVariable 'RHINO_COMPUTE_LOG_LEVEL' 'Debug' }

    Write-Step "Starting the $websiteName site"
    Start-IISSite -Name $websiteName
}
#EndRegion funcs

# Dot-sourcing the script loads the functions without setting anything up, for testing.
if ($MyInvocation.InvocationName -eq '.') { return }

Write-Host @"

  # # # # # # # # # # # # # # # # # # # # #
  #                                       #
  #       R H I N O   C O M P U T E       #
  #                                       #
  #    B O O T S T R A P   S C R I P T    #
  #                                       #
  # # # # # # # # # # # # # # # # # # # # #

"@
$scriptHash = if ($PSCommandPath) { (Get-FileHash -Path $PSCommandPath -Algorithm SHA256).Hash.Substring(0, 12).ToLower() } else { "not run from a file" }
Write-Host "bootstrap.ps1 version $scriptVersion ($scriptHash)"

$os = (Get-CimInstance -ClassName 'Win32_OperatingSystem').Caption
if ($os -notlike '*server*') {
    Write-Host "The script is intended for use on Windows Server. Detected '$os'" -ForegroundColor Red
    exit 1
}

# A second run defaults to the Rhino version the first one chose, and won't set up a different one over it.
$recorded = $null
if (Test-Path $settingsFile) { $recorded = Get-Content $settingsFile -Raw | ConvertFrom-Json }
if ($recorded -and $RhinoVersion -and $RhinoVersion -ne "$($recorded.rhinoVersion)") {
    Write-Host "This server was set up for Rhino $($recorded.rhinoVersion); the bootstrap won't set it up for Rhino $RhinoVersion as well." -ForegroundColor Red
    exit 1
}

# Asked before the transcript starts, so the API key and Rhino token stay out of the log.
if (-not $RhinoVersion) {
    $RhinoVersion = if ($recorded) { "$($recorded.rhinoVersion)" } else { Read-RhinoVersion $defaultRhinoVersion }
}
if (-not $EmailAddress) { $EmailAddress = if ($recorded -and $recorded.emailAddress) { $recorded.emailAddress } else { Read-Required "Email address" } }
if (-not $ApiKey) { $ApiKey = Read-Required "API key" }
if (-not $RhinoToken) { $RhinoToken = Read-Required "Rhino token" }
# Lets callers that can set an environment variable but not pass -SkipHopsInstall opt out of Hops.
if (-not $SkipHopsInstall -and $env:BOOTSTRAP_SKIP_HOPS -eq '1') { $SkipHopsInstall = $true }

New-Item -ItemType Directory -Force -Path $tempPath | Out-Null
try { Stop-Transcript | Out-Null } catch { }
Start-Transcript -Path $logFile -Append | Out-Null
Write-Step "Setting up Rhino.Compute for Rhino $RhinoVersion, $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
Write-Host "bootstrap.ps1 version $scriptVersion ($scriptHash)"
if ($rhinoVersions[$RhinoVersion].Warning) { Write-Warning $rhinoVersions[$RhinoVersion].Warning }

# Any error stops the bootstrap, so it never carries on past a failed step.
$ErrorActionPreference = "Stop"
# Windows PowerShell's progress bar slows Invoke-WebRequest and Expand-Archive down many times over.
$ProgressPreference = "SilentlyContinue"
$currentStep = $null
$failure = $null
try {
    # Recorded first, so update.ps1 and a second run know which Rhino this server is for, and whether it should have Hops.
    $settings = [ordered]@{ rhinoVersion = [int]$RhinoVersion; emailAddress = $EmailAddress; hops = -not $SkipHopsInstall; written = (Get-Date).ToUniversalTime().ToString("o"); script = "bootstrap.ps1 $scriptVersion ($scriptHash)" }
    Save-AdminOnlyFile $settingsFile (($settings | ConvertTo-Json) -split "`r?`n")
    $currentStep = "installing Rhino";               Install-Rhino
    $currentStep = "allowing ping";                  Enable-PingRules
    $currentStep = "installing IIS";                 Install-IISFeatures
    $currentStep = "installing Rhino.Compute";       Install-ComputeBuild
    $currentStep = "installing the hosting bundle";  Install-HostingBundle
    $currentStep = "creating $localUserName";        $password = Set-ComputeUser
    if ($SkipHopsInstall) {
        Write-Step "Skipping Hops (-SkipHopsInstall or BOOTSTRAP_SKIP_HOPS=1)"
        Write-Host "To install Hops later, use the Package Manager, signed in as $localUserName"
    } else {
        $currentStep = "installing Hops";            Install-Hops
    }
    $currentStep = "setting up the IIS site";        Set-ComputeSite $password
}
catch {
    $failure = $_
}
finally {
    Write-Host
    if ($failure) {
        Write-Host "Bootstrap FAILED while $($currentStep): $($failure.Exception.Message)" -ForegroundColor Red
        Write-Host $failure.InvocationInfo.PositionMessage
        Write-Host "Fix the problem, then run the bootstrap again. Log file: $logFile"
    } else {
        Write-Host "Congratulations! Rhino.Compute is set up for Rhino $RhinoVersion." -ForegroundColor Green
    }
    Write-Step "Bootstrap ended at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    Stop-Transcript | Out-Null
    try { Hide-Secrets $logFile @($ApiKey, $RhinoToken) }
    catch { Write-Warning "Couldn't remove the API key and Rhino token from ${logFile}: $($_.Exception.Message)" }
}

# Shown once the transcript has stopped, so the password isn't in the log.
if ($failure) {
    if (Test-Path $credentialFile) { Write-Host "The $localUserName password is in $credentialFile (Administrators only)." }
    exit 1
}
Write-Host
Write-Host "To install third party plugins follow these steps"
Write-Host "  1) Log into your VM using these credentials (write these down)"
Get-Content $credentialFile | ForEach-Object { Write-Host "     $_" }
Write-Host "  2) Install plugins using the Rhino package manager or"
Write-Host "  3) Copy/paste plugin files to C:\Users\$localUserName\AppData\Roaming\Grasshopper\Libraries"
Write-Host "  4) Restart the VM"
Write-Host
Write-Host "The User Name and Password are also saved in $credentialFile, which only Administrators can read. They're not in the log file."
Write-Host "Keep the server up to date with update.ps1."
