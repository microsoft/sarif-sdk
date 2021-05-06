﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Converters.PREFastObjectModel
{
    public class PREFastPath
    {
        [XmlElement("SFA")]
        public List<SFA> SFAs { get; set; }
    }
}
