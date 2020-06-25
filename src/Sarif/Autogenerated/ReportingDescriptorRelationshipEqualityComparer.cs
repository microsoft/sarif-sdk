// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type ReportingDescriptorRelationship for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    internal sealed class ReportingDescriptorRelationshipEqualityComparer : IEqualityComparer<ReportingDescriptorRelationship>
    {
        internal static readonly ReportingDescriptorRelationshipEqualityComparer Instance = new ReportingDescriptorRelationshipEqualityComparer();

        public bool Equals(ReportingDescriptorRelationship left, ReportingDescriptorRelationship right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (!ReportingDescriptorReference.ValueComparer.Equals(left.Target, right.Target))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Kinds, right.Kinds))
            {
                if (left.Kinds == null || right.Kinds == null)
                {
                    return false;
                }

                if (left.Kinds.Count != right.Kinds.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.Kinds.Count; ++index_0)
                {
                    if (left.Kinds[index_0] != right.Kinds[index_0])
                    {
                        return false;
                    }
                }
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

        public int GetHashCode(ReportingDescriptorRelationship obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Target != null)
                {
                    result = (result * 31) + obj.Target.ValueGetHashCode();
                }

                if (obj.Kinds != null)
                {
                    foreach (var value_2 in obj.Kinds)
                    {
                        result = result * 31;
                        if (value_2 != null)
                        {
                            result = (result * 31) + value_2.GetHashCode();
                        }
                    }
                }

                if (obj.Description != null)
                {
                    result = (result * 31) + obj.Description.ValueGetHashCode();
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