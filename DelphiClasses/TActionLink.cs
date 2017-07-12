using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;

namespace DelphiClasses
{
    internal abstract class TActionLink : TBasicActionLink
    {
        protected TActionLink(Component client, TActionList owner)
            : base(client, owner)
        {

        }

        protected internal virtual void SetAutoCheck(bool value) { }
        protected internal virtual void SetCaption(string value) { }
        protected internal virtual void SetChecked(bool value) { }
        protected internal virtual void SetEnabled(bool value) { }
        protected internal virtual void SetGroupIndex(int value) { }
        protected internal virtual void SetHelpContext(int value) { }
        protected internal virtual void SetHelpKeyword(string value) { }
        protected internal virtual void SetHelpType(HelpNavigator value) { }
        protected internal virtual void SetHint(string value) { }
        protected internal virtual void SetImageIndex(int value) { }
        protected internal virtual void SetShortCut(Keys value) { }
        protected internal virtual void SetVisible(bool value) { }

        public virtual bool IsCaptionLinked { get { return Action is TCustomAction; } }
        public virtual bool IsCheckedLinked { get { return Action is TCustomAction; } }
        public virtual bool IsEnabledLinked { get { return Action is TCustomAction; } }
        public virtual bool IsGroupIndexLinked { get { return Action is TCustomAction; } }
        public virtual bool IsHelpContextLinked { get { return Action is TCustomAction; } }
        public virtual bool IsHelpLinked { get { return Action is TCustomAction; } }
        public virtual bool IsHintLinked { get { return Action is TCustomAction; } }
        public virtual bool IsImageIndexLinked { get { return Action is TCustomAction; } }
        public virtual bool IsShortCutLinked { get { return Action is TCustomAction; } }
        public virtual bool IsVisibleLinked { get { return Action is TCustomAction; } }
    }
}
