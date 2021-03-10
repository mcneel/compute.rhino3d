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
        public static Dictionary<string, ClassBuilder> AllClasses { get; private set; }
        static bool AddToDictionary { get; set; }

        public static ClassBuilder[] FilteredList(Dictionary<string, ClassBuilder> classes, string[] filter)
        {
            List<ClassBuilder> rc = new List<ClassBuilder>();
            for (int pass = 0; pass < 2; pass++)
            {
                foreach (var kv in classes)
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
                        bool containsIntersect = kv.Key.Contains(".Intersect");
                        if (0 == pass && containsIntersect)
                            continue;
                        if (1 == pass && !containsIntersect)
                            continue;
                        rc.Add(kv.Value);
                    }
                }
            }
            rc.Sort((a, b) => a.ClassName.CompareTo(b.ClassName));
            return rc.ToArray();
        }

        public static void BuildClassDictionary(string sourcePath)
        {
            AllClasses = new Dictionary<string, ClassBuilder>();
            AddToDictionary = true;
            var options = new Microsoft.CodeAnalysis.CSharp.CSharpParseOptions().WithPreprocessorSymbols("RHINO_SDK").WithDocumentationMode(Microsoft.CodeAnalysis.DocumentationMode.Parse);
            foreach (var file in AllSourceFiles(sourcePath))
            {
                if (System.IO.Path.GetFileName(file).StartsWith("auto", StringComparison.OrdinalIgnoreCase))
                    continue;
                string text = System.IO.File.ReadAllText(file);
                //if (!text.Contains("RHINO_SDK"))
                //    continue;
                //if (!text.Contains("Geometry"))
                //    continue;

                Console.WriteLine($"parse: {file}");
                var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(text, options);
                SourceFileWalker sfw = new SourceFileWalker();
                sfw.Construct(tree.GetRoot());
            }
            AddToDictionary = false;
        }

        public static ClassBuilder Get(string className)
        {
            if (string.IsNullOrWhiteSpace(className))
                return null;

            ClassBuilder cb;
            if (AllClasses.TryGetValue(className, out cb))
                return cb;

            if( !AddToDictionary )
            {
                className = className.Split(new char[] { '.' }).Last();
                foreach (var kv in AllClasses)
                {
                    string key = kv.Key.Split(new char[] { '.' }).Last();
                    if ( key == className)
                        return kv.Value;
                }
                return null;
            }

            cb = new ClassBuilder(className);
            AllClasses[className] = cb;
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

        public string BaseClassName { get; set; }

        public List<Tuple<MethodDeclarationSyntax, DocumentationCommentTriviaSyntax>> Methods { get; } = new List<Tuple<MethodDeclarationSyntax, DocumentationCommentTriviaSyntax>>();

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
                string name = method.ParameterList.Parameters[i].Type.ToString();
                var qualifiedType = method.ParameterList.Parameters[i].Type as QualifiedNameSyntax;
                var genericType = method.ParameterList.Parameters[i].Type as GenericNameSyntax;
                if (genericType == null && qualifiedType != null)
                    genericType = qualifiedType.Right as GenericNameSyntax;
                if (genericType != null)
                {
                    name = genericType.TypeArgumentList.Arguments.ToString() + "Array";
                }
                name = name.Replace("[]", "Array").Replace("Int32", "Int").Replace("Boolean", "Bool");

                url.Append(name);
            }

            return url.ToString().ToLower();
        }
    }


    class SourceFileWalker : Microsoft.CodeAnalysis.CSharp.CSharpSyntaxWalker
    {
        readonly List<Microsoft.CodeAnalysis.Text.TextSpan> _rhinoSdkSpans = new List<Microsoft.CodeAnalysis.Text.TextSpan>();
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

        static string ClassName(TypeDeclarationSyntax node, out string baseClassName)
        {
            baseClassName = null;
            if( node.BaseList!=null )
                baseClassName = node.BaseList.Types[0].ToFullString().Trim();
            NamespaceDeclarationSyntax ns = node.Parent as NamespaceDeclarationSyntax;
            string className = node.Identifier.ToString();
            return ns == null ? className : ns.Name + "." + className;
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

                    if (refCount == 0 && outCount < 3)
                    {
                        TypeDeclarationSyntax tds = node.Parent as TypeDeclarationSyntax;
                        string visitingClass = ClassName(tds, out string baseClass);

                        var docComment = node.GetLeadingTrivia().Select(i => i.GetStructure()).OfType<DocumentationCommentTriviaSyntax>().FirstOrDefault();
                        var cb = ClassBuilder.Get(visitingClass);
                        if (cb != null)
                        {
                            cb.Methods.Add(new Tuple<MethodDeclarationSyntax, DocumentationCommentTriviaSyntax>(node, docComment));
                            if (!string.IsNullOrWhiteSpace(baseClass))
                                cb.BaseClassName = baseClass;
                        }
                    }
                }
            }
            base.VisitMethodDeclaration(node);
        }
    }
}
