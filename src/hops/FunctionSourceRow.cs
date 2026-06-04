using System;

namespace Hops
{
    // Plain data class representing a single Hops function source (name + path). Originally a
    // WinForms TableLayoutPanel used for inline editing inside HopsAppSettingsUserControl; the
    // inline UI was replaced by FunctionSourcesDialog (Eto modal) so the row no longer needs to
    // render itself. Only consumers now are HopsAppSettings.InitFunctionSources (constructs
    // instances on settings load), HopsUIHelper.UpdateFunctionSourceSettings (reads SourceName
    // and SourcePath to persist back), and HopsComponent.AddFunctionMgrControl (reads the same
    // fields to build the right-click "Available Functions" menu).
    public class FunctionSourceRow
    {
        public string SourceName { get; }
        public string SourcePath { get; }

        public FunctionSourceRow(string name, string path)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty", nameof(name));
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));
            SourceName = name;
            SourcePath = path;
        }
    }
}
