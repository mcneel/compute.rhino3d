using System;
using System.Collections.Generic;
using System.IO;
using Nancy;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json;
using Grasshopper.Kernel.Data;
using Resthopper.IO;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using System.Net;
using Nancy.Extensions;

namespace compute.geometry
{
    public class ResthopperEndpointsModule : Nancy.NancyModule
    {
        public ResthopperEndpointsModule(Nancy.Routing.IRouteCacheProvider routeCacheProvider)
        {
            Post["/grasshopper"] = _ => Grasshopper(Context);
            Post["/io"] = _ => GetIoNames(Context, true);
            Get["/io"] = _ => GetIoNames(Context, false);
        }

        public static GH_Archive ArchiveFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            byte[] byteArray = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var memStream = new MemoryStream())
            {
                stream.CopyTo(memStream);
                byteArray = memStream.ToArray();
            }

            try
            {
                var byteArchive = new GH_Archive();
                if (byteArchive.Deserialize_Binary(byteArray))
                    return byteArchive;
            }
            catch (Exception) { }

            var grasshopperXml = StripBom(System.Text.Encoding.UTF8.GetString(byteArray));
            var xmlArchive = new GH_Archive();
            if (xmlArchive.Deserialize_Xml(grasshopperXml))
                return xmlArchive;

            return null;
        }

        static object _ghsolvelock = new object();
        static Response Grasshopper(NancyContext ctx)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            string body = ctx.Request.Body.AsString();
            if (body.StartsWith("[") && body.EndsWith("]"))
                body = body.Substring(1, body.Length - 2);
            Schema input = JsonConvert.DeserializeObject<Schema>(body);

            if (input.CacheSolve)
            {
                // look in the cache to see if this has already been solved
                string cachedReturnJson = DataCache.GetCachedSolveResults(body);
                if (!string.IsNullOrWhiteSpace(cachedReturnJson))
                {
                    Response cachedResponse = cachedReturnJson;
                    cachedResponse.ContentType = "application/json";
                    return cachedResponse;
                }
            }

            // 5 Feb 2021 S. Baer
            // Throw a lock around the entire solve process for now. I can easily
            // repeat multi-threaded issues by creating a catenary component with Hops
            // that has one point for A and multiple points for B.
            // We can narrow down this lock over time. As it stands, launching many
            // compute instances on one computer is going to be a better solution anyway
            // to deal with solving many times simultaniously.
            lock (_ghsolvelock)
            {
                // load grasshopper file
                GrasshopperDefinition definition = GrasshopperDefinition.FromUrl(input.Pointer, true);
                if (definition == null)
                {
                    definition = GrasshopperDefinition.FromBase64String(input.Algo);
                    if (definition != null)
                        DataCache.SetCachedDefinition(definition.CacheKey, definition);
                }
                if (definition == null)
                    throw new Exception("Unable to load grasshopper definition");

                definition.SetInputs(input.Values);
                long decodeTime = stopwatch.ElapsedMilliseconds;
                stopwatch.Restart();
                var output = definition.Solve();
                output.Pointer = definition.CacheKey;
                long solveTime = stopwatch.ElapsedMilliseconds;
                stopwatch.Restart();
                string returnJson = JsonConvert.SerializeObject(output, GeometryResolver.Settings);
                long encodeTime = stopwatch.ElapsedMilliseconds;
                Response res = returnJson;
                res.ContentType = "application/json";
                res = res.WithHeader("Server-Timing", $"decode;dur={decodeTime}, solve;dur={solveTime}, encode;dur={encodeTime}");
                if (definition.HasErrors)
                    res.StatusCode = Nancy.HttpStatusCode.InternalServerError;
                else
                {
                    if (input.CacheSolve)
                    {
                        DataCache.SetCachedSolveResults(body, returnJson, definition);
                    }
                }
                return res;
            }
        }

        Response GetIoNames(NancyContext ctx, bool asPost)
        {
            GrasshopperDefinition definition = null;
            string hash = null;
            if (asPost)
            {
                string body = ctx.Request.Body.AsString();
                if (body.StartsWith("[") && body.EndsWith("]"))
                    body = body.Substring(1, body.Length - 2);

                Schema input = JsonConvert.DeserializeObject<Schema>(body);

                // load grasshopper file
                definition = GrasshopperDefinition.FromUrl(input.Pointer, true);
                if (definition != null)
                {
                    hash = input.Pointer;
                }
                else
                {
                    definition = GrasshopperDefinition.FromBase64String(input.Algo);
                    if (definition!=null)
                    {
                        DataCache.SetCachedDefinition(definition.CacheKey, definition);
                    }
                }
            }
            else
            {
                string url = Request.Query["Pointer"].ToString();
                definition = GrasshopperDefinition.FromUrl(url, true);
            }

            if (definition == null)
                throw new Exception("Unable to load grasshopper definition");

            var responseSchema = definition.GetInputsAndOutputs();
            responseSchema.Hash = hash;
            string jsonResponse = JsonConvert.SerializeObject(responseSchema);

            Response res = jsonResponse;
            res.ContentType = "application/json";
            return res;
        }

        public static ResthopperObject GetResthopperPoint(GH_Point goo)
        {
            var pt = goo.Value;

            ResthopperObject rhObj = new ResthopperObject();
            rhObj.Type = pt.GetType().FullName;
            rhObj.Data = JsonConvert.SerializeObject(pt, GeometryResolver.Settings);
            return rhObj;

        }
        public static ResthopperObject GetResthopperObject<T>(object goo)
        {
            var v = (T)goo;

            ResthopperObject rhObj = new ResthopperObject();
            rhObj.Type = goo.GetType().FullName;
            rhObj.Data = JsonConvert.SerializeObject(v, GeometryResolver.Settings);
            return rhObj;
        }
        public static void PopulateParam<DataType>(GH_Param<IGH_Goo> Param, Resthopper.IO.DataTree<ResthopperObject> tree)
        {

            foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
            {
                GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                List<DataType> objectList = new List<DataType>();
                for (int i = 0; i < entree.Value.Count; i++)
                {
                    ResthopperObject obj = entree.Value[i];
                    DataType data = JsonConvert.DeserializeObject<DataType>(obj.Data);
                    Param.AddVolatileData(path, i, data);
                }

            }

        }

        // strip bom from string -- [239, 187, 191] in byte array == (char)65279
        // https://stackoverflow.com/a/54894929/1902446
        static string StripBom(string str)
        {
            if (!string.IsNullOrEmpty(str) && str[0] == (char)65279)
                str = str.Substring(1);

            return str;
        }
    }
}

namespace System.Exceptions
{
    public class PayAttentionException : Exception
    {
        public PayAttentionException(string m) : base(m)
        {

        }

    }
}
