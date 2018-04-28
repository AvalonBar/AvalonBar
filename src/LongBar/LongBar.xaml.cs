using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.IO;
using System.Windows.Threading;
using System.Diagnostics;
using System.Threading;

namespace LongBar
{
	/// <summary>
	/// Interaction logic for LongBar.xaml
	/// </summary>
	public partial class LongBarMain : Window
	{
		[DllImport("user32.dll")]
		public static extern IntPtr SendMessageW(IntPtr hWnd, UInt32 msg, UInt32 wParam, IntPtr lParam);
		[DllImport("user32.dll")]
		private static extern int FindWindowW(string className, string windowName);
		[DllImport("gdi32.dll")]
		static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

		public IntPtr Handle;
		static internal Settings sett;
		private Options options;
		private string path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		public static List<Tile> Tiles = new List<Tile>();

		public Shadow shadow = new Shadow();
		private Library library;
		public static Slate.Options.Settings settOps;
		private Window dummyWin;

		internal struct Settings
		{
			public bool startup;
			public Slate.General.Sidebar.Side side;
			public string theme;
			public string locale;
			public int width;
			public bool topMost;
			public bool enableGlass;
			public bool enableShadow;
			public bool locked;
			public string[] tiles;
			public string[] heights;
			public string[] pinnedTiles;
			public bool showErrors;
			public bool overlapTaskbar;
			public string screen;
			public string path;
			public bool enableSnowFall;
			public bool enableUpdates;
			public bool debug;
			public string tileToDebug;
		}

		public LongBarMain()
		{
			InitializeComponent();
			options = new Options(this);
			this.Closed += new EventHandler(LongBar_Closed);
			this.SourceInitialized += new EventHandler(LongBar_SourceInitialized);
			this.ContentRendered += new EventHandler(LongBar_ContentRendered);
			this.MouseMove += new MouseEventHandler(LongBar_MouseMove);
			this.MouseDoubleClick += new MouseButtonEventHandler(LongBar_MouseDoubleClick);
			this.DragEnter += new DragEventHandler(LongBar_DragEnter);
			this.Drop += new DragEventHandler(LongBar_Drop);
		}

		private void LongBar_Closed(object sender, EventArgs e)
		{
			shadow.Close();

			if (Slate.General.Sidebar.Overlapped) //&& sett.side == Slate.General.Sidebar.Side.Right)
			{
				Slate.General.Sidebar.UnOverlapTaskbar();
			}
			Slate.General.SystemTray.RemoveIcon();
			Slate.General.Sidebar.AppbarRemove();

			WriteSettings();

			RoutedEventArgs args = new RoutedEventArgs(UserControl.UnloadedEvent);
			foreach (Tile tile in TilesGrid.Children)
			{
			    tile.RaiseEvent(args);
			}
			TilesGrid.Children.Clear();
		}

		private void LongBar_SourceInitialized(object sender, EventArgs e)
		{
			// Create Dummy Window
			dummyWin = new Window() { ShowInTaskbar = false, WindowStyle = System.Windows.WindowStyle.ToolWindow,
										Width = 0, Height = 0, Top = -100, Left = -100, Visibility = Visibility.Hidden };
			dummyWin.Show();
			dummyWin.Hide();
			Owner = shadow.Owner = dummyWin;

			Handle = new WindowInteropHelper(this).Handle;
			ReadSettings();
			Slate.Themes.ThemesManager.LoadTheme(LongBar.LongBarMain.sett.path, sett.theme);
			object enableGlass = Slate.Themes.ThemesManager.GetThemeParameter(LongBar.LongBarMain.sett.path, sett.theme, "boolean", "EnableGlass");
			if (enableGlass != null && !Convert.ToBoolean(enableGlass))
			    sett.enableGlass = false;
			object useSystemColor = Slate.Themes.ThemesManager.GetThemeParameter(LongBar.LongBarMain.sett.path, sett.theme, "boolean", "UseSystemGlassColor");

			if (useSystemColor != null && Convert.ToBoolean(useSystemColor))
			{
			    int color;
			    bool opaque;
			    Slate.DWM.DwmManager.DwmGetColorizationColor(out color, out opaque);
			    Bg.Fill = new SolidColorBrush(Color.FromArgb(System.Drawing.Color.FromArgb(color).A, System.Drawing.Color.FromArgb(color).R, System.Drawing.Color.FromArgb(color).G, System.Drawing.Color.FromArgb(color).B));
			    Slate.General.Sidebar.DwmColorChanged += new EventHandler(SideBar_DwmColorChanged);
			}

			Slate.Localization.LocaleManager.LoadLocale(LongBar.LongBarMain.sett.path, sett.locale);

			this.Width = sett.width;
			Slate.General.SystemTray.AddIcon(this);
			Slate.General.Sidebar.SetSidebar(this, sett.side, false, sett.overlapTaskbar, sett.screen);
			SetSide(sett.side);
			this.MaxWidth = SystemParameters.PrimaryScreenWidth / 2;
			this.MinWidth = 31;

			Slate.DWM.DwmManager.RemoveFromFlip3D(Handle);
			Slate.DWM.DwmManager.RemoveFromAeroPeek(Handle);
			
			Slate.General.SystemTray.SidebarvisibleChanged += new Slate.General.SystemTray.SidebarvisibleChangedEventHandler(SystemTray_SidebarvisibleChanged);

			GetTiles();
		}

