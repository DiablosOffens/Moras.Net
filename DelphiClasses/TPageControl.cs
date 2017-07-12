using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms.VisualStyles;
using System.Drawing.Text;

namespace DelphiClasses
{
    //TODO: Patch method PrefixAmpersands from TabPage, so that it does nothing.
    // This method is superfluous, because if someone want to prefix ampersants in text string
    // at his will, he can just do it, but letting the code do this without his will
    // is just stupid. The problem mentioned in the framework source is maybe not existent at all,
    // accelerators should also work on TabPages. But even if it's not, artificially prefixing
    // ampersands would destroy the default look and feel which every other common control implements.
    // If accelerators realy can't work on TabPages, someone can still implement it by manual handle
    // the key events. So there is absolutely no need for this stupid method.
    // Here is some information to the matter of patching methods:
    // http://stackoverflow.com/questions/7299097/dynamically-replace-the-contents-of-a-c-sharp-method

    // For now override DrawItem, looks ugly even with visual style rendering, but the only way so far:
    //https://www.codeproject.com/Articles/12011/MnemonicTabControl-a-tabcontrol-with-accelerator-k
    [ProvideProperty("TabVisible", typeof(TabPage))]
    public class TPageControl : TabControl, IExtenderProvider, ISupportInitialize
    {
        public new class ControlCollection : Control.ControlCollection
        {
            private TPageControl owner;
            private bool addingToPages;

            public ControlCollection(TPageControl owner)
                : base(owner)
            {
                this.owner = owner;
            }

            public override void Add(Control value)
            {
                if (!owner.insertingForVisibility)
                    base.Add(value);
                if (owner.insertingForHiding)
                    return;
                try
                {
                    addingToPages = true;
                    owner.visiblePagesAdapter.Add(value);
                }
                finally
                {
                    addingToPages = false;
                }
            }

            public override void Remove(Control value)
            {
                if (addingToPages) return;
                base.Remove(value);
                owner.visiblePagesAdapter.Remove(value);
            }

            public override void SetChildIndex(Control child, int newIndex)
            {
                if (addingToPages) return;
                if (!owner.insertingForVisibility)
                    base.SetChildIndex(child, newIndex);
                // order doesn't matter in adapter collection, it has only a meaning
                // in the TabPages collection
                /*if (newIndex >= owner.TabCount)
                    throw new NotImplementedException();
                owner.visiblePagesAdapter.SetChildIndex(child, newIndex);*/
            }
        }

