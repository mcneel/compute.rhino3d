# escape=`

# see https://discourse.mcneel.com/t/docker-support/89322 for usage

# NOTE: use 'process' isolation to build image (otherwise rhino fails to install)

### builder image
FROM mcr.microsoft.com/dotnet/framework/sdk:4.8 as builder

# copy everything, restore nuget packages and build app
COPY src/ ./src/
RUN msbuild /p:Configuration=Release /restore src/compute.sln

### main image
# NOTE: rhino will not install unless the "full" windows base image is used
# this dockerfile is set up to build on my machine (Windows 10 build 1903)
# since process isolation is required, if you need to build it on another version
# of windows then you'll need to change the tag, e.g. 1809 for Windows Server 2019
# you may need to change the version of the .net builder image too
FROM mcr.microsoft.com/windows:1903
SHELL ["powershell", "-Command"]

# install rhino (with “-package -quiet” args)
# NOTE: edit this if you use a different version of rhino!
# the url below will always redirect to the latest rhino 7 wip (email required)
# https://www.rhino3d.com/download/rhino-for-windows/7/wip/direct?email=EMAIL
ADD http://files.mcneel.com/dujour/exe/20200616/rhino_en-us_7.0.20168.13075.exe rhino_installer.exe
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