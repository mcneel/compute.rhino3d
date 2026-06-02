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
using System.IO;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using Rhino;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel.Expressions;
using Serilog.Events;
using Serilog.Templates;
using Serilog;
using System.Linq;

namespace Hops
{
    public class HopsLog : GH_AssemblyPriority
    {
        public static ILogger Log { get; private set; }
        public override GH_LoadingInstruction PriorityLoad()
        {
            Config.Load();

            var date = System.DateTime.Now;
            var path = System.IO.Path.Combine(Config.LogPath, $"log-hops-inside-{System.Diagnostics.Process.GetCurrentProcess().ProcessName}-{date:yyyyMMdd}.txt");
            var limit = Config.LogRetainDays;
            var level = Config.Debug ? LogEventLevel.Debug : LogEventLevel.Information;

            var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(level)
            .WriteTo.File(new ExpressionTemplate("HC   [{@t:HH:mm:ss} {@l:u3}] {@m}\n{@x}"), path);
            Log = loggerConfig.CreateLogger();

            Log.Information($"Hops logging started at {DateTime.Now.ToLocalTime()}");

            return GH_LoadingInstruction.Proceed;
        }
    }

    [Guid("C69BB52C-88BA-4640-B69F-188D111029E8")]
    public class HopsComponent : GH_TaskCapableComponent<Schema>, IGH_VariableParameterComponent
    {
        #region Fields
        int majorVersion = 0;
        int minorVersion = 1;
        RemoteDefinition remoteDefinition = null;
        bool cacheResultsInMemory = true;
        bool cacheResultsOnServer = true;
        bool remoteDefinitionRequiresRebuild = false;
        bool synchronous = true;
        bool showEnabledInput = false;
        bool enabledThisSolve = true;
        bool showPathInput = false;
        int iteration = 0;

        SolveDataList workingSolveList;
        int solveSerialNumber = 0;
        int solveRecursionLevel = 0;
        Schema lastCreatedSchema = null;
        static bool isHeadless = false;
        static int currentSolveSerialNumber = 1;
        #endregion

        static HopsComponent()
        {
            if (!Rhino.Runtime.HostUtils.RunningOnWindows)
                return;
            if (Rhino.RhinoApp.IsRunningHeadless)
                return;
            // Only auto-spawn when the user has chosen the local server source. The remote URL
            // may still be stored in settings (so it persists across radio toggles), so check
            // the explicit toggle rather than the URL list.
            if (!Hops.HopsAppSettings.UseLocalServer)
                return;
            if (Hops.HopsAppSettings.LaunchWorkerAtStart)
            {
                Servers.StartServerOnLaunch();
            }
        }

        public HopsComponent()
          : base("Hops", "Hops", "Solve an external definition using Rhino Compute", "Params", "Util")
        {
            isHeadless = Rhino.RhinoApp.IsRunningHeadless;
        }

        public override Guid ComponentGuid => GetType().GUID;
        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        protected override string HtmlHelp_Source()
        {
            return "GOTO:https://developer.rhino3d.com/guides/compute/hops-component/";
        }

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
            enabledThisSolve = true;
            lastCreatedSchema = null;
            solveRecursionLevel = 0;

            if (isHeadless &&
                    OnPingDocument() is GH_Document doc)
            {
                if (doc.ConstantServer.TryGetValue("ComputeRecursionLevel", out GH_Variant recursionLevel))
                    // compute will set the ComputeRecursionLevel 
                    solveRecursionLevel = recursionLevel._Int;
                else
                    solveRecursionLevel = HopsAppSettings.RecursionLimit;
            }

            if (!solvedCallback)
            {
                solveSerialNumber = currentSolveSerialNumber++;
                if (workingSolveList != null)
                    workingSolveList.Canceled = true;
                workingSolveList = new SolveDataList(solveSerialNumber, this, remoteDefinition, cacheResultsInMemory);
            }

            base.BeforeSolveInstance();
        }

        bool solvedCallback = false;
        public void OnWorkingListComplete()
        {
            solvedCallback = true;
            if (workingSolveList.SolvedFor(solveSerialNumber))
            {
                ExpireSolution(true);
            }
            solvedCallback = false;
        }

        public int SolveSerialNumber => solveSerialNumber;

        HttpRecord httpRecord;
        public HttpRecord HttpRecord
        {
            get
            {
                if (httpRecord == null)
                    httpRecord = new HttpRecord();
                return httpRecord; 
            }
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!enabledThisSolve)
                return;
            iteration++;

            // Limit recursive calls on compute
            if (isHeadless && solveRecursionLevel > HopsAppSettings.RecursionLimit)
            {
                // Don't allow hops components to run on compute for now. Recursive calls will lock
                HopsAddRuntimeMessage(
                    GH_RuntimeMessageLevel.Error,
                    $"Hops recursion level beyond limit of {HopsAppSettings.RecursionLimit}. Please help us understand why you need this by emailing steve@mcneel.com");
                return;

            }

            if (showPathInput && DA.Iteration == 0)
            {
                string path = "";
                if (!DA.GetData("_Path", ref path))
                {
                    HopsAddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No URL or path defined for definition");
                    return;
                }

                if (!string.Equals(path, RemoteDefinitionLocation))
                {
                    RebuildWithNewPathAndRecompute(path);
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(RemoteDefinitionLocation)  && remoteDefinition?.InternalizedDefinition == null)
            {
                HopsAddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No URL or path defined for definition");
                return;
            }

            // The remote definition failed to load its IO description (e.g. the server errored
            // while fetching the definition). DefineInputsAndOutputs surfaces this when the
            // location is set, but Grasshopper clears runtime messages at the start of every
            // solve — so re-surface it here, during SolveInstance, where it survives to render.
            // Skip solving a definition that never loaded.
            if (HttpRecord.IoResponseSchema != null && HttpRecord.IoResponseSchema.Errors.Count > 0)
            {
                foreach (var error in HttpRecord.IoResponseSchema.Errors)
                    HopsAddRuntimeMessage(GH_RuntimeMessageLevel.Error, error);
                return;
            }

            if (showEnabledInput && DA.Iteration == 0)
            {
                bool enabled = true;
                if (DA.GetData("_Enabled", ref enabled) && enabled == false)
                {
                    enabledThisSolve = false;
                    return;
                }
            }

            if (InPreSolve)
            {
                if (workingSolveList.SolvedFor(solveSerialNumber))
                {
                    var solvedTask = Task.FromResult(workingSolveList.SolvedSchema(DA.Iteration));
                    TaskList.Add(solvedTask);
                    return;
                }

                List<string> warnings;
                List<string> errors;
                var inputSchema = remoteDefinition.CreateSolveInput(DA, cacheResultsOnServer, solveRecursionLevel, out warnings, out errors);
                if (warnings != null && warnings.Count > 0)
                {
                    foreach (var warning in warnings)
                    {
                        HopsAddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning);
                    }
                    return;
                }
                if (errors != null && errors.Count > 0)
                {
                    foreach (var error in errors)
                    {
                        HopsAddRuntimeMessage(GH_RuntimeMessageLevel.Error, error);
                    }
                    return;
                }
                if (inputSchema != null)
                {
                    if (lastCreatedSchema==null)
                        lastCreatedSchema = inputSchema;
                    workingSolveList.Add(inputSchema);
                }
                return;
            }

