namespace Moras.Net
{
    partial class TfrmWeaponSlot
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TfrmWeaponSlot));
            this.gbInfo = new System.Windows.Forms.GroupBox();
            this.lbDescription = new DelphiClasses.TLabel();
            this.btOK = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btCancel = new System.Windows.Forms.Button();
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.cbDamageType = new Moras.Net.Components.TMIComboBox();
            this.cbWeaponClass = new Moras.Net.Components.TMIComboBox();
            this.Label2 = new DelphiClasses.TLabel();
            this.Label1 = new DelphiClasses.TLabel();
            this.gbInfo.SuspendLayout();
            this.GroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // gbInfo
            // 
            this.gbInfo.Controls.Add(this.lbDescription);
            this.gbInfo.Location = new System.Drawing.Point(8, 8);
            this.gbInfo.Name = "gbInfo";
            this.gbInfo.Size = new System.Drawing.Size(193, 216);
            this.gbInfo.TabIndex = 0;
            this.gbInfo.TabStop = false;
            // 
            // lbDescription
            // 
            this.lbDescription.AutoSize = false;
            this.lbDescription.BackColor = System.Drawing.SystemColors.Control;
            this.lbDescription.Location = new System.Drawing.Point(8, 16);
            this.lbDescription.Name = "lbDescription";
            this.lbDescription.Size = new System.Drawing.Size(177, 193);
            this.lbDescription.TabIndex = 0;
            this.lbDescription.Text = "lbDescription";
            this.lbDescription.Transparent = false;
            this.lbDescription.WordWrap = true;
            // 
            // btOK
            // 
            this.btOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btOK.Image = ((System.Drawing.Image)(resources.GetObject("btOK.Image")));
            this.btOK.Location = new System.Drawing.Point(208, 198);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(57, 25);
            this.btOK.TabIndex = 1;
            this.btOK.Text = "OK";
            this.btOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip1.SetToolTip(this.btOK, "Gegenstand mit der gewählten Waffen- und Schadensart übernehmen");
            this.btOK.UseVisualStyleBackColor = true;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Image = ((System.Drawing.Image)(resources.GetObject("btCancel.Image")));
            this.btCancel.Location = new System.Drawing.Point(272, 198);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(73, 25);
            this.btCancel.TabIndex = 3;
            this.btCancel.Text = "Abbruch";
            this.btCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip1.SetToolTip(this.btCancel, "Gegenstand nicht übernehmen");
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // GroupBox1
            // 
            this.GroupBox1.Controls.Add(this.cbDamageType);
            this.GroupBox1.Controls.Add(this.cbWeaponClass);
            this.GroupBox1.Controls.Add(this.Label2);
            this.GroupBox1.Controls.Add(this.Label1);
            this.GroupBox1.Location = new System.Drawing.Point(208, 8);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Size = new System.Drawing.Size(137, 185);
            this.GroupBox1.TabIndex = 2;
            this.GroupBox1.TabStop = false;
            // 
            // cbDamageType
            // 
            this.cbDamageType.FormattingEnabled = true;
            this.cbDamageType.Location = new System.Drawing.Point(8, 80);
            this.cbDamageType.Name = "cbDamageType";
            this.cbDamageType.Size = new System.Drawing.Size(121, 22);
            this.cbDamageType.TabIndex = 1;
            // 
            // cbWeaponClass
            // 
            this.cbWeaponClass.FormattingEnabled = true;
            this.cbWeaponClass.Location = new System.Drawing.Point(8, 32);
            this.cbWeaponClass.Name = "cbWeaponClass";
            this.cbWeaponClass.Size = new System.Drawing.Size(121, 22);
            this.cbWeaponClass.TabIndex = 0;
            this.cbWeaponClass.Change += new System.EventHandler(this.cbWeaponClassChange);
            // 
            // Label2
            // 
            this.Label2.AutoSize = false;
            this.Label2.BackColor = System.Drawing.SystemColors.Control;
            this.Label2.Location = new System.Drawing.Point(8, 64);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(63, 13);
            this.Label2.TabIndex = 0;
            this.Label2.Text = "Schadensart:";
            this.Label2.Transparent = false;
            // 
            // Label1
            // 
            this.Label1.AutoSize = false;
            this.Label1.BackColor = System.Drawing.SystemColors.Control;
            this.Label1.Location = new System.Drawing.Point(8, 16);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(50, 13);
            this.Label1.TabIndex = 0;
            this.Label1.Text = "Waffenart:";
            this.Label1.Transparent = false;
            // 
            // TfrmWeaponSlot
            // 
            this.AcceptButton = this.btOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(354, 232);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.GroupBox1);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.gbInfo);
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Location = new System.Drawing.Point(353, 259);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TfrmWeaponSlot";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bitte die Waffenart angeben...";
            this.FormCreate += new System.EventHandler(this.TfrmWeaponSlot_FormCreate);
            this.FormShow += new System.EventHandler(this.Form_Show);
            this.gbInfo.ResumeLayout(false);
            this.GroupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.GroupBox GroupBox1;
        private DelphiClasses.TLabel Label1;
        private DelphiClasses.TLabel Label2;
        private System.Windows.Forms.Button btCancel;
        public System.Windows.Forms.GroupBox gbInfo;
        public DelphiClasses.TLabel lbDescription;
        public Components.TMIComboBox cbWeaponClass;
        public Components.TMIComboBox cbDamageType;

    }
}