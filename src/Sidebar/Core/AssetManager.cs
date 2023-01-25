using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Xml;

namespace Sidebar
{
    public class AssetManager
    {
        public static void Unpack(string path, string file)
        {
            FileInfo info = new FileInfo(file);
            string name = info.Name.Substring(0, info.Name.LastIndexOf('.'));
            string destinationPath = Path.Combine(path, "Library", name);
            // TODO: Handle situations wherein the tile is already installed
            //       and we want to update the said tile.
            ZipFile.ExtractToDirectory(file, destinationPath);
        }

        public static string[] GetLocales(string path)
        {
            List<string> locales = new List<string>();
            if (Directory.Exists(path + @"\Localization"))
            {
                foreach (string file in Directory.GetFiles(path + @"\Localization"))
                {
                    if (file.EndsWith(@".locale.xaml"))
                    {
                        string name = Path.GetFileName(file);
                        locales.Add(name.Substring(0, name.IndexOf(@".locale.xaml")));
                    }
                }
            }
            return locales.ToArray();
        }

        public static void LoadLocale(string path, string name)
        {
            ResourceDictionary locale = new ResourceDictionary();
            if (File.Exists(string.Format(@"{0}\Localization\{1}.locale.xaml", path, name)))
            {
                locale.Source = new Uri(string.Format(@"{0}\Localization\{1}.locale.xaml", path, name));
            }
            else
            {
                MessageBox.Show(name + " localization file not found!", null, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                if (name == "English")
                {
                    return;
                }
                else
                {
                    LoadLocale(path, "English");
                }
                return;
            }
            if (Application.Current.Resources.MergedDictionaries.Count > 1)
            {
                Application.Current.Resources.MergedDictionaries.RemoveAt(1);
                Application.Current.Resources.MergedDictionaries.Insert(1, locale);
            }
            else if (Application.Current.Resources.MergedDictionaries.Count == 1)
            {
                Application.Current.Resources.MergedDictionaries.Add(locale);
            }
            else if (Application.Current.Resources.MergedDictionaries.Count == 0)
            {
                Application.Current.Resources.MergedDictionaries.Add(locale);
            }
        }

        public static bool InstallLocale(string path, string file)
        {
            try
            {
                if (!Directory.Exists(path + @"\Localization"))
                {
                    Directory.CreateDirectory(path + @"\Localization");
                }
                FileInfo info = new FileInfo(file);
                File.Copy(file, path + @"\Localization\" + info.Name, true);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static string[] GetThemes(string path)
        {
            List<string> themes = new List<string>();
            if (Directory.Exists(path + @"\Themes"))
            {
                foreach (string file in Directory.GetFiles(path + @"\Themes"))
                {
                    if (file.EndsWith(@"theme.xaml"))
                    {
                        string name = Path.GetFileName(file);
                        themes.Add(name.Substring(0, name.IndexOf(@"theme.xaml") - 1));
                    }
                }
            }
            return themes.ToArray();
        }

        public static void LoadTheme(string path, string name)
        {
            ResourceDictionary theme = new ResourceDictionary();
            if (File.Exists(string.Format(path + @"\Themes\{0}.theme.xaml", name)))
            {
                theme.Source = new Uri(string.Format(path + @"\Themes\{0}.theme.xaml", name));
            }
            else
            {
                MessageBox.Show("Theme " + name + " not found!", null, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                if (name == "Aero")
                {
                    return;
                }
                else
                {
                    LoadTheme(path, "Aero");
                }

                return;
            }
            if (Application.Current.Resources.MergedDictionaries.Count > 0)
            {
                Application.Current.Resources.MergedDictionaries.RemoveAt(0);
                Application.Current.Resources.MergedDictionaries.Insert(0, theme);
            }
            else
            {
                Application.Current.Resources.MergedDictionaries.Add(theme);
            }
        }

        public static object GetThemeParameter(string path, string themeName, string paramType, string paramName)
        {
            if (File.Exists(string.Format(path + @"\Themes\{0}.theme.xaml", themeName)))
            {
                XmlTextReader reader = new XmlTextReader(string.Format(path + @"\Themes\{0}.theme.xaml", themeName));
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
            {
                return null;
            }
        }

        public static bool InstallTheme(string path, string file)
        {
            try
            {
                if (!Directory.Exists(path + @"\Themes"))
                {
                    Directory.CreateDirectory(path + @"\Themes");
                }

                FileInfo info = new FileInfo(file);
                File.Copy(file, path + @"\Themes\" + info.Name, true);
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
