using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace DelphiClasses
{
    public enum TextAlignment
    {
        Left,
        Right,
        Center,
        Justify
    }

    public class TJvRichEdit : RichTextBox
    {
        private const int MAX_TAB_STOPS = 32;
        private const int PFM_ALIGNMENT = 0x00000008;
        private const int WM_USER = 0x0400;
        private const int EM_GETPARAFORMAT = (WM_USER + 61);
        private const int EM_SETPARAFORMAT = (WM_USER + 71);
        private const int EM_SETTYPOGRAPHYOPTIONS = (WM_USER + 202);
        private const int TO_ADVANCEDTYPOGRAPHY = 1;
        private const int TO_SIMPLELINEBREAK = 2;
        private const int TO_DISABLECUSTOMTEXTOUT = 4;
        private const int TO_ADVANCEDLAYOUT = 8;

        private enum PFA : short
        {
            // PARAFORMAT alignment options 
            LEFT = 1,
            RIGHT = 2,
            CENTER = 3,

            // PARAFORMAT2 alignment options 
            JUSTIFY = 4,	// New paragraph-alignment option 2.0 (*) 
            FULL_INTERWORD = 4,	// These are supported in 3.0 with advanced
            FULL_INTERLETTER = 5,	//  typography enabled
            FULL_SCALED = 6,
            FULL_GLYPHS = 7,
            SNAP_GRID = 8
        }

        [StructLayout(LayoutKind.Sequential)]
        private class PARAFORMAT
        {
            public int cbSize;
            public int dwMask;
            public short wNumbering;
            public short wReserved;
            public int dxStartIndent;
            public int dxRightIndent;
            public int dxOffset;
            public short wAlignment;
            public short cTabCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_TAB_STOPS)]
            public int[] rgxTabs = new int[MAX_TAB_STOPS];

            public PARAFORMAT()
            {
                cbSize = Marshal.SizeOf(this.GetType());
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private class PARAFORMAT2 : PARAFORMAT
        {
            public int dySpaceBefore;			// Vertical spacing before para
            public int dySpaceAfter;			// Vertical spacing after para
            public int dyLineSpacing;			// Line spacing depending on Rule
            public short sStyle;				// Style handle
            public byte bLineSpacingRule;		// Rule for line spacing (see tom.doc)
            public byte bOutlineLevel;			// Outline level
            public short wShadingWeight;		// Shading in hundredths of a per cent
            public short wShadingStyle;		    // Nibble 0: style, 1: cfpat, 2: cbpat
            public short wNumberingStart;		// Starting value for numbering
            public short wNumberingStyle;		// Alignment, roman/arabic, (), ), ., etc.
            public short wNumberingTab;		    // Space bet FirstIndent & 1st-line text
            public short wBorderSpace;			// Border-text spaces (nbl/bdr in pts)
            public short wBorderWidth;			// Pen widths (nbl/bdr in half pts)
            public short wBorders;				// Border styles (nibble/border)
        };

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(HandleRef hWnd, int msg, int wParam, int lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, [In, Out, MarshalAs(UnmanagedType.LPStruct)] PARAFORMAT lParam);

        private void ForceHandleCreate()
        {
            if (!base.IsHandleCreated)
            {
                this.CreateHandle();
            }
        }

        private static bool IsEnumValid(Enum enumValue, int value, int minValue, int maxValue)
        {
            return ((value >= minValue) && (value <= maxValue));
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // Enable support for justification.
            SendMessage(new HandleRef(this, Handle),
                         EM_SETTYPOGRAPHYOPTIONS,
                         TO_ADVANCEDTYPOGRAPHY,
                         TO_ADVANCEDTYPOGRAPHY);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new TextAlignment SelectionAlignment
        {
            get
            {
                ForceHandleCreate();
                PARAFORMAT lParam = new PARAFORMAT2();
                SendMessage(new HandleRef(this, Handle), EM_GETPARAFORMAT, 0x0, lParam);
                if ((PFM_ALIGNMENT & lParam.dwMask) != 0x0)
                {
                    switch ((PFA)lParam.wAlignment)
                    {
                        case PFA.LEFT:
                            return TextAlignment.Left;
                        case PFA.RIGHT:
                            return TextAlignment.Right;
                        case PFA.CENTER:
                            return TextAlignment.Center;
                        case PFA.JUSTIFY:
                            return TextAlignment.Justify;
                    }
                }
                return TextAlignment.Left;
            }
            set
            {
                if (!IsEnumValid(value, (int)value, (int)TextAlignment.Left, (int)TextAlignment.Justify))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(TextAlignment));
                }
                this.ForceHandleCreate();
                PARAFORMAT lParam = new PARAFORMAT2
                {
                    dwMask = PFM_ALIGNMENT
                };
                switch (value)
                {
                    case TextAlignment.Left:
                        lParam.wAlignment = (short)PFA.LEFT;
                        break;
                    case TextAlignment.Right:
                        lParam.wAlignment = (short)PFA.RIGHT;
                        break;
                    case TextAlignment.Center:
                        lParam.wAlignment = (short)PFA.CENTER;
                        break;
                    case TextAlignment.Justify:
                        lParam.wAlignment = (short)PFA.JUSTIFY;
                        break;
                }
                SendMessage(new HandleRef(this, Handle), EM_SETPARAFORMAT, 0x0, lParam);
            }
        }
    }
}
