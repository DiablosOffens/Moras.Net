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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DelphiClasses;
using System.IO;
using dxgettext;

namespace Moras.Net
{
    // Dieses Struct beschreibt den aktuellen Status eines Attributes
    // Ersetzt die temporären Variablen bei "xml_config.arAttributes"
    public struct SWeightState
    {
        // Die dem Attribut zugeordneten Wichtungs-Daten
        public bool bHidden;
        public int iWeight;            // Der normale Gewichtungsfaktor: Dies ist der einzige Wert, der gespeichert werden muss
        // für ein Gewichtungsprofil
        public int iIpMult;            // Zugehöriger IP-Multiplikator für die UpV-Umrechnung
        public Single sUpV;               // An player angepasstes und normiertes Gewicht
        // upv = Utility Per Value; Nutzen pro Attribut-Punkt
    }

    // Alle Attribut und Utilitydaten als Paket
    public class CPlayerWeights : ICloneable
    {
        private static string _(string szMsgId) { return TGnuGettextInstance.gettext(szMsgId); }

        private DynamicArray<SWeightState> arWeights;			// Alle Wichtungen für jedes Attribut
        private String strName;			                    // Bezeichner des Wichtungsprofils
        private String strFileName;		                    // Name der Datei wo die daten gespeichert sind, sofern vorhanden
        private int iClass;                                     // Redundant bzgl. CPlayer. Hiermit kann aber direkt geprüft werden
        // ob die upvs neu berechnet werden müssen.
        private bool bModified;                                 // Daten wurden verändert

        private String strLoadMessages;	                    // Hier werden Info/Warnmeldungen beim Laden zwischengespeichert

        #region Properties
        public String Name { get { return strName; } set { SetName(value); } }
        public String FileName { get { return strFileName; } set { SetFileName(value); } }
        public int Class { get { return iClass; } set { SetClass(value); } }
        private IndexerProperty<int, int> _weight;
        public IndexerProperty<int, int> Weight { get { return _weight ?? (_weight = new IndexerProperty<int, int> { read = GetWeight, write = SetWeight }); } }
        private IndexerProperty<Single, int> _UpV;
        public IndexerProperty<Single, int> UpV { get { return _UpV ?? (_UpV = new IndexerProperty<Single, int> { read = GetUpV }); } }
        #endregion

        //---------------------------------------------------------------------------
        public CPlayerWeights()
        {
            // Erstmal Array anlegen und Wichtungen auf 100 initialisieren
            arWeights.Length = Unit.xml_config.nAttributes;
            for (int i = 0; i < arWeights.Length; i++)
                arWeights.Array[i].iWeight = 100;

            // Daten initialisieren
            strName = _("Neue Gewichtung");
            strFileName = "";
            strLoadMessages = "";
            bModified = false;
            iClass = -1;
            SetClass(0);
        }
        //---------------------------------------------------------------------------
        /*~CPlayerWeights()
        {
            arWeights.Length = 0;
        }*/
        //---------------------------------------------------------------------------

        protected void SetName(String Name)
        {
            // Sicherstellen dass keine Anführungszeichen oder < im Namen sind (wg. Attribut im XML)
            strName = Name.Replace("\"", "'");
            strName = strName.Replace("<", "").Trim();
            bModified = true;
        }
        //---------------------------------------------------------------------------

        protected void SetFileName(String Name)
        {
            strFileName = Name;
            bModified = true;
        }
        //---------------------------------------------------------------------------

        protected void SetClass(int Class)
        {
            if (Class != iClass)
            {
                iClass = Class;
                // Hidden, UpV und IpMult neu berechnen für die Attribute
                bModified = true;
                for (int i = 0; i < arWeights.Length; i++)
                {
                    bool bFound = false;
                    // Ist es ein nuthbares Attribut?
                    if (Unit.xml_config.AttributeUseableByClass(i, Class))
                    {
                        // Suche den Bonus, welcher dieses Attribut als einzigen Wert hat
                        for (int j = 0; j < Unit.xml_config.nBonuses; j++)
                        {
                            if ((Unit.xml_config.arBonuses[j].ip_mult > 0)
                                && (Unit.xml_config.arBonuses[j].arAttributes[0] == i))
                            {
                                arWeights.Array[i].iIpMult = Unit.xml_config.arBonuses[j].ip_mult;
                                arWeights.Array[i].sUpV = (Single)(arWeights[i].iIpMult * arWeights[i].iWeight) / 10000;
                                arWeights.Array[i].bHidden = false;
                                bFound = true;
                                break;
                            }
                        }
                    }
                    if (!bFound)
                    {
                        arWeights.Array[i].iIpMult = 0;
                        arWeights.Array[i].sUpV = (Single)(0.0);
                        arWeights.Array[i].bHidden = true;
                    }
                }
            }
        }
        //---------------------------------------------------------------------------

