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
// Eine einfache Klasse für das lesen und schreiben von xml Dateien

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DelphiClasses;

namespace Moras.Net
{
    public enum ParserState
    {
        STATE_NOTHING,	// Startzustand, kein offener Tag
        STATE_OPEN,		// Tagöffner "<"
        STATE_TAG,		// Weitere Zeichen im Tag
        STATE_CONTENT,	// Die Daten zwischen dem Öffnen- und dem Schliessen-Tag
        STATE_ATTRIBUTE,// Der Attributname
        STATE_VALUE,	// Der Wert des Attributes
        STATE_EMPTY,	// Ein Empty-Tag( <tag /> )
    }

    //TODO: Ersetze das komplett mit System.Xml.XmlDocument
    public class CXml : IDisposable
    {
        private char cCur, cLast;
        private int iLineNo;	// Die aktuell bearbeitete Zeilennummer
        private String strLine;	// Das ist die aktuelle Zeile
        private String strTag;
        private String strContent;
        private String strFileName;
        private DynamicArray<string> strAttributeName;
        private DynamicArray<string> strAttributeValue;
        private ParserState State;
        private StreamReader fiXml;	// Stream for input
        private bool bInFile;	// Wahr, wenn input ein File ist
        private FStreamWrapper foXml;	// und für output
        private List<string> arOpenTags;

        #region Properties
        public String Tag { get { return GetTag(); } }
        public String Content { get { return GetContent(); } }
        public String Attribute { get { return GetAttribute(); } }
        public String Value { get { return GetValue(); } }
        private IndexerProperty<String, String> _attributeValue;
        public IndexerProperty<String, String> AttributeValue { get { return _attributeValue ?? (_attributeValue = new IndexerProperty<String, String> { read = GetAttributeValue }); } }
        public int LineNo { get { return GetLineNo(); } }
        public String currentLine { get { return GetLineString(); } }
        public String FileName { get { return strFileName; } }
        public int FileSize { get { return GetFileSize(); } }
        #endregion
        //---------------------------------------------------------------------------

        public CXml()
        {
            fiXml = null;
            foXml = null;
            arOpenTags = new List<string>();
        }

