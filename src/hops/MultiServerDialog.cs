using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;

namespace Hops
{
    class MultiServerDialog : Dialog<bool>
    {
        readonly List<ServerRow> rows = new List<ServerRow>();
        readonly StackLayout rowsContainer;

        public MultiServerDialog()
        {
            Title = "Hops compute servers";
            Resizable = true;
            ClientSize = new Size(380, 208);
            // Suppress the white flash that Eto.WinForms shows for a frame between
            // creating the underlying Form (default Control.BackColor = White) and the
            // first content paint. SystemColors.Control matches the dialog's normal body
            // colour so any pre-paint frame blends in instead of flashing white.
            BackgroundColor = Eto.Drawing.SystemColors.Control;

            rowsContainer = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 4,
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };

            var addButton = new StyledButton("")
            {
                MinimumSize = new Size(28, 24),
                Icon = HopsEtoIcons.AddRow,
                ToolTip = "Add server"
            };
            addButton.Click += (s, e) => AddRow("");

            var deleteButton = new StyledButton("")
            {
                MinimumSize = new Size(28, 24),
                Icon = HopsEtoIcons.DeleteRow,
                ToolTip = "Delete selected"
            };
            deleteButton.Click += (s, e) => DeleteSelected();

            // 1×1 transparent Drawable used purely as a focus sink. Eto/the OS assigns initial
            // keyboard focus to the first focusable control in tab order, which would otherwise
            // be the delete checkbox of the first row (causing its dashed focus rectangle to
            // show). Drawable doesn't paint a focus indicator, so focusing this sink on Shown
            // results in no visible focus anywhere — while Tab nav still works normally because
            // all real controls remain focusable.
            var focusSink = new Drawable { Size = new Size(1, 1), CanFocus = true };

            var toolbar = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Items = { addButton, deleteButton, focusSink }
            };

            // Pin rowsContainer to the top via a TableLayout with a ScaleHeight spacer below.
            // Without this, the macOS Scrollable bottom-aligns its content when
            // ExpandContentHeight=false and rows pile upward in reversed visual order.
            var topAlignedHost = new TableLayout
            {
                Rows =
                {
                    new TableRow(rowsContainer),
                    new TableRow { ScaleHeight = true }
                }
            };
            // Custom-coloured outline via a 1px-padded Panel. The Scrollable's BackgroundColor
            // is explicitly set to match the dialog body so the Panel's BackgroundColor only
            // shows through the 1px padding on each side (the border) — without bleeding
            // through the otherwise-transparent row panels inside the Scrollable.
            //
            // The Drawable+Paint approach was visually equivalent but caused a single-frame
            // white flicker on modal open (Eto.WinForms Drawable repaint timing). Plain Panel
            // with BackgroundColor doesn't trigger custom Paint and avoids the flicker.
            var scroller = new Scrollable
            {
                Border = BorderType.None,
                BackgroundColor = Eto.Drawing.SystemColors.Control,
                ExpandContentWidth = true,
                ExpandContentHeight = true,
                Content = topAlignedHost
            };
            var scrollerBorder = new Panel
            {
                BackgroundColor = Color.FromArgb(0xDC, 0xDC, 0xDC),
                Padding = new Padding(1),
                Content = scroller
            };

            bool onWindows = Rhino.Runtime.HostUtils.RunningOnWindows;
            // Custom-drawn buttons all around, including the Save/Cancel pair — we route Enter
            // and Escape into them via the dialog's KeyDown handler below, replacing the role
            // that DefaultButton/AbortButton would normally play.
            var saveButton = new StyledButton(onWindows ? "Save" : "Apply") { MinimumSize = new Size(80, 24) };
            saveButton.Click += (s, e) => { Save(); Close(true); };
            var cancelButton = new StyledButton("Cancel") { MinimumSize = new Size(80, 24) };
            cancelButton.Click += (s, e) => Close(false);

            KeyDown += (s, e) =>
            {
                if (e.Key == Keys.Enter)
                {
                    Save();
                    Close(true);
                    e.Handled = true;
                }
                else if (e.Key == Keys.Escape)
                {
                    Close(false);
                    e.Handled = true;
                }
            };

