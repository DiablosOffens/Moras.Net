using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace DelphiClasses
{
    public class TFileOpenWith : TFileOpen
    {
        [Browsable(true)]
        public new string FileName { get; set; }
        [Browsable(true)]
        public event EventHandler AfterOpen;

        public TFileOpenWith(IContainer cont)
            : base(cont)
        {

        }
    }
}
