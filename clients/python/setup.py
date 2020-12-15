import setuptools

long_description = '''
# compute_rhino3d
Python package providing convenience functions to call compute.rhino3d.com
geometry web services

Project Hompage: https://github.com/mcneel/compute.rhino3d

### Supported platforms
* This is a pure python package and should work on all versions of python


## Test

* start `python`
```
>>> from rhino3dm import *
>>> import compute_rhino3d.Util
>>> import compute_rhino3d.Mesh
>>>
>>> compute_rhino3d.Util.authToken = AUTH_TOKEN_FROM (rhino3d.com/compute/login)
>>> center = Point3d(250, 250, 0)
>>> sphere = Sphere(center, 100)
>>> brep = sphere.ToBrep()
>>> meshes = compute_rhino3d.Mesh.CreateFromBrep(brep)
>>> print("Computed mesh with {} faces".format(len(meshes[0].Faces))
```
'''

setuptools.setup(
    name="compute_rhino3d",
    version="0.12.2",
    packages=['compute_rhino3d'],
    author="Robert McNeel & Associates",
    author_email="steve@mcneel.com",
    description="Python client library for compute.rhino3d web service",
    long_description=long_description,
    long_description_content_type="text/markdown",
    url="https://github.com/mcneel/compute.rhino3d",
    install_requires=['requests', 'rhino3dm'],
    classifiers=[
        "Development Status :: 4 - Beta",
        "Intended Audience :: Developers",
        "License :: OSI Approved :: MIT License",
        "Programming Language :: Python"
    ],
)
