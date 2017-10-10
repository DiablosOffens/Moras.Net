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
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using DelphiClasses;
using dxgettext;
using Moras.Net.Components;
using Microsoft.VisualBasic.PowerPacks;

namespace Moras.Net
{
    public partial class TfrmOptions : TCustomForm
    {
        private static TMainMenu.TTBXCustomItem CraftMenu, CraftCurrent;

        private static string _(string szMsgId) { return TGnuGettextInstance.gettext(szMsgId); }
        protected override IContainer GetChildContainer() { return components ?? base.GetChildContainer(); }
        //---------------------------------------------------------------------------
        public TfrmOptions()
        {
            InitializeComponent();
            PageControl1.SelectedTab = TabSheet6;

            CraftCurrent = null;
            PageControl1.SelectedIndex = 0;
        }
        //---------------------------------------------------------------------------

        private void TfrmOptions_FormShow(object sender, EventArgs e)
        {
            // Jetzt irgendwie die Box mit den Preismodellen ausfüllen
            for (int i = 0; i < Unit.xml_config.arMenuItems.Length; i++)
            {
                if (Unit.xml_config.arMenuItems[i].bPricing)
                {
                    cbPriceModel.Add(Unit.xml_config.arMenuItems[i].Name, -1, i);
                    // Schauen, ob dieses Modell aktiv ist
                    CraftMenu = (TMainMenu.TTBXCustomItem)Utils.FindTBXItem(Unit.frmMain.Options1, Unit.xml_config.arMenuItems[i].Group);
                    Debug.Assert(CraftMenu != null);
                    TMainMenu.TTBXCustomItem item = (TMainMenu.TTBXCustomItem)Utils.FindTBXItem(CraftMenu, Unit.xml_config.arMenuItems[i].Name);
                    Debug.Assert(item != null);
                    if (item.Checked)
                        cbPriceModel.SelectData(i);
                }
            }
            cbPriceModelChange(null, EventArgs.Empty);
            // Erstelle Combobox für Bannzauberer-Stufe
            for (int i = 0; i < 20; i++)
            {
                cbSCLevel.Add((i * 50 + 1).ToString() + " - " + ((i + 1) * 50).ToString(), -1, -50 + i * 5);
            }
            cbSCLevel.Add("1001+", -1, 50);
            cbSCLevel.SelectData(Unit.xml_config.iSCLevel);
            chHalfIP.Checked = Unit.xml_config.bHalfIP;
            chEnglish.Checked = Unit.xml_config.bKortEnglish;
            chIgnoreRaceBoni.Checked = Unit.xml_config.bIgnoreRaceBoni;
            rgUpdateMode.ItemIndex = Utils.GetRegistryInteger("UpdateMode", 0);
            cbOverwriteItems.Checked = Utils.GetRegistryInteger("OverwriteItems", 0) != 0;
            cbUseProxy.Checked = Utils.GetRegistryInteger("UseProxy", 0) != 0;
            cbAlternateLogin.Checked = Utils.GetRegistryInteger("AlternateLogin", 0) != 0;
            edServer.Text = Utils.GetRegistryString("ProxyServer", "");
            edPort.Text = Utils.GetRegistryString("ProxyPort", "");
            edUser.Text = Utils.GetRegistryString("ProxyUser", "");
            edPasswd.Text = Utils.GetRegistryString("ProxyPasswd", "");
            cbIPWorkaround.Checked = Utils.GetRegistryInteger("IPWorkaround", 0) != 0;
            cbCalcTotalUtility.Checked = Utils.GetRegistryInteger("CalcTotalUtility", 1) != 0;
            cbItemPreview.Checked = Utils.GetRegistryInteger("ItemPreview", 1) != 0;
            cbWikiHelp.Checked = Utils.GetRegistryInteger("WikiHelp", 1) != 0;
            cbLoadChars.Checked = Utils.GetRegistryInteger("LoadChars", 1) != 0;
            cbCheckForUpdate.Checked = Utils.GetRegistryInteger("CheckForUpdate", 1) != 0;
            cbProcess.Checked = Utils.GetRegistryInteger("ProcessPriority", 0) != 0;
            rgUpdateIntervall.ItemIndex = Utils.GetRegistryInteger("UpdateIntervall", 0);

            // Laden der Erweiterungen
            for (int ext = 0; ext < Unit.xml_config.nExtensions; ext++)
            {
                cbAddons.Items.Add(Unit.xml_config.arExtensions[ext].Name);
                cbAddons.SetItemChecked(ext, Utils.GetRegistryInteger(Unit.xml_config.arExtensions[ext].Name, 1, "\\Extensions") != 0);
            }

            // Stat-Dialog initialisieren:
            rgStatDisplay.ItemIndex = Utils.GetRegistryInteger("StatDisplayMode", 2);
            edDisplayStatFormula.Text = Utils.GetRegistryString("StatDisplayFormula", "%W%(%C-w%|%M-c%)");
            // Die Breite springt dauernd auf 33...wieder korrigieren.
            MCapExample.Width = 198;
            // Die Farben laden
            shFloorCol.BackColor = Utils.Int2Color(Utils.GetRegistryInteger("StatDisplayFloorCol", (int)Utils.Color2Int(Color.White)));
            shFloorCol.Tag = 0;

            shCapCol.BackColor = Utils.Int2Color(Utils.GetRegistryInteger("StatDisplayCapCol", (int)Utils.Color2Int(Color.Gray)));
            shCapCol.Tag = 1;

            shCapIncCol.BackColor = Utils.Int2Color(Utils.GetRegistryInteger("StatDisplayCapIncCol", 0x00aa0000));
            shCapIncCol.Tag = 2;

            shCapSubCol.BackColor = Utils.Int2Color(Utils.GetRegistryInteger("StatDisplayCapSubCol", 0x000000cc));
            shCapSubCol.Tag = 3;

            shCapAddCol.BackColor = Utils.Int2Color(Utils.GetRegistryInteger("StatDisplayCapAddCol", 0x0000cc00));
            shCapAddCol.Tag = 4;

            shValueCol.BackColor = Utils.Int2Color(Utils.GetRegistryInteger("StatDisplayValueCol", (int)Utils.Color2Int(Color.Gray)));
            shValueCol.Tag = 5;

            shValueSubCol.BackColor = Utils.Int2Color(Utils.GetRegistryInteger("StatDisplayValueSubCol", 0x006666dd));
            shValueSubCol.Tag = 6;

            shValueAddCol.BackColor = Utils.Int2Color(Utils.GetRegistryInteger("StatDisplayValueAddCol", 0x0066dd66));
            shValueAddCol.Tag = 7;

            shTextCol.BackColor = Utils.Int2Color(Utils.GetRegistryInteger("StatDisplayTextCol", (int)Utils.Color2Int(SystemColors.ControlText)));
            shTextCol.Tag = 8;

            shTextMouseOverCol.BackColor = Utils.Int2Color(Utils.GetRegistryInteger("StatDisplayTextMouseOverColCol", (int)Utils.Color2Int(Color.Blue)));
            shTextMouseOverCol.Tag = 9;
        }
        //---------------------------------------------------------------------------

