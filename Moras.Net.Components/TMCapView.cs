//---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using dxgettext;
using DelphiClasses;

namespace Moras.Net.Components
{
    public class TMCapView : TLabel, ISupportInitialize
    {
        private static string _(string szMsgId) { return TGnuGettextInstance.gettext(szMsgId); }

        private static DynamicArray<Color> arColors;
        private static string sFormat = "%W%(%C-w%|%M-c%)";
        /* Würde gerne die Utils einbinden, aber die Querverweise aller includes
           sind nicht hierarchisch bisher...
           Daher erstmal manuell die Routinen einbinden!
        */

        private int iCapBase;		// Basis-Cap  = (Level + CapAdd) * CapMult
        private int iCapIncCap;  	// Limit für Cap-Inc, nur durch Overcap überschreitbar
        private int iCapIncOvercap; // Limit für Over-Cap-Inc
        // CapCap = (Level + CCapAdd) * CCapMult + CCapInc

        private int iFloor;			// Der Basiswert. Ist eigentlich immer 0, ausser bei den Rassenboni bei den Resistenzen
        // Es wird der Wert und das Cap um diesen Wert erhöht
        private int iValue;			// Der aktuelle Wert
        private int iValueChange;	// Um soviel wird Wert durch das Item geändert
        private int iCapInc;		// Caperhöhung durch Items
        private int iCapIncChange; 	// Um soviel wird das Cap durch ein Item geändert
        private int iOvercap;		// Über-Caperhöhung durch Items
        private int iOvercapChange;	// Um soviel wird das Über-Cap durch ein Item geändert

        private int iData;			// Andere Daten zu der Anzeige (In dem Fall die Attribute-ID
        private bool bMouseOver;		// Wird immer gesetzt, wenn die Maus über dem Control ist
        private bool bCapViewType; 	// Wenn true, dann wird der gecapte Wert statt ungecaptem angezeigt

        #region Properties
        private SolidBrush _brush;
        private SolidBrush Brush { get { return _brush ?? (_brush = new SolidBrush(Color.Empty)); } }

        [DefaultValue(11)]
        public int CapBase { get { return iCapBase; } set { SetCapBase(value); } }
        [DefaultValue(0)]
        public int CapIncCap { get { return iCapIncCap; } set { SetCapIncCap(value); } }
        [DefaultValue(0)]
        public int CapIncOvercap { get { return iCapIncOvercap; } set { SetCapIncOvercap(value); } }

        [DefaultValue(0)]
        public int Floor { get { return iFloor; } set { SetFloor(value); } }
        [DefaultValue(0)]
        public int Value { get { return iValue; } set { SetValue(value); } }
        [DefaultValue(0)]
        public int ValueChange { get { return iValueChange; } set { SetValueChange(value); } }
        [DefaultValue(0)]
        public int CapInc { get { return iCapInc; } set { SetCapInc(value); } }
        [DefaultValue(0)]
        public int CapIncChange { get { return iCapIncChange; } set { SetCapIncChange(value); } }
        [DefaultValue(0)]
        public int Overcap { get { return iOvercap; } set { SetOvercap(value); } }
        [DefaultValue(0)]
        public int OvercapChange { get { return iOvercapChange; } set { SetOvercapChange(value); } }

        [DefaultValue(0)]
        public int Data { get { return iData; } set { iData = value; } }
        [DefaultValue(false)]
        public bool MouseOver { get { return bMouseOver; } set { SetMouseOver(value); } }
        [DefaultValue(false)]
        public bool CapViewType { get { return bCapViewType; } set { SetCapViewType(value); } }
        #endregion

