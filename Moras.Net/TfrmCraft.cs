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
using System.Diagnostics;

namespace Moras.Net
{
    public partial class TfrmCraft : TCustomForm
    {
        private static string _(string szMsgId) { return TGnuGettextInstance.gettext(szMsgId); }
        protected override IContainer GetChildContainer() { return components ?? base.GetChildContainer(); }

        private CheckBox[] chDone = new CheckBox[4];
        private TLabel[] lbGem = new TLabel[4];
        private TextBox[] tbRemakes = new TextBox[4];
        private TUpDown[] udRemakes = new TUpDown[4];
        private TextBox[] tbTime = new TextBox[4];
        private TLabel[] lbCost = new TLabel[4];
        private TSpeedButton[] btClock = new TSpeedButton[4];
        private int[] iBid = new int[4];	// Die Bonus-IDs der Gems
        private bool bLock;
        private int iLastClock;
        private DateTime dtStart;	// Startzeit

        //---------------------------------------------------------------------------
        public TfrmCraft()
        {
            InitializeComponent();

            bLock = true;
            iLastClock = -1;
        }
        //---------------------------------------------------------------------------
        private void TfrmCraft_FormShow(object sender, EventArgs e)
        {
            int i, iItemCost = 0;
            Utils.LoadWindowPosition("Crafting", this);
            for (i = 0; i < 4; i++)
            {
                chDone[i] = (CheckBox)this.FindComponent("chDone" + (i + 1).ToString());
                lbGem[i] = (TLabel)this.FindComponent("lbGem" + (i + 1).ToString());
                tbRemakes[i] = (TextBox)this.FindComponent("tbRemakes" + (i + 1).ToString());
                udRemakes[i] = (TUpDown)this.FindComponent("udRemakes" + (i + 1).ToString());
                tbTime[i] = (TextBox)this.FindComponent("tbTime" + (i + 1).ToString());
                lbCost[i] = (TLabel)this.FindComponent("lbCost" + (i + 1).ToString());
                btClock[i] = (TSpeedButton)this.FindComponent("btClock" + (i + 1).ToString());
                chDone[i].Checked = Unit.frmMain.chDone[i].Checked;
                // GemName erzeugen
                int iQuality = Unit.player.EffectQuality[Unit.frmMain.iActSlot][i] + 96;
                iBid[i] = Unit.player.ItemEffect[Unit.frmMain.iActSlot][i];
                if ((iBid[i] == -1) || !Unit.xml_config.arBonuses[iBid[i]].bCraftable)
                {	// Gem/Effekt ist nicht craftbar/vorhanden
                    iBid[i] = -1;
                    chDone[i].Enabled = false;
                    lbGem[i].Visible = false;
                    tbRemakes[i].Enabled = false;
                    tbTime[i].Enabled = false;
                    lbCost[i].Visible = false;
                    btClock[i].Enabled = false;
                }
                else
                {
                    lbGem[i].Text = (iQuality).ToString() + "% " + Unit.frmMain.lbGem[i].Text;
                    chDoneClick(chDone[i], EventArgs.Empty);
                }
                // Die Remakes
                udRemakes[i].Value = Unit.frmMain.udRemakes[i].Value;
                iItemCost += Unit.player.GemCost[Unit.frmMain.iActSlot][i];
                lbCost[i].Text = Utils.Int2Gold(Unit.player.GemCost[Unit.frmMain.iActSlot][i]);
                tbTime[i].Text = ((Unit.player.Time[Unit.frmMain.iActSlot][i] + 59) / 60).ToString();
            }
            lbItemCost.Text = Utils.Int2Gold(iItemCost);
            tbTries.Text = Utils.GetRegistryString("ExpectedTries", "6");
            UpdateMaterials();
            bLock = false;
        }
        //---------------------------------------------------------------------------
        private void FormClose(object sender, FormClosedEventArgs e)
        {
            Utils.SaveWindowPosition("Crafting", this);
            // Beim Schliessen werden die Craftzeiten in Minuten übernommen
            for (int i = 0; i < 4; i++)
            {	// Und wenn Timer Aktuiv, deaktivieren
                if (btClock[i].Checked)
                {
                    btClock[i].Checked = false;
                    btClockClick(btClock[i], EventArgs.Empty);
                }
                //		tbTimeChange(tbTime[i]);
            }
        }
        //---------------------------------------------------------------------------
        private void tbExpectedKeyPress(object sender, KeyPressEventArgs e)
        {	// In den Großen Textboxen keine Texteingaben zulassen
            if (e.KeyChar > 31) e.KeyChar = '\0';
        }
        //---------------------------------------------------------------------------
        private void chDoneClick(object sender, EventArgs e)
        {
            int idx = Utils.ConvertTagToInt(((Control)sender).Tag);
            bool bTemp = chDone[idx].Checked;
            tbRemakes[idx].Enabled = !bTemp;
            if (bTemp && btClock[idx].Checked)
            {	// Uhr läuft noch, also stoppen
                btClock[idx].Checked = false;
                btClockClick(btClock[idx], EventArgs.Empty);
            }
            btClock[idx].Enabled = !bTemp;
            tbTime[idx].Enabled = !bTemp;
            if (bLock) return;
            // Kopiere zustand ins Hauptform
            Unit.frmMain.chDone[idx].Checked = bTemp;
            // Disable ein paar Controls bei einem Done
            UpdateMaterials();
        }
        //---------------------------------------------------------------------------
        private void tbRemakesChange(object sender, EventArgs e)
        {
            if (bLock) return;
            int idx = Utils.ConvertTagToInt(((Control)sender).Tag);
            int iItemCost = 0;
            // Kopiere zustand ins Hauptform
            Unit.frmMain.udRemakes[idx].Value = udRemakes[idx].Value;
            // Berechne neue Kosten
            for (int i = 0; i < 4; i++)
            {
                int iCost = Unit.player.GemCost[Unit.frmMain.iActSlot][i];
                iItemCost += iCost;
                lbCost[i].Text = Utils.Int2Gold(iCost);
            }
            lbItemCost.Text = Utils.Int2Gold(iItemCost);
            UpdateMaterials();
        }
        //---------------------------------------------------------------------------
        private void tbTriesChange(object sender, EventArgs e)
        {
            if (bLock) return;
            Utils.SetRegistryString("ExpectedTries", tbTries.Text);
            UpdateMaterials();
        }
        //---------------------------------------------------------------------------

