
namespace Hops
{
    partial class HopsAppSettingsUserControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
      this._serversTextBox = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // _serversTextBox
      // 
      this._serversTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._serversTextBox.Location = new System.Drawing.Point(3, 1);
      this._serversTextBox.Multiline = true;
      this._serversTextBox.Name = "_serversTextBox";
      this._serversTextBox.Size = new System.Drawing.Size(586, 109);
      this._serversTextBox.TabIndex = 1;
      // 
      // HopsAppSettingsUserControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this._serversTextBox);
      this.Name = "HopsAppSettingsUserControl";
      this.Size = new System.Drawing.Size(599, 124);
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox _serversTextBox;
    }
}
