// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Region
    {
        public bool IsBinaryRegion => this.ByteOffset >= 0;

        public bool IsLineColumnBasedTextRegion => this.StartLine >= 1;

        public bool IsOffsetBasedTextRegion => this.CharOffset > 0;

        public override string ToString()
        {
            return this.FormatForVisualStudio();
        }

        public void PopulateDefaults()
        {
            if (this.IsLineColumnBasedTextRegion)
            {
                this.PopulateLineColumnBasedTextDefaults();
            }

            if (this.IsOffsetBasedTextRegion)
            {
                this.PopulateOffsetBasedTextDefaults();
            }

            if (this.IsBinaryRegion)
            {
                this.PopulateBinaryDefaults();
            }
        }

        private void PopulateLineColumnBasedTextDefaults()
        {
            if (this.EndLine == 0)
            {
                this.EndLine = this.StartLine;
            }

            if (this.StartColumn == 0)
            {
                this.StartColumn = 1;
            }

            if (this.EndColumn == 0)
            {
                this.EndColumn = int.MaxValue;
            }
        }

        private void PopulateOffsetBasedTextDefaults()
        {
            if (this.CharLength == -1)
            {
                this.CharLength = 0;
            }
        }

        private void PopulateBinaryDefaults()
        {
            if (this.ByteLength == -1)
            {
                this.ByteLength = 0;
            }
        }

        public static bool IsProperSuperset(Region superRegion, Region subRegion)
        {
            superRegion.PopulateDefaults();
            subRegion.PopulateDefaults();

            if (superRegion.IsLineColumnBasedTextRegion &&
                subRegion.IsLineColumnBasedTextRegion &&
                !IsLineColumnBasedTextRegionProperSuperset(superRegion, subRegion))
            {
                return false;
            }

            if (superRegion.IsOffsetBasedTextRegion &&
                subRegion.IsOffsetBasedTextRegion &&
                !IsOffsetBasedTextRegionProperSupetSet(superRegion, subRegion))
            {
                return false;
            }

            if (superRegion.IsBinaryRegion &&
                subRegion.IsBinaryRegion &&
                !IsBinaryRegionProperSuperset(superRegion, subRegion))
            {
                return false;
            }

            // if we reach here, the region and context region have been expressed as different property sets,
            // and it is not possible to judge validity without looking at the actual content.
            // It is a potential false negative.
            return true;
        }

        private static bool IsLineColumnBasedTextRegionProperSuperset(Region superRegion, Region subRegion)
        {
            if (superRegion.StartLine > subRegion.StartLine || superRegion.EndLine < subRegion.EndLine)
            {
                return false;
            }

            if (superRegion.StartLine == subRegion.StartLine && superRegion.StartColumn > subRegion.StartColumn)
            {
                return false;
            }

            if (superRegion.EndLine == subRegion.EndLine && superRegion.EndColumn < subRegion.EndColumn)
            {
                return false;
            }

            if (superRegion.StartLine == subRegion.StartLine &&
                superRegion.EndLine == subRegion.EndLine &&
                superRegion.StartColumn == subRegion.StartColumn &&
                superRegion.EndColumn == subRegion.EndColumn)
            {
                return false;
            }

            return true;
        }

        private static bool IsBinaryRegionProperSuperset(Region superRegion, Region subRegion)
        {
            if (superRegion.ByteOffset > subRegion.ByteOffset)
            {
                return false;
            }

            if (GetByteEndOffset(superRegion) < GetByteEndOffset(subRegion))
            {
                return false;
            }

            if (superRegion.ByteOffset == subRegion.ByteOffset && superRegion.ByteLength <= subRegion.ByteLength)
            {
                return false;
            }

            return true;
        }

        private static bool IsOffsetBasedTextRegionProperSupetSet(Region superRegion, Region subRegion)
        {
            if (superRegion.CharOffset > subRegion.CharOffset)
            {
                return false;
            }

            if (GetCharEndOffset(superRegion) < GetCharEndOffset(subRegion))
            {
                return false;
            }

            if (superRegion.CharOffset == subRegion.CharOffset && superRegion.CharLength <= subRegion.CharLength)
            {
                return false;
            }

            return true;
        }

        private static int GetCharEndOffset(Region region)
        {
            return region.CharOffset + region.CharLength;
        }

        private static int GetByteEndOffset(Region region)
        {
            return region.ByteOffset + region.ByteLength;
        }
    }
}
