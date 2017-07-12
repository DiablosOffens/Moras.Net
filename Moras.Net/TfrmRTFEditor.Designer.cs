namespace Moras.Net
{
    partial class TfrmRTFEditor
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TfrmRTFEditor));
            this.ImageList1 = new System.Windows.Forms.ImageList(this.components);
            this.reComment = new DelphiClasses.TJvRichEdit();
            this.TBXDock1 = new DelphiClasses.TTBXDock();
            this.TBXToolbar1 = new DelphiClasses.TToolBar();
            this.cbFonts = new System.Windows.Forms.ToolStripComboBox();
            this.seFontsize = new DelphiClasses.TTBXSpinEditItem();
            this.TBXSeparatorItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.btBold = new DelphiClasses.TToolBar.TTBXItem(this.components);
            this.btItalic = new DelphiClasses.TToolBar.TTBXItem(this.components);
            this.btUnderline = new DelphiClasses.TToolBar.TTBXItem(this.components);
            this.TBXSeparatorItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.btLeft = new DelphiClasses.TToolBar.TTBXItem(this.components);
            this.btCenter = new DelphiClasses.TToolBar.TTBXItem(this.components);
            this.btRight = new DelphiClasses.TToolBar.TTBXItem(this.components);
            this.btBlock = new DelphiClasses.TToolBar.TTBXItem(this.components);
            this.TBXSeparatorItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.btIndentInc = new DelphiClasses.TToolBar.TTBXItem(this.components);
            this.btIndentDec = new DelphiClasses.TToolBar.TTBXItem(this.components);
            this.btCount = new DelphiClasses.TToolBar.TTBXItem(this.components);
            this.TBXSeparatorItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.btFontColor = new DelphiClasses.TToolBar.TTBXItem(this.components);
            this.ActionList1 = new DelphiClasses.TActionList(this.components);
            this.ColorDialog = new System.Windows.Forms.ColorDialog();
            this.TBXDock1.SuspendLayout();
            this.TBXToolbar1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ImageList1
            // 
            this.ImageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ImageList1.ImageStream")));
            this.ImageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.ImageList1.Images.SetKeyName(0, "");
            this.ImageList1.Images.SetKeyName(1, "");
            this.ImageList1.Images.SetKeyName(2, "");
            this.ImageList1.Images.SetKeyName(3, "");
            this.ImageList1.Images.SetKeyName(4, "");
            this.ImageList1.Images.SetKeyName(5, "");
            this.ImageList1.Images.SetKeyName(6, "");
            this.ImageList1.Images.SetKeyName(7, "");
            this.ImageList1.Images.SetKeyName(8, "");
            this.ImageList1.Images.SetKeyName(9, "");
            this.ImageList1.Images.SetKeyName(10, "");
            this.ImageList1.Images.SetKeyName(11, "");
            // 
            // reComment
            // 
            this.reComment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.reComment.Font = new System.Drawing.Font("Tahoma", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World, ((byte)(0)));
            this.reComment.Location = new System.Drawing.Point(0, 26);
            this.reComment.Name = "reComment";
            this.reComment.Size = new System.Drawing.Size(766, 432);
            this.reComment.TabIndex = 0;
            this.reComment.Text = "";
            this.reComment.SelectionChanged += new System.EventHandler(this.reCommentChange);
            this.reComment.TextChanged += new System.EventHandler(this.reCommentChange);
            // 
            // TBXDock1
            // 
            this.TBXDock1.Controls.Add(this.TBXToolbar1);
            this.TBXDock1.Dock = System.Windows.Forms.DockStyle.Top;
            this.TBXDock1.Location = new System.Drawing.Point(0, 0);
            this.TBXDock1.Name = "TBXDock1";
            this.TBXDock1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.TBXDock1.RowMargin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.TBXDock1.Size = new System.Drawing.Size(766, 26);
            // 
            // TBXToolbar1
            // 
            this.TBXToolbar1.Dock = System.Windows.Forms.DockStyle.None;
            this.TBXToolbar1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TBXToolbar1.ImageList = this.ImageList1;
            this.TBXToolbar1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cbFonts,
            this.seFontsize,
            this.TBXSeparatorItem1,
            this.btBold,
            this.btItalic,
            this.btUnderline,
            this.TBXSeparatorItem2,
            this.btLeft,
            this.btCenter,
            this.btRight,
            this.btBlock,
            this.TBXSeparatorItem3,
            this.btIndentInc,
            this.btIndentDec,
            this.btCount,
            this.TBXSeparatorItem4,
            this.btFontColor});
            this.TBXToolbar1.Location = new System.Drawing.Point(0, 0);
            this.TBXToolbar1.Name = "TBXToolbar1";
            this.TBXToolbar1.Size = new System.Drawing.Size(495, 25);
            this.TBXToolbar1.TabIndex = 0;
            this.TBXToolbar1.Text = "TBXToolbar1";
            // 
            // cbFonts
            // 
            this.cbFonts.Name = "cbFonts";
            this.cbFonts.Size = new System.Drawing.Size(120, 25);
            this.cbFonts.ToolTipText = "Schriftart";
            this.cbFonts.SelectedIndexChanged += new System.EventHandler(this.cbFontsChange);
            // 
            // seFontsize
            // 
            this.seFontsize.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.seFontsize.Name = "seFontsize";
            this.seFontsize.ReadOnly = true;
            this.seFontsize.Size = new System.Drawing.Size(53, 22);
            this.seFontsize.Text = "0";
            this.seFontsize.ToolTipText = "Schriftgrösse";
            this.seFontsize.ValueChanged += new System.EventHandler(this.seFontsizeValueChange);
            // 
            // TBXSeparatorItem1
            // 
            this.TBXSeparatorItem1.Name = "TBXSeparatorItem1";
            this.TBXSeparatorItem1.Size = new System.Drawing.Size(6, 25);
            // 
            // btBold
            // 
            this.btBold.CheckOnClick = true;
            this.btBold.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btBold.ImageIndex = 0;
            this.btBold.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btBold.Name = "btBold";
            this.btBold.Size = new System.Drawing.Size(23, 22);
            this.btBold.Text = "Fett";
            this.btBold.Click += new System.EventHandler(this.btBoldClick);
            // 
            // btItalic
            // 
            this.btItalic.CheckOnClick = true;
            this.btItalic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btItalic.ImageIndex = 1;
            this.btItalic.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btItalic.Name = "btItalic";
            this.btItalic.Size = new System.Drawing.Size(23, 22);
            this.btItalic.Text = "Kursiv";
            this.btItalic.Click += new System.EventHandler(this.btItalicClick);
            // 
            // btUnderline
            // 
            this.btUnderline.CheckOnClick = true;
            this.btUnderline.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btUnderline.ImageIndex = 2;
            this.btUnderline.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btUnderline.Name = "btUnderline";
            this.btUnderline.Size = new System.Drawing.Size(23, 22);
            this.btUnderline.Text = "Unterstrichen";
            this.btUnderline.Click += new System.EventHandler(this.btUnderlineClick);
            // 
            // TBXSeparatorItem2
            // 
            this.TBXSeparatorItem2.Name = "TBXSeparatorItem2";
            this.TBXSeparatorItem2.Size = new System.Drawing.Size(6, 25);
            // 
            // btLeft
            // 
            this.btLeft.CheckOnClick = true;
            this.btLeft.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btLeft.ImageIndex = 5;
            this.btLeft.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btLeft.Name = "btLeft";
            this.btLeft.Size = new System.Drawing.Size(23, 22);
            this.btLeft.Text = "Links";
            this.btLeft.Click += new System.EventHandler(this.btLeftClick);
            // 
            // btCenter
            // 
            this.btCenter.CheckOnClick = true;
            this.btCenter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btCenter.ImageIndex = 4;
            this.btCenter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btCenter.Name = "btCenter";
            this.btCenter.Size = new System.Drawing.Size(23, 22);
            this.btCenter.Text = "Zentriert";
            this.btCenter.Click += new System.EventHandler(this.btCenterClick);
            // 
            // btRight
            // 
            this.btRight.CheckOnClick = true;
            this.btRight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btRight.ImageIndex = 3;
            this.btRight.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btRight.Name = "btRight";
            this.btRight.Size = new System.Drawing.Size(23, 22);
            this.btRight.Text = "Rechts";
            this.btRight.Click += new System.EventHandler(this.btRightClick);
            // 
            // btBlock
            // 
            this.btBlock.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btBlock.ImageIndex = 11;
            this.btBlock.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btBlock.Name = "btBlock";
            this.btBlock.Size = new System.Drawing.Size(23, 22);
            this.btBlock.Text = "Block";
            this.btBlock.Click += new System.EventHandler(this.btBlockClick);
            // 
            // TBXSeparatorItem3
            // 
            this.TBXSeparatorItem3.Name = "TBXSeparatorItem3";
            this.TBXSeparatorItem3.Size = new System.Drawing.Size(6, 25);
            // 
            // btIndentInc
            // 
            this.btIndentInc.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btIndentInc.ImageIndex = 7;
            this.btIndentInc.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btIndentInc.Name = "btIndentInc";
            this.btIndentInc.Size = new System.Drawing.Size(23, 22);
            this.btIndentInc.Text = "Einzug erhöhen";
            this.btIndentInc.Click += new System.EventHandler(this.btIndentIncClick);
            // 
            // btIndentDec
            // 
            this.btIndentDec.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btIndentDec.ImageIndex = 6;
            this.btIndentDec.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btIndentDec.Name = "btIndentDec";
            this.btIndentDec.Size = new System.Drawing.Size(23, 22);
            this.btIndentDec.Text = "Einzug senken";
            this.btIndentDec.Click += new System.EventHandler(this.btIndentDecClick);
            // 
            // btCount
            // 
            this.btCount.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btCount.ImageIndex = 10;
            this.btCount.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btCount.Name = "btCount";
            this.btCount.Size = new System.Drawing.Size(23, 22);
            this.btCount.Text = "Auszählung";
            this.btCount.Click += new System.EventHandler(this.btCountClick);
            // 
            // TBXSeparatorItem4
            // 
            this.TBXSeparatorItem4.Name = "TBXSeparatorItem4";
            this.TBXSeparatorItem4.Size = new System.Drawing.Size(6, 25);
            // 
            // btFontColor
            // 
            this.btFontColor.BackColor = System.Drawing.SystemColors.WindowText;
            this.btFontColor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.None;
            this.btFontColor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btFontColor.Name = "btFontColor";
            this.btFontColor.Size = new System.Drawing.Size(23, 22);
            this.btFontColor.Text = "Farbe";
            this.btFontColor.Click += new System.EventHandler(this.btFontColorClick);
            // 
            // ActionList1
            // 
            this.ActionList1.ImageList = this.ImageList1;
            // 
            // ColorDialog
            // 
            this.ColorDialog.FullOpen = true;
            // 
            // TfrmRTFEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.reComment);
            this.Controls.Add(this.TBXDock1);
            this.Name = "TfrmRTFEditor";
            this.Size = new System.Drawing.Size(766, 458);
            this.TBXDock1.ResumeLayout(false);
            this.TBXDock1.PerformLayout();
            this.TBXToolbar1.ResumeLayout(false);
            this.TBXToolbar1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public DelphiClasses.TTBXDock TBXDock1;
        public DelphiClasses.TToolBar TBXToolbar1;
        public System.Windows.Forms.ImageList ImageList1;
        public DelphiClasses.TJvRichEdit reComment;
        public DelphiClasses.TActionList ActionList1;
        public System.Windows.Forms.ToolStripComboBox cbFonts;
        public DelphiClasses.TTBXSpinEditItem seFontsize;
        public System.Windows.Forms.ToolStripSeparator TBXSeparatorItem1;
        public System.Windows.Forms.ToolStripSeparator TBXSeparatorItem2;
        public System.Windows.Forms.ToolStripSeparator TBXSeparatorItem3;
        public System.Windows.Forms.ToolStripSeparator TBXSeparatorItem4;
        public DelphiClasses.TToolBar.TTBXItem btBold;
        public DelphiClasses.TToolBar.TTBXItem btItalic;
        public DelphiClasses.TToolBar.TTBXItem btUnderline;
        public DelphiClasses.TToolBar.TTBXItem btLeft;
        public DelphiClasses.TToolBar.TTBXItem btCenter;
        public DelphiClasses.TToolBar.TTBXItem btRight;
        public DelphiClasses.TToolBar.TTBXItem btBlock;
        public DelphiClasses.TToolBar.TTBXItem btIndentInc;
        public DelphiClasses.TToolBar.TTBXItem btIndentDec;
        public DelphiClasses.TToolBar.TTBXItem btCount;
        public DelphiClasses.TToolBar.TTBXItem btFontColor;
        public System.Windows.Forms.ColorDialog ColorDialog;
    }
}
