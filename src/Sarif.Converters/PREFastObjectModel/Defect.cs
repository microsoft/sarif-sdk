using System.Xml.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Converters.PREFastObjectModel
{
    public class Defect
    {
        [XmlElement("SFA")]
        public SFA SFA { get; set; }

        [XmlElement("DEFECTCODE")]
        public string DefectCode { get; set; }

        [XmlElement("DESCRIPTION")]
        public string Description { get; set; }

        [XmlElement("FUNCTION")]
        public string Function { get; set; }

        [XmlElement("DECORATED")]
        public string Decorated { get; set; }

        [XmlElement("FUNCLINE")]
        public string Funcline { get; set; }

        [XmlElement("PROBABILITY")]
        public string Probability { get; set; }

        [XmlElement("RANK")]
        public string Rank { get; set; }

        [XmlElement("PATH")]
        public PREFastPath Path { get; set; }

        [XmlElement("CATEGORY")]
        public Category Category { get; set; }

        [XmlElement("ADDITIONALINFO")]
        public AdditionalInfo AdditionalInfo { get; set; }
    }
}
