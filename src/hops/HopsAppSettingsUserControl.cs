using System;
using System.Windows.Forms;

namespace Hops
{
    partial class HopsAppSettingsUserControl : UserControl
    {
        public HopsAppSettingsUserControl()
        {
            InitializeComponent();
            _serversTextBox.Lines = HopsAppSettings.Servers;
            _serversTextBox.TextChanged += ServersTextboxChanged;
            _apiKeyTextbox.Text = HopsAppSettings.APIKey;
            _apiKeyTextbox.TextChanged += APIKeyTextboxChanged;
            _maxConcurrentRequestsTextbox.Text = HopsAppSettings.MaxConcurrentRequests.ToString();
            _maxConcurrentRequestsTextbox.KeyPress += (s, e) =>
            {
                e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
            };
            _maxConcurrentRequestsTextbox.TextChanged += (s, e) =>
            {
                if (int.TryParse(_maxConcurrentRequestsTextbox.Text, out int result) && result>0)
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
                Size = new System.Drawing.Size(Size.Width, _btnClearMemCache.Bottom + 4);
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
    }
}
