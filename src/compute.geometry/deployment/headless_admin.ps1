Param(
  [switch]$initialize = $false,
  [switch]$updaterhino = $false,
  [string]$LICENSE_KEY
)

if ($initialize) {
    # Make http/https access available to Nancy
    if (${env:com.rhino3d.compute.HTTP_PORT}) {
      netsh http add urlacl url="http://+:${env:com.rhino3d.compute.HTTP_PORT}/" user="Everyone"
    }
    if (${env:com.rhino3d.compute.HTTPS_PORT}) {
      netsh http add urlacl url="https://+:${env:com.rhino3d.compute.HTTPS_PORT}/" user="Everyone"
    }
}

if ($updaterhino) {
    # TODO: probably need to stop the compute.geometry and compute.frontend service

    # Download Rhino
    $url = "http://files.mcneel.com/dujour/exe/20180812/rhino_en-us_7.0.18224.20205.exe"
    $rhino_installer = $PSScriptRoot + "\rhinoinstaller.exe"
    (New-Object System.Net.WebClient).DownloadFile($url, $rhino_installer)
    #Invoke-WebRequest -Uri $url -OutFile $rhino_installer

    # Install Rhino
    if ($LICENSE_KEY) {
        & $rhino_installer LICENSE_METHOD=STANDALONE LICENSE_KEY=$LICENSE_KEY
    } else {
        & $rhino_installer
    }
}
