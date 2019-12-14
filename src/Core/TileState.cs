using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Sidebar.Core
{
    [XmlType("tile")]
    public class TileState
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public double Height { get; set; }
        [XmlAttribute]
        public Guid InstanceGuid { get; set; }
        [XmlAttribute]
        public int Order { get; set; }
        [XmlAttribute]
        public bool IsPinned { get; set; }
        [XmlAttribute]
        public bool IsMinimized { get; set; }
    }
}
