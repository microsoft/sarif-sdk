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
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
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

            if (left.Id != right.Id)
            {
                return false;
            }

            if (!PhysicalLocation.ValueComparer.Equals(left.PhysicalLocation, right.PhysicalLocation))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.LogicalLocations, right.LogicalLocations))
            {
                if (left.LogicalLocations == null || right.LogicalLocations == null)
                {
                    return false;
                }

                if (left.LogicalLocations.Count != right.LogicalLocations.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.LogicalLocations.Count; ++index_0)
                {
                    if (!LogicalLocation.ValueComparer.Equals(left.LogicalLocations[index_0], right.LogicalLocations[index_0]))
                    {
                        return false;
                    }
                }
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

                for (int index_1 = 0; index_1 < left.Annotations.Count; ++index_1)
                {
                    if (!Region.ValueComparer.Equals(left.Annotations[index_1], right.Annotations[index_1]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.Relationships, right.Relationships))
            {
                if (left.Relationships == null || right.Relationships == null)
                {
                    return false;
                }

                if (left.Relationships.Count != right.Relationships.Count)
                {
                    return false;
                }

                for (int index_2 = 0; index_2 < left.Relationships.Count; ++index_2)
                {
                    if (!LocationRelationship.ValueComparer.Equals(left.Relationships[index_2], right.Relationships[index_2]))
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
                result = (result * 31) + obj.Id.GetHashCode();
                if (obj.PhysicalLocation != null)
                {
                    result = (result * 31) + obj.PhysicalLocation.ValueGetHashCode();
                }

                if (obj.LogicalLocations != null)
                {
                    foreach (var value_2 in obj.LogicalLocations)
                    {
                        result = result * 31;
                        if (value_2 != null)
                        {
                            result = (result * 31) + value_2.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Message != null)
                {
                    result = (result * 31) + obj.Message.ValueGetHashCode();
                }

                if (obj.Annotations != null)
                {
                    foreach (var value_3 in obj.Annotations)
                    {
                        result = result * 31;
                        if (value_3 != null)
                        {
                            result = (result * 31) + value_3.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Relationships != null)
                {
                    foreach (var value_4 in obj.Relationships)
                    {
                        result = result * 31;
                        if (value_4 != null)
                        {
                            result = (result * 31) + value_4.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_5 in obj.Properties)
                    {
                        xor_0 ^= value_5.Key.GetHashCode();
                        if (value_5.Value != null)
                        {
                            xor_0 ^= value_5.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }
            }

            return result;
        }
    }
}