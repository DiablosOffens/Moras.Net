using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DelphiClasses;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace dxgettext
{
    internal class TDomain
    {
        private bool Enabled;
        private string vDirectory;
        private TMoFile mofile;
        private string SpecificFilename;
        private string curlang;
        private Boolean OpenHasFailedBefore;

        public TDebugLogger DebugLogger;
        public string Domain;
        public string Directory { get { return vDirectory; } set { setDirectory(value); } }

        private void setDirectory(string dir)
        {
            vDirectory = dir[dir.Length - 1] == Path.DirectorySeparatorChar ? dir : dir + Path.DirectorySeparatorChar;
            SpecificFilename = "";
            CloseMoFile();
        }

        private void OpenMoFile()
        {
            string filename;
            // Check if it is already open
            if (mofile != null)
                return;

            // Check if it has been attempted to open the file before
            if (OpenHasFailedBefore)
                return;

            if (SpecificFilename != "")
            {
                filename = SpecificFilename;
#if DXGETTEXTDEBUG
                DebugLogger("Domain " + Domain + " is bound to specific file " + filename);
#endif
            }
            else
            {
                filename = Directory + curlang + Path.DirectorySeparatorChar + "LC_MESSAGES" + Path.DirectorySeparatorChar + Domain + ".mo";
                if (!TFileLocator.FileExists(filename) && !File.Exists(filename))
                {
#if DXGETTEXTDEBUG
                    DebugLogger("Domain " + Domain + ": File does not exist, neither embedded or in file system: " + filename);
#endif
                    filename = Directory + curlang.Substring(0, 2) + Path.DirectorySeparatorChar + "LC_MESSAGES" + Path.DirectorySeparatorChar + Domain + ".mo";
#if DXGETTEXTDEBUG
                    DebugLogger("Domain " + Domain + " will attempt to use this file: " + filename);
#endif
                }
                else
                {
#if DXGETTEXTDEBUG
                    if (TFileLocator.FileExists(filename))
                        DebugLogger("Domain " + Domain + " will attempt to use this embedded file: " + filename);
                    else
                        DebugLogger("Domain " + Domain + " will attempt to use this file that was found on the file system: " + filename);
#endif
                }
            }
            if (!TFileLocator.FileExists(filename) && !File.Exists(filename))
            {
#if DXGETTEXTDEBUG
                DebugLogger("Domain " + Domain + " failed to locate the file: " + filename);
#endif
                OpenHasFailedBefore = true;
                return;
            }
#if DXGETTEXTDEBUG
            DebugLogger("Domain " + Domain + " now accesses the file.");
#endif
            mofile = TFileLocator.GetMoFile(filename, DebugLogger);

#if DXGETTEXTDEBUG
            if (mofile.isSwappedArchitecture)
                DebugLogger(".mo file is swapped (comes from another CPU architecture)");
#endif

            // Check, that the contents of the file is utf-8
            if (GetTranslationProperty("Content-Type").ToUpper().IndexOf("CHARSET=UTF-8") == -1)
            {
                CloseMoFile();
#if DXGETTEXTDEBUG
                DebugLogger("The translation for the language code " + curlang + " (in " + filename + ") does not have charset=utf-8 in its Content-Type. Translations are turned off.");
#endif
                MessageBox.Show(null, "The translation for the language code " + curlang + " (in " + filename + ") does not have charset=utf-8 in its Content-Type. Translations are turned off.", "Localization problem", MessageBoxButtons.OK);
                Enabled = false;
            }
        }

        private void CloseMoFile()
        {
            if (mofile != null)
            {
                TFileLocator.ReleaseMoFile(mofile);
                mofile = null;
            }
            OpenHasFailedBefore = false;
        }

        public TDomain()
        {
            Enabled = true;
        }

        public void SetLanguageCode(string langcode)
        {
            CloseMoFile();
            curlang = langcode;
        }

        public void SetFilename(string filename)
        {
            CloseMoFile();
            vDirectory = "";
            SpecificFilename = filename;
        }

        public void GetListOfLanguages(TStringList list)
        {
            string filename, path;
            string langcode;
            int j;
            list.Clear();

            // Iterate through filesystem
            foreach (string name in System.IO.Directory.EnumerateDirectories(Directory, "*"))
            {
                if (name != "." && name != "..")
                {
                    filename = Directory + name + Path.DirectorySeparatorChar + "LC_MESSAGES" + Path.DirectorySeparatorChar + Domain + ".mo";
                    if (File.Exists(filename))
                    {
                        langcode = name.ToLower();
                        if (list.IndexOf(langcode) == -1)
                            list.Add(langcode);
                    }
                }
            }

            // Iterate through embedded files
            for (int i = 0; i < TFileLocator.filelist.Count; i++)
            {
                filename = TFileLocator.basedirectory + TFileLocator.filelist.Strings[i];
                path = Directory;
                path = path.ToUpper();
                filename = filename.ToUpper();
                j = path.Length;
                if (filename.Substring(0, j) == path)
                {
                    path = Path.DirectorySeparatorChar + "LC_MESSAGES" + Path.DirectorySeparatorChar + Domain + ".mo";
                    path = path.ToUpper();
                    if (filename.Substring(filename.Length - path.Length, path.Length) == path)
                    {
                        langcode = filename.Substring(j, filename.Length - path.Length - j).ToLower();
                        langcode = langcode.Substring(0, 3) + langcode.Substring(3).ToUpper();
                        if (list.IndexOf(langcode) == -1)
                            list.Add(langcode);
                    }
                }
            }
        }

        public string GetTranslationProperty(string Propertyname)
        {
            Propertyname = Propertyname.ToUpper() + ": ";
            TStringList sl = new TStringList();
            try
            {
                sl.Text = gettext("");
                for (int i = 0; i < sl.Count; i++)
                {
                    string s = sl.Strings[i];
                    if (s.Length > Propertyname.Length && s.Substring(0, Propertyname.Length).ToUpper() == Propertyname)
                    {
                        string Result = s.Substring(Propertyname.Length).Trim();

#if DXGETTEXTDEBUG
                        DebugLogger("GetTranslationProperty(" + Propertyname + ") returns ''" + Result + "''.");
#endif
                        return Result;
                    }
                }
            }
            finally
            {
            }
#if DXGETTEXTDEBUG
            DebugLogger("GetTranslationProperty(" + Propertyname + ") did not find any value. An empty string is returned.");
#endif
            return "";
        }

        public string gettext(string msgid)
        {
            Boolean found;

            if (!Enabled)
            {
                return msgid;
            }
            if (mofile == null && !OpenHasFailedBefore)
                OpenMoFile();
            if (mofile == null)
            {
#if DXGETTEXTDEBUG
                DebugLogger(".mo file is not open. Not translating \"" + msgid + "\"");
#endif
                return msgid;
            }
            else
            {
                string Result = mofile.gettext(msgid, out found);
#if DXGETTEXTDEBUG
                if (found)
                    DebugLogger("Found in .mo (" + Domain + "): \"" + msgid + "\"->\"" + Result + "\"");
                else
                    DebugLogger("Translation not found in .mo file (" + Domain + ") : \"" + msgid + "\"");
#endif
                return Result;
            }
        }
    }
}
