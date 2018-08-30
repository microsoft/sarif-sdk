// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type RuleConfiguration for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.58.0.0")]
    internal sealed class RuleConfigurationEqualityComparer : IEqualityComparer<RuleConfiguration>
    {
        internal static readonly RuleConfigurationEqualityComparer Instance = new RuleConfigurationEqualityComparer();

        public bool Equals(RuleConfiguration left, RuleConfiguration right)
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

            if (left.DefaultLevel != right.DefaultLevel)
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

            return true;
        }

        public int GetHashCode(RuleConfiguration obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                result = (result * 31) + obj.Enabled.GetHashCode();
                result = (result * 31) + obj.DefaultLevel.GetHashCode();
                if (obj.Parameters != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_2 in obj.Parameters)
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