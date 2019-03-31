using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;

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

    public class ResthopperInput
    {
        public string Name { get; set; } = "";
        public GHTypeCodes TypeCode { get; set; }
        public IGH_Param Param { get; set; }

        public static Dictionary<GHTypeCodes, string> TypeCodeToParamType { get; } = new Dictionary<GHTypeCodes, string>()
        {
            { GHTypeCodes.Boolean , "Param_Boolean" }
        };

        public static Dictionary<GHTypeCodes, string> TypeCodeToValueType { get; } = new Dictionary<GHTypeCodes, string>()
        {
            { GHTypeCodes.Boolean, "GH_Boolean" }
        };

        public ResthopperInput()
        {

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

        public bool TrySetData(DataTree<ResthopperObject> tree)
        {
            dynamic typedParam = Convert.ChangeType(Param, Type.GetType($"Grasshopper.Kernel.Parameters.{TypeCodeToParamType[TypeCode]}"));

            foreach (KeyValuePair<string, List<ResthopperObject>> entree in tree)
            {
                GH_Path path = new GH_Path(GhPath.FromString(entree.Key));

                //var objList = new List<Type.GetType($"Grasshopper.Kernel.Types.{TypeCodeToValueType[TypeCode]}")>
                //List<GH_Boolean> objectList = new List<GH_Boolean>();
                for (int i = 0; i < entree.Value.Count; i++)
                {
                    ResthopperObject restobj = entree.Value[i];
                    dynamic data = JsonConvert.DeserializeObject(restobj.Data);
                    //bool boolean = JsonConvert.DeserializeObject<bool>(restobj.Data);
                    Activator.CreateInstance(Type.GetType($"Grasshopper.Kernel.Types.{TypeCodeToValueType[TypeCode]}"), new[] { data });
                    //GH_Boolean data = new GH_Boolean(data);
                    //boolParam.AddVolatileData(path, i, data);
                    typedParam.AddVolatileData(path, i, data);
                }
            }

            return true;
        }
    }

    public class ResthopperOutput
    {

    }
}
