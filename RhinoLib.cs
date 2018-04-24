using System;
using System.Runtime.InteropServices;

class RhinoLib
{
    static RhinoLib()
    {
        Init();
    }

    private static bool _pathsSet = false;
    public static string _rhpath;
    public static void Init()
    {
        if (!_pathsSet)
        {
            string envPath = Environment.GetEnvironmentVariable("path");

            // Add Rhino WIP to the path. Let debug use the internal debug build of Rhino
            // for internal McNeel development. For people without access to internal builds,
            // use the Rhino WIP directory for both debug and release builds
#if DEBUG
            string rhinoSystemDir = @"C:\dev\github\mcneel\rhino\src4\bin\Debug";
            if(!System.IO.File.Exists(rhinoSystemDir))
            {
                string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                rhinoSystemDir = System.IO.Path.Combine(programFiles, "Rhino WIP", "System");
            }
#else
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string rhinoSystemDir = System.IO.Path.Combine(programFiles, "Rhino WIP", "System");
#endif
            Environment.SetEnvironmentVariable("path", envPath + ";" + rhinoSystemDir);
            _pathsSet = true;
            _rhpath = rhinoSystemDir;
        }
    }

    [DllImport("RhinoLibrary.dll")]
    internal static extern int LaunchInProcess(int reserved1, int reserved2);

    [DllImport("RhinoLibrary.dll")]
    internal static extern int ExitInProcess();
}
