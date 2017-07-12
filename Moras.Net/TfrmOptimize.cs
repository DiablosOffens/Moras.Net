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
using System.IO;

namespace Moras.Net
{
    public partial class TfrmOptimize : TCustomForm
    {
        public const int MAX_ITEMS_PER_SLOT = 100;
        public const int MAX_COMBINATIONS = 2000;
        //---------------------------------------------------------------------------

        struct SCombination : INeedsInitialization
        {
            public int[] group { get; private set; }	// Effektgruppe (0=Stats/Hitpoints, 1=Resistenzen/Manapool, 2=Fertigkeiten)
            public int[] value { get; private set; }	// Level des Gems
            public int count;

            public void Init()
            {
                group = new int[4];
                value = new int[4];
            }
        }

        class COItem
        {
            public int id;	// Die id in der Datenbank
            public int nData;	// Wieviele Wertepaare stehen in Data
            public int[] data;

            // Destructor von COItem
            /*~COItem()
            {
                delete data;
            }*/

            // In Count steht, wieviele Datenpaare das Items maximal halten kann
            public void Init(int Count)
            {
                id = -1;
                nData = 0;
                data = new int[Count * 2];	// Jeweils Index und Data
            }

            public void Add(int index, int value)
            {
                data[nData * 2] = index;
                data[nData * 2 + 1] = value;
                nData++;
            }
        }

        class COSlot
        {
            public int id;
            public int nItems;	// Anzahl der Items in diesem Slot (zum optimieren)
            public int nCombinations;
            public int current;
            public bool bSingle;	// Nur ein Item in diesem Slot
            public COItem[] arItems = new COItem[MAX_ITEMS_PER_SLOT];
        }

        class COAttribut
        {
            public int id;
            public int bid;
            public Single upv;
            public int capatt;	// Dieses Attribut erhöht das Cap von ...
            public int cap;	// Das normale Cap
            public int capvalue;	// Aktueller Wert des Caps
            public int max;	// Maximalwert des Attr., das genutzt werden könnte (Cap + max. Caperhöhung)
            public int dist;	// Aktueller Abstand vom Cap
            public int ip_add;
            public int ip_mult;	// Für die Steinchen-Berechnungen
            public bool bUsed;
            public int[] value = new int[17];	// Den Wert in der entprechenden Stufe (Maximal 16 Stufen + Grundwert)
        }

        private DateTime tStart;	// Die Startzeit, wann die optimierung gestartet wurde
        private Int64 nDone;	// Soviele Kombinationen sind abgearbeitet
        private Int64 nChecked;
        private Int64 nJumped;
        private int iPM;	// Hilfvariable für selteneres Aufrufen der Messageloop
        private Int64 BestCombination;
        private Single[] fBestUtility = new Single[5];
        private int nPositions; // Anzahl der zu optimierenden Positionen
        private int nAttributes;	// Anzahl der Attribute, nach denen optimiert werden soll
        private COSlot[] arSlots = new COSlot[16];	// Maximal 16 Slots, da nur maximal 2 Waffenslots
        private COAttribut[] arAttributes;

        private SCombination[] arCombinations = new SCombination[MAX_COMBINATIONS];
        private int nCombinations;

        private int[] nGems = new int[3];
        private int[][] arGems = new int[3][];

        private Single fBestUtil;
        private int iBestCombination;
        private int iBestPermutation;
        private int[] arCraftCount = new int[8];
        private TLabel[] stBest = new TLabel[5];
        private TLabel[] stCraftCount = new TLabel[8];
        private FStreamWrapper fDbg;	// filestream fürs debuggen

        private static string _(string szMsgId) { return TGnuGettextInstance.gettext(szMsgId); }
        protected override IContainer GetChildContainer() { return components ?? base.GetChildContainer(); }
        //---------------------------------------------------------------------------
        public TfrmOptimize()
        {
            for (int i = 0; i < MAX_COMBINATIONS; i++)
                arCombinations[i].Init();
            for (int i = 0; i < arGems.Length; i++)
                arGems[i] = new int[10];

            InitializeComponent();

            cbFrom.SelectedIndex = 0;
            arAttributes = null;
            Setup();
        }
        //---------------------------------------------------------------------------

