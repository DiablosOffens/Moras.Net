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

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using System.Diagnostics;
    using DelphiClasses;
    using dxgettext;
    using System.Runtime.InteropServices;

    public enum ConfigConstants
    {
        // So viele Skills, inklusive der Hidden, darf eine Klasse haben
        MAX_SKILLS = 20,
        MAX_BONUS_NAMES = 5,	// So viele Namen für einen Bonus
        MAX_LIQUIDS = 3,	// So viele Flüssigkeiten kann ein Bonus maximal haben
    }

    public struct SItemUpdate
    {
        public String URL;	// Die zu übergebende URL, mit Platzhalter
        public String Website;	// Eine zusätzliche Website
        public String Name;	// Unter welchem Namen wird es angezeigt
        public String Registry;	// Unter welchem Namen in Registry gespeichert
        public bool bFound;	// Ist wahr, wenn Abschnitt gefunden wurde
    }

    public struct SOptionMenuItem
    {
        public String Group;
        public String Name;
        public String FileName;
        public bool bPricing;
        public bool bRadioItem;
    }

    public struct SSCPriceModel : INeedsInitialization
    {
        public String Name;	// Name des Preismodells
        public int PPGem;	// Preis je Juwel
        public int PPItem;	// Preis je Item
        public int PPOrder;	// Preis je Auftrag
        public int PPLevel;	// Preis pro Item-Level
        public int PPIP;		// Preis Pro Imbue-Punkt des Gems
        public int PPOC;		// Preis Pro Überladungs-Punkt (Pro Gegenstand)
        public int pGeneralMarkup;	// Genereller Aufschlag (%)
        public int PPHour;		// Preis pro Stunde
        public int[] pGQMarkup { get; private set; } // Aufpreis pro Juwelen-Qualitätsstufe
        public bool iGQMarkup;		// Aufpreis pro Qualitätsstufe beachten?
        public int[] PPGTier { get; private set; } // Preis pro Juwelenstufe
        public bool iPPGTier;		// Preis pro Juwelenstufe beachten?
        public bool iCost;			// Sollen die Kosten mit beachtet werden?

        public void Init()
        {
            pGQMarkup = new int[7];
            PPGTier = new int[10];
        }
    }

    public enum ESlotType { Armor, Weapon, Jewelry };
    public struct SItemSlot
    {
        public String strPosId;	// Der Name der Position
        public String strPosClass;
        public String strPosName;
        public String strSlotName;	// Der Name unter dem Slot im Charakterbild
        public String strKortId;
        public DynamicArray<String> arIds;	// In diesen Strings steht z.B. Armlinge
        public int oldpos;	// Die Positionsangabe im alten-Mora-Format
        public ESlotType type;
    }

    public struct SDustsNLiquids : INeedsInitialization
    {
        public String id;
        public String Name;
        public bool bDust;	// Wahr wenn Staub
        public int Price;	// Und wieviel der Kostet
        public int[] Count { get; private set; }

        public void Init()
        {
            Count = new int[2];
        }
    }

    public struct SDamageType
    {
        public String id;
        public String Name;
    }

    public struct SExtension
    {
        public String id;
        public String Name;
    }

    public struct SItemSubClass : INeedsInitialization
    {
        public String id;
        public String Name;
        public int iSpeed;	// Der Speed-Wert bei einer Waffe
        public int idDamage;	// Schadensart bei Waffen
        public int[] iMaterial { get; private set; } // Wieviel Material wird pro Gegenstand gebraucht (6 Materialarten)
        public int[] arValue { get; private set; } // Werte bei ensprechender Materialstufe (AF oder DPS)

        public void Init()
        {
            iMaterial = new int[6];
            arValue = new int[10];
        }
    }

    public struct SItemClass
    {
        public String id;
        public String Name;
        public int oldid;	// Alte ID
        public int iRealm;
        public int idSkill;	// Für Klasse benötigter Skill
        public ESlotType SlotType;	// Für welchen Slot-Typ diese Klasse gilt (armor/weapon)
        public int idMaterial;
        public int iSize;	// Größenangabe bei Schildern
        public int bmPositions;	// Bitmaske der Slotpositionen, wo der Gegenstand hinpasst
        public bool bStats;	// Zeigt an, ob es weitere Stats gibt (DPS, AF) Falsch wenn es nur einen Level gibt
        public bool bDualWield;	// Eine Dualwielder-Waffe?
        public DynamicArray<SItemSubClass> arSubClasses;
    }

    public struct SMaterialLevel
    {
        public String Name;
        public String GemPrefix;
        public int iPrice;
    }

    public struct SMaterial : INeedsInitialization
    {
        public String id;
        public SMaterialLevel[] arLevel { get; private set; } // Gibt auf absehbare Zeit nicht mehr als 10 Materialstufen

        public void Init()
        {
            arLevel = new SMaterialLevel[10];
        }
    }

    public struct SClass : INeedsInitialization
    {
        public String id;
        public String Name;
        public int iRealm;
        public int oldid;	// alte Mora's ID
        public int nRaces;	// Wieviele Rassen sind mit der Klasse möglich
        public int nSkills;	// Wieviele Skills hat die Klasse. Werte danach sind Hidden Skills
        public int nAllSkills;	// Skills gesamt, also normale und Hidden
        public int iShield;	// Schildgröße, wenn Klasse eins haben kann
        public int iArmor;	// Die höchste tragbare Rüstungsklasse
        public int iMagic;	// Welches ist das Magieattribut (direkt der integer-index)
        public bool BDualWield;	// Eine Dualwield-Klasse?
        public int[] arRaces { get; private set; } // Ein Array mit den Rassen-IDs (Hoffe das werden nie mehr als 10)
        public int[] arSkills { get; private set; } // Im Moment ist 8 das maximum, wird hoffentlich nicht mehr

        public void Init()
        {
            arRaces = new int[10];
            arSkills = new int[(int)ConfigConstants.MAX_SKILLS];
        }
    }

    public struct SRace : INeedsInitialization
    {
        public String id;
        public String Name;
        public int[] arResists { get; private set; }

        public void Init()
        {
            arResists = new int[9];
        }
    }

    public struct SAttribute	// Das sind die Angezeigten Werte
    {
        public String id;
        public String Name;
        public int displayid;
        public int SkillId;	// Die id eines zugeordneten Skills (Nur bei Foki interessant im Moment)
        public int CapId;	// Caperhöhung für...

        // Links auf Cap und Overcap-Attribut
        public int iCapRef;			// ID des zugehörigen Caps
        public int iOvercapRef;		// ID des zugehörigen ÜBER-Caps

        public Single capadd;
        public Single capmult;
        //	Single	ccapadd;	// Cap-Cap
        //	Single	ccapmult;
        //	int	sum;	// temporärer Wert für die Berechnung der Attributanzeige
        //	int	cap;	// temporär für Attributanzeige
        //	bool bSingle;	// temporär ...
        public bool bCATA; // Attribut ist ein Cata-Attribut
        public bool bTOA;	// Attribut ist ein TOA-Attribut
        public bool bLOTM;	// Attribut ist ein LOTM-Attribut
        public bool bCapAttr;	// Es werden Cap und Attribut Wert erhöht
        //	int	weight;	// Gewichtung für Utility-Berechnung
        //Single	upv;	// Utility Per Value
    }

    public struct SGroups : INeedsInitialization	// Die Gruppen, in denen die Bannzauberboni eingeordnet sind
    {
        public String id;
        public String Name;
        public bool bDropOnly;	// Wenn dieses Flag, dann Gruppe nur für Drops anzeigen
        public bool bSlot5Only;	// Wenn dieses Flag, dann Gruppe nur für Slot5 anzeigen
        public bool bNoWeight;	// Wenn dieses Flag, dann Gruppe nicht in den Wichtungen anzeigen
        public int ip_add;	// Fixkommawert zur ip-Berechnung (x.xx)
        public int ip_mult;	// Formel ip = Value * ip_mult + ip_add
        public int[] Gemvalue { get; private set; } //Bonus im jeweiligen Gemlevel
        public int[] DustAmount { get; private set; } // Wieviel Staub wird im jeweiligen Level benötigt
        public int[] LiquidAmount { get; private set; } // Wieviel Flüssigkeit wird im jeweiligen Level benötigt

        public void Init()
        {
            Gemvalue = new int[10];
            DustAmount = new int[10];
            LiquidAmount = new int[10];
        }
    }

    public struct SBonus : INeedsInitialization	// Die Boni auf den Items
    {
        public String id;
        public String[] Names { get; private set; } // Alternativer Name (für Chatlog-Import)
        public String GemId;
        public String GemName;
        public String KortId;
        public int idGroup;	// In welcher Gruppe der Bonus eingeordnet ist
        public int iRealm;		// Für welche Realms wirkt dieser Bonus
        public int oldid; 	// Die id von der alten Version
        public int ip_add;	// Fixkommawert zur ip-Berechnung (x.xx)
        public int ip_mult;	// Formel ip = Value * ip_mult + ip_add
        public bool bCraftable;	// Ist der Bonus Craftable, d.h. gibts ein Gem?
        public bool bPercent;	// Ist wahr, wenn Bonus ein Prozentwert ist
        public bool bCraftOnly;	// Nur für Craftable-Items anzeigen
        public int GemDust;	// Staub für Gemherstellung
        public int[] GemLiquids { get; private set; } // Flüssigkeiten
        public int nLiquids;
        public int nNames;	// so viele verschiedene Namen hat der Bonus
        public int[] Gemvalue { get; private set; } //Bonus im jeweiligen Gemlevel
        public int[] DustAmount { get; private set; } // Wieviel Staub wird im jeweiligen Level benötigt
        public int[] LiquidAmount { get; private set; } // Wieviel Flüssigkeit wird im jeweiligen Level benötigt
        public int[] GemAmount { get; private set; } // Wieviel Steine werden im jeweiligen Level benötigt
        public DynamicArray<int> arAttributes;	// Die Liste der beeinflussten Attribute

        public void Init()
        {
            Names = new string[(int)ConfigConstants.MAX_BONUS_NAMES];
            GemLiquids = new int[(int)ConfigConstants.MAX_LIQUIDS];
            Gemvalue = new int[10];
            DustAmount = new int[10];
            LiquidAmount = new int[10];
            GemAmount = new int[10];
        }
    }

    //---------------------------------------------------------------------------
    // Unsere Klasse, welche alle in der config.xml enthaltenen Daten verwaltet
    public class Config
    {
        private static string _(string szMsgId) { return TGnuGettextInstance.gettext(szMsgId); }

        public Config()
        {
            int i, pmax;
            String tmp;
            bool langset = false;

            sysLanguage = TGnuGettextInstance.GetCurrentLanguage();
            bDebugStartup = false;
            bDebugSQL = false;
            pmax = Utils.ParamCount();
            for (int parms = 0; parms <= pmax; ++parms)
            {
                if (Utils.ParamStr(parms).Length > 4)
                {
                    tmp = Utils.ParamStr(parms).Substring(0, 4);
                    // manuelle Sprachumstellung mit lang=en auf zb Englisch
                    if (tmp == "lang")
                    {
                        if (Utils.ParamStr(parms).Length > 5)
                        {
                            tmp = Utils.ParamStr(parms).Substring(5, Utils.ParamStr(parms).Length - 5);
                            TGnuGettextInstance.UseLanguage(tmp);
                            langset = true;
                        }
                    }
                    else if (Utils.ParamStr(parms) == "debugstart")
                    {
                        bDebugStartup = true;
                    }
                    else if (Utils.ParamStr(parms) == "debugsql")
                    {
                        bDebugSQL = true;
                    }
                }
            }
            if (!langset)
            {
                tmp = TGnuGettextInstance.GetCurrentLanguage().Substring(0, 2);
                if (tmp != "de")
                    TGnuGettextInstance.UseLanguage("en");
            }

            strServers = new TStringList();
            strInterfaceElements = new TStringList();
            strInterfaceName = new TStringList();
            iSCLevel = Utils.GetRegistryString("SCLevel", "50").ToIntDef(50);
            bHalfIP = Utils.GetRegistryString("HalfIP", "0").ToIntDef(0) != 0;
            bKortEnglish = Utils.GetRegistryString("KortEnglish", "1").ToIntDef(1) != 0;
            bIgnoreRaceBoni = Utils.GetRegistryString("IgnoreRaceBoni", "0").ToIntDef(0) != 0;
            nServers = 0;
            nClasses = 0;
            nGroups = 0;
            nBonuses = 0;
            nMaterials = 0;
            nExtensions = 0;
            nItemClasses = 0;
            nDamageTypes = 0;
            nRaces = 1;
            nAttributes = 0;
            nIngredients = 0;
            arRaces.Length = 1;
            arRaces.Array[0].id = "NONE";
            arRaces.Array[0].Name = _("<keine>");
            for (i = 0; i < 9; i++)
                arRaces.Array[0].arResists[i] = 0;
            // Noch ein paar vordefinierte Werte setzen
            for (i = 0; i < 6; i++)
                arItemSlots[i].type = ESlotType.Armor;
            for (; i < 10; i++)
                arItemSlots[i].type = ESlotType.Weapon;
            for (; i < CPlayer.PLAYER_ITEMS; i++)
                arItemSlots[i].type = ESlotType.Jewelry;
            for (i = 0; i < CPlayer.PLAYER_ITEMS; i++)
                arItemSlots[i].oldpos = i + 1;
            arItemSlots[15].oldpos = 15;
            arItemSlots[17].oldpos = 17;
        }

        ~Config()
        {
            arServerType.Length = 0;
            arRaces.Length = 0;
            arAttributes.Length = 0;
            arClasses.Length = 0;
            arGroups.Length = 0;
            arBonuses.Length = 0;
            arMaterials.Length = 0;
            arExtensions.Length = 0;
            arItemClasses.Length = 0;
            arDamageTypes.Length = 0;
            arIngredients.Length = 0;
            arMenuItems.Length = 0;
        }

        private enum xGroup
        {
            GROUP_SERVERS,
            GROUP_RACES,
            GROUP_CLASSES,
            GROUP_ATTRIBUTES,
            GROUP_BONUSES,
            GROUP_GROUPS,
            GROUP_ITEMSLOTS,
            GROUP_INTERFACE,
            GROUP_MATERIALS,
            GROUP_ITEMCLASSES,
            GROUP_DAMAGETYPES,
            GROUP_INGREDIENTS,
            GROUP_PRICEMODEL,
            GROUP_ITEMUPDATE,
            GROUP_EXTENSIONS
        }

        public bool OpenConfig(String Filename)
        {	// Öffne das xml-datei und lade alle Werte
            bool bReturn;
            int pi, curClass = -1, curRace = -1, curAttrib = -1, curGroup = -1, curBonus, curSlot = -1;
            int curMaterial = -1, curLevel = -1, curId = -1;
            String strErr;
            int defaultDamage = -1;
            xGroup Group = 0;
            CXml xBase = new CXml();
            curBonus = -1;
            bReturn = xBase.OpenXml(Filename);
            strVersion = xBase.AttributeValue["version"];
            ItemUpdate.bFound = false;
            while (bReturn)
            {	// Zuerst schauen, ob der Tag zu einer übergeordneten Gruppe gehört
                iRealm = 7;
                if (xBase.isTag("servers"))
                    Group = xGroup.GROUP_SERVERS;
                if (xBase.isTag("races"))
                    Group = xGroup.GROUP_RACES;
                if (xBase.isTag("classes"))
                    Group = xGroup.GROUP_CLASSES;
                if (xBase.isTag("attributes"))
                    Group = xGroup.GROUP_ATTRIBUTES;
                if (xBase.isTag("bonuses"))
                    Group = xGroup.GROUP_BONUSES;
                if (xBase.isTag("groups"))
                    Group = xGroup.GROUP_GROUPS;
                if (xBase.isTag("itemslots"))
                    Group = xGroup.GROUP_ITEMSLOTS;
                if (xBase.isTag("interface"))
                    Group = xGroup.GROUP_INTERFACE;
                if (xBase.isTag("materials"))
                    Group = xGroup.GROUP_MATERIALS;
                if (xBase.isTag("extensions"))
                    Group = xGroup.GROUP_EXTENSIONS;
                if (xBase.isTag("itemclasses"))
                    Group = xGroup.GROUP_ITEMCLASSES;
                if (xBase.isTag("damagetypes"))
                    Group = xGroup.GROUP_DAMAGETYPES;
                if (xBase.isTag("dusts") || xBase.isTag("liquids"))
                    Group = xGroup.GROUP_INGREDIENTS;
                if (xBase.isTag("pricemodel"))
                {
                    Group = xGroup.GROUP_PRICEMODEL;
                    // Hier muß Preismodell initialisiert werden
                    sPriceModel.Init();
                    sPriceModel.PPGem = 0;	// Preis je Juwel
                    sPriceModel.PPItem = 0;	// Preis je Item
                    sPriceModel.PPOrder = 0;	// Preis je Auftrag
                    sPriceModel.PPLevel = 0;	// Preis pro Item-Level
                    sPriceModel.PPIP = 0;		// Preis Pro Imbue-Punkt des Gems
                    sPriceModel.PPOC = 0;		// Preis Pro Überladungs-Punkt (Pro Gegenstand)
                    sPriceModel.pGeneralMarkup = 0;	// Genereller Aufschlag (%)
                    sPriceModel.PPHour = 0;		// Preis pro Stunde
                    for (int i = 0; i < 7; i++)
                        sPriceModel.pGQMarkup[i] = 0;	// Aufpreis pro Juwelen-Qualitätsstufe
                    sPriceModel.iGQMarkup = false;		// Aufpreis pro Qualitätsstufe beachten?
                    for (int i = 0; i < 10; i++)
                        sPriceModel.PPGTier[i] = 0;	// Preis pro Juwelenstufe
                    sPriceModel.iPPGTier = false;		// Preis pro Juwelenstufe beachten?
                    sPriceModel.iCost = false;			// Sollen die Kosten mit beachtet werden?
                }
                if (xBase.isTag("item_update"))
                {
                    Group = xGroup.GROUP_ITEMUPDATE;
                    ItemUpdate.bFound = true;
                    ItemUpdate.URL = "";
                    ItemUpdate.Registry = "";
                    ItemUpdate.Website = "";
                }
                // Nun noch schauen, ob es der Language-Tag ist
                if (xBase.isTag("language"))
                {
                    iLanguage = Utils.Language2Int(xBase.Value);
                    strLanguage = xBase.Content;
                }
                // Nun innerhalb der Untergruppen auswerten
                switch (Group)
                {
                    case xGroup.GROUP_SERVERS:
                        if (xBase.isTag("server"))
                        {
                            strServers.Add(xBase.Content);
                            pi = Utils.Language2Int(xBase.Value);
                            arServerType.Length = nServers + 1;
                            arServerType[nServers++] = pi;
                        }
                        break;
                    case xGroup.GROUP_EXTENSIONS:
                        if (xBase.isTag("extension"))
                        {
                            nExtensions++;
                            arExtensions.Length = nExtensions;
                            arExtensions.Array[nExtensions - 1].Name = xBase.Content;
                            arExtensions.Array[nExtensions - 1].id = xBase.AttributeValue["id"].ToLower();
                        } goto case xGroup.GROUP_RACES; //TODO: wirklich?
                    case xGroup.GROUP_RACES:
                        if (xBase.isTag("race"))
                        {	// Steht die Rasse schon im Array?
                            curRace = 0;
                            for (int i = 0; i < nRaces; i++)
                            {
                                if (arRaces[i].id == xBase.Value) curRace = i;
                            }
                            if (curRace == 0)
                            {
                                curRace = nRaces;
                                nRaces++;
                                arRaces.Length = nRaces;
                                arRaces.Array[curRace].id = xBase.Value;
                                arRaces.Array[curRace].Name = "!Kein Rassenname!";
                                for (int i = 0; i < 9; i++)
                                    arRaces[curRace].arResists[i] = 0;
                            }
                        }
                        if (xBase.isTag("name"))
                            arRaces.Array[curRace].Name = xBase.Content;
                        if (xBase.isTag("resist"))
                        {
                            pi = Utils.Resist2Int(xBase.Value);
                            if (pi >= 0)
                                arRaces[curRace].arResists[pi] = int.Parse(xBase.Content);
                            else
                            {	// Fehlermeldung
                                strErr = string.Format(_("Unbekannte Resistenz '%s' in Zeile %d:\n%s"), xBase.Value, xBase.LineNo, xBase.currentLine);
                                Utils.MorasErrorMessage(strErr, _("XML-Fehler in Datei 'config.xml'"));
                                Application.Exit();
                            }
                        }
                        break;
                    case xGroup.GROUP_CLASSES:
                        if (xBase.isTag("class"))
                        {	// Steht die Klasse schon im Array?
                            curClass = -1;
                            for (int i = 0; i < nClasses; i++)
                            {
                                if (arClasses[i].id == xBase.AttributeValue["id"]) curClass = i;
                            }
                            if (curClass == -1)
                            {
                                curClass = nClasses;
                                nClasses++;
                                arClasses.Length = nClasses;
                                arClasses.Array[curClass].id = xBase.AttributeValue["id"];
                                arClasses.Array[curClass].oldid = xBase.AttributeValue["oldid"].ToIntDef(0);
                                arClasses.Array[curClass].Name = "!Kein Klassenname!";
                                arClasses.Array[curClass].nRaces = 0;
                                arClasses.Array[curClass].nSkills = 0;
                                arClasses.Array[curClass].nAllSkills = 0;
                                arClasses.Array[curClass].iMagic = -1;
                                arClasses.Array[curClass].iShield = 0;
                                arClasses.Array[curClass].BDualWield = false;
                            }
                        }
                        else if (xBase.isTag("name"))
                            arClasses.Array[curClass].Name = xBase.Content;
                        else if (xBase.isTag("realm"))
                        {
                            pi = Utils.Realm2Int(xBase.Content);
                            if (pi != 0)
                                arClasses.Array[curClass].iRealm = pi;
                            else
                            {	// Fehlermeldung
                                strErr = string.Format(_("Unbekanntes Reich '%s' in Zeile %d:\n%s"), xBase.Content, xBase.LineNo, xBase.currentLine);
                                Utils.MorasErrorMessage(strErr, _("XML-Fehler in Datei 'config.xml'"));
                                Application.Exit();
                            }
                        }
                        else if (xBase.isTag("race"))
                        {
                            pi = GetRaceId(xBase.Content);
                            if (pi != 0)
                                arClasses[curClass].arRaces[arClasses.Array[curClass].nRaces++] = pi;
                            else
                            {	// Fehlermeldung
                                strErr = string.Format(_("Unbekannte Rasse '%s' in Zeile %d:\n%s"), xBase.Content, xBase.LineNo, xBase.currentLine);
                                Utils.MorasErrorMessage(strErr, _("XML-Fehler in Datei 'config.xml'"));
                                Application.Exit();
                            }
                        }
                        else if (xBase.isTag("armor"))
                        {
                            pi = GetItemClassId(xBase.Content);
                            arClasses.Array[curClass].iArmor = pi;
                            if (pi == -1)
                            {	// Fehlermeldung
                                strErr = string.Format(_("Unbekannte Rüstungsart '%s' in Zeile %d:\n%s"), xBase.Content, xBase.LineNo, xBase.currentLine);
                                Utils.MorasErrorMessage(strErr, _("XML-Fehler in Datei 'config.xml'"));
                                Application.Exit();
                            }
                        }
                        else if (xBase.isTag("magic"))
                        {
                            pi = GetAttributeId(xBase.Content);
                            arClasses.Array[curClass].iMagic = pi;
                            if (pi == -1)
                            {	// Fehlermeldung
                                strErr = string.Format(_("Unbekannte Magieart '%s' in Zeile %d:\n%s"), xBase.Content, xBase.LineNo, xBase.currentLine);
                                Utils.MorasErrorMessage(strErr, _("XML-Fehler in Datei 'config.xml'"));
                                Application.Exit();
                            }
                        }
                        else if (xBase.isTag("dualwield"))
                            arClasses.Array[curClass].BDualWield = true;
                        else if (xBase.isTag("shield"))
                        {
                            if (xBase.Content.Equals("small", StringComparison.CurrentCultureIgnoreCase))
                                arClasses.Array[curClass].iShield = 1;
                            else if (xBase.Content.Equals("medium", StringComparison.CurrentCultureIgnoreCase))
                                arClasses.Array[curClass].iShield = 2;
                            else if (xBase.Content.Equals("large", StringComparison.CurrentCultureIgnoreCase))
                                arClasses.Array[curClass].iShield = 3;
                        }
                        else if (xBase.isTag("skill"))
                        {
                            pi = GetAttributeId(xBase.Content);
                            if (pi != -1)
                            {
                                arClasses[curClass].arSkills[arClasses.Array[curClass].nSkills++] = pi;
                                arClasses.Array[curClass].nAllSkills++;
                            }
                            else
                            {	// Fehlermeldung
                                strErr = string.Format(_("Unbekannter Skill '%s' in Zeile %d:\n%s"), xBase.Content, xBase.LineNo, xBase.currentLine);
                                Utils.MorasErrorMessage(strErr, _("XML-Fehler in Datei 'config.xml'"));
                                Application.Exit();
                            }
                        }
                        else if (xBase.isTag("hidden_skill"))
                        {	// Hidden-Skills müssen immer nach den normalen stehen
                            pi = GetAttributeId(xBase.Content);
                            if (pi != -1)
                                arClasses[curClass].arSkills[arClasses.Array[curClass].nAllSkills++] = pi;
                            else
                            {	// Fehlermeldung
                                strErr = string.Format(_("Unbekannter Skill '%s' in Zeile %d:\n%s"), xBase.Content, xBase.LineNo, xBase.currentLine);
                                Utils.MorasErrorMessage(strErr, _("XML-Fehler in Datei 'config.xml'"));
                                Application.Exit();
                            }
                        }
                        break;
                    case xGroup.GROUP_ATTRIBUTES:
                        if (xBase.isTag("attribute"))
                        {	// Steht das Attribut schon im Array?
                            curAttrib = -1;
                            for (int i = 0; i < nAttributes; i++)
                            {
                                if (arAttributes[i].id == xBase.Value) curAttrib = i;
                            }
                            if (curAttrib == -1)
                            {
                                curAttrib = nAttributes;
                                nAttributes++;
                                arAttributes.Length = nAttributes;
                                arAttributes.Array[curAttrib].id = xBase.Value;
                                arAttributes.Array[curAttrib].Name = "!Kein Attributname!";
                                arAttributes.Array[curAttrib].displayid = -1;
                                arAttributes.Array[curAttrib].SkillId = -1;
                                arAttributes.Array[curAttrib].CapId = -1;
                                arAttributes.Array[curAttrib].capadd = 5;
                                arAttributes.Array[curAttrib].capmult = 0.2f;
                                arAttributes.Array[curAttrib].bCATA = false;
                                arAttributes.Array[curAttrib].bTOA = false;
                                arAttributes.Array[curAttrib].bLOTM = false;
                                arAttributes.Array[curAttrib].bCapAttr = false;
                                arAttributes.Array[curAttrib].iCapRef = -1;
                                arAttributes.Array[curAttrib].iOvercapRef = -1;
                            }
                        }
                        else if (xBase.isTag("name"))
                            arAttributes.Array[curAttrib].Name = xBase.Content;
                        else if (xBase.isTag("cata"))
                            arAttributes.Array[curAttrib].bCATA = true;
                        else if (xBase.isTag("toa"))
                            arAttributes.Array[curAttrib].bTOA = true;
                        else if (xBase.isTag("lotm"))
                            arAttributes.Array[curAttrib].bLOTM = true;
                        else if (xBase.isTag("cap_and_attribute"))
                            arAttributes.Array[curAttrib].bCapAttr = true;
                        else if (xBase.isTag("displayid"))
                            arAttributes.Array[curAttrib].displayid = int.Parse(xBase.Content);
                        else if (xBase.isTag("capadd"))
                            arAttributes.Array[curAttrib].capadd = (float)Utils.Str2Double(xBase.Content);
                        else if (xBase.isTag("capmult"))
                            arAttributes.Array[curAttrib].capmult = (float)Utils.Str2Double(xBase.Content);
                        else if (xBase.isTag("cap"))
                        {	// Erhöht Cap-Wert des folgenden Wertes
                            pi = GetAttributeId(xBase.Content);
                            if (pi != -1)
                                arAttributes.Array[curAttrib].CapId = pi;
                            else
                            {	// Fehlermeldung
                                strErr = string.Format(_("Unbekannter Skill '%s' in Zeile %d:\n%s"), xBase.Content, xBase.LineNo, xBase.currentLine);
                                Utils.MorasErrorMessage(strErr, _("XML-Fehler in Datei 'config.xml'"));
                                Application.Exit();
                            }
                        }
                        else if (xBase.isTag("skill"))
                        {	// Zugehöriger Skill
                            pi = GetAttributeId(xBase.Content);
                            if (pi != -1)
                                arAttributes.Array[curAttrib].SkillId = pi;
                            else
                            {	// Fehlermeldung
                                strErr = string.Format(_("Unbekannter Skill '%s' in Zeile %d:\n%s"), xBase.Content, xBase.LineNo, xBase.currentLine);
                                Utils.MorasErrorMessage(strErr, _("XML-Fehler in Datei 'config.xml'"));
                                Application.Exit();
                            }
                        }
                        break;
                    case xGroup.GROUP_GROUPS:
                        if (xBase.isTag("group"))
                        {	// Steht die Gruppe schon im Array?
                            curGroup = -1;
                            for (int i = 0; i < nGroups; i++)
                            {
                                if (arGroups[i].id == xBase.Value) curGroup = i;
                            }
                            if (curGroup == -1)
                            {
                                curGroup = nGroups;
                                nGroups++;
                                arGroups.Length = nGroups;
                                arGroups.Array[curGroup].id = xBase.Value;
                                arGroups.Array[curGroup].Name = "!Kein Gruppenname!";
                                arGroups.Array[curGroup].ip_add = 0;
                                arGroups.Array[curGroup].ip_mult = 0;
                                arGroups.Array[curGroup].bDropOnly = false;
                                arGroups.Array[curGroup].bSlot5Only = false;
                                arGroups.Array[curGroup].bNoWeight = false;
                                for (int i = 0; i < 10; i++)
                                {
                                    arGroups[curGroup].Gemvalue[i] = 0;
                                    arGroups[curGroup].DustAmount[i] = 0;
                                    arGroups[curGroup].LiquidAmount[i] = 0;
                                }
                            }
                        }
                        if (xBase.isTag("name"))
                            arGroups.Array[curGroup].Name = xBase.Content;
                        if (xBase.isTag("drop_only"))
                            arGroups.Array[curGroup].bDropOnly = true;
                        if (xBase.isTag("slot5_only"))
                            arGroups.Array[curGroup].bSlot5Only = true;
                        if (xBase.isTag("no_weight"))
                            arGroups.Array[curGroup].bNoWeight = true;
                        if (xBase.isTag("ip_add"))
                            arGroups.Array[curGroup].ip_add = (int)Utils.Str2Double(xBase.Content) * 100;
                        if (xBase.isTag("ip_mult"))
                            arGroups.Array[curGroup].ip_mult = (int)Utils.Str2Double(xBase.Content) * 100;
                        if (xBase.isTag("gemlevel"))
                        {
                            pi = int.Parse(xBase.Content);
                            if ((pi > 0) && (pi < 11))
                            {
                                arGroups[curGroup].Gemvalue[pi - 1] = int.Parse(xBase.AttributeValue["bonus"]);
                                arGroups[curGroup].DustAmount[pi - 1] = int.Parse(xBase.AttributeValue["dust"]);
                                arGroups[curGroup].LiquidAmount[pi - 1] = int.Parse(xBase.AttributeValue["liquid"]);
                            }
                        }
                        break;
                    case xGroup.GROUP_BONUSES:
                        if (xBase.isTag("bonus"))
                        {	// Hat der letzte Bonus ein zugeordnetes Attribut?
                            if ((curBonus >= 0) && (arBonuses[curBonus].arAttributes.Length == 0))
                            {	// Fehlermeldung
                                strErr = string.Format(_("Warnung!\nDer Bonus '%s' hat kein zugeordnetes Attribut!\nZeile %d:\n%s")
                                    , arBonuses[curBonus].id, xBase.LineNo, xBase.currentLine);
                                Utils.MorasErrorMessage(strErr, _("XML-Fehler in Datei 'config.xml'"));
                            }
                            // Steht der Bonus schon im Array?
                            curBonus = -1;
                            curClass = 0;	// Dient der Bestimmung, ob Bonusname oder Gemname
                            for (int i = 0; i < nBonuses; i++)
                            {
                                if (arBonuses[i].id == xBase.AttributeValue["id"]) curBonus = i;
                            }
                            if (curBonus == -1)
                            {
                                curBonus = nBonuses;
                                nBonuses++;
                                arBonuses.Length = nBonuses;
                                arBonuses.Array[curBonus].id = xBase.AttributeValue["id"];
                                arBonuses.Array[curBonus].idGroup = -1;
                                arBonuses.Array[curBonus].iRealm = 7;
                                arBonuses.Array[curBonus].bPercent = false;
                                arBonuses.Array[curBonus].oldid = xBase.AttributeValue["oldid"].ToIntDef(0);
                                arBonuses.Array[curBonus].bCraftable = false;
                                arBonuses.Array[curBonus].bCraftOnly = false;
                                arBonuses.Array[curBonus].nLiquids = 0;
                                // Das default Attribut
                                int aid = GetAttributeId(xBase.Value);
                                if (aid >= 0)
                                {
                                    arBonuses.Array[curBonus].arAttributes.Length = 1;
                                    arBonuses.Array[curBonus].arAttributes[0] = aid;
                                }
                                else
                                    arBonuses.Array[curBonus].arAttributes.Length = 0;
                                arBonuses.Array[curBonus].KortId = xBase.AttributeValue["kortid"];
                            }
                            arBonuses[curBonus].Names[0] = "!Kein Bonusname!";
                            arBonuses.Array[curBonus].nNames = 0;
                        }
                        else if (xBase.isTag("name"))
                        {
                            if (curClass != 0)	// Wir sind jetzt im Gem-Tag
                                arBonuses.Array[curBonus].GemName = xBase.Content;
                            else	// Nicht im Gem-Tag, also ein Bonusname
                            {
                                if (arBonuses[curBonus].nNames == (int)ConfigConstants.MAX_BONUS_NAMES)
                                {
                                    strErr = string.Format(_("Zu viele Namen für Bonus '%s' in Zeile %d:\n%s"), arBonuses[curBonus].id, xBase.LineNo, xBase.currentLine);
                                    Utils.MorasErrorMessage(strErr, _("XML-Fehler in Datei 'config.xml'"));
                                    Application.Exit();
                                }
                                arBonuses[curBonus].Names[arBonuses.Array[curBonus].nNames++] = xBase.Content;
                            }
                        }
                        else if (xBase.isTag("ip_add"))
                            arBonuses.Array[curBonus].ip_add = (int)Utils.Str2Double(xBase.Content) * 100;
                        else if (xBase.isTag("ip_mult"))
                            arBonuses.Array[curBonus].ip_mult = (int)Utils.Str2Double(xBase.Content) * 100;
                        else if (xBase.isTag("percent"))
                            arBonuses.Array[curBonus].bPercent = true;
                        else if (xBase.isTag("craftonly"))
                            arBonuses.Array[curBonus].bCraftOnly = true;
                        else if (xBase.isTag("gem"))
                        {	// Es gibt ein Gem, also ist Bonus craftable
                            arBonuses.Array[curBonus].bCraftable = true;
                            arBonuses.Array[curBonus].GemId = xBase.Value;
                            curClass = 1;
                        }
                        else if (xBase.isTag("dust"))
                        {	// Hier mal noch eine ID besorgen
                            pi = GetIngredientId(xBase.Content);
                            if (pi >= 0)
                                arBonuses.Array[curBonus].GemDust = pi;
                            else
                            {	// Fehlermeldung
                                strErr = string.Format(_("Unbekannter Staub '%s' in Zeile %d:\n%s"), xBase.Content, xBase.LineNo, xBase.currentLine);
                                Utils.MorasErrorMessage(strErr, _("XML-Fehler in Datei 'config.xml'"));
                                Application.Exit();
                            }
                        }
                        else if (xBase.isTag("liquid"))
                        {	// Hier mal noch eine ID besorgen
                            pi = GetIngredientId(xBase.Content);
                            if (pi >= 0)
                            {
                                if (arBonuses[curBonus].nLiquids == (int)ConfigConstants.MAX_LIQUIDS)
                                {
                                    strErr = string.Format(_("Zu viele Flüssigkeiten für Bonus '%s' in Zeile %d:\n%s"), arBonuses[curBonus].id, xBase.LineNo, xBase.currentLine);
                                    Utils.MorasErrorMessage(strErr, _("XML-Fehler in Datei 'config.xml'"));
                                    Application.Exit();
                                }
                                arBonuses[curBonus].GemLiquids[arBonuses.Array[curBonus].nLiquids++] = pi;
                            }
                            else
                            {	// Fehlermeldung
                                strErr = string.Format(_("Unbekannte Flüssigkeit '%s' in Zeile %d:\n%s"), xBase.Content, xBase.LineNo, xBase.currentLine);
                                Utils.MorasErrorMessage(strErr, _("XML-Fehler in Datei 'config.xml'"));
                                Application.Exit();
                            }
                        }
                        else if (xBase.isTag("realm"))
                        {
                            pi = Utils.Realm2Int(xBase.Content);
                            if (pi != 0)
                                arBonuses.Array[curBonus].iRealm = pi;
                            else
                            {	// Fehlermeldung
                                strErr = string.Format(_("Unbekanntes Reich '%s' in Zeile %d:\n%s"), xBase.Content, xBase.LineNo, xBase.currentLine);
                                Utils.MorasErrorMessage(strErr, _("XML-Fehler in Datei 'config.xml'"));
                                Application.Exit();
                            }
                        }
                        else if (xBase.isTag("group"))
                        {
                            pi = GetGroupId(xBase.Content);
                            arBonuses.Array[curBonus].idGroup = pi;
                            if (pi == -1)
                            {	// Fehlermeldung
                                strErr = string.Format(_("Unbekannte Gruppe '%s' in Zeile %d:\n%s"), xBase.Content, xBase.LineNo, xBase.currentLine);
                                Utils.MorasErrorMessage(strErr, _("XML-Fehler in Datei 'config.xml'"));
                                Application.Exit();
                            }
                            arBonuses.Array[curBonus].ip_add = arGroups[pi].ip_add;
                            arBonuses.Array[curBonus].ip_mult = arGroups[pi].ip_mult;
                            for (int i = 0; i < 10; i++)
                            {
                                arBonuses[curBonus].Gemvalue[i] = arGroups[pi].Gemvalue[i];
                                arBonuses[curBonus].DustAmount[i] = arGroups[pi].DustAmount[i];
                                arBonuses[curBonus].LiquidAmount[i] = arGroups[pi].LiquidAmount[i];
                                arBonuses[curBonus].GemAmount[i] = 1;
                            }
                        }
                        else if (xBase.isTag("effect"))
                        {
                            pi = GetAttributeId(xBase.Content);
                            if (pi == -1)
                            {	// Fehlermeldung
                                strErr = string.Format(_("Unbekannter Effekt '%s' in Zeile %d:\n%s"), xBase.Content, xBase.LineNo, xBase.currentLine);
                                Utils.MorasErrorMessage(strErr, _("XML-Fehler in Datei 'config.xml'"));
                                Application.Exit();
                            }
                            // Den type auswerten
                            if (xBase.Value == "VALUE")
                            {	// im Moment nichts machen
                            }
                            else if (xBase.Value == "CAP")
                            {
                                pi |= 0x1000;
                            }
                            else
                            {	// Fehlermeldung
                                strErr = string.Format(_("Unbekanntes oder fehlendes type-Attribut in Zeile %d:\n%s"), xBase.LineNo, xBase.currentLine);
                                Utils.MorasErrorMessage(strErr, _("XML-Fehler in Datei 'config.xml'"));
                                Application.Exit();
                            }
                            arBonuses.Array[curBonus].arAttributes.Length++;
                            arBonuses.Array[curBonus].arAttributes[arBonuses[curBonus].arAttributes.Length - 1] = pi;
                        }
                        else if (xBase.isTag("gemlevel"))
                        {	// Werte, die in der Gruppe definiert sind, werden überschrieben
                            pi = int.Parse(xBase.Content);
                            if ((pi > 0) && (pi < 11))
                            {
                                arBonuses[curBonus].Gemvalue[pi - 1] = int.Parse(xBase.AttributeValue["bonus"]);
                                arBonuses[curBonus].DustAmount[pi - 1] = int.Parse(xBase.AttributeValue["dust"]);
                                arBonuses[curBonus].LiquidAmount[pi - 1] = int.Parse(xBase.AttributeValue["liquid"]);
                                arBonuses[curBonus].GemAmount[pi - 1] = xBase.AttributeValue["gems"].ToIntDef(1);
                                // Weiter hinten liegende Felder müssen gelöscht (0) werden.
                                for (int i = pi; i < 10; i++)
                                {
                                    arBonuses[curBonus].Gemvalue[i] = 0;
                                    arBonuses[curBonus].DustAmount[i] = 0;
                                    arBonuses[curBonus].LiquidAmount[i] = 0;
                                    arBonuses[curBonus].GemAmount[i] = 0;
                                }
                            }
                        }
                        break;
                    case xGroup.GROUP_ITEMSLOTS:
                        if (xBase.isTag("slot"))
                        {
                            curSlot = int.Parse(xBase.Value) - 1;
                            curId = 0;
                        }
                        else if (xBase.isTag("pos_id"))
                            arItemSlots[curSlot].strPosId = xBase.Content;
                        else if (xBase.isTag("pos_class"))
                            arItemSlots[curSlot].strPosClass = xBase.Content;
                        else if (xBase.isTag("pos_name"))
                            arItemSlots[curSlot].strPosName = xBase.Content;
                        else if (xBase.isTag("kort_id"))
                            arItemSlots[curSlot].strKortId = xBase.Content;
                        else if (xBase.isTag("slot_name"))
                            arItemSlots[curSlot].strSlotName = xBase.Content;
                        else if (xBase.isTag("id"))
                        {
                            arItemSlots[curSlot].arIds.Length = curId + 1;
                            arItemSlots[curSlot].arIds[curId] = xBase.Content.ToLower();
                            curId++;
                        }
                        break;
                    case xGroup.GROUP_INTERFACE:
                        if (xBase.isTag("element"))
                        {
                            pi = strInterfaceElements.IndexOf(xBase.Value);
                            if (pi == -1)
                            {	// Das Element ist neu
                                strInterfaceElements.Add(xBase.Value);
                                strInterfaceName.Add(xBase.Content);
                            }
                            else
                                strInterfaceName.Strings[pi] = xBase.Content;
                        }
                        break;
                    case xGroup.GROUP_MATERIALS:
                        if (xBase.isTag("material"))
                        {
                            curMaterial = -1;
                            for (int i = 0; i < nMaterials; i++)
                            {
                                if (arMaterials[i].id == xBase.Value) curMaterial = i;
                            }
                            if (curMaterial == -1)
                            {
                                curMaterial = nMaterials;
                                nMaterials++;
                                arMaterials.Length = nMaterials;
                                arMaterials.Array[curMaterial].id = xBase.Value;
                                for (int i = 0; i < 10; i++)
                                    arMaterials[curMaterial].arLevel[i].iPrice = 0;
                            }
                        }
                        if (xBase.isTag("level"))
                        {
                            pi = xBase.Value.ToIntDef(1);
                            curLevel = pi - 1;
                        }
                        if (xBase.isTag("name"))
                        {
                            arMaterials[curMaterial].arLevel[curLevel].Name = xBase.Content;
                        }
                        if (xBase.isTag("gem_prefix"))
                        {
                            arMaterials[curMaterial].arLevel[curLevel].GemPrefix = xBase.Content;
                        }
                        if (xBase.isTag("price"))
                        {
                            arMaterials[curMaterial].arLevel[curLevel].iPrice = xBase.Content.ToIntDef(0);
                        }
                        break;
                    case xGroup.GROUP_ITEMCLASSES:
                        if (xBase.isTag("itemclass"))
                        {
                            curClass = -1;
                            curLevel = -1;
                            for (int i = 0; i < nItemClasses; i++)
                            {
                                if (arItemClasses[i].id == xBase.AttributeValue["id"]) curClass = i;
                            }
                            if (curClass == -1)
                            {
                                curClass = nItemClasses;
                                nItemClasses++;
                                arItemClasses.Length = nItemClasses;
                                arItemClasses.Array[curClass].id = xBase.AttributeValue["id"];
                                arItemClasses.Array[curClass].iRealm = 7;
                                arItemClasses.Array[curClass].idSkill = -1;
                                arItemClasses.Array[curClass].bmPositions = 0;
                                arItemClasses.Array[curClass].oldid = xBase.AttributeValue["oldid"].ToIntDef(0);
                                arItemClasses.Array[curClass].bStats = true;
                                arItemClasses.Array[curClass].bDualWield = false;
                                defaultDamage = -1;
                                arItemClasses.Array[curClass].iSize = 0;
                                if (arItemClasses[curClass].id.StartsWith("SHIELD"))
                                {	// Spezialbehandlung für Schilder
                                    if (arItemClasses[curClass].id.EndsWith("SMALL"))
                                        arItemClasses.Array[curClass].iSize = 1;
                                    else if (arItemClasses[curClass].id.EndsWith("MEDIUM"))
                                        arItemClasses.Array[curClass].iSize = 2;
                                    else if (arItemClasses[curClass].id.EndsWith("LARGE"))
                                        arItemClasses.Array[curClass].iSize = 3;
                                }
                            }
                        }
                        else if (xBase.isTag("name"))
                        {
                            if (curLevel == -1)
                                // Wir sind in keiner subclass
                                arItemClasses.Array[curClass].Name = xBase.Content;
                            else
                                arItemClasses.Array[curClass].arSubClasses.Array[curLevel].Name = xBase.Content;
                        }
                        else if (xBase.isTag("skill"))
                            arItemClasses.Array[curClass].idSkill = GetAttributeId(xBase.Content);
                        else if (xBase.isTag("no_stats"))
                            arItemClasses.Array[curClass].bStats = false;
                        else if (xBase.isTag("dualwield"))
                            arItemClasses.Array[curClass].bDualWield = true;
                        else if (xBase.isTag("material"))
                        {
                            if (curLevel == -1)
                            {	// material hat in der Itemclasse ne andere bedeutung als
                                // in der Unterklasse
                                pi = GetMaterialId(xBase.Content);
                                arItemClasses.Array[curClass].idMaterial = pi;
                            }
                            else
                            {	// Sind nun in der Subklasse
                                arItemClasses[curClass].arSubClasses[curLevel].iMaterial[0] = xBase.AttributeValue["metal"].ToIntDef(0);
                                arItemClasses[curClass].arSubClasses[curLevel].iMaterial[1] = xBase.AttributeValue["ore"].ToIntDef(0);
                                arItemClasses[curClass].arSubClasses[curLevel].iMaterial[2] = xBase.AttributeValue["wood"].ToIntDef(0);
                                arItemClasses[curClass].arSubClasses[curLevel].iMaterial[3] = xBase.AttributeValue["leather"].ToIntDef(0);
                                arItemClasses[curClass].arSubClasses[curLevel].iMaterial[4] = xBase.AttributeValue["cloth"].ToIntDef(0);
                                arItemClasses[curClass].arSubClasses[curLevel].iMaterial[5] = xBase.AttributeValue["thread"].ToIntDef(0);
                            }
                        }
                        else if (xBase.isTag("slot_type"))
                        {
                            if (xBase.Content == "armor")
                                arItemClasses.Array[curClass].SlotType = ESlotType.Armor;
                            else if (xBase.Content == "weapon")
                                arItemClasses.Array[curClass].SlotType = ESlotType.Weapon;
                            else
                                arItemClasses.Array[curClass].SlotType = ESlotType.Jewelry;
                        }
                        else if (xBase.isTag("realm"))
                        {
                            pi = Utils.Realm2Int(xBase.Content);
                            if (pi != 0)
                                arItemClasses.Array[curClass].iRealm = pi;
                            else
                            {	// Fehlermeldung
                                strErr = string.Format(_("Unbekanntes Reich '%s' in Zeile %d:\n%s"), xBase.Content, xBase.LineNo, xBase.currentLine);
                                Utils.MorasErrorMessage(strErr, _("XML-Fehler in Datei 'config.xml'"));
                                Application.Exit();
                            }
                        }
                        else if (xBase.isTag("position"))
                        {
                            Debug.Assert((curClass > -1) && (curClass < nItemClasses));
                            pi = GetSlotPosition(xBase.Content);
                            Debug.Assert((pi >= 0) && (pi < CPlayer.PLAYER_ITEMS));
                            if (pi >= 0)
                            {
                                arItemClasses.Array[curClass].bmPositions |= (1 << pi);
                            }
                        }
                        else if (xBase.isTag("subclass"))
                        {
                            curLevel = -1;
                            for (int i = 0; i < arItemClasses[curClass].arSubClasses.Length; i++)
                            {
                                if (arItemClasses[curClass].arSubClasses[i].id == xBase.Value) curLevel = i;
                            }
                            if (curLevel == -1)
                            {
                                curLevel = arItemClasses[curClass].arSubClasses.Length;
                                arItemClasses.Array[curClass].arSubClasses.Length++;
                                arItemClasses[curClass].arSubClasses.Array[curLevel].id = xBase.Value;
                                arItemClasses[curClass].arSubClasses.Array[curLevel].idDamage = defaultDamage;
                                for (int i = 0; i < 10; i++)
                                    arItemClasses[curClass].arSubClasses[curLevel].arValue[i] = 0;
                                for (int i = 0; i < 6; i++)
                                    arItemClasses[curClass].arSubClasses[curLevel].iMaterial[i] = 0;
                            }
                        }
                        else if (xBase.isTag("af"))
                        {
                            Debug.Assert(curLevel != -1);
                            Debug.Assert((curClass > -1) && (curClass < nItemClasses));
                            pi = xBase.Value.ToIntDef(0);
                            if (pi > 0)
                                arItemClasses[curClass].arSubClasses[curLevel].arValue[pi - 1] = xBase.Content.ToIntDef(0);
                        }
                        else if (xBase.isTag("dps"))
                        {
                            Debug.Assert(curLevel != -1);
                            Debug.Assert((curClass > -1) && (curClass < nItemClasses));
                            pi = xBase.Value.ToIntDef(1);
                            if (pi > 0)
                                arItemClasses[curClass].arSubClasses[curLevel].arValue[pi - 1] = (int)Utils.Str2Double(xBase.Content) * 10;
                        }
                        else if (xBase.isTag("level"))
                        {
                            Debug.Assert(curLevel != -1);
                            Debug.Assert((curClass > -1) && (curClass < nItemClasses));
                            pi = xBase.Value.ToIntDef(1);
                            if (pi > 0)
                                arItemClasses[curClass].arSubClasses[curLevel].arValue[pi - 1] = xBase.Content.ToIntDef(0);
                        }
                        else if (xBase.isTag("speed"))
                        {
                            Debug.Assert(curLevel != -1);
                            Debug.Assert((curClass > -1) && (curClass < nItemClasses));
                            arItemClasses[curClass].arSubClasses.Array[curLevel].iSpeed = (int)Utils.Str2Double(xBase.Content) * 10;
                        }
                        else if (xBase.isTag("damage"))
                        {
                            Debug.Assert((curClass > -1) && (curClass < nItemClasses));
                            if (curLevel == -1)
                            {	// Wenn wir in noch keiner Subclass sind, dann diesen Wert
                                // als Default für die Subclasses
                                defaultDamage = GetDamageType(xBase.Content);
                            }
                            else
                                arItemClasses[curClass].arSubClasses.Array[curLevel].idDamage = GetDamageType(xBase.Content);
                        }
                        break;
                    case xGroup.GROUP_DAMAGETYPES:
                        if (xBase.isTag("damagetype"))
                        {
                            curClass = -1;
                            for (int i = 0; i < nDamageTypes; i++)
                            {
                                if (arDamageTypes[i].id == xBase.Value) curClass = i;
                            }
                            if (curClass == -1)
                            {
                                curClass = nDamageTypes;
                                nDamageTypes++;
                                arDamageTypes.Length = nDamageTypes;
                                arDamageTypes.Array[curClass].id = xBase.Value;
                            }
                        }
                        if (xBase.isTag("name"))
                        {
                            arDamageTypes.Array[curClass].Name = xBase.Content;
                        }
                        break;
                    case xGroup.GROUP_INGREDIENTS:
                        if (xBase.isTag("dust") || xBase.isTag("liquid"))
                        {	// Steht derStoff schon im Array?
                            curClass = -1;
                            for (int i = 0; i < nIngredients; i++)
                            {
                                if (arIngredients[i].id == xBase.AttributeValue["id"]) curClass = i;
                            }
                            if (curClass == -1)
                            {
                                curClass = nIngredients;
                                nIngredients++;
                                arIngredients.Length = nIngredients;
                                arIngredients.Array[curClass].id = xBase.AttributeValue["id"];
                                arIngredients.Array[curClass].Name = "!Kein Inhaltsstoffname!";
                                arIngredients.Array[curClass].Price = 0;
                                arIngredients.Array[curClass].bDust = xBase.isTag("dust");
                            }
                        }
                        if (xBase.isTag("name"))
                            arIngredients.Array[curClass].Name = xBase.Content;
                        if (xBase.isTag("price"))
                            arIngredients.Array[curClass].Price = xBase.Content.ToIntDef(0);
                        break;
                    case xGroup.GROUP_PRICEMODEL:
                        if (xBase.isTag("general_markup"))
                            sPriceModel.pGeneralMarkup = (int)Utils.Str2Double(xBase.Content) * 10;
                        if (xBase.isTag("gem"))
                            sPriceModel.PPGem = xBase.Content.ToIntDef(0);
                        if (xBase.isTag("item"))
                            sPriceModel.PPItem = xBase.Content.ToIntDef(0);
                        if (xBase.isTag("order"))
                            sPriceModel.PPOrder = xBase.Content.ToIntDef(0);
                        if (xBase.isTag("level"))
                            sPriceModel.PPLevel = xBase.Content.ToIntDef(0);
                        if (xBase.isTag("ip"))
                            sPriceModel.PPIP = xBase.Content.ToIntDef(0);
                        if (xBase.isTag("oc"))
                            sPriceModel.PPOC = xBase.Content.ToIntDef(0);
                        if (xBase.isTag("hour"))
                            sPriceModel.PPHour = xBase.Content.ToIntDef(0);
                        if (xBase.isTag("quality"))
                            sPriceModel.pGQMarkup[xBase.AttributeValue["value"].ToIntDef(94) - 94] = (int)Utils.Str2Double(xBase.Content) * 10;
                        if (xBase.isTag("tier"))
                            sPriceModel.PPGTier[xBase.AttributeValue["level"].ToIntDef(0)] = xBase.Content.ToIntDef(0);
                        if (xBase.isTag("include_quality"))
                            sPriceModel.iGQMarkup = true;
                        if (xBase.isTag("include_tier"))
                            sPriceModel.iPPGTier = true;
                        if (xBase.isTag("include_cost"))
                            sPriceModel.iCost = true;
                        break;
                    case xGroup.GROUP_ITEMUPDATE:
                        {	// Hier wird mal nichts gespeichert
                            if (xBase.isTag("update_url"))
                            {	// Entferne noch ein eventuell führendes http://
                                ItemUpdate.URL = xBase.Content;
                            }
                            if (xBase.isTag("registry"))
                                ItemUpdate.Registry = xBase.Content;
                            // Website-Tag kann ich ignorieren momentan
                            if (xBase.isTag("name"))
                                ItemUpdate.Name = xBase.Content;
                            if (xBase.isTag("url"))
                                ItemUpdate.Website = xBase.Content;
                        }
                        break;
                }
                bReturn = xBase.NextTag();
            }
            xBase.CloseXml();
            return bReturn;
        }

        // Aufruf nachdem alle config-xml-Dateien geladen sind
        public void PostLoad()
        {
            int i;

            for (i = 0; i < arAttributes.Length; i++)
            {
                arAttributes.Array[i].iCapRef = -1;
                arAttributes.Array[i].iOvercapRef = -1;
            }

            for (i = 0; i < arAttributes.Length; i++)
                if (arAttributes[i].CapId >= 0)		// Test auf Cap-Erhöhung
                    if (arAttributes[i].bCapAttr)
                        arAttributes.Array[arAttributes[i].CapId].iOvercapRef = i;
                    else
                        arAttributes.Array[arAttributes[i].CapId].iCapRef = i;
        }

        // Hier werden u.a. die Cap/Overcap-Referenzen
        // der Attribute ermittelt
        public int GetRaceId(String RaceId)
        {   // liefert die id des übergebenen Rassen-Strings oder des Rassennamens
            // 0 wenn nicht gefunden
            for (int i = 0; i < nRaces; i++)
            {
                if ((arRaces[i].id == RaceId) || (arRaces[i].Name == RaceId))
                    return i;
            }
            return 0;
        }

        // Gibt die Integer id des übergebenen Klassennamen oder Klassenid-Strings
        public int GetClassId(String ClassName)
        {
            for (int i = 0; i < nClasses; i++)
            {
                if ((arClasses[i].Name == ClassName) || (arClasses[i].id == ClassName))
                    return i;
            }
            return -1;
        }

        // Gibt die Integer id des übergebenen Attributid-Strings
        public int GetAttributeId(String sAttribute)
        {
            for (int i = 0; i < nAttributes; i++)
            {
                if ((arAttributes[i].id == sAttribute) || (arAttributes[i].Name == sAttribute))
                    return i;
            }
            return -1;
        }

        // Gibt die Integer id des übergebenen Groupid- oder Namen-Strings
        public int GetGroupId(String sGroup)
        {
            for (int i = 0; i < nGroups; i++)
            {
                if ((arGroups[i].id == sGroup) || (arGroups[i].Name == sGroup))
                    return i;
            }
            return -1;
        }

        // Gibt die Integer id der übergebenen Bonusid- oder Namen-Strings
        public int GetBonusId(String sBonus)
        {
            if (sBonus.Length > 0)
            {
                for (int i = 0; i < nBonuses; i++)
                {
                    if ((arBonuses[i].id == sBonus) || (arBonuses[i].KortId == sBonus)
                     || (arBonuses[i].bCraftable && (arBonuses[i].GemId == sBonus)))
                        return i;
                    for (int j = 0; j < arBonuses[i].nNames; j++)
                        if (arBonuses[i].Names[j] == sBonus)
                            return i;
                }
            }
            return -1;
        }

        // Gibt die Integer id der übergebenen Bonusnamen-Strings,
        // wenn auch die Prozent-Fähigkeit stimmt
        public int GetBonusId(String sBonus, bool bPercent)
        {
            for (int i = 0; i < nBonuses; i++)
            {
                if (arBonuses[i].bPercent == bPercent)
                {
                    for (int j = 0; j < arBonuses[i].nNames; j++)
                        if (arBonuses[i].Names[j] == sBonus)
                            return i;
                }
            }
            return -1;
        }

        // Gibt die Integer id der übergebenen Bonusnamen-Strings,
        // wenn auch das übergebene Realm überein stimmt
        public int GetBonusId(String sBonus, int Realm)
        {
            for (int i = 0; i < nBonuses; i++)
            {
                if ((arBonuses[i].iRealm & Realm) != 0)
                {
                    if (arBonuses[i].id == sBonus)
                        return i;
                }
            }
            return -1;
        }

        // Gibt die Integer id der übergebenen Materialid zurück
        public int GetMaterialId(String sMaterial)
        {
            for (int i = 0; i < nMaterials; i++)
            {
                if (arMaterials[i].id == sMaterial)
                    return i;
            }
            return -1;
        }

        // Gibt die Integer id der übergebenen ItemClass-Namen oder -ID zurück
        public int GetItemClassId(String sItemClass, bool IncludeRealm = false)
        {
            for (int i = 0; i < nItemClasses; i++)
            {
                if ((!IncludeRealm || (arItemClasses[i].iRealm & iRealm) != 0) && ((arItemClasses[i].Name == sItemClass) || (arItemClasses[i].id == sItemClass)))
                    return i;
            }
            return -1;
        }

        // Diese Funktion liefert zu einer übergebenen AF und ItemClass die Werte
        // für SubClass und Materialstufe, welche dem AF am nächsten kommen
        // Der Rückgabewert ist der Abstand vom projektierten AF
        public int GetItemClassAF(int AF, int ItemClass, out int SubClass, out int MaterialLevel)
        {
            int minDist = 100;
            int sub = 0, mat = 0;
            for (int i = 0; i < arItemClasses[ItemClass].arSubClasses.Length; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    int val = arItemClasses[ItemClass].arSubClasses[i].arValue[j];
                    if ((val > 0) && (val - AF >= 0) && (val - AF < minDist))
                    {
                        minDist = val - AF;
                        sub = i + 1;
                        mat = j;
                    }
                }
            }
            if (minDist == 100)
            {	// Wenn wir keinen Wert gefunden haben, dann die höchste Subclass mit höchstem Level nehmen
                sub = arItemClasses[ItemClass].arSubClasses.Length;
                mat = 9;
            }
            SubClass = sub;
            MaterialLevel = mat;
            return minDist;
        }

        // Gibt die Integer id des übergebenen ItemSubClass-Namens zurück
        public int GetItemSubClassId(int ClassId, String sItemSubClass)
        {
            if (ClassId >= 0)
            {
                for (int i = 0; i < arItemClasses[ClassId].arSubClasses.Length; i++)
                {
                    if ((arItemClasses[ClassId].iRealm & iRealm) != 0
                    && (arItemClasses[ClassId].arSubClasses[i].id == sItemSubClass))
                        return i;
                }
            }
            return -1;
        }

        // Diese Funktion liefert für den übergebenen String die zugehörigen DamageID
        public int GetDamageType(String DamageType)
        {
            for (int i = 0; i < nDamageTypes; i++)
            {
                if (arDamageTypes[i].id == DamageType)
                    return i;
            }
            return -1;
        }

        // Diese Funktion liefert für den übergebenen String die zugehörigen IngredientID
        public int GetIngredientId(String Name)
        {
            for (int i = 0; i < nIngredients; i++)
            {
                if (arIngredients[i].id == Name)
                    return i;
            }
            return -1;
        }

        // Diese Funktion liefert für den übergebenen String die zugehörigen Slotposition
        public int GetSlotPosition(String Position)
        {
            for (int i = 0; i < CPlayer.PLAYER_ITEMS; i++)
            {
                if ((arItemSlots[i].strPosId == Position)
                || (arItemSlots[i].strPosClass == Position)
                || (arItemSlots[i].strKortId == Position))
                    return i;
            }
            return -1;
        }

        // Speichere das Preismodell in die angegebene Datei
        // Menuindex ist dabei der Index im Feld arMenuItems
        public bool SavePriceModel(int MenuIndex)
        {
            CXml xBase = new CXml();
            bool iReturn = xBase.OpenSaveXml(arMenuItems[MenuIndex].FileName);
            if (iReturn)
            {
                xBase.OpenTag("moras", "", "version=\"1.0\"");
                xBase.OpenTag("option", arMenuItems[MenuIndex].Name,
                    "group=\"" + arMenuItems[MenuIndex].Group + "\" pricing=\"true\"");
                xBase.CloseTag();
                xBase.OpenTag("pricemodel", "", "");
                xBase.OpenTag("general_markup", (sPriceModel.pGeneralMarkup * 0.1).ToString("###0.0"), ""); xBase.CloseTag();
                xBase.OpenTag("gem", sPriceModel.PPGem.ToString(), ""); xBase.CloseTag();
                xBase.OpenTag("item", sPriceModel.PPItem.ToString(), ""); xBase.CloseTag();
                xBase.OpenTag("order", sPriceModel.PPOrder.ToString(), ""); xBase.CloseTag();
                xBase.OpenTag("level", sPriceModel.PPLevel.ToString(), ""); xBase.CloseTag();
                xBase.OpenTag("ip", sPriceModel.PPIP.ToString(), ""); xBase.CloseTag();
                xBase.OpenTag("oc", sPriceModel.PPOC.ToString(), ""); xBase.CloseTag();
                xBase.OpenTag("hour", sPriceModel.PPHour.ToString(), ""); xBase.CloseTag();
                for (int i = 0; i < 7; i++)
                {
                    xBase.OpenTag("quality", (sPriceModel.pGQMarkup[i] * 0.1).ToString("###0.0"), "value=\"" + (i + 94).ToString() + "\"");
                    xBase.CloseTag();
                }
                if (sPriceModel.iGQMarkup)
                    xBase.EmptyTag("include_quality");
                for (int i = 0; i < 10; i++)
                {
                    xBase.OpenTag("tier", sPriceModel.PPGTier[i].ToString(), "level=\"" + (i).ToString() + "\"");
                    xBase.CloseTag();
                }
                if (sPriceModel.iPPGTier)
                    xBase.EmptyTag("include_tier");
                if (sPriceModel.iCost)
                    xBase.EmptyTag("include_cost");
                xBase.CloseTag();	// Pricemodell
                xBase.CloseTag();	// Moras
                xBase.CloseXml();
            }
            return iReturn;
        }

        public String GetExtensionId(int Index)
        {
            return arExtensions[Index].id;
        }

        public int GetExtensionId(String Id)
        {
            int result;
            for (result = 0; result < nExtensions; result++)
            {
                if (arExtensions[result].id == Id)
                    break;
            }

            return result;
        }

        // Cinnean 21.09.07
        // Dieser Funktion gehört eher zu Config als zu Player!
        // Liefert wahr, wenn Attribut von der Klasse benutzt werden kann
        public bool AttributeUseableByClass(int AttributeID, int ClassID)
        {
            bool bReturn = true;
            // Bei Caperhöhungen an Grundstats testen
            int aid = (arAttributes[AttributeID].CapId >= 0)
             ? arAttributes[AttributeID].CapId
             : ((arAttributes[AttributeID].SkillId >= 0)
                  ? arAttributes[AttributeID].SkillId
                  : AttributeID);
            // Wenn das Attribut ein Skill ist, dann nur bestimmen,
            // wenn die Klasse diesen Skill hat
            if (arAttributes[aid].capadd == 5)
            {	// Capadd 5 heißt, das es ein skill ist
                // Und in der Klasse schauen, ob sie den hat
                bReturn = false;
                for (int i = 0; i < arClasses[ClassID].nSkills; i++)
                {
                    if (arClasses[ClassID].arSkills[i] == aid)
                        bReturn = true;
                }
            }
            // Magieart (Die DisplayIDs vielleicht noch verallgemeinern
            if ((arClasses[ClassID].iMagic != aid)
             && (arAttributes[aid].displayid == 4))
                bReturn = false;
            // Spielt Magiekraft ne Rolle?
            if ((arAttributes[aid].displayid == 6)
             && (arClasses[ClassID].iMagic == -1))
                bReturn = false;
            return bReturn;
        }

        public int iRealm;									// Aktuell gewähltes Realm
        public String strVersion;						// Version der config.xml
        public String strLanguage;
        public String sysLanguage;						// Systemsprache bevor sie geändert wird
        public int iLanguage;
        public int iSCLevel;								// Level des Bannzauberers
        public bool bHalfIP;								// Halbe Imbue-Punkte beim Bannzaubern
        public bool bKortEnglish;							// Namen von Kort & Leladia Dateien als Orignalnamen importieren
        public bool bIgnoreRaceBoni;						// Ignoriere Rassenboni bei der Resistenzen-Berechnung
        public bool bDebugStartup;							// Schreibt debug infos über den start
        public bool bDebugSQL;								// Schreibt die SQLs
        public TStringList strServers;
        public int nServers;
        public DynamicArray<int> arServerType;			// Ein Array mit dem Servertypen
        public DynamicArray<SRace> arRaces;				// Das Array mit den Rassendaten
        public int nRaces;									// Anzahl der Rassen
        public DynamicArray<SClass> arClasses;			// Das Array mit den Klassendaten
        public int nClasses;								// Anzahl der Klassen
        public DynamicArray<SAttribute> arAttributes;
        public int nAttributes;
        public DynamicArray<SGroups> arGroups;
        public int nGroups;
        public DynamicArray<SBonus> arBonuses;
        public int nBonuses;
        public DynamicArray<SMaterial> arMaterials;
        public int nMaterials;
        public DynamicArray<SExtension> arExtensions;
        public int nExtensions;
        public DynamicArray<SItemClass> arItemClasses;
        public int nItemClasses;
        public DynamicArray<SDamageType> arDamageTypes;
        public int nDamageTypes;
        public DynamicArray<SDustsNLiquids> arIngredients;
        public int nIngredients;
        public DynamicArray<SOptionMenuItem> arMenuItems;
        public SItemSlot[] arItemSlots = new SItemSlot[CPlayer.PLAYER_ITEMS];
        public SSCPriceModel sPriceModel;	// Struktur fürs Preismodell
        public TStringList strInterfaceElements;	// Eine Liste der Interface-Elemente, deren Caption localisiert ist
        public TStringList strInterfaceName;	// Die Captions der Elemente
        public SItemUpdate ItemUpdate;	// Strukur fürs Item-Update
    }
}
