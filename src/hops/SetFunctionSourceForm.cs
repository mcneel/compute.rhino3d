using System;
using Eto.Forms;

namespace Hops
{
    class SetFunctionSourceForm : Dialog<bool>
    {
        public SetFunctionSourceForm(string currentPath, string currentName)
        {
            Path = currentPath;
            Name = currentName;

            Title = "Set Function Source";

            var srcName_Textbox = new TextBox();
            srcName_Textbox.Size = new Eto.Drawing.Size(250, -1);
            srcName_Textbox.PlaceholderText = "Nickname";
            if (!string.IsNullOrWhiteSpace(Name))
            {
                srcName_Textbox.Text = Name;
            }
            srcName_Textbox.Focus();

            var srcPath_Textbox = new TextBox();
            srcPath_Textbox.Size = new Eto.Drawing.Size(250, -1);
            srcPath_Textbox.PlaceholderText = "URL or Path";
            if (!string.IsNullOrWhiteSpace(Path))
            {
                srcPath_Textbox.Text = Path;
            }

            bool onWindows = Rhino.Runtime.HostUtils.RunningOnWindows;
            DefaultButton = new Button { Text = onWindows ? "OK" : "Apply" };
            DefaultButton.Click += (sender, e) => 
            {
                if (String.IsNullOrEmpty(srcName_Textbox.Text) || String.IsNullOrEmpty(srcPath_Textbox.Text))
                {
                    DialogResult result = MessageBox.Show(this, "Nickname and path are required fields.", "Required Field Missing", MessageBoxButtons.OK, MessageBoxType.Information, MessageBoxDefaultButton.OK);
                    if(result == DialogResult.Ok)
                        return;
                } 
                Close(true); 
            };
            AbortButton = new Button { Text = "C&ancel" };
            AbortButton.Click += (sender, e) => Close(false);
            var buttons = new TableLayout();
            if (onWindows)
            {
                buttons.Spacing = new Eto.Drawing.Size(5, 5);
                buttons.Rows.Add(new TableRow(null, DefaultButton, AbortButton));
            }
            else
                buttons.Rows.Add(new TableRow(null, AbortButton, DefaultButton));

            var srcNameRow = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Items = { srcName_Textbox }
            };

            var filePickButton = new Rhino.UI.Controls.ImageButton();
            filePickButton.Image = Rhino.Resources.Assets.Rhino.Eto.Bitmaps.TryGet(Rhino.Resources.ResourceIds.FolderopenPng, new Eto.Drawing.Size(24, 24));
            filePickButton.Click += (sender, e) =>
            {
                var dlg = new SelectFolderDialog();
                // work around an issue with the parent window on Mac
                Window parent = onWindows ? this : null;
                if (dlg.ShowDialog(parent) == DialogResult.Ok)
                {
                    srcPath_Textbox.Text = dlg.Directory;
                }
            };

            var srcPathRow = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = buttons.Spacing.Width,
                Items = { srcPath_Textbox, filePickButton }
            };

            Content = new TableLayout
            {
                Padding = new Eto.Drawing.Padding(10),
                Spacing = new Eto.Drawing.Size(5, 5),
                Rows = {
                        new TableRow { ScaleHeight = true, Cells = { srcNameRow } },
                        new TableRow { ScaleHeight = true, Cells = { srcPathRow } },
                        buttons
                    }
            };
            Closed += (s, e) => { Path = srcPath_Textbox.Text; Name = srcName_Textbox.Text; };
        }

        public string Path
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
    }
}
