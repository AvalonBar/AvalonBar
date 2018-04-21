using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows;

namespace Slate.Localization
{
	public class LocaleManager
	{
		public static string[] GetLocales(string path)
		{
			List<string> locales = new List<string>();
			if (Directory.Exists(path + @"\Localization"))
				foreach (string file in Directory.GetFiles(path + @"\Localization"))
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
