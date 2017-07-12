using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DelphiClasses.HelpIntfs
{
    [Guid("B0FC9366-5F0E-11D3-A3B9-00C04F79AD3A")]
    public interface IExtendedHelpViewer : ICustomHelpViewer
    {
        void DisplayHelpByContext(int ContextID, string HelpFileName);
        void DisplayTopic(string Topic);
        bool UnderstandsContext(int ContextID, string HelpFileName);
        bool UnderstandsTopic(string Topic);
    }
}
