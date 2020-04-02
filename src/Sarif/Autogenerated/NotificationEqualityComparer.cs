// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Notification for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    internal sealed class NotificationEqualityComparer : IEqualityComparer<Notification>
    {
        internal static readonly NotificationEqualityComparer Instance = new NotificationEqualityComparer();

        public bool Equals(Notification left, Notification right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Locations, right.Locations))
            {
                if (left.Locations == null || right.Locations == null)
                {
                    return false;
                }

                if (left.Locations.Count != right.Locations.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.Locations.Count; ++index_0)
                {
                    if (!Location.ValueComparer.Equals(left.Locations[index_0], right.Locations[index_0]))
                    {
                        return false;
                    }
                }
            }

            if (!Message.ValueComparer.Equals(left.Message, right.Message))
            {
                return false;
            }

            if (left.Level != right.Level)
            {
                return false;
            }

            if (left.ThreadId != right.ThreadId)
            {
                return false;
            }

            if (left.TimeUtc != right.TimeUtc)
            {
                return false;
            }

            if (!ExceptionData.ValueComparer.Equals(left.Exception, right.Exception))
            {
                return false;
            }

            if (!ReportingDescriptorReference.ValueComparer.Equals(left.Descriptor, right.Descriptor))
            {
                return false;
            }

            if (!ReportingDescriptorReference.ValueComparer.Equals(left.AssociatedRule, right.AssociatedRule))
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

        public int GetHashCode(Notification obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Locations != null)
                {
                    foreach (var value_2 in obj.Locations)
                    {
                        result = result * 31;
                        if (value_2 != null)
                        {
                            result = (result * 31) + value_2.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Message != null)
                {
                    result = (result * 31) + obj.Message.ValueGetHashCode();
                }

                result = (result * 31) + obj.Level.GetHashCode();
                result = (result * 31) + obj.ThreadId.GetHashCode();
                result = (result * 31) + obj.TimeUtc.GetHashCode();
                if (obj.Exception != null)
                {
                    result = (result * 31) + obj.Exception.ValueGetHashCode();
                }

                if (obj.Descriptor != null)
                {
                    result = (result * 31) + obj.Descriptor.ValueGetHashCode();
                }

                if (obj.AssociatedRule != null)
                {
                    result = (result * 31) + obj.AssociatedRule.ValueGetHashCode();
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