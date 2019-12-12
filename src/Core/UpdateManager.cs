using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Windows;
using System.IO.Compression;
using System.Reflection;
using System.Xml.Serialization;

namespace Sidebar.Core
{
    public class UpdateManager
    {
        public static UpdateInfo CheckForUpdates()
        {
            UpdateInfo result = new UpdateInfo();

            using (WebClient client = new WebClient())
            {
                try
                {
                    string updateInfoFile = client.DownloadString(ServiceUrls.UpdateInfo);
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
