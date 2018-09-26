using System;
using System.Collections.Generic;
using System.Reflection;

namespace compute.geometry
{
    /// <summary>
    /// Master list of all endpoints (URLs) that the compute server supports
    /// </summary>
    static class EndPointDictionary
    {
        static Dictionary<string, EndPoint> _dictionary;

        /// <summary>
        /// Dictionary of all endpoints in the form of a URL and a GeometryEndpoint
        /// handler for that endpoint. We use a dictionary instead of a simple
        /// list to ensure there are no conflicting endpoints that are trying
        /// to use the same URL (this would throw an ArgumentException when
        /// calling add)
        /// </summary>
        /// <returns>
        /// Dictionary of all endpoints that this server supports
        /// </returns>
        public static Dictionary<string, EndPoint> GetDictionary()
        {
            if (_dictionary != null)
                return _dictionary;

            _dictionary = new Dictionary<string, EndPoint>();
            AddEndPoint(_dictionary, EndPoint.CreateGet("", FixedEndpoints.HomePage));
            AddEndPoint(_dictionary, EndPoint.CreateGet("version", FixedEndpoints.GetVersion));
            AddEndPoint(_dictionary, EndPoint.CreateGet("sdk/csharp", FixedEndpoints.CSharpSdk));
            AddEndPoint(_dictionary, EndPoint.CreateGet("hammertime", FixedEndpoints.HammerTime));

            AddEndPoint(_dictionary, new ListSdkEndPoint("sdk"));
            BuildApi(_dictionary, typeof(Rhino.RhinoApp).Assembly, "Rhino.Geometry");
            BuildApi(_dictionary, typeof(Rhino.RhinoApp).Assembly, "Rhino.Geometry.Intersect");
            return _dictionary;
        }

        static void AddEndPoint(Dictionary<string, EndPoint> dict, EndPoint endpoint)
        {
            string key = endpoint.Path.ToLowerInvariant();
            dict.Add(key, endpoint);
        }

        static void BuildApi(Dictionary<string, EndPoint> dict, Assembly assembly, string nameSpace)
        {
            foreach (var export in assembly.GetExportedTypes())
            {
                if (!string.Equals(export.Namespace, nameSpace, StringComparison.Ordinal))
                    continue;
                if (export.IsInterface || export.IsEnum)
                    continue;
                if (export.IsClass || export.IsValueType)
                {
                    var endpoints = GeometryEndPoint.Create(export);
                    foreach (var endpoint in endpoints)
                    {
                        string key = endpoint.Path.ToLowerInvariant();
                        try
                        {
                            AddEndPoint(dict, endpoint);
                        }
                        catch (Exception)
                        {
                            //throw away exception for now
                        }
                    }
                }
            }
        }
    }
}