        /*~TfrmOptimize()
        {
            if (arAttributes != null)
            {
                delete arAttributes;
                arAttributes = null;
            }
        }*/
        //---------------------------------------------------------------------------

        private void TfrmOptimize_FormCreate(object sender, EventArgs e)
        {
            int i;
            Utils.LoadWindowPosition("Optimize", this, false);
            for (i = 0; i < 5; i++)
                stBest[i] = (TLabel)this.FindComponent("stBest" + (i + 1).ToString());
            for (i = 0; i < 8; i++)
                stCraftCount[i] = (TLabel)this.FindComponent("stCraftCount" + (i).ToString());
        }
        //---------------------------------------------------------------------------

        private void FormClose(object sender, FormClosedEventArgs e)
        {
            Utils.SaveWindowPosition("Optimize", this, false);
        }
        //---------------------------------------------------------------------------

        private void cbFromChange(object sender, EventArgs e)
        {
            Setup();
        }
        //---------------------------------------------------------------------------

        private void chNoChangeClick(object sender, EventArgs e)
        {
            Setup();
        }
        //---------------------------------------------------------------------------

        private void Setup()	//Stellt die Itemliste für die Optimierung zusammen
        {
            int i;
            btStart.Enabled = true;
            BestCombination = -1;
            btAccept.Enabled = false;
            // Stelle fest, wieviele Werte interessieren (upv > 0)
            nAttributes = 1;
            for (i = 0; i < Unit.xml_config.nAttributes; i++)
            {
                if (Unit.player.Weights.UpV[i] > 0)
                    nAttributes++;
            }
            // Datenfeld mit allen zu beachtenden Attributen anlegen
            //if (arAttributes != null) delete arAttributes;
            arAttributes = new COAttribut[nAttributes];
            nAttributes = 1;	// Attribut 0 ist Spezial für crafted Items
            arAttributes[0].value[0] = 0;
            arAttributes[0].max = 10000;
            // Setze die Listen der craftbaren Gems zurück
            for (i = 0; i < 3; i++)
                nGems[i] = 0;
            // Das Feld ausfüllen
            for (i = 0; i < Unit.xml_config.nAttributes; i++)
            {
                if (Unit.player.Weights.UpV[i] > 0)
                {
                    arAttributes[nAttributes].id = i;
                    arAttributes[nAttributes].upv = Unit.player.Weights.UpV[i];
                    arAttributes[nAttributes].value[0] = 0;	// Grundwert
                    arAttributes[nAttributes].capatt = -1;
                    arAttributes[nAttributes].cap = (int)((Unit.player.Level + Unit.xml_config.arAttributes[i].capadd) * Unit.xml_config.arAttributes[i].capmult);
                    arAttributes[nAttributes].max = arAttributes[nAttributes].cap;
                    if (Unit.xml_config.arAttributes[i].CapId >= 0)
                    {	// Erhöht das Attribut ein Cap?
                        // Suche das Attribut in unserer Liste
                        for (int j = 0; j < nAttributes; j++)
                        {
                            if (arAttributes[j].id == Unit.xml_config.arAttributes[i].CapId)
                            {
                                arAttributes[j].max += arAttributes[nAttributes].max;
                                arAttributes[nAttributes].capatt = j;
                                break;
                            }
                        }
                    }
                    // Suche den Bonus, der dieses Attribut als erstes hat
                    arAttributes[nAttributes].ip_mult = 0;
                    for (int bid = 0; bid < Unit.xml_config.nBonuses; bid++)
                    {
                        if (Unit.xml_config.arBonuses[bid].arAttributes[0] == i)
                        {
                            arAttributes[nAttributes].ip_mult = Unit.xml_config.arBonuses[bid].ip_mult;
                            arAttributes[nAttributes].bid = bid;
                            // Ist Bonus craftbar?
                            if (Unit.xml_config.arBonuses[bid].bCraftable)
                            {	// Nimm in Liste der Craftbaren auf
                                // In welcher Gruppe ist Bonus?
                                int group;
                                switch (Unit.xml_config.arBonuses[bid].idGroup)
                                {
                                    case 1:
                                    case 3:
                                        group = 0;
                                        break;
                                    case 2:
                                    case 4:
                                        group = 1;
                                        break;
                                    case 6:
                                        group = 2;
                                        break;
                                    default:
                                        throw new NotImplementedException();
                                }
                                arGems[group][nGems[group]] = i;
                                nGems[group]++;
                            }
                            break;
                        }
                    }
                    nAttributes++;
                }
            }
            nPositions = 0;
            for (i = 0; i < CPlayer.ALL_ITEMS; i++)
            {	// Arbeite erstmal alle Items des Spielers durch
                arSlots[nPositions].bSingle = false;
                arSlots[nPositions].id = i;
                arSlots[nPositions].nItems = 0;
                if (i < 10)
                {
                    AddItem(nPositions, Unit.player.Item[i]);
                    if (Unit.player.ItemType[i] != EItemType.Crafted)
                    {	// kein Crafted Item => alle weiteren Items dieses Slots ignorieren
                        arSlots[nPositions].bSingle = true;
                    }
                    if (Unit.player.Item[i].bEquipped)
                        nPositions++;
                }
                else if (i < CPlayer.PLAYER_ITEMS)
                {	// Schmuckstücke. Wenn chNoChange checked, dann gefundenes Item als Single markieren
                    if (!Unit.player.Item[i].isEmpty())
                    {
                        AddItem(nPositions, Unit.player.Item[i]);
                        if (chNoChange.Checked)
                        {
                            arSlots[nPositions].bSingle = true;
                        }
                    }
                    nPositions++;
                }
                else
                {	// Alle anderen Items des Spielers
                    if (!Unit.player.Item[i].isEmpty())
                    {	// Welcher Slot?
                        AddItem(nPositions, Unit.player.Item[i]);
                    }
                }
            }
            // Statistische Werte
            int cSlots = 0;	// Zähle Slots, die optimiert werden sollen
            int cItems = 0;	// Anzahl der Items
            Int64 cCombi = 1;	// Anzahl der möglichen Kombinationen
            for (i = 0; i < nPositions; i++)
            {
                if (arSlots[i].nItems == 0)
                {	// Das sollte nicht sein. Schreibe ne Warnung
                    btStart.Enabled = false;
                }
                else
                {
                    if (arSlots[i].bSingle)
                    {
                    }
                    else
                    {
                        cSlots++;
                        int n = arSlots[i].nItems;
                        //				if (arSlots[i].id < 10) n++;
                        cItems += n;
                        cCombi *= n;
                    }
                }
            }
            stPositions.Text = (cSlots).ToString();
            stItems.Text = (cItems).ToString();
            stCombinations.Text = (cCombi).ToString("###,###,###,###,###,##0");
            // Positionen nochmal rückwärts durcharbeiten
            arSlots[nPositions - 1].nCombinations = 1;
            for (i = nPositions - 1; (i--) != 0; )
            {
                arSlots[i].nCombinations = arSlots[i + 1].nCombinations * arSlots[i + 1].nItems;
            }
            // Erstelle eine Liste der möglichen Steinkonfigurationen für Crafted Items
            //	fDbg = new ofstream("Debug.txt", ios_base::out | ios_base::trunc);
            nCombinations = 0;
            //	for (i = 28; i--;)
            //	{
            CountCombinations(0, 0, 1000, 27);
            //	}
            //	delete fDbg;
            // Schreibe nun ein paar Debug-Infos in ein grid
            Grid.RowCount = nAttributes - 1;
            Grid.Columns[0].HeaderCell.Value = "Name";
            Grid.Columns[1].HeaderCell.Value = "upv";
            Grid.Columns[2].HeaderCell.Value = "ip_mult";
            for (i = 1; i < nAttributes; i++)
            {
                int aid = arAttributes[i].id;
                Grid.Rows[i].HeaderCell.Value = i;
                Grid.Rows[i].Cells[0].Value = Unit.xml_config.arAttributes[aid].Name;
                Grid.Rows[i].Cells[1].Value = arAttributes[i].upv;
                Grid.Rows[i].Cells[2].Value = arAttributes[i].ip_mult;
            }
            for (i = 8; (i--) != 0; )
                arCraftCount[i] = 0;
        }

