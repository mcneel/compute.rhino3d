using System;
using System.IO;

namespace computegen
{
    class Program
    {
        static void Main(string[] args)
        {
            const string rhinocommonPath = @"C:\dev\github\mcneel\rhino\src4\DotNetSDK\rhinocommon\dotnet";
            Console.WriteLine("[BEGIN PARSE]");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            ClassBuilder.BuildClassDictionary(rhinocommonPath);
            Console.ResetColor();
            Console.WriteLine("[END PARSE]");

            string[] filter = new string[] {
                ".AreaMassProperties",
                ".BezierCurve",
                ".Brep", ".BrepFace",
                ".Curve", ".Extrusion", ".Intersection", ".Mesh",
                ".NurbsCurve", ".NurbsSurface", ".SubD", ".Surface",
                ".VolumeMassProperties"
            };

            var di = SharedRepoDirectory();

            Console.ForegroundColor = ConsoleColor.Blue;
            var classes = ClassBuilder.FilteredList(ClassBuilder.AllClasses, filter);

            // Javascript
            Console.WriteLine("Writing javascript client");
            var js = new JavascriptClient();
            string javascriptPath = "compute.rhino3d.js";
            if( di!=null)
            {
                string dir = Path.Combine(di.FullName, "computeclient_js");
                if (Directory.Exists(dir))
                    javascriptPath = Path.Combine(dir, javascriptPath);
            }
            js.Write(ClassBuilder.AllClasses, javascriptPath, filter);
            DirectoryInfo jsdocDirectory = new DirectoryInfo("docs\\javascript");
            if( di!=null )
            {
                string dir = Path.Combine(di.FullName, "computeclient_js", "docs");
                if (Directory.Exists(dir))
                    jsdocDirectory = new DirectoryInfo(dir);
            }
            Console.WriteLine("Writing javascript docs");
            RstClient.WriteJavascriptDocs(classes, jsdocDirectory);

            // Python
            Console.WriteLine("Writing python client");
            string basePythonDirectory = "";
            if(di != null)
            {
                string dir = Path.Combine(di.FullName, "computeclient_py");
                if (Directory.Exists(dir))
                    basePythonDirectory = dir;
            }
            var py = new PythonClient();
            py.Write(ClassBuilder.AllClasses, basePythonDirectory, filter);
            Console.WriteLine("Writing python docs");
            RstClient.WritePythonDocs(basePythonDirectory, classes);

            // C#
            Console.WriteLine("Writing C# client");
            var cs = new DotNetClient();
            cs.Write(ClassBuilder.AllClasses, "RhinoCompute.cs", filter);


            Console.ResetColor();
        }

        static DirectoryInfo SharedRepoDirectory()
        {
            string exeDirectory = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            var di = new DirectoryInfo(exeDirectory);
            while(di != null)
            {
                if (di.Name.Equals("compute.rhino3d"))
                    return di.Parent;
                di = di.Parent;
            }
            return di;
        }
    }
}
