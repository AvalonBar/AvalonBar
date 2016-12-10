using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Windows;
using ICSharpCode.SharpZipLib.Zip;
using System.Reflection;

namespace Slate.Updates
{
    public class UpdatesManager
    {
        public struct UpdateInfo
        {
            public string Build;
            public string Description;
            //public string Link;
        }

        public static UpdateInfo CheckForUpdates(int build)
        {
            UpdateInfo result = new UpdateInfo();

            try
            {
                WebRequest request = WebRequest.Create("http://cid-820d4d5cef8566bf.skydrive.live.com/self.aspx/LongBar%20Project/Updates%202.0/Update.info");
                WebResponse response = request.GetResponse();

                StreamReader reader = new StreamReader(response.GetResponseStream());
                string line = "";

                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();

                    if (line.Contains(@"Update.info\x3fdownload\x26psid\x3d1', downloadUrl:"))
                    {
                        reader.Close();
                        response.Close();

                        line = line.Substring(line.IndexOf(@"Update.info\x3fdownload\x26psid\x3d1', downloadUrl:") + (@"Update.info\x3fdownload\x26psid\x3d1', downloadUrl:").Length + 2, line.IndexOf(@"Update.info\x3fdownload\x26psid\x3d1', demoteUrl:") - line.IndexOf(@"Update.info\x3fdownload\x26psid\x3d1', downloadUrl:") - 17);
                        while (line.Contains(@"\x3a"))
                            line = line.Replace(@"\x3a", ":");
                        while (line.Contains(@"\x2f"))
                            line = line.Replace(@"\x2f", "/");
                        while (line.Contains(@"\x3f"))
                            line = line.Replace(@"\x3f", "?");
                        while (line.Contains(@"\x26"))
                            line = line.Replace(@"\x26", "&");
                        while (line.Contains(@"\x3d"))
                            line = line.Replace(@"\x3d", "=");
                        line = line.Substring(0, line.Length - 16);
                        WebClient client = new WebClient();
                        string[] updateInfo = client.DownloadString(line).Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        if (Convert.ToInt32(updateInfo[0]) > build)
                        {
                            result.Build = updateInfo[0];
                            //result.Link = updateInfo[1];
                            for (int i = 1; i < updateInfo.Length; i++)
                            {
                                result.Description += updateInfo[i] + "\n\r";
                            }
                        }
                        break;
                    }
                }
                reader.Close();
                response.Close();
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
