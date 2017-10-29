using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Converters.PREFastObjectModel
{
    public class Category : Dictionary<string, string>, IXmlSerializable
    {
        public XmlSchema GetSchema()
        {
            return null;
        }

        // Define how XmlSerializer.Deserialize should deserialize into a dictionary
        public void ReadXml(XmlReader reader)
        {
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;
            
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                var key = reader.Name;
                var value = reader.ReadInnerXml();

                if (string.IsNullOrWhiteSpace(key))
                    continue;

                Add(key, value);
            }

            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
