using System;
using System.Collections.Generic;
using System.Text;

namespace computegen
{
    class PythonClient : ComputeClient
    {
        public override void Write(Dictionary<string, ClassBuilder> classes, string path, string[] filter)
        {
            var packageBase = System.IO.Path.Combine(path, "compute_rhino3d");
            System.IO.Directory.CreateDirectory(packageBase);
            var initpy = System.IO.Path.Combine(packageBase, "__init__.py");
            System.IO.File.WriteAllText(initpy, "");

            var utilPath = System.IO.Path.Combine(packageBase, "Util.py");
            System.IO.File.WriteAllText(utilPath, UtilModuleContents);

            foreach (var kv in ClassBuilder.AllClasses)
            {
                if (kv.Key.StartsWith("Rhino.Geometry."))
                {
                    bool skip = true;
                    foreach (var f in filter)
                    {
                        if (kv.Key.EndsWith(f))
                            skip = false;
                    }
                    if (skip)
                        continue;
                    var modulePath = System.IO.Path.Combine(packageBase, $"{kv.Value.ClassName}.py");
                    StringBuilder clientText = new StringBuilder();
                    clientText.Append(Prefix);
                    clientText.Append(ToComputeClient(kv.Value));
                    System.IO.File.WriteAllText(modulePath, clientText.ToString());
                }
            }

        }

        protected override string Prefix
        {
            get
            {
                return @"from . import Util

";
            }
        }

        const string UtilModuleContents =
@"import rhino3dm
import json
import requests

url = ""https://compute.rhino3d.com/""
authToken = None

def ComputeFetch(endpoint, arglist) :
    class __Rhino3dmEncoder(json.JSONEncoder):
        def default(self, o):
            if isinstance(o, rhino3dm.CommonObject) :
                return o.Encode()
            return json.JSONEncoder.default(self, o)
    global authToken
    postdata = json.dumps(arglist, cls = __Rhino3dmEncoder)
    headers = {'Authorization': 'Bearer ' + authToken}
    r = requests.post(url+endpoint, data=postdata, headers=headers)
    return r.json()

";

        protected override string ToComputeClient(ClassBuilder cb)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            int iMethod = 0;
            int overloadIndex = 0;
            string prevMethodName = "";
            foreach (var (method, comment) in cb.Methods)
            {
                string methodName = method.Identifier.ToString();
                if (methodName.Equals(prevMethodName))
                {
                    overloadIndex++;
                    methodName = $"{methodName}{overloadIndex}";
                }
                else
                {
                    overloadIndex = 0;
                    prevMethodName = methodName;
                }
                sb.Append($"def {methodName}(");
                List<string> parameters = new List<string>();
                if (!method.IsStatic())
                {
                    parameters.Add("this" + cb.ClassName);
                }
                for (int i = 0; i < method.ParameterList.Parameters.Count; i++)
                {
                    parameters.Add(method.ParameterList.Parameters[i].Identifier.ToString());
                }

                for (int i = 0; i < parameters.Count; i++)
                {
                    sb.Append(parameters[i]);
                    if (i < (parameters.Count - 1))
                        sb.Append(", ");
                }
                sb.AppendLine("):");
                sb.Append($"{T1}args = [");
                for (int i = 0; i < parameters.Count; i++)
                {
                    sb.Append(parameters[i]);
                    if (i < (parameters.Count - 1))
                        sb.Append(", ");
                }
                sb.AppendLine("]");
                string endpoint = method.Identifier.ToString();
                sb.AppendLine($"{T1}response = Util.ComputeFetch(\"{cb.EndPoint(method)}\", args)");
                sb.AppendLine($"{T1}return response");
                sb.AppendLine();

                iMethod++;
                if (iMethod < cb.Methods.Count)
                    sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
