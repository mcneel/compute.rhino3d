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
@"var RhinoCompute = {
    version: """ + Version + @""",
    url: ""https://compute.rhino3d.com/"",
    authToken: null,

    getAuthToken: function(useLocalStorage=true) {
        let auth = null;
        if (useLocalStorage)
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
@"    Python: {
        pythonEvaluate : function(script, input, output){
            let inputEncoded = rhino3dm.ArchivableDictionary.encodeDict(input);
            let url = 'rhino/python/evaluate';
            let args = [script, JSON.stringify(inputEncoded), output];
            let result = RhinoCompute.computeFetch(url, args);
            let objects = rhino3dm.ArchivableDictionary.decodeDict(JSON.parse(result));
            return objects;
        }
    },
    Grasshopper: {
        DataTree: class {
            constructor(name) {
                this.data = { 'ParamName': name, 'InnerTree': {} }
            }

            append(path, items) {
                /**
                 * Append a path to this tree
                 * @param path (arr): a list of integers defining a path
                 * @param items (arr): list of data to add to the tree
                 */
                let key = path.join(';')
                let innerTreeData = []
                items.forEach(item => {
                    innerTreeData.push({ 'data': item })
                })
                this.data.InnerTree[key] = innerTreeData
            }
        },
        evaluateDefinition : function(definition, trees) {
            /**
             * Evaluate a grasshopper definition
             * @param definition (str|bytearray) contents of .gh/.ghx file or
             *   url pointing to a grasshopper definition file
             * @param trees (arr) list of DataTree instances
             */

            let url = 'grasshopper';
            let args = { 'algo': null, 'pointer': null, 'values': null };
            if (definition.constructor === Uint8Array)
                args['algo'] = base64ByteArray(definition)
            else {
                if (definition.startsWith('http')) {
                    args['pointer'] = definition;
                } else {
                    args['algo'] = btoa(definition);
                }
            }

            let values = [];
            trees.forEach(tree => {
                values.push(tree.data);
            });
            args['values'] = values;

            let promise = RhinoCompute.computeFetch(url, args);
            return promise;
        }
    }
};

// https://gist.github.com/jonleighton/958841
/*
MIT LICENSE
Copyright 2011 Jon Leighton
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the ""Software""), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
function base64ByteArray(bytes) {
    var base64    = ''
    var encodings = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/'

    // var bytes         = new Uint8Array(arrayBuffer)

    // strip bom
    if (bytes[0] === 239 && bytes[1] === 187 && bytes[2] === 191)
        bytes = bytes.slice(3)

    var byteLength    = bytes.byteLength
    var byteRemainder = byteLength % 3
    var mainLength    = byteLength - byteRemainder

    var a, b, c, d
    var chunk

    // Main loop deals with bytes in chunks of 3
    for (var i = 0; i < mainLength; i = i + 3) {
        // Combine the three bytes into a single integer
        chunk = (bytes[i] << 16) | (bytes[i + 1] << 8) | bytes[i + 2]

        // Use bitmasks to extract 6-bit segments from the triplet
        a = (chunk & 16515072) >> 18 // 16515072 = (2^6 - 1) << 18
        b = (chunk & 258048)   >> 12 // 258048   = (2^6 - 1) << 12
        c = (chunk & 4032)     >>  6 // 4032     = (2^6 - 1) << 6
        d = chunk & 63               // 63       = 2^6 - 1

        // Convert the raw binary segments to the appropriate ASCII encoding
        base64 += encodings[a] + encodings[b] + encodings[c] + encodings[d]
    }

    // Deal with the remaining bytes and padding
    if (byteRemainder == 1) {
        chunk = bytes[mainLength]

        a = (chunk & 252) >> 2 // 252 = (2^6 - 1) << 2

        // Set the 4 least significant bits to zero
        b = (chunk & 3)   << 4 // 3   = 2^2 - 1

        base64 += encodings[a] + encodings[b] + '=='
    } else if (byteRemainder == 2) {
        chunk = (bytes[mainLength] << 8) | bytes[mainLength + 1]

        a = (chunk & 64512) >> 10 // 64512 = (2^6 - 1) << 10
        b = (chunk & 1008)  >>  4 // 1008  = (2^6 - 1) << 4

        // Set the 2 least significant bits to zero
        c = (chunk & 15)    <<  2 // 15    = 2^4 - 1

        base64 += encodings[a] + encodings[b] + encodings[c] + '='
    }

    return base64
}

// NODE.JS

// check if we're running in a browser or in node.js
let _is_node = typeof exports === 'object' && typeof module === 'object'

// polyfills
if (_is_node && typeof require === 'function')
{
    if (typeof fetch !== 'function')
        fetch = require('node-fetch')
}

// export RhinoCompute object
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
                if (!method.IsStatic())
                {
                    string paramName = cb.ClassName.ToLower();
                    // make sure this paramName is not already in the parameter list
                    for(int i=0; i<parameters.Count; i++)
                    {
                        if( paramName.Equals(parameters[i],StringComparison.OrdinalIgnoreCase) )
                        {
                            paramName = "_" + paramName;
                            break;
                        }
                    }
                    parameters.Insert(0, paramName);
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
