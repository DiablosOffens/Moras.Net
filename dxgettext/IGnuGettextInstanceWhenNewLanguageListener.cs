using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dxgettext
{
    public interface IGnuGettextInstanceWhenNewLanguageListener
    {
        void WhenNewLanguage(string LanguageID);
    }
}
