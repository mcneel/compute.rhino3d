# Build the Hops yak package for this machine's platform: python3 build_hops_dist.py [--no-package]
# Windows and Mac ship as separate packages, each built on its own OS; Rhino picks the right one.
# --no-package stops before yak so CI can sign the binaries first. Set YAK to the yak executable if it isn't in the default Rhino 9 location.
import argparse
import os
import platform
import shutil
import subprocess
import sys

src_dir = os.path.dirname(os.path.realpath(__file__))
dist_dir = os.path.join(src_dir, 'dist')
on_mac = platform.system() == 'Darwin'

parser = argparse.ArgumentParser()
parser.add_argument('--no-package', action='store_true', help='build the dist folder but skip yak')
args = parser.parse_args()


def run(command, cwd=src_dir):
    print('>', ' '.join(command), flush=True)
    rv = subprocess.call(command, cwd=cwd)
    if rv != 0:
        sys.exit(rv)


def find_yak():
    candidates = [os.environ.get('YAK')]
    if on_mac:
        candidates += ['/Applications/Rhino 9.app/Contents/Resources/bin/yak']
    else:
        candidates += [r'C:\Program Files\Rhino 9\System\Yak.exe']
    for c in candidates:
        if c and os.path.isfile(c):
            return c
    sys.exit('yak not found; set the YAK environment variable to its path')


if os.path.exists(dist_dir):
    shutil.rmtree(dist_dir)

run(['dotnet', 'publish', os.path.join('hops', 'Hops.csproj'), '-c', 'Release', '-o', dist_dir])

# Self-contained, since a Mac has no shared .NET runtime for compute.geometry to use.
for project in ['rhino.compute', 'compute.geometry']:
    run(['dotnet', 'publish', os.path.join(project, project + '.csproj'),
         '-c', 'Release', '--self-contained', 'true', '-o', os.path.join(dist_dir, project)])

if not args.no_package:
    run([find_yak(), 'build', '--platform', 'mac' if on_mac else 'win'], cwd=dist_dir)
