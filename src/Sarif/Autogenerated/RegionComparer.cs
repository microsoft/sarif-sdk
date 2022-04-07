// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Region for sorting.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.3.0")]
    internal sealed class RegionComparer : IComparer<Region>
    {
        internal static readonly RegionComparer Instance = new RegionComparer();

        public int Compare(Region left, Region right)
        {
            int compareResult = 0;
            if (left.TryReferenceCompares(right, out compareResult))
            {
                return compareResult;
            }

            compareResult = left.StartLine.CompareTo(right.StartLine);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.StartColumn.CompareTo(right.StartColumn);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.EndLine.CompareTo(right.EndLine);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.EndColumn.CompareTo(right.EndColumn);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.CharOffset.CompareTo(right.CharOffset);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.CharLength.CompareTo(right.CharLength);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.ByteOffset.CompareTo(right.ByteOffset);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.ByteLength.CompareTo(right.ByteLength);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = ArtifactContentComparer.Instance.Compare(left.Snippet, right.Snippet);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = MessageComparer.Instance.Compare(left.Message, right.Message);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = string.Compare(left.SourceLanguage, right.SourceLanguage);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.Properties.DictionaryCompares(right.Properties, SerializedPropertyInfoComparer.Instance);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}