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
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using DelphiClasses;
using dxgettext;

namespace Moras.Net
{
    //---------------------------------------------------------------------------
    // Alle Daten zu einem Charakter
    public class CPlayer
    {
        public const int PLAYER_ITEMS = 19;
        public const int INVENTORY_ITEMS = 59;	// Index der letzten Inventory-Zelle
        public const int ALL_ITEMS = 99;

        private static string _(string szMsgId) { return TGnuGettextInstance.gettext(szMsgId); }

        // Übersetzungstabelle von und in Leladia (Erster Wert von, zweiter zu Leladia
        private static int[][] SlotLeladia = new int[19][]
        {
	        new int [2]{5, 4},
	        new int [2]{4, 2},
	        new int [2]{1, 5},
	        new int [2]{3, 3},
	        new int [2]{0, 1},
	        new int [2]{2, 0},
	        new int [2]{6, 6},
	        new int [2]{7, 7},
	        new int [2]{8, 8},
	        new int [2]{9, 9},
	        new int [2]{18, 18},
	        new int [2]{10, 10},
	        new int [2]{11, 11},
	        new int [2]{12, 12},
	        new int [2]{13, 13},
	        new int [2]{14, 14},
	        new int [2]{15, 15},
	        new int [2]{16, 16},
	        new int [2]{17, 17}
        };

        public CPlayer()
        {
            Modified = false;
            for (int i = 0; i < ALL_ITEMS; i++)
            {
                CItem item = new CItem();
                arItemSlot[i] = item;
                Debug.Assert(arItemSlot[i] != null);
                ClearItem(i);
            }
            for (int i = 0; i < PLAYER_ITEMS; i++)
            {
                CItem item = new CItem();
                arPreviewSlot[i] = item;
                Debug.Assert(arPreviewSlot[i] != null);

                arPreviewSlot[i].Init();
                arPreviewSlot[i].Slot = i;
                arPreviewSlot[i].Position = i;
                if (i < 10)
                {
                    arPreviewSlot[i].Type = EItemType.Crafted; // Rüstung und Waffen sind crafted als Default
                }
                else
                    arPreviewSlot[i].Type = EItemType.Drop;	// Schmuck ist per Default Drop
            }
            //arWeights.Length = xml_config.nAttributes;
            strFileName = "";
        }
        //---------------------------------------------------------------------------
        /*~CPlayer()
        {
	        for (int i = 0; i < ALL_ITEMS; i++)
	        {
		        delete arItemSlot[i];
                arItemSlot[i] = NULL;
	        }
            //arWeights.Length = 0;
        }*/
        //---------------------------------------------------------------------------
        public void Init()
        {   // Spieler initialisieren
            strName = _("<keiner>");
            strServer = "";
            iClass = 0;	// Hier eventuell Klasse aus Registry laden
            iRace = 0;
            iLevel = 50;
            idAccount = 0;
            idGem = Unit.xml_config.GetMaterialId("GEMS");
            Debug.Assert(idGem > 0);

            // Initialisiere alle Wichtungen
            xWeights.SetAllWeights(100);
            xWeights.Class = iClass;

            // Initialisiere Attribute
            xAttributes.Class = iClass;
            xAttributes.Level = iLevel;
            xAttributes.Clear();

            Modified = false;
        }

        // Lösche oder initialisiere den Gegenstand an der angegebenen Position
        public void ClearItem(int pos)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
            {
                arItemSlot[pos].Init();
                arItemSlot[pos].Slot = pos;
                if (pos < PLAYER_ITEMS) arItemSlot[pos].Position = pos;
                if (pos < 10)
                {
                    arItemSlot[pos].Type = EItemType.Crafted; // Rüstung und Waffen sind crafted als Default
                }
                else
                    arItemSlot[pos].Type = EItemType.Drop;	// Schmuck ist per Default Drop
            }
            Modified = true;
        }

