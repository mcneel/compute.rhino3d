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
using System.Reflection;
using System.Linq;
using BH.oM.RemoteCompute.RhinoCompute;
using BH.oM.RemoteCompute;

namespace compute.geometry
{
    public partial class ResthopperEndpointsModule : Nancy.NancyModule
    {
        public ResthopperEndpointsModule(Nancy.Routing.IRouteCacheProvider routeCacheProvider)
        {
            Post["/grasshopper"] = _ => GrasshopperEndpoint(Context);
            Post["/io"] = _ => IOsEndpoint(Context, true);
            Get["/io"] = _ => IOsEndpoint(Context, false);
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

        static void SetDefaultTolerances(double absoluteTolerance, double angleToleranceDegrees)
        {
            if (absoluteTolerance <= 0 || angleToleranceDegrees <= 0)
                return;

            var setDefaultTolerancesMethod = typeof(Grasshopper.Utility).GetMethod("SetDefaultTolerances", BindingFlags.Public | BindingFlags.Static);
            if (setDefaultTolerancesMethod != null)
                setDefaultTolerancesMethod.Invoke(null, new object[] { absoluteTolerance, angleToleranceDegrees });
        }

        static void SetDefaultUnits(string modelUnits)
        {
            if (String.IsNullOrEmpty(modelUnits))
                return;

            var setDefaultUnitsMethod = typeof(Grasshopper.Utility).GetMethod("SetDefaultUnits", BindingFlags.Public | BindingFlags.Static);

            if (setDefaultUnitsMethod != null)
                setDefaultUnitsMethod.Invoke(null, new object[] { modelUnits });
        }

        static Response GrasshopperEndpoint(NancyContext ctx)
        {
            var _stopwatch = System.Diagnostics.Stopwatch.StartNew();
            string body = ctx.Request.Body.AsString();
            if (body.StartsWith("[") && body.EndsWith("]"))
                body = body.Substring(1, body.Length - 2);

            FullRhinoComputeSchema input = JsonConvert.DeserializeObject<FullRhinoComputeSchema>(body);

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
            if (input.RecursionLevel == 0)
                lock (_ghsolvelock)
                {
                    return GrasshopperSolveHelper(input, body);
                }
            else
                return GrasshopperSolveHelper(input, body); // we can't block on recursive calls
        }

        static Response GrasshopperSolveHelper(FullRhinoComputeSchema receivedSchema, string body)
        {
            GrasshopperDefinition definition = GrasshopperDefinition.FromUrl(receivedSchema.Pointer, true);
            if (definition == null && !string.IsNullOrWhiteSpace(receivedSchema.Algo))
            {
                definition = GrasshopperDefinition.FromBase64String(receivedSchema.Algo, true);
            }

            if (definition == null)
                throw new Exception("Unable to convert Base-64 encoded Grasshopper script to a GrasshopperDefinition object.");

            SetDefaultTolerances(receivedSchema.AbsoluteTolerance, receivedSchema.AngleTolerance);
            SetDefaultUnits(receivedSchema.ModelUnits);

            int recursionLevel = receivedSchema.RecursionLevel + 1;
            definition.GH_Document.DefineConstant("ComputeRecursionLevel", new Grasshopper.Kernel.Expressions.GH_Variant(recursionLevel));

            definition.AssignInputData(receivedSchema.Values);

            long decodeTime = _stopwatch.ElapsedMilliseconds;
            _stopwatch.Restart();

            // Solve definition.
            var output = definition.SolveDefinition();
            long solveTime = _stopwatch.ElapsedMilliseconds;
            _stopwatch.Restart();

            output.Pointer = definition.CacheKey;

            // Serialize result.
            string returnJson = JsonConvert.SerializeObject(output, GeometryResolver.JsonSerializerSettings);
            long encodeTime = _stopwatch.ElapsedMilliseconds;

            // Set up response.
            Response nancyResponse = returnJson;
            nancyResponse.ContentType = "application/json";
            nancyResponse = nancyResponse.WithHeader("Server-Timing", $"decode;dur={decodeTime}, solve;dur={solveTime}, encode;dur={encodeTime}");

            if (definition.HasErrors)
            {
                nancyResponse.StatusCode = Nancy.HttpStatusCode.InternalServerError;
                nancyResponse.ReasonPhrase = "Errors:\n\t" + string.Join("\n\t", definition.ErrorMessages);
            }
            else
                if (receivedSchema.CacheSolve)
                DataCache.SetCachedSolveResults(body, returnJson, definition);

            return nancyResponse;
        }



        // strip bom from string -- [239, 187, 191] in byte array == (char)65279
        // https://stackoverflow.com/a/54894929/1902446
        static string StripBom(string str)
        {
            if (!string.IsNullOrEmpty(str) && str[0] == (char)65279)
                str = str.Substring(1);

            return str;
        }

        static System.Diagnostics.Stopwatch _stopwatch;

        static object _ghsolvelock = new object();
    }
}
