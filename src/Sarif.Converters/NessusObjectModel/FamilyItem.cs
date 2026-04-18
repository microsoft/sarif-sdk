using System.Xml.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Converters.NessusObjectModel
{
    public class FamilyItem
    {
        [XmlElement("FamilyName")]
        public string Name { get; set; } = string.Empty;

        [XmlElement("Status")]
        public string Status { get; set; } = string.Empty;
    }
}
