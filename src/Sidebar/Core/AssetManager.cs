using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Xml;
using System.Collections.ObjectModel;

namespace Sidebar
{
    public enum AssetKind
    {
        Tile,
        Theme,
        Locale
    }

    public class AssetManager
    {
        private static ResourceDictionary _currentLocale;
        private static ResourceDictionary _currentTheme;

        public static ResourceDictionary CurrentLocale
        {
            get { return _currentLocale; }
            private set
            {
                Application.Current.Resources.MergedDictionaries.Remove(_currentLocale);
                _currentLocale = value;
            }
        }

        public static ResourceDictionary CurrentTheme
        {
            get { return _currentTheme; }
            private set
            {
                Application.Current.Resources.MergedDictionaries.Remove(_currentTheme);
                _currentTheme = value;
            }
        }

        public static void Unpack(string path, string file)
        {
            FileInfo info = new FileInfo(file);
            string name = info.Name.Substring(0, info.Name.LastIndexOf('.'));
            string destinationPath = Path.Combine(path, "Library", name);
            // TODO: Handle situations wherein the tile is already installed
            //       and we want to update the said tile.
            ZipFile.ExtractToDirectory(file, destinationPath);
        }

        private static string GetAssetName(AssetKind kind)
        {
            switch (kind)
            {
                case AssetKind.Tile:
                    throw new NotImplementedException();
                case AssetKind.Theme:
                    return "Themes";
                case AssetKind.Locale:
                    return "Localization";
                default:
                    throw new ArgumentException();
            }
        }

        private static string GetAssetExtension(AssetKind kind)
        {
            switch (kind)
            {
                case AssetKind.Tile:
                    throw new NotImplementedException();
                case AssetKind.Theme:
                    return ".theme.xaml";
                case AssetKind.Locale:
                    return ".locale.xaml";
                default:
                    throw new ArgumentException();
            }
        }

        private static string GetAssetDefault(AssetKind kind)
        {
            switch (kind)
            {
                case AssetKind.Tile:
                    throw new NotImplementedException();
                case AssetKind.Theme:
                    return "Aero";
                case AssetKind.Locale:
                    return "English";
                default:
                    throw new ArgumentException();
            }
        }

        public static bool Install(AssetKind kind, string file)
        {
            string assetName = GetAssetName(kind);

            try
            {
                string parentPath = Path.Combine(
                    Settings.Current.path, assetName);
                if (!Directory.Exists(assetName))
                {
                    Directory.CreateDirectory(assetName);
                }

                FileInfo info = new FileInfo(file);
                string assetPath = Path.Combine(parentPath, info.Name);
                File.Copy(file, assetPath, true);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static Collection<string> FindAll(AssetKind kind)
        {
            string assetName = GetAssetName(kind);
            string assetExtension = GetAssetExtension(kind);

            var assets = new Collection<string>();
            string parentPath = Path.Combine(
                Settings.Current.path, assetName);
            if (!Directory.Exists(parentPath))
            {
                return assets;
            }

            foreach (string file in Directory.GetFiles(parentPath))
            {
                if (file.EndsWith(assetExtension))
                {
                    string name = Path.GetFileName(file);
                    assets.Add(name.Substring(0, name.IndexOf(assetExtension)));
                }
            }

            return assets;
        }

        public static void Load(AssetKind kind, string name)
        {
            ResourceDictionary assetTarget = new ResourceDictionary();
            string assetPath = Path.Combine(
                Settings.Current.path,
                GetAssetName(kind),
                name + GetAssetExtension(kind));
            if (File.Exists(assetPath))
            {
                assetTarget.Source = new Uri(assetPath);
            }
            else
            {
                MessageBox.Show(
                    $"{name} {kind} asset not found!",
                    null,
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);

                string assetDefault = GetAssetDefault(kind);
                if (name != assetDefault)
                {
                    Load(kind, assetDefault);
                    return;
                }
            }

            switch (kind)
            {
                case AssetKind.Theme:
                    CurrentTheme = assetTarget;
                    break;
                case AssetKind.Locale:
                    CurrentLocale = assetTarget;
                    break;
                case AssetKind.Tile:
                default:
                    break;
            }
            Application.Current.Resources.MergedDictionaries.Add(assetTarget);
        }

        public static object GetThemeParameter(string themeName, string paramType, string paramName)
        {
            string assetPath = Path.Combine(
                Settings.Current.path,
                GetAssetName(AssetKind.Theme),
                themeName + GetAssetExtension(AssetKind.Theme));
            if (File.Exists(assetPath))
            {
                XmlTextReader reader = new XmlTextReader(assetPath);
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element ||
                        reader.Name.ToLower() != "s:" + paramType.ToLower())
                    {
                        continue;
                    }
                    reader.MoveToAttribute("x:Key");
                    if (reader.Value != paramName)
                    {
                        continue;
                    }
                    reader.MoveToContent();
                    object obj = reader.ReadElementContentAsObject();
                    reader.Close();
                    return obj;
                }
            }
            return null;
        }
    }
}
