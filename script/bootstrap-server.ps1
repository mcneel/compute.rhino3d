# Setup/Install script for compute on a web server
#Requires -RunAsAdministrator

param (
    [Parameter(Mandatory=$true)][string] $EmailAddress,
    [Parameter(Mandatory=$true)][string] $ApiKey
)

$appDirectory = "$home\Desktop"

# make sure cloudzoo.json and GUID.lic files are on desktop
if ((Test-Path "$home\Desktop\cloudzoo.json") -eq $false) {
    throw "cloudzoo.json needs to be copied to the desktop"
}
if ((Test-Path "$home\Desktop\55500d41-3a41-4474-99b3-684032a4f4df.lic") -eq $false) {
    throw "55500d41-3a41-4474-99b3-684032a4f4df.lic needs to be copied to the desktop"
}

function Write-Step { 
    Write-Host "== "$args[0] -ForegroundColor Darkgreen
}
function Download {
    param (
        [Parameter(Mandatory=$true)][string] $url,
        [Parameter(Mandatory=$true)][string] $output
    )
    (New-Object System.Net.WebClient).DownloadFile($url, $output)
}


Write-Step 'Set environment variable'
if ($PSBoundParameters.ContainsKey('ApiKey')) {
    [System.Environment]::SetEnvironmentVariable("RHINO_COMPUTE_KEY", $ApiKey, "Machine")
}
[System.Environment]::SetEnvironmentVariable("COMPUTE_BACKEND_PORT", 80, "Machine")

Write-Step 'Install IIS'
Install-WindowsFeature -name Web-Server -IncludeManagementTools


Write-Step 'Configure URL reservation (80 and 443)'
Start-Process "netsh" -ArgumentList "http", "add", "urlacl", "url='http://+:80/'", "user='Everyone'"
Start-Process "netsh" -ArgumentList "http", "add", "urlacl", "url='http://+:443/'", "user='Everyone'"

Write-Step 'Download and unzip latest build of compute'
if ((Test-Path "$appDirectory\compute") -eq $false) {
    Download "https://ci.appveyor.com/api/projects/mcneel/compute-rhino3d/artifacts/compute.zip?branch=master" "compute.zip"
    Expand-Archive "compute.zip" -DestinationPath "$appDirectory\compute"
    Remove-Item "compute.zip"
}


Write-Step 'Download latest Rhino 7'
$rhino7DownloadUrl = "https://www.rhino3d.com/download/rhino-for-windows/7/wip/direct?email=$EmailAddress"
$rhino7Setup = "rhino7_setup.exe"
Download $rhino7DownloadUrl $rhino7Setup

# Automated install
# https://wiki.mcneel.com/rhino/installingrhino/6
Start-Process -FilePath $rhino7Setup -ArgumentList "-passive" -Wait

# delete installer
Remove-Item $rhino7Setup

Write-Step 'Tell license manager to use CloudZooPlainText'
$settingsXml = '
<?xml version="1.0" encoding="utf-8"?>
<settings id="2.0">
  <settings>
    <child key="LicensingSettings">
      <entry key="CloudZooPlainText">True</entry>
    </child>
  </settings>
</settings>
'
$settingsFile = "$env:APPDATA\McNeel\Rhinoceros\7.0\settings\settings-Scheme__Default.xml"
New-Item -Force -Path $settingsFile -Value $settingsXml

Write-Step 'Copy license files for Rhino'
New-Item -ItemType File -Path "$env:APPDATA\McNeel\Rhinoceros\6.0\License Manager\Licenses\cloudzoo.json" -Force
Copy-Item "$home\Desktop\cloudzoo.json" -Destination "$env:APPDATA\McNeel\Rhinoceros\6.0\License Manager\Licenses\cloudzoo.json"
New-Item -ItemType File -Path "$env:ProgramData\McNeel\Rhinoceros\6.0\License Manager\Licenses\55500d41-3a41-4474-99b3-684032a4f4df.lic" -Force
Copy-Item "$home\Desktop\55500d41-3a41-4474-99b3-684032a4f4df.lic" -Destination "$env:ProgramData\McNeel\Rhinoceros\6.0\License Manager\Licenses\55500d41-3a41-4474-99b3-684032a4f4df.lic"


Write-Step 'Next Steps: Install and start geometry service'
# go to compute directory and run
# ./compute.geometry.exe install
# username = .\Administrator
# password = account password
# ./compute.geometry.exe start


