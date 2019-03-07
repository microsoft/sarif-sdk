// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Location for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    internal sealed class LocationEqualityComparer : IEqualityComparer<Location>
    {
        internal static readonly LocationEqualityComparer Instance = new LocationEqualityComparer();

        public bool Equals(Location left, Location right)
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

            if (!PhysicalLocation.ValueComparer.Equals(left.PhysicalLocation, right.PhysicalLocation))
            {
                return false;
            }

            if (left.FullyQualifiedLogicalName != right.FullyQualifiedLogicalName)
            {
                return false;
            }

            if (left.LogicalLocationIndex != right.LogicalLocationIndex)
            {
                return false;
            }

            if (!Message.ValueComparer.Equals(left.Message, right.Message))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Annotations, right.Annotations))
            {
                if (left.Annotations == null || right.Annotations == null)
                {
                    return false;
                }

                if (left.Annotations.Count != right.Annotations.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.Annotations.Count; ++index_0)
                {
                    if (!Region.ValueComparer.Equals(left.Annotations[index_0], right.Annotations[index_0]))
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

        public int GetHashCode(Location obj)
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

                if (obj.PhysicalLocation != null)
                {
                    result = (result * 31) + obj.PhysicalLocation.ValueGetHashCode();
                }

                if (obj.FullyQualifiedLogicalName != null)
                {
                    result = (result * 31) + obj.FullyQualifiedLogicalName.GetHashCode();
                }

                result = (result * 31) + obj.LogicalLocationIndex.GetHashCode();
                if (obj.Message != null)
                {
                    result = (result * 31) + obj.Message.ValueGetHashCode();
                }

                if (obj.Annotations != null)
                {
                    foreach (var value_2 in obj.Annotations)
                    {
                        result = result * 31;
                        if (value_2 != null)
                        {
                            result = (result * 31) + value_2.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_3 in obj.Properties)
                    {
                        xor_0 ^= value_3.Key.GetHashCode();
                        if (value_3.Value != null)
                        {
                            xor_0 ^= value_3.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }
            }

            return result;
        }
    }
}