        private void CountCombinations(int level, int sum, int max, int start)
        {
            int add, nmax;
            for (int i = start; i >= 0; i--)
            {
                arCombinations[nCombinations].group[level] = i / 10;
                arCombinations[nCombinations].value[level] = i % 10;
                if (i < 10)
                {
                    add = 1 + i * 2;
                }
                else if (i < 20)
                {
                    if (i == 10)
                        add = 1;
                    else if (i == 11)
                        add = 2;
                    else
                        add = (i - 11) * 4;
                }
                else
                {
                    if (i == 20)
                        add = 1;
                    else
                        add = (i - 20) * 5;
                }
                nmax = (level == 0) ? add : max;
                if (level == 0) add += add;
                if (add > max) continue;
                int s = sum + add;
                if (level == 3)
                {
                    if ((s >= 74) && (s <= 75))
                    {
                        if (nCombinations >= MAX_COMBINATIONS - 1) continue;
                        arCombinations[nCombinations].count = 0;
                        nCombinations++;
                        // Kopiere vorherige kombination
                        for (int j = 3; (j--) != 0; )
                        {
                            arCombinations[nCombinations].group[j] = arCombinations[nCombinations - 1].group[j];
                            arCombinations[nCombinations].value[j] = arCombinations[nCombinations - 1].value[j];
                        }
                    }
                }
                else
                {
                    if (s > 74) continue;	// Übergehen, da schon max
                    else
                        CountCombinations(level + 1, s, nmax, i);
                }
            }
        }

