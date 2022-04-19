using System.Windows.Forms;

namespace Hops
{
    public static class HopsUIHelper
    {
        public static bool UpdateRows { get; set; } = false;
        public static int RowHeight { get; set; } = 22;
        public static int MinGroupBoxHeight { get; set; } = 73;
        public static int MinControlHeight { get; set; } = 300;

        public static void RemoveRow(TableLayoutPanel panel, int rowIndex)
        {
            if (rowIndex >= panel.RowCount)
            {
                return;
            }

            // delete all controls of row that we want to delete
            for (int i = 0; i < panel.ColumnCount; i++)
            {
                var control = panel.GetControlFromPosition(i, rowIndex);
                panel.Controls.Remove(control);
            }

            // move up row controls that comes after row we want to remove
            for (int i = rowIndex + 1; i < panel.RowCount; i++)
            {
                for (int j = 0; j < panel.ColumnCount; j++)
                {
                    var control = panel.GetControlFromPosition(j, i);
                    if (control != null)
                    {
                        panel.SetRow(control, i - 1);
                    }
                }
            }

            var removeStyle = panel.RowCount - 1;

            if (panel.RowStyles.Count > removeStyle)
                panel.RowStyles.RemoveAt(removeStyle);

            panel.RowCount--;

            GroupBox groupBox = panel.Parent as GroupBox;
            HopsAppSettingsUserControl settingsPanel = groupBox.Parent as HopsAppSettingsUserControl;
            Control topControl = settingsPanel.Parent;

            panel.Height -= RowHeight;
            groupBox.Height -= RowHeight;
            topControl.Height -= RowHeight;

            HopsAppSettings.FunctionSources.RemoveAt(rowIndex);

            if (groupBox.Height < MinGroupBoxHeight)
                groupBox.Height = MinGroupBoxHeight;
            if (topControl.Height < MinControlHeight)
                topControl.Height = MinControlHeight;
        }
        public static void AddRow(TableLayoutPanel panel, string name, string path, bool update)
        {
            FunctionSourceRow row = new FunctionSourceRow(name, path);
            //checkbox margins are different on macOS. Adjust accordingly
            if (Rhino.Runtime.HostUtils.RunningOnOSX)
                row.RowCheckbox.Margin = new Padding(0, -2, 5, 0);
            
            GroupBox groupBox = panel.Parent as GroupBox;
            HopsAppSettingsUserControl settingsPanel = groupBox.Parent as HopsAppSettingsUserControl;
            Control topControl = settingsPanel.Parent;
            if (UpdateRows)
            {
                panel.RowCount++;
                panel.Height += RowHeight;
                groupBox.Height += RowHeight;
                topControl.Height += RowHeight;
                UpdateRows = true;
            }
            
            row.ReplaceRow += UpdateRow;
            panel.Controls.Add(row, 0, panel.RowCount - 1);

            if (update)
            {
                HopsAppSettings.FunctionSources.Add(row);
                UpdateFunctionSourceSettings();
            }
        }

        public static void AddRow(TableLayoutPanel panel, FunctionSourceRow row, bool update)
        {
            //checkbox margins are different on macOS. Adjust accordingly
            if (Rhino.Runtime.HostUtils.RunningOnOSX)
                row.RowCheckbox.Margin = new Padding(0, -2, 5, 0);
            GroupBox groupBox = panel.Parent as GroupBox;
            UserControl topControl = groupBox.Parent as UserControl;
            if (UpdateRows)
            {
                panel.RowCount++;
                panel.Height += RowHeight;
                groupBox.Height += RowHeight;
                topControl.Height += RowHeight;
                UpdateRows = true;
            }
            row.ReplaceRow += UpdateRow;
            panel.Controls.Add(row, 0, panel.RowCount - 1);

            if (update)
            {
                HopsAppSettings.FunctionSources.Add(row);
                UpdateFunctionSourceSettings();
            }
        }

        private static void UpdateRow(object sender, ReplaceRowArgs e)
        {
            var newRow = sender as FunctionSourceRow;
            for(int i = 0; i < HopsAppSettings.FunctionSources.Count; i++)
            {
                if (HopsAppSettings.FunctionSources[i].SourceName == e.RowName)
                {
                    HopsAppSettings.FunctionSources[i] = newRow;
                    break;
                }
            }
            UpdateFunctionSourceSettings();
        }

        public static void UpdateFunctionSourceSettings()
        {
            int count = HopsAppSettings.FunctionSources.Count;
            string[] names = new string[count];
            string[] paths = new string[count];
            string[] selections = new string[count];
            for (int i = 0; i < count; i++)
            {
                names[i] = HopsAppSettings.FunctionSources[i].SourceName;
                paths[i] = HopsAppSettings.FunctionSources[i].SourcePath;
                selections[i] = HopsAppSettings.FunctionSources[i].RowCheckbox.Checked ? "True" : "False";
            }
            HopsAppSettings.FunctionSourceNames = names;
            HopsAppSettings.FunctionSourcePaths = paths;
            HopsAppSettings.FunctionSourceSelectedStates = selections;
        }
    }
}
