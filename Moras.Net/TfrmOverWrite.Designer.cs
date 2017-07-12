namespace Moras.Net
{
    partial class TfrmOverWrite
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
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.lbDescription1 = new DelphiClasses.TLabel();
            this.lbName1 = new DelphiClasses.TLabel();
            this.Label2 = new DelphiClasses.TLabel();
            this.GroupBox2 = new System.Windows.Forms.GroupBox();
            this.lbDescription2 = new DelphiClasses.TLabel();
            this.Label3 = new DelphiClasses.TLabel();
            this.lbName2 = new DelphiClasses.TLabel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.pTop = new System.Windows.Forms.Panel();
            this.Label1 = new DelphiClasses.TLabel();
            this.pBottom = new System.Windows.Forms.Panel();
            this.Button1 = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.btNo = new System.Windows.Forms.Button();
            this.btOk = new System.Windows.Forms.Button();
            this.Splitter1 = new System.Windows.Forms.Splitter();
            this.cbAllOptions = new DelphiClasses.TComboBox();
            this.GroupBox1.SuspendLayout();
            this.GroupBox2.SuspendLayout();
            this.pTop.SuspendLayout();
            this.pBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // GroupBox1
            // 
            this.GroupBox1.Controls.Add(this.lbDescription1);
            this.GroupBox1.Controls.Add(this.lbName1);
            this.GroupBox1.Controls.Add(this.Label2);
            this.GroupBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.GroupBox1.Location = new System.Drawing.Point(0, 53);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Size = new System.Drawing.Size(300, 365);
            this.GroupBox1.TabIndex = 0;
            this.GroupBox1.TabStop = false;
            this.GroupBox1.Text = "Gegenstand in Datenbank";
            // 
            // lbDescription1
            // 
            this.lbDescription1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbDescription1.AutoSize = false;
            this.lbDescription1.BackColor = System.Drawing.SystemColors.Control;
            this.lbDescription1.Location = new System.Drawing.Point(8, 32);
            this.lbDescription1.Name = "lbDescription1";
            this.lbDescription1.Size = new System.Drawing.Size(282, 323);
            this.lbDescription1.TabIndex = 0;
            this.lbDescription1.Text = "lbDescription1";
            this.lbDescription1.Transparent = false;
            this.lbDescription1.WordWrap = true;
            // 
            // lbName1
            // 
            this.lbName1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbName1.AutoSize = false;
            this.lbName1.BackColor = System.Drawing.SystemColors.Control;
            this.lbName1.Location = new System.Drawing.Point(40, 16);
            this.lbName1.Name = "lbName1";
            this.lbName1.Size = new System.Drawing.Size(250, 17);
            this.lbName1.TabIndex = 0;
            this.lbName1.Text = "lbName1";
            this.lbName1.Transparent = false;
            // 
            // Label2
            // 
            this.Label2.AutoSize = false;
            this.Label2.BackColor = System.Drawing.SystemColors.Control;
            this.Label2.Location = new System.Drawing.Point(8, 16);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(31, 13);
            this.Label2.TabIndex = 0;
            this.Label2.Text = "Name:";
            this.Label2.Transparent = false;
            // 
            // GroupBox2
            // 
            this.GroupBox2.Controls.Add(this.lbDescription2);
            this.GroupBox2.Controls.Add(this.Label3);
            this.GroupBox2.Controls.Add(this.lbName2);
            this.GroupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GroupBox2.Location = new System.Drawing.Point(304, 53);
            this.GroupBox2.Name = "GroupBox2";
            this.GroupBox2.Size = new System.Drawing.Size(282, 365);
            this.GroupBox2.TabIndex = 1;
            this.GroupBox2.TabStop = false;
            this.GroupBox2.Text = "zu speichernder Gegenstand";
            // 
            // lbDescription2
            // 
            this.lbDescription2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbDescription2.AutoSize = false;
            this.lbDescription2.BackColor = System.Drawing.SystemColors.Control;
            this.lbDescription2.Location = new System.Drawing.Point(8, 32);
            this.lbDescription2.Name = "lbDescription2";
            this.lbDescription2.Size = new System.Drawing.Size(264, 323);
            this.lbDescription2.TabIndex = 0;
            this.lbDescription2.Text = "lbDescription2";
            this.lbDescription2.Transparent = false;
            this.lbDescription2.WordWrap = true;
            // 
            // Label3
            // 
            this.Label3.AutoSize = false;
            this.Label3.BackColor = System.Drawing.SystemColors.Control;
            this.Label3.Location = new System.Drawing.Point(8, 16);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(31, 13);
            this.Label3.TabIndex = 0;
            this.Label3.Text = "Name:";
            this.Label3.Transparent = false;
            // 
            // lbName2
            // 
            this.lbName2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbName2.AutoSize = false;
            this.lbName2.BackColor = System.Drawing.SystemColors.Control;
            this.lbName2.Location = new System.Drawing.Point(40, 16);
            this.lbName2.Name = "lbName2";
            this.lbName2.Size = new System.Drawing.Size(232, 17);
            this.lbName2.TabIndex = 0;
            this.lbName2.Text = "lbName2";
            this.lbName2.Transparent = false;
            // 
            // pTop
            // 
            this.pTop.Controls.Add(this.Label1);
            this.pTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pTop.Location = new System.Drawing.Point(0, 0);
            this.pTop.Name = "pTop";
            this.pTop.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.pTop.Size = new System.Drawing.Size(586, 53);
            this.pTop.TabIndex = 3;
            // 
            // Label1
            // 
            this.Label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Label1.AutoSize = false;
            this.Label1.BackColor = System.Drawing.SystemColors.Control;
            this.Label1.Location = new System.Drawing.Point(0, 0);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(571, 41);
            this.Label1.TabIndex = 0;
            this.Label1.Text = "Ein Gegenstand mit gleichem Namen/gleichen Daten ist bereits in der Datenbank. Wa" +
    "s soll mit dem neuen Gegenstand getan werden?";
            this.Label1.Transparent = false;
            this.Label1.WordWrap = true;
            // 
            // pBottom
            // 
            this.pBottom.Controls.Add(this.cbAllOptions);
            this.pBottom.Controls.Add(this.Button1);
            this.pBottom.Controls.Add(this.btCancel);
            this.pBottom.Controls.Add(this.btNo);
            this.pBottom.Controls.Add(this.btOk);
            this.pBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pBottom.Location = new System.Drawing.Point(0, 418);
            this.pBottom.Name = "pBottom";
            this.pBottom.Size = new System.Drawing.Size(586, 34);
            this.pBottom.TabIndex = 2;
            // 
            // Button1
            // 
            this.Button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Button1.Location = new System.Drawing.Point(516, 8);
            this.Button1.Name = "Button1";
            this.Button1.Size = new System.Drawing.Size(68, 25);
            this.Button1.TabIndex = 3;
            this.Button1.Text = "Für Alle";
            this.Button1.UseVisualStyleBackColor = true;
            this.Button1.Click += new System.EventHandler(this.Button1Click);
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(200, 8);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(77, 25);
            this.btCancel.TabIndex = 2;
            this.btCancel.Text = "&Ignorieren";
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // btNo
            // 
            this.btNo.DialogResult = System.Windows.Forms.DialogResult.No;
            this.btNo.Location = new System.Drawing.Point(100, 8);
            this.btNo.Name = "btNo";
            this.btNo.Size = new System.Drawing.Size(97, 25);
            this.btNo.TabIndex = 1;
            this.btNo.Text = "&Neu Speichern";
            this.btNo.UseVisualStyleBackColor = true;
            // 
            // btOk
            // 
            this.btOk.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.btOk.Location = new System.Drawing.Point(0, 8);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(97, 25);
            this.btOk.TabIndex = 0;
            this.btOk.Text = "Über&schreiben";
            this.btOk.UseVisualStyleBackColor = true;
            // 
            // Splitter1
            // 
            this.Splitter1.Location = new System.Drawing.Point(300, 53);
            this.Splitter1.Name = "Splitter1";
            this.Splitter1.Size = new System.Drawing.Size(4, 365);
            this.Splitter1.TabIndex = 1;
            this.Splitter1.TabStop = false;
            // 
            // cbAllOptions
            // 
            this.cbAllOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbAllOptions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbAllOptions.FormattingEnabled = true;
            this.cbAllOptions.Items.AddRange(new object[] {
            "Alle Überschreiben",
            "Alle als neu Speichern",
            "Alle ignorieren"});
            this.cbAllOptions.Location = new System.Drawing.Point(344, 11);
            this.cbAllOptions.Name = "cbAllOptions";
            this.cbAllOptions.SelectedIndex = 0;
            this.cbAllOptions.Size = new System.Drawing.Size(168, 21);
            this.cbAllOptions.TabIndex = 4;
            // 
            // TfrmOverWrite
            // 
            this.AcceptButton = this.btOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(586, 452);
            this.Controls.Add(this.GroupBox2);
            this.Controls.Add(this.Splitter1);
            this.Controls.Add(this.GroupBox1);
            this.Controls.Add(this.pTop);
            this.Controls.Add(this.pBottom);
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.Location = new System.Drawing.Point(327, 202);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(602, 491);
            this.Name = "TfrmOverWrite";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Gegenstand überschreiben?";
            this.FormCreate += new System.EventHandler(this.TfrmOverWrite_FormCreate);
            this.Resize += new System.EventHandler(this.FormResize);
            this.GroupBox1.ResumeLayout(false);
            this.GroupBox2.ResumeLayout(false);
            this.pTop.ResumeLayout(false);
            this.pBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox GroupBox1;
        private System.Windows.Forms.GroupBox GroupBox2;
        private DelphiClasses.TLabel Label2;
        private System.Windows.Forms.ToolTip toolTip1;
        private DelphiClasses.TLabel Label3;
        private System.Windows.Forms.Panel pTop;
        private System.Windows.Forms.Panel pBottom;
        private System.Windows.Forms.Splitter Splitter1;
        public DelphiClasses.TLabel lbName1;
        public DelphiClasses.TLabel lbDescription1;
        public DelphiClasses.TLabel lbName2;
        public DelphiClasses.TLabel lbDescription2;
        private DelphiClasses.TLabel Label1;
        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.Button btNo;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button Button1;
        private DelphiClasses.TComboBox cbAllOptions;

    }
}