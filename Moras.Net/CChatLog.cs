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
using System.Linq;
using System.Text;
using System.IO;
using DelphiClasses;
using System.Diagnostics;
using System.Windows.Forms;

namespace Moras.Net
{
    public class CChatLog : IDisposable
    {
        // Mit diesem String beginnt der Info-Bereich
        const string strInfoStart = "<Info-Beginn:";
        // Mit diesem String endet der Info-Bereich
        const string strInfoStop = "<Info-Ende>";
        // Mit diesem String beginnt der Info-Bereich
        const string strMagBoniStart = "Magieboni:";
        // Ein paar Strings zur identifikation bestimmter Werte
        const string strAF = "Ausgangswert";
        const string strAbsorb = "Absorption";	// Daraus läßt sich Rüstungsklasse gewinnen
        const string strQuality = "Qualität";
        const string strSpeed = "Waffengeschwindigkeit";
        const string strUnique = "Einzigartiger Gegenstand";
        const string strCrafted = "Hergestellt von:";
        const string strBonus = "Bonus auf";
        const string strBonus2 = "Bonus zum";
        const string strRestriction = "Geeignet für:";
        const string strArtifact = "Artefakt:";
        const string strActivLevel = "  (Benötigte Objektstufe:";

        //---------------------------------------------------------------------------
        public CChatLog()
        {
            fChatLog = null;
            arItemNames = new TStringList();
        }

