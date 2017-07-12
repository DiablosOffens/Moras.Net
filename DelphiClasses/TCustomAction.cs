using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Design;

namespace DelphiClasses
{
    public class HintEventArgs : EventArgs
    {
        public string Hint { get; set; }
        public bool CanShow { get; set; }
        public HintEventArgs(string hint, bool canShow)
        {
            Hint = hint;
            CanShow = canShow;
        }
    }

    public class TCustomAction : TContainedAction
    {
        private bool autoCheck;
        private string caption;
        private bool _checked;
        private bool checking;
        private bool enabled = true;
        private int groupIndex;
        private int helpContext;
        private string helpKeyword;
        private int imageIndex = -1;
        private HelpNavigator helpType = HelpNavigator.TopicId;
        private string hint;
        private Keys shortCut;
        private bool visible = true;
        private TShortCutList secondaryShortCuts;
        protected object image;
        protected object mask;

        protected virtual bool HandleShortCut
        {
            get
            {
                return OnExecute(EventArgs.Empty);
            }
        }

        protected internal bool SavedEnabledState { get; set; }

        public override string Name
        {
            get
            {
                return base.Name;
            }
            set
            {
                bool autoChange = base.Name == Caption;
                base.Name = value;
                if (autoChange && clients.Count == 0)
                    Caption = value;
            }
        }

        [DefaultValue(false), Browsable(false)]
        public virtual bool AutoCheck
        {
            get { return autoCheck; }
            set
            {
                if (value != autoCheck)
                {
                    foreach (var client in clients)
                        ((TActionLink)client).SetAutoCheck(value);
                    autoCheck = value;
                    OnChange(EventArgs.Empty);
                }
            }
        }

