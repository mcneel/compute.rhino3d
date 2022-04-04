using System;
using System.Drawing;
using System.Windows.Forms;

namespace Hops
{
    public class FunctionSourceRow : TableLayoutPanel
    {
        private FolderBrowserDialog _folderBrowserDlg;
        public string SourceName { get; set; }
        public string SourcePath { get; set; }
        public Button EditButton { get; set; }
        public TextBox PathTextBox { get; set; }
        public CheckBox RowCheckbox { get; set; }

        public event EventHandler<UpdateRowArgs> UpdateRow;

 
        private void OnUpdateRow(string nameToUpdate)
        {
            if (UpdateRow != null)
            {
                PathTextBox.Text = SourceName;
                UpdateRow(this, new UpdateRowArgs { RowName = nameToUpdate });
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
            ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 17));
            ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1.0F));
            ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 24));
            Location = new System.Drawing.Point(0, 0);
            Name = "FunctionSourceRow";
            Margin = new Padding(0);
            Size = new Size(295, 24);
            Dock = DockStyle.Fill;

            EditButton = InitButton();
            PathTextBox = InitTextBox(SourceName, SourcePath);
            RowCheckbox = InitCheckbox();

            EditButton.Click += (s, e) =>
            {
                var form = new SetFunctionSourceForm(SourcePath.Trim(), SourceName.Trim());
                if (form.ShowModal(Grasshopper.Instances.EtoDocumentEditor))
                {
                    string nameToUpdate = SourceName;
                    SourcePath = form.Path;
                    SourceName = form.Name;
                    OnUpdateRow(nameToUpdate);
                }
            };

            RowCheckbox.CheckedChanged += (s, e) =>
            {
                string nameToUpdate = SourceName;
                OnUpdateRow(nameToUpdate);
            };

            Controls.Add(RowCheckbox, 0, 0);
            Controls.Add(PathTextBox, 1, 0);
            Controls.Add(EditButton, 2, 0);
        }

        Button InitButton()
        {
            Button btn = new Button();
            btn.Size = new Size(22, 22);
            btn.Margin = new Padding(0,0,0,1);
            btn.Image = Hops.Properties.Resources.edit_16x16;
            btn.Name = "SetPathButton";
            return btn;
        }

        TextBox InitTextBox(string name, string path)
        {
            TextBox txt = new TextBox();
            txt.Dock = DockStyle.Fill;
            txt.Margin = new Padding(1);
            txt.Name = "PathTextbox";
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
            cb.Anchor = AnchorStyles.Top;
            cb.Size = new Size(17, 24);
            cb.Margin = new Padding(-1);
            cb.Name = "SourceCheckbox";
            return cb;
        }
    }
    public class UpdateRowArgs : EventArgs
    {
        public string RowName { get; set; }
    }

}
