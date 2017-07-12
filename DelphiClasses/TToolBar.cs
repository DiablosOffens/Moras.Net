using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

namespace DelphiClasses
{
    public class TToolBar : ToolStrip
    {
        public class TTBXItem : ToolStripButton
        {
            private int FGroupIndex;
            private bool FRadioItem;

            public TTBXItem() { }
            public TTBXItem(IContainer container) { container.Add(this); }

            //HINT: Toolbar 2000 implements this different from LCL:
            // RadioItem has no effect and GroupIndex works only if it's not the default value 0.
            // In the latter case it means automatically that it's a radio item and act like it.
            // LCL only act like radio items if RadioItem is true, so it can use 0 as a GroupIndex, too.
            [DefaultValue(0), Browsable(true)]
            public virtual int GroupIndex { get { return FGroupIndex; } set { SetGroupIndex(value); } }
            [DefaultValue(false), Browsable(true)]
            public virtual bool RadioItem { get { return FRadioItem; } set { SetRadioItem(value); } }
            [AttributeProvider("System.Windows.Forms.ToolStripItem", "ImageIndex"), Browsable(true), DefaultValue(-1)]
            public new int ImageIndex { get { return base.ImageIndex; } set { base.ImageIndex = value; } }

            protected override void OnCheckedChanged(EventArgs e)
            {
                base.OnCheckedChanged(e);
                if (Checked)
                    TurnSiblingsOff();
            }

            protected override void OnClick(EventArgs e)
            {
                TToolButtonActionLink actionlink = (TToolButtonActionLink)TActionList.GetActionLink(this);
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
                        TTBXItem other = item as TTBXItem;
                        if (other != null && other != this && other.GroupIndex == GroupIndex)
                        {
                            other.Checked = false;
                        }
                    }
                }
            }
        }

        public class TTBXSubmenuItem : ToolStripDropDownButton
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
