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
using Microsoft.Win32;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.Threading;
using System.ComponentModel;
using DelphiClasses;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
#if MAIN_PROJEKT
using System.Deployment.Application;
#endif

namespace Moras.Net
{
    internal static class Utils
    {
        public static void get(this StreamReader istream, out char value)
        {
            int result = istream.Read();
            if (result == -1)
                throw new EndOfStreamException();
            value = (char)result;
        }

        //HINT: Maybe ToOADate() fits better to delphi datetime values.
        public static double Now()
        {
            return TotalDaysFromDateTime(DateTime.Now);
        }

        public static double Time()
        {
            DateTime now = DateTime.Now;
            return (now - now.Date).TotalDays;
        }

        public static DateTime DateTimeFromTotalDays(double days)
        {
            return DateTime.MinValue + TimeSpan.FromDays(days);
        }

        public static double TotalDaysFromDateTime(DateTime dt)
        {
            return (dt - DateTime.MinValue).TotalDays;
        }

        private static readonly DateTime s_unixStart = new DateTime(1970, 1, 1);

        public static int DateTimeToUnix(DateTime value)
        {
#if !NET46
            return (int)(value.ToUniversalTime() - s_unixStart).TotalSeconds;
#else
            return (int)((DateTimeOffset)value).ToUnixTimeSeconds();
#endif
        }

        public static DateTime UnixToDateTime(int value)
        {
#if !NET46
            return s_unixStart.AddSeconds(value).ToLocalTime();
#else
            return DateTimeOffset.FromUnixTimeSeconds(value).LocalDateTime;
#endif
        }

        private class ImgListSurr : ISurrogateSelector, ISerializationSurrogate
        {
            public byte[] Data;
            #region ISurrogateSelector Members

            public void ChainSelector(ISurrogateSelector selector)
            {
                throw new NotImplementedException();
            }

            public ISurrogateSelector GetNextSelector()
            {
                throw new NotImplementedException();
            }

            public ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector)
            {
                selector = this;
                if (type == typeof(ImageListStreamer))
                    return this;
                return null;
            }

            #endregion

            #region ISerializationSurrogate Members

            public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                info.AddValue("Data", Data);
            }

            public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        internal static string GetBase64ImageList(string filePath)
        {
            ImgListSurr surrogate = new ImgListSurr();
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                surrogate.Data = new byte[stream.Length];
                stream.Read(surrogate.Data, 0, surrogate.Data.Length);
            }
            BinaryFormatter formatter = new BinaryFormatter(surrogate, new StreamingContext());
            using (MemoryStream stream = new MemoryStream())
            {
                using (ImageList imglist = new ImageList())
                {
                    imglist.Images.Add(new Bitmap(32, 32));
                    formatter.Serialize(stream, imglist.ImageStream);
                }
                stream.Position = 0;

                BinaryFormatter b64formatter = new BinaryFormatter();
                ImageListStreamer ils = (ImageListStreamer)b64formatter.Deserialize(stream);
                stream.SetLength(0);
                b64formatter.Serialize(stream, ils);
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        public static int ParamCount()
        {
#if MAIN_PROJEKT
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                string[] args = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData;
                return args != null ? args.Length : 0;
            }
#endif
            return Environment.GetCommandLineArgs().Length - 1;
        }

        public static string ParamStr(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");
#if MAIN_PROJEKT
            if (ApplicationDeployment.IsNetworkDeployed && index != 0)
            {
                string[] args = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData;
                if (args == null)
                    throw new ArgumentOutOfRangeException("index");
                return args[index - 1];
            }
#endif
            return Environment.GetCommandLineArgs()[index];
        }

