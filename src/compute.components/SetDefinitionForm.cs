using System;

namespace Compute.Components
{
    class SetDefinitionForm : Eto.Forms.Dialog<bool>
    {
        public SetDefinitionForm(string currentPath)
        {
            Path = currentPath;

            Title = "Set Definition";

            bool windows = Rhino.Runtime.HostUtils.RunningOnWindows;
            DefaultButton = new Eto.Forms.Button { Text = windows ? "OK" : "Apply" };
            DefaultButton.Click += (sender, e) => Close(true);
            AbortButton = new Eto.Forms.Button { Text = "C&ancel" };
            AbortButton.Click += (sender, e) => Close(false);
            var buttons = new Eto.Forms.TableLayout();
            if (windows)
            {
                buttons.Spacing = new Eto.Drawing.Size(5, 5);
                buttons.Rows.Add(new Eto.Forms.TableRow(null, DefaultButton, AbortButton));
            }
            else
                buttons.Rows.Add(new Eto.Forms.TableRow(null, AbortButton, DefaultButton));
            var textbox = new Eto.Forms.TextBox();
            textbox.Size = new Eto.Drawing.Size(250, -1);
            textbox.PlaceholderText = "URL or Path";
            if (!string.IsNullOrWhiteSpace(Path))
            {
                textbox.Text = Path;
            }
            var filePickButton = new Rhino.UI.Controls.ImageButton();
            filePickButton.Image = Rhino.Resources.Assets.Rhino.Eto.Bitmaps.TryGet(Rhino.Resources.ResourceIds.FolderopenPng, new Eto.Drawing.Size(24, 24));
            filePickButton.Click += (sender, e) =>
            {
                var dlg = new Eto.Forms.OpenFileDialog();
                dlg.Filters.Add(new Eto.Forms.FileFilter("Grasshopper Document", ".gh", ".ghx"));
                if (dlg.ShowDialog(this) == Eto.Forms.DialogResult.Ok)
                {
                    textbox.Text = dlg.FileName;
                }
            };
            var locationRow = new Eto.Forms.StackLayout
            {
                Orientation = Eto.Forms.Orientation.Horizontal,
                Spacing = buttons.Spacing.Width,
                Items = { textbox, filePickButton }
            };
            Content = new Eto.Forms.TableLayout
            {
                Padding = new Eto.Drawing.Padding(10),
                Spacing = new Eto.Drawing.Size(5, 5),
                Rows = {
                        new Eto.Forms.TableRow { ScaleHeight = true, Cells = { locationRow } },
                        buttons
                    }
            };
            Closed += (s, e) => { Path = textbox.Text; };
        }

        public string Path
        {
            get;
            set;
        }
    }
}
