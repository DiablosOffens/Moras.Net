using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Collections;
using System.Globalization;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.ComponentModel;
using System.Drawing.Design;

namespace DelphiClasses
{
    //TODO: create unit tests
    //TODO: fix all uses of indices and that name is no longer the value if there is no separator char
    //TODO: maybe re-implement without use of NameValueCollection with pattern from delphi
    public delegate int TStringListSortCompare(TStringList List, int Index1, int Index2);
    public enum TDuplicates { dupIgnore, dupAccept, dupError }
    [Serializable]
    [DefaultProperty("Strings")]
    public class TStringList : NameValueCollection, IList, INotifyCollectionChanged
    {
        private const string SDuplicateString = "String list does not allow duplicates";
        private const string SSortedListError = "Operation not allowed on sorted list";
        private const string SListIndexError = "List index ({0}) out of bounds";
        private ArrayList FList;

        private int FUpdateCount;
        protected int UpdateCount { get { return FUpdateCount; } }

        private bool FSpecialCharsInited;
        private char FNameValueSeparator;

        private void EnsureSpecialChars()
        {
            if (!FSpecialCharsInited)
            {
                FNameValueSeparator = '=';
                FSpecialCharsInited = true;
            }
        }

        private TDuplicates FDuplicates;
        [DefaultValue(TDuplicates.dupIgnore)]
        public TDuplicates Duplicates { get { return FDuplicates; } set { FDuplicates = value; } }

        bool FSorted;
        bool FForceSort;
        [DefaultValue(false)]
        public bool Sorted { get { return FSorted; } set { SetSorted(value); } }

        private void SetSorted(bool Value)
        {
            if (FSorted != Value)
            {
                if (Value) Sort();
                FSorted = Value;
            }
        }

        private IEqualityComparer<string> _comparer;
        private static readonly FieldInfo _keyComparerField = typeof(NameValueCollection).GetField("_keyComparer", BindingFlags.Instance | BindingFlags.NonPublic);
        private bool FCaseSensitive;
        [DefaultValue(false)]
        public bool CaseSensitive { get { return FCaseSensitive; } set { SetCaseSensitive(value); } }

        private void SetCaseSensitive(bool b)
        {
            if (b == FCaseSensitive)
                return;
            FCaseSensitive = b;
            _comparer = b ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase;
            KeyValuePair<string, string[]>[] items = SaveItems(0);
            _keyComparerField.SetValue(this, _comparer);
            BaseClear();
            if (items != null)
                RestoreItems(items);

            if (FSorted)
            {
                FForceSort = true;
                Sort();
                FForceSort = false;
            }
        }

        public event EventHandler Changing;
        public event EventHandler Change;

        #region INotifyCollectionChanged Members

        private event NotifyCollectionChangedEventHandler _collectionChanged;
        event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
        {
            add { _collectionChanged += value; }
            remove { _collectionChanged -= value; }
        }

        private void NotifyObservers(NotifyCollectionChangedEventArgs e)
        {
            var handler = _collectionChanged;
            if (handler != null)
                handler(this, e);
        }

        #endregion

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override int Count
        {
            get
            {
                int count = base.Count;
                int result = 0;
                for (int i = 0; i < count; i++)
                {
                    string[] values = GetValues(i);
                    result += values.Length;
                }
                return result;
            }
        }

        private NamesCollection _names;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public NamesCollection Names { get { return _names ?? (_names = new NamesCollection(this)); } }

        private ValuesCollection _values;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ValuesCollection Values { get { return _values ?? (_values = new ValuesCollection(this)); } }

        private StringsCollection _strings;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public StringsCollection Strings { get { return _strings ?? (_strings = new StringsCollection(this)); } }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string this[int index] { get { return GetString(index); } set { SetString(index, value); } }

        private ObjectsCollection _objects;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ObjectsCollection Objects { get { return _objects ?? (_objects = new ObjectsCollection(this)); } }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Text { get { return GetTextStr(); } set { SetTextStr(value); } }