        internal static string IncludeTrailingPathDelimiter(string path)
        {
            if (path.EndsWith(Path.DirectorySeparatorChar.ToString()) || path.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
                return path;

            if (path.Contains(Path.AltDirectorySeparatorChar))
                return path + Path.AltDirectorySeparatorChar;
            return path + Path.DirectorySeparatorChar;
        }

        internal static string QuotedStr(string text, char quote)
        {
            string result = text.Replace(quote.ToString(), new string(quote, 2));
            return quote + result + quote;
        }

        //HINT: This is used by string.Split() to normalize line endings. Order is crucial!
        private static readonly string[] NewLineDelemiters = new[] { "\r\n", "\r", "\n\r", "\n" }.Where(nl => nl != Environment.NewLine).ToArray();

        internal static string NormalizeLineEndings(string text)
        {
            return NormalizeLineEndings(text, Environment.NewLine);
        }

        internal static string NormalizeLineEndings(string text, string lineEnding)
        {
            string[] textlines = text.Split(NewLineDelemiters, StringSplitOptions.None);
            return string.Join(lineEnding, textlines);
        }

        // Gibt einen String in die Datei Debug.log aus
        public static void DebugPrint(string msg, params object[] ap)
        {
            Debug.Print(msg, ap);
            //TODO: Debug.Print lässt sich mit der App.config einstellen, also ist der untere Teil überflüssig
            //FileStream stream;
            //String filename;
            //filename = ExtractFilePath(Application.ExeName) + "debug.log";
            //stream = File.Open(filename, "a+");
            //vfprintf(stream, msg, ap);
            //fprintf(stream, "\n");
            //stream.Close();
        }

        public static void SetRegistryInteger(String KeyName, int Value, String Key = "")
        {
            try
            {
                RegistryKey rootKey = Registry.CurrentUser;
                using (RegistryKey subKey = rootKey.OpenSubKey("Software\\Mora's" + Key, true) ?? rootKey.CreateSubKey("Software\\Mora's" + Key))
                {
                    subKey.SetValue(KeyName, Value, RegistryValueKind.DWord);
                }
            }
            catch
            { }	// Mache nichts
        }

        public static int GetRegistryInteger(string KeyName, int Default, string Key = "")
        {
            int iResult = Default;
            try
            {
                using (RegistryKey subKey = Registry.CurrentUser.OpenSubKey("Software\\Mora's" + Key, false))
                {
                    if (subKey != null)
                    {
                        object value = subKey.GetValue(KeyName);
                        if (value != null)
                            iResult = Convert.ToInt32(value);
                    }
                }
            }
            catch (Exception ex)
            {
                DebugPrint("Utils.cs/GetRegistryInteger: {0}", ex.Message);
            }
            return iResult;
        }

        public static void SetRegistryString(String KeyName, String Value, String Key = "", bool HKLM = false)
        {
            try
            {
                RegistryKey rootKey = Registry.CurrentUser;
                if (HKLM)
                    rootKey = Registry.LocalMachine;
                using (RegistryKey subKey = rootKey.OpenSubKey("Software\\Mora's" + Key, true) ?? rootKey.CreateSubKey("Software\\Mora's" + Key))
                {
                    subKey.SetValue(KeyName, Value, RegistryValueKind.String);
                }
            }
            catch
            { }	// Mache nichts
        }

        public static string GetRegistryString(string KeyName, string Default, string Key = "", bool HKLM = false)
        {
            string strResult = Default;
            try
            {
                RegistryKey rootKey = Registry.CurrentUser;
                if (HKLM)
                    rootKey = Registry.LocalMachine;
                // False because we do not want to create it if it doesn't exist
                using (RegistryKey subKey = rootKey.OpenSubKey("Software\\Mora's" + Key, false))
                {
                    if (subKey != null)
                    {
                        object value = subKey.GetValue(KeyName);
                        if (value != null)
                            strResult = Convert.ToString(value);
                    }
                }
            }
            catch (Exception ex)
            {
                DebugPrint("Utils.cpp/GetRegistryString: {0}", ex.Message);
            }
            return strResult;
        }

        // Speicher die Fensterposition in der Registry
        // Wenn bSize true, dann auch die Fenstergröße
        public static void SaveWindowPosition(String WindowName, Form form, bool bSize = false)
        {
            if (form == null)
                return;

            try
            {
                SetRegistryInteger(WindowName + "State", (int)form.WindowState, "\\Windows");
                if (form.WindowState != FormWindowState.Maximized)
                {
                    SetRegistryInteger(WindowName + "Left", form.Left, "\\Windows");
                    SetRegistryInteger(WindowName + "Top", form.Top, "\\Windows");
                    if (bSize)
                    {
                        SetRegistryInteger(WindowName + "Width", form.Width, "\\Windows");
                        SetRegistryInteger(WindowName + "Height", form.Height, "\\Windows");
                    }
                }
            }
            catch
            { }	// Mache nichts
        }

        // bSize gibt an, ob auch die Größe des Fensters geladen werden soll
        public static void LoadWindowPosition(String WindowName, Form form, bool bSize = false)
        {
            if (form == null)
                return;

            try
            {
                form.Left = GetRegistryInteger(WindowName + "Left", form.Left, "\\Windows");
                form.Top = GetRegistryInteger(WindowName + "Top", form.Top, "\\Windows");
                if (bSize)
                {
                    form.Width = GetRegistryInteger(WindowName + "Width", form.Width, "\\Windows");
                    form.Height = GetRegistryInteger(WindowName + "Height", form.Height, "\\Windows");
                }
                form.WindowState = (FormWindowState)GetRegistryInteger(WindowName + "State", (int)form.WindowState, "\\Windows");
            }
            catch (Exception)
            {	// Nur abfangen
            }
        }

        // Einen String in eine dem Realm enstprechendem Integer wandeln
        // Es werden dafür nur die ersten 3 Zeichen ausgewertet
        public static int Realm2Int(String strRealm)
        {
            int iReturn = 0; // Eventuell wäre 7 hier besser
            String str3 = strRealm.Substring(0, 3).ToLower();
            if (str3 == "alb") iReturn = 1;
            else if (str3 == "hib") iReturn = 2;
            else if (str3 == "mid") iReturn = 4;
            else if (str3 == "all") iReturn = 7;
            return iReturn;
        }

        // Ein übergebener String wird in ein integer gewandelt
        // 0 = german, 1 = french, 2 = english, alle anderen Werte auch 0
        public static int Language2Int(String strLanguage)
        {
            int iReturn = 0;

            strLanguage = strLanguage.ToLower();
            if (strLanguage == "french")
                iReturn = 1;
            else if (strLanguage == "english")
                iReturn = 2;
            else if (strLanguage == "pvp")	// ok, ist eigentlich keine Sprache
                iReturn = 3;
            else if (strLanguage == "italian")
                iReturn = 4;
            else if (strLanguage == "spanish")
                iReturn = 8;
            else if (strLanguage == "american")
                iReturn = 9;
            return iReturn;
        }

        // Wandele den Realm-Integer wieder in einen string
        public static String Realm2Str(int iRealm)
        {
            String strReturn;
            switch (iRealm)
            {
                case 1: strReturn = "Albion"; break;
                case 2: strReturn = "Hibernia"; break;
                case 4: strReturn = "Midgard"; break;
                case 7: strReturn = "Alle"; break;
                default: strReturn = "Fehler in Realm2Str"; break;
            }
            return strReturn;
        }

        // Ein Resistenz-String wird in entsprechenden Integer wandeln
        // Es werden dafür nur die ersten 3 Zeichen ausgewertet
        public static int Resist2Int(String strResist)
        {
            int iReturn = -1;

            strResist = strResist.ToLower();
            if (strResist == "crush") iReturn = 0;
            else if (strResist == "slash") iReturn = 1;
            else if (strResist == "thrust") iReturn = 2;
            else if (strResist == "heat") iReturn = 3;
            else if (strResist == "cold") iReturn = 4;
            else if (strResist == "matter") iReturn = 5;
            else if (strResist == "body") iReturn = 6;
            else if (strResist == "spirit") iReturn = 7;
            else if (strResist == "energy") iReturn = 8;
            return iReturn;
        }

        // Einen String in eine Integer-Zahl wandeln
        // Im Unterschied zu StrToIntDef stören führende Ansizeichen garnicht
        // Liefert 0, wenn keine Zahl gefunden
        public static int Str2Int(String Value, int startIndex = 0)
        {
            int iReturn = 0;
            bool bValue = false;
            for (int i = startIndex; i < Value.Length; i++)
            {
                if ((Value[i] >= '0') && (Value[i] <= '9'))
                {
                    iReturn = iReturn * 10 + Value[i] - '0';
                    bValue = true;
                }
                else if (bValue) break;
            }
            return iReturn;
        }

        // Einen String in eine Decimal-Zahl umwandeln
        public static decimal Str2Decimal(String Value)
        {
            decimal dReturn = 0, dFaktor = 1;
            bool bKomma = false, bNegative = false;
            int i;
            for (i = 0; i < Value.Length; i++)
            {
                switch (Value[i])
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        if (bKomma) dFaktor *= 10;
                        dReturn = dReturn * 10 + (decimal)(Value[i] - '0');
                        break;
                    case '-':
                        bNegative = true; break;
                    case '.':
                    case ',':
                        bKomma = true; break;
                    default:
                        i = 1000000;
                        break;
                }
            }
            if (bNegative) dReturn = -dReturn;
            return (dReturn / dFaktor);
        }

