// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type PhysicalLocation for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    internal sealed class PhysicalLocationEqualityComparer : IEqualityComparer<PhysicalLocation>
    {
        internal static readonly PhysicalLocationEqualityComparer Instance = new PhysicalLocationEqualityComparer();

        public bool Equals(PhysicalLocation left, PhysicalLocation right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (!Address.ValueComparer.Equals(left.Address, right.Address))
            {
                return false;
            }

            if (!ArtifactLocation.ValueComparer.Equals(left.ArtifactLocation, right.ArtifactLocation))
            {
                return false;
            }

            if (!Region.ValueComparer.Equals(left.Region, right.Region))
            {
                return false;
            }

            if (!Region.ValueComparer.Equals(left.ContextRegion, right.ContextRegion))
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

        public int GetHashCode(PhysicalLocation obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Address != null)
                {
                    result = (result * 31) + obj.Address.ValueGetHashCode();
                }

                if (obj.ArtifactLocation != null)
                {
                    result = (result * 31) + obj.ArtifactLocation.ValueGetHashCode();
                }

                if (obj.Region != null)
                {
                    result = (result * 31) + obj.Region.ValueGetHashCode();
                }

                if (obj.ContextRegion != null)
                {
                    result = (result * 31) + obj.ContextRegion.ValueGetHashCode();
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