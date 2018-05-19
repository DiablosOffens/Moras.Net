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
using dxgettext;
using System.Diagnostics;
using System.IO;
using System.Collections;
//---------------------------------------------------------------------------

namespace Moras.Net
{
    public struct SItemLink
    {
        public CItem Item;
    }

    public partial class TfrmManageDB : TCustomForm
    {
        public const int USER_COLS = 3;

        // Suchoptionen
        public static int idItemClass;	// Rüstungsklasse
        private static int iRealm;
        private static int iClass;
        private static int iBasePos;
        private static int iPosition;
        private static int iType;
        private static int iMinLevel;
        private static int iMinUtility;
        private static int iMaxLevel;
        private static int iMaxUtility;
        private static int iBonusValue;
        private static int iOnlineDB;
        private static string sName;
        private static string sExtensions;
        // cache translation, as long as the gettext locale isn't changed after app start.
        private static string sSchmuck = _("Schmuck");
        private static string sJa = _("Ja");
        private static string sNein = _("Nein");

        private static DynamicArray<CItem> arSearchItems;

        private static string _(string szMsgId) { return TGnuGettextInstance.gettext(szMsgId); }
        protected override IContainer GetChildContainer() { return components ?? base.GetChildContainer(); }

        private bool bSaveDB;
        private bool bSearchMode;
        private bool bCalcTotalUtility;
        private bool bItemPreview;
        private int[] UserDefId = new int[USER_COLS];	// Das benutzerdefinierte Feld (BonusId)
        //	private TTBXItem mnColUserDef=new TTBXItem[USER_COLS];
        private TMainMenu.TTBXSubmenuItem[] mnColCustom = new TMainMenu.TTBXSubmenuItem[USER_COLS];

        public CItem SearchedItem;

