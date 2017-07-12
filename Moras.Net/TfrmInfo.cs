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
using dxgettext;

//---------------------------------------------------------------------------
namespace Moras.Net
{
    public partial class TfrmInfo : TCustomForm
    {
        //---------------------------------------------------------------------------
        public TfrmInfo()
        {
            InitializeComponent();
            ((Bitmap)btClose.Image).MakeTransparent();
        }
        //---------------------------------------------------------------------------
        private void TfrmInfo_FormCreate(object sender, EventArgs e)
        {
            TGnuGettextInstance.TranslateComponent(this);    
        }
    }

    static partial class Unit
    {
        internal static TfrmInfo frmInfo;
    }
}
//---------------------------------------------------------------------------
