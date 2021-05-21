
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
      this.components = new System.ComponentModel.Container();
      this._serversTextBox = new System.Windows.Forms.TextBox();
      this._hideWorkerWindows = new System.Windows.Forms.CheckBox();
      this._launchWorkerAtStart = new System.Windows.Forms.CheckBox();
      this._btnClearMemCache = new System.Windows.Forms.Button();
      this._lblCacheCount = new System.Windows.Forms.Label();
      this._childComputeCount = new System.Windows.Forms.NumericUpDown();
      this._updateChildCountButton = new System.Windows.Forms.Button();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this._maxConcurrentRequestsTextbox = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this._childComputeCount)).BeginInit();
      this.SuspendLayout();
      // 
      // _serversTextBox
      // 
      this._serversTextBox.Location = new System.Drawing.Point(3, 1);
      this._serversTextBox.Multiline = true;
      this._serversTextBox.Name = "_serversTextBox";
      this._serversTextBox.Size = new System.Drawing.Size(586, 91);
      this._serversTextBox.TabIndex = 1;
      // 
      // _hideWorkerWindows
      // 
      this._hideWorkerWindows.AutoSize = true;
      this._hideWorkerWindows.Location = new System.Drawing.Point(3, 213);
      this._hideWorkerWindows.Name = "_hideWorkerWindows";
      this._hideWorkerWindows.Size = new System.Drawing.Size(409, 29);
      this._hideWorkerWindows.TabIndex = 2;
      this._hideWorkerWindows.Text = "Hide Rhino.Compute Console Window";
      this._hideWorkerWindows.UseVisualStyleBackColor = true;
      // 
      // _launchWorkerAtStart
      // 
      this._launchWorkerAtStart.AutoSize = true;
      this._launchWorkerAtStart.Location = new System.Drawing.Point(3, 256);
      this._launchWorkerAtStart.Name = "_launchWorkerAtStart";
      this._launchWorkerAtStart.Size = new System.Drawing.Size(402, 29);
      this._launchWorkerAtStart.TabIndex = 3;
      this._launchWorkerAtStart.Text = "Launch Local Rhino.Compute at Start";
      this._launchWorkerAtStart.UseVisualStyleBackColor = true;
      // 
      // _btnClearMemCache
      // 
      this._btnClearMemCache.Location = new System.Drawing.Point(3, 154);
      this._btnClearMemCache.Name = "_btnClearMemCache";
      this._btnClearMemCache.Size = new System.Drawing.Size(321, 45);
      this._btnClearMemCache.TabIndex = 4;
      this._btnClearMemCache.Text = "Clear Hops Memory Cache";
      this._btnClearMemCache.UseVisualStyleBackColor = true;
      // 
      // _lblCacheCount
      // 
      this._lblCacheCount.AutoSize = true;
      this._lblCacheCount.Location = new System.Drawing.Point(333, 164);
      this._lblCacheCount.Name = "_lblCacheCount";
      this._lblCacheCount.Size = new System.Drawing.Size(254, 25);
      this._lblCacheCount.TabIndex = 5;
      this._lblCacheCount.Text = "(1000000 items in cache)";
      // 
      // _childComputeCount
      // 
      this._childComputeCount.Location = new System.Drawing.Point(4, 301);
      this._childComputeCount.Name = "_childComputeCount";
      this._childComputeCount.Size = new System.Drawing.Size(120, 31);
      this._childComputeCount.TabIndex = 6;
      // 
      // _updateChildCountButton
      // 
      this._updateChildCountButton.Location = new System.Drawing.Point(136, 296);
      this._updateChildCountButton.Name = "_updateChildCountButton";
      this._updateChildCountButton.Size = new System.Drawing.Size(276, 39);
      this._updateChildCountButton.TabIndex = 7;
      this._updateChildCountButton.Text = "Child Process Count";
      this._updateChildCountButton.UseVisualStyleBackColor = true;
      // 
      // _maxConcurrentRequestsTextbox
      // 
      this._maxConcurrentRequestsTextbox.Location = new System.Drawing.Point(3, 105);
      this._maxConcurrentRequestsTextbox.Name = "_maxConcurrentRequestsTextbox";
      this._maxConcurrentRequestsTextbox.Size = new System.Drawing.Size(80, 31);
      this._maxConcurrentRequestsTextbox.TabIndex = 8;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(102, 110);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(262, 25);
      this.label1.TabIndex = 9;
      this.label1.Text = "Max Concurrent Requests";
      // 
      // HopsAppSettingsUserControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.label1);
      this.Controls.Add(this._maxConcurrentRequestsTextbox);
      this.Controls.Add(this._updateChildCountButton);
      this.Controls.Add(this._childComputeCount);
      this.Controls.Add(this._lblCacheCount);
      this.Controls.Add(this._btnClearMemCache);
      this.Controls.Add(this._launchWorkerAtStart);
      this.Controls.Add(this._hideWorkerWindows);
      this.Controls.Add(this._serversTextBox);
      this.Name = "HopsAppSettingsUserControl";
      this.Size = new System.Drawing.Size(599, 348);
      ((System.ComponentModel.ISupportInitialize)(this._childComputeCount)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox _serversTextBox;
        private System.Windows.Forms.CheckBox _hideWorkerWindows;
        private System.Windows.Forms.CheckBox _launchWorkerAtStart;
        private System.Windows.Forms.Button _btnClearMemCache;
        private System.Windows.Forms.Label _lblCacheCount;
        private System.Windows.Forms.NumericUpDown _childComputeCount;
        private System.Windows.Forms.Button _updateChildCountButton;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TextBox _maxConcurrentRequestsTextbox;
        private System.Windows.Forms.Label label1;
    }
}