        //---------------------------------------------------------------------------
        public static void UpdateFormat()
        {
            sFormat = Utils.GetRegistryString("StatDisplayFormula", "%W%(%C-w%|%M-c%)");
        }
        //---------------------------------------------------------------------------
        public static void UpdateColors()
        {
            arColors.Length = 10;
            arColors[0] = Utils.Int2Color(Utils.GetRegistryInteger("StatDisplayFloorCol", Utils.Color2Int(Color.White)));
            arColors[1] = Utils.Int2Color(Utils.GetRegistryInteger("StatDisplayCapCol", Utils.Color2Int(Color.Gray)));
            arColors[2] = Utils.Int2Color(Utils.GetRegistryInteger("StatDisplayCapIncCol", 0x00aa0000));
            arColors[3] = Utils.Int2Color(Utils.GetRegistryInteger("StatDisplayCapSubCol", 0x000000cc));
            arColors[4] = Utils.Int2Color(Utils.GetRegistryInteger("StatDisplayCapAddCol", 0x0000cc00));
            arColors[5] = Utils.Int2Color(Utils.GetRegistryInteger("StatDisplayValueCol", Utils.Color2Int(Color.Gray)));
            arColors[6] = Utils.Int2Color(Utils.GetRegistryInteger("StatDisplayValueSubCol", 0x006666dd));
            arColors[7] = Utils.Int2Color(Utils.GetRegistryInteger("StatDisplayValueAddCol", 0x0066dd66));
            arColors[8] = Utils.Int2Color(Utils.GetRegistryInteger("StatDisplayTextCol", Utils.Color2Int(SystemColors.ControlText)));
            arColors[9] = Utils.Int2Color(Utils.GetRegistryInteger("StatDisplayTextMouseOverColCol", Utils.Color2Int(Color.Blue)));
        }
        //---------------------------------------------------------------------------
        public static void SetColor(int i, Color col)
        {
            if (arColors.Length != 10)
                UpdateColors();

            if (i >= 0 && i < 9)
                arColors[i] = col;
        }
        //---------------------------------------------------------------------------
        public static void SetFormatStr(string newstr)
        {
            sFormat = newstr;
        }
        //---------------------------------------------------------------------------
        public TMCapView()
        {
            if (arColors.Length != 10)
            {
                UpdateColors();
            }
            BackColor = Color.Transparent;
            // Diese beiden Werte sind die Werte für Fertigkeiten
            iCapBase = 11;
            iCapIncCap = 0;
            iCapIncOvercap = 0;

            iFloor = 0;
            iValue = 0;
            iValueChange = 0;
            iCapInc = 0;
            iCapIncChange = 0;
            iOvercap = 0;
            iOvercapChange = 0;

            iData = 0;
            bMouseOver = false;
            bCapViewType = false;
            //ShowHint = true;
        }

