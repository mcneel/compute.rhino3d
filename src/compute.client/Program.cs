using System;
using System.Text;

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

            JavascriptClient.Write(ClassBuilder.AllClasses, "rhinocompute.js");
        }

    }
}
