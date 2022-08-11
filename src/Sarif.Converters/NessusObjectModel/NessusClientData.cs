// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Xml.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Converters.NessusObjectModel
{
    [XmlRoot("NessusClientData_v2")]
    public class NessusClientData
    {
        [XmlElement("Policy")]
        public Policy Policy { get; set; } = new Policy();

        [XmlElement("Report")]
        public Report Report { get; set; } = new Report();
    }
}
