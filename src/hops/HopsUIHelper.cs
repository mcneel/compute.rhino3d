using System.Windows.Forms;

namespace Hops
{
    public static class HopsUIHelper
    {
        const int rowHeight = 24;
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
            groupBox.Height -= rowHeight;
            if (groupBox.Height < 74)
                groupBox.Height = 74;

        }
        public static void AddRow(TableLayoutPanel panel, string name, string path, bool update)
        {
            GroupBox groupBox = panel.Parent as GroupBox;

            if (HopsAppSettings.HasSourceRows)
            {
                panel.RowCount++;
                panel.Height += rowHeight;
                groupBox.Height += rowHeight;
                HopsAppSettings.HasSourceRows = true;
            }
            FunctionSourceRow row = new FunctionSourceRow(name, path);
            row.UpdateRow += UpdateRow;
            panel.Controls.Add(row, 0, panel.RowCount - 1);

            if (update)
            {
                HopsAppSettings.FunctionSources.Add(row);
                UpdateFunctionSourceSettings();
            }
        }

        private static void UpdateRow(object sender, UpdateRowArgs e)
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
