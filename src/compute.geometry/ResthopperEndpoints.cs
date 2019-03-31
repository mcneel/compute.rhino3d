using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Nancy;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.PlugIns;
using Newtonsoft.Json;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Resthopper.IO;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using System.Net;
using Serilog;

namespace compute.geometry
{
    public class ResthopperEndpointsModule : Nancy.NancyModule
    {
        public ResthopperEndpointsModule(Nancy.Routing.IRouteCacheProvider routeCacheProvider)
        {
            Post["/grasshopper"] = _ => Grasshopper(Context);
            Post["/io"] = _ => GetIoNames(Context);
        }

        static string GetGhxFromPointer(string pointer)
        {
            string grasshopperXml = string.Empty;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(pointer);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                grasshopperXml = reader.ReadToEnd();
            }

            return grasshopperXml;
        }

        static Response Grasshopper(NancyContext ctx)
        {
            // Initialize empty grasshopper archive
            var archive = new GH_Archive();

            Log.Debug(">>> Solution requested!");

            // Begin parsing http request
            string json = string.Empty;
            using (var reader = new StreamReader(ctx.Request.Body))
            {
                json = reader.ReadToEnd();

            }

            // Deserialize request to Schema class
            Schema input = JsonConvert.DeserializeObject<Schema>(json);

            // Locate and extract grasshopper xml data from request
            string grasshopperXml = string.Empty;
            if (input.Algo != null)
            {
                // If request contains markup
                byte[] byteArray = Convert.FromBase64String(input.Algo);
                grasshopperXml = System.Text.Encoding.UTF8.GetString(byteArray);
            }
            else if (input.Pointer != null)
            {
                // If request contains pointer
                string pointer = input.Pointer;
                grasshopperXml = GetGhxFromPointer(pointer);
                
            }
            else
            {
                // If algo and pointer fields are empty
                var res = (Response)"No grasshopper markup data provided.";
                res.WithStatusCode(Nancy.HttpStatusCode.BadRequest);

                return res;
            }

            // Attempt to parse received xml data
            if (!archive.Deserialize_Xml(grasshopperXml))
            {
                // If request could not be parsed
                var res = (Response)"Unable to deserialize provided markup data.";
                res.WithStatusCode(Nancy.HttpStatusCode.BadRequest);

                return res;
            }

            // Initialize empty grasshopper definition
            var definition = new GH_Document();

            // Attempt to extract definition from deserialized xml data
            if (!archive.ExtractObject(definition, "Definition"))
            {
                // If definition could not be extracted
                var res = (Response)"Unable to extract grasshopper definition from provided markup.";
                res.WithStatusCode(Nancy.HttpStatusCode.BadRequest);

                return res;
            }

            // Initialize input and output param caches
            var inputs = new List<ResthopperInput>();
            var outputs = new List<ResthopperOutput>();

            // Begin iterating through definition objects
            foreach (var obj in definition.Objects)
            {
                // (Chuck) Guard clause. Resthopper only looks for params in properly named groups.
                var group = obj as GH_Group;
                if (group == null) continue;

                if (group.NickName.Contains("RH_IN"))
                {
                    // If param is flagged as an input
                    var inputParam = new ResthopperInput();
                   
                    // Attempt to initialize input object
                    if (!inputParam.TryBuildResthopperInput(group.NickName, group.Objects()[0], out var message))
                    {
                        // If input param could not be built, move on to next.
                        continue;

                        // (Chuck) Do we want to kill the whole thing here then, too?
                        var res = (Response)$"Invalid input param {group.NickName}. {message}";
                        res.WithStatusCode(Nancy.HttpStatusCode.BadRequest);

                        return res;
                    }

                    if (group.Objects().Count > 1)
                    {
                        // (Chuck) Do we want to do something to handle groups with more than one param object?
                    }

                    // Attempt to get data for input object from matching request value
                    var inputData = input.Values.FirstOrDefault(x => x.ParamName == group.NickName);

                    if (inputData == null)
                    {
                        // If input value could not be set
                        continue;

                        // (Chuck) Do we want to kill the whole thing here then, too?
                        var res = (Response)$"Could not find input value for param {group.NickName}.";
                        res.WithStatusCode(Nancy.HttpStatusCode.BadRequest);

                        return res;
                    }

                    // Attempt to set data of input object to above value
                    if (!inputParam.TrySetData(inputData))
                    {
                        //If input parsing was unsuccssful
                        var res = (Response)$"Unable to set input data for for param {group.NickName}.";
                        res.WithStatusCode(Nancy.HttpStatusCode.BadRequest);

                        return res;
                    }

                    inputs.Add(inputParam);
                }

                if (group.NickName.Contains("RH_OUT"))
                {
                    //If param is flagged as output
                    var outputParam = new ResthopperOutput();

                    // Attempt to initialize output object
                    if (!outputParam.TryBuildResthopperOutput(group.NickName, group.Objects()[0], out var message))
                    {
                        // If output param could not be built, move on to next.
                        continue;

                        // (Chuck) Do we want to kill the whole thing here then, too?
                        var res = (Response)$"Invalid input param {group.NickName}. {message}";
                        res.WithStatusCode(Nancy.HttpStatusCode.BadRequest);

                        return res;
                    }

                    if (group.Objects().Count > 1)
                    {
                        // (Chuck) Do we want to do something to handle groups with more than one param object?
                    }

                    outputs.Add(outputParam);
                }
            }

            // Validate input params
            foreach (var inParam in inputs)
            {
                Log.Debug($"Input {inParam.Label} loaded with {inParam.Param.VolatileDataCount.ToString()} object(s).");
            }

            // Initialize schema for output results
            Schema OutputSchema = new Schema();
            OutputSchema.Algo = Utils.Base64Encode(string.Empty);

            // Begin solving for output params
            foreach (var output in outputs)
            {
                try
                {
                    // These methods handle the logic for crawling down the definition
                    output.Param.CollectData();
                    output.Param.ComputeData();
                }
                catch (Exception e)
                {
                    output.Param.Phase = GH_SolutionPhase.Failed;

                    var res = (Response)e.Message;
                    res.WithStatusCode(Nancy.HttpStatusCode.BadRequest);
                }

                Log.Debug($"Solved for {output.Label} and generated {output.Param.VolatileDataCount.ToString()} object(s).");

                // Initialize new tree for this param's outputs
                Resthopper.IO.DataTree<ResthopperObject> OutputTree = new Resthopper.IO.DataTree<ResthopperObject>();
                OutputTree.ParamName = output.Label;

                // After a successful solution routine, capture output as volatile data
                var volatileData = output.Param.VolatileData;
                for (int p = 0; p < volatileData.PathCount; p++)
                {
                    List<ResthopperObject> ResthopperObjectList = new List<ResthopperObject>();
                    foreach (var goo in volatileData.get_Branch(p))
                    {
                        if (goo == null)
                        {
                            continue;
                        }

                        // Convert goo from generic object to specific grasshopper type and retrieve value
                        dynamic typedGoo = Convert.ChangeType(goo, goo.GetType());
                        var geo = typedGoo.Value;

                        // Cache output as rhino geometry
                        var rhObj = new ResthopperObject();
                        rhObj.Type = geo.GetType().FullName;
                        rhObj.Data = JsonConvert.SerializeObject(geo);

                        ResthopperObjectList.Add(rhObj);
                    }

                    GhPath path = new GhPath(new int[] { p });
                    OutputTree.Add(path.ToString(), ResthopperObjectList);
                }

                // Add results for output object to output schema
                OutputSchema.Values.Add(OutputTree);
            }

            if (OutputSchema.Values.Count < 1)
            {
                // If, after everything, there were no results...
                throw new System.Exceptions.PayAttentionException("Looks like you've missed something..."); // TODO
            }

            var response = (Response)JsonConvert.SerializeObject(OutputSchema);
            response.WithStatusCode(Nancy.HttpStatusCode.OK);

            return response;

            string returnJson = JsonConvert.SerializeObject(OutputSchema);
            return returnJson;
        }

