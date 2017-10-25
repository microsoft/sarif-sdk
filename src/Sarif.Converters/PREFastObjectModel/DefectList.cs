using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Converters.PREFastObjectModel
{
    [XmlRoot("DEFECTS")]
    public class DefectList
    {
        [XmlElement("DEFECT")]
        public List<Defect> Defects { get; set; } = new List<Defect>();
    }
}
