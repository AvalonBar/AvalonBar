using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;

namespace Slate.DWM
{
    public class DwmManager
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmEnableBlurBehindWindow(IntPtr hWnd, ref BB_Struct BlurBehind);
        [DllImport("dwmapi.dll")]
        private static extern void DwmIsCompositionEnabled(ref bool result);
        [DllImport("dwmapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void DwmGetColorizationColor(out int color, out bool opaque);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private struct BB_Struct //Blur Behind Structure
        {
            public BB_Flags flags;
            public bool enable;
            public IntPtr region;
            public bool transitionOnMaximized;
        }

        private enum BB_Flags : byte //Blur Behind Flags
        {
            DWM_BB_ENABLE = 1,
            DWM_BB_BLURREGION = 2,
            DWM_BB_TRANSITIONONMAXIMIZED = 4,
        };

        public static bool IsGlassAvailable() // Check if it is not a Windows Vista or it is a Windows Vista Home Basic
        {
            if (Environment.OSVersion.Version.Major < 6 || Environment.OSVersion.Version.Build < 5600 || !File.Exists(Environment.SystemDirectory + @"\dwmapi.dll")) {
                return false;
        	}
        	// If Windows 8 or higher is detected, return that glass isn't available
        	// since some changes to DWM that are implemented starting with Windows 8 aren't
        	// implemented yet.
        	else if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 2) {
                return false;
	    	} else {
				return true;
	    	}
        }

        public static bool EnableGlass(ref IntPtr handle, IntPtr rgn) // Try to enable Aero Glass. If success return true
        {
            Region region = new Region();
            region.MakeInfinite();
            BB_Struct bb = new BB_Struct();
            bb.enable = true;
            bb.flags = BB_Flags.DWM_BB_ENABLE | BB_Flags.DWM_BB_BLURREGION;
            if (region != null)
                bb.region = rgn;
            else
                bb.region = IntPtr.Zero; //Region.GetHrgn(Graphics)
            if (DwmEnableBlurBehindWindow(handle, ref bb) != 0)
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
            if (DwmEnableBlurBehindWindow(handle, ref bb) != 0)
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
                DwmSetWindowAttribute(hwnd, 12, ref attrValue, sizeof(int));
            }
        }

        public static void RemoveFromFlip3D(IntPtr hwnd)
        {
            if (IsGlassAvailable())
            {
                int attrValue = (int)Flip3DPolicy.ExcludeBelow; // True
                DwmSetWindowAttribute(hwnd, Flip3D, ref attrValue, sizeof(int));
            }
        }
    }
}
