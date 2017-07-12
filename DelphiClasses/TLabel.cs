using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Security;
using System.Security.Permissions;
using System.Drawing.Drawing2D;

namespace DelphiClasses
{
    public enum DragMode
    {
        Manual,
        Automatic
    }

    internal class DragSource
    {
        public object Source { get; private set; }

        internal DragSource(object source)
        {
            Source = source;
        }
    }

    [ToolboxItem(true)]
    public class TLabel : Label
    {
        private static CodeAccessPermission _ModifyFocus = new UIPermission(UIPermissionWindow.AllWindows);
        private static readonly ContentAlignment anyRight = ContentAlignment.BottomRight | ContentAlignment.MiddleRight | ContentAlignment.TopRight;
        private static readonly ContentAlignment anyBottom = ContentAlignment.BottomRight | ContentAlignment.BottomCenter | ContentAlignment.BottomLeft;
        private static readonly ContentAlignment anyCenter = ContentAlignment.BottomCenter | ContentAlignment.MiddleCenter | ContentAlignment.TopCenter;
        private static readonly ContentAlignment anyMiddle = ContentAlignment.MiddleRight | ContentAlignment.MiddleCenter | ContentAlignment.MiddleLeft;

        private bool useDefaultTextRendering;
        private bool wordWrap;
        private bool showsPath;
        private Rectangle dragRectangle;
        private bool startDragDrop;

        [DefaultValue(true)]
        public override bool AutoSize { get { return base.AutoSize; } set { base.AutoSize = value; } }

        [DefaultValue(false), Category("Behavior")]
        public bool UseDefaultTextRendering
        {
            get { return useDefaultTextRendering; }
            set
            {
                if (useDefaultTextRendering != value)
                {
                    useDefaultTextRendering = value;
                    this.Invalidate();
                }
            }
        }

        [DefaultValue(false), Category("Appearance")]
        public bool ShowsPath
        {
            get { return showsPath; }
            set
            {
                if (showsPath != value)
                {
                    showsPath = value;
                    this.Invalidate();
                }
            }
        }

        [DefaultValue(false), Category("Appearance")]
        public bool WordWrap
        {
            get { return wordWrap; }
            set
            {
                if (wordWrap != value)
                {
                    wordWrap = value;
                    this.Invalidate();
                }
            }
        }

        [DefaultValue(DragMode.Manual), Category("Behavior")]
        public DragMode DragMode { get; set; }

        [DefaultValue(null), Category("Behavior")]
        public Control FocusControl { get; set; }

        [DefaultValue(true), Category("Appearance")]
        public bool Transparent
        {
            get { return this.BackColor.A < 0xff; }
            set { BackColor = value ? Color.Transparent : DefaultBackColor; }
        }

        public TLabel()
        {
            AutoSize = true;
        }

