using System;
using System.Runtime.InteropServices;

namespace Sidebar
{
    internal static class NativeMethods
    {
        // User32
        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessageW(IntPtr hWnd, uint msg, uint wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        internal static extern int FindWindowW(string className, string windowName);
        [DllImport("user32.dll")]
        internal static extern int SetWindowLong(IntPtr hWnd, GetWindowLongMessage nIndex, int dwNewLong);
        // Gdi32
        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect,
           int nBottomRect);
    }

    internal enum GetWindowLongMessage
    {
        GWL_EXSTYLE = -20,
        GWL_HINSTANCE = -6,
        GWL_HWNDPARENT = -8,
        GWL_ID = -12,
        GWL_STYLE = -16,
        GWL_USERDATA = -21,
        GWL_WNDPROC = -4
    }
}
