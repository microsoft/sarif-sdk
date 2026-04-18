// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Xml.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Converters.NessusObjectModel
{
    public class Item
    {
        [XmlElement("fullName")]
        public string FullName { get; set; } = string.Empty;

        [XmlElement("pluginName")]
        public string PluginName { get; set; } = string.Empty;

        [XmlElement("pluginId")]
        public string PluginId { get; set; } = string.Empty;

        [XmlElement("preferenceName")]
        public string PreferenceName { get; set; } = string.Empty;

        [XmlElement("preferenceType")]
        public string PreferenceType { get; set; } = string.Empty;

        [XmlElement("preferenceValues")]
        public string PreferenceValues { get; set; } = string.Empty;

        [XmlElement("selectedValue")]
        public string SelectedValue { get; set; } = string.Empty;
    }
}