        protected void SetWeight(int pos, int Weight)
        {
            if (pos >= 0 && pos <= arWeights.Length)
            {
                if (Weight < 0)
                    arWeights.Array[pos].iWeight = 0;
                else if (Weight > 100)
                    arWeights.Array[pos].iWeight = 100;
                else
                    arWeights.Array[pos].iWeight = Weight;
                arWeights.Array[pos].sUpV = (Single)(arWeights[pos].iIpMult * Weight) / 10000;
                bModified = true;
            }
        }
        //---------------------------------------------------------------------------

        // Speichere Wichtungsdaten unter den Dateinamen in FileName
        public bool Save(CXml pXml = null)
        {
            // Speichern
            bool iReturn = false;
            CXml pBase = pXml;

            try
            {
                // Falls NULL, erstelle neues File, ansonsten wird es in eine bereits offene XML gespeichert, z.B. Char-MOX
                if (pXml == null)
                {
                    pBase = new CXml();
                    if (pBase == null)
                    {
                        Utils.DebugPrint("CPlayerWeights::Save = 'pBase = new CXml();' failed, got NULL-Pointer.");
                        return false;
                    }
                    iReturn = pBase.OpenSaveXml(strFileName);
                }
                else
                {
                    iReturn = true;
                }

                if (iReturn)
                {
                    // Weights-Tag öffnen, Class und Name ist ein MUSS => Attribut
                    String att = "name=\"" + strName + "\"";
                    if (pXml == null)
                    {
                        att += " class=\"" + Unit.xml_config.arClasses[iClass].id + "\"";
                    }
                    pBase.OpenTag("weights", "", att);

                    // Die Wichtungen speichern
                    // Nur Werte die ungleich 100 sind speichern
                    for (int i = 0; i < arWeights.Length; i++)
                    {
                        if (arWeights[i].iWeight != 100)
                        {
                            pBase.OpenTag("weight", arWeights[i].iWeight.ToString(),
                                "attribute=\"" + Unit.xml_config.arAttributes[i].id + "\"");
                            pBase.CloseTag();
                        }
                    }
                    pBase.CloseTag();	// Weights-Tag schliessen
                    if (pXml == null)
                    {
                        pBase.CloseXml();   // wgt-File schliessen
                    }
                    bModified = false;
                }
            }
            catch (Exception e)
            {
                Utils.DebugPrint("CPlayerWeights::Save = %s\niClass=%d", e.Message, iClass);
                iReturn = false;
            }
            return iReturn;
        }
        //---------------------------------------------------------------------------

