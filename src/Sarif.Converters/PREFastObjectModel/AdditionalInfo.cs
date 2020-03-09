// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Converters.PREFastObjectModel
{
    public class AdditionalInfo : Dictionary<string, string>, IXmlSerializable
    {
        public XmlSchema GetSchema()
        {
            return null;
        }

        // Define how XmlSerializer.Deserialize should deserialize into a dictionary.
        public void ReadXml(XmlReader reader)
        {
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
            {
                return;
            }

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                string keyValue = reader.GetAttribute("key");
                if (!string.IsNullOrWhiteSpace(keyValue))
                {
                    string[] tokens = keyValue.Split(':');
                    string key = tokens[0];
                    string value = tokens.Length > 1 ? tokens[1] : null;

                    Add(key, value);
                }
                reader.ReadInnerXml();
            }

            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
