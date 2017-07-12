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

namespace Moras.Net
{
    //---------------------------------------------------------------------------
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using DelphiClasses;
    using System.Windows.Forms;
    using System.Diagnostics;
    using dxgettext;

    public struct SItemEffect
    {
        public int BonusId;	// Die ID des Bonus-effektes
        public int Value;	// Wie hoch ist der Wert
        public int GemQuality;	//
        public int Level;	// Ab welchem Itemlevel wirkt der Effekt
        public int Remakes;	// Wie oft neu gemacht (Nur bei Crafted)
        public int Time;	// Herstellungszeit (in sekunden)
        public bool bDone;	// Fertig gecrafted?
    };

    public enum EItemType { Drop, Unique, Crafted };

    public class CItem : ICloneable, IEquatable<CItem>
    {
        private static string _(string szMsgId) { return TGnuGettextInstance.gettext(szMsgId); }
        private String strName;    // Name des Items
        private String strNameOriginal;    // Original Name im Englischen
        private String strOrigin;	// Fundort/Herkunft
        private String strDescription;	// Zusätzliche Infos
        private String strOnlineURL; // URL zum Item im Internet
        private String strExtension; // zu welcher Erweiterung gehört das Item
        private String strProvider; // von welcher Online DB kommt das Item
        private int iRealm; // in welchem Reich kann das Item benutzt werden
        private int iPosition;	// In welchem Slot wird der Gegenstand getragen
        private int iSlot;	// Wo ist der Gegenstand aktuell
        private EItemType eType;	// Item-Art (Drop, Crafted, Unique)
        private int iLevel;	// Der Ideale Level eines Benutzers
        private int iQuality;   // Qualität des Items
        private int iBonus; // Bonuswert des Items
        private int iClass; // Klasse wie zb Stoff oder Leder
        private int iSubClass;  // Unterklassen wie zb verstärkt
        private int iMaterial;	// Dieser Wert interessiert nur bei gecrafteten Sachen
        private int iAF;	// Der AF-Wert bei Rüstungen
        private int iDPS;	// DPS im Fixkomma-Format xx.x
        private int iSpeed;	// Speed im Fixkomma.Format xx.x
        private int iDamageType;	// Schadensart der Waffe
        private int iCurLevel;	// Der aktuelle Level eines Levelbaren Gegenstandes
        private int iMaxLevel;	// Der maximale Itemlevel
        private int iUseLevel;	// Level, damit der Proc/Ladung funktioniert
        private DateTime dtLastUpdate;	// Zeitpunkt des letzten Item-Updates
        private DynamicArray<int> idClassRestriction;	// Array mit den Klassenbeschränkungen
        private DynamicArray<SItemEffect> arEffects;  // Array mit den Effekten die das Item hat
        private bool bChanged;  // Item wurde geändert
        private bool bDeleted;	// Wenn gesetzt, gilt das Item als nicht vorhanden

        public CItem() { }

        //---------------------------------------------------------------------------
        // Der Copy-Konstruktor
        public CItem(CItem rhs)
        {
            strName = rhs.strName;
            strNameOriginal = rhs.strNameOriginal;
            if (rhs.strOrigin != "")
                strOrigin = rhs.strOrigin;
            if (rhs.strDescription != "")
                strDescription = rhs.strDescription;
            if (rhs.strOnlineURL != "")
                strOnlineURL = rhs.strOnlineURL;
            if (rhs.strExtension != "")
                strExtension = rhs.strExtension;
            if (rhs.strProvider != "")
                strProvider = rhs.strProvider;
            // UID nur kopieren, wenn Source eine hat
            if (rhs.iUID > 0)
                iUID = rhs.iUID;
            bEquipped = rhs.bEquipped;
            iRealm = rhs.iRealm;
            iPosition = rhs.iPosition;
            //iSlot = rhs.iSlot;
            eType = rhs.eType;
            if (rhs.iLevel > 0)
                iLevel = rhs.iLevel;
            iCurLevel = rhs.iCurLevel;
            iMaxLevel = rhs.iMaxLevel;
            iQuality = rhs.iQuality;
            if (rhs.iBonus > 0)
                iBonus = rhs.iBonus;
            iClass = rhs.iClass;
            iSubClass = rhs.iSubClass;
            iMaterial = rhs.iMaterial;
            if (rhs.iAF > 0)
                iAF = rhs.iAF;
            if (rhs.iDPS > 0)
                iDPS = rhs.iDPS;
            if (rhs.iSpeed > 0)
                iSpeed = rhs.iSpeed;
            iDamageType = rhs.iDamageType;
            dtLastUpdate = rhs.dtLastUpdate;
            idClassRestriction = rhs.idClassRestriction.Copy();
            EffectCheck(rhs.arEffects.Length - 1);
            arEffects = rhs.arEffects.Copy();
            bChanged = rhs.bChanged;
            bDeleted = rhs.bDeleted;
            // Die temporären Werte auch kopieren
            sNutzen = rhs.sNutzen;
            sGesamt = rhs.sGesamt;
        }

        #region ICloneable Members

        public object Clone()
        {
            return new CItem(this);
        }

        #endregion

        // Der ==-Operator
        public static bool operator ==(CItem lhs, CItem rhs)
        {
            return Equals(lhs, rhs);
        }

        // Der !=-Operator
        public static bool operator !=(CItem lhs, CItem rhs)
        {
            return !Equals(lhs, rhs);
        }

        public static bool Equals(CItem lhs, CItem rhs)
        {
            if ((object)lhs == (object)rhs)
                return true;
            if ((object)lhs == null || (object)rhs == null)
                return false;

            if ((lhs.strName.Equals(rhs.strName, StringComparison.CurrentCultureIgnoreCase))
             && (lhs.iRealm == rhs.iRealm)
             && (Unit.xml_config.arItemSlots[lhs.iPosition].strPosClass.Equals(Unit.xml_config.arItemSlots[rhs.iPosition].strPosClass, StringComparison.CurrentCultureIgnoreCase))
             && (lhs.strProvider.Equals(rhs.strProvider, StringComparison.CurrentCultureIgnoreCase))
             && (lhs.iClass == rhs.iClass))
            {
                // Wenn wir bis hierher gekommen sind, dann sind sie identisch
                return true;
            }
            return false;
        }

        #region IEquatable<CItem> Members

        public bool Equals(CItem rhs)
        {
            return Equals(this, rhs);
        }

        #endregion

        public override bool Equals(object obj)
        {
            return Equals(this, obj as CItem);
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + strName.ToLower().GetHashCode();
            hash = (hash * 7) + iRealm.GetHashCode();
            hash = (hash * 7) + Unit.xml_config.arItemSlots[iPosition].strPosClass.ToLower().GetHashCode();
            hash = (hash * 7) + strProvider.ToLower().GetHashCode();
            hash = (hash * 7) + iClass.GetHashCode();
            return hash;
        }

