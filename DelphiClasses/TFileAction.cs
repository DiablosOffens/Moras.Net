using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;

namespace DelphiClasses
{
    public abstract class TFileAction : TCommonDialogAction
    {
        public TFileAction(IContainer cont)
            : base(cont)
        {
        }

        protected string FileName
        {
            get
            {
                return GetDialog().FileName;
            }
            set
            {
                GetDialog().FileName = value;
            }
        }

        protected FileDialog GetDialog()
        {
            return (FileDialog)dialog;
        }
    }
}
