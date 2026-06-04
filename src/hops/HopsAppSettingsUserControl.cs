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

        // Measure a label's text in its actual font and resize the label to fit, with a small
        // padding so anti-aliased glyphs don't clip. Used on macOS where the system font is
        // wider than the Windows design metrics and design-time label widths would clip text.
        static void SizeLabelToText(System.Windows.Forms.Label lbl)
        {
            var size = TextRenderer.MeasureText(lbl.Text, lbl.Font);
            lbl.AutoSize = false;
            lbl.Size = new Size(size.Width + 2, 22);
            lbl.TextAlign = ContentAlignment.MiddleLeft;
        }

        // Used so we can vertically center the text inside multiline TextBoxes.
        [StructLayout(LayoutKind.Sequential)]
        struct RECT { public int Left, Top, Right, Bottom; }

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref RECT lParam);

        const uint EM_SETRECT = 0xB3;

        // Current status-dot tooltip message. The "(click to retest)" suffix is added by
        // StatusTooltipPopulating at hover time, not stored here. Updated by SetStatusTip.
        string currentStatusTip = "Not tested";

        void SetStatusTip(string message)
        {
            currentStatusTip = message;
        }

        void StatusTooltipPopulating(object sender, Grasshopper.GUI.GH_TooltipDisplayEventArgs e)
        {
            e.Title = "Server status";
            e.Text = currentStatusTip + ". Click to retest.";
        }

        void AdvancedTooltipPopulating(object sender, Grasshopper.GUI.GH_TooltipDisplayEventArgs e)
        {
            e.Title = "Advanced multi-server setup";
            e.Text = "Configure multiple rhino.compute servers";
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
            // macOS WinForms shim renders the designer layout differently — single-line vs
            // multiline textbox styling, font-based AutoScale drift, Label AutoSize defaults,
            // and a non-functional PictureBox+Paint for the gear button. All Mac-specific
            // overrides live in one place; the Windows path stays untouched.
            if (Rhino.Runtime.HostUtils.RunningOnOSX)
                ApplyMacLayoutAdjustments();
            if (Rhino.Runtime.HostUtils.RunningOnWindows)
                ApplyWindowsLayoutAdjustments();
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
            // Advanced gear button — IconPanel handles icon rendering (DPI-aware via
            // GH_GraphicsUtil.RenderIcon) and hover highlight. Tooltip uses Grasshopper's
            // GH_TooltipComponent so it matches the rest of the GH UI on both platforms.
            _advancedServersButton.Icon = HopsFunctionMgr.SettingsIcon();
            _advancedServersButton.IconPadding = 3;
            _advancedServersButton.Pressed += AdvancedServersClicked;
            var advancedTooltip = new Grasshopper.GUI.GH_TooltipComponent { Target = _advancedServersButton };
            advancedTooltip.PopulateTooltip += AdvancedTooltipPopulating;

            // Status dot — no hover highlight (it's a passive indicator), but still clickable
            // for a manual re-probe. Tooltip text is dynamic; we store it in currentStatusTip
            // and the PopulateTooltip handler reads from there each time the tooltip opens.
            _serverStatusDot.Icon = HopsFunctionMgr.StatusNoneIcon();
            _serverStatusDot.HoverHighlight = false;
            _serverStatusDot.Pressed += (s, e) =>
            {
                autoTestTimer.Stop();
                TestCurrentUrl();
            };
            var statusTooltip = new Grasshopper.GUI.GH_TooltipComponent { Target = _serverStatusDot };
            statusTooltip.PopulateTooltip += StatusTooltipPopulating;
            SetStatusTip("Not tested");

            // Defense in depth: ReadOnly=true should already block typing/pasting, but if any
            // input path slips through (drag-drop text, IME composition, etc.) block it at the
            // key-event layer. SuppressKeyPress stops the key from reaching the textbox.
            _serverUrlTextbox.KeyPress += (s, e) =>
            {
                if (_serverUrlTextbox.ReadOnly) e.Handled = true;
            };
            _serverUrlTextbox.KeyDown += (s, e) =>
            {
                if (_serverUrlTextbox.ReadOnly && e.Control && (e.KeyCode == Keys.V || e.KeyCode == Keys.X))
                {
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                }
            };
            // Prevent selection by shifting focus away whenever the disabled textbox would
            // gain it. BeginInvoke defers the focus shift to the next message loop iteration
            // so it runs after WinForms has finished processing the current click event.
            _serverUrlTextbox.Enter += (s, e) =>
            {
                if (_serverUrlTextbox.ReadOnly)
                    BeginInvoke((Action)(() => _serverUrlTextbox.Parent?.Focus()));
            };

            if (Rhino.Runtime.HostUtils.RunningOnOSX)
            {
                _rdoUseLocal.Enabled = false;
                toolTip1.SetToolTip(_rdoUseLocal, "Local rhino.compute is not supported on macOS");
                // Defense in depth: the macOS WinForms shim doesn't reliably honor
                // CheckBox.Enabled=false (it still fires clicks) nor AutoCheck=false (it auto-flips
                // Checked). Snap Local back to false if anything manages to flip it.
                _rdoUseLocal.CheckedChanged += (s, e) =>
                {
                    if (_rdoUseLocal.Checked && !_rdoUseLocal.Enabled) _rdoUseLocal.Checked = false;
                };
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
                // Show the local-only controls grayed out so users can see what's available
                // on Windows and where the disabled state lives, instead of an empty gap.
                _hideWorkerWindows.Enabled = false;
                _hideWorkerWindows.Checked = HopsAppSettings.HideWorkerWindows;
                _launchWorkerAtStart.Enabled = false;
                _launchWorkerAtStart.Checked = HopsAppSettings.LaunchWorkerAtStart;
                _childComputeCount.Enabled = false;
                _childComputeCount.Value = HopsAppSettings.LocalWorkerCount;
                _updateChildCountButton.Enabled = false;
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
                    if (ShowDialogCenteredOnPrefs(dlg))
                        RefreshFunctionSourceCount();
                };
            }
            else
            {
                _gpboxFunctionMgr.Visible = false;
                Size = new Size(Size.Width, _btnClearMemCache.Bottom + 4);
            }
        }

        // macOS-only overrides applied right after InitializeComponent. Goal: keep the panel
        // functional and readable on the macOS WinForms shim without touching the Windows
        // layout. Two recurring shim quirks drive most of this code: (a) AutoScaleMode=Font
        // multiplies every absolute coordinate by ~1.3 at runtime because the shim's font
        // metrics differ from Windows's design metrics (6F x 13F); (b) Enabled=false and
        // AutoCheck=false aren't honored on several control types (CheckBox, NumericUpDown).
        void ApplyMacLayoutAdjustments()
        {
            // Disable AutoScale entirely — fixes the root cause of the layout drift that pushed
            // the URL row below the group, the cache label above the button, and inflated the
            // status dot. Design coordinates now render at their literal values on Mac too.
            AutoScaleMode = AutoScaleMode.None;

            // Single-line textboxes use NSTextField, which auto-centers text vertically and
            // uses the standard system font. Multiline=true (set by Designer to enable our
            // P/Invoke vertical-centering hack on Windows) uses NSTextView with a smaller
            // default font and top-anchored text on the shim — looks broken in a 22px row.
            _serverUrlTextbox.Multiline = false;
            _apiKeyTextbox.Multiline = false;
            _maxConcurrentRequestsTextbox.Multiline = false;
            _httpTimeoutTextbox.Multiline = false;

            // Measure each label's text in its actual font and size the control to fit, plus
            // 2px breathing room so the rendering doesn't clip. MiddleLeft text alignment puts
            // text at the left edge of the label; the textbox then sits 4px past the label's
            // right edge, producing a consistent ~6px visible gap on every row regardless of
            // how long the label text is.
            SizeLabelToText(label2);
            SizeLabelToText(label1);
            SizeLabelToText(label3);
            _apiKeyTextbox.Location = new Point(label2.Right + 4, _apiKeyTextbox.Top);
            _maxConcurrentRequestsTextbox.Location = new Point(label1.Right + 4, _maxConcurrentRequestsTextbox.Top);
            _httpTimeoutTextbox.Location = new Point(label3.Right + 4, _httpTimeoutTextbox.Top);

            // Right-edge alignment to match Windows behavior: URL/API-key share a right edge,
            // and Max-concurrent/Timeout share a different right edge. URL's absolute right is
            // computed via its container offset since it lives inside the group. The +3 fudge
            // on API key matches the URL textbox's visually-rendered right edge on Mac, which
            // sits a couple of pixels past the computed value.
            int urlAbsRight = _gpboxComputeServer.Left + _serverUrlTextbox.Right;
            _apiKeyTextbox.Size = new Size(urlAbsRight - _apiKeyTextbox.Left + 5, _apiKeyTextbox.Height);
            const int numericRightEdge = 215;
            _maxConcurrentRequestsTextbox.Size = new Size(numericRightEdge - _maxConcurrentRequestsTextbox.Left, _maxConcurrentRequestsTextbox.Height);
            _httpTimeoutTextbox.Size = new Size(numericRightEdge - _httpTimeoutTextbox.Left, _httpTimeoutTextbox.Height);

            // Force fixed sizes on the four checkboxes too — Mac shim Label AutoSize defaults
            // would otherwise re-flow them and break the designed row geometry.
            _rdoUseLocal.AutoSize = false; _rdoUseLocal.Size = new Size(280, 22);
            _rdoUseRemote.AutoSize = false; _rdoUseRemote.Size = new Size(180, 22);
            _hideWorkerWindows.AutoSize = false; _hideWorkerWindows.Size = new Size(240, 22);
            _launchWorkerAtStart.AutoSize = false; _launchWorkerAtStart.Size = new Size(240, 22);

            // Local checkbox text — make the disabled state self-explanatory.
            _rdoUseLocal.Text = "Local rhino.compute (Windows only)";

            // Grow the Compute server source group so the URL row sits inside it. Then lay
            // out every row below the group at a consistent 5px gap. Function-sources group
            // also needs a few extra px so its button isn't clipped. Finally size the
            // UserControl based on the function group's actual bottom so nothing gets cut.
            const int computeGroupGrowth = 26;
            const int functionGroupGrowth = 12;
            const int rowGap = 5;
            const int rowHeight = 22;
            _gpboxComputeServer.Size = new Size(_gpboxComputeServer.Width, _gpboxComputeServer.Height + computeGroupGrowth);

            // Shift every child of the compute group up 4px to trim excess top padding inside
            // the group (the title-bar takes more room on the Mac shim than on Windows, but the
            // children don't need to be that far from the top).
            foreach (var ctrl in new Control[]
            {
                _rdoUseLocal, _advancedServersButton, _hideWorkerWindows, _launchWorkerAtStart,
                _childComputeCount, _updateChildCountButton, _rdoUseRemote,
                _labelServerUrl, _serverUrlTextbox, _serverStatusDot,
            })
            {
                ctrl.Top -= 4;
            }

            // +11 below the (now 4-shorter) group gives breathing room between the group
            // bottom and the first row beneath it.
            int rowTop = _gpboxComputeServer.Bottom + 11;
            label2.Top = rowTop; _apiKeyTextbox.Top = rowTop; rowTop += rowHeight + rowGap;
            label1.Top = rowTop; _maxConcurrentRequestsTextbox.Top = rowTop; rowTop += rowHeight + rowGap;
            label3.Top = rowTop; _httpTimeoutTextbox.Top = rowTop; rowTop += rowHeight + rowGap;
            _btnClearMemCache.Top = rowTop; rowTop += rowHeight + rowGap;
            // Extra +4 gap above the function-sources group so it doesn't crowd the clear-cache row.
            _gpboxFunctionMgr.Top = rowTop + 4;
            _gpboxFunctionMgr.Size = new Size(_gpboxFunctionMgr.Width, _gpboxFunctionMgr.Height + functionGroupGrowth);

            // Position the trailing labels using their adjacent button's runtime position.
            // Setting Size.Height to match the button's height + TextAlign=MiddleLeft makes the
            // label text auto-center vertically with the button regardless of font metrics.
            _lblCacheCount.AutoSize = false;
            _lblCacheCount.Size = new Size(150, _btnClearMemCache.Height);
            _lblCacheCount.Location = new Point(_btnClearMemCache.Right + 6, _btnClearMemCache.Top);
            _lblCacheCount.TextAlign = ContentAlignment.MiddleLeft;
            // The Manage sources button renders low in the group on Mac (group title-bar
            // sizing differs from Windows). 11px up empirically centers it. Position the
            // label after the move so it tracks the button's new Y.
            _manageFunctionSourcesButton.Top -= 11;
            _functionSourceCountLabel.AutoSize = false;
            _functionSourceCountLabel.Size = new Size(170, _manageFunctionSourcesButton.Height);
            _functionSourceCountLabel.Location = new Point(_manageFunctionSourcesButton.Right + 6, _manageFunctionSourcesButton.Top);
            _functionSourceCountLabel.TextAlign = ContentAlignment.MiddleLeft;

            Size = new Size(Width, _gpboxFunctionMgr.Bottom + 4);
            // Constrain the host from shrinking the panel narrower than the gear button's
            // anchor would let it slide over the Use Local checkbox text, or narrower than
            // the Hide/Launch checkbox text fits inside the group. Width 280 gives ~5px of
            // breathing room past both limits. Whether the GH prefs dialog honours this is
            // up to the host's layout engine — worth trying first as a no-cost mitigation.
            MinimumSize = new Size(280, _gpboxFunctionMgr.Bottom + 4);

            // The shim ignores Enabled=false / AutoCheck=false on Remote (just like Local).
            // Local should always be off, Remote should always be on. Snap back if user clicks.
            _rdoUseRemote.CheckedChanged += (s, e) =>
            {
                if (!_rdoUseRemote.Checked) _rdoUseRemote.Checked = true;
            };

            // The shim also ignores Enabled=false on NumericUpDown — its arrows still fire.
            // Snap the value back if it changes while disabled.
            decimal initialChildCount = HopsAppSettings.LocalWorkerCount;
            _childComputeCount.ValueChanged += (s, e) =>
            {
                if (!_childComputeCount.Enabled && _childComputeCount.Value != initialChildCount)
                    _childComputeCount.Value = initialChildCount;
            };
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
            UpdateUrlControlsEnabled();
            UpdateLocalOnlyControlsEnabled();
            if (_rdoUseRemote.Checked && !string.IsNullOrWhiteSpace(_serverUrlTextbox.Text))
                TestCurrentUrl();
        }

        void UpdateUrlControlsEnabled()
        {
            bool remote = _rdoUseRemote.Checked;
            // ReadOnly rather than Enabled — the latter forces SystemColors.Control on the
            // background and ignores our BackColor, leaving the disabled textbox visually
            // identical to the surrounding group. With ReadOnly we keep BackColor honored
            // and the textbox still rejects keyboard input.
            _serverUrlTextbox.ReadOnly = !remote;
            // Color.White explicitly (not SystemColors.Window) — the macOS WinForms shim
            // maps SystemColors.Window to a light grey, which made the enabled state look
            // identical to the disabled state on Mac. White renders consistently on both.
            _serverUrlTextbox.BackColor = remote ? Color.White : Color.FromArgb(0xF9, 0xF9, 0xF9);
            _serverUrlTextbox.ForeColor = remote ? SystemColors.WindowText : Color.FromArgb(0xA0, 0xA0, 0xA0);
            // Arrow cursor in disabled state signals "non-interactive"; clear any active
            // selection so the textbox doesn't carry highlight state into the disabled view.
            _serverUrlTextbox.Cursor = remote ? Cursors.IBeam : Cursors.Default;
            if (!remote)
            {
                _serverUrlTextbox.SelectionStart = 0;
                _serverUrlTextbox.SelectionLength = 0;
                _serverStatusDot.Icon = HopsFunctionMgr.StatusNoneIcon();
                SetStatusTip("Not tested");
            }
        }

        // Windows-only layout tweaks applied after InitializeComponent. Keeps the URL textbox's
        // native border (matching the other textboxes' Win11 focus-underline styling), moves
        // the Advanced gear button inward from the group's right edge, and adds breathing room
        // below the URL textbox + between the Compute server source group and the rows below.
        void ApplyWindowsLayoutAdjustments()
        {
            // Pull the gear button in from the group's right edge — Designer had it 3px away
            // which felt cramped. 5px shift puts it ~8px from the inner edge.
            _advancedServersButton.Left -= 5;

            // Grow the Compute server source group so the URL textbox has more breathing
            // room beneath it. Then shift every sibling control below the group down by the
            // same delta plus a 4px extra gap so the rows aren't pushed up against the group
            // bottom border. Form size grows to accommodate the shift.
            const int computeGroupGrowth = 4;
            const int extraGap = 4;
            int shift = computeGroupGrowth + extraGap;
            _gpboxComputeServer.Size = new Size(_gpboxComputeServer.Width, _gpboxComputeServer.Height + computeGroupGrowth);
            foreach (var ctrl in new Control[]
            {
                label2, _apiKeyTextbox,
                label1, _maxConcurrentRequestsTextbox,
                label3, _httpTimeoutTextbox,
                _btnClearMemCache, _lblCacheCount,
                _gpboxFunctionMgr,
            })
            {
                ctrl.Top += shift;
            }
            // Pull the Clear cache button + its label up 4px on top of the shift above. The
            // Designer left a ~3px gap below Timeout vs ~1px between the textbox rows above —
            // tightening it makes the rows look evenly spaced. 4px empirically matches the
            // textbox spacing better than the visible row metrics alone would suggest.
            _btnClearMemCache.Top -= 4;
            _lblCacheCount.Top -= 4;
            Size = new Size(Width, Height + shift);
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
            // macOS shim doesn't reliably block Click on disabled CheckBoxes; bail and force-revert.
            if (!_rdoUseLocal.Enabled)
            {
                if (_rdoUseLocal.Checked) _rdoUseLocal.Checked = false;
                return;
            }
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
            _serverStatusDot.Icon = HopsFunctionMgr.StatusNoneIcon();
            SetStatusTip("Testing soon...");
            autoTestTimer.Stop();
            autoTestTimer.Start();
        }

        async void TestCurrentUrl()
        {
            string url = _serverUrlTextbox.Text.Trim();
            // Always preserve URL changes so toggling back to Use Remote restores the right value.
            HopsAppSettings.Servers = string.IsNullOrEmpty(url) ? new string[0] : new[] { url };
            if (string.IsNullOrEmpty(url) || !_rdoUseRemote.Checked)
            {
                _serverStatusDot.Icon = HopsFunctionMgr.StatusNoneIcon();
                SetStatusTip(_rdoUseRemote.Checked ? "Not tested" : "Select Use remote server to test");
                return;
            }

            testCts?.Cancel();
            testCts = new CancellationTokenSource();
            var ct = testCts.Token;

            _serverStatusDot.Icon = HopsFunctionMgr.StatusWarningIcon();
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
                            _serverStatusDot.Icon = HopsFunctionMgr.StatusErrorIcon();
                            string hint = (int)resp.StatusCode == 401 ? " (check API key)" : "";
                            SetStatusTip($"HTTP {(int)resp.StatusCode}{hint} from {probeUrl}");
                            return;
                        }
                        _serverStatusDot.Icon = HopsFunctionMgr.StatusOkIcon();
                        SetStatusTip($"Valid URL ({sw.ElapsedMilliseconds}ms)");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                if (ct.IsCancellationRequested) return;
                _serverStatusDot.Icon = HopsFunctionMgr.StatusErrorIcon();
                SetStatusTip($"Timed out after {sw.ElapsedMilliseconds}ms (3s probe limit)");
            }
            catch (Exception ex)
            {
                _serverStatusDot.Icon = HopsFunctionMgr.StatusErrorIcon();
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
                            _serverStatusDot.Icon = HopsFunctionMgr.StatusErrorIcon();
                            string hint = (int)resp.StatusCode == 401 ? " (check API key)" : "";
                            SetStatusTip($"HTTP {(int)resp.StatusCode}{hint} from {probeUrl}");
                            return;
                        }
                        _serverStatusDot.Icon = HopsFunctionMgr.StatusOkIcon();
                        SetStatusTip($"Reachable ({sw.ElapsedMilliseconds}ms) — server is too old for /validate; API key not checked");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                if (ct.IsCancellationRequested) return;
                _serverStatusDot.Icon = HopsFunctionMgr.StatusErrorIcon();
                SetStatusTip($"Timed out after {sw.ElapsedMilliseconds}ms (3s probe limit)");
            }
            catch (Exception ex)
            {
                _serverStatusDot.Icon = HopsFunctionMgr.StatusErrorIcon();
                SetStatusTip("Unreachable: " + ex.Message);
            }
        }

        // Center an Eto modal on the prefs dialog (the parent of this UserControl) and show
        // it. Mirrors the pattern Grasshopper itself uses (GH_UnrecognizedObjectsForm):
        // CenterFormOnWindow before ShowModal, with the latter receiving the same parent.
        // Falls back to the GH canvas if the parent form can't be resolved (defensive).
        bool ShowDialogCenteredOnPrefs(Eto.Forms.Dialog<bool> dlg)
        {
            var parentForm = this.FindForm();
            var parentEto = parentForm != null ? Grasshopper.EtoExtensions.ToEto(parentForm) : null;
            if (parentEto != null)
                Grasshopper.GUI.GH_EtoUtil.CenterFormOnWindow(dlg, parentEto, true);
            return dlg.ShowModal(parentEto ?? Grasshopper.Instances.EtoDocumentEditor);
        }

        void AdvancedServersClicked(object sender, EventArgs e)
        {
            var dlg = new MultiServerDialog();
            if (ShowDialogCenteredOnPrefs(dlg))
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
                _serverStatusDot.Icon = HopsFunctionMgr.StatusNoneIcon();
                SetStatusTip("Testing soon...");
                autoTestTimer.Stop();
                autoTestTimer.Start();
            }
        }

    }
}
