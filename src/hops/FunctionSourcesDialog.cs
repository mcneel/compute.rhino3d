using System;
using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;

namespace Hops
{
    // Eto modal that manages the Hops function-source list (the [name -> path] entries that
    // surface in the right-click "Available Functions" menu on every Hops component).
    //
    // Replaces the old inline TableLayoutPanel + ToolStrip UI in HopsAppSettingsUserControl,
    // which had a fragile manual-resize chain (parent panel/groupbox/control heights were
    // mutated each time a row was added or removed). Eto handles layout properly, and the
    // dialog only appears on demand, so the preferences panel no longer needs to know about
    // function-source row geometry.
    class FunctionSourcesDialog : Dialog<bool>
    {
        readonly List<FunctionSourceEntryRow> rows = new List<FunctionSourceEntryRow>();
        readonly StackLayout rowsContainer;

        public FunctionSourcesDialog()
        {
            Title = "Hops function sources";
            Resizable = true;
            ClientSize = new Size(560, 260);

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
                ToolTip = "Add source"
            };
            addButton.Click += (s, e) => AddRow("", "");

            var deleteButton = new StyledButton("")
            {
                MinimumSize = new Size(28, 24),
                Icon = HopsEtoIcons.DeleteRow,
                ToolTip = "Delete selected"
            };
            deleteButton.Click += (s, e) => DeleteSelected();

            // Focus sink mirrors the MultiServerDialog pattern — absorbs initial keyboard focus
            // so the delete checkbox of the first row doesn't auto-grab it with a dashed cue.
            var focusSink = new Drawable { Size = new Size(1, 1), CanFocus = true };

            var toolbar = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Items = { addButton, deleteButton, focusSink }
            };

            // Pin rowsContainer to the top of the scroller via a TableLayout with a height-
            // scaling spacer row below it. Without this, the macOS Scrollable defaults to
            // bottom-aligning its content when ExpandContentHeight=false: the StackLayout
            // collapses to the bottom of the scroller and new items pile upward (reversed
            // visual order). With the spacer absorbing the leftover vertical space, the
            // rows always sit at the top on both platforms.
            var topAlignedHost = new TableLayout
            {
                Rows =
                {
                    new TableRow(rowsContainer),
                    new TableRow { ScaleHeight = true }
                }
            };
            var scroller = new Scrollable
            {
                Border = BorderType.None,
                ExpandContentWidth = true,
                ExpandContentHeight = true,
                Content = topAlignedHost
            };

            bool onWindows = Rhino.Runtime.HostUtils.RunningOnWindows;
            var saveButton = new StyledButton(onWindows ? "Save" : "Apply")
            {
                MinimumSize = new Size(80, 24)
            };
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
                    new TableRow { ScaleHeight = true, Cells = { scroller } },
                    buttonRow
                }
            };

            // Populate from saved settings.
            HopsAppSettings.InitFunctionSources();
            if (HopsAppSettings.FunctionSources.Count == 0)
            {
                AddRow("", "");
            }
            else
            {
                foreach (var src in HopsAppSettings.FunctionSources)
                    AddRow(src.SourceName, src.SourcePath);
            }

            Shown += (s, e) => focusSink.Focus();
        }

        void AddRow(string name, string path)
        {
            var row = new FunctionSourceEntryRow(this, name, path);
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
                AddRow("", "");
        }

        void Save()
        {
            // Replace HopsAppSettings.FunctionSources with new (headless) FunctionSourceRow
            // instances built from the dialog's working data. Headless = the WinForms control
            // is constructed but never added to a parent layout, since the preferences panel
            // no longer hosts an inline list. Downstream consumers (HopsComponent's right-click
            // menu) only read SourceName/SourcePath, so the lack of parenting is fine.
            HopsAppSettings.FunctionSources.Clear();
            foreach (var row in rows)
            {
                string name = (row.NameText ?? "").Trim();
                string path = (row.PathText ?? "").Trim();
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(path))
                    continue;
                HopsAppSettings.FunctionSources.Add(new FunctionSourceRow(name, path));
            }
            HopsUIHelper.UpdateFunctionSourceSettings();
        }
    }

    class FunctionSourceEntryRow : Panel
    {
        readonly CheckBox deleteCheck;
        readonly UnderlineTextBox nameBox;
        readonly UnderlineTextBox pathBox;

        public FunctionSourceEntryRow(Dialog<bool> parentDialog, string name, string path)
        {
            deleteCheck = new CheckBox { ToolTip = "Select for delete" };
            // Give Name a modest minimum width — enough for short nicknames like "Templates" —
            // and let Path soak up the rest via scaleWidth. Paths are typically much longer than
            // names, so letting them share width 50/50 wastes screen real estate.
            nameBox = new UnderlineTextBox { Text = name, PlaceholderText = "Nickname", Width = 140 };
            pathBox = new UnderlineTextBox { Text = path, PlaceholderText = "URL or folder path" };

            // Shared Rhino folder-open icon — same glyph used by SetDefinitionForm, so picker
            // buttons read consistently as "open a file/folder dialog" everywhere in Hops.
            var folderIcon = Rhino.Resources.Assets.Rhino.Eto.Icons.TryGet(
                Rhino.Resources.ResourceIds.FolderopenPng,
                new Size(24, 24));
            var pickerButton = new StyledButton("")
            {
                MinimumSize = new Size(28, 24),
                Icon = folderIcon,
                ToolTip = "Choose folder..."
            };
            pickerButton.Click += (s, e) =>
            {
                var dlg = new SelectFolderDialog();
                if (!string.IsNullOrWhiteSpace(pathBox.Text))
                    dlg.Directory = pathBox.Text;
                // Mac quirk: passing the dialog as parent crashes; pass null there.
                var owner = Rhino.Runtime.HostUtils.RunningOnWindows ? parentDialog : null;
                if (dlg.ShowDialog(owner) == DialogResult.Ok)
                    pathBox.Text = dlg.Directory;
            };

            Content = new TableLayout
            {
                Spacing = new Size(5, 0),
                Rows =
                {
                    new TableRow(
                        new TableCell(deleteCheck),
                        new TableCell(nameBox),                       // auto-size to its 140 width
                        new TableCell(pathBox, scaleWidth: true),     // takes the rest of the row
                        new TableCell(pickerButton)
                    )
                }
            };
        }

        public string NameText => nameBox.Text ?? string.Empty;
        public string PathText => pathBox.Text ?? string.Empty;
        public bool IsSelectedForDelete => deleteCheck.Checked == true;
    }
}
