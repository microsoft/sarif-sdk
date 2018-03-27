// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.DefaultBaseline
{
    internal class PhysicalLocationBaselineEquals : IEqualityComparer<PhysicalLocation>
    {
        public static readonly PhysicalLocationBaselineEquals Instance = new PhysicalLocationBaselineEquals();
        public bool Equals(PhysicalLocation x, PhysicalLocation y)
        {
            if(!object.ReferenceEquals(x, y))
            {
                if (x == null || y == null)
                {
                    return false;
                }

                // Only compare URI (so file path/relative file path).  UriBaseId and Region may change run over run.
                if (x.Uri != y.Uri)
                {
                    return false;
                }
            }
            
            return true;
        }

        public int GetHashCode(PhysicalLocation obj)
        {
            return obj.Uri.GetNullCheckedHashCode();
        }
    }
}
