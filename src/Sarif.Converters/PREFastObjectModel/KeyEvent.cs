using System.Xml.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Converters.PREFastObjectModel
{
    public class KeyEvent
    {
        [XmlElement("ID")]
        public string Id { get; set; }

        [XmlElement("KIND")]
        public string Kind { get; set; }

        [XmlElement("IMPORTANCE")]
        public string Importance { get; set; }

        [XmlElement("MESSAGE")]
        public string Message { get; set; }
    }
}
