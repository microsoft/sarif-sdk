// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
