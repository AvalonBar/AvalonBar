﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Media;
using System.Reflection;

namespace Sidebar
{
    public enum CompositionMethod
    {
        None,
        Aero,
        AcrylicLegacy,
        Acrylic,
    }

    public class CompositionHelper
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
                    return CompositionMethod.AcrylicLegacy;
                }
                // Windows 10 (build 17134 and above)
                if (OSVersion.Major == 10 && OSVersion.Build >= 17134)
                {
                    return CompositionMethod.Acrylic;
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
                case CompositionMethod.AcrylicLegacy:
                case CompositionMethod.Acrylic:
                    AccentPolicy accent = new AccentPolicy();
                    accent.AccentState = AccentState.ACCENT_DISABLED;

                    if (enabled)
                    {
                        accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;
                        if (AvailableCompositionMethod == CompositionMethod.Acrylic)
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

        public static void ExcludeFromTaskView(IntPtr handle)
        {
            int attributeValue = (int)Flip3DPolicy.ExcludeBelow;
            NativeMethods.DwmSetWindowAttribute(
                handle, DwmWindowAttribute.Flip3DPolicy, ref attributeValue, sizeof(uint));
        }

        public static Color ColorizationColor
        {
            get
            {
                NativeMethods.DwmGetColorizationColor(out uint color, out _);
                // WPF's Color structure has an internal method that creates a
                // new instance from an unsigned 32-bit value.
                MethodInfo ColorFromUInt32 = typeof(Color).GetMethod(
                    "FromUInt32",
                    BindingFlags.NonPublic | BindingFlags.Static);
                return (Color)ColorFromUInt32.Invoke(null, new object[] { color });
            }
        }

        internal static IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // WM_DWMCOMPOSITIONCHANGED
            if (msg == 0x0000031E)
            {
                bool blurEnabled = AvailableCompositionMethod != CompositionMethod.None;
                SetBlurBehindWindow(ref hWnd, blurEnabled);
            }

            // WM_DWMCOLORIZATIONCOLORCHANGED
            if (msg == 0x0320)
            {
                ColorizationColorChanged?.Invoke(null, EventArgs.Empty);
            }

            return IntPtr.Zero;
        }
    }
}
