using System;
using System.Drawing;
using System.Windows.Forms;

namespace Hops
{
    class FunctionSourceRow : TableLayoutPanel
    {
        public string SourceName { get; set; }
        public string SourcePath { get; set; }
        public Button SetPathButton { get; set; }
        public TextBox PathTextBox { get; set; }
        public CheckBox RowCheckbox { get; set; }
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
            Size = new System.Drawing.Size(295, 24);
            Dock = DockStyle.Fill;

            SetPathButton = InitButton();
            PathTextBox = InitTextBox(SourceName);
            RowCheckbox = InitCheckbox();

            Controls.Add(RowCheckbox, 0, 0);
            Controls.Add(PathTextBox, 1, 0);
            Controls.Add(SetPathButton, 2, 0);
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

        TextBox InitTextBox(string name)
        {
            TextBox txt = new TextBox();
            txt.Dock = DockStyle.Fill;
            txt.Margin = new Padding(1);
            txt.Name = "PathTextbox";
            txt.Text = name;
            return txt;
        }
        CheckBox InitCheckbox()
        {
            CheckBox cb = new CheckBox();
            cb.Checked = false;
            cb.Anchor = AnchorStyles.Top;
            cb.Size = new Size(17, 24);
            cb.Margin = new Padding(-1);
            return cb;
        }
    }
}
