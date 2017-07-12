using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DelphiClasses.HelpIntfs
{
    [Guid("006A65EE-9A5E-4EB1-828F-A7F1D79DD202")]
    public interface IHelpSystem3 : IHelpSystem2
    {
        string GetFilter();
        void SetFilter(string Filter);
        void ShowIndex(string Topic, string HelpFileName);
        void ShowSearch(string Topic, string HelpFileName);
        void ShowTopicHelp(string Topic, string Anchor, string HelpFileName);
    }
}
