using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

namespace DelphiClasses
{
    public class TFileSaveAs : TFileAction
    {
        [Browsable(true)]
        public override string Caption { get { return base.Caption; } set { base.Caption = value; } }
        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public SaveFileDialog Dialog { get { return (SaveFileDialog)dialog; } }
        [Browsable(true)]
        public override bool Enabled { get { return base.Enabled; } set { base.Enabled = value; } }
        [Browsable(true)]
        public override int HelpContext { get { return base.HelpContext; } set { base.HelpContext = value; } }
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
        public override event EventHandler BeforeExecute { add { base.BeforeExecute += value; } remove { base.BeforeExecute -= value; } }
        [Browsable(true)]
        public override event EventHandler Accept { add { base.Accept += value; } remove { base.Accept -= value; } }
        [Browsable(true)]
        public override event EventHandler Cancel { add { base.Cancel += value; } remove { base.Cancel -= value; } }
        [Browsable(true)]
        public override event EventHandler<HintEventArgs> DoHint { add { base.DoHint += value; } remove { base.DoHint -= value; } }

        public TFileSaveAs(IContainer cont)
            : base(cont)
        {
        }

        protected override Type GetDialogClass()
        {
            return typeof(SaveFileDialog);
        }
    }
}
