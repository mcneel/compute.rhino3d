﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace computegen
{
    class ParameterInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public List<string> Description { get; } = new List<string>();
    }

    class ReturnInfo
    {
        public string Type { get; set; }
        public List<string> Description { get; } = new List<string>();
    }

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

            base.ReplacePackageVersion();
        }

        protected override string Prefix
        {
            get
            {
                return @"from . import Util

";
            }
        }

        readonly string UtilModuleContents =
$@"import rhino3dm
import json
import requests

__version__ = '{Version}'

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
    headers = {{
        'Authorization': 'Bearer ' + authToken,
        'User-Agent': 'compute.rhino3d.py/' + __version__
    }}
    r = requests.post(posturl, data=postdata, headers=headers)
    return r.json()

";

        public static int SpacesPerTab { get; set; } = 4;
        static string _T(int amount)
        {
            return "".PadLeft(amount*SpacesPerTab);
        }

        static bool IsOutParameter(ParameterSyntax parameter)
        {
            foreach (var modifier in parameter.Modifiers)
            {
                if (modifier.Text == "out")
                    return true;
            }
            return false;
        }


        static System.Xml.XmlDocument DocCommentToXml(DocumentationCommentTriviaSyntax doccomment)
        {
            string comment = doccomment.ToString();
            comment = comment.Replace("///", "");
            comment = comment.Replace("\t", " ");
            comment = comment.Replace("null ", "None ");
            comment = comment.Replace("true ", "True ");
            comment = comment.Replace("false ", "False ");
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml("<doc>" + comment + "</doc>");
            return doc;
        }

        public static string DocCommentToPythonDoc(
            DocumentationCommentTriviaSyntax doccomment, MethodDeclarationSyntax method,
            int indentLevel, out StringBuilder summary, out List<ParameterInfo> parameters, out ReturnInfo returnInfo)
        {
            // See https://sphinxcontrib-napoleon.readthedocs.io/en/latest/example_google.html
            // for docstring examples

            summary = new StringBuilder();
            StringBuilder args = new StringBuilder();
            StringBuilder returns = new StringBuilder();
            StringBuilder outArgs = new StringBuilder();
            parameters = new List<ParameterInfo>();
            returnInfo = new ReturnInfo() { Type = method.ReturnType.ToString() };

            var doc = DocCommentToXml(doccomment);
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
                        returnInfo.Description.Add(line.Trim());
                        if (!firstLine)
                            returns.Append(_T(indentLevel + 1));
                        firstLine = false;
                        returns.AppendLine(line.Trim());
                    }
                }
                else if (element.Name.Equals("param", StringComparison.OrdinalIgnoreCase))
                {
                    string parameterName = element.GetAttribute("name");
                    ParameterInfo pinfo = new ParameterInfo { Name = parameterName };
                    string paramType = "";
                    bool isOutParam = false;
                    foreach(var param in method.ParameterList.Parameters)
                    {
                        if(param.Identifier.ToString().Equals(parameterName, StringComparison.Ordinal))
                        {
                            isOutParam = IsOutParameter(param);
                            paramType = $" ({param.Type})";
                            pinfo.Type = param.Type.ToString();
                        }
                    }

                    if (args.Length == 0 && !isOutParam)
                    {
                        args.AppendLine();
                        args.AppendLine(_T(indentLevel) + "Args:");
                    }

                    bool added = false;
                    StringBuilder sb = isOutParam ? outArgs : args;
                    foreach (var line in lines)
                    {
                        pinfo.Description.Add(line.Trim());
                        if (!added)
                        {
                            added = true;
                            sb.AppendLine(_T(indentLevel + 1) + parameterName + paramType + ": " + line.Trim());
                            continue;
                        }
                        sb.AppendLine(_T(indentLevel + 2) + line.Trim());
                    }
                    if (!isOutParam)
                        parameters.Add(pinfo);
                }
            }

            StringBuilder rc = new StringBuilder();
            rc.AppendLine(_T(indentLevel) + "\"\"\"");
            rc.Append(summary.ToString());
            rc.Append(args.ToString());
            rc.Append(returns.ToString());
            rc.Append(outArgs.ToString());
            rc.AppendLine(_T(indentLevel) + "\"\"\"");
            return rc.ToString();
        }

        public static string GetMethodName(MethodDeclarationSyntax method, ClassBuilder c)
        {
            string methodName = method.Identifier.ToString();
            if (methodName.Equals("dispose", StringComparison.InvariantCultureIgnoreCase))
                return null;
            int overloadIndex = 0;
            foreach( var (m,dc) in c.Methods)
            {
                if (m == method)
                    break;
                if( methodName.Equals(m.Identifier.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    overloadIndex++;
                }
            }
            if (overloadIndex > 0)
                methodName = $"{methodName}{overloadIndex}";
            return methodName;
        }

        public static List<string> GetParameterNames(MethodDeclarationSyntax method, ClassBuilder cb)
        {
            List<string> parameters = new List<string>();
            if (!method.IsStatic())
            {
                parameters.Add("this" + cb.ClassName);
            }
            for (int i = 0; i < method.ParameterList.Parameters.Count; i++)
            {
                bool isOutParameter = false;
                foreach (var modifier in method.ParameterList.Parameters[i].Modifiers)
                {
                    if (modifier.Text == "out")
                        isOutParameter = true;
                }

                if (!isOutParameter)
                    parameters.Add(method.ParameterList.Parameters[i].Identifier.ToString());
            }
            return parameters;
        }

        public static string ToPythonType(string type)
        {
            bool isArray = type.EndsWith("[]");
            if (isArray)
                return ToPythonType(type.Substring(0, type.Length - 2)) + "[]";

            if (type.Equals("double"))
                return "float";
            if (type.Equals($"IEnumerable<double>"))
                return "list[float]";
            if (type.Equals($"IEnumerable<int>"))
                return "list[int]";

            if (type.Equals("string", StringComparison.InvariantCultureIgnoreCase))
                return "str";
            string[] rhino3dm = { "BezierCurve", "BoundingBox", "Box", "Brep", "BrepEdge", "BrepFace", "Curve",
                "GeometryBase", "Interval", "Mesh", "MeshingParameters", "NurbsCurve", "Plane", "Point2d", "Point3d",
                "Polyline", "Sphere", "Surface", "Vector3d" };
            foreach(var item in rhino3dm)
            {
                if (type.Equals(item))
                    return "rhino3dm." + type;
                if (type.Equals($"IEnumerable<{item}>"))
                {
                    int startIndex = "IEnumerable<".Length;
                    int endIndex = type.IndexOf('>');
                    return "list[rhino3dm." + type.Substring(startIndex, endIndex-startIndex) + "]";
                }
            }
            return type;
        }

        protected override string ToComputeClient(ClassBuilder cb)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            int iMethod = 0;

            foreach (var (method, comment) in cb.Methods)
            {
                string methodName = GetMethodName(method, cb);
                if( string.IsNullOrWhiteSpace(methodName))
                    continue;
                sb.Append($"def {methodName}(");
                List<string> parameters = GetParameterNames(method, cb);
                for (int i = 0; i < parameters.Count; i++)
                {
                    sb.Append(parameters[i] + ", ");
                }
                sb.AppendLine("multiple=False):");
                StringBuilder summary;
                List<ParameterInfo> parameterList;
                ReturnInfo returnInfo;
                sb.Append(DocCommentToPythonDoc(comment, method, 1, out summary, out parameterList, out returnInfo));
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

        protected override string PackageFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "dist", "pypi", "setup.py");
        protected override string PackageFileRegex => @"(?<=version="")[0-9\.]*(?="")";
    }
}
