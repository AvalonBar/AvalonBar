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
        private const string MessageGuid = "{5C72002F-E216-4AE1-ABA1-7BCE5B1A2440}";
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

        internal static bool AppbarNew()
        {
            AppBarData messageData = DefaultMessage;
            messageData.uCallBackMessage = NativeMethods.RegisterWindowMessageW(MessageGuid);
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
            if (IsOverlapping)
                MainWindow.Height = Screen.Bounds.Size.Height;
            else
                MainWindow.Height = rt.Bottom - rt.Top;
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
                    RestoreTaskbar();
                }
                SetPos();
            }

            if (!WindowHooksAdded)
            {
                HwndSource.FromHwnd(Handle).AddHook(new HwndSourceHook(WndProc));
                HwndSource.FromHwnd(Handle).AddHook(new HwndSourceHook(CompositionManager.WndProc));
                WindowHooksAdded = true;
            }
        }

        public static void ResizeBar()
        {
            bool visible = SystemTray.IsSidebarVisible;
            SystemTray.IsSidebarVisible = false;
            SystemTray.IsSidebarVisible = true;
            SystemTray.IsSidebarVisible = visible;
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
            IntPtr TaskbarHandle = NativeMethods.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null);
            IntPtr TrayHandle = NativeMethods.FindWindowEx(TaskbarHandle, IntPtr.Zero, "TrayNotifyWnd", null);
            IntPtr RebarHandle = NativeMethods.FindWindowEx(TaskbarHandle, IntPtr.Zero, "RebarWindow32", null);

            WINDOWPLACEMENT WindowPlacement = new WINDOWPLACEMENT();
            WindowPlacement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
            NativeMethods.GetWindowPlacement(TaskbarHandle, ref WindowPlacement);

            // Check if the taskbar is placed at the top/bottom and isn't cropped
            if (WindowPlacement.rcNormalPosition.Top != 0 &&
                WindowPlacement.rcNormalPosition.Width == SystemInformation.PrimaryMonitorSize.Width)
            {
                // Hide system tray by zeroing its width
                NativeMethods.GetWindowPlacement(TrayHandle, ref WindowPlacement);
                trayWndWidth = WindowPlacement.rcNormalPosition.Width;
                trayWndLeft = WindowPlacement.rcNormalPosition.X;
                NativeMethods.MoveWindow(TrayHandle, WindowPlacement.rcNormalPosition.X,
                    WindowPlacement.rcNormalPosition.Y, 0, WindowPlacement.rcNormalPosition.Height, true);
                // Reduce width of running applications bar (unsure if this is necessary)
                NativeMethods.GetWindowPlacement(RebarHandle, ref WindowPlacement);
                NativeMethods.MoveWindow(RebarHandle, WindowPlacement.rcNormalPosition.X,
                    WindowPlacement.rcNormalPosition.Y,
                    SystemInformation.PrimaryMonitorSize.Width - (int)MainWindow.Width -
                    WindowPlacement.rcNormalPosition.X, WindowPlacement.rcNormalPosition.Height, true);
                // Cut the taskbar window
                NativeMethods.GetWindowPlacement(TaskbarHandle, ref WindowPlacement);
                IntPtr rgn = NativeMethods.CreateRectRgn(0, 0,
                    SystemInformation.PrimaryMonitorSize.Width - (int)MainWindow.Width,
                    WindowPlacement.rcNormalPosition.Height);
                NativeMethods.SetWindowRgn(TaskbarHandle, rgn, true);
                IsOverlapping = true;
            }
        }

        public static void RestoreTaskbar()
        {
            if (Screen != Screen.PrimaryScreen)
                return;

            OverlapTimer.Stop();

            IntPtr TaskbarHandle = NativeMethods.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null);
            IntPtr TrayHandle = NativeMethods.FindWindowEx(TaskbarHandle, IntPtr.Zero, "TrayNotifyWnd", null);

            WINDOWPLACEMENT WindowPlacement = new WINDOWPLACEMENT();
            WindowPlacement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
            NativeMethods.GetWindowPlacement(TaskbarHandle, ref WindowPlacement);

            // Check if the taskbar is placed at the top/bottom and cropped
            if (WindowPlacement.rcNormalPosition.Top != 0 &&
                WindowPlacement.rcNormalPosition.Width == SystemInformation.PrimaryMonitorSize.Width)
            {
                // Restore original tray width and X position
                NativeMethods.GetWindowPlacement(TrayHandle, ref WindowPlacement);
                WindowPlacement.rcNormalPosition.Width = trayWndWidth;
                WindowPlacement.rcNormalPosition.X = trayWndLeft;
                NativeMethods.MoveWindow(TrayHandle, WindowPlacement.rcNormalPosition.X,
                    WindowPlacement.rcNormalPosition.Y, trayWndWidth, WindowPlacement.rcNormalPosition.Height, true);
                // Extend the taskbar back to its original dimensions
                NativeMethods.GetWindowPlacement(TaskbarHandle, ref WindowPlacement);
                IntPtr rgn = NativeMethods.CreateRectRgn(0, 0,
                    SystemInformation.PrimaryMonitorSize.Width, WindowPlacement.rcNormalPosition.Height);
                NativeMethods.SetWindowRgn(TaskbarHandle, rgn, true);

                IsOverlapping = false;
            }
        }
    }
}