        private enum eSTRING
        {
            STATE_NONE,
            STATE_STRING,
            STATE_ESCAPE,
        };

        // Liest einen String nach VisualBasic-Art, d.h.
        // Trennzeichen sind \n und Komma, Zeichenketten stehen in Anführungszeichen
        public static String ReadVBString(StreamReader istream)
        {
            char ic, lc = '\0';
            bool bExit = false;
            eSTRING State = eSTRING.STATE_NONE;
            String sReturn = "";
            while (!istream.EndOfStream && !bExit)
            {
                ic = (char)istream.Read();
                switch (State)
                {
                    case eSTRING.STATE_NONE:	// String nicht offen
                        // Ein Return oder Komma gilt hier als Ende
                        if ((ic == '\n') || (ic == ','))
                        {	// Ein führendes \n ausblenden
                            if (lc != 0)
                            {
                                bExit = true;
                            }
                        }
                        else if (ic == '\"')
                            State = eSTRING.STATE_STRING;
                        else
                            sReturn += ic;
                        break;
                    case eSTRING.STATE_STRING:
                        if (ic == '\"')
                            State = eSTRING.STATE_ESCAPE;
                        else
                            sReturn += ic;
                        break;
                    case eSTRING.STATE_ESCAPE:
                        if (ic == '\"')
                        {
                            sReturn += '\"';
                            State = eSTRING.STATE_STRING;
                        }
                        else
                        {	// Ein Return oder Komma gilt hier als Ende
                            if ((ic == '\n') || (ic == ','))
                                bExit = true;
                            else
                                sReturn += ic;
                        }
                        break;
                }
                lc = ic;
            }
            return sReturn;
        }

