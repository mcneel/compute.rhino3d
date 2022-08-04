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
        public static void SetInputsData(this GrasshopperDefinition rc, IEnumerable<ResthopperInputTree> inputsListTrees)
        {
            if (inputsListTrees == null)
                return;

            string availableInputs = string.Join("`, `", rc.Inputs?.Select(inp => inp.Key));
            availableInputs = availableInputs.IsNullOrEmpty() ? availableInputs : $"`{availableInputs}`";
            bool specifiedInputsNotFound = false;
            for (int i = 0; i < inputsListTrees.Count(); i++)
            {
                ResthopperInputTree inputTree = inputsListTrees.ElementAtOrDefault(i);

                // Make sure the input was added to GrasshopperDefinition before populating it with data. This is done via AddInput().
                Input inputGroup = null;
                if (!rc.Inputs.TryGetValue(inputTree.ParamName, out inputGroup))
                {
                    Log.RecordWarning($"Input `{inputTree.ParamName}` does not appear to exist in this script. Check the spelling and the names of the available inputs for this script.", true);

                    if (!string.IsNullOrWhiteSpace(availableInputs))
                        specifiedInputsNotFound = true;

                    continue;
                }

                if (inputGroup.IsAlreadySet(inputTree))
                {
                    Log.RecordWarning($"Input `{inputTree.ParamName}` was set already. Check if you have two or more inputs with the same name.");
                    continue;
                }

                if (inputTree == null || !rc.SetInputsData(inputTree, i))
                    Log.RecordError($"Could not assign the input at index {i}" + (inputTree?.ParamName != null ? $", of name named `{inputTree.ParamName}`." : "."), false, true);
            }

            if (specifiedInputsNotFound)
                Log.RecordWarning($"Some specified inputs were not found in the script. Available inputs: {availableInputs}.", true);
        }

        private static bool SetInputsData(this GrasshopperDefinition rc, ResthopperInputTree inputTree, int inputTreeIndex)
        {
            Input inputGroup = null;

            if (!rc.Inputs.TryGetValue(inputTree.ParamName, out inputGroup))
                return false;

            // CONTEXTUAL DATA ASSIGNMENT
            IGH_ContextualParameter contextualParameter = inputGroup.Param as IGH_ContextualParameter;
            if (contextualParameter != null)
            {
                if (!SetContextualData(contextualParameter, inputGroup.Param.ParamTypeName(), inputTree))
                    Log.RecordError($"Could not assign input {inputTree.ParamName} as Contextual Data.");

                return false;
            }

            // VOLATILE DATA ASSIGMENT
            inputGroup.Param.VolatileData.Clear();
            inputGroup.Param.ExpireSolution(false); // mark param as expired but don't recompute just yet

            inputGroup.InputData = inputTree;

            // BHOM DATA ASSIGNMENT AS VOLATILE DATA
            if (inputGroup.Param.IsBHoMUIParameter())
            {
                foreach (KeyValuePair<string, List<ResthopperObject>> entry in inputTree)
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
                            Log.RecordError($"Could not assign input {inputTree.ParamName} as Volatile Data. Error: {e.Message}");
                            return false;
                        }

                        if (!inputGroup.Param.AddVolatileData(path, i, data))
                        {
                            Log.RecordError($"Could not assign the BHoM input data in {inputTree.ParamName} as Volatile Data.");
                            return false;
                        }
                    }
                }

                return true;
            }

            // OTHER DATA ASSIGNMENT AS VOLATILE DATA
            if (inputGroup.Param is Param_Curve)
            {
                foreach (KeyValuePair<string, List<ResthopperObject>> entry in inputTree)
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
                            Log.RecordError($"Could not assign the Curve in {inputTree.ParamName} as Volatile Data.");
                            return false;
                        }
                    }
                }

                return true;
            }

            return SetVolatileData(inputGroup.Param, inputTree);
        }

        private static bool SetVolatileData(this IGH_Param gH_Param, GrasshopperDataTree<ResthopperObject> dataTree)
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
                        if (paramRhinoType != null && restType != paramRhinoType)
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
                            Log.RecordError($"{errorTextFirstPart}: cannot create instance of required {GHgooType.FullName} to host the input type {rhinoData.GetType().FullName}.");
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

        public static void SetTriggers(this GrasshopperDefinition ghDef)
        {
            foreach (IGH_DocumentObject trigger in ghDef.Triggers.Values)
            {
                if (trigger is Grasshopper.Kernel.Special.GH_BooleanToggle toggle)
                    toggle.Value = true;

                if (trigger is Grasshopper.Kernel.Special.GH_ButtonObject button)
                    button.ButtonDown = true;
            }
        }

        private static JsonSerializerSettings m_jsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
        };
    }
}