        //---------------------------------------------------------------------------
        public TfrmManageDB()
        {
            InitializeComponent();
            ((Bitmap)bnCancel.Image).MakeTransparent();
            ((Bitmap)bnOk.Image).MakeTransparent();
            //Hint: Can't be moved to designer code, because it would be a great hack into the designer to
            //reference non-constant static objects, that are only constructed at runtime.
            cbRealm.ImageList = Unit.frmMain.ImageList1;
            ZQuery.Connection = Unit.frmMain.ZConnection;

            iClass = -1;
            iBasePos = -1;
            iPosition = -1;
            iType = -1;
            iMinLevel = -1;
            iMinUtility = -1;
            iMaxLevel = 100;
            iMaxUtility = 1000;
            iOnlineDB = -1;
            sName = "";
            bSaveDB = false;
            bSearchMode = false;
            SearchedItem = null;
            sExtensions = "";
            int i;
            int j;
            for (i = 0; i < Unit.xml_config.nExtensions; i++)
            {
                if (Utils.GetRegistryInteger(Unit.xml_config.arExtensions[i].Name, 1, "\\Extensions") == 0)
                {
                    if (sExtensions != "")
                        sExtensions = sExtensions + ",";

                    sExtensions = sExtensions + "'" + Unit.xml_config.arExtensions[i].id + "'";
                }
            }

            for (i = 0; i < USER_COLS; i++)
            {
                mnColCustom[i] = (TMainMenu.TTBXSubmenuItem)this.FindComponent("miUser" + (i + 1).ToString());
            }
            // Benutzerdefinirtes Menü aufbauen
            // Lade die letzten benutzerdefinierten Werte
            for (i = 0; i < USER_COLS; i++)
            {
                UserDefId[i] = Unit.xml_config.GetBonusId(Utils.GetRegistryString("UserDefinedCol" + (i).ToString(), ""));
                if (UserDefId[i] < 0)
                    UserDefId[i] = i; // i, damit jeder einen anderen Defaultwert hat
            }

            // Erstelle das Menü für die benutzerdefinierte Zeile
            for (i = 1; i < Unit.xml_config.nGroups; i++)
            {	// Erst die Gruppen erzeugen
                for (j = 0; j < USER_COLS; j++)
                {
                    TMainMenu.TTBXSubmenuItem mnItem = new TMainMenu.TTBXSubmenuItem();
                    mnItem.Text = Unit.xml_config.arGroups[i].Name;
                    mnItem.Tag = i;
                    mnItem.GroupIndex = 1;
                    //			mnItem.CheckOnClick = true;
                    mnItem.CheckedChanged += new EventHandler(mnColCustomCheckedChanged);
                    mnColCustom[j].DropDownItems.Add(mnItem);
                }
            }
            for (i = 0; i < Unit.xml_config.nBonuses; i++)
            {	// Und nun die Boni dort eintragen
                if ((Unit.xml_config.arBonuses[i].iRealm & Unit.player.Realm) != 0)
                {	// Boni nur hinzufügen, wenn im Spielerrealm möglich
                    for (j = 0; j < USER_COLS; j++)
                    {
                        if (Unit.xml_config.arBonuses[i].idGroup < 0)
                        {
                            string msg;
                            msg = "Fehlerhafte Gruppenzuordnung für den Bonus: " + Unit.xml_config.arBonuses[i].Names[0];
                            MessageBox.Show(msg, "XML-Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        }
                        TMainMenu.TTBXSubmenuItem parent = (TMainMenu.TTBXSubmenuItem)mnColCustom[j].DropDownItems[Unit.xml_config.arBonuses[i].idGroup - 1];
                        TMainMenu.TTBXItem mnItem = new TMainMenu.TTBXItem();
                        mnItem.Text = Unit.xml_config.arBonuses[i].Names[0];
                        mnItem.Tag = (j << 16) | i;
                        mnItem.GroupIndex = 2;
                        mnItem.CheckOnClick = true;
                        mnItem.Click += mnColCustomClick;
                        parent.DropDownItems.Add(mnItem);
                        if (UserDefId[j] == i)
                        {
                            mnItem.Checked = true;
                            parent.Checked = true;
                            vtItems.AllColumns[4 + j].Text = Unit.xml_config.arBonuses[i].Names[0];
                        }
                    }
                }
            }
        }
        //---------------------------------------------------------------------------
        ~TfrmManageDB()
        {
            FreeSearchedItems();
        }
        //---------------------------------------------------------------------------
        private void FillRealm()
        {
            cbRealm.Items.Clear();
            cbRealm.Add(_("<Alle>"), -1, 7);
            cbRealm.Add("Albion", 5, 1);
            cbRealm.Add("Hibernia", 6, 2);
            cbRealm.Add("Midgard", 7, 4);
            cbRealm.SelectData(Unit.player.Realm);
            cbRealmChange(null, EventArgs.Empty);
        }
        //---------------------------------------------------------------------------
        private void FillClass()
        {
            cbClass.Items.Clear();
            cbClass.Add(_("<Alle>"), -1, -1);
            for (int i = 0; i < Unit.xml_config.nClasses; i++)
            {
                if ((Unit.xml_config.arClasses[i].iRealm & iRealm) != 0)
                    cbClass.Add(Unit.xml_config.arClasses[i].Name, 0, i);
            }
            cbClass.SelectedIndex = 0;
        }
        //---------------------------------------------------------------------------
        private void FillPosition()
        {
            int i;

            cbPosition.Items.Clear();
            cbPosition.Add(_("<Alle>"), -1, -1);
            for (i = 0; i < CPlayer.PLAYER_ITEMS; i++)
            {
                string name = Unit.xml_config.arItemSlots[i].strPosName;
                cbPosition.SelectString(name);
                if (cbPosition.SelectedIndex == -1)
                {	// Wenn das gleich -1 ist, dann gibt es diesen Eintrag noch nicht
                    cbPosition.Add(name, -1, i);
                }
            }
            cbPosition.SelectedIndex = 0;
            cbType.Items.Clear();
            cbType.Add(_("<Alle>"), -1, -1);
            cbType.SelectedIndex = 0;
        }
        //---------------------------------------------------------------------------
        private void FillType()
        {
            cbType.Clear();

            if (!bSearchMode)
            {
                cbType.Add(_("<Alle>"), -1, -1);
                if (iPosition >= 0)
                {
                    for (int i = 0; i < Unit.xml_config.nItemClasses; i++)
                    {
                        if ((Unit.xml_config.arItemClasses[i].SlotType == Unit.xml_config.arItemSlots[iPosition].type)
                         && (Unit.xml_config.arItemClasses[i].iRealm & iRealm) != 0
                         && ((Unit.xml_config.arItemClasses[i].arSubClasses.Length == 0) || (Unit.xml_config.arItemSlots[iPosition].type == ESlotType.Weapon)))
                            cbType.Add(Unit.xml_config.arItemClasses[i].Name, -1, i);
                    }
                }
                else
                    iType = -1;
                cbType.SelectedIndex = 0;
                cbType.Enabled = (cbType.Items.Count > 1);
            }
            else
            {
                int i;
                bool bArmor = (Unit.xml_config.arItemSlots[iBasePos].type == ESlotType.Armor);
                bool bWeapon = (Unit.xml_config.arItemSlots[iBasePos].type == ESlotType.Weapon);
                // ComboBox für Rüstungstyp aufbauen wenn nötig
                if (bArmor)
                {
                    for (i = 0; i < Unit.xml_config.nItemClasses; i++)
                    {
                        if ((Unit.xml_config.arItemClasses[i].iRealm & Unit.player.Realm) != 0
                         && (Unit.xml_config.arItemClasses[i].SlotType == ESlotType.Armor)
                         && (Unit.xml_config.arItemClasses[i].arSubClasses.Length == 0)
                         && (Unit.xml_config.arItemClasses[i].oldid <= Unit.xml_config.arClasses[Unit.player.Class].iArmor))
                        {
                            cbType.Add(Unit.xml_config.arItemClasses[i].Name, -1, i);
                        }
                    }
                    cbType.SelectedIndex = cbType.Items.Count - 1;
                }
                if (bWeapon)
                {
                    for (i = 0; i < Unit.xml_config.nItemClasses; i++)
                    {
                        if ((Unit.xml_config.arItemClasses[i].iRealm & Unit.player.Realm) != 0
                         && (Unit.xml_config.arItemClasses[i].SlotType == ESlotType.Weapon)
                         && (Unit.xml_config.arItemClasses[i].bmPositions & (1 << iBasePos)) != 0)
                        {
                            // Nun sollte geschaut werden, ob die Klasse diese Waffenart tragen kann
                            if (Unit.xml_config.arItemClasses[i].idSkill >= 0)
                            {
                                for (int j = 0; j < Unit.xml_config.arClasses[Unit.player.Class].nAllSkills; j++)
                                {
                                    if (Unit.xml_config.arItemClasses[i].idSkill == Unit.xml_config.arClasses[Unit.player.Class].arSkills[j])
                                    {
                                        // Jetzt noch eine Spezialbehandlung für Schilde
                                        if ((iBasePos != 7) || ((Unit.xml_config.arAttributes[Unit.xml_config.arItemClasses[i].idSkill].id != "SHIELD")
                                         || (Unit.xml_config.arClasses[Unit.player.Class].iShield >= Unit.xml_config.arItemClasses[i].iSize))
                                         && (!Unit.xml_config.arItemClasses[i].bDualWield || Unit.xml_config.arClasses[Unit.player.Class].BDualWield))
                                        {
                                            cbType.Add(Unit.xml_config.arItemClasses[i].Name, -1, i);
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {	// Keinen Skill aus Vorraussetzung . Können alle nutzen
                                cbType.Add(Unit.xml_config.arItemClasses[i].Name, -1, i);
                            }
                        }
                    }
                    cbType.SelectedIndex = 0;
                }
                cbTypeChange(null, EventArgs.Empty);
                cbType.Enabled = (cbType.Items.Count > 1);
            }
        }
        //---------------------------------------------------------------------------
        private void FillBonus()
        {
            int i;

            cbBonus.Clear();
            cbBonus.Add(_("<keiner>"), -1, -1);
            for (i = 0; i < Unit.xml_config.nBonuses; ++i)
            {
                // nur bonus vom gewähltem reich oder für alle reiche anzeigen
                if (Unit.xml_config.arBonuses[i].iRealm == iRealm || Unit.xml_config.arBonuses[i].iRealm == 7 || iRealm == 7)
                    cbBonus.Add(Unit.xml_config.arBonuses[i].Names[0], -1, i);
            }
            cbBonus.SelectedIndex = 0;
        }
        //---------------------------------------------------------------------------
        private void FillOnlineDB()
        {
            string sql = "select provider from items group by provider";
            ZQuery.CommandText = sql;
            ZQuery.Open();
            cbOnlineDB.Items.Clear();
            cbOnlineDB.Items.Add(_("<Alle>"));
            string provider = "";
            while (!ZQuery.GetEof())
            {
                provider = ZQuery.FieldByName("provider").AsString;
                cbOnlineDB.Items.Add(provider);
                ZQuery.Next();
            }
            ZQuery.Close();
            cbOnlineDB.SelectedIndex = 0;
        }
        //---------------------------------------------------------------------------
        private void InitFilter()
        {
            FillRealm();
            FillPosition();
            FillBonus();
            FillOnlineDB();
        }
        //---------------------------------------------------------------------------

        private void TfrmManageDB_FormCreate(object sender, EventArgs e)
        {
            TGnuGettextInstance.TranslateComponent(this);
            LoadSettings();
            InitFilter();
            Unit.player.SetPreviewMode(true);
            Unit.player.UpdateAttributes();
        }
        //---------------------------------------------------------------------------

        private void cbRealmChange(object sender, EventArgs e)
        {
            iRealm = cbRealm.CurrentData;
            FillClass();
            FillBonus();
            cbPositionChange(null, EventArgs.Empty);
        }
        //---------------------------------------------------------------------------

        private void cbClassChange(object sender, EventArgs e)
        {
            iClass = cbClass.CurrentData;
        }
        //---------------------------------------------------------------------------

        private void cbPositionChange(object sender, EventArgs e)
        {
            iPosition = cbPosition.CurrentData;
            FillType();
        }
        //---------------------------------------------------------------------------

        private void cbTypeChange(object sender, EventArgs e)
        {
            iType = cbType.CurrentData;
        }
        //---------------------------------------------------------------------------

        private void cbMinLevelChange(object sender, EventArgs e)
        {
            if (cbMinLevel.SelectedIndex <= 0)
                iMinLevel = -102;
            else
                iMinLevel = int.Parse(cbMinLevel.Text);
        }
        //---------------------------------------------------------------------------

        private void cbMinUtilityChange(object sender, EventArgs e)
        {
            if (cbMinUtility.SelectedIndex <= 0)
                iMinUtility = -1;
            else
                iMinUtility = int.Parse(cbMinUtility.Text);
        }
        //---------------------------------------------------------------------------

        private void bnSearchClick(object sender, EventArgs e)
        {
            sName = edName.Text.ToLower();
            iBonusValue = edBonus.Text.ToIntDef(0);
            SearchForItems();
        }
        //---------------------------------------------------------------------------
        private string FilterSQL(string fsql, string sql)
        {
            if (fsql != "")
                fsql = fsql + " and ";
            fsql = fsql + sql;

            return fsql;
        }
        //---------------------------------------------------------------------------
        private void SearchForItems()
        {
            int i;

            Cursor cCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                stStatus.Text = _("Suche Gegenstände...");
                Application.DoEvents();
                vtItems.ClearObjects();
                FreeSearchedItems();
                string sql = "";
                string fsql = "";
                sql = "select *";
                sql = sql + " from items i";
                fsql = FilterSQL(fsql, "i.level >= " + (iMinLevel).ToString());
                fsql = FilterSQL(fsql, "i.level <= " + (iMaxLevel).ToString());
                if (sExtensions != "")
                    fsql = FilterSQL(fsql, "i.extension not in (" + sExtensions + ")");
                if (iRealm != 7)
                    fsql = FilterSQL(fsql, "i.realm in (7," + (iRealm).ToString()) + ")";
                if (iPosition >= 0)
                    fsql = FilterSQL(fsql, "i.position = " + (iPosition).ToString());
                if (iType >= 0)
                    fsql = FilterSQL(fsql, "i.class = " + (iType).ToString());
                if (sName != "")
                    fsql = FilterSQL(fsql, "i.name like '%" + sName + "%'");
                if (cbBonus.CurrentData >= 0 && iBonusValue > 0)
                    fsql = FilterSQL(fsql, "i.effects like '%" + Unit.xml_config.arBonuses[cbBonus.CurrentData].id + "%'");
                if (iOnlineDB > 0)
                    fsql = FilterSQL(fsql, "i.provider = " + Utils.QuotedStr((string)cbOnlineDB.Items[iOnlineDB], '\''));

                if (fsql != "")
                    sql = sql + " where " + fsql;
                if (Unit.xml_config.bDebugSQL)
                    Utils.DebugPrint(sql);
                ZQuery.CommandText = sql;
                ZQuery.Open();

                bool bFound;
                int index;
                CItem Item;
                CPlayerAttributes xTempAttributes = new CPlayerAttributes(); // Kopie der aktuellen Attribute

                if (bCalcTotalUtility && bSearchMode)
                {
                    xTempAttributes = Unit.player.Attributes;
                    if (Unit.player.Item[iBasePos].bEquipped)
                        xTempAttributes -= Unit.player.Item[iBasePos];
                }

                while (!ZQuery.GetEof())
                {
                    Item = Unit.ItemDB.GetItem(ZQuery);
                    Item.sNutzen = Item.CalcNutzen();
                    bFound = true;

                    // Kann Item von der Klasse benutzt werden?
                    if (!Item.isUseable(iClass))
                        bFound = false;
                    else if (iMinUtility > -1 && Item.sNutzen < iMinUtility)
                        bFound = false;
                    else if (iMaxUtility < 1000 && Item.sNutzen > iMaxUtility)
                        bFound = false;
                    else if (cbBonus.CurrentData >= 0 && Item.GetBonusEffect(cbBonus.CurrentData) < iBonusValue)
                        bFound = false;

                    // Wenn bFound hier noch true, dann Item in Liste übernehmen
                    if (bFound)
                    {
                        if (bCalcTotalUtility)
                        {
                            if (bSearchMode)
                            {
                                xTempAttributes += Item;
                                Item.sGesamt = xTempAttributes.CalcGesamtNutzen(Unit.player.Weights);
                                xTempAttributes -= Item;
                            }
                            else
                                Item.sGesamt = Item.sNutzen;
                        }
                        arSearchItems.Length++;
                        arSearchItems[arSearchItems.Length - 1] = Item;
                    }
                    else
                    {
                        //delete Item;
                        Item = null;
                    }
                    ZQuery.Next();
                }
                ZQuery.Close();

                stStatus.Text = (arSearchItems.Length).ToString() + _(" Gegenstände gefunden.");
                Application.DoEvents();
                vtItems.ObjectsCount = arSearchItems.Length;
                //vtItems.Sort(vtItems.PrimarySortColumn, vtItems.PrimarySortOrder);
            }
            finally
            {
                Cursor.Current = cCursor;
            }
        }
        //---------------------------------------------------------------------------

        private void vtItemsGetText(object sender, GetCellTextEventArgs e)
        {
            CItem pItem;

            pItem = ((SItemLink)e.Model).Item;
            switch (e.ColumnIndex)
            {
                // Name
                case 0: e.CellText = pItem.Name;
                    break;
                // Nutzen
                case 1: e.CellText = pItem.sNutzen.ToString("0.0");
                    break;
                // Gesamtnutzen
                case 2: e.CellText = pItem.sGesamt.ToString("0.0");
                    break;
                // Level
                case 3: e.CellText = (pItem.Level).ToString();
                    break;
                // Benutzer 1
                case 4: e.CellText = (pItem.GetBonusEffect(UserDefId[0])).ToString();
                    break;
                // Benutzer 2
                case 5: e.CellText = (pItem.GetBonusEffect(UserDefId[1])).ToString();
                    break;
                // Benutzer 3
                case 6: e.CellText = (pItem.GetBonusEffect(UserDefId[2])).ToString();
                    break;
                // Position
                case 7: e.CellText = Unit.xml_config.arItemSlots[pItem.Position].strPosName;
                    break;
                // Art
                case 8: if (pItem.Class >= 0)
                        e.CellText = Unit.xml_config.arItemClasses[pItem.Class].Name;
                    else
                        e.CellText = sSchmuck;
                    break;
                // Artefakt
                case 9: e.CellText = (pItem.MaxLevel == 0) ? sNein : sJa;
                    break;
                // Reich
                case 10: e.CellText = Utils.Realm2Str(pItem.Realm);
                    break;
                // Online-DB
                case 11: e.CellText = pItem.Provider;
                    break;
                // Erweiterung
                case 12: e.CellText = ((pItem.Extension != "") ? Unit.xml_config.arExtensions[Unit.xml_config.GetExtensionId(pItem.Extension)].Name : "");
                    break;
                // Letzte Änderung
                case 13: e.CellText = (pItem.LastUpdate).ToShortDateString();
                    break;
            }
        }
        //---------------------------------------------------------------------------

        private void vtItemsChange(object sender, EventArgs e)
        {
            object model = vtItems.SelectedObject;
            if (model != null)
            {
                CItem pItem = ((SItemLink)model).Item;
                if (pItem != null)
                {
                    gbData.Text = pItem.Name;
                    lbData.Text = pItem.LongInfo;
                    acInternet.Enabled = (pItem.OnlineURL != "");
                    if (bSearchMode && bItemPreview)
                        ItemPreview(pItem);
                }
            }
        }
        //---------------------------------------------------------------------------

        private void bnInternetClick(object sender, EventArgs e)
        {
            CItem pItem = ((SItemLink)vtItems.FocusedObject).Item;
            if (pItem != null && pItem.OnlineURL != "")
                Extensions.ShellExecute(TApplication.Instance.Handle, "open", pItem.OnlineURL, null, null, ProcessWindowStyle.Normal);
        }
        //---------------------------------------------------------------------------

        private void vtItemsHeaderClick(object sender, ColumnClickEventArgs e)
        {
            BrightIdeasSoftware.OLVColumn newsortcol = vtItems.GetColumn(e.Column);
            if (vtItems.PrimarySortColumn == newsortcol)
            {
                // already handled by base class
                //if (vtItems.PrimarySortOrder == SortOrder.Ascending)
                //    vtItems.PrimarySortOrder = SortOrder.Descending;
                //else
                //    vtItems.PrimarySortOrder = SortOrder.Ascending;
            }
            else
            {
                if (vtItems.PrimarySortColumn != null)
                    vtItems.PrimarySortColumn.Renderer = null;
                // already handled by base class
                //vtItems.PrimarySortColumn = newsortcol;
                //vtItems.PrimarySortOrder = SortOrder.Ascending;
                newsortcol.Renderer = sortColumnRenderer;
                this.Invalidate();
            }
            // already handled by base class
            //vtItems.Sort(vtItems.PrimarySortColumn, vtItems.PrimarySortOrder);
        }
        //---------------------------------------------------------------------------
        private void vtItemsCompareModels(object sender, CompareModelsEventArgs e)
        {
            string sTmp1, sTmp2;
            CItem pItem1 = ((SItemLink)e.LeftModel).Item;
            CItem pItem2 = ((SItemLink)e.RightModel).Item;

            switch (e.ColumnIndex)
            {
                case 0: e.Result = string.Compare(pItem1.Name, pItem2.Name);
                    break;
                case 1: e.Result = Comparer<int>.Default.Compare((int)(pItem1.sNutzen * 100), (int)(pItem2.sNutzen * 100));
                    break;
                case 2: e.Result = Comparer<int>.Default.Compare((int)(pItem1.sGesamt * 100), (int)(pItem2.sGesamt * 100));
                    break;
                case 3: e.Result = Comparer<int>.Default.Compare(pItem1.Level, pItem2.Level);
                    break;
                case 4: e.Result = Comparer<int>.Default.Compare(pItem1.GetBonusEffect(UserDefId[0]), pItem2.GetBonusEffect(UserDefId[0]));
                    break;
                case 5: e.Result = Comparer<int>.Default.Compare(pItem1.GetBonusEffect(UserDefId[1]), pItem2.GetBonusEffect(UserDefId[1]));
                    break;
                case 6: e.Result = Comparer<int>.Default.Compare(pItem1.GetBonusEffect(UserDefId[2]), pItem2.GetBonusEffect(UserDefId[2]));
                    break;
                case 7: e.Result = string.Compare(Unit.xml_config.arItemSlots[pItem1.Position].strPosName, Unit.xml_config.arItemSlots[pItem2.Position].strPosName);
                    break;
                case 8: if (pItem1.Class >= 0)
                        sTmp1 = Unit.xml_config.arItemClasses[pItem1.Class].Name;
                    else
                        sTmp1 = sSchmuck;
                    if (pItem2.Class >= 0)
                        sTmp2 = Unit.xml_config.arItemClasses[pItem2.Class].Name;
                    else
                        sTmp2 = sSchmuck;
                    e.Result = string.Compare(sTmp1, sTmp2);
                    break;
                case 9: e.Result = Comparer<int>.Default.Compare(pItem1.MaxLevel, pItem2.MaxLevel);
                    break;
                case 10: e.Result = string.Compare(Utils.Realm2Str(pItem1.Realm), Utils.Realm2Str(pItem2.Realm));
                    break;
                case 11: e.Result = string.Compare(pItem1.Provider, pItem2.Provider);
                    break;
                case 12: e.Result = string.Compare(pItem1.Extension, pItem2.Extension);
                    break;
                case 13: e.Result = Comparer<DateTime>.Default.Compare(pItem1.LastUpdate, pItem2.LastUpdate);
                    break;
            }
        }
        //---------------------------------------------------------------------------
        private void vtItemsGetNodeModelType(object sender, GetNodeModelTypeEventArgs e)
        {
            e.ModelType = typeof(SItemLink);
        }
        //---------------------------------------------------------------------------

        private void vtItemsInitModel(object sender, InitModelEventArgs e)
        {
            e.InitModelValue((ref SItemLink link) =>
            {
                link.Item = arSearchItems[e.Index];
            });
        }
        //---------------------------------------------------------------------------

        private void cbMaxLevelChange(object sender, EventArgs e)
        {
            if (cbMaxLevel.SelectedIndex <= 0)
                iMaxLevel = 102;
            else
                iMaxLevel = int.Parse(cbMaxLevel.Text);
        }
        //---------------------------------------------------------------------------

        private void cbMaxUtilityChange(object sender, EventArgs e)
        {
            if (cbMaxUtility.SelectedIndex <= 0)
                iMaxUtility = 1000;
            else
                iMaxUtility = int.Parse(cbMaxUtility.Text);
        }
        //---------------------------------------------------------------------------

        private void bnDeleteClick(object sender, EventArgs e)
        {
            IEnumerator itmodel = vtItems.SelectedObjects.GetEnumerator();
            if (itmodel != null && itmodel.MoveNext())
            {
                bSaveDB = true;
                do
                {
                    CItem pItem = ((SItemLink)itmodel.Current).Item;
                    Unit.ItemDB.DeleteItem(pItem);
                }
                while (itmodel.MoveNext());
                vtItems.RemoveObjects(vtItems.SelectedObjects);
                ZQuery.Connection.DoCommit();
            }
        }
        //---------------------------------------------------------------------------

        private void bnExportClick(object sender, EventArgs e)
        {
            IEnumerator itmodel = vtItems.SelectedObjects.GetEnumerator();
            if (itmodel != null && itmodel.MoveNext())
            {
                if (SaveDialog.ShowDialog() == DialogResult.OK)
                {
                    CXml xFile = new CXml();
                    if (xFile.OpenSaveXml(SaveDialog.FileName))
                    {
                        xFile.OpenTag("daoc_items", "", "");
                        do
                        {
                            CItem pItem = ((SItemLink)itmodel.Current).Item;
                            pItem.Save(xFile);
                        }
                        while (itmodel.MoveNext());
                        xFile.CloseTag();
                        xFile.CloseXml();
                    }
                    vtItems.SelectedObjects = null;
                }
            }
        }
        //---------------------------------------------------------------------------

        private void FormClose(object sender, FormClosedEventArgs e)
        {
            SaveSettings();
            Unit.player.SetPreviewMode(false);
            Unit.player.UpdateAttributes();
            //    if (bSaveDB)
            //        Unit.ItemDB.Save();
        }
        //---------------------------------------------------------------------------
        private void LoadSettings()
        {
            Utils.LoadWindowPosition("ManageDB", this, true);
            bCalcTotalUtility = Utils.GetRegistryInteger("CalcTotalUtility", 1) != 0;
            bItemPreview = Utils.GetRegistryInteger("ItemPreview", 1) != 0;
            if (!bCalcTotalUtility)
            {
                vtItems.AllColumns[2].IsVisible = false;
                vtItems.RebuildColumns();
            }

            for (int i = 0; i < vtItems.AllColumns.Count; ++i)
            {
                vtItems.AllColumns[i].Width = Utils.GetRegistryInteger("ColumnSize" + (i).ToString(), vtItems.AllColumns[i].Width);
            }

            pData.Width = Utils.GetRegistryInteger("PanelData", pData.Width);

            cbMinLevel.SelectedIndex = Utils.GetRegistryInteger("MinLevel", 0);
            cbMinLevelChange(null, EventArgs.Empty);
            cbMaxLevel.SelectedIndex = Utils.GetRegistryInteger("MaxLevel", 0);
            cbMaxLevelChange(null, EventArgs.Empty);
            cbMinUtility.SelectedIndex = Utils.GetRegistryInteger("MinUtility", 0);
            cbMinUtilityChange(null, EventArgs.Empty);
            cbMaxUtility.SelectedIndex = Utils.GetRegistryInteger("MaxUtility", 0);
            cbMaxUtilityChange(null, EventArgs.Empty);
        }
        //---------------------------------------------------------------------------
        private void SaveSettings()
        {
            Utils.SaveWindowPosition("ManageDB", this, true);
            for (int i = 0; i < vtItems.AllColumns.Count; ++i)
            {
                Utils.SetRegistryInteger("ColumnSize" + (i).ToString(), vtItems.AllColumns[i].Width);
            }
            Utils.SetRegistryInteger("PanelData", pData.Width);
            Utils.SetRegistryInteger("MinLevel", cbMinLevel.SelectedIndex);
            Utils.SetRegistryInteger("MaxLevel", cbMaxLevel.SelectedIndex);
            Utils.SetRegistryInteger("MinUtility", cbMinUtility.SelectedIndex);
            Utils.SetRegistryInteger("MaxUtility", cbMaxUtility.SelectedIndex);
        }
        //---------------------------------------------------------------------------
        public void SearchMode(int iPos)
        {
            iBasePos = iPos;
            if (iPos < CPlayer.PLAYER_ITEMS)
            {
                cbRealm.Enabled = false;
                cbClass.Enabled = false;
                cbPosition.Enabled = false;
                iPosition = iPos;
                bSearchMode = true;
            }
            else
            {
                iPosition = -1;
            }
            bnOk.Visible = true;
            Text = _("Suche Gegenstand");
            cbClass.SelectData(Unit.player.Class);
            cbClassChange(null, EventArgs.Empty);
            cbPosition.SelectString(Unit.xml_config.arItemSlots[iPos].strPosName);
            cbPositionChange(null, EventArgs.Empty);
            FillType();
        }
        //---------------------------------------------------------------------------

        private void vtItemsDblClick(object sender, EventArgs e)
        {
            bnOkClick(null, EventArgs.Empty);
        }
        //---------------------------------------------------------------------------

        private void bnOkClick(object sender, EventArgs e)
        {
            object model = vtItems.FocusedObject;

            if (bnOk.Visible && model != null)
            {
                SearchedItem = ((SItemLink)model).Item;
                DialogResult = DialogResult.OK;
            }
        }
        //---------------------------------------------------------------------------

        private void Form_Show(object sender, EventArgs e)
        {
            bnSearchClick(null, EventArgs.Empty);
        }
        //---------------------------------------------------------------------------
        //private Single GesamtNutzen(CItem item);
        private void ItemPreview(CItem Item = null)
        {
            if (Item == null)
                Item = ((SItemLink)vtItems.FocusedObject).Item;

            if (Item != null)
            {
                Unit.player.PreviewItem[iBasePos] = Item;
                Unit.player.DisplayAttributes();
            }
        }
        //---------------------------------------------------------------------------
        static TMainMenu.TTBXItem lastitem = null;
        private void mnColCustomClick(object sender, EventArgs e)
        {

            TMainMenu.TTBXItem mi = (TMainMenu.TTBXItem)sender;
            int bid = Utils.ConvertTagToInt(mi.Tag) & 0xffff;
            int user = Utils.ConvertTagToInt(mi.Tag) >> 16;
            mi.Checked = true;
            ((TMainMenu.TTBXCustomItem)mi.OwnerItem).Checked = true;
            // die Zeile deshalb, weil radioitem nur im aktuellen menu funktioniert
            //	if (lastitem != NULL)
            //        lastitem.Checked = false;
            lastitem = mi;
            //	mnColUserDef[user].Caption = xml_config.arBonuses[bid].Names[0];
            vtItems.AllColumns[4 + user].Text = Unit.xml_config.arBonuses[bid].Names[0];
            UserDefId[user] = bid;
            // Speichere den letzten benutzerdefinierten Wert in Registry
            Utils.SetRegistryString("UserDefinedCol" + (user).ToString(), Unit.xml_config.arBonuses[bid].id);
            // Und die Anzeige updaten
            vtItems.Refresh();
        }
        private void mnColCustomCheckedChanged(object sender, EventArgs e)
        {
            TMainMenu.TTBXSubmenuItem pmi = (TMainMenu.TTBXSubmenuItem)sender;
            if (!pmi.Checked)
            {
                foreach (TMainMenu.TTBXItem mi in pmi.DropDownItems)
                {
                    mi.Checked = false;
                }
            }
        }
        //---------------------------------------------------------------------------
        private void cbOnlineDBChange(object sender, EventArgs e)
        {
            iOnlineDB = cbOnlineDB.SelectedIndex;
        }
        //---------------------------------------------------------------------------
        private void FreeSearchedItems()
        {
            //CItem pItem;

            //for (int i = 0; i < arSearchItems.Length; i++)
            //{
            //    pItem = (CItem)arSearchItems[i];
            //    delete pItem;
            //    pItem = null;
            //}
            arSearchItems.Length = 0;
        }
        //---------------------------------------------------------------------------
    }

    static partial class Unit
    {
        internal static TfrmManageDB frmManageDB;
    }
}
