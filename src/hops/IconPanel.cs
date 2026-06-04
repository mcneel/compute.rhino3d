using System;
using System.Drawing;
using System.Windows.Forms;

namespace Hops
{
    // Custom-painted icon button modeled on the Grasshopper preferences UI (see
    // GH_GHALoadingOptionsFrontEnd in the rhino9 repo, src4\rhino4\Plug-ins\Grasshopper).
    // Inherits from GH_DoubleBufferedPanel for proper double-buffering plus a Mac-friendly
    // transparent BackColor. Renders the icon via GH_GraphicsUtil.RenderIcon (DPI-aware
    // scaling, which sidesteps the macOS WinForms shim ignoring PictureBox.SizeMode=Zoom)
    // and draws the standard Grasshopper blue hover highlight via RenderHighlightBox.
    //
    // Manual MouseUp -> Pressed dispatch avoids the macOS shim's unreliability around
    // PictureBox.Click and PictureBox.Paint events.
    sealed class IconPanel : Grasshopper.GUI.GH_DoubleBufferedPanel
    {
        Image icon;
        bool hovered;
        bool hoverHighlight = true;
        int iconPadding = 0;

        public event EventHandler Pressed;

        public Image Icon
        {
            get { return icon; }
            set
            {
                if (icon == value) return;
                icon = value;
                Invalidate();
            }
        }

        public bool HoverHighlight
        {
            get { return hoverHighlight; }
            set
            {
                if (hoverHighlight == value) return;
                hoverHighlight = value;
                Invalidate();
            }
        }

        // Pixels to inset the icon frame from the control bounds on each side. Useful when the
        // source icon is edge-to-edge (e.g. our 96x96 SettingsIcon) and would otherwise fill
        // the whole button. Hover highlight still uses the full ClientRectangle.
        public int IconPadding
        {
            get { return iconPadding; }
            set
            {
                if (iconPadding == value) return;
                iconPadding = value;
                Invalidate();
            }
        }

        public IconPanel()
        {
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (!hovered) { hovered = true; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (hovered) { hovered = false; Invalidate(); }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left && ClientRectangle.Contains(e.Location))
                Pressed?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Paint inside a centered square inset 1px from the panel bounds. The square
            // accommodates AutoScale=Font producing non-uniform X/Y ratios on Windows (the
            // panel ends up slightly non-square), and the symmetric 1px inset on every side
            // keeps the hover box's edges from being clipped — `- 2` for the side plus the
            // even centering produces (1, 1) offsets from the panel bounds. Earlier `- 1`
            // left the top/left edges at 0 (integer-division offset of 0) which the macOS
            // shim cropped at the top.
            var client = ClientRectangle;
            int side = Math.Max(0, Math.Min(client.Width, client.Height) - 2);
            var square = new Rectangle(
                client.X + (client.Width - side) / 2,
                client.Y + (client.Height - side) / 2,
                side,
                side);

            if (hovered && hoverHighlight)
                Grasshopper.GUI.GH_GraphicsUtil.RenderHighlightBox(e.Graphics, square, 2);
            if (icon != null)
            {
                var frame = iconPadding > 0
                    ? Rectangle.Inflate(square, -iconPadding, -iconPadding)
                    : square;
                Grasshopper.GUI.GH_GraphicsUtil.RenderIcon(e.Graphics, frame, icon);
            }
        }
    }
}