        private void Create(int count)
        {
            EnsureSpecialChars();
            _comparer = StringComparer.InvariantCultureIgnoreCase;
            FList = new ArrayList(new object[count]);
        }

        public TStringList() { Create(0); }
        public TStringList(NameValueCollection col) : base(col) { Create(Count); }
        protected TStringList(SerializationInfo info, StreamingContext context) : base(info, context) { Create(Count); }

        public virtual void Assign(TStringList Source)
        {
            BeginUpdate();
            try
            {
                Clear();
                FSpecialCharsInited = Source.FSpecialCharsInited;
                FNameValueSeparator = Source.FNameValueSeparator;
                AddStrings(Source);
            }
            finally
            {
                EndUpdate();
            }
        }

        protected virtual void OnChanging(EventArgs e)
        {
            if (FUpdateCount == 0)
            {
                var handler = Changing;
                if (handler != null)
                    handler(this, e);
            }
        }

        protected virtual void OnChanged(EventArgs e)
        {
            if (FUpdateCount == 0)
            {
                var handler = Change;
                if (handler != null)
                    handler(this, e);
                NotifyObservers(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        protected virtual string GetTextStr()
        {
            if (Count == 0)
                return "";

            string result = GetString(0);
            for (int i = 1; i < Count; i++)
            {
                result += Environment.NewLine + GetString(i);
            }
            return result;
        }

        protected virtual int DoCompareText(string s1, string s2)
        {
            if (FCaseSensitive)
                return string.Compare(s1, s2, false);
            else
                return string.Compare(s1, s2, true);
        }

        protected virtual int CompareStrings(string s1, string s2)
        {
            return DoCompareText(s1, s2);
        }

        private void DoSetTextStr(string value, bool clear)
        {
            BeginUpdate();
            try
            {
                if (clear)
                    Clear();

                //accept all line break formats not only Environment.NewLine
                if (value != null)
                    foreach (string line in value.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None))
                        Add(line);
            }
            finally
            {
                EndUpdate();
            }
        }

        protected virtual void SetTextStr(string value)
        {
            DoSetTextStr(value, true);
        }

        private KeyValuePair<string, string[]>[] SaveItems(int index)
        {
            KeyValuePair<string, string[]>[] saveditems = null;
            int ivalue = index;
            int ikey;
            int count = base.Count;
            for (ikey = 0; ikey < count; ikey++)
            {
                string[] values = GetValues(ikey);
                ivalue -= values.Length;
                if (ivalue < 0)
                {
                    count -= ikey;
                    saveditems = new KeyValuePair<string, string[]>[count];
                    if (values.Length + ivalue > 0)
                    {
                        string[] savedvalues = new string[-ivalue];
                        Array.Copy(values, values.Length + ivalue, savedvalues, 0, -ivalue);
                        saveditems[0] = new KeyValuePair<string, string[]>(GetKey(ikey), savedvalues);
                        //HACK: as long as NameValueCollection internally uses ArrayList, do the same here
                        string[] newvalues = new string[values.Length + ivalue];
                        Array.Copy(values, newvalues, values.Length + ivalue);
                        base.BaseSet(ikey, new ArrayList(newvalues));
                    }
                    else
                    {
                        saveditems[0] = new KeyValuePair<string, string[]>(GetKey(ikey), values);
                        base.BaseRemoveAt(ikey);
                    }
                    ikey++;
                    break;
                }
            }
            if (saveditems != null)
            {
                for (int i = 1; i < count; i++, ikey++)
                {
                    saveditems[i] = new KeyValuePair<string, string[]>(GetKey(ikey), GetValues(ikey));
                    base.BaseRemoveAt(ikey);
                }
            }
            return saveditems;
        }

        private void RestoreItems(KeyValuePair<string, string[]>[] items)
        {
            foreach (var item in items)
            {
                if (item.Value != null)
                {
                    foreach (var value in item.Value)
                    {
                        base.Add(item.Key, value);
                    }
                }
                else
                {
                    Debug.Assert(false);
                }
            }
        }

        protected virtual void InsertItem(int index, string line)
        {
            InsertItem(index, line, null);
        }

        protected virtual void InsertItem(int index, string line, object obj)
        {
            int count = Count;
            if (index < 0 || index > count)
                throw new IndexOutOfRangeException();

            int pos = line.IndexOf(FNameValueSeparator);
            string name = null;
            string value = line;
            if (pos != -1)
            {
                name = line.Substring(0, pos);
                value = line.Substring(pos + 1, line.Length - pos - 1);
            }

            OnChanging(EventArgs.Empty);

            // not ideal but we keep compatible with NameValueCollection
            KeyValuePair<string, string[]>[] items = index < count ? SaveItems(index) : null;

            base.Add(name, value);
            FList.Insert(index, obj);

            if (items != null)
                RestoreItems(items);
            Debug.Assert(Count > count);

            OnChanged(EventArgs.Empty);
        }

        public override void Add(string name, string value)
        {
            Add(value != null ? name + FNameValueSeparator + value : name);
        }

        public virtual int Add(string line)
        {
            if (line == null)
                throw new ArgumentNullException("line");

            int index = Count;
            if (Sorted)
            {
                if (Find(line, out index))
                {
                    switch (Duplicates)
                    {
                        case TDuplicates.dupIgnore:
                            return index;
                        case TDuplicates.dupError:
                            throw new Exception(SDuplicateString);
                        default:
                            break;
                    }
                }
            }
            InsertItem(index, line);
            return index;
        }

        public virtual int AddObject(string line, object AObject)
        {
            int result = Add(line);
            FList[result] = AObject;
            return result;
        }

        public virtual void AddText(string text)
        {
            DoSetTextStr(text, false);
        }

        public virtual void AddStrings(TStringList TheStrings)
        {
            try
            {
                BeginUpdate();
                int count = TheStrings.Count;
                for (int i = 0; i < count; i++)
                {
                    this.AddObject(TheStrings[i], TheStrings.Objects[i]);
                }
            }
            finally
            {
                EndUpdate();
            }
        }

        public virtual void AddStrings(string[] TheStrings)
        {
            try
            {
                BeginUpdate();
                int high = Extensions.High(TheStrings);
                for (int i = Extensions.Low(TheStrings); i <= high; i++)
                {
                    this.Add(TheStrings[i]);
                }
            }
            finally
            {
                EndUpdate();
            }
        }

        public void BeginUpdate()
        {
            if (FUpdateCount == 0)
                OnChanging(EventArgs.Empty);
            FUpdateCount++;
        }

        public void EndUpdate()
        {
            if (FUpdateCount > 0)
                FUpdateCount--;
            if (FUpdateCount == 0)
                OnChanged(EventArgs.Empty);
        }

        public virtual bool Find(string line, out int Index)
        {
            bool result = false;
            // Use binary search.
            int L = 0;
            int R = Count - 1;
            while (L <= R)
            {
                int I = L + (R - L) / 2;
                int CompareRes = DoCompareText(line, GetString(I));
                if (CompareRes > 0)
                    L = I + 1;
                else
                {
                    R = I - 1;
                    if (CompareRes == 0)
                    {
                        result = true;
                        if (Duplicates != TDuplicates.dupAccept)
                            L = I; // forces end of while loop
                    }
                }
            }
            Index = L;
            return result;
        }

        public virtual int IndexOf(string line)
        {
            if (!Sorted)
            {
                int count = Count;
                for (int i = 0; i < count; i++)
                {
                    if (DoCompareText(GetString(i), line) == 0)
                        return i;
                }
                return -1;
            }
            else
            {
                int result;
                // faster using binary search...
                if (!Find(line, out result))
                    result = -1;
                return result;
            }
        }

        public virtual int IndexOfName(string name)
        {
            int count = Count;
            for (int i = 0; i < count; i++)
            {
                if (DoCompareText(GetName(i), name) == 0)
                    return i;
            }
            return -1;
        }

        public virtual int IndexOfObject(object AObject)
        {
            int count = Count;
            for (int i = 0; i < count; i++)
            {
                if (Objects[i] == AObject)
                    return i;
            }
            return -1;
        }

        public virtual void Insert(int Index, string ALine)
        {
            if (Sorted)
                throw new Exception(SSortedListError);
            else
                if (Index < 0 || Index > Count)
                    throw new ArgumentOutOfRangeException("Index", Index, SListIndexError);
                else
                    InsertItem(Index, ALine);
        }

        public void InsertObject(int Index, string ALine, object AObject)
        {
            Insert(Index, ALine);
            FList[Index] = AObject;
        }

        protected virtual string GetString(int index)
        {
            for (int ikey = 0; ikey < base.Count; ikey++)
            {
                string[] values = GetValues(ikey);
                index -= values.Length;
                if (index < 0)
                {
                    string value = values[values.Length + index];
                    string name = GetKey(ikey);
                    if (name == null)
                        return value;
                    return name + FNameValueSeparator + value;
                }
            }
            throw new ArgumentOutOfRangeException("index");
        }

        protected virtual object GetObject(int Index)
        {
            if (Index < 0 || Index >= Count)
                throw new ArgumentOutOfRangeException("Index");
            return FList[Index];
        }

        public void GetNameValue(int Index, out string AName, out string AValue)
        {
            AName = string.Empty;
            AValue = string.Empty;
            int count = base.Count;
            for (int ikey = 0; ikey < count; ikey++)
            {
                string[] values = GetValues(ikey);
                Index -= values.Length;
                if (Index < 0)
                {
                    AValue = values[values.Length + Index];
                    AName = GetKey(ikey) ?? string.Empty;
                    break;
                }
            }
        }

        private string GetName(int Index)
        {
            string result, v;
            GetNameValue(Index, out result, out v);
            return result;
        }

        private string GetValue(string Name)
        {
            return GetValues(Name)[0];
        }

        private void SetValue(string Name, string Value)
        {
            Set(Name, Value);
        }

        protected string GetValueFromIndex(int Index)
        {
            string result, n;
            GetNameValue(Index, out n, out result);
            return result;
        }

        protected void SetValueFromIndex(int Index, string Value)
        {
            if (string.IsNullOrEmpty(Value))
                RemoveAt(Index);
            else if (Index < 0)
                Add(null, Value);
            else
            {
                EnsureSpecialChars();
                Strings[Index] = GetName(Index) + FNameValueSeparator + Value;
            }
        }

        protected virtual void SetString(int Index, string line)
        {
            if (Sorted)
                throw new Exception(SSortedListError);
            if (Index < 0 || Index >= Count)
                throw new ArgumentOutOfRangeException("Index", Index, SListIndexError);
            OnChanging(EventArgs.Empty);
            object Obj = FList[Index];
            RemoveAt(Index);
            InsertObject(Index, line, Obj);
            OnChanged(EventArgs.Empty);
        }

        protected virtual void SetObject(int Index, object AObject)
        {
            if (Index < 0 || Index >= Count)
                throw new ArgumentOutOfRangeException("Index");
            OnChanging(EventArgs.Empty);
            FList[Index] = AObject;
            OnChanged(EventArgs.Empty);
        }

        public override void Clear()
        {
            if (Count == 0)
                return;

            OnChanging(EventArgs.Empty);
            base.Clear();
            FList.Clear();
            OnChanged(EventArgs.Empty);
        }

        public override void Remove(string name)
        {
            OnChanging(EventArgs.Empty);
            int index = IndexOfName(name);
            string[] values = GetValues(name);
            base.Remove(name);
            for (int i = 0; i < values.Length; i++)
            {
                FList.RemoveAt(index);
            }
            OnChanged(EventArgs.Empty);
        }

        public virtual void RemoveAt(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");

            OnChanging(EventArgs.Empty);
            int ivalue = index;
            int ikey;
            int count = base.Count;
            for (ikey = 0; ikey < count; ikey++)
            {
                string[] values = GetValues(ikey);
                ivalue -= values.Length;
                if (ivalue < 0)
                {
                    if (values.Length > 1)
                    {
                        //HACK: as long as NameValueCollection internally uses ArrayList, do the same here
                        ArrayList newvalues = new ArrayList(values);
                        newvalues.RemoveAt(values.Length + ivalue);
                        base.BaseSet(ikey, newvalues);
                    }
                    else
                    {
                        base.BaseRemoveAt(ikey);
                    }
                    break;
                }
            }
            if (ikey == count)
                throw new ArgumentOutOfRangeException("index");

            FList.RemoveAt(index);
            OnChanged(EventArgs.Empty);
        }

        public override void Set(string name, string value)
        {
            OnChanging(EventArgs.Empty);
            int oldcount = Count;
            base.Set(name, value);
            if (Count > oldcount)
                FList.Add(null);
            OnChanged(EventArgs.Empty);
        }

        public virtual void SaveToFile(string FileName)
        {
            using (FileStream stream = new FileStream(FileName, FileMode.Create))
            {
                SaveToStream(stream);
            }
        }

        public virtual void SaveToStream(Stream stream)
        {
            string text = Text;
            if (string.IsNullOrEmpty(text)) return;
            using (StreamWriter writer = new StreamWriter(stream, Encoding.Default))
            {
                writer.Write(text);
            }
        }

        private class FuncComparer<T> : IComparer<T>
        {
            Comparison<T> _comp;
            public FuncComparer(Comparison<T> comp)
            {
                _comp = comp;
            }
            #region IComparer<T> Members

            public int Compare(T x, T y)
            {
                return _comp(x, y);
            }

            #endregion
        }

        // use default quicksort implementation of framework
        private void QuickSort(int L, int R, TStringListSortCompare CompareFn)
        {
            if (L != 0 || R != Count - 1)
                throw new NotImplementedException(); // implement own quicksort someday?

            int[] map = Enumerable.Range(0, Count).ToArray();
            object[] objs = FList.ToArray();
            Array.Sort(map, objs, new FuncComparer<int>((x, y) => CompareFn(this, x, y)));
            //var items = map.Select(i => new { key = GetKey(i), values = GetValues(i) }).ToArray();
            //foreach (var item in items)
            //{
            //    if (item.values != null)
            //    {
            //        for (int i = 0; i < item.values.Length; i++)
            //        {
            //            base.Add(item.key, item.values[i]);
            //        }
            //    }
            //    else
            //    {
            //        base.Add(item.key, null);
            //    }
            //}
            //FList.AddRange(objs);
            string[] lines = new string[map.Length];
            for (int i = 0; i < map.Length; i++)
            {
                lines[i] = Strings[map[i]];
            }
            this.Clear();
            for (int i = 0; i < lines.Length; i++)
            {
                AddObject(lines[i], objs[i]);
            }
        }

        private static int StringListAnsiCompare(TStringList List, int Index1, int Index2)
        {
            return List.DoCompareText(List.Strings[Index1], List.Strings[Index2]);
        }

        public void Sort()
        {
            CustomSort(StringListAnsiCompare);
        }

        public void CustomSort(TStringListSortCompare CompareFn)
        {
            if ((FForceSort || !Sorted) && Count > 1)
            {
                OnChanging(EventArgs.Empty);
                QuickSort(0, Count - 1, CompareFn);
                OnChanged(EventArgs.Empty);
            }
        }

        #region IList Members

        int IList.Add(object value) { return Add((string)value); }
        void IList.Clear() { Clear(); }
        bool IList.Contains(object value) { return IndexOf((string)value) != -1; }
        int IList.IndexOf(object value) { return IndexOf((string)value); }
        void IList.Insert(int index, object value) { Insert(index, (string)value); }
        void IList.Remove(object value) { throw new NotImplementedException(); }
        void IList.RemoveAt(int index) { RemoveAt(index); }

        object IList.this[int index] { get { return this[index]; } set { this[index] = (string)value; } }

        bool IList.IsFixedSize { get { return false; } }
        bool IList.IsReadOnly { get { return false; } }

        #endregion

        internal enum EnumeratorType { Names, Values, Strings, Objects }

        public override IEnumerator GetEnumerator()
        {
            return new NameValueEnumerator<string>(this, EnumeratorType.Strings);
        }

        private IEnumerator GetBaseEnumerator()
        {
            return base.GetEnumerator();
        }

        [Serializable]
        internal class NameValueEnumerator<TItem> : IEnumerator, IEnumerator<TItem>
        {
            // Fields
            private TStringList _coll;
            private EnumeratorType _type;
            private int _pos;
            private IEnumerator _versionChecker;

            // Methods
            internal NameValueEnumerator(TStringList coll, EnumeratorType type)
            {
                _coll = coll;
                _type = type;
                _versionChecker = _coll.GetBaseEnumerator();
                _pos = -1;
            }

            public bool MoveNext()
            {
                _versionChecker.MoveNext();

                if (_pos < (_coll.Count - 1))
                {
                    _pos++;
                    return true;
                }
                _pos = _coll.Count;
                return false;
            }

            public void Reset()
            {
                _versionChecker.Reset();

                this._pos = -1;
            }

            public object Current
            {
                get
                {
                    if ((_pos < 0x0) || (_pos >= _coll.Count))
                    {
                        throw new InvalidOperationException();//System.Resources.ResourceManager.GetString("InvalidOperation_EnumOpCantHappen")
                    }
                    switch (_type)
                    {
                        case EnumeratorType.Names:
                            return _coll.GetName(_pos);
                        case EnumeratorType.Values:
                            return _coll.GetValueFromIndex(_pos);
                        case EnumeratorType.Strings:
                            return _coll.GetString(_pos);
                        case EnumeratorType.Objects:
                            return _coll.GetObject(_pos);
                        default:
                            throw new InvalidOperationException();
                    }
                }
            }

            #region IEnumerator<TItem> Members

            TItem IEnumerator<TItem>.Current
            {
                get { return (TItem)Current; }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                if (_versionChecker is IDisposable)
                    ((IDisposable)_versionChecker).Dispose();
            }

            #endregion
        }

        public abstract class NameValueCollectionBase<TItem> : IList<TItem>, IList, IEnumerable
        {
            protected internal TStringList _coll;
            private EnumeratorType _type;

            internal NameValueCollectionBase(TStringList coll, EnumeratorType type)
            {
                _coll = coll;
                _type = type;
            }

            public int Count { get { return _coll.Count; } }
            public TItem this[int index] { get { return Get(index); } set { Set(index, value); } }

            public abstract TItem Get(int index);
            public abstract void Set(int index, TItem value);

            #region IEnumerable<TItem> Members

            public IEnumerator<TItem> GetEnumerator() { return new NameValueEnumerator<TItem>(_coll, _type); }

            #endregion

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

            #endregion

            #region IList<TItem> Members

            public virtual int IndexOf(TItem item) { throw new NotImplementedException(); }
            public virtual void Insert(int index, TItem item) { throw new NotImplementedException(); }
            public virtual void RemoveAt(int index) { throw new NotImplementedException(); }

            #endregion

            #region ICollection<TItem> Members

            public virtual int Add(TItem item) { throw new NotImplementedException(); }
            void ICollection<TItem>.Add(TItem item) { Add(item); }
            public virtual void Clear() { throw new NotImplementedException(); }
            public bool Contains(TItem item) { return IndexOf(item) != -1; }
            public virtual bool Remove(TItem item) { throw new NotImplementedException(); }

            public void CopyTo(TItem[] array, int arrayIndex)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array");
                }
                if (array.Rank != 1)
                {
                    throw new ArgumentException("Only one-dimensional array is supported.", "array");//SR.GetString("Arg_MultiRank")
                }
                if (arrayIndex < 0)
                {
                    throw new ArgumentOutOfRangeException("index", string.Format("{0}", arrayIndex.ToString(CultureInfo.CurrentCulture))); //SR.GetString("IndexOutOfRange", args)
                }
                if ((array.Length - arrayIndex) < this._coll.Count)
                {
                    throw new ArgumentException();//SR.GetString("Arg_InsufficientSpace")
                }
                IEnumerator<TItem> enumerator;
                using ((enumerator = this.GetEnumerator()) as IDisposable)
                {
                    while (enumerator.MoveNext())
                    {
                        array[arrayIndex++] = enumerator.Current;
                    }
                }
            }

            public bool IsReadOnly { get { return false; } }

            #endregion

            #region ICollection Members

            void ICollection.CopyTo(Array array, int index) { CopyTo((TItem[])array, index); }

            bool ICollection.IsSynchronized { get { return false; } }
            object ICollection.SyncRoot { get { return ((ICollection)_coll).SyncRoot; } }

            #endregion

            #region IList Members

            int IList.Add(object value) { return Add((TItem)value); }
            void IList.Clear() { Clear(); }
            bool IList.Contains(object value) { return Contains((TItem)value); }
            int IList.IndexOf(object value) { return IndexOf((TItem)value); }
            void IList.Insert(int index, object value) { Insert(index, (TItem)value); }
            void IList.Remove(object value) { Remove((TItem)value); }
            void IList.RemoveAt(int index) { RemoveAt(index); }

            object IList.this[int index] { get { return this[index]; } set { this[index] = (TItem)value; } }

            bool IList.IsFixedSize { get { return false; } }
            #endregion
        }

