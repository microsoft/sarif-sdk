// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Xml.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Converters.NessusObjectModel
{
    public class PluginItem
    {
        [XmlElement("PluginId")]
        public string PluginId { get; set; } = string.Empty;

        [XmlElement("PluginName")]
        public string PluginName { get; set; } = string.Empty;

        [XmlElement("Family")]
        public string Family { get; set; } = string.Empty;

        [XmlElement("Status")]
        public string Status { get; set; } = string.Empty;
    }
}
