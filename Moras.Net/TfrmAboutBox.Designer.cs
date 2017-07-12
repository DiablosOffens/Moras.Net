namespace Moras.Net
{
    partial class TfrmAboutBox
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TfrmAboutBox));
            this.lbVersion = new DelphiClasses.TLabel();
            this.Panel1 = new System.Windows.Forms.Panel();
            this.Image1 = new System.Windows.Forms.PictureBox();
            this.Comments = new DelphiClasses.TLabel();
            this.Copyright = new DelphiClasses.TLabel();
            this.OKButton = new System.Windows.Forms.Button();
            this.Panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Image1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // lbVersion
            // 
            this.lbVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbVersion.AutoSize = false;
            this.lbVersion.BackColor = System.Drawing.Color.Transparent;
            this.lbVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World);
            this.lbVersion.ForeColor = System.Drawing.Color.White;
            this.lbVersion.Location = new System.Drawing.Point(506, 16);
            this.lbVersion.Name = "lbVersion";
            this.lbVersion.Size = new System.Drawing.Size(127, 20);
            this.lbVersion.TabIndex = 1;
            this.lbVersion.Text = "Version 1.60 Beta";
            this.lbVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Panel1
            // 
            this.Panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Panel1.BackColor = System.Drawing.Color.Transparent;
            this.Panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Panel1.Controls.Add(this.lbVersion);
            this.Panel1.Controls.Add(this.Image1);
            this.Panel1.Controls.Add(this.Comments);
            this.Panel1.Controls.Add(this.Copyright);
            this.Panel1.Location = new System.Drawing.Point(8, 8);
            this.Panel1.Name = "Panel1";
            this.Panel1.Size = new System.Drawing.Size(649, 399);
            this.Panel1.TabIndex = 0;
            // 
            // Image1
            // 
            this.Image1.Location = new System.Drawing.Point(0, 0);
            this.Image1.Name = "Image1";
            this.Image1.Size = new System.Drawing.Size(649, 325);
            this.Image1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.Image1.TabIndex = 3;
            this.Image1.TabStop = false;
            // 
            // Comments
            // 
            this.Comments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Comments.AutoSize = false;
            this.Comments.BackColor = System.Drawing.Color.Transparent;
            this.Comments.Location = new System.Drawing.Point(8, 356);
            this.Comments.Name = "Comments";
            this.Comments.Size = new System.Drawing.Size(631, 37);
            this.Comments.TabIndex = 2;
            this.Comments.Text = resources.GetString("Comments.Text");
            this.Comments.WordWrap = true;
            // 
            // Copyright
            // 
            this.Copyright.AutoSize = false;
            this.Copyright.BackColor = System.Drawing.Color.Transparent;
            this.Copyright.Location = new System.Drawing.Point(8, 336);
            this.Copyright.Name = "Copyright";
            this.Copyright.Size = new System.Drawing.Size(206, 13);
            this.Copyright.TabIndex = 2;
            this.Copyright.Text = "Copyright 2003-2007 by McKenna, Cinnean";
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(296, 415);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 24);
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // TfrmAboutBox
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.ClientSize = new System.Drawing.Size(666, 446);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.Panel1);
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Location = new System.Drawing.Point(246, 141);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TfrmAboutBox";
            this.OldCreateOrder = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "About";
            this.Panel1.ResumeLayout(false);
            this.Panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Image1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public DelphiClasses.TLabel lbVersion;
        private System.Windows.Forms.Panel Panel1;
        private DelphiClasses.TLabel Copyright;
        private DelphiClasses.TLabel Comments;
        private System.Windows.Forms.PictureBox Image1;
        private System.Windows.Forms.Button OKButton;

    }
}