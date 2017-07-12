namespace Moras.Net
{
    partial class TfrmReport
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TfrmReport));
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.btReport4 = new DelphiClasses.TSpeedButton();
            this.btReport3 = new DelphiClasses.TSpeedButton();
            this.btReport2 = new DelphiClasses.TSpeedButton();
            this.btReport1 = new DelphiClasses.TSpeedButton();
            this.GroupBox2 = new System.Windows.Forms.GroupBox();
            this.tbReport = new System.Windows.Forms.RichTextBox();
            this.btClose = new System.Windows.Forms.Button();
            this.btSave = new System.Windows.Forms.Button();
            this.btPrint = new System.Windows.Forms.Button();
            this.btClipboard = new System.Windows.Forms.Button();
            this.SaveDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.PrintDialog1 = new System.Windows.Forms.PrintDialog();
            this.GroupBox1.SuspendLayout();
            this.GroupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // GroupBox1
            // 
            this.GroupBox1.Controls.Add(this.btReport4);
            this.GroupBox1.Controls.Add(this.btReport3);
            this.GroupBox1.Controls.Add(this.btReport2);
            this.GroupBox1.Controls.Add(this.btReport1);
            this.GroupBox1.Location = new System.Drawing.Point(8, 8);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Size = new System.Drawing.Size(457, 49);
            this.GroupBox1.TabIndex = 0;
            this.GroupBox1.TabStop = false;
            this.GroupBox1.Text = "Art";
            // 
            // btReport4
            // 
            this.btReport4.Appearance = System.Windows.Forms.Appearance.Button;
            this.btReport4.Location = new System.Drawing.Point(344, 16);
            this.btReport4.Name = "btReport4";
            this.btReport4.Size = new System.Drawing.Size(105, 25);
            this.btReport4.TabIndex = 0;
            this.btReport4.Tag = "3";
            this.btReport4.Text = "Materialliste";
            this.btReport4.UseVisualStyleBackColor = true;
            this.btReport4.Click += new System.EventHandler(this.btReportClick);
            // 
            // btReport3
            // 
            this.btReport3.Appearance = System.Windows.Forms.Appearance.Button;
            this.btReport3.Location = new System.Drawing.Point(232, 16);
            this.btReport3.Name = "btReport3";
            this.btReport3.Size = new System.Drawing.Size(105, 25);
            this.btReport3.TabIndex = 0;
            this.btReport3.Tag = "2";
            this.btReport3.Text = "Händlerbericht";
            this.btReport3.UseVisualStyleBackColor = true;
            this.btReport3.Click += new System.EventHandler(this.btReportClick);
            // 
            // btReport2
            // 
            this.btReport2.Appearance = System.Windows.Forms.Appearance.Button;
            this.btReport2.Location = new System.Drawing.Point(120, 16);
            this.btReport2.Name = "btReport2";
            this.btReport2.Size = new System.Drawing.Size(105, 25);
            this.btReport2.TabIndex = 0;
            this.btReport2.Tag = "1";
            this.btReport2.Text = "Kurzer Bericht";
            this.btReport2.UseVisualStyleBackColor = true;
            this.btReport2.Click += new System.EventHandler(this.btReportClick);
            // 
            // btReport1
            // 
            this.btReport1.Appearance = System.Windows.Forms.Appearance.Button;
            this.btReport1.Checked = true;
            this.btReport1.Location = new System.Drawing.Point(8, 16);
            this.btReport1.Name = "btReport1";
            this.btReport1.Size = new System.Drawing.Size(105, 25);
            this.btReport1.TabIndex = 0;
            this.btReport1.TabStop = true;
            this.btReport1.Text = "Langer Bericht";
            this.btReport1.UseVisualStyleBackColor = true;
            this.btReport1.Click += new System.EventHandler(this.btReportClick);
            // 
            // GroupBox2
            // 
            this.GroupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GroupBox2.Controls.Add(this.tbReport);
            this.GroupBox2.Location = new System.Drawing.Point(8, 64);
            this.GroupBox2.Name = "GroupBox2";
            this.GroupBox2.Size = new System.Drawing.Size(753, 436);
            this.GroupBox2.TabIndex = 1;
            this.GroupBox2.TabStop = false;
            this.GroupBox2.Text = "Report";
            // 
            // tbReport
            // 
            this.tbReport.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbReport.BackColor = System.Drawing.SystemColors.Window;
            this.tbReport.Font = new System.Drawing.Font("Courier New", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World);
            this.tbReport.Location = new System.Drawing.Point(8, 16);
            this.tbReport.Name = "tbReport";
            this.tbReport.ReadOnly = true;
            this.tbReport.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.tbReport.Size = new System.Drawing.Size(737, 412);
            this.tbReport.TabIndex = 0;
            this.tbReport.Text = "";
            // 
            // btClose
            // 
            this.btClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btClose.Location = new System.Drawing.Point(672, 507);
            this.btClose.Name = "btClose";
            this.btClose.Size = new System.Drawing.Size(89, 25);
            this.btClose.TabIndex = 5;
            this.btClose.Text = "&Schliessen";
            this.btClose.UseVisualStyleBackColor = true;
            // 
            // btSave
            // 
            this.btSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btSave.Image = ((System.Drawing.Image)(resources.GetObject("btSave.Image")));
            this.btSave.Location = new System.Drawing.Point(8, 507);
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size(89, 25);
            this.btSave.TabIndex = 2;
            this.btSave.Text = "&Speichern";
            this.btSave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btSave.UseVisualStyleBackColor = true;
            this.btSave.Click += new System.EventHandler(this.btSaveClick);
            // 
            // btPrint
            // 
            this.btPrint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btPrint.Image = ((System.Drawing.Image)(resources.GetObject("btPrint.Image")));
            this.btPrint.Location = new System.Drawing.Point(104, 507);
            this.btPrint.Name = "btPrint";
            this.btPrint.Size = new System.Drawing.Size(89, 25);
            this.btPrint.TabIndex = 3;
            this.btPrint.Text = "&Drucken";
            this.btPrint.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btPrint.UseVisualStyleBackColor = true;
            this.btPrint.Click += new System.EventHandler(this.btPrintClick);
            // 
            // btClipboard
            // 
            this.btClipboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btClipboard.Image = ((System.Drawing.Image)(resources.GetObject("btClipboard.Image")));
            this.btClipboard.Location = new System.Drawing.Point(200, 507);
            this.btClipboard.Name = "btClipboard";
            this.btClipboard.Size = new System.Drawing.Size(129, 25);
            this.btClipboard.TabIndex = 4;
            this.btClipboard.Text = "&in Zwischenablage";
            this.btClipboard.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btClipboard.UseVisualStyleBackColor = true;
            this.btClipboard.Click += new System.EventHandler(this.btClipboardClick);
            // 
            // SaveDialog1
            // 
            this.SaveDialog1.DefaultExt = "txt";
            this.SaveDialog1.Filter = "Textdateien (*.txt)|*.txt";
            this.SaveDialog1.Title = "Speichere Report unter...";
            // 
            // TfrmReport
            // 
            this.AcceptButton = this.btClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.CancelButton = this.btClose;
            this.ClientSize = new System.Drawing.Size(767, 540);
            this.Controls.Add(this.btClipboard);
            this.Controls.Add(this.btPrint);
            this.Controls.Add(this.btSave);
            this.Controls.Add(this.btClose);
            this.Controls.Add(this.GroupBox2);
            this.Controls.Add(this.GroupBox1);
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.Location = new System.Drawing.Point(259, 123);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(479, 350);
            this.Name = "TfrmReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Konfigurations-Report";
            this.TopMost = true;
            this.FormCreate += new System.EventHandler(this.TfrmReport_FormCreate);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TfrmReport_FormClose);
            this.GroupBox1.ResumeLayout(false);
            this.GroupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox GroupBox1;
        private DelphiClasses.TSpeedButton btReport1;
        private DelphiClasses.TSpeedButton btReport4;
        private DelphiClasses.TSpeedButton btReport3;
        private DelphiClasses.TSpeedButton btReport2;
        private System.Windows.Forms.GroupBox GroupBox2;
        private System.Windows.Forms.RichTextBox tbReport;
        private System.Windows.Forms.Button btClose;
        private System.Windows.Forms.Button btSave;
        private System.Windows.Forms.Button btPrint;
        private System.Windows.Forms.Button btClipboard;
        private System.Windows.Forms.SaveFileDialog SaveDialog1;
        private System.Windows.Forms.PrintDialog PrintDialog1;
    }
}