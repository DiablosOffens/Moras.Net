using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DelphiClasses;

namespace dxgettext
{
    internal class TClassMode
    {
        internal Type HClass;
        internal TTranslator SpecialHandler;
        internal TStringList PropertiesToIgnore; // This is ignored if Handler is set
        public TClassMode()
        {
            PropertiesToIgnore = new TStringList();
            PropertiesToIgnore.Sorted = true;
            PropertiesToIgnore.Duplicates = TDuplicates.dupError;
            PropertiesToIgnore.CaseSensitive = false;
        }
    }
}