        public TMCapView(IContainer container)
            : this()
        {
            container.Add(this);
        }
        //---------------------------------------------------------------------------
        protected override void OnPaint(PaintEventArgs e)
        {
            string strOut, strVal;

            // Berechne die einzelnen Maximas
            int iMaxVal = iValue + Math.Max(iValueChange, 0);
            //int iMaxCap = iCapBase + iCapInc + ((iCapIncChange > 0) ? iCapIncChange : 0) + iOvercap + ((iOvercapChange > 0) ? iOvercapChange : 0);
            int iMaxCapCap = iCapBase + iCapIncCap + iCapIncOvercap;
            // Suche den größten vorkommenden Anzeigewert:
            /*
            int iMaxMax = (iMaxVal > iMaxCap) ? iMaxVal : iMaxCap;
            iMaxMax = iFloor + ( (iMaxCapCap > iMaxMax) ? iMaxCapCap : iMaxMax );
            */
            int iMaxMax = (iMaxVal > iMaxCapCap) ? iMaxVal : iMaxCapCap;

            // Zwischenwert. Breite eines einzelnen Wertes skaliert um 4096
            int iCapT = (iMaxMax <= 0) ? 0 : Width * 4096 / iMaxMax;

            // Tatsächliche Caperhöhung und tatsächliche Änderunge von diesem
            int iRealCapInc = Math.Min(iCapIncCap, iCapInc) + Math.Min(iCapIncOvercap, iOvercap);
            int iRealCapIncChange = Math.Min(iCapIncCap, iCapInc + iCapIncChange) + Math.Min(iCapIncOvercap, iOvercap + iOvercapChange);
            iRealCapIncChange -= iRealCapInc;

            // Startwert für Balken
            int iBarPos = 0, iBarViewPos = 0;
            // Anzeigepositionen
            int x;


            // Hintergrund löschen
            //Brush.Color = SystemColors.ButtonFace;
            //e.Graphics.FillRectangle(Brush, new Rectangle(0, 0, Width, Height));


            // Ab hier werden die Balken zusammengebaut


            // Zuerst den Cap-Balken

            // Gibt es einen Floor, dann beide Balken mit diesem Wert beginnen
            // Cap-Balken: Weiß
            if (iFloor > 0)
            {
                x = iBarPos + iFloor;
                Brush.Color = arColors[0];
                e.Graphics.FillRectangle(Brush, Rectangle.FromLTRB(iBarViewPos, Height - 3, x * iCapT / 4096, Height));
                iBarPos = x;
                iBarViewPos = iBarPos * iCapT / 4096;
            }

            // Der Capwert selbst (Grau)
            Brush.Color = arColors[1];
            x = iBarPos + iCapBase;
            e.Graphics.FillRectangle(Brush, Rectangle.FromLTRB(iBarViewPos, Height - 3, x * iCapT / 4096, Height));
            iBarPos = x;
            iBarViewPos = iBarPos * iCapT / 4096;

            // Der Cap-Inc-wert (Blau)
            Brush.Color = arColors[2];
            x = (iBarPos + iRealCapInc + ((iRealCapIncChange < 0) ? iRealCapIncChange : 0));
            e.Graphics.FillRectangle(Brush, Rectangle.FromLTRB(iBarViewPos, Height - 3, x * iCapT / 4096, Height));
            iBarPos = x;
            iBarViewPos = iBarPos * iCapT / 4096;

            // Die Cap-Differenz (Rot/Grün)
            if (iRealCapIncChange < 0)
            {
                Brush.Color = arColors[3];	// Es geht was verloren, also rot
                x = iBarPos - iRealCapIncChange;
            }
            else if (iRealCapIncChange > 0)
            {
                Brush.Color = arColors[4];	// Wir bekommen was dazu, also grün
                x = iBarPos + iRealCapIncChange;
            }
            if (x != iBarPos)
            {
                e.Graphics.FillRectangle(Brush, Rectangle.FromLTRB(iBarViewPos, Height - 3, x * iCapT / 4096, Height));
                //iBarPos = x;
                //iBarViewPos = iBarPos * iCapT / 4096;
            }


            // Nun den eigentlichen Wert-Balken
            iBarPos = 0;
            iBarViewPos = 0;

            // Der Wert (Grau)
            Brush.Color = arColors[5];
            x = iBarPos + iFloor + iValue + ((iValueChange < 0) ? iValueChange : 0);
            e.Graphics.FillRectangle(Brush, Rectangle.FromLTRB(iBarViewPos, 0, x * iCapT / 4096, Height - 3));
            iBarPos = x;
            iBarViewPos = iBarPos * iCapT / 4096;

            //  Die Differenz (Rot/Grün)
            if (iValueChange < 0)
            {
                Brush.Color = arColors[6];	// Es geht was verloren, also rot
                x = iBarPos - iValueChange;
            }
            else if (iValueChange > 0)
            {
                Brush.Color = arColors[7];	// Wir bekommen was dazu, also grün
                x = iBarPos + iValueChange;
            }
            if (x != iBarPos)
            {
                e.Graphics.FillRectangle(Brush, Rectangle.FromLTRB(iBarViewPos, 0, x * iCapT / 4096, Height - 3));
                //iBarPos = x;
                //iBarViewPos = iBarPos * iCapT / 4096;
            }


            // Nun die Texte
            Color backColor = Color.Transparent;	// Font soll durchsichtig sein
            TextFormatFlags flags = TextFormatFlags.NoPadding |
                TextFormatFlags.PreserveGraphicsClipping |
                TextFormatFlags.PreserveGraphicsTranslateTransform;
            Font font = Font;
            Color fontColor = Color.Empty;

            // Anezigetext, links
            strOut = Text + ":";

            // Ausgabe des Strings mit den Zahlenwerten
            if (iCapInc + iCapIncChange + iOvercap + iOvercapChange > iRealCapInc + iRealCapIncChange)
            {
                strVal = string.Format("{0}({1}|{2})", iFloor + iValue + iValueChange, (iCapBase + iRealCapInc + iRealCapIncChange) - (iValue + iValueChange), iRealCapInc + iRealCapIncChange - (iCapInc + iCapIncChange + iOvercap + iOvercapChange));
            }
            else
            {
                strVal = string.Format("{0}({1})", iFloor + iValue + iValueChange, (iCapBase + iRealCapInc + iRealCapIncChange) - (iValue + iValueChange));
            }
            int ValLen = TextRenderer.MeasureText(e.Graphics, strVal, font, Size.Empty, flags).Width;	// So lang ist der String in Pixel
            int OutLen = TextRenderer.MeasureText(e.Graphics, strOut, font, Size.Empty, flags).Width;

            if (bMouseOver)
                fontColor = arColors[9];
            else
                fontColor = arColors[8];

            // Passt der String vollständig in die Anzeige?
            if (OutLen >= Width - ValLen - 4)
            {	// Anzeigetext kürzen und ... anhängen
                int l = strOut.Length - 3;	// mindestens um 3 Zeichen kürzen
                int ellipsisLen = TextRenderer.MeasureText(e.Graphics, "...", font, Size.Empty, flags).Width;
                while (l > 0)
                {
                    OutLen = TextRenderer.MeasureText(e.Graphics, strOut.Substring(0, l), font, Size.Empty, flags).Width;
                    if (OutLen + ellipsisLen < Width - ValLen - 4)
                        break;
                    l--;
                }
                strOut = strOut.Substring(0, l) + "...";
            }
            TextRenderer.DrawText(e.Graphics, strOut, font, Point.Empty, fontColor, backColor, flags);
            if (bMouseOver)
            {
                fontColor = arColors[9];
                TextRenderer.DrawText(e.Graphics, strVal, font, new Point(Width - ValLen, 0), fontColor, backColor, flags);
            }
            else
            {
                /*
                if ((iCapBase + iRealCapInc + iRealCapIncChange) - (iValue + iValueChange) < 0)
                {
                    AnsiString	strVal1, strVal2, strVal3, strVal4, strVal5;
                    int         ValLen1, ValLen2, ValLen3, ValLen4, ValLen5;

                    strVal1.sprintf("%d(", iValue + iValueChange + iFloor);
                    strVal2.sprintf("%d", (iCapBase + iRealCapInc + iRealCapIncChange) - (iValue + iValueChange));
                    strVal3.sprintf(")");
                    strVal4.sprintf("|");
                    strVal5.sprintf("%d", iRealCapInc + iRealCapIncChange - (iCapInc + iCapIncChange));
                    if (iRealCapInc + iRealCapIncChange - (iCapInc + iCapIncChange)<0) {
                        int pos = Width - ValLen;
                        ValLen1 = Canvas->TextWidth(strVal1);	// So lang ist der vordere String in Pixel
                        ValLen2 = Canvas->TextWidth(strVal2);	// So lang ist der hintere String in Pixel
                        ValLen4 = Canvas->TextWidth(strVal4);	// So lang ist der hintere String in Pixel
                        ValLen5 = Canvas->TextWidth(strVal5);	// So lang ist der hintere String in Pixel
                        Canvas->Font->Color = clBtnText;
                        Canvas->TextOutA(pos, 0, strVal1);
                        pos += ValLen1;

                        Canvas->Font->Color = (TColor)0x000000bb;
                        Canvas->TextOutA(pos, 0, strVal2);
                        pos += ValLen2;

                        Canvas->Font->Color = clBtnText;
                        Canvas->TextOutA(pos, 0, strVal4);
                        pos += ValLen4;

                        Canvas->Font->Color = (TColor)0x000000bb;
                        Canvas->TextOutA(pos, 0, strVal5);
                        pos += ValLen5;

                        Canvas->Font->Color = clBtnText;
                        Canvas->TextOutA(pos, 0, strVal3);
                    }
                    else
                    {
                        ValLen1 = Canvas->TextWidth(strVal1);	// So lang ist der vordere String in Pixel
                        ValLen3 = Canvas->TextWidth(strVal3);	// So lang ist der hintere String in Pixel
                        Canvas->Font->Color = clBtnText;
                        Canvas->TextOutA(Width - ValLen, 0, strVal1);
                        Canvas->Font->Color = (TColor)0x000000bb;
                        Canvas->TextOutA(Width - ValLen + ValLen1, 0, strVal2);
                        Canvas->Font->Color = clBtnText;
                        Canvas->TextOutA(Width - ValLen3, 0, strVal3);
                    }
                }
                else
                */
                {
                    fontColor = arColors[8];
                    TextRenderer.DrawText(e.Graphics, strVal, font, new Point(Width - ValLen, 0), fontColor, backColor, flags);
                }
            }
        }

