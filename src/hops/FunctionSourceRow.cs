using System;
using System.Drawing;
using System.Windows.Forms;

namespace Hops
{
    public class FunctionSourceRow : TableLayoutPanel
    {
        public string SourceName { get; set; }
        public string SourcePath { get; set; }
        public Button EditButton { get; set; }
        public TextBox PathTextBox { get; set; }
        public CheckBox RowCheckbox { get; set; }

        public event EventHandler<ReplaceRowArgs> ReplaceRow;

        private void OnReplaceRow(string rowToReplace)
        {
            if (ReplaceRow != null)
            {
                PathTextBox.Text = SourceName;
                ReplaceRow(this, new ReplaceRowArgs { RowName = rowToReplace });
            }
        }

        public FunctionSourceRow(string name, string path)
        {
            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(path))
                return;
            SourceName = name;
            SourcePath = path;
            RowStyles.Clear();
            ColumnStyles.Clear();
            RowCount = 1;
            ColumnCount = 3;
            RowStyles.Add(new RowStyle(SizeType.Percent, 1.0F));
            ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize, 0.05F));
            ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 0.9F));
            ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize, 0.05F));
            Location = new System.Drawing.Point(0, 0);
            Name = "FunctionSourceRow";

            PathTextBox = InitTextBox(SourceName, SourcePath);
            EditButton = InitButton();
            RowCheckbox = InitCheckbox();

            Anchor = AnchorStyles.Left | AnchorStyles.Right;
            Height = PathTextBox.Height;

            EditButton.Click += (s, e) =>
            {
                var form = new SetFunctionSourceForm(SourcePath.Trim(), SourceName.Trim());
                if (form.ShowModal(Grasshopper.Instances.EtoDocumentEditor))
                {
                    string rowToUpdate = SourceName;
                    SourcePath = form.Path;
                    SourceName = form.Name;
                    OnReplaceRow(rowToUpdate);
                }
            };

            RowCheckbox.CheckedChanged += (s, e) =>
            {
                string rowToUpdate = SourceName;
                OnReplaceRow(rowToUpdate);
            };

            Controls.Add(RowCheckbox, 0, 0);
            Controls.Add(PathTextBox, 1, 0);
            Controls.Add(EditButton, 2, 0);
        }

        Button InitButton()
        {
            Button btn = new Button();
            btn.Size = new Size(PathTextBox.Height, PathTextBox.Height);
            btn.Margin = new Padding(0);
            btn.Image = HopsFunctionMgr.EditIcon();
            btn.Name = "EditRowButton";
            return btn;
        }

        TextBox InitTextBox(string name, string path)
        {
            TextBox txt = new TextBox();
            txt.Dock = DockStyle.Fill;
            txt.Margin = new Padding(0);
            txt.Name = "Textbox";
            txt.ReadOnly = true;
            txt.BackColor = System.Drawing.SystemColors.Window;
            txt.Text = name;
            txt.MouseHover += Txt_MouseHover;
            return txt;
        }

        private void Txt_MouseHover(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            int VisibleTime = 3000;

            ToolTip toolTip = new ToolTip();
            toolTip.Show(SourcePath, textBox, 24, -24, VisibleTime);
        }

        CheckBox InitCheckbox()
        {
            CheckBox cb = new CheckBox();
            cb.Checked = false;
            cb.Size = new Size(PathTextBox.Height, PathTextBox.Height);
            cb.Name = "IsSelectedCheckbox";
            return cb;
        }
    }
    public class ReplaceRowArgs : EventArgs
    {
        public string RowName { get; set; }
    }

}
