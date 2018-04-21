using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Interop;

namespace Slate.General
{
	public class Sidebar
	{
		[DllImport("shell32.dll")]
		private static extern int SHAppBarMessage(int dwMessage, ref APPBARDATA pData);
		[DllImport("user32.dll")]
		private static extern int RegisterWindowMessageW(string LPString);

		private const int WM_DWMCOMPOSITIONCHANGED = 0x0000031E;
		private const int WM_DWMCOLORIZATIONCOLORCHANGED = 0x0320;

		public static event EventHandler DwmColorChanged;

		internal static Screen screen = Screen.PrimaryScreen;
		private static string screenName;

		internal static IntPtr Handle;
		private static Window window;
		private static int appBarMessage;
		internal static Side LongBarSide;
		internal static bool AlwaysTop;

		#region Taskbar Overlapping

		private static DispatcherTimer timer;

		private const int WM_NCPAINT = 0x85;

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

		[DllImport("gdi32.dll")]
		internal static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

		internal struct WINDOWPLACEMENT
		{
			public int length;
			public int flags;
			public int showCmd;
			public System.Drawing.Point ptMinPosition;
			public System.Drawing.Point ptMaxPosition;
			public System.Drawing.Rectangle rcNormalPosition;
		}

		#endregion

		private struct APPBARDATA
		{
			internal int cbSize;
			internal int hWnd;
			internal int uCallBackMessage;
			internal int uEdge;
			internal Rect rc;
			internal int lParam;
		}

		public enum Side
		{
			Left = 0,
			Top = 1,
			Right = 2,
			Bottom = 3
		};

		private enum AppBarMessages
		{
			NewAppBar = 0,
			Remove = 1,
			QueryPos = 2,
			SetPos = 3,
			GetState = 4,
			GetTaskBarPos = 5,
			Activate = 6,
			GetAutoHideBar = 7,
			SetAutoHideBar = 8,
			WindowPosChanged = 9,
			SetState = 10
		};

		private enum AppBarNotifications
		{
			StateChanged = 0,
			PosChanged = 1,
			FullScreenApp = 2,
			WindowArrange = 3
		};

		private struct Rect
		{
			public int Left, Top, Right, Bottom;
		}

		private static IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == appBarMessage)
				switch (wParam.ToInt32())
				{
					case (int)AppBarNotifications.PosChanged:
						SizeAppbar();
						return new IntPtr(msg);
				}

			if (msg == 26 && wParam.ToInt32() == 47 && !AlwaysTop)
			{
				SetPos();
				return new IntPtr(msg);
			}

			if (msg == WM_DWMCOMPOSITIONCHANGED)
			{
				if (DWM.DwmManager.IsGlassAvailable())
				{
					DWM.DwmManager.EnableGlass(ref Handle, IntPtr.Zero);
				}
				else
				{
					DWM.DwmManager.DisableGlass(ref Handle);
				}
			}

			if (msg == WM_DWMCOLORIZATIONCOLORCHANGED)
			{
				if (DwmColorChanged != null)
					DwmColorChanged(null, EventArgs.Empty);
			}

