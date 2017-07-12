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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Moras.Net
{
    //---------------------------------------------------------------------------
    //#define NUM_ATTRIBUTES 64

    public enum EExtensionType
    {
        ET_ATTRIBUTE,
        ET_RESISTANCE,
        ET_SKILL,
        ET_CAPS,
        ET_CATA,
        ET_TOA,
        ET_LOTM
    };

    // Dieses Struct beinhaltet die Zeiger auf alle relevanten
    // AnzeigeKomponenten zu einem Gewicht
    public struct SWeightDisplayView
    {
        public TreeNode pNode;
        public DynamicArray<int> arRows;				// Index to Childs, -1 = leer
        public EExtensionType eType;				// Extensiontype/Addon
    };

    // Dieses Struct beinhaltet die Zeiger auf alle relevanten
    // AnzeigeKomponenten zu einem Gewicht
    public struct SWeightDisplayRow
    {
        public int iParentView;				// Index to Parent (arTabs), -1=none
        public bool bFiltered;					// True => Keine Anzeige
        public int iCurRow; 					// aktuelle Reihe
        public TLabel pLabel;
        public TrackBar pTrackBar;
        public TextBox pEdit;
        public Button pButton0;
        public Button pButton50;
        public Button pButton100;
    };

    public partial class TfrmWeights : TCustomForm
    {
        private static string _(string szMsgId) { return TGnuGettextInstance.gettext(szMsgId); }
        protected override IContainer GetChildContainer() { return components ?? base.GetChildContainer(); }

        private class TemplateItem
        {
            public string Name { get; set; }
            public object Tag { get; set; }

            public TemplateItem(string name, object tag)
            {
                Name = name;
                Tag = tag;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        private int iUpdateLock;
        private Control popParent;

        private DynamicArray<CPlayerWeights> xWeights;
        private DynamicArray<SWeightDisplayView> arViews;
        private DynamicArray<SWeightDisplayRow> arWeightDisplays;
        //---------------------------------------------------------------------------
        public TfrmWeights()
        {
            InitializeComponent();
            ((Bitmap)btCancel.Image).MakeTransparent();
            ((Bitmap)btOk.Image).MakeTransparent();

            popParent = null;

            arViews.Length = 0;
            arWeightDisplays.Length = 0;
            iUpdateLock = 0;
        }

        /*~TfrmWeights()
        {
            int i, idx;
            for (i = 0; i < arWeightDisplays.Length; i++)
            {
                if (arWeightDisplays[i].iParentView >= 0)
                {
                    arWeightDisplays[i].pLabel.Parent = null;
                    arWeightDisplays[i].pTrackBar.Parent = null;
                    arWeightDisplays[i].pEdit.Parent = null;
                    arWeightDisplays[i].pButton0.Parent = null;
                    arWeightDisplays[i].pButton50.Parent = null;
                    arWeightDisplays[i].pButton100.Parent = null;

                    arWeightDisplays[i].pLabel.Dispose();
                    arWeightDisplays[i].pTrackBar.Dispose();
                    arWeightDisplays[i].pEdit.Dispose();
                    arWeightDisplays[i].pButton0.Dispose();
                    arWeightDisplays[i].pButton50.Dispose();
                    arWeightDisplays[i].pButton100.Dispose();
                }
            }
            arWeightDisplays.Length = 0;
            for (i = 0; i < arViews.Length; i++)
            {
                arViews.Array[i].arRows.Length = 0;
            }
            arViews.Length = 0;
        }*/

        const int LAYOUT_LEFT = 5;
        const int LAYOUT_TOP = 5;
        const int LAYOUT_LINEHEIGHT = 20;
        const int LAYOUT_PADDING = 3;
        const int LAYOUT_LABEL_WIDTH = 185;
        const int LAYOUT_TRACKBAR_WIDTH = 150;
        const int LAYOUT_TRACKBAR_RIGHTMARGIN = 7;
        const int LAYOUT_EDIT_WIDTH = 29;
        const int LAYOUT_BUTTON_WIDTH = 29;

        //---------------------------------------------------------------------------
        // Im Tag vom lbWeight wird das Attribut gespeichert.
        // Das macht das auswerten einiges einfacher.
        private void Form_Show(object sender, EventArgs e)
        {
            int i, did, idx, iView;

            // Root Text explizit setzen da dieser wohl nicht automatisch von GNUGetText übersetzt wird
            TvWeights.TopNode.Text = _("Wichtungsgruppen");

            // Alle Gruppen erstellen die hier angezeigt werden dürfen
            for (i = 1; i < Unit.xml_config.nGroups; i++)
            {
                if (!Unit.xml_config.arGroups[i].bNoWeight)
                {
                    TreeNode node = TvWeights.TopNode.Nodes.Add(Unit.xml_config.arGroups[i].Name);
                    iView = arViews.Length;
                    arViews.Length++;
                    arViews.Array[iView].pNode = node;
                    //arViews.Array[iView].arRows.Length = 1;
                    //arViews.Array[iView].arRows[0] = -1;
                    arViews.Array[iView].arRows.Length = 0;
                }
            }

            arWeightDisplays.Length = Unit.xml_config.nAttributes;
            // Alle Attribute eintragen die für alle oder das gewählte Reich vom Charakter sind
            for (i = 0; i < Unit.xml_config.nAttributes; i++)
            {
                arWeightDisplays.Array[i].iParentView = -1;
                arWeightDisplays.Array[i].bFiltered = false;
                arWeightDisplays.Array[i].pLabel = null;
                arWeightDisplays.Array[i].pTrackBar = null;
                arWeightDisplays.Array[i].pEdit = null;
                arWeightDisplays.Array[i].pButton0 = null;
                arWeightDisplays.Array[i].pButton50 = null;
                arWeightDisplays.Array[i].pButton100 = null;

                did = Unit.xml_config.GetBonusId(Unit.xml_config.arAttributes[i].id);
                if (did >= 0 && (Unit.xml_config.arBonuses[did].iRealm == 7 || Unit.xml_config.arBonuses[did].iRealm == Unit.player.Realm))
                    if (Unit.player.Weight[i] >= 0)
                        AddWeightView(i, Unit.xml_config.arBonuses[did].idGroup - 1);
            }
            TvWeights.ExpandAll();
            InitTemplates();
        }
        //---------------------------------------------------------------------------
        private void TrackBarChange(object sender, EventArgs e)
        {
            int idx = Utils.ConvertTagToInt(((Control)sender).Tag);
            arWeightDisplays[idx].pEdit.Text = arWeightDisplays[idx].pTrackBar.Value.ToString();
        }
        //---------------------------------------------------------------------------
        private void tbWeightKeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar > 31)
            {
                e.KeyChar = '\0';
                System.Media.SystemSounds.Beep.Play();
            }
        }
        //---------------------------------------------------------------------------
        private void tbWeightChange(object sender, EventArgs e)
        {
            int idx = Utils.ConvertTagToInt(((Control)sender).Tag);
            int val = arWeightDisplays[idx].pEdit.Text.ToIntDef(0);
            if (val > 100) val = 100;
            if (val < 0) val = 0;
            arWeightDisplays[idx].pTrackBar.Value = val;
            if (val == 0 && btNullFilter.Checked)
            {
                //  Deaktivieren falls rausgefiltert
                arWeightDisplays.Array[idx].bFiltered = true;
                UpdateView();
            }
        }
        //---------------------------------------------------------------------------
        private void btOkClick(object sender, EventArgs e)
        {	// Wichtungen übernehmen
            // Das Attribut ist im Tag vom Label gespeichert, was es etwas leichter macht
            for (int i = 0; i < Unit.xml_config.nAttributes; i++)
            {
                if (arWeightDisplays[i].iParentView >= 0)
                {
                    Debug.Assert(arWeightDisplays[i].pTrackBar != null);
                    Unit.player.Weight[i] = arWeightDisplays[i].pTrackBar.Value;
                }
            }
        }
        //---------------------------------------------------------------------------
        private void SetAllValues(object sender, EventArgs e)
        {
            LockView();
            for (int i = 0; i < Unit.xml_config.nAttributes; i++)
            {
                if (arWeightDisplays[i].iParentView >= 0)
                {
                    Debug.Assert(arWeightDisplays[i].pEdit != null);
                    arWeightDisplays[i].pEdit.Text = Utils.ConvertTagToInt(((Control)sender).Tag).ToString();
                }
            }
            UnlockView();
            UpdateView();
        }
        //---------------------------------------------------------------------------
        private void SetSideValues(object sender, EventArgs e)
        {
            // Sender ist leider nicht die Groupbox, in der Menü aufgerufen wurde
            if (popParent == null)
                return;
            LockView();
            for (int i = 0; i < Unit.xml_config.nAttributes; i++)
            {
                if (arWeightDisplays[i].iParentView >= 0)
                {
                    Debug.Assert(arWeightDisplays[i].pEdit != null);
                    if (arWeightDisplays[i].pEdit.Parent == popParent)
                        arWeightDisplays[i].pEdit.Text = Utils.ConvertTagToInt(((Control)sender).Tag).ToString();
                }
            }
            UnlockView();
            UpdateView();
            popParent = null;
        }
        //---------------------------------------------------------------------------
        private void pmWeightsPopup(object sender, CancelEventArgs e)
        {
            // Sender ist die Groupbox, deshalb zwischenspeichern
            popParent = (Control)((ContextMenuStrip)sender).SourceControl;
        }
        //---------------------------------------------------------------------------
        private void Set0(object sender, EventArgs e)
        {
            Debug.Assert(arWeightDisplays[Utils.ConvertTagToInt(((Control)sender).Tag)].pTrackBar != null);
            arWeightDisplays[Utils.ConvertTagToInt(((Control)sender).Tag)].pTrackBar.Value = 0;
        }
        //---------------------------------------------------------------------------
        private void Set50(object sender, EventArgs e)
        {
            Debug.Assert(arWeightDisplays[Utils.ConvertTagToInt(((Control)sender).Tag)].pTrackBar != null);
            arWeightDisplays[Utils.ConvertTagToInt(((Control)sender).Tag)].pTrackBar.Value = 50;
        }
        //---------------------------------------------------------------------------
        private void Set100(object sender, EventArgs e)
        {
            Debug.Assert(arWeightDisplays[Utils.ConvertTagToInt(((Control)sender).Tag)].pTrackBar != null);
            arWeightDisplays[Utils.ConvertTagToInt(((Control)sender).Tag)].pTrackBar.Value = 100;
        }
        //---------------------------------------------------------------------------

        private void TfrmWeights_FormCreate(object sender, EventArgs e)
        {
            TGnuGettextInstance.TranslateComponent(this);
        }
        //---------------------------------------------------------------------------

        private void AktuelleSeiteaufdiesenWertsetzen1Click(object sender, EventArgs e)
        {
            TrackBar control = (TrackBar)pmTrackbar.SourceControl;
            int value = control.Value;
            Panel parent = (Panel)control.Parent;

            LockView();
            for (int i = 0; i < Unit.xml_config.nAttributes; i++)
            {
                if (arWeightDisplays[i].iParentView >= 0)
                {
                    Debug.Assert(arWeightDisplays[i].pEdit != null);
                    if (arWeightDisplays[i].pEdit.Parent == parent)
                        arWeightDisplays[i].pEdit.Text = (value).ToString();
                }
            }
            UnlockView();
            UpdateView();
        }
        //---------------------------------------------------------------------------

        private void AlleSeitenaufdiesenWertsetzen1Click(object sender, EventArgs e)
        {
            TrackBar control = (TrackBar)pmTrackbar.SourceControl;
            int value = control.Value;

            LockView();
            for (int i = 0; i < Unit.xml_config.nAttributes; i++)
            {
                if (arWeightDisplays[i].iParentView >= 0)
                {
                    Debug.Assert(arWeightDisplays[i].pEdit != null);
                    arWeightDisplays[i].pEdit.Text = (value).ToString();
                }
            }
            UnlockView();
            UpdateView();
        }
        //---------------------------------------------------------------------------
        protected bool AddWeightView(int id, int pos = -1) // -1 = egal
        {
            int iView,
                iPos = pos,
                i;
            TLabel pLabel;
            TrackBar pTrackBar;
            TextBox pEdit;
            Button pButton;

            if (id < 0 || id >= arWeightDisplays.Length)
                return false;

            iPos = arViews[pos].arRows.Length;
            arViews.Array[pos].arRows.Length++;
            arViews.Array[pos].arRows[iPos] = id;

            arWeightDisplays.Array[id].iParentView = pos;
            arWeightDisplays.Array[id].iCurRow = iPos;

            int x = 0, y = LAYOUT_TOP + (LAYOUT_LINEHEIGHT * iPos), tord = 1 + 5 * iPos;

            x = LAYOUT_LEFT;

            pLabel = arWeightDisplays.Array[id].pLabel = new TLabel();
            pLabel.Text = "";
            pLabel.Top = y + 2;
            pLabel.Left = x;
            pLabel.Width = LAYOUT_LABEL_WIDTH;
            pLabel.Height = LAYOUT_LINEHEIGHT - 7;
            pLabel.AutoSize = false;
            pLabel.Transparent = true;
            x += LAYOUT_LABEL_WIDTH + LAYOUT_PADDING;

            pLabel.Text = Unit.xml_config.arAttributes[id].Name + ":";
            pLabel.Tag = id;

            pTrackBar = arWeightDisplays.Array[id].pTrackBar = new TrackBar();
            //pTrackBar.ShowHint = false;
            //pTrackBar.ParentShowHint = false;
            pTrackBar.Left = x;
            pTrackBar.Top = y;
            pTrackBar.Width = LAYOUT_TRACKBAR_WIDTH;
            pTrackBar.Height = LAYOUT_LINEHEIGHT - 3;
            pTrackBar.Maximum = 100;
            pTrackBar.LargeChange = 10;
            pTrackBar.TickFrequency = 10;
            //pTrackBar.ThumbLength = 10;
            pTrackBar.TabIndex = tord++;
            pTrackBar.ValueChanged += TrackBarChange;
            pTrackBar.ContextMenuStrip = pmTrackbar;
            pTrackBar.Tag = id;
            x += LAYOUT_TRACKBAR_WIDTH + LAYOUT_TRACKBAR_RIGHTMARGIN + LAYOUT_PADDING;

            pEdit = arWeightDisplays.Array[id].pEdit = new TextBox();
            pEdit.Left = x;
            pEdit.Top = y;
            pEdit.Width = LAYOUT_EDIT_WIDTH;
            pEdit.Height = LAYOUT_LINEHEIGHT - 3;
            pEdit.AutoSize = false;
            pEdit.TabIndex = tord++;
            pEdit.TextChanged += tbWeightChange;
            pEdit.KeyPress += tbWeightKeyPress;
            pEdit.Tag = id;
            x += LAYOUT_EDIT_WIDTH + LAYOUT_PADDING;

            pEdit.Text = Unit.player.Weight[id].ToString();

            pButton = arWeightDisplays.Array[id].pButton0 = new Button();
            pButton.Left = x;
            pButton.Top = y;
            pButton.Width = LAYOUT_BUTTON_WIDTH;
            pButton.Height = LAYOUT_LINEHEIGHT - 1;
            pButton.Text = "0";
            pButton.TabIndex = tord++;
            pButton.Click += Set0;
            pButton.Tag = id;
            x += LAYOUT_BUTTON_WIDTH + LAYOUT_PADDING;

            pButton = arWeightDisplays.Array[id].pButton50 = new Button();
            pButton.Left = x;
            pButton.Top = y;
            pButton.Width = LAYOUT_BUTTON_WIDTH;
            pButton.Height = LAYOUT_LINEHEIGHT - 1;
            pButton.Text = "50";
            pButton.TabIndex = tord++;
            pButton.Click += Set50;
            pButton.Tag = id;
            x += LAYOUT_BUTTON_WIDTH + LAYOUT_PADDING;

            pButton = arWeightDisplays.Array[id].pButton100 = new Button();
            pButton.Left = x;
            pButton.Top = y;
            pButton.Width = LAYOUT_BUTTON_WIDTH;
            pButton.Height = LAYOUT_LINEHEIGHT - 1;
            pButton.Text = "100";
            pButton.Click += Set100;
            pButton.TabIndex = tord++;
            pButton.Tag = id;
            // Hier noch die aktive Seite auf die erste setzen...

            return true;
        }

        //---------------------------------------------------------------------------
        private void TvWeightsChanging(object sender, TreeViewEventArgs e)
        {
            int i;

            // Die neuen Einträge durch zuweisen sichtbar machen
            for (i = 0; i < arViews.Length; i++)
                if (e.Node == arViews[i].pNode)
                    UpdateView(i);
        }
        //---------------------------------------------------------------------------
        private void InitTemplates()
        {
            if (Directory.Exists(Unit.frmMain.FilePath + "weights\\"))
            {
                foreach (string wgtFile in Directory.EnumerateFiles(Unit.frmMain.FilePath + "weights\\", "*.wgt"))
                {
                    CPlayerWeights pWeight = new CPlayerWeights();
                    Debug.Assert(pWeight != null);
                    pWeight.FileName = wgtFile;
                    pWeight.Load();
                    xWeights.Length++;
                    xWeights[xWeights.Length - 1] = pWeight;
                    CbVorlage.Items.Add(new TemplateItem(pWeight.Name, pWeight));
                }
            }
        }
        //---------------------------------------------------------------------------
        private void AssignToWeight(CPlayerWeights pWeight)
        {
            for (int i = 0; i < Unit.xml_config.nAttributes; i++)
            {
                if (arWeightDisplays[i].iParentView >= 0)
                {
                    Debug.Assert(arWeightDisplays[i].pTrackBar != null);
                    pWeight.Weight[i] = arWeightDisplays[i].pTrackBar.Value;
                }
            }
        }
        //---------------------------------------------------------------------------
        private void AssignToView(CPlayerWeights pWeight)
        {
            //	string msg = IntToStr(xml_config.nAttributes) + " zu " + IntToStr(arWeightDisplays.Length);
            //	Utils.DebugPrint(msg);
            for (int i = 0; i < Unit.xml_config.nAttributes; i++)
            {
                //		Utils.DebugPrint(xml_config.arAttributes[i].id);
                if (arWeightDisplays[i].iParentView >= 0)
                {
                    Debug.Assert(arWeightDisplays[i].pTrackBar != null);
                    arWeightDisplays[i].pTrackBar.Value = pWeight.Weight[i];
                    //			msg = "\t=> " + IntToStr(pWeight.Weight[i]);
                    //			Utils.DebugPrint(msg);
                }
            }
        }
        //---------------------------------------------------------------------------
        private void btTemplateSaveClick(object sender, EventArgs e)
        {
            string wgtFile;

            if (CbVorlage.Text != "")
            {
                int idx = CbVorlage.FindStringExact(CbVorlage.Text);
                if (idx >= 0)
                {
                    string msg;
                    msg = string.Format((_("Eine Vorlage mit dem Name '%s' existiert bereits. Soll diese Überschrieben werden?")).Replace("%s", "{0}"), CbVorlage.Text);
                    if (Utils.MorasAskMessage(msg, _("Überschreiben?")) == (int)DialogResult.Yes)
                    {
                        CPlayerWeights pWeight = (CPlayerWeights)((TemplateItem)CbVorlage.Items[CbVorlage.SelectedIndex]).Tag;
                        AssignToWeight(pWeight);
                        pWeight.Save();
                    }
                }
                else
                {
                    CPlayerWeights pWeight = new CPlayerWeights();
                    AssignToWeight(pWeight);
                    wgtFile = Unit.frmMain.FilePath + "weights\\" + CbVorlage.Text + ".wgt";
                    pWeight.FileName = wgtFile;
                    pWeight.Name = CbVorlage.Text;
                    pWeight.Save();
                    CbVorlage.Items.Add(new TemplateItem(pWeight.Name, pWeight));
                }
            }
            else
            {
                Utils.MorasErrorMessage(_("Bitte gib der Vorlage einen Namen."), _("Vorlagenname"));
            }
        }
        //---------------------------------------------------------------------------

        private void btTemplateDeleteClick(object sender, EventArgs e)
        {
            CPlayerWeights pWeight = (CPlayerWeights)((TemplateItem)CbVorlage.Items[CbVorlage.SelectedIndex]).Tag;
            string msg;
            msg = string.Format((_("Soll die Vorlage '%s' wirklich gelöscht werden?")).Replace("%s", "{0}"), pWeight.Name);
            if (Utils.MorasAskMessage(msg, _("Vorlage löschen?")) == (int)DialogResult.Yes)
            {
                CbVorlage.Items.RemoveAt(CbVorlage.SelectedIndex);
                File.Delete(pWeight.FileName);
                pWeight = null;
            }
        }
        //---------------------------------------------------------------------------

        private void CbVorlageClick(object sender, EventArgs e)
        {
            if (CbVorlage.SelectedIndex == -1) return;
            CPlayerWeights pWeight = (CPlayerWeights)((TemplateItem)CbVorlage.Items[CbVorlage.SelectedIndex]).Tag;
            //	pWeight.WriteDebug();
            AssignToView(pWeight);
        }
        //---------------------------------------------------------------------------

        protected void LockView()
        {
            if (iUpdateLock == 0)
            {
                NativeMethods.SendMessage(new HandleRef(SbWeights, SbWeights.Handle), NativeMethods.WM_SETREDRAW, 0, 0);
            }
            iUpdateLock++;
        }

        protected void UnlockView()
        {
            iUpdateLock--;
            if (iUpdateLock == 0)
            {
                NativeMethods.SendMessage(new HandleRef(SbWeights, SbWeights.Handle), NativeMethods.WM_SETREDRAW, 1, 0);
                SbWeights.Invalidate(true);
                // Control.Invalidate(true) doesn't invalidate the non-client region
                NativeMethods.RedrawWindow(new HandleRef(SbWeights, SbWeights.Handle), IntPtr.Zero, NativeMethods.NullHandleRef, NativeMethods.RDW_ERASE | NativeMethods.RDW_FRAME | NativeMethods.RDW_INVALIDATE | NativeMethods.RDW_ALLCHILDREN);
            }
        }

        protected void UpdateView(int id = -1) // -1 = aktuelle Auswahl
        {
            int i, j, idx, rowcnt;

            if (iUpdateLock > 0)
            {
                return;
            }
            LockView();

            // Viewport zurücksetzen da sonst beim neu Aufbauen die oberen Einträge nicht mehr sichtbar sind wenn man schon nach unten scrollte
            SbWeights.VerticalScroll.Value = 0;

            // Alle momentan sichtbaren Einträge entfernen
            for (i = 0; i < arViews.Length; i++)
            {
                if (TvWeights.SelectedNode == arViews[i].pNode)
                {
                    for (j = 0; j < arViews[i].arRows.Length; j++)
                    {
                        idx = arViews[i].arRows[j];
                        if (idx >= 0)
                        {
                            arWeightDisplays[idx].pLabel.Parent = null;
                            arWeightDisplays[idx].pTrackBar.Parent = null;
                            arWeightDisplays[idx].pEdit.Parent = null;
                            arWeightDisplays[idx].pButton0.Parent = null;
                            arWeightDisplays[idx].pButton50.Parent = null;
                            arWeightDisplays[idx].pButton100.Parent = null;
                        }
                    }
                    break;
                }
            }

            if (id >= 0)
                i = id;

            // Falls nichts gewählt ist => keine Anzeige
            if (i < arViews.Length)
            {
                // Die neuen Einträge durch zuweisen sichtbar machen
                rowcnt = 0;
                for (j = 0; j < arViews[i].arRows.Length; j++)
                {
                    idx = arViews[i].arRows[j];
                    if (idx >= 0 && !arWeightDisplays[idx].bFiltered)
                    {
                        int dy = LAYOUT_LINEHEIGHT * (rowcnt - arWeightDisplays[idx].iCurRow);
                        arWeightDisplays.Array[idx].iCurRow = rowcnt;

                        arWeightDisplays[idx].pLabel.Top += dy;
                        arWeightDisplays[idx].pLabel.Parent = SbWeights;

                        arWeightDisplays[idx].pTrackBar.Top += dy;
                        arWeightDisplays[idx].pTrackBar.Parent = SbWeights;

                        arWeightDisplays[idx].pEdit.Top += dy;
                        arWeightDisplays[idx].pEdit.Parent = SbWeights;

                        arWeightDisplays[idx].pButton0.Top += dy;
                        arWeightDisplays[idx].pButton0.Parent = SbWeights;

                        arWeightDisplays[idx].pButton50.Top += dy;
                        arWeightDisplays[idx].pButton50.Parent = SbWeights;

                        arWeightDisplays[idx].pButton100.Top += dy;
                        arWeightDisplays[idx].pButton100.Parent = SbWeights;

                        rowcnt++;
                    }
                }
            }
            UnlockView();
        }

        private void btPlayerFilterClick(object sender, EventArgs e)
        {
            int i;

            // Filter-Tags anpassen
            for (i = 0; i < arWeightDisplays.Length; i++)
                arWeightDisplays.Array[i].bFiltered = false;

            if (btPlayerFilter.Checked)
                for (i = 0; i < arWeightDisplays.Length; i++)
                    arWeightDisplays.Array[i].bFiltered = arWeightDisplays[i].bFiltered || (!Unit.player.AttributeUseable(i));
            if (btNullFilter.Checked)
                for (i = 0; i < arWeightDisplays.Length; i++)
                    arWeightDisplays.Array[i].bFiltered = arWeightDisplays[i].bFiltered || (arWeightDisplays[i].pEdit.Text.ToIntDef(0) == 0);

            // Views aktualisieren
            UpdateView();
        }
        //---------------------------------------------------------------------------


        private void btNullFilterClick(object sender, EventArgs e)
        {
            int i;

            // Filter-Tags anpassen
            for (i = 0; i < arWeightDisplays.Length; i++)
                arWeightDisplays.Array[i].bFiltered = false;

            if (btPlayerFilter.Checked)
                for (i = 0; i < arWeightDisplays.Length; i++)
                    arWeightDisplays.Array[i].bFiltered = arWeightDisplays[i].bFiltered || (!Unit.player.AttributeUseable(i));
            if (btNullFilter.Checked)
                for (i = 0; i < arWeightDisplays.Length; i++)
                    arWeightDisplays.Array[i].bFiltered = arWeightDisplays[i].bFiltered || (arWeightDisplays[i].pEdit.Text.ToIntDef(0) == 0);

            // Views aktualisieren
            UpdateView();
        }
        //---------------------------------------------------------------------------
    }

    static partial class Unit
    {
        internal static TfrmWeights frmWeights;
    }
}
