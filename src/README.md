# The important directories

#### rhino.compute

The main project – responsible for spinning up/down instances of compute.geometry.exe and proxying HTTP requests to these instances.

#### compute.geometry

The main geometry server project.

#### compute.components

Grasshopper component project (Hops) for communicating with a compute.geometry server.


# The other directories

#### compute.client

Generate (C#, python, javascript) client libraries for communicating with a compute.geometry server.

#### compute.frontend

⚠️ Ignore this project.

The original _compute_ project got bloated with request logging and authentication methods, so we split it into _geometry_ and _frontend_. It's used on compute.rhino3d.com and that's it.

If you want something to put in front of a compute server, check out [Rhino Compute AppServer](https://github.com/mcneel/compute.rhino3d.appserver).
