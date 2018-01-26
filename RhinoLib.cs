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
      // TODO: Use registry to find RhinoInProcess.dll
      //        Microsoft.Win32.Registry.LocalMachine.OpenSubKey
      string envPath = Environment.GetEnvironmentVariable("path");
#if DEBUG
      string rhinoSystemDir = @"C:\dev\github\mcneel\rhino\src4\bin\Debug";
#else
      string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
      string rhinoSystemDir = System.IO.Path.Combine(programFiles, "Rhino 6", "System");
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