        protected override bool ProcessMnemonic(char charCode)
        {
            if (FocusControl == null)
                return base.ProcessMnemonic(charCode);

            if ((!this.UseMnemonic || !Control.IsMnemonic(charCode, this.Text)
                || !FocusControl.CanFocus)
                /*|| !this.CanProcessMnemonic()*/) // some day check this also? but have to use reflection to do so!
            {
                return false;
            }

            _ModifyFocus.Assert();
            try
            {
                FocusControl.Focus();
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }

            return true;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (DragMode == DragMode.Automatic)
            {
                startDragDrop = true;
                Point dragStart = e.Location;
                Size dragSize = SystemInformation.DragSize;
                dragStart.Offset(-(dragSize.Width / 2), -(dragSize.Height / 2));
                dragRectangle = new Rectangle(dragStart, dragSize);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (startDragDrop &&
                !dragRectangle.Contains(e.Location))
            {
                this.DoDragDrop(new DragSource(this), DragDropEffects.All);
                startDragDrop = false;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            startDragDrop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (UseDefaultTextRendering)
                base.OnPaint(e);
            else
            {
                Rectangle face = Extensions.DeflateRect(ClientRectangle, Padding);
                TextFormatFlags flags = CreateTextFormatFlags(this.TextAlign, this.AutoEllipsis, this.UseMnemonic);
                flags |= TextFormatFlags.NoPadding |
                    TextFormatFlags.PreserveGraphicsTranslateTransform |
                    TextFormatFlags.PreserveGraphicsClipping;
                if (base.Enabled)
                {
                    TextRenderer.DrawText(e.Graphics, this.Text, this.Font, face, ForeColor, flags);
                }
                else
                {
                    Color foreColor = DisabledTextColor(this.BackColor);
                    TextRenderer.DrawText(e.Graphics, this.Text, this.Font, face, foreColor, flags);
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // renders only parent control at client rectangle as background for transparency
            base.OnPaintBackground(pevent);

            if (!Transparent)
                return;

            Rectangle client = ClientRectangle;
            Control parent = Parent;
            if (parent != null)
            {
                // how to move the rendering area and setup it's size
                // (we want to translate it to the parent's origin)
                Rectangle shift = new Rectangle(-this.Left, -this.Top, parent.Width, parent.Height);
                // moving the clipping rectangle to the parent coordinate system
                Rectangle newClipRect = new Rectangle(client.Left + this.Left, client.Top + this.Top, client.Width, client.Height);
                Point renderOrigin = new Point(); //pevent.Graphics.RenderingOrigin;
                renderOrigin.Offset(-this.Left, -this.Top);
                int zindex = parent.Controls.GetChildIndex(this);
                for (int i = parent.Controls.Count - 1; i > zindex; i--)
                {
                    Control sibling = parent.Controls[i];
                    Rectangle intersection = Rectangle.Intersect(newClipRect, new Rectangle(sibling.Location, sibling.ClientSize));
                    if (!intersection.IsEmpty)
                    {
                        Rectangle siblingClipRect = intersection;
                        siblingClipRect.Offset(-sibling.Left, -sibling.Top);
                        IntPtr hdc = pevent.Graphics.GetHdc();
                        try
                        {
                            using (Graphics g = Graphics.FromHdc(hdc))
                            {
                                g.PageUnit = GraphicsUnit.Pixel;
                                GraphicsState state = g.Save();
                                using (PaintEventArgs pe = new PaintEventArgs(g, siblingClipRect))
                                {
                                    try
                                    {
                                        Point newOrigin = renderOrigin;
                                        newOrigin.Offset(sibling.Location);
                                        //g.RenderingOrigin = newOrigin; //changing the origin directly isn't working
                                        g.TranslateTransform(newOrigin.X, newOrigin.Y);
                                        InvokePaintBackground(sibling, pe);
                                        InvokePaint(sibling, pe);

                                    }
                                    finally
                                    {
                                        //g.Restore(state); // already done by Graphics.Dispose()
                                    }
                                }
                            }
                        }
                        finally
                        {
                            pevent.Graphics.ReleaseHdc(hdc);
                        }
                    }
                }
            }
        }

        internal static TextFormatFlags TextFormatFlagsForAlignmentGDI(ContentAlignment align)
        {
            TextFormatFlags flags = 0;
            flags |= TranslateAlignmentForGDI(align);
            return (flags | TranslateLineAlignmentForGDI(align));
        }

        internal static TextFormatFlags TranslateAlignmentForGDI(ContentAlignment align)
        {
            if ((align & anyBottom) != 0)
                return TextFormatFlags.Bottom;
            if ((align & anyMiddle) != 0)
                return TextFormatFlags.VerticalCenter;
            return 0;
        }

        internal static TextFormatFlags TranslateLineAlignmentForGDI(ContentAlignment align)
        {
            if ((align & anyRight) != 0)
                return TextFormatFlags.Right;
            if ((align & anyCenter) != 0)
                return TextFormatFlags.HorizontalCenter;
            return 0;
        }

        internal TextFormatFlags CreateTextFormatFlags(ContentAlignment textAlign, bool showEllipsis, bool useMnemonic)
        {
            textAlign = this.RtlTranslateContent(textAlign);
            TextFormatFlags flags = TextFormatFlagsForAlignmentGDI(textAlign);
            if (showEllipsis)
                flags |= this.ShowsPath ? TextFormatFlags.PathEllipsis : TextFormatFlags.EndEllipsis;
            if (this.RightToLeft == RightToLeft.Yes)
                flags |= TextFormatFlags.RightToLeft;
            if (this.WordWrap)
                flags |= TextFormatFlags.WordBreak;

            if (!useMnemonic)
            {
                return (flags | TextFormatFlags.NoPrefix);
            }
            if (!this.ShowKeyboardCues)
            {
                flags |= TextFormatFlags.HidePrefix;
            }
            return flags;
        }

        internal static bool IsDarker(Color c1, Color c2)
        {
            HLSColor color = new HLSColor(c1);
            HLSColor color2 = new HLSColor(c2);
            return (color.Luminosity < color2.Luminosity);
        }

        internal static Color DisabledTextColor(Color backColor)
        {
            Color controlDark = SystemColors.ControlDark;
            if (IsDarker(backColor, SystemColors.Control))
            {
                controlDark = ControlPaint.Dark(backColor);
            }
            return controlDark;
        }
    }
}
