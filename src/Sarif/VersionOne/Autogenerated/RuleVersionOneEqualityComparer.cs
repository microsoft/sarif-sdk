// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type RuleVersionOne for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    internal sealed class RuleVersionOneEqualityComparer : IEqualityComparer<RuleVersionOne>
    {
        internal static readonly RuleVersionOneEqualityComparer Instance = new RuleVersionOneEqualityComparer();

        public bool Equals(RuleVersionOne left, RuleVersionOne right)
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

            if (!object.ReferenceEquals(left.MessageFormats, right.MessageFormats))
            {
                if (left.MessageFormats == null || right.MessageFormats == null || left.MessageFormats.Count != right.MessageFormats.Count)
                {
                    return false;
                }

                foreach (var value_0 in left.MessageFormats)
                {
                    string value_1;
                    if (!right.MessageFormats.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (value_0.Value != value_1)
                    {
                        return false;
                    }
                }
            }

            if (left.Configuration != right.Configuration)
            {
                return false;
            }

            if (left.DefaultLevel != right.DefaultLevel)
            {
                return false;
            }

            if (left.HelpUri != right.HelpUri)
            {
                return false;
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

        public int GetHashCode(RuleVersionOne obj)
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

                if (obj.MessageFormats != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_4 in obj.MessageFormats)
                    {
                        xor_0 ^= value_4.Key.GetHashCode();
                        if (value_4.Value != null)
                        {
                            xor_0 ^= value_4.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                result = (result * 31) + obj.Configuration.GetHashCode();
                result = (result * 31) + obj.DefaultLevel.GetHashCode();
                if (obj.HelpUri != null)
                {
                    result = (result * 31) + obj.HelpUri.GetHashCode();
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