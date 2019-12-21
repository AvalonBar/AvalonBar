using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Sidebar.Core
{
    public static class Services
    {
#if DEBUG
        public static readonly string LandingPageUrl = "http://avalonbar.github.io/";
#else
        public static readonly string LandingPage = "https://avalonbar.github.io/";
#endif
        public static readonly string UpdateUrl = LandingPageUrl + "services/UpdateInfo.xml";
        public static readonly string TileInfoUrl = LandingPageUrl + "services/TileList.xml";
        public static readonly string LanguagesUrl = LandingPageUrl + "languages";
        public static readonly string ThemesUrl = LandingPageUrl + "themes";
        public static readonly string IssuesUrl = LandingPageUrl + "issue-help";

        public static UpdateInfo CheckForUpdates()
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    string updateInfoFile = client.DownloadString(UpdateUrl);
                    XmlSerializer serializer = new XmlSerializer(typeof(UpdateInfo));
                    UpdateInfo updateInfo = (UpdateInfo)serializer.Deserialize(new StringReader(updateInfoFile));

                    if (updateInfo.Version != VersionInfo.Core)
                    {
                        return updateInfo;
                    }
                }
                catch
                {
                    // Ignore exceptions thrown when downloading the update info file
                }
            }

            return UpdateInfo.Empty;
        }

        public static void UpdateFiles(string path)
        {
            string file = path + "\\Updates\\Update";
            using (ZipArchive archive = ZipFile.OpenRead(file))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (!entry.Name.Contains("/"))
                        path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                    if (File.Exists(path + "\\" + entry.Name))
                        File.Move(path + "\\" + entry.Name, path + "\\" + entry.Name + ".old");

                    entry.ExtractToFile(string.Format(@"{0}\{1}", path, entry.Name));
                }
            }
        }
    }
}
