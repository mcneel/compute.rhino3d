using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace computegen
{
    class DotNetClient : ComputeClient
    {
        protected override string ToComputeClient(ClassBuilder cb)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine($"{T1}public static class {cb.ClassName}Compute");
            sb.AppendLine($"{T1}{{");
            sb.AppendLine($"{T2}static string ApiAddress([CallerMemberName] string caller = null)");
            sb.AppendLine($"{T2}{{");
            sb.AppendLine($"{T3}return ComputeServer.ApiAddress(typeof({cb.ClassName}), caller);");
            sb.AppendLine($"{T2}}}");


            foreach (var (method, comment) in cb.Methods)
            {
                if (comment != null)
                {
                    string formattedComment = comment.ToFullString().Replace(T1, T2);
                    sb.Append(T2 + formattedComment);
                }
                bool useAsReturnType;
                if (method.IsNonConst(out useAsReturnType) && useAsReturnType)
                    sb.Append($"{T2}public static {cb.ClassName} {method.Identifier}(");
                else
                    sb.Append($"{T2}public static {method.ReturnType} {method.Identifier}(");

                int paramCount = 0;
                if (!method.IsStatic())
                {
                    sb.Append($"this {cb.ClassName} {cb.ClassName.ToLower()}");
                    paramCount++;
                }
                if (method.IsNonConst(out useAsReturnType) && !useAsReturnType)
                {
                    if (paramCount > 0)
                        sb.Append(", ");
                    sb.Append($"out {cb.ClassName} updatedInstance");
                    paramCount++;
                }
                for (int i = 0; i < method.ParameterList.Parameters.Count; i++)
                {
                    if (paramCount > 0)
                        sb.Append(", ");
                    sb.Append($"{method.ParameterList.Parameters[i].ToFullString()}");
                    paramCount++;
                }
                sb.AppendLine(")");
                sb.AppendLine($"{T2}{{");

                int outParamIndex = -1;
                for (int i = 0; i < method.ParameterList.Parameters.Count; i++)
                {
                    foreach (var modifier in method.ParameterList.Parameters[i].Modifiers)
                    {
                        if (modifier.Text == "out")
                        {
                            outParamIndex = i;
                            break;
                        }
                    }
                    if (outParamIndex >= 0)
                        break;
                }

                if (outParamIndex < 0)
                {
                    if (method.IsNonConst(out useAsReturnType))
                    {
                        if (useAsReturnType)
                            sb.Append($"{T3}return ComputeServer.Post<{cb.ClassName}>(ApiAddress(), ");
                        else
                            sb.Append($"{T3}return ComputeServer.Post<{method.ReturnType}, {cb.ClassName}>(ApiAddress(), out updatedInstance, ");
                    }
                    else
                        sb.Append($"{T3}return ComputeServer.Post<{method.ReturnType}>(ApiAddress(), ");
                }
                else
                {
                    var parameter = method.ParameterList.Parameters[outParamIndex];
                    sb.Append($"{T3}return ComputeServer.Post<{method.ReturnType}, {parameter.Type}>(ApiAddress(), out {parameter.Identifier}, ");
                }
                if (!method.IsStatic())
                {
                    sb.Append($"{cb.ClassName.ToLower()}");
                    if (method.ParameterList.Parameters.Count > 0)
                        sb.Append(", ");
                }

                var orderedParams = new List<ParameterSyntax>();
                foreach (var p in method.ParameterList.Parameters)
                {
                    if (p.Modifiers.Count == 0)
                        orderedParams.Add(p);
                }

                for (int i = 0; i < orderedParams.Count; i++)
                {
                    if (i > 0)
                        sb.Append(", ");
                    var p = orderedParams[i];
                    sb.Append(p.Modifiers.Count > 0 ? $"{p.Modifiers} {p.Identifier}" : $"{p.Identifier}");
                }
                sb.AppendLine(");");
                sb.AppendLine($"{T2}}}");
            }
            sb.AppendLine($"{T1}}}");
            return sb.ToString();
        }

        protected override string Prefix
        {
            get
            {
                return
@"using System;
using System.IO;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Rhino.Compute
{
    public static class ComputeServer
    {
        public static string WebAddress { get; set; } = ""https://compute.rhino3d.com"";
        public static string AuthToken { get; set; }

        public static T Post<T>(string function, params object[] postData)
        {
            if (string.IsNullOrWhiteSpace(AuthToken))
                throw new UnauthorizedAccessException(""AuthToken must be set"");
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(postData);
            if (!function.StartsWith(""/""))
                function = ""/"" + function;
            string uri = (WebAddress + function).ToLower();
            var request = System.Net.WebRequest.Create(uri);
            request.ContentType = ""application/json"";
            request.Headers.Add(""Authorization"", ""Bearer "" + AuthToken);
            request.Method = ""POST"";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
            }

            var response = request.GetResponse();
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                var rc = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(result);
                return rc;
            }
        }

        public static T0 Post<T0, T1>(string function, out T1 out1, params object[] postData)
        {
            if (string.IsNullOrWhiteSpace(AuthToken))
                throw new UnauthorizedAccessException(""AuthToken must be set"");
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(postData);
            if (!function.StartsWith(""/""))
                function = ""/"" + function;
            string uri = (WebAddress + function).ToLower();
            var request = System.Net.WebRequest.Create(uri);
            request.ContentType = ""application/json"";
            request.Headers.Add(""Authorization"", ""Bearer "" + AuthToken);
            request.Method = ""POST"";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
            }

            var response = request.GetResponse();
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var jsonString = streamReader.ReadToEnd();
                object data = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);
                var ja = data as Newtonsoft.Json.Linq.JArray;
                out1 = ja[1].ToObject<T1>();
                return ja[0].ToObject<T0>();
            }
        }

        public static string ApiAddress(Type t, string function)
        {
            string s = t.ToString().Replace('.', '/');
            return s + ""/"" + function;
        }
    }
";
            }
        }
    }
}
