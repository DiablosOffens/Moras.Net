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
// Dieses Modul enthält alle Itemdaten, getrennt nach Drop und Unique/Crafted
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using DelphiClasses;
using System.Windows.Forms;
using System.IO;
using System.Deployment.Application;
using dxgettext;

namespace Moras.Net
{
    public struct SIItem
    {
        public CItem Item;
        public bool bUpdated;	// Zeigt an, das DB-Item neuer als das private Item ist
        public bool bPrivate;	// Item wurde von User mal geändert
        public CPlayer pPlayer;	// Ist Item Player zugeordnet, und welchem
    };

    public class CMDataBase
    {
        private static string _(string szMsgId) { return TGnuGettextInstance.gettext(szMsgId); }

        public CMDataBase()	// Konstruktor
        {	// Dateinamen sind schonmal voreingestellt
            string sPath;
            if (ApplicationDeployment.IsNetworkDeployed)
                sPath = Utils.IncludeTrailingPathDelimiter(ApplicationDeployment.CurrentDeployment.DataDirectory);
            else
                sPath = Utils.IncludeTrailingPathDelimiter(Path.GetDirectoryName(Application.ExecutablePath));
            sFileNameDrops = sPath + "Items.xml";
            sFileNameOthers = sPath + "Items.xml";
            nItems = 0;
            arItems.Length = 0;
        }

        // Initialisieren und laden der Datenbank
        // Einen richtigen Fehlschlag gibts noch nicht, deshalb immer 0 zurück geben
        public int Init()	// Initialisieren und laden der DB
        {
            //    cItems = GetRegistryInteger("CountItems", 0);
            //	OpenXml(sFileNameDrops);
            //    cItems = GetRegistryInteger("CountItemsP", 0);
            //	OpenXml(sFileNameOthers, true);
            // Einen richtigen Fehlschlag gibts noch nicht, deshalb immer 0 zurück geben
            return 0;
        }

        // ProgressBar im Splash mit Werten belegen
        private void initPB()
        {
            int count;

            if (Unit.frmSplash != null)
            {
                Unit.frmSplash.pbLoad.Value = 0;
                if (cItems == 0)
                    Unit.frmSplash.pbLoad.Maximum = (xFile.FileSize / 455); // 440 bytes ist der mögliche durchschnitt für einen xml datensatz
                else
                    Unit.frmSplash.pbLoad.Maximum = cItems;
            }
        }

        // Diese Funktion tested, ob in einer xml-Datei Items sind
        // und lädt diese dann
        // Das Private gibt an, ob die persönliche Itemdatei geladen wird
        public bool OpenXml(string sFileName, bool Private = false)	// Lade eine angegebene Xml-Datei
        {
            int n = 0;
            bool ret;

            if (xFile.OpenXml(sFileName))
            {	// Datei konnte geöffnet werden
                initPB();
                if (xFile.isTag("daoc_items"))
                {
                    CItem Item = new CItem();
                    n = 0;
                    do
                    {
                        ret = Item.Load(xFile);
                        Item.bEquipped = true;	// Items in der Datenbank gelten alle als equipped
                        if (n > 0)
                        {
                            int ii = GetItemUID(Item.iUID);	// Die suche ist eigentlich erst nötig, wenn Private = true ist
                            if (ii < 0)	// Item-UID nicht gefunden, also neu
                            {	// Wenn es kein Bezugsitem für ein Gelöschtes gibt, dann nicht adden
                                if (!Item.Deleted)
                                    AddItem(Item);
                            }
                            else
                            {	// uid gibts
                                // Spezialbehandlung für gelöschte Items
                                if (Item.Deleted)
                                    arItems[ii].Value.Item.Deleted = true;
                                else
                                {	// Zeitstempel vergleichen
                                    if (arItems[ii].Value.Item.LastUpdate <= Item.LastUpdate)
                                    {	// Privates Item ist neuer
                                        arItems[ii].Value.Item = Item;
                                        arItems[ii].Value.bPrivate = Private;
                                    }
                                    else
                                    {	// In Item-DB ist ein neueres oder gleichaltes Item wie das vom Spieler
                                        // Nochmal checken, ob es vielleicht genau das gleiche ist
                                        if (arItems[ii].Value.Item == Item)
                                        {	// Ist genau das gleiche Item. Nicht adden zu privates
                                        }
                                        else
                                        {	// Nicht das gleiche Item
                                            // Als neues Item anlegen und das in DB als updated markieren
                                            arItems[ii].Value.bUpdated = true;
                                            AddItem(Item);
                                        }
                                    }
                                }
                            }
                        }
                        n++;
                        if ((Unit.frmSplash != null) && ((n % 100) == 0))
                        {
                            Unit.frmSplash.pbLoad.Value = n;
                            if (Private)
                                Unit.frmSplash.lbInfo.Text = "Lade persönliche Itemdaten.. (" + (n).ToString() + " geladen)";
                            else
                                Unit.frmSplash.lbInfo.Text = _("Lade Itemdatenbank.. (") + (n).ToString() + _(" geladen)");
                            Application.DoEvents();
                        }
                    } while (ret == true);
                }
                xFile.CloseXml();
                if (Private)
                    Utils.SetRegistryInteger("CountItemsP", n);
                else
                    Utils.SetRegistryInteger("CountItems", n);
                return true;
            }
            else return false;
        }

