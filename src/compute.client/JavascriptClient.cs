using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace computegen
{
    class JavascriptClient : ComputeClient
    {
        protected override string Prefix
        {
            get
            {
                return
@"// check if we're running in a browser or in node.js
let _is_node = typeof exports === 'object' && typeof module === 'object'

// (node.js) polyfill fetch
if (_is_node && typeof fetch !== 'function' && typeof require === 'function')
    fetch = require('node-fetch')

var RhinoCompute = {
    version: """ + Version + @""",

    url: ""https://compute.rhino3d.com/"",

    authToken: null,

    getAuthToken: function(useLocalStorage=true) {
        var auth = null;
        if( useLocalStorage )
            auth = localStorage[""compute_auth""];
        if (auth == null) {
            auth = window.prompt(""Rhino Accounts auth token\nVisit https://www.rhino3d.com/compute/login"");
            if (auth != null && auth.length>20) {
                auth = ""Bearer "" + auth;
                localStorage.setItem(""compute_auth"", auth);
            }
        }
        return auth;
    },

    computeFetch: function(endpoint, arglist) {
        return fetch(RhinoCompute.url+endpoint, {
                ""method"":""POST"",
                ""body"": JSON.stringify(arglist),
                ""headers"": {
                    ""Authorization"": RhinoCompute.authToken,
                    ""User-Agent"": `compute.rhino3d.js/${RhinoCompute.version}`
                },
        }).then(r=>r.json());
    },

    zipArgs: function(multiple, ...args) {
        if(!multiple)
            return args;

        if(args.length==1)
            return args[0].map(function(_,i) { return [args[0][i]]; });
        if(args.length==2)
            return args[0].map(function(_,i) {
                return [args[0][i],args[1][i]]; }
            );
        if(args.length==3)
            return args[0].map(function(_,i) {
                return [args[0][i],args[1][i],args[2][i]]; }
            );
        if(args.length==4)
            return args[0].map(function(_,i) {
                return [args[0][i],args[1][i],args[2][i],args[3][i]]; }
            );
        if(args.length==5)
            return args[0].map(function(_,i) {
                return [args[0][i],args[1][i],args[2][i],args[3][i],args[4][i]]; }
            );
        if(args.length==6)
            return args[0].map(function(_,i) {
                return [args[0][i],args[1][i],args[2][i],args[3][i],args[4][i],args[5][i]]; }
            );
        if(args.length==7)
            return args[0].map(function(_,i) {
                return [args[0][i],args[1][i],args[2][i],args[3][i],args[4][i],args[5][i],args[6][i]]; }
            );
        return args[0].map(function(_,i) {
            return [args[0][i],args[1][i],args[2][i],args[3][i],args[4][i],args[5][i],args[6][i],args[7][i]]; }
        );
    },
";
            }
        }

        protected override string Suffix
        {
            get
            {
              return
@"};

// (node.js) export RhinoCompute object
if (_is_node)
    module.exports = RhinoCompute;";
            }
        }
        public static string GetMethodName(MethodDeclarationSyntax method, ClassBuilder c)
        {
            return CamelCase(PythonClient.GetMethodName(method, c));
        }


        protected override string ToComputeClient(ClassBuilder cb)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine($"{T1}{cb.ClassName} : {{");
            int iMethod = 0;
            foreach (var (method, comment) in cb.Methods)
            {
                string methodName = GetMethodName(method, cb);
                if (string.IsNullOrWhiteSpace(methodName))
                    continue;
                sb.Append($"{T2}{methodName} : function(");
                List<string> parameters = new List<string>();
                if (!method.IsStatic())
                {
                    parameters.Add(cb.ClassName.ToLower());
                }
                for (int i = 0; i < method.ParameterList.Parameters.Count; i++)
                {
                    bool isOutParamter = false;
                    foreach (var modifier in method.ParameterList.Parameters[i].Modifiers)
                    {
                        if (modifier.Text == "out")
                        {
                            isOutParamter = true;
                        }
                    }
                    if(!isOutParamter)
                        parameters.Add(method.ParameterList.Parameters[i].Identifier.ToString());
                }

                for (int i = 0; i < parameters.Count; i++)
                {
                    sb.Append(parameters[i]);
                    if (i < (parameters.Count - 1))
                        sb.Append(", ");
                }
                sb.AppendLine(", multiple=false) {");
                sb.AppendLine($"{T3}let url=\"{cb.EndPoint(method)}\";");
                sb.AppendLine($"{T3}if(multiple) url = url + \"?multiple=true\"");
                sb.Append($"{T3}let args = RhinoCompute.zipArgs(multiple, ");
                for (int i = 0; i < parameters.Count; i++)
                {
                    sb.Append(parameters[i]);
                    if (i < (parameters.Count - 1))
                        sb.Append(", ");
                }
                sb.AppendLine(");");
                string endpoint = method.Identifier.ToString();
                sb.AppendLine($"{T3}var promise = RhinoCompute.computeFetch(url, args);");
                sb.AppendLine($"{T3}return promise;");
                sb.AppendLine($"{T2}}},");

                iMethod++;
                if (iMethod < cb.Methods.Count)
                    sb.AppendLine();
            }
            sb.AppendLine($"{T1}}},");
            return sb.ToString();
        }

    }
}
