using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelphiClasses;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

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
                TApplication.Instance.Initialize();
                TApplication.Instance.Title = "Moras Ausrüstungsplaner";
                TApplication.Instance.HelpFile = "daocpedia.eu";
                TApplication.Instance.CreateForm(out Unit.frmMain);
                TApplication.Instance.CreateForm(out Unit.frmImport);
                Application.Run(TApplication.Instance);
            }
            catch (Exception exception)
            {
                TApplication.Instance.ShowException(exception);
            }
        }

        [PrePrepareMethod]
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            throw e.IsTerminating ? new SystemException(e.ExceptionObject.ToString()) : new WarningException(e.ExceptionObject.ToString());
        }
    }
}
