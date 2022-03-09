using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using BH.Engine.RemoteCompute.RhinoCompute;

using Rhino.Geometry;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using GH_IO.Serialization;

using Resthopper.IO;
using Newtonsoft.Json;
using System.Linq;
using Serilog;
using System.Reflection;

namespace compute.geometry
{
    partial class GrasshopperDefinition
    {
        public Schema Solve()
        {
            HasErrors = false;
            Schema result = new Schema();

            // solve definition
            GH_Document.Enabled = true;
            GH_Document.NewSolution(false, GH_SolutionMode.CommandLine);

            foreach (string msg in ErrorMessages)
                result.Errors.Add(msg);

            LogRuntimeMessages(GH_Document.ActiveObjects(), result);

            foreach (var kvp in _output)
            {
                var param = kvp.Value;
                if (param == null)
                    continue;

                // Get data
                var outputTree = new DataTree<ResthopperObject>();
                outputTree.ParamName = kvp.Key;

                var volatileData = param.VolatileData;
                foreach (var path in volatileData.Paths)
                {
                    var resthopperObjectList = new List<ResthopperObject>();
                    foreach (var goo in volatileData.get_Branch(path))
                    {
                        if (goo == null)
                            continue;

                        switch (goo)
                        {
                            case GH_Boolean ghValue:
                                {
                                    bool rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<bool>(rhValue));
                                }
                                break;
                            case GH_Point ghValue:
                                {
                                    Point3d rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<Point3d>(rhValue));
                                }
                                break;
                            case GH_Vector ghValue:
                                {
                                    Vector3d rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<Vector3d>(rhValue));
                                }
                                break;
                            case GH_Integer ghValue:
                                {
                                    int rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<int>(rhValue));
                                }
                                break;
                            case GH_Number ghValue:
                                {
                                    double rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<double>(rhValue));
                                }
                                break;
                            case GH_String ghValue:
                                {
                                    string rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<string>(rhValue));
                                }
                                break;
                            case GH_SubD ghValue:
                                {
                                    SubD rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<SubD>(rhValue));
                                }
                                break;
                            case GH_Line ghValue:
                                {
                                    Line rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<Line>(rhValue));
                                }
                                break;
                            case GH_Curve ghValue:
                                {
                                    Curve rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<Curve>(rhValue));
                                }
                                break;
                            case GH_Circle ghValue:
                                {
                                    Circle rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<Circle>(rhValue));
                                }
                                break;
                            case GH_Plane ghValue:
                                {
                                    Plane rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<Plane>(rhValue));
                                }
                                break;
                            case GH_Rectangle ghValue:
                                {
                                    Rectangle3d rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<Rectangle3d>(rhValue));
                                }
                                break;
                            case GH_Box ghValue:
                                {
                                    Box rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<Box>(rhValue));
                                }
                                break;
                            case GH_Surface ghValue:
                                {
                                    Brep rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<Brep>(rhValue));
                                }
                                break;
                            case GH_Brep ghValue:
                                {
                                    Brep rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<Brep>(rhValue));
                                }
                                break;
                            case GH_Mesh ghValue:
                                {
                                    Mesh rhValue = ghValue.Value;
                                    resthopperObjectList.Add(GetResthopperObject<Mesh>(rhValue));
                                }
                                break;
                        }
                    }

                    // preserve paths when returning data
                    outputTree.Add(path.ToString(), resthopperObjectList);
                }

                result.Values.Add(outputTree);
            }


            if (result.Values.Count < 1)
                throw new System.Exceptions.PayAttentionException("Looks like you've missed something..."); // TODO

            return result;
        }


    }
}
