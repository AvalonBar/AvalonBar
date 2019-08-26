using System.Xml.Serialization;

namespace Sidebar.Core
{
    [XmlRoot("UpdateInfo")]
    public struct UpdateInfo
    {
        public string Version;
        public string Description;
    }
}
