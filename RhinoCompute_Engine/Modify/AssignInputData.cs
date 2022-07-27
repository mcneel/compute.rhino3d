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
using System.ComponentModel;

namespace BH.Engine.RemoteCompute.RhinoCompute
{
    public static partial class Modify
    {
        public static void AssignInputsData(this GrasshopperDefinition rc, IEnumerable<ResthopperInputTree> inputsListTrees)
        {
            if (inputsListTrees == null)
                return;

            int inputIdx = 0;
            foreach (ResthopperInputTree inputTree in inputsListTrees)
            {
                if (inputTree == null || !rc.AssignInputData(inputTree))
                    Log.RecordError($"Could not assign the input at index {inputIdx}" + (inputTree?.ParamName != null ? $", of name named `{inputTree.ParamName}`." : "."), false, true);

                inputIdx++;
            }
        }

        private static bool AssignInputData(this GrasshopperDefinition rc, ResthopperInputTree tree)
        {
            // Make sure the input has been created before populating it with data.
            // This is done via AddInput().
            Input inputGroup = null;
            if (!rc.Inputs.TryGetValue(tree.ParamName, out inputGroup))
            {
                string error = $"Input `{tree.ParamName}` does not appear to exist in this script. Check the spelling and the names of the available inputs for this script.";

                string availableInputs = string.Join("`, `", rc.Inputs?.Select(i => i.Key));
                if (!string.IsNullOrWhiteSpace(availableInputs))
                    error += "\nInputs available in this script: `" + availableInputs;

                Log.RecordError(error);
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

            inputGroup.InputData = tree;

            // BHOM DATA ASSIGNMENT AS VOLATILE DATA
            if (inputGroup.Param.IsBHoMUIParameter())
            {
                foreach (KeyValuePair<string, List<ResthopperObject>> entry in tree)
                {
                    GH_Path path = new GH_Path(GrasshopperPath.FromString(entry.Key));
                    for (int i = 0; i < entry.Value.Count; i++)
                    {
                        ResthopperObject restobj = entry.Value[i];
                        Type t = restobj.Type?.TypeFromName();

                        object data = null;

                        try
                        {
                            data = JsonConvert.DeserializeObject(restobj.Data, t, m_jsonSerializerSettings);
                        }
                        catch (Exception e)
                        {
                            Log.RecordError($"Could not assign input {tree.ParamName} as Volatile Data. Error: {e.Message}");
                            return false;
                        }

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
                            Rhino.Geometry.Polyline data = JsonConvert.DeserializeObject<Rhino.Geometry.Polyline>(restobj.Data, m_jsonSerializerSettings);
                            Rhino.Geometry.Curve c = new Rhino.Geometry.PolylineCurve(data);
                            ghCurve = new GH_Curve(c);
                        }
                        catch
                        {
                            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(restobj.Data, m_jsonSerializerSettings);
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

            return AssignVolatileData(inputGroup.Param, tree);
        }

        private static bool AssignVolatileData(this IGH_Param gH_Param, GrasshopperDataTree<ResthopperObject> dataTree)
        {
            bool result = true;

            Type paramRhinoType = gH_Param.GetType().GHParamToRhinoType();
            Type GHgooType = paramRhinoType.RhinoToGHType();

            foreach (KeyValuePair<string, List<ResthopperObject>> entry in dataTree)
            {
                GH_Path path = new GH_Path(GrasshopperPath.FromString(entry.Key));
                string errorTextFirstPart = $"Could not assign volatile data from datatree {dataTree.ParamName}, path {path}";

                for (int i = 0; i < entry.Value.Count; i++)
                {
                    ResthopperObject restobj = entry.Value.ElementAtOrDefault(i);
                    Type restType = restobj?.Type?.TypeFromName();

                    // See if a more specific type can be used for the Goo.
                    if (paramRhinoType == typeof(object) && restType != null && restType != typeof(object) && TypeConversions.GHParamToRhinoTypes.Values.Contains(restType))
                    {
                        paramRhinoType = restType;
                        GHgooType = paramRhinoType.RhinoToGHType();
                    }

                    object inputObject = null;
                    try
                    {
                        inputObject = JsonConvert.DeserializeObject(restobj.Data, restType, m_jsonSerializerSettings);
                    }
                    catch
                    {

                        try
                        {
                            inputObject = JsonConvert.DeserializeObject(restobj.Data, paramRhinoType, m_jsonSerializerSettings);
                        }
                        catch
                        {
                            result = false;
                            Log.RecordError($"{errorTextFirstPart}: could not deserialize input object.");
                            continue;
                        }
                    }

                    object dataToAssign = null;
                    object rhinoData = inputObject;
                    if (inputObject != null)
                    {
                        if (restType != paramRhinoType)
                            try
                            {
                                rhinoData = System.Convert.ChangeType(inputObject, paramRhinoType);
                            }
                            catch
                            {
                                result = false;
                                Log.RecordError($"{errorTextFirstPart}: input type was {inputObject.GetType().FullName} which cannot be converted to required type {paramRhinoType.FullName}.");
                                continue;
                            }

                        IGH_Goo grasshopperData = null;
                        try
                        {
                            grasshopperData = Activator.CreateInstance(GHgooType, new object[] { rhinoData }) as IGH_Goo;
                        }
                        catch
                        {
                            result = false;
                            Log.RecordError($"{errorTextFirstPart}: cannot create instance of required {GHgooType.FullName} to host the input type {paramRhinoType.FullName}.");
                            continue;
                        }

                        dataToAssign = grasshopperData;
                    }

                    if (!gH_Param.AddVolatileData(path, i, dataToAssign))
                    {
                        result = false;
                        Log.RecordError($"{errorTextFirstPart}");
                    }
                }
            }

            return result;
        }

        private static JsonSerializerSettings m_jsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
        };
    }
}
