using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using DelphiClasses;
using dxgettext;
using Indy.Sockets;
using System.Diagnostics;
using Moras.Net.IndyCustom;
using Borland.Vcl;
using System.Data.SQLite;

namespace Moras.Net
{
    public enum UpdateMode { umNormal, umVersion, umCounter, umTimeStamp };

    public partial class TfrmImport : TCustomForm
    {
        private static string _(string szMsgId) { return TGnuGettextInstance.gettext(szMsgId); }
        private string asHost;	// Server für Online-Update
        private string asPath;	// Pfad für Update
        private TStringStream asReceived;	// Empfangener Text
        private int nUpdated;	// Interne Zählvariable für die geupdateten Items
        private int nNew;	// Interner Zähler für die neuen Items
        private int nItemCount; // Anzahl Items vor dem Update
        private bool bUpdated;	// Wahr, wenn irgendwas an der Datenbank geändert wurde
        private bool cancelUpdate;
        private bool finishedUpdate;
        private bool errorUpdate;
        private bool ModifiedState;
        private UpdateMode umUpdate;
        private int DownloadSize;

        public bool HasMessage;

        protected override IContainer GetChildContainer() { return components ?? base.GetChildContainer(); }

        //---------------------------------------------------------------------------
        public TfrmImport()
        {
            InitializeComponent();
            ((Bitmap)btOK.Image).MakeTransparent();
            ((Bitmap)btCancel.Image).MakeTransparent();
        }
        //---------------------------------------------------------------------------
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams Params = base.CreateParams;
                Params.Parent = IntPtr.Zero;
                return Params;
            }
        }
        //---------------------------------------------------------------------------

        // ImportFrame ist momentan der einzige Frame, der nicht bei Bedarf geladen wird.
        // Dies deshalb, weil durch die Show()-Funktionen nicht sichergestellt ist, das
        // der Dialog bei beendigen der Import-Funktion nicht geschlossen wird.

        // Initialisierungsfunktion für alle Import-Funktionen
        // Immer am Anfang aufrufen!
        private void StartImport()	// Vor jeder Import-Funktion ausführen
        {
            nUpdated = 0;
            bUpdated = false;
            cancelUpdate = false;
            finishedUpdate = false;
            errorUpdate = false;
            HasMessage = false;
            ModifiedState = Unit.player.Modified;
            nNew = 0;
            lbItems.Text = Unit.ItemDB.NItems.ToString();
            stStatus.Text = "";
            stStatus.Visible = false;
            pbUpdate.Value = 0;
            pbUpdate.Maximum = 100;
            lbCount.Text = "0";
            lbNew.Text = "0";
            lbUpdated.Text = "0";
            btCancel.Text = _("&Abbrechen");
            IdHTTP.ProxyParams.Clear();
            if (Utils.GetRegistryInteger("UseProxy", 0) != 0)
            {
                IdHTTP.ProxyParams.BasicAuthentication = Utils.GetRegistryInteger("AlternateLogin", 0) != 0;
                IdHTTP.ProxyParams.ProxyServer = Utils.GetRegistryString("ProxyServer", "");
                IdHTTP.ProxyParams.ProxyPort = Utils.GetRegistryString("ProxyPort", "").ToIntDef(0);
                IdHTTP.ProxyParams.ProxyUsername = Utils.GetRegistryString("ProxyUser", "");
                IdHTTP.ProxyParams.ProxyPassword = Utils.GetRegistryString("ProxyPasswd", "");
            }
        }
        //---------------------------------------------------------------------------
        public void ImportMora(string AFile = "")
        {
            StartImport();
            btOK.Enabled = false;
            dlgOpen.Filter = _("Moras Itemdatei (*.xml)|*.xml");
            if (AFile != "" || dlgOpen.ShowDialog() == DialogResult.OK)
            {	// Nur importieren, wenn wir auch eine Datei gewählt haben :)
                Cursor.Current = Cursors.WaitCursor;
                if (AFile == "")
                    AFile = dlgOpen.FileName;
                stFrom.ShowsPath = true; //TODO: set to false on other places
                stFrom.Text = AFile;
                pbUpdate.Value = 0;
                pbUpdate.Maximum = (int)(new FileInfo(AFile).Length / 455); // durchschnittliche xml datensatz länge 440 byte
                Application.DoEvents();
                CXml xFile = new CXml();
                if (xFile.OpenXml(AFile))
                {
                    DateTime? result = ImportXml(xFile);
                    if (!result.HasValue)
                    {	// Ist keine Item-Datei
                        Cursor.Current = Cursors.Default;
                        CancelUpdate();
                        if (!HasMessage)
                            Utils.MorasInfoMessage(_("Die gewählte Datei enthält keine Gegenstandsdaten."), _("Fehler"));
                        return;
                    }
                    xFile.CloseXml();
                }
                FinishedUpdate();
            }
            Cursor.Current = Cursors.Default;
        }

        internal void ImportMoraDb(string AFile = "")
        {
            StartImport();
            btOK.Enabled = false;
            dlgOpen.Filter = _("Moras Itemdatenbank|items.db3");
            if (AFile != "" || dlgOpen.ShowDialog() == DialogResult.OK)
            {	// Nur importieren, wenn wir auch eine Datei gewählt haben :)
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    if (AFile == "")
                        AFile = dlgOpen.FileName;
                    stFrom.ShowsPath = true; //TODO: set to false on other places
                    stFrom.Text = AFile;
                    Application.DoEvents();

                    using (SQLiteConnection otherConnection = new SQLiteConnection(Unit.frmMain.ZConnection.ConnectionString))
                    {
                        otherConnection.SetDataSource(AFile);
                        otherConnection.Open();
                        using (SQLiteCommand importQuery = new SQLiteCommand(otherConnection))
                        {
                            try
                            {
                                int version = SQLiteUtils.SQLiteDBVersion(importQuery);
                                if (version == 0)
                                {
                                    Utils.MorasInfoMessage(_("Die gewählte Datei ist nicht kompatibel mit der Moras Datenbank."), _("Fehler"));
                                    return;
                                }
                                else if (version < 4)
                                {
                                    // set type mapping for ansi string
                                    otherConnection.ClearTypeMappings();
                                    otherConnection.AddTypeMapping("VARCHAR2", DbType.Binary, true);
                                    otherConnection.Flags |= SQLiteConnectionFlags.UseConnectionTypes;
                                }

                                DialogResult mr = DialogResult.None;
                                bool answerForAll = false;
                                if (Utils.GetRegistryInteger("OverwriteItems", 0) != 0)
                                {
                                    mr = DialogResult.Yes;
                                    answerForAll = true;
                                }
                                importQuery.SetActive(false);
                                importQuery.CommandText = "select * from items";
                                importQuery.SetActive(true);
                                int itemcount = importQuery.GetRecordCount();
                                nItemCount = Unit.ItemDB.GetNDrops();
                                pbUpdate.Value = 0;
                                pbUpdate.Maximum = itemcount;
                                Application.DoEvents();
                                CItem Item;
                                Show();	// Dialog anzeigen

                                for (int i = 0; i < itemcount; i++)
                                {
                                    Item = Unit.ItemDB.GetItem(importQuery);
                                    int ret = Unit.ItemDB.CheckItem(Item);
                                    AddItem(Item, ret, ref  mr, ref answerForAll, i + 1);
                                    importQuery.Next();
                                }
                            }
                            finally
                            {
                                SQLiteUtils.SQLiteDBClose(importQuery);
                            }
                        }
                    }
                    Unit.ItemDB.Save();

                    FinishedUpdate();
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }
        //---------------------------------------------------------------------------
        // Für Online-Update. Es sind hier nur die Mora-xml-Files gültig
        public void ImportInternet()
        {
            int pos1, pos2;
            StartImport();
            stStatus.Visible = true;
            btOK.Enabled = false;
            if (Unit.xml_config.ItemUpdate.bFound)
            {	// Zur Sicherheit erst starten, wenn auch ItemUpdate gefunden wurde
                // Trenne URL in Host und Pfad
                //		int pos = xml_config.ItemUpdate.URL.AnsiPos("/");
                //		asHost = xml_config.ItemUpdate.URL.SubString(1, pos - 1);
                //		asPath = xml_config.ItemUpdate.URL.SubString(pos, INT_MAX);
                asPath = Unit.xml_config.ItemUpdate.URL;
                umUpdate = UpdateMode.umNormal;
                // Eventuelle Platzhalter im Path ersetzen
                if ((pos1 = asPath.IndexOf('[')) != -1)
                {	// Es gibt einen Platzhalter
                    pos2 = asPath.IndexOf(']');
                    string asReplace = asPath.Substring(pos1 + 1, pos2 - pos1 - 1);
                    if (asReplace == "version")
                    {	// Durch Programmversion ersetzen
                        int major = Unit.frmMain.iMajor, minor = Unit.frmMain.iMinor;
                        asReplace = (major).ToString() + "." + (minor).ToString();
                        umUpdate = UpdateMode.umVersion;
                    }
                    else if (asReplace == "counter")
                    {	// Durch eine laufende Versionsnummer ersetzen
                        // Dazu die letzte erfolgreiche Nummer auslesen (aus Registry)
                        int counter = Utils.GetRegistryString("Update" + Unit.xml_config.ItemUpdate.Registry + "_cnt", "0").ToIntDef(0) + 1;
                        asReplace = (counter).ToString();
                        umUpdate = UpdateMode.umCounter;
                    }
                    else if (asReplace == "timestamp")
                    {	// Durch den aktuellsten, von der Seite empfangenen Timestamp ersetzen
                        // timestamp als Unix-Zeitstempel
                        // Aus Registry letzen Wert laden
                        int mode = Utils.GetRegistryInteger("UpdateMode", 0);
                        int laststamp = Utils.GetRegistryString("Update" + Unit.xml_config.ItemUpdate.Registry + "_ts", "0").ToIntDef(0);

                        switch (mode)
                        {
                            case 0: asReplace = (laststamp).ToString();
                                break;
                            case 1: asReplace = "0";
                                break;
                            case 2: asReplace = Unit.AskForUpdateMode(this, laststamp);
                                break;
                        }
                        umUpdate = UpdateMode.umTimeStamp;
                    }
                    asPath = asPath.Substring(0, pos1) + asReplace + asPath.Substring(pos2 + 1);
                }
                stFrom.Font = new Font(stFrom.Font, FontStyle.Underline);
                stFrom.ForeColor = SystemColors.Highlight;
                stFrom.Cursor = Cursors.Hand;
                stFrom.Text = Unit.xml_config.ItemUpdate.Name;
                Show();	// Sollte gehen, da alles über Events geht
                stStatus.Text = _("Öffne Verbindung...");
                asReceived = new TStringStream("");
                try
                {
                    IdHTTP.Get(asPath, asReceived);
                    Show();
                    ReadUpdate();
                }
                catch (Exception e)
                {
                    Show();
                    stStatus.Text = (_("Fehler: ") + e.Message).Replace(Environment.NewLine, ", ");
                    errorUpdate = true;
                    CancelUpdate();
                }
                IdHTTP.Disconnect();
                Show();
            }
        }
        //---------------------------------------------------------------------------
        // Importiere Kort-Items
        // Verzeichnisstruktur ist dabei wie bei Leladia, außer das es auch ein "All"-Verzeichnis gibt
        // Daten sind allerdings xml
        // Es kommt noch ein spezifischer Dialog für ein paar Import-Optionen
        public void ImportKort()
        {
            StartImport();
            int i;
            dlgOpen.Filter = "Alle Items|*.xml|";
            // Die weiteren templates arbeiten nicht richtig (sehr langsam)
            /*		"Rüstungen|*arms.xml;*chest.xml;*boots.xml;*hands.xml;*helm.xml;*legs.xml|"\
                    "Schmuck|*belt.xml;*cloak.xml;*jewel.xml;*neck.xml;*ring.xml;*bracer.xml|"\
                    "Waffen/Schilde/Instumente|*2hwep.xml;*lhwep.xml;*ranged.xml;*wep.xml";
            */
            dlgOpen.Multiselect = true;
            btOK.Enabled = false;
            DialogResult mr = DialogResult.None;
            bool answerForAll = false;
            if (dlgOpen.ShowDialog() == DialogResult.OK)
            {	// Nur importieren, wenn wir auch eine Datei gewählt haben :)
                Cursor.Current = Cursors.WaitCursor;
                nItemCount = Unit.ItemDB.GetNDrops();
                pbUpdate.Value = 0;
                pbUpdate.Maximum = dlgOpen.FileNames.Length;
                Application.DoEvents();
                CItem Item = new CItem();
                Show();	// Dialog anzeigen
                for (i = 0; i < dlgOpen.FileNames.Length; i++)
                {
                    CXml xFile = new CXml();
                    if (xFile.OpenXml(dlgOpen.FileNames[i]))
                    {	// Es ist immer nur ein Item in einer Datei
                        if (xFile.isTag("SCItem"))
                        {
                            Item.LoadKort(xFile);
                            // LastUpdate-Zeit ist das Dateidatum
                            Item.LastUpdate = File.GetLastWriteTime(dlgOpen.FileNames[i]);
                        }
                        else
                        {	// Ist keine Item-Datei
                            Utils.MorasErrorMessage(_("Datei '") + dlgOpen.FileNames[i] + _("' enthält keine Gegenstandsdaten."), _("Fehler"));
                            return;
                        }
                        xFile.CloseXml();
                    }
                    int ret;
                    if ((Item.Position > 5) && (Item.Position < 10))
                        ret = -2;	// Waffenklasse noch nicht implementiert, also auch nicht adden
                    else
                        ret = Unit.ItemDB.CheckItem(Item);
                    AddItem(Item, ret, ref mr, ref answerForAll, i + 1);
                }
                FinishedUpdate();
                btOK.Focus();
            }
            Cursor.Current = Cursors.Default;
        }
        //---------------------------------------------------------------------------
        public void ImportChatLog()
        {
            StartImport();
            dlgOpen.Filter = "Chatlog-Datei (*.log)|*.log";
            // Hier eventuell noch das DAoC-Verzeichnis einstellen
            btOK.Enabled = false;
            DialogResult mr = DialogResult.None;
            bool answerForAll = false;
            if (dlgOpen.ShowDialog() == DialogResult.OK)
            {	// Nur importieren, wenn wir auch eine Datei gewählt haben :)
                Cursor.Current = Cursors.WaitCursor;
                Application.DoEvents();
                CItem Item = new CItem();
                Show();	// Dialog anzeigen
                using (CChatLog @in = new CChatLog())
                {
                    @in.OpenChatLog(dlgOpen.FileName);
                    @in.Init();
                    nItemCount = Unit.ItemDB.GetNDrops();
                    pbUpdate.Value = 0;
                    pbUpdate.Maximum = @in.NItems;
                    Application.DoEvents();
                    for (int i = 0; i < @in.NItems; i++)
                    {
                        if (@in.GetItem(i, Item) == true)
                        {
                            DialogResult mr2 = 0;
                            // Zuerst feststellen, ob Rüstungsposition abgefragt werden muß
                            if (Item.Position < 0)
                                mr2 = Utils.AskForPosition(Item);
                            // Nun testen, ob neues Item, Updated oder sonstwas
                            int ret;
                            if ((Item.Position > 5) && (Item.Position < 10) && (Item.Class < 0))
                                ret = -2;	// Waffenklasse noch nicht implementiert, also auch nicht adden
                            else
                                ret = Unit.ItemDB.CheckItem(Item);
                            if (mr2 == DialogResult.Cancel) ret = -2;	// Abbruch beim Rüstungs/Waffen/Schmuckdialog gedrückt
                            AddItem(Item, ret, ref  mr, ref answerForAll, i + 1);
                        }
                    }
                }
                FinishedUpdate();
                btOK.Focus();
            }
            Cursor.Current = Cursors.Default;
        }
        //---------------------------------------------------------------------------
        private void btOKClick(object sender, EventArgs e)
        {
            // Speichere die importierten Items
            Cursor.Current = Cursors.WaitCursor;
            Application.DoEvents();
            Unit.frmMain.ZConnection.DoCommit();
            Cursor.Current = Cursors.Default;
            Hide();
        }
        //---------------------------------------------------------------------------
        private void FinishedUpdate()
        {
            btOK.Enabled = !cancelUpdate;
            btCancel.Enabled = true;
            finishedUpdate = true;
            if (cancelUpdate)
                btCancel.Text = _("Schliessen");
            else
                btCancel.Text = _("&Nicht speichern");
            if (!cancelUpdate)
                stStatus.Text = _("Fertig.");
        }
        //---------------------------------------------------------------------------
        private void CancelUpdate()
        {
            cancelUpdate = true;
            if (IdHTTP.Connected())
                IdHTTP.Disconnect();
            if (!errorUpdate)
                stStatus.Text = _("Abgebrochen.");
            FinishedUpdate();
        }
        //---------------------------------------------------------------------------
        private void btCancelClick(object sender, EventArgs e)
        {
            if (!finishedUpdate)
                CancelUpdate();
            else
            {
                DialogResult = DialogResult.Cancel;
                Cursor.Current = Cursors.WaitCursor;
                Application.DoEvents();
                Unit.frmMain.ZConnection.DoRollBack();
                Cursor.Current = Cursors.Default;
                Hide();
            }
        }
        //---------------------------------------------------------------------------
        // Importiere aus einer xml-Datei
        // INTERNE FUNKTION
        // Rückgabewert ist der aktuellste Zeitstempel bzw. -1, wenn keine richtige Datei
        // Cinnean: Änderung wieder von true/false auf timestamp-Werte,
        // da sonst die Timestamp-Speicherung in der Regestry korrumpiert wird
        // Bei falscher Datei wird nun -1 zurückgegeben. Dieser Wert kann
        // bei richtiger Datei nicht als Rückggabewert auftreten
        private DateTime? ImportXml(CXml xFile)
        {
            DateTime iLastTimeStamp = DateTime.MinValue;
            CItem Item = new CItem();
            if (xFile.isTag("daoc_items"))
            {
                int n = 0; nItemCount = Unit.ItemDB.GetNDrops();
                bool ret;
                DialogResult mr = DialogResult.None;
                bool answerForAll = false;
                if (Utils.GetRegistryInteger("OverwriteItems", 0) != 0)
                {
                    mr = DialogResult.Yes;
                    answerForAll = true;
                }
                Show();	// Dialog anzeigen
                do
                {
                    ret = (Item.Load(xFile) && !cancelUpdate);
                    Item.bEquipped = true;	// Items in der Datenbank gelten alle als equipped
                    // Test auf n, da wir beim ersten laden nur ein leeres Item bekommen
                    if (n > 0)
                    {	// Nun testen, ob neues Item, Updated oder sonstwas
                        if (iLastTimeStamp < Item.LastUpdate)
                            iLastTimeStamp = Item.LastUpdate;
                        int ni = Unit.ItemDB.CheckItem(Item);
                        AddItem(Item, ni, ref mr, ref answerForAll, n);
                    }
                    n++;
                } while (ret == true);
                pbUpdate.Value = pbUpdate.Maximum;
                return iLastTimeStamp;
            }
            else
                return null;
        }
        //---------------------------------------------------------------------------
        // Interne Hilfsfunktion für alle Update-Funktionen
        // Soll aufgerufen werden, nachdem die Item-Struktur ausgefüllt wurde
        // Testet ob ein Item schon in DB ist, ob Überschrieben werden soll
        // Und updated die angezeigten Werte
        // nr ist die bereits gefundene Nummer in DB
        // mr ist der Ergebniswert des Überschreiben-Dialogs
        // cnt ist die Nummer des Items
        private void AddItem(CItem Item, int nr, ref DialogResult mr, ref bool answerForAll, int cnt)
        {
            if ((nr >= 0) && !Item.Deleted)
            {	// Ist ein Update.
                CItem DBItem = Unit.ItemDB.GetItem(nr);
                if (DBItem.LastUpdate < Item.LastUpdate)
                {	// Wenn das Datum des neuen Items neuer ist dann evtl Überschreiben
                    if (!answerForAll)
                    {
                        TApplication.Instance.CreateForm(out Unit.frmOverWrite);
                        Unit.frmOverWrite.lbName1.Text = DBItem.Name;
                        Unit.frmOverWrite.lbName1.SetHint(DBItem.Name);
                        Unit.frmOverWrite.lbDescription1.Text = DBItem.LongInfo;
                        Unit.frmOverWrite.lbName2.Text = Item.Name;
                        Unit.frmOverWrite.lbName2.SetHint(Item.Name);
                        Unit.frmOverWrite.lbDescription2.Text = Item.LongInfo;
                        mr = Unit.frmOverWrite.ShowDialog(this);
                        answerForAll = Unit.frmOverWrite.AnswerToAll;
                        Unit.frmOverWrite.Dispose();
                        Unit.frmOverWrite = null;
                    }
                    if (mr == DialogResult.Yes)
                    {	// Überschreiben
                        Unit.ItemDB.UpdateItem(nr, Item);
                        nUpdated++;
                        bUpdated = true;
                    }
                    else if (mr == DialogResult.No)
                    {	// Nicht überschreiben, neues Item
                        nr = -1;
                    }
                    else
                        nr = -2;
                }
                DBItem = null;
            }
            if ((nr == -1) && !Item.Deleted)
            {	// Item ist neu. Master?
                /*int master = Utils.GetRegistryString("MasterKey", "0").ToIntDef(0);
                if (master > 0)
                {	// Sind Masterprogramm. UID vergeben und Masterkey erhöhen
                    Item.iUID = master++;
                    Utils.SetRegistryString("MasterKey", master.ToString());
                }*/
                nNew++;
                Item.Changed = true;	// Als geändert markieren
                Unit.ItemDB.AddItem(Item);
                bUpdated = true;
            }
            lbItems.Text = (nItemCount + nNew).ToString();
            lbNew.Text = nNew.ToString();
            lbUpdated.Text = nUpdated.ToString();
            lbCount.Text = cnt.ToString();
            pbUpdate.Value = cnt;
            Application.DoEvents();
        }
        //---------------------------------------------------------------------------
        private void stFromClick(object sender, EventArgs e)
        {	// Wenn es eine URL ist, dann Browser öffnen
            if (Unit.xml_config.ItemUpdate.Website.Length > 0)
                Extensions.ShellExecute(Handle, "open", Unit.xml_config.ItemUpdate.Website, null, null, ProcessWindowStyle.Normal);
        }
        //---------------------------------------------------------------------------

        private void stFromMouseEnter(object sender, EventArgs e)
        {	// Wenn es eine URL ist, dann Farbe ändern
            if (Unit.xml_config.ItemUpdate.Website.Length > 0)
            {
                stFrom.ForeColor = Color.Red;
            }
        }
        //---------------------------------------------------------------------------

        private void stFromMouseLeave(object sender, EventArgs e)
        {	// Wenn es eine URL ist, dann alles wieder umdrehen
            if (Unit.xml_config.ItemUpdate.Website.Length > 0)
            {
                stFrom.ForeColor = SystemColors.Highlight;
            }
        }
        //---------------------------------------------------------------------------

        private void IdHTTPWorkBegin(object ASender, Indy.Sockets.TWorkMode AWorkMode, long AWorkCountMax)
        {
            DownloadSize = (int)AWorkCountMax;
            stStatus.Text = _("Empfangen Update...");
            try
            {
                stStatus.Text = stStatus.Text + string.Format(" ({0:0.00} MB)", ((DownloadSize / 1024.0) / 1024.0));
            }
            catch
            { }
        }
        //---------------------------------------------------------------------------

        private void IdHTTPWork(object ASender, Indy.Sockets.TWorkMode AWorkMode, long AWorkCount)
        {
            try
            {
                pbUpdate.Value = (int)(AWorkCount / (DownloadSize / 100.0));
            }
            catch
            { }
        }
        //---------------------------------------------------------------------------

        private void IdHTTPConnected(object ASender)
        {
            stStatus.Text = _("Verbindung hergestellt. Suche Updates...");
        }
        //---------------------------------------------------------------------------

        private void ReadUpdate()
        {
            if (!cancelUpdate)
            {
                pbUpdate.Value = 0;
                pbUpdate.Maximum = (asReceived.DataString.Length / 455); // durchschnittliche xml datensatz länge 440 byte
                stStatus.Text = _("Update empfangen. Werte aus...");
                // Stringstream
                IStringStreamWrapper iss = new IStringStreamWrapper(asReceived.DataString);
                asReceived.Free();
                asReceived = null;

                CXml xFile = new CXml();
                xFile.AttachInputStream(iss);
                DateTime? ret;
                if ((ret = ImportXml(xFile)).HasValue)
                {
                    FinishedUpdate();
                    // Ein paar Sachen je nach Updatemodi ausführen
                    if ((!cancelUpdate) && (umUpdate == UpdateMode.umTimeStamp))
                    {	// Timestamp speichern
                        Utils.SetRegistryString("Update" + Unit.xml_config.ItemUpdate.Registry + "_ts", ret.ToString());
                    }
                    if ((!cancelUpdate) && (umUpdate == UpdateMode.umCounter))
                    {	// Zähler laden, incrementieren und neu speichern
                        int cnt = Utils.GetRegistryString("Update" + Unit.xml_config.ItemUpdate.Registry + "_cnt", "0").ToIntDef(0) + 1;
                        Utils.SetRegistryString("Update" + Unit.xml_config.ItemUpdate.Registry + "_cnt", cnt.ToString());
                    }
                }
                else
                {	// Ist keine Item-Datei
                    stStatus.Text = _("Keine Gegenstandsdaten in der Datei!");
                    finishedUpdate = true;
                    return;
                }
                xFile.DetachInputStream();
                xFile.CloseXml();

                iss.Dispose();
            }
        }
        //---------------------------------------------------------------------------

        private void TfrmImport_FormCreate(object sender, EventArgs e)
        {
            TGnuGettextInstance.TranslateComponent(this);
        }
        //---------------------------------------------------------------------------



        private void FormClose(object sender, FormClosedEventArgs e)
        {
            Unit.player.Modified = ModifiedState;
        }
    }
    //---------------------------------------------------------------------------

    static partial class Unit
    {
        internal static TfrmImport frmImport;
    }
}
