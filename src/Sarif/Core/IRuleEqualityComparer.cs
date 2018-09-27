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
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.58.0.0")]
    internal sealed class IRuleEqualityComparer : IEqualityComparer<IRule>
    {
        internal static readonly IRuleEqualityComparer Instance = new IRuleEqualityComparer();

        public bool Equals(IRule left, IRule right)
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

            if (!RuleConfiguration.ValueComparer.Equals(left.Configuration, right.Configuration))
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

            var leftPropertyBagHolder = left as PropertyBagHolder;
            var rightPropertyBagHolder = right as PropertyBagHolder;


            if (leftPropertyBagHolder == null && rightPropertyBagHolder == null)
            {
                return true;
            }


            if (rightPropertyBagHolder == null)
            {
                return leftPropertyBagHolder.Properties == null || leftPropertyBagHolder.Properties.Count == 0;
            }


            if (!object.ReferenceEquals(leftPropertyBagHolder, rightPropertyBagHolder))
            {
                if (!object.ReferenceEquals(leftPropertyBagHolder.Properties, rightPropertyBagHolder.Properties))
                {

                    if (leftPropertyBagHolder.Properties == null ||
                        rightPropertyBagHolder.Properties == null ||
                        leftPropertyBagHolder.Properties.Count != rightPropertyBagHolder.Properties.Count)
                    {
                        return false;
                    }

                    foreach (var value_4 in leftPropertyBagHolder.Properties)
                    {
                        SerializedPropertyInfo value_5;
                        if (!leftPropertyBagHolder.Properties.TryGetValue(value_4.Key, out value_5))
                        {
                            return false;
                        }

                        if (!object.Equals(value_4.Value, value_5))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public int GetHashCode(IRule obj)
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
                    foreach (var value_6 in obj.MessageStrings)
                    {
                        xor_0 ^= value_6.Key.GetHashCode();
                        if (value_6.Value != null)
                        {
                            xor_0 ^= value_6.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                if (obj.RichMessageStrings != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_1 = 0;
                    foreach (var value_7 in obj.RichMessageStrings)
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
                    result = (result * 31) + obj.Help.ValueGetHashCode();
                }

                var propertyBagHolder = obj as PropertyBagHolder;

                if (propertyBagHolder?.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_2 = 0;
                    foreach (var value_8 in propertyBagHolder.Properties)
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