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
using System.Xml;
using System.Diagnostics;
using Moras.Net.IndyCustom;
using TBufferedFS = Moras.Net.Compression.UBufferedFS.TBufferedFS;
using TLZMAProgressAction = Moras.Net.Compression.LZMA.ULZMACommon.TLZMAProgressAction;
using TLZMADecoder = Moras.Net.Compression.LZMA.ULZMADecoder.TLZMADecoder;
using ULZMACommon = Moras.Net.Compression.LZMA.ULZMACommon;
using URIParser = Indy.Sockets.URIParser;
using Borland.Delphi;
using Indy.Sockets;

namespace Moras.Net.Mougdl
{
    public partial class TfoDownload : TCustomForm
    {
        private static string _(string szMsgId) { return TGnuGettextInstance.gettext(szMsgId); }
        //http://www.devsuperpage.com/search/Articles.asp?ArtID=437012
        private static readonly Func<string, IIdTextEncoding, string> s_urlEncode = ClassMethodHelper<URIParser>.CreateClassFuncDelegate<string, IIdTextEncoding, string>(() => URIParser.URLEncode(null, null, null));

        private int curFileSize;
        private int curFileIndex;
        private MemoryStream DataStream;
        private string appFile;
        private string baseFolder;
        private int step;
        private XmlNode nodeFiles;
        private XmlNode nodeFile;
        private TStringList LogFile;
        private EventHandler idleHandler;

        public string url;
        public bool selfUpdate;
        public bool filesUpdated;
        public bool newsUpdated;
        public bool errorUpdate;

        protected override IContainer GetChildContainer() { return components ?? base.GetChildContainer(); }