        // Speichere die Itemdatei, wenn wenigstens ein Item geändert wurde
        // Speichert nur die Private-Items-Datei
        public bool Save()	// Speichere die DB, wenn was geändert wurde
        {
            Unit.frmMain.ZConnection.DoCommit();
            return true;
        }

        // Testet das Übergebene Item
        // Liefert die ID in der Datenbank, wenn es ein Update wäre,
        // -1 wenn es nicht in der Datenbank ist,
        // und -2, wenn es absolut identisch mit einem Item der Datenbank ist
        public int CheckItem(CItem Item)	// Teste auf Doubletten und so Zeug
        {
            int i, TempPos;

            TempPos = Unit.xml_config.GetSlotPosition(Unit.xml_config.arItemSlots[Item.Position].strPosClass);

            Unit.frmMain.ZQuery.SetActive(false);
            string sql = "select id from items where";
            sql += " name = " + Utils.QuotedStr(Item.Name, '\'');
            if (Item.Provider != "")
                sql += " and provider = " + Utils.QuotedStr(Item.Provider, '\'');
            else
                sql += " and (provider = '' or provider is null)";
            sql += " and position = " + (TempPos).ToString();
            sql += " and realm = " + (Item.Realm).ToString();
            sql += " and class = " + (Item.Class).ToString();
            sql += " and subclass = " + (Item.SubClass).ToString();
            Unit.frmMain.ZQuery.CommandText = sql;
            //    DebugPrint(sql.c_str());
            Unit.frmMain.ZQuery.SetActive(true);
            if (Unit.frmMain.ZQuery.GetRecordCount() > 0)
            {
                return Unit.frmMain.ZQuery.FieldByName("id").AsInteger;
            }

            return -1;
        }

