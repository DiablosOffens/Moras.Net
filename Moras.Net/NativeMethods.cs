using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Moras.Net
{
    internal static class NativeMethods
    {
        internal const int EM_LINESCROLL = 182;
        internal const int WM_SETREDRAW = 11;
        internal const int RDW_INVALIDATE = 1,
            RDW_ERASE = 4,
            RDW_ALLCHILDREN = 128,
            RDW_FRAME = 1024;
        internal static HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, int lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, uint lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern bool RedrawWindow(HandleRef hwnd, IntPtr rcUpdate, HandleRef hrgnUpdate, int flags);
    }
}
