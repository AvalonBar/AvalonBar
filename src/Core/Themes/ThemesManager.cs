using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows;
using System.Xml;

namespace Slate.Themes
{
    public class ThemesManager
    {
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
        
        // Should only be used at startup as it could crash the application
		// when other resource dictionaries are already loaded.        
        public static void LoadUITheme(string ThemeURI)
        {
        	Uri uriToTheme = new Uri(ThemeURI, UriKind.Relative);
			object theme = Application.LoadComponent(uriToTheme);
			Application.Current.Resources = (ResourceDictionary)theme; 
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
