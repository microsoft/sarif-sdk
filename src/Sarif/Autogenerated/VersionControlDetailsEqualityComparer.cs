// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type VersionControlDetails for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    internal sealed class VersionControlDetailsEqualityComparer : IEqualityComparer<VersionControlDetails>
    {
        internal static readonly VersionControlDetailsEqualityComparer Instance = new VersionControlDetailsEqualityComparer();

        public bool Equals(VersionControlDetails left, VersionControlDetails right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.RepositoryUri != right.RepositoryUri)
            {
                return false;
            }

            if (left.RevisionId != right.RevisionId)
            {
                return false;
            }

            if (left.Branch != right.Branch)
            {
                return false;
            }

            if (left.RevisionTag != right.RevisionTag)
            {
                return false;
            }

            if (left.AsOfTimeUtc != right.AsOfTimeUtc)
            {
                return false;
            }

            if (!ArtifactLocation.ValueComparer.Equals(left.MappedTo, right.MappedTo))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Properties, right.Properties))
            {
                if (left.Properties == null || right.Properties == null || left.Properties.Count != right.Properties.Count)
                {
                    return false;
                }

                foreach (var value_0 in left.Properties)
                {
                    SerializedPropertyInfo value_1;
                    if (!right.Properties.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (!object.Equals(value_0.Value, value_1))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int GetHashCode(VersionControlDetails obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.RepositoryUri != null)
                {
                    result = (result * 31) + obj.RepositoryUri.GetHashCode();
                }

                if (obj.RevisionId != null)
                {
                    result = (result * 31) + obj.RevisionId.GetHashCode();
                }

                if (obj.Branch != null)
                {
                    result = (result * 31) + obj.Branch.GetHashCode();
                }

                if (obj.RevisionTag != null)
                {
                    result = (result * 31) + obj.RevisionTag.GetHashCode();
                }

                result = (result * 31) + obj.AsOfTimeUtc.GetHashCode();
                if (obj.MappedTo != null)
                {
                    result = (result * 31) + obj.MappedTo.ValueGetHashCode();
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_2 in obj.Properties)
                    {
                        xor_0 ^= value_2.Key.GetHashCode();
                        if (value_2.Value != null)
                        {
                            xor_0 ^= value_2.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }
            }

            return result;
        }
    }
}