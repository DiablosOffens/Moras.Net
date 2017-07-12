using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.ConstrainedExecution;
using System.Security;

namespace DelphiClasses
{
    internal static class IconUtils
    {
        #region win32 native declarations

        [StructLayout(LayoutKind.Sequential, Pack = 0x2)]
        private struct RESOURCE_ICONDIR
        {
            public ushort idReserved;
            public ushort idType;
            public ushort idCount;
            public RESOURCE_ICONDIRENTRY idEntries;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x2)]
        private struct RESOURCE_ICONDIRENTRY
        {
            public byte bWidth;
            public byte bHeight;
            public byte bColorCount;
            public byte bReserved;
            public ushort wPlanes;
            public ushort wBitCount;
            public uint dwBytesInRes;
            public ushort nID;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x2)]
        private struct ICONDIR
        {
            public ushort idReserved;
            public ushort idType;
            public ushort idCount;
            public ICONDIRENTRY idEntries;
            internal ICONDIR(RESOURCE_ICONDIR resdir)
            {
                idReserved = resdir.idReserved;
                idType = resdir.idType;
                idCount = resdir.idCount;
                uint offset = (uint)Marshal.SizeOf(typeof(ICONDIR)) + (uint)((idCount - 1) * Marshal.SizeOf(typeof(ICONDIRENTRY)));
                idEntries = new ICONDIRENTRY(resdir.idEntries, offset);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ICONDIRENTRY
        {
            public byte bWidth;
            public byte bHeight;
            public byte bColorCount;
            public byte bReserved;
            public ushort wPlanes;
            public ushort wBitCount;
            public uint dwBytesInRes;
            public uint dwImageOffset;

            public ICONDIRENTRY(RESOURCE_ICONDIRENTRY resentry, uint offset)
            {
                bWidth = resentry.bWidth;
                bHeight = resentry.bHeight;
                bColorCount = resentry.bColorCount;
                bReserved = resentry.bReserved;
                wPlanes = resentry.wPlanes;
                wBitCount = resentry.wBitCount;
                dwBytesInRes = resentry.dwBytesInRes;
                dwImageOffset = offset;
            }
        }

        private sealed class SafeResourceBuffer : SafeBuffer
        {
            public SafeResourceBuffer()
                : base(false)
            { }

            public SafeResourceBuffer(IntPtr handle)
                : base(false)
            {
                base.SetHandle(handle);
            }

            // Resource buffers don't need to be freed and also can't be freed since Windows XP.
            protected override bool ReleaseHandle() { return true; }
        }

        [SecurityCritical]
        private sealed class SafeLocalAllocHandle : SafeBuffer
        {
            private SafeLocalAllocHandle()
                : base(true)
            { }

            internal SafeLocalAllocHandle(IntPtr handle)
                : base(true)
            {
                base.SetHandle(handle);
            }

            [SecurityCritical]
            protected override bool ReleaseHandle() { return (LocalFree(base.handle) == IntPtr.Zero); }

            internal static SafeLocalAllocHandle InvalidHandle { get { return new SafeLocalAllocHandle(IntPtr.Zero); } }
        }

        public const ushort IDI_APPLICATION = 32512;

        const int IMAGE_ICON = 1;
        const ushort RT_ICON = 3;
        const ushort RT_GROUP_ICON = RT_ICON + 11;
        const int LMEM_FIXED = 0x0;
        const int LMEM_ZEROINIT = 0x40;
        const int ERROR_RESOURCE_DATA_NOT_FOUND = 1812;
        const int ERROR_RESOURCE_TYPE_NOT_FOUND = 1813;
        const int ERROR_RESOURCE_NAME_NOT_FOUND = 1814;
        const int ERROR_RESOURCE_LANG_NOT_FOUND = 1815;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern SafeLocalAllocHandle LocalAlloc([In] int uFlags, [In] UIntPtr sizetdwBytes);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LocalFree(IntPtr handle);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string modName);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadIcon(HandleRef hInstance, UIntPtr lpIconName);
        [DllImport("user32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyIcon(IntPtr hIcon);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr FindResource(HandleRef hModule, UIntPtr lpName, UIntPtr lpType);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr LoadResource(HandleRef hModule, IntPtr hResInfo);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern uint SizeofResource(HandleRef hModule, IntPtr hResInfo);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        private static extern SafeResourceBuffer LockResource(IntPtr hResData);

        private static UIntPtr MAKEINTRESOURCE(ushort wInteger)
        {
            return (UIntPtr)wInteger;
        }

        private static SafeLocalAllocHandle SafeLocalAlloc(int uFlags, uint size)
        {
            SafeLocalAllocHandle result = LocalAlloc(uFlags, (UIntPtr)size);
            if (result == null || result.IsInvalid)
                throw new OutOfMemoryException();
            result.Initialize(size);
            return result;
        }

        private static unsafe SafeResourceBuffer GetResourceBuffer(HandleRef hModule, [In]string name, [In]string type)
        {
            fixed (char* pname = name)
            {
                fixed (char* ptype = type)
                {
                    return GetResourceBuffer(hModule, (UIntPtr)pname, (UIntPtr)ptype);
                }
            }
        }

        private static unsafe SafeResourceBuffer GetResourceBuffer(HandleRef hModule, [In]string name, UIntPtr lpType)
        {
            fixed (char* pname = name)
            {
                return GetResourceBuffer(hModule, (UIntPtr)pname, lpType);
            }
        }

        private static SafeResourceBuffer GetResourceBuffer(HandleRef hModule, UIntPtr lpName, UIntPtr lpType)
        {
            IntPtr hresinfo = FindResource(hModule, lpName, lpType);
            if (hresinfo == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                if (error >= ERROR_RESOURCE_DATA_NOT_FOUND && error <= ERROR_RESOURCE_LANG_NOT_FOUND)
                {
                    Win32Exception ex = new Win32Exception(error);
                    throw new KeyNotFoundException(ex.Message, ex);
                }
                throw new Win32Exception(error);
            }
            uint size = SizeofResource(hModule, hresinfo);
            if (size == 0)
                throw new Win32Exception();
            IntPtr hresdata = LoadResource(hModule, hresinfo);
            if (hresdata == IntPtr.Zero)
                throw new Win32Exception();

            SafeResourceBuffer result = LockResource(hresdata);
            if (result == null || result.IsInvalid)
                throw new OutOfMemoryException();
            result.Initialize(size);
            return result;
        }

        #endregion

        public static Icon LoadIconFromResource(ushort iconId)
        {
            return LoadIconFromResource(MAKEINTRESOURCE(iconId));
        }

        public static unsafe Icon LoadIconFromResource(string iconName)
        {
            fixed (char* pname = iconName)
            {
                return LoadIconFromResource((UIntPtr)pname);
            }
        }

        private static Icon LoadIconFromResource(UIntPtr lpIconName)
        {
            HandleRef hmodule = new HandleRef(null, GetModuleHandle(null));

            uint totalsize;
            RESOURCE_ICONDIRENTRY[] entries;
            SafeResourceBuffer[] entrybuffers;
            RESOURCE_ICONDIR dir;
            using (SafeResourceBuffer buffer = GetResourceBuffer(hmodule, lpIconName, MAKEINTRESOURCE(RT_GROUP_ICON)))
            {
                dir = buffer.Read<RESOURCE_ICONDIR>(0);
                if (dir.idReserved != 0 || dir.idType != IMAGE_ICON || dir.idCount == 0)
                    throw new InvalidDataException("There is no icon directory in resource data.");

                entries = new RESOURCE_ICONDIRENTRY[dir.idCount];
                entrybuffers = new SafeResourceBuffer[dir.idCount];
                uint entrysize = (uint)Marshal.SizeOf(typeof(RESOURCE_ICONDIRENTRY));
                ulong offset = (uint)Marshal.SizeOf(typeof(RESOURCE_ICONDIR)) - entrysize;
                totalsize = (uint)(Marshal.SizeOf(typeof(ICONDIR)) + (Marshal.SizeOf(typeof(ICONDIRENTRY)) * (dir.idCount - 1)));

                entries[0] = dir.idEntries;
                for (int i = 0; i < dir.idCount; i++, offset += entrysize)
                {
                    if (i > 0)
                        entries[i] = buffer.Read<RESOURCE_ICONDIRENTRY>(offset);

                    uint iconsize = entries[i].dwBytesInRes;
                    SafeResourceBuffer entrybuffer = GetResourceBuffer(hmodule, MAKEINTRESOURCE(entries[i].nID), MAKEINTRESOURCE(RT_ICON));
                    if (iconsize != entrybuffer.ByteLength)
                        throw new InvalidDataException("Reported resource size is not equal to the icon size.");

                    entrybuffers[i] = entrybuffer;
                    totalsize += iconsize;
                }
            }

            using (SafeLocalAllocHandle iconbuffer = SafeLocalAlloc(LMEM_ZEROINIT | LMEM_FIXED, totalsize))
            {
                using (UnmanagedMemoryStream outstream = iconbuffer.ToStream(FileAccess.ReadWrite))
                {
                    ICONDIR icondir = new ICONDIR(dir);
                    iconbuffer.Write(0, icondir);

                    uint iconoffset = icondir.idEntries.dwImageOffset;
                    outstream.Position = iconoffset;
                    uint entrysize = (uint)Marshal.SizeOf(typeof(ICONDIRENTRY));
                    ulong offset = (uint)Marshal.SizeOf(typeof(ICONDIR));
                    ICONDIRENTRY entry = icondir.idEntries;
                    for (int i = 0; i < dir.idCount; i++, offset += entrysize)
                    {
                        if (i > 0)
                        {
                            entry = new ICONDIRENTRY(entries[i], iconoffset);
                            iconbuffer.Write(offset, entry);
                        }

                        using (UnmanagedMemoryStream instream = entrybuffers[i].ToStream())
                        {
                            instream.CopyTo(outstream);
                        }

                        if (outstream.Position != (iconoffset + entry.dwBytesInRes))
                            throw new InvalidOperationException();

                        iconoffset += entry.dwBytesInRes;
                    }

                    outstream.Position = 0;
                    return new Icon(outstream);
                }
            }
        }
    }
}
