using System;
using System.Collections.Generic;
using Resthopper.IO;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Data;

namespace Resthopper.GH
{
    public class ResthopperPolymorph : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ResthopperPolymorph class.
        /// </summary>
        public ResthopperPolymorph()
          : base("RH Polymorph", "RH Polymorph",
              "Description",
              "Resthopper", "Send")
        {
        }
        private static HttpClient httpClient = new HttpClient();
        private static string ioRoute = "http://localhost:8081/io";
        private static string ghRoute = "http://localhost:8081/grasshopper";
        private string lastPointer = string.Empty;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Pointer", "Pointer", "RH Pointer", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override async void SolveInstance(IGH_DataAccess DA)
        {
            string pointer = null;
            DA.GetData(0, ref pointer);

            IoResponseSchema io = null;
            if (pointer != this.lastPointer)
            {
                if (this.Params.Input.Count > 1)
                {
                    // clean inputs
                    IGH_Param pt = this.Params.Input[0];
                    foreach(IGH_Param p in this.Params.Input)
                    {
                        p.RemoveAllSources();
                        p.ClearData();
                    }
                    this.Params.Input.Clear();
                    this.Params.RegisterInputParam(pt);
                    
                }
                if (this.Params.Output.Count > 0)
                {
                    // clean inputs
                    this.Params.Output.Clear();
                }


                io = await GetIO(pointer);
                foreach (string input in io.InputNames)
                {
                    IGH_Param param = ParamFromName(input);
                    if (param != null)
                    {
                        this.Params.RegisterInputParam(param);
                    }
                }
                foreach (string output in io.OutputNames)
                {
                    IGH_Param param = ParamFromName(output);
                    if (param != null)
                    {
                        this.Params.RegisterOutputParam(param);
                    }
                }
                this.lastPointer = pointer;
            }

            // Formulate Input Tree
            Schema InputSchema = new Schema();
            InputSchema.Pointer = pointer;
            List<DataTree<ResthopperObject>> trees = new List<DataTree<ResthopperObject>>();
            for (int i = 1; i < this.Params.Input.Count-1; i++)
            {
                IGH_Param param = this.Params.Input[i];
                string name = io.InputNames[i];
                GHTypeCodes code = (GHTypeCodes)Int32.Parse(name.Split(':')[1]);
                var booleanInput = param.VolatileData;

                var InputTree = new Resthopper.IO.DataTree<ResthopperObject>();
                InputTree.ParamName = name;

                for (int p = 0; p < booleanInput.PathCount; p++)
                {
                    List<ResthopperObject> ResthopperObjectList = new List<ResthopperObject>();
                    foreach (var goo in booleanInput.get_Branch(p))
                    {
                        if (goo == null) continue;
                        else if (goo.GetType() == typeof(GH_Boolean))
                        {
                            GH_Boolean ghValue = goo as GH_Boolean;
                            bool rhValue = ghValue.Value;
                            ResthopperObjectList.Add(GetResthopperObject<bool>(rhValue));
                        }
                        else if (goo.GetType() == typeof(GH_Point))
                        {
                            GH_Point ghValue = goo as GH_Point;
                            Point3d rhValue = ghValue.Value;
                            ResthopperObjectList.Add(GetResthopperObject<Point3d>(rhValue));
                        }
                        else if (goo.GetType() == typeof(GH_Vector))
                        {
                            GH_Vector ghValue = goo as GH_Vector;
                            Vector3d rhValue = ghValue.Value;
                            ResthopperObjectList.Add(GetResthopperObject<Vector3d>(rhValue));
                        }
                        else if (goo.GetType() == typeof(GH_Integer))
                        {
                            GH_Integer ghValue = goo as GH_Integer;
                            int rhValue = ghValue.Value;
                            ResthopperObjectList.Add(GetResthopperObject<int>(rhValue));
                        }
                        else if (goo.GetType() == typeof(GH_Number))
                        {
                            GH_Number ghValue = goo as GH_Number;
                            double rhValue = ghValue.Value;
                            ResthopperObjectList.Add(GetResthopperObject<double>(rhValue));
                        }
                        else if (goo.GetType() == typeof(GH_String))
                        {
                            GH_String ghValue = goo as GH_String;
                            string rhValue = ghValue.Value;
                            ResthopperObjectList.Add(GetResthopperObject<string>(rhValue));
                        }
                        else if (goo.GetType() == typeof(GH_Line))
                        {
                            GH_Line ghValue = goo as GH_Line;
                            Line rhValue = ghValue.Value;
                            ResthopperObjectList.Add(GetResthopperObject<Line>(rhValue));
                        }
                        else if (goo.GetType() == typeof(GH_Curve))
                        {
                            GH_Curve ghValue = goo as GH_Curve;
                            Curve rhValue = ghValue.Value;
                            ResthopperObjectList.Add(GetResthopperObject<Curve>(rhValue));
                        }
                        else if (goo.GetType() == typeof(GH_Circle))
                        {
                            GH_Circle ghValue = goo as GH_Circle;
                            Circle rhValue = ghValue.Value;
                            ResthopperObjectList.Add(GetResthopperObject<Circle>(rhValue));
                        }
                        else if (goo.GetType() == typeof(GH_Plane))
                        {
                            GH_Plane ghValue = goo as GH_Plane;
                            Plane rhValue = ghValue.Value;
                            ResthopperObjectList.Add(GetResthopperObject<Plane>(rhValue));
                        }
                        else if (goo.GetType() == typeof(GH_Rectangle))
                        {
                            GH_Rectangle ghValue = goo as GH_Rectangle;
                            Rectangle3d rhValue = ghValue.Value;
                            ResthopperObjectList.Add(GetResthopperObject<Rectangle3d>(rhValue));
                        }
                        else if (goo.GetType() == typeof(GH_Box))
                        {
                            GH_Box ghValue = goo as GH_Box;
                            Box rhValue = ghValue.Value;
                            ResthopperObjectList.Add(GetResthopperObject<Box>(rhValue));
                        }
                        else if (goo.GetType() == typeof(GH_Surface))
                        {
                            GH_Surface ghValue = goo as GH_Surface;
                            Brep rhValue = ghValue.Value;
                            ResthopperObjectList.Add(GetResthopperObject<Brep>(rhValue));
                        }
                        else if (goo.GetType() == typeof(GH_Brep))
                        {
                            GH_Brep ghValue = goo as GH_Brep;
                            Brep rhValue = ghValue.Value;
                            ResthopperObjectList.Add(GetResthopperObject<Brep>(rhValue));
                        }
                        else if (goo.GetType() == typeof(GH_Mesh))
                        {
                            GH_Mesh ghValue = goo as GH_Mesh;
                            Mesh rhValue = ghValue.Value;
                            ResthopperObjectList.Add(GetResthopperObject<Mesh>(rhValue));
                        }
                    }

                    GhPath path = new GhPath(new int[] { p });
                    InputTree.Add(path.ToString(), ResthopperObjectList);
                }

                trees.Add(InputTree);
            }

            // Send to RH
            Schema outputSchema = await SendRequest(InputSchema);
            
            // Assign Values
            for (int integer = 0; integer < this.Params.Output.Count; integer++)
            {

                IGH_Param param = this.Params.Output[integer];
                string name = io.OutputNames[integer];
                GHTypeCodes code = (GHTypeCodes)Int32.Parse(name.Split(':')[1]);

                foreach (var tree in outputSchema.Values)
                {
                    if (tree.ParamName == name)
                    {
                        // SetData
                        foreach (Resthopper.IO.DataTree<ResthopperObject> t in outputSchema.Values)
                        {
                            string paramname = t.ParamName;
                                switch (code)
                                {
                                    case GHTypeCodes.Boolean:
                                        //PopulateParam<GH_Boolean>(goo, tree);
                                        Param_Boolean boolParam = param as Param_Boolean;
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Boolean> objectList = new List<GH_Boolean>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                bool boolean = JsonConvert.DeserializeObject<bool>(restobj.Data);
                                                GH_Boolean data = new GH_Boolean(boolean);
                                                boolParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        break;
                                    case GHTypeCodes.Point:
                                        //PopulateParam<GH_Point>(goo, tree);
                                        Param_Point ptParam = param as Param_Point;
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Point> objectList = new List<GH_Point>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                Rhino.Geometry.Point3d rPt = JsonConvert.DeserializeObject<Rhino.Geometry.Point3d>(restobj.Data);
                                                GH_Point data = new GH_Point(rPt);
                                                ptParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        break;
                                    case GHTypeCodes.Vector:
                                        //PopulateParam<GH_Vector>(goo, tree);
                                        Param_Vector vectorParam = param as Param_Vector;
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Vector> objectList = new List<GH_Vector>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                Rhino.Geometry.Vector3d rhVector = JsonConvert.DeserializeObject<Rhino.Geometry.Vector3d>(restobj.Data);
                                                GH_Vector data = new GH_Vector(rhVector);
                                                vectorParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        break;
                                    case GHTypeCodes.Integer:
                                        //PopulateParam<GH_Integer>(goo, tree);
                                        Param_Integer integerParam = param as Param_Integer;
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Integer> objectList = new List<GH_Integer>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                int rhinoInt = JsonConvert.DeserializeObject<int>(restobj.Data);
                                                GH_Integer data = new GH_Integer(rhinoInt);
                                                integerParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        break;
                                    case GHTypeCodes.Number:
                                        //PopulateParam<GH_Number>(goo, tree);
                                        Param_Number numberParam = param as Param_Number;
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Number> objectList = new List<GH_Number>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                double rhNumber = JsonConvert.DeserializeObject<double>(restobj.Data);
                                                GH_Number data = new GH_Number(rhNumber);
                                                numberParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        break;
                                    case GHTypeCodes.Text:
                                        //PopulateParam<GH_String>(goo, tree);
                                        Param_String stringParam = param as Param_String;
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_String> objectList = new List<GH_String>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                string rhString = JsonConvert.DeserializeObject<string>(restobj.Data);
                                                GH_String data = new GH_String(rhString);
                                                stringParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        break;
                                    case GHTypeCodes.Line:
                                        //PopulateParam<GH_Line>(goo, tree);
                                        Param_Line lineParam = param as Param_Line;
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Line> objectList = new List<GH_Line>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                Rhino.Geometry.Line rhLine = JsonConvert.DeserializeObject<Rhino.Geometry.Line>(restobj.Data);
                                                GH_Line data = new GH_Line(rhLine);
                                                lineParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        break;
                                    case GHTypeCodes.Curve:
                                        //PopulateParam<GH_Curve>(goo, tree);
                                        Param_Curve curveParam = param as Param_Curve;
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Curve> objectList = new List<GH_Curve>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                GH_Curve ghCurve;
                                                try
                                                {
                                                    Rhino.Geometry.Polyline data = JsonConvert.DeserializeObject<Rhino.Geometry.Polyline>(restobj.Data);
                                                    Rhino.Geometry.Curve c = new Rhino.Geometry.PolylineCurve(data);
                                                    ghCurve = new GH_Curve(c);
                                                }
                                                catch
                                                {
                                                    Rhino.Geometry.NurbsCurve data = JsonConvert.DeserializeObject<Rhino.Geometry.NurbsCurve>(restobj.Data);
                                                    Rhino.Geometry.Curve c = new Rhino.Geometry.NurbsCurve(data);
                                                    ghCurve = new GH_Curve(c);
                                                }
                                                curveParam.AddVolatileData(path, i, ghCurve);
                                            }
                                        }
                                        break;
                                    case GHTypeCodes.Circle:
                                        //PopulateParam<GH_Circle>(goo, tree);
                                        Param_Circle circleParam = param as Param_Circle;
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Circle> objectList = new List<GH_Circle>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                Rhino.Geometry.Circle rhCircle = JsonConvert.DeserializeObject<Rhino.Geometry.Circle>(restobj.Data);
                                                GH_Circle data = new GH_Circle(rhCircle);
                                                circleParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        break;
                                    case GHTypeCodes.PLane:
                                        //PopulateParam<GH_Plane>(goo, tree);
                                        Param_Plane planeParam = param as Param_Plane;
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Plane> objectList = new List<GH_Plane>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                Rhino.Geometry.Plane rhPlane = JsonConvert.DeserializeObject<Rhino.Geometry.Plane>(restobj.Data);
                                                GH_Plane data = new GH_Plane(rhPlane);
                                                planeParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        break;
                                    case GHTypeCodes.Rectangle:
                                        //PopulateParam<GH_Rectangle>(goo, tree);
                                        Param_Rectangle rectangleParam = param as Param_Rectangle;
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Rectangle> objectList = new List<GH_Rectangle>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                Rhino.Geometry.Rectangle3d rhRectangle = JsonConvert.DeserializeObject<Rhino.Geometry.Rectangle3d>(restobj.Data);
                                                GH_Rectangle data = new GH_Rectangle(rhRectangle);
                                                rectangleParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        break;
                                    case GHTypeCodes.Box:
                                        //PopulateParam<GH_Box>(goo, tree);
                                        Param_Box boxParam = param as Param_Box;
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Box> objectList = new List<GH_Box>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                Rhino.Geometry.Box rhBox = JsonConvert.DeserializeObject<Rhino.Geometry.Box>(restobj.Data);
                                                GH_Box data = new GH_Box(rhBox);
                                                boxParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        break;
                                    case GHTypeCodes.Surface:
                                        //PopulateParam<GH_Surface>(goo, tree);
                                        Param_Surface surfaceParam = param as Param_Surface;
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Surface> objectList = new List<GH_Surface>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                Rhino.Geometry.Surface rhSurface = JsonConvert.DeserializeObject<Rhino.Geometry.Surface>(restobj.Data);
                                                GH_Surface data = new GH_Surface(rhSurface);
                                                surfaceParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        break;
                                    case GHTypeCodes.Brep:
                                        //PopulateParam<GH_Brep>(goo, tree);
                                        Param_Brep brepParam = param as Param_Brep;
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Brep> objectList = new List<GH_Brep>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                Rhino.Geometry.Brep rhBrep = JsonConvert.DeserializeObject<Rhino.Geometry.Brep>(restobj.Data);
                                                GH_Brep data = new GH_Brep(rhBrep);
                                                brepParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        break;
                                    case GHTypeCodes.Mesh:
                                        //PopulateParam<GH_Mesh>(goo, tree);
                                        Param_Mesh meshParam = param as Param_Mesh;
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Mesh> objectList = new List<GH_Mesh>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                Rhino.Geometry.Mesh rhMesh = JsonConvert.DeserializeObject<Rhino.Geometry.Mesh>(restobj.Data);
                                                GH_Mesh data = new GH_Mesh(rhMesh);
                                                meshParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        break;

                                    case GHTypeCodes.Slider:
                                        //PopulateParam<GH_Number>(goo, tree);
                                        GH_NumberSlider sliderParam = param as GH_NumberSlider;
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Number> objectList = new List<GH_Number>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                double rhNumber = JsonConvert.DeserializeObject<double>(restobj.Data);
                                                GH_Number data = new GH_Number(rhNumber);
                                                sliderParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        break;
                                    case GHTypeCodes.BooleanToggle:
                                        //PopulateParam<GH_Boolean>(goo, tree);
                                        GH_BooleanToggle toggleParam = param as GH_BooleanToggle;
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Boolean> objectList = new List<GH_Boolean>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                bool rhBoolean = JsonConvert.DeserializeObject<bool>(restobj.Data);
                                                GH_Boolean data = new GH_Boolean(rhBoolean);
                                                toggleParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        break;
                                    case GHTypeCodes.Panel:
                                        //PopulateParam<GH_String>(goo, tree);
                                        GH_Panel panelParam = param as GH_Panel;
                                        foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
                                        {
                                            GH_Path path = new GH_Path(GhPath.FromString(entree.Key));
                                            List<GH_Panel> objectList = new List<GH_Panel>();
                                            for (int i = 0; i < entree.Value.Count; i++)
                                            {
                                                ResthopperObject restobj = entree.Value[i];
                                                string rhString = JsonConvert.DeserializeObject<string>(restobj.Data);
                                                GH_String data = new GH_String(rhString);
                                                panelParam.AddVolatileData(path, i, data);
                                            }
                                        }
                                        break;
                                }
                            
                        }
                    }
                }
            }

        }

        private static IGH_Param ParamFromName(string name)
        {
            string[] parts = name.Split(':');
            GHTypeCodes typeCode = (GHTypeCodes)Int32.Parse(parts[1]);
            string nickname = parts[2];
            IGH_Param param = null;
            switch (typeCode)
            {
                case GHTypeCodes.Boolean: param = new Param_Boolean(); break;
                case GHTypeCodes.Point: param = new Param_Point(); break;
                case GHTypeCodes.Vector: param = new Param_Vector(); break;
                case GHTypeCodes.Integer: param = new Param_Integer(); break;
                case GHTypeCodes.Number: param = new Param_Number(); break;
                case GHTypeCodes.Text: param = new Param_String(); break;
                case GHTypeCodes.Line: param = new Param_Line(); break;
                case GHTypeCodes.Circle: param = new Param_Circle(); break;
                case GHTypeCodes.PLane: param = new Param_Plane(); break;
                case GHTypeCodes.Rectangle: param = new Param_Rectangle(); break;
                case GHTypeCodes.Box: param = new Param_Box(); break;
                case GHTypeCodes.Surface: param = new Param_Surface(); break;
                case GHTypeCodes.Brep: param = new Param_Brep(); break;
                case GHTypeCodes.Mesh: param = new Param_Mesh(); break;
                case GHTypeCodes.Slider: param = new Param_Number(); break;
                case GHTypeCodes.BooleanToggle: param = new Param_Boolean(); break;
                case GHTypeCodes.Panel: param = new Param_String(); break;
            }
            param.Access = GH_ParamAccess.tree;
            param.NickName = $"{typeCode.ToString()}:{nickname}";
            param.Name = $"{typeCode.ToString()}:{nickname}";
            param.Description = $"Autogenerated Resthopper {typeCode.ToString()} Param";
            //param.Optional = true;
            param.MutableNickName = false;

            return param;
        }

        private static async Task<IoResponseSchema> GetIO(string pointer)
        {
            IoQuerySchema schema = new IoQuerySchema();
            schema.RequestedFile = pointer;
            string serialized = JsonConvert.SerializeObject(schema);
            string response = await httpClient.PostAsync(ioRoute, new StringContent(serialized, Encoding.UTF8, "application/json")).Result.Content.ReadAsStringAsync();
            IoResponseSchema result = JsonConvert.DeserializeObject<IoResponseSchema>(response);
            return result;
        }

        private static async Task<Schema> SendRequest(Schema schema)
        {
            string serialized = JsonConvert.SerializeObject(schema);
            string response = await httpClient.PostAsync(ghRoute, new StringContent(serialized, Encoding.UTF8, "application/json")).Result.Content.ReadAsStringAsync();
            Schema result = JsonConvert.DeserializeObject<Schema>(response);
            return result;
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("bcc04c7e-d62e-4543-82af-4a41f11ae145"); }
        }

        public static ResthopperObject GetResthopperObject<T>(object goo)
        {
            var v = (T)goo;

            ResthopperObject rhObj = new ResthopperObject();
            rhObj.Type = goo.GetType().FullName;
            rhObj.Data = JsonConvert.SerializeObject(v);
            return rhObj;
        }
    }
}