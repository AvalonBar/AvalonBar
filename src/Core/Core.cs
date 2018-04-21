using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Reflection;
using ICSharpCode.SharpZipLib.Zip;
using System.Diagnostics;
using System.Windows.Threading;
using System.Collections;
using System.Xml;

namespace Slate
{
#region Desktop Window Manager section. Contains some effects of Windows Vista
  namespace Dwm
  {
	public class Glass
	{
	  [DllImport("dwmapi.dll")]
	  private static extern int DwmEnableBlurBehindWindow(IntPtr hWnd, ref BB_Struct BlurBehind);
	  [DllImport("dwmapi.dll")]
	  private static extern void DwmIscompositionEnabled(ref bool result);
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

	  public static bool IsGlassAvailable() //Check if it is not a Windows Vista or it is a Windows Vista Home Basic
	  {
		if (Environment.OSVersion.Version.Major < 6 || Environment.OSVersion.Version.Build < 5600 || !File.Exists(Environment.SystemDirectory + @"\dwmapi.dll"))
		  return false;
		else
		  return true;
	  }

	  public static bool EnableGlass(ref IntPtr handle, IntPtr rgn) //Try to enable Aero Glass. If success return true
	  {
		Region region = new Region();
		region.MakeInfinite();
		//Graphics graphics = Graphics.FromHwnd(handle);
		BB_Struct bb = new BB_Struct();
		bb.enable = true;
		bb.flags = BB_Flags.DWM_BB_ENABLE | BB_Flags.DWM_BB_BLURREGION;
		if (region != null)
		  bb.region = rgn;
		else
		  bb.region = IntPtr.Zero; //Region.GetHrgn(Graphics)
		/*HwndSource hwndSource = HwndSource.FromHwnd(handle);
		hwndSource.CompositionTarget.BackgroundColor = System.Windows.Media.Color.FromArgb(0, 0, 0, 0);*/
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
		bb.region = IntPtr.Zero; //Region.GetHrgn(Graphics)
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
			int attrValue = 1; // TRUE
			DwmSetWindowAttribute(hwnd, 12, ref attrValue, sizeof(int));
		  }
		}

		public static void RemoveFromFlip3D(IntPtr hwnd)
		{
		  if (IsGlassAvailable())
		  {
			int attrValue = (int)Flip3DPolicy.ExcludeBelow; // TRUE
			DwmSetWindowAttribute(hwnd, Flip3D, ref attrValue, sizeof(int));
		  }
		}
	}
  }
#endregion

