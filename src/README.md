# What's in here?

#### compute.geometry

The main geometry server project. This is probably what you're looking for.

#### compute.client

Generates clients for the server.

---

#### compute.frontend

⚠️ Ignore this project.

The original _compute_ project got a bit bloated with request logging and authentication methods, so we split it into _geometry_ and _frontend_. It's used on compute.rhino3d.com and that's it.

If you want something to put in front of a compute server, check out the [Rhino Compute AppServer](https://github.com/mcneel/compute.rhino3d.appserver).
