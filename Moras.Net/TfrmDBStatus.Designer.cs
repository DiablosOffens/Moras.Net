namespace Moras.Net
{
    partial class TfrmDBStatus
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TfrmDBStatus));
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.Label5 = new DelphiClasses.TLabel();
            this.Label4 = new DelphiClasses.TLabel();
            this.Label3 = new DelphiClasses.TLabel();
            this.Label2 = new DelphiClasses.TLabel();
            this.Label1 = new DelphiClasses.TLabel();
            this.btClose = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.lbAlbion = new DelphiClasses.TLabel();
            this.lbHibernia = new DelphiClasses.TLabel();
            this.lbMidgard = new DelphiClasses.TLabel();
            this.lbArtifacts = new DelphiClasses.TLabel();
            this.lbSum = new DelphiClasses.TLabel();
            this.GroupBox2 = new System.Windows.Forms.GroupBox();
            this.Label6 = new DelphiClasses.TLabel();
            this.Label7 = new DelphiClasses.TLabel();
            this.lbDBVersion = new DelphiClasses.TLabel();
            this.lbDBSize = new DelphiClasses.TLabel();
            this.Label8 = new DelphiClasses.TLabel();
            this.lbSysLang = new DelphiClasses.TLabel();
            this.btVacuum = new System.Windows.Forms.Button();
            this.ZQuery = new System.Data.SQLite.SQLiteCommand();
            this.GroupBox1.SuspendLayout();
            this.GroupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // GroupBox1
            // 
            this.GroupBox1.Controls.Add(this.lbSum);
            this.GroupBox1.Controls.Add(this.lbArtifacts);
            this.GroupBox1.Controls.Add(this.lbMidgard);
            this.GroupBox1.Controls.Add(this.lbHibernia);
            this.GroupBox1.Controls.Add(this.lbAlbion);
            this.GroupBox1.Controls.Add(this.Label5);
            this.GroupBox1.Controls.Add(this.Label4);
            this.GroupBox1.Controls.Add(this.Label3);
            this.GroupBox1.Controls.Add(this.Label2);
            this.GroupBox1.Controls.Add(this.Label1);
            this.GroupBox1.Location = new System.Drawing.Point(12, 8);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Size = new System.Drawing.Size(157, 149);
            this.GroupBox1.TabIndex = 1;
            this.GroupBox1.TabStop = false;
            this.GroupBox1.Text = "Items";
            // 
            // Label5
            // 
            this.Label5.AutoSize = false;
            this.Label5.BackColor = System.Drawing.SystemColors.Control;
            this.Label5.Location = new System.Drawing.Point(16, 120);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(49, 17);
            this.Label5.TabIndex = 0;
            this.Label5.Text = "Gesamt:";
            this.Label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.toolTip1.SetToolTip(this.Label5, "Gesamtzahl der gespeicherten Gegenstände");
            this.Label5.Transparent = false;
            // 
            // Label4
            // 
            this.Label4.AutoSize = false;
            this.Label4.BackColor = System.Drawing.SystemColors.Control;
            this.Label4.Location = new System.Drawing.Point(16, 96);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(49, 17);
            this.Label4.TabIndex = 0;
            this.Label4.Text = "Artefakte:";
            this.Label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.toolTip1.SetToolTip(this.Label4, "Alle Artefakte");
            this.Label4.Transparent = false;
            // 
            // Label3
            // 
            this.Label3.AutoSize = false;
            this.Label3.BackColor = System.Drawing.SystemColors.Control;
            this.Label3.Location = new System.Drawing.Point(16, 72);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(49, 17);
            this.Label3.TabIndex = 0;
            this.Label3.Text = "Midgard:";
            this.Label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.toolTip1.SetToolTip(this.Label3, "Summe aller in Midgard nutzbaren Gegenstände, inklusive der Artefakte");
            this.Label3.Transparent = false;
            // 
            // Label2
            // 
            this.Label2.AutoSize = false;
            this.Label2.BackColor = System.Drawing.SystemColors.Control;
            this.Label2.Location = new System.Drawing.Point(16, 48);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(49, 17);
            this.Label2.TabIndex = 0;
            this.Label2.Text = "Hibernia:";
            this.Label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.toolTip1.SetToolTip(this.Label2, "Summe aller in Hibern nutzbaren Gegenstände, inklusive der Artefakte");
            this.Label2.Transparent = false;
            // 
            // Label1
            // 
            this.Label1.AutoSize = false;
            this.Label1.BackColor = System.Drawing.SystemColors.Control;
            this.Label1.Location = new System.Drawing.Point(16, 24);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(49, 17);
            this.Label1.TabIndex = 0;
            this.Label1.Text = "Albion:";
            this.Label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.toolTip1.SetToolTip(this.Label1, "Summe aller in Albion nutzbaren Gegenstände, inklusive der Artefakte");
            this.Label1.Transparent = false;
            // 
            // btClose
            // 
            this.btClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btClose.Image = ((System.Drawing.Image)(resources.GetObject("btClose.Image")));
            this.btClose.Location = new System.Drawing.Point(177, 164);
            this.btClose.Name = "btClose";
            this.btClose.Size = new System.Drawing.Size(88, 25);
            this.btClose.TabIndex = 0;
            this.btClose.Text = "&Schliessen";
            this.btClose.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btClose.UseVisualStyleBackColor = true;
            // 
            // lbAlbion
            // 
            this.lbAlbion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbAlbion.AutoSize = false;
            this.lbAlbion.Location = new System.Drawing.Point(72, 24);
            this.lbAlbion.Name = "lbAlbion";
            this.lbAlbion.Size = new System.Drawing.Size(77, 17);
            this.lbAlbion.TabIndex = 1;
            this.lbAlbion.Text = "|";
            this.lbAlbion.Transparent = false;
            // 
            // lbHibernia
            // 
            this.lbHibernia.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbHibernia.AutoSize = false;
            this.lbHibernia.Location = new System.Drawing.Point(72, 48);
            this.lbHibernia.Name = "lbHibernia";
            this.lbHibernia.Size = new System.Drawing.Size(77, 17);
            this.lbHibernia.TabIndex = 1;
            this.lbHibernia.Text = "|";
            this.lbHibernia.Transparent = false;
            // 
            // lbMidgard
            // 
            this.lbMidgard.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbMidgard.AutoSize = false;
            this.lbMidgard.Location = new System.Drawing.Point(72, 72);
            this.lbMidgard.Name = "lbMidgard";
            this.lbMidgard.Size = new System.Drawing.Size(77, 17);
            this.lbMidgard.TabIndex = 1;
            this.lbMidgard.Text = "|";
            this.lbMidgard.Transparent = false;
            // 
            // lbArtifacts
            // 
            this.lbArtifacts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbArtifacts.AutoSize = false;
            this.lbArtifacts.Location = new System.Drawing.Point(72, 96);
            this.lbArtifacts.Name = "lbArtifacts";
            this.lbArtifacts.Size = new System.Drawing.Size(77, 17);
            this.lbArtifacts.TabIndex = 1;
            this.lbArtifacts.Text = "|";
            this.lbArtifacts.Transparent = false;
            // 
            // lbSum
            // 
            this.lbSum.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbSum.AutoSize = false;
            this.lbSum.Location = new System.Drawing.Point(72, 120);
            this.lbSum.Name = "lbSum";
            this.lbSum.Size = new System.Drawing.Size(77, 17);
            this.lbSum.TabIndex = 1;
            this.lbSum.Text = "|";
            this.lbSum.Transparent = false;
            // 
            // GroupBox2
            // 
            this.GroupBox2.Controls.Add(this.Label8);
            this.GroupBox2.Controls.Add(this.Label7);
            this.GroupBox2.Controls.Add(this.Label6);
            this.GroupBox2.Controls.Add(this.lbSysLang);
            this.GroupBox2.Controls.Add(this.lbDBSize);
            this.GroupBox2.Controls.Add(this.lbDBVersion);
            this.GroupBox2.Location = new System.Drawing.Point(180, 8);
            this.GroupBox2.Name = "GroupBox2";
            this.GroupBox2.Size = new System.Drawing.Size(157, 149);
            this.GroupBox2.TabIndex = 2;
            this.GroupBox2.TabStop = false;
            this.GroupBox2.Text = "Datenbank";
            // 
            // Label6
            // 
            this.Label6.AutoSize = false;
            this.Label6.Location = new System.Drawing.Point(12, 20);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(38, 13);
            this.Label6.TabIndex = 0;
            this.Label6.Text = "Version:";
            this.Label6.Transparent = false;
            // 
            // Label7
            // 
            this.Label7.AutoSize = false;
            this.Label7.Location = new System.Drawing.Point(12, 44);
            this.Label7.Name = "Label7";
            this.Label7.Size = new System.Drawing.Size(36, 13);
            this.Label7.TabIndex = 0;
            this.Label7.Text = "Grösse:";
            this.Label7.Transparent = false;
            // 
            // lbDBVersion
            // 
            this.lbDBVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbDBVersion.AutoSize = false;
            this.lbDBVersion.Location = new System.Drawing.Point(60, 20);
            this.lbDBVersion.Name = "lbDBVersion";
            this.lbDBVersion.Size = new System.Drawing.Size(85, 13);
            this.lbDBVersion.TabIndex = 1;
            this.lbDBVersion.Text = "|";
            this.lbDBVersion.Transparent = false;
            // 
            // lbDBSize
            // 
            this.lbDBSize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbDBSize.AutoSize = false;
            this.lbDBSize.Location = new System.Drawing.Point(60, 44);
            this.lbDBSize.Name = "lbDBSize";
            this.lbDBSize.Size = new System.Drawing.Size(85, 13);
            this.lbDBSize.TabIndex = 1;
            this.lbDBSize.Text = "|";
            this.lbDBSize.Transparent = false;
            // 
            // Label8
            // 
            this.Label8.AutoSize = false;
            this.Label8.Location = new System.Drawing.Point(12, 68);
            this.Label8.Name = "Label8";
            this.Label8.Size = new System.Drawing.Size(43, 13);
            this.Label8.TabIndex = 0;
            this.Label8.Text = "Sprache:";
            this.Label8.Transparent = false;
            // 
            // lbSysLang
            // 
            this.lbSysLang.AutoSize = false;
            this.lbSysLang.Location = new System.Drawing.Point(60, 68);
            this.lbSysLang.Name = "lbSysLang";
            this.lbSysLang.Size = new System.Drawing.Size(85, 13);
            this.lbSysLang.TabIndex = 1;
            this.lbSysLang.Text = "|";
            this.lbSysLang.Transparent = false;
            // 
            // btVacuum
            // 
            this.btVacuum.Image = ((System.Drawing.Image)(resources.GetObject("btVacuum.Image")));
            this.btVacuum.Location = new System.Drawing.Point(84, 164);
            this.btVacuum.Name = "btVacuum";
            this.btVacuum.Size = new System.Drawing.Size(87, 25);
            this.btVacuum.TabIndex = 3;
            this.btVacuum.Text = "Optimieren";
            this.btVacuum.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btVacuum.UseVisualStyleBackColor = true;
            this.btVacuum.Click += new System.EventHandler(this.btVacuumClick);
            // 
            // ZQuery
            // 
            this.ZQuery.CommandText = null;
            // 
            // TfrmDBStatus
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.ClientSize = new System.Drawing.Size(348, 199);
            this.Controls.Add(this.btVacuum);
            this.Controls.Add(this.GroupBox2);
            this.Controls.Add(this.GroupBox1);
            this.Controls.Add(this.btClose);
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Location = new System.Drawing.Point(344, 198);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TfrmDBStatus";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Status der Datenbank";
            this.TopMost = true;
            this.FormCreate += new System.EventHandler(this.TfrmDBStatus_FormCreate);
            this.GroupBox1.ResumeLayout(false);
            this.GroupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btClose;
        private System.Windows.Forms.GroupBox GroupBox1;
        private DelphiClasses.TLabel Label1;
        private System.Windows.Forms.ToolTip toolTip1;
        private DelphiClasses.TLabel Label2;
        private DelphiClasses.TLabel Label3;
        private DelphiClasses.TLabel Label4;
        private DelphiClasses.TLabel Label5;
        private DelphiClasses.TLabel lbAlbion;
        private DelphiClasses.TLabel lbHibernia;
        private DelphiClasses.TLabel lbMidgard;
        private DelphiClasses.TLabel lbArtifacts;
        private DelphiClasses.TLabel lbSum;
        private System.Windows.Forms.GroupBox GroupBox2;
        private DelphiClasses.TLabel Label6;
        private DelphiClasses.TLabel Label7;
        private DelphiClasses.TLabel lbDBVersion;
        private DelphiClasses.TLabel lbDBSize;
        private DelphiClasses.TLabel Label8;
        private DelphiClasses.TLabel lbSysLang;
        private System.Windows.Forms.Button btVacuum;
        private System.Data.SQLite.SQLiteCommand ZQuery;
    }
}