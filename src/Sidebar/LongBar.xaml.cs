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
using Sidebar.Core;
using System.Xml.Serialization;

namespace Sidebar
{
    /// <summary>
    /// Interaction logic for LongBar.xaml
    /// </summary>
    public partial class LongBarMain : Window
    {
        public IntPtr Handle;
        static internal Settings sett;
        private Options options;
        public static List<Tile> Tiles = new List<Tile>();

        public Shadow shadow = new Shadow();
        private Library library;

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

            if (AppBar.IsOverlapping && sett.side == AppBarSide.Right)
                AppBar.RestoreTaskbar();
            SystemTray.RemoveIcon();
            AppBar.AppbarRemove();
            WriteSettings();

            RoutedEventArgs args = new RoutedEventArgs(UserControl.UnloadedEvent);
            foreach (Tile tile in TilesGrid.Children)
                tile.RaiseEvent(args);
            TilesGrid.Children.Clear();
        }

        private void LongBar_SourceInitialized(object sender, EventArgs e)
        {
            Handle = new WindowInteropHelper(this).Handle;
            ReadSettings();
            ThemesManager.LoadTheme(Sidebar.LongBarMain.sett.path, sett.theme);
            object enableGlass = ThemesManager.GetThemeParameter(Sidebar.LongBarMain.sett.path, sett.theme, "boolean", "EnableGlass");
            if (enableGlass != null && !Convert.ToBoolean(enableGlass))
                sett.enableGlass = false;
            object useSystemColor = ThemesManager.GetThemeParameter(Sidebar.LongBarMain.sett.path, sett.theme, "boolean", "UseSystemGlassColor");
            if (useSystemColor != null && Convert.ToBoolean(useSystemColor))
            {
                Bg.Fill = new SolidColorBrush(DwmManager.ColorizationColor);
                DwmManager.ColorizationColorChanged += new EventHandler(SideBar_DwmColorChanged);
            }

            LocaleManager.LoadLocale(Sidebar.LongBarMain.sett.path, sett.locale);

            this.Width = sett.width;
            SystemTray.AddIcon(this);
            // Force set sidebar window style to tool window, bypassing the restriction placed on AllowTransparency
            NativeMethods.SetWindowLong(Handle, GetWindowLongMessage.GWL_EXSTYLE, 128);
            SetSide(sett.side);
            this.MaxWidth = SystemParameters.PrimaryScreenWidth / 2;
            this.MinWidth = 31;

            DwmManager.ExcludeFromFlip3D(Handle);
            DwmManager.ExcludeFromPeek(Handle);

            SystemTray.SidebarVisibilityChanged += new SystemTray.SidebarVisibilityChangedEventHandler(SystemTray_SidebarvisibleChanged);

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
            Bg.Fill = new SolidColorBrush(DwmManager.ColorizationColor);
        }

        private void LongBar_ContentRendered(object sender, EventArgs e)
        {
            OpacityMaskGradStop.BeginAnimation(GradientStop.OffsetProperty, ((DoubleAnimation)this.Resources["LoadAnimOffset"]));
            OpacityMaskGradStop1.BeginAnimation(GradientStop.OffsetProperty, ((DoubleAnimation)this.Resources["LoadAnimOffset1"]));
            this.BeginAnimation(UIElement.OpacityProperty, ((DoubleAnimation)this.Resources["DummyAnim"]));
        }

        private void LoadAnimation_Completed(object sender, EventArgs e)
        {
            if (DwmManager.IsBlurAvailable && sett.enableGlass)
                DwmManager.EnableBlurBehindWindow(ref Handle);

            shadow.Height = this.Height;
            shadow.Top = this.Top;

            if (sett.enableShadow)
            {
                shadow.Show();
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
                    UpdateInfo updateInfo = UpdateManager.CheckForUpdates();
                    if (updateInfo.Version != null && updateInfo.Description != null)
                    {
                        TaskDialogs.UpdateDialog.ShowDialog(updateInfo.Version, updateInfo.Description);
                    }
                };
                Thread thread = new Thread(threadStarter);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }

