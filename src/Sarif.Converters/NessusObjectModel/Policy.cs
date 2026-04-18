// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Converters.NessusObjectModel
{
    public class Policy
    {
        [XmlElement("policyName")]
        public string PolicyName { get; set; } = string.Empty;

        [XmlElement("policyComments")]
        public string PolicyComments { get; set; } = string.Empty;

        [XmlElement("Preferences")]
        public Preferences Preferences { get; set; } = new Preferences();

        [XmlElement("FamilySelection")]
        public FamilySelection FamilySelection { get; set; } = new FamilySelection();

        [XmlElement("IndividualPluginSelection")]
        public IndividualPluginSelection IndividualPluginSelection { get; set; } = new IndividualPluginSelection();
    }
}