        private void BuildHint()
        {
            // Bastele den Hint zusammen
            StringBuilder hintBuilder = new StringBuilder();
            int iCapEff = iCapBase + Math.Min(iCapInc, iCapIncCap) + Math.Min(iOvercap, iCapIncOvercap);
            int iValueEff = iFloor + Math.Min(iValue, iCapEff);

            hintBuilder.AppendFormat("{0}: {1}\n" +
                "{2} = {3}\n" +
                "  +{4} {5}\n", Text, iValueEff, _("Wert"), (iValue + iFloor), iValue, _("(Ausrüstung)"));
            if (iFloor > 0) hintBuilder.AppendFormat("  +{0} {1}\n", iFloor, _("(Rassenboni)"));
            hintBuilder.AppendFormat("\nCap: {0}\n", (iFloor + iCapEff));
            hintBuilder.AppendFormat("{0} = {1}\n", _("Wert"), (iFloor + iCapBase + iCapInc + iOvercap));
            hintBuilder.AppendFormat("  +{0} {1}", iCapBase, _("(Stufe)"));
            if (iCapInc > 0) hintBuilder.AppendFormat("\n  +{0} {1}", (iCapInc), _("(Ausrüstung)"));
            if (iOvercap > 0) hintBuilder.AppendFormat("\n  +{0} {1}", (iOvercap), _("(Myth. Caperhöhung)"));
            if (iFloor > 0) hintBuilder.AppendFormat("\n  +{0} {1}", (iFloor), _("(Rassenboni)"));

            this.SetHint(hintBuilder.Replace("\n", Environment.NewLine).ToString());
        }