        // Das übergebene Item wird zur Database hinzugefügt
        // Achtung, es wird nicht geprüft, ob das Item schon einmal in der Datenbank ist
        // Dafür vorher testen
        // gibt die Id in der DB zurück
        public int AddItem(CItem Item)
        {
            if (Item.Position < 0)
                Utils.AskForPosition(Item);

            int TempPos = Unit.xml_config.GetSlotPosition(Unit.xml_config.arItemSlots[Item.Position].strPosClass);

            Unit.frmMain.ZQuery.SetActive(false);
            string sql = "insert into items ";
            sql = sql + "(name, nameoriginal, origin, description, onlineurl, extension, provider, classrestrictions, effects, realm, position, type, level, quality, bonus, class, subclass, material, af, dps, speed, damagetype, maxlevel, lastupdate)";
            sql = sql + " values (" + Utils.QuotedStr(Item.Name, '\'');
            sql = sql + ", " + Utils.QuotedStr(Item.NameOriginal, '\'');
            sql = sql + ", " + Utils.QuotedStr(Item.Origin, '\'');
            sql = sql + ", " + Utils.QuotedStr(Item.Description, '\'');
            sql = sql + ", " + Utils.QuotedStr(Item.OnlineURL, '\'');
            sql = sql + ", " + Utils.QuotedStr(Item.Extension, '\'');
            sql = sql + ", " + Utils.QuotedStr(Item.Provider, '\'');
            sql = sql + ", " + Utils.QuotedStr(Item.GetClassRestrictionStr(), '\'');
            sql = sql + ", " + Utils.QuotedStr(Item.GetEffectStr(), '\'');
            sql = sql + ", " + (Item.Realm).ToString();
            sql = sql + ", " + (TempPos).ToString();
            sql = sql + ", " + ((int)Item.Type).ToString();
            sql = sql + ", " + (Item.Level).ToString();
            sql = sql + ", " + (Item.Quality).ToString();
            sql = sql + ", " + (Item.Bonus).ToString();
            sql = sql + ", " + (Item.Class).ToString();
            sql = sql + ", " + (Item.SubClass).ToString();
            sql = sql + ", " + (Item.Material).ToString();
            sql = sql + ", " + (Item.AF).ToString();
            sql = sql + ", " + (Item.DPS).ToString();
            sql = sql + ", " + (Item.Speed).ToString();
            sql = sql + ", " + (Item.DamageType).ToString();
            sql = sql + ", " + (Item.MaxLevel).ToString();
            sql = sql + ", " + (Utils.DateTimeToUnix(Item.LastUpdate)).ToString() + ")";
            Unit.frmMain.ZQuery.CommandText = sql;
            Unit.frmMain.ZQuery.ExecuteNonQuery();

            return 0;
        }

        // Lösche ein Item aus der DB.
        // Das geschieht einfach dadurch, das alle nachfolgenden Items nachgerutsch werden
        // Richtig gelöscht werden nur private Items. DB-Items werden nur gelöscht markiert
        public void DeleteItem(int index)
        {
            Unit.frmMain.ZQuery.SetActive(false);
            Unit.frmMain.ZQuery.CommandText = "delete from items where id = " + (index).ToString();
            Unit.frmMain.ZQuery.ExecuteNonQuery();
        }

        // Lösche ein Item aus der DB.
        // Das geschieht einfach dadurch, das alle nachfolgenden Items nachgerutsch werden
        // Richtig gelöscht werden nur private Items. DB-Items werden nur gelöscht markiert
        public void DeleteItem(CItem Item)
        {
            // Suche das Item in der DB
            int i = GetItemIndex(Item);
            DeleteItem(i);
        }

