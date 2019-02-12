// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type MessageDescriptor for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.61.0.0")]
    internal sealed class MessageDescriptorEqualityComparer : IEqualityComparer<MessageDescriptor>
    {
        internal static readonly MessageDescriptorEqualityComparer Instance = new MessageDescriptorEqualityComparer();

        public bool Equals(MessageDescriptor left, MessageDescriptor right)
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

            if (!Message.ValueComparer.Equals(left.Name, right.Name))
            {
                return false;
            }

            if (!Message.ValueComparer.Equals(left.ShortDescription, right.ShortDescription))
            {
                return false;
            }

            if (!Message.ValueComparer.Equals(left.FullDescription, right.FullDescription))
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
                    string value_1;
                    if (!right.MessageStrings.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (value_0.Value != value_1)
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.RichMessageStrings, right.RichMessageStrings))
            {
                if (left.RichMessageStrings == null || right.RichMessageStrings == null || left.RichMessageStrings.Count != right.RichMessageStrings.Count)
                {
                    return false;
                }

                foreach (var value_2 in left.RichMessageStrings)
                {
                    string value_3;
                    if (!right.RichMessageStrings.TryGetValue(value_2.Key, out value_3))
                    {
                        return false;
                    }

                    if (value_2.Value != value_3)
                    {
                        return false;
                    }
                }
            }

            if (!RuleConfiguration.ValueComparer.Equals(left.DefaultConfiguration, right.DefaultConfiguration))
            {
                return false;
            }

            if (left.HelpUri != right.HelpUri)
            {
                return false;
            }

            if (!Message.ValueComparer.Equals(left.Help, right.Help))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Properties, right.Properties))
            {
                if (left.Properties == null || right.Properties == null || left.Properties.Count != right.Properties.Count)
                {
                    return false;
                }

                foreach (var value_4 in left.Properties)
                {
                    SerializedPropertyInfo value_5;
                    if (!right.Properties.TryGetValue(value_4.Key, out value_5))
                    {
                        return false;
                    }

                    if (!object.Equals(value_4.Value, value_5))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int GetHashCode(MessageDescriptor obj)
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
                    foreach (var value_6 in obj.DeprecatedIds)
                    {
                        result = result * 31;
                        if (value_6 != null)
                        {
                            result = (result * 31) + value_6.GetHashCode();
                        }
                    }
                }

                if (obj.Name != null)
                {
                    result = (result * 31) + obj.Name.ValueGetHashCode();
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

                if (obj.RichMessageStrings != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_1 = 0;
                    foreach (var value_8 in obj.RichMessageStrings)
                    {
                        xor_1 ^= value_8.Key.GetHashCode();
                        if (value_8.Value != null)
                        {
                            xor_1 ^= value_8.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_1;
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

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_2 = 0;
                    foreach (var value_9 in obj.Properties)
                    {
                        xor_2 ^= value_9.Key.GetHashCode();
                        if (value_9.Value != null)
                        {
                            xor_2 ^= value_9.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_2;
                }
            }

            return result;
        }
    }
}