using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;
using System.ComponentModel;

namespace DelphiClasses
{
    [DefaultProperty("Item")]
    public class TInterfaceList : IInterfaceList, IEnumerable
    {
        private ArrayList FList = new ArrayList();

        #region IInterfaceList Members

        public object Get(int i)
        {
            lock (FList.SyncRoot)
            {
                return FList[i];
            }
        }

        public int GetCapacity()
        {
            lock (FList.SyncRoot)
            {
                return FList.Capacity;
            }
        }

        public int GetCount()
        {
            lock (FList.SyncRoot)
            {
                return FList.Count;
            }
        }

        public void Put(int i, object item)
        {
            lock (FList.SyncRoot)
            {
                FList[i] = item;
            }
        }

        public void SetCapacity(int NewCapacity)
        {
            lock (FList.SyncRoot)
            {
                FList.Capacity = NewCapacity;
            }
        }

        public void SetCount(int NewCount)
        {
            lock (FList.SyncRoot)
            {
                int diff = NewCount - FList.Count;
                if (diff < 0)
                    FList.RemoveRange(FList.Count + diff, -diff);
                else if (diff > 0)
                    FList.AddRange(new object[diff]);
            }
        }

        public void Clear()
        {
            lock (FList.SyncRoot)
            {
                FList.Clear();
            }
        }

        public void Delete(int index)
        {
            lock (FList.SyncRoot)
            {
                FList.RemoveAt(index);
            }
        }

        public void Exchange(int index1, int index2)
        {
            lock (FList.SyncRoot)
            {
                object temp = FList[index1];
                FList[index1] = FList[index2];
                FList[index2] = temp;
            }
        }

        public object First()
        {
            return Get(0);
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            for (int pos = 0; pos < Count; pos++)
            {
                yield return this[pos];
            }
        }

        #endregion

        public int IndexOf(object item)
        {
            lock (FList.SyncRoot)
            {
                return FList.IndexOf(item);
            }
        }

        public int Add(object item)
        {
            lock (FList.SyncRoot)
            {
                return FList.Add(item);
            }
        }

        public void Insert(int i, object item)
        {
            lock (FList.SyncRoot)
            {
                FList.Insert(i, item);
            }
        }

        public object Last()
        {
            return Get(Count - 1);
        }

        public int Remove(object item)
        {
            lock (FList.SyncRoot)
            {
                int result = FList.IndexOf(item);
                if (result >= 0)
                    FList.RemoveAt(result);
                return result;
            }
        }

        public void Lock()
        {
            Monitor.Enter(FList.SyncRoot);
        }

        public void Unlock()
        {
            Monitor.Exit(FList.SyncRoot);
        }

        public TInterfaceList Expand()
        {
            lock (FList.SyncRoot)
            {
                if (Count < Capacity) return this;
                int IncSize = 4;
                if (Capacity > 3) IncSize += 4;
                if (Capacity > 8) IncSize += 8;
                if (Capacity > 127) IncSize += Capacity >> 2;
                SetCapacity(Capacity + IncSize);
                return this;
            }
        }

        public int Capacity { get { return GetCapacity(); } set { SetCapacity(value); } }

        public int Count { get { return GetCount(); } set { SetCount(value); } }

        public object this[int index] { get { return Get(index); } set { Put(index, value); } }

        #endregion
    }
}
