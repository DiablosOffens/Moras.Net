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
using System.Diagnostics;
using dxgettext;

namespace Moras.Net
{
    public partial class TfrmSetArmor : TCustomForm
    {
        static int idClass, idSubClass, iMatLevel, iAF, iBonus, iSubClass;
        private DynamicArray<int> arATData;	// Daten zur cbArmorType
        //---------------------------------------------------------------------------
        public TfrmSetArmor()
        {
            InitializeComponent();
            ((Bitmap)btCancel.Image).MakeTransparent();
            ((Bitmap)btOK.Image).MakeTransparent();

            int i;
            for (i = 94; i < 101; i++)
                cbQuality.Items.Add((i).ToString() + '%');
            cbQuality.SelectedIndex = 5;

            // Combobox für Rüstungsart ausfüllen
            arATData.Length = 0;
            for (i = 0; i < Unit.xml_config.nItemClasses; i++)
            {
                if ((Unit.xml_config.arItemClasses[i].SlotType == ESlotType.Armor)
                    && (Unit.xml_config.arItemClasses[i].oldid <= Unit.xml_config.arClasses[Unit.player.Class].iArmor)
                    && (Unit.xml_config.arItemClasses[i].iRealm & Unit.player.Realm) != 0
                    && (Unit.xml_config.arItemClasses[i].arSubClasses.Length > 0))
                {
                    cbArmorType.Items.Add(Unit.xml_config.arItemClasses[i].Name);
                    arATData.Length++;
                    arATData[arATData.Length - 1] = i;
                }
            }
            cbArmorType.SelectedIndex = cbArmorType.Items.Count - 1;
            cbArmorTypeChange(null, EventArgs.Empty);
            iAF = Unit.player.Level;
            if (Unit.xml_config.arItemClasses[idClass].idMaterial != Unit.xml_config.GetMaterialId("CLOTH"))
                iAF *= 2;
            iAF += Unit.xml_config.GetItemClassAF(iAF, idClass, out idSubClass, out iMatLevel);
            iSubClass = cbSubClass.FindStringExact(Unit.xml_config.arItemClasses[idClass].arSubClasses[idSubClass - 1].Name);
            cbSubClass.SelectedIndex = iSubClass;
            cbSubClassChange(null, EventArgs.Empty);
            cbMaterial.SelectedIndex = iMatLevel - Utils.ConvertTagToInt(cbMaterial.Tag);
            tbAF.Text = (iAF).ToString();
        }
        //---------------------------------------------------------------------------
        private void cbArmorTypeChange(object sender, EventArgs e)
        {
            idClass = arATData[cbArmorType.SelectedIndex];
            Debug.Assert(idClass != -1);
            // SubClass-Feld füllen
            cbSubClass.Clear();
            for (int i = 0; i < Unit.xml_config.arItemClasses[idClass].arSubClasses.Length; i++)
                cbSubClass.Items.Add(Unit.xml_config.arItemClasses[idClass].arSubClasses[i].Name);
            if (idSubClass < cbSubClass.Items.Count)
                cbSubClass.SelectedIndex = idSubClass;
            else
                cbSubClass.SelectedIndex = 0;
            cbSubClassChange(null, EventArgs.Empty);
        }
        //---------------------------------------------------------------------------
        private void cbSubClassChange(object sender, EventArgs e)
        {
            idSubClass = cbSubClass.SelectedIndex;
            // Materialfeld füllen
            int idMaterial = Unit.xml_config.arItemClasses[idClass].idMaterial;
            cbMaterial.Clear();
            cbMaterial.Tag = -1;
            for (int i = 0; i < 10; i++)
            {
                int val = Unit.xml_config.arItemClasses[idClass].arSubClasses[idSubClass].arValue[i];
                if (val > 0)
                {
                    if (Utils.ConvertTagToInt(cbMaterial.Tag) == -1) cbMaterial.Tag = i;
                    cbMaterial.Items.Add(Unit.xml_config.arMaterials[idMaterial].arLevel[i].Name);
                }
            }
            cbMaterial.SelectedIndex = iMatLevel - Utils.ConvertTagToInt(cbMaterial.Tag);
            if (cbMaterial.SelectedIndex < 0)
            {
                cbMaterial.SelectedIndex = 0;
                iMatLevel = Utils.ConvertTagToInt(cbMaterial.Tag);
            }
            cbMaterialChange(null, EventArgs.Empty);
        }
        //---------------------------------------------------------------------------
        private void cbMaterialChange(object sender, EventArgs e)
        {
            iMatLevel = cbMaterial.SelectedIndex + Utils.ConvertTagToInt(cbMaterial.Tag);
            // Entsprechenden Bonus eintragen
            if (iMatLevel > 2)
                iBonus = (iMatLevel - 2) * 5;
            else if (iMatLevel == 2)
                iBonus = 1;
            else
                iBonus = 0;
            tbBonus.Text = (iBonus).ToString() + "%";
            // Mal schauen welchen AF-Wert diese Stufe hat und eintragen
            iAF = Unit.xml_config.arItemClasses[idClass].arSubClasses[idSubClass].arValue[iMatLevel];
            tbAF.Text = (iAF).ToString();
        }
        //---------------------------------------------------------------------------
        private void btCancelClick(object sender, EventArgs e)
        {
            Unit.frmSetArmor.Close();
        }
        //---------------------------------------------------------------------------
        private void btOKClick(object sender, EventArgs e)
        {
            int i;
            // Die Daten in die entsprechenden Felder beim Spieler eintragen
            for (i = 0; i < 6; i++)
            {
                // Nur bei Gegenständen, die crafted sind
                if (Unit.player.ItemType[i] == EItemType.Crafted)
                {
                    Unit.player.ItemClass[i] = idClass;
                    Unit.player.ItemSubClass[i] = idSubClass + 1;
                    Unit.player.Material[i] = iMatLevel;
                    Unit.player.AF[i] = iAF;
                    Unit.player.Bonus[i] = iBonus;
                    Unit.player.Quality[i] = cbQuality.SelectedIndex + 94;
                    if (Unit.xml_config.arItemClasses[idClass].idMaterial == Unit.xml_config.GetMaterialId("CLOTH"))
                        Unit.player.ItemLevel[i] = iAF;
                    else
                        Unit.player.ItemLevel[i] = iAF / 2;
                }
            }
            Unit.frmSetArmor.Close();
        }
        //---------------------------------------------------------------------------
        private void TfrmSetArmor_FormCreate(object sender, EventArgs e)
        {
            TGnuGettextInstance.TranslateComponent(this);
            cbQuality.SelectedIndex = 5;
            cbArmorType.SelectedIndex = cbArmorType.Items.Count - 1;
            cbArmorTypeChange(null, EventArgs.Empty);
            cbSubClass.SelectedIndex = iSubClass;
            cbSubClassChange(null, EventArgs.Empty);
            cbMaterial.SelectedIndex = iMatLevel - Utils.ConvertTagToInt(cbMaterial.Tag);
        }
        //---------------------------------------------------------------------------
    }

    static partial class Unit
    {
        internal static TfrmSetArmor frmSetArmor;
    }
}
