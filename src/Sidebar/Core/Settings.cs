using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Sidebar
{
    public class Settings
    {
        private const string DefaultFileName = "Settings.xml";

        public Settings()
        {
            side = AppBarSide.Right;
            theme = "Slate";
            locale = "English";
            width = 150;
            topMost = false;
            enableGlass = true;
            enableShadow = true;
            locked = false;
            overlapTaskbar = false;
            showErrors = true;
            screen = "Primary";
            enableUpdates = true;
        }

        public bool startup;
        public AppBarSide side;
        public string theme;
        public string locale;
        public int width;
        public bool topMost;
        public bool enableGlass;
        public bool enableShadow;
        public bool locked;
        public TileState[] tiles;
        public TileState[] pinnedTiles;
        public bool showErrors;
        public bool overlapTaskbar;
        public string screen;
        [XmlIgnore]
        public string path
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }
        public bool enableUpdates;
        public bool debug;
        public string tileToDebug;

        private static Settings _current = null;
        public static Settings Current
        {
            get
            {
                if (_current == null)
                {
                    _current = Read();
                }
                return _current;
            }
        }

        public static Settings Read()
        {
            var settings = new Settings();
            if (File.Exists(DefaultFileName))
            {
                using (StreamReader reader = new StreamReader(DefaultFileName))
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(Settings));
                    settings = (Settings)deserializer.Deserialize(reader);
                }
            }
            return settings;
        }

        public void Save(SidebarWindow sidebar)
        {
            width = (int)sidebar.Width;
            tiles = sidebar.GetAllTileStates(false);
            pinnedTiles = sidebar.GetAllTileStates(true);

            XmlSerializer xmlSerializer = new XmlSerializer(GetType());
            using (TextWriter textWriter = new StreamWriter(DefaultFileName))
            {
                xmlSerializer.Serialize(textWriter, this);
            }
        }
    }
}
