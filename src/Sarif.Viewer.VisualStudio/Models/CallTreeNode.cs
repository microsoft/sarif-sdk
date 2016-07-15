// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.Sarif.Viewer.Models
{
    public class CallTreeNode
    {
        public AnnotatedCodeLocation Location { get; set; }
        public List<CallTreeNode> Children { get; set; }
    }
}
