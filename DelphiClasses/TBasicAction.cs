using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace DelphiClasses
{
    [Designer("Moras.Net.Design.TBasicActionDesigner, Moras.Net.Design, Version=1.0.0.0, Culture=neutral, PublicKeyToken=f9135b47417b1285")]
    public abstract class TBasicAction : Component
    {
        internal List<TBasicActionLink> clients = new List<TBasicActionLink>();
        private EventHandler execute;
        private string name;
        /// <summary>
        /// ActionComponent is set to the component that caused the action to execute, e.g. a toolbutton or a menu item.
        /// The property is set just before the action executes, and is reset to nil after the action was executed.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Component ActionComponent { get; set; }
        [Browsable(false)]
        public virtual string Name
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                {
                    if (Site != null)
                        name = Site.Name;

                    if (name == null)
                        name = string.Empty;
                }
                return name;
            }
            set
            {
                name = value;
            }
        }

        protected event EventHandler Change;
        [Browsable(false)]
        public virtual event EventHandler Execute
        {
            add
            {
                foreach (var client in clients)
                    client.Execute += value;
                execute += value;
                OnChange(EventArgs.Empty);
            }
            remove
            {
                foreach (var client in clients)
                    client.Execute -= value;
                execute -= value;
                OnChange(EventArgs.Empty);
            }
        }
        [Browsable(false)]
        public EventHandler ExecuteDelegate { get { return execute; } }
        [Browsable(false)]
        public virtual event EventHandler Update;

        protected TBasicAction(IContainer cont)
        {
            if (cont != null) cont.Add(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                int count = clients.Count;
                while (count > 0)
                {
                    UnRegisterChanges(clients[--count]);
                }
                clients = null;
            }

            base.Dispose(disposing);
        }

        protected internal virtual void OnChange(EventArgs e)
        {
            if (Change != null)
                Change(this, e);
        }

        public virtual bool OnExecute(EventArgs e)
        {
            if (execute != null)
            {
                execute(this, e);
                return true;
            }
            return false;
        }

        public virtual bool OnUpdate(EventArgs e)
        {
            if (Update != null)
            {
                Update(this, e);
                return true;
            }
            return false;
        }

        public virtual void ExecuteTarget(Component target) { }
        public virtual bool HandlesTarget(Component target) { return false; }
        public virtual void UpdateTarget(Component target) { }

        internal void RegisterChanges(TBasicActionLink client)
        {
            client.SetActionInternal(this);
            clients.Add(client);
        }

        internal void UnRegisterChanges(TBasicActionLink client)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i] == client)
                {
                    client.SetActionInternal(null);
                    clients.RemoveAt(i);
                    break;
                }
            }
        }

        internal bool IsDelegateRegisteredForExecute(EventHandler handler)
        {
            if (execute == null)
                return handler == null;

            return execute.GetInvocationList().Intersect(handler.GetInvocationList()).Any();
        }
    }
}
