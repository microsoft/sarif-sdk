// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

/// <summary>
/// All Comparer implementations should be replaced by auto-generated codes by JSchema as 
/// part of EqualityComparer in a planned comprehensive solution.
/// Tracking by issue: https://github.com/microsoft/jschema/issues/141
/// </summary>
namespace Microsoft.CodeAnalysis.Sarif
{
    internal class ArtifactSortingComparer : IComparer<Artifact>
    {
        internal static readonly ArtifactSortingComparer Instance = new ArtifactSortingComparer();

        public int Compare(Artifact left, Artifact right)
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
            compareResult = MessageSortingComparer.Instance.Compare(left.Description, right.Description);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = ArtifactLocationSortingComparer.Instance.Compare(left.Location, right.Location);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.ParentIndex.CompareTo(right.ParentIndex);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.Offset.CompareTo(right.Offset);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.Length.CompareTo(right.Length);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.Roles.CompareTo(right.Roles);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = string.Compare(left.MimeType, right.MimeType);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = ArtifactContentSortingComparer.Instance.Compare(left.Contents, right.Contents);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = string.Compare(left.Encoding, right.Encoding);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = string.Compare(left.SourceLanguage, right.SourceLanguage);
            if (compareResult != 0)
            {
                return compareResult;
            }

            if (!object.ReferenceEquals(left.Hashes, right.Hashes))
            {
                if (left.Hashes == null)
                {
                    return -1;
                }

                if (right.Hashes == null)
                {
                    return 1;
                }

                compareResult = left.Hashes.Count.CompareTo(right.Hashes.Count);
                if (compareResult != 0)
                {
                    return compareResult;
                }

                for (int i = 0; i < left.Hashes.Count; i++)
                {
                    compareResult = string.Compare(left.Hashes.ElementAt(i).Key, right.Hashes.ElementAt(i).Key);
                    if (compareResult != 0)
                    {
                        return compareResult;
                    }

                    compareResult = string.Compare(left.Hashes.ElementAt(i).Value, right.Hashes.ElementAt(i).Value);
                    if (compareResult != 0)
                    {
                        return compareResult;
                    }
                }
            }

            compareResult = left.LastModifiedTimeUtc.CompareTo(right.LastModifiedTimeUtc);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}
