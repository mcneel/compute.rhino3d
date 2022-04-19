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
            HopsUIHelper.UpdateRows = false;
            // use row height/margin for row height, otherwise top row grows larger the more rows there are.
            var tempRow = new FunctionSourceRow("a", "a");
            HopsUIHelper.RowHeight = (int)(tempRow.PreferredSize.Height + tempRow.Margin.Vertical / 2);
            HopsUIHelper.MinGroupBoxHeight = (int)(_gpboxFunctionMgr.Height);
            HopsUIHelper.MinControlHeight = (int)(Height + (_functionSourceTable.Height * 0.8));
            _deleteFunctionSourceButton.Visible = false;
            _serversTextBox.Lines = HopsAppSettings.Servers;
            _serversTextBox.TextChanged += ServersTextboxChanged;
            _apiKeyTextbox.Text = HopsAppSettings.APIKey;
            _apiKeyTextbox.TextChanged += APIKeyTextboxChanged;
            _maxConcurrentRequestsTextbox.Text = HopsAppSettings.MaxConcurrentRequests.ToString();

            if (Rhino.Runtime.HostUtils.RunningOnOSX)
            {
                // group boxes on mac take up more space, so adjust for that
                // (header is on a separate line, border is larger)
                HopsUIHelper.MinControlHeight = Height;
                var extraSpace = 19;
                HopsUIHelper.MinGroupBoxHeight += extraSpace;
                HopsUIHelper.MinControlHeight -= 32;
                _gpboxFunctionMgr.Height += extraSpace;
            }
            if (HopsAppSettings.FunctionSources.Count > 0)
            {
                foreach (var row in HopsAppSettings.FunctionSources)
                {
                    HopsUIHelper.AddRow(_functionSourceTable, row, false);
                    if (_functionSourceTable.RowCount >= 1 && !_deleteFunctionSourceButton.Visible)
                    {
                        _deleteFunctionSourceButton.Visible = true;
                        HopsUIHelper.UpdateRows = true;
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
                //_gpboxFunctionMgr.Visible = false;
                //Size = new System.Drawing.Size(Size.Width, _btnClearMemCache.Bottom + 4);
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
                    HopsUIHelper.RemoveRow(_functionSourceTable, i);
                }
            }
            HopsUIHelper.UpdateFunctionSourceSettings();
            if (_functionSourceTable.RowCount == 0 && _deleteFunctionSourceButton.Visible)
            {
                _deleteFunctionSourceButton.Visible = false;
                HopsUIHelper.UpdateRows = false;
                _functionSourceTable.RowCount++;
                _functionSourceTable.Height = HopsUIHelper.RowHeight;
                _functionSourceTable.RowStyles.Clear();
                _functionSourceTable.RowStyles.Add(new RowStyle(SizeType.Percent, 1.0F));
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
                HopsUIHelper.AddRow(_functionSourceTable, srcName, srcPath, true);
                if (_functionSourceTable.RowCount >= 1 && !_deleteFunctionSourceButton.Visible)
                {
                    _deleteFunctionSourceButton.Visible = true;
                    HopsUIHelper.UpdateRows = true;
                }
            }
        }
    }
}