        ~CXml()
        {
            this.Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                arOpenTags.Clear();
                CloseXml();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        // Öffnet die xml-Datei für lesezugriff und liest bis zum ersten tag
        public bool OpenXml(string Filename)
        {
            CloseXml();
            strFileName = Path.GetFileName(Filename);
            bool is_open = true;
            try
            {
                fiXml = new IFStreamWrapper(Filename, FileMode.Open, FileAccess.Read);
            }
            catch { is_open = false; }
            bInFile = true;
            strLine = "";
            iLineNo = 0;
            cLast = '\0';
            strTag = "";
            strAttributeName.Length = 1;
            strAttributeValue.Length = 1;
            strContent = "";
            State = ParserState.STATE_NOTHING;
            if (is_open) return NextTag();
            else return false;
        }

        // Weise ein stream dem input zu
        public bool AttachInputStream(StreamReader Stream)
        {
            CloseXml();	// Alle offenen Streams schliessen
            fiXml = Stream;
            bInFile = false;
            strLine = "";
            iLineNo = 0;
            cLast = '\0';
            strTag = "";
            strAttributeName.Length = 1;
            strAttributeValue.Length = 1;
            strContent = "";
            State = ParserState.STATE_NOTHING;
            return NextTag();
        }

        // Inputstream wieder freimachen, da nicht von Klasse verwaltet
        public void DetachInputStream()
        {
            fiXml = null;
        }

        private String GetAttribute()
        {
            if (strAttributeName.Length > 0)
                return strAttributeName[0];
            else
                return "";
        }

        private String GetValue()
        {
            if (strAttributeValue.Length > 0)
                return strAttributeValue[0];
            else
                return "";
        }

        // Gibt den Wert des Attributes mit Namen "id" zurück
        private String GetAttributeValue(String Id)
        {
            for (int i = 0; i < strAttributeName.Length; i++)
            {
                if (strAttributeName[i] == Id)
                    return strAttributeValue[i];
            }
            return "";
        }

        // Öffnet die xml-Datei für Schreibzugriff und schreibe die Header-Zeile
        // Mit OpenTag und CloseTag die tags schreiben
        public bool OpenSaveXml(string Filename)
        {
            CloseXml();
            strFileName = FileName;
            bool fail = false;
            try
            {
                //HINT: FileMode.Create does the same as ofstream with trunc mode, because ofstream creates the file in every mode if it does not exist.
                foXml = new FStreamWrapper(Filename, FileMode.Create, FileAccess.Write);
            }
            catch { fail = true; }
            arOpenTags.Clear();
            if (fail) return false;
            foXml += "<?xml version=\"1.0\" encoding=\"iso-8859-1\" ?>\n";
            return true;
        }

        public void CloseXml()
        {
            if (fiXml != null)
            {
                if (bInFile)
                {
                    ((IFStreamWrapper)fiXml).Close();
                }
            }
            fiXml = null;
            if (foXml != null)
            {
                ((FStreamWrapper)foXml).Close();
            }
            foXml = null;
        }

        //Funktionen für das Schreiben

        // Mit dieser Funktion einen Tag schreiben
        // Variable Argumentenliste ist hier Blödsinn, macht zu viel Arbeit
        public bool OpenTag(String Tag, String Content, String Attribute)
        {
            arOpenTags.Add(Tag);
            foXml += "<" + Tag;
            // Jetzt die attribute wenn wir welche haben
            if (Attribute.Length > 0)
                foXml += ' ' + Attribute;
            foXml += ">" + Content;
            if (Content == "")
                foXml += "\n";
            return true;
        }

        // Schreibe einen leeren Tag in den Stream
        public bool EmptyTag(String Tag)
        {
            foXml += "<" + Tag + "/>\n";
            return true;
        }

        // und den Tag mit dieser Funktion wieder schliessen
        public bool CloseTag()
        {
            if (arOpenTags.Count > 0)
            {
                foXml += "</" + arOpenTags[arOpenTags.Count - 1] + ">\n";
                arOpenTags.RemoveAt(arOpenTags.Count - 1);
                return true;
            }
            return false;
        }

        // Diese Funktion macht die eigentliche Lesearbeit
        // Es liest dabei immer bis zum nächsten tag und
        // stellt die Daten in der Classe zur Verfügung
        public bool NextTag()
        {
            int nAttributes = 0;
            String strTemp = "";
            if (fiXml.EndOfStream)
                return false;
            bool isifstream = fiXml is IFStreamWrapper;
            while (!fiXml.EndOfStream)
            {
                fiXml.get(out cCur);
                // normalize line endings if stream doesn't do it for us
                if (!isifstream && cCur == '\r')
                {
                    cCur = '\n';
                    if (cLast == cCur || (!fiXml.EndOfStream && (char)fiXml.Peek() == cCur))
                        continue;
                }

                if (cCur == '\n')
                {
                    iLineNo++;
                    strLine = "";
                }
                if ((cCur != '\n') || (State == ParserState.STATE_CONTENT))
                {
                    strLine += cCur;
                    switch (State)
                    {
                        case ParserState.STATE_NOTHING:
                            if (cCur == '<')
                            {
                                State = ParserState.STATE_OPEN;
                                if (strTag != "") return true;
                            }
                            break;
                        case ParserState.STATE_OPEN:	// Das erste Zeichen nach einem Tagöffner '<'
                            strTag = "";
                            nAttributes = 0;
                            strAttributeName.Length = 0;
                            strAttributeValue.Length = 0;
                            strTemp = "";
                            strContent = "";
                            switch (cCur)
                            {
                                case '?':	// Diese beiden Tags ignorieren
                                case '!':
                                    State = ParserState.STATE_NOTHING;
                                    break;
                                case '/':	// Es ist ein schliessender Tag
                                    State = ParserState.STATE_NOTHING;
                                    break;
                                default:
                                    strTag = cCur.ToString();
                                    State = ParserState.STATE_TAG;
                                    break;
                            }
                            break;
                        case ParserState.STATE_TAG:
                            switch (cCur)
                            {
                                case '>':	// Tag ist zu Ende
                                    State = ParserState.STATE_CONTENT;
                                    strContent = "";
                                    break;
                                case ' ':	// Leerzeichen, d.h. es folgt nun ein attribut
                                    State = ParserState.STATE_ATTRIBUTE;
                                    break;
                                case '/':	// Ein Empty-Tag. Ist zwar so nicht ganz sauber, sollte aber reichen
                                    State = ParserState.STATE_NOTHING;
                                    break;
                                default:
                                    strTag += cCur;
                                    break;
                            }
                            break;
                        case ParserState.STATE_ATTRIBUTE:
                            if (cCur == '=')
                            {	// Den Attribute-String nun speichern
                                strAttributeName.Length = nAttributes + 1;
                                strAttributeName[nAttributes] = strTemp;
                                strTemp = "";
                                State = ParserState.STATE_VALUE;
                            }
                            else
                                strTemp += cCur;
                            break;
                        case ParserState.STATE_VALUE:
                            switch (cCur)
                            {
                                case '>':	// In Content-Mode schalten
                                    strAttributeValue.Length = nAttributes + 1;
                                    strAttributeValue[nAttributes] = strTemp;
                                    State = ParserState.STATE_CONTENT;
                                    break;
                                case ' ':
                                    if (cLast == '"')
                                    {
                                        strAttributeValue.Length = nAttributes + 1;
                                        strAttributeValue[nAttributes] = strTemp;
                                        nAttributes++;
                                        strTemp = "";
                                        State = ParserState.STATE_ATTRIBUTE;
                                    }
                                    else
                                        strTemp += cCur;
                                    break;
                                case '"':
                                case '\'':	// Nichts tun
                                    break;
                                default:
                                    strTemp += cCur;
                                    break;
                            }
                            break;
                        case ParserState.STATE_CONTENT:	// Alles in strContent kopieren, bis ein '<' kommt
                            if (cCur == '<')
                            {	// Hier noch die xml-Escape-Sequencen umwandeln
                                strContent = strContent.Replace("&lt;", "<");
                                strContent = strContent.Replace("&gt;", ">");
                                strContent = strContent.Replace("&apos;", "'");
                                strContent = strContent.Replace("&quot;", "\"");
                                strContent = strContent.Replace("&amp;", "&");
                                // Line endings should've been normalized to \n, but current platform may have different, so replace them.
                                if (Environment.NewLine != '\n')
                                    strContent = strContent.Replace("\n", Environment.NewLine);
                                State = ParserState.STATE_OPEN;
                                return true;
                            }
                            else
                                strContent += cCur;
                            break;
                    }
                }
                cLast = cCur;
            }
            return false;
        }

        private int GetFileSize()
        {
            FileInfo info = new FileInfo(strFileName);

            return (int)info.Length;
        }

        private String GetTag() { return strTag; }
        private String GetContent() { return strContent; }

        private int GetLineNo() { return iLineNo + 1; }
        private String GetLineString() { return strLine; }

        public bool isTag(string TagName) { return (strTag.Equals(TagName, StringComparison.CurrentCultureIgnoreCase)); }
    }
}
