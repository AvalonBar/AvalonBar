using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Windows;
using ICSharpCode.SharpZipLib.Zip;
using System.Reflection;
using System.Xml.Serialization;

namespace Sidebar.Core
{
    public class UpdateManager
    {
        private const string UpdateInfoUrl = "https://franklindm.github.io/AvalonBar/services/UpdateInfo.xml";

        public static UpdateInfo CheckForUpdates()
        {
            UpdateInfo result = new UpdateInfo();

            using (WebClient client = new WebClient())
            {
                try
                {
                    string updateInfoFile = client.DownloadString(UpdateInfoUrl);
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
            using (FileStream fileStreamIn = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                using (ZipInputStream zipInStream = new ZipInputStream(fileStreamIn))
                {
                    ZipEntry entry;
                    FileInfo info = new FileInfo(file);
                    while (true)
                    {
                        entry = zipInStream.GetNextEntry();
                        if (entry == null)
                            break;
                        if (!entry.IsDirectory)
                        {
                            if (!entry.Name.Contains("/"))
                                path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                            if (File.Exists(path + "\\" + entry.Name))
                                File.Move(path + "\\" + entry.Name, path + "\\" + entry.Name + ".old");

                            using (FileStream fileStreamOut = new FileStream(string.Format(@"{0}\{1}", path, entry.Name), FileMode.Create, FileAccess.Write))
                            {
                                int size;
                                byte[] buffer = new byte[1024];
                                do
                                {
                                    size = zipInStream.Read(buffer, 0, buffer.Length);
                                    fileStreamOut.Write(buffer, 0, size);
                                } while (size > 0);
                                fileStreamOut.Close();
                            }
                        }
                        else
                            if (!Directory.Exists(string.Format(@"{0}\{1}", path, entry.Name)))
                                Directory.CreateDirectory(string.Format(@"{0}\{1}", path, entry.Name));
                    }
                    zipInStream.Close();
                }
                fileStreamIn.Close();
            }
        }
    }
}
