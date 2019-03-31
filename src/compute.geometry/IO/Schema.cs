using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace Resthopper.IO
{
    public class Schema
    {

        public Schema() {
            this.Values = new List<DataTree<ResthopperObject>>();
        }

        [JsonProperty(PropertyName = "algo")]
        public string Algo { get; set; }

        [JsonProperty(PropertyName = "pointer")]
        public string Pointer { get; set; }

        [JsonProperty(PropertyName = "values")]

        public List<DataTree<ResthopperObject>> Values { get; set; }

        //public Dictionary<string, Dictionary<GhPath, List<ResthopperObject>>> Values { get; set; }

    }

    public class IoQuerySchema
    {
        [JsonProperty(PropertyName = "requestedFile")]
        public string RequestedFile { get; set; }
    }

    public class IoResponseSchema
    {
        public List<string> InputNames { get; set; }
        public List<string> OutputNames { get; set; }
    }

    public class ResthopperObject
    {
        [JsonProperty(PropertyName = "type")]
        //public Type Type { get; set; }
        public string Type { get; set; }

        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; }

        [JsonConstructor]
        public ResthopperObject()
        {

        }

        public ResthopperObject(object obj)
        {
            this.Data = JsonConvert.SerializeObject(obj);
            this.Type = obj.GetType().FullName;
        }
    }

    public abstract class ResthopperParam
    {
        public string Name { get; set; } = "";
        public string Label { get; set; }
        public GHTypeCodes TypeCode { get; set; }
        public IGH_Param Param { get; set; }

        public bool TryParseName(string label, out string message)
        {
            message = "";
            Label = label;

            // Begin parsing label for type information
            var labelData = label.Split(':');

            if (labelData.Length < 2)
            {
                message = "Invalid group name format.";
                return false;
            }

            if (!int.TryParse(labelData[1], out var val))
            {
                message = "Could not parse integer value from group name.";
                return false;
            }

            try
            {
                TypeCode = (GHTypeCodes)val;
            }
            catch
            {
                message = "No type found for integer value provided. It may be incorrect or unsupported.";
                return false;
            }

            if (labelData.Length == 3)
            {
                Name = labelData[2];
            }

            return true;
        }
    }

    public class ResthopperInput : ResthopperParam
    {
        public static Dictionary<GHTypeCodes, string> TypeCodeToParamType { get; } = new Dictionary<GHTypeCodes, string>()
        {
            { GHTypeCodes.Boolean , "Param_Boolean" },
            { GHTypeCodes.Point , "Param_Point" },
            { GHTypeCodes.Number , "Param_Number" },
        };

        public static Dictionary<GHTypeCodes, string> TypeCodeToValueType { get; } = new Dictionary<GHTypeCodes, string>()
        {
            { GHTypeCodes.Boolean, "GH_Boolean" },
            { GHTypeCodes.Point, "GH_Point" },
            { GHTypeCodes.Number, "GH_Number" },
        };

        public static Dictionary<GHTypeCodes, Type> TypeCodeToGeometryType { get; } = new Dictionary<GHTypeCodes, Type>()
        {
            { GHTypeCodes.Boolean, typeof(bool) },
            { GHTypeCodes.Point, typeof(Point3d) },
            { GHTypeCodes.Number, typeof(double) },
        };

        public ResthopperInput()
        {

        }

        public static T Cast<T>(object o)
        {
            return (T)o;
        }

        public bool TryBuildResthopperInput(string label, IGH_DocumentObject obj, out string message)
        {
            message = "";

            // Attempt cast of generic object to grasshopper param
            var param = obj as IGH_Param;
            if (param == null)
            {
                message = "Invalid object in group.";
                return false;
            }

            Param = param;

            // Parse label for type information.
            if (!TryParseName(label, out var msg))
            {
                message = msg;
                return false;
            }

            return true;
        }

        public bool TrySetData(DataTree<ResthopperObject> tree)
        {
            Assembly assembly = Assembly.LoadFrom(@"C:\Program Files\Rhino WIP\Plug-ins\Grasshopper\Grasshopper.dll");
            Assembly rhAssembly = Assembly.LoadFrom(@"C:\Program Files\Rhino WIP\System\RhinoCommon.dll");

            Type paramType = assembly.GetType($"Grasshopper.Kernel.Parameters.{TypeCodeToParamType[TypeCode]}");
            //Param_Boolean 
            //object instanceOfMyType = Activator.CreateInstance(type);

            MethodInfo castMethod = this.GetType().GetMethod("Cast").MakeGenericMethod(paramType);
            dynamic typedParam = castMethod.Invoke(null, new object[] { Param });

            //dynamic typedParam = Convert.ChangeType(Param, paramType);

            foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
            {
                GH_Path path = new GH_Path(GhPath.FromString(entree.Key));

                for (int i = 0; i < entree.Value.Count; i++)
                {
                    ResthopperObject restobj = entree.Value[i];
                    var data = GetGeometry(restobj.Data, TypeCode);
                    dynamic ghObj = Activator.CreateInstance(assembly.GetType($"Grasshopper.Kernel.Types.{TypeCodeToValueType[TypeCode]}"), data);
                    typedParam.AddVolatileData(path, i, ghObj);
                }
            }

            return true;
        }

        private dynamic GetGeometry(string json, GHTypeCodes type)
        {
            switch (type)
            {
                case GHTypeCodes.Boolean:
                    return JsonConvert.DeserializeObject<bool>(json);
                case GHTypeCodes.Point:
                    return JsonConvert.DeserializeObject<Point3d>(json);
                case GHTypeCodes.Number:
                    return JsonConvert.DeserializeObject<double>(json);
                default:
                    return null;
            }
        }
    }

    public class ResthopperOutput : ResthopperParam
    {
        public bool TryBuildResthopperOutput(string label, IGH_DocumentObject obj, out string message)
        {
            message = "";

            // Attempt cast of generic object to grasshopper param
            var param = obj as IGH_Param;
            if (param == null)
            {
                message = "Invalid object in group.";
                return false;
            }

            Param = param;

            // Parse label for type information.
            if (!TryParseName(label, out var msg))
            {
                message = msg;
                return false;
            }

            return true;
        }
    }
}
