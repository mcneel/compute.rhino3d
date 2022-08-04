using Grasshopper.GUI;
using System.Drawing;
using System.Windows.Forms;

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
            this.label2 = new System.Windows.Forms.Label();
            this._apiKeyTextbox = new System.Windows.Forms.TextBox();
            this._gpboxFunctionMgr = new System.Windows.Forms.GroupBox();
            this._functionSourceTable = new System.Windows.Forms.TableLayoutPanel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this._addFunctionSourceButton = new System.Windows.Forms.ToolStripButton();
            this._deleteFunctionSourceButton = new System.Windows.Forms.ToolStripButton();
            this.label3 = new System.Windows.Forms.Label();
            this._httpTimeoutTextbox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this._childComputeCount)).BeginInit();
            this._gpboxFunctionMgr.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _serversTextBox
            // 
            this._serversTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._serversTextBox.Location = new System.Drawing.Point(4, 2);
            this._serversTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._serversTextBox.Multiline = true;
            this._serversTextBox.Name = "_serversTextBox";
            this._serversTextBox.Size = new System.Drawing.Size(586, 91);
            this._serversTextBox.TabIndex = 1;
            // 
            // _hideWorkerWindows
            // 
            this._hideWorkerWindows.AutoSize = true;
            this._hideWorkerWindows.Location = new System.Drawing.Point(4, 288);
            this._hideWorkerWindows.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._hideWorkerWindows.Name = "_hideWorkerWindows";
            this._hideWorkerWindows.Size = new System.Drawing.Size(409, 29);
            this._hideWorkerWindows.TabIndex = 5;
            this._hideWorkerWindows.Text = "Hide Rhino.Compute Console Window";
            this._hideWorkerWindows.UseVisualStyleBackColor = true;
            // 
            // _launchWorkerAtStart
            // 
            this._launchWorkerAtStart.AutoSize = true;
            this._launchWorkerAtStart.Location = new System.Drawing.Point(4, 331);
            this._launchWorkerAtStart.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._launchWorkerAtStart.Name = "_launchWorkerAtStart";
            this._launchWorkerAtStart.Size = new System.Drawing.Size(402, 29);
            this._launchWorkerAtStart.TabIndex = 6;
            this._launchWorkerAtStart.Text = "Launch Local Rhino.Compute at Start";
            this._launchWorkerAtStart.UseVisualStyleBackColor = true;
            // 
            // _btnClearMemCache
            // 
            this._btnClearMemCache.Location = new System.Drawing.Point(2, 235);
            this._btnClearMemCache.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._btnClearMemCache.Name = "_btnClearMemCache";
            this._btnClearMemCache.Size = new System.Drawing.Size(320, 42);
            this._btnClearMemCache.TabIndex = 4;
            this._btnClearMemCache.Text = "Clear Hops Memory Cache";
            this._btnClearMemCache.UseVisualStyleBackColor = true;
            // 
            // _lblCacheCount
            // 
            this._lblCacheCount.AutoSize = true;
            this._lblCacheCount.Location = new System.Drawing.Point(332, 244);
            this._lblCacheCount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._lblCacheCount.Name = "_lblCacheCount";
            this._lblCacheCount.Size = new System.Drawing.Size(254, 25);
            this._lblCacheCount.TabIndex = 5;
            this._lblCacheCount.Text = "(1000000 items in cache)";
            // 
            // _childComputeCount
            // 
            this._childComputeCount.Location = new System.Drawing.Point(4, 375);
            this._childComputeCount.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._childComputeCount.Name = "_childComputeCount";
            this._childComputeCount.Size = new System.Drawing.Size(120, 31);
            this._childComputeCount.TabIndex = 7;
            // 
            // _updateChildCountButton
            // 
            this._updateChildCountButton.Location = new System.Drawing.Point(136, 373);
            this._updateChildCountButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._updateChildCountButton.Name = "_updateChildCountButton";
            this._updateChildCountButton.Size = new System.Drawing.Size(276, 42);
            this._updateChildCountButton.TabIndex = 8;
            this._updateChildCountButton.Text = "Child Process Count";
            this._updateChildCountButton.UseVisualStyleBackColor = true;
            // 
            // _maxConcurrentRequestsTextbox
            // 
            this._maxConcurrentRequestsTextbox.Location = new System.Drawing.Point(272, 146);
            this._maxConcurrentRequestsTextbox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._maxConcurrentRequestsTextbox.Name = "_maxConcurrentRequestsTextbox";
            this._maxConcurrentRequestsTextbox.Size = new System.Drawing.Size(150, 31);
            this._maxConcurrentRequestsTextbox.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 154);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(262, 25);
            this.label1.TabIndex = 11;
            this.label1.Text = "Max Concurrent Requests";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 108);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 25);
            this.label2.TabIndex = 10;
            this.label2.Text = "API Key";
            // 
            // _apiKeyTextbox
            // 
            this._apiKeyTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._apiKeyTextbox.Location = new System.Drawing.Point(102, 102);
            this._apiKeyTextbox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._apiKeyTextbox.Name = "_apiKeyTextbox";
            this._apiKeyTextbox.Size = new System.Drawing.Size(488, 31);
            this._apiKeyTextbox.TabIndex = 2;
            // 
            // _gpboxFunctionMgr
            // 
            this._gpboxFunctionMgr.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._gpboxFunctionMgr.Controls.Add(this._functionSourceTable);
            this._gpboxFunctionMgr.Controls.Add(this.toolStrip1);
            this._gpboxFunctionMgr.Location = new System.Drawing.Point(0, 421);
            this._gpboxFunctionMgr.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this._gpboxFunctionMgr.Name = "_gpboxFunctionMgr";
            this._gpboxFunctionMgr.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this._gpboxFunctionMgr.Size = new System.Drawing.Size(600, 140);
            this._gpboxFunctionMgr.TabIndex = 12;
            this._gpboxFunctionMgr.TabStop = false;
            this._gpboxFunctionMgr.Text = "Hops Function Sources";
            // 
            // _functionSourceTable
            // 
            this._functionSourceTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._functionSourceTable.BackColor = System.Drawing.Color.Transparent;
            this._functionSourceTable.ColumnCount = 1;
            this._functionSourceTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._functionSourceTable.Location = new System.Drawing.Point(10, 83);
            this._functionSourceTable.Margin = new System.Windows.Forms.Padding(0);
            this._functionSourceTable.Name = "_functionSourceTable";
            this._functionSourceTable.RowCount = 1;
            this._functionSourceTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 1F));
            this._functionSourceTable.Size = new System.Drawing.Size(582, 50);
            this._functionSourceTable.TabIndex = 16;
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._addFunctionSourceButton,
            this._deleteFunctionSourceButton});
            this.toolStrip1.Location = new System.Drawing.Point(6, 30);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(588, 50);
            this.toolStrip1.TabIndex = 15;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // _addFunctionSourceButton
            // 
            this._addFunctionSourceButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this._addFunctionSourceButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._addFunctionSourceButton.Name = "_addFunctionSourceButton";
            this._addFunctionSourceButton.Size = new System.Drawing.Size(141, 44);
            this._addFunctionSourceButton.Text = "Add Source";
            this._addFunctionSourceButton.ToolTipText = "Add a new function source";
            this._addFunctionSourceButton.Click += new System.EventHandler(this._addFunctionSourceButton_Click);
            // 
            // _deleteFunctionSourceButton
            // 
            this._deleteFunctionSourceButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._deleteFunctionSourceButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this._deleteFunctionSourceButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._deleteFunctionSourceButton.Name = "_deleteFunctionSourceButton";
            this._deleteFunctionSourceButton.Size = new System.Drawing.Size(186, 44);
            this._deleteFunctionSourceButton.Text = "Delete Selected";
            this._deleteFunctionSourceButton.ToolTipText = "Delete selected function sources";
            this._deleteFunctionSourceButton.Click += new System.EventHandler(this._deleteFunctionSourceButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 196);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(290, 25);
            this.label3.TabIndex = 14;
            this.label3.Text = "HTTP Request Timeout (sec)";
            // 
            // _httpTimeoutTextbox
            // 
            this._httpTimeoutTextbox.Location = new System.Drawing.Point(306, 190);
            this._httpTimeoutTextbox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._httpTimeoutTextbox.Name = "_httpTimeoutTextbox";
            this._httpTimeoutTextbox.Size = new System.Drawing.Size(116, 31);
            this._httpTimeoutTextbox.TabIndex = 13;
            // 
            // HopsAppSettingsUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label3);
            this.Controls.Add(this._httpTimeoutTextbox);
            this.Controls.Add(this._gpboxFunctionMgr);
            this.Controls.Add(this._apiKeyTextbox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._maxConcurrentRequestsTextbox);
            this.Controls.Add(this._updateChildCountButton);
            this.Controls.Add(this._childComputeCount);
            this.Controls.Add(this._lblCacheCount);
            this.Controls.Add(this._btnClearMemCache);
            this.Controls.Add(this._launchWorkerAtStart);
            this.Controls.Add(this._hideWorkerWindows);
            this.Controls.Add(this._serversTextBox);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "HopsAppSettingsUserControl";
            this.Size = new System.Drawing.Size(600, 562);
            ((System.ComponentModel.ISupportInitialize)(this._childComputeCount)).EndInit();
            this._gpboxFunctionMgr.ResumeLayout(false);
            this._gpboxFunctionMgr.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
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
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox _apiKeyTextbox;
        private System.Windows.Forms.GroupBox _gpboxFunctionMgr;
        public ToolStrip toolStrip1;
        public ToolStripButton _addFunctionSourceButton;
        private ToolStripButton _deleteFunctionSourceButton;
        private TableLayoutPanel _functionSourceTable;
        private Label label3;
        private TextBox _httpTimeoutTextbox;
    }
}
