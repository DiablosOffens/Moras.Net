namespace Moras.Net
{
    partial class TfrmSplash
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
            this.Image1 = new System.Windows.Forms.PictureBox();
            this.lbInfo = new DelphiClasses.TLabel();
            this.lbVersion = new DelphiClasses.TLabel();
            this.lbXmlVersion = new DelphiClasses.TLabel();
            this.pbLoad = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.Image1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // Image1
            // 
            this.Image1.Location = new System.Drawing.Point(0, 0);
            this.Image1.Name = "Image1";
            this.Image1.Size = new System.Drawing.Size(649, 324);
            this.Image1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.Image1.TabIndex = 0;
            this.Image1.TabStop = false;
            // 
            // lbInfo
            // 
            this.lbInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbInfo.AutoSize = false;
            this.lbInfo.BackColor = System.Drawing.Color.Transparent;
            this.lbInfo.ForeColor = System.Drawing.Color.White;
            this.lbInfo.Location = new System.Drawing.Point(24, 276);
            this.lbInfo.Name = "lbInfo";
            this.lbInfo.Size = new System.Drawing.Size(465, 13);
            this.lbInfo.TabIndex = 1;
            this.lbInfo.Text = "Initialisiere Interface...";
            // 
            // lbVersion
            // 
            this.lbVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbVersion.AutoSize = false;
            this.lbVersion.BackColor = System.Drawing.Color.Transparent;
            this.lbVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World);
            this.lbVersion.ForeColor = System.Drawing.Color.White;
            this.lbVersion.Location = new System.Drawing.Point(500, 12);
            this.lbVersion.Name = "lbVersion";
            this.lbVersion.Size = new System.Drawing.Size(137, 25);
            this.lbVersion.TabIndex = 2;
            this.lbVersion.Text = "Version";
            this.lbVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbXmlVersion
            // 
            this.lbXmlVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbXmlVersion.AutoSize = false;
            this.lbXmlVersion.BackColor = System.Drawing.Color.Transparent;
            this.lbXmlVersion.ForeColor = System.Drawing.Color.White;
            this.lbXmlVersion.Location = new System.Drawing.Point(496, 272);
            this.lbXmlVersion.Name = "lbXmlVersion";
            this.lbXmlVersion.Size = new System.Drawing.Size(129, 17);
            this.lbXmlVersion.TabIndex = 3;
            this.lbXmlVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lbXmlVersion.Visible = false;
            // 
            // pbLoad
            // 
            this.pbLoad.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbLoad.Location = new System.Drawing.Point(24, 292);
            this.pbLoad.Name = "pbLoad";
            this.pbLoad.Size = new System.Drawing.Size(601, 17);
            this.pbLoad.Step = 15;
            this.pbLoad.TabIndex = 0;
            this.pbLoad.Value = 100;
            // 
            // TfrmSplash
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.ClientSize = new System.Drawing.Size(649, 324);
            this.Controls.Add(this.pbLoad);
            this.Controls.Add(this.lbXmlVersion);
            this.Controls.Add(this.lbVersion);
            this.Controls.Add(this.lbInfo);
            this.Controls.Add(this.Image1);
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Location = new System.Drawing.Point(251, 196);
            this.Name = "TfrmSplash";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frmSplash";
            ((System.ComponentModel.ISupportInitialize)(this.Image1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.ProgressBar pbLoad;
        public System.Windows.Forms.PictureBox Image1;
        public DelphiClasses.TLabel lbVersion;
        public DelphiClasses.TLabel lbXmlVersion;
        public DelphiClasses.TLabel lbInfo;
    }
}