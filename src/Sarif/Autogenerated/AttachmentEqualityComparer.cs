// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Attachment for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    internal sealed class AttachmentEqualityComparer : IEqualityComparer<Attachment>
    {
        internal static readonly AttachmentEqualityComparer Instance = new AttachmentEqualityComparer();

        public bool Equals(Attachment left, Attachment right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (!Message.ValueComparer.Equals(left.Description, right.Description))
            {
                return false;
            }

            if (!ArtifactLocation.ValueComparer.Equals(left.ArtifactLocation, right.ArtifactLocation))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Regions, right.Regions))
            {
                if (left.Regions == null || right.Regions == null)
                {
                    return false;
                }

                if (left.Regions.Count != right.Regions.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.Regions.Count; ++index_0)
                {
                    if (!Region.ValueComparer.Equals(left.Regions[index_0], right.Regions[index_0]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.Rectangles, right.Rectangles))
            {
                if (left.Rectangles == null || right.Rectangles == null)
                {
                    return false;
                }

                if (left.Rectangles.Count != right.Rectangles.Count)
                {
                    return false;
                }

                for (int index_1 = 0; index_1 < left.Rectangles.Count; ++index_1)
                {
                    if (!Rectangle.ValueComparer.Equals(left.Rectangles[index_1], right.Rectangles[index_1]))
                    {
                        return false;
                    }
                }
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

        public int GetHashCode(Attachment obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Description != null)
                {
                    result = (result * 31) + obj.Description.ValueGetHashCode();
                }

                if (obj.ArtifactLocation != null)
                {
                    result = (result * 31) + obj.ArtifactLocation.ValueGetHashCode();
                }

                if (obj.Regions != null)
                {
                    foreach (var value_2 in obj.Regions)
                    {
                        result = result * 31;
                        if (value_2 != null)
                        {
                            result = (result * 31) + value_2.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Rectangles != null)
                {
                    foreach (var value_3 in obj.Rectangles)
                    {
                        result = result * 31;
                        if (value_3 != null)
                        {
                            result = (result * 31) + value_3.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_4 in obj.Properties)
                    {
                        xor_0 ^= value_4.Key.GetHashCode();
                        if (value_4.Value != null)
                        {
                            xor_0 ^= value_4.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }
            }

            return result;
        }
    }
}