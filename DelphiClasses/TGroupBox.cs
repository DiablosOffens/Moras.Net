using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

namespace DelphiClasses
{
    public class TGroupBox : GroupBox
    {
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public new event EventHandler Click { add { base.Click += value; } remove { base.Click -= value; } }
    }
}
