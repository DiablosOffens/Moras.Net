//---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using DelphiClasses;

namespace Moras.Net.Components
{
    public class TMIComboBox : TComboBox
    {
        private class ImageComboItem
        {
            public ImageComboItem(string text)
            {
                Text = text;
                ImageIndex = -1;
            }

            public ImageComboItem(string text, int imageIndex)
            {
                Text = text;
                ImageIndex = imageIndex;
            }

            public int ImageIndex { get; set; }

            public object Tag { get; set; }

            public string Text { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }

        private ImageList imageList;

        [DefaultValue(DrawMode.OwnerDrawFixed)]
        public new DrawMode DrawMode { get { return base.DrawMode; } set { base.DrawMode = value; } }

        [DefaultValue(ComboBoxStyle.DropDownList)]
        public new ComboBoxStyle DropDownStyle { get { return base.DropDownStyle; } set { base.DropDownStyle = value; } }

        [DefaultValue(16)]
        public new int ItemHeight { get { return base.ItemHeight; } set { base.ItemHeight = value; } }

        [Browsable(false)]
        public new ObjectCollection Items { get { return base.Items; } }

        [Category("Appearance"), DefaultValue(null)]
        public ImageList ImageList
        {
            get { return imageList; }
            set
            {
                if (imageList != value)
                {
                    imageList = value;
                    if (value != null)
                        this.ItemHeight = value.ImageSize.Height;
                }
            }
        }

        [Browsable(false)]
        public int CurrentData { get { return GetCurrentData(); } }

        private IndexerProperty<int, int> _data;
        public IndexerProperty<int, int> Data { get { return _data ?? (_data = new IndexerProperty<int, int> { read = GetData }); } }

        IndexerProperty<string, int> _strings;
        public IndexerProperty<string, int> Strings
        {
            get
            {
                if (_strings == null)
                    _strings = new IndexerProperty<string, int>
                    {
                        read = index =>
                            {
                                if (index < 0 || index >= this.Items.Count)
                                    throw new ArgumentOutOfRangeException("index");
                                var item = (ImageComboItem)this.Items[index];
                                return item.Text;
                            }
                    };
                return _strings;
            }
        }

        //---------------------------------------------------------------------------
        public TMIComboBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
            ItemHeight = 16;
        }
        //---------------------------------------------------------------------------
        /*protected override void OnKeyPress(KeyPressEventArgs e)
        //protected override void OnKeyDown(KeyEventArgs e)
        {
            // Suche ab dem aktuellen Eintrag + 1 bis zum aktuellen Eintrag,
            // ob ein Eintrag mit dem getippten Anfangsbuchstaben auftaucht
            //	bool bFound = false;
            for (int i = 0; i < Items.Count; i++)
            {
                if ((Strings[(i + SelectedIndex + 1) % Items.Count][0] | 0x20) == (e.KeyChar | 0x20))
                {	// gefunden, also selektieren
                    SelectedIndex = (i + SelectedIndex + 1) % Items.Count;
                    base.OnChange(EventArgs.Empty);
                    //			bFound = true;
                    break;
                }
            }
            // Kein Ton, der nervt nur
            //	if (!bFound) Beep();
            e.KeyChar = '\0';
            //base.OnKeyPress(e);
        }*/

        //---------------------------------------------------------------------------
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            int Left;
            if (ImageList == null)
            {	// Gibt keine Imagelist, drum ganz normal zeichnen
                Left = e.Bounds.Left + 1;
            }
            else
            {
                Left = e.Bounds.Left + ImageList.ImageSize.Width + 2;
            }
            e.DrawBackground(); e.DrawFocusRectangle();
            if (e.Index < 0) return;
            ImageComboItem item = (ImageComboItem)this.Items[e.Index];
            if (ImageList != null && item.ImageIndex != -1)
                this.ImageList.Draw(e.Graphics, e.Bounds.Left + 1, e.Bounds.Top, item.ImageIndex); //(State.Contains(odSelected)) ? Imglist::dsSelected : Imglist::dsNormal, itImage
            Color color = e.ForeColor;
            if ((e.State & DrawItemState.Disabled) != 0)
                color = SystemColors.GrayText;
            using (Brush brush = new SolidBrush(color))
                e.Graphics.DrawString(Strings[e.Index], e.Font, brush, Left, e.Bounds.Top + 1);

            base.OnDrawItem(e);
        }

        public int Add(string Caption, int ImageIndex, int Data)
        {
            int ret = Items.Add(new ImageComboItem(Caption) /*, ImageIndex) { Tag = Data }*/);
            // Ich nehme mal an, ret liefert den index des aktuellen Eintrags
            /*arItems.Length = ItemCount;
            // Nun müßte etwas umkopiert werden, wegen sortieren
            for (int i = ItemCount - 1; i > ret; i--)
            {
                arItems[i].ImageIndex = arItems[i - 1].ImageIndex;
                arItems[i].Data = arItems[i - 1].Data;
            }*/
            ((ImageComboItem)Items[ret]).ImageIndex = ImageIndex;
            ((ImageComboItem)Items[ret]).Tag = Data;
            return ret;
        }

        // Liefert das Datenfeld des gewählten Eintrages
        private int GetCurrentData()
        {
            if (SelectedIndex >= 0)
                return (int)((ImageComboItem)Items[SelectedIndex]).Tag;
            else
                return -1;
        }

        // Liefert das Datenfeld des übergebenen Eintrages
        private int GetData(int Index)
        {
            if ((Index >= 0) && (Index < Items.Count))
                return (int)((ImageComboItem)Items[Index]).Tag;
            else
                return -1;
        }

        // Der Eintrag der dem übergebenen String entspricht, wird selektiert
        public int SelectString(string textSelect)
        {
            int ret = FindStringExact(textSelect);
            SelectedIndex = ret;
            return ret;
        }

        // Der Eintrag mit den entsprechenden Daten wird ausgewählt
        public int SelectData(int dataSelect)
        {
            int ret = -1;
            for (int i = 0; i < Items.Count; i++)
            {
                if (Data[i] == dataSelect)
                {
                    ret = i;
                    break;
                }
            }
            SelectedIndex = ret;
            return ret;
        }
        //---------------------------------------------------------------------------
    }
}
