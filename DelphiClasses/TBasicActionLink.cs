using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace DelphiClasses
{
    //HINT: Controls linked to actions by action links, are only linked one-way from the action to the control.
    // So be carefull with properties on the control. Don't change the linked properties on the control, only
    // change them on the linked action component. If they are only changed on the control, they get out of sync
    // and the action can't work as it should.
    internal abstract class TBasicActionLink : IDisposable
    {
        private TBasicAction action;

        public TBasicAction Action
        {
            get { return action; }
            set { SetAction(value); }
        }

        protected TActionList Owner { get; private set; }

        public event EventHandler Change;
        protected internal abstract event EventHandler Execute;

        public TBasicActionLink(Component client, TActionList owner)
        {
            Owner = owner;
            AssignClient(client);
        }

        ~TBasicActionLink()
        {
            Dispose(false);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (action != null)
                    action.UnRegisterChanges(this);
            }
        }

        protected abstract void AssignClient(Component client);

        protected virtual void OnChange(EventArgs e)
        {
            if (Change != null)
                Change(action, e);
        }

        protected virtual bool IsOnExecuteLinked { get { return true; } }

        protected virtual void SetAction(TBasicAction value)
        {
            if (action != value)
            {
                if (action != null)
                    action.UnRegisterChanges(this);
                action = value;
                if (value != null)
                    value.RegisterChanges(this);
            }
        }

        internal void SetActionInternal(TBasicAction value)
        {
            this.action = value;
        }

        //HINT: Diese Methode wird in Delphi anstelle des Control-eigenen Click-Eventhandlers aufgerufen,
        //wenn dieser Handler das gleiche Ziel hat wie das Action-Objekt, das über dieses
        //ActionLink-Objekt zugeordnet wurde.
        //Dieses Verhalten lässt sich hier nicht nachbilden, außer in eigenen abgeleiteten Klassen,
        //wenn die OnClick-Methode entsprechend überschrieben wird.
        public virtual bool OnExecute(Component target, EventArgs e)
        {
            action.ActionComponent = target;
            try
            {
                return action.OnExecute(e);
            }
            finally
            {
                if (action != null)
                    action.ActionComponent = null;
            }
        }

        public virtual bool OnUpdate(EventArgs e)
        {
            return action.OnUpdate(e);
        }
    }
}
