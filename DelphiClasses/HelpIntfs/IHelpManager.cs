using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DelphiClasses.HelpIntfs
{
    [Guid("6B0CDB05-C30A-414B-99C4-F11CD195385E")]
    public interface IHelpManager
    {
        UIntPtr GetHandle();
        string GetHelpFile();
        void Release(int ViewerID);
    }
}
