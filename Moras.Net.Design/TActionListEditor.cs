using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Design;
using DelphiClasses;

namespace Moras.Net.Design
{
    public class TActionListEditor : CollectionEditor
    {
        public TActionListEditor()
            : base(typeof(TContainedActionCollection))
        {

        }
        protected override Type[] CreateNewItemTypes()
        {
            return new Type[] { typeof(TAction), typeof(TFileExit), typeof(TFileOpen), typeof(TFileOpenWith), typeof(TFileSaveAs) };
        }
    }
}