        // Fügt das angegebene Item bei der Itemliste hinzu
        // Wandelt das Daten in das Format für die Optimierung
        // Crafted Items sind ein Spezialfall
        // Und Adde zu keinem Slot, der als Single markiert ist
        private void AddItem(int Slot, CItem item)
        {
            int i;
            int Slot2 = -1;
            // Stimmt Slot zum Item?
            if (arSlots[Slot].id >= CPlayer.PLAYER_ITEMS || Unit.xml_config.arItemSlots[arSlots[Slot].id].strPosClass != Unit.xml_config.arItemSlots[item.Position].strPosClass)
            {	// Suche die Position, wo das item hin passt
                for (i = 0; i < nPositions; i++)
                {
                    if (arSlots[i].id == item.Position)
                    {
                        Slot = i;
                        break;
                    }
                }
            }
            // Ist Item Ring oder Armreif? (Für Waffen auch noch was machen)
            if (arSlots[Slot].id == 14)
                Slot2 = 15;
            if (arSlots[Slot].id == 16)
                Slot2 = 17;
            // Wenn es nun einen Slots2 gibt, die zugehörige Position finden
            if (Slot2 >= 0)
            {
                for (i = 0; i < nPositions; i++)
                {
                    if (arSlots[i].id == Slot2)
                    {
                        Slot2 = i;
                        break;
                    }
                }
                // Wenn Position noch nicht da, dann ignorieren
                if (i == nPositions) Slot2 = -1;
            }
            // Gibt es das Item schon in derm Slot?
            // Benutze die strenge Vergleichs-Operation
            /*	for (i = 0; i < arSlots[Slot].nItems; i++)
                {
                    int id = arSlots[Slot].arItems[i].id;
                    CItem *cmpItem = ItemDB.GetItem(id);
                    // brauch ich grad noch nicht
                }*/
            // Item adden, da noch nicht in der Liste
            arSlots[Slot].arItems[arSlots[Slot].nItems].Init(nAttributes);
            arSlots[Slot].arItems[arSlots[Slot].nItems].id = Unit.ItemDB.GetItemIndex(item);
            // Wenn es einen zweiten Slot gibt (Ringe/Armreif), dann auch für den adden
            if (Slot2 >= 0)
            {
                arSlots[Slot2].arItems[arSlots[Slot2].nItems].Init(nAttributes);
                arSlots[Slot2].arItems[arSlots[Slot2].nItems].id = Unit.ItemDB.GetItemIndex(item);
            }
            // Die Spezialbehandlung für crafted items
            if (item.Type == EItemType.Crafted)
                arSlots[Slot].arItems[arSlots[Slot].nItems].Add(0, 1 << arSlots[Slot].id);
            // Besorge die Attribute zu den gespeicherten Boni (brauche die Attribute für die optimierung)
            for (i = 0; i < item.nEffects; i++)
            {
                int bid = item.Effect[i];
                if (bid >= 0)
                {	// Gehe die Attribute durch und such sie in der Attributliste
                    for (int a = 0; a < Unit.xml_config.arBonuses[bid].arAttributes.Length; a++)
                    {
                        int aid = Unit.xml_config.arBonuses[bid].arAttributes[a];
                        // Suche in der Liste
                        for (int j = 1; j < nAttributes; j++)
                        {
                            if (aid == arAttributes[j].id)
                            {	// gefunden
                                arSlots[Slot].arItems[arSlots[Slot].nItems].Add(j, item.EffectValue[i]);
                                if (Slot2 >= 0)
                                    arSlots[Slot2].arItems[arSlots[Slot2].nItems].Add(j, item.EffectValue[i]);
                                break;
                            }
                        }
                    }
                }
            }
            if (!arSlots[Slot].bSingle)
                arSlots[Slot].nItems++;
            if (!arSlots[Slot2].bSingle && Slot2 >= 0)
                arSlots[Slot2].nItems++;
        }

