namespace Hops
{
    // Eto.Drawing.Bitmap loaders for icons used by the Eto dialogs (MultiServerDialog,
    // FunctionSourcesDialog). Separate from HopsFunctionMgr because that file returns
    // System.Drawing.Image for WinForms PictureBox; Eto.Forms.ImageView/Drawable wants
    // Eto.Drawing.Image. Both classes read the same embedded PNGs.
    static class HopsEtoIcons
    {
        static Eto.Drawing.Bitmap statusOk;
        static Eto.Drawing.Bitmap statusError;
        static Eto.Drawing.Bitmap statusWarning;
        static Eto.Drawing.Bitmap statusNone;
        static Eto.Drawing.Bitmap addRow;
        static Eto.Drawing.Bitmap deleteRow;

        static Eto.Drawing.Bitmap LoadEmbedded(string resource)
        {
            using (var stream = typeof(HopsComponent).Assembly.GetManifestResourceStream(resource))
                return new Eto.Drawing.Bitmap(stream);
        }

        public static Eto.Drawing.Bitmap StatusOk => statusOk ?? (statusOk = LoadEmbedded("Hops.resources.OK_24x24.png"));
        public static Eto.Drawing.Bitmap StatusError => statusError ?? (statusError = LoadEmbedded("Hops.resources.Error_24x24.png"));
        public static Eto.Drawing.Bitmap StatusWarning => statusWarning ?? (statusWarning = LoadEmbedded("Hops.resources.Warning_24x24.png"));
        public static Eto.Drawing.Bitmap StatusNone => statusNone ?? (statusNone = LoadEmbedded("Hops.resources.None_24x24.png"));
        public static Eto.Drawing.Bitmap AddRow => addRow ?? (addRow = LoadEmbedded("Hops.resources.AddRow_96x96.png"));
        public static Eto.Drawing.Bitmap DeleteRow => deleteRow ?? (deleteRow = LoadEmbedded("Hops.resources.DeleteRow_96x96.png"));
    }
}
