using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

namespace DelphiClasses
{
    internal class TToolButtonActionLink : TToolItemActionLink
    {
        private ToolStripButton client;

        public TToolButtonActionLink(Component client, TActionList owner)
            : base(client, owner)
        {
        }

        protected override void AssignClient(Component client)
        {
            base.AssignClient(client);
            this.client = client as ToolStripButton;
        }

        protected internal override void SetAutoCheck(bool value)
        {
            if (IsAutoCheckLinked) client.CheckOnClick = value;
        }

        protected internal override void SetChecked(bool value)
        {
            if (IsCheckedLinked) client.Checked = value;
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
    }
}
