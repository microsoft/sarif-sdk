// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Converters.NessusObjectModel
{
    public class ReportHost
    {
        [XmlAttribute("name")]
        public string Name { get; set; } = string.Empty;

        [XmlElement("HostProperties")]
        public HostProperties HostProperties { get; set; } = new HostProperties();

        [XmlElement("ReportItem")]
        public List<ReportItem> ReportItems { get; set; }
    }
}