        private void Optimize(int position = 0)
        {
            int i, j, k, l;
            // Arbeite alle Items der Stufe durch
            for (i = 0; i < arSlots[position].nItems; i++)
            {	// kopiere zuerst die Daten von der letzten Stufe
                arSlots[position].current = i;
                for (j = 0; j < nAttributes; j++)
                    arAttributes[j].value[position + 1] = arAttributes[j].value[position];
                // Addiere nun die Werte des aktuellen Items
                int nData = arSlots[position].arItems[i].nData;
                int[] Data = arSlots[position].arItems[i].data;
                bool bBreak = false;
                for (j = 0; j < nData; j++)
                {
                    int att = Data[j * 2];
                    if (arAttributes[att].value[position] >= arAttributes[att].max)
                    {	// Vorzeitiger Abbruch, da wir schon über dem möglichen maximum sind
                        nDone += arSlots[position].nCombinations;
                        nJumped += arSlots[position].nCombinations;
                        bBreak = true;
                        break;
                    }
                    arAttributes[att].value[position + 1] += Data[j * 2 + 1];
                }
                if (bBreak) continue;
                if ((position + 1) < nPositions)
                {	// Sind noch nicht beim letzten Item. Rekursiv zum nächsten
                    Optimize(position + 1);
                }
                else
                {	// Letztes Item. Auswerten...
                    nDone++;
                    nChecked++;
                    if ((iPM++ % 64) == 0)
                        Application.DoEvents();
                    // Errechne die aktuellen Cap-Werte
                    for (j = 1; j < nAttributes; j++)
                    {
                        if (arAttributes[j].capatt >= 0)
                        {
                            int cap = arAttributes[j].value[position];
                            if (cap > arAttributes[j].cap)
                                cap = arAttributes[j].cap;
                            arAttributes[arAttributes[j].capatt].capvalue += cap;
                        }
                        else
                            arAttributes[j].capvalue = arAttributes[j].cap;
                    }
                    for (j = 1; j < nAttributes; j++)
                    {
                        arAttributes[j].dist = arAttributes[j].capvalue - arAttributes[j].value[position];
                        arAttributes[j].bUsed = (arAttributes[j].dist <= 0);	// Markiere als schon benutzt, wenn Wert über oder gleich cap ist
                    }
                    // Zähle die Crafted Items (Da ich im Moment nur 1 bearbeiten will)
                    int count = 0;
                    for (j = 0; j < 8; j++)
                    {
                        if (arAttributes[0].value[j] != arAttributes[0].value[j + 1]) count++;
                    }
                    // Hier sollten jetzt noch eventuelle crafted Sachen gemacht werden
                    if (count > 0)
                    {
                        arCraftCount[count - 1]++;
                        if (count > 1)
                            continue;	// ignoriere mehr als 1 craftet im Moment
                        // Speichere die 4 größten Werte
                        Single[] arDist = new Single[4];
                        int[] arInd = new int[4];
                        for (j = 4; (j--) != 0; )
                            arDist[j] = 0;
                        // Berechne den Abstand zu den Caps (gewichtet)
                        for (j = 1; j < nAttributes; j++)
                        {	// Hier ist noch was zu machen (Nicht steigerbare Attribute müssten nicht getestet werden)
                            if (Unit.xml_config.GetBonusId(Unit.xml_config.arBonuses[arAttributes[j].bid].GemId) >= 0) //TODO: GetBonusId is redundant, why checking for existence at all?
                            {
                                Single dist = (arAttributes[j].value[position] - arAttributes[j].cap) * arAttributes[j].upv;
                                for (k = 0; k < 3; k++)
                                {
                                    if (dist > arDist[k])
                                    {	// Alles eins nach hinten schieben
                                        for (l = 3; l > k; l--)
                                        {
                                            arDist[l] = arDist[l - 1];
                                            arInd[l] = arInd[l - 1];
                                        }
                                        arDist[k] = dist;
                                        arInd[k] = j;
                                        break;
                                    }
                                }
                            }
                        }
                        // Dann mal die 4 Steinchen setzen
                        // Arbeite alle Steinkombinationen durch
                        fBestUtil = 0;
                        for (j = nCombinations; (j--) != 0; )
                        {
                            CheckCombination(0, 0.0f, j, 0);
                        }
                        if (fBestUtil > 0)
                        {	// Gibts überhaupt eine Kombination?
                            arCombinations[iBestCombination].count++;
                            // Rechne jetzt die beste Kombination auf die anderen Items drauf
                            for (j = 0; j < 4; j++)
                            {
                                int aid = (iBestPermutation >> (j * 8)) & 0xff;
                                int value = Unit.xml_config.arBonuses[arAttributes[aid].bid].Gemvalue[arCombinations[iBestCombination].value[j]];
                                arAttributes[aid].value[position] += value;
                            }
                        }
                    }
                    // Rechne die Utility aus
                    Single fUtility = 0;
                    for (j = 1; j < nAttributes; j++)
                    {
                        if (arAttributes[j].capatt < 0)
                        {	// Es interessieren nur die normalen Werte
                            int val = arAttributes[j].value[position];
                            if (val > arAttributes[j].capvalue)
                                val = arAttributes[j].capvalue;
                            fUtility += val * arAttributes[j].upv;
                        }
                    }
                    for (j = 0; j < 5; j++)
                    {
                        if (fUtility > fBestUtility[j])
                        {	// Alles eins nach hinten schieben
                            for (k = 4; k > j; k--)
                                fBestUtility[k] = fBestUtility[k - 1];
                            fBestUtility[j] = fUtility;
                            if (j == 0)
                                BestCombination = nChecked + nJumped - 1;
                            break;
                        }
                    }
                }
            }
        }

