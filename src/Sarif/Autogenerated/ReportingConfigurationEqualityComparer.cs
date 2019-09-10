// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type ReportingConfiguration for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    internal sealed class ReportingConfigurationEqualityComparer : IEqualityComparer<ReportingConfiguration>
    {
        internal static readonly ReportingConfigurationEqualityComparer Instance = new ReportingConfigurationEqualityComparer();

        public bool Equals(ReportingConfiguration left, ReportingConfiguration right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.Enabled != right.Enabled)
            {
                return false;
            }

            if (left.Level != right.Level)
            {
                return false;
            }

            if (left.Rank != right.Rank)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Parameters, right.Parameters))
            {
                if (left.Parameters == null || right.Parameters == null || left.Parameters.Count != right.Parameters.Count)
                {
                    return false;
                }

                foreach (var value_0 in left.Parameters)
                {
                    SerializedPropertyInfo value_1;
                    if (!right.Parameters.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (!object.Equals(value_0.Value, value_1))
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

                foreach (var value_2 in left.Properties)
                {
                    SerializedPropertyInfo value_3;
                    if (!right.Properties.TryGetValue(value_2.Key, out value_3))
                    {
                        return false;
                    }

                    if (!object.Equals(value_2.Value, value_3))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int GetHashCode(ReportingConfiguration obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                result = (result * 31) + obj.Enabled.GetHashCode();
                result = (result * 31) + obj.Level.GetHashCode();
                result = (result * 31) + obj.Rank.GetHashCode();
                if (obj.Parameters != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_4 in obj.Parameters)
                    {
                        xor_0 ^= value_4.Key.GetHashCode();
                        if (value_4.Value != null)
                        {
                            xor_0 ^= value_4.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_1 = 0;
                    foreach (var value_5 in obj.Properties)
                    {
                        xor_1 ^= value_5.Key.GetHashCode();
                        if (value_5.Value != null)
                        {
                            xor_1 ^= value_5.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_1;
                }
            }

            return result;
        }
    }
}