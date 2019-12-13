using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Sidebar
{
    public class Tile
    {
        public Tile(string path)
        {
            if (File.Exists(path))
            {
                Path = path;
            }
        }

        private string path;
        public string Path
        {
            get { return path; }
            protected set
            {
                if (!File.Exists(value))
                {
                    throw new FileNotFoundException();
                }
                path = value;
                SourceAssembly = Assembly.LoadFrom(path);
            }
        }

        public bool HasErrors { get; protected set; }
        public Assembly SourceAssembly { get; protected set; }
        public bool IsLoaded { get; protected set; }

        private bool IsRetrophaseTile()
        {
            try
            {
                for (int i = 0; i < SourceAssembly.GetExportedTypes().Length; i++)
                    if (SourceAssembly.GetExportedTypes()[i].BaseType != null)
                    {
                        if (SourceAssembly.GetExportedTypes()[i].BaseType == typeof(TileLib.BaseTile))
                            return false;
                        if (SourceAssembly.GetExportedTypes()[i].BaseType.ToString() == "Applications.Sidebar.BaseTile")
                            return true;
                    }
            }
            catch (Exception ex)
            {
                // TODO: Logging should not be handled individually
                if (App.Settings.showErrors)
                    MessageBox.Show(string.Format("The tile {0} is incompatible to current version of application. Please contact the tile's developers" +
                "\nSee log for detailed information.", System.IO.Path.GetFileName(Path)), null, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                if (!System.IO.Directory.Exists(App.Settings.path + @"\Logs"))
                    System.IO.Directory.CreateDirectory(App.Settings.path + @"\Logs");
                string logFile = string.Format(@"{0}\Logs\{1}.{2}.{3}.log", App.Settings.path, DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year);
                try
                {
                    System.IO.File.AppendAllText(logFile, String.Format("{0}\r\n{1}\r\n--------------------------------------------------------------------------------------\r\n",
                      DateTime.UtcNow.ToString(), ex));
                }
                catch (Exception ex1)
                {
                    MessageBox.Show("Can't write to log. Reason: " + ex1.Message, null, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                HasErrors = true;
            }
            return false;
        }

        public void Load(double height)
        {
            ////////////////////////////////////
            tileModelType = GetTileModelType(tileAssembly);
            switch (tileModelType)
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
                        tileObject._path = App.Settings.path;

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
                if (App.Settings.showErrors)
                    TaskDialogs.ErrorDialog.ShowDialog("An error occured while loading tile. Please send feedback.", String.Format("Error: {0}\nTile: {1}\nSee log for detailed info.", ex.Message, Info.Name), ex);
                if (!System.IO.Directory.Exists(App.Settings.path + @"\Logs"))
                    System.IO.Directory.CreateDirectory(App.Settings.path + @"\Logs");
                string logFile = string.Format(@"{0}\Logs\{1}.{2}.{3}.log", App.Settings.path, DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year);
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
        }
    }
}
