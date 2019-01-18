using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
stopat = 0

def ComputeFetch(endpoint, arglist) :
    class __Rhino3dmEncoder(json.JSONEncoder):
        def default(self, o):
            if hasattr(o, ""Encode"") :
                return o.Encode()
            return json.JSONEncoder.default(self, o)
    global authToken
    global url
    global stopat
    posturl = url + endpoint
    if(stopat>0):
        if(posturl.find('?')>0): posturl += '&stopat='
        else: posturl += '?stopat='
        posturl += str(stopat)
    postdata = json.dumps(arglist, cls = __Rhino3dmEncoder)
    headers = {'Authorization': 'Bearer ' + authToken}
    r = requests.post(posturl, data=postdata, headers=headers)
    return r.json()

";

        static string _T(int amount)
        {
            string rc = "";
            for (int i = 0; i < amount; i++)
                rc += "    ";
            return rc;
        }

        static string DocCommentToPythonDoc(DocumentationCommentTriviaSyntax doccomment, MethodDeclarationSyntax method, int indentLevel)
        {
            // See https://sphinxcontrib-napoleon.readthedocs.io/en/latest/example_google.html
            // for docstring examples

            StringBuilder summary = new StringBuilder();
            StringBuilder args = new StringBuilder();
            StringBuilder returns = new StringBuilder();


            string comment = doccomment.ToString();
            comment = comment.Replace("///", "");
            comment = comment.Replace("\t", " ");
            comment = comment.Replace("null ", "None ");
            comment = comment.Replace("true ", "True ");
            comment = comment.Replace("false ", "False ");
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml("<doc>" + comment + "</doc>");
            var nodes = doc.FirstChild.ChildNodes;
            foreach (var node in nodes)
            {
                var element = node as System.Xml.XmlElement;
                string elementText = element.InnerText.Trim();
                if (string.IsNullOrWhiteSpace(elementText))
                    continue;
                string[] lines = elementText.Split(new char[] { '\n' });

                if (element.Name.Equals("summary", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var line in lines)
                        summary.AppendLine(_T(indentLevel) + line.Trim());
                }
                else if (element.Name.Equals("returns", StringComparison.OrdinalIgnoreCase))
                {
                    returns.AppendLine();
                    returns.AppendLine(_T(indentLevel) + "Returns:");
                    returns.Append(_T(indentLevel + 1) + $"{method.ReturnType}: ");
                    bool firstLine = true;
                    foreach (var line in lines)
                    {
                        if (!firstLine)
                            returns.Append(_T(indentLevel + 1));
                        firstLine = false;
                        returns.AppendLine(line.Trim());
                    }
                }
                else if (element.Name.Equals("param", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Length == 0)
                    {
                        args.AppendLine();
                        args.AppendLine(_T(indentLevel) + "Args:");
                    }
                    string parameterName = element.GetAttribute("name");

                    string paramType = "";
                    foreach(var param in method.ParameterList.Parameters)
                    {
                        if(param.Identifier.ToString().Equals(parameterName, StringComparison.Ordinal))
                        {
                            paramType = $" ({param.Type})";
                        }
                    }

                    bool added = false;
                    foreach (var line in lines)
                    {
                        if (!added)
                        {
                            added = true;
                            args.AppendLine(_T(indentLevel + 1) + parameterName + paramType + ": " + line.Trim());
                            continue;
                        }
                        args.AppendLine(_T(indentLevel + 2) + line.Trim());
                    }
                }
            }
            StringBuilder rc = new StringBuilder();
            rc.AppendLine(_T(indentLevel) + "\"\"\"");
            rc.Append(summary.ToString());
            rc.Append(args.ToString());
            rc.Append(returns.ToString());
            rc.AppendLine(_T(indentLevel) + "\"\"\"");
            return rc.ToString();
        }


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
                if (methodName.Equals("dispose", StringComparison.InvariantCultureIgnoreCase))
                    continue;
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
                sb.AppendLine(", multiple=False):");
                sb.Append(DocCommentToPythonDoc(comment, method, 1));
                sb.AppendLine($"{T1}url = \"{cb.EndPoint(method)}\"");
                sb.AppendLine($"{T1}if multiple: url += \"?multiple=true\"");
                var paramList = new StringBuilder();
                for (int i = 0; i < parameters.Count; i++)
                {
                    paramList.Append(parameters[i]);
                    if (i < (parameters.Count - 1))
                        paramList.Append(", ");
                }
                sb.AppendLine($"{T1}args = [{paramList.ToString()}]");
                if (parameters.Count == 1)
                    sb.AppendLine($"{T1}if multiple: args = [[item] for item in {parameters[0]}]");
                else
                    sb.AppendLine($"{T1}if multiple: args = zip({paramList.ToString()})");

                string endpoint = method.Identifier.ToString();
                sb.AppendLine($"{T1}response = Util.ComputeFetch(url, args)");
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
