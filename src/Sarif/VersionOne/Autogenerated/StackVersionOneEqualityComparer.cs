// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type StackVersionOne for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    internal sealed class StackVersionOneEqualityComparer : IEqualityComparer<StackVersionOne>
    {
        internal static readonly StackVersionOneEqualityComparer Instance = new StackVersionOneEqualityComparer();

        public bool Equals(StackVersionOne left, StackVersionOne right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.Message != right.Message)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Frames, right.Frames))
            {
                if (left.Frames == null || right.Frames == null)
                {
                    return false;
                }

                if (left.Frames.Count != right.Frames.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.Frames.Count; ++index_0)
                {
                    if (!StackFrameVersionOne.ValueComparer.Equals(left.Frames[index_0], right.Frames[index_0]))
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

        public int GetHashCode(StackVersionOne obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Message != null)
                {
                    result = (result * 31) + obj.Message.GetHashCode();
                }

                if (obj.Frames != null)
                {
                    foreach (var value_2 in obj.Frames)
                    {
                        result = result * 31;
                        if (value_2 != null)
                        {
                            result = (result * 31) + value_2.ValueGetHashCode();
                        }
                    }
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