        // Der "sichere" Copy-Operator
        // Sicher heißt hier, es werden keine Werte übernomen, die im Target einen
        // Wert haben und in der source nicht
        // Außerdem werden nur die Werte kopiert, welche für die Datenbank interessieren,
        // also nicht ob Item equipped oder der aktuelle Artefakt-Level
        public void SafeCopy(CItem Item)
        {	// Sichere Kopie
            if (Item.strName != "")
                Name = Item.strName;
            if (Item.strNameOriginal != "")
                NameOriginal = Item.strNameOriginal;
            if (Item.strOrigin != "")
                Origin = Item.strOrigin;
            if (Item.strDescription != "")
                Description = Item.strDescription;
            if (Item.strOnlineURL != "")
                OnlineURL = Item.strOnlineURL;
            if (Item.strExtension != "")
                Extension = Item.strExtension;
            if (Item.strProvider != "")
                Provider = Item.strProvider;
            if (Item.iUID > 0)
                iUID = Item.iUID;
            Realm = Item.iRealm;
            bDeleted = Item.bDeleted;
            if (Item.Position < 0)
                Item.AskForPosition();
            Position = Item.iPosition;
            if (Item.iLevel > 0)
                Level = Item.iLevel;
            MaxLevel = Item.iMaxLevel;
            if (Item.iQuality > 0)
                Quality = Item.iQuality;
            if (Item.iBonus > 0)
                Bonus = Item.iBonus;
            Class = Item.iClass;
            SubClass = Item.iSubClass;
            Material = Item.iMaterial;
            if (Item.iAF > 0)
                AF = Item.iAF;
            if (Item.iDPS > 0)
                DPS = Item.iDPS;
            if (Item.iSpeed > 0)
                Speed = Item.iSpeed;
            DamageType = Item.iDamageType;
            // Das neuere Datum behalten
            if (LastUpdate < Item.LastUpdate)
                LastUpdate = Item.dtLastUpdate;
            if (idClassRestriction.Length != Item.idClassRestriction.Length)
                idClassRestriction.Length = Item.idClassRestriction.Length;
            for (int i = 0; i < idClassRestriction.Length; i++)
            {
                if ((Item.idClassRestriction.Length > i) && (Item.idClassRestriction[i] >= 0))
                    idClassRestriction[i] = Item.idClassRestriction[i];
                else
                    idClassRestriction[i] = -1;
            }
            // Effekte auf jeden Fall kopieren
            EffectCheck(Item.arEffects.Length - 1);
            if (arEffects.Length != Item.arEffects.Length)
                arEffects.Length = Item.arEffects.Length;
            for (int i = 0; i < Item.arEffects.Length; i++)
            {
                arEffects.Array[i].BonusId = Item.arEffects[i].BonusId;
                arEffects.Array[i].Value = Item.arEffects[i].Value;
                arEffects.Array[i].Level = Item.arEffects[i].Level;
            }
        }

        public int iUID;	// Einzigartige ID des Items. Wird nur vom Masterprogramm vergeben
        public bool bEquipped;	// Ist Angelegt?
        public Single sNutzen;	// Temporärer Wert (Wird nicht gespeichert)
        public Single sGesamt;	// Gesamtnutzen

        // Setze die Standardwerte eines Items
        public void Init()
        {
            // 5 Effektslots fertig machen
            arEffects.Length = 5;
            // initialisiere die neuen Einträge
            for (int i = 0; i < arEffects.Length; i++)
            {
                arEffects.Array[i].BonusId = -1;	// -1 = unbenutzt
                arEffects.Array[i].Value = 0;
                arEffects.Array[i].GemQuality = 3; // 99%
                arEffects.Array[i].Level = 0;
                arEffects.Array[i].Remakes = 0;
                arEffects.Array[i].Time = 0;
                arEffects.Array[i].bDone = false;
            }
            strName = "";
            strNameOriginal = "";
            strOrigin = "";	// Fundort/Herkunft
            strDescription = "";
            strOnlineURL = "";
            strExtension = "";
            strProvider = "";
            iUID = 0;
            bEquipped = true;
            iRealm = 7;	// Default sind alle Realm
            iPosition = -1;
            iSlot = -1;
            eType = EItemType.Drop;	// Item-Art (Drop, Crafted, Unique)
            iLevel = 0;	// Der Ideale Level eines Benutzers
            iCurLevel = 0;	// Der aktuelle Level eines Levelbaren Gegenstandes
            iMaxLevel = 0;	// Der maximale Itemlevel
            iQuality = 99;
            iBonus = 0;
            iClass = -1;
            iSubClass = -1;
            iMaterial = -1;
            sNutzen = 0.0f;
            sGesamt = 0.0f;
            iAF = 0;	// Der AF-Wert bei Rüstungen
            iDPS = 0;
            iSpeed = 0;
            iDamageType = 0;	// Schadensart der Waffe
            dtLastUpdate = DateTime.Now;
            idClassRestriction.Length = 4;
            for (int i = 0; i < idClassRestriction.Length; i++)
                idClassRestriction[i] = -1;
            bDeleted = false;
            bChanged = false;
        }

        ~CItem()
        {
            arEffects.Length = 0;
            idClassRestriction.Length = 0;
        }

        // Lösche die Effecte des Items
        public void ClearEffects()
        {
            // 5 Effektslots fertig machen
            arEffects.Length = 5;
            // initialisiere die Einträge
            for (int i = 0; i < arEffects.Length; i++)
            {
                arEffects.Array[i].BonusId = -1;	// -1 = unbenutzt
                arEffects.Array[i].Value = 0;
                arEffects.Array[i].GemQuality = 3; // 99%
                arEffects.Array[i].Level = 0;
                arEffects.Array[i].Remakes = 0;
                arEffects.Array[i].Time = 0;
                arEffects.Array[i].bDone = false;
            }
            bChanged = true;
        }

        // Liefere die Wert des angegebenen Boni
        // 0 wenn der Effekt nicht vorhanden ist
        public int GetBonusEffect(int BonusId)
        {
            for (int i = 0; i < arEffects.Length; i++)
            {
                if (arEffects[i].BonusId == BonusId)
                    return arEffects[i].Value;
            }
            return 0;
        }

