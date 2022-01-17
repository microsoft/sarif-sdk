// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

/// <summary>
/// All Comparer implementations should be replaced by auto-generated codes by JSchema as 
/// part of EqualityComparer in a planned comprehensive solution.
/// Tracking by issue: https://github.com/microsoft/jschema/issues/141
/// </summary>
namespace Microsoft.CodeAnalysis.Sarif
{
    internal class ArtifactContentSortingComparer : IComparer<ArtifactContent>
    {
        internal static readonly ArtifactContentSortingComparer Instance = new ArtifactContentSortingComparer();

        public int Compare(ArtifactContent left, ArtifactContent right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            int compareResult = 0;
            compareResult = string.Compare(left.Text, right.Text);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = string.Compare(left.Binary, right.Binary);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = MultiformatMessageStringSortingComparer.Instance.Compare(left.Rendered, right.Rendered);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}
