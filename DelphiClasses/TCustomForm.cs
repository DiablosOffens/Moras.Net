using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Diagnostics;

namespace DelphiClasses
{
    //http://cloudbings.com/questions/1413384/enumerate-net-controls-items-generically-menustrip-toolstrip-statusstrip
    public class TCustomForm : Form, ISupportInitialize, ISupportInitializeNotification
    {
        public delegate void MessageProc(ref Message m);

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Auto)]
        private unsafe struct LOGFONT
        {
            public const int LF_FACESIZE = 32;
            public const int DEFAULT_CHARSET = 1;
            public const int NONANTIALIASED_QUALITY = 3;

            public int lfHeight;
            public int lfWidth;
            public int lfEscapement;
            public int lfOrientation;
            public int lfWeight;
            public byte lfItalic;
            public byte lfUnderline;
            public byte lfStrikeOut;
            public byte lfCharSet;
            public byte lfOutPrecision;
            public byte lfClipPrecision;
            public byte lfQuality;
            public byte lfPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LF_FACESIZE)]
            public string lfFaceName;
        }

        private class SiteWithChildContainer : ISite
        {
            private TCustomForm owner;
            private bool inName = false;

            public SiteWithChildContainer(TCustomForm owner)
            {
                this.owner = owner;
            }

            #region ISite Members

            public IComponent Component
            {
                get { return owner; }
            }

            public IContainer Container
            {
                get { return owner.GetChildContainer(); }
            }

            public bool DesignMode
            {
                get { return false; }
            }

            public string Name
            {
                get
                {
                    //protect against recursion
                    if (inName)
                        return null;

                    inName = true;
                    try
                    {
                        return owner.Name;
                    }
                    finally
                    {
                        inName = false;
                    }
                }
                set { owner.Name = value; }
            }

            #endregion

            #region IServiceProvider Members

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(ISite))
                    return this;
                if (serviceType == typeof(IContainer))
                    return Container;
                return null;
            }

            #endregion
        }

        private static readonly object EventFormCreate = new object();
        private static readonly object EventFormDestroy = new object();
        private static readonly object EventFormShow = new object();
        private static readonly object EventFormHide = new object();
        private static readonly object EventInitialized = new object();

        private Dictionary<int, MessageProc> messageTable;
        private bool isSplash;
        private bool initializing;
        private bool oldCreateOrder;

        [Browsable(true)]
        [Localizable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public new Size ClientSize { get { return base.ClientSize; } set { base.ClientSize = value; } }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ClientHeight { get { return base.ClientSize.Height; } set { base.ClientSize = new Size(base.ClientSize.Width, value); } }
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ClientWidth { get { return base.ClientSize.Width; } set { base.ClientSize = new Size(value, base.ClientSize.Height); } }
        [DefaultValue(false), Category("Behavior"),
        Description("Specifies when FormCreate and FormDestroy events occur.")]
        public bool OldCreateOrder { get { return oldCreateOrder; } set { oldCreateOrder = value; } }

        [Browsable(true)]
        [Category("Behavior")]
        public event EventHandler FormCreate { add { base.Events.AddHandler(EventFormCreate, value); } remove { base.Events.RemoveHandler(EventFormCreate, value); } }
        [Browsable(true)]
        [Category("Behavior")]
        public event EventHandler FormDestroy { add { base.Events.AddHandler(EventFormDestroy, value); } remove { base.Events.RemoveHandler(EventFormDestroy, value); } }

        [Category("Behavior")]
        [Description("Occurs whenever the form is shown.")]
        public event EventHandler FormShow { add { base.Events.AddHandler(EventFormShow, value); } remove { base.Events.RemoveHandler(EventFormShow, value); } }
        [Category("Behavior")]
        [Description("Occurs whenever the form is hidden.")]
        public event EventHandler FormHide { add { base.Events.AddHandler(EventFormHide, value); } remove { base.Events.RemoveHandler(EventFormHide, value); } }

        [Browsable(false)]
        [DefaultValue(false)]
        public bool IsSplash
        {
            get { return isSplash; }
            set
            {
                if (isSplash == value)
                    return;

                if (value)
                    FormBorderStyle = FormBorderStyle.None;
                else if (isSplash)
                    FormBorderStyle = FormBorderStyle.Sizable;
                isSplash = value;
            }
        }

        private SiteWithChildContainer siteWithChildContainer;
        public override ISite Site
        {
            get { return base.Site ?? (siteWithChildContainer ?? (siteWithChildContainer = new SiteWithChildContainer(this))); }
            set { base.Site = value; }
        }

        protected TCustomForm()
        {
            messageTable = new Dictionary<int, MessageProc>()
            {
                {TApplication.CM_ACTIONEXECUTE, CMActionExecute},
                {TApplication.CM_ACTIONUPDATE, CMActionUpdate}
            };

            //http://stackoverflow.com/questions/4347873/changing-font-smoothing-for-my-application-alone-and-not-the-global-windows-font
            LOGFONT lfont = new LOGFONT();
            object boxedLFont = lfont;
            this.Font.ToLogFont(boxedLFont);
            lfont = (LOGFONT)boxedLFont;
            lfont.lfCharSet = LOGFONT.DEFAULT_CHARSET;
            lfont.lfQuality = LOGFONT.NONANTIALIASED_QUALITY;
            this.Font = Font.FromLogFont(lfont);
            if (TApplication.Instance.AppInitialized && TApplication.Instance.Icon != null)
                Icon = TApplication.Instance.Icon;

            //TODO: inject a call to AfterConstruction() in the calling frame!
            // Use factory pattern Create<TForm>() as long as the injection is not implemented.
        }

        protected internal virtual void AfterConstruction()
        {
            if (!oldCreateOrder)
                OnFormCreate(EventArgs.Empty);
        }

        public static TForm Create<TForm>() where TForm : TCustomForm, new()
        {
            TForm form = new TForm();
            form.AfterConstruction();
            return form;
        }

        protected virtual void OnFormCreate(EventArgs e)
        {
            EventHandler eh = Events[EventFormCreate] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }
        }

        protected virtual void OnFormDestroy(EventArgs e)
        {
            EventHandler eh = Events[EventFormDestroy] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }
        }

        protected virtual void OnFormShow(EventArgs e)
        {
            EventHandler eh = Events[EventFormShow] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }
        }

        protected virtual void OnFormHide(EventArgs e)
        {
            EventHandler eh = Events[EventFormHide] as EventHandler;
            if (eh != null)
            {
                eh(this, e);
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            if (!RecreatingHandle)
                OnFormDestroy(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
                OnFormShow(EventArgs.Empty);
            else
                OnFormHide(EventArgs.Empty);
        }

        private void CMActionExecute(ref Message m)
        {
            GCHandle handle = GCHandle.FromIntPtr(m.LParam);
            if (DoExecuteAction((TBasicAction)handle.Target))
                m.Result = (IntPtr)1;
        }

        private void CMActionUpdate(ref Message m)
        {
            GCHandle handle = GCHandle.FromIntPtr(m.LParam);
            if (DoUpdateAction((TBasicAction)handle.Target))
                m.Result = (IntPtr)1;
        }

        private bool DoExecuteActionInChildControls(Control parentControl, TBasicAction action)
        {
            var controls = parentControl.Controls;
            for (int i = 0; i < controls.Count; i++)
            {
                Control childControl = controls[i]; //TODO: include components
                if (childControl.Visible)
                {
                    if (controls[i].ExecuteAction(action))
                        return true;
                    if (DoExecuteActionInChildControls(childControl, action))
                        return true;
                }
            }
            return false;
        }

        private bool DoExecuteAction(TBasicAction action)
        {
            if (DesignMode || !Visible)
                return false;

            if (ActiveControl != null && ActiveControl.ExecuteAction(action))
                return true;

            if (this.ExecuteAction(action))
                return true;

            return false;
        }

        private bool ProcessUpdate(Component component, TBasicAction action)
        {
            return component != null && component.UpdateAction(action);
        }

        private bool ComponentAllowed(Component component)
        {
            return !(component is Control) || ((Control)component).Visible;
        }

        private bool TraverseClients(Control container, TBasicAction action)
        {
            if (container.Visible) //use Showing instead of Visible, but it doesn't exist in .net
            {
                var controls = container.Controls;
                for (int i = 0; i < controls.Count; i++)
                {
                    Control childControl = controls[i]; //TODO: include also components
                    if (ComponentAllowed(childControl) && ProcessUpdate(childControl, action) ||
                        childControl is Control && TraverseClients(childControl, action))
                        return true;
                }
            }
            return false;
        }

        private bool DoUpdateAction(TBasicAction action)
        {
            if (DesignMode || !Visible) //use Showing instead of Visible, but it doesn't exist in .net
                return false;

            if (ProcessUpdate(ActiveControl, action) ||
                ProcessUpdate(this, action) ||
                TraverseClients(this, action))
                return true;

            return false;
        }

        public IntPtr Perform(int msg, IntPtr wparam, IntPtr lparam)
        {
            Message message = Message.Create(IntPtr.Zero, msg, wparam, lparam);
            if (this != null)
                Dispatch(ref message);
            return message.Result;
        }

        protected virtual void Dispatch(ref Message message)
        {
            int index = message.Msg;
            MessageProc msghandler;
            if (messageTable.TryGetValue(index, out msghandler))
                msghandler(ref message);
        }

        protected override void WndProc(ref Message m)
        {
            int index = m.Msg;
            MessageProc msghandler;
            if (messageTable.TryGetValue(index, out msghandler))
                msghandler(ref m);
            else
                base.WndProc(ref m);
        }

        IContainer childContainer;
        protected virtual IContainer GetChildContainer()
        {
            // ISite must provide a container, so provide a
            // default value as child container, if it's not overridden.
            return childContainer ?? (childContainer = new Container());
        }

        private void AddControlsToContainer(Control.ControlCollection controls, IContainer container)
        {
            foreach (Control ctl in controls)
            {
                if (ctl.Site == null && !string.IsNullOrEmpty(ctl.Name)) // add only designer named controls
                    container.Add(ctl, ctl.Name);
                AddControlsToContainer(ctl.Controls, container);
            }
        }

        private void NameComponentsByFieldName()
        {
            FieldInfo[] fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var fi in fields)
            {
                if (typeof(IComponent).IsAssignableFrom(fi.FieldType))
                {
                    IComponent comp = (IComponent)fi.GetValue(this);
                    if (comp == null)
                        continue;

                    string name = fi.Name;
                    if (comp is Control)
                        name = ((Control)comp).Name;
                    else if (comp is ToolStripItem)
                        name = ((ToolStripItem)comp).Name;

                    if (comp.Site != null && comp.Site.Name != name)
                        comp.Site.Name = name;
                }
            }
        }

        #region ISupportInitialize Members

        void ISupportInitialize.BeginInit()
        {
            initializing = true;
        }

        void ISupportInitialize.EndInit()
        {
            initializing = false;
            NameComponentsByFieldName();
            AddControlsToContainer(Controls, GetChildContainer());
            OnInitialized();
            if (oldCreateOrder)
                OnFormCreate(EventArgs.Empty);
        }

        #endregion

        private void OnInitialized()
        {
            EventHandler handler = (EventHandler)base.Events[EventInitialized];
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #region ISupportInitializeNotification Members

        [Category("Action"), Description("Occurs whenever EndInit is called.")]
        public event EventHandler Initialized { add { base.Events.AddHandler(EventInitialized, value); } remove { base.Events.RemoveHandler(EventInitialized, value); } }

        [Browsable(false)]
        public bool IsInitialized
        {
            get { return !initializing; }
        }

        #endregion
    }
}