        // Speichere das Item in die übergebene xml-Datei
        // Equipment sagt, das der Gegenstand als Teil eines Templates gespeichert wird,
        // und nicht als Datenbankeintrag
        public bool Save(CXml xFile, bool Equipment = false)
        {
            xFile.OpenTag("item", "\n", "");
            // Wenn das Deleted Flag gesetzt ist, wird danach nichts weiter gespeichert
            if (bDeleted)
                xFile.EmptyTag("deleted");
            else
            {	// Nicht als gelöscht markiert
                if ((Name != "") && !((NameOriginal != "") && (Name == NameOriginal)))
                {
                    xFile.OpenTag("name", Utils.Str2XmlStr(Name), ""); xFile.CloseTag();
                }
                if (NameOriginal != "")
                {
                    xFile.OpenTag("name_org", Utils.Str2XmlStr(NameOriginal), ""); xFile.CloseTag();
                }
                if (Origin != "")
                {
                    xFile.OpenTag("origin", Utils.Str2XmlStr(Origin), ""); xFile.CloseTag();
                }
                if (Description != "")
                {
                    xFile.OpenTag("description", Utils.Str2XmlStr(Description), ""); xFile.CloseTag();
                }
                if (OnlineURL != "")
                {
                    xFile.OpenTag("online_url", Utils.Str2XmlStr(OnlineURL), ""); xFile.CloseTag();
                }
                if (Extension != "")
                {
                    xFile.OpenTag("extension", Utils.Str2XmlStr(Extension), ""); xFile.CloseTag();
                }
                if (Provider != "")
                {
                    xFile.OpenTag("provider", Utils.Str2XmlStr(Provider), ""); xFile.CloseTag();
                }
                // Hier noch die Position speichern
                if (iPosition >= 0)
                {
                    xFile.OpenTag("position", Unit.xml_config.arItemSlots[iPosition].strPosClass, "");
                    xFile.CloseTag();
                    if (Equipment && (Unit.xml_config.arItemSlots[iPosition].strPosClass == "WEAPONS") & bEquipped)
                        xFile.EmptyTag("equipped");
                }
                if (!isEmpty())
                {
                    xFile.OpenTag("lastupdate", dtLastUpdate.ToString(), ""); xFile.CloseTag();
                }
                if (eType != EItemType.Drop)
                {
                    switch (Type)
                    {
                        case EItemType.Unique: xFile.EmptyTag("unique"); break;
                        case EItemType.Crafted: xFile.EmptyTag("crafted"); break;
                    }
                }
                if ((iRealm > 0) && (iRealm != 7))
                {
                    xFile.OpenTag("realm", Utils.Realm2Str(iRealm), ""); xFile.CloseTag();
                }
                if (iLevel > 0)
                {
                    xFile.OpenTag("level", (Level).ToString(), ""); xFile.CloseTag();
                }
                if ((Quality > 0) && !(isEmpty() & (Quality == 99)))
                {
                    xFile.OpenTag("quality", (Quality).ToString(), ""); xFile.CloseTag();
                }
                if (Bonus > 0)
                {
                    xFile.OpenTag("bonus", (Bonus).ToString(), ""); xFile.CloseTag();
                }
                if (iMaxLevel > 0)	// Wenn größer als 0, dann ist es ein Artefakt
                {
                    xFile.OpenTag("artifact_levels", (iMaxLevel).ToString(), ""); xFile.CloseTag();
                    // Wenn Equipment, dann auch den Aktuellen Level speichern
                    if (Equipment & (iCurLevel > 0))
                    {
                        xFile.OpenTag("artifact_level", (iCurLevel).ToString(), ""); xFile.CloseTag();
                    }
                }
                // Wenn es eine Rüstung ist, dann alles dazu in einem Armor-Tag
                if ((iPosition >= 0) && (Unit.xml_config.arItemSlots[Position].type == ESlotType.Armor) && (iClass >= 0))
                {
                    String att = "";
                    if (iAF > 0) att = "af=\"" + (iAF).ToString() + '\"';
                    // Wenn Ausrüstung und crafted
                    if (Equipment & (eType == EItemType.Crafted))
                    {
                        if (iSubClass > 0) att += " subclass=\"" + Unit.xml_config.arItemClasses[iClass].arSubClasses[iSubClass - 1].id + '\"';
                        if (iMaterial >= 0) att += " material=\"" + (iMaterial).ToString() + '\"';
                    }
                    xFile.OpenTag("armor", Unit.xml_config.arItemClasses[Class].id, att);
                    xFile.CloseTag();
                }
                // Wenn es eine Waffe ist, dann alles zugehörige hier rein
                if ((iPosition >= 0) && (Unit.xml_config.arItemSlots[Position].type == ESlotType.Weapon) && (iClass >= 0))
                {
                    String att = "";
                    if (Equipment & (eType == EItemType.Crafted))
                    {
                        if (iSubClass > 0) att += " subclass=\"" + Unit.xml_config.arItemClasses[iClass].arSubClasses[iSubClass - 1].id + '\"';
                        if (iMaterial >= 0) att += " material=\"" + (iMaterial).ToString() + '\"';
                    }
                    if (Unit.xml_config.arItemClasses[Class].bStats)
                    {	// attribute nur wenn die Itemklasse sie hat
                        if (iDamageType > 0)
                            att += " damage=\"" + Unit.xml_config.arDamageTypes[iDamageType - 1].id + '\"';
                        if (iDPS > 0)
                            att += " dps=\"" + (iDPS * 0.1).ToString("000.0") + '\"';
                        if (iSpeed > 0)
                            att += " speed=\"" + (Speed * 0.1).ToString("000.0") + '\"';
                    }
                    xFile.OpenTag("weapon", Unit.xml_config.arItemClasses[Class].id, att.Substring(2, 100));
                    xFile.CloseTag();
                }
                // Eventuelle Klassenbeschränkungen
                for (int i = 0; i < idClassRestriction.Length; i++)
                {
                    if (idClassRestriction[i] >= 0)
                    {
                        xFile.OpenTag("class_restriction", Unit.xml_config.arClasses[idClassRestriction[i]].id, "");
                        xFile.CloseTag();
                    }
                }
                // Nun die Effekte
                for (int i = 0; i < nEffects; i++)
                {
                    if (arEffects[i].BonusId >= 0)
                    {
                        String att = "id=\"" + Unit.xml_config.arBonuses[arEffects[i].BonusId].id + '\"';
                        if (eType == EItemType.Crafted)
                        {	// Wenn Item Crafted, dann noch die Juwelenqualität speichern
                            att += " quality=\"" + (arEffects[i].GemQuality + 96).ToString() + '\"';
                            // Und die Remakes wenn mehr als 0
                            if (arEffects[i].Remakes > 0)
                                att += " remakes=\"" + (arEffects[i].Remakes).ToString() + '\"';
                            if (arEffects[i].Time > 0)
                                att += " time=\"" + (arEffects[i].Time).ToString() + '\"';
                            // Und done, wenn fertig
                            if (arEffects[i].bDone)
                                att += " done=\"true\"";
                            // 5. Slot crafted => Position fest machen
                            if (i == 4) { att += " effpos=\"4\""; }
                        }
                        if ((iMaxLevel > 0) & (arEffects[i].Level > 0))
                            att += " level=\"" + (arEffects[i].Level).ToString() + '\"';
                        xFile.OpenTag("effect", (arEffects[i].Value).ToString(), att);
                        xFile.CloseTag();
                    }
                }
            }
            xFile.CloseTag();	// Schliesse den "item"-Tag
            bChanged = false;	// Haben ja grad gespeichert
            return true;
        }

