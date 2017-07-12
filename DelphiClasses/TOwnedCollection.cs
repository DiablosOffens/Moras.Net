using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;
using System.Collections.Specialized;

namespace DelphiClasses
{
    public class TCollection<T> : ObservableCollection<T> where T : TCollectionItem
    {
        private int FNextID;
        private int FUpdateCount;
        internal int UpdateCount { get { return FUpdateCount; } }

        internal void Update(TCollectionItem item)
        {
            if (item == null)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            else
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, item));
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (FUpdateCount == 0)
                base.OnCollectionChanged(e);
        }

        protected override void InsertItem(int index, T item)
        {
            item.FID = FNextID++;
            base.InsertItem(index, item);
        }

        public T Add()
        {
            return (T)Activator.CreateInstance(typeof(T), this);
        }

        public void BeginUpdate()
        {
            FUpdateCount++;
        }

        public void EndUpdate()
        {
            FUpdateCount--;
            if (FUpdateCount == 0)
                Update(null);
        }
    }

    public class TOwnedCollection<T> : TCollection<T> where T : TCollectionItem
    {
        private object FOwner;

        protected object GetOwner() { return FOwner; }

        public TOwnedCollection(object AOwner)
        {
            FOwner = AOwner;
        }

        public object Owner { get { return GetOwner(); } }
    }

    public class TCollectionItem
    {
        private TCollection<TCollectionItem> FCollection;
        internal int FID;
        //private int FUpdateCount;

        private int GetIndex()
        {
            if (FCollection != null)
                return FCollection.IndexOf(this);
            else
                return -1;
        }

        protected virtual void SetCollection(TCollection<TCollectionItem> Value)
        {
            if (Value != FCollection)
            {
                if (FCollection != null) FCollection.Remove(this);
                if (Value != null) Value.Add(this);
                FCollection = Value;
            }
        }

        protected void OnChanged(bool AllItems)
        {
            if (FCollection != null && FCollection.UpdateCount == 0)
            {
                if (AllItems)
                    FCollection.Update(null);
                else
                    FCollection.Update(this);
            }
        }

        protected virtual string GetDisplayName() { return GetType().FullName; }
        protected virtual void SetIndex(int Value)
        {
            int Temp = GetIndex();
            if (Temp > -1 && Temp != Value)
            {
                FCollection.Move(Temp, Value);
                OnChanged(true);
            }
        }
        protected virtual void SetDisplayName(string Value) { OnChanged(false); }
        //protected int UpdateCount { get { return FUpdateCount; } }

        public TCollectionItem(TCollection<TCollectionItem> ACollection)
        {
            SetCollection(ACollection);
        }
        ~TCollectionItem()
        {
            SetCollection(null);
        }

        public TCollection<TCollectionItem> Collection { get { return FCollection; } set { SetCollection(value); } }
        public int ID { get { return FID; } }
        public int Index { get { return GetIndex(); } set { SetIndex(value); } }
        public string DisplayName { get { return GetDisplayName(); } set { SetDisplayName(value); } }
    }
}
