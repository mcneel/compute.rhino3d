using System;

namespace Hops
{
    class SetDefinitionForm : Eto.Forms.Dialog<bool>
    {
        public SetDefinitionForm(string currentPath)
        {
            Path = currentPath;

            Title = "Set Definition";

            bool onWindows = Rhino.Runtime.HostUtils.RunningOnWindows;
            DefaultButton = new Eto.Forms.Button { Text = onWindows ? "OK" : "Apply" };
            DefaultButton.Click += (sender, e) => Close(true);
            AbortButton = new Eto.Forms.Button { Text = "C&ancel" };
            AbortButton.Click += (sender, e) => Close(false);
            var buttons = new Eto.Forms.TableLayout();
            if (onWindows)
            {
                buttons.Spacing = new Eto.Drawing.Size(5, 5);
                buttons.Rows.Add(new Eto.Forms.TableRow(null, DefaultButton, AbortButton));
            }
            else
                buttons.Rows.Add(new Eto.Forms.TableRow(null, AbortButton, DefaultButton));
            // Use the shared UnderlineTextBox so the input style matches MultiServerDialog and
            // FunctionSourcesDialog (no inset 3D border on Windows; native rendering on macOS).
            var textbox = new UnderlineTextBox
            {
                Width = 250,
                PlaceholderText = "URL or Path"
            };
            if (!string.IsNullOrWhiteSpace(Path))
            {
                textbox.Text = Path;
            }
            // Use the custom StyledButton for visual consistency with the file/folder picker
            // buttons in MultiServerDialog and FunctionSourcesDialog (rounded corners, blue
            // hover fill, DPI-aware hairline stroke). The Rhino.UI.Controls.ImageButton this
            // replaced stretched its icon and used a darker grey hover background.
            var folderIcon = Rhino.Resources.Assets.Rhino.Eto.Icons.TryGet(
                Rhino.Resources.ResourceIds.FolderopenPng,
                new Eto.Drawing.Size(24, 24));
            var filePickButton = new StyledButton("")
            {
                MinimumSize = new Eto.Drawing.Size(28, 24),
                Icon = folderIcon,
                ToolTip = "Select an existing Grasshopper definition"
            };
            filePickButton.Click += (sender, e) =>
            {
                var dlg = new Eto.Forms.OpenFileDialog();
                dlg.Filters.Add(new Eto.Forms.FileFilter("Grasshopper Document", ".gh", ".ghx"));
                // work around an issue with the parent window on Mac
                Eto.Forms.Window parent = onWindows ? this : null;
                if (dlg.ShowDialog(parent) == Eto.Forms.DialogResult.Ok)
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