        static DateTimeFormatInfo dtFormat = (DateTimeFormatInfo)CultureInfo.CurrentUICulture.DateTimeFormat.Clone();
        public static DateTime ReadVBDate(StreamReader istream)
        {
            DateTime dtReturn;
            String sTemp2;
            String sTemp = ReadVBString(istream);
            sTemp2 = sTemp.Substring(2, sTemp.Length - 2);
            // Setze die für das VB-Format
            dtFormat.DateSeparator = "-";
            dtFormat.ShortDatePattern = "yyyy-MM-dd";
            dtFormat.TimeSeparator = ":";
            dtFormat.ShortTimePattern = "hh:mm:ss";
            DateTime.TryParse(sTemp2, dtFormat, DateTimeStyles.None, out dtReturn);
            return dtReturn;
        }

        // Wandele einen String in einen xml-Conformen String um
        // & => &amp;
        // < => &lt;
        // > => &gt;
        public static String Str2XmlStr(String input)
        {
            String strReturn = "";
            for (int i = 0; i < input.Length; i++)
            {
                switch (input[i])
                {
                    case '&': strReturn += "&amp;"; break;
                    case '<': strReturn += "&lt;"; break;
                    case '>': strReturn += "&gt;"; break;
                    case '\'': strReturn += "&apos;"; break;
                    case '"': strReturn += "&quot;"; break;
                    case '\r': break;	// Herausfiltern, da es automatisch angehängt wird
                    default: strReturn += input[i]; break;
                }
            }
            return strReturn;
        }

        // Rechne dem im String enthaltenen Goldwert in ein Integer um
        public static int Gold2Int(String Gold)
        {
            String sNumber = "", sTemp;
            sTemp = Gold.ToLower();
            char cCur;
            int iResult = 0;
            for (int i = 0; i < sTemp.Length; i++)
            {
                cCur = sTemp[i];
                if ((cCur >= '0') && (cCur <= '9'))
                    sNumber += cCur;
                else if (cCur == 'g')
                {
                    iResult += sNumber.ToIntDef(0) * 10000;
                    sNumber = "";
                }
                else if (cCur == 's')
                {
                    iResult += sNumber.ToIntDef(0) * 100;
                    sNumber = "";
                }
                // alle weiteren Zeichen als g und s werden ignoriert
            }
            iResult += sNumber.ToIntDef(0);
            return iResult;
        }

