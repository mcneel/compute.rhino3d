using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace computegen
{
    static class MethodDeclarationExtensions
    {
        public static bool IsStatic(this MethodDeclarationSyntax method)
        {
            foreach (var modifier in method.Modifiers)
            {
                if (modifier.Text == "static")
                    return true;
            }
            return false;
        }

        public static bool IsPublic(this MethodDeclarationSyntax method)
        {
            foreach (var modifier in method.Modifiers)
            {
                if (modifier.Text == "public")
                    return true;
            }
            return false;
        }

        public static bool IsNonConst(this MethodDeclarationSyntax method, out bool useAsReturnType)
        {
            useAsReturnType = false;
            if (method.IsStatic())
                return false;

            foreach (var attr in method.AttributeLists)
            {
                if (attr.ToString().Equals("[ConstOperation]", StringComparison.InvariantCulture))
                    return false;
            }

            useAsReturnType = method.ReturnType.ToString().Equals("void", StringComparison.InvariantCulture);
                
            return true;
        }
    }

    class ClassBuilder
    {
        static Dictionary<string, ClassBuilder> _allClasses;
        public static Dictionary<string, ClassBuilder> AllClasses {  get { return _allClasses; } }

        public static void BuildClassDictionary(string sourcePath)
        {
            _allClasses = new Dictionary<string, ClassBuilder>();
            var options = new Microsoft.CodeAnalysis.CSharp.CSharpParseOptions().WithPreprocessorSymbols("RHINO_SDK").WithDocumentationMode(Microsoft.CodeAnalysis.DocumentationMode.Parse);
            foreach (var file in AllSourceFiles(sourcePath))
            {
                if (System.IO.Path.GetFileName(file).StartsWith("auto", StringComparison.OrdinalIgnoreCase))
                    continue;
                string text = System.IO.File.ReadAllText(file);
                if (!text.Contains("RHINO_SDK"))
                    continue;
                if (!text.Contains("Geometry"))
                    continue;

                Console.WriteLine($"parse: {file}");
                var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(text, options);
                SourceFileWalker sfw = new SourceFileWalker();
                sfw.Construct(tree.GetRoot());
            }

        }

        public static ClassBuilder Get(string className)
        {
            ClassBuilder cb;
            if (_allClasses.TryGetValue(className, out cb))
                return cb;
            cb = new ClassBuilder(className);
            _allClasses[className] = cb;
            return cb;
        }

        static IEnumerable<string> AllSourceFiles(string sourcePath)
        {
            foreach (string file in System.IO.Directory.EnumerateFiles(sourcePath, "*.cs", System.IO.SearchOption.AllDirectories))
            {
                if (file.Contains("\\obj\\"))
                    continue;
                yield return file;
            }
        }

        private ClassBuilder(string rhinocommonClassName)
        {
            FullClassName = rhinocommonClassName;
        }

        public string FullClassName { get; set; }
        public string ClassName
        {
            get
            {
                string s = FullClassName;
                int index = s.LastIndexOf('.');
                return s.Substring(index + 1);
            }
        }

        readonly List<Tuple<MethodDeclarationSyntax, DocumentationCommentTriviaSyntax>> _methods = new List<Tuple<MethodDeclarationSyntax, DocumentationCommentTriviaSyntax>>();
        public void AddMethod(MethodDeclarationSyntax method, DocumentationCommentTriviaSyntax comment)
        {
            _methods.Add(new Tuple<MethodDeclarationSyntax, DocumentationCommentTriviaSyntax>(method, comment));
        }

        public string EndPoint(MethodDeclarationSyntax method)
        {
            StringBuilder url = new StringBuilder(FullClassName.Replace('.', '/'));
            url.Append("/");
            url.Append(method.Identifier.ToString());

            bool appendDash = true;
            if( !method.IsStatic())
            {
                url.Append("-");
                url.Append(ClassName);
                appendDash = false;
            }

            for (int i = 0; i < method.ParameterList.Parameters.Count; i++)
            {
                if (appendDash)
                    url.Append("-");
                else
                    url.Append("_");
                appendDash = false;
                url.Append(method.ParameterList.Parameters[i].Type.ToString());
            }
            return url.ToString().ToLower();
        }

        public string ToComputeString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"    public static class {ClassName}Compute");
            sb.AppendLine("    {");
            sb.AppendLine("        static string ApiAddress([CallerMemberName] string caller = null)");
            sb.AppendLine("        {");
            sb.AppendLine($"            return Client.ApiAddress(typeof({ClassName}), caller);");
            sb.AppendLine("        }");


            foreach (var (method, comment) in _methods)
            {
                if (comment != null)
                    sb.Append("        " + comment.ToFullString());
                bool useAsReturnType;
                if (method.IsNonConst(out useAsReturnType) && useAsReturnType)
                    sb.Append($"        public static {ClassName} {method.Identifier}(");
                else
                    sb.Append($"        public static {method.ReturnType} {method.Identifier}(");
                
                int paramCount = 0;
                if (!method.IsStatic())
                {
                    sb.Append($"this {ClassName} {ClassName.ToLower()}");
                    paramCount++;
                }
                if (method.IsNonConst(out useAsReturnType) && !useAsReturnType)
                {
                    if (paramCount > 0)
                        sb.Append(", ");
                    sb.Append($"out {ClassName} updatedInstance");
                    paramCount++;
                }
                for ( int i=0; i<method.ParameterList.Parameters.Count; i++ )
                {
                    if (paramCount > 0)
                        sb.Append(", ");
                    sb.Append($"{method.ParameterList.Parameters[i].ToFullString()}");
                    paramCount++;
                }
                sb.AppendLine(")");
                sb.AppendLine("        {");

                int outParamIndex = -1;
                for( int i=0; i<method.ParameterList.Parameters.Count; i++ )
                {
                    foreach(var modifier in method.ParameterList.Parameters[i].Modifiers)
                    {
                        if(modifier.Text == "out")
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
                    if( method.IsNonConst(out useAsReturnType))
                    {
                        if( useAsReturnType )
                            sb.Append($"            return Client.Post<{ClassName}>(ApiAddress(), ");
                        else
                            sb.Append($"            return Client.Post<{method.ReturnType}, {ClassName}>(ApiAddress(), out updatedInstance, ");
                    }
                    else
                        sb.Append($"            return Client.Post<{method.ReturnType}>(ApiAddress(), ");
                }
                else
                {
                    var parameter = method.ParameterList.Parameters[outParamIndex];
                    sb.Append($"            return Client.Post<{method.ReturnType}, {parameter.Type}>(ApiAddress(), out {parameter.Identifier}, ");
                }
                if (!method.IsStatic())
                {
                    sb.Append($"{ClassName.ToLower()}");
                    if (method.ParameterList.Parameters.Count > 0)
                        sb.Append(", ");
                }

                List<ParameterSyntax> orderedParams = new List<ParameterSyntax>();
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
                sb.AppendLine("        }");
            }
            sb.AppendLine("  }");
            return sb.ToString();
        }

        static string CamelCase(string text)
        {
            string s = text.Substring(0, 1).ToLower() + text.Substring(1);
            return s;
        }

        const string T1 = "    ";
        const string T2 = "        ";
        const string T3 = "            ";

        public string ToComputeJavascript()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine($"{T1}{ClassName} : {{");
            int iMethod = 0;
            int overloadIndex = 0;
            string prevMethodName = "";
            foreach (var (method, comment) in _methods)
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
                    parameters.Add(ClassName.ToLower());
                }
                for (int i = 0; i < method.ParameterList.Parameters.Count; i++)
                {
                    parameters.Add(method.ParameterList.Parameters[i].Identifier.ToString());
                }

                for(int i=0; i<parameters.Count; i++)
                {
                    sb.Append(parameters[i]);
                    if (i < (parameters.Count - 1))
                        sb.Append(", ");
                }
                sb.AppendLine(") {");
                sb.Append($"{T3}args = [");
                for(int i=0; i<parameters.Count; i++ )
                {
                    sb.Append(parameters[i]);
                    if (i < (parameters.Count - 1))
                        sb.Append(", ");
                }
                sb.AppendLine("];");
                string endpoint = method.Identifier.ToString();
                sb.AppendLine($"{T3}var promise = RhinoCompute.computeFetch(\"{EndPoint(method)}\", args);");
                sb.AppendLine($"{T3}return promise;");
                sb.AppendLine($"{T2}}},");

                iMethod++;
                if(iMethod < _methods.Count)
                    sb.AppendLine();
            }
            sb.AppendLine("    },");
            return sb.ToString();
        }
    }


    class SourceFileWalker : Microsoft.CodeAnalysis.CSharp.CSharpSyntaxWalker
    {
        readonly List<Microsoft.CodeAnalysis.Text.TextSpan> _rhinoSdkSpans = new List<Microsoft.CodeAnalysis.Text.TextSpan>();
        string _visitingClass = null;
        int _activeSpanStart;

        public SourceFileWalker() : base(Microsoft.CodeAnalysis.SyntaxWalkerDepth.StructuredTrivia)
        {
        }

        bool _buildingSpans = true;
        public void Construct(Microsoft.CodeAnalysis.SyntaxNode node)
        {
            _buildingSpans = true;
            Visit(node);
            _buildingSpans = false;
            Visit(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            NamespaceDeclarationSyntax ns = node.Parent as NamespaceDeclarationSyntax;
            string className = node.Identifier.ToString();
            _visitingClass = ns==null? className : ns.Name + "." + className;
            base.VisitClassDeclaration(node);
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            NamespaceDeclarationSyntax ns = node.Parent as NamespaceDeclarationSyntax;
            string className = node.Identifier.ToString();
            _visitingClass = ns == null ? className : ns.Name + "." + className;
            base.VisitStructDeclaration(node);
        }

        public override void VisitIfDirectiveTrivia(IfDirectiveTriviaSyntax node)
        {
            _activeSpanStart = node.SpanStart;
            base.VisitIfDirectiveTrivia(node);
        }
        public override void VisitEndIfDirectiveTrivia(EndIfDirectiveTriviaSyntax node)
        {
            if (_buildingSpans)
            {
                var span = new Microsoft.CodeAnalysis.Text.TextSpan(_activeSpanStart, node.Span.End - _activeSpanStart);
                _rhinoSdkSpans.Add(span);
            }
            base.VisitEndIfDirectiveTrivia(node);
        }

        bool InSpans(Microsoft.CodeAnalysis.Text.TextSpan span)
        {
            for (int i = 0; i < _rhinoSdkSpans.Count; i++)
            {
                if (_rhinoSdkSpans[i].IntersectsWith(span))
                    return true;
            }
            return false;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (!_buildingSpans && InSpans(node.Span))
            {
                bool isPublic = node.IsPublic();
                bool isStatic = node.IsStatic();

                if (isPublic)
                {
                    // skip methods with ref parameters of multiple out parameters for now
                    int refCount = 0;
                    int outCount = 0;
                    foreach (var parameter in node.ParameterList.Parameters)
                    {
                        foreach (var modifier in parameter.Modifiers)
                        {
                            if (modifier.Text == "ref")
                                refCount++;
                            if (modifier.Text == "out")
                                outCount++;
                        }
                    }

                    bool useAsReturnType;
                    if (node.IsNonConst(out useAsReturnType) && !useAsReturnType)
                        outCount++;

                    if (refCount == 0 && outCount < 2)
                    {
                        var docComment = node.GetLeadingTrivia().Select(i => i.GetStructure()).OfType<DocumentationCommentTriviaSyntax>().FirstOrDefault();
                        ClassBuilder.Get(_visitingClass).AddMethod(node, docComment);
                    }
                }
            }
            base.VisitMethodDeclaration(node);
        }
    }
}