        [Localizable(true), Browsable(false)]
        public virtual string Caption
        {
            get { return caption; }
            set
            {
                if (value != caption)
                {
                    foreach (var client in clients)
                        ((TActionLink)client).SetCaption(value);
                    caption = value;
                    OnChange(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(false), Browsable(false)]
        public virtual bool Checked
        {
            get { return _checked; }
            set
            {
                if (checking || _checked == value)
                    return;

                checking = true;
                try
                {
                    foreach (var client in clients)
                        ((TActionLink)client).SetChecked(value);
                    _checked = value;
                    if (groupIndex > 0 && _checked)
                    {
                        foreach (var action in ActionList.Actions)
                        {
                            TCustomAction other = action as TCustomAction;
                            if (other != this && other != null && other.groupIndex == groupIndex)
                                other.Checked = false;
                        }
                    }
                    OnChange(EventArgs.Empty);
                }
                finally
                {
                    checking = false;
                }
            }
        }

        [DefaultValue(false), Browsable(false)]
        public virtual bool DisableIfNoHandler { get; set; }

        [DefaultValue(true), Browsable(false)]
        public virtual bool Enabled
        {
            get { return enabled; }
            set
            {
                if (value != enabled)
                {
                    //if (ActionList != null)
                    //{
                    //    if (ActionList.State == Suspended)
                    //    {
                    //        enabled = value;
                    //        return;
                    //    }
                    //    if (ActionList.State == SuspendedEnabled)
                    //        value = true;
                    //}
                    foreach (var client in clients)
                        ((TActionLink)client).SetEnabled(value);
                    enabled = value;
                    OnChange(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(0), Browsable(false)]
        public virtual int GroupIndex
        {
            get { return groupIndex; }
            set
            {
                if (value != groupIndex)
                {
                    foreach (var client in clients)
                        ((TActionLink)client).SetGroupIndex(value);
                    groupIndex = value;
                    OnChange(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(0), Browsable(false)]
        public virtual int HelpContext
        {
            get { return helpContext; }
            set
            {
                if (value != helpContext)
                {
                    foreach (var client in clients)
                        ((TActionLink)client).SetHelpContext(value);
                    helpContext = value;
                    OnChange(EventArgs.Empty);
                }
            }
        }

        [DefaultValue((string)null), Browsable(false)]
        public virtual string HelpKeyword
        {
            get { return helpKeyword; }
            set
            {
                if (value != helpKeyword)
                {
                    foreach (var client in clients)
                        ((TActionLink)client).SetHelpKeyword(value);
                    helpKeyword = value;
                    OnChange(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(HelpNavigator.TopicId), Browsable(false)]
        public virtual HelpNavigator HelpType
        {
            get { return helpType; }
            set
            {
                if (value != helpType)
                {
                    foreach (var client in clients)
                        ((TActionLink)client).SetHelpType(value);
                    helpType = value;
                    OnChange(EventArgs.Empty);
                }
            }
        }

        [Localizable(true), Browsable(false)]
        public virtual string Hint
        {
            get { return hint; }
            set
            {
                if (value != hint)
                {
                    foreach (var client in clients)
                        ((TActionLink)client).SetHint(value);
                    hint = value;
                    OnChange(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(-1), Browsable(false)]
        [RelatedImageList("ActionList.ImageList"), Localizable(true), RefreshProperties(RefreshProperties.Repaint),
        TypeConverter(typeof(ImageIndexConverter)),
        Editor("System.Windows.Forms.Design.ToolStripImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public virtual int ImageIndex
        {
            get { return imageIndex; }
            set
            {
                if (value != imageIndex)
                {
                    foreach (var client in clients)
                        ((TActionLink)client).SetImageIndex(value);
                    imageIndex = value;
                    OnChange(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(0), Browsable(false)]
        public virtual Keys ShortCut
        {
            get { return shortCut; }
            set
            {
                if (value != shortCut)
                {
                    foreach (var client in clients)
                        ((TActionLink)client).SetShortCut(value);
                    shortCut = value;
                    OnChange(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(true), Browsable(false)]
        public virtual bool Visible
        {
            get { return visible; }
            set
            {
                if (value != visible)
                {
                    foreach (var client in clients)
                        ((TActionLink)client).SetVisible(value);
                    visible = value;
                    OnChange(EventArgs.Empty);
                }
            }
        }

        [Browsable(false)]
        public virtual TShortCutList SecondaryShortCuts
        {
            get
            {
                if (secondaryShortCuts == null)
                    secondaryShortCuts = new TShortCutList();
                return secondaryShortCuts;
            }
            set
            {
                if (secondaryShortCuts == null)
                {
                    if (value == null || value.Count == 0)
                        return;
                    secondaryShortCuts = new TShortCutList();
                }
                secondaryShortCuts.CopyFrom(value);
            }
        }

        private void ResetVisibleSecondaryShortCuts()
        {
            secondaryShortCuts = null;
        }

        private bool ShouldSerializeSecondaryShortCuts()
        {
            return secondaryShortCuts != null && secondaryShortCuts.Count > 0;
        }

        [Browsable(false)]
        public virtual event EventHandler<HintEventArgs> DoHint;

        public TCustomAction(IContainer cont)
            : base(cont)
        {

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (image is IDisposable)
                {
                    ((IDisposable)image).Dispose();
                    image = null;
                }
                if (mask is IDisposable)
                {
                    ((IDisposable)mask).Dispose();
                    mask = null;
                }
                if (secondaryShortCuts != null)
                {
                    secondaryShortCuts.Clear();
                    secondaryShortCuts = null;
                }
            }

            base.Dispose(disposing);
        }

        public virtual bool OnDoHint(ref string strHint)
        {
            HintEventArgs args = new HintEventArgs(strHint, true);
            if (DoHint != null) DoHint(this, args);
            strHint = args.Hint;
            return args.CanShow;
        }

        public override bool OnExecute(EventArgs e)
        {
            if (ActionList != null && ActionList.State != TActionListState.asNormal)
                return false;
            OnUpdate(EventArgs.Empty);
            if (autoCheck)
                Checked = !Checked;
            return Enabled && base.OnExecute(e);
        }
    }
}
