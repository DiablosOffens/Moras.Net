using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelphiClasses;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Deployment.Application;
using System.Threading;

namespace Moras.Net
{
    //Github clickonce deployment: https://refactorsaurusrex.com/post/2015/how-to-host-a-clickonce-installer-on-github/
    //http://www.codeproject.com/Articles/5756/Use-the-ApplicationContext-Class-to-Fully-Encapsul
    static partial class Program
    {
        static Program()
        {
            RuntimeHelpers.RunClassConstructor(typeof(Wikihelp.Unit).TypeHandle);
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                TApplication.Instance.EnsureDebugSymbols += new EventHandler(DownloadDebugSymbols);
                TApplication.Instance.Initialize();
                TApplication.Instance.Title = "Moras Ausrüstungsplaner";
                TApplication.Instance.HelpFile = "camelot.allakhazam.com";
                TApplication.Instance.CreateForm(out Unit.frmMain);
                TApplication.Instance.CreateForm(out Unit.frmImport);
                Application.Run(TApplication.Instance);
            }
            catch (Exception exception)
            {
                TApplication.Instance.ShowException(exception);
            }
        }

        static void DownloadDebugSymbols(object sender, EventArgs e)
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
                if (!ad.IsFileGroupDownloaded("PDB"))
                {
                    try
                    {
                        using (ManualResetEvent updateCompleted = new ManualResetEvent(false))
                        {
                            using (Unit.frmProgress)
                            {
                                TApplication.Instance.CreateForm(out Unit.frmProgress);
                                Unit.frmProgress.ControlBox = false;
                                ad.DownloadFileGroupProgressChanged += (sender2, e2) =>
                                {
                                    if (Unit.frmProgress.pbBar.InvokeRequired)
                                        Unit.frmProgress.pbBar.Invoke(new Action<int>(v => Unit.frmProgress.pbBar.Value = v), e2.ProgressPercentage);
                                    else
                                        Unit.frmProgress.pbBar.Value = e2.ProgressPercentage;
                                };
                                ad.DownloadFileGroupCompleted += (sender2, e2) =>
                                {
                                    updateCompleted.Set();
                                    if (Unit.frmProgress.InvokeRequired)
                                        Unit.frmProgress.Invoke(new Action(Unit.frmProgress.Close));
                                    else
                                        Unit.frmProgress.Close();
                                };
                                ad.DownloadFileGroupAsync("PDB");
                                Unit.frmProgress.ShowDialog();
                            }
                            updateCompleted.WaitOne();
                        }
                    }
                    catch (DeploymentDownloadException /*dde*/)
                    {
                        /*string msg = _("Die aktuellste Version kann nicht installiert werden.") + Environment.NewLine +
                            _("Bitte überprüfen Sie Ihre Netzwerkverbindung oder versuchen Sie es später noch einmal.") + Environment.NewLine +
                            _("Fehler: ") + dde.Message;
                        string title = _("Fehler");
                        MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;*/
                    }
                }
            }
        }

        [PrePrepareMethod]
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            throw e.IsTerminating ? new SystemException(e.ExceptionObject.ToString()) : new WarningException(e.ExceptionObject.ToString());
        }
    }
}
