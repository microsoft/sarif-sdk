// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type OutputDescriptor for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.61.0.0")]
    internal sealed class OutputDescriptorEqualityComparer : IEqualityComparer<OutputDescriptor>
    {
        internal static readonly OutputDescriptorEqualityComparer Instance = new OutputDescriptorEqualityComparer();

        public bool Equals(OutputDescriptor left, OutputDescriptor right)
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

            if (!object.Equals(left.MessageStrings, right.MessageStrings))
            {
                return false;
            }

            if (!object.Equals(left.RichMessageStrings, right.RichMessageStrings))
            {
                return false;
            }

            if (!OutputConfiguration.ValueComparer.Equals(left.DefaultConfiguration, right.DefaultConfiguration))
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

        public int GetHashCode(OutputDescriptor obj)
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
                    foreach (var value_2 in obj.DeprecatedIds)
                    {
                        result = result * 31;
                        if (value_2 != null)
                        {
                            result = (result * 31) + value_2.GetHashCode();
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
                    result = (result * 31) + obj.MessageStrings.GetHashCode();
                }

                if (obj.RichMessageStrings != null)
                {
                    result = (result * 31) + obj.RichMessageStrings.GetHashCode();
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