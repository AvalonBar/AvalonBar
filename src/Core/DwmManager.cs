using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;

namespace Sidebar.Core
{
    public class DwmManager
    {
        private const int WM_DWMCOMPOSITIONCHANGED = 0x0000031E;
        private const int WM_DWMCOLORIZATIONCOLORCHANGED = 0x0320;

        public static event EventHandler ColorizationColorChanged;

        public static bool IsGlassAvailable() //Check if it is not a Windows Vista or it is a Windows Vista Home Basic
        {
            if ((Environment.OSVersion.Version.Major < 6 || Environment.OSVersion.Version.Build < 5600) ||
                !File.Exists(Environment.SystemDirectory + @"\dwmapi.dll") ||
                (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 2))
            {
                return false;
            } else {
                return true;
            }
        }

        public static bool EnableGlass(ref IntPtr handle, IntPtr rgn) //Try to enable Aero Glass. If success return true
        {
            Region region = new Region();
            region.MakeInfinite();
            BB_Struct bb = new BB_Struct();
            bb.enable = true;
            bb.flags = BB_Flags.DWM_BB_ENABLE | BB_Flags.DWM_BB_BLURREGION;
            if (region != null)
                bb.region = rgn;
            else
                bb.region = IntPtr.Zero;
            if (NativeMethods.DwmEnableBlurBehindWindow(handle, ref bb) != 0)
                return false;
            else
                return true;
        }

        public static bool DisableGlass(ref IntPtr handle) //Try to disable Aero Glass. If success return true
        {
            Region region = new Region();
            Graphics graphics = Graphics.FromHwnd(handle);
            BB_Struct bb = new BB_Struct();
            bb.enable = false;
            bb.flags = BB_Flags.DWM_BB_ENABLE | BB_Flags.DWM_BB_BLURREGION;
            bb.region = IntPtr.Zero;
            if (NativeMethods.DwmEnableBlurBehindWindow(handle, ref bb) != 0)
                return false;
            else
                return true;
        }

        private const int ExcludedFromPeek = 12;
        private const int Flip3D = 8;

        public enum Flip3DPolicy
        {
            Default = 0,
            ExcludeBelow,
            ExcludeAbove
        }

        public static void RemoveFromAeroPeek(IntPtr hwnd)
        {
            if (IsGlassAvailable())
            {
                int attrValue = 1; // True
                NativeMethods.DwmSetWindowAttribute(hwnd, 12, ref attrValue, sizeof(int));
            }
        }

        public static void RemoveFromFlip3D(IntPtr hwnd)
        {
            if (IsGlassAvailable())
            {
                int attrValue = (int)Flip3DPolicy.ExcludeBelow; // True
                NativeMethods.DwmSetWindowAttribute(hwnd, Flip3D, ref attrValue, sizeof(int));
            }
        }

        public static void GetColorizationColor(out int color, out bool opaque)
        {
            NativeMethods.DwmGetColorizationColor(out color, out opaque);
        }

        internal static IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_DWMCOMPOSITIONCHANGED)
            {
                if (IsGlassAvailable())
                {
                    EnableGlass(ref hWnd, IntPtr.Zero);
                }
                else
                {
                    DisableGlass(ref hWnd);
                }
            }

            if (msg == WM_DWMCOLORIZATIONCOLORCHANGED)
            {
                if (ColorizationColorChanged != null)
                {
                    ColorizationColorChanged(null, EventArgs.Empty);
                }
            }

            return IntPtr.Zero;
        }
    }
}
