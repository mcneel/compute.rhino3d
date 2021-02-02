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

namespace Compute.Components
{
    [Guid("C69BB52C-88BA-4640-B69F-188D111029E8")]
    public class HopsComponent : GH_TaskCapableComponent<Schema>, IGH_VariableParameterComponent
    {
        #region Fields
        int _majorVersion = 0;
        int _minorVersion = 1;
        RemoteDefinition _remoteDefinition = null;
        bool _cacheSolveResults = true;
        #endregion

        public HopsComponent()
          : base("Hops", "Hops", "Solve an external definition using Rhino Compute", "Params", "Util")
        {
        }

        public override Guid ComponentGuid => GetType().GUID;
        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if( string.IsNullOrWhiteSpace(RemoteDefinitionLocation))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No URL or path defined for definition");
                return;
            }

            if(InPreSolve)
            {
                List<string> warnings;
                string inputJson = _remoteDefinition.CreateInputJson(DA, _cacheSolveResults, out warnings);
                if (warnings != null && warnings.Count > 0)
                {
                    foreach (var warning in warnings)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning);
                    }
                    return;
                }
                if (inputJson != null)
                {
                    var task = System.Threading.Tasks.Task.Run(() => _remoteDefinition.PostToServer(inputJson));
                    TaskList.Add(task);
                }
                return;
            }

            if (!GetSolveResults(DA, out var schema))
            {
                List<string> warnings;
                string inputJson = _remoteDefinition.CreateInputJson(DA, _cacheSolveResults, out warnings);
                if (warnings != null && warnings.Count > 0)
                {
                    foreach (var warning in warnings)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning);
                    }
                    return;
                }
                if (inputJson != null)
                    schema = _remoteDefinition.PostToServer(inputJson);
                else
                    schema = null;
            }

            if (schema != null)
            {
                _remoteDefinition.SetComponentOutputs(schema, DA, Params.Output, this);
            }
            else
            {
                if (!_remoteDefinition.PathIsAppServer && !System.IO.File.Exists(_remoteDefinition.Path))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"No definition at {_remoteDefinition.Path}");
                }
            }
        }

        const string TagVersion = "RemoteSolveVersion";
        const string TagPath = "RemoteDefinitionLocation";
        const string TagCacheResults = "CacheSolveResults";

        public override bool Write(GH_IWriter writer)
        {
            bool rc = base.Write(writer);
            if (rc)
            {
                writer.SetVersion(TagVersion, _majorVersion, _minorVersion, 0);
                writer.SetString(TagPath, RemoteDefinitionLocation);
                writer.SetBoolean(TagCacheResults, _cacheSolveResults);
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

                bool cacheResults = _cacheSolveResults;
                if (reader.TryGetBoolean(TagCacheResults, ref cacheResults))
                    _cacheSolveResults = cacheResults;
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
                var stream = GetType().Assembly.GetManifestResourceStream("Hops.resources.Hops_24x24.png");
                return new System.Drawing.Bitmap(stream);
            }
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            var tsi = new ToolStripMenuItem("&Path...", null, (sender, e) => { ShowSetDefinitionUi(); });
            tsi.Font = new System.Drawing.Font(tsi.Font, System.Drawing.FontStyle.Bold);
            menu.Items.Add(tsi);

            tsi = new ToolStripMenuItem($"Local Computes ({LocalServer.ActiveComputeCount})");
            var tsi_sub = new ToolStripMenuItem("1 More", null, (s, e) => {
                LocalServer.LaunchCompute(false);
            });
            tsi_sub.ToolTipText = "Launch a local compute instance";
            tsi.DropDown.Items.Add(tsi_sub);
            tsi_sub = new ToolStripMenuItem("6 Pack", null, (s, e) => {
                for(int i=0; i<6; i++)
                    LocalServer.LaunchCompute(false);
            });
            tsi_sub.ToolTipText = "Get drunk with power and launch 6 compute instances";
            tsi.DropDown.Items.Add(tsi_sub);
            menu.Items.Add(tsi);

            tsi = new ToolStripMenuItem("Cache On Server", null, (s, e) => { _cacheSolveResults = !_cacheSolveResults; });
            tsi.ToolTipText = "Tell the compute server to cache results for reuse in the future";
            tsi.Checked = _cacheSolveResults;
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
            var parent = Rhino.UI.Runtime.PlatformServiceProvider.Service.GetEtoWindow(Grasshopper.Instances.DocumentEditor.Handle);
            var form = new SetDefinitionForm(RemoteDefinitionLocation);
            if(form.ShowModal(parent))
            {
                RemoteDefinitionLocation = form.Path;
            }
        }

        string RemoteDefinitionLocation
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
                if (!string.Equals(RemoteDefinitionLocation, value, StringComparison.OrdinalIgnoreCase))
                {
                    if(_remoteDefinition != null)
                    {
                        _remoteDefinition.Dispose();
                        _remoteDefinition = null;
                    }
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        _remoteDefinition = new RemoteDefinition(value);
                        DefineInputsAndOutputs();
                        this.Message = DefinitionName;
                    }
                }
            }
        }

        string DefinitionName
        {
            get
            {
                if(!string.IsNullOrWhiteSpace(RemoteDefinitionLocation))
                {
                    string[] pieces = RemoteDefinitionLocation.Split(new char[] { '/', '\\' });
                    return pieces[pieces.Length - 1];
                }
                return "";
            }
        }

        void DefineInputsAndOutputs()
        {
            ClearRuntimeMessages();
            string description = _remoteDefinition.GetDescription();
            if (!string.IsNullOrWhiteSpace(description) && !Description.Equals(description))
            {
                Description = description;
            }
            var inputs = _remoteDefinition.GetInputParams();
            var outputs = _remoteDefinition.GetOutputParams();

            bool buildInputs = inputs != null;
            bool buildOutputs = outputs != null;
            // check to see if the existing params match
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

            if (buildInputs && inputs != null)
            {
                var mgr = CreateInputManager();
                foreach (var kv in inputs)
                {
                    string name = kv.Key;
                    var (input, param) = kv.Value;
                    GH_ParamAccess access = GH_ParamAccess.list;
                    if (input.AtLeast == 1 && input.AtMost == 1)
                        access = GH_ParamAccess.item;
                    string inputDescription = name;
                    if (!string.IsNullOrWhiteSpace(input.Description))
                        inputDescription = input.Description;
                    switch (param)
                    {
                        case Grasshopper.Kernel.Parameters.Param_Arc _:
                            mgr.AddArcParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Boolean _:
                            mgr.AddBooleanParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Box _:
                            mgr.AddBoxParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Brep _:
                            mgr.AddBrepParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Circle _:
                            mgr.AddCircleParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Colour _:
                            mgr.AddColourParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Complex _:
                            mgr.AddComplexNumberParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Culture _:
                            mgr.AddCultureParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Curve _:
                            mgr.AddCurveParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Field _:
                            mgr.AddFieldParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_FilePath _:
                            mgr.AddTextParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_GenericObject _:
                            throw new Exception("generic param not supported");
                        case Grasshopper.Kernel.Parameters.Param_Geometry _:
                            mgr.AddGeometryParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Group _:
                            throw new Exception("group param not supported");
                        case Grasshopper.Kernel.Parameters.Param_Guid _:
                            throw new Exception("guid param not supported");
                        case Grasshopper.Kernel.Parameters.Param_Integer _:
                            mgr.AddIntegerParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Interval _:
                            mgr.AddIntervalParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Interval2D _:
                            mgr.AddInterval2DParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_LatLonLocation _:
                            throw new Exception("latlonlocation param not supported");
                        case Grasshopper.Kernel.Parameters.Param_Line _:
                            mgr.AddLineParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Matrix _:
                            mgr.AddMatrixParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Mesh _:
                            mgr.AddMeshParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_MeshFace _:
                            mgr.AddMeshFaceParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_MeshParameters _:
                            throw new Exception("meshparameters paran not supported");
                        case Grasshopper.Kernel.Parameters.Param_Number _:
                            mgr.AddNumberParameter(name, name, inputDescription, access);
                            break;
                        //case Grasshopper.Kernel.Parameters.Param_OGLShader:
                        case Grasshopper.Kernel.Parameters.Param_Plane _:
                            mgr.AddPlaneParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Point _:
                            mgr.AddPointParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Rectangle _:
                            mgr.AddRectangleParameter(name, name, inputDescription, access);
                            break;
                        //case Grasshopper.Kernel.Parameters.Param_ScriptVariable _:
                        case Grasshopper.Kernel.Parameters.Param_String _:
                            mgr.AddTextParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_StructurePath _:
                            mgr.AddPathParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_SubD _:
                            mgr.AddSubDParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Surface _:
                            mgr.AddSurfaceParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Time _:
                            mgr.AddTimeParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Transform _:
                            mgr.AddTransformParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Vector _:
                            mgr.AddVectorParameter(name, name, inputDescription, access);
                            break;
                        case Grasshopper.Kernel.Special.GH_NumberSlider _:
                            mgr.AddNumberParameter(name, name, inputDescription, access);
                            break;
                    }
                }
            }
            if (buildOutputs && outputs != null)
            {
                var mgr = CreateOutputManager();
                foreach (var kv in outputs)
                {
                    string name = kv.Key;
                    var param = kv.Value;
                    switch (param)
                    {
                        case Grasshopper.Kernel.Parameters.Param_Arc _:
                            mgr.AddArcParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Boolean _:
                            mgr.AddBooleanParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Box _:
                            mgr.AddBoxParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Brep _:
                            mgr.AddBrepParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Circle _:
                            mgr.AddCircleParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Colour _:
                            mgr.AddColourParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Complex _:
                            mgr.AddComplexNumberParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Culture _:
                            mgr.AddCultureParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Curve _:
                            mgr.AddCurveParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Field _:
                            mgr.AddFieldParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_FilePath _:
                            mgr.AddTextParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_GenericObject _:
                            throw new Exception("generic param not supported");
                        case Grasshopper.Kernel.Parameters.Param_Geometry _:
                            mgr.AddGeometryParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Group _:
                            throw new Exception("group param not supported");
                        case Grasshopper.Kernel.Parameters.Param_Guid _:
                            throw new Exception("guid param not supported");
                        case Grasshopper.Kernel.Parameters.Param_Integer _:
                            mgr.AddIntegerParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Interval _:
                            mgr.AddIntervalParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Interval2D _:
                            mgr.AddInterval2DParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_LatLonLocation _:
                            throw new Exception("latlonlocation param not supported");
                        case Grasshopper.Kernel.Parameters.Param_Line _:
                            mgr.AddLineParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Matrix _:
                            mgr.AddMatrixParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Mesh _:
                            mgr.AddMeshParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_MeshFace _:
                            mgr.AddMeshFaceParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_MeshParameters _:
                            throw new Exception("meshparameters paran not supported");
                        case Grasshopper.Kernel.Parameters.Param_Number _:
                            mgr.AddNumberParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        //case Grasshopper.Kernel.Parameters.Param_OGLShader:
                        case Grasshopper.Kernel.Parameters.Param_Plane _:
                            mgr.AddPlaneParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Point _:
                            mgr.AddPointParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Rectangle _:
                            mgr.AddRectangleParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        //case Grasshopper.Kernel.Parameters.Param_ScriptVariable _:
                        case Grasshopper.Kernel.Parameters.Param_String _:
                            mgr.AddTextParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_StructurePath _:
                            mgr.AddPathParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_SubD _:
                            mgr.AddSubDParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Surface _:
                            mgr.AddSurfaceParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Time _:
                            mgr.AddTimeParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Transform _:
                            mgr.AddTransformParameter(name, name, name, GH_ParamAccess.item);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Vector _:
                            mgr.AddVectorParameter(name, name, name, GH_ParamAccess.item);
                            break;
                    }
                }
            }

            if (buildInputs || buildOutputs)
            {
                Params.OnParametersChanged();
                Grasshopper.Instances.ActiveCanvas?.Invalidate();
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
    }
}
