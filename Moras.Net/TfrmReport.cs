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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DelphiClasses;
using dxgettext;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;

namespace Moras.Net
{
    public partial class TfrmReport : TCustomForm
    {
        private static string _(string szMsgId) { return TGnuGettextInstance.gettext(szMsgId); }
        protected override IContainer GetChildContainer() { return components ?? base.GetChildContainer(); }
        //---------------------------------------------------------------------------
        public TfrmReport()
        {
            InitializeComponent();
            ((Bitmap)btSave.Image).MakeTransparent();
            ((Bitmap)btPrint.Image).MakeTransparent();
            ((Bitmap)btClipboard.Image).MakeTransparent();
        }
        //---------------------------------------------------------------------------
        private void btReportClick(object sender, EventArgs e)
        {
            int idx = Utils.ConvertTagToInt(((Control)sender).Tag);
            string curlang = TGnuGettextInstance.GetCurrentLanguage();
            curlang = curlang.Substring(0, 2);

            switch (idx)
            {
                case 0: DoReport(Unit.frmMain.DataPath + "reports\\Configreport_" + curlang + ".rpt"); break;
                case 1: DoReport(Unit.frmMain.DataPath + "reports\\Shortreport_" + curlang + ".rpt"); break;
                case 2: DoReport(Unit.frmMain.DataPath + "reports\\Crafterreport_" + curlang + ".rpt"); break;
                case 3: DoReport(Unit.frmMain.DataPath + "reports\\Materiallist_" + curlang + ".rpt"); break;
            }
            NativeMethods.SendMessage(new HandleRef(tbReport, tbReport.Handle), NativeMethods.EM_LINESCROLL, 0, 0xFFFF8000);
            tbReport.Invalidate();
            Utils.SetRegistryString("LastReport", (idx).ToString());
        }
        //---------------------------------------------------------------------------
        // Baue einen Report zusammen
        private void DoReport(string ReportFile)
        {//TODO: fix indices from delphi port
            string strLine = "", strTag = null, strName = null, strInsert;
            TStringList strText = new TStringList();
            int i;
            char cCur;
            bool[] bVisible = new bool[10];	// Ist Output sichtbar
            bVisible[0] = true;
            int ifLevel = 0;	// If-Ebenen-Verschachtelung
            bool bTag = false;
            bool bConditional = false;	// Ist wahr, wenn in der Zeile ein conditional steht
            bool bName = false;
            tbReport.Clear();
            try
            {
                IFStreamWrapper _in = new IFStreamWrapper(ReportFile);
                _in.get(out cCur);
                while (!_in.EndOfStream)
                {	// Lese alles bis zum Dateiende
                    if (bTag)
                    {
                        if (cCur == '>')
                        {	// Tag ist wieder zu Ende
                            int iPos = strName.Length + 1;	// Erstes Zeichen nach dem Tagnamen
                            strName = strName.ToLower();
                            strInsert = "";
                            int aid = -1;
                            if (strName == "name")
                                strInsert = Unit.player.Name;
                            else if (strName == "if")
                            {
                                ifLevel++;
                                Debug.Assert(ifLevel < 10);
                                if (bVisible[ifLevel - 1])
                                {
                                    string strTemp = strTag.Substring(iPos, Math.Min(99, strTag.Length - iPos));
                                    iPos = strTemp.IndexOf(' ') + 1;
                                    if (PieceStat(strTemp, out strInsert, ref iPos) == 0)
                                        bVisible[ifLevel] = false;
                                    else
                                        bVisible[ifLevel] = true;
                                }
                                else
                                    bVisible[ifLevel] = false;
                                bConditional = true;
                                //						strInsert = "";
                            }
                            else if (strName == "endif")
                            {
                                ifLevel--;
                                bConditional = true;
                            }
                            else if (strName == "else")
                            {
                                if (bVisible[ifLevel - 1])
                                    bVisible[ifLevel] = !bVisible[ifLevel];
                                bConditional = true;
                            }
                            else if (strName == "level")
                                strInsert = (Unit.player.Level).ToString();
                            else if (strName == "totalcost")
                                strInsert = Unit.frmMain.lbSCCost.Text;
                            else if (strName == "str")
                                aid = Unit.xml_config.GetAttributeId("STRENGTH");
                            else if (strName == "con")
                                aid = Unit.xml_config.GetAttributeId("CONSTITUTION");
                            else if (strName == "dex")
                                aid = Unit.xml_config.GetAttributeId("DEXTERITY");
                            else if (strName == "qui")
                                aid = Unit.xml_config.GetAttributeId("QUICKNESS");
                            else if (strName == "int")
                                aid = Unit.xml_config.GetAttributeId("INTELLIGENCE");
                            else if (strName == "pie")
                                aid = Unit.xml_config.GetAttributeId("PIETY");
                            else if (strName == "cha")
                                aid = Unit.xml_config.GetAttributeId("CHARISMA");
                            else if (strName == "emp")
                                aid = Unit.xml_config.GetAttributeId("EMPATHY");
                            else if (strName == "hits")
                                aid = Unit.xml_config.GetAttributeId("HITPOINTS");
                            else if (strName == "power")
                                aid = Unit.xml_config.GetAttributeId("POWER");
                            else if (strName == "crush")
                                aid = Unit.xml_config.GetAttributeId("RES_CRUSH");
                            else if (strName == "slash")
                                aid = Unit.xml_config.GetAttributeId("RES_SLASH");
                            else if (strName == "thrust")
                                aid = Unit.xml_config.GetAttributeId("RES_THRUST");
                            else if (strName == "heat")
                                aid = Unit.xml_config.GetAttributeId("RES_HEAT");
                            else if (strName == "cold")
                                aid = Unit.xml_config.GetAttributeId("RES_COLD");
                            else if (strName == "matter")
                                aid = Unit.xml_config.GetAttributeId("RES_MATTER");
                            else if (strName == "body")
                                aid = Unit.xml_config.GetAttributeId("RES_BODY");
                            else if (strName == "spirit")
                                aid = Unit.xml_config.GetAttributeId("RES_SPIRIT");
                            else if (strName == "energy")
                                aid = Unit.xml_config.GetAttributeId("RES_ENERGY");
                            else if (strName == "skills")
                            {	// Alles Skills listen
                                for (int skill = 0; skill < Unit.xml_config.arClasses[Unit.player.Class].nSkills; skill++)
                                {
                                    int a = Unit.xml_config.arClasses[Unit.player.Class].arSkills[skill];
                                    // Allerdings nur, wenn höher als 0
                                    if (bVisible[ifLevel] && (Unit.player.Attributes.Value[a] > 0))
                                        strText.Add((Unit.player.Attributes.Value[a]).ToString() + " " + Unit.xml_config.arAttributes[a].Name);
                                }
                            }
                            else if (strName == "skill")
                            {
                                int skill = strTag.Substring(iPos, Math.Min(2, strTag.Length - iPos)).ToIntDef(0);
                                iPos += 3;
                                if ((skill > 0) && (skill <= Unit.xml_config.arClasses[Unit.player.Class].nSkills))
                                {
                                    int a = Unit.xml_config.arClasses[Unit.player.Class].arSkills[skill - 1];
                                    strInsert = (Unit.player.Attributes.Value[a]).ToString() + " " + Unit.xml_config.arAttributes[a].Name;
                                }
                            }
                            else if (strName.Length > 5 && strName.Substring(0, 5) == "focus")
                            {
                                int focus = strTag.Substring(5, 1).ToIntDef(0);
                                int fg = Unit.xml_config.GetGroupId("FOCUS");
                                if ((focus > 0) && (fg >= 0))
                                {
                                    int c = 0;
                                    for (i = 0; i < Unit.xml_config.nBonuses; i++)
                                    {
                                        if ((Unit.xml_config.arBonuses[i].idGroup == fg) && (Unit.xml_config.arBonuses[i].arAttributes.Length == 1))
                                        {
                                            int fid = Unit.xml_config.arBonuses[i].arAttributes[0];
                                            if ((Unit.player.Attributes.Value[fid] > 0)
                                             && (Unit.xml_config.arAttributes[fid].SkillId >= 0)
                                             && Unit.player.AttributeUseable(Unit.xml_config.arAttributes[fid].SkillId))
                                            {
                                                c++;
                                                if (c == focus)
                                                    strInsert = (Unit.player.Attributes.Value[fid]).ToString() + " " + Unit.xml_config.arAttributes[fid].Name;
                                            }
                                        }
                                    }
                                }
                            }
                            else if (strName == "capbonuses")
                            {	// Liste die Cap-Boni
                                for (i = 0; i < Unit.xml_config.nAttributes; i++)
                                {
                                    if (Unit.xml_config.arAttributes[i].CapId >= 0 && !Unit.xml_config.arAttributes[i].bCapAttr)
                                    {
                                        int cid = Unit.xml_config.arAttributes[i].CapId;
                                        if ((Unit.player.Attributes.Cap[cid] > 0) && bVisible[ifLevel])
                                            strText.Add((Unit.player.Attributes.Cap[cid]).ToString() + " " + Unit.xml_config.arAttributes[cid].Name);
                                    }
                                }
                            }
                            else if (strName == "otherbonuses")
                            {	// Liste die anderen Boni
                                int idOthers = Unit.xml_config.GetGroupId("OTHERS");
                                Debug.Assert(idOthers >= 0);
                                for (i = 0; i < Unit.xml_config.nBonuses; i++)
                                {
                                    if ((Unit.xml_config.arBonuses[i].idGroup == idOthers) && (Unit.xml_config.arBonuses[i].arAttributes.Length == 1))
                                    {	// Werte für Sinnesschärfe direkt nicht anzeigen
                                        int bid = Unit.xml_config.arBonuses[i].arAttributes[0] & 0xfff;
                                        if ((Unit.player.Attributes.Value[bid] > 0) && bVisible[ifLevel])
                                            strText.Add((Unit.player.Attributes.Value[bid]).ToString() + " " + Unit.xml_config.arAttributes[bid].Name);
                                    }
                                }
                            }
                            else if (strName == "materials")
                            {
                                string strTemp = strTag.Substring(iPos, Math.Min(99, strTag.Length - iPos));
                                if (strTemp == "gemlist")
                                {
                                    int[] arGemList = new int[Unit.xml_config.nBonuses * 10];
                                    Debug.Assert(arGemList != null);
                                    for (i = Unit.xml_config.nBonuses * 10; (i--) > 0; )
                                        arGemList[i] = 0;
                                    for (int p = 0; p < 10; p++)
                                    {
                                        if (Unit.player.ItemType[p] == EItemType.Crafted)
                                        {
                                            for (i = 0; i < Unit.player.nItemEffects[p]; i++)
                                            {
                                                int bid = Unit.player.ItemEffect[p][i];
                                                if (bid >= 0 && i < 4)
                                                {
                                                    int val = Unit.player.EffectValue[p][i];
                                                    // Effektlevel besorgen
                                                    for (int j = 0; j < 10; j++)
                                                    {
                                                        if (Unit.xml_config.arBonuses[bid].Gemvalue[j] == val)
                                                        {
                                                            arGemList[bid * 10 + j]++;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    // Gems ausgeben
                                    for (i = 0; i < Unit.xml_config.nBonuses; i++)
                                    {
                                        for (int j = 0; j < 10; j++)
                                        {
                                            if (arGemList[i * 10 + j] > 0)
                                            {
                                                if (bVisible[ifLevel]) strText.Add((arGemList[i * 10 + j]).ToString() + " " + Unit.xml_config.arBonuses[i].GemName + " (" + Unit.xml_config.arMaterials[Unit.xml_config.GetMaterialId("GEMS")].arLevel[j].GemPrefix + ")");
                                            }
                                        }
                                    }
                                    arGemList = null;
                                }
                                else
                                {
                                    int[] Gems = new int[10];	// Zähler für die Gems
                                    TStringList sGemList;
                                    sGemList = new TStringList();
                                    // Lösche die Zählvariablen
                                    for (i = 0; i < Unit.xml_config.nIngredients; i++)
                                        Unit.xml_config.arIngredients[i].Count[0] = 0;
                                    for (i = 0; i < 10; i++)
                                        Gems[i] = 0;
                                    for (int p = 0; p < 10; p++)
                                    {
                                        if (Unit.player.ItemType[p] == EItemType.Crafted)
                                        {
                                            for (i = 0; i < Unit.player.nItemEffects[p]; i++)
                                            {
                                                int bid = Unit.player.ItemEffect[p][i];
                                                if (bid >= 0 && i < 4)
                                                {
                                                    int val = Unit.player.EffectValue[p][i];
                                                    // Gemlevel bestimmen
                                                    int gemlevel = -1;
                                                    for (int j = 0; j < 10; j++)
                                                    {
                                                        if (Unit.xml_config.arBonuses[bid].Gemvalue[j] == val)
                                                        {
                                                            gemlevel = j;
                                                            break;
                                                        }
                                                    }
                                                    Debug.Assert(gemlevel >= 0);
                                                    // Besorge zu dem gemlevel die Anzahl an Stäuben und Flüssigkeiten
                                                    int dusts = Unit.xml_config.arBonuses[bid].DustAmount[gemlevel];
                                                    int liquids = Unit.xml_config.arBonuses[bid].LiquidAmount[gemlevel];
                                                    // Jetzt wissen wir welches Gem gebraucht wird
                                                    int dust = Unit.xml_config.arBonuses[bid].GemDust;
                                                    Gems[gemlevel] += Unit.xml_config.arBonuses[bid].GemAmount[gemlevel] + (Unit.player.Remakes[p][i] * Unit.xml_config.arBonuses[bid].GemAmount[gemlevel]);
                                                    Unit.xml_config.arIngredients[dust].Count[0] += (Unit.player.Remakes[p][i] + 1) * dusts;
                                                    for (int j = 0; j < Unit.xml_config.arBonuses[bid].nLiquids; j++)
                                                        Unit.xml_config.arIngredients[Unit.xml_config.arBonuses[bid].GemLiquids[j]].Count[0] += (Unit.player.Remakes[p][i] + 1) * liquids;
                                                }
                                            }
                                        }
                                    }
                                    // Die Ausgabe ist jetzt abhängig von der angeforderten Liste
                                    if (strTemp == "gems")
                                    {
                                        int Gem = Unit.xml_config.GetMaterialId("GEMS");
                                        for (i = 0; i < 10; i++)
                                        {
                                            if ((Gems[i] > 0) && bVisible[ifLevel])
                                                strText.Add((Gems[i]).ToString() + " " + Unit.xml_config.arMaterials[Gem].arLevel[i].Name + " " + _("Juwel"));
                                        }
                                    }
                                    else if (strTemp == "dusts")
                                    {
                                        for (i = 0; i < Unit.xml_config.nIngredients; i++)
                                        {
                                            if (Unit.xml_config.arIngredients[i].bDust && (Unit.xml_config.arIngredients[i].Count[0] > 0) && bVisible[ifLevel])
                                                strText.Add((Unit.xml_config.arIngredients[i].Count[0]).ToString() + " " + Unit.xml_config.arIngredients[i].Name);
                                        }
                                    }
                                    else if (strTemp == "liquids")
                                    {
                                        for (i = 0; i < Unit.xml_config.nIngredients; i++)
                                        {
                                            if (!Unit.xml_config.arIngredients[i].bDust && (Unit.xml_config.arIngredients[i].Count[0] > 0) && bVisible[ifLevel])
                                                strText.Add((Unit.xml_config.arIngredients[i].Count[0]).ToString() + " " + Unit.xml_config.arIngredients[i].Name);
                                        }
                                    }
                                    sGemList = null;
                                }
                            }
                            else if (PieceStat(strTag, out strInsert, ref iPos) > 0)
                            { }	// Hier muß nichts mehr gemacht werden
                            else	// Kein bekannter Tag, also den tag im orginal ausgeben
                                strInsert = "<" + strTag + '>';
                            if (aid >= 0)
                                strInsert = Unit.player.Attributes.Value[aid].ToString();
                            if (bVisible[ifLevel])
                            {
                                if (iPos < strTag.Length)
                                {	// Gibt eine Feldlänge
                                    int iLen = strTag.Substring(iPos, Math.Min(2, strTag.Length - iPos)).ToIntDef(0);
                                    if (strInsert.Length < iLen)
                                        strLine += new string(' ', iLen - strInsert.Length);
                                }
                                strLine += strInsert;
                            }
                            bTag = false;
                        }
                        else
                        {	// Im Tag
                            if (cCur == ' ')
                                bName = false;
                            strTag += cCur;
                            if (bName) strName += cCur;
                        }
                    }
                    else	// Wir sind im normalen Text
                    {
                        if (cCur == '<')
                        {
                            bTag = true;
                            bName = true;
                            strTag = "";
                            strName = "";
                        }
                        else if (cCur == '\n')
                        {
                            if (bVisible[ifLevel] && !bConditional) strText.Add(strLine);
                            strLine = "";
                            bConditional = false;
                        }
                        else
                            if (bVisible[ifLevel]) strLine += cCur;
                    }
                    _in.get(out cCur);
                }
                tbReport.Lines = strText.Strings.ToArray();
                strText = null;
            }
            catch (Exception)
            {
                Utils.DebugPrint("Es ist ein Fehler in TfrmReport::DoReport aufgetreten!");
            }
        }

        // Wandelt die Positionsspezifischen Werte um
        // Also sowas wie "Chest usedpoints"
        // in ist der String wo das drin steht,
        // out ist das wo das Ergebnis hinkommt
        // Pos ist immer die erste Attribut
        // der Returnwert gibt die Länge des Ausgabestrings zurück
        private int PieceStat(string _in, out string _out, ref int pos)
        {
            _out = "";
            int iPos;	// Um welche Position geht es
            string strName = _in.Substring(0, pos - 1);
            if (strName == "chest")
                iPos = 5;
            else if (strName == "arms")
                iPos = 4;
            else if (strName == "head")
                iPos = 1;
            else if (strName == "legs")
                iPos = 3;
            else if (strName == "hands")
                iPos = 0;
            else if (strName == "feet")
                iPos = 2;
            else if (strName == "arms")
                iPos = 4;
            else if (strName == "rweapon")
                iPos = 6;
            else if (strName == "lweapon")
                iPos = 7;
            else if (strName == "2handed")
                iPos = 8;
            else if (strName == "ranged")
                iPos = 9;
            else if (strName == "neck")
                iPos = 10;
            else if (strName == "cloak")
                iPos = 11;
            else if (strName == "jewel")
                iPos = 12;
            else if (strName == "belt")
                iPos = 13;
            else if (strName == "rring")
                iPos = 14;
            else if (strName == "lring")
                iPos = 15;
            else if (strName == "rwrist")
                iPos = 16;
            else if (strName == "lwrist")
                iPos = 17;
            else if (strName == "myth")
                iPos = 18;
            else	// Ist kein slot, deshalb Funktion beenden
                return 0;
            // Position des ersten Leerzeichens nach dem Namen
            int pos2 = _in.Substring(pos, Math.Min(99, _in.Length - pos)).IndexOf(' ');
            // Wenn es das Leerzeichen nicht gibt, dann die restliche Stringlänge nehmen
            if (pos2 < 0) pos2 = _in.Length - pos;
            string strAttr1 = _in.Substring(pos, pos2);
            pos += pos2 + 1;
            bool bTemp = true;
            if (strAttr1 == "name")
                _out = Unit.player.ItemName[iPos];
            else if (strAttr1 == "af")
            {	// Der Wert ist abhängig von Position
                if (iPos < 6)
                    _out = Unit.player.AF[iPos].ToString();
                else
                    _out = (Unit.player.DPS[iPos] * 0.1).ToString("##0.0");
            }
            else if (strAttr1 == "quality")
                _out = (Unit.player.Quality[iPos]).ToString() + "%";
            else if (strAttr1 == "drop")
            {
                _out = (Unit.player.ItemType[iPos] != EItemType.Crafted) ? "1" : "";
                bTemp = false;
            }
            else if (strAttr1 == "pcmade")
            {
                _out = (Unit.player.ItemType[iPos] == EItemType.Crafted) ? "1" : "";
                bTemp = false;
            }
            else if (strAttr1 == "notempty")
            {	// liefert eine 1, wenn Slot nicht leer ist
                _out = "";
                for (int i = 0; i < Unit.player.nItemEffects[iPos]; i++)
                {
                    if (Unit.player.ItemEffect[iPos][i] >= 0)
                    {
                        _out = "1";
                        break;
                    }
                }
                bTemp = false;
            }
            else if (strAttr1 == "bonus")
                _out = (Unit.player.Bonus[iPos]).ToString() + "%";
            else if (strAttr1 == "level")
                _out = Unit.player.ItemLevel[iPos].ToString();
            else if (strAttr1 == "fundort")
                _out = Unit.player.Origin[iPos];
            else if (strAttr1 == "utility")
                _out = (Unit.player.Item[iPos].CalcNutzen(false)).ToString("###0.0");
            else if (strAttr1 == "effects")
            {	// Liste alle Effekte in einer Zeile
                for (int i = 0; i < Unit.player.nItemEffects[iPos]; i++)
                {
                    int bid = Unit.player.ItemEffect[iPos][i];
                    if (bid >= 0)
                    {
                        if (i != 0) _out += ",";
                        _out += " " + (Unit.player.EffectValue[iPos][i]).ToString() + " " + Unit.xml_config.arBonuses[bid].Names[0];
                    }
                }
            }
            else if (strAttr1 == "availablepoints")
                _out = Unit.player.CalcAvailIP(iPos).ToString();
            else if (strAttr1 == "usedpoints")
            {
                int iMaxIP = 0;
                int iSumIP = 0;
                for (int i = 0; i < Unit.player.nItemEffects[iPos]; i++)
                {
                    int iIP = Unit.player.EffectIP[iPos][i];
                    if (Unit.player.ItemEffect[iPos][i] >= 0)
                    {
                        if (iIP > iMaxIP) iMaxIP = iIP;
                        iSumIP += iIP;
                    }
                }
                _out = (((double)iSumIP + iMaxIP) / 2).ToString("##0.0");
            }
            else if (strAttr1 == "overcharge")
            {
                int oc = Unit.player.CalcOverCharge(iPos);
                if (oc > 100)
                    _out = _("nicht nötig");
                else if (oc < -100)
                    _out = _("unmöglich");
                else if (oc > 0)
                    _out = (oc).ToString() + "%";
                else	// BOOM
                    _out = "BOOM (" + (oc).ToString() + "%)";
            }
            else if (strAttr1.Length > 3 && strAttr1.Substring(0, 3) == "gem")
            {	// Gem-Werte
                int gem = strAttr1.Substring(3, Math.Min(2, strAttr1.Length - 3)).ToIntDef(0);
                Debug.Assert(gem > 0);
                // Position des ersten Leerzeichens nach dem Attribut
                pos2 = _in.Substring(pos, Math.Min(99, _in.Length - pos)).IndexOf(' ');
                // Wenn es das Leerzeichen nicht gibt, dann die restliche Stringlänge nehmen
                if (pos2 < 0) pos2 = _in.Length - pos;
                string strAttr2 = _in.Substring(pos, pos2);
                pos += pos2 + 1;
                if (strAttr2 == "amount")
                {
                    if (Unit.player.ItemEffect[iPos][gem - 1] >= 0)
                        _out = Unit.player.EffectValue[iPos][gem - 1].ToString();
                }
                else if (strAttr2 == "notempty")
                {	// dient dem ausblenden von leeren Stats
                    _out = (Unit.player.ItemEffect[iPos][gem - 1] >= 0) ? "1" : "";
                    bTemp = false;
                }
                else if (strAttr2 == "effect")
                {
                    int bid = Unit.player.ItemEffect[iPos][gem - 1];
                    if (bid >= 0)
                        _out = Unit.xml_config.arBonuses[bid].Names[0];
                    else
                        _out = _("Leer");
                }
                else if (strAttr2 == "quality")
                {
                    if (Unit.player.ItemEffect[iPos][gem - 1] >= 0)
                        _out = (Unit.player.EffectQuality[iPos][gem - 1] + 96).ToString() + "%";
                }
                else if (strAttr2 == "price")
                {
                    if (Unit.player.ItemEffect[iPos][gem - 1] >= 0)
                        _out = Utils.Int2Gold(Unit.player.GemCost[iPos][gem - 1]);
                }
                else if (strAttr2 == "name")
                {
                    int bid;
                    if ((bid = Unit.player.ItemEffect[iPos][gem - 1]) >= 0)
                    {
                        int mid = Unit.xml_config.GetMaterialId("GEMS");
                        Debug.Assert(mid >= 0);
                        // Den Gemlevel bestimmen
                        for (int i = 0; i < 10; i++)
                        {
                            if (Unit.xml_config.arBonuses[bid].Gemvalue[i] == Unit.player.EffectValue[iPos][gem - 1])
                            {
                                _out = "(" + Unit.xml_config.arMaterials[mid].arLevel[i].GemPrefix
                                    + ") " + Unit.xml_config.arBonuses[bid].GemName;
                                break;
                            }
                        }
                    }
                }
                else
                    bTemp = false;
            }
            else
            {	// Anderes Attribut
                bTemp = false;
            }
            if (bTemp)
            {	// Es gab eine Ausgabe. Gib auf jeden Fall mehr als 0 zurück
                if (_out.Length == 0)
                    return 1;
            }
            return _out.Length;
        }

        private void TfrmReport_FormCreate(object sender, EventArgs e)
        {
            TGnuGettextInstance.TranslateComponent(this);
            Utils.LoadWindowPosition("Report", this, true);
            int l = Utils.GetRegistryString("LastReport", "0").ToIntDef(0);
            TSpeedButton btReport = (TSpeedButton)this.FindComponent("btReport" + (l + 1).ToString());
            btReport.Checked = true;
            btReportClick(btReport, EventArgs.Empty);
        }
        //---------------------------------------------------------------------------

        private void TfrmReport_FormClose(object sender, FormClosedEventArgs e)
        {
            Utils.SaveWindowPosition("Report", this, true);
        }
        //---------------------------------------------------------------------------

        private void btSaveClick(object sender, EventArgs e)
        {
            if (SaveDialog1.ShowDialog() != DialogResult.None)
                tbReport.SaveFile(SaveDialog1.FileName, RichTextBoxStreamType.PlainText);
        }
        //---------------------------------------------------------------------------

        private void btClipboardClick(object sender, EventArgs e)
        {
            string text;
            using (MemoryStream stream = new MemoryStream())
            {
                tbReport.SaveFile(stream, RichTextBoxStreamType.UnicodePlainText);
                stream.Position = 0;
                using (StreamReader reader = new StreamReader(stream, Encoding.Unicode))
                {
                    text = reader.ReadToEnd();
                }
            }
            Clipboard.SetText(text);
        }
        //---------------------------------------------------------------------------

        private void btPrintClick(object sender, EventArgs e)
        {
            PrintDialog1.Document = tbReport.GetPrintDocument();
            if (PrintDialog1.ShowDialog() != DialogResult.None)
            {
                //TODO: remove before release
                tbReport.GetPrintDocument().Preview("Moras Konfigurations-Report");
                tbReport.GetPrintDocument().Print("Moras Konfigurations-Report");
            }
        }
        //---------------------------------------------------------------------------
    }

    static partial class Unit
    {
        internal static TfrmReport frmReport;
    }
}
