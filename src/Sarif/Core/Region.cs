// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Region
    {
        public bool IsBinaryRegion => this.ByteOffset >= 0;

        public bool IsLineColumnBasedTextRegion => this.StartLine >= 1;

        public bool IsOffsetBasedTextRegion => this.CharOffset >= 0;

        /// <summary>
        /// Returns a binary <see cref="Region"/> that covers the entirety of
        /// the artifact at <paramref name="artifactPath"/>:
        /// <c>byteOffset = 0</c>, <c>byteLength = artifact size</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the spec-blessed (SARIF \u00a73.30) shape for "this finding
        /// applies to the whole artifact" — preferable to the common
        /// workaround of synthesising <c>{ startLine: 1 }</c>, which collides
        /// on deduplication when many whole-file findings land on the same
        /// nominal line. A binary region uniquely identifies "the whole file"
        /// without misrepresenting it as line-localized.
        /// </para>
        /// <para>
        /// Files larger than <see cref="int.MaxValue"/> bytes (~2 GiB) are
        /// clamped — SARIF <c>byteLength</c> is an <c>int</c>; representing
        /// a larger region requires a different SARIF construct.
        /// </para>
        /// </remarks>
        /// <param name="artifactPath">Filesystem path to the artifact.</param>
        public static Region ForEntireArtifact(string artifactPath)
        {
            if (string.IsNullOrEmpty(artifactPath))
            {
                throw new ArgumentException("Artifact path must be non-empty.", nameof(artifactPath));
            }

            long length = new FileInfo(artifactPath).Length;
            return ForEntireArtifact(length);
        }

        /// <summary>
        /// Returns a binary <see cref="Region"/> that covers
        /// <paramref name="byteLength"/> bytes starting at offset 0 — i.e., the
        /// whole artifact, when the caller already knows its size in bytes.
        /// Files larger than <see cref="int.MaxValue"/> bytes are clamped.
        /// </summary>
        public static Region ForEntireArtifact(long byteLength)
        {
            if (byteLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(byteLength), "Byte length must be non-negative.");
            }

            int length = byteLength > int.MaxValue ? int.MaxValue : (int)byteLength;
            return new Region { ByteOffset = 0, ByteLength = length };
        }

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

        public bool IsProperSupersetOf(Region subRegion)
        {
            this.PopulateDefaults();
            subRegion.PopulateDefaults();

            if (this.IsLineColumnBasedTextRegion &&
                subRegion.IsLineColumnBasedTextRegion &&
                !IsLineColumnBasedTextRegionProperSupersetOf(subRegion))
            {
                return false;
            }

            if (this.IsOffsetBasedTextRegion &&
                subRegion.IsOffsetBasedTextRegion &&
                !IsOffsetBasedTextRegionProperSupetSetOf(subRegion))
            {
                return false;
            }

            if (this.IsBinaryRegion &&
                subRegion.IsBinaryRegion &&
                !IsBinaryRegionProperSupersetOf(subRegion))
            {
                return false;
            }

            // if we reach here, the region and context region have been expressed as different property sets,
            // and it is not possible to judge validity without looking at the actual content.
            // It is a potential false negative.
            return true;
        }

        private bool IsLineColumnBasedTextRegionProperSupersetOf(Region subRegion)
        {
            if (this.StartLine > subRegion.StartLine || this.EndLine < subRegion.EndLine)
            {
                return false;
            }

            if (this.StartLine == subRegion.StartLine && this.StartColumn > subRegion.StartColumn)
            {
                return false;
            }

            if (this.EndLine == subRegion.EndLine && this.EndColumn < subRegion.EndColumn)
            {
                return false;
            }

            if (this.StartLine == subRegion.StartLine &&
                this.EndLine == subRegion.EndLine &&
                this.StartColumn == subRegion.StartColumn &&
                this.EndColumn == subRegion.EndColumn)
            {
                return false;
            }

            return true;
        }

        private bool IsBinaryRegionProperSupersetOf(Region subRegion)
        {
            if (this.ByteOffset > subRegion.ByteOffset)
            {
                return false;
            }

            if (GetByteEndOffset(this) < GetByteEndOffset(subRegion))
            {
                return false;
            }

            if (this.ByteOffset == subRegion.ByteOffset && this.ByteLength <= subRegion.ByteLength)
            {
                return false;
            }

            return true;
        }

        private bool IsOffsetBasedTextRegionProperSupetSetOf(Region subRegion)
        {
            if (this.CharOffset > subRegion.CharOffset)
            {
                return false;
            }

            if (GetCharEndOffset(this) < GetCharEndOffset(subRegion))
            {
                return false;
            }

            if (this.CharOffset == subRegion.CharOffset && this.CharLength <= subRegion.CharLength)
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