        // Erstelle einen Gold-String aus einem Integer
        // Im moment nur Anzeige bis Gold, kein Platin oder Mithril
        public static String Int2Gold(int Gold)
        {
            String strReturn = "";
            int v = Gold;
            if (v > 10000)
            {
                strReturn += (v / 10000).ToString() + 'g';
                v -= (v / 10000) * 10000;
            }
            if (v >= 100)
            {
                if (strReturn.Length > 0) strReturn += ' ';
                strReturn += (v / 100).ToString() + 's';
                v -= (v / 100) * 100;
            }
            if ((v > 0) || (strReturn.Length == 0))
            {
                if (strReturn.Length > 0) strReturn += ' ';
                strReturn += (v).ToString() + 'k';
            }
            return strReturn;
        }
#if MAIN_PROJEKT
        // sofern keine Position angegeben wird hier versucht eine Position zu erfragen
        public static DialogResult AskForPosition(this CItem Item)
        { // erfragen welche position das Item hat
            DialogResult mr2;

            // wenn AF höher als 0 angegeben wurde könnte es eine Rüstung sein
            if (Item.AF > 0)
            {
                TApplication.Instance.CreateForm(out Unit.frmArmorSlot);
                Unit.frmArmorSlot.gbInfo.Text = Item.Name;
                Unit.frmArmorSlot.gbInfo.SetHint(Item.Name);
                Unit.frmArmorSlot.lbDescription.Text = Item.LongInfo;
                Unit.frmArmorSlot.rdArmorSlot.ItemIndex = 0;
                mr2 = Unit.frmArmorSlot.ShowDialog();
                Item.Position = Unit.frmArmorSlot.rdArmorSlot.ItemIndex;
                Unit.frmArmorSlot.Dispose();
                Unit.frmArmorSlot = null;
            }
            // wenn Schaden höher als 0 angegeben wurden könnte es eine Waffe sein
            else if (Item.DPS > 0)
            {	// Waffen
                Item.Position = 6;	// Bei allen Waffen/Schilden 6
                TApplication.Instance.CreateForm(out Unit.frmWeaponSlot);
                Unit.frmWeaponSlot.gbInfo.Text = Item.Name;
                Unit.frmWeaponSlot.gbInfo.SetHint(Item.Name);
                Unit.frmWeaponSlot.lbDescription.Text = Item.LongInfo;
                Unit.frmWeaponSlot.pItem = Item;
                mr2 = Unit.frmWeaponSlot.ShowDialog();
                Item.Class = Unit.frmWeaponSlot.cbWeaponClass.CurrentData;
                Item.DamageType = Unit.frmWeaponSlot.cbDamageType.CurrentData;
                Unit.frmWeaponSlot.Dispose();
                Unit.frmWeaponSlot = null;
            }
            // wenn weder AF noch Schaden angegeben könnte es eines der anderen Items sein anlegen kann
            else
            {	// Schmuckposition
                TApplication.Instance.CreateForm(out Unit.frmJewelrySlot);
                Unit.frmJewelrySlot.gbInfo.Text = Item.Name;
                Unit.frmJewelrySlot.gbInfo.SetHint(Item.Name);
                Unit.frmJewelrySlot.lbDescription.Text = Item.LongInfo;
                Unit.frmJewelrySlot.rdJewelrySlot.ItemIndex = 0;
                mr2 = Unit.frmJewelrySlot.ShowDialog();
                Item.Position = Unit.frmJewelrySlot.rdJewelrySlot.ItemIndex + 10;
                if (Item.Position == 15) Item.Position = 16;	// Für Handgelenk
                Unit.frmJewelrySlot.Dispose();
                Unit.frmJewelrySlot = null;
            }

            return mr2;
        }
#endif
        public static int MorasMessage(String AMsg, String ATitle, int AFlags)
        {
            return (int)MessageBox.Show(AMsg, ATitle, (MessageBoxButtons)(AFlags & 0xf), (MessageBoxIcon)(AFlags & (~0xf)));
        }

        public static int MorasErrorMessage(String AMsg, String ATitle)
        {
            return MorasMessage(AMsg, ATitle, (int)MessageBoxButtons.OK | (int)MessageBoxIcon.Error);
        }

        public static int MorasInfoMessage(String AMsg, String ATitle)
        {
            return MorasMessage(AMsg, ATitle, (int)MessageBoxButtons.OK | (int)MessageBoxIcon.Information);
        }

