namespace Hops
{
    public static class HopsUIHelper
    {
        // Writes the in-memory HopsAppSettings.FunctionSources list back to persisted settings
        // (names + paths). Called by FunctionSourcesDialog on Save.
        public static void UpdateFunctionSourceSettings()
        {
            int count = HopsAppSettings.FunctionSources.Count;
            string[] names = new string[count];
            string[] paths = new string[count];
            for (int i = 0; i < count; i++)
            {
                names[i] = HopsAppSettings.FunctionSources[i].SourceName;
                paths[i] = HopsAppSettings.FunctionSources[i].SourcePath;
            }
            HopsAppSettings.FunctionSourceNames = names;
            HopsAppSettings.FunctionSourcePaths = paths;
        }
    }
}
