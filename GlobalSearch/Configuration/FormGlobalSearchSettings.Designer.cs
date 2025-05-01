namespace MediaPortal.Search
{
    partial class FormGlobalSearchSettings
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGlobalSearchSettings));
            this.pctGlobe = new System.Windows.Forms.PictureBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.lblVersion = new System.Windows.Forms.Label();
            this.gsSettings = new System.Windows.Forms.GroupBox();
            this.pluginNameLabel = new System.Windows.Forms.Label();
            this.pluginNameTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.numMaxSearches = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.pctGlobe)).BeginInit();
            this.gsSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxSearches)).BeginInit();
            this.SuspendLayout();
            // 
            // pctGlobe
            // 
            this.pctGlobe.Image = ((System.Drawing.Image)(resources.GetObject("pctGlobe.Image")));
            this.pctGlobe.Location = new System.Drawing.Point(0, 0);
            this.pctGlobe.Name = "pctGlobe";
            this.pctGlobe.Size = new System.Drawing.Size(238, 298);
            this.pctGlobe.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pctGlobe.TabIndex = 0;
            this.pctGlobe.TabStop = false;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(570, 308);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.AllowDrop = true;
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(489, 308);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lblVersion
            // 
            this.lblVersion.BackColor = System.Drawing.Color.Transparent;
            this.lblVersion.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVersion.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.lblVersion.Location = new System.Drawing.Point(546, 10);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(99, 14);
            this.lblVersion.TabIndex = 0;
            this.lblVersion.Text = "vX.X.X.X";
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // gsSettings
            // 
            this.gsSettings.Controls.Add(this.numMaxSearches);
            this.gsSettings.Controls.Add(this.label1);
            this.gsSettings.Controls.Add(this.pluginNameLabel);
            this.gsSettings.Controls.Add(this.pluginNameTextBox);
            this.gsSettings.Location = new System.Drawing.Point(251, 52);
            this.gsSettings.Name = "gsSettings";
            this.gsSettings.Size = new System.Drawing.Size(394, 100);
            this.gsSettings.TabIndex = 1;
            this.gsSettings.TabStop = false;
            this.gsSettings.Text = "Plugin Settings";
            // 
            // pluginNameLabel
            // 
            this.pluginNameLabel.AutoSize = true;
            this.pluginNameLabel.Location = new System.Drawing.Point(33, 23);
            this.pluginNameLabel.Name = "pluginNameLabel";
            this.pluginNameLabel.Size = new System.Drawing.Size(78, 14);
            this.pluginNameLabel.TabIndex = 0;
            this.pluginNameLabel.Text = "&Name on Menu";
            this.pluginNameLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // pluginNameTextBox
            // 
            this.pluginNameTextBox.Location = new System.Drawing.Point(129, 20);
            this.pluginNameTextBox.Name = "pluginNameTextBox";
            this.pluginNameTextBox.Size = new System.Drawing.Size(153, 20);
            this.pluginNameTextBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 54);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 14);
            this.label1.TabIndex = 2;
            this.label1.Text = "&Saved Searches";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // numMaxSearches
            // 
            this.numMaxSearches.Location = new System.Drawing.Point(129, 52);
            this.numMaxSearches.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numMaxSearches.Name = "numMaxSearches";
            this.numMaxSearches.Size = new System.Drawing.Size(53, 20);
            this.numMaxSearches.TabIndex = 3;
            this.numMaxSearches.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // FormGlobalSearchSettings
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(659, 343);
            this.Controls.Add(this.gsSettings);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.pctGlobe);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormGlobalSearchSettings";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MediaPortal - Global Music Search";
            this.Load += new System.EventHandler(this.FormGlobalSearchSettings_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pctGlobe)).EndInit();
            this.gsSettings.ResumeLayout(false);
            this.gsSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxSearches)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pctGlobe;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.GroupBox gsSettings;
        private System.Windows.Forms.Label pluginNameLabel;
        private System.Windows.Forms.TextBox pluginNameTextBox;
        private System.Windows.Forms.NumericUpDown numMaxSearches;
        private System.Windows.Forms.Label label1;
    }
}