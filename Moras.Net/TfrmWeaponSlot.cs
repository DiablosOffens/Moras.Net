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

namespace Moras.Net
{
    public partial class TfrmWeaponSlot : TCustomForm
    {
        private static string _(string szMsgId) { return TGnuGettextInstance.gettext(szMsgId); }
        protected override IContainer GetChildContainer() { return components ?? base.GetChildContainer(); }

        public CItem pItem;
        //---------------------------------------------------------------------------
        public TfrmWeaponSlot()
        {
            InitializeComponent();
            ((Bitmap)btCancel.Image).MakeTransparent();
            ((Bitmap)btOK.Image).MakeTransparent();
        }
        //---------------------------------------------------------------------------
        private void Form_Show(object sender, EventArgs e)
        {
            // Das Feld für die Rüstungs-/Waffenart ausfüllen
            for (int i = 0; i < Unit.xml_config.nItemClasses; i++)
            {
                if ((Unit.xml_config.arItemClasses[i].SlotType == ESlotType.Weapon)
                    && (Unit.xml_config.arItemClasses[i].iRealm & pItem.Realm) != 0)
                {
                    cbWeaponClass.Add(Unit.xml_config.arItemClasses[i].Name, -1, i);
                }
            }
            cbWeaponClass.SelectedIndex = 0;
            cbWeaponClassChange(null, EventArgs.Empty);
        }
        //---------------------------------------------------------------------------
        private void cbWeaponClassChange(object sender, EventArgs e)
        {	// Schadensart(en) herausfinden
            int wid = cbWeaponClass.CurrentData;
            cbDamageType.Clear();
            for (int i = 0; i < Unit.xml_config.arItemClasses[wid].arSubClasses.Length; i++)
            {
                int idDamage = Unit.xml_config.arItemClasses[wid].arSubClasses[i].idDamage;
                if (idDamage >= 0)
                {	// Nur Damageliste ausfüllen, wenn die "Waffe" ne Schadensart hat
                    string sDamage = Unit.xml_config.arDamageTypes[idDamage].Name;
                    if (cbDamageType.Items.IndexOf(sDamage) == -1)
                        cbDamageType.Add(sDamage, -1, idDamage);
                }
            }
            if (cbDamageType.Items.Count == 0)
                cbDamageType.Add(_("<keine>"), -1, -1);
            cbDamageType.Enabled = (cbDamageType.Items.Count > 1);
            cbDamageType.SelectedIndex = 0;
        }
        //---------------------------------------------------------------------------
        
        private void TfrmWeaponSlot_FormCreate(object sender, EventArgs e)
        {
            TGnuGettextInstance.TranslateComponent(this);
        }
        //---------------------------------------------------------------------------
    }

    static partial class Unit
    {
        internal static TfrmWeaponSlot frmWeaponSlot;
    }
}
