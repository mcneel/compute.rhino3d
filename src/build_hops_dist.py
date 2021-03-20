# Build yak packages for publishing
import os
import shutil
import sys

src_dir = os.path.dirname(os.path.realpath(__file__))
dist_dir = os.path.join(src_dir, 'dist')

# clear output dir
if os.path.exists(dist_dir):
    shutil.rmtree(dist_dir)

# build hops (inc. self-contained rhino.compute.exe)
os.chdir(src_dir)
build_cmd = 'dotnet publish .\\hops.sln'
build_cmd += ' -c Release'
build_cmd += ' -p:PublishTrimmed=true'
build_cmd += ' --self-contained true'
build_cmd += ' -r win-x64'
rv = os.system(build_cmd)
if (rv != 0): sys.exit(rv)

# build yak package
os.chdir(dist_dir)
os.system('"C:\\Program Files\\Rhino 7\\System\\Yak.exe" build')
# make V8 version as well
for file in os.listdir('.'):
    if file.endswith('.yak'):
        v8name = file.replace('-rh7', '-rh8')
        shutil.copy(file, v8name)
