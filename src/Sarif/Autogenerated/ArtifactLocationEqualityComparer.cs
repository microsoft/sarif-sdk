// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type ArtifactLocation for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    internal sealed class ArtifactLocationEqualityComparer : IEqualityComparer<ArtifactLocation>
    {
        internal static readonly ArtifactLocationEqualityComparer Instance = new ArtifactLocationEqualityComparer();

        public bool Equals(ArtifactLocation left, ArtifactLocation right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.Uri != right.Uri)
            {
                return false;
            }

            if (left.UriBaseId != right.UriBaseId)
            {
                return false;
            }

            if (left.Index != right.Index)
            {
                return false;
            }

            if (!Message.ValueComparer.Equals(left.Description, right.Description))
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

        public int GetHashCode(ArtifactLocation obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Uri != null)
                {
                    result = (result * 31) + obj.Uri.GetHashCode();
                }

                if (obj.UriBaseId != null)
                {
                    result = (result * 31) + obj.UriBaseId.GetHashCode();
                }

                result = (result * 31) + obj.Index.GetHashCode();
                if (obj.Description != null)
                {
                    result = (result * 31) + obj.Description.ValueGetHashCode();
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