        private void DummyAnimation_Completed(object sender, EventArgs e)
        {
            switch (sett.side)
            {
                case AppBarSide.Left:
                    LeftSideItem.IsChecked = true;
                    break;
                case AppBarSide.Right:
                    RightSideItem.IsChecked = true;
                    break;
            }

            LoadTilesAtStartup();
        }

        private void LoadTilesAtStartup()
        {
            if (sett.debug)
            {
                if (Tiles.Count > 0)
                {
                    Tiles[0].Load(sett.side, double.NaN);
                    TilesGrid.Children.Add(Tiles[0]);
                }
                return;
            }

            if (sett.tiles != null && Tiles != null && sett.tiles.Length > 0 && Tiles.Count > 0)
            {
                for (int i = 0; i < sett.tiles.Length; i++)
                {
                    TileMetadata currentMetadata = sett.tiles[i];
                    foreach (Tile tile in Tiles)
                    {
                        if (tile.File.Substring(tile.File.LastIndexOf(@"\") + 1) == currentMetadata.Name)
                        {
                            tile.minimized = currentMetadata.IsMinimized;
                            tile.Load(sett.side, currentMetadata.Height);

                            if (!tile.hasErrors)
                            {
                                TilesGrid.Children.Add(tile);
                            }
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
                        TileMetadata currentMetadata = sett.pinnedTiles[i];
                        if (tile.File.Substring(tile.File.LastIndexOf(@"\") + 1) == currentMetadata.Name)
                        {
                            tile.minimized = currentMetadata.IsMinimized;
                            tile.pinned = true;
                            tile.Load(sett.side, currentMetadata.Height);

                            tile.Header.Visibility = Visibility.Collapsed;
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

        public static void ReadSettings()
        {
            sett = new Settings();
            if (File.Exists("Settings.xml"))
            {
                using (StreamReader reader = new StreamReader("Settings.xml"))
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(Settings));
                    sett = (Settings)deserializer.Deserialize(reader);
                }
            }
        }

        private void WriteSettings()
        {
            sett.width = (int)this.Width;

            Array.Resize(ref sett.tiles, TilesGrid.Children.Count);
            Array.Resize(ref sett.pinnedTiles, PinGrid.Children.Count);

            if (TilesGrid.Children.Count > 0)
            {
                for (int i = 0; i < TilesGrid.Children.Count; i++)
                {
                    Tile currentTile = Tiles[Tiles.IndexOf(((Tile)TilesGrid.Children[i]))];
                    TileMetadata currentMetadata = new TileMetadata();

                    currentMetadata.Name = System.IO.Path.GetFileName(currentTile.File);
                    currentMetadata.IsMinimized = currentTile.minimized;
                    if (currentMetadata.IsMinimized)
                        currentMetadata.Height = currentTile.normalHeight;
                    else
                        currentMetadata.Height = currentTile.Height;
                    sett.tiles[i] = currentMetadata;
                }
            }

            if (PinGrid.Children.Count > 0)
            {
                for (int i = 0; i < PinGrid.Children.Count; i++)
                {
                    Tile currentTile = Tiles[Tiles.IndexOf(((Tile)PinGrid.Children[i]))];
                    TileMetadata currentMetadata = new TileMetadata();

                    currentMetadata.Name = System.IO.Path.GetFileName(currentTile.File);
                    currentMetadata.IsMinimized = currentTile.minimized;
                    currentMetadata.Height = currentTile.Height;
                    if (currentMetadata.IsMinimized)
                        currentMetadata.Height = currentTile.normalHeight;
                    else
                        currentMetadata.Height = currentTile.Height;

                    sett.pinnedTiles[i] = currentMetadata;
                }
            }

            XmlSerializer xmlSerializer = new XmlSerializer(sett.GetType());
            using (TextWriter textWriter = new StreamWriter("Settings.xml"))
            {
                xmlSerializer.Serialize(textWriter, sett);
            }
        }

        private void LongBar_MouseMove(object sender, MouseEventArgs e)
        {
            switch (sett.side)
            {
                case AppBarSide.Right:
                    if (e.GetPosition(this).X <= 5 && !sett.locked)
                    {
                        base.Cursor = Cursors.SizeWE;
                        if (e.LeftButton == MouseButtonState.Pressed)
                        {
                            NativeMethods.SendMessage(Handle, 274, new IntPtr(61441), IntPtr.Zero);
                            sett.width = (int)this.Width;
                            if (sett.topMost)
                                AppBar.SizeAppbar();
                            else
                                AppBar.SetPos();
                        }
                    }
                    else if (base.Cursor != Cursors.Arrow)
                        base.Cursor = Cursors.Arrow;
                    break;
                case AppBarSide.Left:
                    if (e.GetPosition(this).X >= this.Width - 5 && !sett.locked)
                    {
                        base.Cursor = Cursors.SizeWE;
                        if (e.LeftButton == MouseButtonState.Pressed)
                        {
                            NativeMethods.SendMessage(Handle, 274, new IntPtr(61442), IntPtr.Zero);
                            sett.width = (int)this.Width;
                            if (sett.topMost)
                                AppBar.SizeAppbar();
                            else
                                AppBar.SetPos();
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
                case AppBarSide.Right:
                    if (e.GetPosition(this).X <= 5 && !sett.locked)
                    {
                        this.Width = 150;
                        if (sett.topMost)
                            AppBar.SizeAppbar();
                        else
                            AppBar.SetPos();

                        shadow.Left = this.Left - shadow.Width;
                    }
                    break;
                case AppBarSide.Left:
                    if (e.GetPosition(this).X >= this.Width - 5 && !sett.locked)
                    {
                        this.Width = 150;
                        if (sett.topMost)
                            AppBar.SizeAppbar();
                        else
                            AppBar.SetPos();

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

        private void LeftSideItem_Click(object sender, RoutedEventArgs e)
        {
            if (!LeftSideItem.IsChecked)
            {
                RightSideItem.IsChecked = false;
                SetSide(AppBarSide.Left);
                sett.side = AppBarSide.Left;
                LeftSideItem.IsChecked = true;
            }
        }

        private void RightSideItem_Click(object sender, RoutedEventArgs e)
        {
            if (!RightSideItem.IsChecked)
            {
                LeftSideItem.IsChecked = false;
                SetSide(AppBarSide.Right);
                sett.side = AppBarSide.Right;
                RightSideItem.IsChecked = true;
            }
        }

        public void SetSide(AppBarSide side)
        {
            switch (side)
            {
                case AppBarSide.Left:
                    AppBar.SetSidebar(this, AppBarSide.Left, sett.topMost, sett.overlapTaskbar, sett.screen);
                    Bg.FlowDirection = FlowDirection.RightToLeft;
                    BgHighlight.FlowDirection = FlowDirection.RightToLeft;
                    BgHighlight.HorizontalAlignment = HorizontalAlignment.Right;
                    Highlight.FlowDirection = FlowDirection.RightToLeft;
                    Highlight.HorizontalAlignment = HorizontalAlignment.Right;

                    shadow.Left = this.Left + this.Width;
                    shadow.FlowDirection = FlowDirection.RightToLeft;

                    foreach (Tile tile in TilesGrid.Children)
                        tile.ChangeSide(AppBarSide.Left);
                    break;
                case AppBarSide.Right:
                    AppBar.SetSidebar(this, AppBarSide.Right, sett.topMost, sett.overlapTaskbar, sett.screen);
                    Bg.FlowDirection = FlowDirection.LeftToRight;
                    BgHighlight.FlowDirection = FlowDirection.LeftToRight;
                    BgHighlight.HorizontalAlignment = HorizontalAlignment.Left;
                    Highlight.FlowDirection = FlowDirection.LeftToRight;
                    Highlight.HorizontalAlignment = HorizontalAlignment.Left;

                    shadow.Left = this.Left - shadow.Width;
                    shadow.FlowDirection = FlowDirection.LeftToRight;

                    foreach (Tile tile in TilesGrid.Children)
                        tile.ChangeSide(AppBarSide.Right);
                    break;
            }
        }

        public void SetLocale(string locale)
        {
            LocaleManager.LoadLocale(LongBarMain.sett.path, locale);
            SystemTray.SetLocale();
            foreach (Tile tile in TilesGrid.Children)
                tile.ChangeLocale(locale);
        }

        public void SetTheme(string theme)
        {
            ThemesManager.LoadTheme(Sidebar.LongBarMain.sett.path, theme);

            object useSystemColor = ThemesManager.GetThemeParameter(Sidebar.LongBarMain.sett.path, sett.theme, "boolean", "UseSystemGlassColor");
            if (useSystemColor != null && Convert.ToBoolean(useSystemColor))
            {
                Bg.Fill = new SolidColorBrush(DwmManager.ColorizationColor);
                DwmManager.ColorizationColorChanged += new EventHandler(SideBar_DwmColorChanged);
            }
            else
            {
                Bg.SetResourceReference(Rectangle.StyleProperty, "Background");
                DwmManager.ColorizationColorChanged -= new EventHandler(SideBar_DwmColorChanged);
            }

            string file = string.Format(@"{0}\{1}.theme.xaml", sett.path, theme);

            foreach (Tile tile in TilesGrid.Children)
                tile.ChangeTheme(file);
        }

        private void LockItem_Click(object sender, RoutedEventArgs e)
        {
            if (sett.locked)
            {
                LockItem.Header = TryFindResource("Lock");
                sett.locked = false;
            }
            else
            {
                LockItem.Header = TryFindResource("Unlock");
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
            if (sett.locked)
                LockItem.Header = TryFindResource("Unlock");
            else
                LockItem.Header = TryFindResource("Lock");

            if (TilesGrid.Children.Count == 0)
                RemoveTilesItem.IsEnabled = false;
            else
                RemoveTilesItem.IsEnabled = true;

            if (System.IO.Directory.Exists(sett.path + @"\Library") && Tiles.Count != System.IO.Directory.GetDirectories(sett.path + @"\Library").Length)
                foreach (string d in System.IO.Directory.GetDirectories(sett.path + @"\Library"))
                {
                    string file = string.Format(@"{0}\{1}.dll", d, System.IO.Path.GetFileName(d));
                    if (!CheckTileAdded(file))
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
            if (!SystemTray.IsSidebarVisible)
                SystemTray.IsSidebarVisible = true;
            else SystemTray.IsSidebarVisible = false;
        }

        private void LongBar_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
        }

        private void LongBar_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, true);
            if (files.Length > 0)
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].EndsWith(".tile"))
                    {
                        FileInfo info = new FileInfo(files[i]);
                        TaskDialogs.TileInstallDialog.ShowDialog(this, info.Name, files[i]);
                    }
                    if (files[i].EndsWith(".locale.xaml"))
                    {
                        if (LocaleManager.InstallLocale(Sidebar.LongBarMain.sett.path, files[i]))
                        {
                            MessageBox.Show("Localization was succesfully installed!", "Installing localization", MessageBoxButton.OK, MessageBoxImage.Information);
                            string name = System.IO.Path.GetFileName(files[i]);
                            sett.locale = name.Substring(0, name.IndexOf(@".locale.xaml"));
                            SetLocale(sett.locale);
                        }
                        else
                            MessageBox.Show("Can't install localization.", "Installing localization", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    if (files[i].EndsWith(".theme.xaml"))
                    {
                        if (ThemesManager.InstallTheme(Sidebar.LongBarMain.sett.path, files[i]))
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

        private void GetTilesItem_Click(object sender, RoutedEventArgs e)
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
            if (y < panel.PointToScreen(new Point(0, 0)).Y)
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
                case AppBarSide.Right:
                    shadow.Left = this.Left - shadow.Width;
                    break;
                case AppBarSide.Left:
                    shadow.Left = this.Left + this.Width;
                    break;
            }
        }

        public static void ShowNotification()
        {
            Notify notify = new Notify();
            notify.Left = System.Windows.Forms.SystemInformation.WorkingArea.Right - notify.Width;
            notify.Top = System.Windows.Forms.SystemInformation.WorkingArea.Bottom - notify.Height;
            notify.ShowNotification("LongBar 2.1 Release Candidate", "<Hyperlink NavigateUri=\"http://longbar.sourceforge.net\">Click</Hyperlink>");
        }
    }
}
