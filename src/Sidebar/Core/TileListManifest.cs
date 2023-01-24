using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Sidebar
{
    [XmlRoot("tilemanifest")]
    public class TileListManifest
    {
        [XmlElement("provider")]
        public TileListProvider Provider { get; set; }
        [XmlElement("tile")]
        public TileMetadata[] Tiles { get; set; }

        public bool ResolveUrls()
        {
            if (Provider == null && Tiles == null)
            {
                return false;
            }

            for (int i = 0; i < Tiles.Length; i++)
            {
                TileMetadata metadata = Tiles[i];

                metadata.ResolvedIcon = Provider.IconBaseUrl + metadata.Icon;
                metadata.ResolvedDownloadUrl = Provider.PackageBaseUrl + metadata.DownloadUrl;
            }

            return true;
        }
    }
}
