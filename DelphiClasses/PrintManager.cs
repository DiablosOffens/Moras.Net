using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Drawing;

namespace DelphiClasses
{
    public static class PrintManager
    {
        private static Dictionary<Control, PrintDocument> s_printDocs = new Dictionary<Control, PrintDocument>();
        private static object s_printDocsLock = new object();

        public static PrintDocument GetPrintDocument(this Control ctl)
        {
            PrintDocument doc;
            if (s_printDocs.TryGetValue(ctl, out doc))
                return doc;
            lock (s_printDocsLock)
            {
                if (s_printDocs.TryGetValue(ctl, out doc))
                    return doc;
                doc = CreatePrintDocument(ctl);
                try
                {
                    s_printDocs.Add(ctl, doc);
                }
                finally
                {
                    ctl.Disposed += new EventHandler(ctl_Disposed);
                }
            }
            return doc;
        }

        private static void ctl_Disposed(object sender, EventArgs e)
        {
            Control ctl = (Control)sender;
            PrintDocument doc = null;
            lock (s_printDocsLock)
            {
                if (s_printDocs.TryGetValue(ctl, out doc))
                {
                    s_printDocs.Remove(ctl);
                }
            }
            if (doc != null)
                doc.Dispose();
        }

        private static PrintDocument CreatePrintDocument(Control ctl)
        {
            if (ctl is RichTextBox)
            {
                return new RichTextDocument((RichTextBox)ctl);
            }
            //TODO: implement more
            return new PrintDocument();
        }

        public static void Print(this PrintDocument doc, string title)
        {
            string oldtitle = doc.DocumentName;
            try
            {
                doc.DocumentName = title;
                doc.Print();
            }
            finally
            {
                doc.DocumentName = oldtitle;
            }
        }

        public static void Preview(this PrintDocument doc, string title)
        {
            string oldtitle = doc.DocumentName;
            try
            {
                doc.DocumentName = title;
                using (PrintPreviewDialog dlg = new PrintPreviewDialog())
                {
                    dlg.Text += " " + title;
                    dlg.Document = doc;
                    dlg.ShowDialog();
                }
            }
            finally
            {
                doc.DocumentName = oldtitle;
            }
        }

        private class RichTextDocument : PrintDocument
        {
            private RichTextBox rtf;
            private int currentLine;

            public RichTextDocument(RichTextBox rtf)
            {
                this.rtf = rtf;
            }

            protected override void OnBeginPrint(PrintEventArgs e)
            {
                currentLine = 0;
                base.OnBeginPrint(e);
            }

            //TODO: print with formats
            //TODO: check width of string and decide for extra line breaks
            protected override void OnPrintPage(PrintPageEventArgs e)
            {
                int LinesPerPage = 0;
                PointF LinePosition = e.MarginBounds.Location;
                float LeftMargin = e.MarginBounds.Left;
                float TopMargin = e.MarginBounds.Top;
                Font PrintFont = this.rtf.Font;
                float fontHeight = PrintFont.GetHeight(e.Graphics);
                LinesPerPage = (int)(e.MarginBounds.Height / fontHeight);
                string[] Lines = this.rtf.Lines;
                int lastLine = Math.Min(Lines.Length, currentLine + LinesPerPage);

                SolidBrush PrintBrush = new SolidBrush(this.rtf.ForeColor);
                try
                {
                    for (int i = 0; currentLine < lastLine; i++, currentLine++, LinePosition.Y += fontHeight)
                    {
                        e.Graphics.DrawString(Lines[currentLine], PrintFont, PrintBrush, LinePosition, new StringFormat());
                    }
                }
                finally
                {
                    PrintBrush.Dispose();
                }

                e.HasMorePages = currentLine < Lines.Length;

                base.OnPrintPage(e);
            }
        }
    }
}
