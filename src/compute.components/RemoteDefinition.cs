using System;
using System.Collections.Generic;
using System.Text;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Newtonsoft.Json;
using Resthopper.IO;

namespace Compute.Components
{
    class RemoteDefinition : IDisposable
    {
        public RemoteDefinition(string path)
        {
            Path = path;
        }

        public string Path { get; private set; }
        public bool PathIsAppServer
        {
            get
            {
                string p = Path.ToLowerInvariant();
                return p.StartsWith("http:") || p.StartsWith("https:");
            }
        }

        public Dictionary<string, Tuple<InputParamSchema, IGH_Param>> GetInputParams()
        {
            if( _inputParams == null)
            {
                GetRemoteDescription();
            }
            return _inputParams;
        }

        public Dictionary<string, IGH_Param> GetOutputParams()
        {
            if (_outputParams == null)
            {
                GetRemoteDescription();
            }
            return _outputParams;
        }

        public string GetDescription()
        {
            if (_description == null)
            {
                GetRemoteDescription();
            }
            return _description;
        }

        void GetRemoteDescription()
        {
            string address = Path;
            if (!PathIsAppServer)
            {
                address = LocalServer.GetDescriptionUrl(Path);
            }
            using (var client = new System.Net.WebClient())
            {
                string s = client.DownloadString(address);
                var responseSchema = JsonConvert.DeserializeObject<Resthopper.IO.IoResponseSchema>(s);
                _description = responseSchema.Description;
                _inputParams = new Dictionary<string, Tuple<InputParamSchema, IGH_Param>>();
                _outputParams = new Dictionary<string, IGH_Param>();
                foreach(var input in responseSchema.Inputs)
                {
                    string inputParamName = input.Name;
                    if (inputParamName.StartsWith("RH_IN:"))
                    {
                        var chunks = inputParamName.Split(new char[] { ':' });
                        inputParamName = chunks[chunks.Length-1];
                    }
                    _inputParams[inputParamName] = Tuple.Create(input, ParamFromIoResponseSchema(input));
                }
                foreach(var output in responseSchema.Outputs)
                {
                    string outputParamName = output.Name;
                    if (outputParamName.StartsWith("RH_OUT:"))
                    {
                        var chunks = outputParamName.Split(new char[] { ':' });
                        outputParamName = chunks[chunks.Length - 1];
                    }
                    _outputParams[outputParamName] = ParamFromIoResponseSchema(output);
                }
            }
        }

        public void Dispose()
        {
        }

        static System.Net.Http.HttpClient _httpClient = null;
        static System.Net.Http.HttpClient HttpClient
        {
            get
            {
                if (_httpClient==null)
                {
                    _httpClient = new System.Net.Http.HttpClient();
                }
                return _httpClient;
            }
        }

