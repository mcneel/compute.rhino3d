# Build yak packages for publishing
import os
import shutil

dist_directory = 'dist'
if not os.path.exists(dist_directory):
    os.mkdir(dist_directory)

# build hops.gha
build_cmd = 'msbuild .\\hops\\hops.csproj -t:restore'
os.system(build_cmd) #nuget restore
build_cmd = 'msbuild .\\hops\\hops.csproj'
build_cmd += ' -p:Configuration=Release'
build_cmd += ' -p:Platform=x64'
build_cmd += f' -p:OutputPath="..\\{dist_directory}"'
os.system(build_cmd)

# build compute.geometry.exe
build_cmd = 'msbuild .\\compute.geometry\\compute.geometry.csproj -t:restore'
os.system(build_cmd) #nuget restore
build_cmd = 'msbuild .\\compute.geometry\\compute.geometry.csproj'
build_cmd += ' -p:Configuration=Release'
build_cmd += ' -p:Platform=x64'
build_cmd += f' -p:OutputPath="..\\{dist_directory}\\compute.geometry"'
os.system(build_cmd)

# build rhino.compute.exe
build_cmd = 'dotnet publish .\\rhino.compute\\rhino.compute.csproj'
build_cmd += ' -c Release'
build_cmd += f' -o ".\\{dist_directory}\\rhino.compute"'
build_cmd += ' -p:PublishTrimmed=true'
build_cmd += ' -r win-x64'
build_cmd += ' --self-contained true'
os.system(build_cmd)

# write manifest for yak
manifest_content = """---
name: Hops
version: $version
authors:
- Robert McNeel & Associates
description: Out of process definition solving using Rhino Compute
url: https://github.com/mcneel/compute.rhino3d
icon_url: https://raw.githubusercontent.com/mcneel/compute.rhino3d/master/src/hops/resources/Hops_48x48.png
"""
with open(f'{dist_directory}\\manifest.yml', 'w') as text_file:
    text_file.write(manifest_content)

# build yak package
os.chdir(dist_directory)
os.system('"C:\\Program Files\\Rhino 7\\System\\Yak.exe" build')
# make V8 version as well
for file in os.listdir('.'):
    if file.endswith('.yak'):
        v8name = file.replace('-rh7', '-rh8')
        shutil.copy(file, v8name)
