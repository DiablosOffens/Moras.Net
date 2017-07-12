using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DelphiClasses
{
    public class IndexerProperty<TValue, T>
    {
        public Func<T, TValue> read;
        public Action<T, TValue> write;

        public IndexerProperty()
        {

        }

        public IndexerProperty(Func<T, TValue> read, Action<T, TValue> write)
        {
            this.read = read;
            this.write = write;
        }

        public static IndexerProperty<IndexerProperty<TValue, T2>, T> Create<T2>(Func<T, T2, TValue> read, Action<T, T2, TValue> write)
        {
            return new IndexerProperty<IndexerProperty<TValue, T2>, T>
            {
                read = arg1 => new IndexerProperty<TValue, T2>
                {
                    read = read == null ? (Func<T2, TValue>)null : (arg2) => read(arg1, arg2),
                    write = write == null ? (Action<T2, TValue>)null : (arg2, value) => write(arg1, arg2, value)
                }
            };
        }

        public TValue this[T arg]
        {
            get
            {
                if (read == null)
                    throw new FieldAccessException("Kein read-Accessor angegeben.");
                return read(arg);
            }
            set
            {
                if (write == null)
                    throw new FieldAccessException("Kein write-Accessor angegeben.");
                write(arg, value);
            }
        }
    }

    public class IndexerProperty<TValue, T1, T2>
    {
        public Func<T1, T2, TValue> read;
        public Action<T1, T2, TValue> write;

        public IndexerProperty()
        {

        }

        public IndexerProperty(Func<T1, T2, TValue> read, Action<T1, T2, TValue> write)
        {
            this.read = read;
            this.write = write;
        }

        public IndexerProperty<TValue, T2> this[T1 arg1]
        {
            get
            {
                return new IndexerProperty<TValue, T2>
                {
                    read = read == null ? (Func<T2, TValue>)null : (arg2) => read(arg1, arg2),
                    write = write == null ? (Action<T2, TValue>)null : (arg2, value) => write(arg1, arg2, value)
                };
            }
        }
    }
}
