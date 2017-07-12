using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DelphiClasses.HelpIntfs
{
    [Guid("48C5336E-71E2-4406-A08E-F915FBA5C9D4")]
    public interface IHelpSystem2 : IHelpSystem
    {
        bool UnderstandsKeyword(string HelpKeyword, string HelpFileName);
    }
}
