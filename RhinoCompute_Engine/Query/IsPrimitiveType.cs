using System;
using System.ComponentModel;
using BH.oM.Base.Attributes;

namespace BH.Engine.Computing.RhinoCompute
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Determines whether a type is of a primitive C# type.")]
        [Input("type", "Type to be determined.")]
        [Input("includeStrings", "(Optional, defaults to true) Whether strings should count as primitive types.")]
        [Input("includeValueTypes", "(Optional, defaults to true) Whether value types should count as primitive types.")]
        public static bool IsPrimitiveType(this Type type, bool includeStrings = true, bool includeValueTypes = true)
        {
            if (type == null)
                return false;

            bool result = type.IsPrimitive;

            if (includeStrings)
                result |= type == typeof(string);

            if (includeValueTypes)
                result |= type.IsValueType;

            return result;
        }

        /***************************************************/
    }
}
