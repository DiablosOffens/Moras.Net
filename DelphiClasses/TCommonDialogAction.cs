using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;

namespace DelphiClasses
{
    public abstract class TCommonDialogAction : TCustomAction
    {
        private IContainer components = new Container();
        private bool executeResult;
        protected CommonDialog dialog;
        
        [Browsable(false)]
        public virtual event EventHandler BeforeExecute;
        [Browsable(false)]
        public virtual event EventHandler Accept;
        [Browsable(false)]
        public virtual event EventHandler Cancel;

        [Browsable(false)]
        public bool ExecuteResult { get { return executeResult; } }

        public TCommonDialogAction(IContainer cont)
            : base(cont)
        {
            CreateDialog();

            DisableIfNoHandler = false;
            Enabled = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                components.Dispose();

            base.Dispose(disposing);
        }

        protected void OnBeforeExecute(EventArgs e)
        {
            if (BeforeExecute != null)
                BeforeExecute(this, e);
        }

        protected void OnAccept(EventArgs e)
        {
            if (Accept != null)
                Accept(this, e);
        }

        protected void OnCancel(EventArgs e)
        {
            if (Cancel != null)
                Cancel(this, e);
        }

        protected abstract Type GetDialogClass();

        protected virtual void CreateDialog() {
            Type dlgClass = GetDialogClass();
            if (dlgClass != null)
            {
                dialog = (CommonDialog)Activator.CreateInstance(dlgClass);
                components.Add(dialog, dlgClass.Name);
            }
        }

        public override bool HandlesTarget(Component target)
        {
            return dialog != null;
        }

        public override void ExecuteTarget(Component target)
        {
            OnBeforeExecute(EventArgs.Empty);
            executeResult = dialog.ShowDialog() == DialogResult.OK;
            if (executeResult)
                OnAccept(EventArgs.Empty);
            else
                OnCancel(EventArgs.Empty);
        }
    }
}