#region General section. Contains functions which setup LongBar behaviour on screen
  namespace General
  {
	//System tray section. Put's LongBar icon to the system tray.
	public class SystemTray
	{
	  public delegate void SidebarvisibleChangedEventHandler(bool value);
	  public static event SidebarvisibleChangedEventHandler SidebarvisibleChanged;
	  private static bool _sideBarVisible = true;
	  public static bool isRunning = false;
	  protected static void OnSideBarVisibleChanged(bool value)
	  {
		if (SidebarvisibleChanged != null)
		  SidebarvisibleChanged(value);
	  }

	  [DllImport("shell32.dll")]
	  private static extern IntPtr ExtractIcon(IntPtr hInstance, string path, int iconIndex);
	  [DllImport("user32.dll")]
	  private static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
	  [DllImport("user32.dll")]
	  private static extern int SetForeGroundWindow(ref IntPtr hWnd);

	  private static string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

	  private static NotifyIcon trayIcon;
	  private static System.Windows.Controls.ContextMenu trayMenu;
	  private static Window window;
	  private static System.Windows.Controls.MenuItem closeMenuItem;
	  private static System.Windows.Controls.MenuItem showHideMenuItem;

	  public static void AddIcon(Window wnd)
	  {
		if (isRunning)
		  return;
		trayMenu = new System.Windows.Controls.ContextMenu();

		closeMenuItem = new System.Windows.Controls.MenuItem();
		closeMenuItem.AddHandler(System.Windows.Controls.MenuItem.ClickEvent, new RoutedEventHandler(CloseMenuItem_Click));

		showHideMenuItem = new System.Windows.Controls.MenuItem();
		showHideMenuItem.AddHandler(System.Windows.Controls.MenuItem.ClickEvent, new RoutedEventHandler(ShowHideMenuItem_Click));

		SetLocale();

		trayMenu.Items.Add(showHideMenuItem);
		trayMenu.Items.Add(closeMenuItem);

		trayIcon = new NotifyIcon();
		trayIcon.Icon = Icon.FromHandle(ExtractIcon(IntPtr.Zero, path + @"\LongBar.exe", 0));
		trayIcon.Text = "LongBar 2.0";
		trayIcon.MouseClick += new MouseEventHandler(trayIcon_MouseClick);
		trayIcon.MouseDoubleClick += new MouseEventHandler(trayIcon_MouseDoubleClick);
		trayIcon.Visible = true;
		window = wnd;
	  }

	  private static void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
	  {
		if(e.Button == MouseButtons.Left)
		  if (!SideBarVisible)
			SystemTray.SideBarVisible = true;
		  else SystemTray.SideBarVisible = false;
	  }

	  private static void trayIcon_MouseClick(object sender, MouseEventArgs e)
	  {
		if (e.Button == MouseButtons.Left)
		  window.Activate();
		else
		  trayMenu.IsOpen = true;
	  }

	  public static void SetLocale()
	  {
		closeMenuItem.Header = System.Windows.Application.Current.TryFindResource("Close");
		showHideMenuItem.Header = System.Windows.Application.Current.TryFindResource("ShowHide");
	  }

	  public static void RemoveIcon()
	  {
		if (isRunning)
		  return;
		trayIcon.MouseClick -= new MouseEventHandler(trayIcon_MouseClick);
		trayIcon.Visible = false;
		trayIcon.Dispose();
	  }

	  private static void CloseMenuItem_Click(object sender, RoutedEventArgs e)
	  {
		window.Close();
	  }

	  private static void ShowHideMenuItem_Click(object sender, RoutedEventArgs e)
	  {
		if (!SideBarVisible)
		  SystemTray.SideBarVisible = true;
		else SystemTray.SideBarVisible = false;
	  }

	  private static bool overlapTaskbar = false;

	  public static bool SideBarVisible
	  {
		get
		{
		  return _sideBarVisible;
		}
		set
		{
		  if (value)
		  {
			ShowWindow(SideBar.Handle, 5);
			if (SideBar.AlwaysTop)
			{
			  SideBar.AppbarRemove();
			  SideBar.AppbarNew();
			  if (!SideBar.Overlapped && overlapTaskbar)
				  SideBar.OverlapTaskbar();
			  SideBar.SizeAppbar();
			}
			_sideBarVisible = true;
			OnSideBarVisibleChanged(true);
		  }
		  else
		  {
			ShowWindow(SideBar.Handle, 0);
			if (SideBar.AlwaysTop)
			{
				SideBar.AppbarRemove();
				if (SideBar.Overlapped)
				{
					SideBar.UnOverlapTaskbar();
					overlapTaskbar = true;
				}
				else
					overlapTaskbar = true;
			}
			_sideBarVisible = false;
			OnSideBarVisibleChanged(false);
		  }
		}
	  }
	}

	//Sidebar Section. Describes LongBar behaviour on screen.
	public class SideBar
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
		if(msg == appBarMessage)
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
			if (Dwm.Glass.IsGlassAvailable())
			{
				Dwm.Glass.EnableGlass(ref Handle, IntPtr.Zero);
			}
			else
			{
				Dwm.Glass.DisableGlass(ref Handle);
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
		  /*window.Left = SystemInformation.WorkingArea.Right - window.Width;
		  window.Top = SystemInformation.WorkingArea.Top;
		  window.Height = SystemInformation.WorkingArea.Height;*/
		  window.Left = screen.WorkingArea.Right - window.Width;
		  window.Top = screen.WorkingArea.Top;
		  window.Height = screen.WorkingArea.Height;
		}
		else
		{
		  /*window.Left = SystemInformation.WorkingArea.Left;
		  window.Top = SystemInformation.WorkingArea.Top;
		  window.Height = SystemInformation.WorkingArea.Height;*/
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
		  if (!Overlapped && overlapTaskBar && side == Side.Right)
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
		bool visible = Slate.General.SystemTray.SideBarVisible;
		Slate.General.SystemTray.SideBarVisible = false;
		Slate.General.SystemTray.SideBarVisible = true;
		Slate.General.SystemTray.SideBarVisible = visible;
	  }

	  public static void OverlapTaskbar()
	  {
		  //IntPtr taskbarHwnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null);
		  //IntPtr trayHwnd = FindWindowEx(taskbarHwnd, IntPtr.Zero, "TrayNotifyWnd", null);

		  //WINDOWPLACEMENT lpwndpl = new WINDOWPLACEMENT();
		  //lpwndpl.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
		  //GetWindowPlacement(taskbarHwnd, ref lpwndpl);
		  ////Check if taskbar at top or bottom and it isn't cropped
		  //if (lpwndpl.rcNormalPosition.Top != 0
		  //    && lpwndpl.rcNormalPosition.Width == SystemInformation.PrimaryMonitorSize.Width)
		  //{
		  //    //first, hide tray by setting it's width to 0
		  //    GetWindowPlacement(trayHwnd, ref lpwndpl);
		  //    trayWndWidth = lpwndpl.rcNormalPosition.Width; //save original width of tray
		  //    trayWndLeft = lpwndpl.rcNormalPosition.X; //save original left pos of tray
		  //    MoveWindow(trayHwnd, lpwndpl.rcNormalPosition.X + (int)window.Width, lpwndpl.rcNormalPosition.Y, 0, lpwndpl.rcNormalPosition.Height, true);
		  //    //second, cut taskbar window
		  //    GetWindowPlacement(taskbarHwnd, ref lpwndpl);
		  //    IntPtr rgn = CreateRectRgn(0, 0, SystemInformation.PrimaryMonitorSize.Width - (int)window.Width, lpwndpl.rcNormalPosition.Height);
		  //    SetWindowRgn(taskbarHwnd, rgn, true);
		  //    Overlapped = true;
		  //}
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
		  //Check if taskbar at top or bottom and it isn't cropped
		  if (lpwndpl.rcNormalPosition.Top != 0
			  && lpwndpl.rcNormalPosition.Width == SystemInformation.PrimaryMonitorSize.Width)
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
			  GetWindowPlacement(taskbarHwnd, ref lpwndpl);
			  IntPtr rgn = CreateRectRgn(0, 0, SystemInformation.PrimaryMonitorSize.Width - (int)window.Width, lpwndpl.rcNormalPosition.Height);
			  SetWindowRgn(taskbarHwnd, rgn, true);
			  Overlapped = true;
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
#endregion

  namespace Theming
  {
	public class ThemeManager
	{
/*#if DEBUG
	  private static string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
#else
	  private static string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"LongBar Project Group\LongBar");
#endif*/

	  public static string[] GetThemes(string path)
	  {
		List<string> themes = new List<string>();
		if (Directory.Exists(path + @"\Themes"))
		  foreach (string file in Directory.GetFiles(path + @"\Themes"))
			if (file.EndsWith(@"theme.xaml"))
			{
			  string name = Path.GetFileName(file);
			  themes.Add(name.Substring(0, name.IndexOf(@"theme.xaml") - 1));
			}
		return themes.ToArray();
	  }

	  public static void LoadTheme(string path, string name)
	  {
		ResourceDictionary theme = new ResourceDictionary();
		if (File.Exists(String.Format(path + @"\Themes\{0}.theme.xaml", name)))
			theme.Source = new Uri(String.Format(path + @"\Themes\{0}.theme.xaml", name));
		else
		{
			System.Windows.MessageBox.Show("Theme " + name + " not found!", null, MessageBoxButton.OK, MessageBoxImage.Exclamation);
			if (name == "Aero")
				return;
			else
				LoadTheme(path, "Aero");
			return;
		}
		if (System.Windows.Application.Current.Resources.MergedDictionaries.Count > 0)
		{
		  System.Windows.Application.Current.Resources.MergedDictionaries.RemoveAt(0);
		  System.Windows.Application.Current.Resources.MergedDictionaries.Insert(0, theme);
		}
		else
		  System.Windows.Application.Current.Resources.MergedDictionaries.Add(theme);
	  }

	  public static object GetThemeParameter(string path, string themeName, string paramType, string paramName)
	  {
		  if (File.Exists(String.Format(path + @"\Themes\{0}.theme.xaml", themeName)))
		  {
			  XmlTextReader reader = new XmlTextReader(String.Format(path + @"\Themes\{0}.theme.xaml", themeName));
			  while (reader.Read())
			  {
				  if (reader.NodeType == XmlNodeType.Element && reader.Name.ToLower() == "s:" + paramType.ToLower())
				  {
					  reader.MoveToAttribute("x:Key");
					  if (reader.Value == paramName)
					  {
						  reader.MoveToContent();
						  object obj = reader.ReadElementContentAsObject();
						  reader.Close();
						  return obj;
					  }
				  }
			  }
			  return null;
		  }
		  else
			  return null;
	  }

	  public static bool InstallTheme(string path, string file)
	  {
		  try
		  {
			  if (!Directory.Exists(path + @"\Themes"))
				  Directory.CreateDirectory(path + @"\Themes");
			  FileInfo info = new FileInfo(file);
			  File.Copy(file, path + @"\Themes\" + info.Name, true);
		  }
		  catch { return false; }
		  return true;
	  }
	}
  }

  namespace Localization
  {
	public class LocaleManager
	{
/*#if DEBUG
	  private static string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
#else
	  private static string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"LongBar Project Group\LongBar");
#endif*/

	  public static string[] GetLocales(string path)
	  {
		List<string> locales = new List<string>();
		if(Directory.Exists(path + @"\Localization"))
		  foreach(string file in Directory.GetFiles(path + @"\Localization"))
			if (file.EndsWith(@".locale.xaml"))
			{
			  string name = Path.GetFileName(file);
			  locales.Add(name.Substring(0, name.IndexOf(@".locale.xaml")));
			}
		return locales.ToArray();
	  }

	  public static void LoadLocale(string path, string name)
	  {
		ResourceDictionary locale = new ResourceDictionary();
		if (File.Exists(string.Format(@"{0}\Localization\{1}.locale.xaml", path, name)))
			locale.Source = new Uri(string.Format(@"{0}\Localization\{1}.locale.xaml", path, name));
		else
		{
			System.Windows.MessageBox.Show(name + " localization file not found!", null, MessageBoxButton.OK, MessageBoxImage.Exclamation);
			if (name == "English")
				return;
			else
				LoadLocale(path, "English");
			return;
		}
		if (System.Windows.Application.Current.Resources.MergedDictionaries.Count > 1)
		{
		  System.Windows.Application.Current.Resources.MergedDictionaries.RemoveAt(1);
		  System.Windows.Application.Current.Resources.MergedDictionaries.Insert(1, locale);
		}
		else if (System.Windows.Application.Current.Resources.MergedDictionaries.Count == 1)
		  System.Windows.Application.Current.Resources.MergedDictionaries.Add(locale);

		else if (System.Windows.Application.Current.Resources.MergedDictionaries.Count == 0)
		  System.Windows.Application.Current.Resources.MergedDictionaries.Add(locale);
	  }

	  public static bool InstallLocale(string path, string file)
	  {
		  try
		  {
			  if (!Directory.Exists(path + @"\Localization"))
				  Directory.CreateDirectory(path + @"\Localization");
			  FileInfo info = new FileInfo(file);
			  File.Copy(file, path + @"\Localization\" + info.Name, true);
		  }
		  catch { return false; }
		  return true;
	  }
	}
  }

  namespace Packaging
  {
	public class PackageManager
	{
	  //private static string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	  //private static string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"LongBar Project Group\LongBar");

		public static void Unpack(string path, string file)
	  {
		using (FileStream fileStreamIn = new FileStream(file, FileMode.Open, FileAccess.Read))
		{
		  using (ZipInputStream zipInStream = new ZipInputStream(fileStreamIn))
		  {
			ZipEntry entry;
			FileInfo info = new FileInfo(file);
			string name = info.Name.Substring(0, info.Name.LastIndexOf('.'));
			while (true)
			{
			  entry = zipInStream.GetNextEntry();
			  if (entry == null)
				break;
			  if (!entry.IsDirectory)
			  {
				if (!Directory.Exists(path + @"\Library\" + name))
				  Directory.CreateDirectory(path + @"\Library\" + name);

				using (FileStream fileStreamOut = new FileStream(string.Format(@"{0}\Library\{1}\{2}", path, name, entry.Name), FileMode.Create, FileAccess.Write))
				{
				  int size;
				  byte[] buffer = new byte[1024];
				  do
				  {
					size = zipInStream.Read(buffer, 0, buffer.Length);
					fileStreamOut.Write(buffer, 0, size);
				  } while (size > 0);
				  fileStreamOut.Close();
				}
			  }
			  else
				if (!Directory.Exists(string.Format(@"{0}\Library\{1}\{2}", path, name, entry.Name)))
				  Directory.CreateDirectory(string.Format(@"{0}\Library\{1}\{2}", path, name, entry.Name));
			}
			zipInStream.Close();
		  }
		  fileStreamIn.Close();
		}
	  }
	}
  }

  namespace Utilities
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

	  internal static int CalculatePos(Slate.General.SideBar.Side side)
	  {
		int pos = 0;
		Screen[] screens = Screen.AllScreens;
		switch (side)
		{
		  case Slate.General.SideBar.Side.Left:
			pos = SystemInformation.VirtualScreen.Left;
			foreach (Screen scr in screens)
			  if (scr == Slate.General.SideBar.screen)
				break;
			  else
				pos += scr.Bounds.Width;
			break;

		  case Slate.General.SideBar.Side.Right:
			pos = SystemInformation.VirtualScreen.Right;
			for (int i = screens.Length - 1; i > 0; i--)
			  if (screens[i] == Slate.General.SideBar.screen)
				break;
			  else
				pos -= screens[i].Bounds.Width;
			break;
		}
		return pos;
	  }
	}
  }

}
