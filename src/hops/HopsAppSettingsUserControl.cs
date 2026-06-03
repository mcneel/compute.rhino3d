using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Hops
{
    partial class HopsAppSettingsUserControl : UserControl
    {
        // 3-second probe to keep the UI snappy when a server is unreachable.
        readonly HttpClient testClient = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
        readonly System.Windows.Forms.Timer autoTestTimer;
        CancellationTokenSource testCts;
        bool suppressUrlChange;

        // Used so we can vertically center the text inside multiline TextBoxes.
        [StructLayout(LayoutKind.Sequential)]
        struct RECT { public int Left, Top, Right, Bottom; }

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref RECT lParam);

        const uint EM_SETRECT = 0xB3;

        // Tooltip wrapper for the status dot — keeps the rich per-state message but always
        // tells the user the dot is clickable for a manual re-probe.
        void SetStatusTip(string message)
        {
            toolTip1.SetToolTip(_serverStatusDot, message + "\r\n(click to retest)");
        }

        // Builds a rounded-corner GraphicsPath for the given bounds. Insets the right/bottom edges
        // by 1px so a 1-pixel Pen drawn along the path falls fully inside the control bounds and
        // doesn't get clipped at the antialiased edge.
        static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var r = new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // Multiline TextBox always top-anchors its text; this shim pushes the
        // formatting rectangle down so the text reads vertically centered.
        // Inset is derived from font/client height so it scales across DPIs.
        // No-op on macOS — user32.dll doesn't exist there and the P/Invoke would throw.
        // Multiline textboxes will render top-anchored on Mac as a result, which is
        // a cosmetic regression but better than crashing the settings panel.
        static void CenterMultilineText(TextBox tb)
        {
            if (Rhino.Runtime.HostUtils.RunningOnOSX) return;
            void Apply()
            {
                var client = tb.ClientRectangle;
                int topInset = Math.Max(0, (client.Height - tb.Font.Height) / 2);
                var rect = new RECT { Left = 1, Top = topInset, Right = client.Width - 1, Bottom = client.Height };
                SendMessage(tb.Handle, EM_SETRECT, IntPtr.Zero, ref rect);
            }
            if (tb.IsHandleCreated) Apply();
            else tb.HandleCreated += (s, e) => Apply();
            tb.Resize += (s, e) => { if (tb.IsHandleCreated) Apply(); };
        }

        public HopsAppSettingsUserControl()
        {
            InitializeComponent();
            CenterMultilineText(_serverUrlTextbox);
            CenterMultilineText(_apiKeyTextbox);
            CenterMultilineText(_httpTimeoutTextbox);
            CenterMultilineText(_maxConcurrentRequestsTextbox);

            autoTestTimer = new System.Windows.Forms.Timer { Interval = 800 };
            autoTestTimer.Tick += (s, e) =>
            {
                autoTestTimer.Stop();
                TestCurrentUrl();
            };

            // Checkboxes used as radio-button equivalents (RadioButton isn't in the macOS WinForms shim yet).
            // AutoCheck=false + Click handler enforces exactly-one-checked: re-clicking the active box is a
            // no-op, and clicking the other swaps both states atomically. Programmatic Checked= assignments
            // don't fire Click, so initialization stays clean.
            _rdoUseLocal.AutoCheck = false;
            _rdoUseRemote.AutoCheck = false;
            _rdoUseLocal.Click += UseLocalClicked;
            _rdoUseRemote.Click += UseRemoteClicked;
            _serverUrlTextbox.TextChanged += ServerUrlChanged;
            _advancedServersButton.Click += AdvancedServersClicked;
            _advancedServersButton.Cursor = Cursors.Hand;
            // PictureBox draws BackColor + Image first, then fires Paint — which means we can't paint
            // a rounded fill *behind* the image using the event alone. Instead, drop the auto image
            // draw (Image=null) and render everything ourselves in Paint: rounded fill → icon → stroke.
            _advancedServersButton.Image = null;
            _advancedServersButton.BackColor = Color.Transparent;
            var settingsImage = HopsFunctionMgr.SettingsIcon();
            var hoverFill = Color.FromArgb(0xC1, 0xDC, 0xF0);
            var hoverStroke = Color.FromArgb(0x80, 0xAD, 0xC6);
            const int cornerRadius = 3;
            const int iconInset = 4;
            bool advancedHovered = false;
            _advancedServersButton.MouseEnter += (s, e) =>
            {
                advancedHovered = true;
                _advancedServersButton.Invalidate();
            };
            _advancedServersButton.MouseLeave += (s, e) =>
            {
                advancedHovered = false;
                _advancedServersButton.Invalidate();
            };
            _advancedServersButton.Paint += (s, e) =>
            {
                var g = e.Graphics;
                var client = _advancedServersButton.ClientRectangle;
                // WinForms Font-based AutoScale can produce slightly non-square ClientRectangles
                // at HiDPI (different X/Y font metric ratios than design time). Use the smaller
                // dimension as the square side so the icon and hover decorations render true
                // square regardless of what AutoScale did to the control bounds.
                int side = Math.Min(client.Width, client.Height);
                var square = new Rectangle(
                    client.X + (client.Width - side) / 2,
                    client.Y + (client.Height - side) / 2,
                    side,
                    side);
                if (advancedHovered)
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var path = RoundedRect(square, cornerRadius))
                    using (var brush = new SolidBrush(hoverFill))
                        g.FillPath(brush, path);
                }
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                // Inset the icon from the button bounds so the hover fill has visible padding
                // around the artwork — otherwise the 96×96 source (which is itself edge-to-edge)
                // looks crowded inside the rounded rectangle.
                var iconRect = Rectangle.Inflate(square, -iconInset, -iconInset);
                g.DrawImage(settingsImage, iconRect);
                if (advancedHovered)
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var path = RoundedRect(square, cornerRadius))
                    using (var pen = new Pen(hoverStroke))
                        g.DrawPath(pen, path);
                }
            };
            _serverStatusDot.Image = HopsFunctionMgr.StatusNoneIcon();
            _serverStatusDot.Cursor = Cursors.Hand;
            _serverStatusDot.Click += (s, e) =>
            {
                autoTestTimer.Stop();
                TestCurrentUrl();
            };
            SetStatusTip("Not tested");

            if (Rhino.Runtime.HostUtils.RunningOnOSX)
            {
                _rdoUseLocal.Enabled = false;
                toolTip1.SetToolTip(_rdoUseLocal, "Local auto-spawn is not supported on macOS");
            }

            InitServerSourceFromSettings();

            _apiKeyTextbox.Text = HopsAppSettings.APIKey;
            _apiKeyTextbox.TextChanged += APIKeyTextboxChanged;
            _httpTimeoutTextbox.Text = HopsAppSettings.HttpTimeout.ToString();
            _httpTimeoutTextbox.KeyPress += (s, e) =>
            {
                e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
            };
            _httpTimeoutTextbox.TextChanged += (s, e) =>
            {
                if (int.TryParse(_httpTimeoutTextbox.Text, out int result) && result > 0)
                {
                    HopsAppSettings.HttpTimeout = result;
                }
            };
            _maxConcurrentRequestsTextbox.Text = HopsAppSettings.MaxConcurrentRequests.ToString();
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

            if (Rhino.Runtime.HostUtils.RunningOnOSX)
            {
                _hideWorkerWindows.Visible = false;
                _launchWorkerAtStart.Visible = false;
                _childComputeCount.Visible = false;
                _updateChildCountButton.Visible = false;
            }
            else if (Rhino.Runtime.HostUtils.RunningOnWindows)
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
                    int numberToLaunch = HopsAppSettings.LocalWorkerCount - Hops.Servers.ActiveLocalComputeCount;
                    Hops.Servers.LaunchChildComputeGeometry(numberToLaunch);
                };
                toolTip1.SetToolTip(_updateChildCountButton, "Click to force rhino.compute to update");
            }

            UpdateLocalOnlyControlsEnabled();

            HopsAppSettings.CheckFunctionManagerStatus();
            if (HopsAppSettings.ShowFunctionManager)
            {
                HopsAppSettings.InitFunctionSources();
                RefreshFunctionSourceCount();
                _manageFunctionSourcesButton.Click += (s, e) =>
                {
                    var dlg = new FunctionSourcesDialog();
                    if (dlg.ShowModal(Grasshopper.Instances.EtoDocumentEditor))
                        RefreshFunctionSourceCount();
                };
            }
            else
            {
                _gpboxFunctionMgr.Visible = false;
                Size = new Size(Size.Width, _btnClearMemCache.Bottom + 4);
            }
        }

        void RefreshFunctionSourceCount()
        {
            int count = HopsAppSettings.FunctionSources?.Count ?? 0;
            _functionSourceCountLabel.Text = $"({count} source{(count == 1 ? "" : "s")} configured)";
        }

        void InitServerSourceFromSettings()
        {
            var servers = HopsAppSettings.Servers;
            suppressUrlChange = true;
            try
            {
                // The URL is always populated from saved Servers so toggling local/remote preserves it.
                _serverUrlTextbox.Text = servers.Length > 0 ? servers[0] : "";
                _rdoUseLocal.Checked = HopsAppSettings.UseLocalServer;
                _rdoUseRemote.Checked = !HopsAppSettings.UseLocalServer;
            }
            finally
            {
                suppressUrlChange = false;
            }
            UpdateAdvancedButtonTooltip();
            UpdateUrlControlsEnabled();
            UpdateLocalOnlyControlsEnabled();
            if (_rdoUseRemote.Checked && !string.IsNullOrWhiteSpace(_serverUrlTextbox.Text))
                TestCurrentUrl();
        }

        void UpdateAdvancedButtonTooltip()
        {
            int extras = Math.Max(0, HopsAppSettings.Servers.Length - 1);
            string tip = extras > 0
                ? $"Advanced multi-server setup ({extras} additional URL{(extras == 1 ? "" : "s")})"
                : "Advanced multi-server setup";
            toolTip1.SetToolTip(_advancedServersButton, tip);
        }

        void UpdateUrlControlsEnabled()
        {
            bool remote = _rdoUseRemote.Checked;
            _serverUrlTextbox.Enabled = remote;
            if (!remote)
            {
                _serverStatusDot.Image = HopsFunctionMgr.StatusNoneIcon();
                SetStatusTip("Not tested");
            }
        }

        void UpdateLocalOnlyControlsEnabled()
        {
            // macOS hides these entirely (set in the constructor); on Windows, gray them out
            // when Use Remote is selected so the relationship to the radio is obvious.
            if (Rhino.Runtime.HostUtils.RunningOnOSX) return;
            bool local = _rdoUseLocal.Checked;
            _hideWorkerWindows.Enabled = local;
            _launchWorkerAtStart.Enabled = local;
            _childComputeCount.Enabled = local;
            _updateChildCountButton.Enabled = local;
        }

        void UseLocalClicked(object sender, EventArgs e)
        {
            if (_rdoUseLocal.Checked) return;
            _rdoUseLocal.Checked = true;
            _rdoUseRemote.Checked = false;
            autoTestTimer.Stop();
            HopsAppSettings.UseLocalServer = true;
            UpdateUrlControlsEnabled();
            UpdateLocalOnlyControlsEnabled();
        }

        void UseRemoteClicked(object sender, EventArgs e)
        {
            if (_rdoUseRemote.Checked) return;
            _rdoUseRemote.Checked = true;
            _rdoUseLocal.Checked = false;
            HopsAppSettings.UseLocalServer = false;
            UpdateUrlControlsEnabled();
            UpdateLocalOnlyControlsEnabled();
            string url = _serverUrlTextbox.Text.Trim();
            if (!string.IsNullOrEmpty(url))
            {
                autoTestTimer.Stop();
                autoTestTimer.Start();
            }
        }

        void ServerUrlChanged(object sender, EventArgs e)
        {
            if (suppressUrlChange) return;
            _serverStatusDot.Image = HopsFunctionMgr.StatusNoneIcon();
            SetStatusTip("Testing soon...");
            autoTestTimer.Stop();
            autoTestTimer.Start();
        }

        async void TestCurrentUrl()
        {
            string url = _serverUrlTextbox.Text.Trim();
            // Always preserve URL changes so toggling back to Use Remote restores the right value.
            HopsAppSettings.Servers = string.IsNullOrEmpty(url) ? new string[0] : new[] { url };
            UpdateAdvancedButtonTooltip();
            if (string.IsNullOrEmpty(url) || !_rdoUseRemote.Checked)
            {
                _serverStatusDot.Image = HopsFunctionMgr.StatusNoneIcon();
                SetStatusTip(_rdoUseRemote.Checked ? "Not tested" : "Select Use remote server to test");
                return;
            }

            testCts?.Cancel();
            testCts = new CancellationTokenSource();
            var ct = testCts.Token;

            _serverStatusDot.Image = HopsFunctionMgr.StatusWarningIcon();
            SetStatusTip("Testing " + url + "...");

            string baseUrl = url.TrimEnd('/');
            var sw = Stopwatch.StartNew();
            try
            {
                // /validate is the purpose-built probe: cheap liveness + API key check in one
                // hop, with no child wakeup (unlike /version, which would blow our 3s budget on cold start).
                string probeUrl = baseUrl + "/validate";
                // Match RemoteDefinition's auth: send the RhinoComputeKey header when an API key is set.
                using (var request = new HttpRequestMessage(HttpMethod.Get, probeUrl))
                {
                    string apiKey = HopsAppSettings.APIKey;
                    if (!string.IsNullOrEmpty(apiKey))
                        request.Headers.Add("RhinoComputeKey", apiKey);
                    using (var resp = await testClient.SendAsync(request, ct).ConfigureAwait(true))
                    {
                        if (ct.IsCancellationRequested) return;
                        // Older server without /validate — fall back to /healthcheck so we can
                        // at least report reachability. Key validation isn't possible in that case.
                        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            await ProbeHealthcheckFallback(baseUrl, sw, ct);
                            return;
                        }
                        sw.Stop();
                        if (!resp.IsSuccessStatusCode)
                        {
                            _serverStatusDot.Image = HopsFunctionMgr.StatusErrorIcon();
                            string hint = (int)resp.StatusCode == 401 ? " (check API key)" : "";
                            SetStatusTip($"HTTP {(int)resp.StatusCode}{hint} from {probeUrl}");
                            return;
                        }
                        _serverStatusDot.Image = HopsFunctionMgr.StatusOkIcon();
                        SetStatusTip($"Valid URL ({sw.ElapsedMilliseconds}ms)");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                if (ct.IsCancellationRequested) return;
                _serverStatusDot.Image = HopsFunctionMgr.StatusErrorIcon();
                SetStatusTip($"Timed out after {sw.ElapsedMilliseconds}ms (3s probe limit)");
            }
            catch (Exception ex)
            {
                _serverStatusDot.Image = HopsFunctionMgr.StatusErrorIcon();
                SetStatusTip("Unreachable: " + ex.Message);
            }
        }

        async System.Threading.Tasks.Task ProbeHealthcheckFallback(string baseUrl, Stopwatch sw, CancellationToken ct)
        {
            // /validate 404'd — the server hasn't been updated to include it. /healthcheck still
            // proves reachability. Send the API key header too: rhino.compute's ApiKeyMiddleware
            // doesn't exempt GET (unlike compute.geometry's), so an unauthenticated GET would
            // bounce back as 401 and we'd misreport an authenticated-but-old server as broken.
            string probeUrl = baseUrl + "/healthcheck";
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, probeUrl))
                {
                    string apiKey = HopsAppSettings.APIKey;
                    if (!string.IsNullOrEmpty(apiKey))
                        request.Headers.Add("RhinoComputeKey", apiKey);
                    using (var resp = await testClient.SendAsync(request, ct).ConfigureAwait(true))
                    {
                        sw.Stop();
                        if (ct.IsCancellationRequested) return;
                        if (!resp.IsSuccessStatusCode)
                        {
                            _serverStatusDot.Image = HopsFunctionMgr.StatusErrorIcon();
                            string hint = (int)resp.StatusCode == 401 ? " (check API key)" : "";
                            SetStatusTip($"HTTP {(int)resp.StatusCode}{hint} from {probeUrl}");
                            return;
                        }
                        _serverStatusDot.Image = HopsFunctionMgr.StatusOkIcon();
                        SetStatusTip($"Reachable ({sw.ElapsedMilliseconds}ms) — server is too old for /validate; API key not checked");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                if (ct.IsCancellationRequested) return;
                _serverStatusDot.Image = HopsFunctionMgr.StatusErrorIcon();
                SetStatusTip($"Timed out after {sw.ElapsedMilliseconds}ms (3s probe limit)");
            }
            catch (Exception ex)
            {
                _serverStatusDot.Image = HopsFunctionMgr.StatusErrorIcon();
                SetStatusTip("Unreachable: " + ex.Message);
            }
        }

        void AdvancedServersClicked(object sender, EventArgs e)
        {
            var dlg = new MultiServerDialog();
            if (dlg.ShowModal(Grasshopper.Instances.EtoDocumentEditor))
            {
                InitServerSourceFromSettings();
            }
        }

        private void APIKeyTextboxChanged(object sender, EventArgs e)
        {
            HopsAppSettings.APIKey = _apiKeyTextbox.Text;
            // The cached status reflects the previous key; re-probe so the indicator stays honest.
            if (_rdoUseRemote.Checked && !string.IsNullOrWhiteSpace(_serverUrlTextbox.Text))
            {
                _serverStatusDot.Image = HopsFunctionMgr.StatusNoneIcon();
                SetStatusTip("Testing soon...");
                autoTestTimer.Stop();
                autoTestTimer.Start();
            }
        }

    }
}
