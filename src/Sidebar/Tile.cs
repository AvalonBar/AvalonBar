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
            }
        }

        public bool HasErrors { get; protected set; }
        public Assembly SourceAssembly { get; protected set; }
        public bool IsLoaded { get; protected set; }
        public TileLib.TileInfo Info { get; set; }
        public Type TileType { get; protected set; }

        public bool IsRetrophaseTile
        {
            get
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
        }

        public void Load()
        {
            SourceAssembly = Assembly.LoadFrom(path);
            if (IsRetrophaseTile)
            {
                foreach (Type type in SourceAssembly.GetTypes())
                {
                    if (type.BaseType != null && type.BaseType.ToString() == "Applications.Sidebar.BaseTile")
                    {
                        TileType = type;
                    }
                }
                foreach (Attribute attr in SourceAssembly.GetCustomAttributes(false))
                {
                    if (attr.GetType().ToString() == "Applications.Sidebar.SidebarTileInfo")
                    {
                        Info = new TileLib.TileInfo(((Applications.Sidebar.SidebarTileInfo)attr).Title, false, false);
                    }
                }
                return;
            }

            foreach (Type type in SourceAssembly.GetTypes())
            {
                if (type.BaseType == typeof(TileLib.BaseTile))
                    TileType = type;
            }
            foreach (Attribute attr in SourceAssembly.GetCustomAttributes(false))
            {
                if (attr.GetType() == typeof(TileLib.TileInfo))
                {
                    Info = (TileLib.TileInfo)attr;
                }
            }

            IsLoaded = true;
        }
    }
}
