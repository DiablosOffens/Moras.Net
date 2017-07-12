namespace Moras.Net
{
    partial class TfrmSetArmor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TfrmSetArmor));
            this.GroupBox1 = new System.Windows.Forms.GroupBox();
            this.cbArmorType = new DelphiClasses.TComboBox();
            this.Label3 = new DelphiClasses.TLabel();
            this.Label4 = new DelphiClasses.TLabel();
            this.Label2 = new DelphiClasses.TLabel();
            this.Label1 = new DelphiClasses.TLabel();
            this.helpProvider1 = new DelphiClasses.DelphiHelpProvider();
            this.cbQuality = new DelphiClasses.TComboBox();
            this.cbSubClass = new DelphiClasses.TComboBox();
            this.cbMaterial = new DelphiClasses.TComboBox();
            this.tbAF = new System.Windows.Forms.TextBox();
            this.tbBonus = new System.Windows.Forms.TextBox();
            this.btCancel = new System.Windows.Forms.Button();
            this.btOK = new System.Windows.Forms.Button();
            this.GroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // GroupBox1
            // 
            this.GroupBox1.Controls.Add(this.tbBonus);
            this.GroupBox1.Controls.Add(this.tbAF);
            this.GroupBox1.Controls.Add(this.cbQuality);
            this.GroupBox1.Controls.Add(this.cbMaterial);
            this.GroupBox1.Controls.Add(this.cbSubClass);
            this.GroupBox1.Controls.Add(this.cbArmorType);
            this.GroupBox1.Controls.Add(this.Label3);
            this.GroupBox1.Controls.Add(this.Label4);
            this.GroupBox1.Controls.Add(this.Label2);
            this.GroupBox1.Controls.Add(this.Label1);
            this.GroupBox1.Location = new System.Drawing.Point(8, 8);
            this.GroupBox1.Name = "GroupBox1";
            this.GroupBox1.Size = new System.Drawing.Size(225, 121);
            this.GroupBox1.TabIndex = 0;
            this.GroupBox1.TabStop = false;
            this.GroupBox1.Text = "Werte";
            // 
            // cbArmorType
            // 
            this.cbArmorType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbArmorType.FormattingEnabled = true;
            this.helpProvider1.SetHelpKeyword(this.cbArmorType, "Rüstung");
            this.helpProvider1.SetHelpNavigator(this.cbArmorType, System.Windows.Forms.HelpNavigator.KeywordIndex);
            this.cbArmorType.Location = new System.Drawing.Point(8, 32);
            this.cbArmorType.Name = "cbArmorType";
            this.helpProvider1.SetShowHelp(this.cbArmorType, true);
            this.cbArmorType.Size = new System.Drawing.Size(105, 21);
            this.cbArmorType.TabIndex = 0;
            this.cbArmorType.Change += new System.EventHandler(this.cbArmorTypeChange);
            // 
            // Label3
            // 
            this.Label3.AutoSize = false;
            this.Label3.BackColor = System.Drawing.SystemColors.Control;
            this.Label3.Location = new System.Drawing.Point(144, 68);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(16, 13);
            this.Label3.TabIndex = 0;
            this.Label3.Text = "AF:";
            this.Label3.Transparent = false;
            // 
            // Label4
            // 
            this.Label4.AutoSize = false;
            this.Label4.BackColor = System.Drawing.SystemColors.Control;
            this.Label4.Location = new System.Drawing.Point(128, 92);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(33, 13);
            this.Label4.TabIndex = 0;
            this.Label4.Text = "Bonus:";
            this.Label4.Transparent = false;
            // 
            // Label2
            // 
            this.Label2.AutoSize = false;
            this.Label2.BackColor = System.Drawing.SystemColors.Control;
            this.Label2.Location = new System.Drawing.Point(128, 36);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(39, 13);
            this.Label2.TabIndex = 0;
            this.Label2.Text = "Qualität:";
            this.Label2.Transparent = false;
            // 
            // Label1
            // 
            this.Label1.AutoSize = false;
            this.Label1.BackColor = System.Drawing.SystemColors.Control;
            this.Label1.Location = new System.Drawing.Point(8, 16);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(60, 13);
            this.Label1.TabIndex = 0;
            this.Label1.Text = "Rüstungsart:";
            this.Label1.Transparent = false;
            // 
            // cbQuality
            // 
            this.cbQuality.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbQuality.FormattingEnabled = true;
            this.helpProvider1.SetHelpKeyword(this.cbQuality, "Qualität");
            this.helpProvider1.SetHelpNavigator(this.cbQuality, System.Windows.Forms.HelpNavigator.KeywordIndex);
            this.cbQuality.Location = new System.Drawing.Point(168, 32);
            this.cbQuality.Name = "cbQuality";
            this.helpProvider1.SetShowHelp(this.cbQuality, true);
            this.cbQuality.Size = new System.Drawing.Size(49, 21);
            this.cbQuality.TabIndex = 3;
            // 
            // cbSubClass
            // 
            this.cbSubClass.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSubClass.FormattingEnabled = true;
            this.cbSubClass.Location = new System.Drawing.Point(8, 64);
            this.cbSubClass.Name = "cbSubClass";
            this.cbSubClass.Size = new System.Drawing.Size(105, 21);
            this.cbSubClass.TabIndex = 1;
            this.cbSubClass.Change += new System.EventHandler(this.cbSubClassChange);
            // 
            // cbMaterial
            // 
            this.cbMaterial.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbMaterial.FormattingEnabled = true;
            this.cbMaterial.Location = new System.Drawing.Point(8, 88);
            this.cbMaterial.Name = "cbMaterial";
            this.cbMaterial.Size = new System.Drawing.Size(105, 21);
            this.cbMaterial.TabIndex = 2;
            this.cbMaterial.Change += new System.EventHandler(this.cbMaterialChange);
            // 
            // tbAF
            // 
            this.tbAF.Enabled = false;
            this.helpProvider1.SetHelpKeyword(this.tbAF, "AF");
            this.helpProvider1.SetHelpNavigator(this.tbAF, System.Windows.Forms.HelpNavigator.KeywordIndex);
            this.tbAF.Location = new System.Drawing.Point(168, 64);
            this.tbAF.MinimumSize = new System.Drawing.Size(0, 21);
            this.tbAF.Name = "tbAF";
            this.helpProvider1.SetShowHelp(this.tbAF, true);
            this.tbAF.Size = new System.Drawing.Size(49, 21);
            this.tbAF.TabIndex = 4;
            // 
            // tbBonus
            // 
            this.tbBonus.Enabled = false;
            this.helpProvider1.SetHelpKeyword(this.tbBonus, "Bonus");
            this.helpProvider1.SetHelpNavigator(this.tbBonus, System.Windows.Forms.HelpNavigator.KeywordIndex);
            this.tbBonus.Location = new System.Drawing.Point(168, 88);
            this.tbBonus.MinimumSize = new System.Drawing.Size(0, 21);
            this.tbBonus.Name = "tbBonus";
            this.helpProvider1.SetShowHelp(this.tbBonus, true);
            this.tbBonus.Size = new System.Drawing.Size(49, 21);
            this.tbBonus.TabIndex = 5;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Image = ((System.Drawing.Image)(resources.GetObject("btCancel.Image")));
            this.btCancel.Location = new System.Drawing.Point(136, 136);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(97, 25);
            this.btCancel.TabIndex = 2;
            this.btCancel.Text = "&Abbrechen";
            this.btCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancelClick);
            // 
            // btOK
            // 
            this.btOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btOK.Image = ((System.Drawing.Image)(resources.GetObject("btOK.Image")));
            this.btOK.Location = new System.Drawing.Point(8, 136);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(97, 25);
            this.btOK.TabIndex = 1;
            this.btOK.Text = "&Übernehmen";
            this.btOK.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOKClick);
            // 
            // TfrmSetArmor
            // 
            this.AcceptButton = this.btOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(241, 167);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.GroupBox1);
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Location = new System.Drawing.Point(507, 183);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TfrmSetArmor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Erstelle Rüstung...";
            this.TopMost = true;
            this.FormCreate += new System.EventHandler(this.TfrmSetArmor_FormCreate);
            this.GroupBox1.ResumeLayout(false);
            this.GroupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox GroupBox1;
        private DelphiClasses.TLabel Label1;
        private DelphiClasses.TLabel Label2;
        private DelphiClasses.TLabel Label3;
        private DelphiClasses.TLabel Label4;
        private DelphiClasses.TComboBox cbArmorType;
        private DelphiClasses.DelphiHelpProvider helpProvider1;
        private DelphiClasses.TComboBox cbQuality;
        private DelphiClasses.TComboBox cbSubClass;
        private DelphiClasses.TComboBox cbMaterial;
        private System.Windows.Forms.TextBox tbAF;
        private System.Windows.Forms.TextBox tbBonus;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btOK;
    }
}