namespace Moras.Net
{
    partial class TfrmJewelrySlot
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TfrmJewelrySlot));
            this.rdJewelrySlot = new DelphiClasses.TRadioGroup(this.components);
            this.gbInfo = new System.Windows.Forms.GroupBox();
            this.lbDescription = new DelphiClasses.TLabel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btClose = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.rdJewelrySlot)).BeginInit();
            this.gbInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // rdJewelrySlot
            // 
            this.rdJewelrySlot.Items.Strings.AddRange(new string[] {
            "Hals/Halskette",
            "Umhang",
            "Juwel",
            "Taille/Gürtel",
            "Ring",
            "Handgelenk/Armreif"});
            this.rdJewelrySlot.Location = new System.Drawing.Point(208, 8);
            this.rdJewelrySlot.Name = "rdJewelrySlot";
            this.rdJewelrySlot.Size = new System.Drawing.Size(137, 185);
            this.rdJewelrySlot.TabIndex = 0;
            this.rdJewelrySlot.TabStop = false;
            this.rdJewelrySlot.Text = "Schmuckart";
            // 
            // gbInfo
            // 
            this.gbInfo.Controls.Add(this.lbDescription);
            this.gbInfo.Location = new System.Drawing.Point(8, 8);
            this.gbInfo.Name = "gbInfo";
            this.gbInfo.Size = new System.Drawing.Size(193, 216);
            this.gbInfo.TabIndex = 1;
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
            // btClose
            // 
            this.btClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btClose.Image = ((System.Drawing.Image)(resources.GetObject("btClose.Image")));
            this.btClose.Location = new System.Drawing.Point(208, 198);
            this.btClose.Name = "btClose";
            this.btClose.Size = new System.Drawing.Size(57, 25);
            this.btClose.TabIndex = 2;
            this.btClose.Text = "OK";
            this.btClose.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip1.SetToolTip(this.btClose, "Gegenstand als die angegebene Schmuckart übernehmen");
            this.btClose.UseVisualStyleBackColor = true;
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
            this.toolTip1.SetToolTip(this.btCancel, "Gegenstand nicht in Datenbank übernehmen");
            this.btCancel.UseVisualStyleBackColor = true;
            // 
            // TfrmJewelrySlot
            // 
            this.AcceptButton = this.btClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(355, 232);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btClose);
            this.Controls.Add(this.gbInfo);
            this.Controls.Add(this.rdJewelrySlot);
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Location = new System.Drawing.Point(301, 261);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TfrmJewelrySlot";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bitte die Art des Schmuckstücks angeben...";
            this.FormCreate += new System.EventHandler(this.TfrmJewelrySlot_FormCreate);
            ((System.ComponentModel.ISupportInitialize)(this.rdJewelrySlot)).EndInit();
            this.gbInfo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btClose;
        private System.Windows.Forms.Button btCancel;
        public System.Windows.Forms.GroupBox gbInfo;
        public DelphiClasses.TRadioGroup rdJewelrySlot;
        public DelphiClasses.TLabel lbDescription;
    }
}