            var buttonRow = new TableLayout { Spacing = new Size(5, 5) };
            if (onWindows)
                buttonRow.Rows.Add(new TableRow(null, saveButton, cancelButton));
            else
                buttonRow.Rows.Add(new TableRow(null, cancelButton, saveButton));

            Content = new TableLayout
            {
                Padding = new Padding(10),
                Spacing = new Size(5, 5),
                Rows =
                {
                    new TableRow(toolbar),
                    new TableRow { ScaleHeight = true, Cells = { scrollerBorder } },
                    buttonRow
                }
            };

            var servers = HopsAppSettings.Servers;
            if (servers.Length == 0)
                AddRow("");
            else
                foreach (var url in servers)
                    AddRow(url);

            // Drop initial focus onto the invisible sink so no real control shows a focus
            // indicator on dialog open. Tab still navigates through the actual focusable
            // controls (delete checkboxes, URL textboxes, buttons) normally.
            Shown += (s, e) => focusSink.Focus();
        }

        void AddRow(string url)
        {
            var row = new ServerRow(url);
            rows.Add(row);
            rowsContainer.Items.Add(new StackLayoutItem(row, HorizontalAlignment.Stretch));
        }

        void DeleteSelected()
        {
            for (int i = rows.Count - 1; i >= 0; i--)
            {
                if (rows[i].IsSelectedForDelete)
                {
                    rowsContainer.Items.RemoveAt(i);
                    rows.RemoveAt(i);
                }
            }
            if (rows.Count == 0)
                AddRow("");
        }

        void Save()
        {
            var urls = new List<string>();
            foreach (var row in rows)
            {
                string u = row.Url.Trim();
                if (!string.IsNullOrEmpty(u))
                    urls.Add(u);
            }
            HopsAppSettings.Servers = urls.ToArray();
        }
    }

    class ServerRow : Panel
    {
        static readonly HttpClient TestClient = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };

        // Icon accessors live in HopsEtoIcons (shared with FunctionSourcesDialog).
        static Eto.Drawing.Bitmap StatusOk => HopsEtoIcons.StatusOk;
        static Eto.Drawing.Bitmap StatusError => HopsEtoIcons.StatusError;
        static Eto.Drawing.Bitmap StatusWarning => HopsEtoIcons.StatusWarning;
        static Eto.Drawing.Bitmap StatusNone => HopsEtoIcons.StatusNone;

        readonly CheckBox deleteCheck;
        readonly ImageView statusDot;
        readonly UnderlineTextBox urlTextBox;
        readonly UITimer autoTestTimer;

        CancellationTokenSource testCts;

        public ServerRow(string url)
        {
            deleteCheck = new CheckBox { ToolTip = "Select for delete" };

            statusDot = new ImageView
            {
                Size = new Size(12, 12),
                Image = StatusNone,
                Cursor = Cursors.Pointer
            };
            SetStatusTip("Not tested");
            // Eto.Forms.ImageView has no Click event, so use MouseUp filtered to the primary button.
            statusDot.MouseUp += async (s, e) =>
            {
                if (e.Buttons == MouseButtons.Primary)
                {
                    autoTestTimer.Stop();
                    await TestAsync();
                }
            };

            urlTextBox = new UnderlineTextBox { Text = url, PlaceholderText = "http://host:port" };

            // 800ms debounce — matches the main settings panel.
            autoTestTimer = new UITimer { Interval = 0.8 };
            autoTestTimer.Elapsed += async (s, e) =>
            {
                autoTestTimer.Stop();
                await TestAsync();
            };

            urlTextBox.TextChanged += (s, e) =>
            {
                SetStatus(StatusNone, "Testing soon...");
                autoTestTimer.Stop();
                autoTestTimer.Start();
            };

            Content = new TableLayout
            {
                Spacing = new Size(5, 0),
                Rows =
                {
                    new TableRow(
                        new TableCell(deleteCheck),
                        new TableCell(statusDot),
                        new TableCell(urlTextBox, scaleWidth: true)
                    )
                }
            };

            // If pre-populated with a URL when the dialog opens, kick off the initial probe
            // after a debounce so all rows in the dialog don't fire at the exact same instant.
            if (!string.IsNullOrWhiteSpace(url))
                autoTestTimer.Start();
        }

        public string Url => urlTextBox.Text ?? string.Empty;
        public bool IsSelectedForDelete => deleteCheck.Checked == true;

        public void FocusUrl() => urlTextBox.Focus();

        void SetStatus(Eto.Drawing.Image image, string tooltip)
        {
            statusDot.Image = image;
            SetStatusTip(tooltip);
        }

        // Same shape as the main settings panel: rich per-state message plus a constant
        // hint reminding the user that the dot is clickable for a manual re-probe.
        void SetStatusTip(string message)
        {
            statusDot.ToolTip = message + "\r\n(click to retest)";
        }

        async Task TestAsync()
        {
            string url = (urlTextBox.Text ?? "").Trim();
            if (string.IsNullOrEmpty(url))
            {
                SetStatus(StatusNone, "Empty URL");
                return;
            }
            testCts?.Cancel();
            testCts = new CancellationTokenSource();
            var ct = testCts.Token;
            SetStatus(StatusWarning, "Testing " + url + "...");
            string baseUrl = url.TrimEnd('/');
            var sw = Stopwatch.StartNew();
            try
            {
                string probeUrl = baseUrl + "/validate";
                // Match RemoteDefinition's auth so /validate's API-key branch can succeed.
                using (var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, probeUrl))
                {
                    string apiKey = HopsAppSettings.APIKey;
                    if (!string.IsNullOrEmpty(apiKey))
                        request.Headers.Add("RhinoComputeKey", apiKey);
                    using (var resp = await TestClient.SendAsync(request, ct).ConfigureAwait(true))
                    {
                        if (ct.IsCancellationRequested) return;
                        // Older server without /validate — fall back to /healthcheck for liveness.
                        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            await ProbeHealthcheckFallbackAsync(baseUrl, sw, ct);
                            return;
                        }
                        sw.Stop();
                        if (!resp.IsSuccessStatusCode)
                        {
                            string hint = (int)resp.StatusCode == 401 ? " (check API key)" : "";
                            SetStatus(StatusError, $"HTTP {(int)resp.StatusCode}{hint} from {probeUrl}");
                            return;
                        }
                        SetStatus(StatusOk, $"Valid URL ({sw.ElapsedMilliseconds}ms)");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                if (ct.IsCancellationRequested) return;
                SetStatus(StatusError, $"Timed out after {sw.ElapsedMilliseconds}ms (3s probe limit)");
            }
            catch (Exception ex)
            {
                SetStatus(StatusError, "Unreachable: " + ex.Message);
            }
        }

        async Task ProbeHealthcheckFallbackAsync(string baseUrl, Stopwatch sw, CancellationToken ct)
        {
            // /validate 404'd — older server. Fall back to /healthcheck for reachability. Send
            // the API key header: rhino.compute's ApiKeyMiddleware requires it on every request
            // (no GET exemption), so an unauthenticated GET would falsely come back 401.
            string probeUrl = baseUrl + "/healthcheck";
            try
            {
                using (var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, probeUrl))
                {
                    string apiKey = HopsAppSettings.APIKey;
                    if (!string.IsNullOrEmpty(apiKey))
                        request.Headers.Add("RhinoComputeKey", apiKey);
                    using (var resp = await TestClient.SendAsync(request, ct).ConfigureAwait(true))
                    {
                        sw.Stop();
                        if (ct.IsCancellationRequested) return;
                        if (!resp.IsSuccessStatusCode)
                        {
                            string hint = (int)resp.StatusCode == 401 ? " (check API key)" : "";
                            SetStatus(StatusError, $"HTTP {(int)resp.StatusCode}{hint} from {probeUrl}");
                            return;
                        }
                        SetStatus(StatusOk, $"Reachable ({sw.ElapsedMilliseconds}ms) — server is too old for /validate; API key not checked");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                if (ct.IsCancellationRequested) return;
                SetStatus(StatusError, $"Timed out after {sw.ElapsedMilliseconds}ms (3s probe limit)");
            }
            catch (Exception ex)
            {
                SetStatus(StatusError, "Unreachable: " + ex.Message);
            }
        }
    }

    // Custom-drawn button used inside MultiServerDialog. Eto.Forms.Button defers to the native OS
    // button on each backend, which means we can't easily impose a corner radius, custom fill, or
    // a specific hover-stroke color on it. Drawable lets us paint everything ourselves with the
    // exact Windows-11/Fluent action-button colors used by this UI.
    class StyledButton : Drawable
    {
        static readonly Color DefaultFill   = Color.FromArgb(0xFD, 0xFD, 0xFD);
        static readonly Color DefaultStroke = Color.FromArgb(0xD0, 0xD0, 0xD0);
        static readonly Color HoverFill     = Color.FromArgb(0xE0, 0xEE, 0xF9);
        static readonly Color HoverStroke   = Color.FromArgb(0x00, 0x78, 0xD4);
        const float CornerRadius = 3f;

        readonly Font font = SystemFonts.Default();
        string label;
        Eto.Drawing.Image icon;
        bool hovered;
        bool pressed;
        bool focused;

        public event EventHandler<EventArgs> Click;

        public string Text
        {
            get => label;
            set { label = value; Invalidate(); }
        }

        /// <summary>Optional icon. When set and Text is empty, the icon renders centered.</summary>
        public Eto.Drawing.Image Icon
        {
            get => icon;
            set { icon = value; Invalidate(); }
        }

        public StyledButton(string text)
        {
            label = text ?? string.Empty;
            Cursor = Cursors.Pointer;
            CanFocus = true;

            MouseEnter += (s, e) => { hovered = true; Invalidate(); };
            MouseLeave += (s, e) => { hovered = false; pressed = false; Invalidate(); };
            MouseDown += (s, e) =>
            {
                pressed = true;
                Focus();
                Invalidate();
            };
            MouseUp += (s, e) =>
            {
                bool fire = pressed && hovered;
                pressed = false;
                Invalidate();
                if (fire) Click?.Invoke(this, EventArgs.Empty);
            };
            GotFocus += (s, e) => { focused = true; Invalidate(); };
            LostFocus += (s, e) => { focused = false; Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.AntiAlias = true;

            var fill = hovered ? HoverFill : DefaultFill;
            // Show the blue stroke when the button is the keyboard-focused default action too —
            // that way the Save button on dialog open visibly indicates "Enter activates this".
            var stroke = (hovered || focused) ? HoverStroke : DefaultStroke;

            // Render the stroke as a "ring": fill the outer rounded rect with stroke color, then
            // fill an inset inner rounded rect with the body color. Both FillPath calls rasterize
            // symmetrically against pixel boundaries, sidestepping the asymmetric pen rendering
            // that DrawPath produces at non-integer DPI scales.
            //
            // Ring width is scaled to 1 *device* pixel regardless of DPI — at 150% scaling a
            // literal 1-logical-pixel ring would render at 1.5 device pixels, visibly thicker
            // than the native WinForms hairline. Dividing the inset by LogicalPixelSize matches
            // the OS hairline weight at any scale factor.
            float scale = Math.Max(1f, (float)Screen.PrimaryScreen.LogicalPixelSize);
            float aa = 0.5f / scale;
            float ring = 1f / scale;
            float innerOffset = aa + ring;
            float innerRadius = Math.Max(0.5f, CornerRadius - ring);
            using (var outerPath = RoundedPath(new RectangleF(aa, aa, Math.Max(0, Width - aa * 2), Math.Max(0, Height - aa * 2)), CornerRadius))
                g.FillPath(stroke, outerPath);
            using (var innerPath = RoundedPath(new RectangleF(innerOffset, innerOffset, Math.Max(0, Width - innerOffset * 2), Math.Max(0, Height - innerOffset * 2)), innerRadius))
                g.FillPath(fill, innerPath);

            if (!string.IsNullOrEmpty(label))
            {
                var size = font.MeasureString(label);
                g.DrawText(font, Colors.Black,
                    (Width - size.Width) / 2f,
                    (Height - size.Height) / 2f,
                    label);
            }
            else if (icon != null)
            {
                // Inset 4px on each side so the icon doesn't crowd the rounded edge, and draw
                // it as a square scaled from the larger source.
                const int padding = 4;
                int side = Math.Max(0, Math.Min(Width, Height) - padding * 2);
                var iconRect = new RectangleF(
                    (Width - side) / 2f,
                    (Height - side) / 2f,
                    side,
                    side);
                g.ImageInterpolation = ImageInterpolation.High;
                g.DrawImage(icon, iconRect);
            }
        }

        static GraphicsPath RoundedPath(RectangleF rect, float radius)
        {
            var path = new GraphicsPath();
            float d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
