namespace Moras.Net
{
    partial class TfrmUpdateMode
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
            this.RadioGroup1 = new DelphiClasses.TRadioGroup(this.components);
            this.btOk = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.RadioGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // RadioGroup1
            // 
            this.RadioGroup1.ItemIndex = 0;
            this.RadioGroup1.Items.Strings.AddRange(new string[] {
            "Nur neue und geänderte Items herunterladen",
            "Alle Items herunterladen"});
            this.RadioGroup1.Location = new System.Drawing.Point(12, 12);
            this.RadioGroup1.Name = "RadioGroup1";
            this.RadioGroup1.Size = new System.Drawing.Size(277, 101);
            this.RadioGroup1.TabIndex = 0;
            this.RadioGroup1.TabStop = false;
            this.RadioGroup1.Text = "Download Optionen";
            // 
            // btOk
            // 
            this.btOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btOk.Location = new System.Drawing.Point(112, 124);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(75, 25);
            this.btOk.TabIndex = 1;
            this.btOk.Text = "OK";
            this.btOk.UseVisualStyleBackColor = true;
            // 
            // TfrmUpdateMode
            // 
            this.AcceptButton = this.btOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.ClientSize = new System.Drawing.Size(300, 159);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.RadioGroup1);
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Location = new System.Drawing.Point(192, 114);
            this.Name = "TfrmUpdateMode";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Online-Update";
            this.FormCreate += new System.EventHandler(this.TfrmUpdateMode_FormCreate);
            ((System.ComponentModel.ISupportInitialize)(this.RadioGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btOk;
        public DelphiClasses.TRadioGroup RadioGroup1;
    }
}