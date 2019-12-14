using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Sidebar.Core
{
    public class Settings
    {
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
        public List<TileState> tiles;
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
    }
}
