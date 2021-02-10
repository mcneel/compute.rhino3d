# Setup/Install script for compute on a web server
#Requires -RunAsAdministrator

$ErrorActionPreference = "Stop"

Write-Host @"

  # # # # # # # # # # # # # # # # # # # # #
  #                                       #
  #       R H I N O   C O M P U T E       #
  #                                       #
  #       U P D A T E   S C R I P T       #
  #                                       #
  # # # # # # # # # # # # # # # # # # # # #

"@

#Region funcs

function Write-Step { 
    Write-Host
    Write-Host "===> "$args[0] -ForegroundColor Darkgreen
    Write-Host
}

#EndRegion funcs

$appDirectory = "$home\Desktop"
$computeExe = "$appDirectory\compute\compute.geometry.exe"

# check that compute exists
if ((Test-Path "$computeExe") -eq $false) {
    Write-Host "File not found: $computeExe" -ForegroundColor Red
    Write-Host "Run bootstrap script first!" -ForegroundColor Red
    exit 1
}

Write-Step 'Stop compute.geometry service'
Start-Process "$computeExe" -ArgumentList 'stop' -Wait
Start-Sleep -s 5 # wait for process to release files

Write-Step 'Create backup'
$backupDir = "$appDirectory\.compute-backup"
if ((Test-Path "$backupDir") -eq $true) {
    Write-Host "Deleting '$backupDir'"
    Remove-Item -Recurse -Force "$backupDir"
}
Write-Host "Moving '$appDirectory\compute' to '$backupDir'"
Rename-Item -Path "$appDirectory\compute" -NewName "$backupDir"

Write-Step 'Download and unzip latest build of compute'
$baseurl = 'https://ci.appveyor.com/api/projects/mcneel/compute-rhino3d'
$branch = 'master'
$project = Invoke-RestMethod -Method Get -Uri "$baseurl/branch/$branch"
Write-Host "Fetching artifact from build:" $project.build.buildNumber
Invoke-WebRequest -Uri "$baseurl/artifacts/compute.zip?branch=$branch" -Out .\compute.zip
Expand-Archive .\compute.zip -DestinationPath "$appDirectory\compute"
Remove-Item .\compute.zip


Write-Step 'Start compute.geometry service'
Start-Process "$computeExe" -ArgumentList 'start' -Wait

