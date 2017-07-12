using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DelphiClasses.HelpIntfs
{
    [Guid("B0FC9353-5F0E-11D3-A3B9-00C04F79AD3A")]
    public interface IHelpSystem
    {
        void AssignHelpSelector(IHelpSelector Selector);
        bool Hook(UIntPtr Handle, string HelpFile, UInt16 Comand, IntPtr Data);
        void ShowContextHelp(int ContextID, string HelpFileName);
        void ShowHelp(string HelpKeyword, string HelpFileName);
        void ShowTableOfContents();
        void ShowTopicHelp(string Topic, string HelpFileName);
    }
}
