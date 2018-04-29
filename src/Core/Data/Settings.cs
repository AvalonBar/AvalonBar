using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using Slate.General;

namespace Slate.Options
{
	[XmlRootAttribute(Namespace = "", IsNullable = false)]
	public class Settings
	{
		public Links Links { get; set; }
		public Program Program { get; set; }
	}

	[XmlTypeAttribute(AnonymousType = true)]
	public class Links
	{
		public string ProjectURL { get; set; }
		public string ThemesURL { get; set; }
		public string LocalesURL { get; set; }
		public string BugTrackerURL { get; set; }
		public string UpdatesURL { get; set; }
	}

	[XmlTypeAttribute(AnonymousType = true)]
	public class Program
	{
		public bool AutoStart { get; set; }
		public Sidebar.Side Side { get; set; }
		public string Theme { get; set; }
		public string Language { get; set; }
		public int Width { get; set; }
		public bool TopMost { get; set; }
		public bool EnableGlass { get; set; }
		public bool EnableShadow { get; set; }
		public bool Locked { get; set; }
		public bool OverlapTaskbar { get; set; }
		public bool ShowErrors { get; set; }
		public string Screen { get; set; }
		public string Path { get; set; }
		public bool EnableUpdates { get; set; }
		public string[] Tiles { get; set; }
		public string[] Heights { get; set; }
		public string[] PinnedTiles { get; set; }
		public bool EnableSnowFall { get; set; }
	}

	public class StartupFlags
	{
		public bool Debug { get; set; }
		public string TileToDebug { get; set; }
	}

	public class SettingsManager
	{
		public static Settings Load(string settFile)
		{
			try
			{
				using (StreamReader reader = new StreamReader(settFile))
				{
					var deserializer = new XmlSerializer(typeof(Settings));
					return (Settings)deserializer.Deserialize(reader);
				}
			}
			catch (IOException)
			{
				return DefaultSettings;
			}
		}

		public static void Save<T>(T toSerialize, string settFile)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());
			using (TextWriter textWriter = new StreamWriter(settFile))
			{
				xmlSerializer.Serialize(textWriter, toSerialize);
			}
		}

		public static Settings UserSettings
		{
			get
			{
				return Load("Settings.xml");
			}
		}

		public static Settings DefaultSettings
		{
			get
			{
				Settings s = new Settings();
				#region Default values for settings
				// def. values for links
				s.Links = new Links()
				{
					BugTrackerURL = "https://github.com/FranklinDM/AvalonBar/issues",
					LocalesURL = "http://cid-820d4d5cef8566bf.skydrive.live.com/browse.aspx/LongBar%20Project/Localization%202.0",  //"https://github.com/FranklinDM/AvalonBar/blob/gh-pages/Locales.md";
					ProjectURL = "https://franklindm.github.io/AvalonBar",
					ThemesURL = "http://cid-820d4d5cef8566bf.skydrive.live.com/browse.aspx/LongBar%20Project/Themes%202.0", //"https://github.com/FranklinDM/AvalonBar/blob/gh-pages/Themes.md";
					UpdatesURL = "https://sourceforge.net/projects/longbar/files/Debug/LongBar%202.1/Updates/Update.info/download"
				};
				// def. values for program
				s.Program = new Program()
				{
					AutoStart = false,
					Side = Sidebar.Side.Right,
					Theme = "Slate",
					Language = "English",
					Width = 150,
					TopMost = true,
					EnableGlass = true,
					EnableShadow = true,
					Locked = true,
					OverlapTaskbar = false,
					ShowErrors = true,
					Screen = "Primary",
					Path = AppDomain.CurrentDomain.BaseDirectory,
					EnableUpdates = false, //true
					Tiles = new string[0],
					Heights = new string[0],
					PinnedTiles = new string[0],
					EnableSnowFall = false
				};
				#endregion
				return s;
			}
		}
	}
}

