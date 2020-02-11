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
using System.Net;
using System.Threading;
using System.IO.Compression;
using System.Globalization;
using System.Web.Script.Serialization;
using static System.Linq.Enumerable;
using static DelphiClasses.Extensions;

namespace Moras.Net
{
    public enum UpdateMode { umNormal, umVersion, umCounter, umTimeStamp };

    public partial class TfrmImport : TCustomForm
    {
        private static string _(string szMsgId) { return TGnuGettextInstance.gettext(szMsgId); }
        private string asHost;	// Server für Online-Update
        private string asPath;	// Pfad für Update
        private TStringStream asReceived;	// Empfangener Text
        private object jsonReceived;    // Parsed JSON data
        DateTime? jsonGeneratedOn;
        IDictionary<string, IDictionary<int, int>> jsonIdMappings;
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
                                    AddItem(Item, ret, ref mr, ref answerForAll, i + 1);
                                    importQuery.Next();
                                }
                            }
                            finally
                            {
                                importQuery.SetActive(false);
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
        // Für Online-Update. Es sind hier nur die Mora-xml-Files und Broadsword Json-Files gültig.
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
                            case 0:
                                asReplace = (laststamp).ToString();
                                break;
                            case 1:
                                asReplace = "0";
                                break;
                            case 2:
                                asReplace = Unit.AskForUpdateMode(this, laststamp);
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
                jsonReceived = null;
                jsonGeneratedOn = null;
                bool useWebClient = false;
                try
                {
                    Uri downloadUri = new Uri(asPath);
                    if (downloadUri.Scheme == Uri.UriSchemeHttps)
                    {
                        useWebClient = true;
                        DownloadFromUpdateUrlAsync(downloadUri);
                    }
                    else
                    {
                        IdHTTP.Get(asPath, asReceived);
                    }
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
                if (!useWebClient)
                    IdHTTP.Disconnect();
                Show();
            }
        }

        private class JsonObjectTypeResolver : JavaScriptTypeResolver
        {
            // Methods
            public override Type ResolveType(string id)
            {
                return typeof(Dictionary<string, object>);
            }

            public override string ResolveTypeId(Type type)
            {
                return string.Empty;
            }
        }


        private void DownloadFromUpdateUrlAsync(Uri downloadUri)
        {
            ManualResetEvent downloadCompleted = null;
            try
            {
                using (downloadCompleted = new ManualResetEvent(false))
                {
                    bool firstProgress = true;
                    int countProgress = 0;
                    webClient.DownloadProgressChanged += (sender, e) =>
                    {
                        if (firstProgress)
                        {
                            this.Invoke((Action)(() => IdHTTPWorkBegin(sender, TWorkMode.wmRead, e.TotalBytesToReceive)));
                            firstProgress = false;
                        }
                        this.Invoke((Action)(() => IdHTTPWork(sender, TWorkMode.wmRead, e.BytesReceived)));
                        countProgress++;
                    };
                    Exception ex = null;
                    DownloadDataCompletedEventHandler completedHandler = (sender, e) =>
                    {
                        if (e.Error != null)
                            ex = e.Error;
                        else if (e.Cancelled)
                            ex = new OperationCanceledException();
                        else
                        {
                            try
                            {
                                string contentType = webClient.ResponseHeaders[HttpResponseHeader.ContentType];
                                string charset;
                                if (Path.GetExtension(downloadUri.AbsolutePath).ToLowerInvariant() == ".zip"
                                    && contentType.ToLowerInvariant() == "application/zip")
                                {
                                    using (ZipArchive zip = new ZipArchive(new MemoryStream(e.Result), ZipArchiveMode.Read))
                                    {
                                        if (zip.Entries.Count > 1 && zip.Entries.Any(zae => zae.Name.ToLowerInvariant() == "static_objects.json"))
                                        {
                                            using (StreamReader reader = new StreamReader(zip.GetEntry("VERSION").Open(), Encoding.ASCII, true))
                                            {
                                                string line;
                                                Version gameVersion = null;
                                                while ((line = reader.ReadLine()) != null)
                                                {
                                                    string[] nvparts = line.Split(':');
                                                    if (nvparts[0] == "Game Version")
                                                        gameVersion = Version.Parse(nvparts[1]);
                                                    else if (nvparts[0] == "Generated On")
                                                        jsonGeneratedOn = DateTime.Parse(nvparts[1], CultureInfo.InvariantCulture);
                                                }
                                                Utils.DebugPrint("Game Version: {0}" + Environment.NewLine +
                                                    "Generated On: {1}", gameVersion, jsonGeneratedOn);
                                                umUpdate = UpdateMode.umTimeStamp;
                                                //TODO: use this info for item versioning
                                            }
                                            using (StreamReader reader = new StreamReader(zip.GetEntry("daoc_item_metadata.json").Open(), Encoding.UTF8, false))
                                            {
                                                JavaScriptSerializer jsonSerializer = new JavaScriptSerializer(new JsonObjectTypeResolver())
                                                {
                                                    RecursionLimit = 102,
                                                    MaxJsonLength = int.MaxValue
                                                };
                                                jsonReceived = jsonSerializer.DeserializeObject(reader.ReadToEnd());
                                                ReadUpdateMetadata();
                                                jsonReceived = null;
                                            }
                                            using (StreamReader reader = new StreamReader(zip.GetEntry("static_objects.json").Open(), Encoding.UTF8, false))
                                            {
                                                JavaScriptSerializer jsonSerializer = new JavaScriptSerializer(new JsonObjectTypeResolver())
                                                {
                                                    RecursionLimit = 102,
                                                    MaxJsonLength = int.MaxValue
                                                };
                                                jsonReceived = jsonSerializer.DeserializeObject(reader.ReadToEnd());
                                            }
                                        }
                                        else
                                        {
                                            using (StreamReader reader = new StreamReader(zip.Entries[0].Open(), true))
                                                asReceived = new TStringStream(reader.ReadToEnd(), reader.CurrentEncoding);
                                        }
                                    }
                                }
                                else if ((charset = contentType.Split(';').Skip(1).SingleOrDefault(
                                    p => p.TrimStart().StartsWith("charset", StringComparison.InvariantCultureIgnoreCase))) != null)
                                {
                                    string[] nvparts = charset.Split('=');
                                    if (nvparts.Length != 2)
                                        throw new FormatException($"Charset attribute of Content-Type header has invalid format: {charset}");
                                    Encoding enc = Encoding.GetEncoding(nvparts[1]);
                                    if (enc == null)
                                        throw new InvalidOperationException($"Can't find encoding for charset value: {nvparts[1]}");
                                    asReceived = new TStringStream(enc.GetString(e.Result), enc);
                                }
                                else
                                {
                                    using (StreamReader reader = new StreamReader(new MemoryStream(e.Result), true))
                                        asReceived = new TStringStream(reader.ReadToEnd(), reader.CurrentEncoding);
                                }
                            }
                            catch (Exception ex2)
                            {
                                ex = ex2;
                            }
                        }
                        downloadCompleted?.Set();
                    };
                    webClient.DownloadDataCompleted += completedHandler;
                    webClient.DownloadDataAsync(downloadUri);
                    while (!downloadCompleted.WaitOne(0))
                        Application.DoEvents();

                    // use this to test import without downloading data:
                    //var ciEventArgs = typeof(DownloadDataCompletedEventArgs).GetConstructor(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null,
                    //    new Type[] { typeof(byte[]) /*result*/, typeof(Exception) /*exception*/, typeof(bool) /*cancelled*/, typeof(object) /*userToken*/ }, null);
                    //completedHandler(this, (DownloadDataCompletedEventArgs)ciEventArgs.Invoke(new object[] {
                    //File.ReadAllBytes(@"...\daoc_item_database_combined.zip"),
                    //null,false, null }));

                    if (ex != null)
                        throw ex;
                }
            }
            catch (Exception)
            {
                webClient.CancelAsync();
                throw;
            }
            finally
            {
                downloadCompleted = null;
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
                            AddItem(Item, ret, ref mr, ref answerForAll, i + 1);
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
        // Bei falscher Datei wird nun null zurückgegeben. Dieser Wert kann
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
        // Importiere aus einer Json-Datei
        // INTERNE FUNKTION
        // Rückgabewert ist der aktuellste Zeitstempel bzw. null, wenn keine richtige Datei
        private DateTime? ImportJson(object json)
        {
            ICollection<object> items = null;
            if (json is IDictionary<string, object>)
            {
                items = ((IDictionary<string, object>)json)["items"] as ICollection<object>;
            }
            else if (json is ICollection<object>)
            {
                items = (ICollection<object>)json;
            }

            DateTime iLastTimeStamp = DateTime.MinValue;
            CItem Item = new CItem();
            if (items != null)
            {
                int n = 1; nItemCount = Unit.ItemDB.GetNDrops();
                bool ret;
                DialogResult mr = DialogResult.None;
                bool answerForAll = false;
                if (Utils.GetRegistryInteger("OverwriteItems", 0) != 0)
                {
                    mr = DialogResult.Yes;
                    answerForAll = true;
                }
                Show();	// Dialog anzeigen
                foreach (var item in items)
                {
                    ret = (!cancelUpdate && Item.LoadJson(item as IDictionary<string, object>, jsonIdMappings, jsonGeneratedOn));
                    Item.bEquipped = true;	// Items in der Datenbank gelten alle als equipped
                    // Test auf n, da wir beim ersten laden nur ein leeres Item bekommen
                    if (ret)
                    {	// Nun testen, ob neues Item, Updated oder sonstwas
                        if (iLastTimeStamp < Item.LastUpdate)
                            iLastTimeStamp = Item.LastUpdate;
                        int ni = Unit.ItemDB.CheckItem(Item);
                        AddItem(Item, ni, ref mr, ref answerForAll, n);
                    }
                    else if (cancelUpdate)
                        break;
                    else
                    {
                        lbCount.Text = n.ToString();
                        pbUpdate.Value = n;
                        Application.DoEvents();
                    }
                    n++;
                }
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
            if (!string.IsNullOrEmpty(Unit.xml_config.ItemUpdate.Website))
                Extensions.ShellExecute(Handle, "open", Unit.xml_config.ItemUpdate.Website, null, null, ProcessWindowStyle.Normal);
        }
        //---------------------------------------------------------------------------

        private void stFromMouseEnter(object sender, EventArgs e)
        {	// Wenn es eine URL ist, dann Farbe ändern
            if (!string.IsNullOrEmpty(Unit.xml_config.ItemUpdate.Website))
            {
                stFrom.ForeColor = Color.Red;
            }
        }
        //---------------------------------------------------------------------------

        private void stFromMouseLeave(object sender, EventArgs e)
        {	// Wenn es eine URL ist, dann alles wieder umdrehen
            if (!string.IsNullOrEmpty(Unit.xml_config.ItemUpdate.Website))
            {
                stFrom.ForeColor = SystemColors.Highlight;
            }
        }
        //---------------------------------------------------------------------------

        private void IdHTTPWorkBegin(object ASender, Indy.Sockets.TWorkMode AWorkMode, long AWorkCountMax)
        {
            DownloadSize = Math.Max((int)AWorkCountMax, 0);
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
                pbUpdate.Value = DownloadSize != 0 ? (int)(AWorkCount / (DownloadSize / 100.0)) : 100;
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

        private void ReadUpdateMetadata()
        {
            if (jsonReceived is IDictionary<string, object>)
            {
                IDictionary<string, object> meta = (IDictionary<string, object>)jsonReceived;
                jsonIdMappings = new Dictionary<string, IDictionary<int, int>>();

                meta.CreateConfigMappingFromJsonDictionary(jsonIdMappings, "realm", (key) => int.Parse(key),
                    (value) =>
                    {
                        string realm = (string)value;
                        int realmid = Utils.Realm2Int(realm);
                        if (realmid == 0)
                            throw new KeyNotFoundException(realm);
                        return realmid;
                    });

                meta.CreateConfigMappingFromJsonDictionary(jsonIdMappings, "requirements/usable_by", (key) => int.Parse(key),
                    (value) =>
                    {
                        string classname = ((string)value).ToUpper();
                        classname = classname.Replace(' ', '_');
                        classname = classname.Replace("(", "");
                        classname = classname.Replace(")", "");
                        int classid = Unit.xml_config.GetClassId(classname);
                        if (classid == -1)
                            throw new KeyNotFoundException(classname);
                        return classid;
                    });

                Dictionary<string, int> positionIdMapping = new Dictionary<string, int>();
                for (int i = 0; i < Unit.xml_config.arItemSlots.Length; i++)
                {
                    positionIdMapping.Add(Unit.xml_config.arItemSlots[i].strKortId.ToLowerInvariant(), i);
                    for (int j = 0; j < Unit.xml_config.arItemSlots[i].arIds.Length; j++)
                    {
                        positionIdMapping.Add(Unit.xml_config.arItemSlots[i].arIds[j], i); //arIds already in lowercase
                    }
                }
                meta.CreateConfigMappingFromJsonDictionary(jsonIdMappings, "slot", (key) => int.Parse(key),
                    (value) =>
                    {
                        string position = (string)value;
                        int ipos;
                        if (!positionIdMapping.TryGetValue(position.ToLowerInvariant(), out ipos))
                            throw new KeyNotFoundException(position);
                        return ipos;
                    });

                meta.CreateConfigMappingFromJsonDictionary(jsonIdMappings, "categories", (key) => int.Parse(key),
                    (value) =>
                    {
                        string cat = ((string)value).ToUpper();
                        ESlotType slotType;
                        int slotTypeId = -1;
                        if (Enum.TryParse(cat, true, out slotType))
                            slotTypeId = (int)slotType;
                        else if (cat == "SHIELD" || cat == "INSTRUMENT")
                            slotTypeId = (int)ESlotType.Weapon;
                        else
                            slotTypeId = -1;
                        return slotTypeId;
                    });

                meta.CreateConfigMappingFromJsonDictionary(jsonIdMappings, "absorption", (key) => int.Parse(key),
                    (value) =>
                    {
                        string armorType = ((string)value).ToUpper();
                        int itemClassId = Unit.xml_config.GetItemClassId(armorType);
                        if (itemClassId == -1)
                            throw new KeyNotFoundException(armorType);
                        return itemClassId;
                    });

                meta.CreateConfigMappingFromJsonDictionary(jsonIdMappings, "shield_size", (key) => int.Parse(key),
                    (value) =>
                    {
                        string shieldType = "SHIELD_" + ((string)value).ToUpper();
                        int itemClassId = Unit.xml_config.GetItemClassId(shieldType);
                        if (itemClassId == -1)
                            throw new KeyNotFoundException(shieldType);
                        return itemClassId;
                    });

                meta.CreateConfigMappingFromJsonDictionary(jsonIdMappings, "damage_type", (key) => int.Parse(key),
                    (value) =>
                    {
                        string dmgType = ((string)value).ToUpper();
                        int dmgTypeId = Unit.xml_config.GetDamageType(dmgType);
                        if (dmgTypeId == -1)
                            throw new KeyNotFoundException(dmgType);
                        return dmgTypeId;
                    }, (id) => id > 3); //only melee damage types can be mapped

                IDictionary<int, Dictionary<string, int>> bonusesbygrps = (from i in Range(0, Unit.xml_config.nBonuses)
                                                                           group i by Unit.xml_config.arBonuses[i].idGroup).ToDictionary(
                    g => g.Key, g => (from ib in g
                                      from name in new[] {
                                          Unit.xml_config.arBonuses[ib].id,
                                          Unit.xml_config.arBonuses[ib].KortId.ToUpper() }
                                      .Concat(Range(0, Unit.xml_config.arBonuses[ib].nNames)
                                      .Select(j => Unit.xml_config.arBonuses[ib].Names[j].ToUpper()))
                                      .Where(n => !string.IsNullOrEmpty(n))
                                      .Select(n => n.Replace(" ", ""))
                                      group new { name, ib } by name).ToDictionary(g2 => g2.Key, g2 => g2.First().ib));
                Dictionary<string, IDictionary<int, IDictionary<int, int>>> bonus_types = new Dictionary<string, IDictionary<int, IDictionary<int, int>>>();
                Func<string, int> findMatchingGroupId = (sgrp) =>
                {
                    sgrp = sgrp.Replace(" ", "");
                    for (int i = 0; i < Unit.xml_config.nGroups; i++)
                    {
                        string id = Unit.xml_config.arGroups[i].id.Replace(" ", "");
                        string name = Unit.xml_config.arGroups[i].Name.ToUpper().Replace(" ", "");
                        if (string.Compare(sgrp, 0, id, 0, Math.Min(sgrp.Length, id.Length)) == 0 ||
                        string.Compare(sgrp, 0, name, 0, Math.Min(sgrp.Length, name.Length)) == 0)
                            return i;
                    }
                    // hard coded mapping for lack of config data
                    switch (sgrp)
                    {
                        case "HITPOINTS":
                            return Unit.xml_config.GetGroupId("HITS");
                        case "TOAOVERCAP":
                        case "TOAHITPOINTSCAP":
                        case "TOAPOWERPOOLCAP":
                            return Unit.xml_config.GetGroupId("CAP_INC");
                        case "MYTHICALRESISTANCECAP":
                        case "MYTHICALCAPINCREASE":
                            return Unit.xml_config.GetGroupId("OVERCAP_INC");
                        case "MYTHICALRESISTANDCAP":
                            return Unit.xml_config.GetGroupId("RESIST_OVERCAP");
                        case "MYTHICALSTATANDCAPINCREASE":
                            return Unit.xml_config.GetGroupId("STAT_OVERCAP");
                        default:
                            return Unit.xml_config.GetGroupId("OTHERS");
                    }
                };
                int idFocus = Unit.xml_config.GetGroupId("FOCUS");
                int idSkill = Unit.xml_config.GetGroupId("SKILL");

                Func<int, string, int> findmatchingBonusId = (grp, sbonus) =>
                {
                    int bonusId = -1;
                    bool repeat;
                    do
                    {
                        repeat = false;
                        if (bonusesbygrps[grp].TryGetValue(sbonus, out bonusId))
                            return bonusId;
                        foreach (var bonus in bonusesbygrps[grp])
                        {
                            if (string.Compare(sbonus, 0, bonus.Key, 0, Math.Min(sbonus.Length, bonus.Key.Length)) == 0)
                                return bonus.Value;
                        }
                        if (grp == idSkill) // special case for focus bonus, it's grouped as skill in json and as focus in config
                        {
                            grp = idFocus;
                            repeat = true;
                        }
                    } while (repeat);
                    return -1;
                };
                meta.CreateConfigMappingFromJsonDictionary(bonus_types, "bonus_types", (key) => int.Parse(key),
                    (value) =>
                    {
                        var bonus = (IDictionary<string, object>)value;
                        object name;
                        if (!bonus.TryGetValue("name", out name))
                            throw new KeyNotFoundException("name");
                        int grp = findMatchingGroupId(((string)name).ToUpper());
                        object sub_types;
                        IDictionary<string, object> subtypes = null;
                        if (bonus.TryGetValue("sub_types", out sub_types))
                            subtypes = sub_types as IDictionary<string, object>;
                        else if (bonus.TryGetValue("id", out sub_types))
                            subtypes = sub_types as IDictionary<string, object>;
                        var bonusMapping = from st in (subtypes ?? new Dictionary<string, object> { { "0", name } })
                                           let stkey = int.Parse(st.Key)
                                           let stvalue = ((string)st.Value).ToUpper().Replace(" ", "")
                                           let bonusid = findmatchingBonusId(grp, stvalue)
                                           select CreateKeyValuePair(stkey, bonusid);
                        //var onetoone = bonusMapping.Where(kvp => kvp.Value != -1 && kvp.Value != 25).ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
                        //Console.WriteLine(onetoone);
                        return bonusMapping.ToDictionary();
                    });

                var allbonusesMapping = from bt in bonus_types["bonus_types"]
                                        let key = (bt.Key << 16)
                                        from st in bt.Value
                                        select CreateKeyValuePair(key | st.Key, st.Value);
                jsonIdMappings.Add("bonus_types", allbonusesMapping.ToDictionary());

                // there is no information in json data, but we need these mappings to determine item classes,
                // so create it hard coded here as long as there is no better solution
                Dictionary<int, int> skillMap = new Dictionary<int, int>();
                jsonIdMappings.Add("type_data/skill_used", skillMap);
                skillMap.Add(1, Unit.xml_config.GetAttributeId("SLASH"));
                skillMap.Add(2, Unit.xml_config.GetAttributeId("THRUST"));
                skillMap.Add(4, Unit.xml_config.GetAttributeId("SHORT_BOW"));
                skillMap.Add(14, Unit.xml_config.GetAttributeId("SWORD"));
                skillMap.Add(16, Unit.xml_config.GetAttributeId("HAMMER"));
                skillMap.Add(17, Unit.xml_config.GetAttributeId("AXE"));
                skillMap.Add(26, Unit.xml_config.GetAttributeId("SPEAR"));
                skillMap.Add(33, Unit.xml_config.GetAttributeId("CRUSH"));
                skillMap.Add(44, Unit.xml_config.GetAttributeId("THROWN_WEAPONS"));
                skillMap.Add(46, Unit.xml_config.GetAttributeId("FLEXIBLE"));
                // there is no all-realm staff skill, there is only staff skill on alb for friar
                //skillMap.Add(47, Unit.xml_config.GetAttributeId("STAFF"));
                skillMap.Add(64, Unit.xml_config.GetAttributeId("POLEARM"));
                skillMap.Add(65, Unit.xml_config.GetAttributeId("TWO_HANDED"));
                skillMap.Add(78, Unit.xml_config.GetAttributeId("COMPOSITE_BOW"));
                skillMap.Add(90, Unit.xml_config.GetAttributeId("LONGBOW"));
                skillMap.Add(91, Unit.xml_config.GetAttributeId("CROSSBOW"));
                skillMap.Add(101, Unit.xml_config.GetAttributeId("BLADES"));
                skillMap.Add(102, Unit.xml_config.GetAttributeId("BLUNT"));
                skillMap.Add(103, Unit.xml_config.GetAttributeId("PIERCING"));
                skillMap.Add(104, Unit.xml_config.GetAttributeId("LARGE_WEAPONRY"));
                skillMap.Add(112, Unit.xml_config.GetAttributeId("CELTIC_SPEAR"));
                skillMap.Add(113, Unit.xml_config.GetAttributeId("RECURVE_BOW"));
                skillMap.Add(121, Unit.xml_config.GetAttributeId("PAINWORKING"));
                skillMap.Add(124, Unit.xml_config.GetAttributeId("HAND2HAND"));
                skillMap.Add(125, Unit.xml_config.GetAttributeId("SCYTHE"));
                skillMap.Add(147, Unit.xml_config.GetAttributeId("FIST_WRAPS"));
                skillMap.Add(148, Unit.xml_config.GetAttributeId("MAULER_STAFF"));

                Dictionary<int, int> instrumentHibMap = new Dictionary<int, int>();
                Dictionary<int, int> instrumentAlbMap = new Dictionary<int, int>();
                jsonIdMappings.Add("instrument_hib", instrumentHibMap);
                jsonIdMappings.Add("instrument_alb", instrumentAlbMap);
                Action<IEnumerable<int>, int, int> addInstrumentMapping = (keys, hibid, albid) =>
                {
                    foreach (int key in keys)
                    {
                        instrumentHibMap.Add(key, hibid);
                        instrumentAlbMap.Add(key, albid);
                    }
                };
                addInstrumentMapping(new[] { 227, 2117, 2970, 2973, 2976, 2979, 3848 },
                    Unit.xml_config.GetItemClassId("MUSIC_LUTE"),
                    Unit.xml_config.GetItemClassId("INSTRUMENT_LUTE"));
                addInstrumentMapping(new[] { 228, 2114, 2971, 2974, 2977, 2980 },
                    Unit.xml_config.GetItemClassId("MUSIC_DRUM"),
                    Unit.xml_config.GetItemClassId("INSTRUMENT_DRUM"));
                addInstrumentMapping(new[] { 2115, 2972, 2975, 2978, 2981 },
                    Unit.xml_config.GetItemClassId("MUSIC_FLUTE"),
                    Unit.xml_config.GetItemClassId("INSTRUMENT_FLUTE"));
                addInstrumentMapping(new[] { 2116, 3239, 3280, 3688, 3731, 3908, 3985, 4313, 4350, 4391 },
                    Unit.xml_config.GetItemClassId("MUSIC_HARP"),
                    Unit.xml_config.GetItemClassId("INSTRUMENT_HARP"));
            }
        }

        private int JsonItemUpdateCount(object json)
        {
            if (json is IDictionary<string, object>)
            {
                return JsonItemUpdateCount(((IDictionary<string, object>)json)["items"]);
            }
            else if (json is ICollection<object>)
            {
                return ((ICollection<object>)json).Count;
            }
            return 0;
        }

        private void ReadUpdate()
        {
            if (!cancelUpdate)
            {
                stStatus.Text = _("Update empfangen. Werte aus...");
                pbUpdate.Value = 0;
                IStringStreamWrapper iss = null;
                CXml xFile = null;
                if (jsonReceived == null)
                {
                    pbUpdate.Maximum = (asReceived.DataString.Length / 455); // durchschnittliche xml datensatz länge 440 byte

                    // Stringstream
                    iss = new IStringStreamWrapper(asReceived.DataString);
                    asReceived.Free();
                    asReceived = null;

                    xFile = new CXml();
                    xFile.AttachInputStream(iss);
                }
                else
                    pbUpdate.Maximum = JsonItemUpdateCount(jsonReceived);

                DateTime? ret;
                if ((ret = (jsonReceived == null ? ImportXml(xFile) : ImportJson(jsonReceived))).HasValue)
                {
                    FinishedUpdate();
                    // Ein paar Sachen je nach Updatemodi ausführen
                    if ((!cancelUpdate) && (umUpdate == UpdateMode.umTimeStamp))
                    {   // Timestamp speichern
                        Utils.SetRegistryString("Update" + Unit.xml_config.ItemUpdate.Registry + "_ts", ret.ToString());
                    }
                    if ((!cancelUpdate) && (umUpdate == UpdateMode.umCounter))
                    {   // Zähler laden, incrementieren und neu speichern
                        int cnt = Utils.GetRegistryString("Update" + Unit.xml_config.ItemUpdate.Registry + "_cnt", "0").ToIntDef(0) + 1;
                        Utils.SetRegistryString("Update" + Unit.xml_config.ItemUpdate.Registry + "_cnt", cnt.ToString());
                    }
                }
                else
                {   // Ist keine Item-Datei
                    stStatus.Text = _("Keine Gegenstandsdaten in der Datei!");
                    finishedUpdate = true;
                    return;
                }
                if (jsonReceived == null)
                {
                    xFile.DetachInputStream();
                    xFile.CloseXml();

                    iss.Dispose();
                }
                else
                    jsonReceived = null;
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
