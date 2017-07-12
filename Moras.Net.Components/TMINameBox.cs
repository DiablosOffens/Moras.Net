//---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using DelphiClasses;

namespace Moras.Net.Components
{
    public class TMINameBox : TComboBox
    {
        private class ComboItem
        {
            public ComboItem(string text)
            {
                Text = text;
            }

            public object Tag { get; set; }

            public string Text { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }

        private int iLastItem;

        [DefaultValue(AutoCompleteMode.None)]
        public new AutoCompleteMode AutoCompleteMode { get { return base.AutoCompleteMode; } set { base.AutoCompleteMode = value; } }

        [Browsable(false)]
        public new ObjectCollection Items { get { return base.Items; } }

        [Browsable(false)]
        public object CurrentData { get { return GetCurrentData(); } }

        private IndexerProperty<object, int> _data;
        public IndexerProperty<object, int> Data { get { return _data ?? (_data = new IndexerProperty<object, int> { read = GetData }); } }

        private IndexerProperty<string, int> _strings;
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
                            var item = (ComboItem)this.Items[index];
                            return item.Text;
                        },
                        write = (index, value) =>
                        {
                            if (index < 0 || index >= this.Items.Count)
                                throw new ArgumentOutOfRangeException("index");
                            var item = (ComboItem)this.Items[index];
                            item.Text = value;
                        }
                    };
                return _strings;
            }
        }

        //---------------------------------------------------------------------------
        public TMINameBox()
        {
            DropDownStyle = ComboBoxStyle.DropDown;
            AutoCompleteMode = AutoCompleteMode.None;
            iLastItem = 0;
        }
        //---------------------------------------------------------------------------
        protected override void OnChange(EventArgs e)
        {
            // Hier wird bei jeder Eingabe der aktuelle Eintrag der Liste gelöscht
            // und durch den String im Eingabefeld ersetzt
            // Außer bei Return und Hoch/Runter, dann gewählten Wert aus der Liste
            // übernehmen. Das passiert aber automatisch
            if (SelectedIndex >= 0)
                iLastItem = SelectedIndex;
            // Hierdurch wird Itemindex geändert. Deshalb speichern
            Strings[iLastItem] = Text;
            int Cursor = SelectionStart;
            if (Cursor == 0) Cursor = 100;	// 0 gibts nicht nach einer eingabe
            SelectedIndex = iLastItem;
            //	SelectionLength = 0;
            SelectionStart = Cursor;	// Setzt Cursor und löscht Markierung
            base.OnChange(e);
        }

        public int Add<T>(string caption, T data)
        {
            int ret = Items.Add(new ComboItem(caption) /* { Tag = data }*/);
            // Ich nehme mal an, ret liefert den index des aktuellen Eintrags
            /*arItems.Length = ItemCount;
            // Nun müßte etwas umkopiert werden, wegen sortieren
            for (int i = ItemCount - 1; i > ret; i--)
            {
                arItems[i].Data = arItems[i - 1].Data;
            }*/
            ((ComboItem)Items[ret]).Tag = Data;
            return ret;
        }

        // Liefert das Datenfeld des gewählten Eintrages
        object GetCurrentData()
        {
            if (SelectedIndex >= 0)
                return ((ComboItem)Items[SelectedIndex]).Tag;
            else
                return null;
        }

        // Liefert das Datenfeld des übergebenen Eintrages
        private object GetData(int Index)
        {
            if ((Index >= 0) && (Index < Items.Count))
                return ((ComboItem)Items[Index]).Tag;
            else
                return null;
        }

        // Der Eintrag der dem übergebenen String entspricht, wird selektiert
        public int SelectString(string textSelect)
        {
            int ret = FindStringExact(textSelect);
            SelectedIndex = ret;
            return ret;
        }

        public int SelectData<T>(T dataSelect)
        {
            int ret = -1;
            for (int i = 0; i < Items.Count; i++)
            {
                if (Data[i].Equals(dataSelect))
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
