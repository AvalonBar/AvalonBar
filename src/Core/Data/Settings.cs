using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace Slate.Options
{
    [XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class Settings
    {
        public Links Links { get; set; }
        public Experimental Experimental { get; set; }
        public Program Program { get; set; }
    }
    [XmlTypeAttribute(AnonymousType = true)]
    public class Links
    {
        public string ProjectURL { get; set; }
        public string ThemesURL { get; set; }
        public string LocalesURL { get; set; }
        public string BugTrackerURL { get; set; }
    }
    [XmlTypeAttribute(AnonymousType = true)]
    public class Experimental
    {
        public bool AllowAutomaticRestart { get; set; }
    }
    [XmlTypeAttribute(AnonymousType = true)]
    public class Program
    {
        public bool AutoStart { get; set; }
        public int Side { get; set; }
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
        public string Tiles { get; set; }
        public string Heights { get; set; }
        public string PinnedTiles { get; set; }
        public bool EnableSnowFall { get; set; }
        public bool Debug { get; set; }
        public string tileToDebug { get; set; }
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
        public static Settings DefaultSettings {
           get
            {
               var s = new Settings();
               var l = new Links();
               var e = new Experimental();
               var p = new Program();
               #region Default values for settings
               // def. values for links
               l.BugTrackerURL = "https://github.com/FranklinDM/AvalonBar/issues";
               l.LocalesURL = "https://github.com/FranklinDM/AvalonBar/blob/gh-pages/Locales.md";
               l.ProjectURL = "https://franklindm.github.io/AvalonBar";
               l.ThemesURL = "https://github.com/FranklinDM/AvalonBar/blob/gh-pages/Themes.md";
               // def. values for experimental
               e.AllowAutomaticRestart = true;
               // def. values for program
               p.AutoStart = false;
               p.Side = 2;
               p.Theme = "Slate";
               p.Language = "Language";
               p.Width = 150;
               p.TopMost = true;
               p.EnableGlass = true;
               p.EnableShadow = true;
               p.Locked = true;
               p.OverlapTaskbar = false;
               p.ShowErrors = true;
               p.Screen = "Primary";
               p.Path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
               p.EnableUpdates = true;
               p.Tiles = "";
               p.Heights = "";
               p.PinnedTiles = "";
               p.EnableSnowFall = false;
               p.Debug = false;
               // def. values for root settings
               s.Experimental = e;
               s.Links = l;
               s.Program = p;
               #endregion
               return s;
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
    }
}

