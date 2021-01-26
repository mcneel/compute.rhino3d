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
    public class RemoteSolveComponent : GH_Component, IGH_VariableParameterComponent
    {
        int _majorVersion = 0;
        int _minorVersion = 1;

        public RemoteSolveComponent()
          : base("Remote Solve", "Remote", "Solve an external definition using Rhino Compute", "Params", "Util")
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
            _remoteDefinition.SolveInstance(DA, Params.Output, this);
        }

        const string TagVersion = "RemoteSolveVersion";
        const string TagPath = "RemoteDefinitionLocation";
        public override bool Write(GH_IWriter writer)
        {
            bool rc = base.Write(writer);
            if (rc)
            {
                writer.SetVersion(TagVersion, _majorVersion, _minorVersion, 0);
                writer.SetString(TagPath, RemoteDefinitionLocation);
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
                RemoteDefinitionLocation = reader.GetString(TagPath);
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
                var stream = GetType().Assembly.GetManifestResourceStream("Compute.Components.resources.ComputeLogo_24x24.png");
                return new System.Drawing.Bitmap(stream);
            }
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            var tsi = new ToolStripMenuItem("&Path...", null, (sender, e) => { ShowSetDefinitionUi(); });
            tsi.Font = new System.Drawing.Font(tsi.Font, System.Drawing.FontStyle.Bold);
            menu.Items.Add(tsi);
        }

        class ComponentAttributes : GH_ComponentAttributes
        {
            readonly Action _doubleClickAction;
            public ComponentAttributes(GH_Component component, Action doubleClickAction)
              : base(component)
            {
                _doubleClickAction = doubleClickAction;
            }

            public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
            {
                _doubleClickAction();
                return base.RespondToMouseDoubleClick(sender, e);
            }
        }

        public override void CreateAttributes()
        {
            Attributes = new ComponentAttributes(this, ShowSetDefinitionUi);
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

        RemoteDefinition _remoteDefinition = null;
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

            bool buildInputs = true;
            bool buildOutputs = true;
            // check to see if the existing params match
            if (Params.Input.Count == inputs.Count)
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
            if (Params.Output.Count == outputs.Count)
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
                    switch (param)
                    {
                        case Grasshopper.Kernel.Parameters.Param_Arc _:
                            mgr.AddArcParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Boolean _:
                            mgr.AddBooleanParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Box _:
                            mgr.AddBoxParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Brep _:
                            mgr.AddBrepParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Circle _:
                            mgr.AddCircleParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Colour _:
                            mgr.AddColourParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Complex _:
                            mgr.AddComplexNumberParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Culture _:
                            mgr.AddCultureParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Curve _:
                            mgr.AddCurveParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Field _:
                            mgr.AddFieldParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_FilePath _:
                            mgr.AddTextParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_GenericObject _:
                            throw new Exception("generic param not supported");
                        case Grasshopper.Kernel.Parameters.Param_Geometry _:
                            mgr.AddGeometryParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Group _:
                            throw new Exception("group param not supported");
                        case Grasshopper.Kernel.Parameters.Param_Guid _:
                            throw new Exception("guid param not supported");
                        case Grasshopper.Kernel.Parameters.Param_Integer _:
                            mgr.AddIntegerParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Interval _:
                            mgr.AddIntervalParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Interval2D _:
                            mgr.AddInterval2DParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_LatLonLocation _:
                            throw new Exception("latlonlocation param not supported");
                        case Grasshopper.Kernel.Parameters.Param_Line _:
                            mgr.AddLineParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Matrix _:
                            mgr.AddMatrixParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Mesh _:
                            mgr.AddMeshParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_MeshFace _:
                            mgr.AddMeshFaceParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_MeshParameters _:
                            throw new Exception("meshparameters paran not supported");
                        case Grasshopper.Kernel.Parameters.Param_Number _:
                            mgr.AddNumberParameter(name, name, name, access);
                            break;
                        //case Grasshopper.Kernel.Parameters.Param_OGLShader:
                        case Grasshopper.Kernel.Parameters.Param_Plane _:
                            mgr.AddPlaneParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Point _:
                            mgr.AddPointParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Rectangle _:
                            mgr.AddRectangleParameter(name, name, name, access);
                            break;
                        //case Grasshopper.Kernel.Parameters.Param_ScriptVariable _:
                        case Grasshopper.Kernel.Parameters.Param_String _:
                            mgr.AddTextParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_StructurePath _:
                            mgr.AddPathParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Surface _:
                            mgr.AddSurfaceParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Time _:
                            mgr.AddTimeParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Transform _:
                            mgr.AddTransformParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Parameters.Param_Vector _:
                            mgr.AddVectorParameter(name, name, name, access);
                            break;
                        case Grasshopper.Kernel.Special.GH_NumberSlider _:
                            mgr.AddNumberParameter(name, name, name, access);
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

        class SetDefinitionForm : Eto.Forms.Dialog<bool>
        {
            public SetDefinitionForm(string currentPath)
            {
                Path = currentPath;

                Title = "Set Definition";
                
                bool windows = Rhino.Runtime.HostUtils.RunningOnWindows;
                DefaultButton = new Eto.Forms.Button { Text = windows ? "OK" : "Apply" };
                DefaultButton.Click += (sender, e) => Close(true);
                AbortButton = new Eto.Forms.Button { Text = "C&ancel" };
                AbortButton.Click += (sender, e) => Close(false);
                var buttons = new Eto.Forms.TableLayout();
                if (windows)
                {
                    buttons.Spacing = new Eto.Drawing.Size(5, 5);
                    buttons.Rows.Add(new Eto.Forms.TableRow(null, DefaultButton, AbortButton));
                }
                else
                    buttons.Rows.Add(new Eto.Forms.TableRow(null, AbortButton, DefaultButton));
                var textbox = new Eto.Forms.TextBox();
                textbox.Size = new Eto.Drawing.Size(250, -1);
                textbox.PlaceholderText = "URL or Path";
                if( !string.IsNullOrWhiteSpace(Path))
                {
                    textbox.Text = Path;
                }
                var filePickButton = new Rhino.UI.Controls.ImageButton();
                filePickButton.Image = Rhino.Resources.Assets.Rhino.Eto.Bitmaps.TryGet(Rhino.Resources.ResourceIds.FolderopenPng, new Eto.Drawing.Size(24, 24));
                filePickButton.Click += (sender, e) =>
                {
                    var dlg = new Eto.Forms.OpenFileDialog();
                    dlg.Filters.Add(new Eto.Forms.FileFilter("Grasshopper Document", ".gh", ".ghx"));
                    if(dlg.ShowDialog(this) == Eto.Forms.DialogResult.Ok)
                    {
                        textbox.Text = dlg.FileName;
                    }
                };
                var locationRow = new Eto.Forms.StackLayout {
                    Orientation = Eto.Forms.Orientation.Horizontal,
                    Spacing = buttons.Spacing.Width,
                    Items = { textbox, filePickButton }
                };
                Content = new Eto.Forms.TableLayout
                {
                    Padding = new Eto.Drawing.Padding(10),
                    Spacing = new Eto.Drawing.Size(5, 5),
                    Rows = {
                        new Eto.Forms.TableRow { ScaleHeight = true, Cells = { locationRow } },
                        buttons
                    }
                };
                Closed += (s, e) => { Path = textbox.Text; };
            }

            public string Path
            {
                get;
                set;
            }
        }

    }
}
