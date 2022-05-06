using System;
using Grasshopper.Kernel;
using System.Collections.Generic;

namespace compute.geometry.Endpoints
{
    static class GrasshopperEndpoint
    {
        public static Newtonsoft.Json.Linq.JArray Solve(Newtonsoft.Json.Linq.JObject input)
        {
            GH_Document definition = null;
            Newtonsoft.Json.Linq.JToken token;
            if( input.TryGetValue("definition", out token))
            {
                string encoded = token.ToString();
                byte[] byteArray = Convert.FromBase64String(encoded);
                var archive = new GH_IO.Serialization.GH_Archive();
                if (archive.Deserialize_Binary(byteArray))
                {
                    definition = new GH_Document();
                    if (!archive.ExtractObject(definition, "Definition"))
                    {
                        definition.Dispose();
                        definition = null;
                    }
                }
            }

            var rc = new Newtonsoft.Json.Linq.JArray();
            if (definition!=null && input.TryGetValue("input", out token))
            {
                foreach(var obj in token)
                {
                    var item = obj as Newtonsoft.Json.Linq.JObject;
                    string instanceGuid = item.GetValue("instanceGuid").ToString();
                    var id = new System.Guid(instanceGuid);
                    var component = definition.FindParameter(id);
                    Newtonsoft.Json.Linq.JToken pointsToken;
                    if(item.TryGetValue("points", out pointsToken))
                    {
                        var method = component.GetType().GetMethod("AssignContextualData");
                        method.Invoke(component, new object[] { pointsToken });
                        //List<Rhino.Geometry.Point3d> points = new List<Rhino.Geometry.Point3d>();
                        //foreach(var pointToken in pointsToken)
                        //{
                        //    var pts = pointToken as Newtonsoft.Json.Linq.JArray;
                        //    double x = (double)pts[0];
                        //    double y = (double)pts[1];
                        //    double z = (double)pts[2];
                        //    points.Add(new Rhino.Geometry.Point3d(x, y, z));
                        //}
                        //method.Invoke(component, new object[] { points });
                    }
                }

                definition.Enabled = true;
                definition.NewSolution(true, GH_SolutionMode.CommandLine);
                var components = definition.ActiveObjects();

                var jobject = new Newtonsoft.Json.Linq.JObject();
                for (int i = 0; i < components.Count; i++)
                {
                    var method = components[i].GetType().GetMethod("GetContextualGeometry");
                    if (method != null)
                    {
                        var geometryArray = method.Invoke(components[i], null) as IEnumerable<Rhino.Geometry.GeometryBase>;
                        var id = components[i].InstanceGuid;
                        jobject.Add("instanceId", id);
                        var ja = new Newtonsoft.Json.Linq.JArray();
                        foreach(var geometry in geometryArray)
                        {
                            string s = Newtonsoft.Json.JsonConvert.SerializeObject(geometry, GeometryResolver.JsonSerializerSettings);
                            var g = Newtonsoft.Json.Linq.JObject.Parse(s);
                            //var g = Newtonsoft.Json.Linq.JObject.FromObject(geometry);
                            ja.Add(g);
                        }
                        jobject.Add("geometry", ja);
                        rc.Add(jobject);
                    }
                }
            }

            return rc;
        }
    }
}
