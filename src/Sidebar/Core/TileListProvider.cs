using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Sidebar
{
    [XmlRoot("tileprovider")]
    public class TileListProvider
    {
        [XmlElement("name")]
        public string Name { get; set; }
        [XmlElement("homepage")]
        public string HomepageUrl { get; set; }
        [XmlElement("privacypolicy")]
        public string PrivacyPolicyUrl { get; set; }
        [XmlElement("eula")]
        public string EulaUrl { get; set; }
        [XmlElement("iconbaseurl")]
        public string IconBaseUrl { get; set; }
        [XmlElement("packagebaseurl")]
        public string PackageBaseUrl { get; set; }
    }
}
