using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Reflection;
using System.Collections;
//using System.Windows.Forms.Design;

namespace DelphiClasses
{
    [Designer("Moras.Net.Design.TTBXDockablePanelDesigner, Moras.Net.Design, Version=1.0.0.0, Culture=neutral, PublicKeyToken=f9135b47417b1285")]
    public class TTBXDockablePanel : ToolStrip
    {
        //[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip)]
        internal class PanelToolStripItem : ToolStripControlHost
        {
            private class ContentPanel : Panel
            {
                public PanelToolStripItem Owner { get; set; }

                [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
                public override DockStyle Dock { get { return base.Dock; } set { base.Dock = value; } }
                [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
                public new Point Location { get { return base.Location; } set { base.Location = value; } }
                [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
                public new Size Size { get { return base.Size; } set { base.Size = value; } }

                //private bool ShouldSerializeLocation()
                //{
                //    if (Owner == null)
                //        return true;
                //    return Location != Owner.Bounds.Location;
                //}

                //private bool ShouldSerializeSize()
                //{
                //    if (Owner == null)
                //        return Size != DefaultSize;
                //    return Size != Owner.Size;
                //}
            }
            public PanelToolStripItem()
                : base(CreateContentPanel())
            {
                (this.Control as ContentPanel).Owner = this;
            }

            private static ContentPanel CreateContentPanel()
            {
                return new ContentPanel() { Dock = DockStyle.Fill };
            }

            protected override Padding DefaultMargin
            {
                get
                {
                    if (Owner != null && Owner.Orientation == Orientation.Vertical)
                        return new Padding(0, 0, -1, 0);
                    return new Padding(0);
                }
            }
            public Panel Panel { get { return (Panel)Control; } }

            public override Size GetPreferredSize(Size constrainingSize)
            {
                constrainingSize = ConvertZeroToUnbounded(constrainingSize);

                constrainingSize -= Padding.Size;

                ToolStrip parent = this.Parent;
                if (parent == null)
                    parent = this.Owner;

                if (parent == null)
                    return this.Size;

                TTBXDockablePanel dockPanel = parent as TTBXDockablePanel;
                Size result;
                if (parent.Orientation == Orientation.Horizontal)
                    result = new Size(dockPanel.DisplayRectangle.Width, dockPanel.DockedHeight);
                else
                    result = new Size(dockPanel.DockedWidth, dockPanel.DisplayRectangle.Height);

                result -= Margin.Size;

                result.Width = Math.Min(constrainingSize.Width, result.Width);
                result.Height = Math.Min(constrainingSize.Height, result.Height);

                return result + Padding.Size;
            }

            private static Size ConvertZeroToUnbounded(Size size)
            {
                if (size.Width == 0)
                {
                    size.Width = int.MaxValue;
                }
                if (size.Height == 0)
                {
                    size.Height = int.MaxValue;
                }
                return size;
            }

        }

        private static IList s_globalToolStripPanels;

        private PanelToolStripItem panelItem;
        private ToolStripItem gripItem;
        private FieldInfo m_gripThickness;
        private List<ToolStripPanel> m_removedPanels;
        internal bool InsideControlAdded;

        static TTBXDockablePanel()
        {
            s_globalToolStripPanels = (IList)typeof(ToolStripManager).GetProperty("ToolStripPanels", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null, null);
        }

        public TTBXDockablePanel()
        {
            AddPanelItem();
            DockableTo = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        }

        private void AddPanelItem()
        {
            panelItem = new PanelToolStripItem();
            panelItem.Dock = DockStyle.Fill;
            Items.Add(panelItem);
        }

        protected override void OnBeginDrag(EventArgs e)
        {
            base.OnBeginDrag(e);
            m_removedPanels = new List<ToolStripPanel>();
            Rectangle formbounds = new Rectangle();
            Form form = FindForm();
            if (form != null)
                formbounds = form.ClientRectangle;

            for (int i = s_globalToolStripPanels.Count - 1; i >= 0; i--)
            {
                ToolStripPanel panel = (ToolStripPanel)s_globalToolStripPanels[i];
                if ((DockableTo & CalculateArea(panel, panel.Orientation, formbounds)) == 0)
                {
                    m_removedPanels.Add(panel);
                    s_globalToolStripPanels.Remove(panel);
                }
            }
        }

        private static AnchorStyles CalculateArea(Control ctl, Orientation orientation, Rectangle bounds)
        {
            if (orientation == Orientation.Horizontal)
            {
                return ctl.Top < (bounds.Height / 2) ? AnchorStyles.Top : AnchorStyles.Bottom;
            }
            else
            {
                return ctl.Left < (bounds.Width / 2) ? AnchorStyles.Left : AnchorStyles.Right;
            }
        }

        private static AnchorStyles ConvertDockStyle(DockStyle dock)
        {
            switch (dock)
            {
                case DockStyle.Bottom:
                    return AnchorStyles.Bottom;
                case DockStyle.Left:
                    return AnchorStyles.Left;
                case DockStyle.Right:
                    return AnchorStyles.Right;
                case DockStyle.Top:
                    return AnchorStyles.Top;
                case DockStyle.Fill:
                    return AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                case DockStyle.None:
                    return AnchorStyles.None;
                default:
                    throw new ArgumentOutOfRangeException("dock");
            }
        }

        protected override void OnEndDrag(EventArgs e)
        {
            base.OnEndDrag(e);
            if (m_removedPanels != null)
            {
                foreach (var panel in m_removedPanels)
                {
                    s_globalToolStripPanels.Add(panel);
                }
                m_removedPanels = null;
            }
        }

        protected override void OnLayoutStyleChanged(EventArgs e)
        {
            base.OnLayoutStyleChanged(e);
            if (panelItem != null)
                panelItem.ResetMargin();
        }

        protected override void OnPaintGrip(PaintEventArgs e)
        {
            Rectangle gripRect = GripRectangle;
            Padding margin = GripMargin;
            gripRect = Extensions.DeflateRect(gripRect, margin);

            TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.PreserveGraphicsTranslateTransform;
            ButtonRenderer.DrawButton(e.Graphics, gripRect, Text, Font, flags, false, System.Windows.Forms.VisualStyles.PushButtonState.Normal);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if (Parent != null && Parent.Font != null)
                this.Font = Parent.Font;
        }

        protected override void OnParentFontChanged(EventArgs e)
        {
            base.OnParentFontChanged(e);

            if (Parent.Font != null)
                this.Font = Parent.Font;
        }

        private ToolStripItem Grip
        {
            get
            {
                if (gripItem == null)
                {
                    Rectangle gripRect = GripRectangle;
                    gripItem = GetItemAt(gripRect.Location);
                    m_gripThickness = gripItem.GetType().GetField("gripThickness", BindingFlags.Instance | BindingFlags.NonPublic);
                }
                return gripItem;
            }
        }

        [Category("Layout")]
        public int GripSize
        {
            get
            {
                if (Grip != null)
                    return (int)m_gripThickness.GetValue(Grip);
                throw new NullReferenceException();
            }
            set
            {
                if (value != GripSize)
                {
                    m_gripThickness.SetValue(Grip, value);
                    PerformLayout();
                }
            }
        }

        public override DockStyle Dock
        {
            get
            {
                return base.Dock;
            }
            set
            {
                if (InsideControlAdded)
                    return;

                if (value != base.Dock)
                {
                    base.Dock = value;

                    Rectangle formbounds = new Rectangle();
                    Form form = FindForm();
                    if (form != null)
                        formbounds = form.ClientRectangle;

                    ToolStripPanel dockpanel = null;
                    AnchorStyles dockto = ConvertDockStyle(value) & DockableTo;
                    for (int i = s_globalToolStripPanels.Count - 1; i >= 0; i--)
                    {
                        ToolStripPanel panel = (ToolStripPanel)s_globalToolStripPanels[i];
                        if (CalculateArea(panel, panel.Orientation, formbounds) == dockto)
                        {
                            dockpanel = panel;
                            break;
                        }
                    }
                    if (dockpanel != null)
                        dockpanel.Join(this);
                }
            }
        }

        [DefaultValue(AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top)]
        public AnchorStyles DockableTo { get; set; }

        [Category("Layout"), DefaultValue(0), RefreshProperties(RefreshProperties.All)]
        public int DockedWidth
        {
            get { return Orientation == Orientation.Vertical ? MinimumSize.Width : 0; }
            set { if (Orientation == Orientation.Vertical) MinimumSize = new Size(value, MinimumSize.Height); }
        }

        [Category("Layout"), DefaultValue(0), RefreshProperties(RefreshProperties.All)]
        public int DockedHeight
        {
            get { return Orientation == Orientation.Horizontal ? MinimumSize.Height : 0; }
            set { if (Orientation == Orientation.Horizontal) MinimumSize = new Size(MinimumSize.Width, value); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Panel ContentPanel { get { return panelItem.Panel; } }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ToolStripItemCollection Items
        {
            get
            {
                return base.Items;
            }
        }
    }
}