        // Lade ein Item aus der übergebenen xml-Datei
        public bool Load(CXml xFile)
        {
            int eff = 0, cres = 0, efftemp = 0; //, effdest = 0;
            try
            {
                Init();
                // Schleife bis Datei zu Ende (Oder neues Item anfängt)
                while (xFile.NextTag())
                {
                    // Neuer Item fängt an
                    // Funktion beenden
                    if (xFile.isTag("item"))
                    {
                        bChanged = false;	// Ist ja aus Datei geladen
                        return true;
                    }
                    else if (xFile.isTag("message"))
                    {
                        MessageBoxIcon flags;

                        if (xFile.AttributeValue["type"] == "warning")
                            flags = MessageBoxIcon.Warning;
                        else if (xFile.AttributeValue["type"] == "error")
                            flags = MessageBoxIcon.Error;
                        else
                            flags = MessageBoxIcon.Information;

                        Unit.frmImport.HasMessage = true;
                        MessageBox.Show(xFile.Content, "Update Nachricht", MessageBoxButtons.OK, flags);
                    }
                    //			else if (xFile.isTag("uid"))
                    //				iUID = xFile.Content.ToIntDef(0);
                    else if (xFile.isTag("deleted"))
                        bDeleted = true;
                    else if (xFile.isTag("name"))
                        strName = xFile.Content;
                    else if (xFile.isTag("name_org"))
                        strNameOriginal = xFile.Content;
                    else if (xFile.isTag("origin"))
                        strOrigin = xFile.Content;
                    else if (xFile.isTag("description"))
                        strDescription = xFile.Content;
                    else if (xFile.isTag("online_url"))
                        strOnlineURL = xFile.Content;
                    else if (xFile.isTag("extension"))
                        strExtension = xFile.Content.ToLower();
                    else if (xFile.isTag("provider"))
                        strProvider = xFile.Content;
                    else if (xFile.isTag("position"))
                    {
                        iPosition = Unit.xml_config.GetSlotPosition(xFile.Content);
                        if (Unit.xml_config.arItemSlots[Position].type == ESlotType.Weapon)
                            bEquipped = false;
                    }
                    else if (xFile.isTag("equipped"))
                        bEquipped = true;
                    else if (xFile.isTag("lastupdate"))
                    {
                        DateTime tdt;
                        DateTime.TryParse(xFile.Content, out tdt);
                        LastUpdate = tdt;
                    }
                    else if (xFile.isTag("unique"))
                        Type = EItemType.Unique;
                    else if (xFile.isTag("crafted"))
                        Type = EItemType.Crafted;
                    else if (xFile.isTag("realm"))
                        iRealm = Utils.Realm2Int(xFile.Content);
                    else if (xFile.isTag("level"))
                        iLevel = xFile.Content.ToIntDef(0);
                    else if (xFile.isTag("quality"))
                        iQuality = xFile.Content.ToIntDef(0);
                    else if (xFile.isTag("bonus"))
                        iBonus = xFile.Content.ToIntDef(0);
                    else if (xFile.isTag("artifact_levels"))
                        iMaxLevel = iCurLevel = xFile.Content.ToIntDef(0);
                    else if (xFile.isTag("artifact_level"))
                        iCurLevel = xFile.Content.ToIntDef(0);
                    // Alles zur Rüstung
                    else if (xFile.isTag("armor"))
                    {
                        iClass = Unit.xml_config.GetItemClassId(xFile.Content);
                        // AF ist im Moment das einzige Attribut
                        iAF = xFile.AttributeValue["af"].ToIntDef(0);
                        iSubClass = Unit.xml_config.GetItemSubClassId(iClass, xFile.AttributeValue["subclass"]);
                        if (iSubClass >= 0) iSubClass++;
                        iMaterial = xFile.AttributeValue["material"].ToIntDef(-1);
                    }
                    // Alles zu "Waffen"
                    else if (xFile.isTag("weapon"))
                    {
                        iClass = Unit.xml_config.GetItemClassId(xFile.Content);
                        iSubClass = Unit.xml_config.GetItemSubClassId(iClass, xFile.AttributeValue["subclass"]);
                        if (iSubClass >= 0) iSubClass++;
                        iMaterial = xFile.AttributeValue["material"].ToIntDef(-1);
                        iDPS = (int)(Utils.Str2Double(xFile.AttributeValue["dps"]) * 10);
                        iSpeed = (int)(Utils.Str2Double(xFile.AttributeValue["speed"]) * 10);
                        iDamageType = Unit.xml_config.GetDamageType(xFile.AttributeValue["damage"]) + 1;
                    }
                    else if (xFile.isTag("effect"))
                    {
                        // Falls ein Index-Override vorliegt, nehme diesen Index, andernfall den der Laufvariable "eff"
                        efftemp = xFile.AttributeValue["effpos"].ToIntDef(eff);
                        if (efftemp != eff)
                        {
                            // Override des Index => Slot überprüfen
                            if (efftemp < arEffects.Length)
                            {
                                // Dieser Slot ist bereits angelegt. Teste ob er vielleicht nur "Unbenutzt" ist.
                                if (arEffects[efftemp].BonusId >= 0)
                                {
                                    // Slot belegt! Sollte nur auftreten, wenn "effpos" inkonsquent auftrat.
                                    // Verschiebe den "alten" Effekt in den ersten freien Slot.
                                    arEffects[FindFirstFreeEffect()] = arEffects[efftemp];
                                }
                            }
                        }
                        else
                        {
                            // kein Override => Mit Laufvariable "eff" weitergehen für nächsten Effekt
                            eff++;
                        }
                        // Trage den Effekt ein (in Slot "efftemp", s.o.)
                        EffectCheck(efftemp);
                        arEffects.Array[efftemp].BonusId = Unit.xml_config.GetBonusId(xFile.AttributeValue["id"], iRealm);
                        arEffects.Array[efftemp].Value = xFile.Content.ToIntDef(0);
                        arEffects.Array[efftemp].GemQuality = xFile.AttributeValue["quality"].ToIntDef(96).Clamp(96, 100) - 96;
                        arEffects.Array[efftemp].Level = xFile.AttributeValue["level"].ToIntDef(0);
                        arEffects.Array[efftemp].Remakes = xFile.AttributeValue["remakes"].ToIntDef(0);
                        arEffects.Array[efftemp].Time = xFile.AttributeValue["time"].ToIntDef(0);
                        if (xFile.AttributeValue["done"].Length > 0)
                            arEffects.Array[efftemp].bDone = true;
                    }
                    else if (xFile.isTag("class_restriction"))
                    {	// Die Klassenbeschränkungen
                        ClassRestriction[cres] = Unit.xml_config.GetClassId(xFile.Content);
                        cres++;
                    }
                }
            }
            catch (Exception e)
            {
                Utils.DebugPrint("CItem::Load = %s", e.Message);
                return true;
            }
            bChanged = false;	// Ist ja aus Datei geladen
            return false;
        }