        public TfoDownload()
        {
            InitializeComponent();
        }
        /*
        private void ItemProgress(TLZMAProgressAction Action, Int64 Value)
        {
            pbFile.Value = (int)Value;
        }
        */
        private void ExtractFile(String ASrc, String ADest)
        {
            TBufferedFS inStream;
            TBufferedFS outStream;
            int i;
            byte[] properties = new byte[5];
            TLZMADecoder decoder;
            Int64 outSize;
            byte v;
            const int propertiesSize = 5;
            inStream = new TBufferedFS(ASrc, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            outStream = new TBufferedFS(ADest, FileMode.Create, FileAccess.Write, FileShare.Read);

            if (inStream.Read(properties, 0, propertiesSize) != propertiesSize)
                throw new Exception("input .lzma file is too short");
            decoder = new TLZMADecoder();
            if (!decoder.SetDecoderProperties(properties))
                throw new Exception("Incorrect stream properties");
            outSize = 0;
            for (i = 0; i < 8; i++)
            {
                v = ULZMACommon.ReadByte(inStream);
                if (v < 0)
                    throw new Exception("Can't read stream size");
                outSize = (long)((ulong)outSize | ((ulong)v << (8 * i)));
            }
            if (!decoder.Code(inStream, outStream, outSize))
                throw new Exception("Error in data stream");
            outStream.Dispose();
            inStream.Dispose();
        }

        private void UpdateIdle(object Sender, out Boolean Done)
        {
            string dlFile = null;
            string destFile;
            string destFileMD5;
            string msg;
            string title;
            bool destFileExist;
            bool needUpdate;
            bool deleteDestFile;
            Done = false;
            if (step == 1)
            {
                lbFile.ShowsPath = false;
                lbFile.Text = _("Download der Update-Informationen...");
                Application.DoEvents();
                // update informationen downloaden
                try
                {
                    IdHTTP.Get(url + "/moug.upd.7z", DataStream);
                    // zwischen speichern
                    DataStream.SaveToFile(baseFolder + "moug.upd.7z");
                    DataStream.SetLength(0);
                    // informationen in stream entpacken
                    ExtractFile(baseFolder + "moug.upd.7z", baseFolder + "moug.upd");
                    // xml informationen laden
                    xmlUpdate.FileName = baseFolder + "moug.upd";
                    xmlUpdate.Active = true;
                    nodeFiles = xmlUpdate.DocumentElement["files"];
                    step = 2;
                    pbAll.Maximum = nodeFiles.ChildNodes.Count;
                }
                catch (Exception E)
                {
                    step = 3;
                    errorUpdate = true;
                    msg = _("Es ist ein Fehler beim Download der Update-Informationen aufgetreten. Bitte wende dich an den Anbieter der Dateien.");
                    title = _("Download Fehler");
                    MessageBox.Show(msg + Environment.NewLine + E.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (step == 2)
            {
                nodeFile = nodeFiles.ChildNodes[curFileIndex];
                needUpdate = true;
                try
                {
                    if (nodeFile.Name == "file")
                    {
                        destFile = baseFolder + nodeFile.Attributes["folder"].Value + Path.DirectorySeparatorChar + nodeFile.InnerText;
                        deleteDestFile = (nodeFile.Attributes["delete"] != null);
                        //lbFile.Text = MinimizeName(destFile, lbFile.Canvas, lbFile.Width);
                        lbFile.ShowsPath = true;
                        lbFile.Text = destFile;
                        Application.DoEvents();
                        destFileExist = File.Exists(destFile);
                        if (!deleteDestFile)
                        {
                            if (destFileExist)
                            {
                                lbStatus.Text = _("Prüfe:");
                                Application.DoEvents();
                                try
                                {
                                    destFileMD5 = md5.MD5Print(md5.MD5File(destFile));
                                }
                                catch
                                {
                                    destFileMD5 = null;
                                }
                                needUpdate = (destFileMD5 != nodeFile.Attributes["md5"].Value);
                            }
                            if (!destFileExist || needUpdate)
                            {
                                lbStatus.Text = _("Download:");
                                Application.DoEvents();
                                filesUpdated = true;
                                dlFile = url + nodeFile.Attributes["folder"].Value + '/' + nodeFile.InnerText + ".7z";
                                dlFile = dlFile.Replace('\\', '/');
                                dlFile = s_urlEncode(dlFile, null);
                                IdHTTP.Get(dlFile, DataStream);
                                DataStream.SaveToFile(baseFolder + "moug.tmp.7z");
                                Directory.CreateDirectory(baseFolder + nodeFile.Attributes["folder"].Value);
                                UnpackFile(nodeFile.Attributes["folder"].Value, nodeFile.InnerText);
                            }
                        }
                        else if (destFileExist)
                        {
                            Log("Delete: " + nodeFile.Attributes["folder"].Value + '/' + nodeFile.InnerText);
                            File.Delete(destFile);
                            filesUpdated = true;
                        }
                    }
                }
                catch (Exception E) { HandleException(E.Message, dlFile); }
                curFileIndex++;
                pbAll.PerformStep();
                if (curFileIndex == nodeFiles.ChildNodes.Count)
                    step = 3;
            }
            else
            {
                EndIdle();
            }
        }

        private void TfoDownload_FormCreate(object sender, EventArgs e)
        {
            TGnuGettextInstance.TranslateComponent(this);
            appFile = Path.GetFileName(Application.ExecutablePath);
            baseFolder = Utils.IncludeTrailingPathDelimiter(Application.StartupPath);
            LogFile = new TStringList();
            Log("Update Log " + DateTime.Now.ToString());
            DataStream = new MemoryStream();
            step = 1;
            selfUpdate = false;
            filesUpdated = false;
            newsUpdated = false;
            errorUpdate = false;
            bool done;
            idleHandler = (s, args) => UpdateIdle(s, out done);
            Application.Idle += idleHandler;

        }

        private void IdHTTPWorkBegin(object ASender, Indy.Sockets.TWorkMode AWorkMode, long AWorkCountMax)
        {
            curFileSize = (int)AWorkCountMax;
            pbFile.Value = 0;
            DataStream.SetLength(0);
        }

        private void TfoDownload_FormDestroy(object sender, EventArgs e)
        {
            DataStream.Dispose();
            LogFile.SaveToFile(baseFolder + "update.log");
            LogFile.Clear();
            LogFile = null;
            if (idleHandler != null)
                Application.Idle -= idleHandler;
        }

        private void UnpackFile(string AFolder, string AFile)
        {
            lbStatus.Text = _("Update:");
            Application.DoEvents();
            if ((baseFolder + AFolder + Path.DirectorySeparatorChar + AFile) == Application.ExecutablePath)
            {
                selfUpdate = true;
                File.Delete(Path.ChangeExtension(Application.ExecutablePath, ".old"));
                File.Move(Application.ExecutablePath, Path.ChangeExtension(Application.ExecutablePath, ".old"));
            }
            else if (AFile.ToLower() == "mms.xml")
            {
                selfUpdate = true;
            }
            else if (AFile.ToLower() == "changelog.txt")
            {
                newsUpdated = true;
            }
            Log("Download: " + (AFolder + Path.DirectorySeparatorChar + AFile));
            ExtractFile(baseFolder + "moug.tmp.7z", baseFolder + AFolder + Path.DirectorySeparatorChar + AFile);
        }

        private void bnCancelClick(object sender, EventArgs e)
        {
            if (idleHandler != null)
                Application.Idle -= idleHandler;
            Close();
        }

        private void HandleException(string AMsg, string AFile)
        {
            string msg;
            string title;
            msg = _("Es ist ein Fehler beim Download aufgetreten. Wende dich bitte an den Anbieter der Dateien.");
            title = _("Download Fehler");
            Log("Error: " + (msg + Environment.NewLine + AMsg + Environment.NewLine + AFile));
            MessageBox.Show(msg + Environment.NewLine + AMsg + Environment.NewLine + AFile, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void IdHTTPWork(object ASender, Indy.Sockets.TWorkMode AWorkMode, long AWorkCount)
        {
            pbFile.Value = (int)Math.Round(AWorkCount / (curFileSize / 100.0));

        }

        private void EndIdle()
        {
            string msg;
            string title;
            if (idleHandler != null)
                Application.Idle -= idleHandler;
            lbFile.ShowsPath = false;
            if (errorUpdate)
                lbFile.Text = _("Es sind Fehler aufgetreten.");
            else if (filesUpdated)
                lbFile.Text = _("Online-Update beendet.");
            else
                lbFile.Text = _("Die Installation ist auf dem aktuellen Stand.");
            bnCancel.Text = _("Schliessen");
            xmlUpdate.Active = false;
            File.Delete(baseFolder + "moug.upd");
            File.Delete(baseFolder + "moug.upd.7z");
            File.Delete(baseFolder + "moug.tmp.7z");
            if (newsUpdated)
                Extensions.ShellExecute(TApplication.Instance.Handle, "open", Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "changelog.txt"), null, null, ProcessWindowStyle.Normal);
            if (selfUpdate)
            {
                msg = _("Das Programm selbst wurde aktualisiert und muss beendet werden.");
                title = _("Beenden");
                MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
            }
        }

        private void Log(string AMsg)
        {
            LogFile.Add(AMsg);
        }
    }

    // use this as global module in this namespace
    internal static partial class Unit
    {
        internal static TfoDownload foDownload;

        internal static void DownloadUpdates(string AUpdateURL)
        {
            using (foDownload = TfoDownload.Create<TfoDownload>())
            {
                foDownload.url = AUpdateURL;
                foDownload.ShowDialog(TApplication.Instance.MainForm);
            }
        }
        internal static String CheckForUpdates(string AUpdateURL)
        {
            string result = "";
            try
            {
                using (TIdHTTP conn = new TIdHTTP())
                {
                    result = conn.Get(AUpdateURL + Path.DirectorySeparatorChar + "moug-quick.upd");
                }
            }
            catch
            {
            }
            return result;
        }
    }
}