        static Response GetIoNames(NancyContext ctx)
        {
            // load grasshopper file
            var archive = new GH_Archive();
            // TODO: stream to string
            var body = ctx.Request.Body.ToString();
            //
            //var body = input.Algo;

            string json = string.Empty;
            using (var reader = new StreamReader(ctx.Request.Body))
            {
                json = reader.ReadToEnd();

            }

            IoQuerySchema input = JsonConvert.DeserializeObject<IoQuerySchema>(json);
            string pointer = input.RequestedFile;
            string grasshopperXml = GetGhxFromPointer(pointer);

            if (!archive.Deserialize_Xml(grasshopperXml))
                throw new Exception();

            var definition = new GH_Document();
            if (!archive.ExtractObject(definition, "Definition"))
            {
                throw new Exception();
            }

            // Parse input and output names
            List<string> InputNames = new List<string>();
            List<string> OutputNames = new List<string>();
            foreach (var obj in definition.Objects)
            {
                var group = obj as GH_Group;
                if (group == null) continue;

                if (group.NickName.Contains("RH_IN"))
                {
                    InputNames.Add(group.NickName);
                }
                else if (group.NickName.Contains("RH_OUT"))
                {
                    OutputNames.Add(group.NickName);
                }
            }

            IoResponseSchema response = new IoResponseSchema();
            response.InputNames = InputNames;
            response.OutputNames = OutputNames;

            string jsonResponse = JsonConvert.SerializeObject(response);
            return jsonResponse;
        }

        public static ResthopperObject GetResthopperPoint(GH_Point goo)
        {
            var pt = goo.Value;

            ResthopperObject rhObj = new ResthopperObject();
            rhObj.Type = pt.GetType().FullName;
            rhObj.Data = JsonConvert.SerializeObject(pt);
            return rhObj;

        }
        public static ResthopperObject GetResthopperObject<T>(object goo)
        {
            var v = (T)goo;

            ResthopperObject rhObj = new ResthopperObject();
            rhObj.Type = goo.GetType().FullName;
            rhObj.Data = JsonConvert.SerializeObject(v);
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
