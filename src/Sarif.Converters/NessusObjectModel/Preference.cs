// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Xml.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Converters.NessusObjectModel
{
    public class Preference
    {
        [XmlElement("name")]
        public string Name { get; set; } = string.Empty;

        [XmlElement("value")]
        public string Value { get; set; } = string.Empty;
    }
}
