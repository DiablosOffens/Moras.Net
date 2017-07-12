using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DelphiClasses.HelpIntfs
{
    [Guid("B0FC9358-5F0E-11D3-A3B9-00C04F79AD3A")]
    public interface IHelpSelector
    {
        int SelectKeyword(TStringList Keywords);
        int TableOfContents(TStringList Contents);
    }
}