			return IntPtr.Zero;
		}

		private static int RegisterCallBackMessage()
		{
			string uniqueMessageString = Guid.NewGuid().ToString();
			return RegisterWindowMessageW(uniqueMessageString);
		}

		internal static bool AppbarNew()
		{
			APPBARDATA msgData = new APPBARDATA();
			msgData.cbSize = Marshal.SizeOf(msgData);
			msgData.hWnd = Handle.ToInt32();
			msgData.uCallBackMessage = RegisterWindowMessageW("LongBarMessage");
			appBarMessage = msgData.uCallBackMessage;
			int retVal = SHAppBarMessage((int)AppBarMessages.NewAppBar, ref msgData);
			return (retVal != 0);
		}

		public static bool AppbarRemove()
		{
			APPBARDATA msgData = new APPBARDATA();
			msgData.cbSize = Marshal.SizeOf(msgData);
			msgData.hWnd = Handle.ToInt32();
			int retVal = SHAppBarMessage((int)AppBarMessages.Remove, ref msgData);
			return (retVal != 0);
		}

		private static void AppbarQueryPos(ref Rect appRect)
		{
			APPBARDATA msgData = new APPBARDATA();
			msgData.cbSize = Marshal.SizeOf(msgData);
			msgData.hWnd = Handle.ToInt32();
			msgData.uEdge = (int)LongBarSide;
			msgData.rc = appRect;
			SHAppBarMessage((int)AppBarMessages.QueryPos, ref msgData);
		}

		private static void AppbarSetPos(ref Rect appRect)
		{
			APPBARDATA msgData = new APPBARDATA();
			msgData.cbSize = Marshal.SizeOf(msgData);
			msgData.hWnd = Handle.ToInt32();
			msgData.uEdge = (int)LongBarSide;
			msgData.rc = appRect;
			SHAppBarMessage((int)AppBarMessages.SetPos, ref msgData);
			appRect = msgData.rc;
		}

		public static void SizeAppbar()
		{
			screen = Utilities.Utils.GetScreenFromName(screenName);
			Rect rt = new Rect();
			if (LongBarSide == Side.Left || LongBarSide == Side.Right)
			{
				rt.Top = 0;
				//rt.Bottom = SystemInformation.PrimaryMonitorSize.Height;
				rt.Bottom = screen.Bounds.Size.Height;
				if (LongBarSide == Side.Left)
				{
					if (screen != Screen.PrimaryScreen)
						/*if (SystemInformation.VirtualScreen.Left < 0)
						  rt.Left = SystemInformation.VirtualScreen.Left;
						else
						  rt.Left = 0;*/
						rt.Left = Utilities.Utils.CalculatePos(LongBarSide);
					rt.Right = (int)window.Width;
				}
				else
				{
					if (screen != Screen.PrimaryScreen)
						rt.Right = Utilities.Utils.CalculatePos(LongBarSide);
					else
						rt.Right = SystemInformation.PrimaryMonitorSize.Width;
					rt.Left = rt.Right - (int)window.Width;
				}
			}
			AppbarQueryPos(ref rt);
			switch (LongBarSide)
			{
				case Side.Left:
					rt.Right = rt.Left + (int)window.Width;
					break;
				case Side.Right:
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
				//window.Height = SystemInformation.PrimaryMonitorSize.Height;
				window.Height = screen.Bounds.Size.Height;
		}

		public static void SetPos()
		{
			if (LongBarSide == Side.Right)
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
		//private static bool _hideTray;

		public static void SetSidebar(Window wnd, Side side, bool topMost, bool overlapTaskBar, string scrnName)
		{
			window = wnd;
			Handle = new WindowInteropHelper(wnd).Handle;
			LongBarSide = side;
			screenName = scrnName;
			screen = Utilities.Utils.GetScreenFromName(scrnName);
			if (topMost)
			{
				AlwaysTop = topMost;
				AppbarNew();
				if (!Overlapped && overlapTaskBar) //&& side == Side.Right)
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
				if (Overlapped && side == Side.Right)
				{
					UnOverlapTaskbar();
				}
				SetPos();
			}
			HwndSource.FromHwnd(Handle).AddHook(new HwndSourceHook(WndProc));
		}

		public static void ResizeBar()
		{
			bool visible = General.SystemTray.SidebarVisible;
			General.SystemTray.SidebarVisible = false;
			General.SystemTray.SidebarVisible = true;
			General.SystemTray.SidebarVisible = visible;
		}

		public static void OverlapTaskbar()
		{
			/*
			IntPtr taskbarHwnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null);
			IntPtr trayHwnd = FindWindowEx(taskbarHwnd, IntPtr.Zero, "TrayNotifyWnd", null);

			WINDOWPLACEMENT lpwndpl = new WINDOWPLACEMENT();
			lpwndpl.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
			GetWindowPlacement(taskbarHwnd, ref lpwndpl);
			//Check if taskbar at top or bottom and it isn't cropped
			if (lpwndpl.rcNormalPosition.Top != 0
				&& lpwndpl.rcNormalPosition.Width == SystemInformation.PrimaryMonitorSize.Width)
			{
				//first, hide tray by setting it's width to 0
				GetWindowPlacement(trayHwnd, ref lpwndpl);
				trayWndWidth = lpwndpl.rcNormalPosition.Width; //save original width of tray
				trayWndLeft = lpwndpl.rcNormalPosition.X; //save original left pos of tray
				MoveWindow(trayHwnd, lpwndpl.rcNormalPosition.X + (int)window.Width, lpwndpl.rcNormalPosition.Y, 0, lpwndpl.rcNormalPosition.Height, true);
				//second, cut taskbar window
				GetWindowPlacement(taskbarHwnd, ref lpwndpl);
				IntPtr rgn = CreateRectRgn(0, 0, SystemInformation.PrimaryMonitorSize.Width - (int)window.Width, lpwndpl.rcNormalPosition.Height);
				SetWindowRgn(taskbarHwnd, rgn, true);
				Overlapped = true;
			}*/
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
			IntPtr taskbarHwnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null);
			IntPtr trayHwnd = FindWindowEx(taskbarHwnd, IntPtr.Zero, "TrayNotifyWnd", null);
			IntPtr rebarHwnd = FindWindowEx(taskbarHwnd, IntPtr.Zero, "RebarWindow32", null);

			WINDOWPLACEMENT lpwndpl = new WINDOWPLACEMENT();
			lpwndpl.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
			GetWindowPlacement(taskbarHwnd, ref lpwndpl);
			// Check if taskbar at top or bottom and it isn't cropped
			if (lpwndpl.rcNormalPosition.Top != 0 && lpwndpl.rcNormalPosition.Width == SystemInformation.PrimaryMonitorSize.Width)
			{
				//first, hide tray by setting it's width to 0
				GetWindowPlacement(trayHwnd, ref lpwndpl);
				trayWndWidth = lpwndpl.rcNormalPosition.Width; //save original width of tray
				trayWndLeft = lpwndpl.rcNormalPosition.X; //save original left pos of tray
				//if (_hideTray)
				MoveWindow(trayHwnd, lpwndpl.rcNormalPosition.X, lpwndpl.rcNormalPosition.Y, 0, lpwndpl.rcNormalPosition.Height, true);
				//else
				//MoveWindow(trayHwnd, SystemInformation.PrimaryMonitorSize.Width - (int)window.Width - lpwndpl.rcNormalPosition.X, lpwndpl.rcNormalPosition.Y, lpwndpl.rcNormalPosition.Width, lpwndpl.rcNormalPosition.Height, true);

				GetWindowPlacement(rebarHwnd, ref lpwndpl);
				MoveWindow(rebarHwnd, lpwndpl.rcNormalPosition.X, lpwndpl.rcNormalPosition.Y, SystemInformation.PrimaryMonitorSize.Width - (int)window.Width - lpwndpl.rcNormalPosition.X, lpwndpl.rcNormalPosition.Height, true);

				//second, cut taskbar window
				IntPtr rgn;
				// TODO: Check if the sidebar position is left
				if (LongBarSide == Side.Right) {
	                GetWindowPlacement(taskbarHwnd, ref lpwndpl);
	                rgn = CreateRectRgn(0, 0, SystemInformation.PrimaryMonitorSize.Width - (int)window.Width, lpwndpl.rcNormalPosition.Height);
	                SetWindowRgn(taskbarHwnd, rgn, true);
	                Overlapped = true;
				} else {
	                GetWindowPlacement(taskbarHwnd, ref lpwndpl);
	                rgn = CreateRectRgn(SystemInformation.PrimaryMonitorSize.Width - (int)window.Width, lpwndpl.rcNormalPosition.Height, 0, 0);
	                SetWindowRgn(taskbarHwnd, rgn, true);
	                Overlapped = true;
				}
			}
		}

		public static void UnOverlapTaskbar()
		{
			if (screen != Screen.PrimaryScreen)
				return;

			timer.Stop();

			IntPtr taskbarHwnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null);
			IntPtr trayHwnd = FindWindowEx(taskbarHwnd, IntPtr.Zero, "TrayNotifyWnd", null);

			WINDOWPLACEMENT lpwndpl = new WINDOWPLACEMENT();
			lpwndpl.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
			GetWindowPlacement(taskbarHwnd, ref lpwndpl);
			//Check if taskbar at top or bottom and it is cropped
			if (lpwndpl.rcNormalPosition.Top != 0
				&& lpwndpl.rcNormalPosition.Width == SystemInformation.PrimaryMonitorSize.Width)
			{
				//first, return tray by setting it's width back
				GetWindowPlacement(trayHwnd, ref lpwndpl);
				lpwndpl.rcNormalPosition.Width = trayWndWidth; //restore original width of tray
				lpwndpl.rcNormalPosition.X = trayWndLeft; //restore original left pos of tray
				//if (_hideTray)
				MoveWindow(trayHwnd, lpwndpl.rcNormalPosition.X, lpwndpl.rcNormalPosition.Y, trayWndWidth, lpwndpl.rcNormalPosition.Height, true);

				//second, extend taskbar window
				GetWindowPlacement(taskbarHwnd, ref lpwndpl);
				IntPtr rgn = CreateRectRgn(0, 0, SystemInformation.PrimaryMonitorSize.Width, lpwndpl.rcNormalPosition.Height);
				SetWindowRgn(taskbarHwnd, rgn, true);

				Overlapped = false;
			}
		}
	}
}
