// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Converters.PREFastObjectModel
{
    [XmlRoot("DEFECTS")]
    public class DefectList
    {
        [XmlElement("DEFECT")]
        public List<Defect> Defects { get; set; } = new List<Defect>();
    }
}
