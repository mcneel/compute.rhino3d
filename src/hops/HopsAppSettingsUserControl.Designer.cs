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
            this._gpboxComputeServer = new System.Windows.Forms.GroupBox();
            this._rdoUseLocal = new System.Windows.Forms.CheckBox();
            this._advancedServersButton = new System.Windows.Forms.PictureBox();
            this._hideWorkerWindows = new System.Windows.Forms.CheckBox();
            this._launchWorkerAtStart = new System.Windows.Forms.CheckBox();
            this._childComputeCount = new System.Windows.Forms.NumericUpDown();
            this._updateChildCountButton = new System.Windows.Forms.Button();
            this._rdoUseRemote = new System.Windows.Forms.CheckBox();
            this._labelServerUrl = new System.Windows.Forms.Label();
            this._serverUrlTextbox = new System.Windows.Forms.TextBox();
            this._serverStatusDot = new System.Windows.Forms.PictureBox();
            this._btnClearMemCache = new System.Windows.Forms.Button();
            this._lblCacheCount = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this._maxConcurrentRequestsTextbox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this._apiKeyTextbox = new System.Windows.Forms.TextBox();
            this._gpboxFunctionMgr = new System.Windows.Forms.GroupBox();
            this._manageFunctionSourcesButton = new System.Windows.Forms.Button();
            this._functionSourceCountLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this._httpTimeoutTextbox = new System.Windows.Forms.TextBox();
            this._gpboxComputeServer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._childComputeCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._serverStatusDot)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._advancedServersButton)).BeginInit();
            this._gpboxFunctionMgr.SuspendLayout();
            this.SuspendLayout();
            //
            // _gpboxComputeServer
            //
            this._gpboxComputeServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._gpboxComputeServer.Controls.Add(this._rdoUseLocal);
            this._gpboxComputeServer.Controls.Add(this._advancedServersButton);
            this._gpboxComputeServer.Controls.Add(this._hideWorkerWindows);
            this._gpboxComputeServer.Controls.Add(this._launchWorkerAtStart);
            this._gpboxComputeServer.Controls.Add(this._childComputeCount);
            this._gpboxComputeServer.Controls.Add(this._updateChildCountButton);
            this._gpboxComputeServer.Controls.Add(this._rdoUseRemote);
            this._gpboxComputeServer.Controls.Add(this._labelServerUrl);
            this._gpboxComputeServer.Controls.Add(this._serverUrlTextbox);
            this._gpboxComputeServer.Controls.Add(this._serverStatusDot);
            this._gpboxComputeServer.Location = new System.Drawing.Point(2, 2);
            this._gpboxComputeServer.Name = "_gpboxComputeServer";
            this._gpboxComputeServer.Size = new System.Drawing.Size(296, 160);
            this._gpboxComputeServer.TabIndex = 0;
            this._gpboxComputeServer.TabStop = false;
            this._gpboxComputeServer.Text = "Compute server source";
            //
            // _rdoUseLocal
            //
            this._rdoUseLocal.AutoSize = true;
            this._rdoUseLocal.Location = new System.Drawing.Point(8, 18);
            this._rdoUseLocal.Name = "_rdoUseLocal";
            this._rdoUseLocal.Size = new System.Drawing.Size(220, 17);
            this._rdoUseLocal.TabIndex = 0;
            this._rdoUseLocal.Text = "Use local rhino.compute instance";
            this._rdoUseLocal.UseVisualStyleBackColor = true;
            //
            // _advancedServersButton
            //
            this._advancedServersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._advancedServersButton.BackColor = System.Drawing.Color.Transparent;
            this._advancedServersButton.Location = new System.Drawing.Point(267, 13);
            this._advancedServersButton.Name = "_advancedServersButton";
            this._advancedServersButton.Size = new System.Drawing.Size(26, 26);
            this._advancedServersButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._advancedServersButton.TabIndex = 1;
            this._advancedServersButton.TabStop = false;
            //
            // _hideWorkerWindows
            //
            this._hideWorkerWindows.AutoSize = true;
            this._hideWorkerWindows.Location = new System.Drawing.Point(28, 41);
            this._hideWorkerWindows.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this._hideWorkerWindows.Name = "_hideWorkerWindows";
            this._hideWorkerWindows.Size = new System.Drawing.Size(207, 17);
            this._hideWorkerWindows.TabIndex = 2;
            this._hideWorkerWindows.Text = "Hide rhino.compute console window";
            this._hideWorkerWindows.UseVisualStyleBackColor = true;
            //
            // _launchWorkerAtStart
            //
            this._launchWorkerAtStart.AutoSize = true;
            this._launchWorkerAtStart.Location = new System.Drawing.Point(28, 62);
            this._launchWorkerAtStart.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this._launchWorkerAtStart.Name = "_launchWorkerAtStart";
            this._launchWorkerAtStart.Size = new System.Drawing.Size(204, 17);
            this._launchWorkerAtStart.TabIndex = 3;
            this._launchWorkerAtStart.Text = "Launch local rhino.compute at start";
            this._launchWorkerAtStart.UseVisualStyleBackColor = true;
            //
            // _childComputeCount
            //
            this._childComputeCount.Location = new System.Drawing.Point(28, 84);
            this._childComputeCount.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this._childComputeCount.Name = "_childComputeCount";
            this._childComputeCount.Size = new System.Drawing.Size(50, 22);
            this._childComputeCount.TabIndex = 4;
            //
            // _updateChildCountButton
            //
            this._updateChildCountButton.Location = new System.Drawing.Point(84, 84);
            this._updateChildCountButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this._updateChildCountButton.Name = "_updateChildCountButton";
            this._updateChildCountButton.Size = new System.Drawing.Size(138, 22);
            this._updateChildCountButton.TabIndex = 5;
            this._updateChildCountButton.Text = "Child process count";
            this._updateChildCountButton.UseVisualStyleBackColor = true;
            //
            // _rdoUseRemote
            //
            this._rdoUseRemote.AutoSize = true;
            this._rdoUseRemote.Location = new System.Drawing.Point(8, 112);
            this._rdoUseRemote.Name = "_rdoUseRemote";
            this._rdoUseRemote.Size = new System.Drawing.Size(130, 17);
            this._rdoUseRemote.TabIndex = 6;
            this._rdoUseRemote.Text = "Use remote server";
            this._rdoUseRemote.UseVisualStyleBackColor = true;
            //
            // _labelServerUrl
            //
            this._labelServerUrl.Location = new System.Drawing.Point(28, 134);
            this._labelServerUrl.Name = "_labelServerUrl";
            this._labelServerUrl.Size = new System.Drawing.Size(28, 22);
            this._labelServerUrl.TabIndex = 7;
            this._labelServerUrl.Text = "URL:";
            this._labelServerUrl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _serverUrlTextbox
            //
            this._serverUrlTextbox.AcceptsReturn = false;
            this._serverUrlTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._serverUrlTextbox.Location = new System.Drawing.Point(70, 134);
            this._serverUrlTextbox.Multiline = true;
            this._serverUrlTextbox.Name = "_serverUrlTextbox";
            this._serverUrlTextbox.Size = new System.Drawing.Size(224, 22);
            this._serverUrlTextbox.TabIndex = 8;
            this._serverUrlTextbox.WordWrap = false;
            //
            // _serverStatusDot
            //
            this._serverStatusDot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this._serverStatusDot.BackColor = System.Drawing.Color.Transparent;
            this._serverStatusDot.Location = new System.Drawing.Point(57, 140);
            this._serverStatusDot.Name = "_serverStatusDot";
            this._serverStatusDot.Size = new System.Drawing.Size(10, 10);
            this._serverStatusDot.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._serverStatusDot.TabIndex = 10;
            this._serverStatusDot.TabStop = false;
            //
            // _btnClearMemCache
            //
            this._btnClearMemCache.Location = new System.Drawing.Point(1, 236);
            this._btnClearMemCache.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this._btnClearMemCache.Name = "_btnClearMemCache";
            this._btnClearMemCache.Size = new System.Drawing.Size(171, 22);
            this._btnClearMemCache.TabIndex = 16;
            this._btnClearMemCache.Text = "Clear Hops memory cache";
            this._btnClearMemCache.UseVisualStyleBackColor = true;
            //
            // _lblCacheCount
            //
            this._lblCacheCount.AutoSize = true;
            this._lblCacheCount.Location = new System.Drawing.Point(178, 241);
            this._lblCacheCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this._lblCacheCount.Name = "_lblCacheCount";
            this._lblCacheCount.Size = new System.Drawing.Size(126, 13);
            this._lblCacheCount.TabIndex = 17;
            this._lblCacheCount.Text = "(10000 items in cache)";
            //
            // _maxConcurrentRequestsTextbox
            //
            this._maxConcurrentRequestsTextbox.AcceptsReturn = false;
            this._maxConcurrentRequestsTextbox.Location = new System.Drawing.Point(128, 188);
            this._maxConcurrentRequestsTextbox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this._maxConcurrentRequestsTextbox.Multiline = true;
            this._maxConcurrentRequestsTextbox.Name = "_maxConcurrentRequestsTextbox";
            this._maxConcurrentRequestsTextbox.Size = new System.Drawing.Size(45, 22);
            this._maxConcurrentRequestsTextbox.TabIndex = 13;
            this._maxConcurrentRequestsTextbox.WordWrap = false;
            //
            // label1
            //
            this.label1.Location = new System.Drawing.Point(2, 188);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(126, 22);
            this.label1.TabIndex = 14;
            this.label1.Text = "Max concurrent requests";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // label2
            //
            this.label2.Location = new System.Drawing.Point(2, 165);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 22);
            this.label2.TabIndex = 11;
            this.label2.Text = "API key";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _apiKeyTextbox
            //
            this._apiKeyTextbox.AcceptsReturn = false;
            this._apiKeyTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._apiKeyTextbox.Location = new System.Drawing.Point(45, 165);
            this._apiKeyTextbox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this._apiKeyTextbox.Multiline = true;
            this._apiKeyTextbox.Name = "_apiKeyTextbox";
            this._apiKeyTextbox.Size = new System.Drawing.Size(252, 22);
            this._apiKeyTextbox.TabIndex = 12;
            this._apiKeyTextbox.WordWrap = false;
            //
            // _gpboxFunctionMgr
            //
            this._gpboxFunctionMgr.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._gpboxFunctionMgr.Controls.Add(this._manageFunctionSourcesButton);
            this._gpboxFunctionMgr.Controls.Add(this._functionSourceCountLabel);
            this._gpboxFunctionMgr.Location = new System.Drawing.Point(0, 264);
            this._gpboxFunctionMgr.Name = "_gpboxFunctionMgr";
            this._gpboxFunctionMgr.Size = new System.Drawing.Size(300, 50);
            this._gpboxFunctionMgr.TabIndex = 18;
            this._gpboxFunctionMgr.TabStop = false;
            this._gpboxFunctionMgr.Text = "Hops function sources";
            //
            // _manageFunctionSourcesButton
            //
            this._manageFunctionSourcesButton.Location = new System.Drawing.Point(6, 19);
            this._manageFunctionSourcesButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this._manageFunctionSourcesButton.Name = "_manageFunctionSourcesButton";
            this._manageFunctionSourcesButton.Size = new System.Drawing.Size(120, 22);
            this._manageFunctionSourcesButton.TabIndex = 0;
            this._manageFunctionSourcesButton.Text = "Manage sources...";
            this._manageFunctionSourcesButton.UseVisualStyleBackColor = true;
            //
            // _functionSourceCountLabel
            //
            this._functionSourceCountLabel.AutoSize = true;
            this._functionSourceCountLabel.Location = new System.Drawing.Point(132, 24);
            this._functionSourceCountLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this._functionSourceCountLabel.Name = "_functionSourceCountLabel";
            this._functionSourceCountLabel.Size = new System.Drawing.Size(105, 13);
            this._functionSourceCountLabel.TabIndex = 1;
            this._functionSourceCountLabel.Text = "(0 sources configured)";
            //
            // label3
            //
            this.label3.Location = new System.Drawing.Point(2, 211);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 22);
            this.label3.TabIndex = 15;
            this.label3.Text = "Timeout (sec)";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _httpTimeoutTextbox
            //
            this._httpTimeoutTextbox.AcceptsReturn = false;
            this._httpTimeoutTextbox.Location = new System.Drawing.Point(76, 211);
            this._httpTimeoutTextbox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this._httpTimeoutTextbox.Multiline = true;
            this._httpTimeoutTextbox.Name = "_httpTimeoutTextbox";
            this._httpTimeoutTextbox.Size = new System.Drawing.Size(97, 22);
            this._httpTimeoutTextbox.TabIndex = 14;
            this._httpTimeoutTextbox.WordWrap = false;
            //
            // HopsAppSettingsUserControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._gpboxComputeServer);
            this.Controls.Add(this.label3);
            this.Controls.Add(this._httpTimeoutTextbox);
            this.Controls.Add(this._gpboxFunctionMgr);
            this.Controls.Add(this._apiKeyTextbox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._maxConcurrentRequestsTextbox);
            this.Controls.Add(this._lblCacheCount);
            this.Controls.Add(this._btnClearMemCache);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "HopsAppSettingsUserControl";
            this.Size = new System.Drawing.Size(300, 317);
            this._gpboxComputeServer.ResumeLayout(false);
            this._gpboxComputeServer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._childComputeCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._serverStatusDot)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._advancedServersButton)).EndInit();
            this._gpboxFunctionMgr.ResumeLayout(false);
            this._gpboxFunctionMgr.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox _gpboxComputeServer;
        private System.Windows.Forms.CheckBox _rdoUseLocal;
        private System.Windows.Forms.CheckBox _rdoUseRemote;
        private System.Windows.Forms.Label _labelServerUrl;
        private System.Windows.Forms.TextBox _serverUrlTextbox;
        private System.Windows.Forms.PictureBox _serverStatusDot;
        private System.Windows.Forms.PictureBox _advancedServersButton;
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
        private System.Windows.Forms.Button _manageFunctionSourcesButton;
        private System.Windows.Forms.Label _functionSourceCountLabel;
        private Label label3;
        private TextBox _httpTimeoutTextbox;
    }
}
