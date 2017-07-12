using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace DelphiClasses
{
    [Guid("285DEA8A-B865-11D1-AAA7-00C04FB17A72")]
    //[DefaultProperty("Item")]
    public interface IInterfaceList
    {
        [return: MarshalAs(UnmanagedType.IUnknown)]
        object Get(int i);
        int GetCapacity();
        int GetCount();
        void Put(int i, [MarshalAs(UnmanagedType.IUnknown)] object item);
        void SetCapacity(int NewCapacity);
        void SetCount(int NewCount);
        void Clear();
        void Delete(int index);
        void Exchange(int index1, int index2);
        [return: MarshalAs(UnmanagedType.IUnknown)]
        object First();
        int IndexOf([MarshalAs(UnmanagedType.IUnknown)] object item);
        int Add([MarshalAs(UnmanagedType.IUnknown)] object item);
        void Insert(int i, [MarshalAs(UnmanagedType.IUnknown)] object item);
        [return: MarshalAs(UnmanagedType.IUnknown)]
        object Last();
        int Remove([MarshalAs(UnmanagedType.IUnknown)] object item);
        void Lock();
        void Unlock();
        int Capacity { get; set; }
        int Count { get; set; }
        object this[int index]
        {
            [return: MarshalAs(UnmanagedType.IUnknown)]
            get;
            [param: MarshalAs(UnmanagedType.IUnknown)]
            set;
        }
    }
}
