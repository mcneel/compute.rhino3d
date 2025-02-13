using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace computegen
{
    class DotNetClient : ComputeClient
    {
        protected override string ToComputeClient(ClassBuilder cb)
        {
            //skipping these classes as they are not currently exposed in Rhino3dm
            if (cb.ClassName.ToLower().Contains("areamassproperties") || cb.ClassName.ToLower().Contains("volumemassproperties"))
                return string.Empty;

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
                bool skipMethod = false;
                string[] missingTypes = new string[] { 
                    "CurvatureAnalysisSettingsState", 
                    "BrepRegion", 
                    "CurveBooleanRegions",
                    "MeshPipeCapStyle",
                    "RhinoObject",
                    "TextLog",
                    "RhinoViewport",
                    "ViewportInfo",
                    "MeshUnsafeLock",
                    "ShutLiningCurveInfo",
                    "MeshDisplacementInfo",
                    "CurveIntersections",
                    "MeshIntersectionCache",
                    "FilletSurfaceSplitType",
                    "RayShootEvent",
                    "Color"};
                foreach (var parameter in method.ParameterList.Parameters)
                {
                    if (parameter.Type.ToString() == "Commands.Result" || parameter.Type.ToString() == "Result" ||
                        missingTypes.Any(type => parameter.ToString().Contains(type)) ||
                        (IsAsync && parameter.Modifiers.Any(modifier => modifier.Text == "out")))
                    {
                        skipMethod = true;
                        break;
                    }
                }

                if(!skipMethod && missingTypes.Any(type => method.ReturnType.ToString().Contains(type)))
                {
                    skipMethod = true;
                }

                bool useAsReturnType = false;
                if (!skipMethod && method.IsNonConst(out useAsReturnType) && !useAsReturnType)
                {
                    skipMethod = true;
                }

                if (skipMethod)
                continue;
        
                sb.AppendLine();
                sb.Append(IsAsync ? WriteMethodAsync(cb, method, comment, false) : WriteMethod(cb, method, comment, false));
                string remoteVersion = IsAsync ? WriteMethodAsync(cb, method, comment, true) : WriteMethod(cb, method, comment, true);
                if( !string.IsNullOrWhiteSpace(remoteVersion) )
                {
                    sb.AppendLine();
                    sb.Append(remoteVersion);
                }
            }
            sb.AppendLine($"{T1}}}");
            return sb.ToString();
        }

        string WriteMethod(ClassBuilder cb, MethodDeclarationSyntax method, DocumentationCommentTriviaSyntax comment, bool useRemotes)
        {
            int remoteCount = 0;
            StringBuilder sb = new StringBuilder();
            if (comment != null)
            {
                string formattedComment = T2 + comment.ToFullString().Replace(T1, T2);
                foreach (var line in formattedComment.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
                {
                    if (!line.Contains("<since>") && !string.IsNullOrWhiteSpace(line))
                        sb.AppendLine(line);
                }
            }
            bool useAsReturnType;
            if (method.IsNonConst(out useAsReturnType) && useAsReturnType)
                sb.Append($"{T2}public static {cb.ClassName} {method.Identifier}(");
            else
                sb.Append($"{T2}public static {method.ReturnType} {method.Identifier}(");

            int paramCount = 0;
            if (!method.IsStatic())
            {
                if (useRemotes && IsRemoteCandidate(cb.ClassName))
                {
                    sb.Append($"Remote<{cb.ClassName}> {cb.ClassName.ToLower()}");
                    remoteCount++;
                }
                else
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
                var parameter = method.ParameterList.Parameters[i];
                if (useRemotes && IsRemoteCandidate(parameter.Type.ToString()))
                {
                    sb.Append($"Remote<{parameter.Type}> {parameter.Identifier}");
                    remoteCount++;
                }
                else
                    sb.Append($"{parameter.ToFullString().Trim()}");
                paramCount++;
            }
            sb.AppendLine(")");
            sb.AppendLine($"{T2}{{");

            List<int> outParamIndices = new List<int>();
            for (int i = 0; i < method.ParameterList.Parameters.Count; i++)
            {
                foreach (var modifier in method.ParameterList.Parameters[i].Modifiers)
                {
                    if (modifier.Text == "out")
                    {
                        outParamIndices.Add(i);
                    }
                }
            }

            if (outParamIndices.Count == 0)
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
                var parameter0 = method.ParameterList.Parameters[outParamIndices[0]];
                if (outParamIndices.Count == 1)
                    sb.Append($"{T3}return ComputeServer.Post<{method.ReturnType}, {parameter0.Type}>(ApiAddress(), out {parameter0.Identifier}, ");
                else
                {
                    var parameter1 = method.ParameterList.Parameters[outParamIndices[1]];
                    sb.Append($"{T3}return ComputeServer.Post<{method.ReturnType}, {parameter0.Type}, {parameter1.Type}>(ApiAddress(), out {parameter0.Identifier}, out {parameter1.Identifier}, ");
                }
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
            if (remoteCount == 0 && useRemotes)
                return "";
            return sb.ToString();
        }

        string WriteMethodAsync(ClassBuilder cb, MethodDeclarationSyntax method, DocumentationCommentTriviaSyntax comment, bool useRemotes)
        {
            int remoteCount = 0;
            StringBuilder sb = new StringBuilder();
            if (comment != null)
            {
                string formattedComment = T2 + comment.ToFullString().Replace(T1, T2);
                foreach (var line in formattedComment.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
                {
                    if (!line.Contains("<since>") && !string.IsNullOrWhiteSpace(line))
                        sb.AppendLine(line);
                }
            }
            bool useAsReturnType;
            if (method.IsNonConst(out useAsReturnType) && useAsReturnType)
                sb.Append($"{T2}public static async Task<{cb.ClassName}> {method.Identifier}(");
            else
                sb.Append($"{T2}public static async Task<{method.ReturnType}> {method.Identifier}(");

            int paramCount = 0;
            if (!method.IsStatic())
            {
                if (useRemotes && IsRemoteCandidate(cb.ClassName))
                {
                    sb.Append($"Remote<{cb.ClassName}> {cb.ClassName.ToLower()}");
                    remoteCount++;
                }
                else
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
                var parameter = method.ParameterList.Parameters[i];
                if (useRemotes && IsRemoteCandidate(parameter.Type.ToString()))
                {
                    sb.Append($"Remote<{parameter.Type}> {parameter.Identifier}");
                    remoteCount++;
                }
                else
                    sb.Append($"{parameter.ToFullString().Trim()}");
                paramCount++;
            }
            sb.AppendLine(")");
            sb.AppendLine($"{T2}{{");

            List<int> outParamIndices = new List<int>();
            for (int i = 0; i < method.ParameterList.Parameters.Count; i++)
            {
                foreach (var modifier in method.ParameterList.Parameters[i].Modifiers)
                {
                    if (modifier.Text == "out")
                    {
                        outParamIndices.Add(i);
                    }
                }
            }

            if (outParamIndices.Count == 0)
            {
                if (method.IsNonConst(out useAsReturnType))
                {
                    if (useAsReturnType)
                        sb.Append($"{T3}return await ComputeServer.PostAsync<{cb.ClassName}>(ApiAddress(), ");
                    else
                        sb.Append($"{T3}return await ComputeServer.PostAsync<{method.ReturnType}, {cb.ClassName}>(ApiAddress(), out updatedInstance, ");
                }
                else
                    sb.Append($"{T3}return await ComputeServer.PostAsync<{method.ReturnType}>(ApiAddress(), ");
            }
            else
            {
                var parameter0 = method.ParameterList.Parameters[outParamIndices[0]];
                if (outParamIndices.Count == 1)
                    sb.Append($"{T3}return await ComputeServer.PostAsync<{method.ReturnType}, {parameter0.Type}>(ApiAddress(), out {parameter0.Identifier}, ");
                else
                {
                    var parameter1 = method.ParameterList.Parameters[outParamIndices[1]];
                    sb.Append($"{T3}return await ComputeServer.PostAsync<{method.ReturnType}, {parameter0.Type}, {parameter1.Type}>(ApiAddress(), out {parameter0.Identifier}, out {parameter1.Identifier}, ");
                }
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
            if (remoteCount == 0 && useRemotes)
                return "";
            return sb.ToString();
        }

        static bool IsRemoteCandidate(string className)
        {
            string[] classes = new string[] { "GeometryBase", "Curve", "NurbsCurve",
                "Mesh", "Brep", "Extrusion", "Surface", "NurbsSurface" };
            foreach (var item in classes)
                if (item.Equals(className))
                    return true;

            if( className.StartsWith("IEnumerable<"))
            {
                className = className.Split(new char[] { '<', '>' })[1];
                return IsRemoteCandidate(className);
            }

            return false;
        }

        protected override string Prefix
        {
            get
            {
                using (var stream = GetType().Assembly.GetManifestResourceStream("computegen.RhinoComputeHeader.cs"))
                using (var reader = new System.IO.StreamReader(stream))
                {
                    string header = reader.ReadToEnd();
                    return header.Replace("{{VERSION}}", Version);
                }
            }
        }

        protected override string Suffix 
        {
            get
            {
                return $"}}";
            }
        }

        private bool _async = false;
        public bool IsAsync
        {
            get { return _async; }
            set { _async = value; }
        }
    }
}
