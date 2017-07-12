using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

namespace DelphiClasses
{
    internal class TMenuActionLink : TToolItemActionLink
    {
        private ToolStripMenuItem client;

        public TMenuActionLink(Component client, TActionList owner)
            : base(client, owner)
        {
        }

        protected override void AssignClient(Component client)
        {
            base.AssignClient(client);
            this.client = client as ToolStripMenuItem;
        }

        protected internal virtual bool IsAutoCheckLinked
        {
            get { return client.CheckOnClick == (Action as TCustomAction).AutoCheck; }
        }

        public override bool IsCheckedLinked
        {
            get
            {
                return base.IsCheckedLinked &&
                    client.Checked == (Action as TCustomAction).Checked;
            }
        }

        public override bool IsHelpContextLinked
        {
            get
            {
                return base.IsHelpContextLinked/* &&
                    FClient.HelpContext == (Action as TCustomAction).HelpContext*/;
            }
        }

        public override bool IsShortCutLinked
        {
            get
            {
                return base.IsShortCutLinked &&
                    client.ShortcutKeys == (Action as TCustomAction).ShortCut;
            }
        }

        protected internal override void SetAutoCheck(bool value)
        {
            if (IsAutoCheckLinked) client.CheckOnClick = value;
        }

        protected internal override void SetChecked(bool value)
        {
            if (IsCheckedLinked) client.Checked = value;
        }

        protected internal override void SetHelpContext(int value)
        {
            //if (IsHelpContextLinked) client.Help = value;
        }

        protected internal override void SetShortCut(Keys value)
        {
            if (IsShortCutLinked) client.ShortcutKeys = value;
        }
    }
}
