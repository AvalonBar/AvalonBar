using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Sidebar
{
    [XmlType("tile")]
    public class TileState
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public double Height { get; set; }
        [XmlAttribute]
        public bool IsMinimized { get; set; }
    }
}
