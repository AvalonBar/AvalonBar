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
        // Shell32
        [DllImport("shell32.dll")]
        internal static extern IntPtr ShellExecute(
            IntPtr hwnd,
            string lpOperation,
            string lpFile,
            string lpParameters,
            string lpDirectory,
            int nShowCmd);
        // Gdi32
        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect,
           int nBottomRect);
    }
}
