//---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DelphiClasses;
using System.Drawing.Drawing2D;

namespace Moras.Net.Components
{
    [ToolboxItem(true)]
    public class TMItemSlot : TLabel
    {
        private bool bActivated;	// Soll Item als Aktiviert dargestellt werden?
        private int iUsedSlots;	// Wieviele Effekt-Slots sind belegt
        private int iAvailIP;	// Wieviele IPs sind vorhanden?
        // Achtung, negative Zahlen sind Spezialwerte
        // -1 = normaler Drop
        // -2 = unique Drop
        // -3 = Artefakt
        private int iUsedIP;	// Wieviel davon benutzt?

        private static readonly Padding defaultPadding = new Padding(2);
        private Color m_backColor = SystemColors.ButtonFace;

        #region Properties
        protected override Padding DefaultPadding { get { return defaultPadding; } }
        // BackColor wird durch UpdateBackground() immer überschrieben, also muss es nicht im Designer-Code hinterlegt werden
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color BackColor { get { return m_backColor; } set { m_backColor = value; } }
        private Pen _pen;
        private Pen Pen { get { return _pen ?? (_pen = new Pen(Color.Black, 1) { Alignment = PenAlignment.Center }); } }
        private SolidBrush _brush;
        private SolidBrush Brush { get { return _brush ?? (_brush = new SolidBrush(Color.Empty)); } }

        public override string Text { get { return base.Text; } set { SetText(value); } }
        [DefaultValue(false)]
        public bool Activated { get { return bActivated; } set { SetActivated(value); } }
        [DefaultValue(0)]
        public int UsedSlots { get { return iUsedSlots; } set { SetUSlots(value); } }
        [DefaultValue(0)]
        public int AvailIP { get { return iAvailIP; } set { SetAvailIP(value); } }
        [DefaultValue(0)]
        public int UsedIP { get { return iUsedIP; } set { SetUsedIP(value); } }
        #endregion
        //---------------------------------------------------------------------------
        public TMItemSlot()
        {
            AutoSize = false;
            bActivated = false;
            iUsedSlots = 0;
            //ShowHint = true;
        }

        public TMItemSlot(IContainer container)
            : this()
        {
            container.Add(this);
        }

        /*~TMItemSlot()
        {
        }*/
        //---------------------------------------------------------------------------

        private void UpdateBackground()
        {
            if (iAvailIP == -1)	// Dropgegenstand, keine IPs möglich -> Blue-Cyan
                BackColor = Utils.Int2Color(0xff8000);
            else if (iAvailIP == -2)	// Unique Drop
                BackColor = Utils.Int2Color(0xff8080);
            else if (iAvailIP == -3)	// Artefakt
                BackColor = Utils.Int2Color(0xff0080);
            else if (iAvailIP == 0)	// Noch keine gültigen Daten, leer -> ButtonFace
                BackColor = SystemColors.ButtonFace;
            else
            {
                if (iAvailIP < iUsedIP)
                {
                    int ol = 5 - (iUsedIP - iAvailIP);
                    BackColor = Utils.Int2Color((int)(((255 * ol / 5) << 8) + 255));
                }
                else
                    BackColor = Utils.Int2Color((int)(0xff00 + 255 * iUsedIP / iAvailIP));
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle border = ClientRectangle;
            border.Size -= new Size(1, 1); //HACK: kompensiere Pixelfehler bei WinForms
            e.Graphics.DrawRectangle(Pen, border);
            if (iUsedSlots > 0)
            {
                Rectangle slotsrect = new Rectangle(0, 0, ClientRectangle.Width, 10);
                slotsrect = Extensions.DeflateRect(slotsrect, Padding);
                slotsrect.Offset(0, ClientRectangle.Height - 10);
                slotsrect.Width = slotsrect.Width * iUsedSlots / 4;
                //Rectangle.FromLTRB(Padding.Left, Height - 10 + Padding.Top, (Width - Padding.Right) * iUsedSlots / 4, Height - Padding.Bottom);
                Brush.Color = Utils.Int2Color(0x606060);
                e.Graphics.FillRectangle(Brush, slotsrect);
            }
            int nl = Text.IndexOf(Environment.NewLine);
            if (nl == -1)
            {	// gibt kein Newline im string, also String einfach ausgeben
                base.OnPaint(e);
            }
            else
            {	// Es gibt ein Newline. In 2 einzelnen Zeilen ausgeben
                string oldtext = Text;
                try
                {
                    base.Text = oldtext.Substring(nl + Environment.NewLine.Length, Math.Min(10, oldtext.Length - nl - Environment.NewLine.Length));	// maximal 10 Zeichen
                    e.Graphics.TranslateTransform(0, 10);
                    base.OnPaint(e);
                    e.Graphics.ResetTransform();
                    e.Graphics.TranslateTransform(0, -1);
                    base.Text = oldtext.Substring(0, nl);
                    base.OnPaint(e);
                }
                finally
                {
                    e.Graphics.ResetTransform();
                    base.Text = oldtext;
                }
            }
        }

        private void SetActivated(bool Activate)
        {
            if (bActivated != Activate)
            {
                bActivated = Activate;
                if (bActivated)
                {
                    Pen.Color = Color.Red;
                    Pen.Width = 3;
                }
                else
                {
                    Pen.Color = Color.Black;
                    Pen.Width = 1;
                }
                Invalidate();
            }
        }

        private void SetText(string Text)
        {
            if (base.Text != Text)
            {
                base.Text = Text;
                Invalidate();
            }
        }

        private void SetUSlots(int UsedSlots)
        {
            UsedSlots = UsedSlots.Clamp(0, 4);
            if (iUsedSlots != UsedSlots)
            {
                iUsedSlots = UsedSlots;
                Invalidate();
            }
        }

        private void SetAvailIP(int AvailIP)
        {
            if (iAvailIP != AvailIP)
            {
                iAvailIP = AvailIP;
                UpdateBackground();
                Invalidate();
            }
        }

        private void SetUsedIP(int UsedIP)
        {
            if (iUsedIP != UsedIP)
            {
                iUsedIP = UsedIP;
                UpdateBackground();
                Invalidate();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_pen != null)
                {
                    _pen.Dispose();
                    _pen = null;
                }
                if (_brush != null)
                {
                    _brush.Dispose();
                    _brush = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
