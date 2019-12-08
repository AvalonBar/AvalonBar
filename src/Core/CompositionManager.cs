using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;

namespace Sidebar.Core
{
    public enum CompositionMethod
    {
        None,
        Aero,
        AcrylicOld,
        AcrylicRS4,
    }

    public class CompositionManager
    {
        public static event EventHandler ColorizationColorChanged;

        public static CompositionMethod AvailableCompositionMethod
        {
            get
            {
                Version OSVersion = Environment.OSVersion.Version;
                // Windows Vista/7
                if (OSVersion.Major == 6 && OSVersion.Minor <= 1)
                {
                    return CompositionMethod.Aero;
                }
                // Windows 10 (build 10074 and above)
                if (OSVersion.Major == 10 && OSVersion.Build >= 10074 && OSVersion.Build < 17134)
                {
                    return CompositionMethod.AcrylicOld;
                }
                // Windows 10 (build 17134 and above)
                if (OSVersion.Major == 10 && OSVersion.Build >= 17134)
                {
                    return CompositionMethod.AcrylicRS4;
                }
                return CompositionMethod.None;
            }
        }

        public static bool SetBlurBehindWindow(ref IntPtr handle, bool enabled)
        {
            switch (AvailableCompositionMethod)
            {
                case CompositionMethod.Aero:
                    BlurBehind bb = new BlurBehind()
                    {
                        Enabled = enabled,
                        Flags = BlurBehindFlags.DWM_BB_ENABLE | BlurBehindFlags.DWM_BB_BLURREGION,
                        Region = IntPtr.Zero
                    };

                    if (NativeMethods.DwmEnableBlurBehindWindow(handle, ref bb) != 0)
                    {
                        return false;
                    }
                    break;
                case CompositionMethod.AcrylicOld:
                case CompositionMethod.AcrylicRS4:
                    AccentPolicy accent = new AccentPolicy();
                    accent.AccentState = AccentState.ACCENT_DISABLED;

                    if (enabled)
                    {
                        accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;
                        if (AvailableCompositionMethod == CompositionMethod.AcrylicRS4)
                        {
                            accent.AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND;
                            accent.GradientColor = (0 << 24) | (0xFFFFFF);
                        }
                    }

                    int accentStructSize = Marshal.SizeOf(accent);
                    IntPtr accentPtr = Marshal.AllocHGlobal(accentStructSize);
                    Marshal.StructureToPtr(accent, accentPtr, false);

                    WindowCompositionAttributeData data = new WindowCompositionAttributeData();
                    data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
                    data.SizeOfData = accentStructSize;
                    data.Data = accentPtr;

                    NativeMethods.SetWindowCompositionAttribute(handle, ref data);

                    Marshal.FreeHGlobal(accentPtr);
                    break;
                case CompositionMethod.None:
                default:
                    break;
            }

            return true;
        }

        public static void ExcludeFromPeek(IntPtr handle)
        {
            int attributeValue = 1;
            NativeMethods.DwmSetWindowAttribute(
                handle, DwmWindowAttribute.ExcludedFromPeek, ref attributeValue, sizeof(uint));
        }

        public static void ExcludeFromFlip3D(IntPtr handle)
        {
            int attributeValue = (int)Flip3DPolicy.ExcludeBelow;
            NativeMethods.DwmSetWindowAttribute(
                handle, DwmWindowAttribute.Flip3DPolicy, ref attributeValue, sizeof(uint));
        }

        public static System.Windows.Media.Color ColorizationColor
        {
            get
            {
                int color;
                bool opaque;
                NativeMethods.DwmGetColorizationColor(out color, out opaque);
                Color DrawingColor = Color.FromArgb(color);
                return System.Windows.Media.Color.FromArgb(
                    DrawingColor.A, DrawingColor.R, DrawingColor.G, DrawingColor.B);
            }
        }

        internal static IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // WM_DWMCOMPOSITIONCHANGED
            if (msg == 0x0000031E)
            {
                if (AvailableCompositionMethod != CompositionMethod.None)
                {
                    SetBlurBehindWindow(ref hWnd, true);
                }
                else
                {
                    SetBlurBehindWindow(ref hWnd, false);
                }
            }

            // WM_DWMCOLORIZATIONCOLORCHANGED
            if (msg == 0x0320)
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
