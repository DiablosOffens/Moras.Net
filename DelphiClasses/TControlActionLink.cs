using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

namespace DelphiClasses
{
    internal class TControlActionLink : TActionLink
    {
        private Control client;

        public TControlActionLink(Component client, TActionList owner)
            : base(client, owner)
        {
        }

        protected override void AssignClient(Component client)
        {
            this.client = client as Control;
        }

        protected internal override void SetCaption(string value)
        {
            if (IsCaptionLinked) client.Text = value;
        }

        protected internal override void SetEnabled(bool value)
        {
            if (IsEnabledLinked) client.SetShadowProperty(() => client.Enabled, value);
        }

        protected internal override void SetHint(string value)
        {
            if (IsHintLinked) Owner.ToolTipExtender.SetToolTip(client, value);
        }

        protected internal override void SetHelpContext(int value)
        {
            if (IsHelpLinked) Owner.HelpExtender.SetHelpKeyword(client, value.ToString());
        }

        protected internal override void SetHelpKeyword(string value)
        {
            if (IsHelpLinked) Owner.HelpExtender.SetHelpKeyword(client, value);
        }

        protected internal override void SetHelpType(HelpNavigator value)
        {
            if (IsHelpLinked) Owner.HelpExtender.SetHelpNavigator(client, value);
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

        public override bool IsHelpLinked
        {
            get
            {
                if (Owner.HelpExtender != null)
                {
                    string helpkey = Owner.HelpExtender.GetHelpKeyword(client);
                    int helpcontext;
                    if (!int.TryParse(helpkey, out helpcontext))

                        helpcontext = 0;
                    else
                        helpkey = null;
                    return base.IsHelpLinked &&
                        helpkey == (Action as TCustomAction).HelpKeyword &&
                        helpcontext == (Action as TCustomAction).HelpContext &&
                        Owner.HelpExtender.GetHelpNavigator(client) == (Action as TCustomAction).HelpType;
                }
                return false;
            }
        }

        public override bool IsHintLinked
        {
            get
            {
                return base.IsHintLinked && Owner.ToolTipExtender != null &&
                    Owner.ToolTipExtender.GetToolTip(client) == (Action as TCustomAction).Hint;
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
