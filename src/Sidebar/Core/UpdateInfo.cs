using System.Xml.Serialization;

namespace Sidebar
{
    [XmlRoot("UpdateInfo")]
    public struct UpdateInfo
    {
        public string Version;
        public string Description;
        public string PackageUrl;
        
        public static UpdateInfo Empty
        {
            get { return default(UpdateInfo); }
        }
    }
}
