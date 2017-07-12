using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DelphiClasses.WinHelpViewer
{
    [Guid("B0FC9354-5F0E-11D3-A3B9-00C04F79AD3A")]
    public interface IWinHelpTester
    {
        bool CanShowALink(string ALink, string FileName);
        bool CanShowContext(int Context, string FileName);
        bool CanShowTopic(string Topic, string FileName);
        string GetDefaultHelpFile();
        string GetHelpPath();
        TStringList GetHelpStrings(string ALink);
    }

    public static class Unit
    {
        public static string ViewerName;
        public static IWinHelpTester WinHelpTester;
    }
}
