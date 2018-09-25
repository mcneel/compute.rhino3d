using System;

namespace compute.geometry
{
    internal static class Env
    {
        public static bool GetEnvironmentBool(string variable, bool defaultValue)
        {
            string value = System.Environment.GetEnvironmentVariable(variable);
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            var lowerValue = value.ToLower();
            if (lowerValue.StartsWith("1") || lowerValue.StartsWith("y") || lowerValue.StartsWith("t"))
                return true;

            return false;
        }

        public static int GetEnvironmentInt(string variable, int defaultValue)
        {
            string value = System.Environment.GetEnvironmentVariable(variable);
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            int result = 0;
            if (int.TryParse(value, out result))
                return result;

            Console.WriteLine($"environment variable {variable} set to '{value}'; unable to parse as integer.");
            return defaultValue;
        }

        public static string GetEnvironmentString(string variable, string defaultValue)
        {
            string value = System.Environment.GetEnvironmentVariable(variable);
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            return value;
        }
    }
}
