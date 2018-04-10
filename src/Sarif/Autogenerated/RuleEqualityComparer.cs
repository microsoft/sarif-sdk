// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Rule for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    internal sealed class RuleEqualityComparer : IEqualityComparer<Rule>
    {
        internal static readonly RuleEqualityComparer Instance = new RuleEqualityComparer();

        public bool Equals(Rule left, Rule right)
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

            if (left.Name != right.Name)
            {
                return false;
            }

            if (left.ShortDescription != right.ShortDescription)
            {
                return false;
            }

            if (left.FullDescription != right.FullDescription)
            {
                return false;
            }

            if (left.RichDescription != right.RichDescription)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.MessageTemplates, right.MessageTemplates))
            {
                if (left.MessageTemplates == null || right.MessageTemplates == null || left.MessageTemplates.Count != right.MessageTemplates.Count)
                {
                    return false;
                }

                foreach (var value_0 in left.MessageTemplates)
                {
                    string value_1;
                    if (!right.MessageTemplates.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (value_0.Value != value_1)
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.RichMessageTemplates, right.RichMessageTemplates))
            {
                if (left.RichMessageTemplates == null || right.RichMessageTemplates == null || left.RichMessageTemplates.Count != right.RichMessageTemplates.Count)
                {
                    return false;
                }

                foreach (var value_2 in left.RichMessageTemplates)
                {
                    string value_3;
                    if (!right.RichMessageTemplates.TryGetValue(value_2.Key, out value_3))
                    {
                        return false;
                    }

                    if (value_2.Value != value_3)
                    {
                        return false;
                    }
                }
            }

            if (!RuleConfiguration.ValueComparer.Equals(left.Configuration, right.Configuration))
            {
                return false;
            }

            if (left.HelpUri != right.HelpUri)
            {
                return false;
            }

            if (left.Help != right.Help)
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

        public int GetHashCode(Rule obj)
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

                if (obj.Name != null)
                {
                    result = (result * 31) + obj.Name.GetHashCode();
                }

                if (obj.ShortDescription != null)
                {
                    result = (result * 31) + obj.ShortDescription.GetHashCode();
                }

                if (obj.FullDescription != null)
                {
                    result = (result * 31) + obj.FullDescription.GetHashCode();
                }

                if (obj.RichDescription != null)
                {
                    result = (result * 31) + obj.RichDescription.GetHashCode();
                }

                if (obj.MessageTemplates != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_6 in obj.MessageTemplates)
                    {
                        xor_0 ^= value_6.Key.GetHashCode();
                        if (value_6.Value != null)
                        {
                            xor_0 ^= value_6.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                if (obj.RichMessageTemplates != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_1 = 0;
                    foreach (var value_7 in obj.RichMessageTemplates)
                    {
                        xor_1 ^= value_7.Key.GetHashCode();
                        if (value_7.Value != null)
                        {
                            xor_1 ^= value_7.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_1;
                }

                if (obj.Configuration != null)
                {
                    result = (result * 31) + obj.Configuration.ValueGetHashCode();
                }

                if (obj.HelpUri != null)
                {
                    result = (result * 31) + obj.HelpUri.GetHashCode();
                }

                if (obj.Help != null)
                {
                    result = (result * 31) + obj.Help.GetHashCode();
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_2 = 0;
                    foreach (var value_8 in obj.Properties)
                    {
                        xor_2 ^= value_8.Key.GetHashCode();
                        if (value_8.Value != null)
                        {
                            xor_2 ^= value_8.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_2;
                }
            }

            return result;
        }
    }
}