        // Diese Funktion validiert die eingegebenen Zeichen für ein Gold-Inputfeld
        private void GoldKeyPress(object sender, KeyPressEventArgs e)
        {
            string Text = ((TextBox)sender).Text;
            bool bValid = false;
            // Erstmal die allgemein zulässigen Zeichen rausfiltern
            if (((e.KeyChar >= '0') && (e.KeyChar <= '9')) || (e.KeyChar < 32) || (e.KeyChar == ' ')
                || (e.KeyChar == 'g') || (e.KeyChar == 's') || (e.KeyChar == 'k'))
            {
                bValid = true;	// Im Moment einfach alles als kupfer zählen
            }
            if (!bValid)
            {
                e.KeyChar = '\0';
                System.Media.SystemSounds.Beep.Play();
            }
        }
        //---------------------------------------------------------------------------


        private void tbPpGemChange(object sender, EventArgs e)
        {
            Unit.xml_config.sPriceModel.PPGem = Utils.Gold2Int(tbPpGem.Text);
        }
        //---------------------------------------------------------------------------

        private void tbPpItemChange(object sender, EventArgs e)
        {
            Unit.xml_config.sPriceModel.PPItem = Utils.Gold2Int(tbPpItem.Text);
        }
        //---------------------------------------------------------------------------

