using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using System.Drawing;
using DelphiClasses.HelpIntfs;
using System.Threading;

namespace DelphiClasses
{
    //not complete, only for compatibility
    public class TApplication : ApplicationContext, IMessageFilter
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int RegisterWindowMessage(string msg);

        internal static readonly int CM_ACTIONUPDATE = RegisterWindowMessage("TApplication_ActionUpdate");
        internal static readonly int CM_ACTIONEXECUTE = RegisterWindowMessage("TApplication_ActionExecute");
        private static readonly FieldInfo fiExcDialogButtonWidth = typeof(ThreadExceptionDialog).GetField("scaledButtonAlignmentWidth", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo fiExcDialogDetailsButton = typeof(ThreadExceptionDialog).GetField("detailsButton", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo fiExcDialogHelpButton = typeof(ThreadExceptionDialog).GetField("helpButton", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly object s_lockInstance = new object();
        private static TApplication s_instance = null;

        public delegate void FinalizeHandler();
        private static FinalizeHandler ProcessExit;
        public event EventHandler<ActionEventAgs> ActionExecute;
        public event EventHandler<ActionEventAgs> ActionUpdate;
        public event EventHandler EnsureDebugSymbols;

        private Type FCreatingFormType;
        public IntPtr Handle { get { return GetHandle(); } }
        private string FHelpFile;
        public string HelpFile { get { return FHelpFile; } set { FHelpFile = value; } }
        private IHelpSystem FHelpSystem;
        public IHelpSystem HelpSystem { get { return FHelpSystem; } }
        private string FTitle;
        public string Title { get { return GetTitle(); } set { SetTitle(value); } }
        private bool FTerminated;
        public bool Terminated { get { return FTerminated; } }
        public bool NoAppExceptionMessages { get; set; }
        public bool AppInitialized { get; private set; }
        public Icon Icon { get; private set; }

        private TApplication()
            : base()
        {
            Application.AddMessageFilter(this);
            Application.ThreadExit += new EventHandler(Application_ThreadExit);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            HelpIntfs.Unit.GetHelpSystem(out FHelpSystem);
        }

        static void Application_ThreadExit(object sender, EventArgs e)
        {
            if (ProcessExit != null)
                ProcessExit();
        }

        void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            lock (this)
            {
                Application.ThreadException -= new ThreadExceptionEventHandler(Application_ThreadException);
                ShowException(e.Exception);
                Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            }
        }

        public static TApplication Instance
        {
            get
            {
                if (s_instance != null)
                    return s_instance;

                lock (s_lockInstance)
                {
                    if (s_instance == null)
                        s_instance = new TApplication();
                }
                return s_instance;
            }
        }

        public static void AddFinalization(FinalizeHandler f)
        {
            ProcessExit = Delegate.Combine(f, ProcessExit) as FinalizeHandler;
        }

        #region IMessageFilter Members

        bool IMessageFilter.PreFilterMessage(ref Message m)
        {
            if (m.Msg == CM_ACTIONUPDATE || m.Msg == CM_ACTIONEXECUTE)
                return DispatchAction(ref m);
            else
                return false;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            lock (s_lockInstance)
            {
                if (s_instance != null)
                    s_instance = null;
            }
            base.Dispose(disposing);
        }

        protected override void ExitThreadCore()
        {
            base.ExitThreadCore();
            FTerminated = true;
        }

        private IntPtr GetHandle()
        {
            if (MainForm != null && MainForm.IsHandleCreated)
                return MainForm.Handle;
            return IntPtr.Zero;
        }

        private string GetTitle()
        {
            if (string.IsNullOrEmpty(FTitle))
                return Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
            return FTitle;
        }

        protected virtual void SetTitle(string AValue)
        {
            FTitle = AValue;
            if (MainForm != null && MainForm.IsHandleCreated)
                MainForm.Text = AValue;
        }

        private bool DispatchAction(ref Message m)
        {
            bool result = false;
            Form form = Form.ActiveForm;
            if (form is TCustomForm && ((TCustomForm)form).Perform(m.Msg, IntPtr.Zero, m.LParam) == (IntPtr)1)
                result = true;
            else if (MainForm != form && MainForm is TCustomForm && ((TCustomForm)MainForm).Perform(m.Msg, IntPtr.Zero, m.LParam) == (IntPtr)1)
                result = true;

            TBasicAction action = (TBasicAction)GCHandle.FromIntPtr(m.LParam).Target;
            TCustomAction custAction = action as TCustomAction;
            if (!result && custAction != null &&
                custAction.Enabled && custAction.DisableIfNoHandler)
                custAction.Enabled = action.ExecuteDelegate != null;

            return result;
        }

        public virtual void Initialize()
        {
            AppInitialized = true;
            try
            {
                Icon = IconUtils.LoadIconFromResource(IconUtils.IDI_APPLICATION);
            }
            catch (KeyNotFoundException)
            {
            }
        }

        public void CreateForm<TComponent>(out TComponent comp) where TComponent : Component, new()
        {
            bool ok = false;
            try
            {
                if (FCreatingFormType == null && typeof(TCustomForm).IsAssignableFrom(typeof(TComponent)))
                    FCreatingFormType = typeof(TComponent);
                comp = new TComponent();
                ok = true;
            }
            finally
            {
                if (!ok)
                {
                    comp = null;
                    if (FCreatingFormType == typeof(TComponent))
                        FCreatingFormType = null;
                }
            }

            if (comp is TCustomForm)
            {
                TCustomForm form = comp as TCustomForm;
                form.AfterConstruction();
                UpdateMainForm(form);
                IntPtr hwnd;
                if (MainForm == form)
                    hwnd = form.Handle; // force form to create its handle
                if (form.IsSplash)
                {
                    // show the splash form and handle the paint message
                    form.Show();
                    form.Invalidate();
                    Application.DoEvents();
                }
            }
        }

        public void UpdateMainForm(TCustomForm AForm)
        {
            if (MainForm == null &&
                FCreatingFormType == AForm.GetType() && /* !IsDisposing && */
                !AForm.IsMdiChild && !AForm.IsSplash)
                MainForm = AForm;
        }

        public virtual bool ExecuteAction(TBasicAction action)
        {
            ActionEventAgs args = new ActionEventAgs(action, false);
            if (ActionExecute != null)
                ActionExecute(this, args);
            return args.Result;
        }

        public virtual bool UpdateAction(TBasicAction action)
        {
            ActionEventAgs args = new ActionEventAgs(action, false);
            if (ActionUpdate != null)
                ActionUpdate(this, args);
            return args.Result;
        }

        internal static IntPtr SendMessage(int msg, IntPtr wparam, object lparam)
        {
            IntPtr memptr = IntPtr.Zero;
            GCHandle handle = new GCHandle();
            try
            {
                handle = GCHandle.Alloc(lparam);
                Message message = Message.Create(IntPtr.Zero, msg, wparam, (IntPtr)handle);
                // the application does not have a handle, so use filter directly
                if (Application.FilterMessage(ref message))
                    return (IntPtr)1;
                return IntPtr.Zero;
            }
            finally
            {
                handle.Free();
            }
        }

        protected virtual void OnEnsureDebugSymbols()
        {
            if (EnsureDebugSymbols != null)
                EnsureDebugSymbols(this, EventArgs.Empty);
        }

        public virtual void ShowException(Exception exception)
        {
            if (NoAppExceptionMessages) return;
            OnEnsureDebugSymbols();
            if (!Terminated && AppInitialized)
            {
                exception.HelpLink = "https://github.com/DiablosOffens/Moras.Net/issues";
                DialogResult result = DialogResult.OK;
                using (ThreadExceptionDialog dialog = new ThreadExceptionDialog(exception))
                {
                    if (!(exception is WarningException) && fiExcDialogButtonWidth != null &&
                        fiExcDialogDetailsButton != null && fiExcDialogHelpButton != null)
                    {
                        //HACK: use reflection to force display help button if it's not a WarningException.
                        int width = (int)fiExcDialogButtonWidth.GetValue(dialog);
                        Button detailsButton = (Button)fiExcDialogDetailsButton.GetValue(dialog);
                        Button helpButton = (Button)fiExcDialogHelpButton.GetValue(dialog);
                        helpButton.SetBounds(detailsButton.Left + width, detailsButton.Top, detailsButton.Width, detailsButton.Height);
                        dialog.Controls.Add(helpButton);
                    }
                    do
                    {
                        result = dialog.ShowDialog();
                        if (result == DialogResult.Yes)
                            Help.ShowHelp(null, exception.HelpLink, null);
                    } while (result == DialogResult.Yes);
                }
                switch (result)
                {
                    case DialogResult.Abort:
                        NoAppExceptionMessages = true;
                        Application.Exit();
                        Environment.Exit(0);
                        break;
                    default:
                        break;
                }
            }
            else
                Application.OnThreadException(exception);
        }
    }
}
