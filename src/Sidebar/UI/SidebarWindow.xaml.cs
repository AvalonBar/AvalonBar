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
using System.Xml.Serialization;

namespace Sidebar
{
    /// <summary>
    /// Interaction logic for SidebarWindow.xaml
    /// </summary>
    public partial class SidebarWindow : Window
    {
        private IntPtr Handle;
        private OptionsWindow options;
        private SystemTray Tray;
        public static List<Tile> Tiles = new List<Tile>();

        public ShadowWindow shadow = new ShadowWindow();
        private LibraryWindow library;

        public SidebarWindow()
        {
            InitializeComponent();
            options = new OptionsWindow(this);
        }

        public TileState GetTileState(Tile tile)
        {
            TileState tileState = new TileState();

            tileState.Name = System.IO.Path.GetFileName(tile.File);
            tileState.IsMinimized = tile.minimized;
            tileState.Height = tile.Height;
            if (tileState.IsMinimized)
            {
                tileState.Height = tile.normalHeight;
            }

            return tileState;
        }
        public TileState[] GetAllTileStates(bool isPinned)
        {
            TileState[] tileStates = { };

            if (isPinned && PinGrid.Children.Count > 0)
            {
                Array.Resize(ref tileStates, PinGrid.Children.Count);
                for (int i = 0; i < PinGrid.Children.Count; i++)
                {
                    Tile currentTile = Tiles[Tiles.IndexOf(((Tile)PinGrid.Children[i]))];
                    tileStates[i] = GetTileState(currentTile);
                }
                return tileStates;
            }

            if (!isPinned && TilesGrid.Children.Count > 0)
            {
                Array.Resize(ref tileStates, TilesGrid.Children.Count);
                for (int i = 0; i < TilesGrid.Children.Count; i++)
                {
                    Tile currentTile = Tiles[Tiles.IndexOf(((Tile)TilesGrid.Children[i]))];
                    tileStates[i] = GetTileState(currentTile);
                }
                return tileStates;
            }

            return tileStates;
        }

        private void LongBar_Closed(object sender, EventArgs e)
        {
            shadow.Close();

            if (AppBar.IsOverlapping && Settings.Current.side == AppBarSide.Right)
                AppBar.RestoreTaskbar();
            Tray.Dispose();
            AppBar.AppbarRemove();
            Settings.Current.Save(this);

            RoutedEventArgs args = new RoutedEventArgs(UserControl.UnloadedEvent);
            foreach (Tile tile in TilesGrid.Children)
                tile.RaiseEvent(args);
            TilesGrid.Children.Clear();
        }

        private void LongBar_SourceInitialized(object sender, EventArgs e)
        {
            Handle = new WindowInteropHelper(this).Handle;
            AssetManager.Load(AssetKind.Theme, Settings.Current.theme);
            object enableGlass = AssetManager.GetThemeParameter(Settings.Current.theme, "boolean", "EnableGlass");
            if (enableGlass != null && !Convert.ToBoolean(enableGlass))
                Settings.Current.enableGlass = false;
            object useSystemColor = AssetManager.GetThemeParameter(Settings.Current.theme, "boolean", "UseSystemGlassColor");
            if (useSystemColor != null && Convert.ToBoolean(useSystemColor))
            {
                Bg.Fill = new SolidColorBrush(CompositionHelper.ColorizationColor);
                CompositionHelper.ColorizationColorChanged += new EventHandler(SideBar_DwmColorChanged);
            }

            AssetManager.Load(AssetKind.Locale, Settings.Current.locale);

            this.Width = Settings.Current.width;
            Tray = new SystemTray(this);
            // Force set sidebar window style to tool window, bypassing the restriction placed on AllowTransparency
            NativeMethods.SetWindowLong(Handle, GetWindowLongMessage.GWL_EXSTYLE, 128);
            SetSide(Settings.Current.side);
            this.MaxWidth = SystemParameters.PrimaryScreenWidth / 2;
            this.MinWidth = 31;

            CompositionHelper.ExcludeFromTaskView(Handle);
            CompositionHelper.ExcludeFromPeek(Handle);

            GetTiles();
        }

