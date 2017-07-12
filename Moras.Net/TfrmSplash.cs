/*****************************************************************************/
/*                                                                           */
/* Moras Ausrüstungsplaner für Dark Age of Camelot                           */
/* Copyright (C) 2003 - 2004  Mora                                           */
/*                                                                           */
/* This program is free software; you can redistribute it and/or modify      */
/* it under the terms of the GNU General Public License as published by      */
/* the Free Software Foundation; either version 2 of the License, or         */
/* (at your option) any later version.                                       */
/*                                                                           */
/* This program is distributed in the hope that it will be useful,           */
/* but WITHOUT ANY WARRANTY; without even the implied warranty of            */
/* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the             */
/* GNU General Public License for more details.                              */
/*                                                                           */
/* You should have received a copy of the GNU General Public License         */
/* along with this program; if not, write to the Free Software               */
/* Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA */
/*                                                                           */
/*****************************************************************************/

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
using Moras.Net.Properties;
using dxgettext;

namespace Moras.Net
{
    public partial class TfrmSplash : TCustomForm
    {
        private static string _(string szMsgId) { return TGnuGettextInstance.gettext(szMsgId); }

        public TfrmSplash()
        {
            InitializeComponent();
            try
            {
                Image1.Image = Resources.SPLASH; //Bitmap.FromResource(IntPtr.Zero, "SPLASH");
                ClientHeight = Image1.Height;
                ClientWidth = Image1.Width;
            }
            catch
            {
                Utils.MorasErrorMessage(_("Fehler beim Laden der Ressource für das Splash-Bild!"), _("Resourcen Fehler!"));
            }
        }
        //---------------------------------------------------------------------------
        public void SetCaption(string ACaption, bool BStep = true)
        {
            lbInfo.Text = ACaption;
            if (BStep)
                pbLoad.PerformStep();
            Application.DoEvents();
        }
        //---------------------------------------------------------------------------
    }

    static partial class Unit
    {
        internal static TfrmSplash frmSplash;
    }
}
