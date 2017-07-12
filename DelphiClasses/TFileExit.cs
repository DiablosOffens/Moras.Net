using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;

namespace DelphiClasses
{
    public class TFileExit : TCustomAction
    {
        public TFileExit(IContainer cont)
            : base(cont)
        {
        }

        [Browsable(true)]
        public override string Caption { get { return base.Caption; } set { base.Caption = value; } }
        [Browsable(true)]
        public override bool Enabled { get { return base.Enabled; } set { base.Enabled = value; } }
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
        public override Keys ShortCut { get { return base.ShortCut; } set { base.ShortCut = value; } }
        [Browsable(true)]
        public override TShortCutList SecondaryShortCuts { get { return base.SecondaryShortCuts; } set { base.SecondaryShortCuts = value; } }
        [Browsable(true)]
        public override bool Visible { get { return base.Visible; } set { base.Visible = value; } }
        [Browsable(true)]
        public override event EventHandler<HintEventArgs> DoHint { add { base.DoHint += value; } remove { base.DoHint -= value; } }

        public override bool HandlesTarget(Component target)
        {
            return true;
        }

        public override void ExecuteTarget(Component target)
        {
            if (TApplication.Instance != null)
            {
                if (TApplication.Instance.MainForm != null)
                    TApplication.Instance.MainForm.Close();
                else
                    TApplication.Instance.ExitThread();
            }
            else
            {
                Application.Exit();
            }
        }
    }
}