	void SystemTray_SidebarvisibleChanged(bool value)
	{
		if (value)
			shadow.Visibility = Visibility.Visible;
		else
			shadow.Visibility = Visibility.Collapsed;
	}

	void SideBar_DwmColorChanged(object sender, EventArgs e)
	{
		int color;
		bool opaque;
		Slate.DWM.DwmManager.DwmGetColorizationColor(out color, out opaque);
		Bg.Fill = new SolidColorBrush(Color.FromArgb(System.Drawing.Color.FromArgb(color).A, System.Drawing.Color.FromArgb(color).R, System.Drawing.Color.FromArgb(color).G, System.Drawing.Color.FromArgb(color).B));

	}

	private void LongBar_ContentRendered(object sender, EventArgs e)
	{
	  OpacityMaskGradStop.BeginAnimation(GradientStop.OffsetProperty, ((DoubleAnimation)this.Resources["LoadAnimOffset"]));
	  OpacityMaskGradStop1.BeginAnimation(GradientStop.OffsetProperty, ((DoubleAnimation)this.Resources["LoadAnimOffset1"]));
	  this.BeginAnimation(UIElement.OpacityProperty, ((DoubleAnimation)this.Resources["DummyAnim"]));
	}

	private void LoadAnimation_Completed(object sender, EventArgs e)
	{
	  if (Slate.DWM.DwmManager.IsGlassAvailable() && sett.enableGlass)
		Slate.DWM.DwmManager.EnableGlass(ref Handle, IntPtr.Zero);

	  shadow.Height = this.Height;
	  shadow.Top = this.Top;

	  if (sett.enableShadow)
	  {
		  shadow.Show();
	  }

	  if (sett.enableSnowFall)
	  {
		  EnableSnowFall();
	  }

	  if (sett.enableUpdates)
	  {

		  foreach (string file in Directory.GetFiles(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.old", SearchOption.TopDirectoryOnly))
		  {
			  File.Delete(file);
		  }

		  foreach (string file in Directory.GetFiles(sett.path, "*.old", SearchOption.AllDirectories))
		  {
			  File.Delete(file);
		  }

		  ThreadStart threadStarter = delegate
		  {

			  Slate.Updates.UpdatesManager.UpdateInfo updateInfo = Slate.Updates.UpdatesManager.CheckForUpdates(Assembly.GetExecutingAssembly().GetName().Version.Build);
			  if (updateInfo.Build != null && updateInfo.Description != null)
			  {
				  TaskDialogs.UpdateDialog.ShowDialog(updateInfo.Build, updateInfo.Description);
			  }
		  };
		  Thread thread = new Thread(threadStarter);
		  thread.SetApartmentState(ApartmentState.STA);
		  thread.Start();
	  }
	}

	private void DummyAnimation_Completed(object sender, EventArgs e)
	{
		LoadTilesAtStartup();
	}

	private void LoadTilesAtStartup()
	{
		try {
	        if (!sett.debug)
	        {
	            if (sett.tiles != null && Tiles != null && sett.tiles.Length > 0 && Tiles.Count > 0)
	            {
	                for (int i = 0; i < sett.tiles.Length; i++)
	                {
	                    string tileName = sett.tiles[i];
	                    foreach (Tile tile in Tiles)
	                    {
	                        if (tile.File.Substring(tile.File.LastIndexOf(@"\") + 1) == tileName)
	                        {
	                            double tileHeight = double.NaN;
	                            if (sett.heights != null && sett.heights.Length > i)
	                            {
	                                if (sett.heights[i].EndsWith("M"))
	                                {
	                                    tileHeight = double.Parse(sett.heights[i].Replace("M", string.Empty));
	                                    tile.minimized = true;
	                                }
	                                else
	                                    tileHeight = double.Parse(sett.heights[i]);
	                            }
	                            if (!double.IsNaN(tileHeight))
	                                tile.Load(sett.side, tileHeight);
	                            else
	                                tile.Load(sett.side, double.NaN);
	                            if (!tile.hasErrors)
	                                TilesGrid.Children.Add(tile);
	                        }
	                    }
	                }
	            }

	            if (sett.pinnedTiles != null && Tiles != null && sett.pinnedTiles.Length > 0 && Tiles.Count > 0)
	            {
	                for (int i = 0; i < sett.pinnedTiles.Length; i++)
	                {
	                    foreach (Tile tile in Tiles)
	                    {
	                        if (tile.File.EndsWith(sett.pinnedTiles[i]))
	                        {
	                            tile.pinned = true;
	                            tile.Load(sett.side, double.NaN);

	                            tile.Header.Visibility = System.Windows.Visibility.Collapsed;
	                            DockPanel.SetDock(tile.Splitter, Dock.Top);
	                            ((MenuItem)tile.ContextMenu.Items[0]).IsChecked = true;

	                            if (!tile.hasErrors)
	                            {
	                                PinGrid.Children.Add(tile);
	                            }
	                        }
	                    }
	                }
	            }
	        }
	        else
	        {
	            if (Tiles.Count > 0)
	            {
	                Tiles[0].Load(sett.side, double.NaN);
	                TilesGrid.Children.Add(Tiles[0]);
	            }
	        }
	        } catch (Exception) {
				// FIXME: Temporary fix, supresses the error - issue #9
		}
	}

	private void GetTiles()
	{
		if (!sett.debug)
		{
			if (System.IO.Directory.Exists(sett.path + @"\Library"))
				foreach (string dir in System.IO.Directory.GetDirectories(sett.path + @"\Library"))
				{
					string file = string.Format(@"{0}\{1}.dll", dir, System.IO.Path.GetFileName(dir));
					if (System.IO.File.Exists(file))
					{
						Tiles.Add(new Tile(file));
						if (Tiles[Tiles.Count - 1].hasErrors)
							Tiles.RemoveAt(Tiles.Count - 1);
						else
						{
							MenuItem item = new MenuItem();
							if (Tiles[Tiles.Count - 1].Info != null)
								item.Header = Tiles[Tiles.Count - 1].Info.Name;
							item.Click += new RoutedEventHandler(AddTileSubItem_Click);
							Image icon = new Image();
							icon.Source = Tiles[Tiles.Count - 1].TitleIcon.Source;
							icon.Width = 25;
							icon.Height = 25;
							item.Icon = icon;
							AddTileItem.Items.Add(item);
						}
					}
				}
		}
		else
		{
			Tiles.Add(new Tile(sett.tileToDebug));
			if (Tiles[Tiles.Count - 1].hasErrors)
				Tiles.RemoveAt(Tiles.Count - 1);
			else
			{
				MenuItem item = new MenuItem();
				if (Tiles[Tiles.Count - 1].Info != null)
				item.Header = Tiles[Tiles.Count - 1].Info.Name;
				item.Click += new RoutedEventHandler(AddTileSubItem_Click);
				Image icon = new Image();
				icon.Source = Tiles[Tiles.Count - 1].TitleIcon.Source;
				icon.Width = 25;
				icon.Height = 25;
				item.Icon = icon;
				AddTileItem.Items.Add(item);
			}
		}
	}

	public void AddTileSubItem_Click(object sender, RoutedEventArgs e)
	{
	  int index = AddTileItem.Items.IndexOf(sender);
	  if (!((MenuItem)AddTileItem.Items[index]).IsChecked)
	  {
		Tiles[index].Load(sett.side, double.NaN);
		if (!Tiles[index].hasErrors)
		{
			TilesGrid.Children.Insert(0, Tiles[index]);
			((MenuItem)AddTileItem.Items[index]).IsChecked = true;
		}
	  }
	  else
	  {
		Tiles[index].Unload();
		((MenuItem)AddTileItem.Items[index]).IsChecked = false;
	  }
	}
	private static string settFile = "Settings.xml";
	public static void ReadSettings()
	{
	  // Load default settings before trying to check if sett. file exists
	  settOps = Slate.Options.SettingsManager.DefaultSettings;
	  if (File.Exists(settFile))
	  {
		// Load user settings file
		settOps = Slate.Options.SettingsManager.Load(settFile);
	  }
	  // Populate struct with values from settOps
	  sett.startup = settOps.Program.AutoStart;
	  sett.side = (Slate.General.Sidebar.Side)settOps.Program.Side;
	  sett.theme = settOps.Program.Theme;
	  sett.locale = settOps.Program.Language;
	  sett.width = settOps.Program.Width;
	  sett.topMost = settOps.Program.TopMost;
	  sett.enableGlass = settOps.Program.EnableGlass;
	  sett.enableShadow = settOps.Program.EnableShadow;
	  sett.locked = settOps.Program.Locked;
	  sett.overlapTaskbar = settOps.Program.OverlapTaskbar;
	  sett.showErrors = settOps.Program.ShowErrors;
//       if (settOps.Program.Path != "\\") {
	  sett.path = settOps.Program.Path;
//       }
	  sett.enableSnowFall = settOps.Program.EnableSnowFall;
	  sett.enableUpdates = settOps.Program.EnableUpdates;
	  sett.tiles = settOps.Program.Tiles.Split(new char[] {';'},  StringSplitOptions.RemoveEmptyEntries);
	  sett.heights = settOps.Program.Heights.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
	  sett.pinnedTiles = settOps.Program.PinnedTiles.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
	}

	private void WriteSettings()
	{
	  sett.width = (int)this.Width;

	  if (File.Exists(settFile))

	  Array.Resize(ref sett.tiles, TilesGrid.Children.Count);
	  Array.Resize(ref sett.heights, TilesGrid.Children.Count);

	  if (TilesGrid.Children.Count > 0)
	  {
		  for (int i = 0; i < TilesGrid.Children.Count; i++)
		  {
			  sett.tiles[i] = System.IO.Path.GetFileName(Tiles[Tiles.IndexOf(((Tile)TilesGrid.Children[i]))].File);
			  if (Tiles[Tiles.IndexOf(((Tile)TilesGrid.Children[i]))].minimized)
				  sett.heights[i] = Tiles[Tiles.IndexOf(((Tile)TilesGrid.Children[i]))].normalHeight.ToString() + "M";
			  else
				  sett.heights[i] = Tiles[Tiles.IndexOf(((Tile)TilesGrid.Children[i]))].Height.ToString();
		  }
	  }

	  if (PinGrid.Children.Count > 0)
	  {
		  sett.pinnedTiles = new string[PinGrid.Children.Count];

		  for (int i = 0; i < PinGrid.Children.Count; i++)
		  {
			  sett.pinnedTiles[i] = System.IO.Path.GetFileName(Tiles[Tiles.IndexOf(((Tile)PinGrid.Children[i]))].File);
		  }
	  }

	  settOps.Program.AutoStart = sett.startup;
	  settOps.Program.Side = (int)sett.side;
	  settOps.Program.Theme = sett.theme;
	  settOps.Program.Language = sett.locale;
	  settOps.Program.Width = sett.width;
	  settOps.Program.TopMost = sett.topMost;
	  settOps.Program.EnableGlass = sett.enableGlass;
	  settOps.Program.EnableShadow = sett.enableShadow;
	  settOps.Program.Locked = sett.locked;
	  settOps.Program.OverlapTaskbar = sett.overlapTaskbar;
	  settOps.Program.ShowErrors = sett.showErrors;
	  settOps.Program.Screen = sett.screen;
	  settOps.Program.EnableSnowFall = sett.enableSnowFall;
// FIXME: Causes a bug somewhere
//      if (sett.path == System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)) {
//          settOps.Program.Path = "\\";
//      } else {
		  settOps.Program.Path = sett.path;
//      }
	  settOps.Program.EnableUpdates = sett.enableUpdates;

	  if (sett.tiles != null && sett.tiles.Length > 0)
	  {
		string temp="";
		for (int i = 0; i < sett.tiles.Length; i++)
		{
			temp = temp + sett.tiles[i] + ";";
		}
		settOps.Program.Tiles = temp;
	  }

	  if (sett.heights != null && sett.heights.Length > 0)
	  {
		string temp="";
		  for (int i = 0; i < sett.heights.Length; i++)
		  {
			  temp = temp + sett.heights[i] + ";";
		  }
		  settOps.Program.Heights = temp;
	  }

	  if (sett.pinnedTiles != null && sett.pinnedTiles.Length > 0)
	  {
		string temp="";
		  for (int i = 0; i < sett.pinnedTiles.Length; i++)
		  {
			  temp = temp + sett.pinnedTiles[i] + ";";
		  }
		  settOps.Program.PinnedTiles = temp;
	  }
	  // Finally, save the file
	  Slate.Options.SettingsManager.Save<Slate.Options.Settings>(settOps, settFile);
	}

	private void LongBar_MouseMove(object sender, MouseEventArgs e)
	{
	  switch (sett.side)
	  {
		case Slate.General.Sidebar.Side.Right:
		  if (e.GetPosition(this).X <= 5 && !sett.locked)
		  {
			base.Cursor = Cursors.SizeWE;
			if (e.LeftButton == MouseButtonState.Pressed)
			{
			  SendMessageW(Handle, 274, 61441, IntPtr.Zero);
			  sett.width = (int)this.Width;
			  if (sett.topMost) {
				shadow.Topmost = true;
				Slate.General.Sidebar.SizeAppbar();
			  } else {
				shadow.Topmost = false;
				  Slate.General.Sidebar.SetPos();
			  }
			}
		  }
		  else if (base.Cursor != Cursors.Arrow)
			base.Cursor = Cursors.Arrow;
		  break;
		case Slate.General.Sidebar.Side.Left:
		  if (e.GetPosition(this).X >= this.Width - 5 && !sett.locked)
		  {
			base.Cursor = Cursors.SizeWE;
			if (e.LeftButton == MouseButtonState.Pressed)
			{
			  SendMessageW(Handle, 274, 61442, IntPtr.Zero);
			  sett.width = (int)this.Width;
			  if (sett.topMost) {
				shadow.Topmost = true;
				Slate.General.Sidebar.SizeAppbar();
			  } else {
				shadow.Topmost = false;
				  Slate.General.Sidebar.SetPos();
			  }
			}
		  }
		  else if (base.Cursor != Cursors.Arrow)
			base.Cursor = Cursors.Arrow;
		  break;
	  }
	}

	private void LongBar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
	  switch (sett.side)
	  {
		  case Slate.General.Sidebar.Side.Right:
		  if (e.GetPosition(this).X <= 5 && !sett.locked)
		  {
			this.Width = 150;
			  if (sett.topMost) {
				shadow.Topmost = true;
				Slate.General.Sidebar.SizeAppbar();
			  } else {
				shadow.Topmost = false;
				  Slate.General.Sidebar.SetPos();
			  }
			shadow.Left = this.Left - shadow.Width;
		  }
		  break;
		  case Slate.General.Sidebar.Side.Left:
		  if (e.GetPosition(this).X >= this.Width - 5 && !sett.locked)
		  {
			this.Width = 150;
			  if (sett.topMost) {
				shadow.Topmost = true;
				Slate.General.Sidebar.SizeAppbar();
			  } else {
				shadow.Topmost = false;
				  Slate.General.Sidebar.SetPos();
			  }
			shadow.Left = this.Left + this.Width;
		  }
		  break;
	  }
	  if (Keyboard.IsKeyDown(Key.LeftShift))
		  ShowNotification();
	}

	private void DropDownMenu_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
	  Menu.IsOpen = true;
	}

	private void CloseButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
	  this.Close();
	}

	private void CloseItem_Click(object sender, RoutedEventArgs e)
	{
	  this.Close();
	}

	private void LockItem_Checked(object sender, RoutedEventArgs e)
	{
	  sett.locked = true;
	}

	private void LockItem_Unchecked(object sender, RoutedEventArgs e)
	{
	  sett.locked = false;
	}
	public void SetSide(Slate.General.Sidebar.Side side)
	{
	  switch (side)
	  {
		case Slate.General.Sidebar.Side.Left:
		   Slate.General.Sidebar.SetSidebar(this, Slate.General.Sidebar.Side.Left, sett.topMost, sett.overlapTaskbar, sett.screen);
		   Bg.FlowDirection = FlowDirection.RightToLeft;
		   BgHighlight.FlowDirection = FlowDirection.RightToLeft;
		   BgHighlight.HorizontalAlignment = HorizontalAlignment.Right;
		   Highlight.FlowDirection = FlowDirection.RightToLeft;
		   Highlight.HorizontalAlignment = HorizontalAlignment.Right;

		   shadow.Left = this.Left + this.Width;
		   shadow.FlowDirection = FlowDirection.RightToLeft;

		  foreach (Tile tile in TilesGrid.Children)
			  tile.ChangeSide(Slate.General.Sidebar.Side.Left);
		  break;
		case Slate.General.Sidebar.Side.Right:
		  Slate.General.Sidebar.SetSidebar(this, Slate.General.Sidebar.Side.Right, sett.topMost, sett.overlapTaskbar, sett.screen);
		  Bg.FlowDirection = FlowDirection.LeftToRight;
		  BgHighlight.FlowDirection = FlowDirection.LeftToRight;
		  BgHighlight.HorizontalAlignment = HorizontalAlignment.Left;
		  Highlight.FlowDirection = FlowDirection.LeftToRight;
		  Highlight.HorizontalAlignment = HorizontalAlignment.Left;

		  shadow.Left = this.Left - shadow.Width;
		  shadow.FlowDirection = FlowDirection.LeftToRight;
		  if (sett.topMost) { shadow.Topmost = true; } else { shadow.Topmost = false; }

		  foreach (Tile tile in TilesGrid.Children)
			  tile.ChangeSide(Slate.General.Sidebar.Side.Right);
		  break;
		// FIXME: Under-implemented top/left sides
		case Slate.General.Sidebar.Side.Top:
		  Slate.General.Sidebar.SetSidebar(this, Slate.General.Sidebar.Side.Top, sett.topMost, false, sett.screen);
		  Bg.FlowDirection = FlowDirection.LeftToRight;
		  BgHighlight.FlowDirection = FlowDirection.LeftToRight;
		  BgHighlight.HorizontalAlignment = HorizontalAlignment.Left;
		  Highlight.FlowDirection = FlowDirection.LeftToRight;
		  Highlight.HorizontalAlignment = HorizontalAlignment.Left;

		  shadow.Left = this.Left - shadow.Width;
		  shadow.FlowDirection = FlowDirection.LeftToRight;
		  if (sett.topMost) { shadow.Topmost = true; } else { shadow.Topmost = false; }

		  foreach (Tile tile in TilesGrid.Children)
			  tile.ChangeSide(Slate.General.Sidebar.Side.Right);
		  break;
		case Slate.General.Sidebar.Side.Bottom:
		  Slate.General.Sidebar.SetSidebar(this, Slate.General.Sidebar.Side.Bottom, sett.topMost, false, sett.screen);
		  Bg.FlowDirection = FlowDirection.LeftToRight;
		  BgHighlight.FlowDirection = FlowDirection.LeftToRight;
		  BgHighlight.HorizontalAlignment = HorizontalAlignment.Left;
		  Highlight.FlowDirection = FlowDirection.LeftToRight;
		  Highlight.HorizontalAlignment = HorizontalAlignment.Left;

		  shadow.Left = this.Left - shadow.Width;
		  shadow.FlowDirection = FlowDirection.LeftToRight;
		  if (sett.topMost) { shadow.Topmost = true; } else { shadow.Topmost = false; }

		  foreach (Tile tile in TilesGrid.Children)
			  tile.ChangeSide(Slate.General.Sidebar.Side.Right);
		  break;
	  }
	}

	public void SetLocale(string locale)
	{
		Slate.Localization.LocaleManager.LoadLocale(LongBar.LongBarMain.sett.path, locale);
		Slate.General.SystemTray.SetLocale();
		foreach (Tile tile in TilesGrid.Children)
		  tile.ChangeLocale(locale);
	}

	public void SetTheme(string theme)
	{
		Slate.Themes.ThemesManager.LoadTheme(LongBar.LongBarMain.sett.path, theme);

		object useSystemColor = Slate.Themes.ThemesManager.GetThemeParameter(LongBar.LongBarMain.sett.path, sett.theme, "boolean", "UseSystemGlassColor");
		if (useSystemColor != null && Convert.ToBoolean(useSystemColor))
		{
			int color;
			bool opaque;
			Slate.DWM.DwmManager.DwmGetColorizationColor(out color, out opaque);
			//HwndSource.FromHwnd(Handle).CompositionTarget.BackgroundColor = Color.FromArgb(System.Drawing.Color.FromArgb(color).A,System.Drawing.Color.FromArgb(color).R,System.Drawing.Color.FromArgb(color).G,System.Drawing.Color.FromArgb(color).B);
			Bg.Fill = new SolidColorBrush(Color.FromArgb(System.Drawing.Color.FromArgb(color).A, System.Drawing.Color.FromArgb(color).R, System.Drawing.Color.FromArgb(color).G, System.Drawing.Color.FromArgb(color).B));
			Slate.General.Sidebar.DwmColorChanged += new EventHandler(SideBar_DwmColorChanged);
		}
		else
		{
			Bg.SetResourceReference(Rectangle.StyleProperty, "Background");
			Slate.General.Sidebar.DwmColorChanged -= new EventHandler(SideBar_DwmColorChanged);
		}

		string file = string.Format(@"{0}\{1}.theme.xaml", sett.path, theme);

		foreach (Tile tile in TilesGrid.Children)
			tile.ChangeTheme(file);
	}

	private void LockItem_Click(object sender, RoutedEventArgs e)
	{
	  if (sett.locked)
	  {
		LockItem.IsChecked = false;
		LockItem.Header = TryFindResource("Lock");
		sett.locked = false;
	  }
	  else
	  {
		LockItem.IsChecked = true;
		LockItem.Header = TryFindResource("Lock");
		sett.locked = true;
	  }
	}

	private void SettingsItem_Click(object sender, RoutedEventArgs e)
	{
	  if (options.IsVisible)
	  {
		options.Activate();
		return;
	  }
	  options = new Options(this);
	  options.ShowDialog();
	}

	private void Menu_Opened(object sender, RoutedEventArgs e)
	{
		if (sett.locked) {
		LockItem.IsChecked = true;
		LockItem.Header = TryFindResource("Lock");
		} else {
		LockItem.IsChecked = false;
		LockItem.Header = TryFindResource("Lock");
		}
	  if (TilesGrid.Children.Count == 0)
		  RemoveTilesItem.IsEnabled = false;
	  else
		  RemoveTilesItem.IsEnabled = true;

	  if (System.IO.Directory.Exists(sett.path + @"\Library") && Tiles.Count != System.IO.Directory.GetDirectories(sett.path + @"\Library").Length)
		foreach (string d in System.IO.Directory.GetDirectories(sett.path + @"\Library"))
		{
		  string file = string.Format(@"{0}\{1}.dll", d, System.IO.Path.GetFileName(d));
		  if(!CheckTileAdded(file))
			if (System.IO.File.Exists(file))
			{
			  Tiles.Add(new Tile(file));
			  if (Tiles[Tiles.Count - 1].hasErrors)
				Tiles.RemoveAt(Tiles.Count - 1);
			  else
			  {
				MenuItem item = new MenuItem();
				if (Tiles[Tiles.Count - 1].Info != null)
				  item.Header = Tiles[Tiles.Count - 1].Info.Name;
				item.Click += new RoutedEventHandler(AddTileSubItem_Click);
				AddTileItem.Items.Add(item);
			  }
			}
		}
	  for (int i = 0; i < Tiles.Count; i++)
		if (Tiles[i].isLoaded)
		  ((MenuItem)AddTileItem.Items[i]).IsChecked = true;
		else
		  ((MenuItem)AddTileItem.Items[i]).IsChecked = false;
	  if (AddTileItem.Items.Count > 0)
		AddTileItem.IsEnabled = true;
	  else
		AddTileItem.IsEnabled = false;
	}

	private bool CheckTileAdded(string file)
	{
	  foreach (Tile tile in Tiles)
		if (tile.File == file)
		  return true;
	  return false;
	}

	private void MinimizeItem_Click(object sender, RoutedEventArgs e)
	{
		if (!Slate.General.SystemTray.SidebarVisible)
			Slate.General.SystemTray.SidebarVisible = true;
		else Slate.General.SystemTray.SidebarVisible = false;
	}

	private void LongBar_DragEnter(object sender, DragEventArgs e)
	{
	  if(e.Data.GetDataPresent(DataFormats.FileDrop))
		e.Effects = DragDropEffects.Copy;
	}

	private void LongBar_Drop(object sender, DragEventArgs e)
	{
	  string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, true);
	  if(files.Length>0)
		  for (int i = 0; i < files.Length; i++)
		  {
			  if (files[i].EndsWith(".tile"))
			  {
				  FileInfo info = new FileInfo(files[i]);
				  TaskDialogs.TileInstallDialog.ShowDialog(this, info.Name, files[i]);
			  }
			  if (files[i].EndsWith(".locale.xaml"))
			  {
				  if (Slate.Localization.LocaleManager.InstallLocale(LongBar.LongBarMain.sett.path, files[i]))
				  {
					  MessageBox.Show("Locale was succesfully installed!", "Installing localization", MessageBoxButton.OK, MessageBoxImage.Information);
					  string name = System.IO.Path.GetFileName(files[i]);
					  sett.locale = name.Substring(0, name.IndexOf(@".locale.xaml"));
					  SetLocale(sett.locale);
				  }
				  else
					  MessageBox.Show("Can't install locale.", "Installing localization", MessageBoxButton.OK, MessageBoxImage.Error);
			  }
			  if (files[i].EndsWith(".theme.xaml"))
			  {
				  if (Slate.Themes.ThemesManager.InstallTheme(LongBar.LongBarMain.sett.path, files[i]))
				  {
					  MessageBox.Show("Theme was succesfully installed!", "Installing theme", MessageBoxButton.OK, MessageBoxImage.Information);
					  string name = System.IO.Path.GetFileName(files[i]);
					  sett.theme = name.Substring(0, name.IndexOf(@".theme.xaml"));
					  SetTheme(sett.theme);
				  }
				  else
					  MessageBox.Show("Can't install theme.", "Installing theme", MessageBoxButton.OK, MessageBoxImage.Error);
			  }
		  }
	}

	private void TileLibrary_Click(object sender, RoutedEventArgs e)
	{
		if (library != null && library.IsLoaded)
			library.Activate();
		else
		{
			library = new Library(this);
			library.Show();
		}
	}

	private void RemoveTilesItem_Click(object sender, RoutedEventArgs e)
	{
		for (int i = 0; i < TilesGrid.Children.Count; i++)
		{
			int index = Tiles.IndexOf((Tile)TilesGrid.Children[i]);
			Tiles[index].Unload();
			((MenuItem)AddTileItem.Items[index]).IsChecked = false;
		}
	}

	  public static int GetElementIndexByYCoord(StackPanel panel, double y)
	  {
		  Point pos;
		  for (int i = 0; i < panel.Children.Count; i++)
		  {
			  pos = panel.Children[i].PointToScreen(new Point(0, 0));
			  if (y > pos.Y && y < pos.Y + ((FrameworkElement)panel.Children[i]).Height)
				  return i;
		  }
		  if (y < panel.PointToScreen(new Point(0,0)).Y)
			  return -1;
		  else
			  return 100500;
	  }

	  private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
	  {
		  shadow.Height = this.Height;
		  shadow.Top = this.Top;
		  switch (sett.side)
		  {
			  case Slate.General.Sidebar.Side.Right:
				  shadow.Left = this.Left - shadow.Width;
				  break;
			  case Slate.General.Sidebar.Side.Left:
				  shadow.Left = this.Left + this.Width;
				  break;
		  }
	  }

	  public void EnableSnowFall()
	  {
		  if (SnowFallCanvas.Visibility == Visibility.Collapsed)
		  {
			  SnowFallCanvas.Visibility = Visibility.Visible;
			  SnowFallCanvas.Width = this.Width;
			  Random r = new Random(Environment.TickCount);
			  for (int i = 0; i < 50; i++)
			  {
				  SnowFall.SnowFlake snowFlake = new LongBar.SnowFall.SnowFlake();
				  snowFlake.SetValue(Canvas.LeftProperty, (double)r.Next((int)this.Width));
				  snowFlake.SetValue(Canvas.TopProperty, (double)r.Next((int)this.Height));
				  snowFlake.Width = 10 + r.Next(15);
				  snowFlake.Height = snowFlake.Width;
				  snowFlake.Visibility = Visibility.Visible;
				  SnowFallCanvas.Children.Add(snowFlake);
				  snowFlake.Enabled = true;
			  }
		  }
	  }

	  public void DisableSnowFall()
	  {
		  SnowFallCanvas.Visibility = Visibility.Collapsed;
		  foreach (LongBar.SnowFall.SnowFlake snowFlake in SnowFallCanvas.Children)
		  {
			  snowFlake.Enabled = false;
		  }
		  SnowFallCanvas.Children.Clear();
	  }

	  public static void ShowNotification()
	  {
		  Notify notify = new Notify();
		  notify.Left = System.Windows.Forms.SystemInformation.WorkingArea.Right - notify.Width;
		  notify.Top = System.Windows.Forms.SystemInformation.WorkingArea.Bottom - notify.Height;
		  notify.MouseLeftButtonDown += new MouseButtonEventHandler(notify_MouseLeftButtonDown);
		  notify.Show();
	  }

	  static void notify_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	  {
		  ((Window)sender).Close();
	  }
  }
}