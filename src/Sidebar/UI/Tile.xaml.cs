﻿using System;
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
using System.Reflection;
using System.ComponentModel;
using System.Windows.Media.Animation;
using Applications.Sidebar;
using System.Threading;
using System.IO;

namespace Sidebar
{
    /// <summary>
    /// Interaction logic for Tile.xaml
    /// </summary>
    public partial class Tile : UserControl
    {
        public string File;
        public TileLib.TileInfo Info;
        private Type TileType;
        private TileLib.BaseTile tileObject;
        private Applications.Sidebar.Tile tileKObject;
        private Assembly tileAssembly;
        private BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private FlyoutWindow flyout;
        private TileOptionsWindow options;
        private ModelType tileModelType;
        public bool hasErrors;
        internal bool minimized;
        internal double normalHeight; //Unminimized height
        public bool pinned;
        public bool isLoaded = false;

        private enum ModelType
        {
            LongBar,
            KarlsSidebar
        };

        public Tile(string file)
        {
            InitializeComponent();
            if (System.IO.File.Exists(file))
            {
                this.File = file;
                this.Info = new TileLib.TileInfo("", false, false);
                this.Info.Name = file.Substring(file.LastIndexOf(@"\") + 1, file.Length - file.LastIndexOf(@"\") - 5);
            }


        }

        private ModelType GetTileModelType(Assembly assembly)
        {
            try
            {
                if (!System.IO.Directory.Exists(Settings.Current.path + @"\Logs"))
                    System.IO.Directory.CreateDirectory(Settings.Current.path + @"\Logs");
                for (int i = 0; i < assembly.GetExportedTypes().Length; i++)
                    if (assembly.GetExportedTypes()[i].BaseType != null)
                    {
                        if (assembly.GetExportedTypes()[i].BaseType == typeof(TileLib.BaseTile))
                            return ModelType.LongBar;
                        if (assembly.GetExportedTypes()[i].BaseType.ToString() == "Applications.Sidebar.BaseTile")
                            return ModelType.KarlsSidebar;
                    }
            }
            catch (Exception ex)
            {
                if (Settings.Current.showErrors)
                    MessageBox.Show(string.Format("The tile {0} is incompatible to current version of application. Please contact the tile's developers" +
                "\nSee log for detailed information.", System.IO.Path.GetFileName(this.File)), null, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                if (!System.IO.Directory.Exists(Settings.Current.path + @"\Logs"))
                    System.IO.Directory.CreateDirectory(Settings.Current.path + @"\Logs");
                string logFile = string.Format(@"{0}\Logs\{1}.{2}.{3}.log", Settings.Current.path, DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year);
                try
                {
                    System.IO.File.AppendAllText(logFile, String.Format("{0}\r\n{1}\r\n--------------------------------------------------------------------------------------\r\n",
                      DateTime.UtcNow.ToString(), ex));
                }
                catch (Exception ex1)
                {
                    MessageBox.Show("Can't write to log. Reason: " + ex1.Message, null, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                hasErrors = true;
            }
            return ModelType.LongBar;
        }

        FrameworkElement control = null;
        public void Load(AppBarSide side, double height)
        {
            ////////////////////////////////////
            if (!System.IO.File.Exists(this.File))
                return;
            tileAssembly = Assembly.LoadFrom(this.File);
            tileModelType = GetTileModelType(tileAssembly);
            switch ((ModelType)tileModelType)
            {
                case ModelType.LongBar:
                    foreach (Type type in tileAssembly.GetTypes())
                        if (type.BaseType == typeof(TileLib.BaseTile))
                            TileType = type;
                    foreach (Attribute attr in tileAssembly.GetCustomAttributes(false))
                        if (attr.GetType() == typeof(TileLib.TileInfo))
                        {
                            Info = (TileLib.TileInfo)attr;
                            this.TitleTextBlock.Text = Info.Name;
                        }
                    break;

                case ModelType.KarlsSidebar:
                    foreach (Type type in tileAssembly.GetTypes())
                        if (type.BaseType != null && type.BaseType.ToString() == "Applications.Sidebar.BaseTile")
                            TileType = type;
                    foreach (Attribute attr in tileAssembly.GetCustomAttributes(false))
                        if (attr.GetType().ToString() == "Applications.Sidebar.SidebarTileInfo")
                        {
                            Info = new TileLib.TileInfo(((SidebarTileInfo)attr).Title, false, false);
                            this.TitleTextBlock.Text = Info.Name;
                            if (System.IO.File.Exists(System.IO.Path.GetDirectoryName(this.File) + @"\Icon.png"))
                                this.TitleIcon.Source = new BitmapImage(new Uri(System.IO.Path.GetDirectoryName(this.File) + @"\Icon.png"));
                        }
                    break;
            }
            this.Unloaded += new RoutedEventHandler(Tile_Unloaded);
            this.SizeChanged += new SizeChangedEventHandler(Tile_SizeChanged);
            if (Info == null)
                hasErrors = true;
            ////////////////////////////

            try
            {
                switch (tileModelType)
                {
                    case ModelType.LongBar:
                        tileObject = (TileLib.BaseTile)TileType.InvokeMember(null, flags | BindingFlags.CreateInstance, null, null, null);
                        tileObject.CaptionChanged += new TileLib.BaseTile.CaptionChangedEventHandler(TileObject_CaptionChanged);
                        tileObject.IconChanged += new TileLib.BaseTile.IconChangedEventHandler(TileObject_IconChanged);
                        tileObject.ShowOptionsEvent += new TileLib.BaseTile.ShowOptionsEventHandler(TileObject_ShowOptionsEvent);
                        tileObject.ShowFlyoutEvent += new TileLib.BaseTile.ShowFlyoutEventHandler(TileObject_ShowFlyoutEvent);
                        tileObject.HeightChangedEvent += new TileLib.BaseTile.HeightChangedEventHandler(tileObject_HeightChangedEvent);
                        tileObject._path = Settings.Current.path;

                        control = tileObject.Load();

                        control.MouseLeftButtonDown += new MouseButtonEventHandler(TileContentGrid_MouseLeftButtonDown);
                        break;

                    case ModelType.KarlsSidebar:
                        tileKObject = (Applications.Sidebar.Tile)TileType.InvokeMember(null, flags | BindingFlags.CreateInstance, null, null, null);
                        control = tileKObject.SidebarContent;
                        Info = new TileLib.TileInfo(Info.Name, tileKObject.hasFlyout, tileKObject.hasConfigWindow);
                        break;
                }
            }
            catch (Exception ex)
            {
                if (Settings.Current.showErrors)
                    TaskDialogs.ErrorDialog.ShowDialog("An error occured while loading tile. Please send feedback.", String.Format("Error: {0}\nTile: {1}\nSee log for detailed info.", ex.Message, Info.Name), ex);
                if (!System.IO.Directory.Exists(Settings.Current.path + @"\Logs"))
                    System.IO.Directory.CreateDirectory(Settings.Current.path + @"\Logs");
                string logFile = string.Format(@"{0}\Logs\{1}.{2}.{3}.log", Settings.Current.path, DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year);
                try
                {
                    System.IO.File.AppendAllText(logFile, String.Format("{0}\r\n{1}\r\n--------------------------------------------------------------------------------------\r\n",
                      DateTime.UtcNow.ToString(), ex));
                }
                catch (Exception ex1)
                {
                    MessageBox.Show("Can't write to log. Reason: " + ex1.Message, null, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                hasErrors = true;
                return;
            }

            DoubleAnimation LoadHeightAnim = (DoubleAnimation)FindResource("LoadHeightAnim");
            DoubleAnimation LoadOpacityAnim = (DoubleAnimation)FindResource("LoadOpacityAnim");

            LoadHeightAnim.Completed += new EventHandler(LoadHeightAnim_Completed);
            LoadOpacityAnim.Completed += new EventHandler(LoadOpacityAnim_Completed);

            if (!double.IsNaN(height))
                LoadHeightAnim.To = height;
            else if (double.IsNaN(control.Height))
                LoadHeightAnim.To = 125;
            else if (!double.IsNaN(Header.Height) && (DockPanel.GetDock(Header) == Dock.Top || DockPanel.GetDock(Header) == Dock.Bottom))
                LoadHeightAnim.To = control.Height + this.Header.Height + 5;
            else
                LoadHeightAnim.To = control.Height + 5;

            MinimizedItem.IsChecked = false;

            TileContentGrid.Children.Clear();
            TileContentGrid.Children.Add(control);


            if (minimized)
            {
                normalHeight = height;
                this.MinimizedItem.IsChecked = true;
            }


            ChangeTheme(Settings.Current.theme);
            ChangeLocale(Settings.Current.locale);

            isLoaded = true;
            this.BeginAnimation(HeightProperty, LoadHeightAnim);
            TileContentGrid.BeginAnimation(OpacityProperty, LoadOpacityAnim);
        }

        void tileObject_HeightChangedEvent(double height)
        {
            if (!minimized)
            {
                if ((DockPanel.GetDock(Header) == Dock.Top || DockPanel.GetDock(Header) == Dock.Bottom) && Header.IsVisible)
                    this.Height = height + this.Header.Height + 5;
                else
                    this.Height = height;
            }
        }

        public void Unload()
        {
            if (tileObject != null)
            {
                tileObject.CaptionChanged -= new TileLib.BaseTile.CaptionChangedEventHandler(TileObject_CaptionChanged);
                tileObject.IconChanged -= new TileLib.BaseTile.IconChangedEventHandler(TileObject_IconChanged);
                tileObject.ShowOptionsEvent -= new TileLib.BaseTile.ShowOptionsEventHandler(TileObject_ShowOptionsEvent);
                tileObject.ShowFlyoutEvent -= new TileLib.BaseTile.ShowFlyoutEventHandler(TileObject_ShowFlyoutEvent);
                tileObject.HeightChangedEvent -= new TileLib.BaseTile.HeightChangedEventHandler(tileObject_HeightChangedEvent);
                tileObject.Unload();
                tileObject = null;
            }
            TileContentGrid.Children.Clear();
            this.BeginAnimation(HeightProperty, (DoubleAnimation)this.Resources["UnLoadHeightAnim"]);
            isLoaded = false;
        }

        private void Splitter_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture((IInputElement)sender);
        }

        private void Splitter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!MinimizedItem.IsChecked)
                Mouse.Capture(null);
        }

        private void Splitter_MouseMove(object sender, MouseEventArgs e)
        {
            if (!pinned)
            {
                if (Mouse.Captured == Splitter && e.GetPosition(this).Y > this.Header.ActualHeight + 1)
                    this.Height = e.GetPosition(this).Y;
            }
            else
            {
                DockPanel d = (DockPanel)((StackPanel)this.Parent).Parent;

                if (Mouse.Captured == Splitter && (((d.ActualHeight - e.GetPosition(d).Y) - (d.ActualHeight - this.TranslatePoint(new Point(0, 0), d).Y - this.Height)) > 30))
                    this.Height = (d.ActualHeight - e.GetPosition(d).Y) - (d.ActualHeight - this.TranslatePoint(new Point(0, 0), d).Y - this.Height);
            }
        }

        private void Tile_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (TileContentGrid.Children.Count > 0)
            {
                ((FrameworkElement)TileContentGrid.Children[0]).Height = this.TileContentGrid.ActualHeight;
                ((FrameworkElement)TileContentGrid.Children[0]).Width = this.TileContentGrid.ActualWidth;
            }
        }

        private void TileObject_CaptionChanged(string value)
        {
            this.TitleTextBlock.Text = value;
        }

        public void ChangeSide(AppBarSide side)
        {
            switch (side)
            {
                case AppBarSide.Left:
                    Splitter.FlowDirection = FlowDirection.RightToLeft;
                    break;
                case AppBarSide.Right:
                    Splitter.FlowDirection = FlowDirection.LeftToRight;
                    break;
            }
            if (tileObject != null)
                tileObject.ChangeSide((int)side);
        }

        public void ChangeLocale(string locale)
        {
            if (tileObject != null)
                tileObject.ChangeLocale(locale);
        }

        public void ChangeTheme(string theme)
        {
            Style style = (Style)this.TryFindResource("TileHeader");
            foreach (Setter setter in style.Setters)
            {
                if (setter.Property == VisibilityProperty)
                    this.Header.SetValue(VisibilityProperty, setter.Value);
            }

            style = (Style)this.TryFindResource("TileSplitterPanel");
            foreach (Setter setter in style.Setters)
            {
                if (setter.Property == DockPanel.DockProperty)
                    this.Splitter.SetValue(DockPanel.DockProperty, setter.Value);
            }

            if (tileObject != null)
                tileObject.ChangeTheme(theme);
            if (Header.Visibility == Visibility.Visible && !double.IsNaN(Header.Height) && (DockPanel.GetDock(Header) == Dock.Top || DockPanel.GetDock(Header) == Dock.Bottom))
                this.Height = TileContentGrid.ActualHeight + this.Header.Height + 5;
            else
                this.Height = TileContentGrid.ActualHeight + 4;
        }

        private void DockPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (tileModelType)
            {
                case ModelType.LongBar:
                    tileObject.ShowFlyout();
                    break;
                case ModelType.KarlsSidebar:
                    TileKObject_ShowFlyout();
                    break;
            }
        }

        private void TileContentGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            buttonDownPos = e.GetPosition(TileContentGrid);
            if (!Header.IsVisible && e.ClickCount > 1)
                switch (tileModelType)
                {
                    case ModelType.LongBar:
                        tileObject.ShowFlyout();
                        break;
                    case ModelType.KarlsSidebar:
                        TileKObject_ShowFlyout();
                        break;
                }
        }

        private void TileObject_IconChanged(BitmapImage value)
        {
            this.TitleIcon.Source = value;
        }

        private void TileObject_ShowFlyoutEvent()
        {
            if (this.Info.hasflyout && tileObject.FlyoutContent != null)
            {
                if (flyout != null && flyout.IsVisible)
                {
                    flyout.Activate();
                    return;
                }
                flyout = new FlyoutWindow(Info.Name);
                flyout.Left = this.PointToScreen(new Point(0, 0)).X;
                flyout.Top = this.PointToScreen(new Point(0, 0)).Y;
                System.Windows.Forms.Screen screen = Utils.GetScreenFromName(Settings.Current.screen);
                flyout.ContentGrid.Children.Add(tileObject.FlyoutContent);
                flyout.Show();
                if ((flyout.Top + flyout.Height) > screen.WorkingArea.Height)
                {
                    flyout.Top = screen.WorkingArea.Height - flyout.Height;
                }
            }
        }

        private void TileKObject_ShowFlyout()
        {
            if (this.Info.hasflyout && tileKObject.FlyoutContent != null)
            {
                if (flyout != null && flyout.IsVisible)
                {
                    flyout.Activate();
                    return;
                }
                flyout = new FlyoutWindow(Info.Name);
                flyout.Width = tileKObject.FlyoutContent.Width;
                flyout.Height = tileKObject.FlyoutContent.Height;
                flyout.Left = this.PointToScreen(new Point(0, 0)).X;
                flyout.Top = this.PointToScreen(new Point(0, 0)).Y;
                flyout.ContentGrid.Children.Add(tileKObject.FlyoutContent);
                flyout.Show();
            }
        }

        private void CustomizeItem_Click(object sender, RoutedEventArgs e)
        {
            switch (tileModelType)
            {
                case ModelType.LongBar:
                    tileObject.ShowOptions();
                    break;
                case ModelType.KarlsSidebar:
                    TileKObject_ShowOptions();
                    break;
            }
        }

        private void TileObject_ShowOptionsEvent()
        {
            if (this.Info.hasOptions && tileObject.OptionsContent != null)
            {
                if (options != null && options.IsVisible)
                {
                    options.Activate();
                    return;
                }
                options = new TileOptionsWindow();
                options.Width = tileObject.OptionsContent.Width;
                options.Height = tileObject.OptionsContent.Height + 5;
                options.ContentGrid.Children.Add(tileObject.OptionsContent);
                options.Show();
            }
        }

        private void TileKObject_ShowOptions()
        {
            if (this.Info.hasOptions && tileKObject.ConfigurationWindow != null)
            {
                Window configWindow = tileKObject.ConfigurationWindow;
                if (configWindow != null && configWindow.IsVisible)
                {
                    configWindow.Activate();
                    return;
                }

                configWindow.Closing += new CancelEventHandler(KTile_ConfigClosing);
                configWindow.Loaded += new RoutedEventHandler(KTile_ConfigLoaded);
                configWindow.ShowDialog();
            }
        }

        private void KTile_ConfigLoaded(object sender, RoutedEventArgs e)
        {
            tileKObject.OnConfigurationWindowOpened((Window)sender);
        }

        private void KTile_ConfigClosing(object sender, CancelEventArgs e)
        {
            tileKObject.OnConfigurationWindowClosing((Window)sender);
        }

        private void LoadHeightAnim_Completed(object sender, EventArgs e)
        {
            this.BeginAnimation(UserControl.HeightProperty, null);

            if (this.ActualHeight > 0 && !minimized)
            {
                this.Height = this.ActualHeight;
            }
            else
                this.Height = ((DoubleAnimation)FindResource("LoadHeightAnim")).To.GetValueOrDefault();
        }

        private void LoadOpacityAnim_Completed(object sender, EventArgs e)
        {
            this.BeginAnimation(UserControl.OpacityProperty, null);
            this.Opacity = 1;
        }

        private void Tile_Unloaded(object sender, RoutedEventArgs e)
        {
            if (tileObject != null)
                tileObject.Unload();
        }

        private void UnLoadHeightAnim_Completed(object sender, EventArgs e)
        {
            if (pinned)
            {
                ((StackPanel)this.Parent).Children.Remove(this);
                pinned = false;
                PinItem.IsChecked = false;
            }
            else
            {
                ((StackPanel)this.Parent).Children.Remove(this);
            }
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            Unload();
        }

        private void MinimizedItem_Checked(object sender, RoutedEventArgs e)
        {
            Splitter.IsEnabled = false;
            DoubleAnimation minimizeAnim = ((DoubleAnimation)this.Resources["MinimizeAnim"]);
            if (!minimized)
                normalHeight = this.Height;
            if (normalHeight > 0)
                this.Height = normalHeight;
            minimizeAnim.From = this.Height;
            minimizeAnim.To = Header.Height + 2;

            this.BeginAnimation(HeightProperty, minimizeAnim);
        }

        private static Visibility headerState;

        private void MinimizeAnim_Completed(object sender, EventArgs e)
        {
            this.BeginAnimation(HeightProperty, null);
            this.Height = ((DoubleAnimation)this.Resources["MinimizeAnim"]).To.GetValueOrDefault();
            if (tileObject != null)
            {
                tileObject.IsMinimized = true;
                tileObject.Minimized();
            }
            headerState = Header.Visibility;
            Header.Visibility = Visibility.Visible;
            this.minimized = true;
        }

        private void MinimizedItem_Unchecked(object sender, RoutedEventArgs e)
        {
            Splitter.IsEnabled = true;
            DoubleAnimation unMinimizeAnim = ((DoubleAnimation)this.Resources["UnMinimizeAnim"]);
            unMinimizeAnim.To = ((DoubleAnimation)this.Resources["MinimizeAnim"]).From;
            this.BeginAnimation(HeightProperty, unMinimizeAnim);
            Header.Visibility = headerState;
        }

        private void UnMinimizeAnim_Completed(object sender, EventArgs e)
        {
            this.BeginAnimation(HeightProperty, null);
            this.Height = ((DoubleAnimation)this.Resources["UnMinimizeAnim"]).To.GetValueOrDefault();
            if (tileObject != null)
            {
                tileObject.IsMinimized = false;
                tileObject.Unminimized();
            }
            this.minimized = false;
        }

        private void TileMenu_Opened(object sender, RoutedEventArgs e)
        {
            StackPanel p = ((StackPanel)this.Parent);
            if (p.Children.IndexOf(this) > 0)
                MoveUpItem.IsEnabled = true;
            else
                MoveUpItem.IsEnabled = false;
            if (p.Children.IndexOf(this) < p.Children.Count - 1)
                MoveDownItem.IsEnabled = true;
            else
                MoveDownItem.IsEnabled = false;
            if (Info.hasOptions)
                CustomizeItem.IsEnabled = true;
            else
                CustomizeItem.IsEnabled = false;
        }

        private void MoveUpItem_Click(object sender, RoutedEventArgs e)
        {
            StackPanel p = ((StackPanel)this.Parent);
            int index = p.Children.IndexOf(this);
            p.Children.Remove(this);
            p.Children.Insert(index - 1, this);
        }

        private void MoveDownItem_Click(object sender, RoutedEventArgs e)
        {
            StackPanel p = ((StackPanel)this.Parent);
            int index = p.Children.IndexOf(this);
            p.Children.Remove(this);
            p.Children.Insert(index + 1, this);
        }

        private void PinItem_Checked(object sender, RoutedEventArgs e)
        {
            if (!pinned)
            {
                Tile t = this;
                PinTile(ref t);
            }

        }

        private void PinItem_Unchecked(object sender, RoutedEventArgs e)
        {
            if (pinned)
            {
                Tile t = this;
                UnpinTile(ref t, true);
            }
        }

        public static void PinTile(ref Tile tile)
        {
            tile.pinned = true;

            StackPanel pinGrid = (StackPanel)((DockPanel)((StackPanel)tile.Parent).Parent).Children[0];

            StackPanel p = ((StackPanel)tile.Parent);
            p.Children.Remove(tile);

            tile.Header.Visibility = System.Windows.Visibility.Collapsed;
            DockPanel.SetDock(tile.Splitter, Dock.Top);

            pinGrid.Children.Add(tile);
        }

        public static void UnpinTile(ref Tile tile, bool MoveToMain)
        {
            tile.pinned = false;

            StackPanel p = (StackPanel)((DockPanel)((StackPanel)tile.Parent).Parent).Children[1];

            StackPanel pinGrid = ((StackPanel)tile.Parent);

            pinGrid.Children.Remove(tile);

            Style style = (Style)tile.TryFindResource("TileHeader");
            foreach (Setter setter in style.Setters)
            {
                if (setter.Property == VisibilityProperty)
                    tile.Header.SetValue(VisibilityProperty, setter.Value);
            }

            style = (Style)tile.TryFindResource("TileSplitterPanel");
            foreach (Setter setter in style.Setters)
            {
                if (setter.Property == DockPanel.DockProperty)
                    tile.Splitter.SetValue(DockPanel.DockProperty, setter.Value);
            }

            if (MoveToMain)
            {
                p.Children.Insert(0, tile);
            }
        }

        private Point buttonDownPos;
        private bool mousePressed = false;

        private TileDragWindow dragWindow;

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            //This is stupid but I don't see any other way to resolve issue with TextBox and tile dragging
            if (e.LeftButton == MouseButtonState.Pressed && !mousePressed)
            {
                buttonDownPos = e.GetPosition(TileContentGrid);
                mousePressed = true;
            }

            if (e.LeftButton == MouseButtonState.Released && mousePressed)
            {
                mousePressed = false;
            }

            if (e.LeftButton == MouseButtonState.Pressed &&
                (dragWindow == null || !dragWindow.IsLoaded) &&
                buttonDownPos.X > 0 && buttonDownPos.Y > 0)
            {
                double tileX = e.GetPosition(TileContentGrid).X;
                double tileY = e.GetPosition(TileContentGrid).Y;
                if ((tileX > buttonDownPos.X + 20 || tileX < buttonDownPos.X - 20) ||
                    (tileY > buttonDownPos.Y + 20 || tileY < buttonDownPos.Y - 20))
                {
                    dragWindow = new TileDragWindow((StackPanel)Parent, this);
                    var dpiScale = VisualTreeHelper.GetDpi(dragWindow);
                    dragWindow.Left = PointToScreen(new Point(0, 0)).X / dpiScale.DpiScaleX;
                    dragWindow.Top = PointToScreen(new Point(0, 0)).Y / dpiScale.DpiScaleY;
                    dragWindow.Width = ActualWidth + 20;
                    dragWindow.Height = ActualHeight + 25;
                    ((StackPanel)Parent).Children.Remove(this);
                    dragWindow.Show();
                }
            }
        }
    }
}