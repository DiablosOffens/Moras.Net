using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DelphiClasses.HelpIntfs
{
    [Guid("B0FC9364-5F0E-11D3-A3B9-00C04F79AD3A")]
    public interface ICustomHelpViewer
    {
        bool CanShowTableOfContents();
        TStringList GetHelpStrings(string HelpString);
        string GetViewerName();
        void NotifyID(int ViewerID);
        void ShowHelp(string HelpString);
        void ShowTableOfContents();
        void ShutDown();
        void SoftShutDown();
        int UnderstandsKeyword(string HelpString);
    }
}
