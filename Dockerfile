# escape=`

# see https://discourse.mcneel.com/t/docker-support/89322 for troubleshooting

# NOTE: use 'process' isolation to build image (otherwise rhino fails to install)

### builder image
FROM mcr.microsoft.com/dotnet/sdk:5.0 as builder

# copy everything, restore nuget packages and build app
COPY src/ ./src/
RUN dotnet publish -c Release -r win10-x64 --self-contained true src/compute.sln

### main image
# tag must match windows host for build (and run, if running with process isolation)
# e.g. 1903 for Windows 10 version 1903 host
FROM mcr.microsoft.com/windows:1809

#Copy the fonts and font install script
COPY fonts/* fonts/
COPY InstallFont.ps1 .

#Run font install scriptin powershell
RUN powershell -ExecutionPolicy Bypass -Command .\InstallFont.ps1

#Copy and extract Pufferfish plugin
COPY plugins/* plugins/

#RUN powershell Expand-Archive -Path ./test_plugins.zip -DestinationPath .

# install .net 4.8 if you're using the 1809 base image (see https://git.io/JUYio)
# comment this out for 1903 and newer
RUN curl -fSLo dotnet-framework-installer.exe https://download.visualstudio.microsoft.com/download/pr/7afca223-55d2-470a-8edc-6a1739ae3252/abd170b4b0ec15ad0222a809b761a036/ndp48-x86-x64-allos-enu.exe `
    && .\dotnet-framework-installer.exe /q `
    && del .\dotnet-framework-installer.exe `
    && powershell Remove-Item -Force -Recurse ${Env:TEMP}\*

# install rhino (with “-package -quiet” args)
# NOTE: edit this if you use a different version of rhino!
# the url below will always redirect to the latest rhino 7 (email required)
# https://www.rhino3d.com/download/rhino-for-windows/7/latest/direct?email=EMAIL
RUN curl -fSLo rhino_installer.exe https://www.rhino3d.com/download/rhino-for-windows/7/latest/direct?email=nikhil@jewlr.com `
    && .\rhino_installer.exe -package -quiet `
    && del .\rhino_installer.exe

#Create a libraries directory for the plugin and copy .gha file
RUN powershell mkdir C:\Users\ContainerAdministrator\AppData\Roaming\Grasshopper\Libraries

RUN powershell Copy-Item -Path .\plugins\Pufferfish.gha -Destination "C:\Users\ContainerAdministrator\AppData\Roaming\Grasshopper\Libraries\Pufferfish.gha" `
    && powershell Copy-Item -Path .\plugins\Jewlr.gha -Destination "C:\Program` Files\Rhino` 7\Plug-ins\Grasshopper\Components\Jewlr.gha"

#Copy config files
RUN powershell mkdir C:\Users\ContainerAdministrator\config
COPY config/* C:\Users\ContainerAdministrator\config\


# (optional) use the package manager to install plug-ins
#RUN ""C:\Program Files\Rhino 7\System\Yak.exe"" install jswan
RUN ""C:\Program Files\Rhino 7\System\Yak.exe"" install hops

# copy compute app to image
COPY --from=builder ["/src/dist", "/app"]
WORKDIR /app

# bind rhino.compute to port 5000
ENV ASPNETCORE_URLS="http://*:5000"
EXPOSE 5000

# uncomment to build core-hour billing credentials into image (not recommended)
# see https://developer.rhino3d.com/guides/compute/core-hour-billing/
#ENV RHINO_TOKEN=

CMD ["rhino.compute/rhino.compute.exe","--idlespan=1800"]