        private void UpdateMaterials()
        {
            int i, j;
            int[][] Gems = new int[10][];	// Zähler für die Gems
            TStringList[] sGemList = new TStringList[2];
            sGemList[0] = new TStringList();
            sGemList[1] = new TStringList();
            // Lösche die Zählvariablen
            for (i = 0; i < Unit.xml_config.nIngredients; i++)
            {
                Unit.xml_config.arIngredients[i].Count[0] = 0;
                Unit.xml_config.arIngredients[i].Count[1] = 0;
            }
            for (i = 0; i < 10; i++)
            {
                Gems[i] = new int[2];
                Gems[i][0] = 0;
                Gems[i][1] = 0;
            }
            for (i = 0; i < 4; i++)
            {
                if (iBid[i] >= 0)
                {
                    int val = Unit.player.EffectValue[Unit.frmMain.iActSlot][i];
                    // Gemlevel bestimmen
                    int gemlevel = -1;
                    for (j = 0; j < 10; j++)
                    {
                        if (Unit.xml_config.arBonuses[iBid[i]].Gemvalue[j] == val)
                        {
                            gemlevel = j;
                            break;
                        }
                    }
                    Debug.Assert(gemlevel >= 0);
                    // Besorge zu dem gemlevel die anzahl an Stäuben
                    int dusts = Unit.xml_config.arBonuses[iBid[i]].DustAmount[gemlevel];
                    int liquids = Unit.xml_config.arBonuses[iBid[i]].LiquidAmount[gemlevel];
                    int gemamount = Unit.xml_config.arBonuses[iBid[i]].GemAmount[gemlevel];
                    // Jetzt wissen wir welches Gem gebraucht wird
                    int dust = Unit.xml_config.arBonuses[iBid[i]].GemDust;
                    int nliquids = Unit.xml_config.arBonuses[iBid[i]].nLiquids;
                    if (!chDone[i].Checked)
                    {	// Ist noch nicht fertig, also auch zum "Erwartet" Fenster addieren
                        Gems[gemlevel][0] += (gemamount * (int)udTries.Value);
                        Unit.xml_config.arIngredients[dust].Count[0] += (int)udTries.Value * dusts;
                        for (j = 0; j < nliquids; j++)
                            Unit.xml_config.arIngredients[Unit.xml_config.arBonuses[iBid[i]].GemLiquids[j]].Count[0] += (int)udTries.Value * liquids;
                    }
                    // Auf verbrauchte auf jeden Fall addieren
                    Gems[gemlevel][1] += gemamount + (gemamount * Unit.player.Remakes[Unit.frmMain.iActSlot][i]);
                    Unit.xml_config.arIngredients[dust].Count[1] += (Unit.player.Remakes[Unit.frmMain.iActSlot][i] + 1) * dusts;
                    for (j = 0; j < nliquids; j++)
                        Unit.xml_config.arIngredients[Unit.xml_config.arBonuses[iBid[i]].GemLiquids[j]].Count[1] += (Unit.player.Remakes[Unit.frmMain.iActSlot][i] + 1) * liquids;
                }
            }
            int Gem = Unit.xml_config.GetMaterialId("GEMS");
            Debug.Assert(Gem > 0);
            // Und nun die Textboxen selber erstellen
            for (i = 0; i < 10; i++)
            {
                for (j = 0; j < 2; j++)
                {
                    if (Gems[i][j] > 0)
                        sGemList[j].Add((Gems[i][j]).ToString() + " " + Unit.xml_config.arMaterials[Gem].arLevel[i].Name + " Juwel");
                }
            }
            for (i = 0; i < Unit.xml_config.nIngredients; i++)
            {
                for (j = 0; j < 2; j++)
                {
                    if (Unit.xml_config.arIngredients[i].Count[j] > 0)
                        sGemList[j].Add((Unit.xml_config.arIngredients[i].Count[j]).ToString()
                             + " " + Unit.xml_config.arIngredients[i].Name);
                }
            }
            tbExpected.Lines = sGemList[0].Strings.ToArray();
            tbUsed.Lines = sGemList[1].Strings.ToArray();
            sGemList[0] = null;
            sGemList[1] = null;
        }
        private void btClockClick(object sender, EventArgs e)
        {
            int idx = Utils.ConvertTagToInt(((Control)sender).Tag);
            if (btClock[idx].Checked)
            {	// Clock aktiviert
                // Wenn es eine zuletzt aktivierte Clock gibt, dann dort alles rückgängig
                if (iLastClock >= 0)
                {
                    bLock = true;
                    tbTime[iLastClock].Text = ((Unit.player.Time[Unit.frmMain.iActSlot][iLastClock] + 59) / 60).ToString();
                    tbTime[iLastClock].Enabled = true;
                    bLock = false;
                }
                else
                    Timer1.Enabled = true;
                dtStart = Utils.DateTimeFromTotalDays(Utils.Time() - ((double)Unit.player.Time[Unit.frmMain.iActSlot][idx] / 24 / 3600));
                iLastClock = idx;
                Timer1Timer(null, EventArgs.Empty);
            }
            else
            {	// timer deaktiviert
                bLock = true;
                tbTime[idx].Text = ((Unit.player.Time[Unit.frmMain.iActSlot][idx] + 59) / 60).ToString();
                Timer1.Enabled = false;
                bLock = false;
                iLastClock = -1;
            }
            tbTime[idx].Enabled = !btClock[idx].Checked;
        }
        //---------------------------------------------------------------------------

