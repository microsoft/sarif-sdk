// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type ReportingConfigurationOverride for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    internal sealed class ReportingConfigurationOverrideEqualityComparer : IEqualityComparer<ReportingConfigurationOverride>
    {
        internal static readonly ReportingConfigurationOverrideEqualityComparer Instance = new ReportingConfigurationOverrideEqualityComparer();

        public bool Equals(ReportingConfigurationOverride left, ReportingConfigurationOverride right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (!ReportingConfiguration.ValueComparer.Equals(left.Configuration, right.Configuration))
            {
                return false;
            }

            if (left.NotificationIndex != right.NotificationIndex)
            {
                return false;
            }

            if (left.RuleIndex != right.RuleIndex)
            {
                return false;
            }

            if (left.ExtensionIndex != right.ExtensionIndex)
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

        public int GetHashCode(ReportingConfigurationOverride obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Configuration != null)
                {
                    result = (result * 31) + obj.Configuration.ValueGetHashCode();
                }

                result = (result * 31) + obj.NotificationIndex.GetHashCode();
                result = (result * 31) + obj.RuleIndex.GetHashCode();
                result = (result * 31) + obj.ExtensionIndex.GetHashCode();
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