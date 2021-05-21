using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Resthopper.IO;
using Newtonsoft.Json;
using Rhino.Geometry;
using System.Threading.Tasks;

namespace Hops
{
    [Guid("C69BB52C-88BA-4640-B69F-188D111029E8")]
    public class HopsComponent : GH_TaskCapableComponent<Schema>, IGH_VariableParameterComponent
    {
        #region Fields
        int _majorVersion = 0;
        int _minorVersion = 1;
        RemoteDefinition _remoteDefinition = null;
        bool _cacheResultsInMemory = true;
        bool _cacheResultsOnServer = true;
        bool _remoteDefinitionRequiresRebuild = false;
        bool _synchronous = true;

        SolveDataList _workingSolveList;
        int _solveSerialNumber = 0;
        static bool _isHeadless = false;
        static int _currentSolveSerialNumber = 1;
        #endregion

        static HopsComponent()
        {
            if (!Rhino.Runtime.HostUtils.RunningOnWindows)
                return;
            if (Rhino.RhinoApp.IsRunningHeadless)
                return;
            if (Hops.HopsAppSettings.Servers.Length > 0)
                return;
            if (Hops.HopsAppSettings.LaunchWorkerAtStart)
            {
                Servers.StartServerOnLaunch();
            }
        }

        public HopsComponent()
          : base("Hops", "Hops", "Solve an external definition using Rhino Compute", "Params", "Util")
        {
            _isHeadless = Rhino.RhinoApp.IsRunningHeadless;
        }

        public override Guid ComponentGuid => GetType().GUID;
        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            // Nothing to do here. Inputs and outputs are dynamically created
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            // Nothing to do here. Inputs and outputs are dynamically created
        }

        protected override void BeforeSolveInstance()
        {
            Message = "";
            if (!_solvedCallback)
            {
                _solveSerialNumber = _currentSolveSerialNumber++;
                if (_workingSolveList != null)
                    _workingSolveList.Canceled = true;
                _workingSolveList = new SolveDataList(_solveSerialNumber, this, _remoteDefinition, _cacheResultsInMemory);
            }
            base.BeforeSolveInstance();
        }

        bool _solvedCallback = false;
        public void OnWorkingListComplete()
        {
            _solvedCallback = true;
            if (_workingSolveList.SolvedFor(_solveSerialNumber))
            {
                ExpireSolution(true);
            }
            _solvedCallback = false;
        }