        private TabControl.ControlCollection visiblePagesAdapter;
        private bool insertingForVisibility;
        private bool insertingForHiding;
        private bool initializing;
        private HashSet<TabPage> hidingTabCache;
        private int hotTabIndex = -1;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(TabDrawMode.OwnerDrawFixed)]
        public new TabDrawMode DrawMode
        {
            get { return base.DrawMode; }
            set { base.DrawMode = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(-1)]
        public int HotTabIndex
        {
            get { return hotTabIndex; }
            private set
            {
                if (hotTabIndex != value)
                {
                    if (hotTabIndex != -1)
                        Invalidate(GetTabRect(hotTabIndex));
                    hotTabIndex = value;
                    if (value != -1)
                        Invalidate(GetTabRect(value));
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(null)]
        public TabPage HotTab
        {
            get { return hotTabIndex == -1 ? null : TabPages[hotTabIndex]; }
        }

        public TPageControl()
        {
            visiblePagesAdapter = new TabControl.ControlCollection(this);
            DrawMode = TabDrawMode.OwnerDrawFixed;
        }

        protected override Control.ControlCollection CreateControlsInstance()
        {
            return new ControlCollection(this);
        }

        private int TestTab(Point pt)
        {
            for (int index = 0; index < TabCount; index++)
            {
                if (GetTabRect(index).Contains(pt.X, pt.Y))
                    return index;
            }
            return -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (HotTrack)
                HotTabIndex = TestTab(new Point(e.X, e.Y));
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (HotTrack)
                HotTabIndex = -1;
        }

        protected override bool ProcessMnemonic(char charCode)
        {
            foreach (TabPage p in TabPages)
            {
                if (Control.IsMnemonic(charCode, p.Text))
                {
                    SelectedTab = p;
                    Focus();
                    return true;
                }
            }
            return false;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            base.OnDrawItem(e);
            bool vert = (Alignment > TabAlignment.Bottom);
            int off = 1;
            if ((e.State & DrawItemState.Selected) != 0)
                off = -1;

            Rectangle bounds;
            if (vert)
            {
                // tabs are aligned left or right
                System.Drawing.Drawing2D.Matrix m = new System.Drawing.Drawing2D.Matrix();
                m.Translate(0, e.Bounds.Height - TabPages[0].Top);
                m.RotateAt(270, new PointF(e.Bounds.X, e.Bounds.Y));
                e.Graphics.Transform = m;
                bounds = new Rectangle(e.Bounds.Left - TabPages[0].Top, e.Bounds.Top + off,
                                       e.Bounds.Height, e.Bounds.Width);
            }
            else
            {
                // tabs are aligned top or bottom
                bounds = new Rectangle(e.Bounds.X, e.Bounds.Y + off, e.Bounds.Width, e.Bounds.Height);
            }

            if (Application.RenderWithVisualStyles)
            {
                TabItemState state = TabItemState.Normal;
                if ((e.State & DrawItemState.HotLight) != 0 ||
                    // state is never HotLight in OwnerDrawFixed mode, so use this workaround
                    (HotTrack && hotTabIndex == e.Index))
                    state = TabItemState.Hot;
                if ((e.State & DrawItemState.Selected) != 0)
                    state = TabItemState.Selected;
                if ((e.State & DrawItemState.Disabled) != 0)
                    state = TabItemState.Disabled;

                TextFormatFlags flags = TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine |
                    TextFormatFlags.PreserveGraphicsTranslateTransform |
                    TextFormatFlags.PreserveGraphicsClipping;
                if ((e.State & DrawItemState.NoAccelerator) != 0)
                    flags |= TextFormatFlags.HidePrefix;

                bool focused = (e.State & DrawItemState.Focus) != 0 &&
                    (e.State & DrawItemState.NoFocusRect) != DrawItemState.NoFocusRect;

                //bounds = GetTabRect(e.Index);

                TabRenderer.DrawTabItem(e.Graphics, bounds,
                    TabPages[e.Index].Text, e.Font, flags, focused, state);
            }
            else
            {
                e.DrawBackground();

                using (StringFormat sf = new StringFormat(StringFormatFlags.NoClip
                                           | StringFormatFlags.NoWrap))
                {
                    sf.HotkeyPrefix = (e.State & DrawItemState.NoAccelerator) != 0 ? HotkeyPrefix.Hide : HotkeyPrefix.Show;
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    Color textcol;

                    // state is never HotLight in OwnerDrawFixed mode, so use this workaround
                    if (HotTrack && hotTabIndex == e.Index)
                        textcol = SystemColors.HotTrack;
                    switch (e.State & ~(DrawItemState.Selected | DrawItemState.Focus | DrawItemState.Default))
                    {
                        case DrawItemState.Disabled:
                            textcol = SystemColors.GrayText;
                            break;
                        case DrawItemState.HotLight:
                            textcol = SystemColors.HotTrack;
                            break;
                        default:
                            textcol = ForeColor;
                            break;
                    }
                    using (Brush brush = new SolidBrush(textcol))
                    {
                        e.Graphics.DrawString(TabPages[e.Index].Text,
                                     e.Font, brush, bounds, sf);
                    }
                }

                if ((e.State & DrawItemState.Selected) != 0)
                    e.DrawFocusRectangle();
            }

            if (vert)
                e.Graphics.ResetTransform();
        }

        [DefaultValue(true), Category("Appearance"),
        Description("Controls the visibility of the tab item.")]
        public bool GetTabVisible(TabPage page)
        {
            if (initializing && hidingTabCache != null)
                return !hidingTabCache.Contains(page);
            return TabPages.Contains(page);
        }

        public void SetTabVisible(TabPage page, bool value)
        {
            if (initializing)
            {
                if (hidingTabCache == null)
                    hidingTabCache = new HashSet<TabPage>();
                if (!value)
                    hidingTabCache.Add(page);
                return;
            }
            bool oldvalue = GetTabVisible(page);
            if (oldvalue == value)
                return;

            int selindex = SelectedIndex;
            SelectedIndex = -1;
            try
            {
                int index = Controls.IndexOf(page);
                if (index == -1)
                    throw new ArgumentException("Can only set TabVisible to TabPages that are children to this control", "page");
                if (value)
                {
                    if (selindex >= index)
                        selindex++;
                    if (index > TabCount)
                        index = TabCount;
                    insertingForVisibility = true;
                    try
                    {
                        TabPages.Insert(index, page);
                    }
                    finally
                    {
                        insertingForVisibility = false;
                    }
                }
                else
                {
                    // Manipulating ControlsCollection tries to keep z-order with
                    // Control.UpdateChildZOrder, which needs window handles to work
                    // correctly, so create them now to make sure all get handles.
                    IntPtr hwnd;
                    foreach (Control ctl in Controls)
                    {
                        if (!ctl.IsHandleCreated)
                            hwnd = ctl.Handle;
                    }

                    Controls.Remove(page);
                    try
                    {
                        SuspendLayout();
                        insertingForHiding = true;
                        Controls.Add(page);
                        Controls.SetChildIndex(page, index);
                    }
                    finally
                    {
                        insertingForHiding = false;
                        ResumeLayout();
                    }
                }

            }
            finally
            {
                if (selindex >= TabCount)
                    selindex = TabCount - 1;
                if (selindex == SelectedIndex)
                    UpdateTabSelection(true);
                else
                    SelectedIndex = selindex;
            }
        }

        #region IExtenderProvider Members

        bool IExtenderProvider.CanExtend(object extendee)
        {
            return (extendee is TabPage && ((TabPage)extendee).Parent == this);
        }

        #endregion

        #region ISupportInitialize Members

        void ISupportInitialize.BeginInit()
        {
            initializing = true;
        }

        void ISupportInitialize.EndInit()
        {
            initializing = false;
            if (hidingTabCache != null)
            {
                foreach (TabPage page in hidingTabCache)
                {
                    SetTabVisible(page, false);
                }
                hidingTabCache.Clear();
                hidingTabCache = null;
            }
        }

        #endregion
    }
}
