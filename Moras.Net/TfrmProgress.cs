//---------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DelphiClasses;

//---------------------------------------------------------------------------
namespace Moras.Net
{
    public partial class TfrmProgress : TCustomForm
    {
        //---------------------------------------------------------------------------
        public TfrmProgress()
        {
            InitializeComponent();
        }
    }

    static partial class Unit
    {
        internal static TfrmProgress frmProgress;
    }
}
//---------------------------------------------------------------------------
