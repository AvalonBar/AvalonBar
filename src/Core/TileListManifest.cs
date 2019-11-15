using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Sidebar.Core
{
    [XmlRoot("tilemanifest")]
    public class TileListManifest
    {
        [XmlElement("provider")]
        public TileListProvider Provider { get; set; }
        [XmlElement("tile")]
        public TileMetadata[] Tiles { get; set; }
    }
}