        private void CheckCombination(int level, Single fUtil, int Combination, int current)
        {
            int group = arCombinations[Combination].group[level];
            // Und alle möglichen Gems an der Stelle
            for (int i = 0; i < nGems[group]; i++)
            {
                int id = arGems[group][i];
                if (!arAttributes[id].bUsed)
                {
                    arAttributes[id].bUsed = true;
                    int bid = arAttributes[id].bid;
                    int value = Unit.xml_config.arBonuses[bid].Gemvalue[arCombinations[Combination].value[level]];
                    int cval = (value > arAttributes[id].dist) ? arAttributes[id].dist : value;
                    Single fUtil2 = fUtil + cval * arAttributes[id].upv;
                    int current2 = current + (id << (level * 8));
                    if (level == 3)
                    {
                        if (fUtil2 > fBestUtil)
                        {
                            fBestUtil = fUtil2;
                            iBestCombination = Combination;
                            iBestPermutation = current2;
                        }
                    }
                    else
                    {	// Noch nicht alle Effekte gemacht
                        CheckCombination(level + 1, fUtil2, Combination, current2);
                    }
                    arAttributes[id].bUsed = false;
                }
            }
        }

        private void btStartClick(object sender, EventArgs e)
        {
            nDone = 0;
            nChecked = 0;
            nJumped = 0;
            for (int i = 0; i < 5; i++)
                fBestUtility[i] = 0;
            Cursor = Cursors.WaitCursor;
            tStart = DateTime.Now;
            btStart.Enabled = false;
            TimerRefresh.Enabled = true;
            Optimize(0);
            //HINT: FileMode.Create does the same as ofstream with trunc mode, because ofstream creates the file in every mode if it does not exist.
            fDbg = new FStreamWrapper("Debug.txt", FileMode.Create, FileAccess.Write);
            for (int i = 0; i < nCombinations; i++)
            {
                for (int j = 0; j < 4; j++)
                    fDbg = fDbg + arCombinations[i].group[j] + '\t' + arCombinations[i].value[j] + '\t';
                fDbg = fDbg + arCombinations[i].count + '\n';
            }
            fDbg.Dispose();
            TimerRefreshTimer(null, EventArgs.Empty);
            Cursor = Cursors.Default;
            TimerRefresh.Enabled = false;
            btStart.Enabled = true;
            if (BestCombination >= 0)
                btAccept.Enabled = true;
        }
        //---------------------------------------------------------------------------

