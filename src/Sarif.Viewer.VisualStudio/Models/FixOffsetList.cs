// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;

namespace Microsoft.Sarif.Viewer.Models
{
    public class FixOffsetList
    {
        public DateTime LastModified { get; set; }
        public SortedList<int, int> Offsets { get; private set; }

        public FixOffsetList()
        {
            Offsets = new SortedList<int, int>();
            LastModified = DateTime.Now;
        }
    }
}
