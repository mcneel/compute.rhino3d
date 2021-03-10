
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
      this._hideWorkerWindows = new System.Windows.Forms.CheckBox();
      this._launchWorkerAtStart = new System.Windows.Forms.CheckBox();
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
      this._serversTextBox.Size = new System.Drawing.Size(586, 88);
      this._serversTextBox.TabIndex = 1;
      // 
      // _hideWorkerWindows
      // 
      this._hideWorkerWindows.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this._hideWorkerWindows.AutoSize = true;
      this._hideWorkerWindows.Location = new System.Drawing.Point(3, 104);
      this._hideWorkerWindows.Name = "_hideWorkerWindows";
      this._hideWorkerWindows.Size = new System.Drawing.Size(358, 29);
      this._hideWorkerWindows.TabIndex = 2;
      this._hideWorkerWindows.Text = "Hide Compute Console Windows";
      this._hideWorkerWindows.UseVisualStyleBackColor = true;
      // 
      // _launchWorkerAtStart
      // 
      this._launchWorkerAtStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this._launchWorkerAtStart.AutoSize = true;
      this._launchWorkerAtStart.Location = new System.Drawing.Point(3, 147);
      this._launchWorkerAtStart.Name = "_launchWorkerAtStart";
      this._launchWorkerAtStart.Size = new System.Drawing.Size(367, 29);
      this._launchWorkerAtStart.TabIndex = 3;
      this._launchWorkerAtStart.Text = "Launch Compute Console at Start";
      this._launchWorkerAtStart.UseVisualStyleBackColor = true;
      // 
      // HopsAppSettingsUserControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this._launchWorkerAtStart);
      this.Controls.Add(this._hideWorkerWindows);
      this.Controls.Add(this._serversTextBox);
      this.Name = "HopsAppSettingsUserControl";
      this.Size = new System.Drawing.Size(599, 193);
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox _serversTextBox;
        private System.Windows.Forms.CheckBox _hideWorkerWindows;
        private System.Windows.Forms.CheckBox _launchWorkerAtStart;
    }
}