        ~CChatLog()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            CloseChatLog();
            arItemNames = null;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        // Öffnet die ChatLog-Datei für Lesezugriff
        public bool OpenChatLog(string Filename)
        {
            if (fChatLog != null) { fChatLog.Dispose(); fChatLog = null; }
            // Seek funktioniert nur richtig im binary-mode. Nur ist jetzt immer /r hinter einem String
            iLineNo = 0;
            try
            {
                fChatLog = new IFStreamWrapper(Filename, FileMode.Open, FileAccess.Read);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void CloseChatLog()
        {
            if (fChatLog != null)
            {
                fChatLog.Close();
                //fChatLog.Dispose();
            }
            fChatLog = null;
        }

        // enumwerte des Parsers in Init
        enum ParserStateInit
        {
            STATE_START,	// Scanne nach Start einer Info-Section
            STATE_ITEM,	// Suche danach in der Sektion, das es ein Gegenstand ist
        }

        //TODO: fix false indices from delphi conversion
        // Diese Funktion scannt erstmal das gesamte Log nach Gegenständen und merkt
        // sich deren Position. In einem zweiten durchlauf wird versucht, die Herkunft
        // der Gegenstände herauszubekommen. Doppelte Items werden herausgefiltert
        public void Init()	// Einmal vor NextItem ausführen
        {
            int posStart = 0;	// Startposition des aktuellen Info-Bereichs
            string strName = null;	// Name des aktuellen Info-Bereichs
            string Line;	// Hier eine maximale Länge eines Textes heruasfinden
            bool bIsItem = false;

            nItems = 0;
            iLineNo = 0;
            arItemNames.Clear();
            arOffsets.Length = 0;
            ParserStateInit State = ParserStateInit.STATE_START;
            fChatLog.BaseStream.Seek(0, SeekOrigin.Begin);	// Lesezeiger auf Dateianfang
            while (!fChatLog.EndOfStream)
            {
                Line = fChatLog.ReadLine();
                Debug.Assert(Line.Length < 1024);
                iLineNo++;
                switch (State)
                {
                    case ParserStateInit.STATE_START:
                        {	// Suche nach dem Start eines Info-Bereichs
                            int pStart = Line.IndexOf(strInfoStart);
                            if (pStart != -1)
                            {
                                posStart = (int)(fChatLog.BaseStream.Position - Line.Length);
                                string sTemp = Line.Substring(pStart);
                                strName = sTemp.Substring(strInfoStart.Length + 1, sTemp.Length - strInfoStart.Length - 2);
                                // Jetzt mal schauen, ob es einen Doppelpunkt gibt.
                                int p = strName.IndexOf(':');
                                if (p > -1)
                                {	// Wenn ja, dann den String bis dorthin abschneiden
                                    strName = strName.Substring(p + 1, strName.Length - p - 1);
                                }
                                bIsItem = false;
                                State = ParserStateInit.STATE_ITEM;
                            }
                        }
                        break;
                    case ParserStateInit.STATE_ITEM:
                        {	// Suche nach Magieboni oder Info-Sektion Ende
                            int pStart = Line.IndexOf(strMagBoniStart);
                            if (pStart != -1) bIsItem = true;
                            // Ist Info-Sektion zu Ende?
                            pStart = Line.IndexOf(strInfoStop);
                            if (pStart != -1)
                            {
                                State = ParserStateInit.STATE_START;	// Nach der nächsten Info-Sektion suchen
                                if (bIsItem)
                                {	// Ende Info-Sektion und es ist ein Gegenstand
                                    // Also in die Liste der zu untersuchenden Gegenstände aufnehmen
                                    int index;
                                    if ((index = arItemNames.IndexOf(strName)) == -1)
                                    {	// Item mit diesem Namen gibts noch nicht (im Logfile)
                                        arItemNames.Add(strName);
                                        arOffsets.Length = nItems + 1;
                                        arOffsets[nItems] = posStart;
                                        nItems++;
                                    }
                                    else
                                    {	// Name gibts schon. Offset aber auf diesen setzen,
                                        // da immer das letzte das aktuellste sein sollte
                                        arOffsets[index] = posStart;
                                    }
                                }
                            }
                        }
                        break;
                }
            }
        }

        //TODO: fix false indexes from delphi conversion
        // Diese Funktion macht die eigentliche Lesearbeit
        // Input ist dabei der Index des zu beschaffenden Items und einen Zeiger
        // auf eine CItem-Klasse, welche ausgefüllt werden soll
        // Rückgabe ist true für keinen Fehler
        public bool GetItem(int index, CItem Item)	// Liest das spezifizierte Item
        {
            int i;
            string Line;	// Hier eine maximale Länge eines Textes heruasfinden
            StringBuilder sArg = new StringBuilder(64);	// Längere Bonusbezeichnungen gibts glaub nicht
            int NextBonus = 0;	// Welche Position soll der nächste Bonus eingetragen werden
            int NextRestriction = 0;	// Nächster Index einer Klassenbeschränkung
            bool bRestriction = false;

            if ((index < 0) && (index >= nItems)) return false;	// Itemindex nicht im gültigen Bereich
            Item.Init();
            Item.Name = arItemNames.Strings[index];
            Item.Realm = Unit.player.Realm;	// Immer Realm des Spielers annehmen
            Item.Quality = 0;
            // Versuche Aufgrund des Namen auf den Itemslot zu schliessen
            string strTemp = Item.Name.ToLower();
            for (i = 0; i < 18; i++)
            {
                for (int j = 0; j < Unit.xml_config.arItemSlots[i].arIds.Length; j++)
                {
                    if (strTemp.IndexOf(Unit.xml_config.arItemSlots[i].arIds[j]) > -1)
                        Item.Position = i;
                }
            }
            // Lesezeiger auf Beginn der entsprechenden Info-Sektion stellen
            fChatLog.BaseStream.Flush();	// Ist nötig, da wir eventuell noch im eof-state sind
            fChatLog.BaseStream.Seek(arOffsets[index], SeekOrigin.Begin);
            Line = fChatLog.ReadLine();	// Erste Zeile Lesen (kann ignoriert werden)
            Line = fChatLog.ReadLine();	// Erste Zeile mit Infos
            while (!fChatLog.EndOfStream)
            {
                Debug.Assert(Line.Length < 1024);
                int Value = 0;
                // Ist Info-Sektion zu Ende?
                int pStart = Line.IndexOf(strInfoStop);
                if (pStart != -1) break;
                if (Line.Length > 11 && Line[11] == '-')
                {	// Alles danach ist ein Wert, der mich interessiert
                    // Suche erstmal nach einem Zahlenwert. Sollte immer nur einer sein
                    // Bonus-Argument ist immer der String vor einem Doppelpunkt,
                    // oder der String hinter einer Zahl
                    bool bValue = false;	// Gibts eine Zahl?
                    bool bDoppel = false;	// gibt es einen Doppelpunkt im String
                    bool bPercent = false;	// Es ist ein Prozent-Wert
                    sArg.Length = 0;// Aktuelle Position im Argument-String
                    for (i = 13; i < Line.Length; i++)
                    {
                        if ((Line[i] >= '0') && (Line[i] <= '9'))
                        {
                            bValue = true;
                            Value = Value * 10 + Line[i] - '0';
                        }
                        else if (Line[i] == ':') bDoppel = true;
                        else if (Line[i] == ' ')
                        {
                            if (bValue && (bDoppel || (sArg.Length > 0))) break;
                            sArg.Length = 0;
                        }
                        else if (Line[i] == '%')
                            bPercent = true;
                        else if ((Line[i] == '\r') || (Line[i] == '.')) { }	// Diese Zeichen ignorieren
                        else
                        {
                            sArg.Append(Line[i]);
                        }
                    }
                    if (bValue)
                    {	// In xml_config nach einem entsprechendem Boni suchen
                        int bid = Unit.xml_config.GetBonusId(sArg.ToString(), bPercent);
                        if (bid < 0)
                        {	// testen, ob der gesamte Text nicht vielleicht doch ein Bonus ist
                            bid = Unit.xml_config.GetBonusId(Line.Substring(14, i - 16), bPercent);
                        }
                        if (bid >= 0)
                        {	// Es ist ein regulärer Bonus
                            Item.Effect[NextBonus] = bid;
                            Item.EffectValue[NextBonus] = Value;
                            NextBonus++;
                        }
                        else
                        {	// Hier ein paar zusätzliche Werte nehmen, die keine Boni sind
                            if (string.Compare(sArg.ToString(), strAF) == 0) Item.AF = Value;
                            if (string.Compare(sArg.ToString(), strQuality) == 0) Item.Quality = Value;
                            if (string.Compare(sArg.ToString(), strSpeed) == 0) Item.Speed = Value;
                            if (string.Compare(sArg.ToString(), strAbsorb) == 0)
                            {	// Value in Rüstungsklasse umwandeln
                                switch (Value)
                                {
                                    case 0: Item.Class = 0; Item.Level = Item.AF; break;
                                    case 10: Item.Class = 1; Item.Level = Item.AF / 2; break;
                                    case 19: Item.Class = 2; Item.Level = Item.AF / 2; break;
                                    case 27: Item.Class = 3; Item.Level = Item.AF / 2; break;
                                    case 34: Item.Class = 4; Item.Level = Item.AF / 2; break;
                                }
                                //if (Item.Position < 0)
                                //	Item.Position = 0;	// Alle Rüstungen als Handschuhe annehmen
                                // Es gibt leider keine Info darüber im Log
                                // Wenn wir die Position auf -1 lassen, dann kommt automatisch eine Auswahlbox
                            }
                            if (string.Compare(sArg.ToString(), "DPS") == 0)
                            {	// Nur ersten DPS-Wert speichern
                                if (Item.DPS == 0)
                                {
                                    Item.DPS = Value;
                                    // Berechne den Itemlevel
                                    int level = (Value - 11) / 3;
                                    if (level > 51) level = 51;
                                    Item.Level = level;
                                }
                            }
                        }
                    }
                }
                //		else
                {	// noch ein paar besondere Strings auswerten
                    if (string.Compare(Line, 11, strUnique, 0, strUnique.Length) == 0)
                    {	// Unique Gegenstände nicht importieren
                        return false;
                    }
                    if (string.Compare(Line, 11, strCrafted, 0, strCrafted.Length) == 0)
                    {	// Craftet Gegenstände nicht importieren
                        return false;
                    }
                    if (string.Compare(Line, 11, strArtifact, 0, strArtifact.Length) == 0)
                    {
                        Item.MaxLevel = 10;
                        Item.Realm = 7;
                    }
                    if ((string.Compare(Line, 11, strBonus, 0, strBonus.Length) == 0)
                     || (string.Compare(Line, 11, strBonus2, 0, strBonus2.Length) == 0))
                    {	// Spezielle Werte (meist Bonuserhöhungen)
                        // Bonus auf BONUS: VALUE
                        for (i = 21; i < Line.Length; i++)
                        {	// Alles bis zum Doppelpunkt in sArg kopieren
                            if (Line[i] == ':') break;
                            sArg[i - 21] = Line[i];
                        }
                        sArg.Length = i - 21;
                        for (; i < Line.Length; i++)
                        {
                            if ((Line[i] >= '0') && (Line[i] <= '9'))
                            {
                                Value = Value * 10 + Line[i] - '0';
                            }
                        }
                        // In xml_config nach einem entsprechendem Boni suchen
                        int bid = Unit.xml_config.GetBonusId(sArg.ToString());
                        if (bid >= 0)
                        {	// Es ist ein regulärer Bonus
                            Item.Effect[NextBonus] = bid;
                            Item.EffectValue[NextBonus] = Value;
                            NextBonus++;
                        }
                        else
                        {	// Fehlermeldung
                            MessageBox.Show("Ein Bonus '" + sArg.ToString() + "' ist unbekannt!\nDie Datei 'config.xml' bzw. die entsprechende Sprachdatei anpassen!", "Fehler");
                        }
                    }
                    if (string.Compare(Line, 11, strActivLevel, 0, strActivLevel.Length) == 0)
                    {	// Danach kommt ne Zahl, welche den Level angibt
                        int level = Utils.Str2Int(Line, 11 + strActivLevel.Length);
                        Item.EffectLevel[NextBonus - 1] = level;
                    }
                    int cp = 12;
                    if ((string.Compare(Line, 11, strRestriction, 0, strRestriction.Length) == 0)
                     || (string.Compare(Line, 12, strRestriction, 0, strRestriction.Length) == 0))
                    {	// Solange nachfolgende Strings eine Klasse darstellen, solange
                        // diese bei den Beschränkungen eintragen
                        bRestriction = true;
                        cp = 12 + strRestriction.Length;
                    }
                    while (bRestriction && (cp < Line.Length))
                    {
                        int pArg = 0;
                        for (; cp < Line.Length; cp++)
                        {
                            if ((Line[cp] == ' ') || (Line[cp] == ',') || (Line[cp] == '-') || (Line[cp] == '\r'))
                            {
                                sArg.Length = pArg;
                                if (pArg > 0)
                                {	// Es gibt einen String. Schauen obs ne Klasse ist
                                    int cid = Unit.xml_config.GetClassId(sArg.ToString());
                                    if (cid >= 0)
                                    {
                                        Item.ClassRestriction[NextRestriction++] = cid;
                                    }
                                    else
                                    {	// Keine Klasse mehr, Modus aufheben
                                        bRestriction = false;
                                    }
                                }
                                pArg = 0;
                            }
                            else
                            {
                                sArg[pArg++] = Line[cp];
                            }
                        }
                    }
                }
                Line = fChatLog.ReadLine();
            }
            return true;
        }

        private TStringList arItemNames;
        private DynamicArray<int> arOffsets;
        private int nItems;
        private int iLineNo;	// Die aktuell bearbeitete Zeilennummer
        private IFStreamWrapper fChatLog;
        public int LineNo { get { return iLineNo; } }
        public int NItems { get { return nItems; } }
    }
}
