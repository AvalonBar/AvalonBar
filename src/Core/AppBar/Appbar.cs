using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Interop;

namespace Sidebar.Core
{
    public class AppBar
    {
        internal static Screen screen = Screen.PrimaryScreen;
        private static string screenName;

        internal static IntPtr Handle;
        private static Window window;
        private static int appBarMessage;
        internal static AppBarSide LongBarSide;
        internal static bool AlwaysTop;

        #region Taskbar Overlapping

        private static DispatcherTimer timer;

        #endregion

        private static IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == appBarMessage && wParam.ToInt32() == (int)AppBarNotifications.PosChanged)
            {
                SizeAppbar();
                return new IntPtr(msg);
            }

            if (msg == 26 && wParam.ToInt32() == 47 && !AlwaysTop)
            {
                SetPos();
                return new IntPtr(msg);
            }

            return IntPtr.Zero;
        }

        private static int RegisterCallBackMessage()
        {
            string uniqueMessageString = Guid.NewGuid().ToString();
            return NativeMethods.RegisterWindowMessageW(uniqueMessageString);
        }

        internal static bool AppbarNew()
        {
            AppBarData messageData = DefaultMessage;
            messageData.uCallBackMessage = NativeMethods.RegisterWindowMessageW("LongBarMessage");
            appBarMessage = messageData.uCallBackMessage;
            int value = NativeMethods.SHAppBarMessage((int)AppBarMessages.NewAppBar, ref messageData);
            return (value != 0);
        }

        public static bool AppbarRemove()
        {
            AppBarData messageData = DefaultMessage;
            int value = NativeMethods.SHAppBarMessage((int)AppBarMessages.Remove, ref messageData);
            return (value != 0);
        }

        private static void AppbarQueryPos(ref RECT rect)
        {
            AppBarData messageData = DefaultMessage;
            messageData.uEdge = (int)LongBarSide;
            messageData.rc = rect;
            NativeMethods.SHAppBarMessage((int)AppBarMessages.QueryPos, ref messageData);
        }

        private static void AppbarSetPos(ref RECT rect)
        {
            AppBarData messageData = DefaultMessage;
            messageData.uEdge = (int)LongBarSide;
            messageData.rc = rect;
            NativeMethods.SHAppBarMessage((int)AppBarMessages.SetPos, ref messageData);
            rect = messageData.rc;
        }

        private static AppBarData DefaultMessage
        {
            get
            {
                AppBarData messageData = new AppBarData();
                messageData.cbSize = Marshal.SizeOf(messageData);
                messageData.hWnd = Handle.ToInt32();
                return messageData;
            }
        }

        public static void SizeAppbar()
        {
            screen = Utils.GetScreenFromName(screenName);
            RECT rt = new RECT();
            if (LongBarSide == AppBarSide.Left || LongBarSide == AppBarSide.Right)
            {
                rt.Top = 0;
                rt.Bottom = screen.Bounds.Size.Height;
                if (LongBarSide == AppBarSide.Left)
                {
                    if (screen != Screen.PrimaryScreen)
                        rt.Left = Utils.CalculatePos(LongBarSide);
                    rt.Right = (int)window.Width;
                }
                else
                {
                    if (screen != Screen.PrimaryScreen)
                        rt.Right = Utils.CalculatePos(LongBarSide);
                    else
                        rt.Right = SystemInformation.PrimaryMonitorSize.Width;
                    rt.Left = rt.Right - (int)window.Width;
                }
            }
            AppbarQueryPos(ref rt);
            switch (LongBarSide)
            {
                case AppBarSide.Left:
                    rt.Right = rt.Left + (int)window.Width;
                    break;
                case AppBarSide.Right:
                    rt.Left = rt.Right - (int)window.Width;
                    break;
            }
            AppbarSetPos(ref rt);
            window.Left = rt.Left;
            window.Top = rt.Top;
            window.Width = rt.Right - rt.Left;
            if (!Overlapped)
                window.Height = rt.Bottom - rt.Top;
            else
                window.Height = screen.Bounds.Size.Height;
        }

        public static void SetPos()
        {
            if (LongBarSide == AppBarSide.Right)
            {
                window.Left = screen.WorkingArea.Right - window.Width;
                window.Top = screen.WorkingArea.Top;
                window.Height = screen.WorkingArea.Height;
            }
            else
            {
                window.Left = screen.WorkingArea.Left;
                window.Top = screen.WorkingArea.Top;
                window.Height = screen.WorkingArea.Height;
            }
        }

        public static bool Overlapped;
        private static int trayWndWidth;
        private static int trayWndLeft;

        public static void SetSidebar(Window wnd, AppBarSide side, bool topMost, bool overlapTaskBar, string scrnName)
        {
            window = wnd;
            Handle = new WindowInteropHelper(wnd).Handle;
            LongBarSide = side;
            screenName = scrnName;
            screen = Utils.GetScreenFromName(scrnName);
            if (topMost)
            {
                AlwaysTop = topMost;
                AppbarNew();
                if (!Overlapped && overlapTaskBar && side == AppBarSide.Right)
                {
                    OverlapTaskbar();
                }
                SizeAppbar();
                wnd.Topmost = true;
            }
            else
            {
                AlwaysTop = topMost;
                wnd.Topmost = false;
                if (Overlapped && side == AppBarSide.Right)
                {
                    UnOverlapTaskbar();
                }
                SetPos();
            }
            HwndSource.FromHwnd(Handle).AddHook(new HwndSourceHook(WndProc));
            HwndSource.FromHwnd(Handle).AddHook(new HwndSourceHook(DwmManager.WndProc));
        }

        public static void ResizeBar()
        {
            bool visible = SystemTray.SidebarVisible;
            SystemTray.SidebarVisible = false;
            SystemTray.SidebarVisible = true;
            SystemTray.SidebarVisible = visible;
        }

        public static void OverlapTaskbar()
        {
            if (screen != Screen.PrimaryScreen)
                return;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
            timer_Tick(null, null);
        }

        static void timer_Tick(object sender, EventArgs e)
        {
            IntPtr taskbarHwnd = NativeMethods.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null);
            IntPtr trayHwnd = NativeMethods.FindWindowEx(taskbarHwnd, IntPtr.Zero, "TrayNotifyWnd", null);
            IntPtr rebarHwnd = NativeMethods.FindWindowEx(taskbarHwnd, IntPtr.Zero, "RebarWindow32", null);

            WINDOWPLACEMENT lpwndpl = new WINDOWPLACEMENT();
            lpwndpl.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
            NativeMethods.GetWindowPlacement(taskbarHwnd, ref lpwndpl);
            //Check if taskbar at top or bottom and it isn't cropped
            if (lpwndpl.rcNormalPosition.Top != 0
                && lpwndpl.rcNormalPosition.Width == SystemInformation.PrimaryMonitorSize.Width)
            {
                //first, hide tray by setting it's width to 0
                NativeMethods.GetWindowPlacement(trayHwnd, ref lpwndpl);
                trayWndWidth = lpwndpl.rcNormalPosition.Width; //save original width of tray
                trayWndLeft = lpwndpl.rcNormalPosition.X; //save original left pos of tray
                NativeMethods.MoveWindow(trayHwnd, lpwndpl.rcNormalPosition.X, lpwndpl.rcNormalPosition.Y, 0, lpwndpl.rcNormalPosition.Height, true);

                NativeMethods.GetWindowPlacement(rebarHwnd, ref lpwndpl);
                NativeMethods.MoveWindow(rebarHwnd, lpwndpl.rcNormalPosition.X, lpwndpl.rcNormalPosition.Y, SystemInformation.PrimaryMonitorSize.Width - (int)window.Width - lpwndpl.rcNormalPosition.X, lpwndpl.rcNormalPosition.Height, true);

                //second, cut taskbar window
                NativeMethods.GetWindowPlacement(taskbarHwnd, ref lpwndpl);
                IntPtr rgn = NativeMethods.CreateRectRgn(0, 0, SystemInformation.PrimaryMonitorSize.Width - (int)window.Width, lpwndpl.rcNormalPosition.Height);
                NativeMethods.SetWindowRgn(taskbarHwnd, rgn, true);
                Overlapped = true;
            }
        }

        public static void UnOverlapTaskbar()
        {
            if (screen != Screen.PrimaryScreen)
                return;

            timer.Stop();

            IntPtr taskbarHwnd = NativeMethods.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null);
            IntPtr trayHwnd = NativeMethods.FindWindowEx(taskbarHwnd, IntPtr.Zero, "TrayNotifyWnd", null);

            WINDOWPLACEMENT lpwndpl = new WINDOWPLACEMENT();
            lpwndpl.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
            NativeMethods.GetWindowPlacement(taskbarHwnd, ref lpwndpl);
            //Check if taskbar at top or bottom and it is cropped
            if (lpwndpl.rcNormalPosition.Top != 0
                && lpwndpl.rcNormalPosition.Width == SystemInformation.PrimaryMonitorSize.Width)
            {
                //first, return tray by setting it's width back
                NativeMethods.GetWindowPlacement(trayHwnd, ref lpwndpl);
                lpwndpl.rcNormalPosition.Width = trayWndWidth; //restore original width of tray
                lpwndpl.rcNormalPosition.X = trayWndLeft; //restore original left pos of tray
                NativeMethods.MoveWindow(trayHwnd, lpwndpl.rcNormalPosition.X, lpwndpl.rcNormalPosition.Y, trayWndWidth, lpwndpl.rcNormalPosition.Height, true);

                //second, extend taskbar window
                NativeMethods.GetWindowPlacement(taskbarHwnd, ref lpwndpl);
                IntPtr rgn = NativeMethods.CreateRectRgn(0, 0, SystemInformation.PrimaryMonitorSize.Width, lpwndpl.rcNormalPosition.Height);
                NativeMethods.SetWindowRgn(taskbarHwnd, rgn, true);

                Overlapped = false;
            }
        }

    }
}
