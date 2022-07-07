using System;
using System.Collections.Generic;
using BH.oM.RemoteCompute.RhinoCompute;
using BH.oM.RemoteCompute;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json;
using Rhino.Geometry;
using System.Reflection;
using System.Linq;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Modify
    {
        public static void AssignInputData(this GrasshopperDefinition rc, List<ResthopperInputTree> inputsListTrees)
        {
            if (inputsListTrees == null)
                return;

            for (int i = 0; i < inputsListTrees.Count; i++)
            {
                GrasshopperDataTree<ResthopperObject> tree = inputsListTrees[i];
                if (!rc.AssignInputData(tree))
                    Log.RecordError($"Could not assign the input named `{tree.ParamName}` from input list, index {i}.", false, true);
            }
        }

        public static bool AssignInputData(this GrasshopperDefinition rc, GrasshopperDataTree<ResthopperObject> tree)
        {
            // Make sure the input has been created before populating it with data.
            // This is done via AddInput().
            InputGroup inputGroup = null;
            if (!rc.Inputs.TryGetValue(tree.ParamName, out inputGroup))
            {
                Log.RecordError($"Input `{tree.ParamName}` does not appear to exist in this script. Check the spelling and the names of the available inputs for this script.");
                return false;
            }

            if (inputGroup.IsAlreadySet(tree))
            {
                Log.RecordError($"Input `{tree.ParamName}` was set already. Check if you have two or more inputs with the same name.");
                return false;
            }

            // CONTEXTUAL DATA ASSIGNMENT
            IGH_ContextualParameter contextualParameter = inputGroup.Param as IGH_ContextualParameter;
            if (contextualParameter != null)
            {
                if (!AssignContextualData(contextualParameter, inputGroup.Param.ParamTypeName(), tree))
                    Log.RecordError($"Could not assign input {tree.ParamName} as Contextual Data.");

                return false;
            }

            // VOLATILE DATA ASSIGMENT
            inputGroup.Param.VolatileData.Clear();
            inputGroup.Param.ExpireSolution(false); // mark param as expired but don't recompute just yet

            inputGroup.DataTree = tree;

            // BHOM DATA ASSIGNMENT AS VOLATILE DATA
            if (inputGroup.Param.IsBHoMUIParameter())
            {
                foreach (KeyValuePair<string, List<ResthopperObject>> entry in tree)
                {
                    GH_Path path = new GH_Path(GrasshopperPath.FromString(entry.Key));
                    for (int i = 0; i < entry.Value.Count; i++)
                    {
                        ResthopperObject restobj = entry.Value[i];
                        Type t = restobj.Type.TypeFromName();

                        object data = JsonConvert.DeserializeObject(restobj.Data, t);

                        if (!inputGroup.Param.AddVolatileData(path, i, data))
                        {
                            Log.RecordError($"Could not assign the BHoM input data in {tree.ParamName} as Volatile Data.");
                            return false;
                        }
                    }
                }

                return true;
            }

            // OTHER DATA ASSIGNMENT AS VOLATILE DATA
            if (inputGroup.Param is Param_Curve)
            {
                foreach (KeyValuePair<string, List<ResthopperObject>> entry in tree)
                {
                    GH_Path path = new GH_Path(GrasshopperPath.FromString(entry.Key));
                    for (int i = 0; i < entry.Value.Count; i++)
                    {
                        ResthopperObject restobj = entry.Value[i];
                        GH_Curve ghCurve;
                        try
                        {
                            Rhino.Geometry.Polyline data = JsonConvert.DeserializeObject<Rhino.Geometry.Polyline>(restobj.Data);
                            Rhino.Geometry.Curve c = new Rhino.Geometry.PolylineCurve(data);
                            ghCurve = new GH_Curve(c);
                        }
                        catch
                        {
                            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(restobj.Data);
                            var c = (Rhino.Geometry.Curve)Rhino.Runtime.CommonObject.FromJSON(dict);
                            ghCurve = new GH_Curve(c);
                        }

                        if (!inputGroup.Param.AddVolatileData(path, i, ghCurve))
                        {
                            Log.RecordError($"Could not assign the Curve in {tree.ParamName} as Volatile Data.");
                            return false;
                        }
                    }
                }

                return true;
            }

            Type paramType = inputGroup.Param.GetType();

            Type paramRhinoType = paramType.GHParamToRhinoType();
            if (paramRhinoType == null)
                return false;

            Type paramGHType = paramRhinoType.RhinoToGHType();
            if (paramGHType == null)
                return false;

            return AssignVolatileData(inputGroup.Param, paramRhinoType, paramGHType, tree);
        }

        private static bool AssignVolatileData(this IGH_Param gH_Param, Type RType, Type GHType, GrasshopperDataTree<ResthopperObject> dataTree)
        {
            MethodInfo method = typeof(Modify).GetMethod(nameof(Modify.AssignVolatileDataGeneric), BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo generic = method.MakeGenericMethod(new Type[] { RType, GHType });
            return (bool)generic.Invoke(null, new object[] { gH_Param, dataTree });
        }

        private static bool AssignVolatileDataGeneric<RType, GHType>(this IGH_Param gH_Param, GrasshopperDataTree<ResthopperObject> dataTree) where GHType : class, IGH_Goo
        {
            bool result = true;

            foreach (KeyValuePair<string, List<ResthopperObject>> entry in dataTree)
            {
                GH_Path path = new GH_Path(GrasshopperPath.FromString(entry.Key));
                for (int i = 0; i < entry.Value.Count; i++)
                {
                    ResthopperObject restobj = entry.Value[i];
                    RType rhinoData = JsonConvert.DeserializeObject<RType>(restobj.Data);

                    GHType grasshopperData = Activator.CreateInstance(typeof(GHType), new object[] { rhinoData }) as GHType;
                    if (!gH_Param.AddVolatileData(path, i, grasshopperData))
                    {
                        result = false;
                        Log.RecordError($"Could not assign data from datatree {dataTree.ParamName}, path {path} to {grasshopperData.TypeName}, {grasshopperData.TypeDescription}.");
                    }
                }
            }

            return result;
        }
    }
}
