using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Converters.PREFastObjectModel
{
    public class PREFastPath
    {
        [XmlElement("SFA")]
        public List<SFA> SFAs { get; set; }
    }
}