        public int SolveSerialNumber => _solveSerialNumber;

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if( string.IsNullOrWhiteSpace(RemoteDefinitionLocation))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No URL or path defined for definition");
                return;
            }

            // Don't allow hops components to run on compute for now.
            if (_isHeadless)
            {
                AddRuntimeMessage(
                    GH_RuntimeMessageLevel.Error,
                    "Hops components are not allowed to run in external definitions. Please help us understand why you need this by emailing steve@mcneel.com");
                return;
            }

            if(InPreSolve)
            {
                if(_workingSolveList.SolvedFor(_solveSerialNumber))
                {
                    var solvedTask = Task.FromResult(_workingSolveList.SolvedSchema(DA.Iteration));
                    TaskList.Add(solvedTask);
                    return;
                }

                List<string> warnings;
                var inputSchema = _remoteDefinition.CreateSolveInput(DA, _cacheResultsOnServer, out warnings);
                if (warnings != null && warnings.Count > 0)
                {
                    foreach (var warning in warnings)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning);
                    }
                    return;
                }
                if (inputSchema != null)
                {
                    _workingSolveList.Add(inputSchema);
                }
                return;
            }

            if (TaskList.Count == 0)
            {
                _workingSolveList.StartSolving(_synchronous);
                if (!_synchronous)
                {
                    Message = "solving...";
                    return;
                }
                else
                {
                    for(int i=0; i<_workingSolveList.Count; i++)
                    {
                        var output = _workingSolveList.SolvedSchema(i);
                        TaskList.Add(Task.FromResult(output));
                    }
                }
            }

            if (!GetSolveResults(DA, out var schema))
            {
                List<string> warnings;
                var inputSchema = _remoteDefinition.CreateSolveInput(DA, _cacheResultsOnServer, out warnings);
                if (warnings != null && warnings.Count > 0)
                {
                    foreach (var warning in warnings)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning);
                    }
                    return;
                }
                if (inputSchema != null)
                    schema = _remoteDefinition.Solve(inputSchema, _cacheResultsInMemory);
                else
                    schema = null;
            }
            
            if (DA.Iteration == 0)
            {
                // TODO: Having to clear the output data seems like a bug in the
                // TaskCapable components logic. We need to investigate this further.
                foreach (var output in Params.Output)
                    output.ClearData();
            }

            if (schema != null)
            {
                _remoteDefinition.SetComponentOutputs(schema, DA, Params.Output, this);
            }
        }

        const string TagVersion = "RemoteSolveVersion";
        const string TagPath = "RemoteDefinitionLocation";
        const string TagCacheResultsOnServer = "CacheSolveResults";
        const string TagCacheResultsInMemory = "CacheResultsInMemory";

        public override bool Write(GH_IWriter writer)
        {
            bool rc = base.Write(writer);
            if (rc)
            {
                writer.SetVersion(TagVersion, _majorVersion, _minorVersion, 0);
                writer.SetString(TagPath, RemoteDefinitionLocation);
                writer.SetBoolean(TagCacheResultsOnServer, _cacheResultsOnServer);
                writer.SetBoolean(TagCacheResultsInMemory, _cacheResultsInMemory);
            }
            return rc;
        }
        public override bool Read(GH_IReader reader)
        {
            bool rc = base.Read(reader);
            if (rc)
            {
                var version = reader.GetVersion(TagVersion);
                _majorVersion = version.major;
                _minorVersion = version.minor;
                string path = reader.GetString(TagPath);
                try
                {
                    RemoteDefinitionLocation = path;
                }
                catch (System.Net.WebException)
                {
                    // this can happen if a server is not responding and is acceptable in this
                    // case as we want to read without throwing exceptions
                }

                bool cacheResults = _cacheResultsOnServer;
                if (reader.TryGetBoolean(TagCacheResultsOnServer, ref cacheResults))
                    _cacheResultsOnServer = cacheResults;

                cacheResults = _cacheResultsInMemory;
                if (reader.TryGetBoolean(TagCacheResultsInMemory, ref cacheResults))
                    _cacheResultsInMemory = cacheResults;
            }
            return rc;
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index) => false;
        public bool CanRemoveParameter(GH_ParameterSide side, int index) => false;
        public IGH_Param CreateParameter(GH_ParameterSide side, int index) => null;
        public bool DestroyParameter(GH_ParameterSide side, int index) => true;
        public void VariableParameterMaintenance() {}

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Hops24Icon();
            }
        }

        static System.Drawing.Bitmap _hops24Icon;
        static System.Drawing.Bitmap _hops48Icon;
        static System.Drawing.Bitmap Hops24Icon()
        {
            if (_hops24Icon == null)
            {
                var stream = typeof(HopsComponent).Assembly.GetManifestResourceStream("Hops.resources.Hops_24x24.png");
                _hops24Icon = new System.Drawing.Bitmap(stream);
            }
            return _hops24Icon;
        }
        public static System.Drawing.Bitmap Hops48Icon()
        {
            if (_hops48Icon == null)
            {
                var stream = typeof(HopsComponent).Assembly.GetManifestResourceStream("Hops.resources.Hops_48x48.png");
                _hops48Icon = new System.Drawing.Bitmap(stream);
            }
            return _hops48Icon;
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            var tsi = new ToolStripMenuItem("&Path...", null, (sender, e) => { ShowSetDefinitionUi(); });
            tsi.Font = new System.Drawing.Font(tsi.Font, System.Drawing.FontStyle.Bold);
            menu.Items.Add(tsi);

            tsi = new ToolStripMenuItem("Asynchronous", null, (s, e) => { _synchronous = !_synchronous; });
            tsi.ToolTipText = "Do not block while solving";
            tsi.Checked = !_synchronous;
            menu.Items.Add(tsi);

            tsi = new ToolStripMenuItem("Cache In Memory", null, (s, e) => { _cacheResultsInMemory = !_cacheResultsInMemory; });
            tsi.ToolTipText = "Keep previous results in memory cache";
            tsi.Checked = _cacheResultsInMemory;
            menu.Items.Add(tsi);

            tsi = new ToolStripMenuItem("Cache On Server", null, (s, e) => { _cacheResultsOnServer = !_cacheResultsOnServer; });
            tsi.ToolTipText = "Tell the compute server to cache results for reuse in the future";
            tsi.Checked = _cacheResultsOnServer;
            menu.Items.Add(tsi);
        }

        /// <summary>
        /// Used for supporting double click on the component. 
        /// </summary>
        class ComponentAttributes : GH_ComponentAttributes
        {
            HopsComponent _component;
            public ComponentAttributes(HopsComponent parentComponent) : base(parentComponent)
            {
                _component = parentComponent;
            }

            protected override void Render(GH_Canvas canvas, System.Drawing.Graphics graphics, GH_CanvasChannel channel)
            {
                base.Render(canvas, graphics, channel);
                if (channel == GH_CanvasChannel.Objects &&
                    GH_Canvas.ZoomFadeMedium > 0 &&
                    !string.IsNullOrWhiteSpace(_component.RemoteDefinitionLocation)
                    )
                {
                    RenderHop(graphics, GH_Canvas.ZoomFadeMedium, new System.Drawing.PointF(Bounds.Right, Bounds.Bottom));
                }
            }

            void RenderHop(System.Drawing.Graphics graphics, int alpha, System.Drawing.PointF anchor)
            {
                var boxHops = new System.Drawing.RectangleF(anchor.X - 16, anchor.Y - 8, 16, 16);
                var bmp = HopsComponent.Hops48Icon();
                graphics.DrawImage(bmp, boxHops);
            }

            public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
            {
                try
                {
                    _component.ShowSetDefinitionUi();
                }
                catch(Exception ex)
                {
                    _component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                }
                return base.RespondToMouseDoubleClick(sender, e);
            }
        }

        public override void CreateAttributes()
        {
            Attributes = new ComponentAttributes(this);
        }

        void ShowSetDefinitionUi()
        {
            var form = new SetDefinitionForm(RemoteDefinitionLocation);
            if(form.ShowModal(Grasshopper.Instances.EtoDocumentEditor))
            {
                var comp = Grasshopper.Instances.ComponentServer.FindObjectByName(form.Path, true, true);
                if (comp != null)
                    RemoteDefinitionLocation = comp.Guid.ToString();
                else
                    RemoteDefinitionLocation = form.Path;
            }
        }

        // keep public in case external C# code wants to set this
        public string RemoteDefinitionLocation
        {
            get
            {
                if (_remoteDefinition != null)
                {
                    return _remoteDefinition.Path;
                }
                return string.Empty;
            }
            set
            {
                // Always rebuild the remote definition information when setting this property.
                // This way you can poke the path button to force a refresh in case the situation
                // on the server has changed.
                //if (!string.Equals(RemoteDefinitionLocation, value, StringComparison.OrdinalIgnoreCase))
                {
                    if(_remoteDefinition != null)
                    {
                        _remoteDefinition.Dispose();
                        _remoteDefinition = null;
                    }
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        _remoteDefinition = RemoteDefinition.Create(value, this);
                        DefineInputsAndOutputs();
                    }
                }
            }
        }

        void DefineInputsAndOutputs()
        {
            ClearRuntimeMessages();
            string description = _remoteDefinition.GetDescription(out System.Drawing.Bitmap customIcon);

            if (_remoteDefinition.IsNotResponingUrl())
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to connect to server");
                Grasshopper.Instances.ActiveCanvas?.Invalidate();
                return;
            }

            if (!string.IsNullOrWhiteSpace(description) && !Description.Equals(description))
            {
                Description = description;
            }
            var inputs = _remoteDefinition.GetInputParams();
            var outputs = _remoteDefinition.GetOutputParams();

            bool buildInputs = inputs != null;
            bool buildOutputs = outputs != null;

            // check to see if the existing params match
            Dictionary<string, List<IGH_Param>> inputSources = new Dictionary<string, List<IGH_Param>>();
            foreach (var param in Params.Input)
            {
                inputSources.Add(param.Name, new List<IGH_Param>(param.Sources));
            }

            if (buildInputs && Params.Input.Count == inputs.Count)
            {
                buildInputs = false;
                foreach (var param in Params.Input.ToArray())
                {
                    if (!inputs.ContainsKey(param.Name))
                    {
                        buildInputs = true;
                        break;
                    }
                    else
                    {
                        // if input param exists, make sure param access is correct
                        var (input, _) = inputs[param.Name];
                        param.Access = RemoteDefinition.AccessFromInput(input);
                    }
                }
            }
            if (buildOutputs && Params.Output.Count == outputs.Count)
            {
                buildOutputs = false;
                foreach (var param in Params.Output.ToArray())
                {
                    if (!outputs.ContainsKey(param.Name))
                    {
                        buildOutputs = true;
                        break;
                    }
                }
            }

            // Remove all existing inputs and outputs
            if (buildInputs)
            {
                foreach (var param in Params.Input.ToArray())
                {
                    Params.UnregisterInputParameter(param);
                }
            }
            if (buildOutputs)
            {
                foreach (var param in Params.Output.ToArray())
                {
                    Params.UnregisterOutputParameter(param);
                }
            }

            bool recompute = false;
            if (buildInputs && inputs != null)
            {
                bool containsEmptyDefaults = false;
                var mgr = CreateInputManager();
                foreach (var kv in inputs)
                {
                    string name = kv.Key;
                    var (input, param) = kv.Value;
                    GH_ParamAccess access = RemoteDefinition.AccessFromInput(input);
                    string inputDescription = name;
                    if (!string.IsNullOrWhiteSpace(input.Description))
                        inputDescription = input.Description;
                    if (input.Default == null)
                        containsEmptyDefaults = true;
                    string nickname = name;
                    if (!string.IsNullOrWhiteSpace(input.Nickname))
                        nickname = input.Nickname;
                    int paramIndex = -1;
                    switch (param)
                    {
                        case Grasshopper.Kernel.Parameters.Param_Arc _:
                            paramIndex = mgr.AddArcParameter(name, nickname, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Boolean _:
                            if (input.Default == null)
                                paramIndex = mgr.AddBooleanParameter(name, nickname, inputDescription, access);
                            else
                                paramIndex = mgr.AddBooleanParameter(name, nickname, inputDescription, access, Convert.ToBoolean(input.Default));
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Box _:
                            paramIndex = mgr.AddBoxParameter(name, nickname, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Brep _:
                            paramIndex = mgr.AddBrepParameter(name, nickname, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Circle _:
                            paramIndex = mgr.AddCircleParameter(name, nickname, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Colour _:
                            paramIndex = mgr.AddColourParameter(name, nickname, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Complex _:
                            paramIndex = mgr.AddComplexNumberParameter(name, nickname, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Culture _:
                            paramIndex = mgr.AddCultureParameter(name, nickname, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Curve _:
                            paramIndex = mgr.AddCurveParameter(name, nickname, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Field _:
                            paramIndex = mgr.AddFieldParameter(name, nickname, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_FilePath _:
                            if (input.Default == null)
                                paramIndex = mgr.AddTextParameter(name, nickname, inputDescription, access);
                            else
                                paramIndex = mgr.AddTextParameter(name, nickname, inputDescription, access, input.Default.ToString());
                            break;
                        case Grasshopper.Kernel.Parameters.Param_GenericObject _:
                            throw new Exception("generic param not supported");
                        case Grasshopper.Kernel.Parameters.Param_Geometry _:
                            paramIndex = mgr.AddGeometryParameter(name, nickname, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Group _:
                            throw new Exception("group param not supported");
                        case Grasshopper.Kernel.Parameters.Param_Guid _:
                            throw new Exception("guid param not supported");
                        case Grasshopper.Kernel.Parameters.Param_Integer _:
                            if (input.Default == null)
                                paramIndex = mgr.AddIntegerParameter(name, nickname, inputDescription, access);
                            else
                                paramIndex = mgr.AddIntegerParameter(name, nickname, inputDescription, access, Convert.ToInt32(input.Default));
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Interval _:
                            paramIndex = mgr.AddIntervalParameter(name, nickname, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Interval2D _:
                            paramIndex = mgr.AddInterval2DParameter(name, nickname, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_LatLonLocation _:
                            throw new Exception("latlonlocation param not supported");
                        case Grasshopper.Kernel.Parameters.Param_Line _:
                            if (input.Default == null)
                                paramIndex = mgr.AddLineParameter(name, nickname, inputDescription, access);
                            else
                                paramIndex = mgr.AddLineParameter(name, nickname, inputDescription, access, JsonConvert.DeserializeObject<Line>(input.Default.ToString()));
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Matrix _:
                            paramIndex = mgr.AddMatrixParameter(name, nickname, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Mesh _:
                            paramIndex = mgr.AddMeshParameter(name, nickname, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_MeshFace _:
                            paramIndex = mgr.AddMeshFaceParameter(name, nickname, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_MeshParameters _:
                            throw new Exception("meshparameters paran not supported");
                        case Grasshopper.Kernel.Parameters.Param_Number _:
                            if (input.Default == null)
                                paramIndex = mgr.AddNumberParameter(name, nickname, inputDescription, access);
                            else
                                paramIndex = mgr.AddNumberParameter(name, nickname, inputDescription, access, Convert.ToDouble(input.Default));
                            break;
                        //case Grasshopper.Kernel.Parameters.Param_OGLShader:
                        case Grasshopper.Kernel.Parameters.Param_Plane _:
                            if (input.Default == null)
                                paramIndex = mgr.AddPlaneParameter(name, nickname, inputDescription, access);
                            else
                                paramIndex = mgr.AddPlaneParameter(name, nickname, inputDescription, access, JsonConvert.DeserializeObject<Plane>(input.Default.ToString()));
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Point _:
                            if (input.Default == null)
                                paramIndex = mgr.AddPointParameter(name, nickname, inputDescription, access);
                            else
                                paramIndex = mgr.AddPointParameter(name, nickname, inputDescription, access, JsonConvert.DeserializeObject<Point3d>(input.Default.ToString()));
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Rectangle _:
                            paramIndex = mgr.AddRectangleParameter(name, nickname, inputDescription, access);
                            break;
                        //case Grasshopper.Kernel.Parameters.Param_ScriptVariable _:
                        case Grasshopper.Kernel.Parameters.Param_String _:
                            if (input.Default == null)
                                paramIndex = mgr.AddTextParameter(name, nickname, inputDescription, access);
                            else
                                paramIndex = mgr.AddTextParameter(name, nickname, inputDescription, access, input.Default.ToString());
                            break;
                        case Grasshopper.Kernel.Parameters.Param_StructurePath _:
                            paramIndex = mgr.AddPathParameter(name, nickname, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_SubD _:
                            paramIndex = mgr.AddSubDParameter(name, nickname, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Surface _:
                            paramIndex = mgr.AddSurfaceParameter(name, nickname, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Time _:
                            paramIndex = mgr.AddTimeParameter(name, nickname, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Transform _:
                            paramIndex = mgr.AddTransformParameter(name, nickname, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Vector _:
                            if(input.Default == null)
                                paramIndex = mgr.AddVectorParameter(name, nickname, inputDescription, access);
                            else
                                paramIndex = mgr.AddVectorParameter(name, nickname, inputDescription, access, JsonConvert.DeserializeObject<Vector3d>(input.Default.ToString()));
                            break;
                        case Grasshopper.Kernel.Special.GH_NumberSlider _:
                            paramIndex = mgr.AddNumberParameter(name, nickname, inputDescription, access);
                            break;
                    }

                    if (paramIndex >=0 && inputSources.TryGetValue(name, out List<IGH_Param> rehookInputs))
                    {
                        foreach (var rehookInput in rehookInputs)
                            Params.Input[paramIndex].AddSource(rehookInput);
                    }
                }

                if (!containsEmptyDefaults)
                    recompute = true;
            }
            if (buildOutputs && outputs != null)
            {
                var mgr = CreateOutputManager();
                foreach (var kv in outputs)
                {
                    string name = kv.Key;
                    var param = kv.Value;
                    string nickname = name;
                    if (!string.IsNullOrWhiteSpace(param.NickName))
                        nickname = param.NickName;
                    string outputDescription = name;
                    if (!string.IsNullOrWhiteSpace(param.Description))
                        outputDescription = param.Description;
                    switch (param)
                    {
                        case Grasshopper.Kernel.Parameters.Param_Arc _:
                            mgr.AddArcParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Boolean _:
                            mgr.AddBooleanParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Box _:
                            mgr.AddBoxParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Brep _:
                            mgr.AddBrepParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Circle _:
                            mgr.AddCircleParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Colour _:
                            mgr.AddColourParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Complex _:
                            mgr.AddComplexNumberParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Culture _:
                            mgr.AddCultureParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Curve _:
                            mgr.AddCurveParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Field _:
                            mgr.AddFieldParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_FilePath _:
                            mgr.AddTextParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_GenericObject _:
                            mgr.AddGenericParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Geometry _:
                            mgr.AddGeometryParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Group _:
                            throw new Exception("group param not supported");
                        case Grasshopper.Kernel.Parameters.Param_Guid _:
                            throw new Exception("guid param not supported");
                        case Grasshopper.Kernel.Parameters.Param_Integer _:
                            mgr.AddIntegerParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Interval _:
                            mgr.AddIntervalParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Interval2D _:
                            mgr.AddInterval2DParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_LatLonLocation _:
                            throw new Exception("latlonlocation param not supported");
                        case Grasshopper.Kernel.Parameters.Param_Line _:
                            mgr.AddLineParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Matrix _:
                            mgr.AddMatrixParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Mesh _:
                            mgr.AddMeshParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_MeshFace _:
                            mgr.AddMeshFaceParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_MeshParameters _:
                            throw new Exception("meshparameters paran not supported");
                        case Grasshopper.Kernel.Parameters.Param_Number _:
                            mgr.AddNumberParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        //case Grasshopper.Kernel.Parameters.Param_OGLShader:
                        case Grasshopper.Kernel.Parameters.Param_Plane _:
                            mgr.AddPlaneParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Point _:
                            mgr.AddPointParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Rectangle _:
                            mgr.AddRectangleParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        //case Grasshopper.Kernel.Parameters.Param_ScriptVariable _:
                        case Grasshopper.Kernel.Parameters.Param_String _:
                            mgr.AddTextParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_StructurePath _:
                            mgr.AddPathParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_SubD _:
                            mgr.AddSubDParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Surface _:
                            mgr.AddSurfaceParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Time _:
                            mgr.AddTimeParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Transform _:
                            mgr.AddTransformParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Vector _:
                            mgr.AddVectorParameter(name, nickname, outputDescription, GH_ParamAccess.tree);
                            break;
                    }
                }
            }

            if (customIcon != null)
            {
                // Draw hops icon overlay on custom icon. We can add an option
                // to the data returned from a server to skip this overlay in
                // the future.
                // Create a slightly large image so we can cram the hops overlay
                // deeper into the lower right corner
                //var bmp = new System.Drawing.Bitmap(28, 28);
                //using(var graphics = System.Drawing.Graphics.FromImage(bmp))
                //{
                //    // use fill to debug
                //    //graphics.FillRectangle(System.Drawing.Brushes.PowderBlue, 0, 0, 28, 28);
                //    var rect = new System.Drawing.Rectangle(2, 2, 24, 24);
                //    graphics.DrawImage(customIcon, rect);
                //    rect = new System.Drawing.Rectangle(16, 14, 14, 14);
                //    graphics.DrawImage(Hops24Icon(), rect);

                //}
                SetIconOverride(customIcon);
            }
            if (buildInputs || buildOutputs)
            {
                Params.OnParametersChanged();
                Grasshopper.Instances.ActiveCanvas?.Invalidate();

                if (recompute)
                {
                    var doc = OnPingDocument();
                    if (doc != null)
                        doc.NewSolution(true);
                }
            }
        }

        GH_InputParamManager CreateInputManager()
        {
            var constructors = typeof(GH_InputParamManager).GetConstructors(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var mgr = constructors[0].Invoke(new object[] { this }) as GH_InputParamManager;
            return mgr;
        }
        GH_OutputParamManager CreateOutputManager()
        {
            var constructors = typeof(GH_OutputParamManager).GetConstructors(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var mgr = constructors[0].Invoke(new object[] { this }) as GH_OutputParamManager;
            return mgr;
        }

        public void OnRemoteDefinitionChanged()
        {
            if (_remoteDefinitionRequiresRebuild)
                return;

            // this is typically called on a different thread than the main UI thread
            _remoteDefinitionRequiresRebuild = true;
            Rhino.RhinoApp.Idle += RhinoApp_Idle;
        }

        private void RhinoApp_Idle(object sender, EventArgs e)
        {
            if (!_remoteDefinitionRequiresRebuild)
            {
                // not sure how this could happen, but in case it does just
                // remove the idle event and bail
                Rhino.RhinoApp.Idle -= RhinoApp_Idle;
                return;
            }

            var ghdoc = OnPingDocument();
            if (ghdoc != null && ghdoc.SolutionState == GH_ProcessStep.Process)
            {
                // Processing a solution. Wait until the next idle event to do something
                return;
            }

            // stop the idle event watcher
            Rhino.RhinoApp.Idle -= RhinoApp_Idle;
            _remoteDefinitionRequiresRebuild = false;
            DefineInputsAndOutputs();
        }
    }
}
