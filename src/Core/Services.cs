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
        public static readonly string LandingPage = "http://avalonbar.github.io/";
#else
        public static readonly string LandingPage = "https://avalonbar.github.io/";
#endif
        public static readonly string UpdateInfo = LandingPage + "services/UpdateInfo.xml";
        public static readonly string TileInfo = LandingPage + "services/TileList.xml";
        public static readonly string UpdatePackage = LandingPage + "services/UpdatePackage.zip";
        public static readonly string Languages = LandingPage + "languages";
        public static readonly string Themes = LandingPage + "themes";
        public static readonly string Issues = LandingPage + "issue-help";

        public static UpdateInfo CheckForUpdates()
        {
            UpdateInfo result = new UpdateInfo();

            using (WebClient client = new WebClient())
            {
                try
                {
                    string updateInfoFile = client.DownloadString(Services.UpdateInfo);
                    XmlSerializer serializer = new XmlSerializer(typeof(UpdateInfo));
                    UpdateInfo updateInfo = (UpdateInfo)serializer.Deserialize(new StringReader(updateInfoFile));

                    if (updateInfo.Version != VersionInfo.Core)
                    {
                        result = updateInfo;
                    }
                }
                catch
                {
                    // Ignore exceptions thrown when downloading the update info file
                }
            }

            return result;
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
