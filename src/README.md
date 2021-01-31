# The important directories

#### compute.components

Grasshopper component project (Hops) for communicating with a compute.geometry server.

#### compute.geometry

The main geometry server project.


# The other directories

#### compute.client

Generate (C#, python, javascript) client libraries for communicating with a compute.geometry server.

#### compute.frontend

⚠️ Ignore this project.

The original _compute_ project got bloated with request logging and authentication methods, so we split it into _geometry_ and _frontend_. It's used on compute.rhino3d.com and that's it.

If you want something to put in front of a compute server, check out [Rhino Compute AppServer](https://github.com/mcneel/compute.rhino3d.appserver).
