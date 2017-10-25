using System.Xml.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Converters.PREFastObjectModel
{
    public class SFA
    {
        [XmlElement("FILEPATH")]
        public string FilePath { get; set; }

        [XmlElement("FILENAME")]
        public string FileName { get; set; }

        [XmlElement("LINE")]
        public int Line { get; set; }

        [XmlElement("COLUMN")]
        public int Column { get; set; }

        [XmlElement("KEYEVENT")]
        public KeyEvent KeyEvent { get; set; }
    }
}
