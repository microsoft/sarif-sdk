// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type OutputConfiguration for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.61.0.0")]
    internal sealed class OutputConfigurationEqualityComparer : IEqualityComparer<OutputConfiguration>
    {
        internal static readonly OutputConfigurationEqualityComparer Instance = new OutputConfigurationEqualityComparer();

        public bool Equals(OutputConfiguration left, OutputConfiguration right)
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

            if (!PropertyBag.ValueComparer.Equals(left.Parameters, right.Parameters))
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

        public int GetHashCode(OutputConfiguration obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                result = (result * 31) + obj.Enabled.GetHashCode();
                if (obj.Level != null)
                {
                    result = (result * 31) + obj.Level.GetHashCode();
                }

                result = (result * 31) + obj.Rank.GetHashCode();
                if (obj.Parameters != null)
                {
                    result = (result * 31) + obj.Parameters.ValueGetHashCode();
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