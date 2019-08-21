using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Windows;
using ICSharpCode.SharpZipLib.Zip;
using System.Reflection;

namespace Sidebar.Core
{
    public class UpdatesManager
    {
        public struct UpdateInfo
        {
            public string Build;
            public string Description;
        }

        public static UpdateInfo CheckForUpdates(int build)
        {
            UpdateInfo result = new UpdateInfo();

            try
            {
                WebClient client = new WebClient();
                string[] updateInfo = client.DownloadString("https://sourceforge.net/projects/longbar/files/Debug/LongBar%202.1/Updates/Update.info/download").Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (Convert.ToInt32(updateInfo[0]) > build)
                {
                    result.Build = updateInfo[0];
                    for (int i = 1; i < updateInfo.Length; i++)
                    {
                        result.Description += updateInfo[i] + "\n\r";
                    }
                }
            }
            catch { }
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
                                File.Move(path + "\\" + entry.Name,path + "\\" + entry.Name + ".old");

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
                            if (!Directory.Exists(string.Format(@"{0}\{1}", path,  entry.Name)))
                                Directory.CreateDirectory(string.Format(@"{0}\{1}", path,  entry.Name));
                    }
                    zipInStream.Close();
                }
                fileStreamIn.Close();
            }
        }
    }
}