        public void SolveInstance(IGH_DataAccess DA, List<IGH_Param> outputParams, GH_ActiveObject component)
        {
            string inputJson = CreateInputJson(DA);
            string solveUrl = LocalServer.GetSolveUrl();
            if (PathIsAppServer)
            {
                int index = Path.LastIndexOf('/');
                solveUrl = Path.Substring(0, index + 1) + "solve";
            }

            var content = new System.Net.Http.StringContent(inputJson, Encoding.UTF8, "application/json");
            var result = HttpClient.PostAsync(solveUrl, content);
            var responseMessage = result.Result;
            //if (!responseMessage.IsSuccessStatusCode)
            //{
            //    component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"{responseMessage.StatusCode}: {responseMessage.ReasonPhrase}");
            //    return;
            //}
            var remoteSolvedData = responseMessage.Content;
            var stringResult = remoteSolvedData.ReadAsStringAsync().Result;
            var schema = JsonConvert.DeserializeObject<Resthopper.IO.Schema>(stringResult);
            
            foreach (var datatree in schema.Values)
            {
                string outputParamName = datatree.ParamName;
                if (outputParamName.StartsWith("RH_OUT:"))
                {
                    var chunks = outputParamName.Split(new char[] { ':' });
                    outputParamName = chunks[chunks.Length - 1];
                }
                int paramIndex = 0;
                for (int i = 0; i < outputParams.Count; i++)
                {
                    if (outputParams[i].Name.Equals(outputParamName))
                    {
                        paramIndex = i;
                        break;
                    }
                }

                var structure = new Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo>();
                foreach (var kv in datatree.InnerTree)
                {
                    var tokens = kv.Key.Trim(new char[] { '{', '}' }).Split(';');
                    List<int> elements = new List<int>();
                    foreach (var token in tokens)
                    {
                        if (!string.IsNullOrWhiteSpace(token))
                        {
                            elements.Add(int.Parse(token));
                        }
                    }
                    var path = new Grasshopper.Kernel.Data.GH_Path(elements.ToArray());
                    for (int gooIndex = 0; gooIndex < kv.Value.Count; gooIndex++)
                    {
                        var goo = GooFromReshopperObject(kv.Value[gooIndex]);
                        structure.Insert(goo, path, gooIndex);
                    }
                }
                DA.SetDataTree(paramIndex, structure);
            }

            foreach (var error in schema.Errors)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, error);
            }
            foreach (var warning in schema.Warnings)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning);
            }
        }

        static Grasshopper.Kernel.Types.IGH_Goo GooFromReshopperObject(ResthopperObject obj)
        {
            string data = obj.Data.Trim('"');
            switch (obj.Type)
            {
                case "System.String":
                    return new Grasshopper.Kernel.Types.GH_String(data);
                case "Rhino.Geometry.Mesh":
                    {
                        Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string,string>>(data);
                        var mesh = Rhino.Runtime.CommonObject.FromJSON(dict) as Mesh;
                        return new Grasshopper.Kernel.Types.GH_Mesh(mesh);
                    }
            }
            throw new Exception("unable to convert resthopper data");
        }

        static List<IGH_Param> _params;
        static IGH_Param ParamFromIoResponseSchema(IoParamSchema item)
        {
            if (_params == null)
            {
                _params = new List<IGH_Param>();
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Arc());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Boolean());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Box());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Brep());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Circle());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Colour());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Complex());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Culture());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Curve());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Field());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_FilePath());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_GenericObject());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Geometry());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Group());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Guid());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Integer());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Interval());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Interval2D());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_LatLonLocation());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Line());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Matrix());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Mesh());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_MeshFace());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_MeshParameters());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Number());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Plane());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Point());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Rectangle());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_String());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_StructurePath());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_SubD());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Surface());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Time());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Transform());
                _params.Add(new Grasshopper.Kernel.Parameters.Param_Vector());
            }
            foreach(var p in _params)
            {
                if (p.TypeName.Equals(item.ParamType, StringComparison.OrdinalIgnoreCase))
                {
                    var obj = System.Activator.CreateInstance(p.GetType());
                    return obj as IGH_Param;
                }
            }
            return null;
        }

        string CreateInputJson(IGH_DataAccess DA)
        {
            var schema = new Resthopper.IO.Schema();
            var inputs = GetInputParams();
            foreach (var kv in inputs)
            {
                var (input, param) = kv.Value;
                string inputName = kv.Key;
                string computeName = input.Name;
                bool itemAccess = input.AtLeast == 1 && input.AtMost == 1;

                var dataTree = new DataTree<Resthopper.IO.ResthopperObject>();
                dataTree.ParamName = computeName;
                schema.Values.Add(dataTree);
                switch (param)
                {
                    case Grasshopper.Kernel.Parameters.Param_Arc _:
                        {
                            Arc a = new Arc();
                            if (DA.GetData(inputName, ref a))
                            {
                                dataTree.Append(new ResthopperObject(a), "0");
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Boolean _:
                        {
                            if (itemAccess)
                            {
                                bool b = false;
                                if (DA.GetData(inputName, ref b))
                                {
                                    dataTree.Append(new ResthopperObject(b), "0");
                                }
                            }
                            else
                            {
                                List<bool> bools = new List<bool>();
                                if (DA.GetDataList(inputName, bools))
                                {
                                    foreach(var b in bools)
                                    {
                                        dataTree.Append(new ResthopperObject(b), "0");
                                    }
                                }
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Box _:
                        {
                            Box b = Box.Empty;
                            if (DA.GetData(inputName, ref b))
                            {
                                dataTree.Append(new ResthopperObject(b), "0");
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Brep _:
                        {
                            Brep b = null;
                            if (DA.GetData(inputName, ref b))
                            {
                                dataTree.Append(new ResthopperObject(b), "0");
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Circle _:
                        {
                            Circle c = Circle.Unset;
                            if (DA.GetData(inputName, ref c))
                            {
                                dataTree.Append(new ResthopperObject(c), "0");
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Colour _:
                        {
                            System.Drawing.Color c = System.Drawing.Color.Empty;
                            if (DA.GetData(inputName, ref c))
                            {
                                dataTree.Append(new ResthopperObject(c), "0");
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Complex _:
                        {
                            Grasshopper.Kernel.Types.Complex c = Grasshopper.Kernel.Types.Complex.NaN;
                            if (DA.GetData(inputName, ref c))
                            {
                                dataTree.Append(new ResthopperObject(c), "0");
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Culture _:
                        {
                            System.Globalization.CultureInfo c = null;
                            if (DA.GetData(inputName, ref c))
                            {
                                dataTree.Append(new ResthopperObject(c), "0");
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Curve _:
                        {
                            Curve c = null;
                            if (DA.GetData(inputName, ref c))
                            {
                                dataTree.Append(new ResthopperObject(c), "0");
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Field _:
                        {
                            Grasshopper.Kernel.Types.GH_Field field = null;
                            if (DA.GetData(inputName, ref field))
                            {
                                dataTree.Append(new ResthopperObject(field), "0");
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_FilePath _:
                        {
                            string s = string.Empty;
                            if (DA.GetData(inputName, ref s))
                            {
                                dataTree.Append(new ResthopperObject(s), "0");
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_GenericObject _:
                        throw new Exception("generic param not supported");
                    case Grasshopper.Kernel.Parameters.Param_Geometry _:
                        {
                            GeometryBase g = null;
                            if( DA.GetData(inputName, ref g))
                            {
                                dataTree.Append(new ResthopperObject(g), "0");
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Group _:
                        throw new Exception("group param not supported");
                    case Grasshopper.Kernel.Parameters.Param_Guid _:
                        throw new Exception("guid param not supported");
                    case Grasshopper.Kernel.Parameters.Param_Integer _:
                        {
                            if (itemAccess)
                            {
                                int i = 0;
                                if (DA.GetData(inputName, ref i))
                                {
                                    dataTree.Append(new ResthopperObject(i), "0");
                                }
                            }
                            else
                            {
                                List<int> ints = new List<int>();
                                if (DA.GetDataList(inputName, ints))
                                {
                                    foreach(var item in ints)
                                    {
                                        dataTree.Append(new ResthopperObject(item), "0");
                                    }
                                }
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Interval _:
                        {
                            Grasshopper.Kernel.Types.GH_Interval i = null;
                            if (DA.GetData(inputName, ref i))
                            {
                                dataTree.Append(new ResthopperObject(i), "0");
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Interval2D _:
                        {
                            Grasshopper.Kernel.Types.GH_Interval2D i = null;
                            if (DA.GetData(inputName, ref i))
                            {
                                dataTree.Append(new ResthopperObject(i), "0");
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_LatLonLocation _:
                        throw new Exception("latlonlocation param not supported");
                    case Grasshopper.Kernel.Parameters.Param_Line _:
                        {
                            Line l = Line.Unset;
                            if (DA.GetData(inputName, ref l))
                            {
                                dataTree.Append(new ResthopperObject(l), "0");
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Matrix _:
                        {
                            Matrix m = null;
                            if( DA.GetData(inputName, ref m))
                            {
                                dataTree.Append(new ResthopperObject(m), "0");
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Mesh _:
                        {
                            Mesh m = null;
                            if(DA.GetData(inputName, ref m))
                            {
                                dataTree.Append(new ResthopperObject(m), "0");
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_MeshFace _:
                        {
                            MeshFace m = new MeshFace();
                            if (DA.GetData(inputName, ref m))
                            {
                                dataTree.Append(new ResthopperObject(m), "0");
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_MeshParameters _:
                        throw new Exception("meshparameters paran not supported");
                    case Grasshopper.Kernel.Parameters.Param_Number _:
                        {
                            if (itemAccess)
                            {
                                double d = 0;
                                if (DA.GetData(inputName, ref d))
                                {
                                    dataTree.Append(new ResthopperObject(d), "0");
                                }
                            }
                            else
                            {
                                List<double> list = new List<double>();
                                if (DA.GetDataList(inputName, list))
                                {
                                    foreach (var d in list)
                                    {
                                        dataTree.Append(new ResthopperObject(d), "0");
                                    }
                                }
                            }
                        }
                        break;
                    //case Grasshopper.Kernel.Parameters.Param_OGLShader:
                    case Grasshopper.Kernel.Parameters.Param_Plane _:
                        {
                            Plane p = Plane.Unset;
                            if( DA.GetData(inputName, ref p))
                            {
                                dataTree.Append(new ResthopperObject(p), "0");
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Point _:
                        {
                            List<Point3d> points = new List<Point3d>();
                            if (DA.GetDataList(inputName, points))
                            {
                                foreach (var pt in points)
                                {
                                    dataTree.Append(new ResthopperObject(pt), "0");
                                }
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Rectangle _:
                        {
                            Rectangle3d r = Rectangle3d.Unset;
                            if(DA.GetData(inputName, ref r))
                            {
                                dataTree.Append(new ResthopperObject(r), "0");
                            }
                        }
                        break;
                    //case Grasshopper.Kernel.Parameters.Param_ScriptVariable _:
                    case Grasshopper.Kernel.Parameters.Param_String _:
                        {
                            string s = string.Empty;
                            if(DA.GetData(inputName, ref s))
                            {
                                dataTree.Append(new ResthopperObject(s), "0");
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_StructurePath _:
                        {
                            Grasshopper.Kernel.Types.GH_StructurePath p = null;
                            if(DA.GetData(inputName, ref p))
                            {
                                dataTree.Append(new ResthopperObject(p), "0");
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Surface _:
                        {
                            Surface s = null;
                            if(DA.GetData(inputName, ref s))
                            {
                                dataTree.Append(new ResthopperObject(s), "0");
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Time _:
                        {
                            DateTime d = DateTime.Now;
                            if(DA.GetData(inputName, ref d))
                            {
                                dataTree.Append(new ResthopperObject(d), "0");
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Transform _:
                        {
                            Transform t = Transform.Identity;
                            if(DA.GetData(inputName, ref t))
                            {
                                dataTree.Append(new ResthopperObject(t), "0");
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Parameters.Param_Vector _:
                        {
                            Vector3d v = Vector3d.Unset;
                            if(DA.GetData(inputName, ref v))
                            {
                                dataTree.Append(new ResthopperObject(v), "0");
                            }
                        }
                        break;
                    case Grasshopper.Kernel.Special.GH_NumberSlider _:
                        {
                            double d = 0;
                            if(DA.GetData(inputName, ref d))
                            {
                                dataTree.Append(new ResthopperObject(d), "0");
                            }
                        }
                        break;
                }
            }

            schema.Pointer = Path;
            if (PathIsAppServer)
            {
                string definition = Path.Substring(Path.LastIndexOf('/') + 1);
                schema.Pointer = definition;
            }
            string json = JsonConvert.SerializeObject(schema);
            return json;
        }

        Dictionary<string, Tuple<InputParamSchema, IGH_Param>> _inputParams;
        Dictionary<string, IGH_Param> _outputParams;
        string _description = null;
    }
}
