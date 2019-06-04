// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type ReportingDescriptor for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    internal sealed class ReportingDescriptorEqualityComparer : IEqualityComparer<ReportingDescriptor>
    {
        internal static readonly ReportingDescriptorEqualityComparer Instance = new ReportingDescriptorEqualityComparer();

        public bool Equals(ReportingDescriptor left, ReportingDescriptor right)
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

            if (!object.ReferenceEquals(left.DeprecatedIds, right.DeprecatedIds))
            {
                if (left.DeprecatedIds == null || right.DeprecatedIds == null)
                {
                    return false;
                }

                if (left.DeprecatedIds.Count != right.DeprecatedIds.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.DeprecatedIds.Count; ++index_0)
                {
                    if (left.DeprecatedIds[index_0] != right.DeprecatedIds[index_0])
                    {
                        return false;
                    }
                }
            }

            if (left.Guid != right.Guid)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.DeprecatedGuids, right.DeprecatedGuids))
            {
                if (left.DeprecatedGuids == null || right.DeprecatedGuids == null)
                {
                    return false;
                }

                if (left.DeprecatedGuids.Count != right.DeprecatedGuids.Count)
                {
                    return false;
                }

                for (int index_1 = 0; index_1 < left.DeprecatedGuids.Count; ++index_1)
                {
                    if (left.DeprecatedGuids[index_1] != right.DeprecatedGuids[index_1])
                    {
                        return false;
                    }
                }
            }

            if (left.Name != right.Name)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.DeprecatedNames, right.DeprecatedNames))
            {
                if (left.DeprecatedNames == null || right.DeprecatedNames == null)
                {
                    return false;
                }

                if (left.DeprecatedNames.Count != right.DeprecatedNames.Count)
                {
                    return false;
                }

                for (int index_2 = 0; index_2 < left.DeprecatedNames.Count; ++index_2)
                {
                    if (left.DeprecatedNames[index_2] != right.DeprecatedNames[index_2])
                    {
                        return false;
                    }
                }
            }

            if (!MultiformatMessageString.ValueComparer.Equals(left.ShortDescription, right.ShortDescription))
            {
                return false;
            }

            if (!MultiformatMessageString.ValueComparer.Equals(left.FullDescription, right.FullDescription))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.MessageStrings, right.MessageStrings))
            {
                if (left.MessageStrings == null || right.MessageStrings == null || left.MessageStrings.Count != right.MessageStrings.Count)
                {
                    return false;
                }

                foreach (var value_0 in left.MessageStrings)
                {
                    MultiformatMessageString value_1;
                    if (!right.MessageStrings.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (!MultiformatMessageString.ValueComparer.Equals(value_0.Value, value_1))
                    {
                        return false;
                    }
                }
            }

            if (!ReportingConfiguration.ValueComparer.Equals(left.DefaultConfiguration, right.DefaultConfiguration))
            {
                return false;
            }

            if (left.HelpUri != right.HelpUri)
            {
                return false;
            }

            if (!MultiformatMessageString.ValueComparer.Equals(left.Help, right.Help))
            {
                return false;
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

                for (int index_3 = 0; index_3 < left.Relationships.Count; ++index_3)
                {
                    if (!ReportingDescriptorRelationship.ValueComparer.Equals(left.Relationships[index_3], right.Relationships[index_3]))
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

        public int GetHashCode(ReportingDescriptor obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Id != null)
                {
                    result = (result * 31) + obj.Id.GetHashCode();
                }

                if (obj.DeprecatedIds != null)
                {
                    foreach (var value_4 in obj.DeprecatedIds)
                    {
                        result = result * 31;
                        if (value_4 != null)
                        {
                            result = (result * 31) + value_4.GetHashCode();
                        }
                    }
                }

                if (obj.Guid != null)
                {
                    result = (result * 31) + obj.Guid.GetHashCode();
                }

                if (obj.DeprecatedGuids != null)
                {
                    foreach (var value_5 in obj.DeprecatedGuids)
                    {
                        result = result * 31;
                        if (value_5 != null)
                        {
                            result = (result * 31) + value_5.GetHashCode();
                        }
                    }
                }

                if (obj.Name != null)
                {
                    result = (result * 31) + obj.Name.GetHashCode();
                }

                if (obj.DeprecatedNames != null)
                {
                    foreach (var value_6 in obj.DeprecatedNames)
                    {
                        result = result * 31;
                        if (value_6 != null)
                        {
                            result = (result * 31) + value_6.GetHashCode();
                        }
                    }
                }

                if (obj.ShortDescription != null)
                {
                    result = (result * 31) + obj.ShortDescription.ValueGetHashCode();
                }

                if (obj.FullDescription != null)
                {
                    result = (result * 31) + obj.FullDescription.ValueGetHashCode();
                }

                if (obj.MessageStrings != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_7 in obj.MessageStrings)
                    {
                        xor_0 ^= value_7.Key.GetHashCode();
                        if (value_7.Value != null)
                        {
                            xor_0 ^= value_7.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                if (obj.DefaultConfiguration != null)
                {
                    result = (result * 31) + obj.DefaultConfiguration.ValueGetHashCode();
                }

                if (obj.HelpUri != null)
                {
                    result = (result * 31) + obj.HelpUri.GetHashCode();
                }

                if (obj.Help != null)
                {
                    result = (result * 31) + obj.Help.ValueGetHashCode();
                }

                if (obj.Relationships != null)
                {
                    foreach (var value_8 in obj.Relationships)
                    {
                        result = result * 31;
                        if (value_8 != null)
                        {
                            result = (result * 31) + value_8.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_1 = 0;
                    foreach (var value_9 in obj.Properties)
                    {
                        xor_1 ^= value_9.Key.GetHashCode();
                        if (value_9.Value != null)
                        {
                            xor_1 ^= value_9.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_1;
                }
            }

            return result;
        }
    }
}