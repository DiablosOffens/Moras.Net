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
using DelphiClasses;

namespace Moras.Net
{

    public enum EAttType
    {
        AT_ATTRIBUTE,
        AT_RESISTANCE,
        AT_SKILL,
        AT_OTHER,
        AT_NOVIEW
    }

    // Dieses Struct beschreibt den aktuellen Status eines Attributes
    // Ersetzt die temporären Variablen bei "xml_config.arAttributes"
    public struct SAttributeState
    {
        // Allgemeine Eigenschaften, abhängig von iClass:
        public bool bActive;            // falls false, ist dieser Eintrag inaktiv bzgl. Nutzenrechnung
        public bool bUsable;			// falls false, braucht dieses Attribut eines Boni nicht betrachtet werden
        public EAttType eType;              // Attributtyp bzw. Zuordnungen zu den einzelnen Grp-Boxen
        public int iDisplayID;         // (Zwischen-)Speicherung der DisplayID, eigentlich nur für die Skills interessant

        // Die aufsummierten Stats der Items zu diesem Attribut:
        public int iItemsValue;        // Summe Attribut-Wert (z.B. "Stärke")
        public int iItemsCap;          // Summe Cap-Erhöhungen (z.B. "Stärke-Cap")
        public int iItemsOvercap;      // Summe Übercap-Erhöhungen (z.B. "Über-CAP Stärke")
        public int iSingle;			// +1 für jeden Bonus eines Items, der dieses Attribur als "Single" referenziert

        // Die entsprechenden Preview-Summen
        public int iPreviewValue;      // Summe Attribut-Wert (z.B. "Stärke")
        public int iPreviewCap;        // Summe Cap-Erhöhungen (z.B. "Stärke-Cap")
        public int iPreviewOvercap;    // Summe Übercap-Erhöhungen (z.B. "Über-CAP Stärke")

        // Die von iLevel abhängigen Caps:
        public int iPlayerCapBase;     // Normales levelbasiertes Cap
        public int iPlayerCapIncCap;    // Normales levelbasiertes Cap für Cap-Erhöhungen
        public int iPlayerCapIncOvercap;    // Normales levelbasiertes Cap für Über-Cap-Erhöhungen
    }

    //---------------------------------------------------------------------------
    // Alle Attribut und Utilitydaten als Paket
    public class CPlayerAttributes : ICloneable
    {
        private DynamicArray<SAttributeState> arAttributeStates;	// Alle Wichtungen und zugeordnete Werte für jedes Attribut
        private int iLevel;                                 // Redundant bzgl. CPlayer. Hiermit kann aber direkt geprüft werden
        // ob die player_... Eigenschaften neu berechnet werden müssen.
        private int iClass;                                 // Dito...
        private DynamicArray<int> arOthers;                         // ID-Liste der AT_OTHER Attribute
        private bool bPreview;			                    // falls true, werden nur previewwerte geädedrt

        public int Level { get { return iLevel; } set { SetLevel(value); } }
        public int Class { get { return iClass; } set { SetClass(value); } }

        private IndexerProperty<int, int> _effCap;
        public IndexerProperty<int, int> EffCap { get { return _effCap ?? (_effCap = new IndexerProperty<int, int> { read = GetEffCap }); } }
        private IndexerProperty<int, int> _value;
        public IndexerProperty<int, int> Value { get { return _value ?? (_value = new IndexerProperty<int, int> { read = GetValue }); } }
        private IndexerProperty<int, int> _cap;
        public IndexerProperty<int, int> Cap { get { return _cap ?? (_cap = new IndexerProperty<int, int> { read = GetCap }); } }
        private IndexerProperty<int, int> _overcap;
        public IndexerProperty<int, int> Overcap { get { return _overcap ?? (_overcap = new IndexerProperty<int, int> { read = GetOvercap }); } }

