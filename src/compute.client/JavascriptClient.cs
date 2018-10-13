using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace computegen
{
    static class JavascriptClient
    {
        public static void Write(Dictionary<string,ClassBuilder> classes, string path)
        {
            StringBuilder javascript = new StringBuilder();
            javascript.Append(prefix);

            // just do mesh for now
            string[] filter = new string[] { ".Mesh", ".Brep", ".Curve" };
            foreach (var kv in ClassBuilder.AllClasses)
            {
                if (kv.Key.StartsWith("Rhino.Geometry."))
                {
                    bool skip = true;
                    foreach(var f in filter)
                    {
                        if (kv.Key.EndsWith(f))
                            skip = false;
                    }
                    if (skip)
                        continue;
                    javascript.Append(kv.Value.ToComputeJavascript());
                }
            }
            javascript.AppendLine("}");

            System.IO.File.WriteAllText(path, javascript.ToString());
        }

        static string prefix =
@"
var RhinoCompute = {
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
