using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

namespace DelphiClasses
{
    internal class TToolItemActionLink : TActionLink
    {
        private ToolStripItem client;

        public TToolItemActionLink(Component client, TActionList owner)
            : base(client, owner)
        {
        }

        protected override void AssignClient(Component client)
        {
            this.client = client as ToolStripItem;
        }

        protected internal override void SetCaption(string value)
        {
            if (IsCaptionLinked) client.Text = value;
        }

        protected internal override void SetEnabled(bool value)
        {
            if (IsEnabledLinked) client.Enabled = value;
        }

        protected internal override void SetHint(string value)
        {
            if (IsHintLinked) client.ToolTipText = value;
        }

        protected internal override void SetImageIndex(int value)
        {
            if (IsImageIndexLinked) client.ImageIndex = value;
        }

        protected internal override void SetVisible(bool value)
        {
            if (IsVisibleLinked) client.Visible = value;
        }

        protected internal override event EventHandler Execute
        {
            add
            {
                if (IsOnExecuteLinked)
                {
                    client.Click += value;
                }
            }
            remove
            {
                if (IsOnExecuteLinked)
                {
                    client.Click -= value;
                }
            }
        }

        protected override bool IsOnExecuteLinked
        {
            get
            {
                return base.IsOnExecuteLinked &&
                    client.GetClickEvent().AreIntersectingOrBothEmpty(Action.ExecuteDelegate);
            }
        }

        protected virtual bool DoShowHint(ref string strHint)
        {
            if (Action is TCustomAction)
            {
                TCustomAction custAction = (TCustomAction)Action;
                if (custAction.OnDoHint(ref strHint) &&
                    /*Application.HintShortCuts &&*/ //TODO: how to replace this in .Net?
                 (custAction.ShortCut != Keys.None))
                {
                    if (!string.IsNullOrEmpty(strHint))
                        strHint = string.Format("{0} ({0})", strHint,
                                               ShortCutToText(custAction.ShortCut));
                }
            }
            return true;
        }

        private static string ShortCutToText(Keys keys)
        {
            return keys.ToString();
        }

        public override bool IsCaptionLinked
        {
            get
            {
                return base.IsCaptionLinked &&
                    client.Text == (Action as TCustomAction).Caption;
            }
        }

        public override bool IsEnabledLinked
        {
            get
            {
                return base.IsEnabledLinked &&
                    client.Enabled == (Action as TCustomAction).Enabled;
            }
        }

        public override bool IsHintLinked
        {
            get
            {
                return base.IsHintLinked &&
                    client.ToolTipText == (Action as TCustomAction).Hint;
            }
        }

        public override bool IsImageIndexLinked
        {
            get
            {
                return base.IsImageIndexLinked &&
                    client.ImageIndex == (Action as TCustomAction).ImageIndex;
            }
        }

        public override bool IsVisibleLinked
        {
            get
            {
                return base.IsVisibleLinked &&
                    client.Visible == (Action as TCustomAction).Visible;
            }
        }
    }
}
