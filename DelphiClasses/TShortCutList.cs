using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Windows.Forms;

namespace DelphiClasses
{
    public class TShortCutList : NameObjectCollectionBase
    {
        public int Add(string s)
        {
            int result = Count;
            Keys shortcuts = (Keys)Enum.Parse(typeof(Keys), s);
            BaseAdd(s, shortcuts);
            return result;
        }

        public void CopyFrom(TShortCutList source)
        {
            BaseClear();
            int count = source.Count;
            for (int i = 0; i < count; i++)
            {
                BaseAdd(source.BaseGetKey(i), source.BaseGet(i));
            }
        }

        public void Clear() { BaseClear(); }

        public int IndexOf(Keys shortcut)
        {
            for (int i = 0; i < Count; i++)
            {
                if ((Keys)BaseGet(i) == shortcut)
                    return i;
            }
            return -1;
        }

        public Keys this[int index]
        {
            get { return (Keys)BaseGet(index); }
        }
    }
}
