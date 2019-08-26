﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Sidebar.Core
{
    [Serializable]
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
            _path = "\\";
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
        public string[] tiles;
        public string[] heights;
        public string[] pinnedTiles;
        public bool showErrors;
        public bool overlapTaskbar;
        public string screen;
        private string _path;
        public string path
        {
            get
            {
                if (_path == "\\" || _path == null)
                {
                    return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }
                return _path;
            }
            set
            {
                _path = value;
            }
        }
        public bool enableUpdates;
        public bool debug;
        public string tileToDebug;
    }
}