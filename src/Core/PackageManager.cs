using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace Sidebar.Core
{
    public class PackageManager
    {
        public static void Unpack(string path, string file)
        {
            FileInfo info = new FileInfo(file);
            string name = info.Name.Substring(0, info.Name.LastIndexOf('.'));
            string destinationPath = Path.Combine(path, "Library", name);
            // TODO: Handle situations wherein the tile is already installed
            //       and we want to update the said tile.
            ZipFile.ExtractToDirectory(file, destinationPath);
        }
    }
}
