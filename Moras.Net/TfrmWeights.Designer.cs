namespace Moras.Net
{
    partial class TfrmWeights
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Wichtungsgruppen");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TfrmWeights));
            this.Label1 = new DelphiClasses.TLabel();
            this.Label2 = new DelphiClasses.TLabel();
            this.Label3 = new DelphiClasses.TLabel();
            this.btPlayerFilter = new DelphiClasses.TSpeedButton();
            this.btNullFilter = new DelphiClasses.TSpeedButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pmTrackbar = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.AktuelleSeiteaufdiesenWertsetzen1 = new System.Windows.Forms.ToolStripMenuItem();
            this.AlleSeitenaufdiesenWertsetzen1 = new System.Windows.Forms.ToolStripMenuItem();
            this.SbWeights = new System.Windows.Forms.Panel();
            this.pmWeights = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnAll0 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnAll100 = new DelphiClasses.TMainMenu.TTBXItem(this.components);
            this.TvWeights = new System.Windows.Forms.TreeView();
            this.btOk = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.btAllZero = new System.Windows.Forms.Button();
            this.btAll100 = new System.Windows.Forms.Button();
            this.CbVorlage = new DelphiClasses.TComboBox();
            this.btTemplateSave = new System.Windows.Forms.Button();
            this.btTemplateDelete = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.pmTrackbar.SuspendLayout();
            this.pmWeights.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // Label1
            // 
            this.Label1.AutoSize = false;
            this.Label1.BackColor = System.Drawing.SystemColors.Control;
            this.Label1.Location = new System.Drawing.Point(8, 8);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(95, 13);
            this.Label1.TabIndex = 0;
            this.Label1.Text = "Wichtungsvorlagen:";
            this.Label1.Transparent = false;
            // 
            // Label2
            // 
            this.Label2.AutoSize = false;
            this.Label2.BackColor = System.Drawing.SystemColors.Control;
            this.Label2.Location = new System.Drawing.Point(8, 52);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(93, 13);
            this.Label2.TabIndex = 0;
            this.Label2.Text = "Wichtungsgruppen:";
            this.Label2.Transparent = false;
            // 
            // Label3
            // 
            this.Label3.AutoSize = false;
            this.Label3.BackColor = System.Drawing.SystemColors.Control;
            this.Label3.Location = new System.Drawing.Point(227, 52);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(61, 13);
            this.Label3.TabIndex = 0;
            this.Label3.Text = "Wichtungen:";
            this.Label3.Transparent = false;
            // 
            // btPlayerFilter
            // 
            this.btPlayerFilter.AllowAllUp = true;
            this.btPlayerFilter.Appearance = System.Windows.Forms.Appearance.Button;
            this.btPlayerFilter.Location = new System.Drawing.Point(640, 21);
            this.btPlayerFilter.Name = "btPlayerFilter";
            this.btPlayerFilter.Size = new System.Drawing.Size(75, 25);
            this.btPlayerFilter.TabIndex = 0;
            this.btPlayerFilter.Text = "Player-Filter";
            this.btPlayerFilter.UseVisualStyleBackColor = true;
            this.btPlayerFilter.Click += new System.EventHandler(this.btPlayerFilterClick);
            // 
            // btNullFilter
            // 
            this.btNullFilter.AllowAllUp = true;
            this.btNullFilter.Appearance = System.Windows.Forms.Appearance.Button;
            this.btNullFilter.Location = new System.Drawing.Point(0, 0);
            this.btNullFilter.Margin = new System.Windows.Forms.Padding(0);
            this.btNullFilter.Name = "btNullFilter";
            this.btNullFilter.Size = new System.Drawing.Size(75, 25);
            this.btNullFilter.TabIndex = 0;
            this.btNullFilter.Text = "0-Filter";
            this.btNullFilter.UseVisualStyleBackColor = true;
            this.btNullFilter.Click += new System.EventHandler(this.btNullFilterClick);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btNullFilter);
            this.panel1.Location = new System.Drawing.Point(559, 21);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(75, 25);
            this.panel1.TabIndex = 0;
            // 
            // pmTrackbar
            // 
            this.pmTrackbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AktuelleSeiteaufdiesenWertsetzen1,
            this.AlleSeitenaufdiesenWertsetzen1});
            this.pmTrackbar.Name = "contextMenuStrip1";
            this.pmTrackbar.Size = new System.Drawing.Size(267, 48);
            // 
            // AktuelleSeiteaufdiesenWertsetzen1
            // 
            this.AktuelleSeiteaufdiesenWertsetzen1.Name = "AktuelleSeiteaufdiesenWertsetzen1";
            this.AktuelleSeiteaufdiesenWertsetzen1.Size = new System.Drawing.Size(266, 22);
            this.AktuelleSeiteaufdiesenWertsetzen1.Text = "Aktuelle Seite auf diesen Wert setzen";
            this.AktuelleSeiteaufdiesenWertsetzen1.Click += new System.EventHandler(this.AktuelleSeiteaufdiesenWertsetzen1Click);
            // 
            // AlleSeitenaufdiesenWertsetzen1
            // 
            this.AlleSeitenaufdiesenWertsetzen1.Name = "AlleSeitenaufdiesenWertsetzen1";
            this.AlleSeitenaufdiesenWertsetzen1.Size = new System.Drawing.Size(266, 22);
            this.AlleSeitenaufdiesenWertsetzen1.Text = "Alle Seiten auf diesen Wert setzen";
            this.AlleSeitenaufdiesenWertsetzen1.Click += new System.EventHandler(this.AlleSeitenaufdiesenWertsetzen1Click);
            // 
            // SbWeights
            // 
            this.SbWeights.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SbWeights.AutoScroll = true;
            this.SbWeights.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.SbWeights.ContextMenuStrip = this.pmWeights;
            this.SbWeights.Location = new System.Drawing.Point(227, 68);
            this.SbWeights.Name = "SbWeights";
            this.SbWeights.Size = new System.Drawing.Size(502, 319);
            this.SbWeights.TabIndex = 1;
            // 
            // pmWeights
            // 
            this.pmWeights.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnAll0,
            this.mnAll100});
            this.pmWeights.Name = "pmWeights";
            this.pmWeights.Size = new System.Drawing.Size(187, 48);
            this.pmWeights.Opening += new System.ComponentModel.CancelEventHandler(this.pmWeightsPopup);
            // 
            // mnAll0
            // 
            this.mnAll0.Name = "mnAll0";
            this.mnAll0.Size = new System.Drawing.Size(186, 22);
            this.mnAll0.Text = "Aktuelle Seite auf 0";
            this.mnAll0.Click += new System.EventHandler(this.SetSideValues);
            // 
            // mnAll100
            // 
            this.mnAll100.ImageIndex = 43;
            this.mnAll100.Name = "mnAll100";
            this.mnAll100.Size = new System.Drawing.Size(186, 22);
            this.mnAll100.Tag = "100";
            this.mnAll100.Text = "Aktuelle Seite auf 100";
            this.mnAll100.ToolTipText = "Beenden|Die Anwendung beenden";
            this.mnAll100.Click += new System.EventHandler(this.SetSideValues);
            // 
            // TvWeights
            // 
            this.TvWeights.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.TvWeights.Location = new System.Drawing.Point(8, 68);
            this.TvWeights.Name = "TvWeights";
            treeNode1.Name = "Node0";
            treeNode1.Text = "Wichtungsgruppen";
            this.TvWeights.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.TvWeights.Size = new System.Drawing.Size(213, 319);
            this.TvWeights.TabIndex = 0;
            this.TvWeights.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TvWeightsChanging);
            // 
            // btOk
            // 
            this.btOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btOk.Image = ((System.Drawing.Image)(resources.GetObject("btOk.Image")));
            this.btOk.Location = new System.Drawing.Point(545, 397);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(89, 25);
            this.btOk.TabIndex = 4;
            this.btOk.Text = "OK";
            this.btOk.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOkClick);
            // 
            // btCancel
            // 
            this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Image = ((System.Drawing.Image)(resources.GetObject("btCancel.Image")));
            this.btCancel.Location = new System.Drawing.Point(640, 397);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(89, 25);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "Abbruch";
            this.btCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // btAllZero
            // 
            this.btAllZero.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btAllZero.Location = new System.Drawing.Point(225, 397);
            this.btAllZero.Name = "btAllZero";
            this.btAllZero.Size = new System.Drawing.Size(89, 25);
            this.btAllZero.TabIndex = 2;
            this.btAllZero.Text = "Alles 0";
            this.btAllZero.UseVisualStyleBackColor = true;
            this.btAllZero.Click += new System.EventHandler(this.SetAllValues);
            // 
            // btAll100
            // 
            this.btAll100.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btAll100.Location = new System.Drawing.Point(320, 397);
            this.btAll100.Name = "btAll100";
            this.btAll100.Size = new System.Drawing.Size(89, 25);
            this.btAll100.TabIndex = 3;
            this.btAll100.Text = "Alles 100";
            this.btAll100.UseVisualStyleBackColor = true;
            this.btAll100.Click += new System.EventHandler(this.SetAllValues);
            // 
            // CbVorlage
            // 
            this.CbVorlage.FormattingEnabled = true;
            this.CbVorlage.Location = new System.Drawing.Point(8, 25);
            this.CbVorlage.Name = "CbVorlage";
            this.CbVorlage.Size = new System.Drawing.Size(213, 21);
            this.CbVorlage.Sorted = true;
            this.CbVorlage.TabIndex = 6;
            this.CbVorlage.Click += new System.EventHandler(this.CbVorlageClick);
            // 
            // btTemplateSave
            // 
            this.btTemplateSave.Location = new System.Drawing.Point(227, 21);
            this.btTemplateSave.Name = "btTemplateSave";
            this.btTemplateSave.Size = new System.Drawing.Size(75, 25);
            this.btTemplateSave.TabIndex = 7;
            this.btTemplateSave.Text = "Speichern";
            this.btTemplateSave.UseVisualStyleBackColor = true;
            this.btTemplateSave.Click += new System.EventHandler(this.btTemplateSaveClick);
            // 
            // btTemplateDelete
            // 
            this.btTemplateDelete.Location = new System.Drawing.Point(308, 21);
            this.btTemplateDelete.Name = "btTemplateDelete";
            this.btTemplateDelete.Size = new System.Drawing.Size(75, 25);
            this.btTemplateDelete.TabIndex = 8;
            this.btTemplateDelete.Text = "Löschen";
            this.btTemplateDelete.UseVisualStyleBackColor = true;
            this.btTemplateDelete.Click += new System.EventHandler(this.btTemplateDeleteClick);
            // 
            // TfrmWeights
            // 
            this.AcceptButton = this.btOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(739, 430);
            this.Controls.Add(this.btTemplateDelete);
            this.Controls.Add(this.btTemplateSave);
            this.Controls.Add(this.CbVorlage);
            this.Controls.Add(this.btAll100);
            this.Controls.Add(this.btAllZero);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.TvWeights);
            this.Controls.Add(this.SbWeights);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btPlayerFilter);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.Label1);
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.Location = new System.Drawing.Point(57, 180);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(755, 2147483559);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(755, 400);
            this.Name = "TfrmWeights";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Wichtungen der Werte";
            this.TopMost = true;
            this.FormCreate += new System.EventHandler(this.TfrmWeights_FormCreate);
            this.FormShow += new System.EventHandler(this.Form_Show);
            this.panel1.ResumeLayout(false);
            this.pmTrackbar.ResumeLayout(false);
            this.pmWeights.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DelphiClasses.TLabel Label1;
        private DelphiClasses.TLabel Label2;
        private DelphiClasses.TLabel Label3;
        private DelphiClasses.TSpeedButton btPlayerFilter;
        private DelphiClasses.TSpeedButton btNullFilter;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ContextMenuStrip pmTrackbar;
        private System.Windows.Forms.Panel SbWeights;
        private System.Windows.Forms.TreeView TvWeights;
        private System.Windows.Forms.ContextMenuStrip pmWeights;
        private System.Windows.Forms.ToolStripMenuItem AktuelleSeiteaufdiesenWertsetzen1;
        private System.Windows.Forms.ToolStripMenuItem AlleSeitenaufdiesenWertsetzen1;
        private System.Windows.Forms.ToolStripMenuItem mnAll0;
        private DelphiClasses.TMainMenu.TTBXItem mnAll100;
        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btAllZero;
        private System.Windows.Forms.Button btAll100;
        private DelphiClasses.TComboBox CbVorlage;
        private System.Windows.Forms.Button btTemplateSave;
        private System.Windows.Forms.Button btTemplateDelete;
    }
}