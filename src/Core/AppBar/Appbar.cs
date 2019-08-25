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
        internal static Screen Screen = Screen.PrimaryScreen;
        private static string ScreenName;

        internal static IntPtr Handle;
        private static Window MainWindow;
        private static int MessageId;
        internal static AppBarSide Side;
        internal static bool AlwaysTop;

        private static DispatcherTimer OverlapTimer;

        private static IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == MessageId && wParam.ToInt32() == (int)AppBarNotifications.PosChanged)
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
            MessageId = messageData.uCallBackMessage;
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
            messageData.uEdge = (int)Side;
            messageData.rc = rect;
            NativeMethods.SHAppBarMessage((int)AppBarMessages.QueryPos, ref messageData);
        }

        private static void AppbarSetPos(ref RECT rect)
        {
            AppBarData messageData = DefaultMessage;
            messageData.uEdge = (int)Side;
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
            Screen = Utils.GetScreenFromName(ScreenName);
            RECT rt = new RECT();
            if (Side == AppBarSide.Left || Side == AppBarSide.Right)
            {
                rt.Top = 0;
                rt.Bottom = Screen.Bounds.Size.Height;
                if (Side == AppBarSide.Left)
                {
                    if (Screen != Screen.PrimaryScreen)
                        rt.Left = Utils.CalculatePos(Side);
                    rt.Right = (int)MainWindow.Width;
                }
                else
                {
                    if (Screen != Screen.PrimaryScreen)
                        rt.Right = Utils.CalculatePos(Side);
                    else
                        rt.Right = SystemInformation.PrimaryMonitorSize.Width;
                    rt.Left = rt.Right - (int)MainWindow.Width;
                }
            }
            AppbarQueryPos(ref rt);
            switch (Side)
            {
                case AppBarSide.Left:
                    rt.Right = rt.Left + (int)MainWindow.Width;
                    break;
                case AppBarSide.Right:
                    rt.Left = rt.Right - (int)MainWindow.Width;
                    break;
            }
            AppbarSetPos(ref rt);
            MainWindow.Left = rt.Left;
            MainWindow.Top = rt.Top;
            MainWindow.Width = rt.Right - rt.Left;
            if (!IsOverlapping)
                MainWindow.Height = rt.Bottom - rt.Top;
            else
                MainWindow.Height = Screen.Bounds.Size.Height;
        }

        public static void SetPos()
        {
            if (Side == AppBarSide.Right)
            {
                MainWindow.Left = Screen.WorkingArea.Right - MainWindow.Width;
                MainWindow.Top = Screen.WorkingArea.Top;
                MainWindow.Height = Screen.WorkingArea.Height;
            }
            else
            {
                MainWindow.Left = Screen.WorkingArea.Left;
                MainWindow.Top = Screen.WorkingArea.Top;
                MainWindow.Height = Screen.WorkingArea.Height;
            }
        }

        public static bool IsOverlapping;
        private static int trayWndWidth;
        private static int trayWndLeft;
        private static bool WindowHooksAdded;

        public static void SetSidebar(Window wnd, AppBarSide side, bool topMost, bool overlapTaskBar, string scrnName)
        {
            MainWindow = wnd;
            Handle = new WindowInteropHelper(wnd).Handle;
            Side = side;
            ScreenName = scrnName;
            Screen = Utils.GetScreenFromName(scrnName);
            if (topMost)
            {
                AlwaysTop = topMost;
                AppbarNew();
                if (!IsOverlapping && overlapTaskBar && side == AppBarSide.Right)
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
                if (IsOverlapping && side == AppBarSide.Right)
                {
                    UnOverlapTaskbar();
                }
                SetPos();
            }

            if (!WindowHooksAdded)
            {
                HwndSource.FromHwnd(Handle).AddHook(new HwndSourceHook(WndProc));
                HwndSource.FromHwnd(Handle).AddHook(new HwndSourceHook(DwmManager.WndProc));
                WindowHooksAdded = true;
            }
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
            if (Screen != Screen.PrimaryScreen)
                return;

            OverlapTimer = new DispatcherTimer();
            OverlapTimer.Interval = TimeSpan.FromMilliseconds(500);
            OverlapTimer.Tick += new EventHandler(timer_Tick);
            OverlapTimer.Start();
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
                NativeMethods.MoveWindow(rebarHwnd, lpwndpl.rcNormalPosition.X, lpwndpl.rcNormalPosition.Y, SystemInformation.PrimaryMonitorSize.Width - (int)MainWindow.Width - lpwndpl.rcNormalPosition.X, lpwndpl.rcNormalPosition.Height, true);

                //second, cut taskbar window
                NativeMethods.GetWindowPlacement(taskbarHwnd, ref lpwndpl);
                IntPtr rgn = NativeMethods.CreateRectRgn(0, 0, SystemInformation.PrimaryMonitorSize.Width - (int)MainWindow.Width, lpwndpl.rcNormalPosition.Height);
                NativeMethods.SetWindowRgn(taskbarHwnd, rgn, true);
                IsOverlapping = true;
            }
        }

        public static void UnOverlapTaskbar()
        {
            if (Screen != Screen.PrimaryScreen)
                return;

            OverlapTimer.Stop();

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

                IsOverlapping = false;
            }
        }

    }
}