        public CItem GetItem(SQLiteCommand ZQuery)    // Liefert ein Item anhand eines aktuellen Datensatzes
        {
            CItem item = new CItem();
            item.Init();

            item.Name = ZQuery.FieldByName("name").AsString;
            item.NameOriginal = ZQuery.FieldByName("nameoriginal").AsString;
            item.Origin = ZQuery.FieldByName("origin").AsString;
            item.Description = ZQuery.FieldByName("description").AsString;
            item.OnlineURL = ZQuery.FieldByName("onlineurl").AsString;
            item.Extension = ZQuery.FieldByName("extension").AsString;
            item.Provider = ZQuery.FieldByName("provider").AsString;
            item.SetClassRestrictionStr(ZQuery.FieldByName("classrestrictions").AsString);
            item.SetEffectStr(ZQuery.FieldByName("effects").AsString);
            item.Realm = ZQuery.FieldByName("realm").AsInteger;
            item.Position = ZQuery.FieldByName("position").AsInteger;
            item.Type = (EItemType)ZQuery.FieldByName("type").AsInteger;
            item.Level = ZQuery.FieldByName("level").AsInteger;
            item.Quality = ZQuery.FieldByName("quality").AsInteger;
            item.Bonus = ZQuery.FieldByName("bonus").AsInteger;
            item.Class = ZQuery.FieldByName("class").AsInteger;
            item.SubClass = ZQuery.FieldByName("subclass").AsInteger;
            item.Material = ZQuery.FieldByName("material").AsInteger;
            item.AF = ZQuery.FieldByName("af").AsInteger;
            item.DPS = ZQuery.FieldByName("dps").AsInteger;
            item.Speed = ZQuery.FieldByName("speed").AsInteger;
            item.DamageType = ZQuery.FieldByName("damagetype").AsInteger;
            item.MaxLevel = ZQuery.FieldByName("maxlevel").AsInteger;
            if (item.MaxLevel > 0)
                item.CurLevel = item.MaxLevel;
            item.LastUpdate = Utils.UnixToDateTime(ZQuery.FieldByName("lastupdate").AsInteger);

            return item;
        }

        // Gib das Item mit dem angegebenem index zurück
        public CItem GetItem(int index)          // Liefert ein Item anhand des Index
        {
            CItem item;
            Unit.frmMain.ZQuery.SetActive(false);
            Unit.frmMain.ZQuery.CommandText = "select * from items where id = " + (index).ToString();
            Unit.frmMain.ZQuery.SetActive(true);

            item = GetItem(Unit.frmMain.ZQuery);

            return item;
        }

        public void UpdateItem(int index, CItem Item)
        {
            int TempPos = Unit.xml_config.GetSlotPosition(Unit.xml_config.arItemSlots[Item.Position].strPosClass);

            Unit.frmMain.ZQuery.SetActive(false);
            string sql = "update items set";
            sql = sql + " name = " + Utils.QuotedStr(Item.Name, '\'');
            sql = sql + ", nameoriginal = " + Utils.QuotedStr(Item.NameOriginal, '\'');
            sql = sql + ", origin = " + Utils.QuotedStr(Item.Origin, '\'');
            sql = sql + ", description = " + Utils.QuotedStr(Item.Description, '\'');
            sql = sql + ", onlineurl = " + Utils.QuotedStr(Item.OnlineURL, '\'');
            sql = sql + ", extension = " + Utils.QuotedStr(Item.Extension, '\'');
            sql = sql + ", provider = " + Utils.QuotedStr(Item.Provider, '\'');
            sql = sql + ", classrestrictions = " + Utils.QuotedStr(Item.GetClassRestrictionStr(), '\'');
            sql = sql + ", effects = " + Utils.QuotedStr(Item.GetEffectStr(), '\'');
            sql = sql + ", realm = " + (Item.Realm).ToString();
            sql = sql + ", position = " + (TempPos).ToString();
            sql = sql + ", type = " + ((int)Item.Type).ToString();
            sql = sql + ", level = " + (Item.Level).ToString();
            sql = sql + ", quality = " + (Item.Quality).ToString();
            sql = sql + ", bonus = " + (Item.Bonus).ToString();
            sql = sql + ", class = " + (Item.Class).ToString();
            sql = sql + ", subclass = " + (Item.SubClass).ToString();
            sql = sql + ", material = " + (Item.Material).ToString();
            sql = sql + ", af = " + (Item.AF).ToString();
            sql = sql + ", dps = " + (Item.DPS).ToString();
            sql = sql + ", speed = " + (Item.Speed).ToString();
            sql = sql + ", damagetype = " + (Item.DamageType).ToString();
            sql = sql + ", maxlevel = " + (Item.MaxLevel).ToString();
            sql = sql + ", lastupdate = " + (Utils.DateTimeToUnix(Item.LastUpdate)).ToString();
            sql = sql + " where id = " + (index).ToString();
            Unit.frmMain.ZQuery.CommandText = sql;
            Unit.frmMain.ZQuery.ExecuteNonQuery();
        }

