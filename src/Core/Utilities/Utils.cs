using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Slate.Utilities
{
    public class Utils
    {
        public static Process PriorProcess()
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

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DISPLAY_DEVICE
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

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayDevices(string lpDevice,
                                                     int iDevNum, [In, Out] DISPLAY_DEVICE lpDisplayDevice, int dwFlags);

        public static string[] GetScreenFriendlyNames()
        {
            List<string> screenNames = new List<string>();
            foreach (Screen screen in Screen.AllScreens)
            {
                int dwf = 0;
                DISPLAY_DEVICE info = new DISPLAY_DEVICE();
                string monitorname = null;
                info.cb = Marshal.SizeOf(info);
                if (EnumDisplayDevices(screen.DeviceName, 0, info, dwf))
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

        public static Screen GetScreenFromFriendlyName(string monitorname)
        {
            if (monitorname == "Primary")
                return Screen.PrimaryScreen;

            foreach (Screen screen in Screen.AllScreens)
            {
                int dwf = 0;
                DISPLAY_DEVICE info = new DISPLAY_DEVICE();
                info.cb = Marshal.SizeOf(info);
                if (EnumDisplayDevices(screen.DeviceName, 0, info, dwf))
                    if (info.DeviceString != null)
                        if (info.DeviceString == monitorname)
                            return screen;
            }
            return Screen.PrimaryScreen;
        }

        public static Screen GetScreenFromName(string monitorname)
        {
            if (monitorname == "Primary")
                return Screen.PrimaryScreen;

            foreach (Screen screen in Screen.AllScreens)
                if (screen.DeviceName == monitorname)
                    return screen;
            return Screen.PrimaryScreen;
        }

        internal static int CalculatePos(General.Sidebar.Side side)
        {
            int pos = 0;
            Screen[] screens = Screen.AllScreens;
            switch (side)
            {
                case General.Sidebar.Side.Left:
                    pos = SystemInformation.VirtualScreen.Left;
                    foreach (Screen scr in screens)
                        if (scr == General.Sidebar.screen)
                            break;
                        else
                            pos += scr.Bounds.Width;
                    break;

                case General.Sidebar.Side.Right:
                    pos = SystemInformation.VirtualScreen.Right;
                    for (int i = screens.Length - 1; i > 0; i--)
                        if (screens[i] == General.Sidebar.screen)
                            break;
                        else
                            pos -= screens[i].Bounds.Width;
                    break;
            }
            return pos;
        }
    }
}
