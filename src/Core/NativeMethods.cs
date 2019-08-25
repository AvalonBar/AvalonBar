using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;

namespace Sidebar.Core
{
    internal static class NativeMethods
    {
        // Shell32
        [DllImport("shell32.dll")]
        internal static extern int SHAppBarMessage(int dwMessage, ref AppBarData pData);
        [DllImport("shell32.dll")]
        internal static extern IntPtr ExtractIcon(IntPtr hInstance, string path, int iconIndex);
        // User32
        [DllImport("user32.dll")]
        internal static extern int RegisterWindowMessageW(string LPString);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
        [DllImport("user32.dll")]
        internal static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);
        [DllImport("user32.dll")]
        internal static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
        [DllImport("user32.dll")]
        internal static extern int SetForeGroundWindow(ref IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool EnumDisplayDevices(string lpDevice,
                                                     int iDevNum, [In, Out] DISPLAY_DEVICE lpDisplayDevice, int dwFlags);
        // Gdi32
        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);
        // Dwmapi
        [DllImport("dwmapi.dll")]
        internal static extern int DwmEnableBlurBehindWindow(IntPtr hWnd, ref BB_Struct BlurBehind);
        [DllImport("dwmapi.dll")]
        internal static extern void DwmIscompositionEnabled(ref bool result);
        [DllImport("dwmapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern void DwmGetColorizationColor(out int color, out bool opaque);
        [DllImport("dwmapi.dll", PreserveSig = false)]
        internal static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
    }

    internal struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public int showCmd;
        public Point ptMinPosition;
        public Point ptMaxPosition;
        public Rectangle rcNormalPosition;
    }

    internal struct RECT
    {
        public int Left, Top, Right, Bottom;
        public static RECT Empty
        {
            get { return default(RECT); }
        }
    }

    internal struct BB_Struct //Blur Behind Structure
    {
        public BB_Flags flags;
        public bool enable;
        public IntPtr region;
        public bool transitionOnMaximized;
    }

    internal enum BB_Flags : byte //Blur Behind Flags
    {
        DWM_BB_ENABLE = 1,
        DWM_BB_BLURREGION = 2,
        DWM_BB_TRANSITIONONMAXIMIZED = 4,
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal class DISPLAY_DEVICE
    {
        public int cb = 0;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName = new String(' ', 32);
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString = new String(' ', 128);
        public int StateFlags = 0;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID = new String(' ', 128);
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey = new String(' ', 128);
    }
}
