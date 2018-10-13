using System;

namespace Rhino
{
    public static class Python
    {
        public static object[] Evaluate(string script, string[] variables)
        {
            if (script.IndexOf("import") >= 0)
                throw new ArgumentException("import not allowed");
            var pythonscript = Runtime.PythonScript.Create();
            script = "from Rhino.Geometry import *\n" + script;
            pythonscript.ExecuteScript(script);
            object[] rc = new object[variables.Length];
            for (int i = 0; i < variables.Length; i++)
                rc[i] = pythonscript.GetVariable(variables[i]);
            return rc;
        }
    }
}
