// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type NotificationVersionOne for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    internal sealed class NotificationVersionOneEqualityComparer : IEqualityComparer<NotificationVersionOne>
    {
        internal static readonly NotificationVersionOneEqualityComparer Instance = new NotificationVersionOneEqualityComparer();

        public bool Equals(NotificationVersionOne left, NotificationVersionOne right)
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

            if (left.RuleId != right.RuleId)
            {
                return false;
            }

            if (left.RuleKey != right.RuleKey)
            {
                return false;
            }

            if (!PhysicalLocationVersionOne.ValueComparer.Equals(left.PhysicalLocation, right.PhysicalLocation))
            {
                return false;
            }

            if (left.Message != right.Message)
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

            if (left.Time != right.Time)
            {
                return false;
            }

            if (!ExceptionDataVersionOne.ValueComparer.Equals(left.Exception, right.Exception))
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

        public int GetHashCode(NotificationVersionOne obj)
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

                if (obj.RuleId != null)
                {
                    result = (result * 31) + obj.RuleId.GetHashCode();
                }

                if (obj.RuleKey != null)
                {
                    result = (result * 31) + obj.RuleKey.GetHashCode();
                }

                if (obj.PhysicalLocation != null)
                {
                    result = (result * 31) + obj.PhysicalLocation.ValueGetHashCode();
                }

                if (obj.Message != null)
                {
                    result = (result * 31) + obj.Message.GetHashCode();
                }

                result = (result * 31) + obj.Level.GetHashCode();
                result = (result * 31) + obj.ThreadId.GetHashCode();
                result = (result * 31) + obj.Time.GetHashCode();
                if (obj.Exception != null)
                {
                    result = (result * 31) + obj.Exception.ValueGetHashCode();
                }

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