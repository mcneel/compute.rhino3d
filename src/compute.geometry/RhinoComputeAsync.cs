using System;
using System.IO;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Rhino.Collections;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Rhino.Compute
{
    public static class ComputeServer
    {
        public static string WebAddress { get; set; } = "http://localhost:6500";
        public static string AuthToken { get; set; }
        public static string ApiKey { get; set; }
        public static string Version => "0.12.2";

        public static T Post<T>(string function, params object[] postData)
        {
            return PostWithConverter<T>(function, null, postData);
        }

        public static T PostWithConverter<T>(string function, JsonConverter converter, params object[] postData)
        {
            for( int i=0; i<postData.Length; i++ )
            {
                if( postData[i]!=null &&
                    postData[i].GetType().IsGenericType &&
                    postData[i].GetType().GetGenericTypeDefinition() == typeof(Remote<>) )
                {
                    var mi = postData[i].GetType().GetMethod("JsonObject");
                    postData[i] = mi.Invoke(postData[i], null);
                }
            }

            string json = converter == null ?
                JsonConvert.SerializeObject(postData, Formatting.None) :
                JsonConvert.SerializeObject(postData, Formatting.None, converter);
            var response = DoPost(function, json);
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                if (converter == null)
                    return JsonConvert.DeserializeObject<T>(result);
                return JsonConvert.DeserializeObject<T>(result, converter);
            }
        }

        public static T0 Post<T0, T1>(string function, out T1 out1, params object[] postData)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(postData);
            var response = DoPost(function, json);
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var jsonString = streamReader.ReadToEnd();
                object data = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);
                var ja = data as Newtonsoft.Json.Linq.JArray;
                out1 = ja[1].ToObject<T1>();
                return ja[0].ToObject<T0>();
            }
        }

        public static T0 Post<T0, T1, T2>(string function, out T1 out1, out T2 out2, params object[] postData)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(postData);
            var response = DoPost(function, json);
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var jsonString = streamReader.ReadToEnd();
                object data = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);
                var ja = data as Newtonsoft.Json.Linq.JArray;
                out1 = ja[1].ToObject<T1>();
                out2 = ja[2].ToObject<T2>();
                return ja[0].ToObject<T0>();
            }
        }

        // run all synchronous requests through here
        private static System.Net.WebResponse DoPost(string function, string json)
        {
            if (!function.StartsWith("/")) // add leading /
                function = "/" + function; // if not present

            string uri = $"{WebAddress}{function}".ToLower();
            var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(uri);
            request.ContentType = "application/json";
            request.UserAgent = $"compute.rhino3d.cs/{Version}";
            request.Method = "POST";

            // try auth token (compute.rhino3d.com only)
            if (!string.IsNullOrWhiteSpace(AuthToken))
                request.Headers.Add("Authorization", "Bearer " + AuthToken);

            // try api key (self-hosted compute)
            if (!string.IsNullOrWhiteSpace(ApiKey))
                request.Headers.Add("RhinoComputeKey", ApiKey);
            
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
            }

            return request.GetResponse();
        }

        public static async Task<T> PostAsync<T>(string function, params object[] postData)
        {
            return await PostAsyncWithConverter<T>(function, null, postData);
        }

        public static async Task<T> PostAsyncWithConverter<T>(string function, JsonConverter converter, params object[] postData)
        {
            for (int i = 0; i < postData.Length; i++)
            {
                if (postData[i] != null &&
                    postData[i].GetType().IsGenericType &&
                    postData[i].GetType().GetGenericTypeDefinition() == typeof(Remote<>))
                {
                    var mi = postData[i].GetType().GetMethod("JsonObject");
                    postData[i] = mi.Invoke(postData[i], null);
                }
            }

            string json = converter == null ?
                JsonConvert.SerializeObject(postData, Formatting.None) :
                JsonConvert.SerializeObject(postData, Formatting.None, converter);

            var response = await DoPostAsync(function, json);
            var result = await response.Content.ReadAsStringAsync();
            if (converter == null)
                return JsonConvert.DeserializeObject<T>(result);
            return JsonConvert.DeserializeObject<T>(result, converter);
        }

        public static async Task<(T0, T1)> PostAsync<T0, T1>(string function, params object[] postData)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(postData);
            var response = await DoPostAsync(function, json);
            var jsonString = await response.Content.ReadAsStringAsync();
            object data = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);
            var ja = data as Newtonsoft.Json.Linq.JArray;
            T0 out0 = ja[0].ToObject<T0>();
            T1 out1 = ja[1].ToObject<T1>();
            return (out0, out1);
        }

        public static async Task<(T0, T1, T2)> PostAsync<T0, T1, T2>(string function, params object[] postData)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(postData);
            var response = await DoPostAsync(function, json);
            var jsonString = await response.Content.ReadAsStringAsync();
            object data = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);
            var ja = data as Newtonsoft.Json.Linq.JArray;
            T0 out0 = ja[0].ToObject<T0>();
            T1 out1 = ja[1].ToObject<T1>();
            T2 out2 = ja[2].ToObject<T2>();
            return (out0, out1, out2);
        }

        // run all asynchronous requests through here
        private static async Task<System.Net.Http.HttpResponseMessage> DoPostAsync(string function, string json)
        {
            if (!function.StartsWith("/")) // add leading /
              function = "/" + function; // if not present

            string uri = $"{WebAddress}{function}".ToLower();
            using (var client = new System.Net.Http.HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", $"compute.rhino3d.cs/{Version}");
                client.DefaultRequestHeaders
                .Accept
                .Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                // try auth token (compute.rhino3d.com only)
                if (!string.IsNullOrWhiteSpace(AuthToken))
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AuthToken);

                // try api key (self-hosted compute)
                if (!string.IsNullOrWhiteSpace(ApiKey))
                    client.DefaultRequestHeaders.Add("RhinoComputeKey", ApiKey);

                var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
                return await client.PostAsync(uri, content);
            }
        }

        public static string ApiAddress(Type t, string function)
        {
            string s = t.ToString().Replace('.', '/');
            return s + "/" + function;
        }
    }

    public class Remote<T>
    {
        string _url;
        T _data;

        public Remote(string url)
        {
            _url = url;
        }

        public Remote(T data)
        {
            _data = data;
        }

        public object JsonObject()
        {
            if( _url!=null )
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict["url"] = _url;
                return dict;
            }
            return _data;
        }
    }


    public static class PythonCompute
    {
        // NOTE: If you are using a Rhino 6 based version of Rhino3dmIO, you will
        // get compile errors due to ArchivableDictionary not implementing ISerializble in V6
        //
        // Either update to a V7 version of Rhino3dmIO (check the prerelease box on nuget)
        // -or-
        // Delete the entire PythonCompute class here. Python functionality is only available
        // using a V7 based Rhino3dmIO assembly
        class ArchivableDictionaryResolver : JsonConverter
        {
            public override bool CanConvert(Type objectType) { return objectType == typeof(ArchivableDictionary); }
            public override bool CanRead => true;
            public override bool CanWrite => true;

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                string encoded = (string)reader.Value;
                var dh = JsonConvert.DeserializeObject<DictHelper>(encoded);
                return dh.SerializedDictionary;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                string json = JsonConvert.SerializeObject(new DictHelper((ArchivableDictionary)value));
                writer.WriteValue(json);
            }


            [Serializable]
            class DictHelper : ISerializable
            {
                public ArchivableDictionary SerializedDictionary { get; set; }
                public DictHelper(ArchivableDictionary d) { SerializedDictionary = d; }
                public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
                {
                    SerializedDictionary.GetObjectData(info, context);
                }
                protected DictHelper(SerializationInfo info, StreamingContext context)
                {
                    Type t = typeof(ArchivableDictionary);
                    var constructor = t.GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                      null, new Type[] { typeof(SerializationInfo), typeof(StreamingContext) }, null);
                    SerializedDictionary = constructor.Invoke(new object[] { info, context }) as ArchivableDictionary;
                }
            }
        }


        static string ApiAddress([CallerMemberName] string caller = null)
        {
            return "rhino/python/" + caller;
        }

        public static ArchivableDictionary Evaluate(string script, ArchivableDictionary input)
        {
            return ComputeServer.PostWithConverter<ArchivableDictionary>(ApiAddress(), new ArchivableDictionaryResolver(), script, input);
        }
    }


    public static class ExtrusionCompute
    {
        static string ApiAddress([CallerMemberName] string caller = null)
        {
            return ComputeServer.ApiAddress(typeof(Extrusion), caller);
        }

        /// <summary>
        /// Constructs all the Wireframe curves for this Extrusion.
        /// </summary>
        /// <returns>An array of Wireframe curves.</returns>
        public static async Task<Curve[]> GetWireframe(this Extrusion extrusion)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), extrusion);
        }

        /// <summary>
        /// Constructs all the Wireframe curves for this Extrusion.
        /// </summary>
        /// <returns>An array of Wireframe curves.</returns>
        public static async Task<Curve[]> GetWireframe(Remote<Extrusion> extrusion)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), extrusion);
        }
    }

    public static class BezierCurveCompute
    {
        static string ApiAddress([CallerMemberName] string caller = null)
        {
            return ComputeServer.ApiAddress(typeof(BezierCurve), caller);
        }

        /// <summary>
        /// Constructs an array of cubic, non-rational Beziers that fit a curve to a tolerance.
        /// </summary>
        /// <param name="sourceCurve">A curve to approximate.</param>
        /// <param name="distanceTolerance">
        /// The max fitting error. Use RhinoMath.SqrtEpsilon as a minimum.
        /// </param>
        /// <param name="kinkTolerance">
        /// If the input curve has a g1-discontinuity with angle radian measure
        /// greater than kinkTolerance at some point P, the list of beziers will
        /// also have a kink at P.
        /// </param>
        /// <returns>A new array of bezier curves. The array can be empty and might contain null items.</returns>
        public static async Task<BezierCurve[]> CreateCubicBeziers(Curve sourceCurve, double distanceTolerance, double kinkTolerance)
        {
            return await ComputeServer.PostAsync<BezierCurve[]>(ApiAddress(), sourceCurve, distanceTolerance, kinkTolerance);
        }

        /// <summary>
        /// Constructs an array of cubic, non-rational Beziers that fit a curve to a tolerance.
        /// </summary>
        /// <param name="sourceCurve">A curve to approximate.</param>
        /// <param name="distanceTolerance">
        /// The max fitting error. Use RhinoMath.SqrtEpsilon as a minimum.
        /// </param>
        /// <param name="kinkTolerance">
        /// If the input curve has a g1-discontinuity with angle radian measure
        /// greater than kinkTolerance at some point P, the list of beziers will
        /// also have a kink at P.
        /// </param>
        /// <returns>A new array of bezier curves. The array can be empty and might contain null items.</returns>
        public static async Task<BezierCurve[]> CreateCubicBeziers(Remote<Curve> sourceCurve, double distanceTolerance, double kinkTolerance)
        {
            return await ComputeServer.PostAsync<BezierCurve[]>(ApiAddress(), sourceCurve, distanceTolerance, kinkTolerance);
        }

        /// <summary>
        /// Create an array of Bezier curves that fit to an existing curve. Please note, these
        /// Beziers can be of any order and may be rational.
        /// </summary>
        /// <param name="sourceCurve">The curve to fit Beziers to</param>
        /// <returns>A new array of Bezier curves</returns>
        public static async Task<BezierCurve[]> CreateBeziers(Curve sourceCurve)
        {
            return await ComputeServer.PostAsync<BezierCurve[]>(ApiAddress(), sourceCurve);
        }

        /// <summary>
        /// Create an array of Bezier curves that fit to an existing curve. Please note, these
        /// Beziers can be of any order and may be rational.
        /// </summary>
        /// <param name="sourceCurve">The curve to fit Beziers to</param>
        /// <returns>A new array of Bezier curves</returns>
        public static async Task<BezierCurve[]> CreateBeziers(Remote<Curve> sourceCurve)
        {
            return await ComputeServer.PostAsync<BezierCurve[]>(ApiAddress(), sourceCurve);
        }
    }

    public static class BrepCompute
    {
        static string ApiAddress([CallerMemberName] string caller = null)
        {
            return ComputeServer.ApiAddress(typeof(Brep), caller);
        }

        /// <summary>
        /// Verifies a Brep is in the form of a solid box.
        /// </summary>
        /// <returns>true if the Brep is a solid box, false otherwise.</returns>
        public static async Task<bool> IsBox(this Brep brep)
        {
            return await ComputeServer.PostAsync<bool>(ApiAddress(), brep);
        }

        /// <summary>
        /// Verifies a Brep is in the form of a solid box.
        /// </summary>
        /// <returns>true if the Brep is a solid box, false otherwise.</returns>
        public static async Task<bool> IsBox(Remote<Brep> brep)
        {
            return await ComputeServer.PostAsync<bool>(ApiAddress(), brep);
        }

        /// <summary>
        /// Verifies a Brep is in the form of a solid box.
        /// </summary>
        /// <param name="tolerance">The tolerance used to determine if faces are planar and to compare face normals.</param>
        /// <returns>true if the Brep is a solid box, false otherwise.</returns>
        public static async Task<bool> IsBox(this Brep brep, double tolerance)
        {
            return await ComputeServer.PostAsync<bool>(ApiAddress(), brep, tolerance);
        }

        /// <summary>
        /// Verifies a Brep is in the form of a solid box.
        /// </summary>
        /// <param name="tolerance">The tolerance used to determine if faces are planar and to compare face normals.</param>
        /// <returns>true if the Brep is a solid box, false otherwise.</returns>
        public static async Task<bool> IsBox(Remote<Brep> brep, double tolerance)
        {
            return await ComputeServer.PostAsync<bool>(ApiAddress(), brep, tolerance);
        }

        /// <summary>
        /// Change the seam of a closed trimmed surface.
        /// </summary>
        /// <param name="face">A Brep face with a closed underlying surface.</param>
        /// <param name="direction">The parameter direction (0 = U, 1 = V). The face's underlying surface must be closed in this direction.</param>
        /// <param name="parameter">The parameter at which to place the seam.</param>
        /// <param name="tolerance">Tolerance used to cut up surface.</param>
        /// <returns>A new Brep that has the same geometry as the face with a relocated seam if successful, or null on failure.</returns>
        public static async Task<Brep> ChangeSeam(BrepFace face, int direction, double parameter, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), face, direction, parameter, tolerance);
        }

        /// <summary>
        /// Copy all trims from a Brep face onto a surface.
        /// </summary>
        /// <param name="trimSource">Brep face which defines the trimming curves.</param>
        /// <param name="surfaceSource">The surface to trim.</param>
        /// <param name="tolerance">Tolerance to use for rebuilding 3D trim curves.</param>
        /// <returns>A brep with the shape of surfaceSource and the trims of trimSource or null on failure.</returns>
        public static async Task<Brep> CopyTrimCurves(BrepFace trimSource, Surface surfaceSource, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), trimSource, surfaceSource, tolerance);
        }

        /// <summary>
        /// Copy all trims from a Brep face onto a surface.
        /// </summary>
        /// <param name="trimSource">Brep face which defines the trimming curves.</param>
        /// <param name="surfaceSource">The surface to trim.</param>
        /// <param name="tolerance">Tolerance to use for rebuilding 3D trim curves.</param>
        /// <returns>A brep with the shape of surfaceSource and the trims of trimSource or null on failure.</returns>
        public static async Task<Brep> CopyTrimCurves(BrepFace trimSource, Remote<Surface> surfaceSource, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), trimSource, surfaceSource, tolerance);
        }

        /// <summary>
        /// Creates a brep representation of the sphere with two similar trimmed NURBS surfaces, and no singularities.
        /// </summary>
        /// <param name="center">The center of the sphere.</param>
        /// <param name="radius">The radius of the sphere.</param>
        /// <param name="tolerance">
        /// Used in computing 2d trimming curves. If &gt;= 0.0, then the max of 
        /// ON_0.0001 * radius and RhinoMath.ZeroTolerance will be used.
        /// </param>
        /// <returns>A new brep, or null on error.</returns>
        public static async Task<Brep> CreateBaseballSphere(Point3d center, double radius, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), center, radius, tolerance);
        }

        /// <summary>
        /// Creates a single developable surface between two curves.
        /// </summary>
        /// <param name="crv0">The first rail curve.</param>
        /// <param name="crv1">The second rail curve.</param>
        /// <param name="reverse0">Reverse the first rail curve.</param>
        /// <param name="reverse1">Reverse the second rail curve</param>
        /// <param name="density">The number of rulings across the surface.</param>
        /// <returns>The output Breps if successful, otherwise an empty array.</returns>
        public static async Task<Brep[]> CreateDevelopableLoft(Curve crv0, Curve crv1, bool reverse0, bool reverse1, int density)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), crv0, crv1, reverse0, reverse1, density);
        }

        /// <summary>
        /// Creates a single developable surface between two curves.
        /// </summary>
        /// <param name="crv0">The first rail curve.</param>
        /// <param name="crv1">The second rail curve.</param>
        /// <param name="reverse0">Reverse the first rail curve.</param>
        /// <param name="reverse1">Reverse the second rail curve</param>
        /// <param name="density">The number of rulings across the surface.</param>
        /// <returns>The output Breps if successful, otherwise an empty array.</returns>
        public static async Task<Brep[]> CreateDevelopableLoft(Remote<Curve> crv0, Remote<Curve> crv1, bool reverse0, bool reverse1, int density)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), crv0, crv1, reverse0, reverse1, density);
        }

        /// <summary>
        /// Creates a single developable surface between two curves.
        /// </summary>
        /// <param name="rail0">The first rail curve.</param>
        /// <param name="rail1">The second rail curve.</param>
        /// <param name="fixedRulings">
        /// Rulings define lines across the surface that define the straight sections on the developable surface,
        /// where rulings[i].X = parameter on first rail curve, and rulings[i].Y = parameter on second rail curve.
        /// Note, rulings will be automatically adjusted to minimum twist.
        /// </param>
        /// <returns>The output Breps if successful, otherwise an empty array.</returns>
        public static async Task<Brep[]> CreateDevelopableLoft(NurbsCurve rail0, NurbsCurve rail1, IEnumerable<Point2d> fixedRulings)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail0, rail1, fixedRulings);
        }

        /// <summary>
        /// Creates a single developable surface between two curves.
        /// </summary>
        /// <param name="rail0">The first rail curve.</param>
        /// <param name="rail1">The second rail curve.</param>
        /// <param name="fixedRulings">
        /// Rulings define lines across the surface that define the straight sections on the developable surface,
        /// where rulings[i].X = parameter on first rail curve, and rulings[i].Y = parameter on second rail curve.
        /// Note, rulings will be automatically adjusted to minimum twist.
        /// </param>
        /// <returns>The output Breps if successful, otherwise an empty array.</returns>
        public static async Task<Brep[]> CreateDevelopableLoft(Remote<NurbsCurve> rail0, Remote<NurbsCurve> rail1, IEnumerable<Point2d> fixedRulings)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail0, rail1, fixedRulings);
        }

        /// <summary>
        /// Takes a surface and a set of all 3D curves that define a single trimmed surface,
        /// and splits the surface with the curves, keeping the piece that uses all of the curves.
        /// </summary>
        /// <param name="surface">The surface.</param>
        /// <param name="curves">The curves.</param>
        /// <param name="useEdgeCurves">
        /// The 2D trimming curves are made by pulling back the 3D curves using the fitting tolerance.  
        /// If useEdgeCurves is true, the input 3D curves will be used as the edge curves in the result. 
        /// Otherwise, the edges will come from pushing up the 2D pullbacks.
        /// </param>
        /// <param name="tolerance">
        /// The fitting tolerance. 
        /// When in doubt, use the document's model absolute tolerance.
        /// </param>
        /// <returns>
        /// The resulting Breps is successful, otherwise an empty array.
        /// Note, you may want to join the results into a single Brep.
        /// </returns>
        /// <remarks>
        /// This function is useful when importing non-3dm files with questionable geometry.
        /// It is also a good way to quickly create a trimmed surface that has multiple inner boundaries without having to call split or trim and then select all of the pieces to throw away.
        /// If the surface is closed, an attempt will be made to move seams so that they avoid the curves. 
        /// If that cannot be done, the result may have more than one face.
        /// If the surface is closed in both directions, it is possible that the split will result in two faces that use all of the input curves.
        /// In this case all curves must be oriented counter-clockwise, with respect to the surface normal, around the desired result.
        /// The surface may be extended if it is not large enough to contain the curves.
        /// Noisy singularities will be repaired when possible.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<Brep[]> CutUpSurface(Surface surface, IEnumerable<Curve> curves, bool useEdgeCurves, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), surface, curves, useEdgeCurves, tolerance);
        }

        /// <summary>
        /// Takes a surface and a set of all 3D curves that define a single trimmed surface,
        /// and splits the surface with the curves, keeping the piece that uses all of the curves.
        /// </summary>
        /// <param name="surface">The surface.</param>
        /// <param name="curves">The curves.</param>
        /// <param name="useEdgeCurves">
        /// The 2D trimming curves are made by pulling back the 3D curves using the fitting tolerance.  
        /// If useEdgeCurves is true, the input 3D curves will be used as the edge curves in the result. 
        /// Otherwise, the edges will come from pushing up the 2D pullbacks.
        /// </param>
        /// <param name="tolerance">
        /// The fitting tolerance. 
        /// When in doubt, use the document's model absolute tolerance.
        /// </param>
        /// <returns>
        /// The resulting Breps is successful, otherwise an empty array.
        /// Note, you may want to join the results into a single Brep.
        /// </returns>
        /// <remarks>
        /// This function is useful when importing non-3dm files with questionable geometry.
        /// It is also a good way to quickly create a trimmed surface that has multiple inner boundaries without having to call split or trim and then select all of the pieces to throw away.
        /// If the surface is closed, an attempt will be made to move seams so that they avoid the curves. 
        /// If that cannot be done, the result may have more than one face.
        /// If the surface is closed in both directions, it is possible that the split will result in two faces that use all of the input curves.
        /// In this case all curves must be oriented counter-clockwise, with respect to the surface normal, around the desired result.
        /// The surface may be extended if it is not large enough to contain the curves.
        /// Noisy singularities will be repaired when possible.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<Brep[]> CutUpSurface(Remote<Surface> surface, Remote<IEnumerable<Curve>> curves, bool useEdgeCurves, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), surface, curves, useEdgeCurves, tolerance);
        }

        /// <summary>
        /// Constructs a set of planar breps as outlines by the loops.
        /// </summary>
        /// <param name="inputLoops">Curve loops that delineate the planar boundaries.</param>
        /// <returns>An array of Planar Breps.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Brep[]> CreatePlanarBreps(IEnumerable<Curve> inputLoops)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), inputLoops);
        }

        /// <summary>
        /// Constructs a set of planar breps as outlines by the loops.
        /// </summary>
        /// <param name="inputLoops">Curve loops that delineate the planar boundaries.</param>
        /// <returns>An array of Planar Breps.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Brep[]> CreatePlanarBreps(Remote<IEnumerable<Curve>> inputLoops)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), inputLoops);
        }

        /// <summary>
        /// Constructs a set of planar breps as outlines by the loops.
        /// </summary>
        /// <param name="inputLoops">Curve loops that delineate the planar boundaries.</param>
        /// <param name="tolerance"></param>
        /// <returns>An array of Planar Breps.</returns>
        public static async Task<Brep[]> CreatePlanarBreps(IEnumerable<Curve> inputLoops, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), inputLoops, tolerance);
        }

        /// <summary>
        /// Constructs a set of planar breps as outlines by the loops.
        /// </summary>
        /// <param name="inputLoops">Curve loops that delineate the planar boundaries.</param>
        /// <param name="tolerance"></param>
        /// <returns>An array of Planar Breps.</returns>
        public static async Task<Brep[]> CreatePlanarBreps(Remote<IEnumerable<Curve>> inputLoops, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), inputLoops, tolerance);
        }

        /// <summary>
        /// Constructs a set of planar breps as outlines by the loops.
        /// </summary>
        /// <param name="inputLoop">A curve that should form the boundaries of the surfaces or polysurfaces.</param>
        /// <returns>An array of Planar Breps.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Brep[]> CreatePlanarBreps(Curve inputLoop)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), inputLoop);
        }

        /// <summary>
        /// Constructs a set of planar breps as outlines by the loops.
        /// </summary>
        /// <param name="inputLoop">A curve that should form the boundaries of the surfaces or polysurfaces.</param>
        /// <returns>An array of Planar Breps.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Brep[]> CreatePlanarBreps(Remote<Curve> inputLoop)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), inputLoop);
        }

        /// <summary>
        /// Constructs a set of planar breps as outlines by the loops.
        /// </summary>
        /// <param name="inputLoop">A curve that should form the boundaries of the surfaces or polysurfaces.</param>
        /// <param name="tolerance"></param>
        /// <returns>An array of Planar Breps.</returns>
        public static async Task<Brep[]> CreatePlanarBreps(Curve inputLoop, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), inputLoop, tolerance);
        }

        /// <summary>
        /// Constructs a set of planar breps as outlines by the loops.
        /// </summary>
        /// <param name="inputLoop">A curve that should form the boundaries of the surfaces or polysurfaces.</param>
        /// <param name="tolerance"></param>
        /// <returns>An array of Planar Breps.</returns>
        public static async Task<Brep[]> CreatePlanarBreps(Remote<Curve> inputLoop, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), inputLoop, tolerance);
        }

        /// <summary>
        /// Constructs a Brep using the trimming information of a brep face and a surface. 
        /// Surface must be roughly the same shape and in the same location as the trimming brep face.
        /// </summary>
        /// <param name="trimSource">BrepFace which contains trimmingSource brep.</param>
        /// <param name="surfaceSource">Surface that trims of BrepFace will be applied to.</param>
        /// <returns>A brep with the shape of surfaceSource and the trims of trimSource or null on failure.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Brep> CreateTrimmedSurface(BrepFace trimSource, Surface surfaceSource)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), trimSource, surfaceSource);
        }

        /// <summary>
        /// Constructs a Brep using the trimming information of a brep face and a surface. 
        /// Surface must be roughly the same shape and in the same location as the trimming brep face.
        /// </summary>
        /// <param name="trimSource">BrepFace which contains trimmingSource brep.</param>
        /// <param name="surfaceSource">Surface that trims of BrepFace will be applied to.</param>
        /// <returns>A brep with the shape of surfaceSource and the trims of trimSource or null on failure.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Brep> CreateTrimmedSurface(BrepFace trimSource, Remote<Surface> surfaceSource)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), trimSource, surfaceSource);
        }

        /// <summary>
        /// Constructs a Brep using the trimming information of a brep face and a surface. 
        /// Surface must be roughly the same shape and in the same location as the trimming brep face.
        /// </summary>
        /// <param name="trimSource">BrepFace which contains trimmingSource brep.</param>
        /// <param name="surfaceSource">Surface that trims of BrepFace will be applied to.</param>
        /// <param name="tolerance"></param>
        /// <returns>A brep with the shape of surfaceSource and the trims of trimSource or null on failure.</returns>
        public static async Task<Brep> CreateTrimmedSurface(BrepFace trimSource, Surface surfaceSource, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), trimSource, surfaceSource, tolerance);
        }

        /// <summary>
        /// Constructs a Brep using the trimming information of a brep face and a surface. 
        /// Surface must be roughly the same shape and in the same location as the trimming brep face.
        /// </summary>
        /// <param name="trimSource">BrepFace which contains trimmingSource brep.</param>
        /// <param name="surfaceSource">Surface that trims of BrepFace will be applied to.</param>
        /// <param name="tolerance"></param>
        /// <returns>A brep with the shape of surfaceSource and the trims of trimSource or null on failure.</returns>
        public static async Task<Brep> CreateTrimmedSurface(BrepFace trimSource, Remote<Surface> surfaceSource, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), trimSource, surfaceSource, tolerance);
        }

        /// <summary>
        /// Makes a Brep with one face from three corner points.
        /// </summary>
        /// <param name="corner1">A first corner.</param>
        /// <param name="corner2">A second corner.</param>
        /// <param name="corner3">A third corner.</param>
        /// <param name="tolerance">
        /// Minimum edge length allowed before collapsing the side into a singularity.
        /// </param>
        /// <returns>A boundary representation, or null on error.</returns>
        public static async Task<Brep> CreateFromCornerPoints(Point3d corner1, Point3d corner2, Point3d corner3, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), corner1, corner2, corner3, tolerance);
        }

        /// <summary>
        /// Makes a Brep with one face from four corner points.
        /// </summary>
        /// <param name="corner1">A first corner.</param>
        /// <param name="corner2">A second corner.</param>
        /// <param name="corner3">A third corner.</param>
        /// <param name="corner4">A fourth corner.</param>
        /// <param name="tolerance">
        /// Minimum edge length allowed before collapsing the side into a singularity.
        /// </param>
        /// <returns>A boundary representation, or null on error.</returns>
        public static async Task<Brep> CreateFromCornerPoints(Point3d corner1, Point3d corner2, Point3d corner3, Point3d corner4, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), corner1, corner2, corner3, corner4, tolerance);
        }

        /// <summary>
        /// Constructs a coons patch from 2, 3, or 4 curves.
        /// </summary>
        /// <param name="curves">A list, an array or any enumerable set of curves.</param>
        /// <returns>resulting brep or null on failure.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_edgesrf.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_edgesrf.cs' lang='cs'/>
        /// <code source='examples\py\ex_edgesrf.py' lang='py'/>
        /// </example>
        public static async Task<Brep> CreateEdgeSurface(IEnumerable<Curve> curves)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), curves);
        }

        /// <summary>
        /// Constructs a coons patch from 2, 3, or 4 curves.
        /// </summary>
        /// <param name="curves">A list, an array or any enumerable set of curves.</param>
        /// <returns>resulting brep or null on failure.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_edgesrf.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_edgesrf.cs' lang='cs'/>
        /// <code source='examples\py\ex_edgesrf.py' lang='py'/>
        /// </example>
        public static async Task<Brep> CreateEdgeSurface(Remote<IEnumerable<Curve>> curves)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), curves);
        }

        /// <summary>
        /// Constructs a set of planar Breps as outlines by the loops.
        /// </summary>
        /// <param name="inputLoops">Curve loops that delineate the planar boundaries.</param>
        /// <returns>An array of Planar Breps or null on error.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Brep[]> CreatePlanarBreps(Rhino.Collections.CurveList inputLoops)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), inputLoops);
        }

        /// <summary>
        /// Constructs a set of planar Breps as outlines by the loops.
        /// </summary>
        /// <param name="inputLoops">Curve loops that delineate the planar boundaries.</param>
        /// <param name="tolerance"></param>
        /// <returns>An array of Planar Breps.</returns>
        public static async Task<Brep[]> CreatePlanarBreps(Rhino.Collections.CurveList inputLoops, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), inputLoops, tolerance);
        }

        /// <summary>
        /// Offsets a face including trim information to create a new brep.
        /// </summary>
        /// <param name="face">the face to offset.</param>
        /// <param name="offsetDistance">An offset distance.</param>
        /// <param name="offsetTolerance">
        ///  Use 0.0 to make a loose offset. Otherwise, the document's absolute tolerance is usually sufficient.
        /// </param>
        /// <param name="bothSides">When true, offset to both sides of the input face.</param>
        /// <param name="createSolid">When true, make a solid object.</param>
        /// <returns>
        /// A new brep if successful. The brep can be disjoint if bothSides is true and createSolid is false,
        /// or if createSolid is true and connecting the offsets with side surfaces fails.
        /// null if unsuccessful.
        /// </returns>
        public static async Task<Brep> CreateFromOffsetFace(BrepFace face, double offsetDistance, double offsetTolerance, bool bothSides, bool createSolid)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), face, offsetDistance, offsetTolerance, bothSides, createSolid);
        }

        /// <summary>
        /// Constructs closed polysurfaces from surfaces and polysurfaces that bound a region in space.
        /// </summary>
        /// <param name="breps">
        /// The intersecting surfaces and polysurfaces to automatically trim and join into closed polysurfaces.
        /// </param>
        /// <param name="tolerance">
        /// The trim and join tolerance. If set to RhinoMath.UnsetValue, Rhino's global absolute tolerance is used.
        /// </param>
        /// <returns>The resulting polysurfaces on success or null on failure.</returns>
        public static async Task<Brep[]> CreateSolid(IEnumerable<Brep> breps, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), breps, tolerance);
        }

        /// <summary>
        /// Constructs closed polysurfaces from surfaces and polysurfaces that bound a region in space.
        /// </summary>
        /// <param name="breps">
        /// The intersecting surfaces and polysurfaces to automatically trim and join into closed polysurfaces.
        /// </param>
        /// <param name="tolerance">
        /// The trim and join tolerance. If set to RhinoMath.UnsetValue, Rhino's global absolute tolerance is used.
        /// </param>
        /// <returns>The resulting polysurfaces on success or null on failure.</returns>
        public static async Task<Brep[]> CreateSolid(Remote<IEnumerable<Brep>> breps, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), breps, tolerance);
        }

        /// <summary>
        /// Merges two surfaces into one surface at untrimmed edges.
        /// </summary>
        /// <param name="surface0">The first surface to merge.</param>
        /// <param name="surface1">The second surface to merge.</param>
        /// <param name="tolerance">Surface edges must be within this tolerance for the two surfaces to merge.</param>
        /// <param name="angleToleranceRadians">Edge must be within this angle tolerance in order for contiguous edges to be combined into a single edge.</param>
        /// <returns>The merged surfaces as a Brep if successful, null if not successful.</returns>
        public static async Task<Brep> MergeSurfaces(Surface surface0, Surface surface1, double tolerance, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), surface0, surface1, tolerance, angleToleranceRadians);
        }

        /// <summary>
        /// Merges two surfaces into one surface at untrimmed edges.
        /// </summary>
        /// <param name="surface0">The first surface to merge.</param>
        /// <param name="surface1">The second surface to merge.</param>
        /// <param name="tolerance">Surface edges must be within this tolerance for the two surfaces to merge.</param>
        /// <param name="angleToleranceRadians">Edge must be within this angle tolerance in order for contiguous edges to be combined into a single edge.</param>
        /// <returns>The merged surfaces as a Brep if successful, null if not successful.</returns>
        public static async Task<Brep> MergeSurfaces(Remote<Surface> surface0, Remote<Surface> surface1, double tolerance, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), surface0, surface1, tolerance, angleToleranceRadians);
        }

        /// <summary>
        /// Merges two surfaces into one surface at untrimmed edges. Both surfaces must be untrimmed and share an edge.
        /// </summary>
        /// <param name="brep0">The first single-face Brep to merge.</param>
        /// <param name="brep1">The second single-face Brep to merge.</param>
        /// <param name="tolerance">Surface edges must be within this tolerance for the two surfaces to merge.</param>
        /// <param name="angleToleranceRadians">Edge must be within this angle tolerance in order for contiguous edges to be combined into a single edge.</param>
        /// <returns>The merged Brep if successful, null if not successful.</returns>
        public static async Task<Brep> MergeSurfaces(Brep brep0, Brep brep1, double tolerance, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), brep0, brep1, tolerance, angleToleranceRadians);
        }

        /// <summary>
        /// Merges two surfaces into one surface at untrimmed edges. Both surfaces must be untrimmed and share an edge.
        /// </summary>
        /// <param name="brep0">The first single-face Brep to merge.</param>
        /// <param name="brep1">The second single-face Brep to merge.</param>
        /// <param name="tolerance">Surface edges must be within this tolerance for the two surfaces to merge.</param>
        /// <param name="angleToleranceRadians">Edge must be within this angle tolerance in order for contiguous edges to be combined into a single edge.</param>
        /// <returns>The merged Brep if successful, null if not successful.</returns>
        public static async Task<Brep> MergeSurfaces(Remote<Brep> brep0, Remote<Brep> brep1, double tolerance, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), brep0, brep1, tolerance, angleToleranceRadians);
        }

        /// <summary>
        /// Merges two surfaces into one surface at untrimmed edges. Both surfaces must be untrimmed and share an edge.
        /// </summary>
        /// <param name="brep0">The first single-face Brep to merge.</param>
        /// <param name="brep1">The second single-face Brep to merge.</param>
        /// <param name="tolerance">Surface edges must be within this tolerance for the two surfaces to merge.</param>
        /// <param name="angleToleranceRadians">Edge must be within this angle tolerance in order for contiguous edges to be combined into a single edge.</param>
        /// <param name="point0">2D pick point on the first single-face Brep. The value can be unset.</param>
        /// <param name="point1">2D pick point on the second single-face Brep. The value can be unset.</param>
        /// <param name="roundness">Defines the roundness of the merge. Acceptable values are between 0.0 (sharp) and 1.0 (smooth).</param>
        /// <param name="smooth">The surface will be smooth. This makes the surface behave better for control point editing, but may alter the shape of both surfaces.</param>
        /// <returns>The merged Brep if successful, null if not successful.</returns>
        public static async Task<Brep> MergeSurfaces(Brep brep0, Brep brep1, double tolerance, double angleToleranceRadians, Point2d point0, Point2d point1, double roundness, bool smooth)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), brep0, brep1, tolerance, angleToleranceRadians, point0, point1, roundness, smooth);
        }

        /// <summary>
        /// Merges two surfaces into one surface at untrimmed edges. Both surfaces must be untrimmed and share an edge.
        /// </summary>
        /// <param name="brep0">The first single-face Brep to merge.</param>
        /// <param name="brep1">The second single-face Brep to merge.</param>
        /// <param name="tolerance">Surface edges must be within this tolerance for the two surfaces to merge.</param>
        /// <param name="angleToleranceRadians">Edge must be within this angle tolerance in order for contiguous edges to be combined into a single edge.</param>
        /// <param name="point0">2D pick point on the first single-face Brep. The value can be unset.</param>
        /// <param name="point1">2D pick point on the second single-face Brep. The value can be unset.</param>
        /// <param name="roundness">Defines the roundness of the merge. Acceptable values are between 0.0 (sharp) and 1.0 (smooth).</param>
        /// <param name="smooth">The surface will be smooth. This makes the surface behave better for control point editing, but may alter the shape of both surfaces.</param>
        /// <returns>The merged Brep if successful, null if not successful.</returns>
        public static async Task<Brep> MergeSurfaces(Remote<Brep> brep0, Remote<Brep> brep1, double tolerance, double angleToleranceRadians, Point2d point0, Point2d point1, double roundness, bool smooth)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), brep0, brep1, tolerance, angleToleranceRadians, point0, point1, roundness, smooth);
        }

        /// <summary>
        /// Constructs a brep patch.
        /// <para>This is the simple version of fit that uses a specified starting surface.</para>
        /// </summary>
        /// <param name="geometry">
        /// Combination of Curves, BrepTrims, Points, PointClouds or Meshes.
        /// Curves and trims are sampled to get points. Trims are sampled for
        /// points and normals.
        /// </param>
        /// <param name="startingSurface">A starting surface (can be null).</param>
        /// <param name="tolerance">
        /// Tolerance used by input analysis functions for loop finding, trimming, etc.
        /// </param>
        /// <returns>
        /// Brep fit through input on success, or null on error.
        /// </returns>
        public static async Task<Brep> CreatePatch(IEnumerable<GeometryBase> geometry, Surface startingSurface, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), geometry, startingSurface, tolerance);
        }

        /// <summary>
        /// Constructs a brep patch.
        /// <para>This is the simple version of fit that uses a specified starting surface.</para>
        /// </summary>
        /// <param name="geometry">
        /// Combination of Curves, BrepTrims, Points, PointClouds or Meshes.
        /// Curves and trims are sampled to get points. Trims are sampled for
        /// points and normals.
        /// </param>
        /// <param name="startingSurface">A starting surface (can be null).</param>
        /// <param name="tolerance">
        /// Tolerance used by input analysis functions for loop finding, trimming, etc.
        /// </param>
        /// <returns>
        /// Brep fit through input on success, or null on error.
        /// </returns>
        public static async Task<Brep> CreatePatch(Remote<IEnumerable<GeometryBase>> geometry, Remote<Surface> startingSurface, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), geometry, startingSurface, tolerance);
        }

        /// <summary>
        /// Constructs a brep patch.
        /// <para>This is the simple version of fit that uses a plane with u x v spans.
        /// It makes a plane by fitting to the points from the input geometry to use as the starting surface.
        /// The surface has the specified u and v span count.</para>
        /// </summary>
        /// <param name="geometry">
        /// A combination of <see cref="Curve">curves</see>, brep trims,
        /// <see cref="Point">points</see>, <see cref="PointCloud">point clouds</see> or <see cref="Mesh">meshes</see>.
        /// Curves and trims are sampled to get points. Trims are sampled for
        /// points and normals.
        /// </param>
        /// <param name="uSpans">The number of spans in the U direction.</param>
        /// <param name="vSpans">The number of spans in the V direction.</param>
        /// <param name="tolerance">
        /// Tolerance used by input analysis functions for loop finding, trimming, etc.
        /// </param>
        /// <returns>
        /// A brep fit through input on success, or null on error.
        /// </returns>
        public static async Task<Brep> CreatePatch(IEnumerable<GeometryBase> geometry, int uSpans, int vSpans, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), geometry, uSpans, vSpans, tolerance);
        }

        /// <summary>
        /// Constructs a brep patch.
        /// <para>This is the simple version of fit that uses a plane with u x v spans.
        /// It makes a plane by fitting to the points from the input geometry to use as the starting surface.
        /// The surface has the specified u and v span count.</para>
        /// </summary>
        /// <param name="geometry">
        /// A combination of <see cref="Curve">curves</see>, brep trims,
        /// <see cref="Point">points</see>, <see cref="PointCloud">point clouds</see> or <see cref="Mesh">meshes</see>.
        /// Curves and trims are sampled to get points. Trims are sampled for
        /// points and normals.
        /// </param>
        /// <param name="uSpans">The number of spans in the U direction.</param>
        /// <param name="vSpans">The number of spans in the V direction.</param>
        /// <param name="tolerance">
        /// Tolerance used by input analysis functions for loop finding, trimming, etc.
        /// </param>
        /// <returns>
        /// A brep fit through input on success, or null on error.
        /// </returns>
        public static async Task<Brep> CreatePatch(Remote<IEnumerable<GeometryBase>> geometry, int uSpans, int vSpans, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), geometry, uSpans, vSpans, tolerance);
        }

        /// <summary>
        /// Constructs a brep patch using all controls
        /// </summary>
        /// <param name="geometry">
        /// A combination of <see cref="Curve">curves</see>, brep trims,
        /// <see cref="Point">points</see>, <see cref="PointCloud">point clouds</see> or <see cref="Mesh">meshes</see>.
        /// Curves and trims are sampled to get points. Trims are sampled for
        /// points and normals.
        /// </param>
        /// <param name="startingSurface">A starting surface (can be null).</param>
        /// <param name="uSpans">
        /// Number of surface spans used when a plane is fit through points to start in the U direction.
        /// </param>
        /// <param name="vSpans">
        /// Number of surface spans used when a plane is fit through points to start in the U direction.
        /// </param>
        /// <param name="trim">
        /// If true, try to find an outer loop from among the input curves and trim the result to that loop
        /// </param>
        /// <param name="tangency">
        /// If true, try to find brep trims in the outer loop of curves and try to
        /// fit to the normal direction of the trim's surface at those locations.
        /// </param>
        /// <param name="pointSpacing">
        /// Basic distance between points sampled from input curves.
        /// </param>
        /// <param name="flexibility">
        /// Determines the behavior of the surface in areas where its not otherwise
        /// controlled by the input.  Lower numbers make the surface behave more
        /// like a stiff material; higher, less like a stiff material.  That is,
        /// each span is made to more closely match the spans adjacent to it if there
        /// is no input geometry mapping to that area of the surface when the
        /// flexibility value is low.  The scale is logarithmic. Numbers around 0.001
        /// or 0.1 make the patch pretty stiff and numbers around 10 or 100 make the
        /// surface flexible.
        /// </param>
        /// <param name="surfacePull">
        /// Tends to keep the result surface where it was before the fit in areas where
        /// there is on influence from the input geometry
        /// </param>
        /// <param name="fixEdges">
        /// Array of four elements. Flags to keep the edges of a starting (untrimmed)
        /// surface in place while fitting the interior of the surface.  Order of
        /// flags is left, bottom, right, top
        ///</param>
        /// <param name="tolerance">
        /// Tolerance used by input analysis functions for loop finding, trimming, etc.
        /// </param>
        /// <returns>
        /// A brep fit through input on success, or null on error.
        /// </returns>
        public static async Task<Brep> CreatePatch(IEnumerable<GeometryBase> geometry, Surface startingSurface, int uSpans, int vSpans, bool trim, bool tangency, double pointSpacing, double flexibility, double surfacePull, bool[] fixEdges, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), geometry, startingSurface, uSpans, vSpans, trim, tangency, pointSpacing, flexibility, surfacePull, fixEdges, tolerance);
        }

        /// <summary>
        /// Constructs a brep patch using all controls
        /// </summary>
        /// <param name="geometry">
        /// A combination of <see cref="Curve">curves</see>, brep trims,
        /// <see cref="Point">points</see>, <see cref="PointCloud">point clouds</see> or <see cref="Mesh">meshes</see>.
        /// Curves and trims are sampled to get points. Trims are sampled for
        /// points and normals.
        /// </param>
        /// <param name="startingSurface">A starting surface (can be null).</param>
        /// <param name="uSpans">
        /// Number of surface spans used when a plane is fit through points to start in the U direction.
        /// </param>
        /// <param name="vSpans">
        /// Number of surface spans used when a plane is fit through points to start in the U direction.
        /// </param>
        /// <param name="trim">
        /// If true, try to find an outer loop from among the input curves and trim the result to that loop
        /// </param>
        /// <param name="tangency">
        /// If true, try to find brep trims in the outer loop of curves and try to
        /// fit to the normal direction of the trim's surface at those locations.
        /// </param>
        /// <param name="pointSpacing">
        /// Basic distance between points sampled from input curves.
        /// </param>
        /// <param name="flexibility">
        /// Determines the behavior of the surface in areas where its not otherwise
        /// controlled by the input.  Lower numbers make the surface behave more
        /// like a stiff material; higher, less like a stiff material.  That is,
        /// each span is made to more closely match the spans adjacent to it if there
        /// is no input geometry mapping to that area of the surface when the
        /// flexibility value is low.  The scale is logarithmic. Numbers around 0.001
        /// or 0.1 make the patch pretty stiff and numbers around 10 or 100 make the
        /// surface flexible.
        /// </param>
        /// <param name="surfacePull">
        /// Tends to keep the result surface where it was before the fit in areas where
        /// there is on influence from the input geometry
        /// </param>
        /// <param name="fixEdges">
        /// Array of four elements. Flags to keep the edges of a starting (untrimmed)
        /// surface in place while fitting the interior of the surface.  Order of
        /// flags is left, bottom, right, top
        ///</param>
        /// <param name="tolerance">
        /// Tolerance used by input analysis functions for loop finding, trimming, etc.
        /// </param>
        /// <returns>
        /// A brep fit through input on success, or null on error.
        /// </returns>
        public static async Task<Brep> CreatePatch(Remote<IEnumerable<GeometryBase>> geometry, Remote<Surface> startingSurface, int uSpans, int vSpans, bool trim, bool tangency, double pointSpacing, double flexibility, double surfacePull, bool[] fixEdges, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), geometry, startingSurface, uSpans, vSpans, trim, tangency, pointSpacing, flexibility, surfacePull, fixEdges, tolerance);
        }

        /// <summary>
        /// Creates a single walled pipe.
        /// </summary>
        /// <param name="rail">The rail, or path, curve.</param>
        /// <param name="radius">The radius of the pipe.</param>
        /// <param name="localBlending">
        /// The shape blending.
        /// If True, Local (pipe radius stays constant at the ends and changes more rapidly in the middle) is applied.
        /// If False, Global (radius is linearly blended from one end to the other, creating pipes that taper from one radius to the other) is applied.
        /// </param>
        /// <param name="cap">The end cap mode.</param>
        /// <param name="fitRail">
        /// If the curve is a polycurve of lines and arcs, the curve is fit and a single surface is created;
        /// otherwise the result is a Brep with joined surfaces created from the polycurve segments.
        /// </param>
        /// <param name="absoluteTolerance">
        /// The sweeping and fitting tolerance. When in doubt, use the document's absolute tolerance.
        /// </param>
        /// <param name="angleToleranceRadians">
        /// The angle tolerance. When in doubt, use the document's angle tolerance in radians.
        /// </param>
        /// <returns>Array of Breps success.</returns>
        public static async Task<Brep[]> CreatePipe(Curve rail, double radius, bool localBlending, PipeCapMode cap, bool fitRail, double absoluteTolerance, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail, radius, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians);
        }

        /// <summary>
        /// Creates a single walled pipe.
        /// </summary>
        /// <param name="rail">The rail, or path, curve.</param>
        /// <param name="radius">The radius of the pipe.</param>
        /// <param name="localBlending">
        /// The shape blending.
        /// If True, Local (pipe radius stays constant at the ends and changes more rapidly in the middle) is applied.
        /// If False, Global (radius is linearly blended from one end to the other, creating pipes that taper from one radius to the other) is applied.
        /// </param>
        /// <param name="cap">The end cap mode.</param>
        /// <param name="fitRail">
        /// If the curve is a polycurve of lines and arcs, the curve is fit and a single surface is created;
        /// otherwise the result is a Brep with joined surfaces created from the polycurve segments.
        /// </param>
        /// <param name="absoluteTolerance">
        /// The sweeping and fitting tolerance. When in doubt, use the document's absolute tolerance.
        /// </param>
        /// <param name="angleToleranceRadians">
        /// The angle tolerance. When in doubt, use the document's angle tolerance in radians.
        /// </param>
        /// <returns>Array of Breps success.</returns>
        public static async Task<Brep[]> CreatePipe(Remote<Curve> rail, double radius, bool localBlending, PipeCapMode cap, bool fitRail, double absoluteTolerance, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail, radius, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians);
        }

        /// <summary>
        /// Creates a single walled pipe.
        /// </summary>
        /// <param name="rail">The rail, or path, curve.</param>
        /// <param name="railRadiiParameters">
        /// One or more normalized curve parameters where changes in radius occur.
        /// Important: curve parameters must be normalized - ranging between 0.0 and 1.0.
        /// Use Interval.NormalizedParameterAt to calculate these.
        /// </param>
        /// <param name="radii">One or more radii - one at each normalized curve parameter in railRadiiParameters.</param>
        /// <param name="localBlending">
        /// The shape blending.
        /// If True, Local (pipe radius stays constant at the ends and changes more rapidly in the middle) is applied.
        /// If False, Global (radius is linearly blended from one end to the other, creating pipes that taper from one radius to the other) is applied.
        /// </param>
        /// <param name="cap">The end cap mode.</param>
        /// <param name="fitRail">
        /// If the curve is a polycurve of lines and arcs, the curve is fit and a single surface is created;
        /// otherwise the result is a Brep with joined surfaces created from the polycurve segments.
        /// </param>
        /// <param name="absoluteTolerance">
        /// The sweeping and fitting tolerance. When in doubt, use the document's absolute tolerance.
        /// </param>
        /// <param name="angleToleranceRadians">
        /// The angle tolerance. When in doubt, use the document's angle tolerance in radians.
        /// </param>
        /// <returns>Array of Breps success.</returns>
        public static async Task<Brep[]> CreatePipe(Curve rail, IEnumerable<double> railRadiiParameters, IEnumerable<double> radii, bool localBlending, PipeCapMode cap, bool fitRail, double absoluteTolerance, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail, railRadiiParameters, radii, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians);
        }

        /// <summary>
        /// Creates a single walled pipe.
        /// </summary>
        /// <param name="rail">The rail, or path, curve.</param>
        /// <param name="railRadiiParameters">
        /// One or more normalized curve parameters where changes in radius occur.
        /// Important: curve parameters must be normalized - ranging between 0.0 and 1.0.
        /// Use Interval.NormalizedParameterAt to calculate these.
        /// </param>
        /// <param name="radii">One or more radii - one at each normalized curve parameter in railRadiiParameters.</param>
        /// <param name="localBlending">
        /// The shape blending.
        /// If True, Local (pipe radius stays constant at the ends and changes more rapidly in the middle) is applied.
        /// If False, Global (radius is linearly blended from one end to the other, creating pipes that taper from one radius to the other) is applied.
        /// </param>
        /// <param name="cap">The end cap mode.</param>
        /// <param name="fitRail">
        /// If the curve is a polycurve of lines and arcs, the curve is fit and a single surface is created;
        /// otherwise the result is a Brep with joined surfaces created from the polycurve segments.
        /// </param>
        /// <param name="absoluteTolerance">
        /// The sweeping and fitting tolerance. When in doubt, use the document's absolute tolerance.
        /// </param>
        /// <param name="angleToleranceRadians">
        /// The angle tolerance. When in doubt, use the document's angle tolerance in radians.
        /// </param>
        /// <returns>Array of Breps success.</returns>
        public static async Task<Brep[]> CreatePipe(Remote<Curve> rail, IEnumerable<double> railRadiiParameters, IEnumerable<double> radii, bool localBlending, PipeCapMode cap, bool fitRail, double absoluteTolerance, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail, railRadiiParameters, radii, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians);
        }

        /// <summary>
        /// Creates a double-walled pipe.
        /// </summary>
        /// <param name="rail">The rail, or path, curve.</param>
        /// <param name="radius0">The first radius of the pipe.</param>
        /// <param name="radius1">The second radius of the pipe.</param>
        /// <param name="localBlending">
        /// The shape blending.
        /// If True, Local (pipe radius stays constant at the ends and changes more rapidly in the middle) is applied.
        /// If False, Global (radius is linearly blended from one end to the other, creating pipes that taper from one radius to the other) is applied.
        /// </param>
        /// <param name="cap">The end cap mode.</param>
        /// <param name="fitRail">
        /// If the curve is a polycurve of lines and arcs, the curve is fit and a single surface is created;
        /// otherwise the result is a Brep with joined surfaces created from the polycurve segments.
        /// </param>
        /// <param name="absoluteTolerance">
        /// The sweeping and fitting tolerance. When in doubt, use the document's absolute tolerance.
        /// </param>
        /// <param name="angleToleranceRadians">
        /// The angle tolerance. When in doubt, use the document's angle tolerance in radians.
        /// </param>
        /// <returns>Array of Breps success.</returns>
        public static async Task<Brep[]> CreateThickPipe(Curve rail, double radius0, double radius1, bool localBlending, PipeCapMode cap, bool fitRail, double absoluteTolerance, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail, radius0, radius1, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians);
        }

        /// <summary>
        /// Creates a double-walled pipe.
        /// </summary>
        /// <param name="rail">The rail, or path, curve.</param>
        /// <param name="radius0">The first radius of the pipe.</param>
        /// <param name="radius1">The second radius of the pipe.</param>
        /// <param name="localBlending">
        /// The shape blending.
        /// If True, Local (pipe radius stays constant at the ends and changes more rapidly in the middle) is applied.
        /// If False, Global (radius is linearly blended from one end to the other, creating pipes that taper from one radius to the other) is applied.
        /// </param>
        /// <param name="cap">The end cap mode.</param>
        /// <param name="fitRail">
        /// If the curve is a polycurve of lines and arcs, the curve is fit and a single surface is created;
        /// otherwise the result is a Brep with joined surfaces created from the polycurve segments.
        /// </param>
        /// <param name="absoluteTolerance">
        /// The sweeping and fitting tolerance. When in doubt, use the document's absolute tolerance.
        /// </param>
        /// <param name="angleToleranceRadians">
        /// The angle tolerance. When in doubt, use the document's angle tolerance in radians.
        /// </param>
        /// <returns>Array of Breps success.</returns>
        public static async Task<Brep[]> CreateThickPipe(Remote<Curve> rail, double radius0, double radius1, bool localBlending, PipeCapMode cap, bool fitRail, double absoluteTolerance, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail, radius0, radius1, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians);
        }

        /// <summary>
        /// Creates a double-walled pipe.
        /// </summary>
        /// <param name="rail">The rail, or path, curve.</param>
        /// <param name="railRadiiParameters">
        /// One or more normalized curve parameters where changes in radius occur.
        /// Important: curve parameters must be normalized - ranging between 0.0 and 1.0.
        /// Use Interval.NormalizedParameterAt to calculate these.
        /// </param>
        /// <param name="radii0">
        /// One or more radii for the first wall - one at each normalized curve parameter in railRadiiParameters.
        /// </param>
        /// <param name="radii1">
        /// One or more radii for the second wall - one at each normalized curve parameter in railRadiiParameters.
        /// </param>
        /// <param name="localBlending">
        /// The shape blending.
        /// If True, Local (pipe radius stays constant at the ends and changes more rapidly in the middle) is applied.
        /// If False, Global (radius is linearly blended from one end to the other, creating pipes that taper from one radius to the other) is applied.
        /// </param>
        /// <param name="cap">The end cap mode.</param>
        /// <param name="fitRail">
        /// If the curve is a polycurve of lines and arcs, the curve is fit and a single surface is created;
        /// otherwise the result is a Brep with joined surfaces created from the polycurve segments.
        /// </param>
        /// <param name="absoluteTolerance">
        /// The sweeping and fitting tolerance. When in doubt, use the document's absolute tolerance.
        /// </param>
        /// <param name="angleToleranceRadians">
        /// The angle tolerance. When in doubt, use the document's angle tolerance in radians.
        /// </param>
        /// <returns>Array of Breps success.</returns>
        public static async Task<Brep[]> CreateThickPipe(Curve rail, IEnumerable<double> railRadiiParameters, IEnumerable<double> radii0, IEnumerable<double> radii1, bool localBlending, PipeCapMode cap, bool fitRail, double absoluteTolerance, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail, railRadiiParameters, radii0, radii1, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians);
        }

        /// <summary>
        /// Creates a double-walled pipe.
        /// </summary>
        /// <param name="rail">The rail, or path, curve.</param>
        /// <param name="railRadiiParameters">
        /// One or more normalized curve parameters where changes in radius occur.
        /// Important: curve parameters must be normalized - ranging between 0.0 and 1.0.
        /// Use Interval.NormalizedParameterAt to calculate these.
        /// </param>
        /// <param name="radii0">
        /// One or more radii for the first wall - one at each normalized curve parameter in railRadiiParameters.
        /// </param>
        /// <param name="radii1">
        /// One or more radii for the second wall - one at each normalized curve parameter in railRadiiParameters.
        /// </param>
        /// <param name="localBlending">
        /// The shape blending.
        /// If True, Local (pipe radius stays constant at the ends and changes more rapidly in the middle) is applied.
        /// If False, Global (radius is linearly blended from one end to the other, creating pipes that taper from one radius to the other) is applied.
        /// </param>
        /// <param name="cap">The end cap mode.</param>
        /// <param name="fitRail">
        /// If the curve is a polycurve of lines and arcs, the curve is fit and a single surface is created;
        /// otherwise the result is a Brep with joined surfaces created from the polycurve segments.
        /// </param>
        /// <param name="absoluteTolerance">
        /// The sweeping and fitting tolerance. When in doubt, use the document's absolute tolerance.
        /// </param>
        /// <param name="angleToleranceRadians">
        /// The angle tolerance. When in doubt, use the document's angle tolerance in radians.
        /// </param>
        /// <returns>Array of Breps success.</returns>
        public static async Task<Brep[]> CreateThickPipe(Remote<Curve> rail, IEnumerable<double> railRadiiParameters, IEnumerable<double> radii0, IEnumerable<double> radii1, bool localBlending, PipeCapMode cap, bool fitRail, double absoluteTolerance, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail, railRadiiParameters, radii0, radii1, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians);
        }

        /// <summary>
        /// Sweep1 function that fits a surface through a profile curve that define the surface cross-sections
        /// and one curve that defines a surface edge. 
        /// </summary>
        /// <param name="rail">Rail to sweep shapes along</param>
        /// <param name="shape">Shape curve</param>
        /// <param name="closed">Only matters if shape is closed</param>
        /// <param name="tolerance">Tolerance for fitting surface and rails</param>
        /// <returns>Array of Brep sweep results</returns>
        public static async Task<Brep[]> CreateFromSweep(Curve rail, Curve shape, bool closed, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail, shape, closed, tolerance);
        }

        /// <summary>
        /// Sweep1 function that fits a surface through a profile curve that define the surface cross-sections
        /// and one curve that defines a surface edge. 
        /// </summary>
        /// <param name="rail">Rail to sweep shapes along</param>
        /// <param name="shape">Shape curve</param>
        /// <param name="closed">Only matters if shape is closed</param>
        /// <param name="tolerance">Tolerance for fitting surface and rails</param>
        /// <returns>Array of Brep sweep results</returns>
        public static async Task<Brep[]> CreateFromSweep(Remote<Curve> rail, Remote<Curve> shape, bool closed, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail, shape, closed, tolerance);
        }

        /// <summary>
        /// Sweep1 function that fits a surface through profile curves that define the surface cross-sections
        /// and one curve that defines a surface edge.
        /// </summary>
        /// <param name="rail">Rail to sweep shapes along</param>
        /// <param name="shapes">Shape curves</param>
        /// <param name="closed">Only matters if shapes are closed</param>
        /// <param name="tolerance">Tolerance for fitting surface and rails</param>
        /// <returns>Array of Brep sweep results</returns>
        public static async Task<Brep[]> CreateFromSweep(Curve rail, IEnumerable<Curve> shapes, bool closed, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail, shapes, closed, tolerance);
        }

        /// <summary>
        /// Sweep1 function that fits a surface through profile curves that define the surface cross-sections
        /// and one curve that defines a surface edge.
        /// </summary>
        /// <param name="rail">Rail to sweep shapes along</param>
        /// <param name="shapes">Shape curves</param>
        /// <param name="closed">Only matters if shapes are closed</param>
        /// <param name="tolerance">Tolerance for fitting surface and rails</param>
        /// <returns>Array of Brep sweep results</returns>
        public static async Task<Brep[]> CreateFromSweep(Remote<Curve> rail, Remote<IEnumerable<Curve>> shapes, bool closed, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail, shapes, closed, tolerance);
        }

        /// <summary>
        /// Sweep1 function that fits a surface through a series of profile curves that define the surface cross-sections
        /// and one curve that defines a surface edge.
        /// </summary>
        /// <param name="rail">Rail to sweep shapes along.</param>
        /// <param name="shapes">Shape curves.</param>
        /// <param name="startPoint">Optional starting point of sweep. Use Point3d.Unset if you do not want to include a start point.</param>
        /// <param name="endPoint">Optional ending point of sweep. Use Point3d.Unset if you do not want to include an end point.</param>
        /// <param name="frameType">The frame type.</param>
        /// <param name="roadlikeNormal">The roadlike normal directoion. Use Vector3d.Unset if the frame type is not set to roadlike.</param>
        /// <param name="closed">Only matters if shapes are closed.</param>
        /// <param name="blendType">The shape blending type.</param>
        /// <param name="miterType">The mitering type.</param>
        /// <param name="tolerance"></param>
        /// <param name="rebuildType">The rebuild style.</param>
        /// <param name="rebuildPointCount">If rebuild == SweepRebuild.Rebuild, the number of points. Otherwise specify 0.</param>
        /// <param name="refitTolerance">If rebuild == SweepRebuild.Refit, the refit tolerance. Otherwise, specify 0.0</param>
        /// <returns>Array of Brep sweep results.</returns>
        public static async Task<Brep[]> CreateFromSweep(Curve rail, IEnumerable<Curve> shapes, Point3d startPoint, Point3d endPoint, SweepFrame frameType, Vector3d roadlikeNormal, bool closed, SweepBlend blendType, SweepMiter miterType, double tolerance, SweepRebuild rebuildType, int rebuildPointCount, double refitTolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail, shapes, startPoint, endPoint, frameType, roadlikeNormal, closed, blendType, miterType, tolerance, rebuildType, rebuildPointCount, refitTolerance);
        }

        /// <summary>
        /// Sweep1 function that fits a surface through a series of profile curves that define the surface cross-sections
        /// and one curve that defines a surface edge.
        /// </summary>
        /// <param name="rail">Rail to sweep shapes along.</param>
        /// <param name="shapes">Shape curves.</param>
        /// <param name="startPoint">Optional starting point of sweep. Use Point3d.Unset if you do not want to include a start point.</param>
        /// <param name="endPoint">Optional ending point of sweep. Use Point3d.Unset if you do not want to include an end point.</param>
        /// <param name="frameType">The frame type.</param>
        /// <param name="roadlikeNormal">The roadlike normal directoion. Use Vector3d.Unset if the frame type is not set to roadlike.</param>
        /// <param name="closed">Only matters if shapes are closed.</param>
        /// <param name="blendType">The shape blending type.</param>
        /// <param name="miterType">The mitering type.</param>
        /// <param name="tolerance"></param>
        /// <param name="rebuildType">The rebuild style.</param>
        /// <param name="rebuildPointCount">If rebuild == SweepRebuild.Rebuild, the number of points. Otherwise specify 0.</param>
        /// <param name="refitTolerance">If rebuild == SweepRebuild.Refit, the refit tolerance. Otherwise, specify 0.0</param>
        /// <returns>Array of Brep sweep results.</returns>
        public static async Task<Brep[]> CreateFromSweep(Remote<Curve> rail, Remote<IEnumerable<Curve>> shapes, Point3d startPoint, Point3d endPoint, SweepFrame frameType, Vector3d roadlikeNormal, bool closed, SweepBlend blendType, SweepMiter miterType, double tolerance, SweepRebuild rebuildType, int rebuildPointCount, double refitTolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail, shapes, startPoint, endPoint, frameType, roadlikeNormal, closed, blendType, miterType, tolerance, rebuildType, rebuildPointCount, refitTolerance);
        }

        /// <summary>
        /// Sweep1 function that fits a surface through a profile curve that define the surface cross-sections
        /// and one curve that defines a surface edge. The Segmented version breaks the rail at curvature kinks
        /// and sweeps each piece separately, then put the results together into a Brep.
        /// </summary>
        /// <param name="rail">Rail to sweep shapes along</param>
        /// <param name="shape">Shape curve</param>
        /// <param name="closed">Only matters if shape is closed</param>
        /// <param name="tolerance">Tolerance for fitting surface and rails</param>
        /// <returns>Array of Brep sweep results</returns>
        public static async Task<Brep[]> CreateFromSweepSegmented(Curve rail, Curve shape, bool closed, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail, shape, closed, tolerance);
        }

        /// <summary>
        /// Sweep1 function that fits a surface through a profile curve that define the surface cross-sections
        /// and one curve that defines a surface edge. The Segmented version breaks the rail at curvature kinks
        /// and sweeps each piece separately, then put the results together into a Brep.
        /// </summary>
        /// <param name="rail">Rail to sweep shapes along</param>
        /// <param name="shape">Shape curve</param>
        /// <param name="closed">Only matters if shape is closed</param>
        /// <param name="tolerance">Tolerance for fitting surface and rails</param>
        /// <returns>Array of Brep sweep results</returns>
        public static async Task<Brep[]> CreateFromSweepSegmented(Remote<Curve> rail, Remote<Curve> shape, bool closed, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail, shape, closed, tolerance);
        }

        /// <summary>
        /// Sweep1 function that fits a surface through a series of profile curves that define the surface cross-sections
        /// and one curve that defines a surface edge. The Segmented version breaks the rail at curvature kinks
        /// and sweeps each piece separately, then put the results together into a Brep.
        /// </summary>
        /// <param name="rail">Rail to sweep shapes along.</param>
        /// <param name="shapes">Shape curves.</param>
        /// <param name="closed">Only matters if shapes are closed.</param>
        /// <param name="tolerance">Tolerance for fitting surface and rails.</param>
        /// <returns>Array of Brep sweep results.</returns>
        public static async Task<Brep[]> CreateFromSweepSegmented(Curve rail, IEnumerable<Curve> shapes, bool closed, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail, shapes, closed, tolerance);
        }

        /// <summary>
        /// Sweep1 function that fits a surface through a series of profile curves that define the surface cross-sections
        /// and one curve that defines a surface edge. The Segmented version breaks the rail at curvature kinks
        /// and sweeps each piece separately, then put the results together into a Brep.
        /// </summary>
        /// <param name="rail">Rail to sweep shapes along.</param>
        /// <param name="shapes">Shape curves.</param>
        /// <param name="closed">Only matters if shapes are closed.</param>
        /// <param name="tolerance">Tolerance for fitting surface and rails.</param>
        /// <returns>Array of Brep sweep results.</returns>
        public static async Task<Brep[]> CreateFromSweepSegmented(Remote<Curve> rail, Remote<IEnumerable<Curve>> shapes, bool closed, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail, shapes, closed, tolerance);
        }

        /// <summary>
        /// Sweep1 function that fits a surface through a series of profile curves that define the surface cross-sections
        /// and one curve that defines a surface edge. The Segmented version breaks the rail at curvature kinks
        /// and sweeps each piece separately, then put the results together into a Brep.
        /// </summary>
        /// <param name="rail">Rail to sweep shapes along.</param>
        /// <param name="shapes">Shape curves.</param>
        /// <param name="startPoint">Optional starting point of sweep. Use Point3d.Unset if you do not want to include a start point.</param>
        /// <param name="endPoint">Optional ending point of sweep. Use Point3d.Unset if you do not want to include an end point.</param>
        /// <param name="frameType">The frame type.</param>
        /// <param name="roadlikeNormal">The roadlike normal directoion. Use Vector3d.Unset if the frame type is not set to roadlike.</param>
        /// <param name="closed">Only matters if shapes are closed.</param>
        /// <param name="blendType">The shape blending type.</param>
        /// <param name="miterType">The mitering type.</param>
        /// <param name="tolerance"></param>
        /// <param name="rebuildType">The rebuild style.</param>
        /// <param name="rebuildPointCount">If rebuild == SweepRebuild.Rebuild, the number of points. Otherwise specify 0.</param>
        /// <param name="refitTolerance">If rebuild == SweepRebuild.Refit, the refit tolerance. Otherwise, specify 0.0</param>
        /// <returns>Array of Brep sweep results.</returns>
        public static async Task<Brep[]> CreateFromSweepSegmented(Curve rail, IEnumerable<Curve> shapes, Point3d startPoint, Point3d endPoint, SweepFrame frameType, Vector3d roadlikeNormal, bool closed, SweepBlend blendType, SweepMiter miterType, double tolerance, SweepRebuild rebuildType, int rebuildPointCount, double refitTolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail, shapes, startPoint, endPoint, frameType, roadlikeNormal, closed, blendType, miterType, tolerance, rebuildType, rebuildPointCount, refitTolerance);
        }

        /// <summary>
        /// Sweep1 function that fits a surface through a series of profile curves that define the surface cross-sections
        /// and one curve that defines a surface edge. The Segmented version breaks the rail at curvature kinks
        /// and sweeps each piece separately, then put the results together into a Brep.
        /// </summary>
        /// <param name="rail">Rail to sweep shapes along.</param>
        /// <param name="shapes">Shape curves.</param>
        /// <param name="startPoint">Optional starting point of sweep. Use Point3d.Unset if you do not want to include a start point.</param>
        /// <param name="endPoint">Optional ending point of sweep. Use Point3d.Unset if you do not want to include an end point.</param>
        /// <param name="frameType">The frame type.</param>
        /// <param name="roadlikeNormal">The roadlike normal directoion. Use Vector3d.Unset if the frame type is not set to roadlike.</param>
        /// <param name="closed">Only matters if shapes are closed.</param>
        /// <param name="blendType">The shape blending type.</param>
        /// <param name="miterType">The mitering type.</param>
        /// <param name="tolerance"></param>
        /// <param name="rebuildType">The rebuild style.</param>
        /// <param name="rebuildPointCount">If rebuild == SweepRebuild.Rebuild, the number of points. Otherwise specify 0.</param>
        /// <param name="refitTolerance">If rebuild == SweepRebuild.Refit, the refit tolerance. Otherwise, specify 0.0</param>
        /// <returns>Array of Brep sweep results.</returns>
        public static async Task<Brep[]> CreateFromSweepSegmented(Remote<Curve> rail, Remote<IEnumerable<Curve>> shapes, Point3d startPoint, Point3d endPoint, SweepFrame frameType, Vector3d roadlikeNormal, bool closed, SweepBlend blendType, SweepMiter miterType, double tolerance, SweepRebuild rebuildType, int rebuildPointCount, double refitTolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail, shapes, startPoint, endPoint, frameType, roadlikeNormal, closed, blendType, miterType, tolerance, rebuildType, rebuildPointCount, refitTolerance);
        }

        /// <summary>
        /// General 2 rail sweep. If you are not producing the sweep results that you are after, then
        /// use the SweepTwoRail class with options to generate the swept geometry.
        /// </summary>
        /// <param name="rail1">Rail to sweep shapes along</param>
        /// <param name="rail2">Rail to sweep shapes along</param>
        /// <param name="shape">Shape curve</param>
        /// <param name="closed">Only matters if shape is closed</param>
        /// <param name="tolerance">Tolerance for fitting surface and rails</param>
        /// <returns>Array of Brep sweep results</returns>
        public static async Task<Brep[]> CreateFromSweep(Curve rail1, Curve rail2, Curve shape, bool closed, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail1, rail2, shape, closed, tolerance);
        }

        /// <summary>
        /// General 2 rail sweep. If you are not producing the sweep results that you are after, then
        /// use the SweepTwoRail class with options to generate the swept geometry.
        /// </summary>
        /// <param name="rail1">Rail to sweep shapes along</param>
        /// <param name="rail2">Rail to sweep shapes along</param>
        /// <param name="shape">Shape curve</param>
        /// <param name="closed">Only matters if shape is closed</param>
        /// <param name="tolerance">Tolerance for fitting surface and rails</param>
        /// <returns>Array of Brep sweep results</returns>
        public static async Task<Brep[]> CreateFromSweep(Remote<Curve> rail1, Remote<Curve> rail2, Remote<Curve> shape, bool closed, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail1, rail2, shape, closed, tolerance);
        }

        /// <summary>
        /// General 2 rail sweep. If you are not producing the sweep results that you are after, then
        /// use the SweepTwoRail class with options to generate the swept geometry.
        /// </summary>
        /// <param name="rail1">Rail to sweep shapes along</param>
        /// <param name="rail2">Rail to sweep shapes along</param>
        /// <param name="shapes">Shape curves</param>
        /// <param name="closed">Only matters if shapes are closed</param>
        /// <param name="tolerance">Tolerance for fitting surface and rails</param>
        /// <returns>Array of Brep sweep results</returns>
        public static async Task<Brep[]> CreateFromSweep(Curve rail1, Curve rail2, IEnumerable<Curve> shapes, bool closed, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail1, rail2, shapes, closed, tolerance);
        }

        /// <summary>
        /// General 2 rail sweep. If you are not producing the sweep results that you are after, then
        /// use the SweepTwoRail class with options to generate the swept geometry.
        /// </summary>
        /// <param name="rail1">Rail to sweep shapes along</param>
        /// <param name="rail2">Rail to sweep shapes along</param>
        /// <param name="shapes">Shape curves</param>
        /// <param name="closed">Only matters if shapes are closed</param>
        /// <param name="tolerance">Tolerance for fitting surface and rails</param>
        /// <returns>Array of Brep sweep results</returns>
        public static async Task<Brep[]> CreateFromSweep(Remote<Curve> rail1, Remote<Curve> rail2, Remote<IEnumerable<Curve>> shapes, bool closed, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail1, rail2, shapes, closed, tolerance);
        }

        /// <summary>
        /// Sweep2 function that fits a surface through profile curves that define the surface cross-sections
        /// and two curves that defines the surface edges.
        /// </summary>
        /// <param name="rail1">Rail to sweep shapes along</param>
        /// <param name="rail2">Rail to sweep shapes along</param>
        /// <param name="shapes">Shape curves</param>
        /// <param name="start">Optional starting point of sweep. Use Point3d.Unset if you do not want to include a start point.</param>
        /// <param name="end">Optional ending point of sweep. Use Point3d.Unset if you do not want to include an end point.</param>
        /// <param name="closed">Only matters if shapes are closed.</param>
        /// <param name="tolerance">Tolerance for fitting surface and rails.</param>
        /// <param name="rebuild">The rebuild style.</param>
        /// <param name="rebuildPointCount">If rebuild == SweepRebuild.Rebuild, the number of points. Otherwise specify 0.</param>
        /// <param name="refitTolerance">If rebuild == SweepRebuild.Refit, the refit tolerance. Otherwise, specify 0.0</param>
        /// <param name="preserveHeight">Removes the association between the height scaling from the width scaling</param>
        /// <returns>Array of Brep sweep results</returns>
        public static async Task<Brep[]> CreateFromSweep(Curve rail1, Curve rail2, IEnumerable<Curve> shapes, Point3d start, Point3d end, bool closed, double tolerance, SweepRebuild rebuild, int rebuildPointCount, double refitTolerance, bool preserveHeight)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail1, rail2, shapes, start, end, closed, tolerance, rebuild, rebuildPointCount, refitTolerance, preserveHeight);
        }

        /// <summary>
        /// Sweep2 function that fits a surface through profile curves that define the surface cross-sections
        /// and two curves that defines the surface edges.
        /// </summary>
        /// <param name="rail1">Rail to sweep shapes along</param>
        /// <param name="rail2">Rail to sweep shapes along</param>
        /// <param name="shapes">Shape curves</param>
        /// <param name="start">Optional starting point of sweep. Use Point3d.Unset if you do not want to include a start point.</param>
        /// <param name="end">Optional ending point of sweep. Use Point3d.Unset if you do not want to include an end point.</param>
        /// <param name="closed">Only matters if shapes are closed.</param>
        /// <param name="tolerance">Tolerance for fitting surface and rails.</param>
        /// <param name="rebuild">The rebuild style.</param>
        /// <param name="rebuildPointCount">If rebuild == SweepRebuild.Rebuild, the number of points. Otherwise specify 0.</param>
        /// <param name="refitTolerance">If rebuild == SweepRebuild.Refit, the refit tolerance. Otherwise, specify 0.0</param>
        /// <param name="preserveHeight">Removes the association between the height scaling from the width scaling</param>
        /// <returns>Array of Brep sweep results</returns>
        public static async Task<Brep[]> CreateFromSweep(Remote<Curve> rail1, Remote<Curve> rail2, Remote<IEnumerable<Curve>> shapes, Point3d start, Point3d end, bool closed, double tolerance, SweepRebuild rebuild, int rebuildPointCount, double refitTolerance, bool preserveHeight)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail1, rail2, shapes, start, end, closed, tolerance, rebuild, rebuildPointCount, refitTolerance, preserveHeight);
        }

        /// <summary>
        /// Sweep2 function that fits a surface through profile curves that define the surface cross-sections
        /// and two curves that defines the surface edges.
        /// </summary>
        /// <param name="rail1">Rail to sweep shapes along</param>
        /// <param name="rail2">Rail to sweep shapes along</param>
        /// <param name="shapes">Shape curves</param>
        /// <param name="start">Optional starting point of sweep. Use Point3d.Unset if you do not want to include a start point.</param>
        /// <param name="end">Optional ending point of sweep. Use Point3d.Unset if you do not want to include an end point.</param>
        /// <param name="closed">Only matters if shapes are closed.</param>
        /// <param name="tolerance">Tolerance for fitting surface and rails.</param>
        /// <param name="rebuild">The rebuild style.</param>
        /// <param name="rebuildPointCount">If rebuild == SweepRebuild.Rebuild, the number of points. Otherwise specify 0.</param>
        /// <param name="refitTolerance">If rebuild == SweepRebuild.Refit, the refit tolerance. Otherwise, specify 0.0</param>
        /// <param name="preserveHeight">Removes the association between the height scaling from the width scaling</param>
        /// <param name="autoAdjust">
        /// Set to true to have shape curves adjusted, sorted, and matched automatically.
        /// This will produce results comparable to Rhino's Sweep2 command.
        /// Set to false to not have shape curves adjusted, sorted, and matched automatically.
        /// </param>
        /// <returns>Array of Brep sweep results</returns>
        public static async Task<Brep[]> CreateFromSweep(Curve rail1, Curve rail2, IEnumerable<Curve> shapes, Point3d start, Point3d end, bool closed, double tolerance, SweepRebuild rebuild, int rebuildPointCount, double refitTolerance, bool preserveHeight, bool autoAdjust)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail1, rail2, shapes, start, end, closed, tolerance, rebuild, rebuildPointCount, refitTolerance, preserveHeight, autoAdjust);
        }

        /// <summary>
        /// Sweep2 function that fits a surface through profile curves that define the surface cross-sections
        /// and two curves that defines the surface edges.
        /// </summary>
        /// <param name="rail1">Rail to sweep shapes along</param>
        /// <param name="rail2">Rail to sweep shapes along</param>
        /// <param name="shapes">Shape curves</param>
        /// <param name="start">Optional starting point of sweep. Use Point3d.Unset if you do not want to include a start point.</param>
        /// <param name="end">Optional ending point of sweep. Use Point3d.Unset if you do not want to include an end point.</param>
        /// <param name="closed">Only matters if shapes are closed.</param>
        /// <param name="tolerance">Tolerance for fitting surface and rails.</param>
        /// <param name="rebuild">The rebuild style.</param>
        /// <param name="rebuildPointCount">If rebuild == SweepRebuild.Rebuild, the number of points. Otherwise specify 0.</param>
        /// <param name="refitTolerance">If rebuild == SweepRebuild.Refit, the refit tolerance. Otherwise, specify 0.0</param>
        /// <param name="preserveHeight">Removes the association between the height scaling from the width scaling</param>
        /// <param name="autoAdjust">
        /// Set to true to have shape curves adjusted, sorted, and matched automatically.
        /// This will produce results comparable to Rhino's Sweep2 command.
        /// Set to false to not have shape curves adjusted, sorted, and matched automatically.
        /// </param>
        /// <returns>Array of Brep sweep results</returns>
        public static async Task<Brep[]> CreateFromSweep(Remote<Curve> rail1, Remote<Curve> rail2, Remote<IEnumerable<Curve>> shapes, Point3d start, Point3d end, bool closed, double tolerance, SweepRebuild rebuild, int rebuildPointCount, double refitTolerance, bool preserveHeight, bool autoAdjust)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail1, rail2, shapes, start, end, closed, tolerance, rebuild, rebuildPointCount, refitTolerance, preserveHeight, autoAdjust);
        }

        /// <summary>
        /// Makes a 2 rail sweep. Like CreateFromSweep but the result is split where parameterization along a rail changes abruptly.
        /// </summary>
        /// <param name="rail1">Rail to sweep shapes along</param>
        /// <param name="rail2">Rail to sweep shapes along</param>
        /// <param name="shapes">Shape curves</param>        /// <param name="rail_params">Shape parameters</param>
        /// <param name="closed">Only matters if shapes are closed</param>
        /// <param name="tolerance">Tolerance for fitting surface and rails</param>
        /// <returns>Array of Brep sweep results</returns>
        public static async Task<Brep[]> CreateFromSweepInParts(Curve rail1, Curve rail2, IEnumerable<Curve> shapes, IEnumerable<Point2d> rail_params, bool closed, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail1, rail2, shapes, rail_params, closed, tolerance);
        }

        /// <summary>
        /// Makes a 2 rail sweep. Like CreateFromSweep but the result is split where parameterization along a rail changes abruptly.
        /// </summary>
        /// <param name="rail1">Rail to sweep shapes along</param>
        /// <param name="rail2">Rail to sweep shapes along</param>
        /// <param name="shapes">Shape curves</param>        /// <param name="rail_params">Shape parameters</param>
        /// <param name="closed">Only matters if shapes are closed</param>
        /// <param name="tolerance">Tolerance for fitting surface and rails</param>
        /// <returns>Array of Brep sweep results</returns>
        public static async Task<Brep[]> CreateFromSweepInParts(Remote<Curve> rail1, Remote<Curve> rail2, Remote<IEnumerable<Curve>> shapes, IEnumerable<Point2d> rail_params, bool closed, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), rail1, rail2, shapes, rail_params, closed, tolerance);
        }

        /// <summary>
        /// Extrude a curve to a taper making a brep (potentially more than 1)
        /// </summary>
        /// <param name="curveToExtrude">the curve to extrude</param>
        /// <param name="distance">the distance to extrude</param>
        /// <param name="direction">the direction of the extrusion</param>
        /// <param name="basePoint">the base point of the extrusion</param>
        /// <param name="draftAngleRadians">angle of the extrusion</param>
        /// <param name="cornerType"></param>
        /// <param name="tolerance">tolerance to use for the extrusion</param>
        /// <param name="angleToleranceRadians">angle tolerance to use for the extrusion</param>
        /// <returns>array of breps on success</returns>
        public static async Task<Brep[]> CreateFromTaperedExtrude(Curve curveToExtrude, double distance, Vector3d direction, Point3d basePoint, double draftAngleRadians, ExtrudeCornerType cornerType, double tolerance, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), curveToExtrude, distance, direction, basePoint, draftAngleRadians, cornerType, tolerance, angleToleranceRadians);
        }

        /// <summary>
        /// Extrude a curve to a taper making a brep (potentially more than 1)
        /// </summary>
        /// <param name="curveToExtrude">the curve to extrude</param>
        /// <param name="distance">the distance to extrude</param>
        /// <param name="direction">the direction of the extrusion</param>
        /// <param name="basePoint">the base point of the extrusion</param>
        /// <param name="draftAngleRadians">angle of the extrusion</param>
        /// <param name="cornerType"></param>
        /// <param name="tolerance">tolerance to use for the extrusion</param>
        /// <param name="angleToleranceRadians">angle tolerance to use for the extrusion</param>
        /// <returns>array of breps on success</returns>
        public static async Task<Brep[]> CreateFromTaperedExtrude(Remote<Curve> curveToExtrude, double distance, Vector3d direction, Point3d basePoint, double draftAngleRadians, ExtrudeCornerType cornerType, double tolerance, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), curveToExtrude, distance, direction, basePoint, draftAngleRadians, cornerType, tolerance, angleToleranceRadians);
        }

        /// <summary>
        /// Extrude a curve to a taper making a brep (potentially more than 1)
        /// </summary>
        /// <param name="curveToExtrude">the curve to extrude</param>
        /// <param name="distance">the distance to extrude</param>
        /// <param name="direction">the direction of the extrusion</param>
        /// <param name="basePoint">the base point of the extrusion</param>
        /// <param name="draftAngleRadians">angle of the extrusion</param>
        /// <param name="cornerType"></param>
        /// <returns>array of breps on success</returns>
        /// <remarks>tolerances used are based on the active doc tolerance</remarks>
        /// <deprecated>6.0</deprecated>
        public static async Task<Brep[]> CreateFromTaperedExtrude(Curve curveToExtrude, double distance, Vector3d direction, Point3d basePoint, double draftAngleRadians, ExtrudeCornerType cornerType)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), curveToExtrude, distance, direction, basePoint, draftAngleRadians, cornerType);
        }

        /// <summary>
        /// Extrude a curve to a taper making a brep (potentially more than 1)
        /// </summary>
        /// <param name="curveToExtrude">the curve to extrude</param>
        /// <param name="distance">the distance to extrude</param>
        /// <param name="direction">the direction of the extrusion</param>
        /// <param name="basePoint">the base point of the extrusion</param>
        /// <param name="draftAngleRadians">angle of the extrusion</param>
        /// <param name="cornerType"></param>
        /// <returns>array of breps on success</returns>
        /// <remarks>tolerances used are based on the active doc tolerance</remarks>
        /// <deprecated>6.0</deprecated>
        public static async Task<Brep[]> CreateFromTaperedExtrude(Remote<Curve> curveToExtrude, double distance, Vector3d direction, Point3d basePoint, double draftAngleRadians, ExtrudeCornerType cornerType)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), curveToExtrude, distance, direction, basePoint, draftAngleRadians, cornerType);
        }

        /// <summary>
        /// Creates one or more Breps by extruding a curve a distance along an axis with draft angle.
        /// </summary>
        /// <param name="curve">The curve to extrude.</param>
        /// <param name="direction">The extrusion direction.</param>
        /// <param name="distance">The extrusion distance.</param>
        /// <param name="draftAngle">The extrusion draft angle in radians.</param>
        /// <param name="plane">
        /// The end of the extrusion will be parallel to this plane, and "distance" from the plane's origin.
        /// The plane's origin is generally be a point on the curve. For planar curves, a natural choice for the
        /// plane's normal direction will be the normal direction of the curve's plane. In any case, 
        /// plane.Normal = direction may make sense.
        /// </param>
        /// <param name="tolerance">The intersecting and trimming tolerance.</param>
        /// <returns>An array of Breps if successful.</returns>
        public static async Task<Brep[]> CreateFromTaperedExtrudeWithRef(Curve curve, Vector3d direction, double distance, double draftAngle, Plane plane, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), curve, direction, distance, draftAngle, plane, tolerance);
        }

        /// <summary>
        /// Creates one or more Breps by extruding a curve a distance along an axis with draft angle.
        /// </summary>
        /// <param name="curve">The curve to extrude.</param>
        /// <param name="direction">The extrusion direction.</param>
        /// <param name="distance">The extrusion distance.</param>
        /// <param name="draftAngle">The extrusion draft angle in radians.</param>
        /// <param name="plane">
        /// The end of the extrusion will be parallel to this plane, and "distance" from the plane's origin.
        /// The plane's origin is generally be a point on the curve. For planar curves, a natural choice for the
        /// plane's normal direction will be the normal direction of the curve's plane. In any case, 
        /// plane.Normal = direction may make sense.
        /// </param>
        /// <param name="tolerance">The intersecting and trimming tolerance.</param>
        /// <returns>An array of Breps if successful.</returns>
        public static async Task<Brep[]> CreateFromTaperedExtrudeWithRef(Remote<Curve> curve, Vector3d direction, double distance, double draftAngle, Plane plane, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), curve, direction, distance, draftAngle, plane, tolerance);
        }

        /// <summary>
        /// Makes a surface blend between two surface edges.
        /// </summary>
        /// <param name="face0">First face to blend from.</param>
        /// <param name="edge0">First edge to blend from.</param>
        /// <param name="domain0">The domain of edge0 to use.</param>
        /// <param name="rev0">If false, edge0 will be used in its natural direction. If true, edge0 will be used in the reversed direction.</param>
        /// <param name="continuity0">Continuity for the blend at the start.</param>
        /// <param name="face1">Second face to blend from.</param>
        /// <param name="edge1">Second edge to blend from.</param>
        /// <param name="domain1">The domain of edge1 to use.</param>
        /// <param name="rev1">If false, edge1 will be used in its natural direction. If true, edge1 will be used in the reversed direction.</param>
        /// <param name="continuity1">Continuity for the blend at the end.</param>
        /// <returns>Array of Breps if successful.</returns>
        public static async Task<Brep[]> CreateBlendSurface(BrepFace face0, BrepEdge edge0, Interval domain0, bool rev0, BlendContinuity continuity0, BrepFace face1, BrepEdge edge1, Interval domain1, bool rev1, BlendContinuity continuity1)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), face0, edge0, domain0, rev0, continuity0, face1, edge1, domain1, rev1, continuity1);
        }

        /// <summary>
        /// Makes a curve blend between points on two surface edges. The blend will be tangent to the surfaces and perpendicular to the edges.
        /// </summary>
        /// <param name="face0">First face to blend from.</param>
        /// <param name="edge0">First edge to blend from.</param>
        /// <param name="t0">Location on first edge for first end of blend curve.</param>
        /// <param name="rev0">If false, edge0 will be used in its natural direction. If true, edge0 will be used in the reversed direction.</param>
        /// <param name="continuity0">Continuity for the blend at the start.</param>
        /// <param name="face1">Second face to blend from.</param>
        /// <param name="edge1">Second edge to blend from.</param>
        /// <param name="t1">Location on second edge for second end of blend curve.</param>
        /// <param name="rev1">If false, edge1 will be used in its natural direction. If true, edge1 will be used in the reversed direction.</param>
        /// <param name="continuity1">>Continuity for the blend at the end.</param>
        /// <returns>The blend curve on success. null on failure</returns>
        public static async Task<Curve> CreateBlendShape(BrepFace face0, BrepEdge edge0, double t0, bool rev0, BlendContinuity continuity0, BrepFace face1, BrepEdge edge1, double t1, bool rev1, BlendContinuity continuity1)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), face0, edge0, t0, rev0, continuity0, face1, edge1, t1, rev1, continuity1);
        }

        /// <summary>
        /// Creates a constant-radius round surface between two surfaces.
        /// </summary>
        /// <param name="face0">First face to fillet from.</param>
        /// <param name="uv0">A parameter face0 at the side you want to keep after filleting.</param>
        /// <param name="face1">Second face to fillet from.</param>
        /// <param name="uv1">A parameter face1 at the side you want to keep after filleting.</param>
        /// <param name="radius">The fillet radius.</param>
        /// <param name="extend">If true, then when one input surface is longer than the other, the fillet surface is extended to the input surface edges.</param>
        /// <param name="tolerance">The tolerance. When in doubt, use the document's model absolute tolerance.</param>
        /// <returns>Array of Breps if successful.</returns>
        public static async Task<Brep[]> CreateFilletSurface(BrepFace face0, Point2d uv0, BrepFace face1, Point2d uv1, double radius, bool extend, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), face0, uv0, face1, uv1, radius, extend, tolerance);
        }

        /// <summary>
        /// Creates a ruled surface as a bevel between two input surface edges.
        /// </summary>
        /// <param name="face0">First face to chamfer from.</param>
        /// <param name="uv0">A parameter face0 at the side you want to keep after chamfering.</param>
        /// <param name="radius0">The distance from the intersection of face0 to the edge of the chamfer.</param>
        /// <param name="face1">Second face to chamfer from.</param>
        /// <param name="uv1">A parameter face1 at the side you want to keep after chamfering.</param>
        /// <param name="radius1">The distance from the intersection of face1 to the edge of the chamfer.</param>
        /// <param name="extend">If true, then when one input surface is longer than the other, the chamfer surface is extended to the input surface edges.</param>
        /// <param name="tolerance">The tolerance. When in doubt, use the document's model absolute tolerance.</param>
        /// <returns>Array of Breps if successful.</returns>
        public static async Task<Brep[]> CreateChamferSurface(BrepFace face0, Point2d uv0, double radius0, BrepFace face1, Point2d uv1, double radius1, bool extend, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), face0, uv0, radius0, face1, uv1, radius1, extend, tolerance);
        }

        /// <summary>
        /// Fillets, chamfers, or blends the edges of a brep.
        /// </summary>
        /// <param name="brep">The brep to fillet, chamfer, or blend edges.</param>
        /// <param name="edgeIndices">An array of one or more edge indices where the fillet, chamfer, or blend will occur.</param>
        /// <param name="startRadii">An array of starting fillet, chamfer, or blend radaii, one for each edge index.</param>
        /// <param name="endRadii">An array of ending fillet, chamfer, or blend radaii, one for each edge index.</param>
        /// <param name="blendType">The blend type.</param>
        /// <param name="railType">The rail type.</param>
        /// <param name="tolerance">The tolerance to be used to perform calculations.</param>
        /// <returns>Array of Breps if successful.</returns>
        public static async Task<Brep[]> CreateFilletEdges(Brep brep, IEnumerable<int> edgeIndices, IEnumerable<double> startRadii, IEnumerable<double> endRadii, BlendType blendType, RailType railType, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brep, edgeIndices, startRadii, endRadii, blendType, railType, tolerance);
        }

        /// <summary>
        /// Fillets, chamfers, or blends the edges of a brep.
        /// </summary>
        /// <param name="brep">The brep to fillet, chamfer, or blend edges.</param>
        /// <param name="edgeIndices">An array of one or more edge indices where the fillet, chamfer, or blend will occur.</param>
        /// <param name="startRadii">An array of starting fillet, chamfer, or blend radaii, one for each edge index.</param>
        /// <param name="endRadii">An array of ending fillet, chamfer, or blend radaii, one for each edge index.</param>
        /// <param name="blendType">The blend type.</param>
        /// <param name="railType">The rail type.</param>
        /// <param name="tolerance">The tolerance to be used to perform calculations.</param>
        /// <returns>Array of Breps if successful.</returns>
        public static async Task<Brep[]> CreateFilletEdges(Remote<Brep> brep, IEnumerable<int> edgeIndices, IEnumerable<double> startRadii, IEnumerable<double> endRadii, BlendType blendType, RailType railType, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brep, edgeIndices, startRadii, endRadii, blendType, railType, tolerance);
        }

        /// <summary>
        /// Fillets, chamfers, or blends the edges of a brep.
        /// </summary>
        /// <param name="brep">The brep to fillet, chamfer, or blend edges.</param>
        /// <param name="edgeIndices">An array of one or more edge indices where the fillet, chamfer, or blend will occur.</param>
        /// <param name="startRadii">An array of starting fillet, chamfer, or blend radaii, one for each edge index.</param>
        /// <param name="endRadii">An array of ending fillet, chamfer, or blend radaii, one for each edge index.</param>
        /// <param name="blendType">The blend type.</param>
        /// <param name="railType">The rail type.</param>
        /// <param name="setbackFillets">UJse setback fillets (only used with blendType=<see cref="BlendType.Blend"/>)</param>
        /// <param name="tolerance">The tolerance to be used to perform calculations.</param>
        /// <param name="angleTolerance">Angle tolerance to be used to perform calculations [radians].</param>
        /// <returns>Array of Breps if successful.</returns>
        public static async Task<Brep[]> CreateFilletEdges(Brep brep, IEnumerable<int> edgeIndices, IEnumerable<double> startRadii, IEnumerable<double> endRadii, BlendType blendType, RailType railType, bool setbackFillets, double tolerance, double angleTolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brep, edgeIndices, startRadii, endRadii, blendType, railType, setbackFillets, tolerance, angleTolerance);
        }

        /// <summary>
        /// Fillets, chamfers, or blends the edges of a brep.
        /// </summary>
        /// <param name="brep">The brep to fillet, chamfer, or blend edges.</param>
        /// <param name="edgeIndices">An array of one or more edge indices where the fillet, chamfer, or blend will occur.</param>
        /// <param name="startRadii">An array of starting fillet, chamfer, or blend radaii, one for each edge index.</param>
        /// <param name="endRadii">An array of ending fillet, chamfer, or blend radaii, one for each edge index.</param>
        /// <param name="blendType">The blend type.</param>
        /// <param name="railType">The rail type.</param>
        /// <param name="setbackFillets">UJse setback fillets (only used with blendType=<see cref="BlendType.Blend"/>)</param>
        /// <param name="tolerance">The tolerance to be used to perform calculations.</param>
        /// <param name="angleTolerance">Angle tolerance to be used to perform calculations [radians].</param>
        /// <returns>Array of Breps if successful.</returns>
        public static async Task<Brep[]> CreateFilletEdges(Remote<Brep> brep, IEnumerable<int> edgeIndices, IEnumerable<double> startRadii, IEnumerable<double> endRadii, BlendType blendType, RailType railType, bool setbackFillets, double tolerance, double angleTolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brep, edgeIndices, startRadii, endRadii, blendType, railType, setbackFillets, tolerance, angleTolerance);
        }

        /// <summary>
        /// Joins two naked edges, or edges that are coincident or close together, from two Breps.
        /// </summary>
        /// <param name="brep0">The first Brep.</param>
        /// <param name="edgeIndex0">The edge index on the first Brep.</param>
        /// <param name="brep1">The second Brep.</param>
        /// <param name="edgeIndex1">The edge index on the second Brep.</param>
        /// <param name="joinTolerance">The join tolerance.</param>
        /// <returns>The resulting Brep if successful, null on failure.</returns>
        public static async Task<Brep> CreateFromJoinedEdges(Brep brep0, int edgeIndex0, Brep brep1, int edgeIndex1, double joinTolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), brep0, edgeIndex0, brep1, edgeIndex1, joinTolerance);
        }

        /// <summary>
        /// Joins two naked edges, or edges that are coincident or close together, from two Breps.
        /// </summary>
        /// <param name="brep0">The first Brep.</param>
        /// <param name="edgeIndex0">The edge index on the first Brep.</param>
        /// <param name="brep1">The second Brep.</param>
        /// <param name="edgeIndex1">The edge index on the second Brep.</param>
        /// <param name="joinTolerance">The join tolerance.</param>
        /// <returns>The resulting Brep if successful, null on failure.</returns>
        public static async Task<Brep> CreateFromJoinedEdges(Remote<Brep> brep0, int edgeIndex0, Remote<Brep> brep1, int edgeIndex1, double joinTolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), brep0, edgeIndex0, brep1, edgeIndex1, joinTolerance);
        }

        /// <summary>
        /// Constructs one or more Breps by lofting through a set of curves.
        /// </summary>
        /// <param name="curves">
        /// The curves to loft through. This function will not perform any curve sorting. You must pass in
        /// curves in the order you want them lofted. This function will not adjust the directions of open
        /// curves. Use Curve.DoDirectionsMatch and Curve.Reverse to adjust the directions of open curves.
        /// This function will not adjust the seams of closed curves. Use Curve.ChangeClosedCurveSeam to
        /// adjust the seam of closed curves.
        /// </param>
        /// <param name="start">
        /// Optional starting point of loft. Use Point3d.Unset if you do not want to include a start point.
        /// </param>
        /// <param name="end">
        /// Optional ending point of loft. Use Point3d.Unset if you do not want to include an end point.
        /// </param>
        /// <param name="loftType">type of loft to perform.</param>
        /// <param name="closed">true if the last curve in this loft should be connected back to the first one.</param>
        /// <returns>
        /// Constructs a closed surface, continuing the surface past the last curve around to the
        /// first curve. Available when you have selected three shape curves.
        /// </returns>
        /// <example>
        /// <code source='examples\vbnet\ex_loft.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_loft.cs' lang='cs'/>
        /// <code source='examples\py\ex_loft.py' lang='py'/>
        /// </example>
        public static async Task<Brep[]> CreateFromLoft(IEnumerable<Curve> curves, Point3d start, Point3d end, LoftType loftType, bool closed)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), curves, start, end, loftType, closed);
        }

        /// <summary>
        /// Constructs one or more Breps by lofting through a set of curves.
        /// </summary>
        /// <param name="curves">
        /// The curves to loft through. This function will not perform any curve sorting. You must pass in
        /// curves in the order you want them lofted. This function will not adjust the directions of open
        /// curves. Use Curve.DoDirectionsMatch and Curve.Reverse to adjust the directions of open curves.
        /// This function will not adjust the seams of closed curves. Use Curve.ChangeClosedCurveSeam to
        /// adjust the seam of closed curves.
        /// </param>
        /// <param name="start">
        /// Optional starting point of loft. Use Point3d.Unset if you do not want to include a start point.
        /// </param>
        /// <param name="end">
        /// Optional ending point of loft. Use Point3d.Unset if you do not want to include an end point.
        /// </param>
        /// <param name="loftType">type of loft to perform.</param>
        /// <param name="closed">true if the last curve in this loft should be connected back to the first one.</param>
        /// <returns>
        /// Constructs a closed surface, continuing the surface past the last curve around to the
        /// first curve. Available when you have selected three shape curves.
        /// </returns>
        /// <example>
        /// <code source='examples\vbnet\ex_loft.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_loft.cs' lang='cs'/>
        /// <code source='examples\py\ex_loft.py' lang='py'/>
        /// </example>
        public static async Task<Brep[]> CreateFromLoft(Remote<IEnumerable<Curve>> curves, Point3d start, Point3d end, LoftType loftType, bool closed)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), curves, start, end, loftType, closed);
        }

        /// <summary>
        /// Constructs one or more Breps by lofting through a set of curves. Input for the loft is simplified by
        /// rebuilding to a specified number of control points.
        /// </summary>
        /// <param name="curves">
        /// The curves to loft through. This function will not perform any curve sorting. You must pass in
        /// curves in the order you want them lofted. This function will not adjust the directions of open
        /// curves. Use Curve.DoDirectionsMatch and Curve.Reverse to adjust the directions of open curves.
        /// This function will not adjust the seams of closed curves. Use Curve.ChangeClosedCurveSeam to
        /// adjust the seam of closed curves.
        /// </param>
        /// <param name="start">
        /// Optional starting point of loft. Use Point3d.Unset if you do not want to include a start point.
        /// </param>
        /// <param name="end">
        /// Optional ending point of lost. Use Point3d.Unset if you do not want to include an end point.
        /// </param>
        /// <param name="loftType">type of loft to perform.</param>
        /// <param name="closed">true if the last curve in this loft should be connected back to the first one.</param>
        /// <param name="rebuildPointCount">A number of points to use while rebuilding the curves. 0 leaves turns this parameter off.</param>
        /// <returns>
        /// Constructs a closed surface, continuing the surface past the last curve around to the
        /// first curve. Available when you have selected three shape curves.
        /// </returns>
        public static async Task<Brep[]> CreateFromLoftRebuild(IEnumerable<Curve> curves, Point3d start, Point3d end, LoftType loftType, bool closed, int rebuildPointCount)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), curves, start, end, loftType, closed, rebuildPointCount);
        }

        /// <summary>
        /// Constructs one or more Breps by lofting through a set of curves. Input for the loft is simplified by
        /// rebuilding to a specified number of control points.
        /// </summary>
        /// <param name="curves">
        /// The curves to loft through. This function will not perform any curve sorting. You must pass in
        /// curves in the order you want them lofted. This function will not adjust the directions of open
        /// curves. Use Curve.DoDirectionsMatch and Curve.Reverse to adjust the directions of open curves.
        /// This function will not adjust the seams of closed curves. Use Curve.ChangeClosedCurveSeam to
        /// adjust the seam of closed curves.
        /// </param>
        /// <param name="start">
        /// Optional starting point of loft. Use Point3d.Unset if you do not want to include a start point.
        /// </param>
        /// <param name="end">
        /// Optional ending point of lost. Use Point3d.Unset if you do not want to include an end point.
        /// </param>
        /// <param name="loftType">type of loft to perform.</param>
        /// <param name="closed">true if the last curve in this loft should be connected back to the first one.</param>
        /// <param name="rebuildPointCount">A number of points to use while rebuilding the curves. 0 leaves turns this parameter off.</param>
        /// <returns>
        /// Constructs a closed surface, continuing the surface past the last curve around to the
        /// first curve. Available when you have selected three shape curves.
        /// </returns>
        public static async Task<Brep[]> CreateFromLoftRebuild(Remote<IEnumerable<Curve>> curves, Point3d start, Point3d end, LoftType loftType, bool closed, int rebuildPointCount)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), curves, start, end, loftType, closed, rebuildPointCount);
        }

        /// <summary>
        /// Constructs one or more Breps by lofting through a set of curves. Input for the loft is simplified by
        /// refitting to a specified tolerance.
        /// </summary>
        /// <param name="curves">
        /// The curves to loft through. This function will not perform any curve sorting. You must pass in
        /// curves in the order you want them lofted. This function will not adjust the directions of open
        /// curves. Use Curve.DoDirectionsMatch and Curve.Reverse to adjust the directions of open curves.
        /// This function will not adjust the seams of closed curves. Use Curve.ChangeClosedCurveSeam to
        /// adjust the seam of closed curves.
        /// </param>
        /// <param name="start">
        /// Optional starting point of loft. Use Point3d.Unset if you do not want to include a start point.
        /// </param>
        /// <param name="end">
        /// Optional ending point of lost. Use Point3d.Unset if you do not want to include an end point.
        /// </param>
        /// <param name="loftType">type of loft to perform.</param>
        /// <param name="closed">true if the last curve in this loft should be connected back to the first one.</param>
        /// <param name="refitTolerance">A distance to use in refitting, or 0 if you want to turn this parameter off.</param>
        /// <returns>
        /// Constructs a closed surface, continuing the surface past the last curve around to the
        /// first curve. Available when you have selected three shape curves.
        /// </returns>
        public static async Task<Brep[]> CreateFromLoftRefit(IEnumerable<Curve> curves, Point3d start, Point3d end, LoftType loftType, bool closed, double refitTolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), curves, start, end, loftType, closed, refitTolerance);
        }

        /// <summary>
        /// Constructs one or more Breps by lofting through a set of curves. Input for the loft is simplified by
        /// refitting to a specified tolerance.
        /// </summary>
        /// <param name="curves">
        /// The curves to loft through. This function will not perform any curve sorting. You must pass in
        /// curves in the order you want them lofted. This function will not adjust the directions of open
        /// curves. Use Curve.DoDirectionsMatch and Curve.Reverse to adjust the directions of open curves.
        /// This function will not adjust the seams of closed curves. Use Curve.ChangeClosedCurveSeam to
        /// adjust the seam of closed curves.
        /// </param>
        /// <param name="start">
        /// Optional starting point of loft. Use Point3d.Unset if you do not want to include a start point.
        /// </param>
        /// <param name="end">
        /// Optional ending point of lost. Use Point3d.Unset if you do not want to include an end point.
        /// </param>
        /// <param name="loftType">type of loft to perform.</param>
        /// <param name="closed">true if the last curve in this loft should be connected back to the first one.</param>
        /// <param name="refitTolerance">A distance to use in refitting, or 0 if you want to turn this parameter off.</param>
        /// <returns>
        /// Constructs a closed surface, continuing the surface past the last curve around to the
        /// first curve. Available when you have selected three shape curves.
        /// </returns>
        public static async Task<Brep[]> CreateFromLoftRefit(Remote<IEnumerable<Curve>> curves, Point3d start, Point3d end, LoftType loftType, bool closed, double refitTolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), curves, start, end, loftType, closed, refitTolerance);
        }

        /// <summary>
        /// Constructs one or more Breps by lofting through a set of curves, optionally matching start and
        /// end tangents of surfaces when first and/or last loft curves are surface edges
        /// </summary>
        /// <param name="curves">
        /// The curves to loft through. This function will not perform any curve sorting. You must pass in
        /// curves in the order you want them lofted. This function will not adjust the directions of open
        /// curves. Use Curve.DoDirectionsMatch and Curve.Reverse to adjust the directions of open curves.
        /// This function will not adjust the seams of closed curves. Use Curve.ChangeClosedCurveSeam to
        /// adjust the seam of closed curves.
        /// </param>
        /// <param name="start">
        /// Optional starting point of loft. Use Point3d.Unset if you do not want to include a start point.
        /// "start" and "StartTangent" cannot both be true.
        /// </param>
        /// <param name="end">
        /// Optional ending point of loft. Use Point3d.Unset if you do not want to include an end point.
        /// "end and "EndTangent" cannot both be true.
        /// </param>
        /// <param name="StartTangent">
        /// If StartTangent is true and the first loft curve is a surface edge, the loft will match the tangent 
        /// of the surface behind that edge.
        /// </param>
        /// <param name="EndTangent">
        /// If EndTangent is true and the first loft curve is a surface edge, the loft will match the tangent 
        /// of the surface behind that edge.
        /// </param>
        /// <param name="StartTrim">
        /// BrepTrim from the surface edge where start tangent is to be matched
        /// </param>
        /// <param name="EndTrim">
        /// BrepTrim from the surface edge where end tangent is to be matched
        /// </param>
        /// <param name="loftType">type of loft to perform.</param>
        /// <param name="closed">true if the last curve in this loft should be connected back to the first one.</param>
        /// <returns>
        /// Constructs a closed surface, continuing the surface past the last curve around to the
        /// first curve. Available when you have selected three shape curves.
        /// </returns>
        public static async Task<Brep[]> CreateFromLoft(IEnumerable<Curve> curves, Point3d start, Point3d end, bool StartTangent, bool EndTangent, BrepTrim StartTrim, BrepTrim EndTrim, LoftType loftType, bool closed)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), curves, start, end, StartTangent, EndTangent, StartTrim, EndTrim, loftType, closed);
        }

        /// <summary>
        /// Constructs one or more Breps by lofting through a set of curves, optionally matching start and
        /// end tangents of surfaces when first and/or last loft curves are surface edges
        /// </summary>
        /// <param name="curves">
        /// The curves to loft through. This function will not perform any curve sorting. You must pass in
        /// curves in the order you want them lofted. This function will not adjust the directions of open
        /// curves. Use Curve.DoDirectionsMatch and Curve.Reverse to adjust the directions of open curves.
        /// This function will not adjust the seams of closed curves. Use Curve.ChangeClosedCurveSeam to
        /// adjust the seam of closed curves.
        /// </param>
        /// <param name="start">
        /// Optional starting point of loft. Use Point3d.Unset if you do not want to include a start point.
        /// "start" and "StartTangent" cannot both be true.
        /// </param>
        /// <param name="end">
        /// Optional ending point of loft. Use Point3d.Unset if you do not want to include an end point.
        /// "end and "EndTangent" cannot both be true.
        /// </param>
        /// <param name="StartTangent">
        /// If StartTangent is true and the first loft curve is a surface edge, the loft will match the tangent 
        /// of the surface behind that edge.
        /// </param>
        /// <param name="EndTangent">
        /// If EndTangent is true and the first loft curve is a surface edge, the loft will match the tangent 
        /// of the surface behind that edge.
        /// </param>
        /// <param name="StartTrim">
        /// BrepTrim from the surface edge where start tangent is to be matched
        /// </param>
        /// <param name="EndTrim">
        /// BrepTrim from the surface edge where end tangent is to be matched
        /// </param>
        /// <param name="loftType">type of loft to perform.</param>
        /// <param name="closed">true if the last curve in this loft should be connected back to the first one.</param>
        /// <returns>
        /// Constructs a closed surface, continuing the surface past the last curve around to the
        /// first curve. Available when you have selected three shape curves.
        /// </returns>
        public static async Task<Brep[]> CreateFromLoft(Remote<IEnumerable<Curve>> curves, Point3d start, Point3d end, bool StartTangent, bool EndTangent, BrepTrim StartTrim, BrepTrim EndTrim, LoftType loftType, bool closed)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), curves, start, end, StartTangent, EndTangent, StartTrim, EndTrim, loftType, closed);
        }

        /// <summary>
        /// CreatePlanarUnion
        /// </summary>
        /// <param name="breps">The planar regions on which to perform the union operation.</param>
        /// <param name="plane">The plane in which all the input breps lie</param>
        /// <param name="tolerance">Tolerance to use for union operation.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        public static async Task<Brep[]> CreatePlanarUnion(IEnumerable<Brep> breps, Plane plane, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), breps, plane, tolerance);
        }

        /// <summary>
        /// CreatePlanarUnion
        /// </summary>
        /// <param name="breps">The planar regions on which to perform the union operation.</param>
        /// <param name="plane">The plane in which all the input breps lie</param>
        /// <param name="tolerance">Tolerance to use for union operation.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        public static async Task<Brep[]> CreatePlanarUnion(Remote<IEnumerable<Brep>> breps, Plane plane, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), breps, plane, tolerance);
        }

        /// <summary>
        /// CreatePlanarUnion
        /// </summary>
        /// <param name="b0">The first brep to union.</param>
        /// <param name="b1">The first brep to union.</param>
        /// <param name="plane">The plane in which all the input breps lie</param>
        /// <param name="tolerance">Tolerance to use for union operation.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        public static async Task<Brep[]> CreatePlanarUnion(Brep b0, Brep b1, Plane plane, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), b0, b1, plane, tolerance);
        }

        /// <summary>
        /// CreatePlanarUnion
        /// </summary>
        /// <param name="b0">The first brep to union.</param>
        /// <param name="b1">The first brep to union.</param>
        /// <param name="plane">The plane in which all the input breps lie</param>
        /// <param name="tolerance">Tolerance to use for union operation.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        public static async Task<Brep[]> CreatePlanarUnion(Remote<Brep> b0, Remote<Brep> b1, Plane plane, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), b0, b1, plane, tolerance);
        }

        /// <summary>
        /// CreatePlanarDifference
        /// </summary>
        /// <param name="b0">The first brep to intersect.</param>
        /// <param name="b1">The first brep to intersect.</param>
        /// <param name="plane">The plane in which all the input breps lie</param>
        /// <param name="tolerance">Tolerance to use for Difference operation.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        public static async Task<Brep[]> CreatePlanarDifference(Brep b0, Brep b1, Plane plane, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), b0, b1, plane, tolerance);
        }

        /// <summary>
        /// CreatePlanarDifference
        /// </summary>
        /// <param name="b0">The first brep to intersect.</param>
        /// <param name="b1">The first brep to intersect.</param>
        /// <param name="plane">The plane in which all the input breps lie</param>
        /// <param name="tolerance">Tolerance to use for Difference operation.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        public static async Task<Brep[]> CreatePlanarDifference(Remote<Brep> b0, Remote<Brep> b1, Plane plane, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), b0, b1, plane, tolerance);
        }

        /// <summary>
        /// CreatePlanarIntersection
        /// </summary>
        /// <param name="b0">The first brep to intersect.</param>
        /// <param name="b1">The second brep to intersect.</param>
        /// <param name="plane">The plane in which all the input breps lie</param>
        /// <param name="tolerance">Tolerance to use for intersection operation.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        public static async Task<Brep[]> CreatePlanarIntersection(Brep b0, Brep b1, Plane plane, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), b0, b1, plane, tolerance);
        }

        /// <summary>
        /// CreatePlanarIntersection
        /// </summary>
        /// <param name="b0">The first brep to intersect.</param>
        /// <param name="b1">The second brep to intersect.</param>
        /// <param name="plane">The plane in which all the input breps lie</param>
        /// <param name="tolerance">Tolerance to use for intersection operation.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        public static async Task<Brep[]> CreatePlanarIntersection(Remote<Brep> b0, Remote<Brep> b1, Plane plane, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), b0, b1, plane, tolerance);
        }

        /// <summary>
        /// Compute the Boolean Union of a set of Breps.
        /// </summary>
        /// <param name="breps">Breps to union.</param>
        /// <param name="tolerance">Tolerance to use for union operation.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        public static async Task<Brep[]> CreateBooleanUnion(IEnumerable<Brep> breps, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), breps, tolerance);
        }

        /// <summary>
        /// Compute the Boolean Union of a set of Breps.
        /// </summary>
        /// <param name="breps">Breps to union.</param>
        /// <param name="tolerance">Tolerance to use for union operation.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        public static async Task<Brep[]> CreateBooleanUnion(Remote<IEnumerable<Brep>> breps, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), breps, tolerance);
        }

        /// <summary>
        /// Compute the Boolean Union of a set of Breps.
        /// </summary>
        /// <param name="breps">Breps to union.</param>
        /// <param name="tolerance">Tolerance to use for union operation.</param>
        /// <param name="manifoldOnly">If true, non-manifold input breps are ignored.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        public static async Task<Brep[]> CreateBooleanUnion(IEnumerable<Brep> breps, double tolerance, bool manifoldOnly)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), breps, tolerance, manifoldOnly);
        }

        /// <summary>
        /// Compute the Boolean Union of a set of Breps.
        /// </summary>
        /// <param name="breps">Breps to union.</param>
        /// <param name="tolerance">Tolerance to use for union operation.</param>
        /// <param name="manifoldOnly">If true, non-manifold input breps are ignored.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        public static async Task<Brep[]> CreateBooleanUnion(Remote<IEnumerable<Brep>> breps, double tolerance, bool manifoldOnly)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), breps, tolerance, manifoldOnly);
        }

        /// <summary>
        /// Compute the Solid Intersection of two sets of Breps.
        /// </summary>
        /// <param name="firstSet">First set of Breps.</param>
        /// <param name="secondSet">Second set of Breps.</param>
        /// <param name="tolerance">Tolerance to use for intersection operation.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        /// <remarks>The solid orientation of the breps make a difference when calling this function</remarks>
        public static async Task<Brep[]> CreateBooleanIntersection(IEnumerable<Brep> firstSet, IEnumerable<Brep> secondSet, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), firstSet, secondSet, tolerance);
        }

        /// <summary>
        /// Compute the Solid Intersection of two sets of Breps.
        /// </summary>
        /// <param name="firstSet">First set of Breps.</param>
        /// <param name="secondSet">Second set of Breps.</param>
        /// <param name="tolerance">Tolerance to use for intersection operation.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        /// <remarks>The solid orientation of the breps make a difference when calling this function</remarks>
        public static async Task<Brep[]> CreateBooleanIntersection(Remote<IEnumerable<Brep>> firstSet, Remote<IEnumerable<Brep>> secondSet, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), firstSet, secondSet, tolerance);
        }

        /// <summary>
        /// Compute the Solid Intersection of two sets of Breps.
        /// </summary>
        /// <param name="firstSet">First set of Breps.</param>
        /// <param name="secondSet">Second set of Breps.</param>
        /// <param name="tolerance">Tolerance to use for intersection operation.</param>
        /// <param name="manifoldOnly">If true, non-manifold input breps are ignored.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        /// <remarks>The solid orientation of the breps make a difference when calling this function</remarks>
        public static async Task<Brep[]> CreateBooleanIntersection(IEnumerable<Brep> firstSet, IEnumerable<Brep> secondSet, double tolerance, bool manifoldOnly)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), firstSet, secondSet, tolerance, manifoldOnly);
        }

        /// <summary>
        /// Compute the Solid Intersection of two sets of Breps.
        /// </summary>
        /// <param name="firstSet">First set of Breps.</param>
        /// <param name="secondSet">Second set of Breps.</param>
        /// <param name="tolerance">Tolerance to use for intersection operation.</param>
        /// <param name="manifoldOnly">If true, non-manifold input breps are ignored.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        /// <remarks>The solid orientation of the breps make a difference when calling this function</remarks>
        public static async Task<Brep[]> CreateBooleanIntersection(Remote<IEnumerable<Brep>> firstSet, Remote<IEnumerable<Brep>> secondSet, double tolerance, bool manifoldOnly)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), firstSet, secondSet, tolerance, manifoldOnly);
        }

        /// <summary>
        /// Compute the Solid Intersection of two Breps.
        /// </summary>
        /// <param name="firstBrep">First Brep for boolean intersection.</param>
        /// <param name="secondBrep">Second Brep for boolean intersection.</param>
        /// <param name="tolerance">Tolerance to use for intersection operation.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        /// <remarks>The solid orientation of the breps make a difference when calling this function</remarks>
        public static async Task<Brep[]> CreateBooleanIntersection(Brep firstBrep, Brep secondBrep, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), firstBrep, secondBrep, tolerance);
        }

        /// <summary>
        /// Compute the Solid Intersection of two Breps.
        /// </summary>
        /// <param name="firstBrep">First Brep for boolean intersection.</param>
        /// <param name="secondBrep">Second Brep for boolean intersection.</param>
        /// <param name="tolerance">Tolerance to use for intersection operation.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        /// <remarks>The solid orientation of the breps make a difference when calling this function</remarks>
        public static async Task<Brep[]> CreateBooleanIntersection(Remote<Brep> firstBrep, Remote<Brep> secondBrep, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), firstBrep, secondBrep, tolerance);
        }

        /// <summary>
        /// Compute the Solid Intersection of two Breps.
        /// </summary>
        /// <param name="firstBrep">First Brep for boolean intersection.</param>
        /// <param name="secondBrep">Second Brep for boolean intersection.</param>
        /// <param name="tolerance">Tolerance to use for intersection operation.</param>
        /// <param name="manifoldOnly">If true, non-manifold input breps are ignored.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        /// <remarks>The solid orientation of the breps make a difference when calling this function</remarks>
        public static async Task<Brep[]> CreateBooleanIntersection(Brep firstBrep, Brep secondBrep, double tolerance, bool manifoldOnly)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), firstBrep, secondBrep, tolerance, manifoldOnly);
        }

        /// <summary>
        /// Compute the Solid Intersection of two Breps.
        /// </summary>
        /// <param name="firstBrep">First Brep for boolean intersection.</param>
        /// <param name="secondBrep">Second Brep for boolean intersection.</param>
        /// <param name="tolerance">Tolerance to use for intersection operation.</param>
        /// <param name="manifoldOnly">If true, non-manifold input breps are ignored.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        /// <remarks>The solid orientation of the breps make a difference when calling this function</remarks>
        public static async Task<Brep[]> CreateBooleanIntersection(Remote<Brep> firstBrep, Remote<Brep> secondBrep, double tolerance, bool manifoldOnly)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), firstBrep, secondBrep, tolerance, manifoldOnly);
        }

        /// <summary>
        /// Compute the Solid Difference of two sets of Breps.
        /// </summary>
        /// <param name="firstSet">First set of Breps (the set to subtract from).</param>
        /// <param name="secondSet">Second set of Breps (the set to subtract).</param>
        /// <param name="tolerance">Tolerance to use for difference operation.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        /// <remarks>The solid orientation of the breps make a difference when calling this function</remarks>
        /// <example>
        /// <code source='examples\vbnet\ex_booleandifference.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_booleandifference.cs' lang='cs'/>
        /// <code source='examples\py\ex_booleandifference.py' lang='py'/>
        /// </example>
        public static async Task<Brep[]> CreateBooleanDifference(IEnumerable<Brep> firstSet, IEnumerable<Brep> secondSet, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), firstSet, secondSet, tolerance);
        }

        /// <summary>
        /// Compute the Solid Difference of two sets of Breps.
        /// </summary>
        /// <param name="firstSet">First set of Breps (the set to subtract from).</param>
        /// <param name="secondSet">Second set of Breps (the set to subtract).</param>
        /// <param name="tolerance">Tolerance to use for difference operation.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        /// <remarks>The solid orientation of the breps make a difference when calling this function</remarks>
        /// <example>
        /// <code source='examples\vbnet\ex_booleandifference.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_booleandifference.cs' lang='cs'/>
        /// <code source='examples\py\ex_booleandifference.py' lang='py'/>
        /// </example>
        public static async Task<Brep[]> CreateBooleanDifference(Remote<IEnumerable<Brep>> firstSet, Remote<IEnumerable<Brep>> secondSet, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), firstSet, secondSet, tolerance);
        }

        /// <summary>
        /// Compute the Solid Difference of two sets of Breps.
        /// </summary>
        /// <param name="firstSet">First set of Breps (the set to subtract from).</param>
        /// <param name="secondSet">Second set of Breps (the set to subtract).</param>
        /// <param name="tolerance">Tolerance to use for difference operation.</param>
        /// <param name="manifoldOnly">If true, non-manifold input breps are ignored.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        /// <remarks>The solid orientation of the breps make a difference when calling this function</remarks>
        /// <example>
        /// <code source='examples\vbnet\ex_booleandifference.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_booleandifference.cs' lang='cs'/>
        /// <code source='examples\py\ex_booleandifference.py' lang='py'/>
        /// </example>
        public static async Task<Brep[]> CreateBooleanDifference(IEnumerable<Brep> firstSet, IEnumerable<Brep> secondSet, double tolerance, bool manifoldOnly)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), firstSet, secondSet, tolerance, manifoldOnly);
        }

        /// <summary>
        /// Compute the Solid Difference of two sets of Breps.
        /// </summary>
        /// <param name="firstSet">First set of Breps (the set to subtract from).</param>
        /// <param name="secondSet">Second set of Breps (the set to subtract).</param>
        /// <param name="tolerance">Tolerance to use for difference operation.</param>
        /// <param name="manifoldOnly">If true, non-manifold input breps are ignored.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        /// <remarks>The solid orientation of the breps make a difference when calling this function</remarks>
        /// <example>
        /// <code source='examples\vbnet\ex_booleandifference.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_booleandifference.cs' lang='cs'/>
        /// <code source='examples\py\ex_booleandifference.py' lang='py'/>
        /// </example>
        public static async Task<Brep[]> CreateBooleanDifference(Remote<IEnumerable<Brep>> firstSet, Remote<IEnumerable<Brep>> secondSet, double tolerance, bool manifoldOnly)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), firstSet, secondSet, tolerance, manifoldOnly);
        }

        /// <summary>
        /// Compute the Solid Difference of two Breps.
        /// </summary>
        /// <param name="firstBrep">First Brep for boolean difference.</param>
        /// <param name="secondBrep">Second Brep for boolean difference.</param>
        /// <param name="tolerance">Tolerance to use for difference operation.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        /// <remarks>The solid orientation of the breps make a difference when calling this function</remarks>
        public static async Task<Brep[]> CreateBooleanDifference(Brep firstBrep, Brep secondBrep, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), firstBrep, secondBrep, tolerance);
        }

        /// <summary>
        /// Compute the Solid Difference of two Breps.
        /// </summary>
        /// <param name="firstBrep">First Brep for boolean difference.</param>
        /// <param name="secondBrep">Second Brep for boolean difference.</param>
        /// <param name="tolerance">Tolerance to use for difference operation.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        /// <remarks>The solid orientation of the breps make a difference when calling this function</remarks>
        public static async Task<Brep[]> CreateBooleanDifference(Remote<Brep> firstBrep, Remote<Brep> secondBrep, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), firstBrep, secondBrep, tolerance);
        }

        /// <summary>
        /// Compute the Solid Difference of two Breps.
        /// </summary>
        /// <param name="firstBrep">First Brep for boolean difference.</param>
        /// <param name="secondBrep">Second Brep for boolean difference.</param>
        /// <param name="tolerance">Tolerance to use for difference operation.</param>
        /// <param name="manifoldOnly">If true, non-manifold input breps are ignored.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        /// <remarks>The solid orientation of the breps make a difference when calling this function</remarks>
        public static async Task<Brep[]> CreateBooleanDifference(Brep firstBrep, Brep secondBrep, double tolerance, bool manifoldOnly)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), firstBrep, secondBrep, tolerance, manifoldOnly);
        }

        /// <summary>
        /// Compute the Solid Difference of two Breps.
        /// </summary>
        /// <param name="firstBrep">First Brep for boolean difference.</param>
        /// <param name="secondBrep">Second Brep for boolean difference.</param>
        /// <param name="tolerance">Tolerance to use for difference operation.</param>
        /// <param name="manifoldOnly">If true, non-manifold input breps are ignored.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        /// <remarks>The solid orientation of the breps make a difference when calling this function</remarks>
        public static async Task<Brep[]> CreateBooleanDifference(Remote<Brep> firstBrep, Remote<Brep> secondBrep, double tolerance, bool manifoldOnly)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), firstBrep, secondBrep, tolerance, manifoldOnly);
        }

        /// <summary>
        /// Splits shared areas of Breps and creates separate Breps from the shared and unshared parts.
        /// </summary>
        /// <param name="firstBrep">The Brep to split.</param>
        /// <param name="secondBrep">The cutting Brep.</param>
        /// <param name="tolerance">Tolerance to use for splitting operation. When in doubt, use the document's model absolute tolerance.</param>
        /// <returns>An array of Brep if successful, an empty array on failure.</returns>
        public static async Task<Brep[]> CreateBooleanSplit(Brep firstBrep, Brep secondBrep, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), firstBrep, secondBrep, tolerance);
        }

        /// <summary>
        /// Splits shared areas of Breps and creates separate Breps from the shared and unshared parts.
        /// </summary>
        /// <param name="firstBrep">The Brep to split.</param>
        /// <param name="secondBrep">The cutting Brep.</param>
        /// <param name="tolerance">Tolerance to use for splitting operation. When in doubt, use the document's model absolute tolerance.</param>
        /// <returns>An array of Brep if successful, an empty array on failure.</returns>
        public static async Task<Brep[]> CreateBooleanSplit(Remote<Brep> firstBrep, Remote<Brep> secondBrep, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), firstBrep, secondBrep, tolerance);
        }

        /// <summary>
        /// Splits shared areas of Breps and creates separate Breps from the shared and unshared parts.
        /// </summary>
        /// <param name="firstSet">The Breps to split.</param>
        /// <param name="secondSet">The cutting Breps.</param>
        /// <param name="tolerance">Tolerance to use for splitting operation. When in doubt, use the document's model absolute tolerance.</param>
        /// <returns>An array of Brep if successful, an empty array on failure.</returns>
        public static async Task<Brep[]> CreateBooleanSplit(IEnumerable<Brep> firstSet, IEnumerable<Brep> secondSet, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), firstSet, secondSet, tolerance);
        }

        /// <summary>
        /// Splits shared areas of Breps and creates separate Breps from the shared and unshared parts.
        /// </summary>
        /// <param name="firstSet">The Breps to split.</param>
        /// <param name="secondSet">The cutting Breps.</param>
        /// <param name="tolerance">Tolerance to use for splitting operation. When in doubt, use the document's model absolute tolerance.</param>
        /// <returns>An array of Brep if successful, an empty array on failure.</returns>
        public static async Task<Brep[]> CreateBooleanSplit(Remote<IEnumerable<Brep>> firstSet, Remote<IEnumerable<Brep>> secondSet, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), firstSet, secondSet, tolerance);
        }

        /// <summary>
        /// Creates a hollowed out shell from a solid Brep. Function only operates on simple, solid, manifold Breps.
        /// </summary>
        /// <param name="brep">The solid Brep to shell.</param>
        /// <param name="facesToRemove">The indices of the Brep faces to remove. These surfaces are removed and the remainder is offset inward, using the outer parts of the removed surfaces to join the inner and outer parts.</param>
        /// <param name="distance">The distance, or thickness, for the shell. This is a signed distance value with respect to face normals and flipped faces.</param>
        /// <param name="tolerance">The offset tolerance. When in doubt, use the document's absolute tolerance.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        public static async Task<Brep[]> CreateShell(Brep brep, IEnumerable<int> facesToRemove, double distance, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brep, facesToRemove, distance, tolerance);
        }

        /// <summary>
        /// Creates a hollowed out shell from a solid Brep. Function only operates on simple, solid, manifold Breps.
        /// </summary>
        /// <param name="brep">The solid Brep to shell.</param>
        /// <param name="facesToRemove">The indices of the Brep faces to remove. These surfaces are removed and the remainder is offset inward, using the outer parts of the removed surfaces to join the inner and outer parts.</param>
        /// <param name="distance">The distance, or thickness, for the shell. This is a signed distance value with respect to face normals and flipped faces.</param>
        /// <param name="tolerance">The offset tolerance. When in doubt, use the document's absolute tolerance.</param>
        /// <returns>An array of Brep results or null on failure.</returns>
        public static async Task<Brep[]> CreateShell(Remote<Brep> brep, IEnumerable<int> facesToRemove, double distance, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brep, facesToRemove, distance, tolerance);
        }

        /// <summary>
        /// Joins the breps in the input array at any overlapping edges to form
        /// as few as possible resulting breps. There may be more than one brep in the result array.
        /// </summary>
        /// <param name="brepsToJoin">A list, an array or any enumerable set of breps to join.</param>
        /// <param name="tolerance">3d distance tolerance for detecting overlapping edges.</param>
        /// <returns>New joined breps on success, null on failure.</returns>
        public static async Task<Brep[]> JoinBreps(IEnumerable<Brep> brepsToJoin, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brepsToJoin, tolerance);
        }

        /// <summary>
        /// Joins the breps in the input array at any overlapping edges to form
        /// as few as possible resulting breps. There may be more than one brep in the result array.
        /// </summary>
        /// <param name="brepsToJoin">A list, an array or any enumerable set of breps to join.</param>
        /// <param name="tolerance">3d distance tolerance for detecting overlapping edges.</param>
        /// <returns>New joined breps on success, null on failure.</returns>
        public static async Task<Brep[]> JoinBreps(Remote<IEnumerable<Brep>> brepsToJoin, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brepsToJoin, tolerance);
        }

        /// <summary>
        /// Joins the breps in the input array at any overlapping edges to form
        /// as few as possible resulting breps. There may be more than one brep in the result array.
        /// </summary>
        /// <param name="brepsToJoin">A list, an array or any enumerable set of breps to join.</param>
        /// <param name="tolerance">
        /// 3d distance tolerance for detecting overlapping edges. 
        /// When in doubt, use the document's model absolute tolerance.
        /// </param>
        /// <param name="angleTolerance">
        /// Angle tolerance, in radians, used for merging edges.
        /// When in doubt, use the document's model angle tolerance.
        /// </param>
        /// <returns>New joined breps on success, null on failure.</returns>
        public static async Task<Brep[]> JoinBreps(IEnumerable<Brep> brepsToJoin, double tolerance, double angleTolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brepsToJoin, tolerance, angleTolerance);
        }

        /// <summary>
        /// Joins the breps in the input array at any overlapping edges to form
        /// as few as possible resulting breps. There may be more than one brep in the result array.
        /// </summary>
        /// <param name="brepsToJoin">A list, an array or any enumerable set of breps to join.</param>
        /// <param name="tolerance">
        /// 3d distance tolerance for detecting overlapping edges. 
        /// When in doubt, use the document's model absolute tolerance.
        /// </param>
        /// <param name="angleTolerance">
        /// Angle tolerance, in radians, used for merging edges.
        /// When in doubt, use the document's model angle tolerance.
        /// </param>
        /// <returns>New joined breps on success, null on failure.</returns>
        public static async Task<Brep[]> JoinBreps(Remote<IEnumerable<Brep>> brepsToJoin, double tolerance, double angleTolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brepsToJoin, tolerance, angleTolerance);
        }

        /// <summary>
        /// Separates, or splits, a disjoint Brep into separate Breps.
        /// </summary>
        /// <param name="brep">The disjoint Brep to separate.</param>
        /// <returns>An array of Brep pieces if successful, otherwise an empty array if the Brep is not disjoint or on error.</returns>
        public static async Task<Brep[]> SplitDisjointPieces(Brep brep)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brep);
        }

        /// <summary>
        /// Separates, or splits, a disjoint Brep into separate Breps.
        /// </summary>
        /// <param name="brep">The disjoint Brep to separate.</param>
        /// <returns>An array of Brep pieces if successful, otherwise an empty array if the Brep is not disjoint or on error.</returns>
        public static async Task<Brep[]> SplitDisjointPieces(Remote<Brep> brep)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brep);
        }

        /// <summary>
        /// Combines two or more breps into one. A merge is like a boolean union that keeps the inside pieces. This
        /// function creates non-manifold Breps which in general are unusual in Rhino. You may want to consider using
        /// JoinBreps or CreateBooleanUnion functions instead.
        /// </summary>
        /// <param name="brepsToMerge">must contain more than one Brep.</param>
        /// <param name="tolerance">the tolerance to use when merging.</param>
        /// <returns>Single merged Brep on success. Null on error.</returns>
        public static async Task<Brep> MergeBreps(IEnumerable<Brep> brepsToMerge, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), brepsToMerge, tolerance);
        }

        /// <summary>
        /// Combines two or more breps into one. A merge is like a boolean union that keeps the inside pieces. This
        /// function creates non-manifold Breps which in general are unusual in Rhino. You may want to consider using
        /// JoinBreps or CreateBooleanUnion functions instead.
        /// </summary>
        /// <param name="brepsToMerge">must contain more than one Brep.</param>
        /// <param name="tolerance">the tolerance to use when merging.</param>
        /// <returns>Single merged Brep on success. Null on error.</returns>
        public static async Task<Brep> MergeBreps(Remote<IEnumerable<Brep>> brepsToMerge, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), brepsToMerge, tolerance);
        }

        /// <summary>
        /// Constructs the contour curves for a brep at a specified interval.
        /// </summary>
        /// <param name="brepToContour">A brep or polysurface.</param>
        /// <param name="contourStart">A point to start.</param>
        /// <param name="contourEnd">A point to use as the end.</param>
        /// <param name="interval">The interaxial offset in world units.</param>
        /// <returns>An array with intersected curves. This array can be empty.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_makerhinocontours.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_makerhinocontours.cs' lang='cs'/>
        /// <code source='examples\py\ex_makerhinocontours.py' lang='py'/>
        /// </example>
        public static async Task<Curve[]> CreateContourCurves(Brep brepToContour, Point3d contourStart, Point3d contourEnd, double interval)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), brepToContour, contourStart, contourEnd, interval);
        }

        /// <summary>
        /// Constructs the contour curves for a brep at a specified interval.
        /// </summary>
        /// <param name="brepToContour">A brep or polysurface.</param>
        /// <param name="contourStart">A point to start.</param>
        /// <param name="contourEnd">A point to use as the end.</param>
        /// <param name="interval">The interaxial offset in world units.</param>
        /// <returns>An array with intersected curves. This array can be empty.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_makerhinocontours.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_makerhinocontours.cs' lang='cs'/>
        /// <code source='examples\py\ex_makerhinocontours.py' lang='py'/>
        /// </example>
        public static async Task<Curve[]> CreateContourCurves(Remote<Brep> brepToContour, Point3d contourStart, Point3d contourEnd, double interval)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), brepToContour, contourStart, contourEnd, interval);
        }

        /// <summary>
        /// Constructs the contour curves for a brep, using a slicing plane.
        /// </summary>
        /// <param name="brepToContour">A brep or polysurface.</param>
        /// <param name="sectionPlane">A plane.</param>
        /// <returns>An array with intersected curves. This array can be empty.</returns>
        public static async Task<Curve[]> CreateContourCurves(Brep brepToContour, Plane sectionPlane)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), brepToContour, sectionPlane);
        }

        /// <summary>
        /// Constructs the contour curves for a brep, using a slicing plane.
        /// </summary>
        /// <param name="brepToContour">A brep or polysurface.</param>
        /// <param name="sectionPlane">A plane.</param>
        /// <returns>An array with intersected curves. This array can be empty.</returns>
        public static async Task<Curve[]> CreateContourCurves(Remote<Brep> brepToContour, Plane sectionPlane)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), brepToContour, sectionPlane);
        }

        /// <summary>
        /// Destroys a Brep's region topology information.
        /// </summary>
        public static async Task<Brep> DestroyRegionTopology(this Brep brep)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), brep);
        }

        /// <summary>
        /// Destroys a Brep's region topology information.
        /// </summary>
        public static async Task<Brep> DestroyRegionTopology(Remote<Brep> brep)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), brep);
        }

        /// <summary>
        /// If this Brep has two or more components connected by tangent edges,
        /// then duplicates of the connected components are returned.
        /// </summary>
        /// <param name="angleTolerance">The angle tolerance, in radians, used to determine tangent edges.</param>
        /// <param name="includeMeshes">If true, any cached meshes on this Brep are copied to the returned Breps.</param>
        /// <returns>An array of tangent edge connected components, or an empty array.</returns>
        public static async Task<Brep[]> GetTangentConnectedComponents(this Brep brep, double angleTolerance, bool includeMeshes)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brep, angleTolerance, includeMeshes);
        }

        /// <summary>
        /// If this Brep has two or more components connected by tangent edges,
        /// then duplicates of the connected components are returned.
        /// </summary>
        /// <param name="angleTolerance">The angle tolerance, in radians, used to determine tangent edges.</param>
        /// <param name="includeMeshes">If true, any cached meshes on this Brep are copied to the returned Breps.</param>
        /// <returns>An array of tangent edge connected components, or an empty array.</returns>
        public static async Task<Brep[]> GetTangentConnectedComponents(Remote<Brep> brep, double angleTolerance, bool includeMeshes)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brep, angleTolerance, includeMeshes);
        }

        /// <summary>
        /// Constructs all the Wireframe curves for this Brep.
        /// </summary>
        /// <param name="density">Wireframe density. Valid values range between -1 and 99.</param>
        /// <returns>An array of Wireframe curves or null on failure.</returns>
        public static async Task<Curve[]> GetWireframe(this Brep brep, int density)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), brep, density);
        }

        /// <summary>
        /// Constructs all the Wireframe curves for this Brep.
        /// </summary>
        /// <param name="density">Wireframe density. Valid values range between -1 and 99.</param>
        /// <returns>An array of Wireframe curves or null on failure.</returns>
        public static async Task<Curve[]> GetWireframe(Remote<Brep> brep, int density)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), brep, density);
        }

        /// <summary>
        /// Finds a point on the brep that is closest to testPoint.
        /// </summary>
        /// <param name="testPoint">Base point to project to brep.</param>
        /// <returns>The point on the Brep closest to testPoint or Point3d.Unset if the operation failed.</returns>
        public static async Task<Point3d> ClosestPoint(this Brep brep, Point3d testPoint)
        {
            return await ComputeServer.PostAsync<Point3d>(ApiAddress(), brep, testPoint);
        }

        /// <summary>
        /// Finds a point on the brep that is closest to testPoint.
        /// </summary>
        /// <param name="testPoint">Base point to project to brep.</param>
        /// <returns>The point on the Brep closest to testPoint or Point3d.Unset if the operation failed.</returns>
        public static async Task<Point3d> ClosestPoint(Remote<Brep> brep, Point3d testPoint)
        {
            return await ComputeServer.PostAsync<Point3d>(ApiAddress(), brep, testPoint);
        }

        /// <summary>
        /// Determines if point is inside a Brep.  This question only makes sense when
        /// the brep is a closed and manifold.  This function does not check for
        /// closed or manifold, so result is not valid in those cases.  Intersects
        /// a line through point with brep, finds the intersection point Q closest
        /// to point, and looks at face normal at Q.  If the point Q is on an edge
        /// or the intersection is not transverse at Q, then another line is used.
        /// </summary>
        /// <param name="point">3d point to test.</param>
        /// <param name="tolerance">
        /// 3d distance tolerance used for intersection and determining strict inclusion.
        /// A good default is RhinoMath.SqrtEpsilon.
        /// </param>
        /// <param name="strictlyIn">
        /// if true, point is in if inside brep by at least tolerance.
        /// if false, point is in if truly in or within tolerance of boundary.
        /// </param>
        /// <returns>
        /// true if point is in, false if not.
        /// </returns>
        public static async Task<bool> IsPointInside(this Brep brep, Point3d point, double tolerance, bool strictlyIn)
        {
            return await ComputeServer.PostAsync<bool>(ApiAddress(), brep, point, tolerance, strictlyIn);
        }

        /// <summary>
        /// Determines if point is inside a Brep.  This question only makes sense when
        /// the brep is a closed and manifold.  This function does not check for
        /// closed or manifold, so result is not valid in those cases.  Intersects
        /// a line through point with brep, finds the intersection point Q closest
        /// to point, and looks at face normal at Q.  If the point Q is on an edge
        /// or the intersection is not transverse at Q, then another line is used.
        /// </summary>
        /// <param name="point">3d point to test.</param>
        /// <param name="tolerance">
        /// 3d distance tolerance used for intersection and determining strict inclusion.
        /// A good default is RhinoMath.SqrtEpsilon.
        /// </param>
        /// <param name="strictlyIn">
        /// if true, point is in if inside brep by at least tolerance.
        /// if false, point is in if truly in or within tolerance of boundary.
        /// </param>
        /// <returns>
        /// true if point is in, false if not.
        /// </returns>
        public static async Task<bool> IsPointInside(Remote<Brep> brep, Point3d point, double tolerance, bool strictlyIn)
        {
            return await ComputeServer.PostAsync<bool>(ApiAddress(), brep, point, tolerance, strictlyIn);
        }

        /// <summary>
        /// Returns a new Brep that is equivalent to this Brep with all planar holes capped.
        /// </summary>
        /// <param name="tolerance">Tolerance to use for capping.</param>
        /// <returns>New brep on success. null on error.</returns>
        public static async Task<Brep> CapPlanarHoles(this Brep brep, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), brep, tolerance);
        }

        /// <summary>
        /// Returns a new Brep that is equivalent to this Brep with all planar holes capped.
        /// </summary>
        /// <param name="tolerance">Tolerance to use for capping.</param>
        /// <returns>New brep on success. null on error.</returns>
        public static async Task<Brep> CapPlanarHoles(Remote<Brep> brep, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), brep, tolerance);
        }

        /// <summary>
        /// Splits a Brep into pieces using a Brep as a cutter.
        /// </summary>
        /// <param name="cutter">The Brep to use as a cutter.</param>
        /// <param name="intersectionTolerance">The tolerance with which to compute intersections.</param>
        /// <returns>A new array of Breps. This array can be empty.</returns>
        public static async Task<Brep[]> Split(this Brep brep, Brep cutter, double intersectionTolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brep, cutter, intersectionTolerance);
        }

        /// <summary>
        /// Splits a Brep into pieces using a Brep as a cutter.
        /// </summary>
        /// <param name="cutter">The Brep to use as a cutter.</param>
        /// <param name="intersectionTolerance">The tolerance with which to compute intersections.</param>
        /// <returns>A new array of Breps. This array can be empty.</returns>
        public static async Task<Brep[]> Split(Remote<Brep> brep, Remote<Brep> cutter, double intersectionTolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brep, cutter, intersectionTolerance);
        }

        /// <summary>
        /// Splits a Brep into pieces using Breps as cutters.
        /// </summary>
        /// <param name="cutters">One or more Breps to use as cutters.</param>
        /// <param name="intersectionTolerance">The tolerance with which to compute intersections.</param>
        /// <returns>A new array of Breps. This array can be empty.</returns>
        public static async Task<Brep[]> Split(this Brep brep, IEnumerable<Brep> cutters, double intersectionTolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brep, cutters, intersectionTolerance);
        }

        /// <summary>
        /// Splits a Brep into pieces using Breps as cutters.
        /// </summary>
        /// <param name="cutters">One or more Breps to use as cutters.</param>
        /// <param name="intersectionTolerance">The tolerance with which to compute intersections.</param>
        /// <returns>A new array of Breps. This array can be empty.</returns>
        public static async Task<Brep[]> Split(Remote<Brep> brep, Remote<IEnumerable<Brep>> cutters, double intersectionTolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brep, cutters, intersectionTolerance);
        }

        /// <summary>
        /// Splits a Brep into pieces using curves, at least partially on the Brep, as cutters.
        /// </summary>
        /// <param name="cutters">The splitting curves. Only the portion of the curve on the Brep surface will be used for cutting.</param>
        /// <param name="intersectionTolerance">The tolerance with which to compute intersections.</param>
        /// <returns>A new array of Breps. This array can be empty.</returns>
        public static async Task<Brep[]> Split(this Brep brep, IEnumerable<Curve> cutters, double intersectionTolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brep, cutters, intersectionTolerance);
        }

        /// <summary>
        /// Splits a Brep into pieces using curves, at least partially on the Brep, as cutters.
        /// </summary>
        /// <param name="cutters">The splitting curves. Only the portion of the curve on the Brep surface will be used for cutting.</param>
        /// <param name="intersectionTolerance">The tolerance with which to compute intersections.</param>
        /// <returns>A new array of Breps. This array can be empty.</returns>
        public static async Task<Brep[]> Split(Remote<Brep> brep, Remote<IEnumerable<Curve>> cutters, double intersectionTolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brep, cutters, intersectionTolerance);
        }

        /// <summary>
        /// Splits a Brep into pieces using a combination of curves, to be extruded, and Breps as cutters.
        /// </summary>
        /// <param name="cutters">The curves, surfaces, faces and Breps to be used as cutters. Any other geometry is ignored.</param>
        /// <param name="normal">A construction plane normal, used in deciding how to extrude a curve into a cutter.</param>
        /// <param name="planView">Set true if the assume view is a plan, or parallel projection, view.</param>
        /// <param name="intersectionTolerance">The tolerance with which to compute intersections.</param>
        /// <returns>A new array of Breps. This array can be empty.</returns>
        /// <remarks>
        /// A Curve in cutters is extruded to produce a surface to use as a cutter. The extrusion direction is chosen, as in the Rhino Split command,
        /// based on the properties of the active view. In particular the construction plane Normal and whether the active view is a plan view, 
        /// a parallel projection with construction plane normal as the view direction. If planView is false and the curve is planar then the curve
        /// is extruded perpendicular to the curve, otherwise the curve is extruded in the normal direction.
        /// </remarks>
        public static async Task<Brep[]> Split(this Brep brep, IEnumerable<GeometryBase> cutters, Vector3d normal, bool planView, double intersectionTolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brep, cutters, normal, planView, intersectionTolerance);
        }

        /// <summary>
        /// Splits a Brep into pieces using a combination of curves, to be extruded, and Breps as cutters.
        /// </summary>
        /// <param name="cutters">The curves, surfaces, faces and Breps to be used as cutters. Any other geometry is ignored.</param>
        /// <param name="normal">A construction plane normal, used in deciding how to extrude a curve into a cutter.</param>
        /// <param name="planView">Set true if the assume view is a plan, or parallel projection, view.</param>
        /// <param name="intersectionTolerance">The tolerance with which to compute intersections.</param>
        /// <returns>A new array of Breps. This array can be empty.</returns>
        /// <remarks>
        /// A Curve in cutters is extruded to produce a surface to use as a cutter. The extrusion direction is chosen, as in the Rhino Split command,
        /// based on the properties of the active view. In particular the construction plane Normal and whether the active view is a plan view, 
        /// a parallel projection with construction plane normal as the view direction. If planView is false and the curve is planar then the curve
        /// is extruded perpendicular to the curve, otherwise the curve is extruded in the normal direction.
        /// </remarks>
        public static async Task<Brep[]> Split(Remote<Brep> brep, Remote<IEnumerable<GeometryBase>> cutters, Vector3d normal, bool planView, double intersectionTolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brep, cutters, normal, planView, intersectionTolerance);
        }

        /// <summary>
        /// Trims a brep with an oriented cutter. The parts of the brep that lie inside
        /// (opposite the normal) of the cutter are retained while the parts to the
        /// outside (in the direction of the normal) are discarded.  If the Cutter is
        /// closed, then a connected component of the Brep that does not intersect the
        /// cutter is kept if and only if it is contained in the inside of cutter.
        /// That is the region bounded by cutter opposite from the normal of cutter,
        /// If cutter is not closed all these components are kept.
        /// </summary>
        /// <param name="cutter">A cutting brep.</param>
        /// <param name="intersectionTolerance">A tolerance value with which to compute intersections.</param>
        /// <returns>This Brep is not modified, the trim results are returned in an array.</returns>
        public static async Task<Brep[]> Trim(this Brep brep, Brep cutter, double intersectionTolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brep, cutter, intersectionTolerance);
        }

        /// <summary>
        /// Trims a brep with an oriented cutter. The parts of the brep that lie inside
        /// (opposite the normal) of the cutter are retained while the parts to the
        /// outside (in the direction of the normal) are discarded.  If the Cutter is
        /// closed, then a connected component of the Brep that does not intersect the
        /// cutter is kept if and only if it is contained in the inside of cutter.
        /// That is the region bounded by cutter opposite from the normal of cutter,
        /// If cutter is not closed all these components are kept.
        /// </summary>
        /// <param name="cutter">A cutting brep.</param>
        /// <param name="intersectionTolerance">A tolerance value with which to compute intersections.</param>
        /// <returns>This Brep is not modified, the trim results are returned in an array.</returns>
        public static async Task<Brep[]> Trim(Remote<Brep> brep, Remote<Brep> cutter, double intersectionTolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brep, cutter, intersectionTolerance);
        }

        /// <summary>
        /// Trims a Brep with an oriented cutter.  The parts of Brep that lie inside
        /// (opposite the normal) of the cutter are retained while the parts to the
        /// outside ( in the direction of the normal ) are discarded. A connected
        /// component of Brep that does not intersect the cutter is kept if and only
        /// if it is contained in the inside of Cutter.  That is the region bounded by
        /// cutter opposite from the normal of cutter, or in the case of a Plane cutter
        /// the half space opposite from the plane normal.
        /// </summary>
        /// <param name="cutter">A cutting plane.</param>
        /// <param name="intersectionTolerance">A tolerance value with which to compute intersections.</param>
        /// <returns>This Brep is not modified, the trim results are returned in an array.</returns>
        public static async Task<Brep[]> Trim(this Brep brep, Plane cutter, double intersectionTolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brep, cutter, intersectionTolerance);
        }

        /// <summary>
        /// Trims a Brep with an oriented cutter.  The parts of Brep that lie inside
        /// (opposite the normal) of the cutter are retained while the parts to the
        /// outside ( in the direction of the normal ) are discarded. A connected
        /// component of Brep that does not intersect the cutter is kept if and only
        /// if it is contained in the inside of Cutter.  That is the region bounded by
        /// cutter opposite from the normal of cutter, or in the case of a Plane cutter
        /// the half space opposite from the plane normal.
        /// </summary>
        /// <param name="cutter">A cutting plane.</param>
        /// <param name="intersectionTolerance">A tolerance value with which to compute intersections.</param>
        /// <returns>This Brep is not modified, the trim results are returned in an array.</returns>
        public static async Task<Brep[]> Trim(Remote<Brep> brep, Plane cutter, double intersectionTolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brep, cutter, intersectionTolerance);
        }

        /// <summary>
        /// Un-joins, or separates, edges within the Brep. Note, seams in closed surfaces will not separate.
        /// </summary>
        /// <param name="edgesToUnjoin">The indices of the Brep edges to un-join.</param>
        /// <returns>This Brep is not modified, the trim results are returned in an array.</returns>
        public static async Task<Brep[]> UnjoinEdges(this Brep brep, IEnumerable<int> edgesToUnjoin)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brep, edgesToUnjoin);
        }

        /// <summary>
        /// Un-joins, or separates, edges within the Brep. Note, seams in closed surfaces will not separate.
        /// </summary>
        /// <param name="edgesToUnjoin">The indices of the Brep edges to un-join.</param>
        /// <returns>This Brep is not modified, the trim results are returned in an array.</returns>
        public static async Task<Brep[]> UnjoinEdges(Remote<Brep> brep, IEnumerable<int> edgesToUnjoin)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), brep, edgesToUnjoin);
        }

        /// <summary>
        /// Compute the Area of the Brep. If you want proper Area data with moments 
        /// and error information, use the AreaMassProperties class.
        /// </summary>
        /// <returns>The area of the Brep.</returns>
        public static async Task<double> GetArea(this Brep brep)
        {
            return await ComputeServer.PostAsync<double>(ApiAddress(), brep);
        }

        /// <summary>
        /// Compute the Area of the Brep. If you want proper Area data with moments 
        /// and error information, use the AreaMassProperties class.
        /// </summary>
        /// <returns>The area of the Brep.</returns>
        public static async Task<double> GetArea(Remote<Brep> brep)
        {
            return await ComputeServer.PostAsync<double>(ApiAddress(), brep);
        }

        /// <summary>
        /// Compute the Area of the Brep. If you want proper Area data with moments 
        /// and error information, use the AreaMassProperties class.
        /// </summary>
        /// <param name="relativeTolerance">Relative tolerance to use for area calculation.</param>
        /// <param name="absoluteTolerance">Absolute tolerance to use for area calculation.</param>
        /// <returns>The area of the Brep.</returns>
        public static async Task<double> GetArea(this Brep brep, double relativeTolerance, double absoluteTolerance)
        {
            return await ComputeServer.PostAsync<double>(ApiAddress(), brep, relativeTolerance, absoluteTolerance);
        }

        /// <summary>
        /// Compute the Area of the Brep. If you want proper Area data with moments 
        /// and error information, use the AreaMassProperties class.
        /// </summary>
        /// <param name="relativeTolerance">Relative tolerance to use for area calculation.</param>
        /// <param name="absoluteTolerance">Absolute tolerance to use for area calculation.</param>
        /// <returns>The area of the Brep.</returns>
        public static async Task<double> GetArea(Remote<Brep> brep, double relativeTolerance, double absoluteTolerance)
        {
            return await ComputeServer.PostAsync<double>(ApiAddress(), brep, relativeTolerance, absoluteTolerance);
        }

        /// <summary>
        /// Compute the Volume of the Brep. If you want proper Volume data with moments 
        /// and error information, use the VolumeMassProperties class.
        /// </summary>
        /// <returns>The volume of the Brep.</returns>
        public static async Task<double> GetVolume(this Brep brep)
        {
            return await ComputeServer.PostAsync<double>(ApiAddress(), brep);
        }

        /// <summary>
        /// Compute the Volume of the Brep. If you want proper Volume data with moments 
        /// and error information, use the VolumeMassProperties class.
        /// </summary>
        /// <returns>The volume of the Brep.</returns>
        public static async Task<double> GetVolume(Remote<Brep> brep)
        {
            return await ComputeServer.PostAsync<double>(ApiAddress(), brep);
        }

        /// <summary>
        /// Compute the Volume of the Brep. If you want proper Volume data with moments 
        /// and error information, use the VolumeMassProperties class.
        /// </summary>
        /// <param name="relativeTolerance">Relative tolerance to use for area calculation.</param>
        /// <param name="absoluteTolerance">Absolute tolerance to use for area calculation.</param>
        /// <returns>The volume of the Brep.</returns>
        public static async Task<double> GetVolume(this Brep brep, double relativeTolerance, double absoluteTolerance)
        {
            return await ComputeServer.PostAsync<double>(ApiAddress(), brep, relativeTolerance, absoluteTolerance);
        }

        /// <summary>
        /// Compute the Volume of the Brep. If you want proper Volume data with moments 
        /// and error information, use the VolumeMassProperties class.
        /// </summary>
        /// <param name="relativeTolerance">Relative tolerance to use for area calculation.</param>
        /// <param name="absoluteTolerance">Absolute tolerance to use for area calculation.</param>
        /// <returns>The volume of the Brep.</returns>
        public static async Task<double> GetVolume(Remote<Brep> brep, double relativeTolerance, double absoluteTolerance)
        {
            return await ComputeServer.PostAsync<double>(ApiAddress(), brep, relativeTolerance, absoluteTolerance);
        }

        /// <summary>
        /// Individually insets faces of a brep by offsetting the faces outer edges inward and inner edges out then splitting the face with the offset curves.
        /// </summary>
        /// <param name="faceIndices">the indices of the faces to inset</param>
        /// <param name="distance">The distance to offset along the face</param>
        /// <param name="loose">If true, offset by moving edit points otherwise offset within tolerance.</param>
        /// <param name="ignoreSeams">If true, the seam edges are not offset and adjacent edges are extended to meet the seam. Otherwise offset normally.</param>
        /// <param name="creaseCorners">If true, splitting curves will be made between the creases on edge curves and creases on the inset curves.</param>
        /// <param name="tolerance">The fitting tolerance for the offset. When in doubt, use the document's absolute tolerance.</param>
        /// <param name="angleTolerance">The angle tolerance in radians for identifying creases when creasing corners.  When in doubt, use the document's angle tolerance.</param>
        /// <returns>The brep with inset faces on success. Null on error.</returns>
        public static async Task<Brep> InsetFaces(this Brep brep, IEnumerable<int> faceIndices, double distance, bool loose, bool ignoreSeams, bool creaseCorners, double tolerance, double angleTolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), brep, faceIndices, distance, loose, ignoreSeams, creaseCorners, tolerance, angleTolerance);
        }

        /// <summary>
        /// Individually insets faces of a brep by offsetting the faces outer edges inward and inner edges out then splitting the face with the offset curves.
        /// </summary>
        /// <param name="faceIndices">the indices of the faces to inset</param>
        /// <param name="distance">The distance to offset along the face</param>
        /// <param name="loose">If true, offset by moving edit points otherwise offset within tolerance.</param>
        /// <param name="ignoreSeams">If true, the seam edges are not offset and adjacent edges are extended to meet the seam. Otherwise offset normally.</param>
        /// <param name="creaseCorners">If true, splitting curves will be made between the creases on edge curves and creases on the inset curves.</param>
        /// <param name="tolerance">The fitting tolerance for the offset. When in doubt, use the document's absolute tolerance.</param>
        /// <param name="angleTolerance">The angle tolerance in radians for identifying creases when creasing corners.  When in doubt, use the document's angle tolerance.</param>
        /// <returns>The brep with inset faces on success. Null on error.</returns>
        public static async Task<Brep> InsetFaces(Remote<Brep> brep, IEnumerable<int> faceIndices, double distance, bool loose, bool ignoreSeams, bool creaseCorners, double tolerance, double angleTolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), brep, faceIndices, distance, loose, ignoreSeams, creaseCorners, tolerance, angleTolerance);
        }

        /// <summary>
        /// No support is available for this function.
        /// <para>Expert user function used by MakeValidForV2 to convert trim
        /// curves from one surface to its NURBS form. After calling this function,
        /// you need to change the surface of the face to a NurbsSurface.</para>
        /// </summary>
        /// <param name="face">
        /// Face whose underlying surface has a parameterization that is different
        /// from its NURBS form.
        /// </param>
        /// <param name="nurbsSurface">NURBS form of the face's underlying surface.</param>
        /// <remarks>
        /// Don't call this function unless you know exactly what you are doing.
        /// </remarks>
        public static async Task<Brep> RebuildTrimsForV2(this Brep brep, BrepFace face, NurbsSurface nurbsSurface)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), brep, face, nurbsSurface);
        }

        /// <summary>
        /// No support is available for this function.
        /// <para>Expert user function used by MakeValidForV2 to convert trim
        /// curves from one surface to its NURBS form. After calling this function,
        /// you need to change the surface of the face to a NurbsSurface.</para>
        /// </summary>
        /// <param name="face">
        /// Face whose underlying surface has a parameterization that is different
        /// from its NURBS form.
        /// </param>
        /// <param name="nurbsSurface">NURBS form of the face's underlying surface.</param>
        /// <remarks>
        /// Don't call this function unless you know exactly what you are doing.
        /// </remarks>
        public static async Task<Brep> RebuildTrimsForV2(Remote<Brep> brep, BrepFace face, Remote<NurbsSurface> nurbsSurface)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), brep, face, nurbsSurface);
        }

        /// <summary>
        /// Splits, or cuts up, a surface. Designed to split the underlying surface of a Brep face with edge curves.
        /// </summary>
        /// <param name="surface">The surface to cut up.</param>
        /// <param name="curves">
        /// The edge curves with consistent orientation.
        /// The curves should lie on the surface.</param>
        /// <param name="flip">If true, the input curves are oriented clockwise.</param>
        /// <param name="fitTolerance">The fitting tolerance.</param>
        /// <param name="keepTolerance">Used to decide which face to keep. For best results, should be at least 2 * fitTolerance.</param>
        /// <returns>The Brep pieces if successful, an empty array on failure.</returns>
        public static async Task<Brep[]> CutUpSurface(Surface surface, IEnumerable<Curve> curves, bool flip, double fitTolerance, double keepTolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), surface, curves, flip, fitTolerance, keepTolerance);
        }

        /// <summary>
        /// Splits, or cuts up, a surface. Designed to split the underlying surface of a Brep face with edge curves.
        /// </summary>
        /// <param name="surface">The surface to cut up.</param>
        /// <param name="curves">
        /// The edge curves with consistent orientation.
        /// The curves should lie on the surface.</param>
        /// <param name="flip">If true, the input curves are oriented clockwise.</param>
        /// <param name="fitTolerance">The fitting tolerance.</param>
        /// <param name="keepTolerance">Used to decide which face to keep. For best results, should be at least 2 * fitTolerance.</param>
        /// <returns>The Brep pieces if successful, an empty array on failure.</returns>
        public static async Task<Brep[]> CutUpSurface(Remote<Surface> surface, Remote<IEnumerable<Curve>> curves, bool flip, double fitTolerance, double keepTolerance)
        {
            return await ComputeServer.PostAsync<Brep[]>(ApiAddress(), surface, curves, flip, fitTolerance, keepTolerance);
        }

        /// <summary>
        /// Remove all inner loops, or holes, in a Brep.
        /// </summary>
        /// <param name="tolerance">The tolerance. When in doubt, use the document's model absolute tolerance.</param>
        /// <returns>The Brep without holes if successful, null otherwise.</returns>
        public static async Task<Brep> RemoveHoles(this Brep brep, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), brep, tolerance);
        }

        /// <summary>
        /// Remove all inner loops, or holes, in a Brep.
        /// </summary>
        /// <param name="tolerance">The tolerance. When in doubt, use the document's model absolute tolerance.</param>
        /// <returns>The Brep without holes if successful, null otherwise.</returns>
        public static async Task<Brep> RemoveHoles(Remote<Brep> brep, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), brep, tolerance);
        }

        /// <summary>
        /// Removes inner loops, or holes, in a Brep.
        /// </summary>
        /// <param name="loops">A list of BrepLoop component indexes, where BrepLoop.LoopType == Rhino.Geometry.BrepLoopType.Inner.</param>
        /// <param name="tolerance">The tolerance. When in doubt, use the document's model absolute tolerance.</param>
        /// <returns>The Brep without holes if successful, null otherwise.</returns>
        public static async Task<Brep> RemoveHoles(this Brep brep, IEnumerable<ComponentIndex> loops, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), brep, loops, tolerance);
        }

        /// <summary>
        /// Removes inner loops, or holes, in a Brep.
        /// </summary>
        /// <param name="loops">A list of BrepLoop component indexes, where BrepLoop.LoopType == Rhino.Geometry.BrepLoopType.Inner.</param>
        /// <param name="tolerance">The tolerance. When in doubt, use the document's model absolute tolerance.</param>
        /// <returns>The Brep without holes if successful, null otherwise.</returns>
        public static async Task<Brep> RemoveHoles(Remote<Brep> brep, IEnumerable<ComponentIndex> loops, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), brep, loops, tolerance);
        }
    }

    public static class BrepFaceCompute
    {
        static string ApiAddress([CallerMemberName] string caller = null)
        {
            return ComputeServer.ApiAddress(typeof(BrepFace), caller);
        }

        /// <summary>
        /// Pulls one or more points to a brep face.
        /// </summary>
        /// <param name="points">Points to pull.</param>
        /// <param name="tolerance">Tolerance for pulling operation. Only points that are closer than tolerance will be pulled to the face.</param>
        /// <returns>An array of pulled points.</returns>
        public static async Task<Point3d[]> PullPointsToFace(this BrepFace brepface, IEnumerable<Point3d> points, double tolerance)
        {
            return await ComputeServer.PostAsync<Point3d[]>(ApiAddress(), brepface, points, tolerance);
        }

        /// <summary>
        /// Remove all inner loops, or holes, from a Brep face.
        /// </summary>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static async Task<Brep> RemoveHoles(this BrepFace brepface, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), brepface, tolerance);
        }

        /// <summary>
        /// Split this face using 3D trimming curves.
        /// </summary>
        /// <param name="curves">Curves to split with.</param>
        /// <param name="tolerance">Tolerance for splitting, when in doubt use the Document Absolute Tolerance.</param>
        /// <returns>A brep consisting of all the split fragments, or null on failure.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_tightboundingbox.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_tightboundingbox.cs' lang='cs'/>
        /// <code source='examples\py\ex_tightboundingbox.py' lang='py'/>
        /// </example>
        public static async Task<Brep> Split(this BrepFace brepface, IEnumerable<Curve> curves, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), brepface, curves, tolerance);
        }

        /// <summary>
        /// Split this face using 3D trimming curves.
        /// </summary>
        /// <param name="curves">Curves to split with.</param>
        /// <param name="tolerance">Tolerance for splitting, when in doubt use the Document Absolute Tolerance.</param>
        /// <returns>A brep consisting of all the split fragments, or null on failure.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_tightboundingbox.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_tightboundingbox.cs' lang='cs'/>
        /// <code source='examples\py\ex_tightboundingbox.py' lang='py'/>
        /// </example>
        public static async Task<Brep> Split(this BrepFace brepface, Remote<IEnumerable<Curve>> curves, double tolerance)
        {
            return await ComputeServer.PostAsync<Brep>(ApiAddress(), brepface, curves, tolerance);
        }

        /// <summary>
        /// Tests if a parameter space point is in the active region of a face.
        /// </summary>
        /// <param name="u">Parameter space point U value.</param>
        /// <param name="v">Parameter space point V value.</param>
        /// <returns>A value describing the relationship between the point and the face.</returns>
        public static async Task<PointFaceRelation> IsPointOnFace(this BrepFace brepface, double u, double v)
        {
            return await ComputeServer.PostAsync<PointFaceRelation>(ApiAddress(), brepface, u, v);
        }

        /// <summary>
        /// Tests if a parameter space point is in the active region of a face.
        /// </summary>
        /// <param name="u">Parameter space point U value.</param>
        /// <param name="v">Parameter space point V value.</param>
        /// <param name="tolerance">3D tolerance used when checking to see if the point is on a face or inside of a loop.</param>
        /// <returns>A value describing the relationship between the point and the face.</returns>
        public static async Task<PointFaceRelation> IsPointOnFace(this BrepFace brepface, double u, double v, double tolerance)
        {
            return await ComputeServer.PostAsync<PointFaceRelation>(ApiAddress(), brepface, u, v, tolerance);
        }

        /// <summary>
        /// Gets intervals where the iso curve exists on a BrepFace (trimmed surface)
        /// </summary>
        /// <param name="direction">Direction of isocurve.
        /// <para>0 = Isocurve connects all points with a constant U value.</para>
        /// <para>1 = Isocurve connects all points with a constant V value.</para>
        /// </param>
        /// <param name="constantParameter">Surface parameter that remains identical along the isocurves.</param>
        /// <returns>
        /// If direction = 0, the parameter space iso interval connects the 2d points
        /// (intervals[i][0],iso_constant) and (intervals[i][1],iso_constant).
        /// If direction = 1, the parameter space iso interval connects the 2d points
        /// (iso_constant,intervals[i][0]) and (iso_constant,intervals[i][1]).
        /// </returns>
        public static async Task<Interval[]> TrimAwareIsoIntervals(this BrepFace brepface, int direction, double constantParameter)
        {
            return await ComputeServer.PostAsync<Interval[]>(ApiAddress(), brepface, direction, constantParameter);
        }

        /// <summary>
        /// Similar to IsoCurve function, except this function pays attention to trims on faces 
        /// and may return multiple curves.
        /// </summary>
        /// <param name="direction">Direction of isocurve.
        /// <para>0 = Isocurve connects all points with a constant U value.</para>
        /// <para>1 = Isocurve connects all points with a constant V value.</para>
        /// </param>
        /// <param name="constantParameter">Surface parameter that remains identical along the isocurves.</param>
        /// <returns>Isoparametric curves connecting all points with the constantParameter value.</returns>
        /// <remarks>
        /// In this function "direction" indicates which direction the resulting curve runs.
        /// 0: horizontal, 1: vertical
        /// In the other Surface functions that take a "direction" argument,
        /// "direction" indicates if "constantParameter" is a "u" or "v" parameter.
        /// </remarks>
        public static async Task<Curve[]> TrimAwareIsoCurve(this BrepFace brepface, int direction, double constantParameter)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), brepface, direction, constantParameter);
        }

        /// <summary>
        /// Used by SubD functions that create breps to transmit the subd face SubDFace.PackId value 
        /// to the brep face or faces generated from the subd face.
        /// Unless you are an expert and doing something very carefully and very fancy, to not call this function.
        /// </summary>
        /// <param name="packId">The pack id.</param>
        public static async Task<BrepFace> SetPackId(this BrepFace brepface, uint packId)
        {
            return await ComputeServer.PostAsync<BrepFace>(ApiAddress(), brepface, packId);
        }

        /// <summary>
        /// Sets BrepFace.PackId to 0.
        /// </summary>
        public static async Task<BrepFace> ClearPackId(this BrepFace brepface)
        {
            return await ComputeServer.PostAsync<BrepFace>(ApiAddress(), brepface);
        }
    }

    public static class CurveCompute
    {
        static string ApiAddress([CallerMemberName] string caller = null)
        {
            return ComputeServer.ApiAddress(typeof(Curve), caller);
        }

        /// <summary>
        /// Returns the type of conic section based on the curve's shape.
        /// </summary>
        /// <returns></returns>
        public static async Task<ConicSectionType> GetConicSectionType(this Curve curve)
        {
            return await ComputeServer.PostAsync<ConicSectionType>(ApiAddress(), curve);
        }

        /// <summary>
        /// Returns the type of conic section based on the curve's shape.
        /// </summary>
        /// <returns></returns>
        public static async Task<ConicSectionType> GetConicSectionType(Remote<Curve> curve)
        {
            return await ComputeServer.PostAsync<ConicSectionType>(ApiAddress(), curve);
        }

        /// <summary>
        /// Interpolates a sequence of points. Used by InterpCurve Command
        /// This routine works best when degree=3.
        /// </summary>
        /// <param name="degree">The degree of the curve >=1.  Degree must be odd.</param>
        /// <param name="points">
        /// Points to interpolate (Count must be >= 2)
        /// </param>
        /// <returns>interpolated curve on success. null on failure.</returns>
        public static async Task<Curve> CreateInterpolatedCurve(IEnumerable<Point3d> points, int degree)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), points, degree);
        }

        /// <summary>
        /// Interpolates a sequence of points. Used by InterpCurve Command
        /// This routine works best when degree=3.
        /// </summary>
        /// <param name="degree">The degree of the curve >=1. Note: Even degree > 3 periodic interpolation
        /// results in a non-periodic closed curve.</param>
        /// <param name="points">
        /// Points to interpolate. For periodic curves if the final point is a
        /// duplicate of the initial point it is  ignored. (Count must be >=2)
        /// </param>
        /// <param name="knots">
        /// Knot-style to use  and specifies if the curve should be periodic.
        /// </param>
        /// <returns>interpolated curve on success. null on failure.</returns>
        public static async Task<Curve> CreateInterpolatedCurve(IEnumerable<Point3d> points, int degree, CurveKnotStyle knots)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), points, degree, knots);
        }

        /// <summary>
        /// Interpolates a sequence of points. Used by InterpCurve Command
        /// This routine works best when degree=3.
        /// </summary>
        /// <param name="degree">The degree of the curve >=1. Note: Even degree > 3 periodic interpolation
        /// results in a non-periodic closed curve.</param>
        /// <param name="points">
        /// Points to interpolate. For periodic curves if the final point is a
        /// duplicate of the initial point it is  ignored. (Count must be >=2)
        /// </param>
        /// <param name="knots">
        /// Knot-style to use  and specifies if the curve should be periodic.
        /// </param>
        /// <param name="startTangent">A starting tangent.</param>
        /// <param name="endTangent">An ending tangent.</param>
        /// <returns>interpolated curve on success. null on failure.</returns>
        public static async Task<Curve> CreateInterpolatedCurve(IEnumerable<Point3d> points, int degree, CurveKnotStyle knots, Vector3d startTangent, Vector3d endTangent)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), points, degree, knots, startTangent, endTangent);
        }

        /// <summary>
        /// Creates a soft edited curve from an existing curve using a smooth field of influence.
        /// </summary>
        /// <param name="curve">The curve to soft edit.</param>
        /// <param name="t">
        /// A parameter on the curve to move from. This location on the curve is moved, and the move
        ///  is smoothly tapered off with increasing distance along the curve from this parameter.
        /// </param>
        /// <param name="delta">The direction and magnitude, or maximum distance, of the move.</param>
        /// <param name="length">
        /// The distance along the curve from the editing point over which the strength 
        /// of the editing falls off smoothly.
        /// </param>
        /// <param name="fixEnds"></param>
        /// <returns>The soft edited curve if successful. null on failure.</returns>
        public static async Task<Curve> CreateSoftEditCurve(Curve curve, double t, Vector3d delta, double length, bool fixEnds)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, t, delta, length, fixEnds);
        }

        /// <summary>
        /// Creates a soft edited curve from an existing curve using a smooth field of influence.
        /// </summary>
        /// <param name="curve">The curve to soft edit.</param>
        /// <param name="t">
        /// A parameter on the curve to move from. This location on the curve is moved, and the move
        ///  is smoothly tapered off with increasing distance along the curve from this parameter.
        /// </param>
        /// <param name="delta">The direction and magnitude, or maximum distance, of the move.</param>
        /// <param name="length">
        /// The distance along the curve from the editing point over which the strength 
        /// of the editing falls off smoothly.
        /// </param>
        /// <param name="fixEnds"></param>
        /// <returns>The soft edited curve if successful. null on failure.</returns>
        public static async Task<Curve> CreateSoftEditCurve(Remote<Curve> curve, double t, Vector3d delta, double length, bool fixEnds)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, t, delta, length, fixEnds);
        }

        /// <summary>
        /// Rounds the corners of a kinked curve with arcs of a single, specified radius.
        /// </summary>
        /// <param name="curve">The curve to fillet.</param>
        /// <param name="radius">The fillet radius.</param>
        /// <param name="tolerance">The tolerance. When in doubt, use the document's model space absolute tolerance.</param>
        /// <param name="angleTolerance">The angle tolerance in radians. When in doubt, use the document's model space angle tolerance.</param>
        /// <returns>The filleted curve if successful. null on failure.</returns>
        public static async Task<Curve> CreateFilletCornersCurve(Curve curve, double radius, double tolerance, double angleTolerance)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, radius, tolerance, angleTolerance);
        }

        /// <summary>
        /// Rounds the corners of a kinked curve with arcs of a single, specified radius.
        /// </summary>
        /// <param name="curve">The curve to fillet.</param>
        /// <param name="radius">The fillet radius.</param>
        /// <param name="tolerance">The tolerance. When in doubt, use the document's model space absolute tolerance.</param>
        /// <param name="angleTolerance">The angle tolerance in radians. When in doubt, use the document's model space angle tolerance.</param>
        /// <returns>The filleted curve if successful. null on failure.</returns>
        public static async Task<Curve> CreateFilletCornersCurve(Remote<Curve> curve, double radius, double tolerance, double angleTolerance)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, radius, tolerance, angleTolerance);
        }

        /// <summary>
        /// Creates an arc-cornered (rounded) rectangular curve.
        /// </summary>
        /// <param name="rectangle">The rectangle.</param>
        /// <param name="radius">The arc radius at each corner.</param>
        /// <returns>Aa arc-cornered rectangular curve if successful, null otherwise.</returns>
        public static async Task<Curve> CreateArcCornerRectangle(Rectangle3d rectangle, double radius)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), rectangle, radius);
        }

        /// <summary>
        /// Creates a conic-corned (rounded) rectangular curve.
        /// </summary>
        /// <param name="rectangle">The rectangle.</param>
        /// <param name="rho">The rho value at each corner, in the exclusive range (0.0, 1.0).</param>
        /// <returns>A conic-cornered rectangular curve if successful, null otherwise.</returns>
        public static async Task<Curve> CreateConicCornerRectangle(Rectangle3d rectangle, double rho)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), rectangle, rho);
        }

        /// <summary>
        /// Creates an arc-line-arc blend curve between two curves.
        /// The output is generally a PolyCurve with three segments: arc, line, arc.
        /// In some cases, one or more of those segments will be absent because they would have 0 length. 
        /// If there is only a single segment, the result will either be an ArcCurve or a LineCurve.
        /// </summary>
        /// <param name="startPt">Start of the blend curve.</param>
        /// <param name="startDir">Start direction of the blend curve.</param>
        /// <param name="endPt">End of the blend curve.</param>
        /// <param name="endDir">End direction of the arc blend curve.</param>
        /// <param name="radius">The radius of the arc segments.</param>
        /// <returns>The blend curve if successful, false otherwise.</returns>
        /// <remarks>
        /// The first arc segment will start at startPt, with starting tangent startDir.
        /// The second arc segment will end at endPt with end tangent endDir.
        /// The line segment will start from the end of the first arc segment and end at start of the second arc segment, 
        /// and it will be tangent to both arcs at those points.
        /// </remarks>
        public static async Task<Curve> CreateArcLineArcBlend(Point3d startPt, Vector3d startDir, Point3d endPt, Vector3d endDir, double radius)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), startPt, startDir, endPt, endDir, radius);
        }

        /// <summary>
        /// Creates a polycurve consisting of two tangent arc segments that connect two points and two directions.
        /// </summary>
        /// <param name="startPt">Start of the arc blend curve.</param>
        /// <param name="startDir">Start direction of the arc blend curve.</param>
        /// <param name="endPt">End of the arc blend curve.</param>
        /// <param name="endDir">End direction of the arc blend curve.</param>
        /// <param name="controlPointLengthRatio">
        /// The ratio of the control polygon lengths of the two arcs. Note, a value of 1.0 
        /// means the control polygon lengths for both arcs will be the same.
        /// </param>
        /// <returns>The arc blend curve, or null on error.</returns>
        public static async Task<Curve> CreateArcBlend(Point3d startPt, Vector3d startDir, Point3d endPt, Vector3d endDir, double controlPointLengthRatio)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), startPt, startDir, endPt, endDir, controlPointLengthRatio);
        }

        /// <summary>
        /// Constructs a mean, or average, curve from two curves.
        /// </summary>
        /// <param name="curveA">A first curve.</param>
        /// <param name="curveB">A second curve.</param>
        /// <param name="angleToleranceRadians">
        /// The angle tolerance, in radians, used to match kinks between curves.
        /// If you are unsure how to set this parameter, then either use the
        /// document's angle tolerance RhinoDoc.AngleToleranceRadians,
        /// or the default value (RhinoMath.UnsetValue)
        /// </param>
        /// <returns>The average curve, or null on error.</returns>
        /// <exception cref="ArgumentNullException">If curveA or curveB are null.</exception>
        public static async Task<Curve> CreateMeanCurve(Curve curveA, Curve curveB, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curveA, curveB, angleToleranceRadians);
        }

        /// <summary>
        /// Constructs a mean, or average, curve from two curves.
        /// </summary>
        /// <param name="curveA">A first curve.</param>
        /// <param name="curveB">A second curve.</param>
        /// <param name="angleToleranceRadians">
        /// The angle tolerance, in radians, used to match kinks between curves.
        /// If you are unsure how to set this parameter, then either use the
        /// document's angle tolerance RhinoDoc.AngleToleranceRadians,
        /// or the default value (RhinoMath.UnsetValue)
        /// </param>
        /// <returns>The average curve, or null on error.</returns>
        /// <exception cref="ArgumentNullException">If curveA or curveB are null.</exception>
        public static async Task<Curve> CreateMeanCurve(Remote<Curve> curveA, Remote<Curve> curveB, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curveA, curveB, angleToleranceRadians);
        }

        /// <summary>
        /// Constructs a mean, or average, curve from two curves.
        /// </summary>
        /// <param name="curveA">A first curve.</param>
        /// <param name="curveB">A second curve.</param>
        /// <returns>The average curve, or null on error.</returns>
        /// <exception cref="ArgumentNullException">If curveA or curveB are null.</exception>
        public static async Task<Curve> CreateMeanCurve(Curve curveA, Curve curveB)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curveA, curveB);
        }

        /// <summary>
        /// Constructs a mean, or average, curve from two curves.
        /// </summary>
        /// <param name="curveA">A first curve.</param>
        /// <param name="curveB">A second curve.</param>
        /// <returns>The average curve, or null on error.</returns>
        /// <exception cref="ArgumentNullException">If curveA or curveB are null.</exception>
        public static async Task<Curve> CreateMeanCurve(Remote<Curve> curveA, Remote<Curve> curveB)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curveA, curveB);
        }

        /// <summary>
        /// Create a Blend curve between two existing curves.
        /// </summary>
        /// <param name="curveA">Curve to blend from (blending will occur at curve end point).</param>
        /// <param name="curveB">Curve to blend to (blending will occur at curve start point).</param>
        /// <param name="continuity">Continuity of blend.</param>
        /// <returns>A curve representing the blend between A and B or null on failure.</returns>
        public static async Task<Curve> CreateBlendCurve(Curve curveA, Curve curveB, BlendContinuity continuity)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curveA, curveB, continuity);
        }

        /// <summary>
        /// Create a Blend curve between two existing curves.
        /// </summary>
        /// <param name="curveA">Curve to blend from (blending will occur at curve end point).</param>
        /// <param name="curveB">Curve to blend to (blending will occur at curve start point).</param>
        /// <param name="continuity">Continuity of blend.</param>
        /// <returns>A curve representing the blend between A and B or null on failure.</returns>
        public static async Task<Curve> CreateBlendCurve(Remote<Curve> curveA, Remote<Curve> curveB, BlendContinuity continuity)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curveA, curveB, continuity);
        }

        /// <summary>
        /// Create a Blend curve between two existing curves.
        /// </summary>
        /// <param name="curveA">Curve to blend from (blending will occur at curve end point).</param>
        /// <param name="curveB">Curve to blend to (blending will occur at curve start point).</param>
        /// <param name="continuity">Continuity of blend.</param>
        /// <param name="bulgeA">Bulge factor at curveA end of blend. Values near 1.0 work best.</param>
        /// <param name="bulgeB">Bulge factor at curveB end of blend. Values near 1.0 work best.</param>
        /// <returns>A curve representing the blend between A and B or null on failure.</returns>
        public static async Task<Curve> CreateBlendCurve(Curve curveA, Curve curveB, BlendContinuity continuity, double bulgeA, double bulgeB)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curveA, curveB, continuity, bulgeA, bulgeB);
        }

        /// <summary>
        /// Create a Blend curve between two existing curves.
        /// </summary>
        /// <param name="curveA">Curve to blend from (blending will occur at curve end point).</param>
        /// <param name="curveB">Curve to blend to (blending will occur at curve start point).</param>
        /// <param name="continuity">Continuity of blend.</param>
        /// <param name="bulgeA">Bulge factor at curveA end of blend. Values near 1.0 work best.</param>
        /// <param name="bulgeB">Bulge factor at curveB end of blend. Values near 1.0 work best.</param>
        /// <returns>A curve representing the blend between A and B or null on failure.</returns>
        public static async Task<Curve> CreateBlendCurve(Remote<Curve> curveA, Remote<Curve> curveB, BlendContinuity continuity, double bulgeA, double bulgeB)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curveA, curveB, continuity, bulgeA, bulgeB);
        }

        /// <summary>
        /// Makes a curve blend between 2 curves at the parameters specified
        /// with the directions and continuities specified
        /// </summary>
        /// <param name="curve0">First curve to blend from</param>
        /// <param name="t0">Parameter on first curve for blend endpoint</param>
        /// <param name="reverse0">
        /// If false, the blend will go in the natural direction of the curve.
        /// If true, the blend will go in the opposite direction to the curve
        /// </param>
        /// <param name="continuity0">Continuity for the blend at the start</param>
        /// <param name="curve1">Second curve to blend from</param>
        /// <param name="t1">Parameter on second curve for blend endpoint</param>
        /// <param name="reverse1">
        /// If false, the blend will go in the natural direction of the curve.
        /// If true, the blend will go in the opposite direction to the curve
        /// </param>
        /// <param name="continuity1">Continuity for the blend at the end</param>
        /// <returns>The blend curve on success. null on failure</returns>
        public static async Task<Curve> CreateBlendCurve(Curve curve0, double t0, bool reverse0, BlendContinuity continuity0, Curve curve1, double t1, bool reverse1, BlendContinuity continuity1)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve0, t0, reverse0, continuity0, curve1, t1, reverse1, continuity1);
        }

        /// <summary>
        /// Makes a curve blend between 2 curves at the parameters specified
        /// with the directions and continuities specified
        /// </summary>
        /// <param name="curve0">First curve to blend from</param>
        /// <param name="t0">Parameter on first curve for blend endpoint</param>
        /// <param name="reverse0">
        /// If false, the blend will go in the natural direction of the curve.
        /// If true, the blend will go in the opposite direction to the curve
        /// </param>
        /// <param name="continuity0">Continuity for the blend at the start</param>
        /// <param name="curve1">Second curve to blend from</param>
        /// <param name="t1">Parameter on second curve for blend endpoint</param>
        /// <param name="reverse1">
        /// If false, the blend will go in the natural direction of the curve.
        /// If true, the blend will go in the opposite direction to the curve
        /// </param>
        /// <param name="continuity1">Continuity for the blend at the end</param>
        /// <returns>The blend curve on success. null on failure</returns>
        public static async Task<Curve> CreateBlendCurve(Remote<Curve> curve0, double t0, bool reverse0, BlendContinuity continuity0, Remote<Curve> curve1, double t1, bool reverse1, BlendContinuity continuity1)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve0, t0, reverse0, continuity0, curve1, t1, reverse1, continuity1);
        }

        /// <summary>
        /// Changes a curve end to meet a specified curve with a specified continuity.
        /// </summary>
        /// <param name="curve0">The open curve to change.</param>
        /// <param name="reverse0">Reverse the direction of the curve to change before matching.</param>
        /// <param name="continuity">The continuity at the curve end.</param>
        /// <param name="curve1">The open curve to match.</param>
        /// <param name="reverse1">Reverse the direction of the curve to match before matching.</param>
        /// <param name="preserve">Prevent modification of the curvature at the end opposite the match for curves with fewer than six control points.</param>
        /// <param name="average">Adjust both curves to match each other.</param>
        /// <returns>The results of the curve matching, if successful, otherwise an empty array.</returns>
        public static async Task<Curve[]> CreateMatchCurve(Curve curve0, bool reverse0, BlendContinuity continuity, Curve curve1, bool reverse1, PreserveEnd preserve, bool average)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve0, reverse0, continuity, curve1, reverse1, preserve, average);
        }

        /// <summary>
        /// Changes a curve end to meet a specified curve with a specified continuity.
        /// </summary>
        /// <param name="curve0">The open curve to change.</param>
        /// <param name="reverse0">Reverse the direction of the curve to change before matching.</param>
        /// <param name="continuity">The continuity at the curve end.</param>
        /// <param name="curve1">The open curve to match.</param>
        /// <param name="reverse1">Reverse the direction of the curve to match before matching.</param>
        /// <param name="preserve">Prevent modification of the curvature at the end opposite the match for curves with fewer than six control points.</param>
        /// <param name="average">Adjust both curves to match each other.</param>
        /// <returns>The results of the curve matching, if successful, otherwise an empty array.</returns>
        public static async Task<Curve[]> CreateMatchCurve(Remote<Curve> curve0, bool reverse0, BlendContinuity continuity, Remote<Curve> curve1, bool reverse1, PreserveEnd preserve, bool average)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve0, reverse0, continuity, curve1, reverse1, preserve, average);
        }

        /// <summary>
        /// Creates curves between two open or closed input curves. Uses the control points of the curves for finding tween curves.
        /// That means the first control point of first curve is matched to first control point of the second curve and so on.
        /// There is no matching of curves direction. Caller must match input curves direction before calling the function.
        /// </summary>
        /// <param name="curve0">The first, or starting, curve.</param>
        /// <param name="curve1">The second, or ending, curve.</param>
        /// <param name="numCurves">Number of tween curves to create.</param>
        /// <returns>An array of tween curves. This array can be empty.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Curve[]> CreateTweenCurves(Curve curve0, Curve curve1, int numCurves)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve0, curve1, numCurves);
        }

        /// <summary>
        /// Creates curves between two open or closed input curves. Uses the control points of the curves for finding tween curves.
        /// That means the first control point of first curve is matched to first control point of the second curve and so on.
        /// There is no matching of curves direction. Caller must match input curves direction before calling the function.
        /// </summary>
        /// <param name="curve0">The first, or starting, curve.</param>
        /// <param name="curve1">The second, or ending, curve.</param>
        /// <param name="numCurves">Number of tween curves to create.</param>
        /// <returns>An array of tween curves. This array can be empty.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Curve[]> CreateTweenCurves(Remote<Curve> curve0, Remote<Curve> curve1, int numCurves)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve0, curve1, numCurves);
        }

        /// <summary>
        /// Creates curves between two open or closed input curves. Uses the control points of the curves for finding tween curves.
        /// That means the first control point of first curve is matched to first control point of the second curve and so on.
        /// There is no matching of curves direction. Caller must match input curves direction before calling the function.
        /// </summary>
        /// <param name="curve0">The first, or starting, curve.</param>
        /// <param name="curve1">The second, or ending, curve.</param>
        /// <param name="numCurves">Number of tween curves to create.</param>
        /// <param name="tolerance"></param>
        /// <returns>An array of tween curves. This array can be empty.</returns>
        public static async Task<Curve[]> CreateTweenCurves(Curve curve0, Curve curve1, int numCurves, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve0, curve1, numCurves, tolerance);
        }

        /// <summary>
        /// Creates curves between two open or closed input curves. Uses the control points of the curves for finding tween curves.
        /// That means the first control point of first curve is matched to first control point of the second curve and so on.
        /// There is no matching of curves direction. Caller must match input curves direction before calling the function.
        /// </summary>
        /// <param name="curve0">The first, or starting, curve.</param>
        /// <param name="curve1">The second, or ending, curve.</param>
        /// <param name="numCurves">Number of tween curves to create.</param>
        /// <param name="tolerance"></param>
        /// <returns>An array of tween curves. This array can be empty.</returns>
        public static async Task<Curve[]> CreateTweenCurves(Remote<Curve> curve0, Remote<Curve> curve1, int numCurves, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve0, curve1, numCurves, tolerance);
        }

        /// <summary>
        /// Creates curves between two open or closed input curves. Make the structure of input curves compatible if needed.
        /// Refits the input curves to have the same structure. The resulting curves are usually more complex than input unless
        /// input curves are compatible and no refit is needed. There is no matching of curves direction.
        /// Caller must match input curves direction before calling the function.
        /// </summary>
        /// <param name="curve0">The first, or starting, curve.</param>
        /// <param name="curve1">The second, or ending, curve.</param>
        /// <param name="numCurves">Number of tween curves to create.</param>
        /// <returns>An array of tween curves. This array can be empty.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Curve[]> CreateTweenCurvesWithMatching(Curve curve0, Curve curve1, int numCurves)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve0, curve1, numCurves);
        }

        /// <summary>
        /// Creates curves between two open or closed input curves. Make the structure of input curves compatible if needed.
        /// Refits the input curves to have the same structure. The resulting curves are usually more complex than input unless
        /// input curves are compatible and no refit is needed. There is no matching of curves direction.
        /// Caller must match input curves direction before calling the function.
        /// </summary>
        /// <param name="curve0">The first, or starting, curve.</param>
        /// <param name="curve1">The second, or ending, curve.</param>
        /// <param name="numCurves">Number of tween curves to create.</param>
        /// <returns>An array of tween curves. This array can be empty.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Curve[]> CreateTweenCurvesWithMatching(Remote<Curve> curve0, Remote<Curve> curve1, int numCurves)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve0, curve1, numCurves);
        }

        /// <summary>
        /// Creates curves between two open or closed input curves. Make the structure of input curves compatible if needed.
        /// Refits the input curves to have the same structure. The resulting curves are usually more complex than input unless
        /// input curves are compatible and no refit is needed. There is no matching of curves direction.
        /// Caller must match input curves direction before calling the function.
        /// </summary>
        /// <param name="curve0">The first, or starting, curve.</param>
        /// <param name="curve1">The second, or ending, curve.</param>
        /// <param name="numCurves">Number of tween curves to create.</param>
        /// <param name="tolerance"></param>
        /// <returns>An array of tween curves. This array can be empty.</returns>
        public static async Task<Curve[]> CreateTweenCurvesWithMatching(Curve curve0, Curve curve1, int numCurves, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve0, curve1, numCurves, tolerance);
        }

        /// <summary>
        /// Creates curves between two open or closed input curves. Make the structure of input curves compatible if needed.
        /// Refits the input curves to have the same structure. The resulting curves are usually more complex than input unless
        /// input curves are compatible and no refit is needed. There is no matching of curves direction.
        /// Caller must match input curves direction before calling the function.
        /// </summary>
        /// <param name="curve0">The first, or starting, curve.</param>
        /// <param name="curve1">The second, or ending, curve.</param>
        /// <param name="numCurves">Number of tween curves to create.</param>
        /// <param name="tolerance"></param>
        /// <returns>An array of tween curves. This array can be empty.</returns>
        public static async Task<Curve[]> CreateTweenCurvesWithMatching(Remote<Curve> curve0, Remote<Curve> curve1, int numCurves, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve0, curve1, numCurves, tolerance);
        }

        /// <summary>
        /// Creates curves between two open or closed input curves. Use sample points method to make curves compatible.
        /// This is how the algorithm works: Divides the two curves into an equal number of points, finds the midpoint between the 
        /// corresponding points on the curves and interpolates the tween curve through those points. There is no matching of curves
        /// direction. Caller must match input curves direction before calling the function.
        /// </summary>
        /// <param name="curve0">The first, or starting, curve.</param>
        /// <param name="curve1">The second, or ending, curve.</param>
        /// <param name="numCurves">Number of tween curves to create.</param>
        /// <param name="numSamples">Number of sample points along input curves.</param>
        /// <returns>>An array of tween curves. This array can be empty.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Curve[]> CreateTweenCurvesWithSampling(Curve curve0, Curve curve1, int numCurves, int numSamples)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve0, curve1, numCurves, numSamples);
        }

        /// <summary>
        /// Creates curves between two open or closed input curves. Use sample points method to make curves compatible.
        /// This is how the algorithm works: Divides the two curves into an equal number of points, finds the midpoint between the 
        /// corresponding points on the curves and interpolates the tween curve through those points. There is no matching of curves
        /// direction. Caller must match input curves direction before calling the function.
        /// </summary>
        /// <param name="curve0">The first, or starting, curve.</param>
        /// <param name="curve1">The second, or ending, curve.</param>
        /// <param name="numCurves">Number of tween curves to create.</param>
        /// <param name="numSamples">Number of sample points along input curves.</param>
        /// <returns>>An array of tween curves. This array can be empty.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Curve[]> CreateTweenCurvesWithSampling(Remote<Curve> curve0, Remote<Curve> curve1, int numCurves, int numSamples)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve0, curve1, numCurves, numSamples);
        }

        /// <summary>
        /// Creates curves between two open or closed input curves. Use sample points method to make curves compatible.
        /// This is how the algorithm works: Divides the two curves into an equal number of points, finds the midpoint between the 
        /// corresponding points on the curves and interpolates the tween curve through those points. There is no matching of curves
        /// direction. Caller must match input curves direction before calling the function.
        /// </summary>
        /// <param name="curve0">The first, or starting, curve.</param>
        /// <param name="curve1">The second, or ending, curve.</param>
        /// <param name="numCurves">Number of tween curves to create.</param>
        /// <param name="numSamples">Number of sample points along input curves.</param>
        /// <param name="tolerance">The tolerance. When in doubt, use the document's model absolute tolerance.</param>
        /// <returns>>An array of tween curves. This array can be empty.</returns>
        public static async Task<Curve[]> CreateTweenCurvesWithSampling(Curve curve0, Curve curve1, int numCurves, int numSamples, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve0, curve1, numCurves, numSamples, tolerance);
        }

        /// <summary>
        /// Creates curves between two open or closed input curves. Use sample points method to make curves compatible.
        /// This is how the algorithm works: Divides the two curves into an equal number of points, finds the midpoint between the 
        /// corresponding points on the curves and interpolates the tween curve through those points. There is no matching of curves
        /// direction. Caller must match input curves direction before calling the function.
        /// </summary>
        /// <param name="curve0">The first, or starting, curve.</param>
        /// <param name="curve1">The second, or ending, curve.</param>
        /// <param name="numCurves">Number of tween curves to create.</param>
        /// <param name="numSamples">Number of sample points along input curves.</param>
        /// <param name="tolerance">The tolerance. When in doubt, use the document's model absolute tolerance.</param>
        /// <returns>>An array of tween curves. This array can be empty.</returns>
        public static async Task<Curve[]> CreateTweenCurvesWithSampling(Remote<Curve> curve0, Remote<Curve> curve1, int numCurves, int numSamples, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve0, curve1, numCurves, numSamples, tolerance);
        }

        /// <summary>
        /// Makes adjustments to the ends of one or both input curves so that they meet at a point.
        /// </summary>
        /// <param name="curveA">1st curve to adjust.</param>
        /// <param name="adjustStartCurveA">
        /// Which end of the 1st curve to adjust: true is start, false is end.
        /// </param>
        /// <param name="curveB">2nd curve to adjust.</param>
        /// <param name="adjustStartCurveB">
        /// which end of the 2nd curve to adjust true==start, false==end.
        /// </param>
        /// <returns>true on success.</returns>
        public static async Task<bool> MakeEndsMeet(Curve curveA, bool adjustStartCurveA, Curve curveB, bool adjustStartCurveB)
        {
            return await ComputeServer.PostAsync<bool>(ApiAddress(), curveA, adjustStartCurveA, curveB, adjustStartCurveB);
        }

        /// <summary>
        /// Makes adjustments to the ends of one or both input curves so that they meet at a point.
        /// </summary>
        /// <param name="curveA">1st curve to adjust.</param>
        /// <param name="adjustStartCurveA">
        /// Which end of the 1st curve to adjust: true is start, false is end.
        /// </param>
        /// <param name="curveB">2nd curve to adjust.</param>
        /// <param name="adjustStartCurveB">
        /// which end of the 2nd curve to adjust true==start, false==end.
        /// </param>
        /// <returns>true on success.</returns>
        public static async Task<bool> MakeEndsMeet(Remote<Curve> curveA, bool adjustStartCurveA, Remote<Curve> curveB, bool adjustStartCurveB)
        {
            return await ComputeServer.PostAsync<bool>(ApiAddress(), curveA, adjustStartCurveA, curveB, adjustStartCurveB);
        }

        /// <summary>
        /// Computes the fillet arc for a curve filleting operation.
        /// </summary>
        /// <param name="curve0">First curve to fillet.</param>
        /// <param name="curve1">Second curve to fillet.</param>
        /// <param name="radius">Fillet radius.</param>
        /// <param name="t0Base">Parameter on curve0 where the fillet ought to start (approximately).</param>
        /// <param name="t1Base">Parameter on curve1 where the fillet ought to end (approximately).</param>
        /// <returns>The fillet arc on success, or Arc.Unset on failure.</returns>
        public static async Task<Arc> CreateFillet(Curve curve0, Curve curve1, double radius, double t0Base, double t1Base)
        {
            return await ComputeServer.PostAsync<Arc>(ApiAddress(), curve0, curve1, radius, t0Base, t1Base);
        }

        /// <summary>
        /// Computes the fillet arc for a curve filleting operation.
        /// </summary>
        /// <param name="curve0">First curve to fillet.</param>
        /// <param name="curve1">Second curve to fillet.</param>
        /// <param name="radius">Fillet radius.</param>
        /// <param name="t0Base">Parameter on curve0 where the fillet ought to start (approximately).</param>
        /// <param name="t1Base">Parameter on curve1 where the fillet ought to end (approximately).</param>
        /// <returns>The fillet arc on success, or Arc.Unset on failure.</returns>
        public static async Task<Arc> CreateFillet(Remote<Curve> curve0, Remote<Curve> curve1, double radius, double t0Base, double t1Base)
        {
            return await ComputeServer.PostAsync<Arc>(ApiAddress(), curve0, curve1, radius, t0Base, t1Base);
        }

        /// <summary>
        /// Creates a tangent arc between two curves and trims or extends the curves to the arc.
        /// </summary>
        /// <param name="curve0">The first curve to fillet.</param>
        /// <param name="point0">
        /// A point on the first curve that is near the end where the fillet will
        /// be created.
        /// </param>
        /// <param name="curve1">The second curve to fillet.</param>
        /// <param name="point1">
        /// A point on the second curve that is near the end where the fillet will
        /// be created.
        /// </param>
        /// <param name="radius">The radius of the fillet.</param>
        /// <param name="join">Join the output curves.</param>
        /// <param name="trim">
        /// Trim copies of the input curves to the output fillet curve.
        /// </param>
        /// <param name="arcExtension">
        /// Applies when arcs are filleted but need to be extended to meet the
        /// fillet curve or chamfer line. If true, then the arc is extended
        /// maintaining its validity. If false, then the arc is extended with a
        /// line segment, which is joined to the arc converting it to a polycurve.
        /// </param>
        /// <param name="tolerance">
        /// The tolerance, generally the document's absolute tolerance.
        /// </param>
        /// <param name="angleTolerance"></param>
        /// <returns>
        /// The results of the fillet operation. The number of output curves depends
        /// on the input curves and the values of the parameters that were used
        /// during the fillet operation. In most cases, the output array will contain
        /// either one or three curves, although two curves can be returned if the
        /// radius is zero and join = false.
        /// For example, if both join and trim = true, then the output curve
        /// will be a polycurve containing the fillet curve joined with trimmed copies
        /// of the input curves. If join = false and trim = true, then three curves,
        /// the fillet curve and trimmed copies of the input curves, will be returned.
        /// If both join and trim = false, then just the fillet curve is returned.
        /// </returns>
        /// <example>
        /// <code source='examples\vbnet\ex_filletcurves.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_filletcurves.cs' lang='cs'/>
        /// <code source='examples\py\ex_filletcurves.py' lang='py'/>
        /// </example>
        public static async Task<Curve[]> CreateFilletCurves(Curve curve0, Point3d point0, Curve curve1, Point3d point1, double radius, bool join, bool trim, bool arcExtension, double tolerance, double angleTolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve0, point0, curve1, point1, radius, join, trim, arcExtension, tolerance, angleTolerance);
        }

        /// <summary>
        /// Creates a tangent arc between two curves and trims or extends the curves to the arc.
        /// </summary>
        /// <param name="curve0">The first curve to fillet.</param>
        /// <param name="point0">
        /// A point on the first curve that is near the end where the fillet will
        /// be created.
        /// </param>
        /// <param name="curve1">The second curve to fillet.</param>
        /// <param name="point1">
        /// A point on the second curve that is near the end where the fillet will
        /// be created.
        /// </param>
        /// <param name="radius">The radius of the fillet.</param>
        /// <param name="join">Join the output curves.</param>
        /// <param name="trim">
        /// Trim copies of the input curves to the output fillet curve.
        /// </param>
        /// <param name="arcExtension">
        /// Applies when arcs are filleted but need to be extended to meet the
        /// fillet curve or chamfer line. If true, then the arc is extended
        /// maintaining its validity. If false, then the arc is extended with a
        /// line segment, which is joined to the arc converting it to a polycurve.
        /// </param>
        /// <param name="tolerance">
        /// The tolerance, generally the document's absolute tolerance.
        /// </param>
        /// <param name="angleTolerance"></param>
        /// <returns>
        /// The results of the fillet operation. The number of output curves depends
        /// on the input curves and the values of the parameters that were used
        /// during the fillet operation. In most cases, the output array will contain
        /// either one or three curves, although two curves can be returned if the
        /// radius is zero and join = false.
        /// For example, if both join and trim = true, then the output curve
        /// will be a polycurve containing the fillet curve joined with trimmed copies
        /// of the input curves. If join = false and trim = true, then three curves,
        /// the fillet curve and trimmed copies of the input curves, will be returned.
        /// If both join and trim = false, then just the fillet curve is returned.
        /// </returns>
        /// <example>
        /// <code source='examples\vbnet\ex_filletcurves.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_filletcurves.cs' lang='cs'/>
        /// <code source='examples\py\ex_filletcurves.py' lang='py'/>
        /// </example>
        public static async Task<Curve[]> CreateFilletCurves(Remote<Curve> curve0, Point3d point0, Remote<Curve> curve1, Point3d point1, double radius, bool join, bool trim, bool arcExtension, double tolerance, double angleTolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve0, point0, curve1, point1, radius, join, trim, arcExtension, tolerance, angleTolerance);
        }

        /// <summary>
        /// Calculates the boolean union of two or more closed, planar curves. 
        /// Note, curves must be co-planar.
        /// </summary>
        /// <param name="curves">The co-planar curves to union.</param>
        /// <returns>Result curves on success, empty array if no union could be calculated.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Curve[]> CreateBooleanUnion(IEnumerable<Curve> curves)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curves);
        }

        /// <summary>
        /// Calculates the boolean union of two or more closed, planar curves. 
        /// Note, curves must be co-planar.
        /// </summary>
        /// <param name="curves">The co-planar curves to union.</param>
        /// <returns>Result curves on success, empty array if no union could be calculated.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Curve[]> CreateBooleanUnion(Remote<IEnumerable<Curve>> curves)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curves);
        }

        /// <summary>
        /// Calculates the boolean union of two or more closed, planar curves. 
        /// Note, curves must be co-planar.
        /// </summary>
        /// <param name="curves">The co-planar curves to union.</param>
        /// <param name="tolerance"></param>
        /// <returns>Result curves on success, empty array if no union could be calculated.</returns>
        public static async Task<Curve[]> CreateBooleanUnion(IEnumerable<Curve> curves, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curves, tolerance);
        }

        /// <summary>
        /// Calculates the boolean union of two or more closed, planar curves. 
        /// Note, curves must be co-planar.
        /// </summary>
        /// <param name="curves">The co-planar curves to union.</param>
        /// <param name="tolerance"></param>
        /// <returns>Result curves on success, empty array if no union could be calculated.</returns>
        public static async Task<Curve[]> CreateBooleanUnion(Remote<IEnumerable<Curve>> curves, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curves, tolerance);
        }

        /// <summary>
        /// Calculates the boolean intersection of two closed, planar curves. 
        /// Note, curves must be co-planar.
        /// </summary>
        /// <param name="curveA">The first closed, planar curve.</param>
        /// <param name="curveB">The second closed, planar curve.</param>
        /// <returns>Result curves on success, empty array if no intersection could be calculated.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Curve[]> CreateBooleanIntersection(Curve curveA, Curve curveB)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curveA, curveB);
        }

        /// <summary>
        /// Calculates the boolean intersection of two closed, planar curves. 
        /// Note, curves must be co-planar.
        /// </summary>
        /// <param name="curveA">The first closed, planar curve.</param>
        /// <param name="curveB">The second closed, planar curve.</param>
        /// <returns>Result curves on success, empty array if no intersection could be calculated.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Curve[]> CreateBooleanIntersection(Remote<Curve> curveA, Remote<Curve> curveB)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curveA, curveB);
        }

        /// <summary>
        /// Calculates the boolean intersection of two closed, planar curves. 
        /// Note, curves must be co-planar.
        /// </summary>
        /// <param name="curveA">The first closed, planar curve.</param>
        /// <param name="curveB">The second closed, planar curve.</param>
        /// <param name="tolerance"></param>
        /// <returns>Result curves on success, empty array if no intersection could be calculated.</returns>
        public static async Task<Curve[]> CreateBooleanIntersection(Curve curveA, Curve curveB, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curveA, curveB, tolerance);
        }

        /// <summary>
        /// Calculates the boolean intersection of two closed, planar curves. 
        /// Note, curves must be co-planar.
        /// </summary>
        /// <param name="curveA">The first closed, planar curve.</param>
        /// <param name="curveB">The second closed, planar curve.</param>
        /// <param name="tolerance"></param>
        /// <returns>Result curves on success, empty array if no intersection could be calculated.</returns>
        public static async Task<Curve[]> CreateBooleanIntersection(Remote<Curve> curveA, Remote<Curve> curveB, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curveA, curveB, tolerance);
        }

        /// <summary>
        /// Calculates the boolean difference between two closed, planar curves. 
        /// Note, curves must be co-planar.
        /// </summary>
        /// <param name="curveA">The first closed, planar curve.</param>
        /// <param name="curveB">The second closed, planar curve.</param>
        /// <returns>Result curves on success, empty array if no difference could be calculated.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Curve[]> CreateBooleanDifference(Curve curveA, Curve curveB)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curveA, curveB);
        }

        /// <summary>
        /// Calculates the boolean difference between two closed, planar curves. 
        /// Note, curves must be co-planar.
        /// </summary>
        /// <param name="curveA">The first closed, planar curve.</param>
        /// <param name="curveB">The second closed, planar curve.</param>
        /// <returns>Result curves on success, empty array if no difference could be calculated.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Curve[]> CreateBooleanDifference(Remote<Curve> curveA, Remote<Curve> curveB)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curveA, curveB);
        }

        /// <summary>
        /// Calculates the boolean difference between two closed, planar curves. 
        /// Note, curves must be co-planar.
        /// </summary>
        /// <param name="curveA">The first closed, planar curve.</param>
        /// <param name="curveB">The second closed, planar curve.</param>
        /// <param name="tolerance"></param>
        /// <returns>Result curves on success, empty array if no difference could be calculated.</returns>
        public static async Task<Curve[]> CreateBooleanDifference(Curve curveA, Curve curveB, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curveA, curveB, tolerance);
        }

        /// <summary>
        /// Calculates the boolean difference between two closed, planar curves. 
        /// Note, curves must be co-planar.
        /// </summary>
        /// <param name="curveA">The first closed, planar curve.</param>
        /// <param name="curveB">The second closed, planar curve.</param>
        /// <param name="tolerance"></param>
        /// <returns>Result curves on success, empty array if no difference could be calculated.</returns>
        public static async Task<Curve[]> CreateBooleanDifference(Remote<Curve> curveA, Remote<Curve> curveB, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curveA, curveB, tolerance);
        }

        /// <summary>
        /// Calculates the boolean difference between a closed planar curve, and a list of closed planar curves. 
        /// Note, curves must be co-planar.
        /// </summary>
        /// <param name="curveA">The first closed, planar curve.</param>
        /// <param name="subtractors">curves to subtract from the first closed curve.</param>
        /// <returns>Result curves on success, empty array if no difference could be calculated.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Curve[]> CreateBooleanDifference(Curve curveA, IEnumerable<Curve> subtractors)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curveA, subtractors);
        }

        /// <summary>
        /// Calculates the boolean difference between a closed planar curve, and a list of closed planar curves. 
        /// Note, curves must be co-planar.
        /// </summary>
        /// <param name="curveA">The first closed, planar curve.</param>
        /// <param name="subtractors">curves to subtract from the first closed curve.</param>
        /// <returns>Result curves on success, empty array if no difference could be calculated.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Curve[]> CreateBooleanDifference(Remote<Curve> curveA, Remote<IEnumerable<Curve>> subtractors)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curveA, subtractors);
        }

        /// <summary>
        /// Calculates the boolean difference between a closed planar curve, and a list of closed planar curves. 
        /// Note, curves must be co-planar.
        /// </summary>
        /// <param name="curveA">The first closed, planar curve.</param>
        /// <param name="subtractors">curves to subtract from the first closed curve.</param>
        /// <param name="tolerance"></param>
        /// <returns>Result curves on success, empty array if no difference could be calculated.</returns>
        public static async Task<Curve[]> CreateBooleanDifference(Curve curveA, IEnumerable<Curve> subtractors, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curveA, subtractors, tolerance);
        }

        /// <summary>
        /// Calculates the boolean difference between a closed planar curve, and a list of closed planar curves. 
        /// Note, curves must be co-planar.
        /// </summary>
        /// <param name="curveA">The first closed, planar curve.</param>
        /// <param name="subtractors">curves to subtract from the first closed curve.</param>
        /// <param name="tolerance"></param>
        /// <returns>Result curves on success, empty array if no difference could be calculated.</returns>
        public static async Task<Curve[]> CreateBooleanDifference(Remote<Curve> curveA, Remote<IEnumerable<Curve>> subtractors, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curveA, subtractors, tolerance);
        }

        /// <summary>
        /// Creates outline curves created from a text string. The functionality is similar to what you find in Rhino's TextObject command or TextEntity.Explode() in RhinoCommon.
        /// </summary>
        /// <param name="text">The text from which to create outline curves.</param>
        /// <param name="font">The text font. If the font does not exist on the system. Rhino will use a substitute with similar properties.</param>
        /// <param name="textHeight">The text height.</param>
        /// <param name="textStyle">The font style. The font style can be any number of the following: 0 - Normal, 1 - Bold, 2 - Italic</param>
        /// <param name="closeLoops">Set this value to True when dealing with normal fonts and when you expect closed loops. You may want to set this to False when specifying a single-stroke font where you don't want closed loops.</param>
        /// <param name="plane">The plane on which the outline curves will lie.</param>
        /// <param name="smallCapsScale">Displays lower-case letters as small caps. Set the relative text size to a percentage of the normal text.</param>
        /// <param name="tolerance">The tolerance for the operation.</param>
        /// <returns>An array containing one or more curves if successful.</returns>
        public static async Task<Curve[]> CreateTextOutlines(string text, string font, double textHeight, int textStyle, bool closeLoops, Plane plane, double smallCapsScale, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), text, font, textHeight, textStyle, closeLoops, plane, smallCapsScale, tolerance);
        }

        /// <summary>
        /// Creates a third curve from two curves that are planar in different construction planes. 
        /// The new curve looks the same as each of the original curves when viewed in each plane.
        /// </summary>
        /// <param name="curveA">The first curve.</param>
        /// <param name="curveB">The second curve.</param>
        /// <param name="vectorA">A vector defining the normal direction of the plane which the first curve is drawn upon.</param>
        /// <param name="vectorB">A vector defining the normal direction of the plane which the second curve is drawn upon.</param>
        /// <param name="tolerance">The tolerance for the operation.</param>
        /// <param name="angleTolerance">The angle tolerance for the operation.</param>
        /// <returns>An array containing one or more curves if successful.</returns>
        public static async Task<Curve[]> CreateCurve2View(Curve curveA, Curve curveB, Vector3d vectorA, Vector3d vectorB, double tolerance, double angleTolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curveA, curveB, vectorA, vectorB, tolerance, angleTolerance);
        }

        /// <summary>
        /// Creates a third curve from two curves that are planar in different construction planes. 
        /// The new curve looks the same as each of the original curves when viewed in each plane.
        /// </summary>
        /// <param name="curveA">The first curve.</param>
        /// <param name="curveB">The second curve.</param>
        /// <param name="vectorA">A vector defining the normal direction of the plane which the first curve is drawn upon.</param>
        /// <param name="vectorB">A vector defining the normal direction of the plane which the second curve is drawn upon.</param>
        /// <param name="tolerance">The tolerance for the operation.</param>
        /// <param name="angleTolerance">The angle tolerance for the operation.</param>
        /// <returns>An array containing one or more curves if successful.</returns>
        public static async Task<Curve[]> CreateCurve2View(Remote<Curve> curveA, Remote<Curve> curveB, Vector3d vectorA, Vector3d vectorB, double tolerance, double angleTolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curveA, curveB, vectorA, vectorB, tolerance, angleTolerance);
        }

        /// <summary>
        /// Creates a revision cloud curve from a planar curve.
        /// </summary>
        /// <param name="curve">The input planar curve.</param>
        /// <param name="segmentCount">
        /// The number of segments in the output revision cloud curve.
        /// If zero, the number of segments in the output revision cloud curve is based on the NURB form of the input curve.
        /// </param>
        /// <param name="angle">
        /// The angle in radians, between PI/2.0 and PI radians (90 and 180 degrees). 
        /// This angle indicates the amount of bulge in the segments.
        /// </param>
        /// <param name="flip">The arc segments in output revision cloud curve will be in the opposite direction to the input curve.</param>
        /// <returns>A revision cloud curve is successful, false otherwise.</returns>
        public static async Task<Curve> CreateRevisionCloud(Curve curve, int segmentCount, double angle, bool flip)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, segmentCount, angle, flip);
        }

        /// <summary>
        /// Creates a revision cloud curve from a planar curve.
        /// </summary>
        /// <param name="curve">The input planar curve.</param>
        /// <param name="segmentCount">
        /// The number of segments in the output revision cloud curve.
        /// If zero, the number of segments in the output revision cloud curve is based on the NURB form of the input curve.
        /// </param>
        /// <param name="angle">
        /// The angle in radians, between PI/2.0 and PI radians (90 and 180 degrees). 
        /// This angle indicates the amount of bulge in the segments.
        /// </param>
        /// <param name="flip">The arc segments in output revision cloud curve will be in the opposite direction to the input curve.</param>
        /// <returns>A revision cloud curve is successful, false otherwise.</returns>
        public static async Task<Curve> CreateRevisionCloud(Remote<Curve> curve, int segmentCount, double angle, bool flip)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, segmentCount, angle, flip);
        }

        /// <summary>
        /// Creates a revision cloud curve from points that make up a planar polyline.
        /// </summary>
        /// <param name="points">The input points.</param>
        /// <param name="angle">
        /// The angle in radians, between PI/2.0 and PI radians (90 and 180 degrees). 
        /// This angle indicates the amount of bulge in the segments.
        /// </param>
        /// <param name="flip">The arc segments in output revision cloud curve will be in the opposite direction to the input curve.</param>
        /// <returns>A revision cloud curve is successful, false otherwise.</returns>
        public static async Task<Curve> CreateRevisionCloud(IEnumerable<Point3d> points, double angle, bool flip)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), points, angle, flip);
        }

        /// <summary>
        /// Determines whether two curves travel more or less in the same direction.
        /// </summary>
        /// <param name="curveA">First curve to test.</param>
        /// <param name="curveB">Second curve to test.</param>
        /// <returns>true if both curves more or less point in the same direction, 
        /// false if they point in the opposite directions.</returns>
        public static async Task<bool> DoDirectionsMatch(Curve curveA, Curve curveB)
        {
            return await ComputeServer.PostAsync<bool>(ApiAddress(), curveA, curveB);
        }

        /// <summary>
        /// Determines whether two curves travel more or less in the same direction.
        /// </summary>
        /// <param name="curveA">First curve to test.</param>
        /// <param name="curveB">Second curve to test.</param>
        /// <returns>true if both curves more or less point in the same direction, 
        /// false if they point in the opposite directions.</returns>
        public static async Task<bool> DoDirectionsMatch(Remote<Curve> curveA, Remote<Curve> curveB)
        {
            return await ComputeServer.PostAsync<bool>(ApiAddress(), curveA, curveB);
        }

        /// <summary>
        /// Projects a curve to a mesh using a direction and tolerance.
        /// </summary>
        /// <param name="curve">A curve.</param>
        /// <param name="mesh">A mesh.</param>
        /// <param name="direction">A direction vector.</param>
        /// <param name="tolerance">A tolerance value.</param>
        /// <returns>An array of curves if successful, an empty array otherwise.</returns>
        public static async Task<Curve[]> ProjectToMesh(Curve curve, Mesh mesh, Vector3d direction, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, mesh, direction, tolerance);
        }

        /// <summary>
        /// Projects a curve to a mesh using a direction and tolerance.
        /// </summary>
        /// <param name="curve">A curve.</param>
        /// <param name="mesh">A mesh.</param>
        /// <param name="direction">A direction vector.</param>
        /// <param name="tolerance">A tolerance value.</param>
        /// <returns>An array of curves if successful, an empty array otherwise.</returns>
        public static async Task<Curve[]> ProjectToMesh(Remote<Curve> curve, Remote<Mesh> mesh, Vector3d direction, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, mesh, direction, tolerance);
        }

        /// <summary>
        /// Projects a curve to a set of meshes using a direction and tolerance.
        /// </summary>
        /// <param name="curve">A curve.</param>
        /// <param name="meshes">A list, an array, or any enumerable of meshes.</param>
        /// <param name="direction">A direction vector.</param>
        /// <param name="tolerance">A tolerance value.</param>
        /// <returns>An array of curves if successful, an empty array otherwise.</returns>
        public static async Task<Curve[]> ProjectToMesh(Curve curve, IEnumerable<Mesh> meshes, Vector3d direction, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, meshes, direction, tolerance);
        }

        /// <summary>
        /// Projects a curve to a set of meshes using a direction and tolerance.
        /// </summary>
        /// <param name="curve">A curve.</param>
        /// <param name="meshes">A list, an array, or any enumerable of meshes.</param>
        /// <param name="direction">A direction vector.</param>
        /// <param name="tolerance">A tolerance value.</param>
        /// <returns>An array of curves if successful, an empty array otherwise.</returns>
        public static async Task<Curve[]> ProjectToMesh(Remote<Curve> curve, Remote<IEnumerable<Mesh>> meshes, Vector3d direction, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, meshes, direction, tolerance);
        }

        /// <summary>
        /// Projects a curve to a set of meshes using a direction and tolerance.
        /// </summary>
        /// <param name="curves">A list, an array, or any enumerable of curves.</param>
        /// <param name="meshes">A list, an array, or any enumerable of meshes.</param>
        /// <param name="direction">A direction vector.</param>
        /// <param name="tolerance">A tolerance value.</param>
        /// <returns>An array of curves if successful, an empty array otherwise.</returns>
        public static async Task<Curve[]> ProjectToMesh(IEnumerable<Curve> curves, IEnumerable<Mesh> meshes, Vector3d direction, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curves, meshes, direction, tolerance);
        }

        /// <summary>
        /// Projects a curve to a set of meshes using a direction and tolerance.
        /// </summary>
        /// <param name="curves">A list, an array, or any enumerable of curves.</param>
        /// <param name="meshes">A list, an array, or any enumerable of meshes.</param>
        /// <param name="direction">A direction vector.</param>
        /// <param name="tolerance">A tolerance value.</param>
        /// <returns>An array of curves if successful, an empty array otherwise.</returns>
        public static async Task<Curve[]> ProjectToMesh(Remote<IEnumerable<Curve>> curves, Remote<IEnumerable<Mesh>> meshes, Vector3d direction, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curves, meshes, direction, tolerance);
        }

        /// <summary>
        /// Projects a curve to a set of meshes using a direction and tolerance.
        /// </summary>
        /// <param name="curves">A list, an array, or any enumerable of curves.</param>
        /// <param name="meshes">A list, an array, or any enumerable of meshes.</param>
        /// <param name="direction">A direction vector.</param>
        /// <param name="tolerance">A tolerance value.</param>
        /// <param name="loose">If true, then project curve edit points onto meshes.</param>
        /// <returns>An array of curves if successful, an empty array otherwise.</returns>
        public static async Task<Curve[]> ProjectToMesh(IEnumerable<Curve> curves, IEnumerable<Mesh> meshes, Vector3d direction, double tolerance, bool loose)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curves, meshes, direction, tolerance, loose);
        }

        /// <summary>
        /// Projects a curve to a set of meshes using a direction and tolerance.
        /// </summary>
        /// <param name="curves">A list, an array, or any enumerable of curves.</param>
        /// <param name="meshes">A list, an array, or any enumerable of meshes.</param>
        /// <param name="direction">A direction vector.</param>
        /// <param name="tolerance">A tolerance value.</param>
        /// <param name="loose">If true, then project curve edit points onto meshes.</param>
        /// <returns>An array of curves if successful, an empty array otherwise.</returns>
        public static async Task<Curve[]> ProjectToMesh(Remote<IEnumerable<Curve>> curves, Remote<IEnumerable<Mesh>> meshes, Vector3d direction, double tolerance, bool loose)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curves, meshes, direction, tolerance, loose);
        }

        /// <summary>
        /// Projects a Curve onto a Brep along a given direction.
        /// </summary>
        /// <param name="curve">Curve to project.</param>
        /// <param name="brep">Brep to project onto.</param>
        /// <param name="direction">Direction of projection.</param>
        /// <param name="tolerance">Tolerance to use for projection.</param>
        /// <returns>An array of projected curves or empty array if the projection set is empty.</returns>
        public static async Task<Curve[]> ProjectToBrep(Curve curve, Brep brep, Vector3d direction, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, brep, direction, tolerance);
        }

        /// <summary>
        /// Projects a Curve onto a Brep along a given direction.
        /// </summary>
        /// <param name="curve">Curve to project.</param>
        /// <param name="brep">Brep to project onto.</param>
        /// <param name="direction">Direction of projection.</param>
        /// <param name="tolerance">Tolerance to use for projection.</param>
        /// <returns>An array of projected curves or empty array if the projection set is empty.</returns>
        public static async Task<Curve[]> ProjectToBrep(Remote<Curve> curve, Remote<Brep> brep, Vector3d direction, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, brep, direction, tolerance);
        }

        /// <summary>
        /// Projects a Curve onto a collection of Breps along a given direction.
        /// </summary>
        /// <param name="curve">Curve to project.</param>
        /// <param name="breps">Breps to project onto.</param>
        /// <param name="direction">Direction of projection.</param>
        /// <param name="tolerance">Tolerance to use for projection.</param>
        /// <returns>An array of projected curves or empty array if the projection set is empty.</returns>
        public static async Task<Curve[]> ProjectToBrep(Curve curve, IEnumerable<Brep> breps, Vector3d direction, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, breps, direction, tolerance);
        }

        /// <summary>
        /// Projects a Curve onto a collection of Breps along a given direction.
        /// </summary>
        /// <param name="curve">Curve to project.</param>
        /// <param name="breps">Breps to project onto.</param>
        /// <param name="direction">Direction of projection.</param>
        /// <param name="tolerance">Tolerance to use for projection.</param>
        /// <returns>An array of projected curves or empty array if the projection set is empty.</returns>
        public static async Task<Curve[]> ProjectToBrep(Remote<Curve> curve, Remote<IEnumerable<Brep>> breps, Vector3d direction, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, breps, direction, tolerance);
        }

        /// <summary>
        /// Projects a collection of Curves onto a collection of Breps along a given direction.
        /// </summary>
        /// <param name="curves">Curves to project.</param>
        /// <param name="breps">Breps to project onto.</param>
        /// <param name="direction">Direction of projection.</param>
        /// <param name="tolerance">Tolerance to use for projection.</param>
        /// <returns>An array of projected curves or empty array if the projection set is empty.</returns>
        public static async Task<Curve[]> ProjectToBrep(IEnumerable<Curve> curves, IEnumerable<Brep> breps, Vector3d direction, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curves, breps, direction, tolerance);
        }

        /// <summary>
        /// Projects a collection of Curves onto a collection of Breps along a given direction.
        /// </summary>
        /// <param name="curves">Curves to project.</param>
        /// <param name="breps">Breps to project onto.</param>
        /// <param name="direction">Direction of projection.</param>
        /// <param name="tolerance">Tolerance to use for projection.</param>
        /// <returns>An array of projected curves or empty array if the projection set is empty.</returns>
        public static async Task<Curve[]> ProjectToBrep(Remote<IEnumerable<Curve>> curves, Remote<IEnumerable<Brep>> breps, Vector3d direction, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curves, breps, direction, tolerance);
        }

        /// <summary>
        /// Constructs a curve by projecting an existing curve to a plane.
        /// </summary>
        /// <param name="curve">A curve.</param>
        /// <param name="plane">A plane.</param>
        /// <returns>The projected curve on success; null on failure.</returns>
        public static async Task<Curve> ProjectToPlane(Curve curve, Plane plane)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, plane);
        }

        /// <summary>
        /// Constructs a curve by projecting an existing curve to a plane.
        /// </summary>
        /// <param name="curve">A curve.</param>
        /// <param name="plane">A plane.</param>
        /// <returns>The projected curve on success; null on failure.</returns>
        public static async Task<Curve> ProjectToPlane(Remote<Curve> curve, Plane plane)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, plane);
        }

        /// <summary>
        /// Pull a curve to a BrepFace using closest point projection.
        /// </summary>
        /// <param name="curve">Curve to pull.</param>
        /// <param name="face">Brep face that pulls.</param>
        /// <param name="tolerance">Tolerance to use for pulling.</param>
        /// <returns>An array of pulled curves, or an empty array on failure.</returns>
        public static async Task<Curve[]> PullToBrepFace(Curve curve, BrepFace face, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, face, tolerance);
        }

        /// <summary>
        /// Pull a curve to a BrepFace using closest point projection.
        /// </summary>
        /// <param name="curve">Curve to pull.</param>
        /// <param name="face">Brep face that pulls.</param>
        /// <param name="tolerance">Tolerance to use for pulling.</param>
        /// <returns>An array of pulled curves, or an empty array on failure.</returns>
        public static async Task<Curve[]> PullToBrepFace(Remote<Curve> curve, BrepFace face, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, face, tolerance);
        }

        /// <summary>
        /// Pull a curve to a BrepFace using closest point projection.
        /// </summary>
        /// <param name="curve">Curve to pull.</param>
        /// <param name="face">Brep face that pulls.</param>
        /// <param name="tolerance">Tolerance to use for pulling.</param>
        /// <param name="loose">
        /// If true, the curve's edit points are pulled back to the Brep face's underlying surface.
        /// If any edit point misses the surface, the curve will not be created.
        /// </param>
        /// <returns>An array of pulled curves, or an empty array on failure.</returns>
        public static async Task<Curve[]> PullToBrepFace(Curve curve, BrepFace face, double tolerance, bool loose)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, face, tolerance, loose);
        }

        /// <summary>
        /// Pull a curve to a BrepFace using closest point projection.
        /// </summary>
        /// <param name="curve">Curve to pull.</param>
        /// <param name="face">Brep face that pulls.</param>
        /// <param name="tolerance">Tolerance to use for pulling.</param>
        /// <param name="loose">
        /// If true, the curve's edit points are pulled back to the Brep face's underlying surface.
        /// If any edit point misses the surface, the curve will not be created.
        /// </param>
        /// <returns>An array of pulled curves, or an empty array on failure.</returns>
        public static async Task<Curve[]> PullToBrepFace(Remote<Curve> curve, BrepFace face, double tolerance, bool loose)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, face, tolerance, loose);
        }

        /// <summary>
        /// Determines whether two coplanar simple closed curves are disjoint or intersect;
        /// otherwise, if the regions have a containment relationship, discovers
        /// which curve encloses the other.
        /// </summary>
        /// <param name="curveA">A first curve.</param>
        /// <param name="curveB">A second curve.</param>
        /// <param name="testPlane">A plane.</param>
        /// <param name="tolerance">A tolerance value.</param>
        /// <returns>
        /// A value indicating the relationship between the first and the second curve.
        /// </returns>
        public static async Task<RegionContainment> PlanarClosedCurveRelationship(Curve curveA, Curve curveB, Plane testPlane, double tolerance)
        {
            return await ComputeServer.PostAsync<RegionContainment>(ApiAddress(), curveA, curveB, testPlane, tolerance);
        }

        /// <summary>
        /// Determines whether two coplanar simple closed curves are disjoint or intersect;
        /// otherwise, if the regions have a containment relationship, discovers
        /// which curve encloses the other.
        /// </summary>
        /// <param name="curveA">A first curve.</param>
        /// <param name="curveB">A second curve.</param>
        /// <param name="testPlane">A plane.</param>
        /// <param name="tolerance">A tolerance value.</param>
        /// <returns>
        /// A value indicating the relationship between the first and the second curve.
        /// </returns>
        public static async Task<RegionContainment> PlanarClosedCurveRelationship(Remote<Curve> curveA, Remote<Curve> curveB, Plane testPlane, double tolerance)
        {
            return await ComputeServer.PostAsync<RegionContainment>(ApiAddress(), curveA, curveB, testPlane, tolerance);
        }

        /// <summary>
        /// Determines if two coplanar curves collide (intersect).
        /// </summary>
        /// <param name="curveA">A curve.</param>
        /// <param name="curveB">Another curve.</param>
        /// <param name="testPlane">A valid plane containing the curves.</param>
        /// <param name="tolerance">A tolerance value for intersection.</param>
        /// <returns>true if the curves intersect, otherwise false</returns>
        public static async Task<bool> PlanarCurveCollision(Curve curveA, Curve curveB, Plane testPlane, double tolerance)
        {
            return await ComputeServer.PostAsync<bool>(ApiAddress(), curveA, curveB, testPlane, tolerance);
        }

        /// <summary>
        /// Determines if two coplanar curves collide (intersect).
        /// </summary>
        /// <param name="curveA">A curve.</param>
        /// <param name="curveB">Another curve.</param>
        /// <param name="testPlane">A valid plane containing the curves.</param>
        /// <param name="tolerance">A tolerance value for intersection.</param>
        /// <returns>true if the curves intersect, otherwise false</returns>
        public static async Task<bool> PlanarCurveCollision(Remote<Curve> curveA, Remote<Curve> curveB, Plane testPlane, double tolerance)
        {
            return await ComputeServer.PostAsync<bool>(ApiAddress(), curveA, curveB, testPlane, tolerance);
        }

        /// <summary>
        /// Returns a curve's inflection points. An inflection point is a location on
        /// a curve at which the sign of the curvature (i.e., the concavity) changes. 
        /// The curvature at these locations is always 0.
        /// </summary>
        /// <returns>An array of points if successful, null if not successful or on error.</returns>
        public static async Task<Point3d[]> InflectionPoints(this Curve curve)
        {
            return await ComputeServer.PostAsync<Point3d[]>(ApiAddress(), curve);
        }

        /// <summary>
        /// Returns a curve's inflection points. An inflection point is a location on
        /// a curve at which the sign of the curvature (i.e., the concavity) changes. 
        /// The curvature at these locations is always 0.
        /// </summary>
        /// <returns>An array of points if successful, null if not successful or on error.</returns>
        public static async Task<Point3d[]> InflectionPoints(Remote<Curve> curve)
        {
            return await ComputeServer.PostAsync<Point3d[]>(ApiAddress(), curve);
        }

        /// <summary>
        /// Returns a curve's maximum curvature points. The maximum curvature points identify
        /// where the curvature starts to decrease in both directions from the points.
        /// </summary>
        /// <returns>An array of points if successful, null if not successful or on error.</returns>
        public static async Task<Point3d[]> MaxCurvaturePoints(this Curve curve)
        {
            return await ComputeServer.PostAsync<Point3d[]>(ApiAddress(), curve);
        }

        /// <summary>
        /// Returns a curve's maximum curvature points. The maximum curvature points identify
        /// where the curvature starts to decrease in both directions from the points.
        /// </summary>
        /// <returns>An array of points if successful, null if not successful or on error.</returns>
        public static async Task<Point3d[]> MaxCurvaturePoints(Remote<Curve> curve)
        {
            return await ComputeServer.PostAsync<Point3d[]>(ApiAddress(), curve);
        }

        /// <summary>
        /// Computes the relationship between a point and a closed curve region. 
        /// This curve must be closed or the return value will be Unset.
        /// Both curve and point are projected to the World XY plane.
        /// </summary>
        /// <param name="testPoint">Point to test.</param>
        /// <returns>Relationship between point and curve region.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<PointContainment> Contains(this Curve curve, Point3d testPoint)
        {
            return await ComputeServer.PostAsync<PointContainment>(ApiAddress(), curve, testPoint);
        }

        /// <summary>
        /// Computes the relationship between a point and a closed curve region. 
        /// This curve must be closed or the return value will be Unset.
        /// Both curve and point are projected to the World XY plane.
        /// </summary>
        /// <param name="testPoint">Point to test.</param>
        /// <returns>Relationship between point and curve region.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<PointContainment> Contains(Remote<Curve> curve, Point3d testPoint)
        {
            return await ComputeServer.PostAsync<PointContainment>(ApiAddress(), curve, testPoint);
        }

        /// <summary>
        /// Computes the relationship between a point and a closed curve region. 
        /// This curve must be closed or the return value will be Unset.
        /// </summary>
        /// <param name="testPoint">Point to test.</param>
        /// <param name="plane">Plane in which to compare point and region.</param>
        /// <returns>Relationship between point and curve region.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<PointContainment> Contains(this Curve curve, Point3d testPoint, Plane plane)
        {
            return await ComputeServer.PostAsync<PointContainment>(ApiAddress(), curve, testPoint, plane);
        }

        /// <summary>
        /// Computes the relationship between a point and a closed curve region. 
        /// This curve must be closed or the return value will be Unset.
        /// </summary>
        /// <param name="testPoint">Point to test.</param>
        /// <param name="plane">Plane in which to compare point and region.</param>
        /// <returns>Relationship between point and curve region.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<PointContainment> Contains(Remote<Curve> curve, Point3d testPoint, Plane plane)
        {
            return await ComputeServer.PostAsync<PointContainment>(ApiAddress(), curve, testPoint, plane);
        }

        /// <summary>
        /// Computes the relationship between a point and a closed curve region. 
        /// This curve must be closed or the return value will be Unset.
        /// </summary>
        /// <param name="testPoint">Point to test.</param>
        /// <param name="plane">Plane in which to compare point and region.</param>
        /// <param name="tolerance">Tolerance to use during comparison.</param>
        /// <returns>Relationship between point and curve region.</returns>
        public static async Task<PointContainment> Contains(this Curve curve, Point3d testPoint, Plane plane, double tolerance)
        {
            return await ComputeServer.PostAsync<PointContainment>(ApiAddress(), curve, testPoint, plane, tolerance);
        }

        /// <summary>
        /// Computes the relationship between a point and a closed curve region. 
        /// This curve must be closed or the return value will be Unset.
        /// </summary>
        /// <param name="testPoint">Point to test.</param>
        /// <param name="plane">Plane in which to compare point and region.</param>
        /// <param name="tolerance">Tolerance to use during comparison.</param>
        /// <returns>Relationship between point and curve region.</returns>
        public static async Task<PointContainment> Contains(Remote<Curve> curve, Point3d testPoint, Plane plane, double tolerance)
        {
            return await ComputeServer.PostAsync<PointContainment>(ApiAddress(), curve, testPoint, plane, tolerance);
        }

        /// <summary>
        /// Returns the parameter values of all local extrema. 
        /// Parameter values are in increasing order so consecutive extrema 
        /// define an interval on which each component of the curve is monotone. 
        /// Note, non-periodic curves always return the end points.
        /// </summary>
        /// <param name="direction">The direction in which to perform the calculation.</param>
        /// <returns>The parameter values of all local extrema.</returns>
        public static async Task<double[]> ExtremeParameters(this Curve curve, Vector3d direction)
        {
            return await ComputeServer.PostAsync<double[]>(ApiAddress(), curve, direction);
        }

        /// <summary>
        /// Returns the parameter values of all local extrema. 
        /// Parameter values are in increasing order so consecutive extrema 
        /// define an interval on which each component of the curve is monotone. 
        /// Note, non-periodic curves always return the end points.
        /// </summary>
        /// <param name="direction">The direction in which to perform the calculation.</param>
        /// <returns>The parameter values of all local extrema.</returns>
        public static async Task<double[]> ExtremeParameters(Remote<Curve> curve, Vector3d direction)
        {
            return await ComputeServer.PostAsync<double[]>(ApiAddress(), curve, direction);
        }

        /// <summary>
        /// Removes kinks from a curve. Periodic curves deform smoothly without kinks.
        /// </summary>
        /// <param name="curve">The curve to make periodic. Curve must have degree >= 2.</param>
        /// <returns>The resulting curve if successful, null otherwise.</returns>
        public static async Task<Curve> CreatePeriodicCurve(Curve curve)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve);
        }

        /// <summary>
        /// Removes kinks from a curve. Periodic curves deform smoothly without kinks.
        /// </summary>
        /// <param name="curve">The curve to make periodic. Curve must have degree >= 2.</param>
        /// <returns>The resulting curve if successful, null otherwise.</returns>
        public static async Task<Curve> CreatePeriodicCurve(Remote<Curve> curve)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve);
        }

        /// <summary>
        /// Removes kinks from a curve. Periodic curves deform smoothly without kinks.
        /// </summary>
        /// <param name="curve">The curve to make periodic. Curve must have degree >= 2.</param>
        /// <param name="smooth">
        /// If true, smooths any kinks in the curve and moves control points to make a smooth curve. 
        /// If false, control point locations are not changed or changed minimally (only one point may move) and only the knot vector is altered.
        /// </param>
        /// <returns>The resulting curve if successful, null otherwise.</returns>
        public static async Task<Curve> CreatePeriodicCurve(Curve curve, bool smooth)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, smooth);
        }

        /// <summary>
        /// Removes kinks from a curve. Periodic curves deform smoothly without kinks.
        /// </summary>
        /// <param name="curve">The curve to make periodic. Curve must have degree >= 2.</param>
        /// <param name="smooth">
        /// If true, smooths any kinks in the curve and moves control points to make a smooth curve. 
        /// If false, control point locations are not changed or changed minimally (only one point may move) and only the knot vector is altered.
        /// </param>
        /// <returns>The resulting curve if successful, null otherwise.</returns>
        public static async Task<Curve> CreatePeriodicCurve(Remote<Curve> curve, bool smooth)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, smooth);
        }

        /// <summary>
        /// Gets a point at a certain length along the curve. The length must be 
        /// non-negative and less than or equal to the length of the curve. 
        /// Lengths will not be wrapped when the curve is closed or periodic.
        /// </summary>
        /// <param name="length">Length along the curve between the start point and the returned point.</param>
        /// <returns>Point on the curve at the specified length from the start point or Poin3d.Unset on failure.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_arclengthpoint.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_arclengthpoint.cs' lang='cs'/>
        /// <code source='examples\py\ex_arclengthpoint.py' lang='py'/>
        /// </example>
        public static async Task<Point3d> PointAtLength(this Curve curve, double length)
        {
            return await ComputeServer.PostAsync<Point3d>(ApiAddress(), curve, length);
        }

        /// <summary>
        /// Gets a point at a certain length along the curve. The length must be 
        /// non-negative and less than or equal to the length of the curve. 
        /// Lengths will not be wrapped when the curve is closed or periodic.
        /// </summary>
        /// <param name="length">Length along the curve between the start point and the returned point.</param>
        /// <returns>Point on the curve at the specified length from the start point or Poin3d.Unset on failure.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_arclengthpoint.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_arclengthpoint.cs' lang='cs'/>
        /// <code source='examples\py\ex_arclengthpoint.py' lang='py'/>
        /// </example>
        public static async Task<Point3d> PointAtLength(Remote<Curve> curve, double length)
        {
            return await ComputeServer.PostAsync<Point3d>(ApiAddress(), curve, length);
        }

        /// <summary>
        /// Gets a point at a certain normalized length along the curve. The length must be 
        /// between or including 0.0 and 1.0, where 0.0 equals the start of the curve and 
        /// 1.0 equals the end of the curve. 
        /// </summary>
        /// <param name="length">Normalized length along the curve between the start point and the returned point.</param>
        /// <returns>Point on the curve at the specified normalized length from the start point or Poin3d.Unset on failure.</returns>
        public static async Task<Point3d> PointAtNormalizedLength(this Curve curve, double length)
        {
            return await ComputeServer.PostAsync<Point3d>(ApiAddress(), curve, length);
        }

        /// <summary>
        /// Gets a point at a certain normalized length along the curve. The length must be 
        /// between or including 0.0 and 1.0, where 0.0 equals the start of the curve and 
        /// 1.0 equals the end of the curve. 
        /// </summary>
        /// <param name="length">Normalized length along the curve between the start point and the returned point.</param>
        /// <returns>Point on the curve at the specified normalized length from the start point or Poin3d.Unset on failure.</returns>
        public static async Task<Point3d> PointAtNormalizedLength(Remote<Curve> curve, double length)
        {
            return await ComputeServer.PostAsync<Point3d>(ApiAddress(), curve, length);
        }

        /// <summary>
        /// Gets a collection of perpendicular frames along the curve. Perpendicular frames 
        /// are also known as 'Zero-twisting frames' and they minimize rotation from one frame to the next.
        /// </summary>
        /// <param name="parameters">A collection of <i>strictly increasing</i> curve parameters to place perpendicular frames on.</param>
        /// <returns>An array of perpendicular frames on success or null on failure.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the curve parameters are not increasing.</exception>
        public static async Task<Plane[]> GetPerpendicularFrames(this Curve curve, IEnumerable<double> parameters)
        {
            return await ComputeServer.PostAsync<Plane[]>(ApiAddress(), curve, parameters);
        }

        /// <summary>
        /// Gets a collection of perpendicular frames along the curve. Perpendicular frames 
        /// are also known as 'Zero-twisting frames' and they minimize rotation from one frame to the next.
        /// </summary>
        /// <param name="parameters">A collection of <i>strictly increasing</i> curve parameters to place perpendicular frames on.</param>
        /// <returns>An array of perpendicular frames on success or null on failure.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the curve parameters are not increasing.</exception>
        public static async Task<Plane[]> GetPerpendicularFrames(Remote<Curve> curve, IEnumerable<double> parameters)
        {
            return await ComputeServer.PostAsync<Plane[]>(ApiAddress(), curve, parameters);
        }

        /// <summary>
        /// Gets the length of the curve with a fractional tolerance of 1.0e-8.
        /// </summary>
        /// <returns>The length of the curve on success, or zero on failure.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_arclengthpoint.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_arclengthpoint.cs' lang='cs'/>
        /// <code source='examples\py\ex_arclengthpoint.py' lang='py'/>
        /// </example>
        public static async Task<double> GetLength(this Curve curve)
        {
            return await ComputeServer.PostAsync<double>(ApiAddress(), curve);
        }

        /// <summary>
        /// Gets the length of the curve with a fractional tolerance of 1.0e-8.
        /// </summary>
        /// <returns>The length of the curve on success, or zero on failure.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_arclengthpoint.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_arclengthpoint.cs' lang='cs'/>
        /// <code source='examples\py\ex_arclengthpoint.py' lang='py'/>
        /// </example>
        public static async Task<double> GetLength(Remote<Curve> curve)
        {
            return await ComputeServer.PostAsync<double>(ApiAddress(), curve);
        }

        /// <summary>Get the length of the curve.</summary>
        /// <param name="fractionalTolerance">
        /// Desired fractional precision. 
        /// fabs(("exact" length from start to t) - arc_length)/arc_length &lt;= fractionalTolerance.
        /// </param>
        /// <returns>The length of the curve on success, or zero on failure.</returns>
        public static async Task<double> GetLength(this Curve curve, double fractionalTolerance)
        {
            return await ComputeServer.PostAsync<double>(ApiAddress(), curve, fractionalTolerance);
        }

        /// <summary>Get the length of the curve.</summary>
        /// <param name="fractionalTolerance">
        /// Desired fractional precision. 
        /// fabs(("exact" length from start to t) - arc_length)/arc_length &lt;= fractionalTolerance.
        /// </param>
        /// <returns>The length of the curve on success, or zero on failure.</returns>
        public static async Task<double> GetLength(Remote<Curve> curve, double fractionalTolerance)
        {
            return await ComputeServer.PostAsync<double>(ApiAddress(), curve, fractionalTolerance);
        }

        /// <summary>Get the length of a sub-section of the curve with a fractional tolerance of 1e-8.</summary>
        /// <param name="subdomain">
        /// The calculation is performed on the specified sub-domain of the curve (must be non-decreasing).
        /// </param>
        /// <returns>The length of the sub-curve on success, or zero on failure.</returns>
        public static async Task<double> GetLength(this Curve curve, Interval subdomain)
        {
            return await ComputeServer.PostAsync<double>(ApiAddress(), curve, subdomain);
        }

        /// <summary>Get the length of a sub-section of the curve with a fractional tolerance of 1e-8.</summary>
        /// <param name="subdomain">
        /// The calculation is performed on the specified sub-domain of the curve (must be non-decreasing).
        /// </param>
        /// <returns>The length of the sub-curve on success, or zero on failure.</returns>
        public static async Task<double> GetLength(Remote<Curve> curve, Interval subdomain)
        {
            return await ComputeServer.PostAsync<double>(ApiAddress(), curve, subdomain);
        }

        /// <summary>Get the length of a sub-section of the curve.</summary>
        /// <param name="fractionalTolerance">
        /// Desired fractional precision. 
        /// fabs(("exact" length from start to t) - arc_length)/arc_length &lt;= fractionalTolerance.
        /// </param>
        /// <param name="subdomain">
        /// The calculation is performed on the specified sub-domain of the curve (must be non-decreasing).
        /// </param>
        /// <returns>The length of the sub-curve on success, or zero on failure.</returns>
        public static async Task<double> GetLength(this Curve curve, double fractionalTolerance, Interval subdomain)
        {
            return await ComputeServer.PostAsync<double>(ApiAddress(), curve, fractionalTolerance, subdomain);
        }

        /// <summary>Get the length of a sub-section of the curve.</summary>
        /// <param name="fractionalTolerance">
        /// Desired fractional precision. 
        /// fabs(("exact" length from start to t) - arc_length)/arc_length &lt;= fractionalTolerance.
        /// </param>
        /// <param name="subdomain">
        /// The calculation is performed on the specified sub-domain of the curve (must be non-decreasing).
        /// </param>
        /// <returns>The length of the sub-curve on success, or zero on failure.</returns>
        public static async Task<double> GetLength(Remote<Curve> curve, double fractionalTolerance, Interval subdomain)
        {
            return await ComputeServer.PostAsync<double>(ApiAddress(), curve, fractionalTolerance, subdomain);
        }

        /// <summary>Used to quickly find short curves.</summary>
        /// <param name="tolerance">Length threshold value for "shortness".</param>
        /// <returns>true if the length of the curve is &lt;= tolerance.</returns>
        /// <remarks>Faster than calling Length() and testing the result.</remarks>
        /// <example>
        /// <code source='examples\vbnet\ex_dividebylength.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_dividebylength.cs' lang='cs'/>
        /// <code source='examples\py\ex_dividebylength.py' lang='py'/>
        /// </example>
        public static async Task<bool> IsShort(this Curve curve, double tolerance)
        {
            return await ComputeServer.PostAsync<bool>(ApiAddress(), curve, tolerance);
        }

        /// <summary>Used to quickly find short curves.</summary>
        /// <param name="tolerance">Length threshold value for "shortness".</param>
        /// <returns>true if the length of the curve is &lt;= tolerance.</returns>
        /// <remarks>Faster than calling Length() and testing the result.</remarks>
        /// <example>
        /// <code source='examples\vbnet\ex_dividebylength.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_dividebylength.cs' lang='cs'/>
        /// <code source='examples\py\ex_dividebylength.py' lang='py'/>
        /// </example>
        public static async Task<bool> IsShort(Remote<Curve> curve, double tolerance)
        {
            return await ComputeServer.PostAsync<bool>(ApiAddress(), curve, tolerance);
        }

        /// <summary>Used to quickly find short curves.</summary>
        /// <param name="tolerance">Length threshold value for "shortness".</param>
        /// <param name="subdomain">
        /// The test is performed on the interval that is the intersection of sub-domain with Domain()
        /// </param>
        /// <returns>true if the length of the curve is &lt;= tolerance.</returns>
        /// <remarks>Faster than calling Length() and testing the result.</remarks>
        public static async Task<bool> IsShort(this Curve curve, double tolerance, Interval subdomain)
        {
            return await ComputeServer.PostAsync<bool>(ApiAddress(), curve, tolerance, subdomain);
        }

        /// <summary>Used to quickly find short curves.</summary>
        /// <param name="tolerance">Length threshold value for "shortness".</param>
        /// <param name="subdomain">
        /// The test is performed on the interval that is the intersection of sub-domain with Domain()
        /// </param>
        /// <returns>true if the length of the curve is &lt;= tolerance.</returns>
        /// <remarks>Faster than calling Length() and testing the result.</remarks>
        public static async Task<bool> IsShort(Remote<Curve> curve, double tolerance, Interval subdomain)
        {
            return await ComputeServer.PostAsync<bool>(ApiAddress(), curve, tolerance, subdomain);
        }

        /// <summary>
        /// Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve. 
        /// A fractional tolerance of 1e-8 is used in this version of the function.
        /// </summary>
        /// <param name="s">
        /// Array of normalized arc length parameters. 
        /// E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
        /// </param>
        /// <param name="absoluteTolerance">
        /// If absoluteTolerance > 0, then the difference between (s[i+1]-s[i])*curve_length 
        /// and the length of the curve segment from t[i] to t[i+1] will be &lt;= absoluteTolerance.
        /// </param>
        /// <returns>
        /// If successful, array of curve parameters such that the length of the curve from its start to t[i] is s[i]*curve_length. 
        /// Null on failure.
        /// </returns>
        public static async Task<double[]> NormalizedLengthParameters(this Curve curve, double[] s, double absoluteTolerance)
        {
            return await ComputeServer.PostAsync<double[]>(ApiAddress(), curve, s, absoluteTolerance);
        }

        /// <summary>
        /// Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve. 
        /// A fractional tolerance of 1e-8 is used in this version of the function.
        /// </summary>
        /// <param name="s">
        /// Array of normalized arc length parameters. 
        /// E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
        /// </param>
        /// <param name="absoluteTolerance">
        /// If absoluteTolerance > 0, then the difference between (s[i+1]-s[i])*curve_length 
        /// and the length of the curve segment from t[i] to t[i+1] will be &lt;= absoluteTolerance.
        /// </param>
        /// <returns>
        /// If successful, array of curve parameters such that the length of the curve from its start to t[i] is s[i]*curve_length. 
        /// Null on failure.
        /// </returns>
        public static async Task<double[]> NormalizedLengthParameters(Remote<Curve> curve, double[] s, double absoluteTolerance)
        {
            return await ComputeServer.PostAsync<double[]>(ApiAddress(), curve, s, absoluteTolerance);
        }

        /// <summary>
        /// Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve.
        /// </summary>
        /// <param name="s">
        /// Array of normalized arc length parameters. 
        /// E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
        /// </param>
        /// <param name="absoluteTolerance">
        /// If absoluteTolerance > 0, then the difference between (s[i+1]-s[i])*curve_length 
        /// and the length of the curve segment from t[i] to t[i+1] will be &lt;= absoluteTolerance.
        /// </param>
        /// <param name="fractionalTolerance">
        /// Desired fractional precision for each segment. 
        /// fabs("true" length - actual length)/(actual length) &lt;= fractionalTolerance.
        /// </param>
        /// <returns>
        /// If successful, array of curve parameters such that the length of the curve from its start to t[i] is s[i]*curve_length. 
        /// Null on failure.
        /// </returns>
        public static async Task<double[]> NormalizedLengthParameters(this Curve curve, double[] s, double absoluteTolerance, double fractionalTolerance)
        {
            return await ComputeServer.PostAsync<double[]>(ApiAddress(), curve, s, absoluteTolerance, fractionalTolerance);
        }

        /// <summary>
        /// Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve.
        /// </summary>
        /// <param name="s">
        /// Array of normalized arc length parameters. 
        /// E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
        /// </param>
        /// <param name="absoluteTolerance">
        /// If absoluteTolerance > 0, then the difference between (s[i+1]-s[i])*curve_length 
        /// and the length of the curve segment from t[i] to t[i+1] will be &lt;= absoluteTolerance.
        /// </param>
        /// <param name="fractionalTolerance">
        /// Desired fractional precision for each segment. 
        /// fabs("true" length - actual length)/(actual length) &lt;= fractionalTolerance.
        /// </param>
        /// <returns>
        /// If successful, array of curve parameters such that the length of the curve from its start to t[i] is s[i]*curve_length. 
        /// Null on failure.
        /// </returns>
        public static async Task<double[]> NormalizedLengthParameters(Remote<Curve> curve, double[] s, double absoluteTolerance, double fractionalTolerance)
        {
            return await ComputeServer.PostAsync<double[]>(ApiAddress(), curve, s, absoluteTolerance, fractionalTolerance);
        }

        /// <summary>
        /// Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve. 
        /// A fractional tolerance of 1e-8 is used in this version of the function.
        /// </summary>
        /// <param name="s">
        /// Array of normalized arc length parameters. 
        /// E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
        /// </param>
        /// <param name="absoluteTolerance">
        /// If absoluteTolerance > 0, then the difference between (s[i+1]-s[i])*curve_length 
        /// and the length of the curve segment from t[i] to t[i+1] will be &lt;= absoluteTolerance.
        /// </param>
        /// <param name="subdomain">
        /// The calculation is performed on the specified sub-domain of the curve. 
        /// A 0.0 s value corresponds to sub-domain->Min() and a 1.0 s value corresponds to sub-domain->Max().
        /// </param>
        /// <returns>
        /// If successful, array of curve parameters such that the length of the curve from its start to t[i] is s[i]*curve_length. 
        /// Null on failure.
        /// </returns>
        public static async Task<double[]> NormalizedLengthParameters(this Curve curve, double[] s, double absoluteTolerance, Interval subdomain)
        {
            return await ComputeServer.PostAsync<double[]>(ApiAddress(), curve, s, absoluteTolerance, subdomain);
        }

        /// <summary>
        /// Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve. 
        /// A fractional tolerance of 1e-8 is used in this version of the function.
        /// </summary>
        /// <param name="s">
        /// Array of normalized arc length parameters. 
        /// E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
        /// </param>
        /// <param name="absoluteTolerance">
        /// If absoluteTolerance > 0, then the difference between (s[i+1]-s[i])*curve_length 
        /// and the length of the curve segment from t[i] to t[i+1] will be &lt;= absoluteTolerance.
        /// </param>
        /// <param name="subdomain">
        /// The calculation is performed on the specified sub-domain of the curve. 
        /// A 0.0 s value corresponds to sub-domain->Min() and a 1.0 s value corresponds to sub-domain->Max().
        /// </param>
        /// <returns>
        /// If successful, array of curve parameters such that the length of the curve from its start to t[i] is s[i]*curve_length. 
        /// Null on failure.
        /// </returns>
        public static async Task<double[]> NormalizedLengthParameters(Remote<Curve> curve, double[] s, double absoluteTolerance, Interval subdomain)
        {
            return await ComputeServer.PostAsync<double[]>(ApiAddress(), curve, s, absoluteTolerance, subdomain);
        }

        /// <summary>
        /// Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve.
        /// </summary>
        /// <param name="s">
        /// Array of normalized arc length parameters. 
        /// E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
        /// </param>
        /// <param name="absoluteTolerance">
        /// If absoluteTolerance > 0, then the difference between (s[i+1]-s[i])*curve_length 
        /// and the length of the curve segment from t[i] to t[i+1] will be &lt;= absoluteTolerance.
        /// </param>
        /// <param name="fractionalTolerance">
        /// Desired fractional precision for each segment. 
        /// fabs("true" length - actual length)/(actual length) &lt;= fractionalTolerance.
        /// </param>
        /// <param name="subdomain">
        /// The calculation is performed on the specified sub-domain of the curve. 
        /// A 0.0 s value corresponds to sub-domain->Min() and a 1.0 s value corresponds to sub-domain->Max().
        /// </param>
        /// <returns>
        /// If successful, array of curve parameters such that the length of the curve from its start to t[i] is s[i]*curve_length. 
        /// Null on failure.
        /// </returns>
        public static async Task<double[]> NormalizedLengthParameters(this Curve curve, double[] s, double absoluteTolerance, double fractionalTolerance, Interval subdomain)
        {
            return await ComputeServer.PostAsync<double[]>(ApiAddress(), curve, s, absoluteTolerance, fractionalTolerance, subdomain);
        }

        /// <summary>
        /// Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve.
        /// </summary>
        /// <param name="s">
        /// Array of normalized arc length parameters. 
        /// E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
        /// </param>
        /// <param name="absoluteTolerance">
        /// If absoluteTolerance > 0, then the difference between (s[i+1]-s[i])*curve_length 
        /// and the length of the curve segment from t[i] to t[i+1] will be &lt;= absoluteTolerance.
        /// </param>
        /// <param name="fractionalTolerance">
        /// Desired fractional precision for each segment. 
        /// fabs("true" length - actual length)/(actual length) &lt;= fractionalTolerance.
        /// </param>
        /// <param name="subdomain">
        /// The calculation is performed on the specified sub-domain of the curve. 
        /// A 0.0 s value corresponds to sub-domain->Min() and a 1.0 s value corresponds to sub-domain->Max().
        /// </param>
        /// <returns>
        /// If successful, array of curve parameters such that the length of the curve from its start to t[i] is s[i]*curve_length. 
        /// Null on failure.
        /// </returns>
        public static async Task<double[]> NormalizedLengthParameters(Remote<Curve> curve, double[] s, double absoluteTolerance, double fractionalTolerance, Interval subdomain)
        {
            return await ComputeServer.PostAsync<double[]>(ApiAddress(), curve, s, absoluteTolerance, fractionalTolerance, subdomain);
        }

        /// <summary>
        /// Divide the curve into a number of equal-length segments.
        /// </summary>
        /// <param name="segmentCount">Segment count. Note that the number of division points may differ from the segment count.</param>
        /// <param name="includeEnds">If true, then the point at the start of the first division segment is returned.</param>
        /// <returns>
        /// List of curve parameters at the division points on success, null on failure.
        /// </returns>
        public static async Task<double[]> DivideByCount(this Curve curve, int segmentCount, bool includeEnds)
        {
            return await ComputeServer.PostAsync<double[]>(ApiAddress(), curve, segmentCount, includeEnds);
        }

        /// <summary>
        /// Divide the curve into a number of equal-length segments.
        /// </summary>
        /// <param name="segmentCount">Segment count. Note that the number of division points may differ from the segment count.</param>
        /// <param name="includeEnds">If true, then the point at the start of the first division segment is returned.</param>
        /// <returns>
        /// List of curve parameters at the division points on success, null on failure.
        /// </returns>
        public static async Task<double[]> DivideByCount(Remote<Curve> curve, int segmentCount, bool includeEnds)
        {
            return await ComputeServer.PostAsync<double[]>(ApiAddress(), curve, segmentCount, includeEnds);
        }

        /// <summary>
        /// Divide the curve into specific length segments.
        /// </summary>
        /// <param name="segmentLength">The length of each and every segment (except potentially the last one).</param>
        /// <param name="includeEnds">If true, then the point at the start of the first division segment is returned.</param>
        /// <returns>Array containing division curve parameters if successful, null on failure.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_dividebylength.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_dividebylength.cs' lang='cs'/>
        /// <code source='examples\py\ex_dividebylength.py' lang='py'/>
        /// </example>
        public static async Task<double[]> DivideByLength(this Curve curve, double segmentLength, bool includeEnds)
        {
            return await ComputeServer.PostAsync<double[]>(ApiAddress(), curve, segmentLength, includeEnds);
        }

        /// <summary>
        /// Divide the curve into specific length segments.
        /// </summary>
        /// <param name="segmentLength">The length of each and every segment (except potentially the last one).</param>
        /// <param name="includeEnds">If true, then the point at the start of the first division segment is returned.</param>
        /// <returns>Array containing division curve parameters if successful, null on failure.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_dividebylength.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_dividebylength.cs' lang='cs'/>
        /// <code source='examples\py\ex_dividebylength.py' lang='py'/>
        /// </example>
        public static async Task<double[]> DivideByLength(Remote<Curve> curve, double segmentLength, bool includeEnds)
        {
            return await ComputeServer.PostAsync<double[]>(ApiAddress(), curve, segmentLength, includeEnds);
        }

        /// <summary>
        /// Divide the curve into specific length segments.
        /// </summary>
        /// <param name="segmentLength">The length of each and every segment (except potentially the last one).</param>
        /// <param name="includeEnds">If true, then the point at the start of the first division segment is returned.</param>
        /// <param name="reverse">If true, then the divisions start from the end of the curve.</param>
        /// <returns>Array containing division curve parameters if successful, null on failure.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_dividebylength.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_dividebylength.cs' lang='cs'/>
        /// <code source='examples\py\ex_dividebylength.py' lang='py'/>
        /// </example>
        public static async Task<double[]> DivideByLength(this Curve curve, double segmentLength, bool includeEnds, bool reverse)
        {
            return await ComputeServer.PostAsync<double[]>(ApiAddress(), curve, segmentLength, includeEnds, reverse);
        }

        /// <summary>
        /// Divide the curve into specific length segments.
        /// </summary>
        /// <param name="segmentLength">The length of each and every segment (except potentially the last one).</param>
        /// <param name="includeEnds">If true, then the point at the start of the first division segment is returned.</param>
        /// <param name="reverse">If true, then the divisions start from the end of the curve.</param>
        /// <returns>Array containing division curve parameters if successful, null on failure.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_dividebylength.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_dividebylength.cs' lang='cs'/>
        /// <code source='examples\py\ex_dividebylength.py' lang='py'/>
        /// </example>
        public static async Task<double[]> DivideByLength(Remote<Curve> curve, double segmentLength, bool includeEnds, bool reverse)
        {
            return await ComputeServer.PostAsync<double[]>(ApiAddress(), curve, segmentLength, includeEnds, reverse);
        }

        /// <summary>
        /// Calculates 3d points on a curve where the linear distance between the points is equal.
        /// </summary>
        /// <param name="distance">The distance between division points.</param>
        /// <returns>An array of equidistant points, or null on error.</returns>
        /// <remarks>
        /// Unlike the other divide methods, which divides a curve based on arc length, 
        /// or the distance along the curve between two points, this function divides a curve
        /// based on the linear distance between points.
        /// </remarks>
        public static async Task<Point3d[]> DivideEquidistant(this Curve curve, double distance)
        {
            return await ComputeServer.PostAsync<Point3d[]>(ApiAddress(), curve, distance);
        }

        /// <summary>
        /// Calculates 3d points on a curve where the linear distance between the points is equal.
        /// </summary>
        /// <param name="distance">The distance between division points.</param>
        /// <returns>An array of equidistant points, or null on error.</returns>
        /// <remarks>
        /// Unlike the other divide methods, which divides a curve based on arc length, 
        /// or the distance along the curve between two points, this function divides a curve
        /// based on the linear distance between points.
        /// </remarks>
        public static async Task<Point3d[]> DivideEquidistant(Remote<Curve> curve, double distance)
        {
            return await ComputeServer.PostAsync<Point3d[]>(ApiAddress(), curve, distance);
        }

        /// <summary>
        /// Divides this curve at fixed steps along a defined contour line.
        /// </summary>
        /// <param name="contourStart">The start of the contouring line.</param>
        /// <param name="contourEnd">The end of the contouring line.</param>
        /// <param name="interval">A distance to measure on the contouring axis.</param>
        /// <returns>An array of points; or null on error.</returns>
        public static async Task<Point3d[]> DivideAsContour(this Curve curve, Point3d contourStart, Point3d contourEnd, double interval)
        {
            return await ComputeServer.PostAsync<Point3d[]>(ApiAddress(), curve, contourStart, contourEnd, interval);
        }

        /// <summary>
        /// Divides this curve at fixed steps along a defined contour line.
        /// </summary>
        /// <param name="contourStart">The start of the contouring line.</param>
        /// <param name="contourEnd">The end of the contouring line.</param>
        /// <param name="interval">A distance to measure on the contouring axis.</param>
        /// <returns>An array of points; or null on error.</returns>
        public static async Task<Point3d[]> DivideAsContour(Remote<Curve> curve, Point3d contourStart, Point3d contourEnd, double interval)
        {
            return await ComputeServer.PostAsync<Point3d[]>(ApiAddress(), curve, contourStart, contourEnd, interval);
        }

        /// <summary>
        /// Shortens a curve by a given length
        /// </summary>
        /// <param name="side"></param>
        /// <param name="length"></param>
        /// <returns>Trimmed curve if successful, null on failure.</returns>
        public static async Task<Curve> Trim(this Curve curve, CurveEnd side, double length)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, side, length);
        }

        /// <summary>
        /// Shortens a curve by a given length
        /// </summary>
        /// <param name="side"></param>
        /// <param name="length"></param>
        /// <returns>Trimmed curve if successful, null on failure.</returns>
        public static async Task<Curve> Trim(Remote<Curve> curve, CurveEnd side, double length)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, side, length);
        }

        /// <summary>
        /// Splits a curve into pieces using a polysurface.
        /// </summary>
        /// <param name="cutter">A cutting surface or polysurface.</param>
        /// <param name="tolerance">A tolerance for computing intersections.</param>
        /// <returns>An array of curves. This array can be empty.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Curve[]> Split(this Curve curve, Brep cutter, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, cutter, tolerance);
        }

        /// <summary>
        /// Splits a curve into pieces using a polysurface.
        /// </summary>
        /// <param name="cutter">A cutting surface or polysurface.</param>
        /// <param name="tolerance">A tolerance for computing intersections.</param>
        /// <returns>An array of curves. This array can be empty.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Curve[]> Split(Remote<Curve> curve, Remote<Brep> cutter, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, cutter, tolerance);
        }

        /// <summary>
        /// Splits a curve into pieces using a polysurface.
        /// </summary>
        /// <param name="cutter">A cutting surface or polysurface.</param>
        /// <param name="tolerance">A tolerance for computing intersections.</param>
        /// <param name="angleToleranceRadians"></param>
        /// <returns>An array of curves. This array can be empty.</returns>
        public static async Task<Curve[]> Split(this Curve curve, Brep cutter, double tolerance, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, cutter, tolerance, angleToleranceRadians);
        }

        /// <summary>
        /// Splits a curve into pieces using a polysurface.
        /// </summary>
        /// <param name="cutter">A cutting surface or polysurface.</param>
        /// <param name="tolerance">A tolerance for computing intersections.</param>
        /// <param name="angleToleranceRadians"></param>
        /// <returns>An array of curves. This array can be empty.</returns>
        public static async Task<Curve[]> Split(Remote<Curve> curve, Remote<Brep> cutter, double tolerance, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, cutter, tolerance, angleToleranceRadians);
        }

        /// <summary>
        /// Splits a curve into pieces using a surface.
        /// </summary>
        /// <param name="cutter">A cutting surface or polysurface.</param>
        /// <param name="tolerance">A tolerance for computing intersections.</param>
        /// <returns>An array of curves. This array can be empty.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Curve[]> Split(this Curve curve, Surface cutter, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, cutter, tolerance);
        }

        /// <summary>
        /// Splits a curve into pieces using a surface.
        /// </summary>
        /// <param name="cutter">A cutting surface or polysurface.</param>
        /// <param name="tolerance">A tolerance for computing intersections.</param>
        /// <returns>An array of curves. This array can be empty.</returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Curve[]> Split(Remote<Curve> curve, Remote<Surface> cutter, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, cutter, tolerance);
        }

        /// <summary>
        /// Splits a curve into pieces using a surface.
        /// </summary>
        /// <param name="cutter">A cutting surface or polysurface.</param>
        /// <param name="tolerance">A tolerance for computing intersections.</param>
        /// <param name="angleToleranceRadians"></param>
        /// <returns>An array of curves. This array can be empty.</returns>
        public static async Task<Curve[]> Split(this Curve curve, Surface cutter, double tolerance, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, cutter, tolerance, angleToleranceRadians);
        }

        /// <summary>
        /// Splits a curve into pieces using a surface.
        /// </summary>
        /// <param name="cutter">A cutting surface or polysurface.</param>
        /// <param name="tolerance">A tolerance for computing intersections.</param>
        /// <param name="angleToleranceRadians"></param>
        /// <returns>An array of curves. This array can be empty.</returns>
        public static async Task<Curve[]> Split(Remote<Curve> curve, Remote<Surface> cutter, double tolerance, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, cutter, tolerance, angleToleranceRadians);
        }

        /// <summary>
        /// Where possible, analytically extends curve to include the given domain. 
        /// This will not work on closed curves. The original curve will be identical to the 
        /// restriction of the resulting curve to the original curve domain.
        /// </summary>
        /// <param name="t0">Start of extension domain, if the start is not inside the 
        /// Domain of this curve, an attempt will be made to extend the curve.</param>
        /// <param name="t1">End of extension domain, if the end is not inside the 
        /// Domain of this curve, an attempt will be made to extend the curve.</param>
        /// <returns>Extended curve on success, null on failure.</returns>
        public static async Task<Curve> Extend(this Curve curve, double t0, double t1)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, t0, t1);
        }

        /// <summary>
        /// Where possible, analytically extends curve to include the given domain. 
        /// This will not work on closed curves. The original curve will be identical to the 
        /// restriction of the resulting curve to the original curve domain.
        /// </summary>
        /// <param name="t0">Start of extension domain, if the start is not inside the 
        /// Domain of this curve, an attempt will be made to extend the curve.</param>
        /// <param name="t1">End of extension domain, if the end is not inside the 
        /// Domain of this curve, an attempt will be made to extend the curve.</param>
        /// <returns>Extended curve on success, null on failure.</returns>
        public static async Task<Curve> Extend(Remote<Curve> curve, double t0, double t1)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, t0, t1);
        }

        /// <summary>
        /// Where possible, analytically extends curve to include the given domain. 
        /// This will not work on closed curves. The original curve will be identical to the 
        /// restriction of the resulting curve to the original curve domain.
        /// </summary>
        /// <param name="domain">Extension domain.</param>
        /// <returns>Extended curve on success, null on failure.</returns>
        public static async Task<Curve> Extend(this Curve curve, Interval domain)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, domain);
        }

        /// <summary>
        /// Where possible, analytically extends curve to include the given domain. 
        /// This will not work on closed curves. The original curve will be identical to the 
        /// restriction of the resulting curve to the original curve domain.
        /// </summary>
        /// <param name="domain">Extension domain.</param>
        /// <returns>Extended curve on success, null on failure.</returns>
        public static async Task<Curve> Extend(Remote<Curve> curve, Interval domain)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, domain);
        }

        /// <summary>
        /// Extends a curve by a specific length.
        /// </summary>
        /// <param name="side">Curve end to extend.</param>
        /// <param name="length">Length to add to the curve end.</param>
        /// <param name="style">Extension style.</param>
        /// <returns>A curve with extended ends or null on failure.</returns>
        public static async Task<Curve> Extend(this Curve curve, CurveEnd side, double length, CurveExtensionStyle style)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, side, length, style);
        }

        /// <summary>
        /// Extends a curve by a specific length.
        /// </summary>
        /// <param name="side">Curve end to extend.</param>
        /// <param name="length">Length to add to the curve end.</param>
        /// <param name="style">Extension style.</param>
        /// <returns>A curve with extended ends or null on failure.</returns>
        public static async Task<Curve> Extend(Remote<Curve> curve, CurveEnd side, double length, CurveExtensionStyle style)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, side, length, style);
        }

        /// <summary>
        /// Extends a curve until it intersects a collection of objects.
        /// </summary>
        /// <param name="side">The end of the curve to extend.</param>
        /// <param name="style">The style or type of extension to use.</param>
        /// <param name="geometry">A collection of objects. Allowable object types are Curve, Surface, Brep.</param>
        /// <returns>New extended curve result on success, null on failure.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_extendcurve.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_extendcurve.cs' lang='cs'/>
        /// <code source='examples\py\ex_extendcurve.py' lang='py'/>
        /// </example>
        public static async Task<Curve> Extend(this Curve curve, CurveEnd side, CurveExtensionStyle style, System.Collections.Generic.IEnumerable<GeometryBase> geometry)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, side, style, geometry);
        }

        /// <summary>
        /// Extends a curve until it intersects a collection of objects.
        /// </summary>
        /// <param name="side">The end of the curve to extend.</param>
        /// <param name="style">The style or type of extension to use.</param>
        /// <param name="geometry">A collection of objects. Allowable object types are Curve, Surface, Brep.</param>
        /// <returns>New extended curve result on success, null on failure.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_extendcurve.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_extendcurve.cs' lang='cs'/>
        /// <code source='examples\py\ex_extendcurve.py' lang='py'/>
        /// </example>
        public static async Task<Curve> Extend(Remote<Curve> curve, CurveEnd side, CurveExtensionStyle style, System.Collections.Generic.IEnumerable<GeometryBase> geometry)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, side, style, geometry);
        }

        /// <summary>
        /// Extends a curve to a point.
        /// </summary>
        /// <param name="side">The end of the curve to extend.</param>
        /// <param name="style">The style or type of extension to use.</param>
        /// <param name="endPoint">A new end point.</param>
        /// <returns>New extended curve result on success, null on failure.</returns>
        public static async Task<Curve> Extend(this Curve curve, CurveEnd side, CurveExtensionStyle style, Point3d endPoint)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, side, style, endPoint);
        }

        /// <summary>
        /// Extends a curve to a point.
        /// </summary>
        /// <param name="side">The end of the curve to extend.</param>
        /// <param name="style">The style or type of extension to use.</param>
        /// <param name="endPoint">A new end point.</param>
        /// <returns>New extended curve result on success, null on failure.</returns>
        public static async Task<Curve> Extend(Remote<Curve> curve, CurveEnd side, CurveExtensionStyle style, Point3d endPoint)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, side, style, endPoint);
        }

        /// <summary>
        /// Extends a curve on a surface.
        /// </summary>
        /// <param name="side">The end of the curve to extend.</param>
        /// <param name="surface">Surface that contains the curve.</param>
        /// <returns>New extended curve result on success, null on failure.</returns>
        public static async Task<Curve> ExtendOnSurface(this Curve curve, CurveEnd side, Surface surface)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, side, surface);
        }

        /// <summary>
        /// Extends a curve on a surface.
        /// </summary>
        /// <param name="side">The end of the curve to extend.</param>
        /// <param name="surface">Surface that contains the curve.</param>
        /// <returns>New extended curve result on success, null on failure.</returns>
        public static async Task<Curve> ExtendOnSurface(Remote<Curve> curve, CurveEnd side, Remote<Surface> surface)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, side, surface);
        }

        /// <summary>
        /// Extends a curve on a surface.
        /// </summary>
        /// <param name="side">The end of the curve to extend.</param>
        /// <param name="face">BrepFace that contains the curve.</param>
        /// <returns>New extended curve result on success, null on failure.</returns>
        public static async Task<Curve> ExtendOnSurface(this Curve curve, CurveEnd side, BrepFace face)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, side, face);
        }

        /// <summary>
        /// Extends a curve on a surface.
        /// </summary>
        /// <param name="side">The end of the curve to extend.</param>
        /// <param name="face">BrepFace that contains the curve.</param>
        /// <returns>New extended curve result on success, null on failure.</returns>
        public static async Task<Curve> ExtendOnSurface(Remote<Curve> curve, CurveEnd side, BrepFace face)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, side, face);
        }

        /// <summary>
        /// Extends a curve by a line until it intersects a collection of objects.
        /// </summary>
        /// <param name="side">The end of the curve to extend.</param>
        /// <param name="geometry">A collection of objects. Allowable object types are Curve, Surface, Brep.</param>
        /// <returns>New extended curve result on success, null on failure.</returns>
        public static async Task<Curve> ExtendByLine(this Curve curve, CurveEnd side, System.Collections.Generic.IEnumerable<GeometryBase> geometry)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, side, geometry);
        }

        /// <summary>
        /// Extends a curve by a line until it intersects a collection of objects.
        /// </summary>
        /// <param name="side">The end of the curve to extend.</param>
        /// <param name="geometry">A collection of objects. Allowable object types are Curve, Surface, Brep.</param>
        /// <returns>New extended curve result on success, null on failure.</returns>
        public static async Task<Curve> ExtendByLine(Remote<Curve> curve, CurveEnd side, System.Collections.Generic.IEnumerable<GeometryBase> geometry)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, side, geometry);
        }

        /// <summary>
        /// Extends a curve by an Arc until it intersects a collection of objects.
        /// </summary>
        /// <param name="side">The end of the curve to extend.</param>
        /// <param name="geometry">A collection of objects. Allowable object types are Curve, Surface, Brep.</param>
        /// <returns>New extended curve result on success, null on failure.</returns>
        public static async Task<Curve> ExtendByArc(this Curve curve, CurveEnd side, System.Collections.Generic.IEnumerable<GeometryBase> geometry)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, side, geometry);
        }

        /// <summary>
        /// Extends a curve by an Arc until it intersects a collection of objects.
        /// </summary>
        /// <param name="side">The end of the curve to extend.</param>
        /// <param name="geometry">A collection of objects. Allowable object types are Curve, Surface, Brep.</param>
        /// <returns>New extended curve result on success, null on failure.</returns>
        public static async Task<Curve> ExtendByArc(Remote<Curve> curve, CurveEnd side, System.Collections.Generic.IEnumerable<GeometryBase> geometry)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, side, geometry);
        }

        /// <summary>
        /// Returns a geometrically equivalent PolyCurve.
        /// <para>The PolyCurve has the following properties</para>
        /// <para>
        ///	1. All the PolyCurve segments are LineCurve, PolylineCurve, ArcCurve, or NurbsCurve.
        /// </para>
        /// <para>
        ///	2. The NURBS Curves segments do not have fully multiple interior knots.
        /// </para>
        /// <para>
        ///	3. Rational NURBS curves do not have constant weights.
        /// </para>
        /// <para>
        ///	4. Any segment for which IsLinear() or IsArc() is true is a Line, 
        ///        Polyline segment, or an Arc.
        /// </para>
        /// <para>
        ///	5. Adjacent co-linear or co-circular segments are combined.
        /// </para>
        /// <para>
        ///	6. Segments that meet with G1-continuity have there ends tuned up so
        ///        that they meet with G1-continuity to within machine precision.
        /// </para>
        /// </summary>
        /// <param name="options">Simplification options.</param>
        /// <param name="distanceTolerance">A distance tolerance for the simplification.</param>
        /// <param name="angleToleranceRadians">An angle tolerance for the simplification.</param>
        /// <returns>New simplified curve on success, null on failure.</returns>
        public static async Task<Curve> Simplify(this Curve curve, CurveSimplifyOptions options, double distanceTolerance, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, options, distanceTolerance, angleToleranceRadians);
        }

        /// <summary>
        /// Returns a geometrically equivalent PolyCurve.
        /// <para>The PolyCurve has the following properties</para>
        /// <para>
        ///	1. All the PolyCurve segments are LineCurve, PolylineCurve, ArcCurve, or NurbsCurve.
        /// </para>
        /// <para>
        ///	2. The NURBS Curves segments do not have fully multiple interior knots.
        /// </para>
        /// <para>
        ///	3. Rational NURBS curves do not have constant weights.
        /// </para>
        /// <para>
        ///	4. Any segment for which IsLinear() or IsArc() is true is a Line, 
        ///        Polyline segment, or an Arc.
        /// </para>
        /// <para>
        ///	5. Adjacent co-linear or co-circular segments are combined.
        /// </para>
        /// <para>
        ///	6. Segments that meet with G1-continuity have there ends tuned up so
        ///        that they meet with G1-continuity to within machine precision.
        /// </para>
        /// </summary>
        /// <param name="options">Simplification options.</param>
        /// <param name="distanceTolerance">A distance tolerance for the simplification.</param>
        /// <param name="angleToleranceRadians">An angle tolerance for the simplification.</param>
        /// <returns>New simplified curve on success, null on failure.</returns>
        public static async Task<Curve> Simplify(Remote<Curve> curve, CurveSimplifyOptions options, double distanceTolerance, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, options, distanceTolerance, angleToleranceRadians);
        }

        /// <summary>
        /// Same as SimplifyCurve, but simplifies only the last two segments at "side" end.
        /// </summary>
        /// <param name="end">If CurveEnd.Start the function simplifies the last two start 
        /// side segments, otherwise if CurveEnd.End the last two end side segments are simplified.
        /// </param>
        /// <param name="options">Simplification options.</param>
        /// <param name="distanceTolerance">A distance tolerance for the simplification.</param>
        /// <param name="angleToleranceRadians">An angle tolerance for the simplification.</param>
        /// <returns>New simplified curve on success, null on failure.</returns>
        public static async Task<Curve> SimplifyEnd(this Curve curve, CurveEnd end, CurveSimplifyOptions options, double distanceTolerance, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, end, options, distanceTolerance, angleToleranceRadians);
        }

        /// <summary>
        /// Same as SimplifyCurve, but simplifies only the last two segments at "side" end.
        /// </summary>
        /// <param name="end">If CurveEnd.Start the function simplifies the last two start 
        /// side segments, otherwise if CurveEnd.End the last two end side segments are simplified.
        /// </param>
        /// <param name="options">Simplification options.</param>
        /// <param name="distanceTolerance">A distance tolerance for the simplification.</param>
        /// <param name="angleToleranceRadians">An angle tolerance for the simplification.</param>
        /// <returns>New simplified curve on success, null on failure.</returns>
        public static async Task<Curve> SimplifyEnd(Remote<Curve> curve, CurveEnd end, CurveSimplifyOptions options, double distanceTolerance, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, end, options, distanceTolerance, angleToleranceRadians);
        }

        /// <summary>
        /// Fairs a curve object. Fair works best on degree 3 (cubic) curves. Attempts to 
        /// remove large curvature variations while limiting the geometry changes to be no 
        /// more than the specified tolerance. 
        /// </summary>
        /// <param name="distanceTolerance">Maximum allowed distance the faired curve is allowed to deviate from the input.</param>
        /// <param name="angleTolerance">(in radians) kinks with angles &lt;= angleTolerance are smoothed out 0.05 is a good default.</param>
        /// <param name="clampStart">The number of (control vertices-1) to preserve at start. 
        /// <para>0 = preserve start point</para>
        /// <para>1 = preserve start point and 1st derivative</para>
        /// <para>2 = preserve start point, 1st and 2nd derivative</para>
        /// </param>
        /// <param name="clampEnd">Same as clampStart.</param>
        /// <param name="iterations">The number of iterations to use in adjusting the curve.</param>
        /// <returns>Returns new faired Curve on success, null on failure.</returns>
        public static async Task<Curve> Fair(this Curve curve, double distanceTolerance, double angleTolerance, int clampStart, int clampEnd, int iterations)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, distanceTolerance, angleTolerance, clampStart, clampEnd, iterations);
        }

        /// <summary>
        /// Fairs a curve object. Fair works best on degree 3 (cubic) curves. Attempts to 
        /// remove large curvature variations while limiting the geometry changes to be no 
        /// more than the specified tolerance. 
        /// </summary>
        /// <param name="distanceTolerance">Maximum allowed distance the faired curve is allowed to deviate from the input.</param>
        /// <param name="angleTolerance">(in radians) kinks with angles &lt;= angleTolerance are smoothed out 0.05 is a good default.</param>
        /// <param name="clampStart">The number of (control vertices-1) to preserve at start. 
        /// <para>0 = preserve start point</para>
        /// <para>1 = preserve start point and 1st derivative</para>
        /// <para>2 = preserve start point, 1st and 2nd derivative</para>
        /// </param>
        /// <param name="clampEnd">Same as clampStart.</param>
        /// <param name="iterations">The number of iterations to use in adjusting the curve.</param>
        /// <returns>Returns new faired Curve on success, null on failure.</returns>
        public static async Task<Curve> Fair(Remote<Curve> curve, double distanceTolerance, double angleTolerance, int clampStart, int clampEnd, int iterations)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, distanceTolerance, angleTolerance, clampStart, clampEnd, iterations);
        }

        /// <summary>
        /// Fits a new curve through an existing curve.
        /// </summary>
        /// <param name="degree">The degree of the returned Curve. Must be bigger than 1.</param>
        /// <param name="fitTolerance">The fitting tolerance. If fitTolerance is RhinoMath.UnsetValue or &lt;=0.0,
        /// the document absolute tolerance is used.</param>
        /// <param name="angleTolerance">The kink smoothing tolerance in radians.
        /// <para>If angleTolerance is 0.0, all kinks are smoothed</para>
        /// <para>If angleTolerance is &gt;0.0, kinks smaller than angleTolerance are smoothed</para>  
        /// <para>If angleTolerance is RhinoMath.UnsetValue or &lt;0.0, the document angle tolerance is used for the kink smoothing</para>
        /// </param>
        /// <returns>Returns a new fitted Curve if successful, null on failure.</returns>
        public static async Task<Curve> Fit(this Curve curve, int degree, double fitTolerance, double angleTolerance)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, degree, fitTolerance, angleTolerance);
        }

        /// <summary>
        /// Fits a new curve through an existing curve.
        /// </summary>
        /// <param name="degree">The degree of the returned Curve. Must be bigger than 1.</param>
        /// <param name="fitTolerance">The fitting tolerance. If fitTolerance is RhinoMath.UnsetValue or &lt;=0.0,
        /// the document absolute tolerance is used.</param>
        /// <param name="angleTolerance">The kink smoothing tolerance in radians.
        /// <para>If angleTolerance is 0.0, all kinks are smoothed</para>
        /// <para>If angleTolerance is &gt;0.0, kinks smaller than angleTolerance are smoothed</para>  
        /// <para>If angleTolerance is RhinoMath.UnsetValue or &lt;0.0, the document angle tolerance is used for the kink smoothing</para>
        /// </param>
        /// <returns>Returns a new fitted Curve if successful, null on failure.</returns>
        public static async Task<Curve> Fit(Remote<Curve> curve, int degree, double fitTolerance, double angleTolerance)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, degree, fitTolerance, angleTolerance);
        }

        /// <summary>
        /// Rebuild a curve with a specific point count.
        /// </summary>
        /// <param name="pointCount">Number of control points in the rebuild curve.</param>
        /// <param name="degree">Degree of curve. Valid values are between and including 1 and 11.</param>
        /// <param name="preserveTangents">If true, the end tangents of the input curve will be preserved.</param>
        /// <returns>A NURBS curve on success or null on failure.</returns>
        public static async Task<NurbsCurve> Rebuild(this Curve curve, int pointCount, int degree, bool preserveTangents)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), curve, pointCount, degree, preserveTangents);
        }

        /// <summary>
        /// Rebuild a curve with a specific point count.
        /// </summary>
        /// <param name="pointCount">Number of control points in the rebuild curve.</param>
        /// <param name="degree">Degree of curve. Valid values are between and including 1 and 11.</param>
        /// <param name="preserveTangents">If true, the end tangents of the input curve will be preserved.</param>
        /// <returns>A NURBS curve on success or null on failure.</returns>
        public static async Task<NurbsCurve> Rebuild(Remote<Curve> curve, int pointCount, int degree, bool preserveTangents)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), curve, pointCount, degree, preserveTangents);
        }

        /// <summary>
        /// Gets a polyline approximation of a curve.
        /// </summary>
        /// <param name="mainSegmentCount">
        /// If mainSegmentCount &lt;= 0, then both subSegmentCount and mainSegmentCount are ignored. 
        /// If mainSegmentCount &gt; 0, then subSegmentCount must be &gt;= 1. In this 
        /// case the NURBS will be broken into mainSegmentCount equally spaced 
        /// chords. If needed, each of these chords can be split into as many 
        /// subSegmentCount sub-parts if the subdivision is necessary for the 
        /// mesh to meet the other meshing constraints. In particular, if 
        /// subSegmentCount = 0, then the curve is broken into mainSegmentCount 
        /// pieces and no further testing is performed.</param>
        /// <param name="subSegmentCount">An amount of subsegments.</param>
        /// <param name="maxAngleRadians">
        /// ( 0 to pi ) Maximum angle (in radians) between unit tangents at 
        /// adjacent vertices.</param>
        /// <param name="maxChordLengthRatio">Maximum permitted value of 
        /// (distance chord midpoint to curve) / (length of chord).</param>
        /// <param name="maxAspectRatio">If maxAspectRatio &lt; 1.0, the parameter is ignored. 
        /// If 1 &lt;= maxAspectRatio &lt; sqrt(2), it is treated as if maxAspectRatio = sqrt(2). 
        /// This parameter controls the maximum permitted value of 
        /// (length of longest chord) / (length of shortest chord).</param>
        /// <param name="tolerance">If tolerance = 0, the parameter is ignored. 
        /// This parameter controls the maximum permitted value of the 
        /// distance from the curve to the polyline.</param>
        /// <param name="minEdgeLength">The minimum permitted edge length.</param>
        /// <param name="maxEdgeLength">If maxEdgeLength = 0, the parameter 
        /// is ignored. This parameter controls the maximum permitted edge length.
        /// </param>
        /// <param name="keepStartPoint">If true the starting point of the curve 
        /// is added to the polyline. If false the starting point of the curve is 
        /// not added to the polyline.</param>
        /// <returns>PolylineCurve on success, null on error.</returns>
        public static async Task<PolylineCurve> ToPolyline(this Curve curve, int mainSegmentCount, int subSegmentCount, double maxAngleRadians, double maxChordLengthRatio, double maxAspectRatio, double tolerance, double minEdgeLength, double maxEdgeLength, bool keepStartPoint)
        {
            return await ComputeServer.PostAsync<PolylineCurve>(ApiAddress(), curve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint);
        }

        /// <summary>
        /// Gets a polyline approximation of a curve.
        /// </summary>
        /// <param name="mainSegmentCount">
        /// If mainSegmentCount &lt;= 0, then both subSegmentCount and mainSegmentCount are ignored. 
        /// If mainSegmentCount &gt; 0, then subSegmentCount must be &gt;= 1. In this 
        /// case the NURBS will be broken into mainSegmentCount equally spaced 
        /// chords. If needed, each of these chords can be split into as many 
        /// subSegmentCount sub-parts if the subdivision is necessary for the 
        /// mesh to meet the other meshing constraints. In particular, if 
        /// subSegmentCount = 0, then the curve is broken into mainSegmentCount 
        /// pieces and no further testing is performed.</param>
        /// <param name="subSegmentCount">An amount of subsegments.</param>
        /// <param name="maxAngleRadians">
        /// ( 0 to pi ) Maximum angle (in radians) between unit tangents at 
        /// adjacent vertices.</param>
        /// <param name="maxChordLengthRatio">Maximum permitted value of 
        /// (distance chord midpoint to curve) / (length of chord).</param>
        /// <param name="maxAspectRatio">If maxAspectRatio &lt; 1.0, the parameter is ignored. 
        /// If 1 &lt;= maxAspectRatio &lt; sqrt(2), it is treated as if maxAspectRatio = sqrt(2). 
        /// This parameter controls the maximum permitted value of 
        /// (length of longest chord) / (length of shortest chord).</param>
        /// <param name="tolerance">If tolerance = 0, the parameter is ignored. 
        /// This parameter controls the maximum permitted value of the 
        /// distance from the curve to the polyline.</param>
        /// <param name="minEdgeLength">The minimum permitted edge length.</param>
        /// <param name="maxEdgeLength">If maxEdgeLength = 0, the parameter 
        /// is ignored. This parameter controls the maximum permitted edge length.
        /// </param>
        /// <param name="keepStartPoint">If true the starting point of the curve 
        /// is added to the polyline. If false the starting point of the curve is 
        /// not added to the polyline.</param>
        /// <returns>PolylineCurve on success, null on error.</returns>
        public static async Task<PolylineCurve> ToPolyline(Remote<Curve> curve, int mainSegmentCount, int subSegmentCount, double maxAngleRadians, double maxChordLengthRatio, double maxAspectRatio, double tolerance, double minEdgeLength, double maxEdgeLength, bool keepStartPoint)
        {
            return await ComputeServer.PostAsync<PolylineCurve>(ApiAddress(), curve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint);
        }

        /// <summary>
        /// Gets a polyline approximation of a curve.
        /// </summary>
        /// <param name="mainSegmentCount">
        /// If mainSegmentCount &lt;= 0, then both subSegmentCount and mainSegmentCount are ignored. 
        /// If mainSegmentCount &gt; 0, then subSegmentCount must be &gt;= 1. In this 
        /// case the NURBS will be broken into mainSegmentCount equally spaced 
        /// chords. If needed, each of these chords can be split into as many 
        /// subSegmentCount sub-parts if the subdivision is necessary for the 
        /// mesh to meet the other meshing constraints. In particular, if 
        /// subSegmentCount = 0, then the curve is broken into mainSegmentCount 
        /// pieces and no further testing is performed.</param>
        /// <param name="subSegmentCount">An amount of subsegments.</param>
        /// <param name="maxAngleRadians">
        /// ( 0 to pi ) Maximum angle (in radians) between unit tangents at 
        /// adjacent vertices.</param>
        /// <param name="maxChordLengthRatio">Maximum permitted value of 
        /// (distance chord midpoint to curve) / (length of chord).</param>
        /// <param name="maxAspectRatio">If maxAspectRatio &lt; 1.0, the parameter is ignored. 
        /// If 1 &lt;= maxAspectRatio &lt; sqrt(2), it is treated as if maxAspectRatio = sqrt(2). 
        /// This parameter controls the maximum permitted value of 
        /// (length of longest chord) / (length of shortest chord).</param>
        /// <param name="tolerance">If tolerance = 0, the parameter is ignored. 
        /// This parameter controls the maximum permitted value of the 
        /// distance from the curve to the polyline.</param>
        /// <param name="minEdgeLength">The minimum permitted edge length.</param>
        /// <param name="maxEdgeLength">If maxEdgeLength = 0, the parameter 
        /// is ignored. This parameter controls the maximum permitted edge length.
        /// </param>
        /// <param name="keepStartPoint">If true the starting point of the curve 
        /// is added to the polyline. If false the starting point of the curve is 
        /// not added to the polyline.</param>
        /// <param name="curveDomain">This sub-domain of the NURBS curve is approximated.</param>
        /// <returns>PolylineCurve on success, null on error.</returns>
        public static async Task<PolylineCurve> ToPolyline(this Curve curve, int mainSegmentCount, int subSegmentCount, double maxAngleRadians, double maxChordLengthRatio, double maxAspectRatio, double tolerance, double minEdgeLength, double maxEdgeLength, bool keepStartPoint, Interval curveDomain)
        {
            return await ComputeServer.PostAsync<PolylineCurve>(ApiAddress(), curve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint, curveDomain);
        }

        /// <summary>
        /// Gets a polyline approximation of a curve.
        /// </summary>
        /// <param name="mainSegmentCount">
        /// If mainSegmentCount &lt;= 0, then both subSegmentCount and mainSegmentCount are ignored. 
        /// If mainSegmentCount &gt; 0, then subSegmentCount must be &gt;= 1. In this 
        /// case the NURBS will be broken into mainSegmentCount equally spaced 
        /// chords. If needed, each of these chords can be split into as many 
        /// subSegmentCount sub-parts if the subdivision is necessary for the 
        /// mesh to meet the other meshing constraints. In particular, if 
        /// subSegmentCount = 0, then the curve is broken into mainSegmentCount 
        /// pieces and no further testing is performed.</param>
        /// <param name="subSegmentCount">An amount of subsegments.</param>
        /// <param name="maxAngleRadians">
        /// ( 0 to pi ) Maximum angle (in radians) between unit tangents at 
        /// adjacent vertices.</param>
        /// <param name="maxChordLengthRatio">Maximum permitted value of 
        /// (distance chord midpoint to curve) / (length of chord).</param>
        /// <param name="maxAspectRatio">If maxAspectRatio &lt; 1.0, the parameter is ignored. 
        /// If 1 &lt;= maxAspectRatio &lt; sqrt(2), it is treated as if maxAspectRatio = sqrt(2). 
        /// This parameter controls the maximum permitted value of 
        /// (length of longest chord) / (length of shortest chord).</param>
        /// <param name="tolerance">If tolerance = 0, the parameter is ignored. 
        /// This parameter controls the maximum permitted value of the 
        /// distance from the curve to the polyline.</param>
        /// <param name="minEdgeLength">The minimum permitted edge length.</param>
        /// <param name="maxEdgeLength">If maxEdgeLength = 0, the parameter 
        /// is ignored. This parameter controls the maximum permitted edge length.
        /// </param>
        /// <param name="keepStartPoint">If true the starting point of the curve 
        /// is added to the polyline. If false the starting point of the curve is 
        /// not added to the polyline.</param>
        /// <param name="curveDomain">This sub-domain of the NURBS curve is approximated.</param>
        /// <returns>PolylineCurve on success, null on error.</returns>
        public static async Task<PolylineCurve> ToPolyline(Remote<Curve> curve, int mainSegmentCount, int subSegmentCount, double maxAngleRadians, double maxChordLengthRatio, double maxAspectRatio, double tolerance, double minEdgeLength, double maxEdgeLength, bool keepStartPoint, Interval curveDomain)
        {
            return await ComputeServer.PostAsync<PolylineCurve>(ApiAddress(), curve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint, curveDomain);
        }

        /// <summary>
        /// Gets a polyline approximation of a curve.
        /// </summary>
        /// <param name="tolerance">The tolerance. This is the maximum deviation from line midpoints to the curve. When in doubt, use the document's model space absolute tolerance.</param>
        /// <param name="angleTolerance">The angle tolerance in radians. This is the maximum deviation of the line directions. When in doubt, use the document's model space angle tolerance.</param>
        /// <param name="minimumLength">The minimum segment length.</param>
        /// <param name="maximumLength">The maximum segment length.</param>
        /// <returns>PolyCurve on success, null on error.</returns>
        public static async Task<PolylineCurve> ToPolyline(this Curve curve, double tolerance, double angleTolerance, double minimumLength, double maximumLength)
        {
            return await ComputeServer.PostAsync<PolylineCurve>(ApiAddress(), curve, tolerance, angleTolerance, minimumLength, maximumLength);
        }

        /// <summary>
        /// Gets a polyline approximation of a curve.
        /// </summary>
        /// <param name="tolerance">The tolerance. This is the maximum deviation from line midpoints to the curve. When in doubt, use the document's model space absolute tolerance.</param>
        /// <param name="angleTolerance">The angle tolerance in radians. This is the maximum deviation of the line directions. When in doubt, use the document's model space angle tolerance.</param>
        /// <param name="minimumLength">The minimum segment length.</param>
        /// <param name="maximumLength">The maximum segment length.</param>
        /// <returns>PolyCurve on success, null on error.</returns>
        public static async Task<PolylineCurve> ToPolyline(Remote<Curve> curve, double tolerance, double angleTolerance, double minimumLength, double maximumLength)
        {
            return await ComputeServer.PostAsync<PolylineCurve>(ApiAddress(), curve, tolerance, angleTolerance, minimumLength, maximumLength);
        }

        /// <summary>
        /// Converts a curve into polycurve consisting of arc segments. Sections of the input curves that are nearly straight are converted to straight-line segments.
        /// </summary>
        /// <param name="tolerance">The tolerance. This is the maximum deviation from arc midpoints to the curve. When in doubt, use the document's model space absolute tolerance.</param>
        /// <param name="angleTolerance">The angle tolerance in radians. This is the maximum deviation of the arc end directions from the curve direction. When in doubt, use the document's model space angle tolerance.</param>
        /// <param name="minimumLength">The minimum segment length.</param>
        /// <param name="maximumLength">The maximum segment length.</param>
        /// <returns>PolyCurve on success, null on error.</returns>
        public static async Task<PolyCurve> ToArcsAndLines(this Curve curve, double tolerance, double angleTolerance, double minimumLength, double maximumLength)
        {
            return await ComputeServer.PostAsync<PolyCurve>(ApiAddress(), curve, tolerance, angleTolerance, minimumLength, maximumLength);
        }

        /// <summary>
        /// Converts a curve into polycurve consisting of arc segments. Sections of the input curves that are nearly straight are converted to straight-line segments.
        /// </summary>
        /// <param name="tolerance">The tolerance. This is the maximum deviation from arc midpoints to the curve. When in doubt, use the document's model space absolute tolerance.</param>
        /// <param name="angleTolerance">The angle tolerance in radians. This is the maximum deviation of the arc end directions from the curve direction. When in doubt, use the document's model space angle tolerance.</param>
        /// <param name="minimumLength">The minimum segment length.</param>
        /// <param name="maximumLength">The maximum segment length.</param>
        /// <returns>PolyCurve on success, null on error.</returns>
        public static async Task<PolyCurve> ToArcsAndLines(Remote<Curve> curve, double tolerance, double angleTolerance, double minimumLength, double maximumLength)
        {
            return await ComputeServer.PostAsync<PolyCurve>(ApiAddress(), curve, tolerance, angleTolerance, minimumLength, maximumLength);
        }

        /// <summary>
        /// Makes a polyline approximation of the curve and gets the closest point on the mesh for each point on the curve. 
        /// Then it "connects the points" so that you have a polyline on the mesh.
        /// </summary>
        /// <param name="mesh">Mesh to project onto.</param>
        /// <param name="tolerance">Input tolerance. When in doubt, the the document's model absolute tolerance.</param>
        /// <returns>A curve if success, or null on failure.</returns>
        public static async Task<PolylineCurve> PullToMesh(this Curve curve, Mesh mesh, double tolerance)
        {
            return await ComputeServer.PostAsync<PolylineCurve>(ApiAddress(), curve, mesh, tolerance);
        }

        /// <summary>
        /// Makes a polyline approximation of the curve and gets the closest point on the mesh for each point on the curve. 
        /// Then it "connects the points" so that you have a polyline on the mesh.
        /// </summary>
        /// <param name="mesh">Mesh to project onto.</param>
        /// <param name="tolerance">Input tolerance. When in doubt, the the document's model absolute tolerance.</param>
        /// <returns>A curve if success, or null on failure.</returns>
        public static async Task<PolylineCurve> PullToMesh(Remote<Curve> curve, Remote<Mesh> mesh, double tolerance)
        {
            return await ComputeServer.PostAsync<PolylineCurve>(ApiAddress(), curve, mesh, tolerance);
        }

        /// <summary>
        /// Projects this curve onto a mesh.
        /// </summary>
        /// <param name="mesh">Mesh to project onto.</param>
        /// <param name="tolerance">Input tolerance. When in doubt, the the document's model absolute tolerance.</param>
        /// <param name="loose">
        /// If true, the curve's edit points are pulled back to the mesh.
        /// If any edit point misses the mesh, the curve will not be created.
        /// </param>
        /// <returns>A curve if success, or null on failure.</returns>
        public static async Task<Curve> PullToMesh(this Curve curve, Mesh mesh, double tolerance, bool loose)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, mesh, tolerance, loose);
        }

        /// <summary>
        /// Projects this curve onto a mesh.
        /// </summary>
        /// <param name="mesh">Mesh to project onto.</param>
        /// <param name="tolerance">Input tolerance. When in doubt, the the document's model absolute tolerance.</param>
        /// <param name="loose">
        /// If true, the curve's edit points are pulled back to the mesh.
        /// If any edit point misses the mesh, the curve will not be created.
        /// </param>
        /// <returns>A curve if success, or null on failure.</returns>
        public static async Task<Curve> PullToMesh(Remote<Curve> curve, Remote<Mesh> mesh, double tolerance, bool loose)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, mesh, tolerance, loose);
        }

        /// <summary>
        /// Offsets this curve. If you have a nice offset, then there will be one entry in 
        /// the array. If the original curve had kinks or the offset curve had self 
        /// intersections, you will get multiple segments in the output array.
        /// </summary>
        /// <param name="plane">Offset solution plane.</param>
        /// <param name="distance">The positive or negative distance to offset.</param>
        /// <param name="tolerance">The offset or fitting tolerance.</param>
        /// <param name="cornerStyle">Corner style for offset kinks.</param>
        /// <returns>Offset curves on success, null on failure.</returns>
        public static async Task<Curve[]> Offset(this Curve curve, Plane plane, double distance, double tolerance, CurveOffsetCornerStyle cornerStyle)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, plane, distance, tolerance, cornerStyle);
        }

        /// <summary>
        /// Offsets this curve. If you have a nice offset, then there will be one entry in 
        /// the array. If the original curve had kinks or the offset curve had self 
        /// intersections, you will get multiple segments in the output array.
        /// </summary>
        /// <param name="plane">Offset solution plane.</param>
        /// <param name="distance">The positive or negative distance to offset.</param>
        /// <param name="tolerance">The offset or fitting tolerance.</param>
        /// <param name="cornerStyle">Corner style for offset kinks.</param>
        /// <returns>Offset curves on success, null on failure.</returns>
        public static async Task<Curve[]> Offset(Remote<Curve> curve, Plane plane, double distance, double tolerance, CurveOffsetCornerStyle cornerStyle)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, plane, distance, tolerance, cornerStyle);
        }

        /// <summary>
        /// Offsets this curve. If you have a nice offset, then there will be one entry in 
        /// the array. If the original curve had kinks or the offset curve had self 
        /// intersections, you will get multiple segments in the output array.
        /// </summary>
        /// <param name="directionPoint">A point that indicates the direction of the offset.</param>
        /// <param name="normal">The normal to the offset plane.</param>
        /// <param name="distance">The positive or negative distance to offset.</param>
        /// <param name="tolerance">The offset or fitting tolerance.</param>
        /// <param name="cornerStyle">Corner style for offset kinks.</param>
        /// <returns>Offset curves on success, null on failure.</returns>
        public static async Task<Curve[]> Offset(this Curve curve, Point3d directionPoint, Vector3d normal, double distance, double tolerance, CurveOffsetCornerStyle cornerStyle)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, directionPoint, normal, distance, tolerance, cornerStyle);
        }

        /// <summary>
        /// Offsets this curve. If you have a nice offset, then there will be one entry in 
        /// the array. If the original curve had kinks or the offset curve had self 
        /// intersections, you will get multiple segments in the output array.
        /// </summary>
        /// <param name="directionPoint">A point that indicates the direction of the offset.</param>
        /// <param name="normal">The normal to the offset plane.</param>
        /// <param name="distance">The positive or negative distance to offset.</param>
        /// <param name="tolerance">The offset or fitting tolerance.</param>
        /// <param name="cornerStyle">Corner style for offset kinks.</param>
        /// <returns>Offset curves on success, null on failure.</returns>
        public static async Task<Curve[]> Offset(Remote<Curve> curve, Point3d directionPoint, Vector3d normal, double distance, double tolerance, CurveOffsetCornerStyle cornerStyle)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, directionPoint, normal, distance, tolerance, cornerStyle);
        }

        /// <summary>
        /// Offsets this curve. If you have a nice offset, then there will be one entry in 
        /// the array. If the original curve had kinks or the offset curve had self 
        /// intersections, you will get multiple segments in the output array.
        /// </summary>
        /// <param name="directionPoint">A point that indicates the direction of the offset.</param>
        /// <param name="normal">The normal to the offset plane.</param>
        /// <param name="distance">The positive or negative distance to offset.</param>
        /// <param name="tolerance">The offset or fitting tolerance.</param>
        /// <param name="angleTolerance">The angle tolerance, in radians, used to decide whether to split at kinks.</param>
        /// <param name="loose">If false, offset within tolerance. If true, offset by moving edit points.</param>
        /// <param name="cornerStyle">Corner style for offset kinks.</param>
        /// <param name="endStyle">End style for non-loose, non-closed curve offsets.</param>
        /// <returns>Offset curves on success, null on failure.</returns>
        public static async Task<Curve[]> Offset(this Curve curve, Point3d directionPoint, Vector3d normal, double distance, double tolerance, double angleTolerance, bool loose, CurveOffsetCornerStyle cornerStyle, CurveOffsetEndStyle endStyle)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, directionPoint, normal, distance, tolerance, angleTolerance, loose, cornerStyle, endStyle);
        }

        /// <summary>
        /// Offsets this curve. If you have a nice offset, then there will be one entry in 
        /// the array. If the original curve had kinks or the offset curve had self 
        /// intersections, you will get multiple segments in the output array.
        /// </summary>
        /// <param name="directionPoint">A point that indicates the direction of the offset.</param>
        /// <param name="normal">The normal to the offset plane.</param>
        /// <param name="distance">The positive or negative distance to offset.</param>
        /// <param name="tolerance">The offset or fitting tolerance.</param>
        /// <param name="angleTolerance">The angle tolerance, in radians, used to decide whether to split at kinks.</param>
        /// <param name="loose">If false, offset within tolerance. If true, offset by moving edit points.</param>
        /// <param name="cornerStyle">Corner style for offset kinks.</param>
        /// <param name="endStyle">End style for non-loose, non-closed curve offsets.</param>
        /// <returns>Offset curves on success, null on failure.</returns>
        public static async Task<Curve[]> Offset(Remote<Curve> curve, Point3d directionPoint, Vector3d normal, double distance, double tolerance, double angleTolerance, bool loose, CurveOffsetCornerStyle cornerStyle, CurveOffsetEndStyle endStyle)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, directionPoint, normal, distance, tolerance, angleTolerance, loose, cornerStyle, endStyle);
        }

        /// <summary>
        /// Offset this curve on a brep face surface. This curve must lie on the surface.
        /// </summary>
        /// <param name="face">The brep face on which to offset.</param>
        /// <param name="distance">A distance to offset (+)left, (-)right.</param>
        /// <param name="fittingTolerance">A fitting tolerance.</param>
        /// <returns>Offset curves on success, or null on failure.</returns>
        /// <exception cref="ArgumentNullException">If face is null.</exception>
        public static async Task<Curve[]> OffsetOnSurface(this Curve curve, BrepFace face, double distance, double fittingTolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, face, distance, fittingTolerance);
        }

        /// <summary>
        /// Offset this curve on a brep face surface. This curve must lie on the surface.
        /// </summary>
        /// <param name="face">The brep face on which to offset.</param>
        /// <param name="distance">A distance to offset (+)left, (-)right.</param>
        /// <param name="fittingTolerance">A fitting tolerance.</param>
        /// <returns>Offset curves on success, or null on failure.</returns>
        /// <exception cref="ArgumentNullException">If face is null.</exception>
        public static async Task<Curve[]> OffsetOnSurface(Remote<Curve> curve, BrepFace face, double distance, double fittingTolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, face, distance, fittingTolerance);
        }

        /// <summary>
        /// Offset a curve on a brep face surface. This curve must lie on the surface.
        /// <para>This overload allows to specify a surface point at which the offset will pass.</para>
        /// </summary>
        /// <param name="face">The brep face on which to offset.</param>
        /// <param name="throughPoint">2d point on the brep face to offset through.</param>
        /// <param name="fittingTolerance">A fitting tolerance.</param>
        /// <returns>Offset curves on success, or null on failure.</returns>
        /// <exception cref="ArgumentNullException">If face is null.</exception>
        public static async Task<Curve[]> OffsetOnSurface(this Curve curve, BrepFace face, Point2d throughPoint, double fittingTolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, face, throughPoint, fittingTolerance);
        }

        /// <summary>
        /// Offset a curve on a brep face surface. This curve must lie on the surface.
        /// <para>This overload allows to specify a surface point at which the offset will pass.</para>
        /// </summary>
        /// <param name="face">The brep face on which to offset.</param>
        /// <param name="throughPoint">2d point on the brep face to offset through.</param>
        /// <param name="fittingTolerance">A fitting tolerance.</param>
        /// <returns>Offset curves on success, or null on failure.</returns>
        /// <exception cref="ArgumentNullException">If face is null.</exception>
        public static async Task<Curve[]> OffsetOnSurface(Remote<Curve> curve, BrepFace face, Point2d throughPoint, double fittingTolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, face, throughPoint, fittingTolerance);
        }

        /// <summary>
        /// Offset a curve on a brep face surface. This curve must lie on the surface.
        /// <para>This overload allows to specify different offsets for different curve parameters.</para>
        /// </summary>
        /// <param name="face">The brep face on which to offset.</param>
        /// <param name="curveParameters">Curve parameters corresponding to the offset distances.</param>
        /// <param name="offsetDistances">distances to offset (+)left, (-)right.</param>
        /// <param name="fittingTolerance">A fitting tolerance.</param>
        /// <returns>Offset curves on success, or null on failure.</returns>
        /// <exception cref="ArgumentNullException">If face, curveParameters or offsetDistances are null.</exception>
        public static async Task<Curve[]> OffsetOnSurface(this Curve curve, BrepFace face, double[] curveParameters, double[] offsetDistances, double fittingTolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, face, curveParameters, offsetDistances, fittingTolerance);
        }

        /// <summary>
        /// Offset a curve on a brep face surface. This curve must lie on the surface.
        /// <para>This overload allows to specify different offsets for different curve parameters.</para>
        /// </summary>
        /// <param name="face">The brep face on which to offset.</param>
        /// <param name="curveParameters">Curve parameters corresponding to the offset distances.</param>
        /// <param name="offsetDistances">distances to offset (+)left, (-)right.</param>
        /// <param name="fittingTolerance">A fitting tolerance.</param>
        /// <returns>Offset curves on success, or null on failure.</returns>
        /// <exception cref="ArgumentNullException">If face, curveParameters or offsetDistances are null.</exception>
        public static async Task<Curve[]> OffsetOnSurface(Remote<Curve> curve, BrepFace face, double[] curveParameters, double[] offsetDistances, double fittingTolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, face, curveParameters, offsetDistances, fittingTolerance);
        }

        /// <summary>
        /// Offset a curve on a surface. This curve must lie on the surface.
        /// </summary>
        /// <param name="surface">A surface on which to offset.</param>
        /// <param name="distance">A distance to offset (+)left, (-)right.</param>
        /// <param name="fittingTolerance">A fitting tolerance.</param>
        /// <returns>Offset curves on success, or null on failure.</returns>
        /// <exception cref="ArgumentNullException">If surface is null.</exception>
        public static async Task<Curve[]> OffsetOnSurface(this Curve curve, Surface surface, double distance, double fittingTolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, surface, distance, fittingTolerance);
        }

        /// <summary>
        /// Offset a curve on a surface. This curve must lie on the surface.
        /// </summary>
        /// <param name="surface">A surface on which to offset.</param>
        /// <param name="distance">A distance to offset (+)left, (-)right.</param>
        /// <param name="fittingTolerance">A fitting tolerance.</param>
        /// <returns>Offset curves on success, or null on failure.</returns>
        /// <exception cref="ArgumentNullException">If surface is null.</exception>
        public static async Task<Curve[]> OffsetOnSurface(Remote<Curve> curve, Remote<Surface> surface, double distance, double fittingTolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, surface, distance, fittingTolerance);
        }

        /// <summary>
  /// Offset a curve on a surface. This curve must lie on the surface.
  /// <para>This overload allows to specify a surface point at which the offset will pass.</para>
  /// </summary>
  /// <param name="surface">A surface on which to offset.</param>
  /// <param name="throughPoint">2d point on the brep face to offset through.</param>
  /// <param name="fittingTolerance">A fitting tolerance.</param>
  /// <returns>Offset curves on success, or null on failure.</returns>
  /// <exception cref="ArgumentNullException">If surface is null.</exception>
        public static async Task<Curve[]> OffsetOnSurface(this Curve curve, Surface surface, Point2d throughPoint, double fittingTolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, surface, throughPoint, fittingTolerance);
        }

        /// <summary>
  /// Offset a curve on a surface. This curve must lie on the surface.
  /// <para>This overload allows to specify a surface point at which the offset will pass.</para>
  /// </summary>
  /// <param name="surface">A surface on which to offset.</param>
  /// <param name="throughPoint">2d point on the brep face to offset through.</param>
  /// <param name="fittingTolerance">A fitting tolerance.</param>
  /// <returns>Offset curves on success, or null on failure.</returns>
  /// <exception cref="ArgumentNullException">If surface is null.</exception>
        public static async Task<Curve[]> OffsetOnSurface(Remote<Curve> curve, Remote<Surface> surface, Point2d throughPoint, double fittingTolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, surface, throughPoint, fittingTolerance);
        }

        /// <summary>
        /// Offset this curve on a surface. This curve must lie on the surface.
        /// <para>This overload allows to specify different offsets for different curve parameters.</para>
        /// </summary>
        /// <param name="surface">A surface on which to offset.</param>
        /// <param name="curveParameters">Curve parameters corresponding to the offset distances.</param>
        /// <param name="offsetDistances">Distances to offset (+)left, (-)right.</param>
        /// <param name="fittingTolerance">A fitting tolerance.</param>
        /// <returns>Offset curves on success, or null on failure.</returns>
        /// <exception cref="ArgumentNullException">If surface, curveParameters or offsetDistances are null.</exception>
        public static async Task<Curve[]> OffsetOnSurface(this Curve curve, Surface surface, double[] curveParameters, double[] offsetDistances, double fittingTolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, surface, curveParameters, offsetDistances, fittingTolerance);
        }

        /// <summary>
        /// Offset this curve on a surface. This curve must lie on the surface.
        /// <para>This overload allows to specify different offsets for different curve parameters.</para>
        /// </summary>
        /// <param name="surface">A surface on which to offset.</param>
        /// <param name="curveParameters">Curve parameters corresponding to the offset distances.</param>
        /// <param name="offsetDistances">Distances to offset (+)left, (-)right.</param>
        /// <param name="fittingTolerance">A fitting tolerance.</param>
        /// <returns>Offset curves on success, or null on failure.</returns>
        /// <exception cref="ArgumentNullException">If surface, curveParameters or offsetDistances are null.</exception>
        public static async Task<Curve[]> OffsetOnSurface(Remote<Curve> curve, Remote<Surface> surface, double[] curveParameters, double[] offsetDistances, double fittingTolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), curve, surface, curveParameters, offsetDistances, fittingTolerance);
        }

        /// <summary>
        /// Finds a curve by offsetting an existing curve normal to a surface.
        /// The caller is responsible for ensuring that the curve lies on the input surface.
        /// </summary>
        /// <param name="surface">Surface from which normals are calculated.</param>
        /// <param name="height">Offset distance.</param>
        /// <returns>
        /// Curve offset normal to the surface, if successful, null otherwise.
        /// The offset curve is interpolated through a small number of points so if the
        /// surface is irregular or complicated, the result will not be a very accurate offset.
        /// </returns>
        public static async Task<Curve> OffsetNormalToSurface(this Curve curve, Surface surface, double height)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, surface, height);
        }

        /// <summary>
        /// Finds a curve by offsetting an existing curve normal to a surface.
        /// The caller is responsible for ensuring that the curve lies on the input surface.
        /// </summary>
        /// <param name="surface">Surface from which normals are calculated.</param>
        /// <param name="height">Offset distance.</param>
        /// <returns>
        /// Curve offset normal to the surface, if successful, null otherwise.
        /// The offset curve is interpolated through a small number of points so if the
        /// surface is irregular or complicated, the result will not be a very accurate offset.
        /// </returns>
        public static async Task<Curve> OffsetNormalToSurface(Remote<Curve> curve, Remote<Surface> surface, double height)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, surface, height);
        }

        /// <summary>
        /// Finds a curve by offsetting an existing curve tangent to a surface.
        /// The caller is responsible for ensuring that the curve lies on the input surface.
        /// </summary>
        /// <param name="surface">Surface from which tangents are calculated.</param>
        /// <param name="height">Offset distance.</param>
        /// <returns>
        /// Curve offset tangent to the surface, if successful, null otherwise.
        /// The offset curve is interpolated through a small number of points so if the
        /// surface is irregular or complicated, the result will not be a very accurate offset.
        /// </returns>
        public static async Task<Curve> OffsetTangentToSurface(this Curve curve, Surface surface, double height)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, surface, height);
        }

        /// <summary>
        /// Finds a curve by offsetting an existing curve tangent to a surface.
        /// The caller is responsible for ensuring that the curve lies on the input surface.
        /// </summary>
        /// <param name="surface">Surface from which tangents are calculated.</param>
        /// <param name="height">Offset distance.</param>
        /// <returns>
        /// Curve offset tangent to the surface, if successful, null otherwise.
        /// The offset curve is interpolated through a small number of points so if the
        /// surface is irregular or complicated, the result will not be a very accurate offset.
        /// </returns>
        public static async Task<Curve> OffsetTangentToSurface(Remote<Curve> curve, Remote<Surface> surface, double height)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), curve, surface, height);
        }

        /// <summary>
        /// Gets the curve's control polygon.
        /// </summary>
        /// <returns>The control polygon as a polyline if successful, null otherwise.</returns>
        public static async Task<Polyline> ControlPolygon(this Curve curve)
        {
            return await ComputeServer.PostAsync<Polyline>(ApiAddress(), curve);
        }

        /// <summary>
        /// Gets the curve's control polygon.
        /// </summary>
        /// <returns>The control polygon as a polyline if successful, null otherwise.</returns>
        public static async Task<Polyline> ControlPolygon(Remote<Curve> curve)
        {
            return await ComputeServer.PostAsync<Polyline>(ApiAddress(), curve);
        }
    }

    public static class GeometryBaseCompute
    {
        static string ApiAddress([CallerMemberName] string caller = null)
        {
            return ComputeServer.ApiAddress(typeof(GeometryBase), caller);
        }

        /// <summary>
        /// Bounding box solver. Gets the world axis aligned bounding box for the geometry.
        /// </summary>
        /// <param name="accurate">If true, a physically accurate bounding box will be computed. 
        /// If not, a bounding box estimate will be computed. For some geometry types there is no 
        /// difference between the estimate and the accurate bounding box. Estimated bounding boxes 
        /// can be computed much (much) faster than accurate (or "tight") bounding boxes. 
        /// Estimated bounding boxes are always similar to or larger than accurate bounding boxes.</param>
        /// <returns>
        /// The bounding box of the geometry in world coordinates or BoundingBox.Empty 
        /// if not bounding box could be found.
        /// </returns>
        /// <example>
        /// <code source='examples\vbnet\ex_curveboundingbox.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_curveboundingbox.cs' lang='cs'/>
        /// <code source='examples\py\ex_curveboundingbox.py' lang='py'/>
        /// </example>
        public static async Task<BoundingBox> GetBoundingBox(this GeometryBase geometrybase, bool accurate)
        {
            return await ComputeServer.PostAsync<BoundingBox>(ApiAddress(), geometrybase, accurate);
        }

        /// <summary>
        /// Bounding box solver. Gets the world axis aligned bounding box for the geometry.
        /// </summary>
        /// <param name="accurate">If true, a physically accurate bounding box will be computed. 
        /// If not, a bounding box estimate will be computed. For some geometry types there is no 
        /// difference between the estimate and the accurate bounding box. Estimated bounding boxes 
        /// can be computed much (much) faster than accurate (or "tight") bounding boxes. 
        /// Estimated bounding boxes are always similar to or larger than accurate bounding boxes.</param>
        /// <returns>
        /// The bounding box of the geometry in world coordinates or BoundingBox.Empty 
        /// if not bounding box could be found.
        /// </returns>
        /// <example>
        /// <code source='examples\vbnet\ex_curveboundingbox.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_curveboundingbox.cs' lang='cs'/>
        /// <code source='examples\py\ex_curveboundingbox.py' lang='py'/>
        /// </example>
        public static async Task<BoundingBox> GetBoundingBox(Remote<GeometryBase> geometrybase, bool accurate)
        {
            return await ComputeServer.PostAsync<BoundingBox>(ApiAddress(), geometrybase, accurate);
        }

        /// <summary>
        /// Aligned Bounding box solver. Gets the world axis aligned bounding box for the transformed geometry.
        /// </summary>
        /// <param name="xform">Transformation to apply to object prior to the BoundingBox computation. 
        /// The geometry itself is not modified.</param>
        /// <returns>The accurate bounding box of the transformed geometry in world coordinates 
        /// or BoundingBox.Empty if not bounding box could be found.</returns>
        public static async Task<BoundingBox> GetBoundingBox(this GeometryBase geometrybase, Transform xform)
        {
            return await ComputeServer.PostAsync<BoundingBox>(ApiAddress(), geometrybase, xform);
        }

        /// <summary>
        /// Aligned Bounding box solver. Gets the world axis aligned bounding box for the transformed geometry.
        /// </summary>
        /// <param name="xform">Transformation to apply to object prior to the BoundingBox computation. 
        /// The geometry itself is not modified.</param>
        /// <returns>The accurate bounding box of the transformed geometry in world coordinates 
        /// or BoundingBox.Empty if not bounding box could be found.</returns>
        public static async Task<BoundingBox> GetBoundingBox(Remote<GeometryBase> geometrybase, Transform xform)
        {
            return await ComputeServer.PostAsync<BoundingBox>(ApiAddress(), geometrybase, xform);
        }

        /// <summary>
        /// Determines if two geometries equal one another, in pure geometrical shape.
        /// This version only compares the geometry itself and does not include any user
        /// data comparisons.
        /// This is a comparison by value: for two identical items it will be true, no matter
        /// where in memory they may be stored.
        /// </summary>
        /// <param name="first">The first geometry</param>
        /// <param name="second">The second geometry</param>
        /// <returns>The indication of equality</returns>
        public static async Task<bool> GeometryEquals(GeometryBase first, GeometryBase second)
        {
            return await ComputeServer.PostAsync<bool>(ApiAddress(), first, second);
        }

        /// <summary>
        /// Determines if two geometries equal one another, in pure geometrical shape.
        /// This version only compares the geometry itself and does not include any user
        /// data comparisons.
        /// This is a comparison by value: for two identical items it will be true, no matter
        /// where in memory they may be stored.
        /// </summary>
        /// <param name="first">The first geometry</param>
        /// <param name="second">The second geometry</param>
        /// <returns>The indication of equality</returns>
        public static async Task<bool> GeometryEquals(Remote<GeometryBase> first, Remote<GeometryBase> second)
        {
            return await ComputeServer.PostAsync<bool>(ApiAddress(), first, second);
        }
    }

    public static class MeshCompute
    {
        static string ApiAddress([CallerMemberName] string caller = null)
        {
            return ComputeServer.ApiAddress(typeof(Mesh), caller);
        }

        /// <summary>
        /// Constructs a planar mesh grid.
        /// </summary>
        /// <param name="plane">Plane of mesh.</param>
        /// <param name="xInterval">Interval describing size and extends of mesh along plane x-direction.</param>
        /// <param name="yInterval">Interval describing size and extends of mesh along plane y-direction.</param>
        /// <param name="xCount">Number of faces in x-direction.</param>
        /// <param name="yCount">Number of faces in y-direction.</param>
        /// <exception cref="ArgumentException">Thrown when plane is a null reference.</exception>
        /// <exception cref="ArgumentException">Thrown when xInterval is a null reference.</exception>
        /// <exception cref="ArgumentException">Thrown when yInterval is a null reference.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when xCount is less than or equal to zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when yCount is less than or equal to zero.</exception>
        public static async Task<Mesh> CreateFromPlane(Plane plane, Interval xInterval, Interval yInterval, int xCount, int yCount)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), plane, xInterval, yInterval, xCount, yCount);
        }

        /// <summary>
        /// Constructs a sub-mesh, that contains a filtered list of faces.
        /// </summary>
        /// <param name="original">The mesh to copy. If null, null is returned.</param>
        /// <param name="inclusion">A series of true and false values, that determine if each face is used in the new mesh. 
        /// If null or empty, a non-filtered copy of the original mesh is returned.
        /// <para>If the amount does not match the length of the face list, the pattern is repeated. If it exceeds the amount
        /// of faces in the mesh face list, the pattern is truncated.</para>
        /// </param>
        /// <returns>A copy of the original mesh with its faces filtered, or null on failure.</returns>
        public static async Task<Mesh> CreateFromFilteredFaceList(Mesh original, IEnumerable<bool> inclusion)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), original, inclusion);
        }

        /// <summary>
        /// Constructs a sub-mesh, that contains a filtered list of faces.
        /// </summary>
        /// <param name="original">The mesh to copy. If null, null is returned.</param>
        /// <param name="inclusion">A series of true and false values, that determine if each face is used in the new mesh. 
        /// If null or empty, a non-filtered copy of the original mesh is returned.
        /// <para>If the amount does not match the length of the face list, the pattern is repeated. If it exceeds the amount
        /// of faces in the mesh face list, the pattern is truncated.</para>
        /// </param>
        /// <returns>A copy of the original mesh with its faces filtered, or null on failure.</returns>
        public static async Task<Mesh> CreateFromFilteredFaceList(Remote<Mesh> original, IEnumerable<bool> inclusion)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), original, inclusion);
        }

        /// <summary>
        /// Constructs new mesh that matches a bounding box.
        /// </summary>
        /// <param name="box">A box to use for creation.</param>
        /// <param name="xCount">Number of faces in x-direction.</param>
        /// <param name="yCount">Number of faces in y-direction.</param>
        /// <param name="zCount">Number of faces in z-direction.</param>
        /// <returns>A new brep, or null on failure.</returns>
        public static async Task<Mesh> CreateFromBox(BoundingBox box, int xCount, int yCount, int zCount)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), box, xCount, yCount, zCount);
        }

        /// <summary>
        ///  Constructs new mesh that matches an aligned box.
        /// </summary>
        /// <param name="box">Box to match.</param>
        /// <param name="xCount">Number of faces in x-direction.</param>
        /// <param name="yCount">Number of faces in y-direction.</param>
        /// <param name="zCount">Number of faces in z-direction.</param>
        /// <returns></returns>
        public static async Task<Mesh> CreateFromBox(Box box, int xCount, int yCount, int zCount)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), box, xCount, yCount, zCount);
        }

        /// <summary>
        /// Constructs new mesh from 8 corner points.
        /// </summary>
        /// <param name="corners">
        /// 8 points defining the box corners arranged as the vN labels indicate.
        /// <pre>
        /// <para>v7_____________v6</para>
        /// <para>|\                         |\</para>
        /// <para>| \                        | \</para>
        /// <para>|  \ _____________\</para>
        /// <para>|   v4                 |   v5</para>
        /// <para>|   |                  |   |</para>
        /// <para>|   |                  |   |</para>
        /// <para>v3--|----------v2  |</para>
        /// <para> \  |                   \  |</para>
        /// <para>  \ |                        \ |</para>
        /// <para>   \|                         \|</para>
        /// <para>        v0_____________v1</para>
        /// </pre>
        /// </param>
        /// <param name="xCount">Number of faces in x-direction.</param>
        /// <param name="yCount">Number of faces in y-direction.</param>
        /// <param name="zCount">Number of faces in z-direction.</param>
        /// <returns>A new brep, or null on failure.</returns>
        /// <returns>A new box mesh, on null on error.</returns>
        public static async Task<Mesh> CreateFromBox(IEnumerable<Point3d> corners, int xCount, int yCount, int zCount)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), corners, xCount, yCount, zCount);
        }

        /// <summary>
        /// Constructs a mesh sphere.
        /// </summary>
        /// <param name="sphere">Base sphere for mesh.</param>
        /// <param name="xCount">Number of faces in the around direction.</param>
        /// <param name="yCount">Number of faces in the top-to-bottom direction.</param>
        /// <exception cref="ArgumentException">Thrown when sphere is invalid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when xCount is less than or equal to two.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when yCount is less than or equal to two.</exception>
        /// <returns></returns>
        public static async Task<Mesh> CreateFromSphere(Sphere sphere, int xCount, int yCount)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), sphere, xCount, yCount);
        }

        /// <summary>
        /// Constructs a icospherical mesh. A mesh icosphere differs from a standard
        /// UV mesh sphere in that it's vertices are evenly distributed. A mesh icosphere
        /// starts from an icosahedron (a regular polyhedron with 20 equilateral triangles).
        /// It is then refined by splitting each triangle into 4 smaller triangles.
        /// This splitting can be done several times.
        /// </summary>
        /// <param name="sphere">The input sphere provides the orienting plane and radius.</param>
        /// <param name="subdivisions">
        /// The number of times you want the faces split, where 0  &lt;= subdivisions &lt;= 7. 
        /// Note, the total number of mesh faces produces is: 20 * (4 ^ subdivisions)
        /// </param>
        /// <returns>A welded mesh icosphere if successful, or null on failure.</returns>
        public static async Task<Mesh> CreateIcoSphere(Sphere sphere, int subdivisions)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), sphere, subdivisions);
        }

        /// <summary>
        /// Constructs a quad mesh sphere. A quad mesh sphere differs from a standard
        /// UV mesh sphere in that it's vertices are evenly distributed. A quad mesh sphere
        /// starts from a cube (a regular polyhedron with 6 square sides).
        /// It is then refined by splitting each quad into 4 smaller quads.
        /// This splitting can be done several times.
        /// </summary>
        /// <param name="sphere">The input sphere provides the orienting plane and radius.</param>
        /// <param name="subdivisions">
        /// The number of times you want the faces split, where 0  &lt;= subdivisions &lt;= 8. 
        /// Note, the total number of mesh faces produces is: 6 * (4 ^ subdivisions)
        /// </param>
        /// <returns>A welded quad mesh sphere if successful, or null on failure.</returns>
        public static async Task<Mesh> CreateQuadSphere(Sphere sphere, int subdivisions)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), sphere, subdivisions);
        }

        /// <summary>Constructs a capped mesh cylinder.</summary>
        /// <param name="cylinder"></param>
        /// <param name="vertical">Number of faces in the top-to-bottom direction.</param>
        /// <param name="around">Number of faces around the cylinder.</param>
        /// <exception cref="ArgumentException">Thrown when cylinder is invalid.</exception>
        /// <returns>Returns a mesh cylinder if successful, null otherwise.</returns>
        public static async Task<Mesh> CreateFromCylinder(Cylinder cylinder, int vertical, int around)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), cylinder, vertical, around);
        }

        /// <summary>Constructs a mesh cylinder.</summary>
        /// <param name="cylinder"></param>
        /// <param name="vertical">Number of faces in the top-to-bottom direction.</param>
        /// <param name="around">Number of faces around the cylinder.</param>
        /// <param name="capBottom">If true end at Cylinder.Height1 should be capped.</param>
        /// <param name="capTop">If true end at Cylinder.Height2 should be capped.</param>
        /// <exception cref="ArgumentException">Thrown when cylinder is invalid.</exception>
        /// <returns>Returns a mesh cylinder if successful, null otherwise.</returns>
        public static async Task<Mesh> CreateFromCylinder(Cylinder cylinder, int vertical, int around, bool capBottom, bool capTop)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), cylinder, vertical, around, capBottom, capTop);
        }

        /// <summary>Constructs a mesh cylinder.</summary>
        /// <param name="cylinder"></param>
        /// <param name="vertical">Number of faces in the top-to-bottom direction.</param>
        /// <param name="around">Number of faces around the cylinder.</param>
        /// <param name="capBottom">If true end at Cylinder.Height1 should be capped.</param>
        /// <param name="capTop">If true end at Cylinder.Height2 should be capped.</param>
        /// <param name="quadCaps">If true and it's possible to make quad caps, i.e.. around is even, then caps will have quad faces.</param>
        /// <exception cref="ArgumentException">Thrown when cylinder is invalid.</exception>
        /// <returns>Returns a mesh cylinder if successful, null otherwise.</returns>
        public static async Task<Mesh> CreateFromCylinder(Cylinder cylinder, int vertical, int around, bool capBottom, bool capTop, bool quadCaps)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), cylinder, vertical, around, capBottom, capTop, quadCaps);
        }

        /// <summary>Constructs a mesh cylinder.</summary>
        /// <param name="cylinder"></param>
        /// <param name="vertical">Number of faces in the top-to-bottom direction.</param>
        /// <param name="around">Number of faces around the cylinder.</param>
        /// <param name="capBottom">If true end at Cylinder.Height1 should be capped.</param>
        /// <param name="capTop">If true end at Cylinder.Height2 should be capped.</param>
        /// <param name="circumscribe">If true end polygons will circumscribe circle.</param>
        /// <param name="quadCaps">If true and it's possible to make quad caps, i.e.. around is even, then caps will have quad faces.</param>
        /// <exception cref="ArgumentException">Thrown when cylinder is invalid.</exception>
        /// <returns>Returns a mesh cylinder if successful, null otherwise.</returns>
        public static async Task<Mesh> CreateFromCylinder(Cylinder cylinder, int vertical, int around, bool capBottom, bool capTop, bool circumscribe, bool quadCaps)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), cylinder, vertical, around, capBottom, capTop, circumscribe, quadCaps);
        }

        /// <summary>Constructs a solid mesh cone.</summary>
        /// <param name="cone"></param>
        /// <param name="vertical">Number of faces in the top-to-bottom direction.</param>
        /// <param name="around">Number of faces around the cone.</param>
        /// <exception cref="ArgumentException">Thrown when cone is invalid.</exception>
        /// <returns>A valid mesh if successful.</returns>
        public static async Task<Mesh> CreateFromCone(Cone cone, int vertical, int around)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), cone, vertical, around);
        }

        /// <summary>Constructs a mesh cone.</summary>
        /// <param name="cone"></param>
        /// <param name="vertical">Number of faces in the top-to-bottom direction.</param>
        /// <param name="around">Number of faces around the cone.</param>
        /// <param name="solid">If false the mesh will be open with no faces on the circular planar portion.</param>
        /// <exception cref="ArgumentException">Thrown when cone is invalid.</exception>
        /// <returns>A valid mesh if successful.</returns>
        public static async Task<Mesh> CreateFromCone(Cone cone, int vertical, int around, bool solid)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), cone, vertical, around, solid);
        }

        /// <summary>Constructs a mesh cone.</summary>
        /// <param name="cone"></param>
        /// <param name="vertical">Number of faces in the top-to-bottom direction.</param>
        /// <param name="around">Number of faces around the cone.</param>
        /// <param name="solid">If false the mesh will be open with no faces on the circular planar portion.</param>
        /// <param name="quadCaps">If true and it's possible to make quad caps, i.e.. around is even, then caps will have quad faces.</param>
        /// <exception cref="ArgumentException">Thrown when cone is invalid.</exception>
        /// <returns>A valid mesh if successful.</returns>
        public static async Task<Mesh> CreateFromCone(Cone cone, int vertical, int around, bool solid, bool quadCaps)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), cone, vertical, around, solid, quadCaps);
        }

        /// <summary>
        /// Constructs a mesh torus.
        /// </summary>
        /// <param name="torus">The torus.</param>
        /// <param name="vertical">Number of faces in the top-to-bottom direction.</param>
        /// <param name="around">Number of faces around the torus.</param>
        /// <exception cref="ArgumentException">Thrown when torus is invalid.</exception>
        /// <returns>Returns a mesh torus if successful, null otherwise.</returns>
        public static async Task<Mesh> CreateFromTorus(Torus torus, int vertical, int around)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), torus, vertical, around);
        }

        /// <summary>
        /// Do not use this overload. Use version that takes a tolerance parameter instead.
        /// </summary>
        /// <param name="boundary">Do not use.</param>
        /// <param name="parameters">Do not use.</param>
        /// <returns>
        /// Do not use.
        /// </returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Mesh> CreateFromPlanarBoundary(Curve boundary, MeshingParameters parameters)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), boundary, parameters);
        }

        /// <summary>
        /// Do not use this overload. Use version that takes a tolerance parameter instead.
        /// </summary>
        /// <param name="boundary">Do not use.</param>
        /// <param name="parameters">Do not use.</param>
        /// <returns>
        /// Do not use.
        /// </returns>
        /// <deprecated>6.0</deprecated>
        public static async Task<Mesh> CreateFromPlanarBoundary(Remote<Curve> boundary, MeshingParameters parameters)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), boundary, parameters);
        }

        /// <summary>
        /// Attempts to construct a mesh from a closed planar curve.RhinoMakePlanarMeshes
        /// </summary>
        /// <param name="boundary">must be a closed planar curve.</param>
        /// <param name="parameters">parameters used for creating the mesh.</param>
        /// <param name="tolerance">Tolerance to use during operation.</param>
        /// <returns>
        /// New mesh on success or null on failure.
        /// </returns>
        public static async Task<Mesh> CreateFromPlanarBoundary(Curve boundary, MeshingParameters parameters, double tolerance)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), boundary, parameters, tolerance);
        }

        /// <summary>
        /// Attempts to construct a mesh from a closed planar curve.RhinoMakePlanarMeshes
        /// </summary>
        /// <param name="boundary">must be a closed planar curve.</param>
        /// <param name="parameters">parameters used for creating the mesh.</param>
        /// <param name="tolerance">Tolerance to use during operation.</param>
        /// <returns>
        /// New mesh on success or null on failure.
        /// </returns>
        public static async Task<Mesh> CreateFromPlanarBoundary(Remote<Curve> boundary, MeshingParameters parameters, double tolerance)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), boundary, parameters, tolerance);
        }

        /// <summary>
        /// Attempts to create a Mesh that is a triangulation of a simple closed polyline that projects onto a plane.
        /// </summary>
        /// <param name="polyline">must be closed</param>
        /// <returns>
        /// New mesh on success or null on failure.
        /// </returns>
        public static async Task<Mesh> CreateFromClosedPolyline(Polyline polyline)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), polyline);
        }

        /// <summary>
        /// Attempts to create a mesh that is a triangulation of a list of points, projected on a plane,
        /// including its holes and fixed edges.
        /// </summary>
        /// <param name="points">A list, an array or any enumerable of points.</param>
        /// <param name="plane">A plane.</param>
        /// <param name="allowNewVertices">If true, the mesh might have more vertices than the list of input points,
        /// if doing so will improve long thin triangles.</param>
        /// <param name="edges">A list of polylines, or other lists of points representing edges.
        /// This can be null. If nested enumerable items are null, they will be discarded.</param>
        /// <returns>A new mesh, or null if not successful.</returns>
        /// <exception cref="ArgumentNullException">If points is null.</exception>
        /// <exception cref="ArgumentException">If plane is not valid.</exception>
        public static async Task<Mesh> CreateFromTessellation(IEnumerable<Point3d> points, IEnumerable<IEnumerable<Point3d>> edges, Plane plane, bool allowNewVertices)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), points, edges, plane, allowNewVertices);
        }

        /// <summary>
        /// Constructs a mesh from a brep.
        /// </summary>
        /// <param name="brep">Brep to approximate.</param>
        /// <returns>An array of meshes.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_tightboundingbox.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_tightboundingbox.cs' lang='cs'/>
        /// <code source='examples\py\ex_tightboundingbox.py' lang='py'/>
        /// </example>
        /// <deprecated>6.0</deprecated>
        public static async Task<Mesh[]> CreateFromBrep(Brep brep)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), brep);
        }

        /// <summary>
        /// Constructs a mesh from a brep.
        /// </summary>
        /// <param name="brep">Brep to approximate.</param>
        /// <returns>An array of meshes.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_tightboundingbox.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_tightboundingbox.cs' lang='cs'/>
        /// <code source='examples\py\ex_tightboundingbox.py' lang='py'/>
        /// </example>
        /// <deprecated>6.0</deprecated>
        public static async Task<Mesh[]> CreateFromBrep(Remote<Brep> brep)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), brep);
        }

        /// <summary>
        /// Constructs a mesh from a brep.
        /// </summary>
        /// <param name="brep">Brep to approximate.</param>
        /// <param name="meshingParameters">Parameters to use during meshing.</param>
        /// <returns>An array of meshes.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_createmeshfrombrep.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_createmeshfrombrep.cs' lang='cs'/>
        /// <code source='examples\py\ex_createmeshfrombrep.py' lang='py'/>
        /// </example>
        public static async Task<Mesh[]> CreateFromBrep(Brep brep, MeshingParameters meshingParameters)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), brep, meshingParameters);
        }

        /// <summary>
        /// Constructs a mesh from a brep.
        /// </summary>
        /// <param name="brep">Brep to approximate.</param>
        /// <param name="meshingParameters">Parameters to use during meshing.</param>
        /// <returns>An array of meshes.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_createmeshfrombrep.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_createmeshfrombrep.cs' lang='cs'/>
        /// <code source='examples\py\ex_createmeshfrombrep.py' lang='py'/>
        /// </example>
        public static async Task<Mesh[]> CreateFromBrep(Remote<Brep> brep, MeshingParameters meshingParameters)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), brep, meshingParameters);
        }

        /// <summary>
        /// Constructs a mesh from a surface
        /// </summary>
        /// <param name="surface">Surface to approximate</param>
        /// <returns>New mesh representing the surface</returns>
        public static async Task<Mesh> CreateFromSurface(Surface surface)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), surface);
        }

        /// <summary>
        /// Constructs a mesh from a surface
        /// </summary>
        /// <param name="surface">Surface to approximate</param>
        /// <returns>New mesh representing the surface</returns>
        public static async Task<Mesh> CreateFromSurface(Remote<Surface> surface)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), surface);
        }

        /// <summary>
        /// Constructs a mesh from a surface
        /// </summary>
        /// <param name="surface">Surface to approximate</param>
        /// <param name="meshingParameters">settings used to create the mesh</param>
        /// <returns>New mesh representing the surface</returns>
        public static async Task<Mesh> CreateFromSurface(Surface surface, MeshingParameters meshingParameters)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), surface, meshingParameters);
        }

        /// <summary>
        /// Constructs a mesh from a surface
        /// </summary>
        /// <param name="surface">Surface to approximate</param>
        /// <param name="meshingParameters">settings used to create the mesh</param>
        /// <returns>New mesh representing the surface</returns>
        public static async Task<Mesh> CreateFromSurface(Remote<Surface> surface, MeshingParameters meshingParameters)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), surface, meshingParameters);
        }

        /// <summary>
        /// Create a mesh from a SubD limit surface
        /// </summary>
        /// <param name="subd">SubD to mesh</param>
        /// <param name="displayDensity">
        /// Adaptive display density value to use. If in doubt, pass 
        /// <see cref="SubDDisplayParameters.Density.DefaultDensity"/>: this is what the cache uses.
        /// </param>
        /// <returns>New mesh representing the SubD</returns>
        /// <seealso cref="SubD.UpdateSurfaceMeshCache(bool)"/>
        public static async Task<Mesh> CreateFromSubD(SubD subd, int displayDensity)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), subd, displayDensity);
        }

        /// <summary>
        /// Constructs a mesh from a brep.
        /// </summary>
        /// <param name="extrusion">Brep to approximate.</param>
        /// <param name="meshingParameters">Parameters to use during meshing.</param>
        /// <returns>A mesh.</returns>
        public static async Task<Mesh> CreateFromExtrusion(Extrusion extrusion, MeshingParameters meshingParameters)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), extrusion, meshingParameters);
        }

        /// <summary>
        /// Constructs a mesh from a brep.
        /// </summary>
        /// <param name="extrusion">Brep to approximate.</param>
        /// <param name="meshingParameters">Parameters to use during meshing.</param>
        /// <returns>A mesh.</returns>
        public static async Task<Mesh> CreateFromExtrusion(Remote<Extrusion> extrusion, MeshingParameters meshingParameters)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), extrusion, meshingParameters);
        }

        /// <summary>
        /// Construct a mesh patch from a variety of input geometry.
        /// </summary>
        /// <param name="outerBoundary">(optional: can be null) Outer boundary
        /// polyline, if provided this will become the outer boundary of the
        /// resulting mesh. Any of the input that is completely outside the outer
        /// boundary will be ignored and have no impact on the result. If any of
        /// the input intersects the outer boundary the result will be
        /// unpredictable and is likely to not include the entire outer boundary.
        /// </param>
        /// <param name="angleToleranceRadians">
        /// Maximum angle between unit tangents and adjacent vertices. Used to
        /// divide curve inputs that cannot otherwise be represented as a polyline.
        /// </param>
        /// <param name="innerBoundaryCurves">
        /// (optional: can be null) Polylines to create holes in the output mesh.
        /// If innerBoundaryCurves are the only input then the result may be null
        /// if trimback is set to false (see comments for trimback) because the
        /// resulting mesh could be invalid (all faces created contained vertices
        /// from the perimeter boundary).
        /// </param>
        /// <param name="pullbackSurface">
        /// (optional: can be null) Initial surface where 3d input will be pulled
        /// to make a 2d representation used by the function that generates the mesh.
        /// Providing a pullbackSurface can be helpful when it is similar in shape
        /// to the pattern of the input, the pulled 2d points will be a better
        /// representation of the 3d points. If all of the input is more or less
        /// coplanar to start with, providing pullbackSurface has no real benefit.
        /// </param>
        /// <param name="innerBothSideCurves">
        /// (optional: can be null) These polylines will create faces on both sides
        /// of the edge. If there are only input points(innerPoints) there is no
        /// way to guarantee a triangulation that will create an edge between two
        /// particular points. Adding a line, or polyline, to innerBothsideCurves
        /// that includes points from innerPoints will help guide the triangulation.
        /// </param>
        /// <param name="innerPoints">
        /// (optional: can be null) Points to be used to generate the mesh. If
        /// outerBoundary is not null, points outside of that boundary after it has
        /// been pulled to pullbackSurface (or the best plane through the input if
        /// pullbackSurface is null) will be ignored.
        /// </param>
        /// <param name="trimback">
        /// Only used when a outerBoundary has not been provided. When that is the
        /// case, the function uses the perimeter of the surface as the outer boundary
        /// instead. If true, any face of the resulting triangulated mesh that
        /// contains a vertex of the perimeter boundary will be removed.
        /// </param>
        /// <param name="divisions">
        /// Only used when a outerBoundary has not been provided. When that is the
        /// case, division becomes the number of divisions each side of the surface's
        /// perimeter will be divided into to create an outer boundary to work with.
        /// </param>
        /// <returns>mesh on success; null on failure</returns>
        public static async Task<Mesh> CreatePatch(Polyline outerBoundary, double angleToleranceRadians, Surface pullbackSurface, IEnumerable<Curve> innerBoundaryCurves, IEnumerable<Curve> innerBothSideCurves, IEnumerable<Point3d> innerPoints, bool trimback, int divisions)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), outerBoundary, angleToleranceRadians, pullbackSurface, innerBoundaryCurves, innerBothSideCurves, innerPoints, trimback, divisions);
        }

        /// <summary>
        /// Construct a mesh patch from a variety of input geometry.
        /// </summary>
        /// <param name="outerBoundary">(optional: can be null) Outer boundary
        /// polyline, if provided this will become the outer boundary of the
        /// resulting mesh. Any of the input that is completely outside the outer
        /// boundary will be ignored and have no impact on the result. If any of
        /// the input intersects the outer boundary the result will be
        /// unpredictable and is likely to not include the entire outer boundary.
        /// </param>
        /// <param name="angleToleranceRadians">
        /// Maximum angle between unit tangents and adjacent vertices. Used to
        /// divide curve inputs that cannot otherwise be represented as a polyline.
        /// </param>
        /// <param name="innerBoundaryCurves">
        /// (optional: can be null) Polylines to create holes in the output mesh.
        /// If innerBoundaryCurves are the only input then the result may be null
        /// if trimback is set to false (see comments for trimback) because the
        /// resulting mesh could be invalid (all faces created contained vertices
        /// from the perimeter boundary).
        /// </param>
        /// <param name="pullbackSurface">
        /// (optional: can be null) Initial surface where 3d input will be pulled
        /// to make a 2d representation used by the function that generates the mesh.
        /// Providing a pullbackSurface can be helpful when it is similar in shape
        /// to the pattern of the input, the pulled 2d points will be a better
        /// representation of the 3d points. If all of the input is more or less
        /// coplanar to start with, providing pullbackSurface has no real benefit.
        /// </param>
        /// <param name="innerBothSideCurves">
        /// (optional: can be null) These polylines will create faces on both sides
        /// of the edge. If there are only input points(innerPoints) there is no
        /// way to guarantee a triangulation that will create an edge between two
        /// particular points. Adding a line, or polyline, to innerBothsideCurves
        /// that includes points from innerPoints will help guide the triangulation.
        /// </param>
        /// <param name="innerPoints">
        /// (optional: can be null) Points to be used to generate the mesh. If
        /// outerBoundary is not null, points outside of that boundary after it has
        /// been pulled to pullbackSurface (or the best plane through the input if
        /// pullbackSurface is null) will be ignored.
        /// </param>
        /// <param name="trimback">
        /// Only used when a outerBoundary has not been provided. When that is the
        /// case, the function uses the perimeter of the surface as the outer boundary
        /// instead. If true, any face of the resulting triangulated mesh that
        /// contains a vertex of the perimeter boundary will be removed.
        /// </param>
        /// <param name="divisions">
        /// Only used when a outerBoundary has not been provided. When that is the
        /// case, division becomes the number of divisions each side of the surface's
        /// perimeter will be divided into to create an outer boundary to work with.
        /// </param>
        /// <returns>mesh on success; null on failure</returns>
        public static async Task<Mesh> CreatePatch(Polyline outerBoundary, double angleToleranceRadians, Remote<Surface> pullbackSurface, Remote<IEnumerable<Curve>> innerBoundaryCurves, Remote<IEnumerable<Curve>> innerBothSideCurves, IEnumerable<Point3d> innerPoints, bool trimback, int divisions)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), outerBoundary, angleToleranceRadians, pullbackSurface, innerBoundaryCurves, innerBothSideCurves, innerPoints, trimback, divisions);
        }

        /// <summary>
        /// Computes the solid union of a set of meshes. WARNING: Use the overload that takes a tolerance or options.
        /// </summary>
        /// <param name="meshes">Meshes to union.</param>
        /// <returns>An array of Mesh results or null on failure.</returns>
        public static async Task<Mesh[]> CreateBooleanUnion(IEnumerable<Mesh> meshes)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), meshes);
        }

        /// <summary>
        /// Computes the solid union of a set of meshes. WARNING: Use the overload that takes a tolerance or options.
        /// </summary>
        /// <param name="meshes">Meshes to union.</param>
        /// <returns>An array of Mesh results or null on failure.</returns>
        public static async Task<Mesh[]> CreateBooleanUnion(Remote<IEnumerable<Mesh>> meshes)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), meshes);
        }

        /// <summary>
        /// Computes the solid union of a set of meshes.
        /// </summary>
        /// <param name="meshes">Meshes to union.</param>
        /// <param name="tolerance">A valid tolerance value. See <see cref="Intersect.Intersection.MeshIntersectionsTolerancesCoefficient"/></param>
        /// <returns>An array of Mesh results or null on failure.</returns>
        public static async Task<Mesh[]> CreateBooleanUnion(IEnumerable<Mesh> meshes, double tolerance)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), meshes, tolerance);
        }

        /// <summary>
        /// Computes the solid union of a set of meshes.
        /// </summary>
        /// <param name="meshes">Meshes to union.</param>
        /// <param name="tolerance">A valid tolerance value. See <see cref="Intersect.Intersection.MeshIntersectionsTolerancesCoefficient"/></param>
        /// <returns>An array of Mesh results or null on failure.</returns>
        public static async Task<Mesh[]> CreateBooleanUnion(Remote<IEnumerable<Mesh>> meshes, double tolerance)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), meshes, tolerance);
        }

        /// <summary>
        /// Computes the solid difference of two sets of Meshes.
        /// </summary>
        /// <param name="firstSet">First set of Meshes (the set to subtract from).</param>
        /// <param name="secondSet">Second set of Meshes (the set to subtract).</param>
        /// <returns>An array of Mesh results or null on failure.</returns>
        public static async Task<Mesh[]> CreateBooleanDifference(IEnumerable<Mesh> firstSet, IEnumerable<Mesh> secondSet)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), firstSet, secondSet);
        }

        /// <summary>
        /// Computes the solid difference of two sets of Meshes.
        /// </summary>
        /// <param name="firstSet">First set of Meshes (the set to subtract from).</param>
        /// <param name="secondSet">Second set of Meshes (the set to subtract).</param>
        /// <returns>An array of Mesh results or null on failure.</returns>
        public static async Task<Mesh[]> CreateBooleanDifference(Remote<IEnumerable<Mesh>> firstSet, Remote<IEnumerable<Mesh>> secondSet)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), firstSet, secondSet);
        }

        /// <summary>
        /// Computes the solid intersection of two sets of meshes.
        /// </summary>
        /// <param name="firstSet">First set of Meshes.</param>
        /// <param name="secondSet">Second set of Meshes.</param>
        /// <returns>An array of Mesh results or null on failure.</returns>
        public static async Task<Mesh[]> CreateBooleanIntersection(IEnumerable<Mesh> firstSet, IEnumerable<Mesh> secondSet)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), firstSet, secondSet);
        }

        /// <summary>
        /// Computes the solid intersection of two sets of meshes.
        /// </summary>
        /// <param name="firstSet">First set of Meshes.</param>
        /// <param name="secondSet">Second set of Meshes.</param>
        /// <returns>An array of Mesh results or null on failure.</returns>
        public static async Task<Mesh[]> CreateBooleanIntersection(Remote<IEnumerable<Mesh>> firstSet, Remote<IEnumerable<Mesh>> secondSet)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), firstSet, secondSet);
        }

        /// <summary>
        /// Splits a set of meshes with another set.
        /// </summary>
        /// <param name="meshesToSplit">A list, an array, or any enumerable set of meshes to be split. If this is null, null will be returned.</param>
        /// <param name="meshSplitters">A list, an array, or any enumerable set of meshes that cut. If this is null, null will be returned.</param>
        /// <returns>A new mesh array, or null on error.</returns>
        public static async Task<Mesh[]> CreateBooleanSplit(IEnumerable<Mesh> meshesToSplit, IEnumerable<Mesh> meshSplitters)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), meshesToSplit, meshSplitters);
        }

        /// <summary>
        /// Splits a set of meshes with another set.
        /// </summary>
        /// <param name="meshesToSplit">A list, an array, or any enumerable set of meshes to be split. If this is null, null will be returned.</param>
        /// <param name="meshSplitters">A list, an array, or any enumerable set of meshes that cut. If this is null, null will be returned.</param>
        /// <returns>A new mesh array, or null on error.</returns>
        public static async Task<Mesh[]> CreateBooleanSplit(Remote<IEnumerable<Mesh>> meshesToSplit, Remote<IEnumerable<Mesh>> meshSplitters)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), meshesToSplit, meshSplitters);
        }

        /// <summary>
        /// Constructs a mesh from an extruded curve.
        /// This method is designed for projecting curves onto a mesh. 
        /// In most cases, a better to use <see cref="Mesh.CreateExtrusion(Curve, Vector3d)"/>.
        /// </summary>
        /// <param name="curve">A curve to extrude.</param>
        /// <param name="direction">The direction of extrusion.</param>
        /// <param name="parameters">The parameters of meshing.</param>
        /// <param name="boundingBox">The bounding box controls the length of the extrusion.</param>
        /// <returns>A new mesh, or null on failure.</returns>
        public static async Task<Mesh> CreateFromCurveExtrusion(Curve curve, Vector3d direction, MeshingParameters parameters, BoundingBox boundingBox)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), curve, direction, parameters, boundingBox);
        }

        /// <summary>
        /// Constructs a mesh from an extruded curve.
        /// This method is designed for projecting curves onto a mesh. 
        /// In most cases, a better to use <see cref="Mesh.CreateExtrusion(Curve, Vector3d)"/>.
        /// </summary>
        /// <param name="curve">A curve to extrude.</param>
        /// <param name="direction">The direction of extrusion.</param>
        /// <param name="parameters">The parameters of meshing.</param>
        /// <param name="boundingBox">The bounding box controls the length of the extrusion.</param>
        /// <returns>A new mesh, or null on failure.</returns>
        public static async Task<Mesh> CreateFromCurveExtrusion(Remote<Curve> curve, Vector3d direction, MeshingParameters parameters, BoundingBox boundingBox)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), curve, direction, parameters, boundingBox);
        }

        /// <summary>
        /// Constructs a mesh by extruding a curve along a vector.
        /// </summary>
        /// <param name="profile">Profile curve to extrude.</param>
        /// <param name="direction">Direction and length of extrusion.</param>
        /// <returns>A mesh on success or null on failure.</returns>
        public static async Task<Mesh> CreateExtrusion(Curve profile, Vector3d direction)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), profile, direction);
        }

        /// <summary>
        /// Constructs a mesh by extruding a curve along a vector.
        /// </summary>
        /// <param name="profile">Profile curve to extrude.</param>
        /// <param name="direction">Direction and length of extrusion.</param>
        /// <returns>A mesh on success or null on failure.</returns>
        public static async Task<Mesh> CreateExtrusion(Remote<Curve> profile, Vector3d direction)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), profile, direction);
        }

        /// <summary>
        /// Constructs a mesh by extruding a curve along a vector.
        /// </summary>
        /// <param name="profile">Profile curve to extrude.</param>
        /// <param name="direction">Direction and length of extrusion.</param>
        /// <param name="parameters">Parameters used to create the mesh.</param>
        /// <returns>A mesh on success or null on failure.</returns>
        public static async Task<Mesh> CreateExtrusion(Curve profile, Vector3d direction, MeshingParameters parameters)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), profile, direction, parameters);
        }

        /// <summary>
        /// Constructs a mesh by extruding a curve along a vector.
        /// </summary>
        /// <param name="profile">Profile curve to extrude.</param>
        /// <param name="direction">Direction and length of extrusion.</param>
        /// <param name="parameters">Parameters used to create the mesh.</param>
        /// <returns>A mesh on success or null on failure.</returns>
        public static async Task<Mesh> CreateExtrusion(Remote<Curve> profile, Vector3d direction, MeshingParameters parameters)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), profile, direction, parameters);
        }

        /// <summary>Repairs meshes with vertices that are too near, using a tolerance value.</summary>
        /// <param name="meshes">The meshes to be repaired.</param>
        /// <param name="tolerance">A minimum distance for clean vertices.</param>
        /// <returns>A valid meshes array if successful. If no change was required, some meshes can be null. Otherwise, null, when no changes were done.</returns>
        /// <remarks><seealso cref="RequireIterativeCleanup"/></remarks>
        public static async Task<Mesh[]> CreateFromIterativeCleanup(IEnumerable<Mesh> meshes, double tolerance)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), meshes, tolerance);
        }

        /// <summary>Repairs meshes with vertices that are too near, using a tolerance value.</summary>
        /// <param name="meshes">The meshes to be repaired.</param>
        /// <param name="tolerance">A minimum distance for clean vertices.</param>
        /// <returns>A valid meshes array if successful. If no change was required, some meshes can be null. Otherwise, null, when no changes were done.</returns>
        /// <remarks><seealso cref="RequireIterativeCleanup"/></remarks>
        public static async Task<Mesh[]> CreateFromIterativeCleanup(Remote<IEnumerable<Mesh>> meshes, double tolerance)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), meshes, tolerance);
        }

        /// <summary>
        /// Analyzes some meshes, and determines if a pass of CreateFromIterativeCleanup would change the array.
        /// <para>All available cleanup steps are used. Currently available cleanup steps are:</para>
        /// <para>- mending of single precision coincidence even though double precision vertices differ.</para>
        /// <para>- union of nearly identical vertices, irrespectively of their origin.</para>
        /// <para>- removal of t-joints along edges.</para>
        /// </summary>
        /// <param name="meshes">A list, and array or any enumerable of meshes.</param>
        /// <param name="tolerance">A 3d distance. This is usually a value of about 10e-7 magnitude.</param>
        /// <returns>True if meshes would be changed, otherwise false.</returns>
        /// <remarks><seealso cref="Rhino.Geometry.Mesh.CreateFromIterativeCleanup"/></remarks>
        public static async Task<bool> RequireIterativeCleanup(IEnumerable<Mesh> meshes, double tolerance)
        {
            return await ComputeServer.PostAsync<bool>(ApiAddress(), meshes, tolerance);
        }

        /// <summary>
        /// Analyzes some meshes, and determines if a pass of CreateFromIterativeCleanup would change the array.
        /// <para>All available cleanup steps are used. Currently available cleanup steps are:</para>
        /// <para>- mending of single precision coincidence even though double precision vertices differ.</para>
        /// <para>- union of nearly identical vertices, irrespectively of their origin.</para>
        /// <para>- removal of t-joints along edges.</para>
        /// </summary>
        /// <param name="meshes">A list, and array or any enumerable of meshes.</param>
        /// <param name="tolerance">A 3d distance. This is usually a value of about 10e-7 magnitude.</param>
        /// <returns>True if meshes would be changed, otherwise false.</returns>
        /// <remarks><seealso cref="Rhino.Geometry.Mesh.CreateFromIterativeCleanup"/></remarks>
        public static async Task<bool> RequireIterativeCleanup(Remote<IEnumerable<Mesh>> meshes, double tolerance)
        {
            return await ComputeServer.PostAsync<bool>(ApiAddress(), meshes, tolerance);
        }

        /// <summary>
        /// Compute volume of the mesh. 
        /// </summary>
        /// <returns>Volume of the mesh.</returns>
        public static async Task<double> Volume(this Mesh mesh)
        {
            return await ComputeServer.PostAsync<double>(ApiAddress(), mesh);
        }

        /// <summary>
        /// Compute volume of the mesh. 
        /// </summary>
        /// <returns>Volume of the mesh.</returns>
        public static async Task<double> Volume(Remote<Mesh> mesh)
        {
            return await ComputeServer.PostAsync<double>(ApiAddress(), mesh);
        }

        /// <summary>
        /// Determines if a point is inside a solid mesh.
        /// </summary>
        /// <param name="point">3d point to test.</param>
        /// <param name="tolerance">
        /// (&gt;=0) 3d distance tolerance used for ray-mesh intersection
        /// and determining strict inclusion. This is expected to be a tiny value.
        /// </param>
        /// <param name="strictlyIn">
        /// If strictlyIn is true, then point must be inside mesh by at least
        /// tolerance in order for this function to return true.
        /// If strictlyIn is false, then this function will return true if
        /// point is inside or the distance from point to a mesh face is &lt;= tolerance.
        /// </param>
        /// <returns>
        /// true if point is inside the solid mesh, false if not.
        /// </returns>
        /// <remarks>
        /// The caller is responsible for making certain the mesh is valid, closed, and 2-manifold before
        /// calling this function. If the mesh is not, the behavior is undetermined.
        /// </remarks>
        public static async Task<bool> IsPointInside(this Mesh mesh, Point3d point, double tolerance, bool strictlyIn)
        {
            return await ComputeServer.PostAsync<bool>(ApiAddress(), mesh, point, tolerance, strictlyIn);
        }

        /// <summary>
        /// Determines if a point is inside a solid mesh.
        /// </summary>
        /// <param name="point">3d point to test.</param>
        /// <param name="tolerance">
        /// (&gt;=0) 3d distance tolerance used for ray-mesh intersection
        /// and determining strict inclusion. This is expected to be a tiny value.
        /// </param>
        /// <param name="strictlyIn">
        /// If strictlyIn is true, then point must be inside mesh by at least
        /// tolerance in order for this function to return true.
        /// If strictlyIn is false, then this function will return true if
        /// point is inside or the distance from point to a mesh face is &lt;= tolerance.
        /// </param>
        /// <returns>
        /// true if point is inside the solid mesh, false if not.
        /// </returns>
        /// <remarks>
        /// The caller is responsible for making certain the mesh is valid, closed, and 2-manifold before
        /// calling this function. If the mesh is not, the behavior is undetermined.
        /// </remarks>
        public static async Task<bool> IsPointInside(Remote<Mesh> mesh, Point3d point, double tolerance, bool strictlyIn)
        {
            return await ComputeServer.PostAsync<bool>(ApiAddress(), mesh, point, tolerance, strictlyIn);
        }

        /// <summary>
        /// Makes sure that faces sharing an edge and having a difference of normal greater
        /// than or equal to angleToleranceRadians have unique vertices along that edge,
        /// adding vertices if necessary.
        /// </summary>
        /// <param name="angleToleranceRadians">Angle at which to make unique vertices.</param>
        /// <param name="modifyNormals">
        /// Determines whether new vertex normals will have the same vertex normal as the original (false)
        /// or vertex normals made from the corresponding face normals (true)
        /// </param>
        public static async Task<Mesh> Unweld(this Mesh mesh, double angleToleranceRadians, bool modifyNormals)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), mesh, angleToleranceRadians, modifyNormals);
        }

        /// <summary>
        /// Makes sure that faces sharing an edge and having a difference of normal greater
        /// than or equal to angleToleranceRadians have unique vertices along that edge,
        /// adding vertices if necessary.
        /// </summary>
        /// <param name="angleToleranceRadians">Angle at which to make unique vertices.</param>
        /// <param name="modifyNormals">
        /// Determines whether new vertex normals will have the same vertex normal as the original (false)
        /// or vertex normals made from the corresponding face normals (true)
        /// </param>
        public static async Task<Mesh> Unweld(Remote<Mesh> mesh, double angleToleranceRadians, bool modifyNormals)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), mesh, angleToleranceRadians, modifyNormals);
        }

        /// <summary>
        /// Makes sure that faces sharing an edge and having a difference of normal greater
        /// than or equal to angleToleranceRadians share vertices along that edge, vertex normals
        /// are averaged.
        /// </summary>
        /// <param name="angleToleranceRadians">Angle at which to weld vertices.</param>
        public static async Task<Mesh> Weld(this Mesh mesh, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), mesh, angleToleranceRadians);
        }

        /// <summary>
        /// Makes sure that faces sharing an edge and having a difference of normal greater
        /// than or equal to angleToleranceRadians share vertices along that edge, vertex normals
        /// are averaged.
        /// </summary>
        /// <param name="angleToleranceRadians">Angle at which to weld vertices.</param>
        public static async Task<Mesh> Weld(Remote<Mesh> mesh, double angleToleranceRadians)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), mesh, angleToleranceRadians);
        }

        /// <summary>
        /// Creates a new unwelded mesh from an existing mesh. Texture coordinates are ignored.
        /// </summary>
        /// <param name="mesh">The source mesh to copy.</param>
        /// <returns>The new unwelded mesh if successful, null otherwise.</returns>
        public static async Task<Mesh> CreateUnweldedMesh(Mesh mesh)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), mesh);
        }

        /// <summary>
        /// Creates a new unwelded mesh from an existing mesh. Texture coordinates are ignored.
        /// </summary>
        /// <param name="mesh">The source mesh to copy.</param>
        /// <returns>The new unwelded mesh if successful, null otherwise.</returns>
        public static async Task<Mesh> CreateUnweldedMesh(Remote<Mesh> mesh)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), mesh);
        }

        /// <summary>
        /// Creates a single mesh face from the given input.
        /// </summary>
        /// <param name="mesh">The input mesh.</param>
        /// <param name="components">An enumeration of component indexes from the input mesh.
        /// This can be one following combinations:
        /// 1 vertex (MeshVertex or MeshTopologyVertex) and 1 edge (MeshTopologyEdge), 
        /// 2 edges (MeshTopologyEdge), 
        /// or 3 vertices (MeshVertex or MeshTopologyVertex).
        /// </param>
        /// <returns>A new mesh if successful, null otherwise.</returns>
        public static async Task<Mesh> CreateFromPatchSingleFace(Mesh mesh, IEnumerable<ComponentIndex> components)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), mesh, components);
        }

        /// <summary>
        /// Creates a single mesh face from the given input.
        /// </summary>
        /// <param name="mesh">The input mesh.</param>
        /// <param name="components">An enumeration of component indexes from the input mesh.
        /// This can be one following combinations:
        /// 1 vertex (MeshVertex or MeshTopologyVertex) and 1 edge (MeshTopologyEdge), 
        /// 2 edges (MeshTopologyEdge), 
        /// or 3 vertices (MeshVertex or MeshTopologyVertex).
        /// </param>
        /// <returns>A new mesh if successful, null otherwise.</returns>
        public static async Task<Mesh> CreateFromPatchSingleFace(Remote<Mesh> mesh, IEnumerable<ComponentIndex> components)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), mesh, components);
        }

        /// <summary>
        /// Removes mesh normals and reconstructs the face and vertex normals based
        /// on the orientation of the faces.
        /// </summary>
        public static async Task<Mesh> RebuildNormals(this Mesh mesh)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), mesh);
        }

        /// <summary>
        /// Removes mesh normals and reconstructs the face and vertex normals based
        /// on the orientation of the faces.
        /// </summary>
        public static async Task<Mesh> RebuildNormals(Remote<Mesh> mesh)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), mesh);
        }

        /// <summary>
        /// Splits up the mesh into its unconnected pieces.
        /// </summary>
        /// <returns>An array containing all the disjoint pieces that make up this Mesh.</returns>
        public static async Task<Mesh[]> SplitDisjointPieces(this Mesh mesh)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), mesh);
        }

        /// <summary>
        /// Splits up the mesh into its unconnected pieces.
        /// </summary>
        /// <returns>An array containing all the disjoint pieces that make up this Mesh.</returns>
        public static async Task<Mesh[]> SplitDisjointPieces(Remote<Mesh> mesh)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), mesh);
        }

        /// <summary>
        /// Split a mesh by an infinite plane.
        /// </summary>
        /// <param name="plane">The splitting plane.</param>
        /// <returns>A new mesh array with the split result. This can be null if no result was found.</returns>
        public static async Task<Mesh[]> Split(this Mesh mesh, Plane plane)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), mesh, plane);
        }

        /// <summary>
        /// Split a mesh by an infinite plane.
        /// </summary>
        /// <param name="plane">The splitting plane.</param>
        /// <returns>A new mesh array with the split result. This can be null if no result was found.</returns>
        public static async Task<Mesh[]> Split(Remote<Mesh> mesh, Plane plane)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), mesh, plane);
        }

        /// <summary>
        /// Split a mesh with another mesh. Suggestion: upgrade to overload with tolerance.
        /// </summary>
        /// <param name="mesh">Mesh to split with.</param>
        /// <returns>An array of mesh segments representing the split result.</returns>
        public static async Task<Mesh[]> Split(this Mesh _mesh, Mesh mesh)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), _mesh, mesh);
        }

        /// <summary>
        /// Split a mesh with another mesh. Suggestion: upgrade to overload with tolerance.
        /// </summary>
        /// <param name="mesh">Mesh to split with.</param>
        /// <returns>An array of mesh segments representing the split result.</returns>
        public static async Task<Mesh[]> Split(Remote<Mesh> _mesh, Remote<Mesh> mesh)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), _mesh, mesh);
        }

        /// <summary>
        /// Split a mesh with a collection of meshes. Suggestion: upgrade to overload with tolerance.
        /// Does not split at coplanar intersections.
        /// </summary>
        /// <param name="meshes">Meshes to split with.</param>
        /// <returns>An array of mesh segments representing the split result.</returns>
        public static async Task<Mesh[]> Split(this Mesh mesh, IEnumerable<Mesh> meshes)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), mesh, meshes);
        }

        /// <summary>
        /// Split a mesh with a collection of meshes. Suggestion: upgrade to overload with tolerance.
        /// Does not split at coplanar intersections.
        /// </summary>
        /// <param name="meshes">Meshes to split with.</param>
        /// <returns>An array of mesh segments representing the split result.</returns>
        public static async Task<Mesh[]> Split(Remote<Mesh> mesh, Remote<IEnumerable<Mesh>> meshes)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), mesh, meshes);
        }

        /// <summary>
        /// Constructs the outlines of a mesh projected against a plane.
        /// </summary>
        /// <param name="plane">A plane to project against.</param>
        /// <returns>An array of polylines, or null on error.</returns>
        public static async Task<Polyline[]> GetOutlines(this Mesh mesh, Plane plane)
        {
            return await ComputeServer.PostAsync<Polyline[]>(ApiAddress(), mesh, plane);
        }

        /// <summary>
        /// Constructs the outlines of a mesh projected against a plane.
        /// </summary>
        /// <param name="plane">A plane to project against.</param>
        /// <returns>An array of polylines, or null on error.</returns>
        public static async Task<Polyline[]> GetOutlines(Remote<Mesh> mesh, Plane plane)
        {
            return await ComputeServer.PostAsync<Polyline[]>(ApiAddress(), mesh, plane);
        }

        /// <summary>
        /// Returns all edges of a mesh that are considered "naked" in the
        /// sense that the edge only has one face.
        /// </summary>
        /// <returns>An array of polylines, or null on error.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_dupmeshboundary.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_dupmeshboundary.cs' lang='cs'/>
        /// <code source='examples\py\ex_dupmeshboundary.py' lang='py'/>
        /// </example>
        public static async Task<Polyline[]> GetNakedEdges(this Mesh mesh)
        {
            return await ComputeServer.PostAsync<Polyline[]>(ApiAddress(), mesh);
        }

        /// <summary>
        /// Returns all edges of a mesh that are considered "naked" in the
        /// sense that the edge only has one face.
        /// </summary>
        /// <returns>An array of polylines, or null on error.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_dupmeshboundary.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_dupmeshboundary.cs' lang='cs'/>
        /// <code source='examples\py\ex_dupmeshboundary.py' lang='py'/>
        /// </example>
        public static async Task<Polyline[]> GetNakedEdges(Remote<Mesh> mesh)
        {
            return await ComputeServer.PostAsync<Polyline[]>(ApiAddress(), mesh);
        }

        /// <summary>
        /// Explode the mesh into sub-meshes where a sub-mesh is a collection of faces that are contained
        /// within a closed loop of "unwelded" edges. Unwelded edges are edges where the faces that share
        /// the edge have unique mesh vertices (not mesh topology vertices) at both ends of the edge.
        /// </summary>
        /// <returns>
        /// Array of sub-meshes on success; null on error. If the count in the returned array is 1, then
        /// nothing happened and the output is essentially a copy of the input.
        /// </returns>
        public static async Task<Mesh[]> ExplodeAtUnweldedEdges(this Mesh mesh)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), mesh);
        }

        /// <summary>
        /// Explode the mesh into sub-meshes where a sub-mesh is a collection of faces that are contained
        /// within a closed loop of "unwelded" edges. Unwelded edges are edges where the faces that share
        /// the edge have unique mesh vertices (not mesh topology vertices) at both ends of the edge.
        /// </summary>
        /// <returns>
        /// Array of sub-meshes on success; null on error. If the count in the returned array is 1, then
        /// nothing happened and the output is essentially a copy of the input.
        /// </returns>
        public static async Task<Mesh[]> ExplodeAtUnweldedEdges(Remote<Mesh> mesh)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), mesh);
        }

        /// <summary>
        /// Gets the point on the mesh that is closest to a given test point.
        /// </summary>
        /// <param name="testPoint">Point to search for.</param>
        /// <returns>The point on the mesh closest to testPoint, or Point3d.Unset on failure.</returns>
        public static async Task<Point3d> ClosestPoint(this Mesh mesh, Point3d testPoint)
        {
            return await ComputeServer.PostAsync<Point3d>(ApiAddress(), mesh, testPoint);
        }

        /// <summary>
        /// Gets the point on the mesh that is closest to a given test point.
        /// </summary>
        /// <param name="testPoint">Point to search for.</param>
        /// <returns>The point on the mesh closest to testPoint, or Point3d.Unset on failure.</returns>
        public static async Task<Point3d> ClosestPoint(Remote<Mesh> mesh, Point3d testPoint)
        {
            return await ComputeServer.PostAsync<Point3d>(ApiAddress(), mesh, testPoint);
        }

        /// <summary>
        /// Gets the point on the mesh that is closest to a given test point. Similar to the 
        /// ClosestPoint function except this returns a MeshPoint class which includes
        /// extra information beyond just the location of the closest point.
        /// </summary>
        /// <param name="testPoint">The source of the search.</param>
        /// <param name="maximumDistance">
        /// Optional upper bound on the distance from test point to the mesh. 
        /// If you are only interested in finding a point Q on the mesh when 
        /// testPoint.DistanceTo(Q) &lt; maximumDistance, 
        /// then set maximumDistance to that value. 
        /// This parameter is ignored if you pass 0.0 for a maximumDistance.
        /// </param>
        /// <returns>closest point information on success. null on failure.</returns>
        public static async Task<MeshPoint> ClosestMeshPoint(this Mesh mesh, Point3d testPoint, double maximumDistance)
        {
            return await ComputeServer.PostAsync<MeshPoint>(ApiAddress(), mesh, testPoint, maximumDistance);
        }

        /// <summary>
        /// Gets the point on the mesh that is closest to a given test point. Similar to the 
        /// ClosestPoint function except this returns a MeshPoint class which includes
        /// extra information beyond just the location of the closest point.
        /// </summary>
        /// <param name="testPoint">The source of the search.</param>
        /// <param name="maximumDistance">
        /// Optional upper bound on the distance from test point to the mesh. 
        /// If you are only interested in finding a point Q on the mesh when 
        /// testPoint.DistanceTo(Q) &lt; maximumDistance, 
        /// then set maximumDistance to that value. 
        /// This parameter is ignored if you pass 0.0 for a maximumDistance.
        /// </param>
        /// <returns>closest point information on success. null on failure.</returns>
        public static async Task<MeshPoint> ClosestMeshPoint(Remote<Mesh> mesh, Point3d testPoint, double maximumDistance)
        {
            return await ComputeServer.PostAsync<MeshPoint>(ApiAddress(), mesh, testPoint, maximumDistance);
        }

        /// <summary>
        /// Evaluate a mesh at a set of barycentric coordinates.
        /// </summary>
        /// <param name="meshPoint">MeshPoint instance containing a valid Face Index and Barycentric coordinates.</param>
        /// <returns>A Point on the mesh or Point3d.Unset if the faceIndex is not valid or if the barycentric coordinates could not be evaluated.</returns>
        public static async Task<Point3d> PointAt(this Mesh mesh, MeshPoint meshPoint)
        {
            return await ComputeServer.PostAsync<Point3d>(ApiAddress(), mesh, meshPoint);
        }

        /// <summary>
        /// Evaluate a mesh at a set of barycentric coordinates.
        /// </summary>
        /// <param name="meshPoint">MeshPoint instance containing a valid Face Index and Barycentric coordinates.</param>
        /// <returns>A Point on the mesh or Point3d.Unset if the faceIndex is not valid or if the barycentric coordinates could not be evaluated.</returns>
        public static async Task<Point3d> PointAt(Remote<Mesh> mesh, MeshPoint meshPoint)
        {
            return await ComputeServer.PostAsync<Point3d>(ApiAddress(), mesh, meshPoint);
        }

        /// <summary>
        /// Evaluates a mesh at a set of barycentric coordinates. Barycentric coordinates must 
        /// be assigned in accordance with the rules as defined by MeshPoint.T.
        /// </summary>
        /// <param name="faceIndex">Index of triangle or quad to evaluate.</param>
        /// <param name="t0">First barycentric coordinate.</param>
        /// <param name="t1">Second barycentric coordinate.</param>
        /// <param name="t2">Third barycentric coordinate.</param>
        /// <param name="t3">Fourth barycentric coordinate. If the face is a triangle, this coordinate will be ignored.</param>
        /// <returns>A Point on the mesh or Point3d.Unset if the faceIndex is not valid or if the barycentric coordinates could not be evaluated.</returns>
        public static async Task<Point3d> PointAt(this Mesh mesh, int faceIndex, double t0, double t1, double t2, double t3)
        {
            return await ComputeServer.PostAsync<Point3d>(ApiAddress(), mesh, faceIndex, t0, t1, t2, t3);
        }

        /// <summary>
        /// Evaluates a mesh at a set of barycentric coordinates. Barycentric coordinates must 
        /// be assigned in accordance with the rules as defined by MeshPoint.T.
        /// </summary>
        /// <param name="faceIndex">Index of triangle or quad to evaluate.</param>
        /// <param name="t0">First barycentric coordinate.</param>
        /// <param name="t1">Second barycentric coordinate.</param>
        /// <param name="t2">Third barycentric coordinate.</param>
        /// <param name="t3">Fourth barycentric coordinate. If the face is a triangle, this coordinate will be ignored.</param>
        /// <returns>A Point on the mesh or Point3d.Unset if the faceIndex is not valid or if the barycentric coordinates could not be evaluated.</returns>
        public static async Task<Point3d> PointAt(Remote<Mesh> mesh, int faceIndex, double t0, double t1, double t2, double t3)
        {
            return await ComputeServer.PostAsync<Point3d>(ApiAddress(), mesh, faceIndex, t0, t1, t2, t3);
        }

        /// <summary>
        /// Evaluate a mesh normal at a set of barycentric coordinates.
        /// </summary>
        /// <param name="meshPoint">MeshPoint instance containing a valid Face Index and Barycentric coordinates.</param>
        /// <returns>A Normal vector to the mesh or Vector3d.Unset if the faceIndex is not valid or if the barycentric coordinates could not be evaluated.</returns>
        public static async Task<Vector3d> NormalAt(this Mesh mesh, MeshPoint meshPoint)
        {
            return await ComputeServer.PostAsync<Vector3d>(ApiAddress(), mesh, meshPoint);
        }

        /// <summary>
        /// Evaluate a mesh normal at a set of barycentric coordinates.
        /// </summary>
        /// <param name="meshPoint">MeshPoint instance containing a valid Face Index and Barycentric coordinates.</param>
        /// <returns>A Normal vector to the mesh or Vector3d.Unset if the faceIndex is not valid or if the barycentric coordinates could not be evaluated.</returns>
        public static async Task<Vector3d> NormalAt(Remote<Mesh> mesh, MeshPoint meshPoint)
        {
            return await ComputeServer.PostAsync<Vector3d>(ApiAddress(), mesh, meshPoint);
        }

        /// <summary>
        /// Evaluate a mesh normal at a set of barycentric coordinates. Barycentric coordinates must 
        /// be assigned in accordance with the rules as defined by MeshPoint.T.
        /// </summary>
        /// <param name="faceIndex">Index of triangle or quad to evaluate.</param>
        /// <param name="t0">First barycentric coordinate.</param>
        /// <param name="t1">Second barycentric coordinate.</param>
        /// <param name="t2">Third barycentric coordinate.</param>
        /// <param name="t3">Fourth barycentric coordinate. If the face is a triangle, this coordinate will be ignored.</param>
        /// <returns>A Normal vector to the mesh or Vector3d.Unset if the faceIndex is not valid or if the barycentric coordinates could not be evaluated.</returns>
        public static async Task<Vector3d> NormalAt(this Mesh mesh, int faceIndex, double t0, double t1, double t2, double t3)
        {
            return await ComputeServer.PostAsync<Vector3d>(ApiAddress(), mesh, faceIndex, t0, t1, t2, t3);
        }

        /// <summary>
        /// Evaluate a mesh normal at a set of barycentric coordinates. Barycentric coordinates must 
        /// be assigned in accordance with the rules as defined by MeshPoint.T.
        /// </summary>
        /// <param name="faceIndex">Index of triangle or quad to evaluate.</param>
        /// <param name="t0">First barycentric coordinate.</param>
        /// <param name="t1">Second barycentric coordinate.</param>
        /// <param name="t2">Third barycentric coordinate.</param>
        /// <param name="t3">Fourth barycentric coordinate. If the face is a triangle, this coordinate will be ignored.</param>
        /// <returns>A Normal vector to the mesh or Vector3d.Unset if the faceIndex is not valid or if the barycentric coordinates could not be evaluated.</returns>
        public static async Task<Vector3d> NormalAt(Remote<Mesh> mesh, int faceIndex, double t0, double t1, double t2, double t3)
        {
            return await ComputeServer.PostAsync<Vector3d>(ApiAddress(), mesh, faceIndex, t0, t1, t2, t3);
        }

        /// <summary>
        /// Pulls a collection of points to a mesh.
        /// </summary>
        /// <param name="points">An array, a list or any enumerable set of points.</param>
        /// <returns>An array of points. This can be empty.</returns>
        public static async Task<Point3d[]> PullPointsToMesh(this Mesh mesh, IEnumerable<Point3d> points)
        {
            return await ComputeServer.PostAsync<Point3d[]>(ApiAddress(), mesh, points);
        }

        /// <summary>
        /// Pulls a collection of points to a mesh.
        /// </summary>
        /// <param name="points">An array, a list or any enumerable set of points.</param>
        /// <returns>An array of points. This can be empty.</returns>
        public static async Task<Point3d[]> PullPointsToMesh(Remote<Mesh> mesh, IEnumerable<Point3d> points)
        {
            return await ComputeServer.PostAsync<Point3d[]>(ApiAddress(), mesh, points);
        }

        /// <summary>
        /// Gets a polyline approximation of the input curve and then moves its control points to the closest point on the mesh.
        /// Then it "connects the points" over edges so that a polyline on the mesh is formed.
        /// </summary>
        /// <param name="curve">A curve to pull.</param>
        /// <param name="tolerance">A tolerance value.</param>
        /// <returns>A polyline curve, or null if none could be constructed.</returns>
        public static async Task<PolylineCurve> PullCurve(this Mesh mesh, Curve curve, double tolerance)
        {
            return await ComputeServer.PostAsync<PolylineCurve>(ApiAddress(), mesh, curve, tolerance);
        }

        /// <summary>
        /// Gets a polyline approximation of the input curve and then moves its control points to the closest point on the mesh.
        /// Then it "connects the points" over edges so that a polyline on the mesh is formed.
        /// </summary>
        /// <param name="curve">A curve to pull.</param>
        /// <param name="tolerance">A tolerance value.</param>
        /// <returns>A polyline curve, or null if none could be constructed.</returns>
        public static async Task<PolylineCurve> PullCurve(Remote<Mesh> mesh, Remote<Curve> curve, double tolerance)
        {
            return await ComputeServer.PostAsync<PolylineCurve>(ApiAddress(), mesh, curve, tolerance);
        }

        /// <summary>
        /// Splits a mesh by adding edges in correspondence with input polylines, and divides the mesh at partitioned areas.
        /// Polyline segments that are measured not to be on the mesh will be ignored.
        /// </summary>
        /// <param name="curves">An array, a list or any enumerable of polyline curves.</param>
        /// <param name="tolerance">A tolerance value.</param>
        /// <returns>An array of meshes, or null if no change would happen.</returns>
        public static async Task<Mesh[]> SplitWithProjectedPolylines(this Mesh mesh, IEnumerable<PolylineCurve> curves, double tolerance)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), mesh, curves, tolerance);
        }

        /// <summary>
        /// Splits a mesh by adding edges in correspondence with input polylines, and divides the mesh at partitioned areas.
        /// Polyline segments that are measured not to be on the mesh will be ignored.
        /// </summary>
        /// <param name="curves">An array, a list or any enumerable of polyline curves.</param>
        /// <param name="tolerance">A tolerance value.</param>
        /// <returns>An array of meshes, or null if no change would happen.</returns>
        public static async Task<Mesh[]> SplitWithProjectedPolylines(Remote<Mesh> mesh, IEnumerable<PolylineCurve> curves, double tolerance)
        {
            return await ComputeServer.PostAsync<Mesh[]>(ApiAddress(), mesh, curves, tolerance);
        }

        /// <summary>
        /// Makes a new mesh with vertices offset a distance in the opposite direction of the existing vertex normals.
        /// Same as Mesh.Offset(distance, false)
        /// </summary>
        /// <param name="distance">A distance value to use for offsetting.</param>
        /// <returns>A new mesh on success, or null on failure.</returns>
        public static async Task<Mesh> Offset(this Mesh mesh, double distance)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), mesh, distance);
        }

        /// <summary>
        /// Makes a new mesh with vertices offset a distance in the opposite direction of the existing vertex normals.
        /// Same as Mesh.Offset(distance, false)
        /// </summary>
        /// <param name="distance">A distance value to use for offsetting.</param>
        /// <returns>A new mesh on success, or null on failure.</returns>
        public static async Task<Mesh> Offset(Remote<Mesh> mesh, double distance)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), mesh, distance);
        }

        /// <summary>
        /// Makes a new mesh with vertices offset a distance in the opposite direction of the existing vertex normals.
        /// Optionally, based on the value of solidify, adds the input mesh and a ribbon of faces along any naked edges.
        /// If solidify is false it acts exactly as the Offset(distance) function.
        /// </summary>
        /// <param name="distance">A distance value.</param>
        /// <param name="solidify">true if the mesh should be solidified.</param>
        /// <returns>A new mesh on success, or null on failure.</returns>
        public static async Task<Mesh> Offset(this Mesh mesh, double distance, bool solidify)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), mesh, distance, solidify);
        }

        /// <summary>
        /// Makes a new mesh with vertices offset a distance in the opposite direction of the existing vertex normals.
        /// Optionally, based on the value of solidify, adds the input mesh and a ribbon of faces along any naked edges.
        /// If solidify is false it acts exactly as the Offset(distance) function.
        /// </summary>
        /// <param name="distance">A distance value.</param>
        /// <param name="solidify">true if the mesh should be solidified.</param>
        /// <returns>A new mesh on success, or null on failure.</returns>
        public static async Task<Mesh> Offset(Remote<Mesh> mesh, double distance, bool solidify)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), mesh, distance, solidify);
        }

        /// <summary>
        /// Makes a new mesh with vertices offset a distance along the direction parameter.
        /// Optionally, based on the value of solidify, adds the input mesh and a ribbon of faces along any naked edges.
        /// If solidify is false it acts exactly as the Offset(distance) function.
        /// </summary>
        /// <param name="distance">A distance value.</param>
        /// <param name="solidify">true if the mesh should be solidified.</param>
        /// <param name="direction">Direction of offset for all vertices.</param>
        /// <returns>A new mesh on success, or null on failure.</returns>
        public static async Task<Mesh> Offset(this Mesh mesh, double distance, bool solidify, Vector3d direction)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), mesh, distance, solidify, direction);
        }

        /// <summary>
        /// Makes a new mesh with vertices offset a distance along the direction parameter.
        /// Optionally, based on the value of solidify, adds the input mesh and a ribbon of faces along any naked edges.
        /// If solidify is false it acts exactly as the Offset(distance) function.
        /// </summary>
        /// <param name="distance">A distance value.</param>
        /// <param name="solidify">true if the mesh should be solidified.</param>
        /// <param name="direction">Direction of offset for all vertices.</param>
        /// <returns>A new mesh on success, or null on failure.</returns>
        public static async Task<Mesh> Offset(Remote<Mesh> mesh, double distance, bool solidify, Vector3d direction)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), mesh, distance, solidify, direction);
        }

        /// <summary>
        /// Constructs new mesh from the current one, with edge softening applied to it.
        /// </summary>
        /// <param name="softeningRadius">The softening radius.</param>
        /// <param name="chamfer">Specifies whether to chamfer the edges.</param>
        /// <param name="faceted">Specifies whether the edges are faceted.</param>
        /// <param name="force">Specifies whether to soften edges despite too large a radius.</param>
        /// <param name="angleThreshold">Threshold angle (in degrees) which controls whether an edge is softened or not.
        /// The angle refers to the angles between the adjacent faces of an edge.</param>
        /// <returns>A new mesh with soft edges.</returns>
        /// <exception cref="InvalidOperationException">If displacement failed
        /// because of an error. The exception message specifies the error.</exception>
        public static async Task<Mesh> WithEdgeSoftening(this Mesh mesh, double softeningRadius, bool chamfer, bool faceted, bool force, double angleThreshold)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), mesh, softeningRadius, chamfer, faceted, force, angleThreshold);
        }

        /// <summary>
        /// Constructs new mesh from the current one, with edge softening applied to it.
        /// </summary>
        /// <param name="softeningRadius">The softening radius.</param>
        /// <param name="chamfer">Specifies whether to chamfer the edges.</param>
        /// <param name="faceted">Specifies whether the edges are faceted.</param>
        /// <param name="force">Specifies whether to soften edges despite too large a radius.</param>
        /// <param name="angleThreshold">Threshold angle (in degrees) which controls whether an edge is softened or not.
        /// The angle refers to the angles between the adjacent faces of an edge.</param>
        /// <returns>A new mesh with soft edges.</returns>
        /// <exception cref="InvalidOperationException">If displacement failed
        /// because of an error. The exception message specifies the error.</exception>
        public static async Task<Mesh> WithEdgeSoftening(Remote<Mesh> mesh, double softeningRadius, bool chamfer, bool faceted, bool force, double angleThreshold)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), mesh, softeningRadius, chamfer, faceted, force, angleThreshold);
        }

        /// <summary>
        /// Create QuadRemesh from a Brep
        /// Set Brep Face Mode by setting QuadRemeshParameters.PreserveMeshArrayEdgesMode
        /// </summary>
        /// <param name="brep"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static async Task<Mesh> QuadRemeshBrep(Brep brep, QuadRemeshParameters parameters)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), brep, parameters);
        }

        /// <summary>
        /// Create QuadRemesh from a Brep
        /// Set Brep Face Mode by setting QuadRemeshParameters.PreserveMeshArrayEdgesMode
        /// </summary>
        /// <param name="brep"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static async Task<Mesh> QuadRemeshBrep(Remote<Brep> brep, QuadRemeshParameters parameters)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), brep, parameters);
        }

        /// <summary>
        /// Create Quad Remesh from a Brep
        /// </summary>
        /// <param name="brep">
        /// Set Brep Face Mode by setting QuadRemeshParameters.PreserveMeshArrayEdgesMode
        /// </param>
        /// <param name="parameters"></param>
        /// <param name="guideCurves">
        /// A curve array used to influence mesh face layout
        /// The curves should touch the input mesh
        /// Set Guide Curve Influence by using QuadRemeshParameters.GuideCurveInfluence
        /// </param>
        /// <returns></returns>
        public static async Task<Mesh> QuadRemeshBrep(Brep brep, QuadRemeshParameters parameters, IEnumerable<Curve> guideCurves)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), brep, parameters, guideCurves);
        }

        /// <summary>
        /// Create Quad Remesh from a Brep
        /// </summary>
        /// <param name="brep">
        /// Set Brep Face Mode by setting QuadRemeshParameters.PreserveMeshArrayEdgesMode
        /// </param>
        /// <param name="parameters"></param>
        /// <param name="guideCurves">
        /// A curve array used to influence mesh face layout
        /// The curves should touch the input mesh
        /// Set Guide Curve Influence by using QuadRemeshParameters.GuideCurveInfluence
        /// </param>
        /// <returns></returns>
        public static async Task<Mesh> QuadRemeshBrep(Remote<Brep> brep, QuadRemeshParameters parameters, Remote<IEnumerable<Curve>> guideCurves)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), brep, parameters, guideCurves);
        }

        /// <summary>
        /// Creates a unified ShrinkWrap mesh from a collection of input meshes.
        /// Returns null on error or failure. 
        /// </summary>
        /// <param name="meshes"></param>
        /// <param name="parameters">A ShrinkWrapParameters object that specifies the configuration settings for the ShrinkWrap process.</param>
        /// <returns></returns>
        public static async Task<Mesh> ShrinkWrap(IEnumerable<Mesh> meshes, ShrinkWrapParameters parameters)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), meshes, parameters);
        }

        /// <summary>
        /// Creates a unified ShrinkWrap mesh from a collection of input meshes.
        /// Returns null on error or failure. 
        /// </summary>
        /// <param name="meshes"></param>
        /// <param name="parameters">A ShrinkWrapParameters object that specifies the configuration settings for the ShrinkWrap process.</param>
        /// <returns></returns>
        public static async Task<Mesh> ShrinkWrap(Remote<IEnumerable<Mesh>> meshes, ShrinkWrapParameters parameters)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), meshes, parameters);
        }

        /// <summary>
        /// Creates a unified ShrinkWrap mesh from a point cloud
        /// returns null on error or failure
        /// </summary>
        /// <param name="pointCloud"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static async Task<Mesh> ShrinkWrap(PointCloud pointCloud, ShrinkWrapParameters parameters)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), pointCloud, parameters);
        }

        /// <summary>
        /// Creates a unified ShrinkWrap mesh from a collection of GeometryBase objects.
        /// returns null or error on failure
        /// </summary>
        /// <param name="geometryBases"></param>
        /// <param name="parameters">A ShrinkWrapParameters object that specifies the configuration settings for the ShrinkWrap process. </param>
        /// <param name="meshingParameters">GeometryBase objects are converted to meshes first using the MeshingParameters provided. Those meshes are then used in the ShrinkWrap process.</param>
        /// <returns></returns>
        public static async Task<Mesh> ShrinkWrap(IEnumerable<GeometryBase> geometryBases, ShrinkWrapParameters parameters, MeshingParameters meshingParameters)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), geometryBases, parameters, meshingParameters);
        }

        /// <summary>
        /// Creates a unified ShrinkWrap mesh from a collection of GeometryBase objects.
        /// returns null or error on failure
        /// </summary>
        /// <param name="geometryBases"></param>
        /// <param name="parameters">A ShrinkWrapParameters object that specifies the configuration settings for the ShrinkWrap process. </param>
        /// <param name="meshingParameters">GeometryBase objects are converted to meshes first using the MeshingParameters provided. Those meshes are then used in the ShrinkWrap process.</param>
        /// <returns></returns>
        public static async Task<Mesh> ShrinkWrap(Remote<IEnumerable<GeometryBase>> geometryBases, ShrinkWrapParameters parameters, MeshingParameters meshingParameters)
        {
            return await ComputeServer.PostAsync<Mesh>(ApiAddress(), geometryBases, parameters, meshingParameters);
        }

        /// <summary>
        /// Compute thickness metrics for this mesh.
        /// </summary>
        /// <param name="meshes">Meshes to include in thickness analysis.</param>
        /// <param name="maximumThickness">Maximum thickness to consider. Use as small a thickness as possible to speed up the solver.</param>
        /// <returns>Array of thickness measurements.</returns>
        public static async Task<MeshThicknessMeasurement[]> ComputeThickness(IEnumerable<Mesh> meshes, double maximumThickness)
        {
            return await ComputeServer.PostAsync<MeshThicknessMeasurement[]>(ApiAddress(), meshes, maximumThickness);
        }

        /// <summary>
        /// Compute thickness metrics for this mesh.
        /// </summary>
        /// <param name="meshes">Meshes to include in thickness analysis.</param>
        /// <param name="maximumThickness">Maximum thickness to consider. Use as small a thickness as possible to speed up the solver.</param>
        /// <returns>Array of thickness measurements.</returns>
        public static async Task<MeshThicknessMeasurement[]> ComputeThickness(Remote<IEnumerable<Mesh>> meshes, double maximumThickness)
        {
            return await ComputeServer.PostAsync<MeshThicknessMeasurement[]>(ApiAddress(), meshes, maximumThickness);
        }

        /// <summary>
        /// Compute thickness metrics for this mesh.
        /// </summary>
        /// <param name="meshes">Meshes to include in thickness analysis.</param>
        /// <param name="maximumThickness">Maximum thickness to consider. Use as small a thickness as possible to speed up the solver.</param>
        /// <param name="cancelToken">Computation cancellation token.</param>
        /// <returns>Array of thickness measurements.</returns>
        public static async Task<MeshThicknessMeasurement[]> ComputeThickness(IEnumerable<Mesh> meshes, double maximumThickness, System.Threading.CancellationToken cancelToken)
        {
            return await ComputeServer.PostAsync<MeshThicknessMeasurement[]>(ApiAddress(), meshes, maximumThickness, cancelToken);
        }

        /// <summary>
        /// Compute thickness metrics for this mesh.
        /// </summary>
        /// <param name="meshes">Meshes to include in thickness analysis.</param>
        /// <param name="maximumThickness">Maximum thickness to consider. Use as small a thickness as possible to speed up the solver.</param>
        /// <param name="cancelToken">Computation cancellation token.</param>
        /// <returns>Array of thickness measurements.</returns>
        public static async Task<MeshThicknessMeasurement[]> ComputeThickness(Remote<IEnumerable<Mesh>> meshes, double maximumThickness, System.Threading.CancellationToken cancelToken)
        {
            return await ComputeServer.PostAsync<MeshThicknessMeasurement[]>(ApiAddress(), meshes, maximumThickness, cancelToken);
        }

        /// <summary>
        /// Compute thickness metrics for this mesh.
        /// </summary>
        /// <param name="meshes">Meshes to include in thickness analysis.</param>
        /// <param name="maximumThickness">Maximum thickness to consider. Use as small a thickness as possible to speed up the solver.</param>
        /// <param name="sharpAngle">Sharpness angle in radians.</param>
        /// <param name="cancelToken">Computation cancellation token.</param>
        /// <returns>Array of thickness measurements.</returns>
        public static async Task<MeshThicknessMeasurement[]> ComputeThickness(IEnumerable<Mesh> meshes, double maximumThickness, double sharpAngle, System.Threading.CancellationToken cancelToken)
        {
            return await ComputeServer.PostAsync<MeshThicknessMeasurement[]>(ApiAddress(), meshes, maximumThickness, sharpAngle, cancelToken);
        }

        /// <summary>
        /// Compute thickness metrics for this mesh.
        /// </summary>
        /// <param name="meshes">Meshes to include in thickness analysis.</param>
        /// <param name="maximumThickness">Maximum thickness to consider. Use as small a thickness as possible to speed up the solver.</param>
        /// <param name="sharpAngle">Sharpness angle in radians.</param>
        /// <param name="cancelToken">Computation cancellation token.</param>
        /// <returns>Array of thickness measurements.</returns>
        public static async Task<MeshThicknessMeasurement[]> ComputeThickness(Remote<IEnumerable<Mesh>> meshes, double maximumThickness, double sharpAngle, System.Threading.CancellationToken cancelToken)
        {
            return await ComputeServer.PostAsync<MeshThicknessMeasurement[]>(ApiAddress(), meshes, maximumThickness, sharpAngle, cancelToken);
        }

        /// <summary>
        /// (Old call maintained for compatibility.)
        /// </summary>
        /// <param name="meshToContour">Avoid.</param>
        /// <param name="contourStart">Avoid.</param>
        /// <param name="contourEnd">Avoid.</param>
        /// <param name="interval">Avoid.</param>
        /// <returns>Avoid.</returns>
        /// <deprecated>7.13</deprecated>
        public static async Task<Curve[]> CreateContourCurves(Mesh meshToContour, Point3d contourStart, Point3d contourEnd, double interval)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), meshToContour, contourStart, contourEnd, interval);
        }

        /// <summary>
        /// (Old call maintained for compatibility.)
        /// </summary>
        /// <param name="meshToContour">Avoid.</param>
        /// <param name="contourStart">Avoid.</param>
        /// <param name="contourEnd">Avoid.</param>
        /// <param name="interval">Avoid.</param>
        /// <returns>Avoid.</returns>
        /// <deprecated>7.13</deprecated>
        public static async Task<Curve[]> CreateContourCurves(Remote<Mesh> meshToContour, Point3d contourStart, Point3d contourEnd, double interval)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), meshToContour, contourStart, contourEnd, interval);
        }

        /// <summary>
        /// Constructs contour curves for a mesh, sectioned along a linear axis.
        /// </summary>
        /// <param name="meshToContour">A mesh to contour.</param>
        /// <param name="contourStart">A start point of the contouring axis.</param>
        /// <param name="contourEnd">An end point of the contouring axis.</param>
        /// <param name="interval">An interval distance.</param>
        /// <param name="tolerance">A tolerance value. If negative, the positive value will be used.
        /// WARNING! Good tolerance values are in the magnitude of 10^-7, or RhinoMath.SqrtEpsilon*10.
        /// See comments at <see cref="Intersect.Intersection.MeshIntersectionsTolerancesCoefficient"/></param>
        /// <returns>An array of curves. This array can be empty.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_makerhinocontours.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_makerhinocontours.cs' lang='cs'/>
        /// <code source='examples\py\ex_makerhinocontours.py' lang='py'/>
        /// </example>
        public static async Task<Curve[]> CreateContourCurves(Mesh meshToContour, Point3d contourStart, Point3d contourEnd, double interval, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), meshToContour, contourStart, contourEnd, interval, tolerance);
        }

        /// <summary>
        /// Constructs contour curves for a mesh, sectioned along a linear axis.
        /// </summary>
        /// <param name="meshToContour">A mesh to contour.</param>
        /// <param name="contourStart">A start point of the contouring axis.</param>
        /// <param name="contourEnd">An end point of the contouring axis.</param>
        /// <param name="interval">An interval distance.</param>
        /// <param name="tolerance">A tolerance value. If negative, the positive value will be used.
        /// WARNING! Good tolerance values are in the magnitude of 10^-7, or RhinoMath.SqrtEpsilon*10.
        /// See comments at <see cref="Intersect.Intersection.MeshIntersectionsTolerancesCoefficient"/></param>
        /// <returns>An array of curves. This array can be empty.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_makerhinocontours.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_makerhinocontours.cs' lang='cs'/>
        /// <code source='examples\py\ex_makerhinocontours.py' lang='py'/>
        /// </example>
        public static async Task<Curve[]> CreateContourCurves(Remote<Mesh> meshToContour, Point3d contourStart, Point3d contourEnd, double interval, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), meshToContour, contourStart, contourEnd, interval, tolerance);
        }

        /// <summary>
        /// (Old call maintained for compatibility.)
        /// </summary>
        /// <param name="meshToContour">Avoid.</param>
        /// <param name="sectionPlane">Avoid.</param>
        /// <returns>Avoid.</returns>
        /// <deprecated>7.13</deprecated>
        public static async Task<Curve[]> CreateContourCurves(Mesh meshToContour, Plane sectionPlane)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), meshToContour, sectionPlane);
        }

        /// <summary>
        /// (Old call maintained for compatibility.)
        /// </summary>
        /// <param name="meshToContour">Avoid.</param>
        /// <param name="sectionPlane">Avoid.</param>
        /// <returns>Avoid.</returns>
        /// <deprecated>7.13</deprecated>
        public static async Task<Curve[]> CreateContourCurves(Remote<Mesh> meshToContour, Plane sectionPlane)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), meshToContour, sectionPlane);
        }

        /// <summary>
        /// Constructs contour curves for a mesh, sectioned at a plane.
        /// </summary>
        /// <param name="meshToContour">A mesh to contour.</param>
        /// <param name="sectionPlane">A cutting plane.</param>
        /// <param name="tolerance">A tolerance value. If negative, the positive value will be used.
        /// WARNING! Good tolerance values are in the magnitude of 10^-7, or RhinoMath.SqrtEpsilon*10.
        /// See comments at <see cref="Intersect.Intersection.MeshIntersectionsTolerancesCoefficient"/></param>
        /// <returns>An array of curves. This array can be empty.</returns>
        /// <seealso cref="Intersect.Intersection.MeshPlane(Mesh, Plane)"/>
        public static async Task<Curve[]> CreateContourCurves(Mesh meshToContour, Plane sectionPlane, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), meshToContour, sectionPlane, tolerance);
        }

        /// <summary>
        /// Constructs contour curves for a mesh, sectioned at a plane.
        /// </summary>
        /// <param name="meshToContour">A mesh to contour.</param>
        /// <param name="sectionPlane">A cutting plane.</param>
        /// <param name="tolerance">A tolerance value. If negative, the positive value will be used.
        /// WARNING! Good tolerance values are in the magnitude of 10^-7, or RhinoMath.SqrtEpsilon*10.
        /// See comments at <see cref="Intersect.Intersection.MeshIntersectionsTolerancesCoefficient"/></param>
        /// <returns>An array of curves. This array can be empty.</returns>
        /// <seealso cref="Intersect.Intersection.MeshPlane(Mesh, Plane)"/>
        public static async Task<Curve[]> CreateContourCurves(Remote<Mesh> meshToContour, Plane sectionPlane, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), meshToContour, sectionPlane, tolerance);
        }
    }

    public static class NurbsCurveCompute
    {
        static string ApiAddress([CallerMemberName] string caller = null)
        {
            return ComputeServer.ApiAddress(typeof(NurbsCurve), caller);
        }

        /// <summary>
        /// For expert use only. From the input curves, make an array of compatible NURBS curves.
        /// </summary>
        /// <param name="curves">The input curves.</param>
        /// <param name="startPt">The start point. To omit, specify Point3d.Unset.</param>
        /// <param name="endPt">The end point. To omit, specify Point3d.Unset.</param>
        /// <param name="simplifyMethod">The simplify method.</param>
        /// <param name="numPoints">The number of rebuild points.</param>
        /// <param name="refitTolerance">The refit tolerance.</param>
        /// <param name="angleTolerance">The angle tolerance in radians.</param>
        /// <returns>The output NURBS surfaces if successful.</returns>
        public static async Task<NurbsCurve[]> MakeCompatible(IEnumerable<Curve> curves, Point3d startPt, Point3d endPt, int simplifyMethod, int numPoints, double refitTolerance, double angleTolerance)
        {
            return await ComputeServer.PostAsync<NurbsCurve[]>(ApiAddress(), curves, startPt, endPt, simplifyMethod, numPoints, refitTolerance, angleTolerance);
        }

        /// <summary>
        /// For expert use only. From the input curves, make an array of compatible NURBS curves.
        /// </summary>
        /// <param name="curves">The input curves.</param>
        /// <param name="startPt">The start point. To omit, specify Point3d.Unset.</param>
        /// <param name="endPt">The end point. To omit, specify Point3d.Unset.</param>
        /// <param name="simplifyMethod">The simplify method.</param>
        /// <param name="numPoints">The number of rebuild points.</param>
        /// <param name="refitTolerance">The refit tolerance.</param>
        /// <param name="angleTolerance">The angle tolerance in radians.</param>
        /// <returns>The output NURBS surfaces if successful.</returns>
        public static async Task<NurbsCurve[]> MakeCompatible(Remote<IEnumerable<Curve>> curves, Point3d startPt, Point3d endPt, int simplifyMethod, int numPoints, double refitTolerance, double angleTolerance)
        {
            return await ComputeServer.PostAsync<NurbsCurve[]>(ApiAddress(), curves, startPt, endPt, simplifyMethod, numPoints, refitTolerance, angleTolerance);
        }

        /// <summary>
        /// Fits a NURBS curve to a dense, ordered set of points.
        /// </summary>
        /// <param name="points">An enumeration of 3D points.</param>
        /// <param name="tolerance">The fitting tolerance. When in doubt, use the document's model absolute tolerance.</param>
        /// <param name="periodic">Set true to create a periodic curve.</param>
        /// <returns>A NURBS curve if successful, or null on failure.</returns>
        public static async Task<NurbsCurve> CreateFromFitPoints(IEnumerable<Point3d> points, double tolerance, bool periodic)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), points, tolerance, periodic);
        }

        /// <summary>
        /// Fits a NURBS curve to a dense, ordered set of points.
        /// </summary>
        /// <param name="points">An enumeration of 3D points.</param>
        /// <param name="tolerance">The fitting tolerance. When in doubt, use the document's model absolute tolerance.</param>
        /// <param name="degree">The desired degree of the output curve.</param>
        /// <param name="periodic">Set true to create a periodic curve.</param>
        /// <param name="startTangent">The tangent direction at the start of the curve. If unknown, set to <see cref="Vector3d.Unset"/>.</param>
        /// <param name="endTangent">The tangent direction at the end of the curve. If unknown, set to <see cref="Vector3d.Unset"/>.</param>
        /// <returns>A NURBS curve if successful, or null on failure.</returns>
        public static async Task<NurbsCurve> CreateFromFitPoints(IEnumerable<Point3d> points, double tolerance, int degree, bool periodic, Vector3d startTangent, Vector3d endTangent)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), points, tolerance, degree, periodic, startTangent, endTangent);
        }

        /// <summary>
        /// Creates a parabola from vertex and end points.
        /// </summary>
        /// <param name="vertex">The vertex point.</param>
        /// <param name="startPoint">The start point.</param>
        /// <param name="endPoint">The end point</param>
        /// <returns>A 2 degree NURBS curve if successful, false otherwise.</returns>
        public static async Task<NurbsCurve> CreateParabolaFromVertex(Point3d vertex, Point3d startPoint, Point3d endPoint)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), vertex, startPoint, endPoint);
        }

        /// <summary>
        /// Creates a parabola from focus and end points.
        /// </summary>
        /// <param name="focus">The focal point.</param>
        /// <param name="startPoint">The start point.</param>
        /// <param name="endPoint">The end point</param>
        /// <returns>A 2 degree NURBS curve if successful, false otherwise.</returns>
        public static async Task<NurbsCurve> CreateParabolaFromFocus(Point3d focus, Point3d startPoint, Point3d endPoint)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), focus, startPoint, endPoint);
        }

        /// <summary>
        /// Creates a parabola from three points.
        /// </summary>
        /// <param name="startPoint">The start point.</param>
        /// <param name="innerPoint">A point on the curve.</param>
        /// <param name="endPoint">The end point</param>
        /// <returns>A 2 degree NURBS curve if successful, false otherwise.</returns>
        public static async Task<NurbsCurve> CreateParabolaFromPoints(Point3d startPoint, Point3d innerPoint, Point3d endPoint)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), startPoint, innerPoint, endPoint);
        }

        /// <summary>
        /// Create a uniform non-rational cubic NURBS approximation of an arc.
        /// </summary>
        /// <param name="arc"></param>
        /// <param name="degree">&gt;=1</param>
        /// <param name="cvCount">CV count &gt;=5</param>
        /// <returns>NURBS curve approximation of an arc on success</returns>
        public static async Task<NurbsCurve> CreateFromArc(Arc arc, int degree, int cvCount)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), arc, degree, cvCount);
        }

        /// <summary>
        /// Creates a non-rational approximation of a rational arc as a single bezier segment
        /// </summary>
        /// <param name="degree">The degree of the non-rational approximation, can be either 3, 4, or 5</param>
        /// <param name="center">The arc center</param>
        /// <param name="start">A point in the direction of the start point of the arc</param>
        /// <param name="end">A point in the direction of the end point of the arc</param>
        /// <param name="radius">The radius of the arc</param>
        /// <param name="tanSlider">a number between zero and one which moves the tangent control point toward the mid control point of an equivalent quadratic rational arc</param>
        /// <param name="midSlider">a number between zero and one which moves the mid control points toward the mid control point of an equivalent quadratic rational arc</param>
        /// <returns>The approximated arc.</returns>
        public static async Task<NurbsCurve> CreateNonRationalArcBezier(int degree, Point3d center, Point3d start, Point3d end, double radius, double tanSlider, double midSlider)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), degree, center, start, end, radius, tanSlider, midSlider);
        }

        /// <summary>
        /// Construct an H-spline from a sequence of interpolation points
        /// </summary>
        /// <param name="points">Points to interpolate</param>
        /// <returns></returns>
        public static async Task<NurbsCurve> CreateHSpline(IEnumerable<Point3d> points)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), points);
        }

        /// <summary>
        /// Construct an H-spline from a sequence of interpolation points and
        /// optional start and end derivative information
        /// </summary>
        /// <param name="points">Points to interpolate</param>
        /// <param name="startTangent">Unit tangent vector or Unset</param>
        /// <param name="endTangent">Unit tangent vector or Unset</param>
        /// <returns>NURBS curve approximation of an arc on success</returns>
        public static async Task<NurbsCurve> CreateHSpline(IEnumerable<Point3d> points, Vector3d startTangent, Vector3d endTangent)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), points, startTangent, endTangent);
        }

        /// <summary>
        /// Create a NURBS curve, that is suitable for calculations like lofting SubD objects, through a sequence of curves.
        /// </summary>
        /// <param name="points">
        /// An enumeration of points. Adjacent points must not be equal.
        /// If periodicClosedCurve is false, there must be at least two points.
        /// If periodicClosedCurve is true, there must be at least three points and it is not necessary to duplicate the first and last points.
        /// When periodicClosedCurve is true and the first and last points are equal, the duplicate last point is automatically ignored.
        /// </param>
        /// <param name="interpolatePoints">
        /// True if the curve should interpolate the points. False if points specify control point locations.
        /// In either case, the curve will begin at the first point and end at the last point.
        /// </param>
        /// <param name="periodicClosedCurve">
        /// True to create a periodic closed curve. Do not duplicate the start/end point in the point input.
        /// </param>
        /// <returns>A SubD friendly NURBS curve is successful, null otherwise.</returns>
        public static async Task<NurbsCurve> CreateSubDFriendly(IEnumerable<Point3d> points, bool interpolatePoints, bool periodicClosedCurve)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), points, interpolatePoints, periodicClosedCurve);
        }

        /// <summary>
        /// Create a NURBS curve, that is suitable for calculations like lofting SubD objects, from an existing curve.
        /// </summary>
        /// <param name="curve">Curve to rebuild as a SubD friendly curve.</param>
        /// <returns>A SubD friendly NURBS curve is successful, null otherwise.</returns>
        public static async Task<NurbsCurve> CreateSubDFriendly(Curve curve)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), curve);
        }

        /// <summary>
        /// Create a NURBS curve, that is suitable for calculations like lofting SubD objects, from an existing curve.
        /// </summary>
        /// <param name="curve">Curve to rebuild as a SubD friendly curve.</param>
        /// <returns>A SubD friendly NURBS curve is successful, null otherwise.</returns>
        public static async Task<NurbsCurve> CreateSubDFriendly(Remote<Curve> curve)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), curve);
        }

        /// <summary>
        /// Create a NURBS curve, that is suitable for calculations like lofting SubD objects, from an existing curve.
        /// </summary>
        /// <param name="curve">Curve to rebuild as a SubD friendly curve.</param>
        /// <param name="pointCount">
        /// Desired number of control points. If periodicClosedCurve is true, the number must be &gt;= 6, otherwise the number must be &gt;= 4.
        /// </param>
        /// <param name="periodicClosedCurve">True if the SubD friendly curve should be closed and periodic. False in all other cases.</param>
        /// <returns>A SubD friendly NURBS curve is successful, null otherwise.</returns>
        public static async Task<NurbsCurve> CreateSubDFriendly(Curve curve, int pointCount, bool periodicClosedCurve)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), curve, pointCount, periodicClosedCurve);
        }

        /// <summary>
        /// Create a NURBS curve, that is suitable for calculations like lofting SubD objects, from an existing curve.
        /// </summary>
        /// <param name="curve">Curve to rebuild as a SubD friendly curve.</param>
        /// <param name="pointCount">
        /// Desired number of control points. If periodicClosedCurve is true, the number must be &gt;= 6, otherwise the number must be &gt;= 4.
        /// </param>
        /// <param name="periodicClosedCurve">True if the SubD friendly curve should be closed and periodic. False in all other cases.</param>
        /// <returns>A SubD friendly NURBS curve is successful, null otherwise.</returns>
        public static async Task<NurbsCurve> CreateSubDFriendly(Remote<Curve> curve, int pointCount, bool periodicClosedCurve)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), curve, pointCount, periodicClosedCurve);
        }

        /// <summary>
        /// Create a uniform non-rational cubic NURBS approximation of a circle.
        /// </summary>
        /// <param name="circle"></param>
        /// <param name="degree">&gt;=1</param>
        /// <param name="cvCount">CV count &gt;=5</param>
        /// <returns>NURBS curve approximation of a circle on success</returns>
        public static async Task<NurbsCurve> CreateFromCircle(Circle circle, int degree, int cvCount)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), circle, degree, cvCount);
        }

        /// <summary>
        /// Gets Greville points for this curve.
        /// </summary>
        /// <param name="all">If true, then all Greville points are returns. If false, only edit points are returned.</param>
        /// <returns>A list of points if successful, null otherwise.</returns>
        public static async Task<Point3dList> GrevillePoints(this NurbsCurve nurbscurve, bool all)
        {
            return await ComputeServer.PostAsync<Point3dList>(ApiAddress(), nurbscurve, all);
        }

        /// <summary>
        /// Gets Greville points for this curve.
        /// </summary>
        /// <param name="all">If true, then all Greville points are returns. If false, only edit points are returned.</param>
        /// <returns>A list of points if successful, null otherwise.</returns>
        public static async Task<Point3dList> GrevillePoints(Remote<NurbsCurve> nurbscurve, bool all)
        {
            return await ComputeServer.PostAsync<Point3dList>(ApiAddress(), nurbscurve, all);
        }

        /// <summary>
        /// Creates a C1 cubic NURBS approximation of a helix or spiral. For a helix,
        /// you may have radius0 == radius1. For a spiral radius0 == radius1 produces
        /// a circle. Zero and negative radii are permissible.
        /// </summary>
        /// <param name="axisStart">Helix's axis starting point or center of spiral.</param>
        /// <param name="axisDir">Helix's axis vector or normal to spiral's plane.</param>
        /// <param name="radiusPoint">
        /// Point used only to get a vector that is perpendicular to the axis. In
        /// particular, this vector must not be (anti)parallel to the axis vector.
        /// </param>
        /// <param name="pitch">
        /// The pitch, where a spiral has a pitch = 0, and pitch > 0 is the distance
        /// between the helix's "threads".
        /// </param>
        /// <param name="turnCount">The number of turns in spiral or helix. Positive
        /// values produce counter-clockwise orientation, negative values produce
        /// clockwise orientation. Note, for a helix, turnCount * pitch = length of
        /// the helix's axis.
        /// </param>
        /// <param name="radius0">The starting radius.</param>
        /// <param name="radius1">The ending radius.</param>
        /// <returns>NurbsCurve on success, null on failure.</returns>
        public static async Task<NurbsCurve> CreateSpiral(Point3d axisStart, Vector3d axisDir, Point3d radiusPoint, double pitch, double turnCount, double radius0, double radius1)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), axisStart, axisDir, radiusPoint, pitch, turnCount, radius0, radius1);
        }

        /// <summary>
        /// Create a C2 non-rational uniform cubic NURBS approximation of a swept helix or spiral.
        /// </summary>
        /// <param name="railCurve">The rail curve.</param>
        /// <param name="t0">Starting portion of rail curve's domain to sweep along.</param>
        /// <param name="t1">Ending portion of rail curve's domain to sweep along.</param>
        /// <param name="radiusPoint">
        /// Point used only to get a vector that is perpendicular to the axis. In
        /// particular, this vector must not be (anti)parallel to the axis vector.
        /// </param>
        /// <param name="pitch">
        /// The pitch. Positive values produce counter-clockwise orientation,
        /// negative values produce clockwise orientation.
        /// </param>
        /// <param name="turnCount">
        /// The turn count. If != 0, then the resulting helix will have this many
        /// turns. If = 0, then pitch must be != 0 and the approximate distance
        /// between turns will be set to pitch. Positive values produce counter-clockwise
        /// orientation, negative values produce clockwise orientation.
        /// </param>
        /// <param name="radius0">
        /// The starting radius. At least one radii must be nonzero. Negative values
        /// are allowed.
        /// </param>
        /// <param name="radius1">
        /// The ending radius. At least one radii must be nonzero. Negative values
        /// are allowed.
        /// </param>
        /// <param name="pointsPerTurn">
        /// Number of points to interpolate per turn. Must be greater than 4.
        /// When in doubt, use 12.
        /// </param>
        /// <returns>NurbsCurve on success, null on failure.</returns>
        public static async Task<NurbsCurve> CreateSpiral(Curve railCurve, double t0, double t1, Point3d radiusPoint, double pitch, double turnCount, double radius0, double radius1, int pointsPerTurn)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), railCurve, t0, t1, radiusPoint, pitch, turnCount, radius0, radius1, pointsPerTurn);
        }

        /// <summary>
        /// Create a C2 non-rational uniform cubic NURBS approximation of a swept helix or spiral.
        /// </summary>
        /// <param name="railCurve">The rail curve.</param>
        /// <param name="t0">Starting portion of rail curve's domain to sweep along.</param>
        /// <param name="t1">Ending portion of rail curve's domain to sweep along.</param>
        /// <param name="radiusPoint">
        /// Point used only to get a vector that is perpendicular to the axis. In
        /// particular, this vector must not be (anti)parallel to the axis vector.
        /// </param>
        /// <param name="pitch">
        /// The pitch. Positive values produce counter-clockwise orientation,
        /// negative values produce clockwise orientation.
        /// </param>
        /// <param name="turnCount">
        /// The turn count. If != 0, then the resulting helix will have this many
        /// turns. If = 0, then pitch must be != 0 and the approximate distance
        /// between turns will be set to pitch. Positive values produce counter-clockwise
        /// orientation, negative values produce clockwise orientation.
        /// </param>
        /// <param name="radius0">
        /// The starting radius. At least one radii must be nonzero. Negative values
        /// are allowed.
        /// </param>
        /// <param name="radius1">
        /// The ending radius. At least one radii must be nonzero. Negative values
        /// are allowed.
        /// </param>
        /// <param name="pointsPerTurn">
        /// Number of points to interpolate per turn. Must be greater than 4.
        /// When in doubt, use 12.
        /// </param>
        /// <returns>NurbsCurve on success, null on failure.</returns>
        public static async Task<NurbsCurve> CreateSpiral(Remote<Curve> railCurve, double t0, double t1, Point3d radiusPoint, double pitch, double turnCount, double radius0, double radius1, int pointsPerTurn)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), railCurve, t0, t1, radiusPoint, pitch, turnCount, radius0, radius1, pointsPerTurn);
        }
    }

    public static class NurbsSurfaceCompute
    {
        static string ApiAddress([CallerMemberName] string caller = null)
        {
            return ComputeServer.ApiAddress(typeof(NurbsSurface), caller);
        }

        /// <summary>
        /// Create a bi-cubic SubD friendly surface from a surface.
        /// </summary>
        /// <param name="surface">>Surface to rebuild as a SubD friendly surface.</param>
        /// <returns>A SubD friendly NURBS surface is successful, null otherwise.</returns>
        public static async Task<NurbsSurface> CreateSubDFriendly(Surface surface)
        {
            return await ComputeServer.PostAsync<NurbsSurface>(ApiAddress(), surface);
        }

        /// <summary>
        /// Create a bi-cubic SubD friendly surface from a surface.
        /// </summary>
        /// <param name="surface">>Surface to rebuild as a SubD friendly surface.</param>
        /// <returns>A SubD friendly NURBS surface is successful, null otherwise.</returns>
        public static async Task<NurbsSurface> CreateSubDFriendly(Remote<Surface> surface)
        {
            return await ComputeServer.PostAsync<NurbsSurface>(ApiAddress(), surface);
        }

        /// <summary>
        /// Creates a NURBS surface from a plane and additional parameters.
        /// </summary>
        /// <param name="plane">The plane.</param>
        /// <param name="uInterval">The interval describing the extends of the output surface in the U direction.</param>
        /// <param name="vInterval">The interval describing the extends of the output surface in the V direction.</param>
        /// <param name="uDegree">The degree of the output surface in the U direction.</param>
        /// <param name="vDegree">The degree of the output surface in the V direction.</param>
        /// <param name="uPointCount">The number of control points of the output surface in the U direction.</param>
        /// <param name="vPointCount">The number of control points of the output surface in the V direction.</param>
        /// <returns>A NURBS surface if successful, or null on failure.</returns>
        public static async Task<NurbsSurface> CreateFromPlane(Plane plane, Interval uInterval, Interval vInterval, int uDegree, int vDegree, int uPointCount, int vPointCount)
        {
            return await ComputeServer.PostAsync<NurbsSurface>(ApiAddress(), plane, uInterval, vInterval, uDegree, vDegree, uPointCount, vPointCount);
        }

        /// <summary>
        /// Computes a discrete spline curve on the surface. In other words, computes a sequence 
        /// of points on the surface, each with a corresponding parameter value.
        /// </summary>
        /// <param name="surface">
        /// The surface on which the curve is constructed. The surface should be G1 continuous. 
        /// If the surface is closed in the u or v direction and is G1 at the seam, the
        /// function will construct point sequences that cross over the seam.
        /// </param>
        /// <param name="fixedPoints">Surface points to interpolate given by parameters. These must be distinct.</param>
        /// <param name="tolerance">Relative tolerance used by the solver. When in doubt, use a tolerance of 0.0.</param>
        /// <param name="periodic">When true constructs a smoothly closed curve.</param>
        /// <param name="initCount">Maximum number of points to insert between fixed points on the first level.</param>
        /// <param name="levels">The number of levels (between 1 and 3) to be used in multi-level solver. Use 1 for single level solve.</param>
        /// <returns>
        /// A sequence of surface points, given by surface parameters, if successful.
        /// The number of output points is approximately: 2 ^ (level-1) * initCount * fixedPoints.Count.
        /// </returns>
        /// <remarks>
        /// To create a curve from the output points, use Surface.CreateCurveOnSurface.
        /// </remarks>
        public static async Task<Point2d[]> CreateCurveOnSurfacePoints(Surface surface, IEnumerable<Point2d> fixedPoints, double tolerance, bool periodic, int initCount, int levels)
        {
            return await ComputeServer.PostAsync<Point2d[]>(ApiAddress(), surface, fixedPoints, tolerance, periodic, initCount, levels);
        }

        /// <summary>
        /// Computes a discrete spline curve on the surface. In other words, computes a sequence 
        /// of points on the surface, each with a corresponding parameter value.
        /// </summary>
        /// <param name="surface">
        /// The surface on which the curve is constructed. The surface should be G1 continuous. 
        /// If the surface is closed in the u or v direction and is G1 at the seam, the
        /// function will construct point sequences that cross over the seam.
        /// </param>
        /// <param name="fixedPoints">Surface points to interpolate given by parameters. These must be distinct.</param>
        /// <param name="tolerance">Relative tolerance used by the solver. When in doubt, use a tolerance of 0.0.</param>
        /// <param name="periodic">When true constructs a smoothly closed curve.</param>
        /// <param name="initCount">Maximum number of points to insert between fixed points on the first level.</param>
        /// <param name="levels">The number of levels (between 1 and 3) to be used in multi-level solver. Use 1 for single level solve.</param>
        /// <returns>
        /// A sequence of surface points, given by surface parameters, if successful.
        /// The number of output points is approximately: 2 ^ (level-1) * initCount * fixedPoints.Count.
        /// </returns>
        /// <remarks>
        /// To create a curve from the output points, use Surface.CreateCurveOnSurface.
        /// </remarks>
        public static async Task<Point2d[]> CreateCurveOnSurfacePoints(Remote<Surface> surface, IEnumerable<Point2d> fixedPoints, double tolerance, bool periodic, int initCount, int levels)
        {
            return await ComputeServer.PostAsync<Point2d[]>(ApiAddress(), surface, fixedPoints, tolerance, periodic, initCount, levels);
        }

        /// <summary>
        /// Fit a sequence of 2d points on a surface to make a curve on the surface.
        /// </summary>
        /// <param name="surface">Surface on which to construct curve.</param>
        /// <param name="points">Parameter space coordinates of the points to interpolate.</param>
        /// <param name="tolerance">Curve should be within tolerance of surface and points.</param>
        /// <param name="periodic">When true make a periodic curve.</param>
        /// <returns>A curve interpolating the points if successful, null on error.</returns>
        /// <remarks>
        /// To produce the input points, use Surface.CreateCurveOnSurfacePoints.
        /// </remarks>
        public static async Task<NurbsCurve> CreateCurveOnSurface(Surface surface, IEnumerable<Point2d> points, double tolerance, bool periodic)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), surface, points, tolerance, periodic);
        }

        /// <summary>
        /// Fit a sequence of 2d points on a surface to make a curve on the surface.
        /// </summary>
        /// <param name="surface">Surface on which to construct curve.</param>
        /// <param name="points">Parameter space coordinates of the points to interpolate.</param>
        /// <param name="tolerance">Curve should be within tolerance of surface and points.</param>
        /// <param name="periodic">When true make a periodic curve.</param>
        /// <returns>A curve interpolating the points if successful, null on error.</returns>
        /// <remarks>
        /// To produce the input points, use Surface.CreateCurveOnSurfacePoints.
        /// </remarks>
        public static async Task<NurbsCurve> CreateCurveOnSurface(Remote<Surface> surface, IEnumerable<Point2d> points, double tolerance, bool periodic)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), surface, points, tolerance, periodic);
        }

        /// <summary>
        /// Constructs a NURBS surface from a 2D grid of control points.
        /// </summary>
        /// <param name="points">Control point locations.</param>
        /// <param name="uCount">Number of points in U direction.</param>
        /// <param name="vCount">Number of points in V direction.</param>
        /// <param name="uDegree">Degree of surface in U direction.</param>
        /// <param name="vDegree">Degree of surface in V direction.</param>
        /// <returns>A NurbsSurface on success or null on failure.</returns>
        /// <remarks>uCount multiplied by vCount must equal the number of points supplied.</remarks>
        public static async Task<NurbsSurface> CreateFromPoints(IEnumerable<Point3d> points, int uCount, int vCount, int uDegree, int vDegree)
        {
            return await ComputeServer.PostAsync<NurbsSurface>(ApiAddress(), points, uCount, vCount, uDegree, vDegree);
        }

        /// <summary>
        /// Constructs a NURBS surface from a 2D grid of points.
        /// </summary>
        /// <param name="points">Control point locations.</param>
        /// <param name="uCount">Number of points in U direction.</param>
        /// <param name="vCount">Number of points in V direction.</param>
        /// <param name="uDegree">Degree of surface in U direction.</param>
        /// <param name="vDegree">Degree of surface in V direction.</param>
        /// <param name="uClosed">true if the surface should be closed in the U direction.</param>
        /// <param name="vClosed">true if the surface should be closed in the V direction.</param>
        /// <returns>A NurbsSurface on success or null on failure.</returns>
        /// <remarks>uCount multiplied by vCount must equal the number of points supplied.</remarks>
        public static async Task<NurbsSurface> CreateThroughPoints(IEnumerable<Point3d> points, int uCount, int vCount, int uDegree, int vDegree, bool uClosed, bool vClosed)
        {
            return await ComputeServer.PostAsync<NurbsSurface>(ApiAddress(), points, uCount, vCount, uDegree, vDegree, uClosed, vClosed);
        }

        /// <summary>
        /// Makes a surface from 4 corner points.
        /// <para>This is the same as calling <see cref="CreateFromCorners(Point3d,Point3d,Point3d,Point3d,double)"/> with tolerance 0.</para>
        /// </summary>
        /// <param name="corner1">The first corner.</param>
        /// <param name="corner2">The second corner.</param>
        /// <param name="corner3">The third corner.</param>
        /// <param name="corner4">The fourth corner.</param>
        /// <returns>the resulting surface or null on error.</returns>
        /// <example>
        /// <code source='examples\vbnet\ex_srfpt.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_srfpt.cs' lang='cs'/>
        /// <code source='examples\py\ex_srfpt.py' lang='py'/>
        /// </example>
        public static async Task<NurbsSurface> CreateFromCorners(Point3d corner1, Point3d corner2, Point3d corner3, Point3d corner4)
        {
            return await ComputeServer.PostAsync<NurbsSurface>(ApiAddress(), corner1, corner2, corner3, corner4);
        }

        /// <summary>
        /// Makes a surface from 4 corner points.
        /// </summary>
        /// <param name="corner1">The first corner.</param>
        /// <param name="corner2">The second corner.</param>
        /// <param name="corner3">The third corner.</param>
        /// <param name="corner4">The fourth corner.</param>
        /// <param name="tolerance">Minimum edge length without collapsing to a singularity.</param>
        /// <returns>The resulting surface or null on error.</returns>
        public static async Task<NurbsSurface> CreateFromCorners(Point3d corner1, Point3d corner2, Point3d corner3, Point3d corner4, double tolerance)
        {
            return await ComputeServer.PostAsync<NurbsSurface>(ApiAddress(), corner1, corner2, corner3, corner4, tolerance);
        }

        /// <summary>
        /// Makes a surface from 3 corner points.
        /// </summary>
        /// <param name="corner1">The first corner.</param>
        /// <param name="corner2">The second corner.</param>
        /// <param name="corner3">The third corner.</param>
        /// <returns>The resulting surface or null on error.</returns>
        public static async Task<NurbsSurface> CreateFromCorners(Point3d corner1, Point3d corner2, Point3d corner3)
        {
            return await ComputeServer.PostAsync<NurbsSurface>(ApiAddress(), corner1, corner2, corner3);
        }

        /// <summary>
        /// Constructs a railed Surface-of-Revolution.
        /// </summary>
        /// <param name="profile">Profile curve for revolution.</param>
        /// <param name="rail">Rail curve for revolution.</param>
        /// <param name="axis">Axis of revolution.</param>
        /// <param name="scaleHeight">If true, surface will be locally scaled.</param>
        /// <returns>A NurbsSurface or null on failure.</returns>
        public static async Task<NurbsSurface> CreateRailRevolvedSurface(Curve profile, Curve rail, Line axis, bool scaleHeight)
        {
            return await ComputeServer.PostAsync<NurbsSurface>(ApiAddress(), profile, rail, axis, scaleHeight);
        }

        /// <summary>
        /// Constructs a railed Surface-of-Revolution.
        /// </summary>
        /// <param name="profile">Profile curve for revolution.</param>
        /// <param name="rail">Rail curve for revolution.</param>
        /// <param name="axis">Axis of revolution.</param>
        /// <param name="scaleHeight">If true, surface will be locally scaled.</param>
        /// <returns>A NurbsSurface or null on failure.</returns>
        public static async Task<NurbsSurface> CreateRailRevolvedSurface(Remote<Curve> profile, Remote<Curve> rail, Line axis, bool scaleHeight)
        {
            return await ComputeServer.PostAsync<NurbsSurface>(ApiAddress(), profile, rail, axis, scaleHeight);
        }
    }

    public static class SubDCompute
    {
        static string ApiAddress([CallerMemberName] string caller = null)
        {
            return ComputeServer.ApiAddress(typeof(SubD), caller);
        }

        /// <summary>
        /// Gets Nurbs form of all edges in this SubD, with clamped knots.
        /// NB: Does not update the SubD evaluation cache before getting the edges.
        /// </summary>
        /// <returns>An array of edge curves.</returns>
        /// <seealso cref="SubD.UpdateSurfaceMeshCache(bool)"/>
        public static async Task<Curve[]> DuplicateEdgeCurves(this SubD subd)
        {
            return await ComputeServer.PostAsync<Curve[]>(ApiAddress(), subd);
        }

        /// <summary>
        ///  Creates a SubD sphere made from quad faces.
        /// </summary>
        /// <param name="sphere">Location, size and orientation of the sphere.</param>
        /// <param name="vertexLocation">
        /// If vertexLocation = SubDComponentLocation::ControlNet, 
        /// then the control net points will be on the surface of the sphere.
        /// Otherwise the limit surface points will be on the sphere.
        /// </param>
        /// <param name="quadSubdivisionLevel">
        /// The resulting sphere will have 6*4^subdivision level quads.
        /// (0 for 6 quads, 1 for 24 quads, 2 for 96 quads, ...).
        /// </param>
        /// <returns>
        /// If the input parameters are valid, a SubD quad sphere is returned. Otherwise null is returned.
        /// </returns>
        public static async Task<SubD> CreateQuadSphere(Sphere sphere, SubDComponentLocation vertexLocation, uint quadSubdivisionLevel)
        {
            return await ComputeServer.PostAsync<SubD>(ApiAddress(), sphere, vertexLocation, quadSubdivisionLevel);
        }

        /// <summary>
        /// Creates a SubD sphere made from polar triangle fans and bands of quads.
        /// The result resembles a globe with triangle fans at the poles and the
        /// edges forming latitude parallels and longitude meridians.
        /// </summary>
        /// <param name="sphere">Location, size and orientation of the sphere.</param>
        /// <param name="vertexLocation">
        /// If vertexLocation = SubDComponentLocation::ControlNet, 
        /// then the control net points will be on the surface of the sphere.
        /// Otherwise the limit surface points will be on the sphere.
        /// </param>
        /// <param name="axialFaceCount">
        /// Number of faces along the sphere's meridians. (axialFaceCount &gt;= 2)
        /// For example, if you wanted each face to span 30 degrees of latitude, 
        /// you would pass 6 (=180 degrees/30 degrees) for axialFaceCount.
        /// </param>
        /// <param name="equatorialFaceCount">
        /// Number of faces around the sphere's parallels. (equatorialFaceCount &gt;= 3)
        /// For example, if you wanted each face to span 30 degrees of longitude, 
        /// you would pass 12 (=360 degrees/30 degrees) for equatorialFaceCount.
        /// </param>
        /// <returns>
        /// If the input parameters are valid, a SubD globe sphere is returned. Otherwise null is returned.
        /// </returns>
        public static async Task<SubD> CreateGlobeSphere(Sphere sphere, SubDComponentLocation vertexLocation, uint axialFaceCount, uint equatorialFaceCount)
        {
            return await ComputeServer.PostAsync<SubD>(ApiAddress(), sphere, vertexLocation, axialFaceCount, equatorialFaceCount);
        }

        /// <summary>
        /// Creates a SubD sphere made from triangular faces.
        /// This is a goofy topology for a Catmull-Clark subdivision surface
        /// (all triangles and all vertices have 5 or 6 edges).
        /// You may want to consider using the much behaved result from
        /// CreateSubDQuadSphere() or even the result from CreateSubDGlobeSphere().
        /// </summary>
        /// <param name="sphere">Location, size and orientation of the sphere.</param>
        /// <param name="vertexLocation">
        /// If vertexLocation = SubDComponentLocation::ControlNet, 
        /// then the control net points will be on the surface of the sphere.
        /// Otherwise the limit surface points will be on the sphere.
        /// </param>
        /// <param name="triSubdivisionLevel">
        /// The resulting sphere will have 20*4^subdivision level triangles.
        /// (0 for 20 triangles, 1 for 80 triangles, 2 for 320 triangles, ...).
        /// </param>
        /// <returns>
        /// If the input parameters are valid, a SubD tri sphere is returned. Otherwise null is returned.
        /// </returns>
        public static async Task<SubD> CreateTriSphere(Sphere sphere, SubDComponentLocation vertexLocation, uint triSubdivisionLevel)
        {
            return await ComputeServer.PostAsync<SubD>(ApiAddress(), sphere, vertexLocation, triSubdivisionLevel);
        }

        /// <summary>
        /// Creates a SubD sphere based on an icosohedron (20 triangular faces and 5 valent vertices).
        /// This is a goofy topology for a Catmull-Clark subdivision surface
        /// (all triangles, all vertices have 5 edges).
        /// You may want to consider using the much behaved result from
        /// CreateSubDQuadSphere(sphere, vertexLocation, 1) 
        /// or even the result from CreateSubDGlobeSphere().
        /// </summary>
        /// <param name="sphere">Location, size and orientation of the sphere.</param>
        /// <param name="vertexLocation">
        /// If vertexLocation = SubDComponentLocation::ControlNet, 
        /// then the control net points will be on the surface of the sphere.
        /// Otherwise the limit surface points will be on the sphere.
        /// </param>
        /// <returns>
        /// If the input parameters are valid, a SubD icosahedron is returned. Otherwise null is returned.
        /// </returns>
        public static async Task<SubD> CreateIcosahedron(Sphere sphere, SubDComponentLocation vertexLocation)
        {
            return await ComputeServer.PostAsync<SubD>(ApiAddress(), sphere, vertexLocation);
        }

        /// <summary>
        /// Makes a new SubD with vertices offset at distance in the direction of the control net vertex normals.
        /// Optionally, based on the value of solidify, adds the input SubD and a ribbon of faces along any naked edges.
        /// </summary>
        /// <param name="distance">The distance to offset.</param>
        /// <param name="solidify">true if the output SubD should be turned into a closed SubD.</param>
        /// <returns>A new SubD if successful, or null on failure.</returns>
        public static async Task<SubD> Offset(this SubD subd, double distance, bool solidify)
        {
            return await ComputeServer.PostAsync<SubD>(ApiAddress(), subd, distance, solidify);
        }

        /// <summary>
        /// Creates a SubD lofted through shape curves.
        /// </summary>
        /// <param name="curves">An enumeration of SubD-friendly NURBS curves to loft through.</param>
        /// <param name="closed">Creates a SubD that is closed in the lofting direction. Must have three or more shape curves.</param>
        /// <param name="addCorners">With open curves, adds creased vertices to the SubD at both ends of the first and last curves.</param>
        /// <param name="addCreases">With kinked curves, adds creased edges to the SubD along the kinks.</param>
        /// <param name="divisions">The segment number between adjacent input curves.</param>
        /// <returns>A new SubD if successful, or null on failure.</returns>
        /// <remarks>
        /// Shape curves must be in the proper order and orientation and have point counts for the desired surface.
        /// Shape curves must be either all open or all closed.
        /// </remarks>
        public static async Task<SubD> CreateFromLoft(IEnumerable<NurbsCurve> curves, bool closed, bool addCorners, bool addCreases, int divisions)
        {
            return await ComputeServer.PostAsync<SubD>(ApiAddress(), curves, closed, addCorners, addCreases, divisions);
        }

        /// <summary>
        /// Creates a SubD lofted through shape curves.
        /// </summary>
        /// <param name="curves">An enumeration of SubD-friendly NURBS curves to loft through.</param>
        /// <param name="closed">Creates a SubD that is closed in the lofting direction. Must have three or more shape curves.</param>
        /// <param name="addCorners">With open curves, adds creased vertices to the SubD at both ends of the first and last curves.</param>
        /// <param name="addCreases">With kinked curves, adds creased edges to the SubD along the kinks.</param>
        /// <param name="divisions">The segment number between adjacent input curves.</param>
        /// <returns>A new SubD if successful, or null on failure.</returns>
        /// <remarks>
        /// Shape curves must be in the proper order and orientation and have point counts for the desired surface.
        /// Shape curves must be either all open or all closed.
        /// </remarks>
        public static async Task<SubD> CreateFromLoft(Remote<IEnumerable<NurbsCurve>> curves, bool closed, bool addCorners, bool addCreases, int divisions)
        {
            return await ComputeServer.PostAsync<SubD>(ApiAddress(), curves, closed, addCorners, addCreases, divisions);
        }

        /// <summary>
        /// Fits a SubD through a series of profile curves that define the SubD cross-sections and one curve that defines a SubD edge.
        /// </summary>
        /// <param name="rail1">A SubD-friendly NURBS curve to sweep along.</param>
        /// <param name="shapes">An enumeration of SubD-friendly NURBS curves to sweep through.</param>
        /// <param name="closed">Creates a SubD that is closed in the rail curve direction.</param>
        /// <param name="addCorners">With open curves, adds creased vertices to the SubD at both ends of the first and last curves.</param>
        /// <param name="roadlikeFrame">
        /// Determines how sweep frame rotations are calculated.
        /// If false (Freeform), frame are propagated based on a reference direction taken from the rail curve curvature direction.
        /// If true (Roadlike), frame rotations are calculated based on a vector supplied in "roadlikeNormal" and the world coordinate system.
        /// </param>
        /// <param name="roadlikeNormal">
        /// If roadlikeFrame = true, provide 3D vector used to calculate the frame rotations for sweep shapes.
        /// If roadlikeFrame = false, then pass <see cref=" Vector3d.Unset"/>.
        /// </param>
        /// <returns>A new SubD if successful, or null on failure.</returns>
        /// <remarks>
        /// Shape curves must be in the proper order and orientation.
        /// Shape curves must have the same point counts and rail curves must have the same point counts.
        /// Shape curves will relocated to the nearest pair of Greville points on the rails.
        /// Shape curves will be made at each pair of rail edit points where there isn't an input shape.
        /// </remarks>
        public static async Task<SubD> CreateFromSweep(NurbsCurve rail1, IEnumerable<NurbsCurve> shapes, bool closed, bool addCorners, bool roadlikeFrame, Vector3d roadlikeNormal)
        {
            return await ComputeServer.PostAsync<SubD>(ApiAddress(), rail1, shapes, closed, addCorners, roadlikeFrame, roadlikeNormal);
        }

        /// <summary>
        /// Fits a SubD through a series of profile curves that define the SubD cross-sections and one curve that defines a SubD edge.
        /// </summary>
        /// <param name="rail1">A SubD-friendly NURBS curve to sweep along.</param>
        /// <param name="shapes">An enumeration of SubD-friendly NURBS curves to sweep through.</param>
        /// <param name="closed">Creates a SubD that is closed in the rail curve direction.</param>
        /// <param name="addCorners">With open curves, adds creased vertices to the SubD at both ends of the first and last curves.</param>
        /// <param name="roadlikeFrame">
        /// Determines how sweep frame rotations are calculated.
        /// If false (Freeform), frame are propagated based on a reference direction taken from the rail curve curvature direction.
        /// If true (Roadlike), frame rotations are calculated based on a vector supplied in "roadlikeNormal" and the world coordinate system.
        /// </param>
        /// <param name="roadlikeNormal">
        /// If roadlikeFrame = true, provide 3D vector used to calculate the frame rotations for sweep shapes.
        /// If roadlikeFrame = false, then pass <see cref=" Vector3d.Unset"/>.
        /// </param>
        /// <returns>A new SubD if successful, or null on failure.</returns>
        /// <remarks>
        /// Shape curves must be in the proper order and orientation.
        /// Shape curves must have the same point counts and rail curves must have the same point counts.
        /// Shape curves will relocated to the nearest pair of Greville points on the rails.
        /// Shape curves will be made at each pair of rail edit points where there isn't an input shape.
        /// </remarks>
        public static async Task<SubD> CreateFromSweep(Remote<NurbsCurve> rail1, Remote<IEnumerable<NurbsCurve>> shapes, bool closed, bool addCorners, bool roadlikeFrame, Vector3d roadlikeNormal)
        {
            return await ComputeServer.PostAsync<SubD>(ApiAddress(), rail1, shapes, closed, addCorners, roadlikeFrame, roadlikeNormal);
        }

        /// <summary>
        /// Fits a SubD through a series of profile curves that define the SubD cross-sections and two curves that defines SubD edges.
        /// </summary>
        /// <param name="rail1">The first SubD-friendly NURBS curve to sweep along.</param>
        /// <param name="rail2">The second SubD-friendly NURBS curve to sweep along.</param>
        /// <param name="shapes">An enumeration of SubD-friendly NURBS curves to sweep through.</param>
        /// <param name="closed">Creates a SubD that is closed in the rail curve direction.</param>
        /// <param name="addCorners">With open curves, adds creased vertices to the SubD at both ends of the first and last curves.</param>
        /// <returns>A new SubD if successful, or null on failure.</returns>
        /// <remarks>
        /// Shape curves must be in the proper order and orientation.
        /// Shape curves must have the same point counts and rail curves must have the same point counts.
        /// Shape curves will relocated to the nearest pair of Greville points on the rails.
        /// Shape curves will be made at each pair of rail edit points where there isn't an input shape.
        /// </remarks>
        public static async Task<SubD> CreateFromSweep(NurbsCurve rail1, NurbsCurve rail2, IEnumerable<NurbsCurve> shapes, bool closed, bool addCorners)
        {
            return await ComputeServer.PostAsync<SubD>(ApiAddress(), rail1, rail2, shapes, closed, addCorners);
        }

        /// <summary>
        /// Fits a SubD through a series of profile curves that define the SubD cross-sections and two curves that defines SubD edges.
        /// </summary>
        /// <param name="rail1">The first SubD-friendly NURBS curve to sweep along.</param>
        /// <param name="rail2">The second SubD-friendly NURBS curve to sweep along.</param>
        /// <param name="shapes">An enumeration of SubD-friendly NURBS curves to sweep through.</param>
        /// <param name="closed">Creates a SubD that is closed in the rail curve direction.</param>
        /// <param name="addCorners">With open curves, adds creased vertices to the SubD at both ends of the first and last curves.</param>
        /// <returns>A new SubD if successful, or null on failure.</returns>
        /// <remarks>
        /// Shape curves must be in the proper order and orientation.
        /// Shape curves must have the same point counts and rail curves must have the same point counts.
        /// Shape curves will relocated to the nearest pair of Greville points on the rails.
        /// Shape curves will be made at each pair of rail edit points where there isn't an input shape.
        /// </remarks>
        public static async Task<SubD> CreateFromSweep(Remote<NurbsCurve> rail1, Remote<NurbsCurve> rail2, Remote<IEnumerable<NurbsCurve>> shapes, bool closed, bool addCorners)
        {
            return await ComputeServer.PostAsync<SubD>(ApiAddress(), rail1, rail2, shapes, closed, addCorners);
        }
    }

    public static class SurfaceCompute
    {
        static string ApiAddress([CallerMemberName] string caller = null)
        {
            return ComputeServer.ApiAddress(typeof(Surface), caller);
        }

        /// <summary>
        /// Create tween surfaces that gradually transition between two bounding surfaces using point sampling.
        /// </summary>
        /// <param name="surface0">The first, or starting, surface.</param>
        /// <param name="surface1">The second, or ending, curve.</param>
        /// <param name="numSurfaces">Number of tween surfaces to create.</param>
        /// <param name="numSamples">Number of sample points along input surfaces.</param>
        /// <param name="tolerance">The tolerance. When in doubt, use the document's model absolute tolerance.</param>
        /// <returns>>An array of tween surfaces if successful, an empty array on error.</returns>
        public static async Task<Surface[]> CreateTweenSurfacesWithSampling(Surface surface0, Surface surface1, int numSurfaces, int numSamples, double tolerance)
        {
            return await ComputeServer.PostAsync<Surface[]>(ApiAddress(), surface0, surface1, numSurfaces, numSamples, tolerance);
        }

        /// <summary>
        /// Create tween surfaces that gradually transition between two bounding surfaces using point sampling.
        /// </summary>
        /// <param name="surface0">The first, or starting, surface.</param>
        /// <param name="surface1">The second, or ending, curve.</param>
        /// <param name="numSurfaces">Number of tween surfaces to create.</param>
        /// <param name="numSamples">Number of sample points along input surfaces.</param>
        /// <param name="tolerance">The tolerance. When in doubt, use the document's model absolute tolerance.</param>
        /// <returns>>An array of tween surfaces if successful, an empty array on error.</returns>
        public static async Task<Surface[]> CreateTweenSurfacesWithSampling(Remote<Surface> surface0, Remote<Surface> surface1, int numSurfaces, int numSamples, double tolerance)
        {
            return await ComputeServer.PostAsync<Surface[]>(ApiAddress(), surface0, surface1, numSurfaces, numSamples, tolerance);
        }

        /// <summary>
        /// Constructs a rolling ball fillet between two surfaces.
        /// </summary>
        /// <param name="surfaceA">A first surface.</param>
        /// <param name="surfaceB">A second surface.</param>
        /// <param name="radius">A radius value.</param>
        /// <param name="tolerance">A tolerance value.</param>
        /// <returns>A new array of rolling ball fillet surfaces; this array can be empty on failure.</returns>
        /// <exception cref="ArgumentNullException">If surfaceA or surfaceB are null.</exception>
        public static async Task<Surface[]> CreateRollingBallFillet(Surface surfaceA, Surface surfaceB, double radius, double tolerance)
        {
            return await ComputeServer.PostAsync<Surface[]>(ApiAddress(), surfaceA, surfaceB, radius, tolerance);
        }

        /// <summary>
        /// Constructs a rolling ball fillet between two surfaces.
        /// </summary>
        /// <param name="surfaceA">A first surface.</param>
        /// <param name="surfaceB">A second surface.</param>
        /// <param name="radius">A radius value.</param>
        /// <param name="tolerance">A tolerance value.</param>
        /// <returns>A new array of rolling ball fillet surfaces; this array can be empty on failure.</returns>
        /// <exception cref="ArgumentNullException">If surfaceA or surfaceB are null.</exception>
        public static async Task<Surface[]> CreateRollingBallFillet(Remote<Surface> surfaceA, Remote<Surface> surfaceB, double radius, double tolerance)
        {
            return await ComputeServer.PostAsync<Surface[]>(ApiAddress(), surfaceA, surfaceB, radius, tolerance);
        }

        /// <summary>
        /// Constructs a rolling ball fillet between two surfaces.
        /// </summary>
        /// <param name="surfaceA">A first surface.</param>
        /// <param name="flipA">A value that indicates whether A should be used in flipped mode.</param>
        /// <param name="surfaceB">A second surface.</param>
        /// <param name="flipB">A value that indicates whether B should be used in flipped mode.</param>
        /// <param name="radius">A radius value.</param>
        /// <param name="tolerance">A tolerance value.</param>
        /// <returns>A new array of rolling ball fillet surfaces; this array can be empty on failure.</returns>
        /// <exception cref="ArgumentNullException">If surfaceA or surfaceB are null.</exception>
        public static async Task<Surface[]> CreateRollingBallFillet(Surface surfaceA, bool flipA, Surface surfaceB, bool flipB, double radius, double tolerance)
        {
            return await ComputeServer.PostAsync<Surface[]>(ApiAddress(), surfaceA, flipA, surfaceB, flipB, radius, tolerance);
        }

        /// <summary>
        /// Constructs a rolling ball fillet between two surfaces.
        /// </summary>
        /// <param name="surfaceA">A first surface.</param>
        /// <param name="flipA">A value that indicates whether A should be used in flipped mode.</param>
        /// <param name="surfaceB">A second surface.</param>
        /// <param name="flipB">A value that indicates whether B should be used in flipped mode.</param>
        /// <param name="radius">A radius value.</param>
        /// <param name="tolerance">A tolerance value.</param>
        /// <returns>A new array of rolling ball fillet surfaces; this array can be empty on failure.</returns>
        /// <exception cref="ArgumentNullException">If surfaceA or surfaceB are null.</exception>
        public static async Task<Surface[]> CreateRollingBallFillet(Remote<Surface> surfaceA, bool flipA, Remote<Surface> surfaceB, bool flipB, double radius, double tolerance)
        {
            return await ComputeServer.PostAsync<Surface[]>(ApiAddress(), surfaceA, flipA, surfaceB, flipB, radius, tolerance);
        }

        /// <summary>
        /// Constructs a rolling ball fillet between two surfaces.
        /// </summary>
        /// <param name="surfaceA">A first surface.</param>
        /// <param name="uvA">A point in the parameter space of FaceA near where the fillet is expected to hit the surface.</param>
        /// <param name="surfaceB">A second surface.</param>
        /// <param name="uvB">A point in the parameter space of FaceB near where the fillet is expected to hit the surface.</param>
        /// <param name="radius">A radius value.</param>
        /// <param name="tolerance">A tolerance value used for approximating and intersecting offset surfaces.</param>
        /// <returns>A new array of rolling ball fillet surfaces; this array can be empty on failure.</returns>
        /// <exception cref="ArgumentNullException">If surfaceA or surfaceB are null.</exception>
        public static async Task<Surface[]> CreateRollingBallFillet(Surface surfaceA, Point2d uvA, Surface surfaceB, Point2d uvB, double radius, double tolerance)
        {
            return await ComputeServer.PostAsync<Surface[]>(ApiAddress(), surfaceA, uvA, surfaceB, uvB, radius, tolerance);
        }

        /// <summary>
        /// Constructs a rolling ball fillet between two surfaces.
        /// </summary>
        /// <param name="surfaceA">A first surface.</param>
        /// <param name="uvA">A point in the parameter space of FaceA near where the fillet is expected to hit the surface.</param>
        /// <param name="surfaceB">A second surface.</param>
        /// <param name="uvB">A point in the parameter space of FaceB near where the fillet is expected to hit the surface.</param>
        /// <param name="radius">A radius value.</param>
        /// <param name="tolerance">A tolerance value used for approximating and intersecting offset surfaces.</param>
        /// <returns>A new array of rolling ball fillet surfaces; this array can be empty on failure.</returns>
        /// <exception cref="ArgumentNullException">If surfaceA or surfaceB are null.</exception>
        public static async Task<Surface[]> CreateRollingBallFillet(Remote<Surface> surfaceA, Point2d uvA, Remote<Surface> surfaceB, Point2d uvB, double radius, double tolerance)
        {
            return await ComputeServer.PostAsync<Surface[]>(ApiAddress(), surfaceA, uvA, surfaceB, uvB, radius, tolerance);
        }

        /// <summary>
        /// Constructs a surface by extruding a curve along a vector.
        /// </summary>
        /// <param name="profile">Profile curve to extrude.</param>
        /// <param name="direction">Direction and length of extrusion.</param>
        /// <returns>A surface on success or null on failure.</returns>
        public static async Task<Surface> CreateExtrusion(Curve profile, Vector3d direction)
        {
            return await ComputeServer.PostAsync<Surface>(ApiAddress(), profile, direction);
        }

        /// <summary>
        /// Constructs a surface by extruding a curve along a vector.
        /// </summary>
        /// <param name="profile">Profile curve to extrude.</param>
        /// <param name="direction">Direction and length of extrusion.</param>
        /// <returns>A surface on success or null on failure.</returns>
        public static async Task<Surface> CreateExtrusion(Remote<Curve> profile, Vector3d direction)
        {
            return await ComputeServer.PostAsync<Surface>(ApiAddress(), profile, direction);
        }

        /// <summary>
        /// Constructs a surface by extruding a curve to a point.
        /// </summary>
        /// <param name="profile">Profile curve to extrude.</param>
        /// <param name="apexPoint">Apex point of extrusion.</param>
        /// <returns>A Surface on success or null on failure.</returns>
        public static async Task<Surface> CreateExtrusionToPoint(Curve profile, Point3d apexPoint)
        {
            return await ComputeServer.PostAsync<Surface>(ApiAddress(), profile, apexPoint);
        }

        /// <summary>
        /// Constructs a surface by extruding a curve to a point.
        /// </summary>
        /// <param name="profile">Profile curve to extrude.</param>
        /// <param name="apexPoint">Apex point of extrusion.</param>
        /// <returns>A Surface on success or null on failure.</returns>
        public static async Task<Surface> CreateExtrusionToPoint(Remote<Curve> profile, Point3d apexPoint)
        {
            return await ComputeServer.PostAsync<Surface>(ApiAddress(), profile, apexPoint);
        }

        /// <summary>
        /// Constructs a periodic surface from a base surface and a direction.
        /// </summary>
        /// <param name="surface">The surface to make periodic.</param>
        /// <param name="direction">The direction to make periodic, either 0 = U, or 1 = V.</param>
        /// <returns>A Surface on success or null on failure.</returns>
        public static async Task<Surface> CreatePeriodicSurface(Surface surface, int direction)
        {
            return await ComputeServer.PostAsync<Surface>(ApiAddress(), surface, direction);
        }

        /// <summary>
        /// Constructs a periodic surface from a base surface and a direction.
        /// </summary>
        /// <param name="surface">The surface to make periodic.</param>
        /// <param name="direction">The direction to make periodic, either 0 = U, or 1 = V.</param>
        /// <returns>A Surface on success or null on failure.</returns>
        public static async Task<Surface> CreatePeriodicSurface(Remote<Surface> surface, int direction)
        {
            return await ComputeServer.PostAsync<Surface>(ApiAddress(), surface, direction);
        }

        /// <summary>
        /// Constructs a periodic surface from a base surface and a direction.
        /// </summary>
        /// <param name="surface">The surface to make periodic.</param>
        /// <param name="direction">The direction to make periodic, either 0 = U, or 1 = V.</param>
        /// <param name="bSmooth">
        /// Controls kink removal. If true, smooths any kinks in the surface and moves control points
        /// to make a smooth surface. If false, control point locations are not changed or changed minimally
        /// (only one point may move) and only the knot vector is altered.
        /// </param>
        /// <returns>A periodic surface if successful, null on failure.</returns>
        public static async Task<Surface> CreatePeriodicSurface(Surface surface, int direction, bool bSmooth)
        {
            return await ComputeServer.PostAsync<Surface>(ApiAddress(), surface, direction, bSmooth);
        }

        /// <summary>
        /// Constructs a periodic surface from a base surface and a direction.
        /// </summary>
        /// <param name="surface">The surface to make periodic.</param>
        /// <param name="direction">The direction to make periodic, either 0 = U, or 1 = V.</param>
        /// <param name="bSmooth">
        /// Controls kink removal. If true, smooths any kinks in the surface and moves control points
        /// to make a smooth surface. If false, control point locations are not changed or changed minimally
        /// (only one point may move) and only the knot vector is altered.
        /// </param>
        /// <returns>A periodic surface if successful, null on failure.</returns>
        public static async Task<Surface> CreatePeriodicSurface(Remote<Surface> surface, int direction, bool bSmooth)
        {
            return await ComputeServer.PostAsync<Surface>(ApiAddress(), surface, direction, bSmooth);
        }

        /// <summary>
        /// Creates a soft edited surface from an existing surface using a smooth field of influence.
        /// </summary>
        /// <param name="surface">The surface to soft edit.</param>
        /// <param name="uv">
        /// A point in the parameter space to move from. This location on the surface is moved, 
        /// and the move is smoothly tapered off with increasing distance along the surface from
        /// this parameter.
        /// </param>
        /// <param name="delta">The direction and magnitude, or maximum distance, of the move.</param>
        /// <param name="uLength">
        /// The distance along the surface's u-direction from the editing point over which the
        /// strength of the editing falls off smoothly.
        /// </param>
        /// <param name="vLength">
        /// The distance along the surface's v-direction from the editing point over which the
        /// strength of the editing falls off smoothly.
        /// </param>
        /// <param name="tolerance">The active document's model absolute tolerance.</param>
        /// <param name="fixEnds">Keeps edge locations fixed.</param>
        /// <returns>The soft edited surface if successful. null on failure.</returns>
        public static async Task<Surface> CreateSoftEditSurface(Surface surface, Point2d uv, Vector3d delta, double uLength, double vLength, double tolerance, bool fixEnds)
        {
            return await ComputeServer.PostAsync<Surface>(ApiAddress(), surface, uv, delta, uLength, vLength, tolerance, fixEnds);
        }

        /// <summary>
        /// Creates a soft edited surface from an existing surface using a smooth field of influence.
        /// </summary>
        /// <param name="surface">The surface to soft edit.</param>
        /// <param name="uv">
        /// A point in the parameter space to move from. This location on the surface is moved, 
        /// and the move is smoothly tapered off with increasing distance along the surface from
        /// this parameter.
        /// </param>
        /// <param name="delta">The direction and magnitude, or maximum distance, of the move.</param>
        /// <param name="uLength">
        /// The distance along the surface's u-direction from the editing point over which the
        /// strength of the editing falls off smoothly.
        /// </param>
        /// <param name="vLength">
        /// The distance along the surface's v-direction from the editing point over which the
        /// strength of the editing falls off smoothly.
        /// </param>
        /// <param name="tolerance">The active document's model absolute tolerance.</param>
        /// <param name="fixEnds">Keeps edge locations fixed.</param>
        /// <returns>The soft edited surface if successful. null on failure.</returns>
        public static async Task<Surface> CreateSoftEditSurface(Remote<Surface> surface, Point2d uv, Vector3d delta, double uLength, double vLength, double tolerance, bool fixEnds)
        {
            return await ComputeServer.PostAsync<Surface>(ApiAddress(), surface, uv, delta, uLength, vLength, tolerance, fixEnds);
        }

        /// <summary>
        /// Gets the side that is closest, in terms of 3D-distance, to a U and V parameter.
        /// </summary>
        /// <param name="u">A u parameter.</param>
        /// <param name="v">A v parameter.</param>
        /// <returns>A side.</returns>
        public static async Task<IsoStatus> ClosestSide(this Surface surface, double u, double v)
        {
            return await ComputeServer.PostAsync<IsoStatus>(ApiAddress(), surface, u, v);
        }

        /// <summary>
        /// Gets the side that is closest, in terms of 3D-distance, to a U and V parameter.
        /// </summary>
        /// <param name="u">A u parameter.</param>
        /// <param name="v">A v parameter.</param>
        /// <returns>A side.</returns>
        public static async Task<IsoStatus> ClosestSide(Remote<Surface> surface, double u, double v)
        {
            return await ComputeServer.PostAsync<IsoStatus>(ApiAddress(), surface, u, v);
        }

        /// <summary>
        /// Extends an untrimmed surface along one edge.
        /// </summary>
        /// <param name="edge">
        /// Edge to extend.  Must be North, South, East, or West.
        /// </param>
        /// <param name="extensionLength">distance to extend.</param>
        /// <param name="smooth">
        /// true for smooth (C-infinity) extension. 
        /// false for a C1- ruled extension.
        /// </param>
        /// <returns>New extended surface on success.</returns>
        public static async Task<Surface> Extend(this Surface surface, IsoStatus edge, double extensionLength, bool smooth)
        {
            return await ComputeServer.PostAsync<Surface>(ApiAddress(), surface, edge, extensionLength, smooth);
        }

        /// <summary>
        /// Extends an untrimmed surface along one edge.
        /// </summary>
        /// <param name="edge">
        /// Edge to extend.  Must be North, South, East, or West.
        /// </param>
        /// <param name="extensionLength">distance to extend.</param>
        /// <param name="smooth">
        /// true for smooth (C-infinity) extension. 
        /// false for a C1- ruled extension.
        /// </param>
        /// <returns>New extended surface on success.</returns>
        public static async Task<Surface> Extend(Remote<Surface> surface, IsoStatus edge, double extensionLength, bool smooth)
        {
            return await ComputeServer.PostAsync<Surface>(ApiAddress(), surface, edge, extensionLength, smooth);
        }

        /// <summary>
        /// Rebuilds an existing surface to a given degree and point count.
        /// </summary>
        /// <param name="uDegree">the output surface u degree.</param>
        /// <param name="vDegree">the output surface u degree.</param>
        /// <param name="uPointCount">
        /// The number of points in the output surface u direction. Must be bigger
        /// than uDegree (maximum value is 1000)
        /// </param>
        /// <param name="vPointCount">
        /// The number of points in the output surface v direction. Must be bigger
        /// than vDegree (maximum value is 1000)
        /// </param>
        /// <returns>new rebuilt surface on success. null on failure.</returns>
        public static async Task<NurbsSurface> Rebuild(this Surface surface, int uDegree, int vDegree, int uPointCount, int vPointCount)
        {
            return await ComputeServer.PostAsync<NurbsSurface>(ApiAddress(), surface, uDegree, vDegree, uPointCount, vPointCount);
        }

        /// <summary>
        /// Rebuilds an existing surface to a given degree and point count.
        /// </summary>
        /// <param name="uDegree">the output surface u degree.</param>
        /// <param name="vDegree">the output surface u degree.</param>
        /// <param name="uPointCount">
        /// The number of points in the output surface u direction. Must be bigger
        /// than uDegree (maximum value is 1000)
        /// </param>
        /// <param name="vPointCount">
        /// The number of points in the output surface v direction. Must be bigger
        /// than vDegree (maximum value is 1000)
        /// </param>
        /// <returns>new rebuilt surface on success. null on failure.</returns>
        public static async Task<NurbsSurface> Rebuild(Remote<Surface> surface, int uDegree, int vDegree, int uPointCount, int vPointCount)
        {
            return await ComputeServer.PostAsync<NurbsSurface>(ApiAddress(), surface, uDegree, vDegree, uPointCount, vPointCount);
        }

        /// <summary>
        /// Rebuilds an existing surface with a new surface to a given point count in either the u or v directions independently.
        /// </summary>
        /// <param name="direction">The direction (0 = U, 1 = V).</param>
        /// <param name="pointCount">The number of points in the output surface in the "direction" direction.</param>
        /// <param name="loftType">The loft type</param>
        /// <param name="refitTolerance">The refit tolerance. When in doubt, use the document's model absolute tolerance.</param>
        /// <returns>new rebuilt surface on success. null on failure.</returns>
        public static async Task<NurbsSurface> RebuildOneDirection(this Surface surface, int direction, int pointCount, LoftType loftType, double refitTolerance)
        {
            return await ComputeServer.PostAsync<NurbsSurface>(ApiAddress(), surface, direction, pointCount, loftType, refitTolerance);
        }

        /// <summary>
        /// Rebuilds an existing surface with a new surface to a given point count in either the u or v directions independently.
        /// </summary>
        /// <param name="direction">The direction (0 = U, 1 = V).</param>
        /// <param name="pointCount">The number of points in the output surface in the "direction" direction.</param>
        /// <param name="loftType">The loft type</param>
        /// <param name="refitTolerance">The refit tolerance. When in doubt, use the document's model absolute tolerance.</param>
        /// <returns>new rebuilt surface on success. null on failure.</returns>
        public static async Task<NurbsSurface> RebuildOneDirection(Remote<Surface> surface, int direction, int pointCount, LoftType loftType, double refitTolerance)
        {
            return await ComputeServer.PostAsync<NurbsSurface>(ApiAddress(), surface, direction, pointCount, loftType, refitTolerance);
        }

        /// <summary>
        /// Constructs a new surface which is offset from the current surface.
        /// </summary>
        /// <param name="distance">Distance (along surface normal) to offset.</param>
        /// <param name="tolerance">Offset accuracy.</param>
        /// <returns>The offset surface or null on failure.</returns>
        public static async Task<Surface> Offset(this Surface surface, double distance, double tolerance)
        {
            return await ComputeServer.PostAsync<Surface>(ApiAddress(), surface, distance, tolerance);
        }

        /// <summary>
        /// Constructs a new surface which is offset from the current surface.
        /// </summary>
        /// <param name="distance">Distance (along surface normal) to offset.</param>
        /// <param name="tolerance">Offset accuracy.</param>
        /// <returns>The offset surface or null on failure.</returns>
        public static async Task<Surface> Offset(Remote<Surface> surface, double distance, double tolerance)
        {
            return await ComputeServer.PostAsync<Surface>(ApiAddress(), surface, distance, tolerance);
        }

        /// <summary>Fits a new surface through an existing surface.</summary>
        /// <param name="uDegree">the output surface U degree. Must be bigger than 1.</param>
        /// <param name="vDegree">the output surface V degree. Must be bigger than 1.</param>
        /// <param name="fitTolerance">The fitting tolerance.</param>
        /// <returns>A surface, or null on error.</returns>
        public static async Task<Surface> Fit(this Surface surface, int uDegree, int vDegree, double fitTolerance)
        {
            return await ComputeServer.PostAsync<Surface>(ApiAddress(), surface, uDegree, vDegree, fitTolerance);
        }

        /// <summary>Fits a new surface through an existing surface.</summary>
        /// <param name="uDegree">the output surface U degree. Must be bigger than 1.</param>
        /// <param name="vDegree">the output surface V degree. Must be bigger than 1.</param>
        /// <param name="fitTolerance">The fitting tolerance.</param>
        /// <returns>A surface, or null on error.</returns>
        public static async Task<Surface> Fit(Remote<Surface> surface, int uDegree, int vDegree, double fitTolerance)
        {
            return await ComputeServer.PostAsync<Surface>(ApiAddress(), surface, uDegree, vDegree, fitTolerance);
        }

        /// <summary>
        /// Returns a curve that interpolates points on a surface. The interpolant lies on the surface.
        /// </summary>
        /// <param name="points">List of at least two UV parameter locations on the surface.</param>
        /// <param name="tolerance">Tolerance used for the fit of the push-up curve. Generally, the resulting interpolating curve will be within tolerance of the surface.</param>
        /// <returns>A new NURBS curve if successful, or null on error.</returns>
        public static async Task<NurbsCurve> InterpolatedCurveOnSurfaceUV(this Surface surface, System.Collections.Generic.IEnumerable<Point2d> points, double tolerance)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), surface, points, tolerance);
        }

        /// <summary>
        /// Returns a curve that interpolates points on a surface. The interpolant lies on the surface.
        /// </summary>
        /// <param name="points">List of at least two UV parameter locations on the surface.</param>
        /// <param name="tolerance">Tolerance used for the fit of the push-up curve. Generally, the resulting interpolating curve will be within tolerance of the surface.</param>
        /// <returns>A new NURBS curve if successful, or null on error.</returns>
        public static async Task<NurbsCurve> InterpolatedCurveOnSurfaceUV(Remote<Surface> surface, System.Collections.Generic.IEnumerable<Point2d> points, double tolerance)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), surface, points, tolerance);
        }

        /// <summary>
        /// Returns a curve that interpolates points on a surface. The interpolant lies on the surface.
        /// </summary>
        /// <param name="points">List of at least two UV parameter locations on the surface.</param>
        /// <param name="tolerance">Tolerance used for the fit of the push-up curve. Generally, the resulting interpolating curve will be within tolerance of the surface.</param>
        /// <param name="closed">If false, the interpolating curve is not closed. If true, the interpolating curve is closed, and the last point and first point should generally not be equal.</param>
        /// <param name="closedSurfaceHandling">
        /// If 0, all points must be in the rectangular domain of the surface. If the surface is closed in some direction, 
        /// then this routine will interpret each point and place it at an appropriate location in the covering space. 
        /// This is the simplest option and should give good results. 
        /// If 1, then more options for more control of handling curves going across seams are available.
        /// If the surface is closed in some direction, then the points are taken as points in the covering space. 
        /// Example, if srf.IsClosed(0)=true and srf.IsClosed(1)=false and srf.Domain(0)=srf.Domain(1)=Interval(0,1) 
        /// then if closedSurfaceHandling=1 a point(u, v) in points can have any value for the u coordinate, but must have 0&lt;=v&lt;=1.  
        /// In particular, if points = { (0.0,0.5), (2.0,0.5) } then the interpolating curve will wrap around the surface two times in the closed direction before ending at start of the curve.
        /// If closed=true the last point should equal the first point plus an integer multiple of the period on a closed direction.
        /// </param>
        /// <returns>A new NURBS curve if successful, or null on error.</returns>
        public static async Task<NurbsCurve> InterpolatedCurveOnSurfaceUV(this Surface surface, System.Collections.Generic.IEnumerable<Point2d> points, double tolerance, bool closed, int closedSurfaceHandling)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), surface, points, tolerance, closed, closedSurfaceHandling);
        }

        /// <summary>
        /// Returns a curve that interpolates points on a surface. The interpolant lies on the surface.
        /// </summary>
        /// <param name="points">List of at least two UV parameter locations on the surface.</param>
        /// <param name="tolerance">Tolerance used for the fit of the push-up curve. Generally, the resulting interpolating curve will be within tolerance of the surface.</param>
        /// <param name="closed">If false, the interpolating curve is not closed. If true, the interpolating curve is closed, and the last point and first point should generally not be equal.</param>
        /// <param name="closedSurfaceHandling">
        /// If 0, all points must be in the rectangular domain of the surface. If the surface is closed in some direction, 
        /// then this routine will interpret each point and place it at an appropriate location in the covering space. 
        /// This is the simplest option and should give good results. 
        /// If 1, then more options for more control of handling curves going across seams are available.
        /// If the surface is closed in some direction, then the points are taken as points in the covering space. 
        /// Example, if srf.IsClosed(0)=true and srf.IsClosed(1)=false and srf.Domain(0)=srf.Domain(1)=Interval(0,1) 
        /// then if closedSurfaceHandling=1 a point(u, v) in points can have any value for the u coordinate, but must have 0&lt;=v&lt;=1.  
        /// In particular, if points = { (0.0,0.5), (2.0,0.5) } then the interpolating curve will wrap around the surface two times in the closed direction before ending at start of the curve.
        /// If closed=true the last point should equal the first point plus an integer multiple of the period on a closed direction.
        /// </param>
        /// <returns>A new NURBS curve if successful, or null on error.</returns>
        public static async Task<NurbsCurve> InterpolatedCurveOnSurfaceUV(Remote<Surface> surface, System.Collections.Generic.IEnumerable<Point2d> points, double tolerance, bool closed, int closedSurfaceHandling)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), surface, points, tolerance, closed, closedSurfaceHandling);
        }

        /// <summary>
        /// Constructs an interpolated curve on a surface, using 3D points.
        /// </summary>
        /// <param name="points">A list, an array or any enumerable set of points.</param>
        /// <param name="tolerance">A tolerance value.</param>
        /// <returns>A new NURBS curve, or null on error.</returns>
        public static async Task<NurbsCurve> InterpolatedCurveOnSurface(this Surface surface, System.Collections.Generic.IEnumerable<Point3d> points, double tolerance)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), surface, points, tolerance);
        }

        /// <summary>
        /// Constructs an interpolated curve on a surface, using 3D points.
        /// </summary>
        /// <param name="points">A list, an array or any enumerable set of points.</param>
        /// <param name="tolerance">A tolerance value.</param>
        /// <returns>A new NURBS curve, or null on error.</returns>
        public static async Task<NurbsCurve> InterpolatedCurveOnSurface(Remote<Surface> surface, System.Collections.Generic.IEnumerable<Point3d> points, double tolerance)
        {
            return await ComputeServer.PostAsync<NurbsCurve>(ApiAddress(), surface, points, tolerance);
        }

        /// <summary>
        /// Constructs a geodesic between 2 points, used by ShortPath command in Rhino.
        /// </summary>
        /// <param name="start">start point of curve in parameter space. Points must be distinct in the domain of the surface.</param>
        /// <param name="end">end point of curve in parameter space. Points must be distinct in the domain of the surface.</param>
        /// <param name="tolerance">tolerance used in fitting discrete solution.</param>
        /// <returns>a geodesic curve on the surface on success. null on failure.</returns>
        public static async Task<Curve> ShortPath(this Surface surface, Point2d start, Point2d end, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), surface, start, end, tolerance);
        }

        /// <summary>
        /// Constructs a geodesic between 2 points, used by ShortPath command in Rhino.
        /// </summary>
        /// <param name="start">start point of curve in parameter space. Points must be distinct in the domain of the surface.</param>
        /// <param name="end">end point of curve in parameter space. Points must be distinct in the domain of the surface.</param>
        /// <param name="tolerance">tolerance used in fitting discrete solution.</param>
        /// <returns>a geodesic curve on the surface on success. null on failure.</returns>
        public static async Task<Curve> ShortPath(Remote<Surface> surface, Point2d start, Point2d end, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), surface, start, end, tolerance);
        }

        /// <summary>
        /// Computes a 3d curve that is the composite of a 2d curve and the surface map.
        /// </summary>
        /// <param name="curve2d">a 2d curve whose image is in the surface's domain.</param>
        /// <param name="tolerance">
        /// the maximum acceptable distance from the returned 3d curve to the image of curve_2d on the surface.
        /// </param>
        /// <param name="curve2dSubdomain">The curve interval (a sub-domain of the original curve) to use.</param>
        /// <returns>3d curve.</returns>
        public static async Task<Curve> Pushup(this Surface surface, Curve curve2d, double tolerance, Interval curve2dSubdomain)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), surface, curve2d, tolerance, curve2dSubdomain);
        }

        /// <summary>
        /// Computes a 3d curve that is the composite of a 2d curve and the surface map.
        /// </summary>
        /// <param name="curve2d">a 2d curve whose image is in the surface's domain.</param>
        /// <param name="tolerance">
        /// the maximum acceptable distance from the returned 3d curve to the image of curve_2d on the surface.
        /// </param>
        /// <param name="curve2dSubdomain">The curve interval (a sub-domain of the original curve) to use.</param>
        /// <returns>3d curve.</returns>
        public static async Task<Curve> Pushup(Remote<Surface> surface, Remote<Curve> curve2d, double tolerance, Interval curve2dSubdomain)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), surface, curve2d, tolerance, curve2dSubdomain);
        }

        /// <summary>
        /// Computes a 3d curve that is the composite of a 2d curve and the surface map.
        /// </summary>
        /// <param name="curve2d">a 2d curve whose image is in the surface's domain.</param>
        /// <param name="tolerance">
        /// the maximum acceptable distance from the returned 3d curve to the image of curve_2d on the surface.
        /// </param>
        /// <returns>3d curve.</returns>
        public static async Task<Curve> Pushup(this Surface surface, Curve curve2d, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), surface, curve2d, tolerance);
        }

        /// <summary>
        /// Computes a 3d curve that is the composite of a 2d curve and the surface map.
        /// </summary>
        /// <param name="curve2d">a 2d curve whose image is in the surface's domain.</param>
        /// <param name="tolerance">
        /// the maximum acceptable distance from the returned 3d curve to the image of curve_2d on the surface.
        /// </param>
        /// <returns>3d curve.</returns>
        public static async Task<Curve> Pushup(Remote<Surface> surface, Remote<Curve> curve2d, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), surface, curve2d, tolerance);
        }

        /// <summary>
        /// Pulls a 3d curve back to the surface's parameter space.
        /// </summary>
        /// <param name="curve3d">The curve to pull.</param>
        /// <param name="tolerance">
        /// the maximum acceptable 3d distance between from surface(curve_2d(t))
        /// to the locus of points on the surface that are closest to curve_3d.
        /// </param>
        /// <returns>2d curve.</returns>
        public static async Task<Curve> Pullback(this Surface surface, Curve curve3d, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), surface, curve3d, tolerance);
        }

        /// <summary>
        /// Pulls a 3d curve back to the surface's parameter space.
        /// </summary>
        /// <param name="curve3d">The curve to pull.</param>
        /// <param name="tolerance">
        /// the maximum acceptable 3d distance between from surface(curve_2d(t))
        /// to the locus of points on the surface that are closest to curve_3d.
        /// </param>
        /// <returns>2d curve.</returns>
        public static async Task<Curve> Pullback(Remote<Surface> surface, Remote<Curve> curve3d, double tolerance)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), surface, curve3d, tolerance);
        }

        /// <summary>
        /// Pulls a 3d curve back to the surface's parameter space.
        /// </summary>
        /// <param name="curve3d">A curve.</param>
        /// <param name="tolerance">
        /// the maximum acceptable 3d distance between from surface(curve_2d(t))
        /// to the locus of points on the surface that are closest to curve_3d.
        /// </param>
        /// <param name="curve3dSubdomain">A sub-domain of the curve to sample.</param>
        /// <returns>2d curve.</returns>
        public static async Task<Curve> Pullback(this Surface surface, Curve curve3d, double tolerance, Interval curve3dSubdomain)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), surface, curve3d, tolerance, curve3dSubdomain);
        }

        /// <summary>
        /// Pulls a 3d curve back to the surface's parameter space.
        /// </summary>
        /// <param name="curve3d">A curve.</param>
        /// <param name="tolerance">
        /// the maximum acceptable 3d distance between from surface(curve_2d(t))
        /// to the locus of points on the surface that are closest to curve_3d.
        /// </param>
        /// <param name="curve3dSubdomain">A sub-domain of the curve to sample.</param>
        /// <returns>2d curve.</returns>
        public static async Task<Curve> Pullback(Remote<Surface> surface, Remote<Curve> curve3d, double tolerance, Interval curve3dSubdomain)
        {
            return await ComputeServer.PostAsync<Curve>(ApiAddress(), surface, curve3d, tolerance, curve3dSubdomain);
        }
    }

    public static class IntersectionCompute
    {
        static string ApiAddress([CallerMemberName] string caller = null)
        {
            return ComputeServer.ApiAddress(typeof(Intersection), caller);
        }

        /// <summary>
        /// Intersects a mesh with an infinite plane.
        /// </summary>
        /// <param name="mesh">Mesh to intersect.</param>
        /// <param name="plane">Plane to intersect with.</param>
        /// <returns>
        /// An array of polylines describing the intersection loops, 
        /// or null if no intersections could be found.
        /// </returns>
        public static async Task<Polyline[]> MeshPlane(Mesh mesh, Plane plane)
        {
            return await ComputeServer.PostAsync<Polyline[]>(ApiAddress(), mesh, plane);
        }

        /// <summary>
        /// Intersects a mesh with an infinite plane.
        /// </summary>
        /// <param name="mesh">Mesh to intersect.</param>
        /// <param name="plane">Plane to intersect with.</param>
        /// <returns>
        /// An array of polylines describing the intersection loops, 
        /// or null if no intersections could be found.
        /// </returns>
        public static async Task<Polyline[]> MeshPlane(Remote<Mesh> mesh, Plane plane)
        {
            return await ComputeServer.PostAsync<Polyline[]>(ApiAddress(), mesh, plane);
        }

        /// <summary>
        /// Intersects a mesh with a collection of infinite planes.
        /// </summary>
        /// <param name="mesh">Mesh to intersect.</param>
        /// <param name="planes">Planes to intersect with.</param>
        /// <returns>
        /// An array of polylines describing the intersection loops, 
        /// or null if no intersections could be found.
        /// </returns>
        public static async Task<Polyline[]> MeshPlane(Mesh mesh, IEnumerable<Plane> planes)
        {
            return await ComputeServer.PostAsync<Polyline[]>(ApiAddress(), mesh, planes);
        }

        /// <summary>
        /// Intersects a mesh with a collection of infinite planes.
        /// </summary>
        /// <param name="mesh">Mesh to intersect.</param>
        /// <param name="planes">Planes to intersect with.</param>
        /// <returns>
        /// An array of polylines describing the intersection loops, 
        /// or null if no intersections could be found.
        /// </returns>
        public static async Task<Polyline[]> MeshPlane(Remote<Mesh> mesh, IEnumerable<Plane> planes)
        {
            return await ComputeServer.PostAsync<Polyline[]>(ApiAddress(), mesh, planes);
        }

        /// <summary>
        /// This is an old overload kept for compatibility. Overlaps and near misses are ignored.
        /// </summary>
        /// <param name="meshA">First mesh for intersection.</param>
        /// <param name="meshB">Second mesh for intersection.</param>
        /// <returns>An array of intersection line segments, or null if no intersections were found.</returns>
        public static async Task<Line[]> MeshMeshFast(Mesh meshA, Mesh meshB)
        {
            return await ComputeServer.PostAsync<Line[]>(ApiAddress(), meshA, meshB);
        }

        /// <summary>
        /// This is an old overload kept for compatibility. Overlaps and near misses are ignored.
        /// </summary>
        /// <param name="meshA">First mesh for intersection.</param>
        /// <param name="meshB">Second mesh for intersection.</param>
        /// <returns>An array of intersection line segments, or null if no intersections were found.</returns>
        public static async Task<Line[]> MeshMeshFast(Remote<Mesh> meshA, Remote<Mesh> meshB)
        {
            return await ComputeServer.PostAsync<Line[]>(ApiAddress(), meshA, meshB);
        }

        /// <summary>
        /// Intersects two meshes. Overlaps and near misses are handled. This is an old method kept for compatibility.
        /// </summary>
        /// <param name="meshA">First mesh for intersection.</param>
        /// <param name="meshB">Second mesh for intersection.</param>
        /// <param name="tolerance">A tolerance value. If negative, the positive value will be used.
        /// WARNING! Good tolerance values are in the magnitude of 10^-7, or RhinoMath.SqrtEpsilon*10.</param>
        /// <returns>An array of intersection and overlaps polylines.</returns>
        public static async Task<Polyline[]> MeshMeshAccurate(Mesh meshA, Mesh meshB, double tolerance)
        {
            return await ComputeServer.PostAsync<Polyline[]>(ApiAddress(), meshA, meshB, tolerance);
        }

        /// <summary>
        /// Intersects two meshes. Overlaps and near misses are handled. This is an old method kept for compatibility.
        /// </summary>
        /// <param name="meshA">First mesh for intersection.</param>
        /// <param name="meshB">Second mesh for intersection.</param>
        /// <param name="tolerance">A tolerance value. If negative, the positive value will be used.
        /// WARNING! Good tolerance values are in the magnitude of 10^-7, or RhinoMath.SqrtEpsilon*10.</param>
        /// <returns>An array of intersection and overlaps polylines.</returns>
        public static async Task<Polyline[]> MeshMeshAccurate(Remote<Mesh> meshA, Remote<Mesh> meshB, double tolerance)
        {
            return await ComputeServer.PostAsync<Polyline[]>(ApiAddress(), meshA, meshB, tolerance);
        }

        /// <summary>Finds the first intersection of a ray with a mesh.</summary>
        /// <param name="mesh">A mesh to intersect.</param>
        /// <param name="ray">A ray to be casted.</param>
        /// <returns>
        /// &gt;= 0.0 parameter along ray if successful.
        /// &lt; 0.0 if no intersection found.
        /// </returns>
        public static async Task<double> MeshRay(Mesh mesh, Ray3d ray)
        {
            return await ComputeServer.PostAsync<double>(ApiAddress(), mesh, ray);
        }

        /// <summary>Finds the first intersection of a ray with a mesh.</summary>
        /// <param name="mesh">A mesh to intersect.</param>
        /// <param name="ray">A ray to be casted.</param>
        /// <returns>
        /// &gt;= 0.0 parameter along ray if successful.
        /// &lt; 0.0 if no intersection found.
        /// </returns>
        public static async Task<double> MeshRay(Remote<Mesh> mesh, Ray3d ray)
        {
            return await ComputeServer.PostAsync<double>(ApiAddress(), mesh, ray);
        }

        /// <summary>
        /// Finds the intersections of a mesh and a line.
        /// </summary>
        /// <param name="mesh">A mesh to intersect</param>
        /// <param name="line">The line to intersect with the mesh</param>
        /// <returns>An array of points: one for each face that was passed by the faceIds out reference.
        /// Empty if no items are found.</returns>
        public static async Task<Point3d[]> MeshLine(Mesh mesh, Line line)
        {
            return await ComputeServer.PostAsync<Point3d[]>(ApiAddress(), mesh, line);
        }

        /// <summary>
        /// Finds the intersections of a mesh and a line.
        /// </summary>
        /// <param name="mesh">A mesh to intersect</param>
        /// <param name="line">The line to intersect with the mesh</param>
        /// <returns>An array of points: one for each face that was passed by the faceIds out reference.
        /// Empty if no items are found.</returns>
        public static async Task<Point3d[]> MeshLine(Remote<Mesh> mesh, Line line)
        {
            return await ComputeServer.PostAsync<Point3d[]>(ApiAddress(), mesh, line);
        }

        /// <summary>
        /// Computes point intersections that occur when shooting a ray to a collection of surfaces and Breps.
        /// </summary>
        /// <param name="ray">A ray used in intersection.</param>
        /// <param name="geometry">Only Surface and Brep objects are currently supported. Trims are ignored on Breps.</param>
        /// <param name="maxReflections">The maximum number of reflections. This value should be any value between 1 and 1000, inclusive.</param>
        /// <returns>An array of points: one for each surface or Brep face that was hit, or an empty array on failure.</returns>
        /// <exception cref="ArgumentNullException">geometry is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">maxReflections is strictly outside the [1-1000] range.</exception>
        public static async Task<Point3d[]> RayShoot(Ray3d ray, IEnumerable<GeometryBase> geometry, int maxReflections)
        {
            return await ComputeServer.PostAsync<Point3d[]>(ApiAddress(), ray, geometry, maxReflections);
        }

        /// <summary>
        /// Computes point intersections that occur when shooting a ray to a collection of surfaces and Breps.
        /// </summary>
        /// <param name="ray">A ray used in intersection.</param>
        /// <param name="geometry">Only Surface and Brep objects are currently supported. Trims are ignored on Breps.</param>
        /// <param name="maxReflections">The maximum number of reflections. This value should be any value between 1 and 1000, inclusive.</param>
        /// <returns>An array of points: one for each surface or Brep face that was hit, or an empty array on failure.</returns>
        /// <exception cref="ArgumentNullException">geometry is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">maxReflections is strictly outside the [1-1000] range.</exception>
        public static async Task<Point3d[]> RayShoot(Ray3d ray, Remote<IEnumerable<GeometryBase>> geometry, int maxReflections)
        {
            return await ComputeServer.PostAsync<Point3d[]>(ApiAddress(), ray, geometry, maxReflections);
        }

        /// <summary>
                /// Projects points onto meshes.
                /// </summary>
                /// <param name="meshes">the meshes to project on to.</param>
                /// <param name="points">the points to project.</param>
                /// <param name="direction">the direction to project.</param>
                /// <param name="tolerance">
                /// Projection tolerances used for culling close points and for line-mesh intersection.
                /// </param>
                /// <returns>
                /// Array of projected points, or null in case of any error or invalid input.
                /// </returns>
        public static async Task<Point3d[]> ProjectPointsToMeshes(IEnumerable<Mesh> meshes, IEnumerable<Point3d> points, Vector3d direction, double tolerance)
        {
            return await ComputeServer.PostAsync<Point3d[]>(ApiAddress(), meshes, points, direction, tolerance);
        }

        /// <summary>
                /// Projects points onto meshes.
                /// </summary>
                /// <param name="meshes">the meshes to project on to.</param>
                /// <param name="points">the points to project.</param>
                /// <param name="direction">the direction to project.</param>
                /// <param name="tolerance">
                /// Projection tolerances used for culling close points and for line-mesh intersection.
                /// </param>
                /// <returns>
                /// Array of projected points, or null in case of any error or invalid input.
                /// </returns>
        public static async Task<Point3d[]> ProjectPointsToMeshes(Remote<IEnumerable<Mesh>> meshes, IEnumerable<Point3d> points, Vector3d direction, double tolerance)
        {
            return await ComputeServer.PostAsync<Point3d[]>(ApiAddress(), meshes, points, direction, tolerance);
        }

        /// <summary>
        /// Projects points onto breps.
        /// </summary>
        /// <param name="breps">The breps projection targets.</param>
        /// <param name="points">The points to project.</param>
        /// <param name="direction">The direction to project.</param>
        /// <param name="tolerance">The tolerance used for intersections.</param>
        /// <returns>
        /// Array of projected points, or null in case of any error or invalid input.
        /// </returns>
        /// <example>
        /// <code source='examples\vbnet\ex_projectpointstobreps.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_projectpointstobreps.cs' lang='cs'/>
        /// <code source='examples\py\ex_projectpointstobreps.py' lang='py'/>
        /// </example>
        public static async Task<Point3d[]> ProjectPointsToBreps(IEnumerable<Brep> breps, IEnumerable<Point3d> points, Vector3d direction, double tolerance)
        {
            return await ComputeServer.PostAsync<Point3d[]>(ApiAddress(), breps, points, direction, tolerance);
        }

        /// <summary>
        /// Projects points onto breps.
        /// </summary>
        /// <param name="breps">The breps projection targets.</param>
        /// <param name="points">The points to project.</param>
        /// <param name="direction">The direction to project.</param>
        /// <param name="tolerance">The tolerance used for intersections.</param>
        /// <returns>
        /// Array of projected points, or null in case of any error or invalid input.
        /// </returns>
        /// <example>
        /// <code source='examples\vbnet\ex_projectpointstobreps.vb' lang='vbnet'/>
        /// <code source='examples\cs\ex_projectpointstobreps.cs' lang='cs'/>
        /// <code source='examples\py\ex_projectpointstobreps.py' lang='py'/>
        /// </example>
        public static async Task<Point3d[]> ProjectPointsToBreps(Remote<IEnumerable<Brep>> breps, IEnumerable<Point3d> points, Vector3d direction, double tolerance)
        {
            return await ComputeServer.PostAsync<Point3d[]>(ApiAddress(), breps, points, direction, tolerance);
        }
    }
}