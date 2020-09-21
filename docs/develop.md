# Developing with Rhino Compute

This is a short guide to getting set up to compile and debug Compute on your local Windows 10 machine.

1. [Download](https://www.rhino3d.com/download/rhino-for-windows/wip) and install Rhino 7 WIP.
1. Start Rhino at least once to configure its license.
1. Clone this repository.
1. Open `src\compute.sln` in Visual Studio 2019 and compile as `Debug`.
1. Make sure that `compute.geometry` is set as the startup project.
1. Start the application in the debugger.
1. Wait for Compute to load... ☕️
    ![compute.geometry.exe](images/compute_geometry_screenshot.png)
1. Browse to http://localhost:8081/version to check that it's working!