        [Serializable]
        public class NamesCollection : NameValueCollectionBase<string>
        {
            internal NamesCollection(TStringList coll)
                : base(coll, EnumeratorType.Names)
            {
            }

            public override string Get(int index) { return _coll.GetName(index); }
            public override void Set(int index, string value) { throw new NotSupportedException(); }
        }

        [Serializable]
        public class ValuesCollection : NameValueCollectionBase<string>
        {
            internal ValuesCollection(TStringList coll)
                : base(coll, EnumeratorType.Values)
            {
            }

            public string this[string name] { get { return Get(name); } set { Set(name, value); } }

            public override string Get(int index) { return _coll.GetValueFromIndex(index); }
            public override void Set(int index, string value) { _coll.SetValueFromIndex(index, value); }
            public virtual string Get(string name) { return _coll.GetValue(name); }
            public virtual void Set(string name, string value) { _coll.SetValue(name, value); }
        }

        [Serializable]
        public class StringsCollection : NameValueCollectionBase<string>
        {
            internal StringsCollection(TStringList coll)
                : base(coll, EnumeratorType.Strings)
            {
            }

            public override string Get(int index) { return _coll.GetString(index); }
            public override void Set(int index, string value) { _coll.SetString(index, value); }

            public override int Add(string item) { return _coll.Add(item); }
            public void AddRange(string[] items) { _coll.AddStrings(items); }
            public override void Clear() { _coll.Clear(); }
            public override int IndexOf(string item) { return _coll.IndexOf(item); }
            public override void Insert(int index, string item) { _coll.Insert(index, item); }
            public override void RemoveAt(int index) { _coll.RemoveAt(index); }
        }

        [Serializable]
        public class ObjectsCollection : NameValueCollectionBase<object>
        {
            internal ObjectsCollection(TStringList coll)
                : base(coll, EnumeratorType.Objects)
            {
            }

            public override object Get(int index) { return _coll.GetObject(index); }
            public override void Set(int index, object value) { _coll.SetObject(index, value); }
        }
    }

}