            if (TaskList.Count == 0)
            {
                workingSolveList.StartSolving(synchronous);
                if (!synchronous)
                {
                    Message = "solving...";
                    return;
                }
                else
                {
                    for (int i = 0; i < workingSolveList.Count; i++)
                    {
                        var output = workingSolveList.SolvedSchema(i);
                        TaskList.Add(Task.FromResult(output));
                    }
                }
            }

            if (!GetSolveResults(DA, out var schema))
            {
                List<string> errors;
                List<string> warnings;
                var inputSchema = remoteDefinition.CreateSolveInput(DA, cacheResultsOnServer, solveRecursionLevel, out warnings, out errors);
                if (warnings != null && warnings.Count > 0)
                {
                    foreach (var warning in warnings)
                    {
                        HopsAddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning);
                    }
                    return;
                }
                if (errors != null && errors.Count > 0)
                {
                    foreach (var error in errors)
                    {
                        HopsAddRuntimeMessage(GH_RuntimeMessageLevel.Error, error);
                    }
                    return;
                }
                if (inputSchema != null)
                {
                    schema = remoteDefinition.Solve(inputSchema, cacheResultsInMemory);
                    if (lastCreatedSchema == null)
                        lastCreatedSchema = inputSchema;
                }
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
                remoteDefinition.SetComponentOutputs(schema, DA, Params.Output, this);
            }
        }

        const string TagVersion = "RemoteSolveVersion";
        const string TagPath = "RemoteDefinitionLocation";
        const string TagCacheResultsOnServer = "CacheSolveResults";
        const string TagCacheResultsInMemory = "CacheResultsInMemory";
        const string TagSynchronousSolve = "SynchronousSolve";
        const string TagShowEnabled = "ShowInput_Enabled";
        const string TagShowPath = "ShowInput_Path";
        const string TagInternalizeDefinitionFlag = "InternalizeFlag";
        const string TagInternalizeDefinition = "InternalizeDefinition";

        public override bool Write(GH_IWriter writer)
        {
            bool rc = base.Write(writer);
            if (rc)
            {
                writer.SetVersion(TagVersion, majorVersion, minorVersion, 0);
                writer.SetString(TagPath, RemoteDefinitionLocation);
                writer.SetBoolean(TagCacheResultsOnServer, cacheResultsOnServer);
                writer.SetBoolean(TagCacheResultsInMemory, cacheResultsInMemory);
                writer.SetBoolean(TagSynchronousSolve, synchronous);
                writer.SetBoolean(TagShowEnabled, showEnabledInput);
                writer.SetBoolean(TagShowPath, showPathInput);
                if(remoteDefinition?.InternalizedDefinition != null)
                {
                    writer.SetByteArray(TagInternalizeDefinition, remoteDefinition.InternalizedDefinition);
                }
            }
            return rc;
        }
        public override bool Read(GH_IReader reader)
        {
            bool rc = base.Read(reader);
            if (rc)
            {
                var version = reader.GetVersion(TagVersion);
                majorVersion = version.major;
                minorVersion = version.minor;
                string path = reader.GetString(TagPath);

                bool cacheResults = cacheResultsOnServer;
                if (reader.TryGetBoolean(TagCacheResultsOnServer, ref cacheResults))
                    cacheResultsOnServer = cacheResults;

                cacheResults = cacheResultsInMemory;
                if (reader.TryGetBoolean(TagCacheResultsInMemory, ref cacheResults))
                    cacheResultsInMemory = cacheResults;

                bool synchronous = this.synchronous;
                if (reader.TryGetBoolean(TagSynchronousSolve, ref synchronous))
                    this.synchronous = synchronous;

                bool showEnabled = showEnabledInput;
                if (reader.TryGetBoolean(TagShowEnabled, ref showEnabled))
                    showEnabledInput = showEnabled;

                bool showPath = showPathInput;
                if (reader.TryGetBoolean(TagShowPath, ref showPath))
                    showPathInput = showPath;

                if(reader.ItemExists(TagInternalizeDefinition))
                {
                    try
                    {
                        byte[] internalizedDefinition = reader.GetByteArray(TagInternalizeDefinition);
                        if(remoteDefinition == null)
                            remoteDefinition = RemoteDefinition.Create(null, this);
                        remoteDefinition.InternalizedDefinition = internalizedDefinition;
                        remoteDefinition.pathType = RemoteDefinition.PathType.InternalizedDefinition;
                        remoteDefinition.GetRemoteDescription();
                    }
                    catch(Exception ex)
                    {
                        HopsAddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to deserialize internalized grasshopper definition. " + ex.Message);
                    }
                }

                // set remote definition location last as it will need all of the
                // previous values to define inputs and outputs
                if (!String.IsNullOrWhiteSpace(path))
                {
                    try
                    {
                        var pathType = RemoteDefinition.GetPathType(path);
                        if (pathType == RemoteDefinition.PathType.GrasshopperDefinition)
                        {
                            if (!File.Exists(path) && !RemoteDefinition.IsWebUrl(path))
                            {
                                HopsLog.Log.Debug($"{path} does not exist. Trying to find it in the same directory as the definition.");
                                // See if the file is in the same directory as this definition. If it
                                // is then use that file. NOTE: This will change the saved path for
                                // for this component when we save the GH definition again. That may or
                                // may not be a problem; I'm not sure yet.
                                string parentDirectory = Path.GetDirectoryName(reader.ArchiveLocation);
                                if (!String.IsNullOrEmpty(parentDirectory) && Directory.Exists(parentDirectory))
                                {
                                    string remoteFileName = Path.GetFileName(path);
                                    if (!string.IsNullOrEmpty(remoteFileName))
                                    {
                                        string filePath = Path.Combine(parentDirectory, remoteFileName);
                                        if (File.Exists(filePath))
                                        {
                                            path = filePath;
                                        }
                                        else
                                        {
                                            HopsAddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Remote definition not found: {path}. Check that the file path exists.");
                                        }
                                    }
                                }
                                else
                                {
                                    HopsAddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Remote definition not found: {path}. Check that the file path exists.");
                                }
                            }
                        }
                        RemoteDefinitionLocation = path;
                    }
                    catch (System.Net.WebException)
                    {
                        // this can happen if a server is not responding and is acceptable in this
                        // case as we want to read without throwing exceptions
                    }
                }
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

        static System.Drawing.Bitmap hops24Icon;
        static System.Drawing.Bitmap hops48Icon;
        static System.Drawing.Bitmap Hops24Icon()
        {
            if (hops24Icon == null)
            {
                var stream = typeof(HopsComponent).Assembly.GetManifestResourceStream("Hops.resources.Hops_24x24.png");
                hops24Icon = new System.Drawing.Bitmap(stream);
            }
            return hops24Icon;
        }
        public static System.Drawing.Bitmap Hops48Icon()
        {
            if (hops48Icon == null)
            {
                var stream = typeof(HopsComponent).Assembly.GetManifestResourceStream("Hops.resources.Hops_48x48.png");
                hops48Icon = new System.Drawing.Bitmap(stream);
            }
            return hops48Icon;
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);

            // remove parallel computing and variable parameters menu items
            // as they aren't useful for this component
            for (int i = menu.Items.Count - 1; i >= 0; i--)
            {
                if (menu.Items[i].Text.Equals("parallel computing", StringComparison.OrdinalIgnoreCase))
                {
                    menu.Items.RemoveAt(i);
                    continue;
                }
                if (menu.Items[i].Text.Equals("variable parameters", StringComparison.OrdinalIgnoreCase))
                {
                    menu.Items.RemoveAt(i);
                    continue;
                }
            }
            
            //remove extra separator from menu
            var separator = menu.Items[menu.Items.Count - 1] as ToolStripSeparator;
            if (separator != null)
                menu.Items.RemoveAt(menu.Items.Count - 1);

            menu.Items.Add(new ToolStripSeparator());

            var tsi = new ToolStripMenuItem("&Path...", null, (sender, e) => { ShowSetDefinitionUi(); });
            if (!showPathInput)
                tsi.Font = new System.Drawing.Font(tsi.Font, System.Drawing.FontStyle.Bold);
            tsi.Enabled = !showPathInput;
            menu.Items.Add(tsi);

            tsi = AddFunctionMgrControl();
            if (tsi != null)
                menu.Items.Add(tsi);

            tsi = new ToolStripMenuItem("Internalize Definition", null, (s, e) => {
                if (File.Exists(RemoteDefinitionLocation))
                {
                    try
                    {
                        remoteDefinition.InternalizeDefinition(RemoteDefinitionLocation);
                        DefineInputsAndOutputs();
                    }
                    catch (InvalidOperationException ex)
                    {
                        HopsAddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                    }
                }
            });
            tsi.ToolTipText = "Make the referenced definition permanent and clear any existing source paths";
            if (!File.Exists(RemoteDefinitionLocation))
            {
                tsi.Enabled = false;
            }
            menu.Items.Add(tsi);

            menu.Items.Add(new ToolStripSeparator());

            tsi = new ToolStripMenuItem("Show Input: Path", null, (s, e) => {
                showPathInput = !showPathInput;
                DefineInputsAndOutputs();
            });
            tsi.ToolTipText = "Create input for path";
            tsi.Checked = showPathInput;
            menu.Items.Add(tsi);

            tsi = new ToolStripMenuItem("Show Input: Enabled", null, (s, e) => {
                showEnabledInput = !showEnabledInput;
                DefineInputsAndOutputs();
            });
            tsi.ToolTipText = "Create input for enabled";
            tsi.Checked = showEnabledInput;
            menu.Items.Add(tsi);

            tsi = new ToolStripMenuItem("Asynchronous", null, (s, e) => { synchronous = !synchronous; });
            tsi.ToolTipText = "Do not block while solving";
            tsi.Checked = !synchronous;
            menu.Items.Add(tsi);

            tsi = new ToolStripMenuItem("Cache In Memory", null, (s, e) => { cacheResultsInMemory = !cacheResultsInMemory; });
            tsi.ToolTipText = "Keep previous results in memory cache";
            tsi.Checked = cacheResultsInMemory;
            menu.Items.Add(tsi);

            tsi = new ToolStripMenuItem("Cache On Server", null, (s, e) => { cacheResultsOnServer = !cacheResultsOnServer; });
            tsi.ToolTipText = "Tell the compute server to cache results for reuse in the future";
            tsi.Checked = cacheResultsOnServer;
            menu.Items.Add(tsi);

            var exportTsi = new ToolStripMenuItem("Export");
            exportTsi.Enabled = remoteDefinition != null;
            menu.Items.Add(exportTsi);
            tsi = new ToolStripMenuItem("Export python sample...", null, (s, e) => { ExportAsPython(); });
            exportTsi.DropDownItems.Add(tsi);

            var restAPITsi = new ToolStripMenuItem("REST API");
            restAPITsi.Enabled = remoteDefinition != null;
            exportTsi.DropDownItems.Add(restAPITsi);

            tsi = new ToolStripMenuItem("Last IO request...", null, (s, e) => { ExportLastIoRequest(); });
            restAPITsi.DropDownItems.Add(tsi);

            tsi = new ToolStripMenuItem("Last IO response...", null, (s, e) => { ExportLastIoResponse(); });
            restAPITsi.DropDownItems.Add(tsi);

            tsi = new ToolStripMenuItem("Last Solve request...", null, (s, e) => { ExportLastSolveRequest(); });
            restAPITsi.DropDownItems.Add(tsi);

            tsi = new ToolStripMenuItem("Last Solve response...", null, (s, e) => { ExportLastSolveResponse(); });
            restAPITsi.DropDownItems.Add(tsi);
        }

        public ToolStripMenuItem AddFunctionMgrControl()
        {
            HopsAppSettings.InitFunctionSources();
            if (HopsAppSettings.FunctionSources.Count <= 0)
                return null;
            ToolStripMenuItem mainMenu = new ToolStripMenuItem("Available Functions", null, null, "Available Functions");
            mainMenu.DropDownItems.Clear();
            foreach (var row in HopsAppSettings.FunctionSources)
            {
                ToolStripMenuItem menuItem = new ToolStripMenuItem(row.SourceName, null, null, row.SourceName);
                GenerateFunctionPathMenu(menuItem, row);
                if (menuItem.DropDownItems.Count > 0)
                    mainMenu.DropDownItems.Add(menuItem);
            }
            //InitThumbnailViewer();
            return mainMenu;
        }

        private void GenerateFunctionPathMenu(ToolStripMenuItem menu, FunctionSourceRow row)
        {
            if (String.IsNullOrEmpty(row.SourceName) || String.IsNullOrEmpty(row.SourcePath))
                return;
            if (RemoteDefinition.IsWebUrl(row.SourcePath))
            {
                try
                {
                    using var cts = new System.Threading.CancellationTokenSource(
                        TimeSpan.FromSeconds(HopsAppSettings.HttpTimeout));
                    var getTask = HopsFunctionMgr.HttpClient.GetAsync(row.SourcePath, cts.Token);
                    if (getTask != null)
                    {
                        var responseMessage = getTask.Result;
                        var remoteSolvedData = responseMessage.Content;
                        var stringResult = remoteSolvedData.ReadAsStringAsync().Result;
                        if (string.IsNullOrEmpty(stringResult))
                        {
                            //invalid URL
                            return;
                        }
                        else
                        {
                            var response = JsonConvert.DeserializeObject<FunctionMgr_Schema[]>(stringResult);
                            if (response != null)
                            {
                                UriFunctionPathInfo functionPaths = new UriFunctionPathInfo(row.SourcePath, true);
                                functionPaths.isRoot = true;
                                functionPaths.RootURL = row.SourcePath;
                                if (!String.IsNullOrEmpty(response[0].Uri))
                                {
                                    //If the Schema Uri exists, then the response is likely from the ghhops_server.
                                    //Otherwise, let's assume the response is from the appserver
                                    foreach (FunctionMgr_Schema obj in response)
                                    {
                                        HopsFunctionMgr.SeekFunctionMenuDirs(functionPaths, obj.Uri, obj.Uri, row);
                                    }
                                }
                                else if (!String.IsNullOrEmpty(response[0].Name))
                                {
                                    foreach (FunctionMgr_Schema obj in response)
                                    {
                                        HopsFunctionMgr.SeekFunctionMenuDirs(functionPaths, "/" + obj.Name, "/" + obj.Name, row);
                                    }
                                }
                                if (functionPaths.Paths.Count != 0)
                                    functionPaths.BuildMenus(menu, new MouseEventHandler(tsm_UriClick));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    HopsLog.Log.Debug(ex, "Failed to load function-source menu for {SourcePath}", row.SourcePath);
                }
            }
            else if (Directory.Exists(row.SourcePath))
            {
                FunctionPathInfo functionPaths = new FunctionPathInfo(row.SourcePath, true);
                functionPaths.isRoot = true;

                HopsFunctionMgr.SeekFunctionMenuDirs(functionPaths);
                if (functionPaths.Paths.Count != 0)
                {
                    functionPaths.BuildMenus(menu, tsm_FileClick, HopsFunctionMgr.tsm_HoverEnter, HopsFunctionMgr.tsm_HoverExit);
                    functionPaths.RemoveEmptyMenuItems(menu, tsm_FileClick, HopsFunctionMgr.tsm_HoverEnter, HopsFunctionMgr.tsm_HoverExit);
                }
            }
        }

        private void tsm_FileClick(object sender, MouseEventArgs e)
        {
            if (!(sender is ToolStripItem))
                return;
            ToolStripItem ti = sender as ToolStripItem;

            switch (e.Button)
            {
                case MouseButtons.Left:
                    RemoteDefinitionLocation = ti.Name;
                    this.ExpireSolution(true);
                    break;
                case MouseButtons.Right:
                    try
                    {
                        Instances.DocumentEditor.ScriptAccess_OpenDocument(ti.Name);
                    }
                    catch (Exception ex)
                    {
                        HopsLog.Log.Debug(ex, "Failed to open document {DocumentName}", ti.Name);
                    }
                    break;
            }

        }

        private void tsm_UriClick(object sender, MouseEventArgs e)
        {
            if (!(sender is ToolStripItem))
                return;
            ToolStripItem ti = sender as ToolStripItem;
            RemoteDefinitionLocation = ti.Tag as string;
            this.ExpireSolution(true);
        }


        /// <summary>
        /// Used for supporting double click on the component. 
        /// </summary>
        class ComponentAttributes : GH_ComponentAttributes
        {
            HopsComponent component;
            public ComponentAttributes(HopsComponent parentComponent) : base(parentComponent)
            {
                component = parentComponent;
            }

            protected override void Render(GH_Canvas canvas, System.Drawing.Graphics graphics, GH_CanvasChannel channel)
            {
                base.Render(canvas, graphics, channel);
                if (channel == GH_CanvasChannel.Objects &&
                    GH_Canvas.ZoomFadeMedium > 0 &&
                    !string.IsNullOrWhiteSpace(component.RemoteDefinitionLocation)
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
                    component.ShowSetDefinitionUi();
                }
                catch(Exception ex)
                {
                    component.HopsAddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
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

        void ExportAsPython()
        {
            if (lastCreatedSchema == null)
            {
                Eto.Forms.MessageBox.Show("No input created. Run this component at least once", Eto.Forms.MessageBoxType.Error);
                return;
            }

            var dlg = new Eto.Forms.SaveFileDialog();
            dlg.Filters.Add(new Eto.Forms.FileFilter("Python script", ".py"));
            if (dlg.ShowDialog(Grasshopper.Instances.EtoDocumentEditor) == Eto.Forms.DialogResult.Ok)
            {
                string solveUrl = Servers.GetSolveUrl();
                if (solveUrl.EndsWith("grasshopper", StringComparison.InvariantCultureIgnoreCase))
                    solveUrl = solveUrl.Substring(0, solveUrl.Length - "grasshopper".Length);
                var sb = new System.Text.StringBuilder();
                sb.Append(@"# pip install compute_rhino3d and rhino3dm
import compute_rhino3d.Util
import compute_rhino3d.Grasshopper as gh
import rhino3dm
import json

compute_rhino3d.Util.url = '");
                sb.Append(solveUrl);
sb.Append(@"'

# create DataTree for each input
input_trees = []
");

                foreach(var val in lastCreatedSchema.Values)
                {
                    sb.AppendLine($"tree = gh.DataTree(\"{val.ParamName}\")");
                    foreach (var kv in val.InnerTree)
                    {
                        List<string> values = new List<string>();
                        foreach (var v in kv.Value)
                            values.Add(v.Data);
                        string innerData = JsonConvert.SerializeObject(values);
                        sb.AppendLine($"tree.Append([{kv.Key}], {innerData})");
                        sb.AppendLine("input_trees.append(tree)");
                        sb.AppendLine();
                    }
                }

                sb.AppendLine($"output = gh.EvaluateDefinition('{RemoteDefinitionLocation.Replace("\\", "\\\\")}', input_trees)");
                sb.Append(@"errors = output['errors']
if errors:
    print('ERRORS')
    for error in errors:
        print(error)
warnings = output['warnings']
if warnings:
    print('WARNINGS')
    for warning in warnings:
        print(warning)

values = output['values']
for value in values:
    name = value['ParamName']
    inner_tree = value['InnerTree']
    print(name)
    for path in inner_tree:
        print(path)
        values_at_path = inner_tree[path]
        for value_at_path in values_at_path:
            data = value_at_path['data']
            if isinstance(data, str) and 'archive3dm' in data:
                obj = rhino3dm.CommonObject.Decode(json.loads(data))
                print(obj)
            else:
                print(data)
");
                System.IO.File.WriteAllText(dlg.FileName, sb.ToString());
            }
        }

        void ExportLastIoRequest()
        {
            if (String.IsNullOrEmpty(HttpRecord.IoRequest))
            {
                Eto.Forms.MessageBox.Show("No IO request has been made. Run this component at least once", Eto.Forms.MessageBoxType.Error);
                return;
            }
            var dlg = new Eto.Forms.SaveFileDialog();
            dlg.Filters.Add(new Eto.Forms.FileFilter("JSON file", ".json"));
            if (dlg.ShowDialog(Grasshopper.Instances.EtoDocumentEditor) == Eto.Forms.DialogResult.Ok)
            {
                System.IO.File.WriteAllText(dlg.FileName, HttpRecord.IoRequest);
            }
        }

        void ExportLastIoResponse()
        {
            if (String.IsNullOrEmpty(HttpRecord.IoResponse))
            {
                Eto.Forms.MessageBox.Show("No IO response has been received. Run this component at least once", Eto.Forms.MessageBoxType.Error);
                return;
            }
            var dlg = new Eto.Forms.SaveFileDialog();
            dlg.Filters.Add(new Eto.Forms.FileFilter("JSON file", ".json"));
            if (dlg.ShowDialog(Grasshopper.Instances.EtoDocumentEditor) == Eto.Forms.DialogResult.Ok)
            {
                System.IO.File.WriteAllText(dlg.FileName, HttpRecord.IoResponse);
            }
        }

        void ExportLastSolveRequest()
        {
            if (String.IsNullOrEmpty(HttpRecord.SolveRequest))
            {
                Eto.Forms.MessageBox.Show("No solve request has been made. Run this component at least once", Eto.Forms.MessageBoxType.Error);
                return;
            }
            var dlg = new Eto.Forms.SaveFileDialog();
            dlg.Filters.Add(new Eto.Forms.FileFilter("JSON file", ".json"));
            if (dlg.ShowDialog(Grasshopper.Instances.EtoDocumentEditor) == Eto.Forms.DialogResult.Ok)
            {
                System.IO.File.WriteAllText(dlg.FileName, HttpRecord.SolveRequest);
            }
        }

        void ExportLastSolveResponse()
        {
            if (String.IsNullOrEmpty(HttpRecord.SolveResponse))
            {
                Eto.Forms.MessageBox.Show("No solve response has been received. Run this component at least once", Eto.Forms.MessageBoxType.Error);
                return;
            }
            var dlg = new Eto.Forms.SaveFileDialog();
            dlg.Filters.Add(new Eto.Forms.FileFilter("JSON file", ".json"));
            if (dlg.ShowDialog(Grasshopper.Instances.EtoDocumentEditor) == Eto.Forms.DialogResult.Ok)
            {
                System.IO.File.WriteAllText(dlg.FileName, HttpRecord.SolveResponse);
            }
        }

        string tempPath;
        void RebuildWithNewPathAndRecompute(string path)
        {
            if (string.Equals(path, RemoteDefinitionLocation))
                return;
            tempPath = path;
            Rhino.RhinoApp.Idle += RebuildAfterSolution;
        }

        private void RebuildAfterSolution(object sender, EventArgs e)
        {
            var doc = OnPingDocument();
            if (doc != null && doc.SolutionDepth == 0)
            {
                Rhino.RhinoApp.Idle -= RebuildAfterSolution;
                RemoteDefinitionLocation = tempPath;
                tempPath = null;
            }
        }

        // keep public in case external C# code wants to set this
        public string RemoteDefinitionLocation
        {
            get
            {
                if (remoteDefinition != null)
                {
                    return remoteDefinition.Path;
                }
                return string.Empty;
            }
            set
            {
                // Always rebuild the remote definition information when setting this property.
                // This way you can poke the path button to force a refresh in case the situation
                // on the server has changed.
                {
                    if(remoteDefinition != null)
                    {
                        remoteDefinition.Dispose();
                        remoteDefinition = null;
                    }
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        remoteDefinition = RemoteDefinition.Create(value, this);
                        HopsLog.Log.Debug($"Remote definition location set to {value}");
                        DefineInputsAndOutputs();
                    }
                }
            }
        }

        public override void CollectData()
        {
            base.CollectData();
            if (showPathInput &&
                !string.IsNullOrWhiteSpace(RemoteDefinitionLocation) &&
                !Params.Input[0].VolatileData.IsEmpty)
            {
                var path = Params.Input[0].VolatileData.get_Path(0);
                string newPath = Params.Input[0].VolatileData.get_Branch(path)[0].ToString();
                if (!string.Equals(newPath, RemoteDefinitionLocation))
                    RebuildWithNewPathAndRecompute(newPath);
            }
        }

        static string DecodeStringDefault(string data)
        {
            try
            {
                return JsonConvert.DeserializeObject<string>(data);
            }
            catch (Newtonsoft.Json.JsonException)
            {
                // Intentional fallback: when the data isn't valid JSON-encoded string syntax,
                // treat it as already-decoded and just unescape any backslash sequences.
                return System.Text.RegularExpressions.Regex.Unescape(data);
            }
        }

        // Shared helper for DefineInputsAndOutputs' input-side switch. Builds the parameter via
        // the supplied addParam delegate, then applies any default values from the input schema.
        // Handles both tree-structured defaults (input.Default.ToString() contains "InnerTree")
        // and scalar defaults. Per-type variation is captured by two delegates:
        //   wireToGoo:    converts a single ResthopperObject's payload into a TGoo (tree path)
        //   defaultToGoo: converts input.Default.ToString() into a TGoo (scalar path). Pass null
        //                 for types whose original code only handled the tree path (Colour,
        //                 Matrix, MeshFace, Transform, Geometry).
        // Either callback may return null to skip an item / skip the scalar default.
        static int AddInputWithDefault<TGoo>(
            GH_InputParamManager mgr, InputParamSchema input,
            string name, string nickname, string description, GH_ParamAccess access,
            Func<GH_InputParamManager, string, string, string, GH_ParamAccess, int> addParam,
            Func<ResthopperObject, TGoo> wireToGoo,
            Func<string, TGoo> defaultToGoo,
            HopsComponent component)
            where TGoo : class, IGH_Goo
        {
            int paramIndex = addParam(mgr, name, nickname, description, access);
            if (input.Default == null)
                return paramIndex;
            var pParam = mgr[paramIndex] as GH_PersistentParam<TGoo>;
            if (pParam == null)
                return paramIndex;

            if (input.Default.ToString().Contains("InnerTree"))
            {
                var tree = JsonConvert.DeserializeObject<Resthopper.IO.DataTree<ResthopperObject>>(input.Default.ToString());
                if (tree.InnerTree != null)
                {
                    pParam.PersistentData.Clear();
                    foreach (var branch in tree.InnerTree)
                    {
                        var pathElements = branch.Key.Trim('{', '}').Split(';');
                        GH_Path path = new GH_Path(Array.ConvertAll(pathElements, int.Parse));
                        foreach (var item in branch.Value)
                        {
                            var goo = wireToGoo(item);
                            if (goo != null)
                                pParam.PersistentData.Append(goo, path);
                        }
                    }
                }
            }
            else if (defaultToGoo != null)
            {
                try
                {
                    var goo = defaultToGoo(input.Default.ToString());
                    if (goo != null)
                        pParam.PersistentData.Append(goo);
                }
                catch (Exception e)
                {
                    component.HopsAddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
                }
            }
            return paramIndex;
        }

        public void HopsAddRuntimeMessage(GH_RuntimeMessageLevel level, string message)
        {
            if(HopsLog.Log is object)
            {
                switch (level)
                {
                    case GH_RuntimeMessageLevel.Remark:
                        HopsLog.Log.Information(message);
                        break;
                    case GH_RuntimeMessageLevel.Warning:
                        HopsLog.Log.Warning(message);
                        break;
                    case GH_RuntimeMessageLevel.Error:
                        HopsLog.Log.Error(message);
                        break;
                    default:
                        HopsLog.Log.Debug(message);
                        break;
                }
            }

            AddRuntimeMessage(level, message);
        }

        void DefineInputsAndOutputs()
        {
            if (remoteDefinition != null)
            {
                ClearRuntimeMessages();
                string description = remoteDefinition.GetDescription(out System.Drawing.Bitmap customIcon);

                if (remoteDefinition.IsNotRespondingUrl())
                {
                    var msg = $"Unable to connect to {RemoteDefinitionLocation} within the configured {HopsAppSettings.HttpTimeout}-second HTTP timeout (raise it in the Hops settings panel if the server is just slow).";
                    var errSchema = new IoResponseSchema();
                    errSchema.Errors.Add(msg);
                    HttpRecord.IoResponseSchema = errSchema;
                    HopsAddRuntimeMessage(GH_RuntimeMessageLevel.Error, msg);
                    Grasshopper.Instances.ActiveCanvas?.Invalidate();
                    return;
                }

                if (remoteDefinition.IsInvalidUrl())
                {
                    var msg = $"The URL {RemoteDefinitionLocation} responded but did not return any Hops definition data. Verify it points to a Hops/Compute endpoint or to a Grasshopper .gh/.ghx file.";
                    var errSchema = new IoResponseSchema();
                    errSchema.Errors.Add(msg);
                    HttpRecord.IoResponseSchema = errSchema;
                    HopsAddRuntimeMessage(GH_RuntimeMessageLevel.Error, msg);
                    Grasshopper.Instances.ActiveCanvas?.Invalidate();
                    return;
                }
                if(HttpRecord.IoResponseSchema != null && HttpRecord.IoResponseSchema.Errors.Count > 0)
                {
                    foreach(var error in HttpRecord.IoResponseSchema.Errors)
                    {
                        HopsAddRuntimeMessage(GH_RuntimeMessageLevel.Error, error);
                        Grasshopper.Instances.ActiveCanvas?.Invalidate();
                        return;
                    }
                }
                if(HttpRecord.IoResponseSchema != null && HttpRecord.IoResponseSchema.Warnings.Count > 0)
                {
                    foreach (var warning in HttpRecord.IoResponseSchema.Warnings)
                    {
                        HopsAddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning);
                    }
                }

                if (!string.IsNullOrWhiteSpace(description) && !Description.Equals(description))
                {
                    Description = description;
                }
                var inputs = remoteDefinition.GetInputParams();
                var outputs = remoteDefinition.GetOutputParams();

                bool buildInputs = inputs != null;
                bool buildOutputs = outputs != null;

                // check to see if the existing params match
                Dictionary<string, List<IGH_Param>> inputSources = new Dictionary<string, List<IGH_Param>>();
                foreach (var param in Params.Input)
                {
                    inputSources.Add(param.Name, new List<IGH_Param>(param.Sources));
                }

                Dictionary<string, List<IGH_Param>> outputRecipients = new Dictionary<string, List<IGH_Param>>();
                foreach (var param in Params.Output)
                {
                    outputRecipients.Add(param.Name, new List<IGH_Param>(param.Recipients));
                }

                int inputCount = inputs!=null ? inputs.Count : 0;
                if (showEnabledInput)
                    inputCount++;
                if (showPathInput)
                    inputCount++;
     
                if(iteration == 0)
                {
                    if (buildInputs && Params.Input.Count == inputCount)
                    {
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
                    HopsLog.Log.Debug($"Hops component rebuilding input parameters...");

                    var mgr = CreateInputManager();

                    if (showPathInput)
                    {
                        const string name = "_Path";
                        int paramIndex = mgr.AddTextParameter(name, "Path", "URL to remote process", GH_ParamAccess.item);
                        if (paramIndex >= 0 && inputSources.TryGetValue(name, out List<IGH_Param> rehookInputs))
                        {
                            foreach (var rehookInput in rehookInputs)
                                Params.Input[paramIndex].AddSource(rehookInput);
                        }
                    }

                    if (showEnabledInput)
                    {
                        const string name = "_Enabled";
                        int paramIndex = mgr.AddBooleanParameter(name, "Enabled", "Enabled state for solving", GH_ParamAccess.item);
                        if (paramIndex >= 0 && inputSources.TryGetValue(name, out List<IGH_Param> rehookInputs))
                        {
                            foreach (var rehookInput in rehookInputs)
                                Params.Input[paramIndex].AddSource(rehookInput);
                        }
                    }

                    foreach (var kv in inputs)
                    {
                        string name = kv.Key;
                        var (input, param) = kv.Value;
                        GH_ParamAccess access = RemoteDefinition.AccessFromInput(input);
                        string inputDescription = name;
                        if (!string.IsNullOrWhiteSpace(input.Description))
                            inputDescription = input.Description;

                        if (input.Minimum != null)
                        {
                            double min = Convert.ToDouble(input.Minimum);
                            int digits = min.ToString(System.Globalization.CultureInfo.InvariantCulture).SkipWhile(c => c != '.').Skip(1).Count();
                            string formatter = digits < 1 ? "N1" : "N" + digits.ToString();
                            inputDescription += $"\nMinimum: {min.ToString(formatter, System.Globalization.CultureInfo.InvariantCulture)}";
                        }

                        if (input.Maximum != null)
                        {
                            double max = Convert.ToDouble(input.Maximum);
                            int digits = max.ToString(System.Globalization.CultureInfo.InvariantCulture).SkipWhile(c => c != '.').Skip(1).Count();
                            string formatter = digits < 1 ? "N1" : "N" + digits.ToString();
                            inputDescription += $"\nMaximum: {max.ToString(formatter, System.Globalization.CultureInfo.InvariantCulture)}";
                        }

                        string nickname = name;
                        if (!string.IsNullOrWhiteSpace(input.Nickname))
                            nickname = input.Nickname;
                        int paramIndex = -1;
                        switch (param)
                        {
                            case Grasshopper.Kernel.Parameters.Param_Arc _:
                                paramIndex = AddInputWithDefault<GH_Arc>(
                                    mgr, input, name, nickname, inputDescription, access,
                                    (m, n, nn, d, a) => m.AddArcParameter(n, nn, d, a),
                                    r => new GH_Arc(JsonConvert.DeserializeObject<Arc>(r.Data.ToString())),
                                    def => { var v = JsonConvert.DeserializeObject<Arc>(def); return v.IsValid ? new GH_Arc(v) : null; },
                                    this);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Boolean _:
                                paramIndex = AddInputWithDefault<GH_Boolean>(
                                    mgr, input, name, nickname, inputDescription, access,
                                    (m, n, nn, d, a) => m.AddBooleanParameter(n, nn, d, a),
                                    r => new GH_Boolean(Convert.ToBoolean(r.Data.ToString())),
                                    def => bool.TryParse(def, out bool v) ? new GH_Boolean(v) : null,
                                    this);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Box _:
                                paramIndex = AddInputWithDefault<GH_Box>(
                                    mgr, input, name, nickname, inputDescription, access,
                                    (m, n, nn, d, a) => m.AddBoxParameter(n, nn, d, a),
                                    r => new GH_Box(JsonConvert.DeserializeObject<Box>(r.Data.ToString())),
                                    def => { var v = JsonConvert.DeserializeObject<Box>(def); return v.IsValid ? new GH_Box(v) : null; },
                                    this);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Brep _:
                                paramIndex = AddInputWithDefault<GH_Brep>(
                                    mgr, input, name, nickname, inputDescription, access,
                                    (m, n, nn, d, a) => m.AddBrepParameter(n, nn, d, a),
                                    r => new GH_Brep(JsonConvert.DeserializeObject<Brep>(r.Data.ToString())),
                                    def => { var v = JsonConvert.DeserializeObject<Brep>(def); return v.IsValid ? new GH_Brep(v) : null; },
                                    this);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Circle _:
                                paramIndex = AddInputWithDefault<GH_Circle>(
                                    mgr, input, name, nickname, inputDescription, access,
                                    (m, n, nn, d, a) => m.AddCircleParameter(n, nn, d, a),
                                    r => new GH_Circle(JsonConvert.DeserializeObject<Circle>(r.Data.ToString())),
                                    def => { var v = JsonConvert.DeserializeObject<Circle>(def); return v.IsValid ? new GH_Circle(v) : null; },
                                    this);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Colour _:
                                paramIndex = AddInputWithDefault<GH_Colour>(
                                    mgr, input, name, nickname, inputDescription, access,
                                    (m, n, nn, d, a) => m.AddColourParameter(n, nn, d, a),
                                    r => new GH_Colour(JsonConvert.DeserializeObject<Color>(r.Data.ToString())),
                                    null,
                                    this);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Complex _:
                                paramIndex = mgr.AddComplexNumberParameter(name, nickname, inputDescription, access);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Culture _:
                                paramIndex = mgr.AddCultureParameter(name, nickname, inputDescription, access);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Curve _:
                                paramIndex = AddInputWithDefault<GH_Curve>(
                                    mgr, input, name, nickname, inputDescription, access,
                                    (m, n, nn, d, a) => m.AddCurveParameter(n, nn, d, a),
                                    r => new GH_Curve(JsonConvert.DeserializeObject<Curve>(r.Data.ToString())),
                                    def => { var v = JsonConvert.DeserializeObject<Curve>(def); return v.IsValid ? new GH_Curve(v) : null; },
                                    this);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Field _:
                                paramIndex = mgr.AddFieldParameter(name, nickname, inputDescription, access);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_FilePath _:
                                paramIndex = AddInputWithDefault<GH_String>(
                                    mgr, input, name, nickname, inputDescription, access,
                                    (m, n, nn, d, a) => m.AddTextParameter(n, nn, d, a),
                                    r => new GH_String(DecodeStringDefault(r.Data.ToString())),
                                    def => new GH_String(def),
                                    this);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_GenericObject _:
                                throw new Exception("Generic param not supported");
                            case Grasshopper.Kernel.Parameters.Param_Geometry _:
                                paramIndex = AddInputWithDefault<IGH_GeometricGoo>(
                                    mgr, input, name, nickname, inputDescription, access,
                                    (m, n, nn, d, a) => m.AddGeometryParameter(n, nn, d, a),
                                    r => GH_Convert.ToGeometricGoo(JsonConvert.DeserializeObject(r.Data.ToString(), typeof(RhinoApp).Assembly.GetType(r.Type))),
                                    null,
                                    this);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Group _:
                                throw new Exception("Group param not supported");
                            case Grasshopper.Kernel.Parameters.Param_Guid _:
                                throw new Exception("Guid param not supported");
                            case Grasshopper.Kernel.Parameters.Param_Integer _:
                                paramIndex = AddInputWithDefault<GH_Integer>(
                                    mgr, input, name, nickname, inputDescription, access,
                                    (m, n, nn, d, a) => m.AddIntegerParameter(n, nn, d, a),
                                    r => new GH_Integer(Convert.ToInt32(r.Data.ToString())),
                                    def => int.TryParse(def, out int v) ? new GH_Integer(v) : null,
                                    this);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Interval _:
                                paramIndex = AddInputWithDefault<GH_Interval>(
                                    mgr, input, name, nickname, inputDescription, access,
                                    (m, n, nn, d, a) => m.AddIntervalParameter(n, nn, d, a),
                                    r => new GH_Interval(JsonConvert.DeserializeObject<Interval>(r.Data.ToString())),
                                    def => { var v = JsonConvert.DeserializeObject<Interval>(def); return v.IsValid ? new GH_Interval(v) : null; },
                                    this);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Interval2D _:
                                paramIndex = mgr.AddInterval2DParameter(name, nickname, inputDescription, access);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_LatLonLocation _:
                                throw new Exception("Lat Lon Location param not supported");
                            case Grasshopper.Kernel.Parameters.Param_Line _:
                                paramIndex = AddInputWithDefault<GH_Line>(
                                    mgr, input, name, nickname, inputDescription, access,
                                    (m, n, nn, d, a) => m.AddLineParameter(n, nn, d, a),
                                    r => new GH_Line(JsonConvert.DeserializeObject<Line>(r.Data.ToString())),
                                    def => { var v = JsonConvert.DeserializeObject<Line>(def); return v.IsValid ? new GH_Line(v) : null; },
                                    this);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Matrix _:
                                paramIndex = AddInputWithDefault<GH_Matrix>(
                                    mgr, input, name, nickname, inputDescription, access,
                                    (m, n, nn, d, a) => m.AddMatrixParameter(n, nn, d, a),
                                    r => new GH_Matrix(JsonConvert.DeserializeObject<Matrix>(r.Data.ToString())),
                                    null,
                                    this);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Mesh _:
                                paramIndex = AddInputWithDefault<GH_Mesh>(
                                    mgr, input, name, nickname, inputDescription, access,
                                    (m, n, nn, d, a) => m.AddMeshParameter(n, nn, d, a),
                                    r => new GH_Mesh(JsonConvert.DeserializeObject<Mesh>(r.Data.ToString())),
                                    def => { var v = JsonConvert.DeserializeObject<Mesh>(def); return v.IsValid ? new GH_Mesh(v) : null; },
                                    this);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_MeshFace _:
                                paramIndex = AddInputWithDefault<GH_MeshFace>(
                                    mgr, input, name, nickname, inputDescription, access,
                                    (m, n, nn, d, a) => m.AddMeshFaceParameter(n, nn, d, a),
                                    r => new GH_MeshFace(JsonConvert.DeserializeObject<MeshFace>(r.Data.ToString())),
                                    null,
                                    this);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_MeshParameters _:
                                throw new Exception("Mesh parameters param not supported");
                            case Grasshopper.Kernel.Parameters.Param_Number _:
                                paramIndex = AddInputWithDefault<GH_Number>(
                                    mgr, input, name, nickname, inputDescription, access,
                                    (m, n, nn, d, a) => m.AddNumberParameter(n, nn, d, a),
                                    r => new GH_Number(Convert.ToDouble(r.Data.ToString())),
                                    def => Double.TryParse(def, out double v) ? new GH_Number(v) : null,
                                    this);
                                break;
                            //case Grasshopper.Kernel.Parameters.Param_OGLShader:
                            case Grasshopper.Kernel.Parameters.Param_Plane _:
                                paramIndex = AddInputWithDefault<GH_Plane>(
                                    mgr, input, name, nickname, inputDescription, access,
                                    (m, n, nn, d, a) => m.AddPlaneParameter(n, nn, d, a),
                                    r => new GH_Plane(JsonConvert.DeserializeObject<Plane>(r.Data.ToString())),
                                    def => { var v = JsonConvert.DeserializeObject<Plane>(def); return v.IsValid ? new GH_Plane(v) : null; },
                                    this);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Point _:
                                paramIndex = AddInputWithDefault<GH_Point>(
                                    mgr, input, name, nickname, inputDescription, access,
                                    (m, n, nn, d, a) => m.AddPointParameter(n, nn, d, a),
                                    r => new GH_Point(JsonConvert.DeserializeObject<Point3d>(r.Data.ToString())),
                                    def => { var v = JsonConvert.DeserializeObject<Point3d>(def); return v.IsValid ? new GH_Point(v) : null; },
                                    this);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Rectangle _:
                                paramIndex = AddInputWithDefault<GH_Rectangle>(
                                    mgr, input, name, nickname, inputDescription, access,
                                    (m, n, nn, d, a) => m.AddRectangleParameter(n, nn, d, a),
                                    r => new GH_Rectangle(JsonConvert.DeserializeObject<Rectangle3d>(r.Data.ToString())),
                                    def => { var v = JsonConvert.DeserializeObject<Rectangle3d>(def); return v.IsValid ? new GH_Rectangle(v) : null; },
                                    this);
                                break;
                            //case Grasshopper.Kernel.Parameters.Param_ScriptVariable _:
                            case Grasshopper.Kernel.Parameters.Param_String _:
                                paramIndex = AddInputWithDefault<GH_String>(
                                    mgr, input, name, nickname, inputDescription, access,
                                    (m, n, nn, d, a) => m.AddTextParameter(n, nn, d, a),
                                    r => new GH_String(DecodeStringDefault(r.Data.ToString())),
                                    def => new GH_String(def),
                                    this);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_StructurePath _:
                                paramIndex = mgr.AddPathParameter(name, nickname, inputDescription, access);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_SubD _:
                                paramIndex = AddInputWithDefault<GH_SubD>(
                                    mgr, input, name, nickname, inputDescription, access,
                                    (m, n, nn, d, a) => m.AddSubDParameter(n, nn, d, a),
                                    r => new GH_SubD(JsonConvert.DeserializeObject<SubD>(r.Data.ToString())),
                                    def => { var v = JsonConvert.DeserializeObject<SubD>(def); return v.IsValid ? new GH_SubD(v) : null; },
                                    this);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Surface _:
                                paramIndex = AddInputWithDefault<GH_Surface>(
                                    mgr, input, name, nickname, inputDescription, access,
                                    (m, n, nn, d, a) => m.AddSurfaceParameter(n, nn, d, a),
                                    r => new GH_Surface(JsonConvert.DeserializeObject<Surface>(r.Data.ToString())),
                                    def => { var v = JsonConvert.DeserializeObject<Surface>(def); return v.IsValid ? new GH_Surface(v) : null; },
                                    this);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Time _:
                                paramIndex = mgr.AddTimeParameter(name, nickname, inputDescription, access);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Transform _:
                                paramIndex = AddInputWithDefault<GH_Transform>(
                                    mgr, input, name, nickname, inputDescription, access,
                                    (m, n, nn, d, a) => m.AddTransformParameter(n, nn, d, a),
                                    r => new GH_Transform(JsonConvert.DeserializeObject<Transform>(r.Data.ToString())),
                                    null,
                                    this);
                                break;
                            case Grasshopper.Kernel.Parameters.Param_Vector _:
                                paramIndex = AddInputWithDefault<GH_Vector>(
                                    mgr, input, name, nickname, inputDescription, access,
                                    (m, n, nn, d, a) => m.AddVectorParameter(n, nn, d, a),
                                    r => new GH_Vector(JsonConvert.DeserializeObject<Vector3d>(r.Data.ToString())),
                                    def => { var v = JsonConvert.DeserializeObject<Vector3d>(def); return v.IsValid ? new GH_Vector(v) : null; },
                                    this);
                                break;
                            case Grasshopper.Kernel.Special.GH_NumberSlider _:
                                paramIndex = AddInputWithDefault<GH_Number>(
                                    mgr, input, name, nickname, inputDescription, access,
                                    (m, n, nn, d, a) => m.AddNumberParameter(n, nn, d, a),
                                    r => new GH_Number(Convert.ToDouble(r.Data.ToString())),
                                    def => Double.TryParse(def, out double v) ? new GH_Number(v) : null,
                                    this);
                                break;
                        }

                        //make this parameter optional if user specified AtLeast value of zero
                        if (input.AtLeast == 0)
                            Params.Input[paramIndex].Optional = true;

                        if (paramIndex >= 0 && inputSources.TryGetValue(name, out List<IGH_Param> rehookInputs))
                        {
                            foreach (var rehookInput in rehookInputs)
                                Params.Input[paramIndex].AddSource(rehookInput);
                        }
                    }

                    recompute = true;
                }
                if (buildOutputs && outputs != null)
                {
                    HopsLog.Log.Debug($"Hops component rebuilding output parameters...");

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
                        int paramIndex = param switch
                        {
                            Grasshopper.Kernel.Parameters.Param_Arc _            => mgr.AddArcParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_Boolean _        => mgr.AddBooleanParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_Box _            => mgr.AddBoxParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_Brep _           => mgr.AddBrepParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_Circle _         => mgr.AddCircleParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_Colour _         => mgr.AddColourParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_Complex _        => mgr.AddComplexNumberParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_Culture _        => mgr.AddCultureParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_Curve _          => mgr.AddCurveParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_Field _          => mgr.AddFieldParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_FilePath _       => mgr.AddTextParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_GenericObject _  => mgr.AddGenericParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_Geometry _       => mgr.AddGeometryParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_Group _          => throw new Exception("group param not supported"),
                            Grasshopper.Kernel.Parameters.Param_Guid _           => throw new Exception("guid param not supported"),
                            Grasshopper.Kernel.Parameters.Param_Integer _        => mgr.AddIntegerParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_Interval _       => mgr.AddIntervalParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_Interval2D _     => mgr.AddInterval2DParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_LatLonLocation _ => throw new Exception("latlonlocation param not supported"),
                            Grasshopper.Kernel.Parameters.Param_Line _           => mgr.AddLineParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_Matrix _         => mgr.AddMatrixParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_Mesh _           => mgr.AddMeshParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_MeshFace _       => mgr.AddMeshFaceParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_MeshParameters _ => throw new Exception("meshparameters param not supported"),
                            Grasshopper.Kernel.Parameters.Param_Number _         => mgr.AddNumberParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_Plane _          => mgr.AddPlaneParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_Point _          => mgr.AddPointParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_Rectangle _      => mgr.AddRectangleParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_String _         => mgr.AddTextParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_StructurePath _  => mgr.AddPathParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_SubD _           => mgr.AddSubDParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_Surface _        => mgr.AddSurfaceParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_Time _           => mgr.AddTimeParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_Transform _      => mgr.AddTransformParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            Grasshopper.Kernel.Parameters.Param_Vector _         => mgr.AddVectorParameter(name, nickname, outputDescription, GH_ParamAccess.tree),
                            _                                                    => -1
                        };

                        if (paramIndex >= 0 && outputRecipients.TryGetValue(name, out List<IGH_Param> rehookOutputs))
                        {
                            foreach (var rehookOutput in rehookOutputs)
                                rehookOutput.AddSource(Params.Output[paramIndex]); 
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
            else
            {
                foreach (var param in Params.Input.ToArray())
                {
                    Params.UnregisterInputParameter(param);
                }

                var mgr = CreateInputManager();
                if (showPathInput)
                    mgr.AddTextParameter("_Path", "Path", "URL to remote process", GH_ParamAccess.item);
                if (showEnabledInput)
                    mgr.AddBooleanParameter("_Enabled", "Enabled", "Enabled state for solving", GH_ParamAccess.item, true);
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

        public void OnRemoteDefinitionChanged()
        {
            if (remoteDefinitionRequiresRebuild)
                return;

            // this is typically called on a different thread than the main UI thread
            remoteDefinitionRequiresRebuild = true;
            Rhino.RhinoApp.Idle += RhinoApp_Idle;
        }

        private void RhinoApp_Idle(object sender, EventArgs e)
        {
            if (!remoteDefinitionRequiresRebuild)
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
            remoteDefinitionRequiresRebuild = false;
            DefineInputsAndOutputs();
        }
    }
}
