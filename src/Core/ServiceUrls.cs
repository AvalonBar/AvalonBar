using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sidebar.Core
{
    public static class ServiceUrls
    {
#if DEBUG
        public static readonly string LandingPage = "http://franklindm.github.io/AvalonBar/";
#else
        public static readonly string LandingPage = "https://franklindm.github.io/AvalonBar/";
#endif
        public static readonly string UpdateInfo = LandingPage + "services/UpdateInfo.xml";
        public static readonly string TileInfo = LandingPage + "services/TileList.xml";
        public static readonly string UpdatePackage = LandingPage + "services/UpdatePackage.zip";
        public static readonly string Languages = LandingPage + "languages";
        public static readonly string Themes = LandingPage + "themes";
    }
}
