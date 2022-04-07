// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Replacement for sorting.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.3.0")]
    internal sealed class ReplacementComparer : IComparer<Replacement>
    {
        internal static readonly ReplacementComparer Instance = new ReplacementComparer();

        public int Compare(Replacement left, Replacement right)
        {
            int compareResult = 0;
            if (left.TryReferenceCompares(right, out compareResult))
            {
                return compareResult;
            }

            compareResult = RegionComparer.Instance.Compare(left.DeletedRegion, right.DeletedRegion);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = ArtifactContentComparer.Instance.Compare(left.InsertedContent, right.InsertedContent);
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