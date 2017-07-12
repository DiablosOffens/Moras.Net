using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace DelphiClasses
{
    public class TContainedAction : TBasicAction
    {
        private TActionList actionList;
        private string category;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TActionList ActionList
        {
            get { return actionList; }
            set
            {
                if (actionList != value)
                {
                    if (actionList != null)
                        actionList.Actions.Remove(this);
                    if (value != null)
                        value.Actions.Add(this);
                }
            }
        }

        public string Category
        {
            get { return category; }
            set
            {
                if (category != value)
                {
                    category = value;
                    if (actionList != null)
                        actionList.OnChange(EventArgs.Empty);
                }
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Index
        {
            get
            {
                if (ActionList != null)
                    return ActionList.Actions.IndexOf(this);
                return -1;
            }
            set
            {
                int curIndex = Index;
                if (curIndex >= 0)
                {
                    int count = ActionList.Actions.Count;
                    if (value < 0) value = 0;
                    if (value >= count) value = count - 1;
                    if (value != curIndex)
                        ActionList.Actions.Move(curIndex, value);
                }
            }
        }

        public TContainedAction(IContainer cont)
            : base(cont)
        {

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (ActionList != null)
                    ActionList.Actions.Remove(this);
            }

            base.Dispose(disposing);
        }

        internal void SetActionList(TActionList actionList)
        {
            this.actionList = actionList;
        }

        public override bool OnExecute(EventArgs e)
        {
            return (ActionList != null && ActionList.ExecuteAction(this)) ||
                (TApplication.Instance != null && TApplication.Instance.ExecuteAction(this)) ||
                base.OnExecute(e) ||
                TApplication.SendMessage(TApplication.CM_ACTIONEXECUTE, IntPtr.Zero, this) == (IntPtr)1;
        }

        public override bool OnUpdate(EventArgs e)
        {
            return (ActionList != null && ActionList.UpdateAction(this)) ||
                (TApplication.Instance != null && TApplication.Instance.UpdateAction(this)) ||
                base.OnUpdate(e) ||
                TApplication.SendMessage(TApplication.CM_ACTIONUPDATE, IntPtr.Zero, this) == (IntPtr)1;
        }
    }
}
