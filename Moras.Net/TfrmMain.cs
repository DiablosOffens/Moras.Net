using DelphiClasses;
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
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using System.IO;
    using System.Drawing.Design;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Runtime.Serialization;
    using System.Deployment.Application;
    using DelphiClasses;
    using Moras.Net.Components;
    using System.Data.SQLite;
    using dxgettext;
    using System.Threading;

    public partial class TfrmMain : TCustomForm
    {
        //---------------------------------------------------------------------------
        private static string _(string szMsgId) { return TGnuGettextInstance.gettext(szMsgId); }
        protected override IContainer GetChildContainer() { return components ?? base.GetChildContainer(); }

        static int iLastMouseOver;	// Über welchem Attribute-Feld war die Maus zuletzt
        static int iPopupSlot;	// Für welchen Slot wurde Popup ausgelöst
        static int iLastPlayer;

        #region Private fields
        private ToolStripMenuItem[] History = new ToolStripMenuItem[4];	// Ein Array der History-Menuitems
        private String[] HistoryFile = new String[4];	                //Ein Array der History-Dateinamen
        private TMItemSlot[] ItemSlot = new TMItemSlot[19];
        private TMIComboBox[] cbType = new TMIComboBox[15];
        private TMIComboBox[] cbEffect = new TMIComboBox[15];
        private TMIComboBox[] cbValue = new TMIComboBox[5];
        private TextBox[] tbValue = new TextBox[10];
        private TextBox[] teLevel = new TextBox[10];
        private TUpDown[] udLevel = new TUpDown[10];
        private TextBox[] tbRemakes = new TextBox[5];
        private TMIComboBox[] cbGemQuality = new TMIComboBox[5];
        private TRadioButton[] rbItemType = new TRadioButton[3];
        private TMIComboBox[] cbRestriction = new TMIComboBox[4];	// Comboboxen mit den Klassenbeschränkungen
        private TLabel[] lbRestriction = new TLabel[4];
        private TLabel[] lbIP = new TLabel[5];
        private TLabel[] lbMat = new TLabel[6];
        private TLabel[] lbMatLevel = new TLabel[6];
        private TLabel[] lbMatPrice = new TLabel[6];
        private ToolStripMenuItem[] mnSwap = new ToolStripMenuItem[6];
        private bool bNameMode;	// false = Name, true = Orginalname
        private int bLock;	// Wenn gesetzt, wird der Wert nicht wirklich geändert
        #endregion
        #region Public fields
        public int iActSlot;                                // Aktuell gewählter Itemslot
        public int iMajor;                                  // Versionsnummer
        public int iMinor;                                  // Versionsnummer Subversion
        public String Version;                              // Versionsnummer als String
        public String AppPath;                             // Das ist das Programverzeichnis
        public string DataPath;                             // Verzeichnis für veränderbare Daten
        public String CheckUpdateMD5;                       // MD5 prüfsumme der neuen version auf dem server
        public String CaptionBackup;                        // Speichern des Fenstertitels um Dateiname mit anzuzeigen
        public bool NewDatabase;                            // Flag ob die Datenbank neu angelegt wurde
        public TMCapView[] Attributes = new TMCapView[34];
        public CheckBox[] chWeapon = new CheckBox[4];       // Checkbox für Waffenaktivierung
        public CheckBox[] chDone = new CheckBox[5];
        public TUpDown[] udRemakes = new TUpDown[5];
        public TLabel[] lbGem = new TLabel[5];              // Hier wird die Bezeichnung des Bannzauberjuwelen angegeben
        public TLabel[] lbCost = new TLabel[5];
        #endregion

        // Kopiere die Daten aus der Player-struktur in die Form
        public void Player2Form()
        {
            bLock++;
            UpdateMainCaption(Unit.player.FileName);
            cbName.Text = Unit.player.Name;
            tbLevel.Text = Unit.player.Level.ToString();
            cbAccount.SelectData(Unit.player.Account);
            cbRealm.SelectData(Unit.player.Realm);
            cbRealmChange(null, EventArgs.Empty);
            cbServer.SelectedIndex = Unit.xml_config.strServers.IndexOf(Unit.player.Server) + 1;
            if (cbServer.SelectedIndex == -1) cbServer.SelectedIndex = 0;
            cbServerChange(null, EventArgs.Empty);	// Damit der Server auch als Voreingestellt gespeichert wird
            if (cbClass.SelectData(Unit.player.Class) == -1) cbClass.SelectedIndex = 0;
            cbClassChange(null, EventArgs.Empty);
            if (cbRace.SelectData(Unit.player.Race) == -1) cbRace.SelectedIndex = 0;
            for (int i = 0; i < CPlayer.ALL_ITEMS; i++)
            {
                UpdateItemSlot(i);
            }
            // Die aktivierungen der Waffenslots setzen
            for (int i = 0; i < 4; i++)
            {
                // Cinnean, 15.09.07
                // Sicherstellen, dass die Equips der Items und der Checkboxen stimmen
                // Start-Bug-Fix
                if (Unit.player.Item[i + 6].bEquipped && Unit.player.Item[i + 6].isEmpty())
                {
                    // Leere Waffen zu equipen macht kein Sinn
                    Unit.player.Item[i + 6].bEquipped = false;
                }
                chWeapon[i].Checked = Unit.player.Item[i + 6].bEquipped;
                chWeaponClick(chWeapon[i], EventArgs.Empty);  // Durch die letzten Änderungen macht dies wieder Sinn!
                // Start-Ende-Fix
            }
            bLock--;
            if (iActSlot < CPlayer.PLAYER_ITEMS)
                MItemSlotClick(ItemSlot[iActSlot], EventArgs.Empty);
        }

        //---------------------------------------------------------------------------
        public TfrmMain()
        {
            InitializeComponent();
            PageControl1.SelectedTab = TabSheet1;
            pcItem.SelectedTab = TabBoni;
            pcLoot.SelectedTab = TabInventory;
            tbMainmenu.Font = this.Font;
            GridInventory.RemoveRowHeaderIcons();
            GridTreasury.RemoveRowHeaderIcons();

            iActSlot = 0;
            bLock = 0;
            iLastMouseOver = -1;
            iLastPlayer = 0;
            toolTip1.AutoPopDelay = 40000;	// Zeige ein Hint-Popup für 30 Sekunden an
            toolTip1.ReshowDelay = 100;
            AppPath = Utils.IncludeTrailingPathDelimiter(Path.GetDirectoryName(Application.ExecutablePath));
            if (ApplicationDeployment.IsNetworkDeployed)
                DataPath = Utils.IncludeTrailingPathDelimiter(ApplicationDeployment.CurrentDeployment.DataDirectory);
            else
                DataPath = AppPath;
            // Bestimme die Programmversion
            {
                FileVersionInfo info = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
                iMajor = info.FileMajorPart;
                iMinor = info.FileMinorPart;
                Version = iMajor.ToString() + "." + iMinor.ToString();
            }
            if (Utils.GetRegistryInteger("ProcessPriority", 0) != 0)
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
            if (Utils.GetRegistryInteger("WikiHelp", 1) == 0)
                TApplication.Instance.HelpFile = "";
        }
        //---------------------------------------------------------------------------
        public void UpdateMainCaption(String AFilename)
        {
            if (AFilename != "")
                Text = CaptionBackup + " [" + AFilename + "]";
            else
                Text = CaptionBackup;
            TApplication.Instance.Title = Text;
        }
        //---------------------------------------------------------------------------
        private void About1Click(object sender, EventArgs e)
        {
            TApplication.Instance.CreateForm(out Unit.frmAboutBox);
            Unit.frmAboutBox.lbVersion.Text = "Version " + Version;
            Unit.frmAboutBox.ShowDialog();
            Unit.frmAboutBox.Dispose(); Unit.frmAboutBox = null;
        }
        //---------------------------------------------------------------------------
        private void TfrmMain_FormCreate(object sender, EventArgs e)
        {
            string tstr;
            bLock++;
            TGnuGettextInstance.TranslateComponent(this);
            CaptionBackup = _("Moras Ausrüstungsplaner ") + Version;
            Text = CaptionBackup;
            Utils.LoadWindowPosition("Main", Unit.frmMain, true);
            Panel2Resize(null, EventArgs.Empty);
            TApplication.Instance.CreateForm(out Unit.frmSplash);
            Unit.frmSplash.lbVersion.Text = "Version " + Version;
            Unit.frmSplash.Show();
            Application.DoEvents();
            History[0] = History1;
            History[1] = History2;
            History[2] = History3;
            History[3] = History4;
            for (int i = 0; i < 34; i++)
                Attributes[i] = (TMCapView)this.FindComponent("MCapView" + (i + 1).ToString());
            for (int i = 0; i < CPlayer.PLAYER_ITEMS; i++)
                ItemSlot[i] = (TMItemSlot)this.FindComponent("MItemSlot" + (i + 1).ToString());
            chWeapon[0] = chWeapon1;
            chWeapon[1] = chWeapon2;
            chWeapon[2] = chWeapon3;
            chWeapon[3] = chWeapon4;
            for (int i = 0; i < 5; i++)
            {
                cbGemQuality[i] = (TMIComboBox)this.FindComponent("cbGemQuality1_" + (i + 1).ToString());
                for (int j = 96; j < 101; j++)
                    cbGemQuality[i].Add(j.ToString() + "%", -1, j);
            }
            for (int j = 96; j < 101; j++)
                cbQuality.Items.Add((j).ToString() + "%");
            string strAdd;
            for (int i = 0; i < 15; i++)
            {
                if (i < 5)
                    strAdd = "1_" + (i + 1).ToString();
                else
                    strAdd = "2_" + (i - 4).ToString();
                cbType[i] = (TMIComboBox)this.FindComponent("CbType" + strAdd);
                cbEffect[i] = (TMIComboBox)this.FindComponent("CbEffect" + strAdd);
            }
            for (int i = 0; i < 10; i++)
            {
                tbValue[i] = (TextBox)this.FindComponent("tbValue2_" + (i + 1).ToString());
                teLevel[i] = (TextBox)this.FindComponent("tbLevel2_" + (i + 1).ToString());
                udLevel[i] = (TUpDown)this.FindComponent("udLevel2_" + (i + 1).ToString());
            }
            for (int i = 0; i < 6; i++)
            {
                lbMat[i] = (TLabel)this.FindComponent("lbMat" + (i).ToString());
                lbMatLevel[i] = (TLabel)this.FindComponent("lbMatLevel" + (i).ToString());
                lbMatPrice[i] = (TLabel)this.FindComponent("lbMatPrice" + (i).ToString());
                mnSwap[i] = (TMainMenu.TTBXItem)this.FindComponent("mnSwap" + (i).ToString());
            }
            cbValue[0] = CbValue1_1;
            cbValue[1] = CbValue1_2;
            cbValue[2] = CbValue1_3;
            cbValue[3] = CbValue1_4;
            cbValue[4] = CbValue1_5;
            rbItemType[0] = rbItemType1;
            rbItemType[1] = rbItemType2;
            rbItemType[2] = rbItemType3;
            lbIP[0] = lbIP1_1;
            lbIP[1] = lbIP1_2;
            lbIP[2] = lbIP1_3;
            lbIP[3] = lbIP1_4;
            lbIP[4] = lbIP1_5;
            tbRemakes[0] = tbRemakes1_1;
            tbRemakes[1] = tbRemakes1_2;
            tbRemakes[2] = tbRemakes1_3;
            tbRemakes[3] = tbRemakes1_4;
            tbRemakes[4] = tbRemakes1_5;
            udRemakes[0] = udRemakes1_1;
            udRemakes[1] = udRemakes1_2;
            udRemakes[2] = udRemakes1_3;
            udRemakes[3] = udRemakes1_4;
            udRemakes[4] = udRemakes1_5;
            lbCost[0] = lbCost1_1;
            lbCost[1] = lbCost1_2;
            lbCost[2] = lbCost1_3;
            lbCost[3] = lbCost1_4;
            lbCost[4] = lbCost1_5;
            chDone[0] = chDone1_1;
            chDone[1] = chDone1_2;
            chDone[2] = chDone1_3;
            chDone[3] = chDone1_4;
            chDone[4] = chDone1_5;
            cbRestriction[0] = cbRestriction1;
            cbRestriction[1] = cbRestriction2;
            cbRestriction[2] = cbRestriction3;
            cbRestriction[3] = cbRestriction4;
            lbRestriction[0] = lbRestriction1;
            lbRestriction[1] = lbRestriction2;
            lbRestriction[2] = lbRestriction3;
            lbRestriction[3] = lbRestriction4;
            lbGem[0] = lbGem1_1;
            lbGem[1] = lbGem1_2;
            lbGem[2] = lbGem1_3;
            lbGem[3] = lbGem1_4;
            lbGem[4] = lbGem1_5;
            for (int i = 0; i < 4; i++)
            {
                tstr = string.Format("RecentFile{0}", i);
                HistoryFile[i] = Utils.GetRegistryString(tstr, "");
            }
            UpdateHistory(HistoryFile[0]);
            // Baue Grid für Inventar auf
            GridInventory.Columns[0].HeaderCell.Value = _("Gegenstand");
            GridInventory.RowHeadersWidth = 24;
            GridInventory.Columns[0].Width = GridInventory.Width - 45;
            GridInventory.RowCount = CPlayer.INVENTORY_ITEMS - CPlayer.PLAYER_ITEMS;
            for (int i = CPlayer.PLAYER_ITEMS; i < CPlayer.INVENTORY_ITEMS; i++)
                GridInventory.Rows[i - CPlayer.PLAYER_ITEMS].HeaderCell.Value = (i - CPlayer.PLAYER_ITEMS + 1).ToString() + ".";
            // Das selbe nochmal für die Schatzkiste
            GridTreasury.Columns[0].HeaderCell.Value = _("Gegenstand");
            GridTreasury.RowHeadersWidth = 24;
            GridTreasury.Columns[0].Width = GridInventory.Width - 45;
            GridTreasury.RowCount = CPlayer.ALL_ITEMS - CPlayer.INVENTORY_ITEMS;
            for (int i = CPlayer.INVENTORY_ITEMS; i < CPlayer.ALL_ITEMS; i++)
                GridTreasury.Rows[i - CPlayer.INVENTORY_ITEMS].HeaderCell.Value = (i - CPlayer.INVENTORY_ITEMS + 1).ToString() + ".";

            string splashCaption = _("Lade Konfigurationsdateien...");
            Unit.frmSplash.SetCaption(splashCaption);
            if (Unit.xml_config.bDebugStartup)
                Utils.DebugPrint("Configfiles\n----------------------------------------------------");

            TStringList cfgFiles = new TStringList();
            if (Directory.Exists(DataPath + "configs\\"))
            {
                foreach (string cfgFile in Directory.EnumerateFiles(DataPath + "configs\\", "*.xml"))
                {

                    cfgFiles.Add(cfgFile);
                } //
            }
            cfgFiles.Sort();
            for (int i = 0; i < cfgFiles.Count; i++)
            {
                Unit.frmSplash.SetCaption(splashCaption + "(" + Path.GetFileName(cfgFiles.Strings[i]) + ")", false);
                if (Unit.xml_config.bDebugStartup)
                    Utils.DebugPrint(cfgFiles.Strings[i]);
                Unit.xml_config.OpenConfig(cfgFiles.Strings[i]);
            }
            cfgFiles.Clear(); cfgFiles = null;
            Unit.xml_config.PostLoad();

            Unit.frmSplash.SetCaption(_("Lade Optionsdateien..."));
            if (Unit.xml_config.bDebugStartup)
                Utils.DebugPrint("Optionfiles\n----------------------------------------------------");
            Unit.player = Unit.account.Init();
            Debug.Assert(Unit.player != null);
            // Mal schauen, ob es ein paar Optionen-Xml-Dateien im Verzeichnis gibt
            int FirstUserOption = 0;
            int UserOptions = 0;

            if (Directory.Exists(DataPath + "options\\"))
            {
                foreach (string optFile in Directory.EnumerateFiles(DataPath + "options\\", "*.xml"))
                {
                    CXml tXml = new CXml();


                    if (Unit.xml_config.bDebugStartup)
                        Utils.DebugPrint(optFile);
                    if (tXml.OpenXml(optFile) && tXml.NextTag())
                    {
                        if (tXml.isTag("option"))
                        {	// Es ist ein Options-File
                            // Gibt es den Über-Eintrag schon?
                            TMainMenu.TTBXCustomItem parent = (TMainMenu.TTBXCustomItem)Utils.FindTBXItem(Options1, tXml.AttributeValue["group"]);
                            if (parent == null)
                            {	// Es gibt ihn noch nicht, also erzeugen
                                ++UserOptions;
                                parent = new TMainMenu.TTBXSubmenuItem();
                                parent.Text = tXml.AttributeValue["group"];
                                parent.Tag = UserOptions;
                                if (FirstUserOption == 0)
                                    FirstUserOption = Options1.DropDownItems.Count;
                                Options1.DropDownItems.Add(parent);
                            }
                            // Neuen Eintrag in xml_config erzeugen
                            int idx = Unit.xml_config.arMenuItems.Length;
                            Unit.xml_config.arMenuItems.Length++;
                            Unit.xml_config.arMenuItems.Array[idx].Group = tXml.AttributeValue["group"];
                            Unit.xml_config.arMenuItems.Array[idx].Name = tXml.Content;
                            Unit.xml_config.arMenuItems.Array[idx].FileName = optFile;
                            Unit.xml_config.arMenuItems.Array[idx].bPricing = (tXml.AttributeValue["pricing"].Length > 0);
                            Unit.xml_config.arMenuItems.Array[idx].bRadioItem = (tXml.AttributeValue["noradio"] != "true");
                            TMainMenu.TTBXItem NewItem = new TMainMenu.TTBXItem();
                            Debug.Assert(NewItem != null);
                            NewItem.Text = tXml.Content;
                            NewItem.Tag = idx;
                            if (Unit.xml_config.arMenuItems[idx].bRadioItem)
                                NewItem.GroupIndex = Utils.ConvertTagToInt(parent.Tag);
                            NewItem.Click += mnUserOptionClick;
                            if ((tXml.AttributeValue["default"].Length > 0) && Unit.xml_config.arMenuItems[idx].bRadioItem)
                            {
                                NewItem.Checked = true;
                            }
                            parent.DropDownItems.Add(NewItem);
                        }
                    }
                }
            }
            if (FirstUserOption == 0)
                FirstUserOption = Options1.DropDownItems.Count;
            // Die Anzeigefelder initialisieren, die sich nie ändern
            for (int i = 0; i < Unit.xml_config.nAttributes; i++)
            {
                int idx = Unit.xml_config.arAttributes[i].displayid;
                if (idx >= 0)
                    Attributes[idx].Data = i;
            }
            Unit.frmSplash.SetCaption(_("Update Itemdatenbank..."));
            if (Unit.xml_config.bDebugStartup)
                Utils.DebugPrint("Update Itemdatenbank\n----------------------------------------------------");
            if (Unit.xml_config.bDebugStartup && Unit.xml_config.bDebugSQL)
            {
                string tmpstr = "Wähle Database '" + DataPath + "items.db3'";
                Utils.DebugPrint(tmpstr);
            }
            ZConnection.SetDataSource(DataPath + "items.db3");
            if (Unit.xml_config.bDebugStartup && Unit.xml_config.bDebugSQL)
                Utils.DebugPrint("Öffne Database");
            ZConnection.Open();
            if (Unit.xml_config.bDebugStartup)
                Utils.DebugPrint("Mit DB verbunden");

            SQLiteUtils.SQLiteDBInit();
            if (Unit.xml_config.bDebugStartup)
                Utils.DebugPrint("DB initialisiert");

            //	TDateTime dtFirst = TDateTime::CurrentDateTime();
            //	ItemDB.Init();
            //	TDateTime dtSecond = TDateTime::CurrentDateTime();
            //	TDateTime dtDiff = dtSecond - dtFirst;
            //	AnsiString strTemp = TimeToStr(dtDiff);
            //	frmSplash.lbInfo.Caption = "Zeit zum laden: " + strTemp + " s";
            //	Application.ProcessMessages();
            cbServer.Add(_("<keiner>"), -1, -1);
            for (int i = 0; i < Unit.xml_config.nServers; i++)
            {
                cbServer.Add(Unit.xml_config.strServers.Strings[i], Unit.xml_config.arServerType[i], i);
            }
            cbServer.SelectedIndex = Unit.xml_config.strServers.IndexOf(Utils.GetRegistryString("DefaultServer", "")) + 1;
            if (cbServer.SelectedIndex == -1) cbServer.SelectedIndex = 0;
            if (Unit.xml_config.bDebugStartup)
                Utils.DebugPrint("Server geladen");
            cbRealm.Add("Albion", 5, 1);
            cbRealm.Add("Hibernia", 6, 2);
            cbRealm.Add("Midgard", 7, 4);
            //	cbRealm.Add("Alle", -1, 7);
            cbRealm.SelectString(Utils.GetRegistryString("DefaultRealm", "Albion"));
            if (cbRealm.SelectedIndex == -1) cbRealm.SelectedIndex = 0;
            if (Unit.xml_config.bDebugStartup)
                Utils.DebugPrint("Reiche initialisiert");
            cbRealmChange(null, EventArgs.Empty);
            cbServerChange(null, EventArgs.Empty);
            if (Unit.xml_config.bDebugStartup)
                Utils.DebugPrint("Lade Standardklasse");
            // Default Klasse laden
            int idClass = Unit.xml_config.GetClassId(Utils.GetRegistryString("DefaultClass", "ARMSMAN"));
            if (cbClass.SelectData(idClass) == -1) cbClass.SelectedIndex = 0;
            if (Unit.xml_config.bDebugStartup)
                Utils.DebugPrint("Klasse geladen");
            cbClassChange(null, EventArgs.Empty);
            if (Unit.xml_config.bDebugStartup)
                Utils.DebugPrint("Klasse gewahlt");
            if (Utils.GetRegistryInteger("LoadChars", 1) == 1)
            {
                // Accounts laden. Dazu alle mox-Dateien im Verzeichnis scannen
                Unit.frmSplash.SetCaption(_("Lade Charaktere..."));
                if (Unit.xml_config.bDebugStartup)
                    Utils.DebugPrint("Lade Charaktere\n----------------------------------------------------");
                if (Directory.Exists(DataPath + "chars\\"))
                {
                    foreach (var charFile in Directory.EnumerateFiles(DataPath + "chars\\", "*.mox"))
                    {
                        try
                        {
                            CPlayer tplayer = Unit.account.NewPlayer();
                            Debug.Assert(tplayer != null);
                            tplayer.FileName = charFile;
                            tplayer.Load();
                            // Spieler jetzt noch dem korrekten Account zuordnen
                            Unit.account.AssignAccounts();
                        }
                        catch (Exception)
                        {
                            string msg;
                            msg = "Fehler beim Laden des Charakters '" + charFile + "'";
                            Utils.DebugPrint(msg);
                        }
                    } //
                }
            }
            if (Unit.xml_config.bDebugStartup)
                Utils.DebugPrint("Lade Accounts\n----------------------------------------------------");

            // Alle Accounts in Liste kopieren
            for (int i = 0; i < Unit.account.NAccounts; i++)
            {
                cbAccount.Add(Unit.account.Name[i], -1, i);
                // Alle Spieler des Accounts in die Namensliste kopieren
                for (int j = 0; j < Unit.account.NPlayers[i]; j++)
                {
                    CPlayer tplayer = Unit.account.Player[i][j];
                    cbName.Add(tplayer.Name, tplayer);
                }
            }

            // prüfen ob eine neue version vorliegt
            if (Unit.xml_config.bDebugStartup)
                Utils.DebugPrint("Update Prüfung\n----------------------------------------------------");
            if (Utils.GetRegistryInteger("CheckForUpdate", 1) == 1)
            {
                Unit.frmSplash.SetCaption(_("Prüfe auf neue Version..."));
                bool doUpdate = false;
                int lastUpdate = Utils.GetRegistryInteger("LastUpdateIntervall", 0);

                switch (Utils.GetRegistryInteger("UpdateIntervall", 0))
                {
                    case 2: doUpdate = (int)(Utils.Now() - lastUpdate) >= 30;
                        break;
                    case 1: doUpdate = (int)(Utils.Now() - lastUpdate) >= 7;
                        break;
                    default: doUpdate = (int)(Utils.Now() - lastUpdate) >= 1;
                        break;
                }

                if (doUpdate)
                {
                    acCheckForUpdateExecute(null, EventArgs.Empty);
                    Utils.SetRegistryInteger("LastUpdateIntervall", (int)Utils.Now());
                }
            }

            Unit.frmSplash.SetCaption(_("Initialisiere..."));
            if (Unit.xml_config.bDebugStartup)
                Utils.DebugPrint("Initialisiere\n----------------------------------------------------");

            cbName.SelectedIndex = 0;
            cbAccount.SelectedIndex = 0;
            // Craft-Mode initialisieren
            int iMode = Utils.GetRegistryString("CraftMode", "0").ToIntDef(0);
            mnCraftMode.Checked = iMode != 0;
            mnCraftModeClick(null, EventArgs.Empty);
            if (Unit.xml_config.bDebugStartup)
                Utils.DebugPrint("Craftmodus gesetzt");


            // Aufpreise fürs Waffenschmieden laden
            tbMarkup.Text = Utils.GetRegistryString("MaterialMarkup", "20").ToIntDef(0).ToString();
            tbRemakeMarkup.Text = Utils.GetRegistryString("RemakeMarkup", "20").ToIntDef(0).ToString();
            if (Unit.xml_config.bDebugStartup)
                Utils.DebugPrint("Waffenschmieden initialisiert");


            // Die eingestellten oder default User-Konfigs laden
            //	frmSplash.lbInfo.Caption = "Lade Itemdaten...";
            //	Application.ProcessMessages();
            // arbeite die UserMenü-Kategorien durch
            if (Unit.xml_config.bDebugStartup)
                Utils.DebugPrint("Optionen\n----------------------------------------------------");
            for (int i = FirstUserOption; i < Options1.DropDownItems.Count; i++)
            {
                TMainMenu.TTBXCustomItem item = (TMainMenu.TTBXCustomItem)Options1.DropDownItems[i];
                string sSaved = Utils.GetRegistryString("Option" + item.Text, "");
                TMainMenu.TTBXItem firstItem = (TMainMenu.TTBXItem)item.DropDownItems[0];
                if (Unit.xml_config.arMenuItems[Utils.ConvertTagToInt(firstItem.Tag)].bRadioItem)
                {
                    // Suche den registry-Eintrag, oder den default
                    int idx = -1;
                    for (int j = 0; j < item.DropDownItems.Count; j++)
                    {
                        TMainMenu.TTBXItem userItem = (TMainMenu.TTBXItem)item.DropDownItems[j];
                        if (userItem.Text == sSaved)
                        {
                            idx = j;
                            userItem.Checked = true;
                            userItem.PerformClick();
                            break;
                        }
                        if (userItem.Checked)
                            idx = j;
                    }
                    // falls nicht gefunden oder noch nichts gespeichert ist den ersten eintrag als standard auswählen
                    if ((idx < 0) && Unit.xml_config.arMenuItems[Utils.ConvertTagToInt(firstItem.Tag)].bRadioItem)
                    {
                        idx = 0;
                        firstItem.Checked = true;
                        firstItem.PerformClick();
                    }
                    // Lade die Config
                    if (idx >= 0)
                    {
                        SetConfig(Unit.xml_config.arMenuItems[Utils.ConvertTagToInt(item.DropDownItems[idx].Tag)].FileName);
                    }
                }
            }

            // Übergebene Characterdatei laden
            if (Utils.ParamCount() > 0)
            {
                if (Unit.xml_config.bDebugStartup)
                    Utils.DebugPrint("Öffne Mox-File\n----------------------------------------------------");
                string file = Utils.ParamStr(1);
                // Nur laden wenn die Datei auch wirklich existiert
                if (File.Exists(file))
                {
                    LoadCharacterFile(file);
                }
            }
            if (Unit.xml_config.bDebugStartup)
                Utils.DebugPrint("Initialisierung abgeschlossen. Starte Mainframe\n----------------------------------------------------");

            bLock--;
            // Aktuelles Item ist hier immer 0, so das hier hein test auf PLAYER_ITEMS erfolgen muß.
            MItemSlotClick(ItemSlot[iActSlot], EventArgs.Empty);
            Unit.frmSplash.pbLoad.Value = Unit.frmSplash.pbLoad.Maximum;
            Unit.frmSplash.Dispose();
            Unit.frmSplash = null;
            Unit.player.Modified = false;
        }
        //---------------------------------------------------------------------------
        // Lade eine neue Konfiguration und sorge dafür, das alles geupdated wird
        public void SetConfig(string FileName)
        {
            Unit.xml_config.OpenConfig(FileName);
            Unit.xml_config.PostLoad();	// Momentan eher unnötig, aber falls weitere PostLoad-Prozesse
            // eingearbeitet werden, kann es essenziell werden...
            int i, idx, x;
            if (Unit.xml_config.ItemUpdate.bFound)
            {	// ItemUpdate-Sektion im xml gefunden. Starte Internet-Update
                //		Application.CreateForm(__classid(TfrmImport), &frmImport);
                Unit.frmImport.ImportInternet();
                //		delete frmImport;
            }
            // Die Namen der einzelnen Itemslots
            for (i = 0; i < CPlayer.PLAYER_ITEMS; i++)
                ((TLabel)this.FindComponent("lbSlot" + (i + 1).ToString())).Text = Unit.xml_config.arItemSlots[i].strSlotName;
            // Die ersten 6 Slotnamen für Juwelentausch
            for (i = 0; i < 6; i++)
                mnSwap[i].Text = Unit.xml_config.arItemSlots[i].strSlotName;
            // Die Einträge für Effekt-Typ
            for (idx = 0; idx < 15; idx++)
            {
                cbType[idx].Clear();
                for (i = 0; i < Unit.xml_config.nGroups; i++)
                {
                    if (((idx < 4) && !(Unit.xml_config.arGroups[i].bSlot5Only || Unit.xml_config.arGroups[i].bDropOnly)) ||
                         ((idx == 4)) ||
                         ((idx > 4) && !Unit.xml_config.arGroups[i].bSlot5Only))
                        cbType[idx].Add(Unit.xml_config.arGroups[i].Name, -1, i);
                }
            }
            // Nicht wechselnde Statuswerte
            for (i = 0; i < Unit.xml_config.nAttributes; i++)
            {
                idx = Unit.xml_config.arAttributes[i].displayid;
                if (idx >= 0)
                    Attributes[idx].Text = Unit.xml_config.arAttributes[i].Name;
            }
            // Die Klasseneinträge
            cbRealmChange(null, EventArgs.Empty);
            // Jetzt alle Einträge aus der INTERFACE-Section durcharbeiten
            for (i = 0; i < Unit.xml_config.strInterfaceElements.Count; i++)
            {
                Component tctrl = (Component)this.FindComponent(Unit.xml_config.strInterfaceElements.Strings[i]);
                if (tctrl != null)
                {
                    if (tctrl is Control)
                        ((TLabel)tctrl).Text = Unit.xml_config.strInterfaceName.Strings[i];
                    else if (tctrl is ToolStripMenuItem)
                        ((ToolStripMenuItem)tctrl).Text = Unit.xml_config.strInterfaceName.Strings[i];
                }
            }
            if (iActSlot < CPlayer.PLAYER_ITEMS)
                MItemSlotClick(ItemSlot[iActSlot], EventArgs.Empty);
        }

        // Das ist ne schwere Funktion. Alle möglichen Dialogelemente in eine bestimmte Sprache setzen
        // Wird nicht mehr benutzt, macht alles SetConfig nun
        public void SetLanguage(int Language)	// Setze eine neue Sprache
        {
            int i, idx, x;
            // Wenn Language ungültig, dann entweder automatisch oder deutch als Sprache nehmen
            /*	if ((Language < 0) || (Language > 2))
                {
                    if (cbServer.ItemIndex > 0)
                    {	// Es gibt einen gewählten Server, also Sprache nach ihm auswählen
                        Language = -1;
                        x = xml_config.arServerType[cbServer.ItemIndex - 1];
                        // Jetzt noch schauen, welche Datei die Language-ID hat
                        for (i = 0; i < nLanguages; i++)
                            if (iLanguageId[i] == x)
                                Language = i;
                        if (Language == -1)
                            Language = 0;
                    }
                    else
                        Language = 0;
                }
                xml_config.OpenConfig(strLanguageFile[Language]);
            // Die Namen der einzelnen Itemslots */
            for (i = 0; i < CPlayer.PLAYER_ITEMS; i++)
                ((TLabel)this.FindComponent("lbSlot" + (i + 1).ToString())).Text = Unit.xml_config.arItemSlots[i].strSlotName;
            // Die Einträge für Effekt-Typ
            for (idx = 0; idx < 15; idx++)
            {
                cbType[idx].Clear();
                for (i = 0; i < Unit.xml_config.nGroups; i++)
                {
                    if (((idx < 4) && !(Unit.xml_config.arGroups[i].bSlot5Only || Unit.xml_config.arGroups[i].bDropOnly)) ||
                         ((idx == 4)) ||
                         ((idx > 4) && !Unit.xml_config.arGroups[i].bSlot5Only))
                        cbType[idx].Add(Unit.xml_config.arGroups[i].Name, -1, i);
                }
            }
            // Nicht wechselnde Statuswerte
            for (i = 0; i < Unit.xml_config.nAttributes; i++)
            {
                idx = Unit.xml_config.arAttributes[i].displayid;
                if (idx >= 0)
                    Attributes[idx].Text = Unit.xml_config.arAttributes[i].Name;
            }
            // Die Klasseneinträge
            cbRealmChange(null, EventArgs.Empty);
            // Jetzt alle Einträge aus der INTERFACE-Section durcharbeiten
            for (i = 0; i < Unit.xml_config.strInterfaceElements.Count; i++)
            {
                TLabel tctrl = (TLabel)this.FindComponent(Unit.xml_config.strInterfaceElements.Strings[i]);
                if (tctrl != null) tctrl.Text = Unit.xml_config.strInterfaceName.Strings[i];
            }
            if (iActSlot < CPlayer.PLAYER_ITEMS)
                MItemSlotClick(ItemSlot[iActSlot], EventArgs.Empty);
        }

        public void UpdateAttributes()
        {	// Anzeige der Attribute aktualisieren
            // Nebenbei auch die Kosten zusammen rechnen
            Unit.player.UpdateAttributes();
            lbSCCost.Text = Utils.Int2Gold(Unit.player.SCCost);
            lbSCPrice.Text = Utils.Int2Gold(Unit.player.SCPrice);
            lbGesamt.Text = (Unit.player.CalcGesamtNutzen()).ToString("###0.0");
            Unit.player.DisplayAttributes();

            CheckAlerts();
        }

        // Teste auf nicht erlaubte Sachen, wie
        // 2 gleiche Effekte auf einem Item, Envenom auf Waffen
        public void CheckAlerts()
        {
            tbAlerts.Clear();
            List<string> lines = new List<string>();
            for (int slot = 0; slot < CPlayer.PLAYER_ITEMS; slot++)
            {
                for (int i = 0; i < Unit.player.nItemEffects[slot]; i++)
                {
                    int bid = Unit.player.ItemEffect[slot][i];
                    if (bid >= 0)
                    {
                        if ((Unit.xml_config.arItemSlots[slot].type == ESlotType.Weapon) && (Unit.xml_config.arBonuses[bid].id == "ENVENOM"))
                            lines.Add(_("Vergiften im Waffenslot '") + Unit.xml_config.arItemSlots[slot].strSlotName + _("' ist nicht sinnvoll!"));
                        for (int j = i + 1; j < Unit.player.nItemEffects[slot]; j++)
                        {
                            if (bid == Unit.player.ItemEffect[slot][j])
                            {
                                lines.Add(_("Effekt '") + Unit.xml_config.arBonuses[bid].Names[0] + _("' mehrfach in Position '") + Unit.xml_config.arItemSlots[slot].strSlotName + "'!");
                                break;
                            }
                        }
                    }
                }
            }
            tbAlerts.Lines = lines.ToArray();
        }

        // Aktualisiere die Datein eines Gegenstandes
        // Interface => Item
        // Wenn crafted, dann auch den IP-Verbrauch bestimmen
        public void UpdateItem(int pos)
        {
            UpdateItemSlot(pos);
            //	if (bLock) return;
            int i, iUsedSlots = 0;
            bool bCrafted = (Unit.player.ItemType[pos] == EItemType.Crafted);
            // Arbeite die Effekt-Slots durch
            int iMaxIP = 0;
            int iSumIP = 0;
            int[] iIP = new int[4];
            if (bCrafted)
            {
                for (i = 0; i < 4; i++)
                {
                    lbCost[i].Text = Utils.Int2Gold(Unit.player.GemCost[pos][i]);
                    int bid = cbEffect[i].CurrentData;
                    if (bLock == 0)
                        Unit.player.ItemEffect[iActSlot][i] = bid;
                    if (bLock == 0 && (bid >= 0))
                        Unit.player.EffectValue[iActSlot][i] = cbValue[i].CurrentData;
                    iIP[i] = Unit.player.EffectIP[pos][i];
                    if (bid >= 0)
                    {
                        if (iIP[i] > iMaxIP) iMaxIP = iIP[i];
                        iSumIP += iIP[i];
                        iUsedSlots++;
                    }
                    // Wenn wir die richtigen IP anzeigen sollen, dann diese Zeile
                    if (!Unit.xml_config.bHalfIP)
                        lbIP[i].Text = iIP[i].ToString();
                }
                // Cinnean, 14.09.07:
                // Bug-Fix zu: https://sourceforge.net/tracker/index.php?func=detail&aid=1756266&group_id=108488&atid=650615
                // Die Boni in den Boxen 10-14 (CbEffect2_6...CbEffect2_10)
                // wurden beim Umschalten von Drop/Unique auf Crafted
                // zwar unsichtbar, waren aber noch aktiv, und wurden auch weiterhin
                // eingerechnet.
                // Dies stellt sicher, dass nach einem Wechsel die Auswahl auf Unbenutzt zurückgeschaltet wird
                // Start-Bug-Fix
                for (i = 10; i < 15; i++)
                {
                    if (bLock == 0)
                        Unit.player.ItemEffect[iActSlot][i] = -1;
                    cbType[i].SelectedIndex = 0;
                    cbEffect[i].SelectedIndex = -1;
                    cbEffect[i].Enabled = false;
                    tbValue[i - 5].Text = "";
                    tbValue[i - 5].Enabled = false;
                }
                // Ende-Bug-Fix
                // Berechne verfügbare IP
                int AvailIP = Unit.player.CalcAvailIP(pos);
                int UsedIP = iSumIP + iMaxIP;
                lbAvailIP.Text = AvailIP.ToString();
                lbUsedIP.Text = (((double)UsedIP) / 2).ToString("#0.0");
                // Für halbe-IP
                if (Unit.xml_config.bHalfIP)
                {
                    for (i = 0; i < 4; i++)
                    {
                        if (iIP[i] == iMaxIP)
                        {
                            lbIP[i].Text = iIP[i].ToString();
                            iMaxIP = 1000;
                        }
                        else
                            lbIP[i].Text = (((double)iIP[i]) / 2).ToString("#0.0");
                    }
                }
                int oc = Unit.player.CalcOverCharge(pos);
                // Hier später mal noch mit Farben arbeiten
                if (oc > 100)
                {
                    lbOverLoad.ForeColor = SystemColors.WindowText;
                    lbOverLoad.Text = _("nicht nötig");
                }
                else if (oc < -100)
                {
                    lbOverLoad.ForeColor = Color.Red;
                    lbOverLoad.Text = _("unmöglich");
                }
                else if (oc > 0)
                {
                    if (oc >= 85)
                        lbOverLoad.ForeColor = Color.Green;
                    else if (oc >= 60)
                        lbOverLoad.ForeColor = Color.Yellow;
                    else
                        lbOverLoad.ForeColor = Color.Red;
                    lbOverLoad.Text = (oc).ToString() + "%";
                }
                else	// BOOM
                {
                    lbOverLoad.ForeColor = Color.Red;
                    lbOverLoad.Text = "BOOM (" + (oc).ToString() + "%)";
                }
                // Gib die Idealen IP aus
                if (iUsedSlots < 4)
                {	// Nur berechnen, wenn noch nicht alle Slots belegt
                    int IdealIP = CalcIdealIP(pos);
                    lbIdealIP.Text = (((double)IdealIP) / 10).ToString("#0.#");
                }
                else
                    lbIdealIP.Text = "--";
            }
            else
            {
                if (bLock != 0) return;
                // Schalte den Slot auf Drop-Anzeige um
                for (i = 0; i < 10; i++)
                {
                    if (cbEffect[i + 5].Visible)
                    {
                        int bid = cbEffect[i + 5].CurrentData;
                        if (bid >= 0)
                        {
                            Unit.player.EffectValue[iActSlot][i] = tbValue[i].Text.ToIntDef(0);
                            // Noch eine Spezialbehandlung für den Fall, das ein nicht craftbarer
                            // Effekt bei einem Unique Gegenstand auftaucht
                            /*					if ((player.ItemType[pos] == Unique) && \
                                                    (!xml_config.arBonuses[bid].bCraftable))
                                                {	// Setze Effekt 0 und stelle Boxen auf unbenutzt
                                                    bid = -1;
                                                    cbType[i + 4].ItemIndex = 0;
                                                    CbTypeChange(cbType[i + 4]);
                                                    tbValue[i].Clear();
                                                }
                             */
                        }
                        Unit.player.ItemEffect[iActSlot][i] = bid;
                    }
                }
            }
        }

        // Aktualisiere die Anzeige des Itemslots
        // D.h. die Farbe, die Belegung und den Tooltiptext
        // Bei Inventar-Items den Namen
        public void UpdateItemSlot(int pos) // Setze die Farbe und den Tooltiptext des Itemslots
        {
            bool bCrafted = (Unit.player.ItemType[pos] == EItemType.Crafted);
            if (pos < CPlayer.PLAYER_ITEMS)
            {	// Tooltips erstellen
                ItemSlot[pos].SetHint(Unit.player.Item[pos].ToolTipText);
                if (bCrafted)
                {
                    int iMaxIP = 0;
                    int iSumIP = 0;
                    int iUsedSlots = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        int bid = Unit.player.ItemEffect[pos][i];
                        int iIP = Unit.player.EffectIP[pos][i];
                        if (bid >= 0)
                        {
                            if (iIP > iMaxIP) iMaxIP = iIP;
                            iSumIP += iIP;
                            iUsedSlots++;
                        }
                    }
                    ItemSlot[pos].UsedSlots = iUsedSlots;
                    ItemSlot[pos].AvailIP = (Unit.player.Item[pos].isEmpty()) ? 0 : Unit.player.CalcAvailIP(pos);
                    ItemSlot[pos].UsedIP = (iSumIP + iMaxIP) / 2;
                }
                else
                {	// Nicht Crafted
                    ItemSlot[pos].UsedSlots = 0;
                    if (Unit.player.Item[pos].isEmpty())
                        ItemSlot[pos].AvailIP = 0;
                    else if (Unit.player.ItemType[pos] == EItemType.Unique)
                        ItemSlot[pos].AvailIP = -2;
                    else // Irgend ein Drop nun, der auch nicht leer ist
                    {	// Nur noch zwischen normalen Drop und Artefakt unterscheiden
                        if (Unit.player.Item[pos].MaxLevel > 0)
                            ItemSlot[pos].AvailIP = -3;	// Artefakt
                        else
                            ItemSlot[pos].AvailIP = -1;	// normaler Drop
                    }
                }
            }
            else if (pos < CPlayer.INVENTORY_ITEMS)
            {	// Es ist ein Inventar-Slot
                GridInventory.Rows[pos - CPlayer.PLAYER_ITEMS].Cells[0].Value = Unit.player.Item[pos].Name;
            }
            else
            {	// Es ist ein Schatzkisten-Slot
                GridTreasury.Rows[pos - CPlayer.INVENTORY_ITEMS].Cells[0].Value = Unit.player.Item[pos].Name;
            }
        }

        // Berechne die Idealen IPs dieses Gegenstandes
        // Gibt Festkommawert mit einer Zehnerstelle zurück
        public int CalcIdealIP(int pos)
        {
            int i;
            int IdealIP = 0;
            // Arbeite die Effect-Slots durch
            if (Unit.player.ItemType[pos] == EItemType.Crafted)
            {	// Das ganze nur berechnen, wenn es ein crafted-Item ist
                int iMaxIP = 0, iUsedSlots = 0;
                int iSumIP = 0;
                for (i = 0; i < 4; i++)
                {
                    int iEffect = Unit.player.ItemEffect[pos][i];
                    int iIP = Unit.player.EffectIP[pos][i];
                    if (iEffect >= 0)
                    {
                        if (iIP > iMaxIP) iMaxIP = iIP;
                        iSumIP += iIP;
                        iUsedSlots++;
                    }
                }
                // Berechne verfügbare IP
                int AvailIP = Unit.player.CalcAvailIP(pos);
                // Berechne die "Idealen" IP. Das ist der Wert, bei dem der Gegenstand ideal genutzt wird
                // IdealIP = (AvailIP + Overload + 0,5) / (leere Slots + 1) x 2
                // Wenn dieser Wert größer als MaxIP, dann "--" ausgeben
                if (iUsedSlots < 4)
                {	// Nur berechnen, wenn noch nicht alle Slots belegt
                    IdealIP = ((AvailIP + cbOverload.SelectedIndex) * 2 - iSumIP + 1) * 10 / (5 - iUsedSlots);
                    if (IdealIP < iMaxIP * 10)
                    {	// Wenn die IdealIP kleiner als der bisherige Maximalwert sind,
                        // stimmt die Rechnung nicht. Deshalb neu rechnen
                        IdealIP = ((AvailIP + cbOverload.SelectedIndex) * 2 - iSumIP - iMaxIP + 1) * 10 / (4 - iUsedSlots);
                    }
                }
            }
            return IdealIP;
        }

        private void acFileSaveExecute(object sender, EventArgs e)
        {
            if (Unit.player.FileName == "")
                acFileSaveAs.OnExecute(EventArgs.Empty);
            else
            {
                if (Unit.player.Save(1) == false)
                {	// Fehler beim speichern
                    Utils.MorasErrorMessage(_("Es ist ein Fehler beim speichern der Datei '") + Unit.player.FileName + _("' aufgetreten!"), _("Fehler"));
                }
            }
        }
        //---------------------------------------------------------------------------

        private void acFileSaveAsAccept(object sender, EventArgs e)
        {
            Unit.player.FileName = acFileSaveAs.Dialog.FileName;
            UpdateMainCaption(Unit.player.FileName);
            Utils.SetRegistryString("LastCharDir", Utils.IncludeTrailingPathDelimiter(Path.GetDirectoryName(acFileSaveAs.Dialog.FileName)));
            if (Unit.player.Save(acFileSaveAs.Dialog.FilterIndex) == false)
            {	// Fehler beim speichern
                Utils.MorasErrorMessage(_("Es ist ein Fehler beim speichern der Datei '") + Unit.player.FileName + _("' aufgetreten!"), _("Fehler"));
            }
            UpdateHistory(Unit.player.FileName);
        }
        //---------------------------------------------------------------------------

        private void acFileSaveAsBeforeExecute(object sender, EventArgs e)
        {
            string tmp;

            acFileSaveAs.Dialog.InitialDirectory = Utils.GetRegistryString("LastCharDir", "");
            if (acFileSaveAs.Dialog.InitialDirectory == "")
            {
                acFileSaveAs.Dialog.InitialDirectory = DataPath + "Chars";
            }
            if ((Unit.player.FileName == "") && (Unit.player.Name.IndexOf("<") == -1))
            {
                tmp = string.Format("{0}({1})", Unit.player.Name, Unit.player.Level);	// Das muss über nen temp-string gehen
                acFileSaveAs.Dialog.FileName = tmp;	// Keine Ahnung warum
            }
        }
        //---------------------------------------------------------------------------

        private void cbServerChange(object sender, EventArgs e)
        {
            if (cbServer.SelectedIndex == 0)
                Unit.player.Server = "";
            else
                Unit.player.Server = cbServer.Text;//Caption;
            Utils.SetRegistryString("DefaultServer", Unit.player.Server);
        }
        //---------------------------------------------------------------------------

        private void acFileOpenBeforeExecute(object sender, EventArgs e)
        {
            acFileOpen.Dialog.InitialDirectory = Utils.GetRegistryString("LastCharDir", "");
            if (acFileOpen.Dialog.InitialDirectory == "")
            {
                acFileOpen.Dialog.InitialDirectory = DataPath + "Chars";
            }
        }
        //---------------------------------------------------------------------------

        private void acFileOpenAccept(object sender, EventArgs e)
        {
            Utils.SetRegistryString("LastCharDir", Utils.IncludeTrailingPathDelimiter(Path.GetDirectoryName(acFileOpen.Dialog.FileName)));
            LoadCharacterFile(acFileOpen.Dialog.FileName);
        }
        //---------------------------------------------------------------------------
        public void LoadCharacterFile(string AFile)
        {
            Unit.player.FileName = AFile;
            if (Unit.player.Load() == false)
            {
                UpdateHistory(AFile);
                Player2Form();
                Unit.player.Modified = false;
            }
            else
            {	// Eine Fehlermeldung wäre wohl echt nicht schlecht hier
                Utils.MorasErrorMessage(_("Beim laden der Datei '") + Unit.player.FileName + _("' ist ein Fehler aufgetreten!"), _("Fehler"));
                // Ein Updatehistory eigentlich auch, nur das der Eintrag gelöscht werden sollte
                Unit.player.FileName = "";	// Dateiname löschen, damit der später nicht verwendet wird
            }
        }
        //---------------------------------------------------------------------------
        // Hält die Dateihistory auf den aktuellen stand
        public void UpdateHistory(String FileName)
        {	// zuerst suchen, ob der Eintrag bereits vorhanden ist
            int i, j;
            String tstr;
            for (i = 0; i < 4; i++)
            {
                if (string.Equals(HistoryFile[i], FileName, StringComparison.CurrentCultureIgnoreCase))
                {	// Name bereits vorhanden
                    // Die restlichen Files 1 nach vorne holen
                    for (j = i; j < 3; j++)
                    {
                        HistoryFile[j] = HistoryFile[j + 1];
                    }
                }
            }
            // Alle Einträge 1 nach hinten schieben
            for (i = 2; i >= 0; i--)
            {
                HistoryFile[i + 1] = HistoryFile[i];
            }
            HistoryFile[0] = FileName;
            // Nun die Dateien in das Menu schreiben
            for (i = 0; i < 4; i++)
            {
                tstr = string.Format("&{0} {1}", i + 1, HistoryFile[i]);
                History[i].Text = tstr;
                if (HistoryFile[i].Length != 0)
                {
                    History[i].Visible = true;
                }
                // Eintrag in der Registry speichern
                tstr = string.Format("RecentFile{0}", i);
                Utils.SetRegistryString(tstr, HistoryFile[i]);
            }
        }
        //---------------------------------------------------------------------------

        private void History1Click(object sender, EventArgs e)
        {
            int index = Utils.ConvertTagToInt(((TMainMenu.TTBXItem)sender).Tag);
            LoadCharacterFile(HistoryFile[index]);
        }
        //---------------------------------------------------------------------------

        private void cbRealmChange(object sender, EventArgs e)
        {
            Unit.player.Realm = cbRealm.CurrentData;
            Unit.xml_config.iRealm = Unit.player.Realm;
            Utils.SetRegistryString("DefaultRealm", cbRealm.Text);
            // Die Combobox cbClass und die für Klassenbeschränkungen ausfüllen
            cbClass.Items.Clear();
            for (int i = 0; i < Unit.xml_config.nClasses; i++)
            {
                if ((Unit.xml_config.arClasses[i].iRealm & Unit.player.Realm) != 0)
                {
                    cbClass.Add(Unit.xml_config.arClasses[i].Name, 0, i);
                }
            }
            Debug.Assert(cbClass.Items.Count > 0);
            if (cbClass.SelectData(Unit.player.Class) == -1) cbClass.SelectedIndex = 0;
            cbClassChange(null, EventArgs.Empty);
            chAllRealmsClick(null, EventArgs.Empty);
            if (iActSlot < CPlayer.PLAYER_ITEMS)
                MItemSlotClick(ItemSlot[iActSlot], EventArgs.Empty);
        }
        //---------------------------------------------------------------------------

        private void cbClassChange(object sender, EventArgs e)
        {
            int m, i;
            // Player-Klasse setzen. Dies aktualisiert automatisch die Attribute und Wichtungen!
            Unit.player.Class = cbClass.CurrentData;
            if (bLock == 0)
                Utils.SetRegistryString("DefaultClass", Unit.xml_config.arClasses[Unit.player.Class].id);
            // Anzeigen Für Magiekraft und Magieart entsprechend setzen
            m = Unit.xml_config.arClasses[Unit.player.Class].iMagic;
            if (m == -1)
            {	// Keine Magie, Felder nur unsichtbar machen
                Attributes[4].Visible = false;
                Attributes[6].Visible = false;
            }
            else
            {	// Magie vorhanden, sichtbar machen und die richtige Art anzeigen
                Attributes[4].Text = Unit.xml_config.arAttributes[m].Name;
                Attributes[4].Data = m;
                Attributes[4].Visible = true;
                Attributes[6].Visible = true;
            }
            // Die Skills eintragen
            for (i = 0; i < 8; i++)
            {
                m = Unit.xml_config.arClasses[Unit.player.Class].arSkills[i];
                if (i < Unit.xml_config.arClasses[Unit.player.Class].nSkills)
                {
                    Attributes[17 + i].Visible = true;
                    Attributes[17 + i].Text = Unit.xml_config.arAttributes[m].Name;
                    Attributes[17 + i].Data = m;
                }
                else
                    Attributes[17 + i].Visible = false;
            }
            // Die Combobox cbRace ausfüllen
            cbRace.Items.Clear();
            cbRace.Add(_("<keine>"), -1, 0);
            for (i = 0; i < Unit.xml_config.arClasses[Unit.player.Class].nRaces; i++)
            {
                int idRace = Unit.xml_config.arClasses[Unit.player.Class].arRaces[i];
                cbRace.Add(Unit.xml_config.arRaces[idRace].Name, -1, idRace);
            }
            if (cbRace.SelectData(Unit.player.Race) == -1) cbRace.SelectedIndex = 0;
            cbRaceChange(null, EventArgs.Empty);

            Unit.player.DisplayAttributes(); // Anzeige aktualisieren
            if (iActSlot < CPlayer.PLAYER_ITEMS)
                MItemSlotClick(ItemSlot[iActSlot], EventArgs.Empty);
        }
        //---------------------------------------------------------------------------

        private void cbRaceChange(object sender, EventArgs e)
        {
            Unit.player.Race = cbRace.CurrentData;
            // Rassenboni in den Resistenzen setzen
            for (int i = 0; i < 9; i++)
            {
                Attributes[i + 8].Floor = (Unit.xml_config.bIgnoreRaceBoni) ? 0 : Unit.xml_config.arRaces[Unit.player.Race].arResists[i];
            }
        }
        //---------------------------------------------------------------------------

        private void tbLevelChange(object sender, EventArgs e)
        {
            if (tbLevel.Text.Length > 0)
            {
                if (int.Parse(tbLevel.Text) > 50)
                    tbLevel.Text = "50";
                // Player-Level setzen. Dies aktualisert automatisch auch die Attribute
                Unit.player.Level = int.Parse(tbLevel.Text);

                Unit.player.DisplayAttributes(); // Anzeige aktualisieren
            }
        }
        //---------------------------------------------------------------------------

        // Genereller Keyhandler für Integer-Inputboxen
        private void tbLevelKeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar > 31)
            {
                e.KeyChar = '\0';
                System.Media.SystemSounds.Beep.Play();
            }
        }
        //---------------------------------------------------------------------------

        // Keypress Handler für Float-Inputboxen
        // Vielleicht noch auswerten, das nur ein Komma möglich ist
        private void tbDPSKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!((e.KeyChar >= '0' && e.KeyChar <= '9') || e.KeyChar == '.' || e.KeyChar == ',') && e.KeyChar > 31)
            {
                e.KeyChar = '\0';
                System.Media.SystemSounds.Beep.Play();
            }
        }
        //---------------------------------------------------------------------------

        //	 Setze die Sichtbarkeiten der Bedienelemente abhängig fom Itemtype
        public void SetVisibility()           // Setze die Sichtbarkeiten entsprechend
        {
            bool bArmor = false, bWeapon = false, bDrop = false, bUnique = false, bCrafted = false;
            // Den zuletzt aktivierten Itemslot deaktiveren und neuen aktivieren

            switch (Unit.player.ItemType[iActSlot])
            {
                case EItemType.Drop:
                    bDrop = true;
                    break;
                case EItemType.Unique:
                    bUnique = true;
                    break;
                case EItemType.Crafted:
                    bCrafted = true;
                    break;
            }
            // Slotabhängige Felder (un)sichtbar machen
            if (Unit.player.Position[iActSlot] < 6)
            {
                bArmor = true;
                cbItemClass.SetHelpKeyword(_("Rüstung"));
            }
            else if (Unit.player.Position[iActSlot] < 10)
            {
                bWeapon = true;
                cbItemClass.SetHelpKeyword(_("Waffenart"));
            }
            btSaveItem.Visible = bDrop;	// Speichern-Button nur bei Drops
            chAllRealms.Visible = bDrop;
            cbQuality.Visible = bCrafted;
            tbQuality.Visible = bDrop | bUnique;
            lbArmorType.Visible = bArmor;
            tbAF.Visible = bArmor;
            lbAF.Visible = bArmor;
            tbDPS.Visible = bWeapon;
            lbDPS.Visible = bWeapon;
            tbSpeed.Visible = bWeapon;
            lbSpeed.Visible = bWeapon;
            lbWeaponType.Visible = bWeapon;
            lbDamage.Visible = bWeapon;
            cbDamage.Visible = bWeapon;
            rbItemType3.Visible = bArmor | bWeapon;	// Hier muss noch was getan werden
            lbMaxLevel.Visible = bDrop;
            tbMaxLevel.Visible = bDrop;
            udMaxLevel.Visible = bDrop;
            /*	lbCurLevel.Visible = bDrop;
                tbCurLevel.Visible = bDrop;
                udCurLevel.Visible = bDrop;
                lbActivationLevel.Visible = bDrop;
                for (int i = 0; i < 10; i++)
                {
                    teLevel[i].Visible = bDrop;
                    udLevel[i].Visible = bDrop;
                }
            */
            cbItemSubClass.Visible = bCrafted;
            cbMaterial.Visible = bCrafted;
            tbItemLevel.Enabled = !(bArmor | bWeapon);
            tbBonus.Enabled = !(bArmor | bWeapon);
            pcItem.SetTabVisible(TabCraft, bWeapon & bCrafted & mnCraftMode.Checked);
        }

        private void ItemSlotHelper()
        {
            int i, bid;
            bool bArmor = false, bWeapon = false, bCrafted = false;
            bool ModifiedState = Unit.player.Modified;

            if (bLock != 0) return;
            bLock++;
            // Kopfzeile des Itemfeldes erstellen
            string sCap;
            if (iActSlot < CPlayer.PLAYER_ITEMS)
                sCap = Unit.xml_config.arItemSlots[iActSlot].strSlotName;
            else
            {
                if (iActSlot - CPlayer.INVENTORY_ITEMS < 0)
                    sCap = _("Inventar ") + (iActSlot - CPlayer.PLAYER_ITEMS + 1).ToString();
                else
                    sCap = _("Schatzkiste ") + (iActSlot - CPlayer.INVENTORY_ITEMS + 1).ToString();
            }
            sCap += " - " + Unit.player.ItemName[iActSlot];
            gbInfo.Text = sCap;
            SetVisibility();
            // ItemType setzen
            switch (Unit.player.ItemType[iActSlot])
            {
                case EItemType.Drop:
                    rbItemType[0].Checked = true;
                    break;
                case EItemType.Unique:
                    rbItemType[1].Checked = true;
                    break;
                case EItemType.Crafted:
                    rbItemType[2].Checked = true;
                    bCrafted = true;
                    break;
            }
            // Slotabhängige Felder (un)sichtbar machen
            if (Unit.player.Position[iActSlot] < 6)
                bArmor = true;
            else if (Unit.player.Position[iActSlot] < 10)
                bWeapon = true;

            // Das Feld für die Rüstungs-/Waffenart ausfüllen
            cbItemClass.Clear();
            for (i = 0; i < Unit.xml_config.nItemClasses; i++)
            {
                if ((Unit.xml_config.arItemClasses[i].SlotType == Unit.xml_config.arItemSlots[Unit.player.Position[iActSlot]].type)
                    && (Unit.xml_config.arItemClasses[i].iRealm & Unit.player.Realm) != 0
                    && (Unit.xml_config.arItemClasses[i].bmPositions & (1 << Unit.player.Slot[iActSlot])) != 0
                    && (((Unit.player.ItemType[iActSlot] == EItemType.Crafted) ^ (Unit.xml_config.arItemClasses[i].arSubClasses.Length == 0)) | bWeapon))
                {
                    cbItemClass.Add(Unit.xml_config.arItemClasses[i].Name, -1, i);
                }
            }
            if (cbItemClass.Items.Count == 0)
            {	// Wenn kein Eintrag in der Box ist, dann sie gleich unsichtbar machen
                cbItemClass.Visible = false;
                cbItemSubClass.Visible = false;
                cbMaterial.Visible = false;
            }
            else
            {
                cbItemClass.Visible = true;
                // Und den entsprechenden Eintrag setzen
                i = Unit.player.ItemClass[iActSlot];
                if (i >= 0)
                    i = cbItemClass.Items.IndexOf(Unit.xml_config.arItemClasses[i].Name);
                if (i == -1)
                {	// Wenn noch keine Klasse festgelegt ist,
                    // dann bei Rüstungen die der Klasse,
                    // bei Waffen einfach der erste Eintrag
                    if (bWeapon && cbItemClass.Items.Count != 0) i = 0;
                    else if (!bWeapon)
                    {	// Den Eintrag suchen, der die gleiche Rüstungsklasse ist wie default der Klasse
                        int idArmor = Unit.xml_config.arClasses[Unit.player.Class].iArmor;
                        Debug.Assert(idArmor >= 0);
                        for (int j = 0; j < cbItemClass.Items.Count; j++)
                        {
                            if (Unit.xml_config.arItemClasses[cbItemClass.Data[j]].oldid == idArmor)
                            {
                                i = j;
                                break;
                            }
                        }
                    }
                }
                cbItemClass.SelectedIndex = i;
                cbItemClassChange(null, EventArgs.Empty);
            }

            // Die Daten des aktuellen Gegenstandes eintragen
            if (bNameMode)
                tbItemName.Text = Unit.player.ItemNameOriginal[iActSlot];
            else
                tbItemName.Text = Unit.player.ItemName[iActSlot];
            if (iActSlot >= CPlayer.PLAYER_ITEMS)
            {
                if (iActSlot < CPlayer.INVENTORY_ITEMS)
                    GridInventory.Rows[iActSlot - CPlayer.PLAYER_ITEMS].Cells[0].Value = tbItemName.Text;
                else
                    GridTreasury.Rows[iActSlot - CPlayer.INVENTORY_ITEMS].Cells[0].Value = tbItemName.Text;
            }
            i = Unit.player.Quality[iActSlot];
            if (bCrafted)
            {
                if ((i >= 96) & (i <= 100)) cbQuality.SelectedIndex = i - 96;
            }
            tbQuality.Text = i.ToString();
            tbBonus.Text = Unit.player.Bonus[iActSlot].ToString();
            if (bArmor)
                tbAF.Text = (Unit.player.AF[iActSlot]).ToString();
            if (bWeapon)
            {
                tbDPS.Text = (Unit.player.DPS[iActSlot] * 0.1).ToString("#0.0");
                tbSpeed.Text = (Unit.player.Speed[iActSlot] * 0.1).ToString("#0.0");
            }
            tbOrigin.Text = Unit.player.Origin[iActSlot];
            tbDescription.Text = Unit.player.Description[iActSlot];
            tbItemLevel.Text = (Unit.player.ItemLevel[iActSlot]).ToString();
            if (Unit.player.Item[iActSlot].Provider != "")
                lbProviderContent.Text = Unit.player.Item[iActSlot].Provider;
            else
                lbProviderContent.Text = _("Keine Angabe");
            udMaxLevel.Value = Unit.player.MaxLevel[iActSlot];
            seMaxLevelChange(null, EventArgs.Empty);
            udCurLevel.Value = Unit.player.CurLevel[iActSlot];
            chAllRealms.Checked = (Unit.player.ItemRealm[iActSlot] == 7);
            int effectMax = (bCrafted ? 5 : 10);
            int controlIndex;
            for (i = 0; i < effectMax; i++)
            {
                bid = Unit.player.ItemEffect[iActSlot][i];
                controlIndex = (bCrafted ? i : i + 5);
                if (bid >= 0)
                {
                    cbType[controlIndex].SelectData(Unit.xml_config.arBonuses[bid].idGroup);
                    CbTypeChange(cbType[controlIndex], EventArgs.Empty);
                    // Der richtige Effekt wird automatisch in CbTypeChange gesetzt
                    // cbEffect[i].ItemIndex = cbEffect[i].Items.IndexOf(xml_config.arBonuses[bid].Name);
                    //CbEffectChange(cbEffect[i]);	// Das hier muss trotzdem sein (Oder nicht)
                    // Die Value selbst wird wieder automatisch gesetzt
                    // cbValue[i].ItemIndex = cbValue[i].Items.IndexOf(IntToStr(player.EffectValue[idx][i]));
                    //CbValueChange(cbValue[i]);
                    udLevel[i].Value = Unit.player.EffectLevel[iActSlot][i];
                }
                else
                {
                    cbType[controlIndex].SelectedIndex = 0;
                    CbTypeChange(cbType[controlIndex], EventArgs.Empty);
                }
            }
            // Klassenbeschränkungen setzen
            ShowClassRestriction(0);
            lbUtility.Text = (Unit.player.Item[iActSlot].CalcNutzen()).ToString("##0.0");
            UpdateItemSlot(iActSlot);
            bLock--;
            tbItemLevelChange(null, EventArgs.Empty);
            Unit.player.Modified = ModifiedState;
        }

        // OnClick-Handler für die ItemSlots
        private void MItemSlotClick(object sender, EventArgs e)
        {
            // Den zuletzt aktivierten Itemslot deaktiveren und neuen aktivieren
            int idx = Utils.ConvertTagToInt(((TMItemSlot)sender).Tag);
            if (idx > 19 || iActSlot > 19)
                System.Media.SystemSounds.Beep.Play();
            ItemSlot[iActSlot].Activated = false;
            ItemSlot[idx].Activated = true;
            iActSlot = idx;

            ItemSlotHelper();
        }
        //---------------------------------------------------------------------------

        // Zeige die Klassenbeschränkungen an, mit der angegebenen Startposition oben
        public void ShowClassRestriction(int Position)
        {
            for (int i = 0; i < 4; i++)
            {
                lbRestriction[i].Text = (Position + i + 1).ToString() + ".";
                if (Unit.player.ClassRestriction[iActSlot][Position + i] >= 0)
                {
                    cbRestriction[i].SelectData(Unit.player.ClassRestriction[iActSlot][Position + i]);
                }
                else
                    cbRestriction[i].SelectedIndex = 0;
                cbRestrictionChange(cbRestriction[i], EventArgs.Empty);
            }
            sbRestrictions.Maximum = Unit.player.NClassRestrictions[iActSlot] - 4;
            sbRestrictions.Value = Position;
            sbRestrictions.Visible = (Unit.player.NClassRestrictions[iActSlot] > 4);
        }

        // Behandle die Felder in beiden Boni-Tabs, als wären sie jeweils eins
        private void CbTypeChange(object sender, EventArgs e)
        {
            int gid;
            int idx = Utils.ConvertTagToInt(((TComboBox)sender).Tag);
            int pos = (idx < 5) ? idx : idx - 5;
            bool bDrop = (idx > 4);
            gid = cbType[idx].CurrentData;
            cbEffect[pos + 5].Clear();	// Effekte in Boni2 auf jeden Fall löschen
            udLevel[pos].Value = 0;
            if (pos < 5) cbEffect[pos].Clear();	// In Boni nur, wenn Effektslot < 4
            // Effekttyp in jeweils anderen Boni-Tab setzen
            if (!bDrop)
                cbType[idx + 5].SetSelectedIndexSafe(cbType[idx].SelectedIndex);
            else
                if (pos < 5) cbType[pos].SetSelectedIndexSafe(cbType[idx].SelectedIndex);
            if (pos < 5)
            {
                cbValue[pos].SelectedIndex = -1;
                udRemakes[pos].Value = 0;
                chDone[pos].Checked = false;
            }
            tbValue[pos].Text = "";
            if (pos < 5) cbGemQuality[pos].SelectedIndex = -1;
            if (gid >= 0)
            {
                if (gid == 0)
                {	// "Unbenutzt gewählt, d.h. den Effekt löschen
                    if (bLock == 0) Unit.player.ItemEffect[iActSlot][pos] = -1;
                    // Utility neu berechnen
                    lbUtility.Text = (Unit.player.Item[iActSlot].CalcNutzen()).ToString("##0.0");
                }
                else
                {
                    // Alle möglichen Effekte in beide Effekt-Boxen eintragen
                    for (int i = 0; i < Unit.xml_config.nBonuses; i++)
                    {
                        if ((Unit.xml_config.arBonuses[i].idGroup == gid) && (Unit.xml_config.arBonuses[i].iRealm & Unit.player.Realm) != 0)
                        {
                            // Für Craftitems nur eintragen, wenn Gem craftbar
                            if ((pos < 5) && Unit.xml_config.arBonuses[i].bCraftable)
                                cbEffect[pos].Add(Unit.xml_config.arBonuses[i].Names[0], -1, i);
                            else if (pos == 4 && !Unit.xml_config.arBonuses[i].bCraftOnly)
                                cbEffect[pos].Add(Unit.xml_config.arBonuses[i].Names[0], -1, i);
                            // Effekt für Dropitems eintragen, wenn CraftOnly false ist
                            if (!Unit.xml_config.arBonuses[i].bCraftOnly)
                                cbEffect[pos + 5].Add(Unit.xml_config.arBonuses[i].Names[0], -1, i);
                        }
                    }
                }
            }
            // Wähle den obersten Eintrag oder deaktiviere das Effektfeld
            if (cbEffect[idx].Items.Count == 0)
            {	// "Unbenutzt" oder was anderes ohne Effekte gewählt
                cbEffect[pos + 5].Enabled = false;
                if (pos < 5)
                {
                    cbEffect[pos].Enabled = false;
                    cbValue[pos].Enabled = false;
                    cbGemQuality[pos].Enabled = false;
                    tbRemakes[pos].Enabled = false;
                    lbGem[pos].Text = "";
                    lbCost[pos].Text = "";
                    chDone[pos].Enabled = false;
                }
                tbValue[pos].Enabled = false;
                teLevel[pos].Enabled = false;
            }
            else
            {
                // Versuche den idealen Gemlevel zu bestimmen (nur bei Craft-Items)
                // Dazu zuerst den Effekt im Player erstmal auf -1 stellen
                /*		int IdealIP;
                        if ((!bLock) && (player.ItemType[iActSlot] == Crafted))
                        {
                            player.ItemEffect[iActSlot][pos] = -1;
                            IdealIP = CalcIdealIP(iActSlot) / 10 * 10;
                        }
                */
                cbEffect[pos + 5].Enabled = true;
                if (pos < 5)
                    cbEffect[pos].Enabled = true;
                int bid = Unit.player.ItemEffect[iActSlot][pos];
                if (cbEffect[idx].SelectData(bid) == -1)
                    cbEffect[idx].SelectedIndex = 0;
                CbEffectChange(cbEffect[idx], EventArgs.Empty);
                // Wenn dieser Effektslot nicht die letzte freie Position ist,
                // Dann testweise die Gems durchtesten, bis wir unter die idealen ip fallen
                /*		if ((!bLock) && (player.ItemType[iActSlot] == Crafted))
                        {
                            bool bFree = false;
                            for (int i = 0; i < 4; i++)
                                if (player.ItemEffect[iActSlot][i] == -1)
                                    // Es gibt ne freie position
                                    bFree = true;
                            if (bFree)
                            {
                                for (int i = 0; i < cbValue[idx].Items.Count; i++)
                                {
                                    cbValue[idx].ItemIndex = i;
                                    CbValueChange(cbValue[idx]);
                                    int ip = CalcIdealIP(iActSlot);
                                    if (ip < IdealIP)
                                    {
                                        cbValue[idx].ItemIndex = (i > 0)? i - 1 : 0;
                                        CbValueChange(cbValue[idx]);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                for (int i = cbValue[idx].Items.Count - 1; i >= 0; i--)
                                {
                                    cbValue[idx].ItemIndex = i;
                                    CbValueChange(cbValue[idx]);
                                    int ip = player.EffectIP[iActSlot][idx] * 10;
                                    if (ip <= IdealIP)
                                    {
                                        cbValue[idx].ItemIndex = i;
                                        CbValueChange(cbValue[idx]);
                                        break;
                                    }
                                }
                            }
                        }
                */
            }
            UpdateItem(iActSlot);
            UpdateAttributes();
        }
        //---------------------------------------------------------------------------

        private void CbEffectChange(object sender, EventArgs e)
        {
            int idx = Utils.ConvertTagToInt(((TComboBox)sender).Tag);
            int pos = (idx < 5) ? idx : idx - 5;
            bool bDrop = (idx > 4);
            int bid = cbEffect[idx].CurrentData;
            if (pos < 5) cbValue[pos].Clear();
            if (bid >= 0)
            {
                if (pos < 5)
                {
                    // Den Effekt im jeweiligen anderen Boni-Feld setzen
                    if (bDrop)
                    {
                        cbEffect[pos].SelectData(bid);
                    }
                    else
                        cbEffect[pos + 5].SelectData(bid);
                    // Liste der Effektwerte erstellen, wenn es ein craftable-Effekt ist
                    if (Unit.xml_config.arBonuses[bid].bCraftable || pos == 4)
                    {
                        for (int i = 0; i < 10; i++)
                            if (Unit.xml_config.arBonuses[bid].Gemvalue[i] > 0)
                            {
                                int t = Unit.xml_config.arBonuses[bid].Gemvalue[i];
                                cbValue[pos].Add(t.ToString(), -1, t);
                            }
                    }
                }
                // Trage den Effekt beim Item ein (Sollte eigentlich nur was ändern, wenn der User was eingibt)
                if (bLock == 0) Unit.player.ItemEffect[iActSlot][pos] = bid;
            }
            else
                if (!bDrop) cbValue[idx].Add("ERROR", -1, 0);
            // Alten Wert wieder setzen, wenn möglich
            int value = Unit.player.EffectValue[iActSlot][pos];
            if ((pos < 5) && (bid >= 0))
            {
                // Suche in cbValue nach einem Wert, der gleich oder niedriger wie value ist
                for (int i = cbValue[pos].Items.Count; (i--) != 0; )
                {
                    if (int.Parse(cbValue[pos].Strings[i]) <= value)
                    {
                        cbValue[pos].SelectedIndex = i;
                        break;
                    }
                }
                if (cbValue[pos].SelectedIndex == -1 && cbValue[pos].Items.Count != 0) cbValue[pos].SelectedIndex = 0;
                cbValue[pos].Enabled = true;
                cbGemQuality[pos].SelectedIndex = Unit.player.EffectQuality[iActSlot][pos];
                cbGemQuality[pos].Enabled = true;
                udRemakes[pos].Value = Unit.player.Remakes[iActSlot][pos];
                //		tbRemakes[pos].Text = player.Remakes[iActSlot][pos];
                tbRemakes[pos].Enabled = true;
                chDone[pos].Checked = Unit.player.EffectDone[iActSlot][pos];
                chDone[pos].Enabled = true;
            }
            if (pos < 5)
                CbValueChange(cbValue[pos], EventArgs.Empty);
            if (bDrop)
                tbValue[pos].Text = value.ToString();
            else
                tbValue[pos].Text = cbValue[pos].Text;
            tbValue[pos].Enabled = true;
            teLevel[pos].Enabled = true;
            if ((pos < 5) && (cbEffect[pos].SelectedIndex == -1))
            {	// Ein nicht craftbarer Effect, deshalb alles deaktivieren
                cbType[pos].SelectedIndex = 0;
                cbEffect[pos].SelectedIndex = -1;
                cbEffect[pos].Enabled = false;
                cbValue[pos].SelectedIndex = -1;
                cbValue[pos].Enabled = false;
                cbGemQuality[pos].SelectedIndex = -1;
                cbGemQuality[pos].Enabled = false;
                udRemakes[pos].Value = 0;
                tbRemakes[pos].Enabled = false;
                chDone[pos].Checked = false;
                chDone[pos].Enabled = false;
            }
            UpdateAttributes();
        }
        //---------------------------------------------------------------------------

        private void CbValueChange(object sender, EventArgs e)
        {
            int idx = Utils.ConvertTagToInt(((TComboBox)sender).Tag);
            int bid = Unit.player.ItemEffect[iActSlot][idx];
            Debug.Assert(bid >= 0);
            int mid = Unit.xml_config.GetMaterialId("GEMS");
            Debug.Assert(mid >= 0);
            if (cbValue[idx].SelectedIndex >= 0 && idx != 4)
            {
                lbGem[idx].Text = "(" + Unit.xml_config.arMaterials[mid].arLevel[cbValue[idx].SelectedIndex].GemPrefix
                    + ") " + Unit.xml_config.arBonuses[bid].GemName;
            }
            else
                lbGem[idx].Text = "";
            //	lbCost[idx].Caption = Int2Gold(player.GemCost[iActSlot][idx]);
            UpdateItem(iActSlot);
            if (bLock != 0) return;
            int value = cbValue[idx].CurrentData;
            Unit.player.EffectValue[iActSlot][idx] = value;
            tbValue[idx].Text = cbValue[idx].Text;
            lbUtility.Text = (Unit.player.Item[iActSlot].CalcNutzen()).ToString("##0.0");
            UpdateAttributes();
        }
        //---------------------------------------------------------------------------

        private void tbValueChange(object sender, EventArgs e)
        {
            if (bLock != 0) return;
            int idx = Utils.ConvertTagToInt(((TextBox)sender).Tag);
            int value = tbValue[idx].Text.ToIntDef(1);
            Unit.player.EffectValue[iActSlot][idx] = value;
            if ((idx < 5) && (cbEffect[idx].SelectedIndex >= 0))
            {	// Suche in cbValue nach einem Wert, der gleich oder niedriger wie value ist
                for (int i = cbValue[idx].Items.Count; (i--) != 0; )
                {
                    if (int.Parse(cbValue[idx].Strings[i]) <= value)
                    {
                        cbValue[idx].SelectedIndex = i;
                        break;
                    }
                }
            }
            lbUtility.Text = (Unit.player.Item[iActSlot].CalcNutzen()).ToString("##0.0");
            UpdateAttributes();
            UpdateItem(iActSlot);
        }
        //---------------------------------------------------------------------------

        private void btNameModeClick(object sender, EventArgs e)
        {
            bNameMode = !bNameMode;
            if (bNameMode)
            {	// Wir sind im Name-mode
                btNameMode.Text = _("Orginalname:");
                tbItemName.Text = Unit.player.ItemNameOriginal[iActSlot];
            }
            else
            {	// sind im Orginalmode
                btNameMode.Text = "Name:";
                tbItemName.Text = Unit.player.ItemName[iActSlot];
            }
            tbItemName.SelectionStart = tbItemName.Text.Length;
            tbItemName.Focus();
        }
        //---------------------------------------------------------------------------

        private void tbItemNameChange(object sender, EventArgs e)
        {
            if (bLock != 0) return;
            if (bNameMode)
                Unit.player.ItemNameOriginal[iActSlot] = tbItemName.Text;
            else
                Unit.player.ItemName[iActSlot] = tbItemName.Text;
        }
        //---------------------------------------------------------------------------

        private void cbGemQualityChange(object sender, EventArgs e)
        {
            if (bLock != 0) return;
            int idx = Utils.ConvertTagToInt(((TComboBox)sender).Tag);
            Unit.player.EffectQuality[iActSlot][idx] = cbGemQuality[idx].SelectedIndex;
            UpdateItem(iActSlot);
            UpdateAttributes();
        }
        //---------------------------------------------------------------------------


        private void cbItemClassChange(object sender, EventArgs e)
        {
            int SubClass;
            // zuerst den Wert speichern
            int idClass = cbItemClass.CurrentData;
            Debug.Assert(idClass != -1);
            //	if (!bLock)
            Unit.player.ItemClass[iActSlot] = idClass;

            // Nun die Box für die Subclass ausfüllen
            // Erster Eintrag ist für freie Dateneingabe
            cbItemSubClass.Clear();
            cbItemSubClass.Add(_("<freie Eingabe>"), -1, -1);
            for (int i = 0; i < Unit.xml_config.arItemClasses[idClass].arSubClasses.Length; i++)
                cbItemSubClass.Add(Unit.xml_config.arItemClasses[idClass].arSubClasses[i].Name, -1, i);

            cbItemSubClass.SetSelectedIndexSafe(Unit.player.ItemSubClass[iActSlot]);
            if (cbItemSubClass.SelectedIndex == -1) cbItemSubClass.SelectedIndex = 0;

            // Wenn es ein Waffenslot ist, dann die Damage-Combobox ausfüllen
            if (Unit.xml_config.arItemSlots[iActSlot].type == ESlotType.Weapon)
            {
                cbDamage.Clear();
                for (int i = 0; i < Unit.xml_config.arItemClasses[idClass].arSubClasses.Length; i++)
                {
                    int idDamage = Unit.xml_config.arItemClasses[idClass].arSubClasses[i].idDamage;
                    if (idDamage >= 0)
                    {	// Nur Damageliste ausfüllen, wenn die "Waffe" ne Schadensart hat
                        string sDamage = Unit.xml_config.arDamageTypes[idDamage].Name;
                        if (cbDamage.Items.IndexOf(sDamage) == -1)
                            cbDamage.Add(sDamage, -1, idDamage);
                    }
                }
                if (cbDamage.Items.Count != 0)
                {
                    // Erstmal eine Schadensart einstellen
                    cbDamage.SelectedIndex = 0;
                    cbDamage.Visible = true;
                    lbDamage.Visible = true;
                }
                else
                {
                    // Wenn die Damage-Combobox nun leer ist, dann Fald ausblenden
                    cbDamage.Visible = false;
                    lbDamage.Visible = false;
                }
            }

            //	tbAF.Visible = bArmor;
            //	lbAF.Visible = bArmor;
            bool bTemp = ((Unit.player.Position[iActSlot] > 5) && Unit.xml_config.arItemClasses[idClass].bStats);
            tbDPS.Visible = bTemp;
            lbDPS.Visible = bTemp;
            tbSpeed.Visible = bTemp;
            lbSpeed.Visible = bTemp;

            bool bFree = (Unit.player.ItemType[iActSlot] != EItemType.Crafted)
                | (Unit.xml_config.arItemClasses[idClass].arSubClasses.Length == 0);
            tbItemLevel.Enabled = bFree;
            tbBonus.Enabled = bFree;
            tbDPS.Enabled = bFree;
            tbSpeed.Enabled = bFree;
            tbAF.Enabled = bFree;
            // Subclass nur sichtbar machen, wenn es ein crafted item ist
            if (Unit.player.ItemType[iActSlot] == EItemType.Crafted)
            {
                cbItemSubClass.Visible = true;
                cbItemSubClassChange(null, EventArgs.Empty);
            }
        }
        //---------------------------------------------------------------------------

        private void cbItemSubClassChange(object sender, EventArgs e)
        {
            // zuerst den Wert speichern
            int idSubClass = cbItemSubClass.SelectedIndex;
            Debug.Assert(idSubClass != -1);
            //	if (!bLock)
            Unit.player.ItemSubClass[iActSlot] = idSubClass;
            // Die Box für das Materialstufen ausfüllen
            // Im Tag von cbMaterial den ersten Level speichern, der in der Box auftaucht
            // Das ist wichtig für die Subclassen, die keine niedrigen Materialstufen haben
            // Wenn Subclass = 0 (freie Eingabe), dann keine Materialklasse bestimmen
            bool bSubs = (Unit.player.ItemSubClass[iActSlot] > 0);
            if (bSubs)
            {
                cbMaterial.Clear();
                int idMaterial = Unit.xml_config.arItemClasses[Unit.player.ItemClass[iActSlot]].idMaterial;
                for (int i = 0; i < 10; i++)
                {
                    int val = Unit.xml_config.arItemClasses[Unit.player.ItemClass[iActSlot]].arSubClasses[Unit.player.ItemSubClass[iActSlot] - 1].arValue[i];
                    if (val > 0)
                        cbMaterial.Add(Unit.xml_config.arMaterials[idMaterial].arLevel[i].Name, -1, i);
                }
                // Materialstufe bestimmen
                if (cbMaterial.SelectData(Unit.player.Material[iActSlot]) < 0)
                    cbMaterial.SetSelectedIndexSafe(0);
                cbMaterialChange(null, EventArgs.Empty);

                // Schadensart und Speed bei Waffen einstellen
                if (Unit.xml_config.arItemSlots[iActSlot].type == ESlotType.Weapon)
                {
                    int value = Unit.xml_config.arItemClasses[Unit.player.ItemClass[iActSlot]].arSubClasses[idSubClass - 1].iSpeed;
                    tbSpeed.Text = (value * 0.1).ToString("#0.0");
                    int damage = Unit.xml_config.arItemClasses[Unit.player.ItemClass[iActSlot]].arSubClasses[idSubClass - 1].idDamage;
                    if (damage >= 0)
                        cbDamage.SelectData(damage);
                }

                // Schreibe die gebrauchten Materialien in die Felder beim "Schmied"-Tab
                for (int i = 0; i < 6; i++)
                    lbMat[i].Text = Unit.xml_config.arItemClasses[Unit.player.ItemClass[iActSlot]].arSubClasses[idSubClass - 1].iMaterial[i].ToString();
            }
            else
            {
                // Gebrauchten Materialien sind hier alle 0, da unbekannt
                for (int i = 0; i < 6; i++)
                    lbMat[i].Text = "0";
            }

            cbMaterial.Visible = bSubs;
            tbItemLevel.Enabled = !bSubs;
            tbDPS.Enabled = !bSubs;
            tbSpeed.Enabled = !bSubs;
            tbAF.Enabled = !bSubs;
            tbBonus.Enabled = !bSubs;
        }
        //---------------------------------------------------------------------------

        private void tbAFChange(object sender, EventArgs e)
        {
            if (bLock != 0) return;
            int value;
            if (tbAF.Text.Length > 0)
            {
                if (int.Parse(tbAF.Text) > 102)
                    tbAF.Text = "102";
                value = int.Parse(tbAF.Text);
                Unit.player.AF[iActSlot] = value;
                // Berechne den Itemlevel
                int ic = Unit.player.ItemClass[iActSlot];
                if ((ic >= 0) && (Unit.xml_config.arItemClasses[ic].idMaterial == Unit.xml_config.GetMaterialId("CLOTH")))
                    tbItemLevel.Text = value.ToString();
                else
                    tbItemLevel.Text = (value / 2).ToString();
            }
        }
        //---------------------------------------------------------------------------

        private void cbQualityChange(object sender, EventArgs e)
        {
            if (bLock != 0) return;
            Unit.player.Quality[iActSlot] = cbQuality.SelectedIndex + 96;
            tbQuality.Text = (cbQuality.SelectedIndex + 96).ToString();
            UpdateItem(iActSlot);
        }
        //---------------------------------------------------------------------------

        private void cbMaterialChange(object sender, EventArgs e)
        {
            int level = cbMaterial.CurrentData;

            // Entsprechenden Bonus eintragen
            if (level > 2)
                tbBonus.Text = ((level - 2) * 5).ToString();
            else if (level == 2)
                tbBonus.Text = "1";
            else
                tbBonus.Text = "0";

            // Mal schauen welchen AF-Wert diese Stufe hat und eintragen
            int value = Unit.xml_config.arItemClasses[Unit.player.ItemClass[iActSlot]].arSubClasses[Unit.player.ItemSubClass[iActSlot] - 1].arValue[level];
            if (Unit.xml_config.arItemClasses[Unit.player.ItemClass[iActSlot]].bStats)
            {
                if (iActSlot < 6)
                    tbAF.Text = value.ToString();
                else if (iActSlot < 10)
                    tbDPS.Text = (value * 0.1).ToString("#0.0");
            }
            else
            {
                tbItemLevel.Text = value.ToString();
            }

            // Es gibt immer eine Subklasse, wenn wir hier sind
            int iTotal = 0;	// Materialpreis gesamt
            for (int i = 0; i < 6; i++)
            {
                lbMatLevel[i].Text = Unit.xml_config.arMaterials[i].arLevel[level].Name;
                int iPrice = Unit.xml_config.arMaterials[i].arLevel[level].iPrice *
                    Unit.xml_config.arItemClasses[Unit.player.ItemClass[iActSlot]].arSubClasses[Unit.player.ItemSubClass[iActSlot] - 1].iMaterial[i];
                iTotal += iPrice;
                lbMatPrice[i].Text = Utils.Int2Gold(iPrice);
            }
            lbMatTotalP.Tag = iTotal;
            lbMatTotalP.Text = Utils.Int2Gold(iTotal);
            tbMarkupChange(null, EventArgs.Empty);	// Endpreis updaten
            if (bLock == 0) Unit.player.Material[iActSlot] = level;
        }
        //---------------------------------------------------------------------------

        private void rbItemTypeClick(object sender, EventArgs e)
        {
            int idx = Utils.ConvertTagToInt(((TRadioButton)sender).Tag);
            int ti = pcItem.SelectedIndex;
            switch (idx)
            {
                case 0:
                    Unit.player.ItemType[iActSlot] = EItemType.Drop;
                    pcItem.SetTabVisible(TabBoni2, true);
                    //		if (PageControl1.SelectedIndex == 1) PageControl1.SelectedIndex = 2;
                    pcItem.SetTabVisible(TabBoni, false);
                    break;
                case 1:
                    Unit.player.ItemType[iActSlot] = EItemType.Unique;
                    pcItem.SetTabVisible(TabBoni2, true);
                    //		if (PageControl1.SelectedIndex == 1) PageControl1.SelectedIndex = 2;
                    pcItem.SetTabVisible(TabBoni, false);
                    break;
                case 2:
                    Unit.player.ItemType[iActSlot] = EItemType.Crafted;
                    pcItem.SetTabVisible(TabBoni, true);
                    if (ti == 1) pcItem.SelectNextPage(false);
                    pcItem.SetTabVisible(TabBoni2, false);
                    break;
            }
            SetVisibility();
            if (bLock != 0) return;
            UpdateItem(iActSlot);
            UpdateAttributes();
            if (iActSlot < CPlayer.PLAYER_ITEMS)
                MItemSlotClick(ItemSlot[iActSlot], EventArgs.Empty);
        }
        //---------------------------------------------------------------------------


        private void tbItemLevelChange(object sender, EventArgs e)
        {
            if (bLock != 0) return;
            int level = tbItemLevel.Text.ToIntDef(0);
            Unit.player.ItemLevel[iActSlot] = level;
            CbType1_5.Enabled = (level == 51);
            if (!CbType1_5.Enabled)
            {
                CbEffect1_5.Enabled = false;
                CbValue1_5.Enabled = false;
            }
            UpdateItem(iActSlot);
        }
        //---------------------------------------------------------------------------

        private void btCreateArmorClick(object sender, EventArgs e)
        {
            TApplication.Instance.CreateForm(out Unit.frmSetArmor);
            Unit.frmSetArmor.ShowDialog();
            Unit.frmSetArmor.Dispose();
            for (int i = 0; i < 6; i++)
            {
                UpdateItemSlot(i);
            }
            if (iActSlot < CPlayer.PLAYER_ITEMS)
                MItemSlotClick(ItemSlot[iActSlot], EventArgs.Empty);
        }
        //---------------------------------------------------------------------------

        private void tbDPSChange(object sender, EventArgs e)
        {
            if (bLock != 0) return;
            int value, level;
            if (tbDPS.Text.Length > 0)
            {
                value = (int)(Utils.Str2Decimal(tbDPS.Text) * 10);
                Unit.player.DPS[iActSlot] = value;
                // Berechne den Itemlevel
                level = (value - 11) / 3;
                if (level > 51) level = 51;
                tbItemLevel.Text = level.ToString();
            }
        }
        //---------------------------------------------------------------------------

        private void cbOverloadChange(object sender, EventArgs e)
        {
            UpdateItem(iActSlot);
        }
        //---------------------------------------------------------------------------

        private void ScrollBar1Change(object sender, EventArgs e)
        {
            panScroll.Top = -ScrollBar1.Value;
        }
        //---------------------------------------------------------------------------

        // Diese Funktion soll dafür sorgen, das ein fokusiertes Objekt immer sichtbar ist
        private void OnScrollEnter(object sender, EventArgs e)
        {
            int idx = Utils.ConvertTagToInt(((Control)sender).Tag);
            if (idx > 3)
            {	// Diese Funktion macht nur für einen idx von 4 bis 13 Sinn
                // Wie groß ist der sichtbare Bereich?
                int height = Panel2.Height - panScroll.Height;
                // Nur weiter machen, wenn height < 0
                if (height < 0)
                {	// Berechne den Bereich, in dem die Zeile vollständig sichtbar ist
                    int min = ((-height / 24) + 5 - idx) * 24;
                    if (min > 0) min = 0;
                    int max = (4 - idx) * 24;
                    if (max < height) max = height;
                    if (panScroll.Top > min) panScroll.Top = min;
                    if (panScroll.Top < max) panScroll.Top = max;
                    ScrollBar1.Value = -panScroll.Top;
                }
            }
        }
        //---------------------------------------------------------------------------

        private void seMaxLevelChange(object sender, EventArgs e)
        {
            // Events in c# are triggered even if it is not fully initialized.
            // It needs no window handle to do so.
            if (!this.IsInitialized) return;
            udCurLevel.Maximum = udMaxLevel.Value;
            for (int i = 0; i < 10; i++)
            {
                udLevel[i].Maximum = udMaxLevel.Value;
                if (udLevel[i].Value > udMaxLevel.Value)
                    udLevel[i].Value = udMaxLevel.Value;
                // Wenn MaxLevel = 0, dann die Aktivierungslevel-Felder unsichtbar machen
                teLevel[i].Visible = (udMaxLevel.Value > 0);
                udLevel[i].Visible = (udMaxLevel.Value > 0);
            }
            if (udCurLevel.Value > udCurLevel.Maximum)
                udCurLevel.Value = udCurLevel.Maximum;
            // Wenn Maxlevel = 0, dann currentlevel unsichtbar
            tbCurLevel.Visible = (udMaxLevel.Value > 0);
            udCurLevel.Visible = (udMaxLevel.Value > 0);
            lbCurLevel.Visible = (udMaxLevel.Value > 0);
            lbActivationLevel.Visible = (udMaxLevel.Value > 0);
            if (udMaxLevel.Value > 0)
                chAllRealms.Checked = true;
            if (bLock == 0)
                Unit.player.MaxLevel[iActSlot] = (int)udMaxLevel.Value;
        }
        //---------------------------------------------------------------------------

        private void seCurLevelChange(object sender, EventArgs e)
        {
            if (bLock != 0) return;
            Unit.player.CurLevel[iActSlot] = (int)udCurLevel.Value;
            lbUtility.Text = (Unit.player.Item[iActSlot].CalcNutzen()).ToString("##0.0");
            UpdateAttributes();
        }
        //---------------------------------------------------------------------------

        private void seLevelChange(object sender, EventArgs e)
        {
            if (bLock != 0) return;
            int idx = Utils.ConvertTagToInt(((Control)sender).Tag);
            Unit.player.EffectLevel[iActSlot][idx] = (int)udLevel[idx].Value;
            lbUtility.Text = (Unit.player.Item[iActSlot].CalcNutzen()).ToString("##0.0");
            UpdateAttributes();
        }
        //---------------------------------------------------------------------------


        private void tbOriginChange(object sender, EventArgs e)
        {
            if (bLock == 0)
                Unit.player.Origin[iActSlot] = tbOrigin.Text;
        }
        //---------------------------------------------------------------------------

        private void btSaveItemClick(object sender, EventArgs e)
        {
            // Ein paar Tests
            // Ein Dropgegenstand muss einen Namen haben
            if (Unit.player.ItemName[iActSlot] == "")
            {
                Utils.MorasInfoMessage(_("Dropgegenstände benötigen einen Namen!"), _("Gegenstand speichern"));
                ActiveControl = tbItemName;
                return;
            }
            if (Unit.player.ItemRealm[iActSlot] == 0)
                Unit.player.ItemRealm[iActSlot] = Unit.player.Realm;
            // Dem item einen aktuellen Zeitstempel verpassen
            Unit.player.Item[iActSlot].LastUpdate = DateTime.Now;
            // Hier nun testen, obs das Item schon in DB gibt
            int ret = Unit.ItemDB.CheckItem(Unit.player.Item[iActSlot]);
            if (ret >= 0)
            {	// Ist ein Update
                TApplication.Instance.CreateForm(out Unit.frmOverWrite);
                Unit.frmOverWrite.lbName1.Text = Unit.ItemDB.GetItem(ret).Name;
                Unit.frmOverWrite.lbName1.SetHint(Unit.ItemDB.GetItem(ret).Name);
                Unit.frmOverWrite.lbDescription1.Text = Unit.ItemDB.GetItem(ret).LongInfo;
                Unit.frmOverWrite.lbName2.Text = Unit.player.ItemName[iActSlot];
                Unit.frmOverWrite.lbName2.SetHint(Unit.player.ItemName[iActSlot]);
                Unit.frmOverWrite.lbDescription2.Text = Unit.player.Item[iActSlot].LongInfo;
                DialogResult mr = Unit.frmOverWrite.ShowDialog();
                bool answerForAll = Unit.frmOverWrite.AnswerToAll;
                Unit.frmOverWrite.Dispose();
                if ((mr == DialogResult.Yes) /*| (mr == DialogResult.YesToAll)*/)
                {	// Überschreiben
                    Unit.ItemDB.UpdateItem(ret, Unit.player.Item[iActSlot]);
                }
                else if ((mr == DialogResult.No) /*| (mr == DialogResult.NoToAll)*/)
                {	// Nicht überschreiben, neues Item
                    ret = Unit.ItemDB.AddItem(Unit.player.Item[iActSlot]);
                }
                else
                    ret = -2;
            }
            else if (ret == -1)
            {
                ret = Unit.ItemDB.AddItem(Unit.player.Item[iActSlot]);
            }
            Unit.ItemDB.Save();
        }
        //---------------------------------------------------------------------------

        private void mnImportMoraClick(object sender, EventArgs e)
        {
            //	Application.CreateForm(__classid(TfrmImport), &frmImport);
            Unit.frmImport.ImportMora();
            //	delete frmImport;
        }

        private void mnImportMoraDbClick(object sender, EventArgs e)
        {
            Unit.frmImport.ImportMoraDb();
        }
        //---------------------------------------------------------------------------

        private void mnImportLeladiaClick(object sender, EventArgs e)
        {
            //	Application.CreateForm(__classid(TfrmImport), &frmImport);
            //	frmImport.ImportLeladia();
            //	delete frmImport;
        }
        //---------------------------------------------------------------------------

        private void MItemSlotDragOver(object sender, DragEventArgs e)
        {
            int didx = Utils.ConvertTagToInt(((Control)sender).Tag);	// Destination
            int sidx = Utils.ConvertTagToInt(((Control)e.GetSource()).Tag);	// Source
            // Accept wenn Source == Dest oder wenn Dest Crafted
            if ((sidx == didx) || (Unit.player.ItemType[didx] == EItemType.Crafted))
            {
                e.Effect = e.AllowedEffect & DragDropEffects.Move;
            }
        }
        //---------------------------------------------------------------------------

        // Tausche die Juwelen der angegebenen Slots
        public void SwapJewels(int dest, int src)
        {
            if (dest != src)
            {	// source muss ungleich destination sein
                int temp;
                for (int i = 0; i < Unit.player.nItemEffects[src]; i++)
                {
                    temp = Unit.player.ItemEffect[src][i];
                    Unit.player.ItemEffect[src][i] = Unit.player.ItemEffect[dest][i];
                    Unit.player.ItemEffect[dest][i] = temp;
                    temp = Unit.player.EffectValue[src][i];
                    Unit.player.EffectValue[src][i] = Unit.player.EffectValue[dest][i];
                    Unit.player.EffectValue[dest][i] = temp;
                    temp = Unit.player.EffectQuality[src][i];
                    Unit.player.EffectQuality[src][i] = Unit.player.EffectQuality[dest][i];
                    Unit.player.EffectQuality[dest][i] = temp;
                    temp = Unit.player.EffectLevel[src][i];
                    Unit.player.EffectLevel[src][i] = Unit.player.EffectLevel[dest][i];
                    Unit.player.EffectLevel[dest][i] = temp;
                }
                temp = ItemSlot[src].UsedIP;
                ItemSlot[src].UsedIP = ItemSlot[dest].UsedIP;
                ItemSlot[dest].UsedIP = temp;
                temp = ItemSlot[src].UsedSlots;
                ItemSlot[src].UsedSlots = ItemSlot[dest].UsedSlots;
                ItemSlot[dest].UsedSlots = temp;
            }
        }

        private void MItemSlotDragDrop(object sender, DragEventArgs e)
        {
            int didx = Utils.ConvertTagToInt(((Control)sender).Tag);	// Destination
            int sidx = Utils.ConvertTagToInt(((Control)e.GetSource()).Tag);	// Source
            SwapJewels(didx, sidx);
            // Das hier ist für so ne Art Doppelklick Emulation
            if (sidx == didx)
            {
                // (unnötig bei verzögertem Drag&Drop)
                /*if (DblClkTimer.Enabled && (Utils.ConvertTagToInt(DblClkTimer.Tag) == sidx))
                    MItemSlotDblClick(sender, EventArgs.Empty);
                else
                    MItemSlotClick(sender, EventArgs.Empty);
                // Starte Doppelklick-Timer neu
                DblClkTimer.Enabled = true;
                DblClkTimer.Tag = sidx;*/
            }
            else
                MItemSlotClick(sender, EventArgs.Empty);
        }
        //---------------------------------------------------------------------------

        private void MItemSlotDblClick(object sender, EventArgs e)
        {
            /*	TApplication.Instance.CreateForm(out Unit.frmSearchItem);
                CItem Item = Unit.frmSearchItem.SearchItems(iActSlot);
                Unit.frmSearchItem.Dispose();*/
            CItem Item = null;

            Unit.frmManageDB = TfrmManageDB.Create<TfrmManageDB>();
            Unit.frmManageDB.SearchMode(iActSlot);
            if (Unit.frmManageDB.ShowDialog(this) == DialogResult.OK)
                Item = Unit.frmManageDB.SearchedItem;

            if (Item != null)
            {	// Ok gedrückt, also Item laden
                Unit.player.Item[iActSlot] = Item;
            }
            Unit.frmManageDB.Dispose();

            if (iActSlot < CPlayer.PLAYER_ITEMS)
            {
                MItemSlotClick(sender, e);
                // Falls Waffenslot und noch nicht aktiv => aktivieren
                if (iActSlot >= 6 && iActSlot <= 9)
                    if (!chWeapon[iActSlot - 6].Checked)
                        chWeapon[iActSlot - 6].Checked = true;
            }
            else if (iActSlot < CPlayer.INVENTORY_ITEMS)
                GridInventoryClick(sender, e is DataGridViewCellEventArgs ?
                    (DataGridViewCellEventArgs)e :
                    new DataGridViewCellEventArgs(0, iActSlot - CPlayer.PLAYER_ITEMS));
            else
                GridTreasuryClick(sender, e is DataGridViewCellEventArgs ?
                    (DataGridViewCellEventArgs)e :
                    new DataGridViewCellEventArgs(0, iActSlot - CPlayer.INVENTORY_ITEMS));

        }

        private void MItemSlotDblClick(object sender, DataGridViewCellEventArgs e)
        {
            MItemSlotDblClick(sender, (EventArgs)e);
        }
        //---------------------------------------------------------------------------

        private void tbBonusChange(object sender, EventArgs e)
        {
            if (bLock != 0) return;
            Unit.player.Bonus[iActSlot] = tbBonus.Text.ToIntDef(0);
        }
        //---------------------------------------------------------------------------

        private void tbSpeedChange(object sender, EventArgs e)
        {
            if (bLock != 0) return;
            if (tbSpeed.Text.Length > 0)
            {
                int value = (int)(Utils.Str2Decimal(tbSpeed.Text) * 10);
                Unit.player.Speed[iActSlot] = value;
            }
        }
        //---------------------------------------------------------------------------

        private void tbQualityChange(object sender, EventArgs e)
        {
            if (bLock != 0) return;
            int value = tbQuality.Text.ToIntDef(0);
            Unit.player.Quality[iActSlot] = value;
            // Wenn wir hier eine Qualität zwischen 96 und 100 haben,
            // Dann auch die Quality-Combobox setzen
            if ((value >= 96) & (value <= 100))
                cbQuality.SelectedIndex = value - 96;
        }
        //---------------------------------------------------------------------------


        // Cinnean, 16.09.06
        // Myth. Overcaps mit eingebaut, Caps werden richtig korrigiert nun  
        private void MCapViewMouseEnter(object sender, EventArgs e)
        {
            int idx = Utils.ConvertTagToInt(((Control)sender).Tag);
            iLastMouseOver = idx;
            Attributes[idx].MouseOver = true;

            // Den Text in den Itemslots setzen, welche das Attribut haben
            int aid = Attributes[idx].Data;
            Debug.Assert(aid != -1);	// Kann nicht sein

            // Cap und Overvap ID dazu holen
            int cid = Unit.xml_config.arAttributes[aid].iCapRef;
            int overcid = Unit.xml_config.arAttributes[aid].iOvercapRef;

            for (int i = 0; i < CPlayer.PLAYER_ITEMS; i++)
            {
                // v = der Wert von dem Effekt, c = der Wert für die Cap-Erhöhung, ovc = der Overvap-Wert
                int v = 0, c = 0, ovc = 0;
                for (int n = 0; n < Unit.player.nItemEffects[i]; n++)
                {
                    int bid = Unit.player.ItemEffect[i][n];
                    if (bid >= 0)
                    {
                        for (int j = 0; j < Unit.xml_config.arBonuses[bid].arAttributes.Length; j++)
                        {
                            if (Unit.xml_config.arBonuses[bid].arAttributes[j] == aid)
                                v += Unit.player.EffectValue[i][n];
                            else if (Unit.xml_config.arBonuses[bid].arAttributes[j] == cid)
                                c += Unit.player.EffectValue[i][n];
                            else if (Unit.xml_config.arBonuses[bid].arAttributes[j] == overcid)
                                ovc += Unit.player.EffectValue[i][n];
                        }
                    }
                }
                if ((v != 0) || (c != 0) || (ovc != 0))
                {
                    string strTemp = "";
                    int ccap = Unit.player.EffCap[aid];
                    if (v > 0)
                        strTemp = "+" + (v).ToString();
                    if (v < 0)
                        strTemp = v.ToString();
                    if ((v != 0) && (c + ovc != 0))
                        strTemp += Environment.NewLine; // '\n';
                    if (c + ovc != 0)
                        strTemp += "(" + ((c + ovc < ccap) ? c + ovc : ccap).ToString() + ")";
                    ItemSlot[i].Text = strTemp;
                }
            }
        }
        //---------------------------------------------------------------------------

        private void MCapViewMouseLeave(object sender, EventArgs e)
        {
            int idx = Utils.ConvertTagToInt(((Control)sender).Tag);
            Attributes[idx].MouseOver = false;
            // Die Texte aller Itemslots löschen
            for (int i = 0; i < CPlayer.PLAYER_ITEMS; i++)
                ItemSlot[i].Text = "";
        }
        //---------------------------------------------------------------------------

        // Passt auf, das immer die richtigen Waffenslots aktiviert sind
        // Rechts => Fern und 2Hand deaktivieren
        // Links => Fern und 2Hand deaktivieren
        // 2Hand => Wenn 2Hand-Waffe, dann alle anderen, sonst nur Rechts und Fern deaktivieren
        // Fernwaffe => alle anderen deaktivieren
        private void chWeaponClick(object sender, EventArgs e)
        {
            int idx = Utils.ConvertTagToInt(((Control)sender).Tag);
            bool bChecked = chWeapon[idx].Checked;
            // Testen, ob eine 2Handwaffe im 2Hand-Slot ist
            bool b2Hand = false;
            int cid = Unit.player.ItemClass[8];
            if (cid >= 0)
            {
                if ((Unit.xml_config.arItemClasses[cid].bmPositions & 0x03c0) == 0x0100)
                    b2Hand = true;
            }
            if (bChecked)
            {
                switch (idx)
                {
                    case 0:
                        chWeapon[2].Checked = false;
                        chWeapon[3].Checked = false;
                        break;
                    case 1:
                        if (b2Hand) chWeapon[2].Checked = false;
                        chWeapon[3].Checked = false;
                        break;
                    case 2:
                        chWeapon[0].Checked = false;
                        if (b2Hand) chWeapon[1].Checked = false;
                        chWeapon[3].Checked = false;
                        break;
                    case 3:
                        chWeapon[0].Checked = false;
                        chWeapon[1].Checked = false;
                        chWeapon[2].Checked = false;
                        break;
                }
            }
            if (bLock != 0) return;
            // Checked-States in die Slots übernehmen
            for (int i = 0; i < 4; i++)
                Unit.player.Item[i + 6].bEquipped = chWeapon[i].Checked;
            UpdateAttributes();
        }
        //---------------------------------------------------------------------------

        // Tiefer liegende Comboboxen deaktivieren
        private void cbRestrictionChange(object sender, EventArgs e)
        {
            int idx = Utils.ConvertTagToInt(((Control)sender).Tag);
            int pos = sbRestrictions.Value;
            if (cbRestriction[idx].SelectedIndex == 0)
            {	// keine Klasse gewählt, also alle tiefer liegenden auf
                // <keine> setzen und deaktivieren (Nur die der Anzeige)
                for (int i = idx + 1; i < 4; i++)
                {
                    cbRestriction[i].SelectedIndex = 0;
                    cbRestriction[i].Enabled = false;
                }
                for (int i = pos + idx + 1; i < Unit.player.NClassRestrictions[iActSlot]; i++)
                {	// Das ganze nochmal für alle Beschränkungen des Items
                    Unit.player.ClassRestriction[iActSlot][i] = -1;
                }
                Unit.player.ClassRestriction[iActSlot][idx + pos] = -1;
            }
            else
            {	// Klassenid heraus finden und tiefer liegende box aktivieren
                if (bLock == 0)
                    Unit.player.ClassRestriction[iActSlot][idx + pos] = cbRestriction[idx].CurrentData;
                if (idx < 3)
                    cbRestriction[idx + 1].Enabled = true;
                else
                {	// es ist die unterste Box, also Klassen erweitern
                    sbRestrictions.Maximum = Unit.player.NClassRestrictions[iActSlot] - 4;
                    sbRestrictions.Visible = true;
                    // Außerdem das "Feld" nach oben scrollen
                    // Das funktioniert so nicht ganz
                    //			sbRestrictions.Position = pos + 1;
                    //			ShowClassRestriction(pos + 1);
                    //			cbRestriction[2].SetFocus();
                }
            }
            if (bLock == 0) UpdateAttributes();
            lbUtility.Text = (Unit.player.Item[iActSlot].CalcNutzen()).ToString("##0.0");
        }
        //---------------------------------------------------------------------------

        private void chAllRealmsClick(object sender, EventArgs e)
        {

            int iRealm = (chAllRealms.Checked) ? 7 : Unit.player.Realm;
            // Die Combobox cbClass und die für Klassenbeschränkungen ausfüllen
            for (int i = 0; i < 4; i++)
            {
                cbRestriction[i].Items.Clear();
                cbRestriction[i].Add(_("<keine>"), -1, -1);
                cbRestriction[i].SelectedIndex = 0;
            }
            for (int i = 0; i < Unit.xml_config.nClasses; i++)
            {
                if ((Unit.xml_config.arClasses[i].iRealm & iRealm) != 0)
                {
                    for (int j = 0; j < 4; j++)
                        cbRestriction[j].Add(Unit.xml_config.arClasses[i].Name, -1, i);
                }
            }
            if (bLock == 0)
                Unit.player.ItemRealm[iActSlot] = iRealm;
        }
        //---------------------------------------------------------------------------

        private void mnClearItemClick(object sender, EventArgs e)
        {
            Unit.player.ClearItem(iPopupSlot);	// Löschen
            // Und wenn idx = aktuelle Position, dann Anzeige aktualisieren
            if (iPopupSlot == iActSlot)
                MItemSlotClick(ItemSlot[iActSlot], EventArgs.Empty);
            else
            {
                UpdateItemSlot(iPopupSlot);
                UpdateAttributes();
            }
        }
        //---------------------------------------------------------------------------

        private void mnClearEffectsClick(object sender, EventArgs e)
        {
            Unit.player.ClearEffects(iPopupSlot);	// Löschen
            // Und wenn idx = aktuelle Position, dann Anzeige aktualisieren
            if (iPopupSlot == iActSlot)
                MItemSlotClick(ItemSlot[iActSlot], EventArgs.Empty);
            else
            {
                UpdateItemSlot(iPopupSlot);
                UpdateAttributes();
            }
        }
        //---------------------------------------------------------------------------

        private void pmItemSlotPopup(object sender, CancelEventArgs e)
        {
            iPopupSlot = Utils.ConvertTagToInt(pmItemSlots.SourceControl.Tag);
            // Wenn der Slot kleiner als 6, und außerdem crafted, dann "Jewelen tauschen" aktivieren
            if ((iPopupSlot < 6) && (Unit.player.ItemType[iPopupSlot] == EItemType.Crafted))
            {
                mnSwapJewels.Visible = true;
                // Jetzt noch die Positionen sichtbar machen, mit denen getauscht werden kann
                for (int i = 0; i < 6; i++)
                {	// Es kann getauscht werden wenn nicht der aktuelle Slot, und der Zielslot auch crafted ist
                    mnSwap[i].Visible = (i != iPopupSlot) && (Unit.player.ItemType[i] == EItemType.Crafted);
                }
            }
            else
                mnSwapJewels.Visible = false;
            // Wenn idx < 10, dann mnClearEffects sichtbar machen
            //	mnClearEffects.Visible = (iPopupSlot < 10);
            // Und wenn itemart crafted ist, dann auch enablen
            //	mnClearEffects.Enabled = (player.ItemType[iPopupSlot] == Crafted);
        }
        //---------------------------------------------------------------------------

        private void btClearItemClick(object sender, EventArgs e)
        {
            // temporär Itemtyp sichern
            EItemType t = Unit.player.ItemType[iActSlot];
            Unit.player.ClearItem(iActSlot);	// Löschen
            // und zurück setzen
            Unit.player.ItemType[iActSlot] = t;
            if (iActSlot < CPlayer.PLAYER_ITEMS)
                MItemSlotClick(ItemSlot[iActSlot], EventArgs.Empty);	// Und läßt die Daten im Display updaten
            else if (iActSlot < CPlayer.INVENTORY_ITEMS)
                GridInventoryClick(null, new DataGridViewCellEventArgs(0, iActSlot - CPlayer.PLAYER_ITEMS));
            else
                GridTreasuryClick(null, new DataGridViewCellEventArgs(0, iActSlot - CPlayer.INVENTORY_ITEMS));
        }
        //---------------------------------------------------------------------------

        private void btClearEffectsClick(object sender, EventArgs e)
        {
            Unit.player.ClearEffects(iActSlot);
            if (iActSlot < CPlayer.PLAYER_ITEMS)
                MItemSlotClick(ItemSlot[iActSlot], EventArgs.Empty);	// Und läßt die Daten im Display updaten
        }
        //---------------------------------------------------------------------------

        private void DblClkTimerTimer(object sender, EventArgs e)
        {
            DblClkTimer.Enabled = false;	// Zeit ist abgelaufen, und kein Neustart
        }
        //---------------------------------------------------------------------------

        private void New1Click(object sender, EventArgs e)
        {
            Unit.player = Unit.account.NewPlayer();
            Unit.player.Init();
            cbName.Add(Unit.player.Name, Unit.player);
            // Ein paar Voreinstellungen für den neuen Char
            Unit.player.Realm = Unit.xml_config.iRealm;	// Zuletzt benutztes Realm
            Player2Form();
        }
        //---------------------------------------------------------------------------

        private void tbRemakesChange(object sender, EventArgs e)
        {
            if (bLock != 0) return;
            int idx = Utils.ConvertTagToInt(((TComboBox)sender).Tag);
            Unit.player.Remakes[iActSlot][idx] = tbRemakes[idx].Text.ToIntDef(0);
            UpdateItem(iActSlot);
            UpdateAttributes();
        }
        //---------------------------------------------------------------------------

        private void mnOptionsClick(object sender, EventArgs e)
        {
            bool ModifiedState = Unit.player.Modified;
            TApplication.Instance.CreateForm(out Unit.frmOptions);
            Unit.frmOptions.ShowDialog();
            Unit.frmOptions.Dispose();
            UpdateAttributes();
            Unit.player.Modified = ModifiedState;
        }
        //---------------------------------------------------------------------------

        private void chDoneClick(object sender, EventArgs e)
        {
            if (bLock != 0) return;
            int idx = Utils.ConvertTagToInt(((TComboBox)sender).Tag);
            Unit.player.EffectDone[iActSlot][idx] = chDone[idx].Checked;
        }
        //---------------------------------------------------------------------------

        internal void mnUserOptionClick(object sender, EventArgs e)
        {
            TMainMenu.TTBXCustomItem item = (TMainMenu.TTBXCustomItem)sender;
            int idx = Utils.ConvertTagToInt(item.Tag);
            // Die Configuration laden
            SetConfig(Unit.xml_config.arMenuItems[idx].FileName);
            if (Unit.xml_config.arMenuItems[idx].bRadioItem)
            {	// Diese Sachen hier nur, wenn Menupunkt ein radioitem ist
                // Option als Checked markieren
                item.Checked = true;
                // in Registry speichern
                Utils.SetRegistryString("Option" + Unit.xml_config.arMenuItems[idx].Group, Unit.xml_config.arMenuItems[idx].Name);
            }
        }
        //---------------------------------------------------------------------------
        private void mnCraftModeClick(object sender, EventArgs e)
        {
            // Hier einfach alle Craftspezifischen Componenten aus-/einblenden
            bool bVis = mnCraftMode.Checked;
            for (int i = 0; i < 4; i++)
            {
                tbRemakes[i].Visible = bVis;
                chDone[i].Visible = bVis;
            }
            lbRemakes.Visible = bVis;
            lbDone.Visible = bVis;
            udRemakes1_1.Visible = bVis;
            udRemakes1_2.Visible = bVis;
            udRemakes1_3.Visible = bVis;
            udRemakes1_4.Visible = bVis;
            //	udRemakes1_5.Visible = bVis;
            btCraft.Visible = bVis;
            Utils.SetRegistryString("CraftMode", (bVis).ToString());
        }
        //---------------------------------------------------------------------------

        private void btCraftClick(object sender, EventArgs e)
        {
            TApplication.Instance.CreateForm(out Unit.frmCraft);
            Unit.frmCraft.ShowDialog();
            Unit.frmCraft.Dispose();
        }
        //---------------------------------------------------------------------------

        private void Panel2Resize(object sender, EventArgs e)
        {
            int max = panScroll.Height - Panel2.Height;
            if (max < 0) max = 0;
            ScrollBar1.Maximum = max;
        }
        //---------------------------------------------------------------------------

        private void mnSearchItemClick(object sender, EventArgs e)
        {
            CItem Item = null;

            Unit.frmManageDB = TfrmManageDB.Create<TfrmManageDB>();
            Unit.frmManageDB.SearchMode(iActSlot);
            if (Unit.frmManageDB.ShowDialog(this) == DialogResult.OK)
                Item = Unit.frmManageDB.SearchedItem;

            if (Item != null)
            {	// Ok gedrückt, also Item laden
                Unit.player.Item[iPopupSlot] = Item;
            }
            Unit.frmManageDB.Dispose();
            // Und wenn idx = aktuelle Position, dann Anzeige aktualisieren
            if (iPopupSlot == iActSlot)
                MItemSlotClick(ItemSlot[iActSlot], EventArgs.Empty);
            else
            {
                UpdateItemSlot(iPopupSlot);
                UpdateAttributes();
            }
            // Falls Waffenslot und noch nicht aktiv => aktivieren
            if (iPopupSlot >= 6 && iPopupSlot <= 9)
            {
                if (!chWeapon[iPopupSlot - 6].Checked)
                {
                    chWeapon[iPopupSlot - 6].Checked = true;
                }
            }
        }
        //---------------------------------------------------------------------------

        private void TfrmMain_FormClose(object sender, FormClosedEventArgs e)
        {
            Utils.SaveWindowPosition("Main", this, true);
        }
        //---------------------------------------------------------------------------


        private void btWeightsClick(object sender, EventArgs e)
        {
            TApplication.Instance.CreateForm(out Unit.frmWeights);
            if ((Unit.frmWeights.ShowDialog() == DialogResult.OK) && (iActSlot < CPlayer.PLAYER_ITEMS))
                MItemSlotClick(ItemSlot[iActSlot], EventArgs.Empty);
            Unit.frmWeights.Dispose();
            cbClassChange(null, EventArgs.Empty);
        }
        //---------------------------------------------------------------------------

        private void btSearchClick(object sender, EventArgs e)
        {
            CItem Item = null;

            Unit.frmManageDB = TfrmManageDB.Create<TfrmManageDB>();
            Unit.frmManageDB.SearchMode(iActSlot);
            if (Unit.frmManageDB.ShowDialog(this) == DialogResult.OK)
                Item = Unit.frmManageDB.SearchedItem;

            if (Item != null)
            {	// Ok gedrückt, also Item laden
                Unit.player.Item[iActSlot] = Item;
                ItemSlotHelper();
            }
            Unit.frmManageDB.Dispose();
            // Falls Waffenslot und noch nicht aktiv => aktivieren
            if (iActSlot >= 6 && iActSlot <= 9)
            {
                if (!chWeapon[iActSlot - 6].Checked)
                {
                    chWeapon[iActSlot - 6].Checked = true;
                }
            }
        }
        //---------------------------------------------------------------------------
        private void mnManageDBClick(object sender, EventArgs e)
        {
            Unit.frmManageDB = TfrmManageDB.Create<TfrmManageDB>();
            Unit.frmManageDB.ShowDialog(this);
            Unit.frmManageDB.Dispose();
        }
        //---------------------------------------------------------------------------

        private void tbItemNameKeyPress(object sender, KeyPressEventArgs e)
        {	// Bei Enter hier zwischen Name und Orginalname umschalten
            if (e.KeyChar == 13)
            {
                btNameModeClick(null, EventArgs.Empty);
                e.KeyChar = '\0';
            }
        }
        //---------------------------------------------------------------------------

        private void mnDBStatusClick(object sender, EventArgs e)
        {
            TApplication.Instance.CreateForm(out Unit.frmDBStatus);
            Unit.frmDBStatus.ShowDialog();
            Unit.frmDBStatus.Dispose();
        }
        //---------------------------------------------------------------------------

        private void sbRestrictionsChange(object sender, EventArgs e)
        {
            ShowClassRestriction(sbRestrictions.Value);
        }
        //---------------------------------------------------------------------------

        private void GridInventoryClick(object sender, DataGridViewCellEventArgs e)
        {	// Hier sowas wie bei MItemSlotClick machen
            iActSlot = e.RowIndex + CPlayer.PLAYER_ITEMS;

            ItemSlotHelper();
        }
        //---------------------------------------------------------------------------

        private void GridTreasuryClick(object sender, DataGridViewCellEventArgs e)
        {	// Hier sowas wie bei MItemSlotClick machen
            iActSlot = e.RowIndex + CPlayer.INVENTORY_ITEMS;

            ItemSlotHelper();
        }
        //---------------------------------------------------------------------------

        //TODO: Character panel was also a tab sheet, so it was only one item slot active
        // and visible at any time. Now it's separated into all-time visible panel and
        // tab page control "pcLoot", so this needs a rewrite.

        // Hier muß iActSlot aktualisiert werden
        private void pcCharacterChange(object sender, EventArgs e)
        {
            switch (((TPageControl)sender).SelectedIndex + Utils.ConvertTagToInt(((Control)sender).Tag))
            {
                case 0:	// Erste Tab. Hier muß der ItemSlot gesucht werden, der aktiv geschaltet ist
                    {
                        for (int i = 0; i < CPlayer.PLAYER_ITEMS; i++)
                        {
                            if (ItemSlot[i].Activated) iActSlot = i;
                        }
                        break;
                    }
                case 1:
                    iActSlot = GridInventory.CurrentRow.Index + CPlayer.PLAYER_ITEMS;
                    break;
                case 2:
                    iActSlot = GridTreasury.CurrentRow.Index + CPlayer.INVENTORY_ITEMS;
                    break;
            }
            ItemSlotHelper();
        }
        //---------------------------------------------------------------------------

        private void mnNewAccountClick(object sender, EventArgs e)
        {
            // Hier erstmal einen namen besorgen
            TApplication.Instance.CreateForm(out Unit.frmNewName);
            DialogResult ret = Unit.frmNewName.ShowDialog();
            if ((ret == DialogResult.OK) && (Unit.frmNewName.tbName.Text.Length > 0))
            {
                int oldcount = Unit.account.NAccounts;
                int index = Unit.account.NewAccount(Unit.frmNewName.tbName.Text);
                if (index == oldcount)
                    cbAccount.Add(Unit.account.Name[index], -1, index);
                cbAccount.SelectedIndex = index;
                cbAccountChange(null, EventArgs.Empty);
            }
            else
                System.Media.SystemSounds.Beep.Play();
            Unit.frmNewName.Dispose();
        }
        //---------------------------------------------------------------------------

        private void cbAccountChange(object sender, EventArgs e)
        {
            Unit.player.Account = cbAccount.CurrentData;
        }
        //---------------------------------------------------------------------------

        private void cbNameChange(object sender, EventArgs e)
        {
            if (iLastPlayer != cbName.SelectedIndex)
            {	// Anderer Spieler gewählt
                if (Unit.player.Modified)
                {
                    switch (Utils.MorasAskMessage(_("Der Charakter wurde geändert und noch nicht gespeichert. Möchtest du Ihn jetzt speichern?"), _("Speichern?")))
                    {
                        case (int)DialogResult.Yes: acFileSave.OnExecute(EventArgs.Empty);
                            break;
                        case (int)DialogResult.Cancel: cbName.SelectedIndex = iLastPlayer;
                            return;
                    }
                }
                iLastPlayer = cbName.SelectedIndex;
                Unit.player = (CPlayer)cbName.CurrentData;
                Player2Form();
                mnSave.Enabled = (cbName.Text.Length != 0);
                Unit.player.Name = cbName.Text;
                Unit.player.Modified = false;
            }
            else
            {
                Unit.player.Name = cbName.Text;
                mnSave.Enabled = (cbName.Text.Length != 0);
            }
        }
        //---------------------------------------------------------------------------

        private void mnImportChatLogClick(object sender, EventArgs e)
        {
            // TApplication.Instance.CreateForm(out Unit.frmImport);
            Unit.frmImport.ImportChatLog();
            // Unit.frmImport.Dispose();
        }
        //---------------------------------------------------------------------------

        private void tbDescriptionChange(object sender, EventArgs e)
        {
            if (bLock == 0)
                Unit.player.Description[iActSlot] = tbDescription.Text;
        }
        //---------------------------------------------------------------------------

        // Diese Funktion wird immer aufgerufen, wenn der Endpreis aktualisiert werden soll
        private void UpdateTotalMatPrice()
        {
            int price = (int)(Utils.ConvertTagToInt(lbMatTotalP.Tag) * (1 + Utils.Str2Decimal(tbMarkup.Text) / 100));
            price += (int)(Utils.ConvertTagToInt(lbMatTotalP.Tag) * Utils.Str2Int(tbRetries.Text) * 0.25m * (1 + Utils.Str2Decimal(tbRemakeMarkup.Text) / 100));
            lbTotalPrice.Text = Utils.Int2Gold(price);
        }

        private void tbMarkupChange(object sender, EventArgs e)
        {
            Utils.SetRegistryString("MaterialMarkup", tbMarkup.Text);
            UpdateTotalMatPrice();
        }
        //---------------------------------------------------------------------------

        private void tbRemakeMarkupChange(object sender, EventArgs e)
        {
            Utils.SetRegistryString("RemakeMarkup", tbRemakeMarkup.Text);
            UpdateTotalMatPrice();
        }
        //---------------------------------------------------------------------------

        private void tbRetriesChange(object sender, EventArgs e)
        {
            UpdateTotalMatPrice();
        }
        //---------------------------------------------------------------------------

        private void mnWhatsNewClick(object sender, EventArgs e)
        {
            string curlang = TGnuGettextInstance.GetCurrentLanguage();
            curlang = curlang.Substring(0, 2);

            TApplication.Instance.CreateForm(out Unit.frmInfo);
            Unit.frmInfo.Memo.Lines = Extensions.LoadFromFile(AppPath + "history_" + curlang + ".txt");
            Unit.frmInfo.Text = _("Was ist neu?");
            Unit.frmInfo.ShowDialog();
            Unit.frmInfo.Dispose();

        }
        //---------------------------------------------------------------------------
        private void mnContentClick(object sender, EventArgs e)
        {
            Extensions.ShellExecute(TApplication.Instance.Handle, "open", "moras.hlp", null, null, ProcessWindowStyle.Normal);
        }
        //---------------------------------------------------------------------------

        private void mnKnownProblemsClick(object sender, EventArgs e)
        {
            string curlang = TGnuGettextInstance.GetCurrentLanguage();
            curlang = curlang.Substring(0, 2);

            TApplication.Instance.CreateForm(out Unit.frmInfo);
            Unit.frmInfo.Memo.Lines = Extensions.LoadFromFile(AppPath + "knownproblems_" + curlang + ".txt");
            Unit.frmInfo.Text = _("Bekannte Probleme");
            Unit.frmInfo.ShowDialog();
            Unit.frmInfo.Dispose();
        }
        //---------------------------------------------------------------------------

        private void mnKortClick(object sender, EventArgs e)
        {
            Unit.frmImport.ImportKort();
        }
        //---------------------------------------------------------------------------


        private void btOptimizeClick(object sender, EventArgs e)
        {
            TApplication.Instance.CreateForm(out Unit.frmOptimize);
            Unit.frmOptimize.ShowDialog();
            Unit.frmOptimize.Dispose();
            Player2Form();
        }
        //---------------------------------------------------------------------------


        private void mnSwapClick(object sender, EventArgs e)
        {
            int idx = Utils.ConvertTagToInt(((ToolStripMenuItem)sender).Tag);
            SwapJewels(idx, iPopupSlot);
            MItemSlotClick(ItemSlot[iPopupSlot], EventArgs.Empty);
        }
        //---------------------------------------------------------------------------

        private void sbOthersChange(object sender, EventArgs e)
        {
            UpdateAttributes();
        }
        //---------------------------------------------------------------------------

        private void Homepage1Click(object sender, EventArgs e)
        {
            Extensions.ShellExecute(TApplication.Instance.Handle, "open", "http://moras.sf.net", null, null, ProcessWindowStyle.Normal);
        }
        //---------------------------------------------------------------------------

        private void CamelotHandwerk1Click(object sender, EventArgs e)
        {
            Extensions.ShellExecute(TApplication.Instance.Handle, "open", "http://www.camelot-handwerk.de/wbboard/board.php?boardid=24", null, null, ProcessWindowStyle.Normal);
        }
        //---------------------------------------------------------------------------

        private void N4Player1Click(object sender, EventArgs e)
        {
            Extensions.ShellExecute(TApplication.Instance.Handle, "open", "http://daoc-guide.4players.de/forums/forumdisplay.php?f=216", null, null, ProcessWindowStyle.Normal);
        }
        //---------------------------------------------------------------------------

        private void TfrmMain_FormCloseQuery(object sender, FormClosingEventArgs e)
        {
            if (Unit.player.Modified)
            {
                switch (Utils.MorasAskMessage(_("Der Charakter wurde geändert und noch nicht gespeichert. Möchtest du Ihn jetzt speichern?"), _("Speichern?")))
                {
                    case (int)DialogResult.Yes: acFileSave.OnExecute(EventArgs.Empty);
                        break;
                    // use Application.Exit[Internal] as forced exit reason, so ignore cancel button.
                    case (int)DialogResult.Cancel: if (e.CloseReason != CloseReason.ApplicationExitCall) e.Cancel = true;
                        break;
                }
            }
        }
        //---------------------------------------------------------------------------

        private void NutzenClick(object sender, EventArgs e)
        {
            TApplication.Instance.HelpSystem.ShowHelp(_("Nutzen"), "");
        }
        //---------------------------------------------------------------------------

        private void SkillClick(object sender, EventArgs e)
        {
            TMCapView tmp = (TMCapView)sender;
            if (tmp != null)
                TApplication.Instance.HelpSystem.ShowHelp(tmp.Text, "");
        }
        //---------------------------------------------------------------------------

        private void GroupClick(object sender, EventArgs e)
        {
            TGroupBox tmp = (TGroupBox)sender;
            if (tmp != null)
                TApplication.Instance.HelpSystem.ShowHelp(tmp.Text, "");
        }
        //---------------------------------------------------------------------------
        private void acCheckForUpdateExecute(object sender, EventArgs e)
        {
            string msg;
            string title;
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
                UpdateCheckInfo info = null;

                try
                {
                    info = ad.CheckForDetailedUpdate();
                }
                catch (DeploymentDownloadException dde)
                {
                    msg = _("Im Moment kann keine neue Version heruntergeladen werden.") + Environment.NewLine +
                        _("Bitte überprüfen Sie Ihre Netzwerkverbindung oder versuchen Sie es später noch einmal.") + Environment.NewLine +
                        _("Fehler: ") + dde.Message;
                    title = _("Fehler");
                    MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                catch (InvalidDeploymentException ide)
                {
                    msg = _("Auf eine neue Version kann nicht geprüft werden. Das ClickOnce-Deployment ist beschädigt.") + Environment.NewLine +
                        _("Bitte führen Sie ein neues Deployment durch und versuchen es noch einmal.") + Environment.NewLine +
                        _("Fehler: ") + ide.Message;
                    title = _("Fehler");
                    MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                catch (InvalidOperationException ioe)
                {
                    msg = _("This application cannot be updated.") + Environment.NewLine +
                       _("It is likely not a ClickOnce application.") + Environment.NewLine +
                       _("Fehler: ") + ioe.Message;
                    title = _("Fehler");
                    MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (info.UpdateAvailable)
                {
                    Boolean doUpdate = true;

                    if (!info.IsUpdateRequired)
                    {
                        msg = _("Es gibt eine neue Version. Möchten Sie jetzt das Update durchführen?");
                        title = _("Neue Version");
                        if (DialogResult.Yes != MessageBox.Show(msg, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                        {
                            doUpdate = false;
                        }
                    }
                    else
                    {
                        // Display a message that the app MUST reboot. Display the minimum required version.
                        msg = string.Format(_("Das Update zu Version {0} wurde als erforderlich eingestuft. " +
                            "Das Update wird jetzt installiert und das Programm danach neugestartet."), info.MinimumRequiredVersion);
                        title = _("Neue Version");
                        MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    if (doUpdate)
                    {
                        try
                        {
                            SQLiteUtils.SQLiteDBClose(ZQuery);
                            using (ManualResetEvent updateCompleted = new ManualResetEvent(false))
                            {
                                using (Unit.frmProgress)
                                {
                                    TApplication.Instance.CreateForm(out Unit.frmProgress);
                                    Unit.frmProgress.ControlBox = false;
                                    ad.UpdateProgressChanged += (sender2, e2) =>
                                    {
                                        if (Unit.frmProgress.pbBar.InvokeRequired)
                                            Unit.frmProgress.pbBar.Invoke(new Action<int>(v => Unit.frmProgress.pbBar.Value = v), e2.ProgressPercentage);
                                        else
                                            Unit.frmProgress.pbBar.Value = e2.ProgressPercentage;
                                    };
                                    ad.UpdateCompleted += (sender2, e2) =>
                                    {
                                        updateCompleted.Set();
                                        if (Unit.frmProgress.InvokeRequired)
                                            Unit.frmProgress.Invoke(new Action(Unit.frmProgress.Close));
                                        else
                                            Unit.frmProgress.Close();
                                    };
                                    ad.UpdateAsync();
                                    Unit.frmProgress.ShowDialog(this);
                                }
                                updateCompleted.WaitOne();
                            }
                            msg = _("Das Update war erfolgreich und das Programm wird jetzt neugestartet.");
                            title = _("Update erfolgreich");
                            MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Application.Restart();
                        }
                        catch (DeploymentDownloadException dde)
                        {
                            msg = _("Die aktuellste Version kann nicht installiert werden.") + Environment.NewLine +
                                _("Bitte überprüfen Sie Ihre Netzwerkverbindung oder versuchen Sie es später noch einmal.") + Environment.NewLine +
                                _("Fehler: ") + dde.Message;
                            title = _("Fehler");
                            MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
                else
                {
                    msg = _("Im Moment gibt es keine neue Version.");
                    title = _("Keine neue Version");
                    MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                TIniFile MorasIni = new TIniFile(AppPath + "moras.ini");
                string RegMD5 = MorasIni.ReadString("Update", "VersionMD5", "");
                MorasIni.Dispose();

                if (RegMD5 != "")
                {
                    CheckUpdateMD5 = Mougdl.Unit.CheckForUpdates("http://moras.sourceforge.net/onlineupdates/equipmentplaner");
                    if (CheckUpdateMD5 != "" && CheckUpdateMD5 != RegMD5)
                    {
                        msg = _("Es gibt eine neue Version, möchtest du auf die Homepage um sie zu installieren?");
                        title = _("Neue Version");
                        if (MessageBox.Show(msg, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            Extensions.ShellExecute(TApplication.Instance.Handle, "open", "http://moras.sourceforge.net", null, null, ProcessWindowStyle.Normal);
                    }
                    else if (sender != null)
                    {
                        msg = _("Im Moment gibt es keine neue Version.");
                        title = _("Keine neue Version");
                        MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }
        //---------------------------------------------------------------------------

        private void acNetDBAutoBestiaryExecute(object sender, EventArgs e)
        {
            Extensions.ShellExecute(TApplication.Instance.Handle, "open", "http://bestiary.telaran.org/?v=ab/bu", null, null, ProcessWindowStyle.Normal);
        }
        //---------------------------------------------------------------------------

        private void acNetDBBruderschaftExecute(object sender, EventArgs e)
        {
            Extensions.ShellExecute(TApplication.Instance.Handle, "open", "http://items.bruderschaft-des-chaos.de/item_neu_log.php", null, null, ProcessWindowStyle.Normal);
        }
        //---------------------------------------------------------------------------

        private void acNetDBLokisExecute(object sender, EventArgs e)
        {
            Extensions.ShellExecute(TApplication.Instance.Handle, "open", "http://lw.caraweb.ch/mora.php", null, null, ProcessWindowStyle.Normal);
        }
        //---------------------------------------------------------------------------

        private void acWikiMainpageExecute(object sender, EventArgs e)
        {
            TApplication.Instance.HelpSystem.ShowTableOfContents();
        }
        //---------------------------------------------------------------------------

        private void acWikiSpellcrafterExecute(object sender, EventArgs e)
        {
            TApplication.Instance.HelpSystem.ShowHelp(_("Bannzauberer"), "");
        }
        //---------------------------------------------------------------------------

        private void acWikiNewArmorExecute(object sender, EventArgs e)
        {
            TApplication.Instance.HelpSystem.ShowHelp(_("Besondere_R%C3%BCstungen"), "");
        }
        //---------------------------------------------------------------------------

        private void acNetDBItemizerExecute(object sender, EventArgs e)
        {
            Extensions.ShellExecute(TApplication.Instance.Handle, "open", "http://www.daocs-validated-items.de", null, null, ProcessWindowStyle.Normal);
        }
        //---------------------------------------------------------------------------

        private void acReportExecute(object sender, EventArgs e)
        {
            TApplication.Instance.CreateForm(out Unit.frmReport);
            Unit.frmReport.ShowDialog();
            Unit.frmReport.Dispose();
        }
        //---------------------------------------------------------------------------

        private void TfrmMain_FormShow(object sender, EventArgs e)
        {
            if (NewDatabase)
            {
                string itemsfile = DataPath + "items.xml";
                NewDatabase = false;
                if (File.Exists(itemsfile))
                {
                    string title = _("Items.xml importieren?");
                    string msg = _("Eine neue Datenbank wurde erstellt. Soll die vorhandene Items.xml importiert werden?");
                    if (MessageBox.Show(msg, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        Unit.frmImport.ImportMora(itemsfile);
                        ZConnection.DoCommit();
                    }
                }
            }
        }
        //---------------------------------------------------------------------------
        private void acDBVacuumExecute(object sender, EventArgs e)
        {
            int oldsize = (int)new FileInfo(ZConnection.GetDataSource()).Length;
            SQLiteUtils.SQLiteDBVacuum();
            int newsize = (int)new FileInfo(ZConnection.GetDataSource()).Length;
            int diff = (int)((oldsize - newsize) / 1024.0);
            string title = _("Optimieren ergebnis");
            string msg = _("Das Optimieren der Datenbank hat folgenden Speicher wieder freigegeben: ") + string.Format("{0:###,##0} KBytes", diff);
            MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        //---------------------------------------------------------------------------
    }

    static partial class Unit
    {
        internal static TfrmMain frmMain;

        internal static Config xml_config = new Config();
        internal static CMDataBase ItemDB = new CMDataBase();
        internal static Account account = new Account();
        internal static CPlayer player;
    }
}