        // Lade Spielerdaten
        public bool Load(CXml pXml = null)
        {
            // Laden
            bool bReturn = false;
            String strTemp;
            String strAllErrors;
            int iErrCnt = 0;
            int iTemp;
            CXml pBase = pXml;

            try
            {
                // Falls NULL, wird aus einem eigenen File geladen...
                if (pBase == null)
                {
                    pBase = new CXml();
                    if (pBase == null)
                    {
                        Utils.DebugPrint("CPlayerWeights::Load = 'pBase = new CXml();' failed, got NULL-Pointer.");
                        return false;
                    }
                    String Ext = Path.GetExtension(strFileName);
                    SetAllWeights(100);
                    if (Ext.Equals(".wgt", StringComparison.CurrentCultureIgnoreCase))
                    {
                        strAllErrors = string.Format(_("Datei: %s\n\n").Replace("%s", "{0}"), pBase.FileName);
                        bReturn = pBase.OpenXml(strFileName);
                        while (bReturn)
                        {
                            if (pBase.isTag("weights"))
                            {
                                int cid = Unit.xml_config.GetClassId(pBase.AttributeValue["class"]);
                                if (cid >= 0)
                                    SetClass(cid);
                                else
                                    SetClass(0);

                                SetName(pBase.AttributeValue["name"]);
                            }
                            else if (pBase.isTag("weight"))
                            {
                                int aid = Unit.xml_config.GetAttributeId(pBase.AttributeValue["attribute"]);
                                if (aid >= 0)	// aid ist nur gültig, wenn sie in der aktuellen config gefunden wird
                                    SetWeight(aid, pBase.Content.ToIntDef(0));
                                else
                                {	// Warnmeldung ausgeben
                                    String strErr;
                                    iErrCnt++;
                                    strErr = string.Format(_("Attribut '%s' ist nicht mehr gültig.\n").Replace("%s", "{0}"), pBase.AttributeValue["attribute"]);
                                    strAllErrors += strErr;
                                }
                            }
                            bReturn = pBase.NextTag();
                        }
                        pBase.CloseXml();
                        if (iErrCnt > 0)
                        {
                            strAllErrors += _("\nBitte Wichtungen überprüfen.");
                            Utils.MorasInfoMessage(strAllErrors.Replace("\n", Environment.NewLine), _("Unbekannte Wichtung"));
                        }
                    }
                    else
                    {
                        // Warnmeldung ausgeben
                        String strErr;
                        strErr = string.Format(_("Fehler beim Laden einer Wichtungsdatei. Dateiendung '.wgt' erwartet!"));
                        Utils.MorasErrorMessage(strErr, _("Ungültige Dateiendung"));
                    }
                    bModified = false;
                }
                else
                {
                    if (pBase.isTag("weights"))
                    {
                        int cid = Unit.xml_config.GetClassId(pBase.AttributeValue["class"]);
                        if (cid >= 0)
                            SetClass(cid);
                        else
                            SetClass(0);

                        SetName(pBase.AttributeValue["name"]);
                    }
                    else if (pBase.isTag("weight"))
                    {
                        int aid = Unit.xml_config.GetAttributeId(pBase.AttributeValue["attribute"]);
                        if (aid >= 0)	// aid ist nur gültig, wenn sie in der aktuellen config gefunden wird
                            SetWeight(aid, pBase.Content.ToIntDef(0));
                        else
                        {	// Warnmeldung ausgeben
                            String strErr;

                            if (strLoadMessages.Length == 0)
                            {
                                strLoadMessages = string.Format(_("Datei: %s\n\n").Replace("%s", "{0}"), pBase.FileName);
                            }
                            strErr = string.Format(_("Attribut '%s' ist nicht mehr gültig.\n").Replace("%s", "{0}"), pBase.AttributeValue["attribute"]);
                            strLoadMessages += strErr;
                        }
                    }
                    bModified = false;
                }
            }
            catch
            {
                Utils.DebugPrint("Es ist ein Fehler in CPlayerWeights::Load aufgetreten!");
                bReturn = false;
            }
            return bReturn;
        }
        //---------------------------------------------------------------------------

        // Gibt gesammelte Info/Warn-Meldungen aus, die durch Wichtungsladen
        // innerhalb eines xml-Files entstanden sind.
        // Betrifft bisher nur alte .xml-Files
        public void PostLoadMessages()
        {
            if (strLoadMessages.Length > 0)
            {
                strLoadMessages += _("\nBitte Wichtungen überprüfen.");
                Utils.MorasInfoMessage(strLoadMessages, _("Unbekannte Wichtung"));
            }
            strLoadMessages = "";
        }
        //---------------------------------------------------------------------------

        // Setze alle Wichtungen
        public void SetAllWeights(int Weight)
        {
            for (int i = 0; i < arWeights.Length; i++)
            {
                SetWeight(i, Weight);
            }
        }
        //---------------------------------------------------------------------------

        public CPlayerWeights(CPlayerWeights rhs)
        {
            iClass = rhs.iClass;
            bModified = rhs.bModified;
            strName = rhs.strName;
            strFileName = rhs.strFileName;

            arWeights.Length = rhs.arWeights.Length;
            for (int i = 0; i < arWeights.Length; i++)
            {
                arWeights[i] = rhs.arWeights[i];
            }
        }

        #region ICloneable Members

        public object Clone()
        {
            return new CPlayerWeights(this);
        }

        #endregion
        //---------------------------------------------------------------------------
        // Den Inhalt ins Debug.log schreiben
        public void WriteDebug()
        {
            Utils.DebugPrint("Debug Weights\n-------------------------------------------------------");
            for (int i = 0; i < arWeights.Length; i++)
            {
                String msg = Unit.xml_config.arAttributes[i].id + "=" + arWeights[i].iWeight.ToString();
                Utils.DebugPrint(msg);
            }
        }
        //---------------------------------------------------------------------------

        //protected int GetWeight(int pos) { if (pos >= 0 && pos < arWeights.Length) { if (arWeights[pos].bHidden) { return -1; } else { return arWeights[pos].iWeight; } } else { return 0; } }
        protected int GetWeight(int pos) { if (pos >= 0 && pos < arWeights.Length) { return arWeights[pos].iWeight; } else { return 0; } }
        protected Single GetUpV(int pos) { if (pos >= 0 && pos < arWeights.Length) { return arWeights[pos].sUpV; } else { return (Single)0.0; } }
    }
}