        // Gib das Item mit dem angegebenem index zurück (DB-Format)
        public ValueRef<SIItem> GetSItem(int index)	// Liefere das Item im DB-Format
        {
            if ((index >= 0) && (index < nItems))
                return arItems[index];
            else
                return null;
        }

        // Liefere den Index des Items
        // -1 wenn nicht gefunden
        public int GetItemIndex(CItem Item)	// Liefere den Index des Items
        {
            return CheckItem(Item);
        }

        // Liefere den Index des Items, mit das angegebenen uid
        // Bei uid = 0 oder nicht gefunden, wird -1 als Ergebnis geliefert
        private int GetItemUID(int UID)	// Liefere den Index des Items, mit das angegebenen uid
        {
            if (UID > 0)
            {
                for (int i = 0; i < nItems; i++)
                {	// Darf kein getragenes Item sein (Drop müsste es eh sein, wenn es ne uid hat)
                    if ((arItems[i].Value.pPlayer == null) && arItems[i].Value.Item.iUID == UID) return i;
                }
            }
            return -1;
        }

        // Ist das angegebene Item eins von einem Spieler?
        private bool GetbPlayer(int index)
        {
            if ((index >= 0) && (index < nItems))
                return (arItems[index].Value.pPlayer != null);
            else
                return false;
        }

        // Welchem Spieler ist das Item zugeordnet?
        private CPlayer GetPlayer(int index)
        {
            if ((index >= 0) && (index < nItems))
                return arItems[index].Value.pPlayer;
            else
                return null;
        }

        // Ist das angegebene Item ein Privates?
        private bool GetbPrivate(int index)
        {
            if ((index >= 0) && (index < nItems))
                return arItems[index].Value.bPrivate;
            else
                return false;
        }

        // Liefert Anzahl der Dropitems in der Datenbank
        public int GetNDrops()	// Liefert Anzahl der Dropitems in der Datenbank
        {
            string sql = "select count(*) as cnt from items where type = " + ((int)EItemType.Drop).ToString();
            if (Unit.frmMain != null)
            {
                Unit.frmMain.ZQuery.SetActive(false);
                Unit.frmMain.ZQuery.CommandText = sql;
                Unit.frmMain.ZQuery.SetActive(true);
            }
            return Unit.frmMain.ZQuery.FieldByName("cnt").AsInteger;
        }

        // Verwalte das Array in 100er oder 1000er Schritten
        private int nItems;	// Anzahl der Items in der DB
        private int cItems; // für evtl Hochrechnung der Items im Splash
        private int dropItems; // Counter für die Master-Items
        private int otherItems; // Counter für restliche Items
        private CXml xFile;	// Das xml-Ojekt
        private string sFileNameDrops;	// Dateiname der Datei mit den Dropgegenständen
        private string sFileNameOthers;	// Dateiname für die anderen Gegenstände
        private DynamicArray<ValueRef<SIItem>> arItems;

        public int NItems { get { return GetNDrops(); } }
        private IndexerProperty<bool, int> _isPlayerItem;
        public IndexerProperty<bool, int> isPlayerItem { get { return _isPlayerItem ?? (_isPlayerItem = new IndexerProperty<bool, int> { read = GetbPlayer }); } }
        private IndexerProperty<CPlayer, int> _player;
        public IndexerProperty<CPlayer, int> Player { get { return _player ?? (_player = new IndexerProperty<CPlayer, int> { read = GetPlayer }); } }
        IndexerProperty<bool, int> _isPrivate;
        public IndexerProperty<bool, int> isPrivate { get { return _isPrivate ?? (_isPrivate = new IndexerProperty<bool, int> { read = GetbPrivate }); } }
    }
}