        // Lösche oder initialisiere die Effekte des Gegenstand an der angegebenen Position
        public void ClearEffects(int pos)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
            {
                arItemSlot[pos].ClearEffects();
            }
            Modified = true;
        }

        // Speichere Spielerdaten unter den Dateinamen in FileName. Format gibt das Speicherformat an
        // Format 1 = Mein Format
        // Format 2 = Leladia
        public bool Save(int Format)
        {
            bool iReturn = false;
            CXml xBase = new CXml();
            FStreamWrapper fOut = null;
            try
            {
                if (Format == 1)
                {	// Mein Format
                    iReturn = xBase.OpenSaveXml(strFileName);
                    if (iReturn)
                    {
                        xBase.OpenTag("player", "", "");
                        xBase.OpenTag("name", strName, "");
                        xBase.CloseTag();
                        xBase.OpenTag("level", (iLevel).ToString(), "");
                        xBase.CloseTag();
                        if (idAccount > 0)
                        {
                            xBase.OpenTag("account", Unit.account.Name[idAccount], "");
                            xBase.CloseTag();
                        }
                        xBase.OpenTag("realm", Utils.Realm2Str(iRealm), "");
                        xBase.CloseTag();
                        if (strServer.Length > 0)
                        {
                            xBase.OpenTag("server", strServer, "");
                            xBase.CloseTag();
                        }
                        xBase.OpenTag("class", Unit.xml_config.arClasses[iClass].id, "");
                        xBase.CloseTag();
                        if (iRace > 0)
                        {
                            xBase.OpenTag("race", Unit.xml_config.arRaces[iRace].id, "");
                            xBase.CloseTag();
                        }
                        // Die Wichtungen speichern
                        xWeights.Save(xBase);

                        // Nun die Items abspeichern
                        for (int i = 0; i < ALL_ITEMS; i++)
                            arItemSlot[i].Save(xBase, true);
                        xBase.CloseTag();	// Player-Tag schliessen
                    }
                    xBase.CloseXml();
                }
                if (Format == 2)
                {	// Leladia
                    //HINT: FileMode.Create does the same as ofstream with trunc mode, because ofstream creates the file in every mode if it does not exist.
                    fOut = new FStreamWrapper(strFileName, FileMode.Create, FileAccess.Write);
                    fOut += "CHAR_NAME=" + strName;
                    fOut += "\nCHAR_CLASS=";
                    switch (iRealm)
                    {
                        case 1: fOut += "ALB"; break;
                        case 2: fOut += "HIB"; break;
                        case 4: fOut += "MID"; break;
                    }
                    fOut += "_" + Unit.xml_config.arClasses[iClass].id;
                    fOut += "\nCHAR_LEVEL=" + iLevel + "\nFILE_REALM=";
                    switch (iRealm)
                    {
                        case 1: fOut += "0"; break;
                        case 2: fOut += "1"; break;
                        case 4: fOut += "2"; break;
                    }
                    // Nun die einzelnen Items. Habe kein Spare-Item, darum nur bis 17
                    for (int i = 0; i < 19; i++)
                    {
                        int s = SlotLeladia[i][0];
                        String strItem;
                        strItem = string.Format("\nITEM{0:00}_", i);//%02d_
                        // Qualität, wird unterschiedlich gehandhabt bei crafted und drops
                        fOut += strItem + "QUALITY=";
                        fOut += ((arItemSlot[s].Type == EItemType.Crafted) ? arItemSlot[s].Quality - 94 : arItemSlot[s].Quality).ToString();
                        fOut += strItem + "LEVEL=" + arItemSlot[s].Level;
                        // Equipped muss noch angepasst werden
                        fOut += strItem + "EQUIPPED=" + (((s < 6) || (s > 9) || arItemSlot[s].bEquipped) ? 1 : 0);
                        fOut += strItem + "PLAYER_MADE=" + ((arItemSlot[s].Type == EItemType.Crafted) ? 1 : 0);
                        // DPS zeigt AF oder DPS an
                        fOut += strItem + "DPS=";
                        if (s < 6)
                            fOut += arItemSlot[s].AF;
                        else if (s < 10)
                            fOut += (arItemSlot[s].DPS * 0.1).ToString("#0.0");
                        else
                            fOut += 0;
                        fOut += strItem + "SPEED=";
                        if ((s > 5) & (s < 10) & (arItemSlot[s].Speed > 0))
                            fOut += (arItemSlot[s].Speed * 0.1).ToString("#0.0");
                        else
                            fOut += 0;
                        fOut += strItem + "BONUS=" + arItemSlot[s].Bonus;
                        fOut += strItem + "NAME=" + arItemSlot[s].Name;
                        // Nun die Gems. Nur als Leladia 2.0, also immer nur 4 Gems
                        for (int j = 0; j < 4; j++)
                        {
                            strItem = string.Format("\nITEM{0:00}_GEM{1}_", i, j);
                            fOut += strItem + "QUALITY=";
                            if (arItemSlot[s].EffectQuality[j] > 0)
                                fOut += arItemSlot[s].EffectQuality[j];
                            else
                                fOut += 0;
                            // Level ist bei crafted etwas kompliziert
                            fOut += strItem + "LEVEL=";
                            if (arItemSlot[s].Type == EItemType.Crafted)
                            {
                                int bid;
                                if ((bid = arItemSlot[s].Effect[j]) >= 0)
                                {
                                    for (int k = 0; k < 10; k++)
                                    {
                                        if (Unit.xml_config.arBonuses[bid].Gemvalue[k] == arItemSlot[s].EffectValue[j])
                                            fOut += k;
                                    }
                                }
                            }
                            else
                                fOut += arItemSlot[s].EffectValue[j];
                            fOut += strItem + "REMAKES=" + arItemSlot[s].EffectRemakes[j];
                            fOut += strItem + "MINUTES=" + (arItemSlot[s].EffectTime[j] + 59) / 60;
                            fOut += strItem + "DONE=" + ((arItemSlot[s].EffectDone[j]) ? 1 : 0);
                            fOut += strItem + "GEM_ID=";
                            if (arItemSlot[s].Effect[j] >= 0)
                            {	// Wenn _JEWEL am Ende steht, dann das weglassen
                                String strTemp = Unit.xml_config.arBonuses[arItemSlot[s].Effect[j]].GemId;
                                int pos;
                                if ((pos = strTemp.IndexOf("_JEWEL")) >= 0)
                                    strTemp = strTemp.Substring(1, pos - 1);
                                fOut += strTemp;
                            }
                        }
                    }
                    fOut += '\n';
                    iReturn = true;
                }
                Modified = false;
            }
            catch (Exception e)
            {
                Utils.DebugPrint("CPlayer::Save = {0}\niClass={1}, iRace={2}", e.Message, iClass, iRace);
                iReturn = false;
            }
            finally
            {
                if (fOut != null)
                    fOut.Dispose();
            }
            return iReturn;
        }

        // Lade Spielerkonfiguration.
        // Angabe eines Formats ist leider Blödsinn, da auch ohne Dialog geladen werden kann
        public bool Load()
        {
            bool iReturn = false;
            String strTemp;
            int iTemp, i;
            CXml xBase = new CXml();
            try
            {
                Init();

                String Ext = Path.GetExtension(strFileName);
                if (Ext.Equals(".mox", StringComparison.CurrentCultureIgnoreCase) || Ext.Equals(".mpx", StringComparison.CurrentCultureIgnoreCase))
                {
                    iReturn = xBase.OpenXml(strFileName);
                    while (iReturn)
                    {
                        if (xBase.isTag("name"))
                            strName = xBase.Content;
                        else if (xBase.isTag("level"))
                            SetLevel(int.Parse(xBase.Content)); // Besser die zugehörige Funktion benutzen wg. Nenebeffekten (xAttributes)
                        else if (xBase.isTag("account"))
                        {	// wenn es den account schon gibt, dann diesem zuordnen
                            idAccount = Unit.account.GetAccountId(xBase.Content);
                            if (idAccount == 0)
                            {
                                Unit.account.NewAccount(xBase.Content);
                                // Und nochmal versuchen
                                idAccount = Unit.account.GetAccountId(xBase.Content);
                            }
                        }
                        else if (xBase.isTag("realm"))
                            iRealm = Utils.Realm2Int(xBase.Content);
                        else if (xBase.isTag("server"))
                            strServer = xBase.Content;
                        else if (xBase.isTag("class"))
                            SetClass(Unit.xml_config.GetClassId(xBase.Content)); // Besser die zugehörige Funktion benutzen wg. Nenebeffekten (xAttributes, xWeights)
                        else if (xBase.isTag("race"))
                            iRace = Unit.xml_config.GetRaceId(xBase.Content);
                        else if (xBase.isTag("weight") || xBase.isTag("weights"))
                            xWeights.Load(xBase);
                        else if (xBase.isTag("item"))
                        {
                            for (i = 0; i < ALL_ITEMS; i++)
                            {
                                arItemSlot[i].Load(xBase);
                                arItemSlot[i].Slot = i;
                                if ((i < PLAYER_ITEMS) && (arItemSlot[i].Position == -1))
                                    arItemSlot[i].Position = i;
                            }
                        }
                        iReturn = xBase.NextTag();
                    }
                    xWeights.PostLoadMessages();	// Warn-/Infomeldungen gesammelt ausgeben.
                    xBase.CloseXml();
                }
                Modified = false;
            }
            catch
            {
                Utils.DebugPrint("Es ist ein Fehler in CPlayer::Load aufgetreten!");
                iReturn = true;
            }
            return iReturn;
        }

        // Berechne die Kosten für die Herstellung des Gems
        // Cost = Gemcost + Remakes * Neumachverlust :)
        protected int CalcGemCost(int pos, int number)
        {
            int iCost = 0;
            // gibts überhaupt ein Gem hier und ist Item crafted?
            if ((arItemSlot[pos].Type == EItemType.Crafted) && (arItemSlot[pos].Effect[number] >= 0))
            {	// Bestimmen des Gemlevels
                int gemlevel = -1;
                int bid = arItemSlot[pos].Effect[number];
                int val = arItemSlot[pos].EffectValue[number];
                for (int i = 0; i < 10; i++)
                {
                    if (Unit.xml_config.arBonuses[bid].Gemvalue[i] == val)
                    {
                        gemlevel = i;
                        break;
                    }
                }
                if (gemlevel >= 0 && Unit.xml_config.arBonuses[bid].bCraftable)
                {	// Konnte der Gemlevel bestimmt werden?
                    // Erstmal den Gem bestimmen
                    int gem = Unit.xml_config.arMaterials[idGem].arLevel[gemlevel].iPrice
                                * Unit.xml_config.arBonuses[bid].GemAmount[gemlevel];
                    // 2/3tel von Gem sind der Preis, den der Händler für den Gem zahlt
                    iCost += gem;
                    // Preis für Dust
                    int did = Unit.xml_config.arBonuses[bid].GemDust;
                    if (did >= 0)
                        iCost += Unit.xml_config.arIngredients[did].Price * Unit.xml_config.arBonuses[bid].DustAmount[gemlevel];
                    // Und Kosten für die Flüssigkeiten
                    for (int i = 0; i < Unit.xml_config.arBonuses[bid].nLiquids; i++)
                        iCost += Unit.xml_config.arIngredients[Unit.xml_config.arBonuses[bid].GemLiquids[i]].Price * Unit.xml_config.arBonuses[bid].LiquidAmount[gemlevel];
                    // Nun die Remakes berechnen
                    iCost += arItemSlot[pos].EffectRemakes[number] * (iCost - gem * 2 / 3);
                }
            }
            return iCost;
        }

        // Berechne den Gesamtpreis für das Spellcrafting
        protected int CalcSCPrice()
        {
            int iSCPrice = 0;
            bool bOrder = false;
            // Arbeite alle potentiellen Crafting-Slots durch
            for (int slot = 0; slot < 10; slot++)
            {
                if (arItemSlot[slot].Type == EItemType.Crafted)
                {	// Muss Crafted item sein
                    int iSumIP = 0;
                    int iMaxIP = 0;
                    bool bItem = false;	// Wird wahr gesetzt, wenn ein Gem crafted ist
                    for (int pos = 0; pos < 4; pos++)
                    {
                        int bid = arItemSlot[slot].Effect[pos];
                        int val = arItemSlot[slot].EffectValue[pos];
                        if ((bid >= 0) && Unit.xml_config.arBonuses[bid].bCraftable)
                        {	// Es ist ein Gem im aktuellen Slot
                            Debug.Assert(val > 0);
                            // Gemlevel bestimmen
                            int gemlevel = -1;
                            for (int i = 0; i < 10; i++)
                            {
                                if (Unit.xml_config.arBonuses[bid].Gemvalue[i] == val)
                                {
                                    gemlevel = i;
                                    break;
                                }
                            }
                            if (gemlevel >= 0)
                            {	// Bestimme nun die Kosten des Juwelen
                                // Erstmal den Gem bestimmen
                                int gem = Unit.xml_config.arMaterials[idGem].arLevel[gemlevel].iPrice
                                            * Unit.xml_config.arBonuses[bid].GemAmount[gemlevel];
                                // 2/3tel von Gem sind der Preis, den der Händler für den Gem zahlt
                                int iCost = gem;
                                // Preis für Dust
                                iCost += Unit.xml_config.arIngredients[Unit.xml_config.arBonuses[bid].GemDust].Price
                                    * Unit.xml_config.arBonuses[bid].DustAmount[gemlevel];
                                // Und Kosten für die Flüssigkeiten
                                for (int i = 0; i < Unit.xml_config.arBonuses[bid].nLiquids; i++)
                                    iCost += Unit.xml_config.arIngredients[Unit.xml_config.arBonuses[bid].GemLiquids[i]].Price * Unit.xml_config.arBonuses[bid].LiquidAmount[gemlevel];
                                // Kosten für einen Gemversuch nun fertig
                                // Qualitätsaufschlag muß eigentlich auf dem Gempreis ohne Remakes basieren
                                if (Unit.xml_config.sPriceModel.iGQMarkup)
                                {
                                    int percent = Unit.xml_config.sPriceModel.pGQMarkup[arItemSlot[slot].EffectQuality[pos]];
                                    iSCPrice += (iCost * percent + 999) / 1000;
                                }
                                // Nun die Remakes berechnen
                                iCost += arItemSlot[slot].EffectRemakes[pos] * (iCost - gem * 2 / 3);
                                bItem = true;
                                if (Unit.xml_config.sPriceModel.iCost) iSCPrice += iCost;
                                // General_markup - Die 999 sind hier, um alles aufzurunden
                                iSCPrice += (iCost * Unit.xml_config.sPriceModel.pGeneralMarkup + 999) / 1000;
                                iSCPrice += Unit.xml_config.sPriceModel.PPGem; // Aufpreis pro Juwel
                                // Pro Gemstufe
                                if (Unit.xml_config.sPriceModel.iPPGTier)
                                    iSCPrice += Unit.xml_config.sPriceModel.PPGTier[gemlevel];
                                // Pro Zeit (kein integer hier, kann leicht Bereichsüberlauf ergeben (64 Bit gibts hier leider nicht
                                int temp = (int)((double)Unit.xml_config.sPriceModel.PPHour * (double)arItemSlot[slot].EffectTime[pos] / (double)3600);
                                iSCPrice += temp;
                                int iIP = (val * Unit.xml_config.arBonuses[bid].ip_mult + Unit.xml_config.arBonuses[bid].ip_add) / 100;
                                if (iIP > iMaxIP) iMaxIP = iIP;
                                iSumIP += iIP;
                            }
                        }
                    }
                    // Item ist durch, nun die Preise pro Item berechnen
                    if (bItem)
                    {	// Gegenstand Bannzaubert, verschiedene Aufpreise pro Item
                        iSCPrice += Unit.xml_config.sPriceModel.PPItem;	// Pro Item generell
                        // Nun der pro Itemlevel
                        iSCPrice += Unit.xml_config.sPriceModel.PPLevel * arItemSlot[slot].Level;
                        // Wieviel IP hat Item?
                        int iUsedIP = (iSumIP + iMaxIP) / 2;
                        // Preis pro ImbuePunkt
                        iSCPrice += iUsedIP * Unit.xml_config.sPriceModel.PPIP;
                        int iAvailIP = CalcAvailIP(slot);
                        // Preis pro Überladungspunkt
                        if (iUsedIP > iAvailIP)
                            iSCPrice += (iUsedIP - iAvailIP) * Unit.xml_config.sPriceModel.PPOC;
                        bOrder = true;
                    }
                }
            }
            // Wenn bisher ein Preis, dann ist es ein Auftrag :)
            if (bOrder) iSCPrice += Unit.xml_config.sPriceModel.PPOrder;
            return iSCPrice;
        }

        #region Property Accessors
        //protected String GetName();

        protected void SetName(String Name)
        {
            strName = Name;
            Modified = true;
        }

        protected void SetFileName(String Name)
        {
            strFileName = Name;
            Modified = true;
        }

        protected void SetServer(String Name)
        {
            strServer = Name;
            Modified = true;
        }

        protected void SetAccount(int Account)
        {
            idAccount = Account;
            Modified = true;
        }

        protected void SetRealm(int Realm)
        {
            iRealm = Realm;
            Modified = true;
        }

        protected void SetLevel(int Level)
        {
            iLevel = Level;
            xAttributes.Level = Level;
            UpdateAttributes();
            Modified = true;
        }

        protected void SetClass(int Class)
        {
            iClass = Class;
            xWeights.Class = Class;
            xAttributes.Class = Class;
            UpdateAttributes();
            Modified = true;
        }

        protected void SetRace(int Race)
        {
            iRace = Race;
            Modified = true;
        }

        // pos = Die Slotposition, also ob Körper, Umhang oder so
        // number = Nummer des Effektes
        protected void SetItemEffect(int pos, int number, int EffectId)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
            {
                arItemSlot[pos].Effect[number] = EffectId;
            }
            Modified = true;
        }

        protected int GetItemEffect(int pos, int number)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                return arItemSlot[pos].Effect[number];
            else
                return -1;
        }

        protected int GetnItemEffects(int pos)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                return arItemSlot[pos].nEffects;
            else
                return 0;
        }

        // pos = Die Slotposition, also ob Körper, Umhang oder so
        // number = Nummer des Effektes
        protected void SetEffectValue(int pos, int number, int Value)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
            {
                arItemSlot[pos].EffectValue[number] = Value;
            }
            Modified = true;
        }

        protected int GetEffectValue(int pos, int number)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                return arItemSlot[pos].EffectValue[number];
            else
                return 0;
        }

        // pos = Die Slotposition, also ob Körper, Umhang oder so
        // number = Nummer des Effektes
        protected void SetEffectLevel(int pos, int number, int Value)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
            {
                arItemSlot[pos].EffectLevel[number] = Value;
            }
            Modified = true;
        }

        protected int GetEffectLevel(int pos, int number)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                return arItemSlot[pos].EffectLevel[number];
            else
                return 0;
        }

        // pos = Die Slotposition, also ob Körper, Umhang oder so
        // number = Nummer des Effektes
        protected void SetEffectDone(int pos, int number, bool Value)
        {
            if ((pos >= 0) && (pos < 10))
            {
                arItemSlot[pos].EffectDone[number] = Value;
            }
            Modified = true;
        }

        protected bool GetEffectDone(int pos, int number)
        {
            if ((pos >= 0) && (pos < 10))
                return arItemSlot[pos].EffectDone[number];
            else
                return false;
        }

        // pos = Die Slotposition, also ob Körper, Umhang oder so
        // number = Nummer des Effektes
        protected void SetRemakes(int pos, int number, int Remakes)
        {
            if ((pos >= 0) && (pos < 10))
            {
                arItemSlot[pos].EffectRemakes[number] = Remakes;
            }
            Modified = true;
        }

        protected int GetRemakes(int pos, int number)
        {
            if ((pos >= 0) && (pos < 10))
                return arItemSlot[pos].EffectRemakes[number];
            else
                return 0;
        }

        // pos = Die Slotposition, also ob Körper, Umhang oder so
        // number = Nummer des Effektes
        protected void SetTime(int pos, int number, int Seconds)
        {
            if ((pos >= 0) && (pos < 10))
            {
                arItemSlot[pos].EffectTime[number] = Seconds;
            }
            Modified = true;
        }

        protected int GetTime(int pos, int number)
        {
            if ((pos >= 0) && (pos < 10))
                return arItemSlot[pos].EffectTime[number];
            else
                return 0;
        }

        // pos = Die Slotposition, also ob Körper, Umhang oder so
        // number = Nummer des Klassenbeschränkung
        protected void SetClassRestriction(int pos, int number, int Value)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
            {
                arItemSlot[pos].ClassRestriction[number] = Value;
            }
            Modified = true;
        }

        protected int GetClassRestriction(int pos, int number)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                return arItemSlot[pos].ClassRestriction[number];
            else
                return -1;
        }

        protected int GetNClassRestrictions(int pos)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                return arItemSlot[pos].nClassRestrictions;
            else
                return 4;
        }

        // pos = Die Slotposition, also ob Körper, Umhang oder so
        protected void SetItemRealm(int pos, int Value)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                arItemSlot[pos].Realm = Value;
            Modified = true;
        }

        protected int GetItemRealm(int pos)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                return arItemSlot[pos].Realm;
            else
                return 0;
        }

        // pos = Die Slotposition, also ob Körper, Umhang oder so
        protected void SetItemLevel(int pos, int Value)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                arItemSlot[pos].Level = Value;
            Modified = true;
        }

        protected int GetItemLevel(int pos)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                return arItemSlot[pos].Level;
            else
                return 0;
        }

        // pos = Die Slotposition, also ob Körper, Umhang oder so
        protected void SetMaxLevel(int pos, int Value)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                arItemSlot[pos].MaxLevel = Value;
            Modified = true;
        }

        protected int GetMaxLevel(int pos)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                return arItemSlot[pos].MaxLevel;
            else
                return 0;
        }

        // pos = Die Slotposition, also ob Körper, Umhang oder so
        protected void SetCurLevel(int pos, int Value)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                arItemSlot[pos].CurLevel = Value;
            Modified = true;
        }

        protected int GetCurLevel(int pos)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                return arItemSlot[pos].CurLevel;
            else
                return 0;
        }

        // pos = Die Slotposition, also ob Körper, Umhang oder so
        // number = Nummer des Effektes
        protected void SetEffectQuality(int pos, int number, int Value)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
            {
                arItemSlot[pos].EffectQuality[number] = Value;
            }
            Modified = true;
        }

        protected int GetEffectQuality(int pos, int number)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                return arItemSlot[pos].EffectQuality[number];
            else
                return 0;
        }

        protected String GetItemName(int pos)
        {	// liefere den orginalnamen, wenn Name selbst noch nicht definiert ist
            if ((pos >= 0) && (pos < ALL_ITEMS))
            {
                return arItemSlot[pos].Name;
            }
            return "";
        }

        protected void SetItemName(int pos, String Name)
        {	// Wenn Name == Orignalname, dann lösche den strName
            if ((pos >= 0) && (pos < ALL_ITEMS))
            {
                arItemSlot[pos].Name = Name;
            }
            Modified = true;
        }

        protected String GetItemNameOriginal(int pos)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                return arItemSlot[pos].NameOriginal;
            return "";
        }

        protected void SetItemNameOriginal(int pos, String Name)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                arItemSlot[pos].NameOriginal = Name;
            Modified = true;
        }

        protected String GetOrigin(int pos)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                return arItemSlot[pos].Origin;
            return "";
        }

        protected void SetOrigin(int pos, String Origin)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                arItemSlot[pos].Origin = Origin;
            Modified = true;
        }

        protected String GetDescription(int pos)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                return arItemSlot[pos].Description;
            return "";
        }

        protected void SetDescription(int pos, String Description)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                arItemSlot[pos].Description = Description;
            Modified = true;
        }

        // pos = Die Slotposition, also ob Körper, Umhang oder so
        protected void SetItemType(int pos, EItemType type)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                arItemSlot[pos].Type = type;
            Modified = true;
        }

        protected EItemType GetItemType(int pos)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                return arItemSlot[pos].Type;
            else
                return (EItemType)(-1);
        }

        // pos = Die Slotposition, also ob Körper, Umhang, Inventar oder so
        protected void SetItemPosition(int pos, int Value)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                arItemSlot[pos].Position = Value;
            Modified = true;
        }

        protected int GetItemPosition(int pos)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                return arItemSlot[pos].Position;
            else
                return -1;
        }

        // pos = Die Slotposition, also ob Körper, Umhang, Inventar oder so
        protected void SetItemSlot(int pos, int Value)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                arItemSlot[pos].Slot = Value;
            Modified = true;
        }

        protected int GetItemSlot(int pos)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                return arItemSlot[pos].Slot;
            else
                return -1;
        }

        // pos = Die Slotposition, also ob Körper, Umhang oder so
        protected void SetItemClass(int pos, int Value)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                arItemSlot[pos].Class = Value;
            Modified = true;
        }

        protected int GetItemClass(int pos)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS) && (arItemSlot[pos].Position < 10))
                return arItemSlot[pos].Class;
            else
                return -1;
        }

        // pos = Die Slotposition, also ob Körper, Umhang oder so
        protected void SetItemSubClass(int pos, int Value)
        {
            if ((pos >= 0) && (pos < 10))
                arItemSlot[pos].SubClass = Value;
            Modified = true;
        }

        protected int GetItemSubClass(int pos)
        {
            if ((pos >= 0) && (pos < 10))
                return arItemSlot[pos].SubClass;
            else
                return -1;
        }

        // pos = Die Slotposition, also ob Körper, Umhang oder so
        protected void SetMaterial(int pos, int Value)
        {
            if ((pos >= 0) && (pos < 10))
                arItemSlot[pos].Material = Value;
            Modified = true;
        }

        protected int GetMaterial(int pos)
        {
            if ((pos >= 0) && (pos < 10))
                return arItemSlot[pos].Material;
            else
                return -1;
        }

        // pos = Die Slotposition, also ob Körper, Umhang oder so
        protected void SetAF(int pos, int Value)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                arItemSlot[pos].AF = Value;
            Modified = true;
        }

        protected int GetAF(int pos)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS) && (arItemSlot[pos].Position < 6))
                return arItemSlot[pos].AF;
            else
                return 0;
        }

        // pos = Die Slotposition, also ob Körper, Umhang oder so
        protected void SetDPS(int pos, int Value)
        {
            if ((pos > 5) && (pos < 10))
                arItemSlot[pos].DPS = Value;
            Modified = true;
        }

        protected int GetDPS(int pos)
        {
            if ((pos > 5) && (pos < 10))
                return arItemSlot[pos].DPS;
            else
                return 0;
        }

        // pos = Die Slotposition, also ob Körper, Umhang oder so
        protected void SetSpeed(int pos, int Value)
        {
            if ((pos > 5) && (pos < 10))
                arItemSlot[pos].Speed = Value;
            Modified = true;
        }

        protected int GetSpeed(int pos)
        {
            if ((pos > 5) && (pos < 10))
                return arItemSlot[pos].Speed;
            else
                return 0;
        }

        // pos = Die Slotposition, also ob Körper, Umhang oder so
        protected void SetBonus(int pos, int Value)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                arItemSlot[pos].Bonus = Value;
            Modified = true;
        }

        protected int GetBonus(int pos)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                return arItemSlot[pos].Bonus;
            else
                return -1;
        }

        // pos = Die Slotposition, also ob Körper, Umhang oder so
        protected void SetQuality(int pos, int Value)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                arItemSlot[pos].Quality = Value;
            Modified = true;
        }

        protected int GetQuality(int pos)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                return arItemSlot[pos].Quality;
            else
                return -1;
        }

        protected CItem GetPreviewItem(int pos)
        {
            if ((pos >= 0) && (pos < PLAYER_ITEMS))
                return arPreviewSlot[pos];
            else
                return null;
        }

        protected void SetPreviewItem(int pos, CItem Item)
        {
            if ((pos >= 0) && (pos < PLAYER_ITEMS))
            {
                // Update Preview
                xAttributes -= arPreviewSlot[pos];
                xAttributes += Item;
                arPreviewSlot[pos] = new CItem(Item);
            }
            Modified = true;
        }

        protected CItem GetItem(int pos)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                return arItemSlot[pos];
            else
                return null;
        }

        protected void SetItem(int pos, CItem Item)
        {
            if ((pos >= 0) && (pos < ALL_ITEMS))
                arItemSlot[pos] = new CItem(Item);
            Modified = true;
        }

        protected int GetWeight(int index) { return xWeights.Weight[index]; }
        protected void SetWeight(int index, int value) { xWeights.Weight[index] = value; Modified = true; }
        protected Single GetUpV(int index) { return xWeights.UpV[index]; }
        protected int GetEffCap(int pos) { return xAttributes.EffCap[pos]; }
        #endregion

        // Berechne Overcharge-Chance
        // > 100 = Keine Überladung
        // <-100 = unmögliche Überladung
        public int CalcOverCharge(int pos)
        {
            int iAvailIP = CalcAvailIP(pos);
            int iMaxIP = 0;
            int iSumIP = 0;
            for (int i = 0; i < 4; i++)
            {
                int iIP = CalcEffectIP(pos, i);
                if (arItemSlot[pos].Effect[i] >= 0)
                {
                    if (iIP > iMaxIP) iMaxIP = iIP;
                    iSumIP += iIP;
                }
            }
            int iUsedIP = (iSumIP + iMaxIP) / 2;
            int iOC = iUsedIP - iAvailIP;
            int iOCC = 0;
            if (iOC <= 0)	// Keine Überladung
                return 101;
            if (iOC > 5)	// Mehr als 5 Überladen
                return -101;
            switch (iOC)
            {
                case 1: iOCC = -10; break;
                case 2: iOCC = -20; break;
                case 3: iOCC = -30; break;
                case 4: iOCC = -50; break;
                case 5: iOCC = -70; break;
            }
            // Gemquality-Abhängige Sachen
            for (int i = 0; i < 4; i++)
            {
                if (arItemSlot[pos].Effect[i] >= 0)
                {
                    switch (arItemSlot[pos].EffectQuality[i])
                    {
                        case 0: iOCC += 1; break;
                        case 1: iOCC += 3; break;
                        case 2: iOCC += 5; break;
                        case 3: iOCC += 8; break;
                        case 4: iOCC += 11; break;
                    }
                }
            }
            // ItemQuality
            switch (arItemSlot[pos].Quality)
            {
                case 96: iOCC += 6; break;
                case 97: iOCC += 8; break;
                case 98: iOCC += 10; break;
                case 99: iOCC += 18; break;
                case 100: iOCC += 26; break;
            }
            // Nun nur noch der Skillmod vom Bannzauberer
            iOCC += Unit.xml_config.iSCLevel;
            if (iOCC > 100) iOCC = 100;
            if (iOCC < -100) iOCC = -100;
            return iOCC;
        }

        // Berechne die verfügbaren IPs
        public int CalcAvailIP(int pos)
        {
            Double fFaktor = 0.61;
            int result = 0;

            if ((pos >= 0) && (pos < 10) && (arItemSlot[pos].Type == EItemType.Crafted))
            {
                /*		switch (arItemSlot[pos].Quality)
                        {
                        case 94: fFaktor = 0.2; break;
                        case 95: fFaktor = 0.28; break;
                        case 96: fFaktor = 0.34; break;
                        case 97: fFaktor = 0.4; break;
                        case 98: fFaktor = 0.46; break;
                        case 99: fFaktor = 0.53; break;
                        case 100: fFaktor = 0.61; break;
                        }*/

                result = (int)Math.Floor(fFaktor * (arItemSlot[pos].Level + 1) + 0.5);

                if (Utils.GetRegistryInteger("IPWorkaround", 0) != 0 && ((arItemSlot[pos].AF / 2) != arItemSlot[pos].Level) && (arItemSlot[pos].DPS == 0))
                    return result - 1;
                else
                    return result;
            }
            else
                return result;
        }

        // Berechne die IP, die der Effect verbraucht
        protected int CalcEffectIP(int pos, int number)
        {
            int iReturn;
            int idEffect = arItemSlot[pos].Effect[number];
            if (idEffect < 0) return 0;
            if (!Unit.xml_config.arBonuses[idEffect].bCraftable) return 0;
            iReturn = (arItemSlot[pos].EffectValue[number]
                * Unit.xml_config.arBonuses[idEffect].ip_mult + Unit.xml_config.arBonuses[idEffect].ip_add) / 100;
            if (iReturn < 1) iReturn = 1;
            return iReturn;
        }

        // Liefert wahr, wenn Attribut von der Klasse benutzt werden kann
        public bool AttributeUseable(int AttributeID)
        {
            return Unit.xml_config.AttributeUseableByClass(AttributeID, iClass);
        }

        public void SetPreviewMode(bool Preview)
        {
            xAttributes.SetPreviewMode(Preview);
            if (Preview)
            {
                for (int slot = 0; slot < PLAYER_ITEMS; slot++)
                    arPreviewSlot[slot] = new CItem(arItemSlot[slot]);
            }
            else
            {
                for (int slot = 0; slot < PLAYER_ITEMS; slot++)
                    arPreviewSlot[slot].Init();
            }
        }

        public void UpdateAttributes()
        {
            int slot;

            // Alle Attribute auf 0 setzen
            xAttributes.Clear();

            // Nun die Boni alle Gegenstände zu den Attributen addieren
            for (slot = 0; slot < PLAYER_ITEMS; slot++)
                if (Item[slot].bEquipped) // Ist Gegenstand ausgerüsted?
                    xAttributes += Item[slot];  // Effekte aufaddieren
        }

        public double CalcGesamtNutzen()
        {
            return GesamtNutzen = xAttributes.CalcGesamtNutzen(xWeights);
        }

        public void DisplayAttributes()
        {
            xAttributes.Display();
        }

        protected int CalcSCCost()
        {
            int iSCCost = 0;

            for (int i = 0; i < 10; ++i)
            {
                if (ItemType[i] == EItemType.Crafted)
                {
                    for (int j = 0; j < 4; j++)
                        iSCCost += GemCost[i][j];
                }
            }

            return iSCCost;
        }

        private String strName;	// Name des Spielers
        private String strFileName;	// Name der Datei wo die daten gespeichert sind, sofern vorhanden
        private int idAccount;
        private int iLevel;
        private String strServer;
        private int iRealm;
        private int iClass;
        private int iRace;
        private int idGem;	// die Material Id der Gems
        private CItem[] arItemSlot = new CItem[ALL_ITEMS];	// Die 18 tragbaren Gegenstände, sowie die vom Inventar und Truhe
        private CItem[] arPreviewSlot = new CItem[PLAYER_ITEMS];	// Die 18 tragbaren Gegenstände
        //private DynamicArray<int>	arWeights;	// Alle Wichtungen für jedes Attribut
        private CPlayerAttributes xAttributes = new CPlayerAttributes();
        private CPlayerWeights xWeights = new CPlayerWeights();

        public bool Modified;
        public double GesamtNutzen;
        public int others;

        #region Properties
        public String Name { get { return strName; } set { SetName(value); } }
        public String FileName { get { return strFileName; } set { SetFileName(value); } }
        public String Server { get { return strServer; } set { SetServer(value); } }
        public int Account { get { return idAccount; } set { SetAccount(value); } }
        public int Realm { get { return iRealm; } set { SetRealm(value); } }
        public int Level { get { return iLevel; } set { SetLevel(value); } }
        public int Class { get { return iClass; } set { SetClass(value); } }
        public int Race { get { return iRace; } set { SetRace(value); } }
        private IndexerProperty<EItemType, int> _itemType;
        public IndexerProperty<EItemType, int> ItemType { get { return _itemType ?? (_itemType = new IndexerProperty<EItemType, int> { read = GetItemType, write = SetItemType }); } }
        private IndexerProperty<int, int> _itemClass;
        public IndexerProperty<int, int> ItemClass { get { return _itemClass ?? (_itemClass = new IndexerProperty<int, int> { read = GetItemClass, write = SetItemClass }); } }
        private IndexerProperty<int, int> _itemSubClass;
        public IndexerProperty<int, int> ItemSubClass { get { return _itemSubClass ?? (_itemSubClass = new IndexerProperty<int, int> { read = GetItemSubClass, write = SetItemSubClass }); } }
        private IndexerProperty<int, int> _position;
        public IndexerProperty<int, int> Position { get { return _position ?? (_position = new IndexerProperty<int, int> { read = GetItemPosition, write = SetItemPosition }); } }
        private IndexerProperty<int, int> _slot;
        public IndexerProperty<int, int> Slot { get { return _slot ?? (_slot = new IndexerProperty<int, int> { read = GetItemSlot, write = SetItemSlot }); } }
        private IndexerProperty<int, int> _material;
        public IndexerProperty<int, int> Material { get { return _material ?? (_material = new IndexerProperty<int, int> { read = GetMaterial, write = SetMaterial }); } }
        private IndexerProperty<int, int> _AF;
        public IndexerProperty<int, int> AF { get { return _AF ?? (_AF = new IndexerProperty<int, int> { read = GetAF, write = SetAF }); } }
        private IndexerProperty<int, int> _DPS;
        public IndexerProperty<int, int> DPS { get { return _DPS ?? (_DPS = new IndexerProperty<int, int> { read = GetDPS, write = SetDPS }); } }
        private IndexerProperty<int, int> _speed;
        public IndexerProperty<int, int> Speed { get { return _speed ?? (_speed = new IndexerProperty<int, int> { read = GetSpeed, write = SetSpeed }); } }
        private IndexerProperty<int, int> _bonus;
        public IndexerProperty<int, int> Bonus { get { return _bonus ?? (_bonus = new IndexerProperty<int, int> { read = GetBonus, write = SetBonus }); } }
        private IndexerProperty<int, int> _itemRealm;
        public IndexerProperty<int, int> ItemRealm { get { return _itemRealm ?? (_itemRealm = new IndexerProperty<int, int> { read = GetItemRealm, write = SetItemRealm }); } }
        private IndexerProperty<int, int> _itemLevel;
        public IndexerProperty<int, int> ItemLevel { get { return _itemLevel ?? (_itemLevel = new IndexerProperty<int, int> { read = GetItemLevel, write = SetItemLevel }); } }
        private IndexerProperty<int, int> _curLevel;
        public IndexerProperty<int, int> CurLevel { get { return _curLevel ?? (_curLevel = new IndexerProperty<int, int> { read = GetCurLevel, write = SetCurLevel }); } }
        private IndexerProperty<int, int> _maxLevel;
        public IndexerProperty<int, int> MaxLevel { get { return _maxLevel ?? (_maxLevel = new IndexerProperty<int, int> { read = GetMaxLevel, write = SetMaxLevel }); } }
        private IndexerProperty<int, int> _quality;
        public IndexerProperty<int, int> Quality { get { return _quality ?? (_quality = new IndexerProperty<int, int> { read = GetQuality, write = SetQuality }); } }
        private IndexerProperty<int, int, int> _itemEffect;
        public IndexerProperty<int, int, int> ItemEffect { get { return _itemEffect ?? (_itemEffect = new IndexerProperty<int, int, int> { read = GetItemEffect, write = SetItemEffect }); } }
        private IndexerProperty<int, int> _nItemEffects;
        public IndexerProperty<int, int> nItemEffects { get { return _nItemEffects ?? (_nItemEffects = new IndexerProperty<int, int> { read = GetnItemEffects }); } }
        private IndexerProperty<int, int, int> _effectValue;
        public IndexerProperty<int, int, int> EffectValue { get { return _effectValue ?? (_effectValue = new IndexerProperty<int, int, int> { read = GetEffectValue, write = SetEffectValue }); } }
        private IndexerProperty<int, int, int> _effectQuality;
        public IndexerProperty<int, int, int> EffectQuality { get { return _effectQuality ?? (_effectQuality = new IndexerProperty<int, int, int> { read = GetEffectQuality, write = SetEffectQuality }); } }
        private IndexerProperty<int, int, int> _effectLevel;
        public IndexerProperty<int, int, int> EffectLevel { get { return _effectLevel ?? (_effectLevel = new IndexerProperty<int, int, int> { read = GetEffectLevel, write = SetEffectLevel }); } }
        private IndexerProperty<bool, int, int> _effectDone;
        public IndexerProperty<bool, int, int> EffectDone { get { return _effectDone ?? (_effectDone = new IndexerProperty<bool, int, int> { read = GetEffectDone, write = SetEffectDone }); } }
        private IndexerProperty<int, int, int> _remakes;
        public IndexerProperty<int, int, int> Remakes { get { return _remakes ?? (_remakes = new IndexerProperty<int, int, int> { read = GetRemakes, write = SetRemakes }); } }
        private IndexerProperty<int, int, int> _time;
        public IndexerProperty<int, int, int> Time { get { return _time ?? (_time = new IndexerProperty<int, int, int> { read = GetTime, write = SetTime }); } }
        private IndexerProperty<String, int> _itemName;
        public IndexerProperty<String, int> ItemName { get { return _itemName ?? (_itemName = new IndexerProperty<String, int> { read = GetItemName, write = SetItemName }); } }
        private IndexerProperty<String, int> _itemNameOriginal;
        public IndexerProperty<String, int> ItemNameOriginal { get { return _itemNameOriginal ?? (_itemNameOriginal = new IndexerProperty<String, int> { read = GetItemNameOriginal, write = SetItemNameOriginal }); } }
        private IndexerProperty<String, int> _origin;
        public IndexerProperty<String, int> Origin { get { return _origin ?? (_origin = new IndexerProperty<String, int> { read = GetOrigin, write = SetOrigin }); } }
        private IndexerProperty<String, int> _description;
        public IndexerProperty<String, int> Description { get { return _description ?? (_description = new IndexerProperty<String, int> { read = GetDescription, write = SetDescription }); } }
        private IndexerProperty<int, int, int> _classRestriction;
        public IndexerProperty<int, int, int> ClassRestriction { get { return _classRestriction ?? (_classRestriction = new IndexerProperty<int, int, int> { read = GetClassRestriction, write = SetClassRestriction }); } }
        private IndexerProperty<int, int> _nClassRestrictions;
        public IndexerProperty<int, int> NClassRestrictions { get { return _nClassRestrictions ?? (_nClassRestrictions = new IndexerProperty<int, int> { read = GetNClassRestrictions }); } }
        private IndexerProperty<int, int, int> _effectIP;
        public IndexerProperty<int, int, int> EffectIP { get { return _effectIP ?? (_effectIP = new IndexerProperty<int, int, int> { read = CalcEffectIP }); } }
        private IndexerProperty<int, int, int> _gemCost;
        public IndexerProperty<int, int, int> GemCost { get { return _gemCost ?? (_gemCost = new IndexerProperty<int, int, int> { read = CalcGemCost }); } }
        public int SCPrice { get { return CalcSCPrice(); } }
        public int SCCost { get { return CalcSCCost(); } }
        private IndexerProperty<CItem, int> _item;
        public IndexerProperty<CItem, int> Item { get { return _item ?? (_item = new IndexerProperty<CItem, int> { read = GetItem, write = SetItem }); } }
        private IndexerProperty<CItem, int> _previewItem;
        public IndexerProperty<CItem, int> PreviewItem { get { return _previewItem ?? (_previewItem = new IndexerProperty<CItem, int> { read = GetPreviewItem, write = SetPreviewItem }); } }
        private IndexerProperty<int, int> _weight;
        public IndexerProperty<int, int> Weight { get { return _weight ?? (_weight = new IndexerProperty<int, int> { read = GetWeight, write = SetWeight }); } }
        private IndexerProperty<Single, int> _UpV;
        public IndexerProperty<Single, int> UpV { get { return _UpV ?? (_UpV = new IndexerProperty<Single, int> { read = GetUpV }); } }
        private IndexerProperty<int, int> _effCap;
        public IndexerProperty<int, int> EffCap { get { return _effCap ?? (_effCap = new IndexerProperty<int, int> { read = GetEffCap }); } }
        public CPlayerAttributes Attributes { get { return xAttributes; } }
        public CPlayerWeights Weights { get { return xWeights; } }
        #endregion
    }
}