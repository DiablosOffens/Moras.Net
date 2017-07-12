using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

namespace DelphiClasses
{
    internal class TToolControlActionLink : TToolItemActionLink
    {
        private ToolStripControlHost client;
        private Control hostedControl;

        public TToolControlActionLink(Component client, TActionList owner)
            : base(client, owner)
        {
        }

        protected override void AssignClient(Component client)
        {
            base.AssignClient(client);
            this.client = client as ToolStripControlHost;
            this.hostedControl = this.client.Control;
        }

        public override bool IsGroupIndexLinked
        {
            get
            {
                RadioButton radio = hostedControl as RadioButton;
                return radio != null && base.IsGroupIndexLinked &&
                    (radio.Parent is Form || radio.Parent is Panel || radio.Parent is GroupBox);
                //TODO: calculate an index and compare to (Action as TCustomAction).GroupIndex
            }
        }

        public override bool IsHelpLinked
        {
            get
            {
                if (Owner.HelpExtender != null)
                {
                    string helpkey = Owner.HelpExtender.GetHelpKeyword(hostedControl);
                    int helpcontext;
                    if (!int.TryParse(helpkey, out helpcontext))

                        helpcontext = 0;
                    else
                        helpkey = null;
                    return base.IsHelpLinked &&
                        helpkey == (Action as TCustomAction).HelpKeyword &&
                        helpcontext == (Action as TCustomAction).HelpContext &&
                        Owner.HelpExtender.GetHelpNavigator(hostedControl) == (Action as TCustomAction).HelpType;
                }
                return false;
            }
        }

        protected internal override void SetHelpContext(int value)
        {
            if (IsHelpLinked) Owner.HelpExtender.SetHelpKeyword(hostedControl, value.ToString());
        }

        protected internal override void SetHelpKeyword(string value)
        {
            if (IsHelpLinked) Owner.HelpExtender.SetHelpKeyword(hostedControl, value);
        }

        protected internal override void SetHelpType(HelpNavigator value)
        {
            if (IsHelpLinked) Owner.HelpExtender.SetHelpNavigator(hostedControl, value);
        }

        //TODO: ovveride the behavior
    }
}