        private void Timer1Timer(object sender, EventArgs e)
        {
            int iTime = (int)((Utils.Time() - Utils.TotalDaysFromDateTime(dtStart)) * 24 * 60 * 60);
            string tstr;
            tstr = string.Format("{0}:{1:00}", iTime / 60, iTime % 60);
            // Aus irgend einem Grund kann ich nicht direkt sprintf auf den Text machen
            tbTime[iLastClock].Text = tstr;
        }
        //---------------------------------------------------------------------------

        private void tbTimeChange(object sender, EventArgs e)
        {
            int iTime;
            if (bLock) return;
            int idx = Utils.ConvertTagToInt(((Control)sender).Tag);
            // Erst feststellen, ob ein Doppelpunkt im String ist
            int iPos = tbTime[idx].Text.IndexOf(':');
            if (iPos > -1)
            {	// Ist einer drin
                iTime = tbTime[idx].Text.Substring(0, iPos).ToIntDef(0) * 60;
                iTime += tbTime[idx].Text.Substring(iPos + 1, 2).ToIntDef(0);
            }
            else
                iTime = tbTime[idx].Text.ToIntDef(0) * 60;
            Unit.player.Time[Unit.frmMain.iActSlot][idx] = iTime;
        }
        //---------------------------------------------------------------------------
        private void TfrmCraft_FormCreate(object sender, EventArgs e)
        {
            TGnuGettextInstance.TranslateComponent(this);
        }
        //---------------------------------------------------------------------------
    }

    static partial class Unit
    {
        internal static TfrmCraft frmCraft;
    }
}