        private void SetCapBase(int CapBase)
        {
            // Nur setzten und neu zeichnen wenn die Wert anders sind. Sollte das Zeichnen etwas beschleunigen
            if (iCapBase != CapBase)
            {
                iCapBase = CapBase;
                BuildHint();
                Invalidate();
            }
        }

        private void SetCapIncCap(int CapIncCap)
        {
            // Nur setzten und neu zeichnen wenn die Wert anders sind. Sollte das Zeichnen etwas beschleunigen
            if (iCapIncCap != CapIncCap)
            {
                iCapIncCap = CapIncCap;
                BuildHint();
                Invalidate();
            }
        }

        private void SetCapIncOvercap(int CapIncOvercap)
        {
            // Nur setzten und neu zeichnen wenn die Wert anders sind. Sollte das Zeichnen etwas beschleunigen
            if (iCapIncOvercap != CapIncOvercap)
            {
                iCapIncOvercap = CapIncOvercap;
                BuildHint();
                Invalidate();
            }
        }

        private void SetFloor(int Value)
        {
            // Nur setzten und neu zeichnen wenn die Wert anders sind. Sollte das Zeichnen etwas beschleunigen
            if (iFloor != Value)
            {
                iFloor = Value;
                BuildHint();
                Invalidate();
            }
        }

        private void SetValue(int Value)
        {
            // Nur setzten und neu zeichnen wenn die Wert anders sind. Sollte das Zeichnen etwas beschleunigen
            if (iValue != Value)
            {
                iValue = Value;
                BuildHint();
                Invalidate();
            }
        }

        private void SetValueChange(int Change)
        {
            // Nur setzten und neu zeichnen wenn die Wert anders sind. Sollte das Zeichnen etwas beschleunigen
            if (iValueChange != Change)
            {
                iValueChange = Change;
                Invalidate();
            }
        }

        private void SetCapInc(int Value)
        {
            // Nur setzten und neu zeichnen wenn die Wert anders sind. Sollte das Zeichnen etwas beschleunigen
            if (iCapInc != Value)
            {
                iCapInc = Value;
                BuildHint();
                Invalidate();
            }
        }

        private void SetCapIncChange(int Change)
        {
            // Nur setzten und neu zeichnen wenn die Wert anders sind. Sollte das Zeichnen etwas beschleunigen
            if (iCapIncChange != Change)
            {
                iCapIncChange = Change;
                Invalidate();
            }
        }

        private void SetOvercap(int Overcap)
        {
            // Nur setzten und neu zeichnen wenn die Wert anders sind. Sollte das Zeichnen etwas beschleunigen
            if (iOvercap != Overcap)
            {
                iOvercap = Overcap;
                BuildHint();
                Invalidate();
            }
        }

        private void SetOvercapChange(int Change)
        {
            // Nur setzten und neu zeichnen wenn die Wert anders sind. Sollte das Zeichnen etwas beschleunigen
            if (iOvercapChange != Change)
            {
                iOvercapChange = Change;
                Invalidate();
            }
        }

        private void SetMouseOver(bool MouseOver)
        {
            bMouseOver = MouseOver;
            BuildHint();
            Invalidate();
        }

        private void SetCapViewType(bool CapViewType)
        {
            bCapViewType = CapViewType;
            Invalidate();
        }

        #region ISupportInitialize Members

        void ISupportInitialize.BeginInit()
        {
        }

        void ISupportInitialize.EndInit()
        {
            BuildHint();
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
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
