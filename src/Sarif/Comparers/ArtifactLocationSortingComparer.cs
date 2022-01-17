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
    internal class ArtifactLocationSortingComparer : IComparer<ArtifactLocation>
    {
        internal static readonly ArtifactLocationSortingComparer Instance = new ArtifactLocationSortingComparer();

        public int Compare(ArtifactLocation left, ArtifactLocation right)
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
            if (ReferenceEquals(left.Uri, right.Uri))
            {
                return 0;
            }

            if (left.Uri == null)
            {
                return -1;
            }

            if (right.Uri == null)
            {
                return 1;
            }

            compareResult = string.Compare(left.Uri.OriginalString, right.Uri.OriginalString);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = string.Compare(left.UriBaseId, right.UriBaseId);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.Index.CompareTo(right.Index);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = MessageSortingComparer.Instance.Compare(left.Description, right.Description);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}
