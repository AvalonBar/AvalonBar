using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Slate.Packaging
{
    public class PackageManager
    {
        public static void Unpack(string path, string file)
        {
            using (FileStream fileStreamIn = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                using (ZipInputStream zipInStream = new ZipInputStream(fileStreamIn))
                {
                    ZipEntry entry;
                    FileInfo info = new FileInfo(file);
                    string name = info.Name.Substring(0, info.Name.LastIndexOf('.'));
                    while (true)
                    {
                        entry = zipInStream.GetNextEntry();
                        if (entry == null)
                            break;
                        if (!entry.IsDirectory)
                        {
                            if (!Directory.Exists(path + @"\Library\" + name))
                                Directory.CreateDirectory(path + @"\Library\" + name);

                            using (FileStream fileStreamOut = new FileStream(string.Format(@"{0}\Library\{1}\{2}", path, name, entry.Name), FileMode.Create, FileAccess.Write))
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
                            if (!Directory.Exists(string.Format(@"{0}\Library\{1}\{2}", path, name, entry.Name)))
                                Directory.CreateDirectory(string.Format(@"{0}\Library\{1}\{2}", path, name, entry.Name));
                    }
                    zipInStream.Close();
                }
                fileStreamIn.Close();
            }
        }
    }
}