        private void tbPpOrderChange(object sender, EventArgs e)
        {
            Unit.xml_config.sPriceModel.PPOrder = Utils.Gold2Int(tbPpOrder.Text);
        }
        //---------------------------------------------------------------------------

        // Keypress Händler für die Eingabeboxen mit Komma-Zahlenwerten
        private void KommaKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!((e.KeyChar >= '0' && e.KeyChar <= '9') || e.KeyChar == '.' || e.KeyChar == ',') && e.KeyChar > 31)
            {
                e.KeyChar = '\0';
                System.Media.SystemSounds.Beep.Play();
            }
        }
        //---------------------------------------------------------------------------

        private void tbGeneralMarkupChange(object sender, EventArgs e)
        {
            Unit.xml_config.sPriceModel.pGeneralMarkup = (int)(Utils.Str2Decimal(tbGeneralMarkup.Text) * 10);
        }
        //---------------------------------------------------------------------------
        private void tbPpLevelChange(object sender, EventArgs e)
        {
            Unit.xml_config.sPriceModel.PPLevel = Utils.Gold2Int(tbPpLevel.Text);
        }
        //---------------------------------------------------------------------------

        private void tbPpIPChange(object sender, EventArgs e)
        {
            Unit.xml_config.sPriceModel.PPIP = Utils.Gold2Int(tbPpIP.Text);
        }
        //---------------------------------------------------------------------------

        private void tbPpOCChange(object sender, EventArgs e)
        {
            Unit.xml_config.sPriceModel.PPOC = Utils.Gold2Int(tbPpOC.Text);
        }
        //---------------------------------------------------------------------------

        private void chQualityClick(object sender, EventArgs e)
        {
            Unit.xml_config.sPriceModel.iGQMarkup = chQuality.Checked;
        }
        //---------------------------------------------------------------------------

        private void cbQualityChange(object sender, EventArgs e)
        {
            tbQuality.Text = (Unit.xml_config.sPriceModel.pGQMarkup[cbQuality.SelectedIndex] * 0.1).ToString("###0.0");
        }
        //---------------------------------------------------------------------------

        private void tbQualityChange(object sender, EventArgs e)
        {
            Unit.xml_config.sPriceModel.pGQMarkup[cbQuality.SelectedIndex] = (int)(Utils.Str2Decimal(tbQuality.Text) * 10);
        }
        //---------------------------------------------------------------------------

        private void tbTierChange(object sender, EventArgs e)
        {
            Unit.xml_config.sPriceModel.PPGTier[cbTier.SelectedIndex] = Utils.Gold2Int(tbTier.Text);
        }
        //---------------------------------------------------------------------------

        private void cbTierChange(object sender, EventArgs e)
        {
            tbTier.Text = Utils.Int2Gold(Unit.xml_config.sPriceModel.PPGTier[cbTier.SelectedIndex]);
        }
        //---------------------------------------------------------------------------

        private void tbHourChange(object sender, EventArgs e)
        {
            Unit.xml_config.sPriceModel.PPHour = Utils.Gold2Int(tbHour.Text);
        }
        //---------------------------------------------------------------------------

        private void chTierClick(object sender, EventArgs e)
        {
            Unit.xml_config.sPriceModel.iPPGTier = chTier.Checked;
        }
        //---------------------------------------------------------------------------

        private void chCostClick(object sender, EventArgs e)
        {
            Unit.xml_config.sPriceModel.iCost = chCost.Checked;
        }
        //---------------------------------------------------------------------------

        private void cbPriceModelChange(object sender, EventArgs e)
        {
            // Lade das zugeordnete xml
            Unit.xml_config.OpenConfig(Unit.xml_config.arMenuItems[cbPriceModel.CurrentData].FileName);
            tbPpGem.Text = Utils.Int2Gold(Unit.xml_config.sPriceModel.PPGem);
            tbPpItem.Text = Utils.Int2Gold(Unit.xml_config.sPriceModel.PPItem);
            tbPpOrder.Text = Utils.Int2Gold(Unit.xml_config.sPriceModel.PPOrder);
            tbPpLevel.Text = Utils.Int2Gold(Unit.xml_config.sPriceModel.PPLevel);
            tbPpIP.Text = Utils.Int2Gold(Unit.xml_config.sPriceModel.PPIP);
            tbPpOC.Text = Utils.Int2Gold(Unit.xml_config.sPriceModel.PPOC);
            tbHour.Text = Utils.Int2Gold(Unit.xml_config.sPriceModel.PPHour);
            tbGeneralMarkup.Text = (Unit.xml_config.sPriceModel.pGeneralMarkup * 0.1).ToString("###0.0");
            chQuality.Checked = Unit.xml_config.sPriceModel.iGQMarkup;
            chTier.Checked = Unit.xml_config.sPriceModel.iPPGTier;
            chCost.Checked = Unit.xml_config.sPriceModel.iCost;
            cbQualityChange(null, EventArgs.Empty);
            cbTierChange(null, EventArgs.Empty);
        }
        //---------------------------------------------------------------------------

        private void btCancelClick(object sender, EventArgs e)
        {	// Lade wieder das eingestellte Preismodell
            if (CraftCurrent != null)
            {
                Unit.xml_config.OpenConfig(Unit.xml_config.arMenuItems[Utils.ConvertTagToInt(CraftCurrent.Tag)].FileName);
            }
            TMCapView.UpdateColors(); // Stelle wieder die Farben zurück
            TMCapView.UpdateFormat(); // Stelle wieder Formatierung zurück
        }
        //---------------------------------------------------------------------------

        private void btOkClick(object sender, EventArgs e)
        {
            Utils.SetRegistryInteger("UpdateMode", rgUpdateMode.ItemIndex);
            Utils.SetRegistryInteger("OverwriteItems", cbOverwriteItems.Checked ? 1 : 0);
            Utils.SetRegistryInteger("UseProxy", cbUseProxy.Checked ? 1 : 0);
            Utils.SetRegistryInteger("AlternateLogin", cbAlternateLogin.Checked ? 1 : 0);
            Utils.SetRegistryString("ProxyServer", edServer.Text);
            Utils.SetRegistryString("ProxyPort", edPort.Text);
            Utils.SetRegistryString("ProxyUser", edUser.Text);
            Utils.SetRegistryString("ProxyPasswd", edPasswd.Text);
            Utils.SetRegistryInteger("IPWorkaround", cbIPWorkaround.Checked ? 1 : 0);
            Utils.SetRegistryInteger("CalcTotalUtility", cbCalcTotalUtility.Checked ? 1 : 0);
            Utils.SetRegistryInteger("ItemPreview", cbItemPreview.Checked ? 1 : 0);
            Utils.SetRegistryInteger("WikiHelp", cbWikiHelp.Checked ? 1 : 0);
            Utils.SetRegistryInteger("LoadChars", cbLoadChars.Checked ? 1 : 0);
            Utils.SetRegistryInteger("CheckForUpdate", cbCheckForUpdate.Checked ? 1 : 0);
            Utils.SetRegistryInteger("ProcessPriority", cbProcess.Checked ? 1 : 0);
            Utils.SetRegistryInteger("UpdateIntervall", rgUpdateIntervall.ItemIndex);

            Utils.SetRegistryInteger("StatDisplayMode", rgStatDisplay.ItemIndex);
            Utils.SetRegistryString("StatDisplayFormula", edDisplayStatFormula.Text);
            Utils.SetRegistryInteger("StatDisplayFloorCol", (int)Utils.Color2Int(shFloorCol.BackColor));
            Utils.SetRegistryInteger("StatDisplayCapCol", (int)Utils.Color2Int(shCapCol.BackColor));
            Utils.SetRegistryInteger("StatDisplayCapIncCol", (int)Utils.Color2Int(shCapIncCol.BackColor));
            Utils.SetRegistryInteger("StatDisplayCapAddCol", (int)Utils.Color2Int(shCapAddCol.BackColor));
            Utils.SetRegistryInteger("StatDisplayCapSubCol", (int)Utils.Color2Int(shCapSubCol.BackColor));
            Utils.SetRegistryInteger("StatDisplayValueCol", (int)Utils.Color2Int(shValueCol.BackColor));
            Utils.SetRegistryInteger("StatDisplayValueAddCol", (int)Utils.Color2Int(shValueAddCol.BackColor));
            Utils.SetRegistryInteger("StatDisplayValueSubCol", (int)Utils.Color2Int(shValueSubCol.BackColor));
            Utils.SetRegistryInteger("StatDisplayTextCol", (int)Utils.Color2Int(shTextCol.BackColor));
            Utils.SetRegistryInteger("StatDisplayTextMouseOverColCol", (int)Utils.Color2Int(shTextMouseOverCol.BackColor));
            TMCapView.UpdateColors();
            TMCapView.UpdateFormat();

            // gewähltes Preismodell speichern
            Unit.xml_config.SavePriceModel(cbPriceModel.CurrentData);
            // Und zugehörigen Menüpunkt aktivieren
            if (CraftMenu != null)
            {
                TMainMenu.TTBXCustomItem item = (TMainMenu.TTBXCustomItem)Utils.FindTBXItem(CraftMenu, Unit.xml_config.arMenuItems[cbPriceModel.CurrentData].Name);
                Debug.Assert(item != null);
                Unit.frmMain.mnUserOptionClick(item, EventArgs.Empty);
            }

            //Speichern der Erweiterungen
            for (int ext = 0; ext < Unit.xml_config.nExtensions; ext++)
            {
                Utils.SetRegistryInteger(Unit.xml_config.arExtensions[ext].Name, cbAddons.GetItemChecked(ext) ? 1 : 0, "\\Extensions");
            }
        }
        //---------------------------------------------------------------------------

        private void GoldExit(object sender, EventArgs e)
        {
            TextBox edit = (TextBox)sender;
            int gold = Utils.Gold2Int(edit.Text);
            edit.Text = Utils.Int2Gold(gold);
        }
        //---------------------------------------------------------------------------

        private void KommaExit(object sender, EventArgs e)
        {
            TextBox edit = (TextBox)sender;
            decimal Gold = Utils.Str2Decimal(edit.Text);
            edit.Text = (Gold).ToString("###0.0");
        }
        //---------------------------------------------------------------------------

        private void btNewClick(object sender, EventArgs e)
        {
            // Hier ist einiges zu tun
            // Erstmal einen Namen besorgen
            TApplication.Instance.CreateForm(out Unit.frmNewName);
            DialogResult ret = Unit.frmNewName.ShowDialog();
            if ((ret == DialogResult.OK) && (Unit.frmNewName.tbName.Text.Length > 0))
            {
                // Neuen Menueintrag erzeugen
                Debug.Assert(CraftMenu != null);
                int idx = Unit.xml_config.arMenuItems.Length;
                Unit.xml_config.arMenuItems.Length++;
                Unit.xml_config.arMenuItems.Array[idx].bPricing = true;
                Unit.xml_config.arMenuItems.Array[idx].Name = Unit.frmNewName.tbName.Text;
                Unit.xml_config.arMenuItems.Array[idx].Group = CraftMenu.Name;
                string dataPath;
                if (ApplicationDeployment.IsNetworkDeployed)
                    dataPath = Utils.IncludeTrailingPathDelimiter(ApplicationDeployment.CurrentDeployment.DataDirectory);
                else
                    dataPath = Utils.IncludeTrailingPathDelimiter(Path.GetDirectoryName(Application.ExecutablePath));
                Unit.xml_config.arMenuItems.Array[idx].FileName = dataPath + "options\\sc_" + Unit.frmNewName.tbName.Text + ".xml";
                TMainMenu.TTBXItem NewItem = new TMainMenu.TTBXItem();
                Debug.Assert(NewItem != null);
                NewItem.Text = Unit.frmNewName.tbName.Text;
                NewItem.Tag = idx;
                NewItem.GroupIndex = Utils.ConvertTagToInt(CraftMenu.Tag);
                NewItem.Click += Unit.frmMain.mnUserOptionClick;
                CraftMenu.DropDownItems.Add(NewItem);
                cbPriceModel.Add(Unit.frmNewName.tbName.Text, -1, idx);
                cbPriceModel.SelectData(idx);
            }
            Unit.frmNewName.Dispose();
        }
        //---------------------------------------------------------------------------

        private void cbSCLevelChange(object sender, EventArgs e)
        {
            Utils.SetRegistryString("SCLevel", (cbSCLevel.CurrentData).ToString());
            Unit.xml_config.iSCLevel = cbSCLevel.CurrentData;
        }
        //---------------------------------------------------------------------------

        private void chHalfIPClick(object sender, EventArgs e)
        {
            Utils.SetRegistryString("HalfIP", (chHalfIP.Checked ? 1 : 0).ToString());
            Unit.xml_config.bHalfIP = chHalfIP.Checked;
        }
        //---------------------------------------------------------------------------

        private void chEnglishClick(object sender, EventArgs e)
        {
            Utils.SetRegistryString("KortEnglish", (chEnglish.Checked ? 1 : 0).ToString());
            Unit.xml_config.bKortEnglish = chEnglish.Checked;
        }
        //---------------------------------------------------------------------------

        private void chIgnoreRaceBoniClick(object sender, EventArgs e)
        {
            Utils.SetRegistryString("IgnoreRaceBoni", (chIgnoreRaceBoni.Checked ? 1 : 0).ToString());
            Unit.xml_config.bIgnoreRaceBoni = chIgnoreRaceBoni.Checked;
        }
        //---------------------------------------------------------------------------

        private void TfrmOptions_FormCreate(object sender, EventArgs e)
        {
            TGnuGettextInstance.TranslateComponent(this);
        }
        //---------------------------------------------------------------------------



        private void capViewChange(object sender, EventArgs e)
        {
            if (sender == edBonusFloor)
                MCapExample.Floor = edBonusFloor.Text.ToIntDef(0);
            if (sender == edBonusBaseCap || sender == edBonusCapIncCap)
            {
                MCapExample.CapBase = edBonusBaseCap.Text.ToIntDef(75);
                MCapExample.CapIncCap = edBonusCapIncCap.Text.ToIntDef(101) - edBonusBaseCap.Text.ToIntDef(75);
            }
            if (sender == edBonusValue || sender == edBonusValueChange)
            {
                MCapExample.Value = edBonusValue.Text.ToIntDef(0);
                MCapExample.ValueChange = edBonusValueChange.Text.ToIntDef(0) - edBonusValue.Text.ToIntDef(0);
            }
            if (sender == edBonusCapInc || sender == edBonusCapIncChange)
            {
                MCapExample.CapInc = edBonusCapInc.Text.ToIntDef(0);
                MCapExample.CapIncChange = edBonusCapIncChange.Text.ToIntDef(0) - edBonusCapInc.Text.ToIntDef(0);
            }
            if (sender == edBonusOvercap || sender == edBonusOvercapChange)
            {
                MCapExample.Overcap = edBonusOvercap.Text.ToIntDef(0);
                MCapExample.OvercapChange = edBonusOvercapChange.Text.ToIntDef(0) - edBonusOvercap.Text.ToIntDef(0);
            }
            MCapExample.Width = 198;
        }
        //---------------------------------------------------------------------------



        private void ShapeOnClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                cdColDialog.Color = ((RectangleShape)sender).BackColor;
                if (cdColDialog.ShowDialog(this) == DialogResult.OK)
                {
                    ((RectangleShape)sender).BackColor = cdColDialog.Color;
                    TMCapView.SetColor(Utils.ConvertTagToInt(((RectangleShape)sender).Tag), cdColDialog.Color);
                    MCapExample.Refresh();
                }
            }
        }
        //---------------------------------------------------------------------------

        //already handled by shape
        /*private void ShapeContext(object sender, HandledMouseEventArgs e)
        {
            Point abspos = ((RectangleShape)sender).PointToScreen(e.Location);
            //pmStatColors.SourceControl = ((RectangleShape)sender).Parent;
            pmStatColors.Show(((RectangleShape)sender).Parent, abspos.X, abspos.Y);
        }*/
        //---------------------------------------------------------------------------

        private void ShapeNewCol(object sender, EventArgs e)
        {
            ContextMenuStrip menu = (ContextMenuStrip)(((ToolStripMenuItem)sender).GetCurrentParent());
            ShapeContainer container = (ShapeContainer)(menu).SourceControl;
            RectangleShape pShape = (RectangleShape)container.GetChildAtPoint(container.PointToClient(menu.Location));
            cdColDialog.Color = pShape.BackColor;
            if (cdColDialog.ShowDialog(this) == DialogResult.OK)
            {
                pShape.BackColor = cdColDialog.Color;
                TMCapView.SetColor(Utils.ConvertTagToInt(pShape.Tag), cdColDialog.Color);
                MCapExample.Refresh();
            }
        }
        //---------------------------------------------------------------------------

        private void ShapeRestoreCol(object sender, EventArgs e)
        {
            Color[] OldCols = new Color[10] { Color.White, Color.Gray, Utils.Int2Color(0x00aa0000), Utils.Int2Color(0x000000cc), Utils.Int2Color(0x0000cc00), Color.Gray,
                Utils.Int2Color(0x006666dd), Utils.Int2Color(0x0066dd66), SystemColors.ControlText, Color.Blue };

            ContextMenuStrip menu = (ContextMenuStrip)(((ToolStripMenuItem)sender).GetCurrentParent());
            ShapeContainer container = (ShapeContainer)(menu).SourceControl;
            RectangleShape pShape = (RectangleShape)container.GetChildAtPoint(container.PointToClient(menu.Location)); ;
            if (Utils.ConvertTagToInt(pShape.Tag) >= 0 && Utils.ConvertTagToInt(pShape.Tag) < 10)
            {
                pShape.BackColor = OldCols[Utils.ConvertTagToInt(pShape.Tag)];
                TMCapView.SetColor(Utils.ConvertTagToInt(pShape.Tag), OldCols[Utils.ConvertTagToInt(pShape.Tag)]);
                MCapExample.Refresh();
            }
        }
        //---------------------------------------------------------------------------

        private void StatModusChange(object sender, EventArgs e)
        {
            int newmode = rgStatDisplay.ItemIndex;
            if (newmode == 3)
                edDisplayStatFormula.Enabled = true;
            else
            {
                switch (newmode)
                {
                    case 0: edDisplayStatFormula.Text = "%w%(%c-w%)"; break;
                    case 1: edDisplayStatFormula.Text = "%w%(%C-w%|%M-c%)"; break;
                    case 2: edDisplayStatFormula.Text = "%W%(%C-w%|%M-c%)"; break;
                }
                edDisplayStatFormula.Enabled = false;
            }
        }
        //---------------------------------------------------------------------------
    }

    static partial class Unit
    {
        internal static TfrmOptions frmOptions;
    }
}
