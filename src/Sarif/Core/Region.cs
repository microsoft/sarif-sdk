// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Region
    {
        public bool IsBinaryRegion
        {
            get
            {
                // Is this right? What about an insertion point right after a BOM in a text file??
                // Do we need to just bite the bullet and make these Nullable type so that we have a
                // clear indicator of whether the region is binary vs. textual? I tend to think so.
                return
                     this.StartLine == 0 &&
                     this.CharLength == 0 &&
                     this.CharOffset == 0;
            }
        }

        public override string ToString()
        {
            return this.FormatForVisualStudio();
        }
    }
}