        void SideBar_DwmColorChanged(object sender, EventArgs e)
        {
            Bg.Fill = new SolidColorBrush(CompositionHelper.ColorizationColor);
        }

        private void LongBar_ContentRendered(object sender, EventArgs e)
        {
            OpacityMaskGradStop.BeginAnimation(GradientStop.OffsetProperty, ((DoubleAnimation)this.Resources["LoadAnimOffset"]));
            OpacityMaskGradStop1.BeginAnimation(GradientStop.OffsetProperty, ((DoubleAnimation)this.Resources["LoadAnimOffset1"]));
            this.BeginAnimation(UIElement.OpacityProperty, ((DoubleAnimation)this.Resources["DummyAnim"]));
        }

        private bool _initialAnimationDone = false;
        private void LoadAnimation_Completed(object sender, EventArgs e)
        {
            if (CompositionHelper.AvailableCompositionMethod != CompositionMethod.None && Settings.Current.enableGlass)
                CompositionHelper.SetBlurBehindWindow(ref Handle, true);

            shadow.Height = this.Height;
            shadow.Top = this.Top;

            if (Settings.Current.enableShadow)
            {
                shadow.Show();
                shadow.Owner = this;
            }

            if (Settings.Current.enableUpdates)
            {
                foreach (string file in Directory.GetFiles(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.old", SearchOption.TopDirectoryOnly))
                {
                    File.Delete(file);
                }

                foreach (string file in Directory.GetFiles(Settings.Current.path, "*.old", SearchOption.AllDirectories))
                {
                    File.Delete(file);
                }

                ThreadStart threadStarter = delegate
                {
                    UpdateInfo updateInfo = Services.CheckForUpdates();
                    if (updateInfo.Version != null && updateInfo.Description != null)
                    {
                        UpdateDialog.ShowDialog(updateInfo);
                    }
                };
                Thread thread = new Thread(threadStarter);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }

            _initialAnimationDone = true;
        }

        private void DummyAnimation_Completed(object sender, EventArgs e)
        {
            switch (Settings.Current.side)
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
            if (Settings.Current.debug)
            {
                if (Tiles.Count > 0)
                {
                    Tiles[0].Load(Settings.Current.side, double.NaN);
                    TilesGrid.Children.Add(Tiles[0]);
                }
                return;
            }

            if (Settings.Current.tiles != null && Tiles != null && Settings.Current.tiles.Length > 0 && Tiles.Count > 0)
            {
                for (int i = 0; i < Settings.Current.tiles.Length; i++)
                {
                    TileState currentMetadata = Settings.Current.tiles[i];
                    foreach (Tile tile in Tiles)
                    {
                        if (tile.File.Substring(tile.File.LastIndexOf(@"\") + 1) == currentMetadata.Name)
                        {
                            tile.minimized = currentMetadata.IsMinimized;
                            tile.Load(Settings.Current.side, currentMetadata.Height);

                            if (!tile.hasErrors)
                            {
                                TilesGrid.Children.Add(tile);
                            }
                        }
                    }
                }
            }

            if (Settings.Current.pinnedTiles != null && Tiles != null && Settings.Current.pinnedTiles.Length > 0 && Tiles.Count > 0)
            {
                for (int i = 0; i < Settings.Current.pinnedTiles.Length; i++)
                {
                    foreach (Tile tile in Tiles)
                    {
                        TileState currentMetadata = Settings.Current.pinnedTiles[i];
                        if (tile.File.Substring(tile.File.LastIndexOf(@"\") + 1) == currentMetadata.Name)
                        {
                            tile.minimized = currentMetadata.IsMinimized;
                            tile.pinned = true;
                            tile.Load(Settings.Current.side, currentMetadata.Height);

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
            if (!Settings.Current.debug)
            {
                if (System.IO.Directory.Exists(Settings.Current.path + @"\Library"))
                    foreach (string dir in System.IO.Directory.GetDirectories(Settings.Current.path + @"\Library"))
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
                Tiles.Add(new Tile(Settings.Current.tileToDebug));
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

                Tiles[index].Load(Settings.Current.side, double.NaN);

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

        private void LongBar_MouseMove(object sender, MouseEventArgs e)
        {
            switch (Settings.Current.side)
            {
                case AppBarSide.Right:
                    if (e.GetPosition(this).X <= 5 && !Settings.Current.locked)
                    {
                        base.Cursor = Cursors.SizeWE;
                        if (e.LeftButton == MouseButtonState.Pressed)
                        {
                            NativeMethods.SendMessage(Handle, 274, new IntPtr(61441), IntPtr.Zero);
                            Settings.Current.width = (int)this.Width;
                            if (Settings.Current.topMost)
                                AppBar.SizeAppbar();
                            else
                                AppBar.SetPos();
                        }
                    }
                    else if (base.Cursor != Cursors.Arrow)
                        base.Cursor = Cursors.Arrow;
                    break;
                case AppBarSide.Left:
                    if (e.GetPosition(this).X >= this.Width - 5 && !Settings.Current.locked)
                    {
                        base.Cursor = Cursors.SizeWE;
                        if (e.LeftButton == MouseButtonState.Pressed)
                        {
                            NativeMethods.SendMessage(Handle, 274, new IntPtr(61442), IntPtr.Zero);
                            Settings.Current.width = (int)this.Width;
                            if (Settings.Current.topMost)
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
            switch (Settings.Current.side)
            {
                case AppBarSide.Right:
                    if (e.GetPosition(this).X <= 5 && !Settings.Current.locked)
                    {
                        this.Width = 150;
                        if (Settings.Current.topMost)
                            AppBar.SizeAppbar();
                        else
                            AppBar.SetPos();

                        shadow.Left = this.Left - shadow.Width;
                    }
                    break;
                case AppBarSide.Left:
                    if (e.GetPosition(this).X >= this.Width - 5 && !Settings.Current.locked)
                    {
                        this.Width = 150;
                        if (Settings.Current.topMost)
                            AppBar.SizeAppbar();
                        else
                            AppBar.SetPos();

                        shadow.Left = this.Left + this.Width;
                    }
                    break;
            }
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
            Settings.Current.locked = true;
        }

        private void LockItem_Unchecked(object sender, RoutedEventArgs e)
        {
            Settings.Current.locked = false;
        }

        private void LeftSideItem_Click(object sender, RoutedEventArgs e)
        {
            if (!LeftSideItem.IsChecked)
            {
                RightSideItem.IsChecked = false;
                SetSide(AppBarSide.Left);
                Settings.Current.side = AppBarSide.Left;
                LeftSideItem.IsChecked = true;
            }
        }

        private void RightSideItem_Click(object sender, RoutedEventArgs e)
        {
            if (!RightSideItem.IsChecked)
            {
                LeftSideItem.IsChecked = false;
                SetSide(AppBarSide.Right);
                Settings.Current.side = AppBarSide.Right;
                RightSideItem.IsChecked = true;
            }
        }

        public void SetSide(AppBarSide side)
        {
            switch (side)
            {
                case AppBarSide.Left:
                    AppBar.SetSidebar(this, AppBarSide.Left, Settings.Current.topMost, Settings.Current.overlapTaskbar, Settings.Current.screen);
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
                    AppBar.SetSidebar(this, AppBarSide.Right, Settings.Current.topMost, Settings.Current.overlapTaskbar, Settings.Current.screen);
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
            AssetManager.Load(AssetKind.Locale, locale);
            Tray.SetLocale();
            foreach (Tile tile in TilesGrid.Children)
                tile.ChangeLocale(locale);
        }

        public void SetTheme(string theme)
        {
            AssetManager.Load(AssetKind.Theme, theme);

            object useSystemColor = AssetManager.GetThemeParameter(Settings.Current.theme, "boolean", "UseSystemGlassColor");
            if (useSystemColor != null && Convert.ToBoolean(useSystemColor))
            {
                Bg.Fill = new SolidColorBrush(CompositionHelper.ColorizationColor);
                CompositionHelper.ColorizationColorChanged += new EventHandler(SideBar_DwmColorChanged);
            }
            else
            {
                Bg.SetResourceReference(Rectangle.StyleProperty, "Background");
                CompositionHelper.ColorizationColorChanged -= new EventHandler(SideBar_DwmColorChanged);
            }

            string file = string.Format(@"{0}\{1}.theme.xaml", Settings.Current.path, theme);

            foreach (Tile tile in TilesGrid.Children)
                tile.ChangeTheme(file);
        }

        private void LockItem_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.Current.locked)
            {
                LockItem.Header = Utils.FindString("Lock");
                Settings.Current.locked = false;
            }
            else
            {
                LockItem.Header = Utils.FindString("Unlock");
                Settings.Current.locked = true;
            }
        }

        private void SettingsItem_Click(object sender, RoutedEventArgs e)
        {
            if (options.IsVisible)
            {
                options.Activate();
                return;
            }
            options = new OptionsWindow(this);
            options.ShowDialog();
        }

        private void Menu_Opened(object sender, RoutedEventArgs e)
        {
            if (Settings.Current.locked)
                LockItem.Header = Utils.FindString("Unlock");
            else
                LockItem.Header = Utils.FindString("Lock");

            if (TilesGrid.Children.Count == 0)
                RemoveTilesItem.IsEnabled = false;
            else
                RemoveTilesItem.IsEnabled = true;

            if (System.IO.Directory.Exists(Settings.Current.path + @"\Library") && Tiles.Count != System.IO.Directory.GetDirectories(Settings.Current.path + @"\Library").Length)
                foreach (string d in System.IO.Directory.GetDirectories(Settings.Current.path + @"\Library"))
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
            Hide();
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
                        TileInstallDialog.ShowDialog(this, info.Name, files[i]);
                    }
                    if (files[i].EndsWith(".locale.xaml"))
                    {
                        if (AssetManager.Install(AssetKind.Locale, files[i]))
                        {
                            MessageBox.Show("Localization was succesfully installed!", "Installing localization", MessageBoxButton.OK, MessageBoxImage.Information);
                            string name = System.IO.Path.GetFileName(files[i]);
                            Settings.Current.locale = name.Substring(0, name.IndexOf(@".locale.xaml"));
                            SetLocale(Settings.Current.locale);
                        }
                        else
                            MessageBox.Show("Can't install localization.", "Installing localization", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    if (files[i].EndsWith(".theme.xaml"))
                    {
                        if (AssetManager.Install(AssetKind.Theme, files[i]))
                        {
                            MessageBox.Show("Theme was succesfully installed!", "Installing theme", MessageBoxButton.OK, MessageBoxImage.Information);
                            string name = System.IO.Path.GetFileName(files[i]);
                            Settings.Current.theme = name.Substring(0, name.IndexOf(@".theme.xaml"));
                            SetTheme(Settings.Current.theme);
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
                library = new LibraryWindow(this);
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
            switch (Settings.Current.side)
            {
                case AppBarSide.Right:
                    shadow.Left = this.Left - shadow.Width;
                    break;
                case AppBarSide.Left:
                    shadow.Left = this.Left + this.Width;
                    break;
            }
        }

        // TODO: Taken from SystemTray.css
        private static bool overlapTaskbar = false;
        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                if (AppBar.IsTopmost)
                {
                    AppBar.AppbarRemove();
                    AppBar.AppbarNew();
                    if (!AppBar.IsOverlapping && overlapTaskbar)
                        AppBar.OverlapTaskbar();
                    AppBar.SizeAppbar();
                }
                if (Settings.Current.enableShadow && _initialAnimationDone)
                {
                    shadow.Show();
                }
            }
            else
            {
                if (AppBar.IsTopmost)
                {
                    AppBar.AppbarRemove();
                    if (AppBar.IsOverlapping)
                    {
                        AppBar.RestoreTaskbar();
                        overlapTaskbar = true;
                    }
                    else
                    {
                        overlapTaskbar = false;
                    }
                }
                shadow.Hide();
            }
        }
    }
}