        private void TimerRefreshTimer(object sender, EventArgs e)
        {
            int i;
            stDone.Text = (nDone).ToString("###,###,###,###,###,##0");
            stChecked.Text = (nChecked).ToString("###,###,###,###,###,##0");
            stJumped.Text = (nJumped).ToString("###,###,###,###,###,##0");
            for (i = 0; i < 5; i++)
                stBest[i].Text = (fBestUtility[i]).ToString("#0.00");
            for (i = 0; i < 8; i++)
                stCraftCount[i].Text = (arCraftCount[i]).ToString("###,###,###,###,###,##0");
            stTime.Text = (DateTime.Now - tStart).ToString("hh:mm:ss");
        }
        //---------------------------------------------------------------------------

        private void btAcceptClick(object sender, EventArgs e)
        {	// Überhnehme die Kombination in BestCombination
            for (int slot = nPositions; (slot--) != 0; )
            {
                int index = (int)(BestCombination % arSlots[slot].nItems);
                BestCombination /= arSlots[slot].nItems;
                Unit.player.Item[arSlots[slot].id] = Unit.ItemDB.GetItem(arSlots[slot].arItems[index].id);
            }
            this.Close();
        }
        //---------------------------------------------------------------------------
    }

    static partial class Unit
    {
        internal static TfrmOptimize frmOptimize;
    }
}
