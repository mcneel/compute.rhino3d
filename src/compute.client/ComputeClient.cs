using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace computegen
{
    abstract class ComputeClient
    {
        public virtual void Write(Dictionary<string, ClassBuilder> classes, string path, string[] filter)
        {
            StringBuilder clientText = new StringBuilder();
            clientText.Append(Prefix);

            for (int pass= 0; pass < 2; pass++)
            {
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
                        bool containsIntersect = kv.Key.Contains(".Intersect");
                        if (0 == pass && containsIntersect)
                            continue;
                        if (1 == pass && !containsIntersect)
                            continue;
                        clientText.Append(ToComputeClient(kv.Value));
                    }
                }
            }

            clientText.Append(Suffix);
            System.IO.File.WriteAllText(path, clientText.ToString());
        }

        protected virtual int TabSize { get { return 4; } }
        protected string T1 { get { return "".PadLeft(TabSize); } }
        protected string T2 { get { return "".PadLeft(TabSize*2); } }
        protected string T3 { get { return "".PadLeft(TabSize*3); } }

        protected virtual string Prefix
        {
            get { return ""; }
        }

        protected virtual string Suffix
        {
            get { return ""; }
        }

        protected abstract string ToComputeClient(ClassBuilder cb);

        protected static string CamelCase(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;
            string s = text.Substring(0, 1).ToLower() + text.Substring(1);
            return s;
        }

        protected static string Version => "0.12.2";
    }
}
