using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;

namespace DelphiClasses
{
    public class TAction : TCustomAction
    {
        public TAction(IContainer cont)
            : base(cont)
        {
            DisableIfNoHandler = true;
        }
        [Browsable(true)]
        public override bool AutoCheck { get { return base.AutoCheck; } set { base.AutoCheck = value; } }
        [Browsable(true)]
        public override string Caption { get { return base.Caption; } set { base.Caption = value; } }
        [Browsable(true)]
        public override bool Checked { get { return base.Checked; } set { base.Checked = value; } }
        [Browsable(true), DefaultValue(true)]
        public override bool DisableIfNoHandler { get { return base.DisableIfNoHandler; } set { base.DisableIfNoHandler = value; } }
        [Browsable(true)]
        public override bool Enabled { get { return base.Enabled; } set { base.Enabled = value; } }
        [Browsable(true)]
        public override int GroupIndex { get { return base.GroupIndex; } set { base.GroupIndex = value; } }
        [Browsable(true)]
        public override int HelpContext { get { return base.HelpContext; } set { base.HelpContext = value; } }
        [Browsable(true)]
        public override string HelpKeyword { get { return base.HelpKeyword; } set { base.HelpKeyword = value; } }
        [Browsable(true)]
        public override HelpNavigator HelpType { get { return base.HelpType; } set { base.HelpType = value; } }
        [Browsable(true)]
        public override string Hint { get { return base.Hint; } set { base.Hint = value; } }
        [Browsable(true)]
        public override int ImageIndex { get { return base.ImageIndex; } set { base.ImageIndex = value; } }
        [Browsable(true)]
        public override event EventHandler Execute { add { base.Execute += value; } remove { base.Execute -= value; } }
        [Browsable(true)]
        public override event EventHandler<HintEventArgs> DoHint { add { base.DoHint += value; } remove { base.DoHint -= value; } }
        [Browsable(true)]
        public override event EventHandler Update { add { base.Update += value; } remove { base.Update -= value; } }
        [Browsable(true)]
        public override TShortCutList SecondaryShortCuts { get { return base.SecondaryShortCuts; } set { base.SecondaryShortCuts = value; } }
        [Browsable(true)]
        public override Keys ShortCut { get { return base.ShortCut; } set { base.ShortCut = value; } }
        [Browsable(true)]
        public override bool Visible { get { return base.Visible; } set { base.Visible = value; } }
    }
}
