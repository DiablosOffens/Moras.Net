using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

namespace DelphiClasses
{
    public class TMainMenu : MenuStrip
    {
        public abstract class TTBXCustomItem : ToolStripMenuItem
        {
            private int FGroupIndex;
            private bool FRadioItem;

            protected TTBXCustomItem() { }

            //HINT: Toolbar 2000 implements this different from LCL:
            // RadioItem has no effect and GroupIndex works only if it's not the default value 0.
            // In the latter case it means automatically that it's a radio item and act like it.
            // LCL only act like radio items if RadioItem is true, so it can use 0 as a GroupIndex, too.
            [DefaultValue(0), Browsable(false)]
            public virtual int GroupIndex { get { return FGroupIndex; } set { SetGroupIndex(value); } }
            [DefaultValue(false), Browsable(false)]
            public virtual bool RadioItem { get { return FRadioItem; } set { SetRadioItem(value); } }

            protected override void OnCheckedChanged(EventArgs e)
            {
                base.OnCheckedChanged(e);
                if (Checked)
                    TurnSiblingsOff();
            }

            protected override void OnClick(EventArgs e)
            {
                TMenuActionLink actionlink = (TMenuActionLink)TActionList.GetActionLink(this);
                TBasicAction action = actionlink != null ? actionlink.Action : null;
                EventHandler clickhandler = this.GetClickEvent();

                // Following code based on D4's TControl.Click
                // Call OnClick if assigned and not equal to associated action's OnExecute.
                // If associated action's OnExecute assigned then call it, otherwise, call
                // OnClick.
                if (DesignMode || actionlink == null ||
                    action == null || !Extensions.AreIntersectingOrBothEmpty(clickhandler, action.ExecuteDelegate))
                    base.OnClick(e);
                else
                {
                    // Following code based on D6's TMenuItem.Click
                    if (CheckOnClick && (actionlink == null || !actionlink.IsAutoCheckLinked))
                        Checked = !Checked;
                    actionlink.OnExecute(this, EventArgs.Empty);
                }
            }

            protected void SetGroupIndex(int Value)
            {
                if (FGroupIndex != Value)
                {
                    FGroupIndex = Value;
                    if (Checked)
                        TurnSiblingsOff();
                }
            }

            private void SetRadioItem(bool Value)
            {
                if (FRadioItem != Value)
                {
                    FRadioItem = Value;
                }
            }

            private void TurnSiblingsOff()
            {
                if (GroupIndex != 0 && Owner != null)
                {
                    foreach (ToolStripItem item in Owner.Items)
                    {
                        TTBXCustomItem other = item as TTBXCustomItem;
                        if (other != null && other != this && other.GroupIndex == GroupIndex)
                        {
                            other.Checked = false;
                        }
                    }
                }
            }
        }

        public class TTBXItem : TTBXCustomItem
        {
            public TTBXItem() { }
            public TTBXItem(IContainer container) { container.Add(this); }

            [Browsable(true)]
            public override int GroupIndex { get { return base.GroupIndex; } set { base.GroupIndex = value; } }
            [Browsable(true)]
            public override bool RadioItem { get { return base.RadioItem; } set { base.RadioItem = value; } }
            [AttributeProvider("System.Windows.Forms.ToolStripItem", "ImageIndex"), Browsable(true), DefaultValue(-1)]
            public new int ImageIndex { get { return base.ImageIndex; } set { base.ImageIndex = value; } }
        }

        public class TTBXSubmenuItem : TTBXCustomItem
        {
            private ToolStripDropDownItem linkedDropDownItem;

            public TTBXSubmenuItem() { }
            public TTBXSubmenuItem(IContainer container) { container.Add(this); }

            protected override ToolStripDropDown CreateDefaultDropDown()
            {
                ToolStripDropDown dropDown = base.CreateDefaultDropDown();
                if (Owner != null && Owner.ImageList != null)
                    dropDown.ImageList = Owner.ImageList;
                return dropDown;
            }

            protected override void OnOwnerChanged(EventArgs e)
            {
                base.OnOwnerChanged(e);

                if (Owner.ImageList != null)
                    SetChildToolStripsImageList(DropDown, Owner.ImageList);
            }

            [AttributeProvider("System.Windows.Forms.ToolStripDropDownItem", "DropDown")]
            public new ToolStripDropDown DropDown { get { return base.DropDown; } set { base.DropDown = value; } }

            [AttributeProvider("System.Windows.Forms.ToolStripDropDownItem", "DropDownItems")]
            public new ToolStripItemCollection DropDownItems { get { return base.DropDownItems; } }

            [AttributeProvider("System.Windows.Forms.ToolStripItem", "ImageIndex"), Browsable(true), DefaultValue(-1)]
            public new int ImageIndex { get { return base.ImageIndex; } set { base.ImageIndex = value; } }

            [DefaultValue(null), RefreshProperties(RefreshProperties.All), Category("Data")]
            public ToolStripDropDownItem LinkDropDownItems
            {
                get { return linkedDropDownItem; }
                set
                {
                    linkedDropDownItem = value;
                    if (value != null)
                        DropDown = value.DropDown;
                    else
                        DropDown = null;
                }
            }

            [AttributeProvider("System.Windows.Forms.ToolStripDropDownMenu", "ShowCheckMargin")]
            public bool ShowCheckMargin
            {
                get
                {
                    var submenue = DropDown as ToolStripDropDownMenu;
                    if (submenue != null)
                        return submenue.ShowCheckMargin;
                    return false;
                }
                set
                {
                    var submenue = DropDown as ToolStripDropDownMenu;
                    if (submenue != null)
                        submenue.ShowCheckMargin = value;
                }
            }

            [AttributeProvider("System.Windows.Forms.ToolStripDropDownMenu", "ShowImageMargin")]
            public bool ShowImageMargin
            {
                get
                {
                    var submenue = DropDown as ToolStripDropDownMenu;
                    if (submenue != null)
                        return submenue.ShowImageMargin;
                    return true;
                }
                set
                {
                    var submenue = DropDown as ToolStripDropDownMenu;
                    if (submenue != null)
                        submenue.ShowImageMargin = value;
                }
            }

            private bool ShouldSerializeDropDown()
            {
                if (linkedDropDownItem != null && DropDown == linkedDropDownItem.DropDown)
                    return false;
                return !DropDown.IsAutoGenerated;
            }

            private bool ShouldSerializeDropDownItems()
            {
                if (linkedDropDownItem != null && DropDown == linkedDropDownItem.DropDown)
                    return false;
                return DropDown.IsAutoGenerated;
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(base.DefaultSize.Width, 23);
            }
        }

        [AttributeProvider("System.Windows.Forms.ToolStrip", "ImageList"), Browsable(true)]
        public new ImageList ImageList
        {
            get { return base.ImageList; }
            set
            {
                if (value != null)
                    SetChildToolStripsImageList(this, value);
                else
                    base.ImageList = value;
            }
        }

        private static void SetChildToolStripsImageList(ToolStrip toolStrip, ImageList value)
        {
            if (toolStrip.ImageList == null)
                toolStrip.ImageList = value;

            foreach (ToolStripItem item in toolStrip.Items)
            {
                if (item is ToolStripDropDownItem)
                {
                    SetChildToolStripsImageList(((ToolStripDropDownItem)item).DropDown, value);
                }
            }
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
    }
}
