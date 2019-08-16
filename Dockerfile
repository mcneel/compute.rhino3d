# escape=`

# base image should match the host operating system
# rhino only works with process isolation
FROM mcr.microsoft.com/windows:1809

# install dotnet
RUN powershell -NoProfile -ExecutionPolicy unrestricted -Command " `
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; `
    &([scriptblock]::Create((Invoke-WebRequest -useb 'https://dot.net/v1/dotnet-install.ps1'))) -version 2.2.203"

ENV DOTNET_RUNNING_IN_CONTAINER=true `
    NUGET_XMLDOC_MODE=skip `
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true

# install .NET 4.6.2 framework via chocolatey
RUN powershell -Command Set-ExecutionPolicy Bypass -Scope Process -Force; `
        iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
RUN powershell -Command "choco install -y netfx-4.6.2-devpack nuget.commandline"
RUN setx path "%path%;C:\Users\ContainerAdministrator\AppData\Local\Microsoft\dotnet"

# install rhino (with “-package -quiet” args)
# NOTE: edit this if you use a different version of rhino!
ADD https://files.mcneel.com/dujour/exe/20190917/rhino_en-us_7.0.19260.11525.exe rhino_installer.exe
RUN powershell -Command " `
    Start-Process .\rhino_installer.exe -ArgumentList '-package', '-quiet' -NoNewWindow -Wait; `
    Remove-Item .\rhino_installer.exe"

# setup cloudzoo auth
# NOTE: switch on CloudZooPlainText and copy 55500d41-3a41-4474-99b3-684032a4f4df.lic,
#       cloudzoo.json and settings-Scheme__Default.xml to the working dir
COPY ["55500d41-3a41-4474-99b3-684032a4f4df.lic", "C:/ProgramData/McNeel/Rhinoceros/6.0/License Manager/Licenses/"]
COPY ["cloudzoo.json", "C:/Users/ContainerAdministrator/AppData/Roaming/McNeel/Rhinoceros/6.0/License Manager/Licenses/"]
COPY ["settings-Scheme__Default.xml", "C:/Users/ContainerAdministrator/AppData/Roaming/McNeel/Rhinoceros/7.0/settings/"]

# compile compute
COPY src src
RUN powershell -Command " `
    nuget restore .\src; `
    dotnet msbuild /p:Configuration=Release .\src"

EXPOSE 80

CMD .\src\bin\Release\compute.frontend.exe