        public static int MorasAskMessage(String AMsg, String ATitle)
        {
            return MorasMessage(AMsg, ATitle, (int)MessageBoxButtons.YesNoCancel | (int)MessageBoxIcon.Question);
        }

        public static ToolStripItem FindTBXItem(Component CParent, String ACaption)
        {
            ToolStripItem result = null;
            IComponent[] comps = null;
            if (CParent is ToolStripDropDownItem)
            {
                comps = new ToolStripItem[((ToolStripDropDownItem)CParent).DropDownItems.Count];
                ((ToolStripDropDownItem)CParent).DropDownItems.CopyTo((ToolStripItem[])comps, 0);
            }
            else if (CParent is Control)
            {
                comps = new IComponent[((Control)CParent).Controls.Count];
                ((Control)CParent).Controls.CopyTo(comps, 0);
            }
            else if (CParent.Container != null)
            {
                comps = new IComponent[CParent.Container.Components.Count];
                CParent.Container.Components.CopyTo(comps, 0);
            }

            if (comps != null)
            {
                for (int i = 0; i < comps.Length; i++)
                {
                    IComponent tmp = comps[i];
                    if (tmp is DelphiClasses.TMainMenu.TTBXItem || tmp is DelphiClasses.TToolBar.TTBXItem ||
                        tmp is DelphiClasses.TMainMenu.TTBXSubmenuItem || tmp is DelphiClasses.TToolBar.TTBXSubmenuItem)
                    {
                        ToolStripItem item = (ToolStripItem)tmp;
                        if (item.Text == ACaption)
                        {
                            result = item;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        public static int ConvertTagToInt(object tag)
        {
            return tag == null ? 0 : Convert.ToInt32(tag);
        }

        // use only lower 24 bits, ignore alpha value and make it full opaque
        public static Color Int2Color(int color)
        {
            return Color.FromArgb(color & 0xFF, (color >> 8) & 0xFF, (color >> 16) & 0xFF);
        }

        public static int Color2Int(Color col)
        {
            return col.R | (col.G << 8) | (col.B << 16);
        }

        private static PropertyInfo stylePaddingInternal = typeof(DataGridViewCellStyle).GetProperty("PaddingInternal", BindingFlags.Instance | BindingFlags.NonPublic);

        internal static void RemoveRowHeaderIcons(this DataGridView grid)
        {
            if (stylePaddingInternal != null)
            {
                int iconMarginWidth = 3;
                int iconsWidth = 12;
                int textMargin = 4;
                Padding noicons = new Padding(-(iconsWidth + (2 * iconMarginWidth) + textMargin), 0, 0, 0);
                stylePaddingInternal.SetValue(grid.RowHeadersDefaultCellStyle, new Padding(-22, 0, 0, 0), null);
            }
        }

        internal static void CreateConfigMappingFromJsonDictionary<TValue>(this IDictionary<string, object> jsonDictionary, IDictionary<string, IDictionary<string, TValue>> allMappings, string path, Func<object, TValue> jsonValueMapper, Func<string, bool> skipJsonId = null)
        {
            CreateConfigMappingFromJsonDictionary(jsonDictionary, allMappings, path, Extensions.IdentityFunction<string>.Instance, jsonValueMapper, skipJsonId);
        }

        internal static void CreateConfigMappingFromJsonDictionary<TKey, TValue>(this IDictionary<string, object> jsonDictionary, IDictionary<string, IDictionary<TKey, TValue>> allMappings, string path, Func<string, TKey> jsonIdSelector, Func<object, TValue> jsonValueMapper, Func<TKey, bool> skipJsonId = null)
        {
            string[] pathParts = path.Split('/');
            for (int i = 0; i < pathParts.Length; i++)
            {
                object child;
                if (!jsonDictionary.TryGetValue(pathParts[i], out child) || !(child is IDictionary<string, object>))
                    throw new KeyNotFoundException(pathParts[i]);
                jsonDictionary = (IDictionary<string, object>)child;
            }
            Dictionary<TKey, TValue> mapping = new Dictionary<TKey, TValue>();
            allMappings.Add(path, mapping);
            foreach (var pair in jsonDictionary)
            {
                TKey jsonid = jsonIdSelector(pair.Key);
                if (skipJsonId != null && skipJsonId(jsonid))
                    continue;
                TValue value = jsonValueMapper(pair.Value);
                mapping.Add(jsonid, value);
            }
        }
    }
}