        public CPlayerAttributes()
        {
            // Erstmal Array anlegen
            arAttributeStates.Length = Unit.xml_config.nAttributes;
            arOthers.Length = 0;

            // Total-Reset
            for (int i = 0; i < arAttributeStates.Length; i++)
            {
                arAttributeStates.Array[i].bActive = false;
                arAttributeStates.Array[i].bUsable = false;
                arAttributeStates.Array[i].eType = EAttType.AT_NOVIEW;
                arAttributeStates.Array[i].iDisplayID = -1;

                arAttributeStates.Array[i].iItemsValue = 0;
                arAttributeStates.Array[i].iItemsCap = 0;
                arAttributeStates.Array[i].iItemsOvercap = 0;
                arAttributeStates.Array[i].iSingle = 0;

                arAttributeStates.Array[i].iPreviewValue = 0;
                arAttributeStates.Array[i].iPreviewCap = 0;
                arAttributeStates.Array[i].iPreviewOvercap = 0;

                arAttributeStates.Array[i].iPlayerCapBase = 0;
                arAttributeStates.Array[i].iPlayerCapIncCap = 0;
                arAttributeStates.Array[i].iPlayerCapIncOvercap = 0;
            }

            // Daten initialisieren
            bPreview = false;
            iClass = -1;
            iLevel = 50;
            SetClass(0); // Level wird automatisch mitaktualisiert, auch Clear wird automatisch aufgerufen
        }
        //---------------------------------------------------------------------------
        /*~CPlayerAttributes()
        {
            arAttributeStates.Length = 0;
        }*/
        //---------------------------------------------------------------------------
        public void Clear()
        {
            int i, did;
            if (!bPreview)
            {
                for (i = 0; i < arAttributeStates.Length; i++)
                {
                    arAttributeStates.Array[i].iItemsValue = 0;
                    arAttributeStates.Array[i].iItemsCap = 0;
                    arAttributeStates.Array[i].iItemsOvercap = 0;
                    arAttributeStates.Array[i].iSingle = 0;
                }
                arOthers.Length = 0;
            }
            else
            {
                for (i = 0; i < arAttributeStates.Length; i++)
                {
                    arAttributeStates.Array[i].iPreviewValue = 0;
                    arAttributeStates.Array[i].iPreviewCap = 0;
                    arAttributeStates.Array[i].iPreviewOvercap = 0;
                }
            }
        }
        //---------------------------------------------------------------------------
        protected int GetEffCap(int pos)
        {
            if (pos >= 0 && pos <= arAttributeStates.Length)
            {
                int iEffMaxCap = Math.Min(arAttributeStates[pos].iItemsCap, arAttributeStates[pos].iPlayerCapIncCap);
                if (arAttributeStates[pos].iPlayerCapIncOvercap > 0) // use cap from over-cap only if this attribute has a cap in over-cap
                    iEffMaxCap = Math.Min(iEffMaxCap + arAttributeStates[pos].iItemsOvercap, arAttributeStates[pos].iPlayerCapIncOvercap);
                return arAttributeStates[pos].iPlayerCapBase + iEffMaxCap;
            }
            else
            {
                return 0;
            }
        }
        //---------------------------------------------------------------------------
        protected int GetValue(int pos)
        {
            if (pos >= 0 && pos <= arAttributeStates.Length)
            {
                return arAttributeStates[pos].iItemsValue;
            }
            else
            {
                return 0;
            }
        }
        //---------------------------------------------------------------------------
        protected int GetCap(int pos)
        {
            if (pos >= 0 && pos <= arAttributeStates.Length)
            {
                return arAttributeStates[pos].iItemsCap;
            }
            else
            {
                return 0;
            }
        }
        //---------------------------------------------------------------------------
        protected int GetOvercap(int pos)
        {
            if (pos >= 0 && pos <= arAttributeStates.Length)
            {
                return arAttributeStates[pos].iItemsOvercap;
            }
            else
            {
                return 0;
            }
        }
        //---------------------------------------------------------------------------
        // Falls später Preview auch für zu ladende Chars gelten soll,
        // muss hier noch die bPreview-Variante rein
        // Für ManageDB ist dies noch nicht nötig...
        protected void SetClass(int Class)
        {                // Setzt iClass und rechnet ggf. die player_... Eigenschaften neu aus
            if (Class != iClass)
            {
                iClass = Class;
                Clear();	// Reset!

                for (int i = 0; i < arAttributeStates.Length; i++)
                {
                    // Erstmal aktualisieren, was für Typ und ob aktiv
                    int did = Unit.xml_config.arAttributes[i].displayid;
                    if (did >= 0)
                    {	// Wert wird auf jeden Fall angezeigt, wenn es eine displayid gibt
                        // Spezialbehandlung für Magie-Wert
                        int j = Unit.xml_config.arClasses[Class].iMagic;
                        // displayid=4: Int, Frö, Cha, Emp
                        // displayid=6: Manapool
                        // j==i: Magiewert dieser Klasse => Auf jedenfall aktiv
                        if ((j == i) || ((did != 4) && ((did != 6) || (j >= 0))))
                        {
                            // Resi
                            arAttributeStates.Array[i].bActive = true;
                            arAttributeStates.Array[i].bUsable = true;
                            arAttributeStates.Array[i].eType = (did < 8) ? EAttType.AT_ATTRIBUTE : EAttType.AT_RESISTANCE;
                            arAttributeStates.Array[i].iDisplayID = did;
                        }
                        else
                        {
                            arAttributeStates.Array[i].bActive = false;
                            arAttributeStates.Array[i].bUsable = false;
                            arAttributeStates.Array[i].eType = (arAttributeStates[i].iSingle > 0) ? EAttType.AT_OTHER : EAttType.AT_NOVIEW;
                            arAttributeStates.Array[i].iDisplayID = -1;
                            // z.B. Sinnesschärfe bei Tanks wird nicht angezeigt.
                            // Aber Frömmigkeit bei Hib/Alb Chars bei Sonstiges
                        }
                    }
                    else
                    {
                        // Test auf nutzbaren Skill
                        if (Unit.xml_config.arAttributes[i].capadd == 5)
                        {
                            // Nur Skills haben ein CapAdd von 5.
                            bool bFound = false;
                            for (int j = 0; j < Unit.xml_config.arClasses[Class].nSkills; j++)
                            {
                                int aid = Unit.xml_config.arClasses[Class].arSkills[j];
                                if (aid == i)
                                {
                                    arAttributeStates.Array[i].bActive = true;
                                    arAttributeStates.Array[i].bUsable = true;
                                    arAttributeStates.Array[i].eType = EAttType.AT_SKILL;
                                    arAttributeStates.Array[i].iDisplayID = 17 + j;
                                    bFound = true;
                                    break;
                                }
                            }
                            if (!bFound)
                            {
                                arAttributeStates.Array[i].bActive = false;
                                arAttributeStates.Array[i].bUsable = false;
                                arAttributeStates.Array[i].eType = EAttType.AT_OTHER;
                                arAttributeStates.Array[i].iDisplayID = -1;
                            }
                            continue; /* fertig */
                        }

                        // Test auf nutzbaren Fokus
                        if (Unit.xml_config.arAttributes[i].SkillId >= 0)
                        {
                            // SkillId = zugehöriger Skill
                            bool bFound = false;
                            int aid = Unit.xml_config.arAttributes[i].SkillId;
                            for (int j = 0; j < Unit.xml_config.arClasses[Class].nSkills; j++)
                            {
                                if (aid == Unit.xml_config.arClasses[Class].arSkills[j])
                                {
                                    arAttributeStates.Array[i].bActive = true;
                                    arAttributeStates.Array[i].bUsable = true;
                                    arAttributeStates.Array[i].eType = EAttType.AT_OTHER;
                                    arAttributeStates.Array[i].iDisplayID = -1;
                                    bFound = true;
                                    break;
                                }
                            }
                            if (!bFound)
                            {
                                arAttributeStates.Array[i].bActive = false;
                                arAttributeStates.Array[i].bUsable = false;
                                arAttributeStates.Array[i].eType = EAttType.AT_OTHER;
                                arAttributeStates.Array[i].iDisplayID = -1;
                            }
                            continue; /* fertig */
                        }

                        // Test auf Cap-Erhöhung
                        if (Unit.xml_config.arAttributes[i].CapId >= 0)
                        {
                            arAttributeStates.Array[i].bUsable = true;
                            arAttributeStates.Array[i].bActive = false;
                            arAttributeStates.Array[i].eType = EAttType.AT_NOVIEW;
                            arAttributeStates.Array[i].iDisplayID = -1;
                        }
                        else
                        {
                            // Bleibt nur noch ToA/Cata/LotM Bonis => Other
                            arAttributeStates.Array[i].bActive = true;
                            arAttributeStates.Array[i].bUsable = true;
                            arAttributeStates.Array[i].eType = EAttType.AT_OTHER;
                            arAttributeStates.Array[i].iDisplayID = -1;
                        }
                    }
                }

                int iTmpLevel = iLevel;
                iLevel = -1;
                SetLevel(iTmpLevel);
            }
        }
        //---------------------------------------------------------------------------
        // Falls später Preview auch für zu ladende Chars gelten soll,
        // muss hier noch die bPreview-Variante rein
        // Für ManageDB ist dies noch nicht nötig...
        protected void SetLevel(int Level)
        {                // Setzt iLevel und rechnet ggf. die player_... Eigenschaften neu aus
            if (Level != iLevel)
            {
                iLevel = Level;
                // Erstmal alle Cap-Werte berechnen
                for (int i = 0; i < arAttributeStates.Length; i++)
                {
                    arAttributeStates.Array[i].iPlayerCapBase = (int)(((Single)Level + Unit.xml_config.arAttributes[i].capadd) * Unit.xml_config.arAttributes[i].capmult);
                    arAttributeStates.Array[i].iPlayerCapIncCap = 0; // Cap-Limit initialisieren
                    arAttributeStates.Array[i].iPlayerCapIncOvercap = 0;
                }
                // Nun die Cap-Limits kopieren
                for (int i = 0; i < arAttributeStates.Length; i++)
                {
                    int iStatId = Unit.xml_config.arAttributes[i].CapId;
                    if (iStatId >= 0)
                    {
                        if (!Unit.xml_config.arAttributes[i].bCapAttr)
                            arAttributeStates.Array[iStatId].iPlayerCapIncCap = arAttributeStates[i].iPlayerCapBase;
                        else
                            arAttributeStates.Array[iStatId].iPlayerCapIncOvercap = arAttributeStates[i].iPlayerCapBase;
                    }
                }
            }
        }
        //---------------------------------------------------------------------------
        // Addiert/Subtrahiert die Effekte eines Items
        public static CPlayerAttributes operator +(CPlayerAttributes lhs, CItem rhs)
        {			// Add-Operator
            // Gibt es eine Klassenbeschränkung?
            if (rhs.isUseable(lhs.iClass))
            {
                for (int i = 0; i < rhs.nEffects; i++)
                {
                    int bid = rhs.Effect[i];
                    if (bid >= 0)
                    {
                        // Wenn es ein Dropitem ist, dann Effektlevel beachten
                        if ((rhs.Type != EItemType.Drop) ||
                            (rhs.EffectLevel[i] <= rhs.CurLevel))
                        {
                            for (int j = 0; j < Unit.xml_config.arBonuses[bid].arAttributes.Length; j++)
                            {
                                int aid = Unit.xml_config.arBonuses[bid].arAttributes[j];
                                // Hier nun testen, ob der Spieler das Attribut hat
                                // Nur testen, wenn mehr als 1 Attribut
                                if ((Unit.xml_config.arBonuses[bid].arAttributes.Length == 1) || lhs.arAttributeStates[aid].bUsable)
                                {	// Spieler hat das Attribut, deshalb rechnen
                                    int cid = Unit.xml_config.arAttributes[aid].CapId;
                                    // Falls Preview-Modus, wird dieser Gegenstand nur zu den Preview-Werten addiert
                                    if (!lhs.bPreview)
                                    {
                                        if (cid >= 0)
                                        {	// Caperhöhung
                                            if (!Unit.xml_config.arAttributes[aid].bCapAttr)
                                                lhs.arAttributeStates.Array[cid].iItemsCap += rhs.EffectValue[i];
                                            else
                                                lhs.arAttributeStates.Array[cid].iItemsOvercap += rhs.EffectValue[i];
                                        }
                                        else
                                            lhs.arAttributeStates.Array[aid].iItemsValue += rhs.EffectValue[i]; // Normale Statserhöhung
                                    }
                                    else
                                    {
                                        if (cid >= 0)
                                        {	// Caperhöhung
                                            if (!Unit.xml_config.arAttributes[aid].bCapAttr)
                                                lhs.arAttributeStates.Array[cid].iPreviewCap += rhs.EffectValue[i];
                                            else
                                                lhs.arAttributeStates.Array[cid].iPreviewOvercap += rhs.EffectValue[i];
                                        }
                                        else
                                            lhs.arAttributeStates.Array[aid].iPreviewValue += rhs.EffectValue[i]; // Normale Statserhöhung
                                    }
                                    if (Unit.xml_config.arBonuses[bid].arAttributes.Length == 1)
                                    {
                                        lhs.arAttributeStates.Array[aid].iSingle++;

                                        if (!lhs.arAttributeStates[i].bActive &&
                                         (Unit.xml_config.arAttributes[i].displayid == 4 || Unit.xml_config.arAttributes[i].displayid == 6))
                                            lhs.arAttributeStates.Array[i].eType = (lhs.arAttributeStates[i].iSingle > 0) ? EAttType.AT_OTHER : EAttType.AT_NOVIEW;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return lhs;
        }
        //---------------------------------------------------------------------------
        public static CPlayerAttributes operator -(CPlayerAttributes lhs, CItem rhs)
        {			// Sub-Operator
            // Gibt es eine Klassenbeschränkung?
            if (rhs.isUseable(lhs.iClass))
            {
                for (int i = 0; i < rhs.nEffects; i++)
                {
                    int bid = rhs.Effect[i];
                    if (bid >= 0)
                    {
                        // Wenn es ein Dropitem ist, dann Effektlevel beachten
                        if ((rhs.Type != EItemType.Drop) ||
                            (rhs.EffectLevel[i] <= rhs.CurLevel))
                        {
                            for (int j = 0; j < Unit.xml_config.arBonuses[bid].arAttributes.Length; j++)
                            {
                                int aid = Unit.xml_config.arBonuses[bid].arAttributes[j];
                                // Hier nun testen, ob der Spieler das Attribut hat
                                // Nur testen, wenn mehr als 1 Attribut
                                if ((Unit.xml_config.arBonuses[bid].arAttributes.Length == 1) || lhs.arAttributeStates[aid].bUsable)
                                {	// Spieler hat das Attribut, deshalb rechnen
                                    int cid = Unit.xml_config.arAttributes[aid].CapId;
                                    // Falls Preview-Modus, wird dieser Gegenstand nur von den Preview-Werten abgezogen
                                    if (!lhs.bPreview)
                                    {
                                        if (cid >= 0)
                                        {	// Caperhöhung
                                            lhs.arAttributeStates.Array[cid].iItemsCap -= rhs.EffectValue[i];
                                            if (Unit.xml_config.arAttributes[aid].bCapAttr)
                                                lhs.arAttributeStates.Array[cid].iItemsOvercap -= rhs.EffectValue[i];
                                        }
                                        else
                                            lhs.arAttributeStates.Array[aid].iItemsValue -= rhs.EffectValue[i]; // Normale Statserhöhung
                                    }
                                    else
                                    {
                                        if (cid >= 0)
                                        {	// Caperhöhung
                                            lhs.arAttributeStates.Array[cid].iPreviewCap -= rhs.EffectValue[i];
                                            if (Unit.xml_config.arAttributes[aid].bCapAttr)
                                                lhs.arAttributeStates.Array[cid].iPreviewOvercap -= rhs.EffectValue[i];
                                        }
                                        else
                                            lhs.arAttributeStates.Array[aid].iPreviewValue -= rhs.EffectValue[i]; // Normale Statserhöhung
                                    }

                                    if (Unit.xml_config.arBonuses[bid].arAttributes.Length == 1)
                                    {
                                        lhs.arAttributeStates.Array[aid].iSingle--;

                                        if (!lhs.arAttributeStates[i].bActive &&
                                         (Unit.xml_config.arAttributes[i].displayid == 4 || Unit.xml_config.arAttributes[i].displayid == 6))
                                            lhs.arAttributeStates.Array[i].eType = (lhs.arAttributeStates[i].iSingle > 0) ? EAttType.AT_OTHER : EAttType.AT_NOVIEW;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return lhs;
        }
        //---------------------------------------------------------------------------
        public CPlayerAttributes(CPlayerAttributes rhs)
        { // Copy-Konstruktor
            iClass = rhs.iClass;
            iLevel = rhs.iLevel;
            arAttributeStates.Length = rhs.arAttributeStates.Length;
            for (int i = 0; i < arAttributeStates.Length; i++)
            {
                arAttributeStates[i] = rhs.arAttributeStates[i];
            }
            arOthers.Length = rhs.arOthers.Length;
            for (int i = 0; i < arOthers.Length; i++)
            {
                arOthers[i] = rhs.arOthers[i];
            }
        }
        #region ICloneable Members

        public object Clone()
        {
            return new CPlayerAttributes(this);
        }

        #endregion
        //---------------------------------------------------------------------------
        public Single CalcGesamtNutzen(CPlayerWeights Weights)
        {
            double dTotalUtility = 0;
            // Erstmal alle Cap-Werte berechnen
            for (int i = 0; i < arAttributeStates.Length; i++)
            {
                if (arAttributeStates[i].bActive)
                {
                    int iEffMaxCap = Math.Min(arAttributeStates[i].iItemsCap, arAttributeStates[i].iPlayerCapIncCap);
                    if (arAttributeStates[i].iPlayerCapIncOvercap > 0) // use cap from over-cap only if this attribute has a cap in over-cap
                        iEffMaxCap = Math.Min(iEffMaxCap + arAttributeStates[i].iItemsOvercap, arAttributeStates[i].iPlayerCapIncOvercap);
                    int iEffCap = arAttributeStates[i].iPlayerCapBase + iEffMaxCap;
                    int iEffVal = Math.Min(arAttributeStates[i].iItemsValue, iEffCap);
                    dTotalUtility += Weights.UpV[i] * iEffVal;
                }
            }
            return (Single)dTotalUtility;
        }
        //---------------------------------------------------------------------------
        public void SetPreviewMode(bool Preview)
        {
            if (!bPreview && Preview)
            {
                // Start Preview: Kopiere Daten erstmal
                bPreview = true;
                for (int i = 0; i < arAttributeStates.Length; i++)
                {
                    arAttributeStates.Array[i].iPreviewValue = arAttributeStates[i].iItemsValue;
                    arAttributeStates.Array[i].iPreviewCap = arAttributeStates[i].iItemsCap;
                    arAttributeStates.Array[i].iPreviewOvercap = arAttributeStates[i].iItemsOvercap;
                }
            }
            else if (bPreview && !Preview)
            {
                // Ende Preview: Lösche Daten
                bPreview = false;
                for (int i = 0; i < arAttributeStates.Length; i++)
                {
                    arAttributeStates.Array[i].iPreviewValue = 0;
                    arAttributeStates.Array[i].iPreviewCap = 0;
                    arAttributeStates.Array[i].iPreviewOvercap = 0;
                }
            }
        }
        //---------------------------------------------------------------------------

        // Aktualisiert die Anzeige mit den Werten dieses CPlayerAttributes-Objektes
        public void Display() // Bildet Status auf frmMain ab
        {
            int i, j, did, imax, idx, iOtherPos = Unit.frmMain.sbOthers.Value;

            // Erst alle aktiven Attribute durchgehen
            arOthers.Length = 0;
            for (i = 0; i < arAttributeStates.Length; i++)
                if (arAttributeStates[i].bActive)
                {
                    did = arAttributeStates[i].iDisplayID;
                    switch (arAttributeStates[i].eType)
                    {
                        case EAttType.AT_ATTRIBUTE:
                        case EAttType.AT_RESISTANCE:
                        case EAttType.AT_SKILL:
                            // Aktualisiere Anzeige-Werte
                            Unit.frmMain.Attributes[did].CapBase = arAttributeStates[i].iPlayerCapBase;
                            Unit.frmMain.Attributes[did].CapIncCap = arAttributeStates[i].iPlayerCapIncCap;
                            Unit.frmMain.Attributes[did].CapIncOvercap = arAttributeStates[i].iPlayerCapIncOvercap;

                            Unit.frmMain.Attributes[did].Value = arAttributeStates[i].iItemsValue;
                            Unit.frmMain.Attributes[did].CapInc = arAttributeStates[i].iItemsCap;
                            Unit.frmMain.Attributes[did].Overcap = arAttributeStates[i].iItemsOvercap;
                            if (!bPreview)
                            {
                                Unit.frmMain.Attributes[did].ValueChange = 0;
                                Unit.frmMain.Attributes[did].CapIncChange = 0;
                                Unit.frmMain.Attributes[did].OvercapChange = 0;
                            }
                            else
                            {
                                Unit.frmMain.Attributes[did].ValueChange = arAttributeStates[i].iPreviewValue - arAttributeStates[i].iItemsValue;
                                Unit.frmMain.Attributes[did].CapIncChange = arAttributeStates[i].iPreviewCap - arAttributeStates[i].iItemsCap;
                                Unit.frmMain.Attributes[did].OvercapChange = arAttributeStates[i].iPreviewOvercap - arAttributeStates[i].iItemsOvercap;
                            }
                            break;
                        case EAttType.AT_OTHER:
                            // Speichere ID
                            if (arAttributeStates[i].iItemsValue > 0 ||
                                arAttributeStates[i].iItemsCap > 0 ||
                                arAttributeStates[i].iItemsOvercap > 0 ||
                                arAttributeStates[i].iPreviewValue > 0 ||
                                arAttributeStates[i].iPreviewCap > 0 ||
                                arAttributeStates[i].iPreviewOvercap > 0)
                            {
                                arOthers.Length++;
                                arOthers[arOthers.Length - 1] = i;
                            }
                            break;
                        case EAttType.AT_NOVIEW:
                            break;  // keine Anzeige!
                    }
                }
            // Dann noch die inaktiven AT_OTHER Attribute anhängen
            for (i = 0; i < arAttributeStates.Length; i++)
                if ((!arAttributeStates[i].bActive) && (arAttributeStates[i].eType == EAttType.AT_OTHER) && (
                        arAttributeStates[i].iItemsValue > 0 ||
                        arAttributeStates[i].iItemsCap > 0 ||
                        arAttributeStates[i].iItemsOvercap > 0 ||
                        arAttributeStates[i].iPreviewValue > 0 ||
                        arAttributeStates[i].iPreviewCap > 0 ||
                        arAttributeStates[i].iPreviewOvercap > 0))
                {
                    arOthers.Length++;
                    arOthers[arOthers.Length - 1] = i;
                }
            // iOtherPos validieren!
            if (iOtherPos < 0 || (iOtherPos > 0 && arOthers.Length <= 9))
                iOtherPos = 0;
            if (arOthers.Length > 9 && arOthers.Length - iOtherPos < 9)
                iOtherPos = arOthers.Length - 9;

            // Nun die Anzeige der 9 Einträge aktualisieren
            for (i = iOtherPos, idx = 25; idx < 34; i++, idx++)
            {
                if (i < arOthers.Length)
                {
                    j = arOthers[i]; // Hole ID
                    // Aktualisieren Grunddaten
                    Unit.frmMain.Attributes[idx].Visible = true;
                    Unit.frmMain.Attributes[idx].Text = Unit.xml_config.arAttributes[j].Name;
                    Unit.frmMain.Attributes[idx].Data = j;

                    // Aktualisiere Anzeige-Werte
                    Unit.frmMain.Attributes[idx].CapBase = arAttributeStates[j].iPlayerCapBase;
                    Unit.frmMain.Attributes[idx].CapIncCap = arAttributeStates[j].iPlayerCapIncCap;
                    Unit.frmMain.Attributes[idx].CapIncOvercap = arAttributeStates[j].iPlayerCapIncOvercap;

                    Unit.frmMain.Attributes[idx].Value = arAttributeStates[j].iItemsValue;
                    Unit.frmMain.Attributes[idx].CapInc = arAttributeStates[j].iItemsCap;
                    Unit.frmMain.Attributes[idx].Overcap = arAttributeStates[j].iItemsOvercap;

                    if (!bPreview)
                    {
                        Unit.frmMain.Attributes[idx].ValueChange = 0;
                        Unit.frmMain.Attributes[idx].CapIncChange = 0;
                        Unit.frmMain.Attributes[idx].OvercapChange = 0;
                    }
                    else
                    {
                        Unit.frmMain.Attributes[idx].ValueChange = arAttributeStates[j].iPreviewValue - arAttributeStates[j].iItemsValue;
                        Unit.frmMain.Attributes[idx].CapIncChange = arAttributeStates[j].iPreviewCap - arAttributeStates[j].iItemsCap;
                        Unit.frmMain.Attributes[idx].OvercapChange = arAttributeStates[j].iPreviewOvercap - arAttributeStates[j].iItemsOvercap;
                    }

                }
                else
                    Unit.frmMain.Attributes[idx].Visible = false; // Eintrag unsichtbar
            }

            // Anzeige des Scrollbalkens aktualisieren
            if (arOthers.Length > 9)
            {
                Unit.frmMain.sbOthers.Visible = true;
                Unit.frmMain.sbOthers.Maximum = arOthers.Length - 9;
            }
            else
            {
                Unit.frmMain.sbOthers.Visible = false;
                Unit.frmMain.sbOthers.Maximum = 0;
            }
            Unit.frmMain.sbOthers.Value = iOtherPos;
        }
        /*
        //---------------------------------------------------------------------------
        void CompareWith(CplayerAttributes SecAttributes)
        {

        }
        //---------------------------------------------------------------------------
        void DontCompare()
        {

        }
        //---------------------------------------------------------------------------
        */
    }
}
