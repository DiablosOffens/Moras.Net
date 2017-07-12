using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing.Design;
using System.Collections;

namespace DelphiClasses
{
    [ListBindable(false)]
    [Editor("Moras.Net.Design.TActionListEditor, Moras.Net.Design, Version=1.0.0.0, Culture=neutral, PublicKeyToken=f9135b47417b1285", typeof(UITypeEditor))]
    public class TContainedActionCollection : IList<TContainedAction>, IList
    {
        private TActionList owner;
        private List<TContainedAction> innerList = new List<TContainedAction>();

        public TContainedActionCollection(TActionList owner)
        {
            this.owner = owner;
        }

        public void AddRange(TContainedAction[] actions)
        {
            if (actions == null)
                throw new ArgumentNullException("actions");

            for (int i = 0; i < actions.Length; i++)
            {
                this.Add(actions[i]);
            }
        }

        public void AddRange(TContainedActionCollection actions)
        {
            if (actions == null)
                throw new ArgumentNullException("actions");

            int count = actions.Count;
            for (int i = 0; i < count; i++)
            {
                this.Add(actions[i]);
            }
        }

        #region IList<TContainedAction> Members

        public int IndexOf(TContainedAction item)
        {
            return innerList.IndexOf(item);
        }

        public void Insert(int index, TContainedAction item)
        {
            item.SetActionList(owner);
            innerList.Insert(index, item);
        }

        public void Move(int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || oldIndex > Count - 1)
                throw new IndexOutOfRangeException(string.Format("List index ({0}) out of bounds", oldIndex));
            if (newIndex < 0 || newIndex > Count - 1)
                throw new IndexOutOfRangeException(string.Format("List index ({0}) out of bounds", newIndex));

            TContainedAction item = innerList[oldIndex];
            innerList.RemoveAt(oldIndex);
            innerList.Insert(newIndex, item);
        }

        public void RemoveAt(int index)
        {
            TContainedAction item = null;
            if ((index < this.Count) && (index >= 0x0))
            {
                item = innerList[index];
            }
            innerList.RemoveAt(index);
            if (item != null)
                item.SetActionList(null);
        }

        public TContainedAction this[int index]
        {
            get
            {
                return innerList[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        #region ICollection<TContainedAction> Members

        public void Add(TContainedAction item)
        {
            item.SetActionList(owner);
            innerList.Add(item);
        }

        public void Clear()
        {
            while (Count != 0)
            {
                RemoveAt(Count - 1);
            }
        }

        public bool Contains(TContainedAction item)
        {
            return innerList.Contains(item);
        }

        public void CopyTo(TContainedAction[] array, int arrayIndex)
        {
            innerList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return innerList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(TContainedAction item)
        {
            try
            {
                return innerList.Remove(item);
            }
            finally
            {
                item.SetActionList(null);
            }
        }

        #endregion

        #region IEnumerable<TContainedAction> Members

        public IEnumerator<TContainedAction> GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IList Members

        int IList.Add(object value)
        {
            int result = this.Count;
            this.Add(value as TContainedAction);
            return result;
        }

        void IList.Clear()
        {
            this.Clear();
        }

        bool IList.Contains(object value)
        {
            return this.Contains(value as TContainedAction);
        }

        int IList.IndexOf(object value)
        {
            return this.IndexOf(value as TContainedAction);
        }

        void IList.Insert(int index, object value)
        {
            this.Insert(index, value as TContainedAction);
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        bool IList.IsReadOnly
        {
            get { return this.IsReadOnly; }
        }

        void IList.Remove(object value)
        {
            this.Remove(value as TContainedAction);
        }

        void IList.RemoveAt(int index)
        {
            this.RemoveAt(index);
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = value as TContainedAction;
            }
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)innerList).CopyTo(array, index);
        }

        int ICollection.Count
        {
            get { return this.Count; }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return ((ICollection)innerList).SyncRoot; }
        }

        #endregion
    }
}
