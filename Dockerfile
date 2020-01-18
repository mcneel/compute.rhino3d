# escape=`

# NOTE: use 'process' isolation to build image (otherwise rhino fails to install)

### builder image
FROM mcr.microsoft.com/dotnet/framework/sdk:4.8 as builder
RUN powershell -Command Invoke-WebRequest -OutFile nuget.exe https://dist.nuget.org/win-x86-commandline/v5.0.2/nuget.exe

# copy everything, restore nuget packages and build app
COPY src/ ./src/
RUN nuget.exe restore .\src\compute.sln
RUN msbuild /p:Configuration=Release src/compute.sln

### main image
FROM mcr.microsoft.com/windows:1903
SHELL ["powershell", "-Command"]

# install rhino (with “-package -quiet” args)
# NOTE: edit this if you use a different version of rhino!
ADD http://files.mcneel.com/dujour/exe/20200114/rhino_en-us_7.0.20014.11185.exe rhino_installer.exe
RUN Start-Process .\rhino_installer.exe -ArgumentList '-package', '-quiet' -NoNewWindow -Wait
RUN Remove-Item .\rhino_installer.exe

# setup cloudzoo auth
# NOTE: switch on CloudZooPlainText and copy 55500d41-3a41-4474-99b3-684032a4f4df.lic,
#       cloudzoo.json and settings-Scheme__Default.xml to the working dir
COPY ["55500d41-3a41-4474-99b3-684032a4f4df.lic", "C:/ProgramData/McNeel/Rhinoceros/6.0/License Manager/Licenses/"]
COPY ["cloudzoo.json", "C:/Users/ContainerAdministrator/AppData/Roaming/McNeel/Rhinoceros/6.0/License Manager/Licenses/"]
COPY ["settings-Scheme__Default.xml", "C:/Users/ContainerAdministrator/AppData/Roaming/McNeel/Rhinoceros/7.0/settings/"]

COPY --from=builder ["/src/bin/Release", "/app"]

WORKDIR /app

EXPOSE 80

CMD ["compute.frontend.exe"]