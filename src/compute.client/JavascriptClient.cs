using System;
using System.Collections.Generic;
using System.Text;

namespace computegen
{
    class JavascriptClient : ComputeClient
    {
        protected override string Prefix
        {
            get
            {
                return 
@"var RhinoCompute = {
    url: ""https://compute.rhino3d.com/"",

    authToken: null,

    getAuthToken: function(useLocalStorage=true) {
        var auth = null;
        if( useLocalStorage )
            auth = localStorage[""compute_auth""];
        if (auth == null) {
            auth = window.prompt(""Rhino Accounts auth token"");
            if (auth != null && auth.length>20) {
                auth = ""Bearer "" + auth;
                localStorage.setItem(""compute_auth"", auth);
            }
        }
        return auth;
    },

    computeFetch: function(endpoint, arglist) {
        for (i = 0; i < arglist.length; i++) {
            if (arglist[i].encode != null)
                arglist[i] = arglist[i].encode();
        }
        return fetch(RhinoCompute.url+endpoint, {
                ""method"":""POST"",
                ""body"": JSON.stringify(arglist),
                ""headers"": {""Authorization"":RhinoCompute.authToken}
        }).then(r=>r.json());
    },
";
            }
        }

        protected override string Suffix
        {
            get { return "};"; }
        }

        protected override string ToComputeClient(ClassBuilder cb)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine($"{T1}{cb.ClassName} : {{");
            int iMethod = 0;
            int overloadIndex = 0;
            string prevMethodName = "";
            foreach (var (method, comment) in cb.Methods)
            {
                string methodName = CamelCase(method.Identifier.ToString());
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
                sb.Append($"{T2}{methodName} : function(");
                List<string> parameters = new List<string>();
                if (!method.IsStatic())
                {
                    parameters.Add(cb.ClassName.ToLower());
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
                sb.AppendLine(") {");
                sb.Append($"{T3}args = [");
                for (int i = 0; i < parameters.Count; i++)
                {
                    sb.Append(parameters[i]);
                    if (i < (parameters.Count - 1))
                        sb.Append(", ");
                }
                sb.AppendLine("];");
                string endpoint = method.Identifier.ToString();
                sb.AppendLine($"{T3}var promise = RhinoCompute.computeFetch(\"{cb.EndPoint(method)}\", args);");
                sb.AppendLine($"{T3}return promise;");
                sb.AppendLine($"{T3}}},");

                iMethod++;
                if (iMethod < cb.Methods.Count)
                    sb.AppendLine();
            }
            sb.AppendLine($"{T1}}},");
            return sb.ToString();
        }

    }
}
