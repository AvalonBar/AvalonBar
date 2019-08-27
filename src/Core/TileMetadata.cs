using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Sidebar.Core
{
    [Serializable]
    [XmlType("Tile")]
    public class TileMetadata
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public double Height { get; set; }
        [XmlAttribute]
        public bool IsMinimized { get; set; }
    }
}
