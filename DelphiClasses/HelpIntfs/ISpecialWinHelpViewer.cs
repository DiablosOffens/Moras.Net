using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DelphiClasses.HelpIntfs
{
    [Guid("1A7B2224-1EAE-4313-BAD6-3C32F8F77085")]
    public interface ISpecialWinHelpViewer : IExtendedHelpViewer
    {
        bool CallWinHelp(UIntPtr Handle, string HelpFile, UInt16 Command, UIntPtr Data);
    }
}
