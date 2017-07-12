//---------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Text;
using DelphiClasses;

//---------------------------------------------------------------------------
namespace Moras.Net
{
    public partial class TfrmRTFEditor : TFrame
    {
        //---------------------------------------------------------------------------
        public TfrmRTFEditor()
        {
            InitializeComponent();
            TabStop = true;

            using (var fonts = new InstalledFontCollection())
            {
                cbFonts.Items.AddRange(fonts.Families);
            }
        }
        //---------------------------------------------------------------------------
        public void reCommentChange(object sender, EventArgs e)
        {
            btBold.Checked = reComment.SelectionFont.Bold;
            btItalic.Checked = reComment.SelectionFont.Italic;
            btUnderline.Checked = reComment.SelectionFont.Underline;
            btFontColor.BackColor = reComment.SelectionColor;
            cbFonts.SelectedIndex = cbFonts.Items.IndexOf(reComment.SelectionFont.FontFamily);
            seFontsize.Value = (decimal)reComment.SelectionFont.Size;
            switch (reComment.SelectionAlignment)
            {
                case TextAlignment.Right: btRight.Checked = true;
                    break;
                case TextAlignment.Center: btCenter.Checked = true;
                    break;
                case TextAlignment.Justify: btBlock.Checked = true;
                    break;
                case TextAlignment.Left:
                default: btLeft.Checked = true;
                    break;
            }

        }
        //---------------------------------------------------------------------------
        public void cbFontsChange(object sender, EventArgs e)
        {
            reComment.SelectionFont = reComment.SelectionFont.CloneWithFamily((FontFamily)cbFonts.SelectedItem);
        }
        //---------------------------------------------------------------------------
        public void btBoldClick(object sender, EventArgs e)
        {
            reComment.SelectionFont = reComment.SelectionFont.CloneWithBold(btBold.Checked);
        }
        //---------------------------------------------------------------------------
        public void btItalicClick(object sender, EventArgs e)
        {
            reComment.SelectionFont = reComment.SelectionFont.CloneWithItalic(btItalic.Checked);
        }
        //---------------------------------------------------------------------------
        public void btUnderlineClick(object sender, EventArgs e)
        {
            reComment.SelectionFont = reComment.SelectionFont.CloneWithUnderline(btUnderline.Checked);
        }
        //---------------------------------------------------------------------------
        public void btFontColorClick(object sender, EventArgs e)
        {
            ColorDialog.Color = reComment.SelectionColor;
            if (ColorDialog.ShowDialog() == DialogResult.OK)
                reComment.SelectionColor = ColorDialog.Color;
        }
        //---------------------------------------------------------------------------
        public void btLeftClick(object sender, EventArgs e)
        {
            reComment.SelectionAlignment = TextAlignment.Left;
        }
        //---------------------------------------------------------------------------
        public void btCenterClick(object sender, EventArgs e)
        {
            reComment.SelectionAlignment = TextAlignment.Center;
        }
        //---------------------------------------------------------------------------
        public void btRightClick(object sender, EventArgs e)
        {
            reComment.SelectionAlignment = TextAlignment.Right;
        }
        //---------------------------------------------------------------------------
        public void seFontsizeValueChange(object sender, EventArgs e)
        {
            reComment.SelectionFont = reComment.SelectionFont.CloneWithSize((float)seFontsize.Value);
        }
        //---------------------------------------------------------------------------
        public void btIndentIncClick(object sender, EventArgs e)
        {
            reComment.SelectionHangingIndent++;
        }
        //---------------------------------------------------------------------------
        public void btIndentDecClick(object sender, EventArgs e)
        {
            reComment.SelectionHangingIndent--;
        }
        //---------------------------------------------------------------------------
        public void btCountClick(object sender, EventArgs e)
        {
            reComment.SelectionBullet = !reComment.SelectionBullet;
        }
        //---------------------------------------------------------------------------
        public void btBlockClick(object sender, EventArgs e)
        {
            reComment.SelectionAlignment = TextAlignment.Justify;
        }
    }

    static partial class Unit
    {
        internal static TfrmRTFEditor frmRTFEditor; /* TFrame: File Type */
    }
}
