namespace Moras.Net
{
    partial class TfrmArmorSlot
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TfrmArmorSlot));
            this.rdArmorSlot = new DelphiClasses.TRadioGroup(this.components);
            this.gbInfo = new System.Windows.Forms.GroupBox();
            this.lbDescription = new DelphiClasses.TLabel();
            this.btClose = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.rdArmorSlot)).BeginInit();
            this.gbInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // rdArmorSlot
            // 
            this.rdArmorSlot.Items.Strings.AddRange(new string[] {
            "Hände/Handschuhe",
            "Kopf/Helm",
            "Füsse/Stiefel",
            "Beine/Beinlinge",
            "Arme/Armlinge",
            "Körper/Brustpanzer"});
            this.rdArmorSlot.Location = new System.Drawing.Point(208, 8);
            this.rdArmorSlot.Name = "rdArmorSlot";
            this.rdArmorSlot.Size = new System.Drawing.Size(137, 185);
            this.rdArmorSlot.TabIndex = 0;
            this.rdArmorSlot.TabStop = false;
            this.rdArmorSlot.Text = "Rüstungsposition";
            // 
            // gbInfo
            // 
            this.gbInfo.Controls.Add(this.lbDescription);
            this.gbInfo.Location = new System.Drawing.Point(8, 8);
            this.gbInfo.Name = "gbInfo";
            this.gbInfo.Size = new System.Drawing.Size(193, 216);
            this.gbInfo.TabIndex = 3;
            this.gbInfo.TabStop = false;
            // 
            // lbDescription
            // 
            this.lbDescription.AutoSize = false;
            this.lbDescription.BackColor = System.Drawing.SystemColors.Control;
            this.lbDescription.Location = new System.Drawing.Point(8, 16);
            this.lbDescription.Name = "lbDescription";
            this.lbDescription.Size = new System.Drawing.Size(177, 193);
            this.lbDescription.TabIndex = 5;
            this.lbDescription.Text = "lbDescription";
            this.lbDescription.Transparent = false;
            this.lbDescription.WordWrap = true;
            // 
            // btClose
            // 
            this.btClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btClose.Image = ((System.Drawing.Image)(resources.GetObject("btClose.Image")));
            this.btClose.Location = new System.Drawing.Point(208, 198);
            this.btClose.Name = "btClose";
            this.btClose.Size = new System.Drawing.Size(57, 25);
            this.btClose.TabIndex = 1;
            this.btClose.Text = "OK";
            this.btClose.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip1.SetToolTip(this.btClose, "Gegenstand mit der angegebenen Rüstungsart übernehmen");
            this.btClose.UseVisualStyleBackColor = true;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Image = ((System.Drawing.Image)(resources.GetObject("btCancel.Image")));
            this.btCancel.Location = new System.Drawing.Point(272, 198);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(73, 25);
            this.btCancel.TabIndex = 2;
            this.btCancel.Text = "Abbruch";
            this.btCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip1.SetToolTip(this.btCancel, "Rüstung nicht in die Datenbank übernehmen");
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // TfrmArmorSlot
            // 
            this.AcceptButton = this.btClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(353, 232);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btClose);
            this.Controls.Add(this.gbInfo);
            this.Controls.Add(this.rdArmorSlot);
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Location = new System.Drawing.Point(353, 259);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TfrmArmorSlot";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bitte die Rüstungsposition angeben...";
            this.FormCreate += new System.EventHandler(this.TfrmArmorSlot_FormCreate);
            ((System.ComponentModel.ISupportInitialize)(this.rdArmorSlot)).EndInit();
            this.gbInfo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btClose;
        private System.Windows.Forms.Button btCancel;
        public DelphiClasses.TRadioGroup rdArmorSlot;
        public System.Windows.Forms.GroupBox gbInfo;
        public DelphiClasses.TLabel lbDescription;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}