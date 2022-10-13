namespace BH.Engine.Computing.RhinoCompute
{
    public static partial class Query
    {
        // strip bom from string -- [239, 187, 191] in byte array == (char)65279
        // https://stackoverflow.com/a/54894929/1902446
        public static string StripBom(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            char BOMChar = (char)65279;

            bool hasBom = str[0] == BOMChar;
            if (hasBom)
                str = str.Substring(1);

            return str;
        }
    }
}