        // Lade ein Kort-Item
        public bool LoadKort(CXml xFile)
        {
            int eff = 0;
            bool bDI = false;
            try
            {
                Init();
                // Schleife bis Datei zu Ende (Oder neues Item anfängt)
                while (xFile.NextTag())
                {
                    if (xFile.isTag("Realm"))
                        iRealm = Utils.Realm2Int(xFile.Content);
                    else if (xFile.isTag("ItemName"))
                    {
                        if (Unit.xml_config.bKortEnglish)
                            strNameOriginal = xFile.Content;
                        else
                            strName = xFile.Content;
                    }
                    else if (xFile.isTag("Level"))
                    {
                        iLevel = xFile.Content.ToIntDef(0);
                        if (iLevel == 51) iLevel = 0;	// 51 ist bei Kort default
                    }
                    else if (xFile.isTag("Bonus"))
                        iBonus = xFile.Content.ToIntDef(0);
                    else if (xFile.isTag("AFDPS"))
                    {
                        iAF = xFile.Content.ToIntDef(0);
                        iDPS = (int)(Utils.Str2Double(xFile.Content) * 10);
                    }
                    else if (xFile.isTag("Speed"))
                        iSpeed = (int)(Utils.Str2Double(xFile.Content) * 10);
                    else if (xFile.isTag("ItemQuality"))
                        iQuality = xFile.Content.ToIntDef(0);
                    else if (xFile.isTag("DROPITEM"))
                        bDI = true;	// Wir sind nun im Dropitem-tag
                    else if (xFile.isTag("ActiveState"))
                    {
                        if (xFile.Content != "drop")
                            return false;	// Nur dropitems importieren
                    }
                    else if (xFile.isTag("Location"))
                    {	// Kort-spezifisch
                        iPosition = Unit.xml_config.GetSlotPosition(xFile.Content);
                    }
                    else if (bDI)
                    {	// Alle Daten zu den Slots
                        // Es interessieren aber nur effect und amount (wegen Drop)
                        if (xFile.isTag("SLOT"))
                        {
                            eff = xFile.AttributeValue["Number"].ToIntDef(0);
                            EffectCheck(eff);
                        }
                        else if (xFile.isTag("Amount"))
                            arEffects.Array[eff].Value = xFile.Content.ToIntDef(0);
                        else if (xFile.isTag("Effect"))
                        {	// Das wird nochmal kompliziert
                            int bid = -1;
                            if (xFile.Content.Length > 0)
                            {
                                bid = Unit.xml_config.GetBonusId(xFile.Content);
                                if (bid < 0)
                                {	// Nicht gefundener Bonus
                                    MessageBox.Show("Unbekannter Effect '" + xFile.Content
                                     + "' in Datei '" + xFile.FileName + "' gefunden!", "Fehler");
                                }
                            }
                            arEffects.Array[eff].BonusId = bid;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Utils.DebugPrint("CItem::LoadKort = %s", e.Message);
                return false;
            }
            // Kleine Nachbehandlung, wenn es ein Rüstungsteil ist
            if (iPosition < 6)
                iClass = Unit.xml_config.GetItemClassId("CLOTH");	// Alle Rüstungen auf Stoff setzen
            bChanged = false;	// Ist ja aus Datei geladen
            return true;
        }

        // Liefert wahr, wenn der Item keine Daten enthält
        // das ist im Moment immer dann, wenn kein Name oder keine Effekte da sind
        public bool isEmpty()
        {
            if (Name.Length > 0) return false;
            for (int i = 0; i < arEffects.Length; i++)
            {
                if (arEffects[i].BonusId >= 0) return false;
            }
            return true;
        }

        // Liefert wahr, wenn die Boni wirken für angegebene Klasse
        public bool isUseable(int idClass)
        {
            if (idClassRestriction[0] >= 0)
            {	// Es gibt eine Klassenbeschränkung
                for (int i = 0; i < idClassRestriction.Length; i++)
                {
                    if (idClassRestriction[i] == idClass)
                        return true;
                }
                return false;
            }
            else	// Es gibt keine Beschränkung, also wahr
                return true;
        }

        // Berechne den Nutzen dieses Gegenstandes
        // Wenn bUseEquipped, dann Nutzen nur berechnen, wenn bEquipped = true
        public Single CalcNutzen(bool bUseEquipped = true)
        {
            Single sReturn = 0;
            if (!bUseEquipped || bEquipped)
            {
                // Gibt es eine Klassenbeschränkung?
                if (isUseable(Unit.player.Class))
                {
                    for (int i = 0; i < arEffects.Length; i++)
                    {
                        int bid = arEffects[i].BonusId;
                        if (bid >= 0)
                        {	// Wenn es ein Dropitem ist, dann Effektlevel beachten
                            if ((eType != EItemType.Drop) ||
                                (arEffects[i].Level <= iCurLevel))
                            {
                                for (int j = 0; j < Unit.xml_config.arBonuses[bid].arAttributes.Length; j++)
                                {
                                    int aid = Unit.xml_config.arBonuses[bid].arAttributes[j];
                                    sReturn += arEffects[i].Value * Unit.player.UpV[aid];
                                }
                            }
                        }
                    }
                }
            }
            return sReturn;
        }

        // Berechne den klassischen Nutzen
        public Single CalcUtility()
        {
            Single sReturn = 0;
            for (int i = 0; i < arEffects.Length; i++)
            {
                int bid = arEffects[i].BonusId;
                if (bid >= 0)
                {	// Wenn es ein Dropitem ist, dann Effektlevel beachten
                    if ((eType != EItemType.Drop) ||
                        (arEffects[i].Level <= iCurLevel))
                    {
                        int aid = Unit.xml_config.arBonuses[bid].arAttributes[0];
                        Single upv = 0;
                        int idFocus = Unit.xml_config.GetGroupId("FOCUS");
                        Debug.Assert(idFocus >= 0);
                        // Suche den Bonus, welcher dieses Attribut als einzigen Wert hat
                        for (int j = 0; j < Unit.xml_config.nBonuses; j++)
                        {
                            if ((Unit.xml_config.arBonuses[j].arAttributes.Length == 1)
                             && (Unit.xml_config.arBonuses[j].arAttributes[0] == aid)
                             && (Unit.xml_config.arBonuses[j].idGroup != idFocus))
                            {
                                upv = (Single)(Unit.xml_config.arBonuses[j].ip_mult) / 100;
                                break;
                            }
                        }
                        sReturn += arEffects[i].Value * upv;
                    }
                }
            }
            return sReturn;
        }

        // Erstelle einen String, der als Tooltiphelp für den Gegenstand
        // angezeigt werden kann
        // Name von Gegenstand bei Drop, oder Unique/Crafted
        // Alle Effekte
        protected String GetToolTipText()
        {
            String strReturn;
            if (isEmpty()) return "";
            if (Type == EItemType.Drop)
            {
                strReturn = "Drop - \"" + Name + "\"";
                if ((strNameOriginal.Length > 0) && (Name != strNameOriginal))
                    strReturn += "/\"" + strNameOriginal + "\"";
            }
            else if (Type == EItemType.Unique)
                strReturn = _("Einzigartig");
            else
                strReturn = _("Spieler-Hergestellt");
            // Effekte durcharbeiten
            for (int i = 0; i < arEffects.Length; i++)
            {
                if (arEffects[i].BonusId >= 0)
                {
                    strReturn += "\n  ";
                    if (arEffects[i].Value >= 0) strReturn += "+";
                    strReturn += (arEffects[i].Value).ToString();
                    if (Unit.xml_config.arBonuses[arEffects[i].BonusId].bPercent)
                        strReturn += '%';
                    strReturn += " " + Unit.xml_config.arBonuses[arEffects[i].BonusId].Names[0];
                }
            }
            return strReturn;
        }

        // Liefere Langen Beschreibungstext des Items
        // Gedacht für die Anzeige im Suchfenster
        protected String GetLongInfo()
        {
            String strReturn;
            strReturn = _("Letzte Änderung: ") + dtLastUpdate.ToShortDateString() + " " + dtLastUpdate.ToShortTimeString();
            strReturn += "\n" + _("Stufe:") + "\t" + (iLevel).ToString();
            if (iMaxLevel > 0)	// Ist ein Artefakt
                strReturn += "\t" + _("(Artefakt)");
            strReturn += "\n" + _("Qualität:") + "\t" + (iQuality).ToString();
            strReturn += "%\tBonus:\t" + (iBonus).ToString() + "%\n";
            if (Unit.xml_config.arItemSlots[Position].type == ESlotType.Armor)
            {
                strReturn += "AF:\t" + (iAF).ToString();
                if (iClass >= 0)
                    strReturn += " (" + Unit.xml_config.arItemClasses[iClass].Name + ")";
                strReturn += "\n";
            }
            if (Unit.xml_config.arItemSlots[Position].type == ESlotType.Weapon)
            {
                strReturn += "DPS:\t" + (iDPS * 0.1).ToString("000.0")
                    + "\tSpeed:\t" + (iSpeed * 0.1).ToString("000.0") + "\n";
            }
            strReturn += "\n" + _("Effekte:") + "\n";
            // Nun alle Effekte auflisten
            for (int i = 0; i < arEffects.Length; i++)
            {
                if (arEffects[i].BonusId >= 0)
                {
                    strReturn += "  ";
                    if (arEffects[i].Value >= 0) strReturn += "+";
                    strReturn += (arEffects[i].Value).ToString();
                    if (Unit.xml_config.arBonuses[arEffects[i].BonusId].bPercent)
                        strReturn += '%';
                    strReturn += " " + Unit.xml_config.arBonuses[arEffects[i].BonusId].Names[0];
                    // Wenn Artefakt und dieser Effekt Level > 0, dann den auch hinschreiben
                    if ((iMaxLevel > 0) && (arEffects[i].Level != 0))
                        strReturn += " (" + (arEffects[i].Level).ToString() + ")";
                    strReturn += "\n";
                }
            }
            // Nun die Klassenbeschränkungen
            strReturn += "\n" + _("Klassen:") + "\n  ";
            if (idClassRestriction[0] == -1)
            {
                if (iRealm == 7)	// Alle Reiche
                    strReturn += _("Alle Reiche");
                else
                    strReturn += _("Alle Klassen (") + Utils.Realm2Str(iRealm) + ")";
            }
            else
            {	// Klassen auflisten
                for (int i = 0; i < idClassRestriction.Length; i++)
                {
                    if (idClassRestriction[i] >= 0)
                    {
                        if (i != 0) strReturn += ", ";
                        strReturn += Unit.xml_config.arClasses[idClassRestriction[i]].Name;
                    }
                }
            }
            if (strProvider != "")
                strReturn += "\n\n" + _("Online-DB:") + " " + strProvider;
            if (strExtension != "")
                strReturn += "\n\n" + _("Erweiterung:") + " " + Unit.xml_config.arExtensions[Unit.xml_config.GetExtensionId(strExtension)].Name;
            if (strOrigin != "")
                strReturn += "\n\n" + _("Fundort/Herkunft:") + "\n" + strOrigin;
            return strReturn;
        }

        // Interne Utility-Function
        // Sucht den ersten freien Slot
        // Falls keiner frei ist => neuen anlegen
        protected int FindFirstFreeEffect()
        {
            int len = arEffects.Length, i;
            for (i = 0; i < len; i++)
                if (arEffects[i].BonusId < 0) return i;

            arEffects.Length++;

            arEffects.Array[len].BonusId = -1;	// -1 = unbenutzt
            arEffects.Array[len].Value = 1;
            arEffects.Array[len].GemQuality = 0;
            arEffects.Array[len].Level = 0;
            return len;
        }

        // Interne Utility-Function
        // Tested, ob ein Effekteintrag schon vorhanden ist, und legt einen neu an wenn nicht
        protected void EffectCheck(int number)
        {
            int len = arEffects.Length;
            if (len <= number)
            {
                arEffects.Length = number + 1;
                // initialisiere die neuen Einträge
                for (int i = len; i <= number; i++)
                {
                    arEffects.Array[i].BonusId = -1;	// -1 = unbenutzt
                    arEffects.Array[i].Value = 1;
                    arEffects.Array[i].GemQuality = 0;
                    arEffects.Array[i].Level = 0;
                }
            }
        }

        // Achtung!!! Liefert immer mindestens 4. arEffects kann also auch leere Effekte haben.
        protected int GetnEffects()
        {
            return arEffects.Length;
        }

        protected int GetEffect(int number)
        {
            if ((number >= 0) && (number < arEffects.Length))
                return arEffects[number].BonusId;
            else
                return -1;
        }

        protected void SetEffect(int number, int EffectId)
        {
            if ((number >= 0) && (number < 10))
            {
                EffectCheck(number);
                arEffects.Array[number].BonusId = EffectId;
                bChanged = true;
            }
        }

        public void SetEffectStr(String AEffects)
        {
            int pos, pos1, pos2;
            int cur = 0;
            String tmp = "";
            String tmp1;
            String tmp2;
            String tmp3;

            ClearEffects();
            while (AEffects != "")
            {
                pos = AEffects.IndexOf(";");
                if (pos == -1)
                    pos = AEffects.Length;
                tmp = AEffects.Substring(0, pos);

                pos1 = tmp.IndexOf(":");
                pos2 = tmp.LastIndexOf(":");
                tmp1 = tmp.Substring(0, pos1);
                tmp2 = tmp.Substring(pos1 + 1, (pos2 - pos1) - 1);
                tmp3 = tmp.Substring(pos2 + 1, tmp.Length - pos2 - 1);

                SetEffect(cur, Unit.xml_config.GetBonusId(tmp1));
                SetEffectValue(cur, int.Parse(tmp2));
                SetEffectLevel(cur, int.Parse(tmp3));

                AEffects = pos + 1 >= AEffects.Length ? string.Empty : AEffects.Substring(pos + 1, AEffects.Length - pos - 1);
                cur++;
            }
        }

        public String GetEffectStr()
        {
            String result = "";

            for (int i = 0; i < arEffects.Length; i++)
            {
                if (arEffects[i].BonusId >= 0)
                {
                    if (result != "")
                        result += ";";
                    result += Unit.xml_config.arBonuses[arEffects[i].BonusId].id + ":" + (arEffects[i].Value).ToString() + ":" + (arEffects[i].Level).ToString();
                }
            }

            return result;
        }

        protected int GetEffectValue(int number)
        {
            if ((number >= 0) && (number < arEffects.Length))
                return arEffects[number].Value;
            else
                return 0;
        }

        protected void SetEffectValue(int number, int Value)
        {
            if ((number >= 0) && (number < 10))
            {
                EffectCheck(number);
                arEffects.Array[number].Value = Value;
                bChanged = true;
            }
        }

        protected int GetEffectQuality(int number)
        {
            if ((number >= 0) && (number < arEffects.Length))
                return arEffects[number].GemQuality;
            else
                return 0;
        }

        protected void SetEffectQuality(int number, int Value)
        {
            if ((number >= 0) && (number < 10))
            {
                EffectCheck(number);
                arEffects.Array[number].GemQuality = Value;
                bChanged = true;
            }
        }

        protected int GetEffectLevel(int number)
        {
            if ((number >= 0) && (number < arEffects.Length))
                return arEffects[number].Level;
            else
                return 0;
        }

        protected void SetEffectLevel(int number, int Value)
        {
            if ((number >= 0) && (number < 10))
            {
                EffectCheck(number);
                arEffects.Array[number].Level = Value;
                bChanged = true;
            }
        }

        protected int GetEffectRemakes(int number)
        {
            if ((number >= 0) && (number < 5))
                return arEffects[number].Remakes;
            else
                return 0;
        }

        protected void SetEffectRemakes(int number, int Value)
        {
            if ((number >= 0) && (number < 5))
            {
                arEffects.Array[number].Remakes = Value;
                bChanged = true;
            }
        }

        protected int GetEffectTime(int number)
        {
            if ((number >= 0) && (number < 5))
                return arEffects[number].Time;
            else
                return 0;
        }

        protected void SetEffectTime(int number, int Value)
        {
            if ((number >= 0) && (number < 5))
            {
                arEffects.Array[number].Time = Value;
                bChanged = true;
            }
        }

        protected bool GetEffectDone(int number)
        {
            if ((number >= 0) && (number < 5))
                return arEffects[number].bDone;
            else
                return false;
        }

        protected void SetEffectDone(int number, bool Value)
        {
            if ((number >= 0) && (number < 5))
            {
                arEffects.Array[number].bDone = Value;
                bChanged = true;
            }
        }

        // Wenn der Name selbst leer ist, dann gib NameOriginal zurück
        protected String GetName()
        {
            if (strName.Length > 0)
                return strName;
            else
                return strNameOriginal;
        }

        protected void SetName(String Name)
        {
            strName = Name;
            bChanged = true;
        }

        protected void SetNameOriginal(String Name)
        {
            strNameOriginal = Name;
            bChanged = true;
        }

        protected void SetOrigin(String sOrigin)
        {
            strOrigin = sOrigin;
            bChanged = true;
        }

        protected void SetDescription(String sDescription)
        {
            strDescription = sDescription;
            bChanged = true;
        }

        protected void SetOnlineURL(String URL)
        {
            if (URL != "")
                strOnlineURL = URL;
            bChanged = true;
        }

        protected void SetExtension(String Extension)
        {
            if (Extension != "")
                strExtension = Extension;
            bChanged = true;
        }

        protected void SetProvider(String Provider)
        {
            if (Provider != "")
                strProvider = Provider;
            bChanged = true;
        }

        protected void SetClass(int Value)
        {
            iClass = Value;
            bChanged = true;
        }

        protected void SetSubClass(int Value)
        {
            iSubClass = Value;
            bChanged = true;
        }

        protected void SetMaterial(int Value)
        {
            iMaterial = Value;
            bChanged = true;
        }

        protected void SetAF(int Value)
        {
            iAF = Value;
            bChanged = true;
        }

        protected void SetDamageType(int Value)
        {
            iDamageType = Value;
            bChanged = true;
        }

        protected void SetDPS(int Value)
        {
            iDPS = Value;
            bChanged = true;
        }

        protected void SetSpeed(int Value)
        {
            iSpeed = Value;
            bChanged = true;
        }

        protected void SetBonus(int Value)
        {
            iBonus = Value;
            bChanged = true;
        }

        protected void SetRealm(int Value)
        {
            iRealm = Value;
            bChanged = true;
        }

        protected void SetPosition(int Value)
        {
            iPosition = Value;
            bChanged = true;
        }

        protected void SetSlot(int Value)
        {
            iSlot = Value;
            // Eine änderung hier ändert den Gegenstand nicht
            //	bChanged = true;
        }

        protected void SetLevel(int Value)
        {
            iLevel = Value;
            bChanged = true;
        }

        protected void SetMaxLevel(int Value)
        {
            iMaxLevel = Value;
            bChanged = true;
        }

        protected void SetCurLevel(int Value)
        {
            iCurLevel = Value;
            bChanged = true;
        }

        protected void SetQuality(int Value)
        {
            iQuality = Value;
            bChanged = true;
        }

        protected void SetType(EItemType type)
        {
            eType = type;
            bChanged = true;
        }

        protected void SetLastUpdate(DateTime Time)
        {
            dtLastUpdate = Time;
            bChanged = true;
        }

        protected int GetRestriction(int number)
        {
            if ((number >= 0) && (number < idClassRestriction.Length))
                return idClassRestriction[number];
            else
                return 0;
        }

        protected void SetRestriction(int number, int idClass)
        {
            if (number >= 0)
            {
                if (idClassRestriction.Length <= number)
                    idClassRestriction.Length = number + 1;
                idClassRestriction[number] = idClass;
                if ((idClass >= 0) && (idClassRestriction.Length <= number + 1))
                {	// Ich gehe hier davon aus, das maximal um eins erweitert werden muss
                    Debug.Assert(idClassRestriction.Length == number + 1);
                    idClassRestriction.Length = number + 2;
                    idClassRestriction[number + 1] = -1;
                }
                bChanged = true;
            }
        }

        public void SetClassRestrictionStr(String AClasses)
        {
            int pos;
            int cur = 0;
            String tmp = "";

            ClearEffects();
            while (AClasses != "")
            {
                pos = AClasses.IndexOf(";");
                if (pos == -1)
                    pos = AClasses.Length;
                tmp = AClasses.Substring(0, pos);

                SetRestriction(cur, Unit.xml_config.GetClassId(tmp));

                AClasses = pos + 1 >= AClasses.Length ? string.Empty : AClasses.Substring(pos + 1, AClasses.Length - pos - 1);
                cur++;
            }
        }

        public String GetClassRestrictionStr()
        {
            String result = "";

            for (int i = 0; i < idClassRestriction.Length - 1; i++)
            {
                if (idClassRestriction[i] >= 0)
                {
                    if (result != "")
                        result += ";";
                    result += Unit.xml_config.arClasses[idClassRestriction[i]].id;
                }
            }

            return result;
        }

        protected int GetNRestrictions()
        {
            return idClassRestriction.Length;
        }

        protected void SetDeleted(bool Flag)
        {
            bDeleted = Flag;
            bChanged = true;
        }

        #region Properties
        public String Name { get { return GetName(); } set { SetName(value); } }
        public String NameOriginal { get { return strNameOriginal; } set { SetNameOriginal(value); } }
        public String Origin { get { return strOrigin; } set { SetOrigin(value); } }
        public String Description { get { return strDescription; } set { SetDescription(value); } }
        public String OnlineURL { get { return strOnlineURL; } set { SetOnlineURL(value); } }
        public String Extension { get { return strExtension; } set { SetExtension(value); } }
        public String Provider { get { return strProvider; } set { SetProvider(value); } }
        public EItemType Type { get { return eType; } set { SetType(value); } }
        public int Class { get { return iClass; } set { SetClass(value); } }
        public int SubClass { get { return iSubClass; } set { SetSubClass(value); } }
        public int Material { get { return iMaterial; } set { SetMaterial(value); } }
        public int Bonus { get { return iBonus; } set { SetBonus(value); } }
        public int Quality { get { return iQuality; } set { SetQuality(value); } }
        public int AF { get { return iAF; } set { SetAF(value); } }
        public int DamageType { get { return iDamageType; } set { SetDamageType(value); } }
        public int DPS { get { return iDPS; } set { SetDPS(value); } }
        public int Speed { get { return iSpeed; } set { SetSpeed(value); } }
        public int Realm { get { return iRealm; } set { SetRealm(value); } }
        public int Position { get { return iPosition; } set { SetPosition(value); } }
        public int Slot { get { return iSlot; } set { SetSlot(value); } }
        public int Level { get { return iLevel; } set { SetLevel(value); } }
        public int CurLevel { get { return iCurLevel; } set { SetCurLevel(value); } }
        public int MaxLevel { get { return iMaxLevel; } set { SetMaxLevel(value); } }
        public DateTime LastUpdate { get { return dtLastUpdate; } set { SetLastUpdate(value); } }
        public int nEffects { get { return GetnEffects(); } }
        private IndexerProperty<int, int> _effect;
        public IndexerProperty<int, int> Effect { get { return _effect ?? (_effect = new IndexerProperty<int, int> { read = GetEffect, write = SetEffect }); } }
        private IndexerProperty<int, int> _effectValue;
        public IndexerProperty<int, int> EffectValue { get { return _effectValue ?? (_effectValue = new IndexerProperty<int, int> { read = GetEffectValue, write = SetEffectValue }); } }
        private IndexerProperty<int, int> _effectQuality;
        public IndexerProperty<int, int> EffectQuality { get { return _effectQuality ?? (_effectQuality = new IndexerProperty<int, int> { read = GetEffectQuality, write = SetEffectQuality }); } }
        private IndexerProperty<int, int> _effectLevel;
        public IndexerProperty<int, int> EffectLevel { get { return _effectLevel ?? (_effectLevel = new IndexerProperty<int, int> { read = GetEffectLevel, write = SetEffectLevel }); } }
        private IndexerProperty<int, int> _effectRemakes;
        public IndexerProperty<int, int> EffectRemakes { get { return _effectRemakes ?? (_effectRemakes = new IndexerProperty<int, int> { read = GetEffectRemakes, write = SetEffectRemakes }); } }
        private IndexerProperty<int, int> _effectTime;
        public IndexerProperty<int, int> EffectTime { get { return _effectTime ?? (_effectTime = new IndexerProperty<int, int> { read = GetEffectTime, write = SetEffectTime }); } }
        private IndexerProperty<bool, int> _effectDone;
        public IndexerProperty<bool, int> EffectDone { get { return _effectDone ?? (_effectDone = new IndexerProperty<bool, int> { read = GetEffectDone, write = SetEffectDone }); } }
        private IndexerProperty<int, int> _classRestriction;
        public IndexerProperty<int, int> ClassRestriction { get { return _classRestriction ?? (_classRestriction = new IndexerProperty<int, int> { read = GetRestriction, write = SetRestriction }); } }
        public int nClassRestrictions { get { return GetNRestrictions(); } }
        public String ToolTipText { get { return GetToolTipText(); } }
        public String LongInfo { get { return GetLongInfo(); } }
        public bool Changed { get { return bChanged; } set { bChanged = value; } }
        public bool Deleted { get { return bDeleted; } set { SetDeleted(value); } }
        #endregion
    }
}
