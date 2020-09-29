# escape=`

FROM mcr.microsoft.com/dotnet/framework/sdk:4.8 as builder

COPY . ./src/compute.frontend/
RUN msbuild -p:Configuration=Release -restore -v:Minimal src/compute.frontend/compute.frontend.csproj

FROM mcr.microsoft.com/dotnet/framework/runtime:4.8-windowsservercore-ltsc2019

COPY --from=builder ["/src/bin/Release", "/app"]
WORKDIR /app

ENV COMPUTE_SPAWN_GEOMETRY_SERVER=false
ENV COMPUTE_STASH_METHOD=NONE
ENV COMPUTE_HTTP_PORT=80

EXPOSE 80

CMD ["compute.frontend.exe"]
