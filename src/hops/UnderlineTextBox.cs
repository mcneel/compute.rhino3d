using System;
using Eto.Drawing;
using Eto.Forms;

namespace Hops
{
    // Eto wrapper around Eto.Forms.TextBox. On Windows it mimics the Win11/Fluent style used by
    // the main settings panel — hides the inset 3D border and paints a thin underline that
    // thickens to blue when the field is focused. On macOS it short-circuits to native rendering
    // (keeps the platform-standard NSTextField bezel and focus ring) so the dialogs look
    // platform-native rather than Windows-mimicking.
    //
    // Windows layout: vertical TableLayout with the wrapped TextBox on top and a 2px-tall
    // Drawable beneath it that paints the underline. The TextBox's GotFocus/LostFocus toggles
    // the underline state and invalidates the Drawable.
    class UnderlineTextBox : Panel
    {
        static readonly Color DefaultLine = Color.FromArgb(0x83, 0x83, 0x83);
        static readonly Color FocusLine = Color.FromArgb(0x00, 0x67, 0xC0);

        readonly TextBox textBox;
        Drawable line;          // null on non-Windows (no underline painted)
        bool focused;

        public UnderlineTextBox()
        {
            bool onWindows = Rhino.Runtime.HostUtils.RunningOnWindows;
            // On Windows hide the native border so our underline is the only chrome. On other
            // platforms keep ShowBorder=true so NSTextField (etc.) renders with its native
            // bezel and the dialog reads as platform-native.
            textBox = new TextBox { ShowBorder = !onWindows };
            textBox.TextChanged += (s, e) => TextChanged?.Invoke(this, EventArgs.Empty);

            if (onWindows)
            {
                textBox.GotFocus += (s, e) => { focused = true; line.Invalidate(); };
                textBox.LostFocus += (s, e) => { focused = false; line.Invalidate(); };

                line = new Drawable { Height = 2 };
                line.Paint += OnLinePaint;

                Content = new TableLayout
                {
                    Spacing = new Size(0, 0),
                    Padding = new Padding(0),
                    Rows =
                    {
                        new TableRow(new TableCell(textBox, scaleWidth: true)) { ScaleHeight = true },
                        new TableRow(new TableCell(line, scaleWidth: true))
                    }
                };
            }
            else
            {
                // Native pass-through: no custom chrome, no underline, just the textbox.
                Content = textBox;
            }
        }

        void OnLinePaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var color = focused ? FocusLine : DefaultLine;
            // Aim for true 1-device-pixel hairlines regardless of DPI. Eto draws in logical
            // pixels, so a literal "1" here becomes 1.5 device pixels at 150% scaling — visibly
            // thicker than the native WinForms hairline. Dividing by LogicalPixelSize gives back
            // the device-pixel target (e.g., 1 / 1.5 ≈ 0.67 logical px → 1 device px).
            float scale = Math.Max(1f, (float)Screen.PrimaryScreen.LogicalPixelSize);
            float thickness = (focused ? 2f : 1f) / scale;
            float y = Math.Max(0f, line.Height - thickness);
            g.FillRectangle(color, new RectangleF(0, y, line.Width, thickness));
        }

        public string Text
        {
            get => textBox.Text;
            set => textBox.Text = value;
        }

        public string PlaceholderText
        {
            get => textBox.PlaceholderText;
            set => textBox.PlaceholderText = value;
        }

        public bool ReadOnly
        {
            get => textBox.ReadOnly;
            set => textBox.ReadOnly = value;
        }

        public event EventHandler<EventArgs> TextChanged;

        // Hides Control.Focus(). When something asks the panel to focus, we actually want
        // keyboard focus on the wrapped TextBox so typing lands there.
        public new void Focus() => textBox.Focus();
    }
}
