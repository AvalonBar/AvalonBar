using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Sidebar
{
    internal static class Utils
    {
        internal static Process PriorProcess()
        // Returns a System.Diagnostics.Process pointing to
        // a pre-existing process with the same name as the
        // current one, if any; or null if the current process
        // is unique.
        {
            Process curr = Process.GetCurrentProcess();
            Process[] procs = Process.GetProcessesByName(curr.ProcessName);
            foreach (Process p in procs)
            {
                if ((p.Id != curr.Id) &&
                    (p.MainModule.FileName == curr.MainModule.FileName))
                    return p;
            }
            return null;
        }

        internal static string[] GetScreenFriendlyNames()
        {
            List<string> screenNames = new List<string>();
            foreach (Screen screen in Screen.AllScreens)
            {
                int dwf = 0;
                DISPLAY_DEVICE info = new DISPLAY_DEVICE();
                string monitorname = null;
                info.cb = Marshal.SizeOf(info);
                if (NativeMethods.EnumDisplayDevices(screen.DeviceName, 0, info, dwf))
                {
                    monitorname = info.DeviceString;
                }
                if (monitorname == null)
                {
                    monitorname = "";
                }
                screenNames.Add(monitorname);
            }
            return screenNames.ToArray();
        }

        internal static Screen GetScreenFromFriendlyName(string monitorname)
        {
            if (monitorname == "Primary")
                return Screen.PrimaryScreen;

            foreach (Screen screen in Screen.AllScreens)
            {
                int dwf = 0;
                DISPLAY_DEVICE info = new DISPLAY_DEVICE();
                info.cb = Marshal.SizeOf(info);
                if (NativeMethods.EnumDisplayDevices(screen.DeviceName, 0, info, dwf))
                    if (info.DeviceString != null)
                        if (info.DeviceString == monitorname)
                            return screen;
            }
            return Screen.PrimaryScreen;
        }

        internal static Screen GetScreenFromName(string monitorname)
        {
            if (monitorname == "Primary")
                return Screen.PrimaryScreen;

            foreach (Screen screen in Screen.AllScreens)
                if (screen.DeviceName == monitorname)
                    return screen;
            return Screen.PrimaryScreen;
        }

        internal static int CalculatePos(AppBarSide side)
        {
            int pos = 0;
            Screen[] screens = Screen.AllScreens;
            switch (side)
            {
                case AppBarSide.Left:
                    pos = SystemInformation.VirtualScreen.Left;
                    foreach (Screen scr in screens)
                        if (scr == AppBar.Screen)
                            break;
                        else
                            pos += scr.Bounds.Width;
                    break;

                case AppBarSide.Right:
                    pos = SystemInformation.VirtualScreen.Right;
                    for (int i = screens.Length - 1; i > 0; i--)
                        if (screens[i] == AppBar.Screen)
                            break;
                        else
                            pos -= screens[i].Bounds.Width;
                    break;
            }
            return pos;
        }

        internal static string FindString(object resourceKey)
        {
            string resource = System.Windows.Application.Current.TryFindResource(resourceKey) as string;
#if DEBUG
            if (resource == null)
            {
                throw new KeyNotFoundException();
            }
#endif
            return resource;
        }
    }
}
