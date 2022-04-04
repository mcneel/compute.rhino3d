using System;
using System.Windows.Forms;

namespace Hops
{
    partial class HopsAppSettingsUserControl : UserControl
    {
        public HopsAppSettingsUserControl()
        {
            InitializeComponent();
            HopsAppSettings.InitFunctionSources();
            if (!HopsAppSettings.HasSourceRows)
                _deleteFunctionSourceButton.Visible = false;
            _serversTextBox.Lines = HopsAppSettings.Servers;
            _serversTextBox.TextChanged += ServersTextboxChanged;
            _apiKeyTextbox.Text = HopsAppSettings.APIKey;
            _apiKeyTextbox.TextChanged += APIKeyTextboxChanged;
            _maxConcurrentRequestsTextbox.Text = HopsAppSettings.MaxConcurrentRequests.ToString();
            if (HopsAppSettings.FunctionSources.Count > 0)
            {
                foreach (var row in HopsAppSettings.FunctionSources)
                {
                    //HopsUIHelper.AddRow(testPanel, row.SourceName, row.SourcePath, false);
                    HopsUIHelper.AddRow(testPanel, row, false);
                    if (testPanel.RowCount >= 1 && !_deleteFunctionSourceButton.Visible)
                    {
                        _deleteFunctionSourceButton.Visible = true;
                        HopsAppSettings.HasSourceRows = true;
                    }
                }
            }
            _maxConcurrentRequestsTextbox.KeyPress += (s, e) =>
            {
                e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
            };
            _maxConcurrentRequestsTextbox.TextChanged += (s, e) =>
            {
                if (int.TryParse(_maxConcurrentRequestsTextbox.Text, out int result) && result > 0)
                {
                    HopsAppSettings.MaxConcurrentRequests = result;
                }
            };
            _btnClearMemCache.Click += (s, e) =>
            {
                Hops.MemoryCache.ClearCache();
                _lblCacheCount.Text = $"({Hops.MemoryCache.EntryCount} items in cache)";
            };
            _lblCacheCount.Text = $"({Hops.MemoryCache.EntryCount} items in cache)";


            if (!Rhino.Runtime.HostUtils.RunningOnWindows)
            {
                _hideWorkerWindows.Visible = false;
                _launchWorkerAtStart.Visible = false;
                _childComputeCount.Visible = false;
                _updateChildCountButton.Visible = false;
                _gpboxFunctionMgr.Top -= 74;
                Size = new System.Drawing.Size(Size.Width, _gpboxFunctionMgr.Bottom + 4);
            }
            else
            {
                _hideWorkerWindows.Checked = HopsAppSettings.HideWorkerWindows;
                _hideWorkerWindows.CheckedChanged += (s, e) =>
                {
                    HopsAppSettings.HideWorkerWindows = _hideWorkerWindows.Checked;
                };
                _launchWorkerAtStart.Checked = HopsAppSettings.LaunchWorkerAtStart;
                _launchWorkerAtStart.CheckedChanged += (s, e) =>
                {
                    HopsAppSettings.LaunchWorkerAtStart = _launchWorkerAtStart.Checked;
                };
                _childComputeCount.Value = HopsAppSettings.LocalWorkerCount;
                _childComputeCount.ValueChanged += (s, e) =>
                {
                    HopsAppSettings.LocalWorkerCount = (int)_childComputeCount.Value;
                };
                _updateChildCountButton.Click += (s, e) =>
                {
                    int numberToLaunch = HopsAppSettings.LocalWorkerCount - Servers.ActiveLocalComputeCount;
                    Servers.LaunchChildComputeGeometry(numberToLaunch);
                };
                toolTip1.SetToolTip(_updateChildCountButton, "Click to force Rhino.Compute to update");
            }
        }

        private void ServersTextboxChanged(object sender, EventArgs e)
        {
            string[] lines = _serversTextBox.Lines;
            HopsAppSettings.Servers = lines;
        }

        private void APIKeyTextboxChanged(object sender, EventArgs e)
        {
            HopsAppSettings.APIKey = _apiKeyTextbox.Text;
        }

        private void _deleteFunctionSourceButton_Click(object sender, EventArgs e)
        {
            for (int i = HopsAppSettings.FunctionSources.Count - 1; i >= 0; i--)
            {
                if (HopsAppSettings.FunctionSources[i].RowCheckbox.Checked)
                {
                    HopsUIHelper.RemoveRow(testPanel, i);
                    HopsAppSettings.FunctionSources.RemoveAt(i);
                }
            }
            HopsUIHelper.UpdateFunctionSourceSettings();
            if (testPanel.RowCount == 0 && _deleteFunctionSourceButton.Visible)
            {
                _deleteFunctionSourceButton.Visible = false;
                HopsAppSettings.HasSourceRows = false;
                testPanel.RowCount++;
                testPanel.RowStyles.Clear();
                testPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize, 0.0F));
            }
        }

        private void _addFunctionSourceButton_Click(object sender, EventArgs e)
        {
            string srcPath = "";
            string srcName = "";
            var form = new SetFunctionSourceForm(srcPath, srcName);
            if (form.ShowModal(Grasshopper.Instances.EtoDocumentEditor))
            {
                srcPath = form.Path;
                srcName = form.Name;
                HopsUIHelper.AddRow(testPanel, srcName, srcPath, true);
                if (testPanel.RowCount >= 1 && !_deleteFunctionSourceButton.Visible)
                {
                    _deleteFunctionSourceButton.Visible = true;
                    HopsAppSettings.HasSourceRows = true;
                }
            }
        }
    }
}
