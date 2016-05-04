// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Notification for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.22.0.0")]
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

            if (left.Id != right.Id)
            {
                return false;
            }

            if (left.RuleId != right.RuleId)
            {
                return false;
            }

            if (!PhysicalLocation.ValueComparer.Equals(left.AnalysisTarget, right.AnalysisTarget))
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

            if (left.Time != right.Time)
            {
                return false;
            }

            if (!ExceptionData.ValueComparer.Equals(left.Exception, right.Exception))
            {
                return false;
            }

            if (!Object.ReferenceEquals(left.Properties, right.Properties))
            {
                if (left.Properties == null || right.Properties == null || left.Properties.Count != right.Properties.Count)
                {
                    return false;
                }

                foreach (var value_0 in left.Properties)
                {
                    string value_1;
                    if (!right.Properties.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (value_0.Value != value_1)
                    {
                        return false;
                    }
                }
            }

            if (!Object.ReferenceEquals(left.Tags, right.Tags))
            {
                if (left.Tags == null || right.Tags == null)
                {
                    return false;
                }

                if (left.Tags.Count != right.Tags.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.Tags.Count; ++index_0)
                {
                    if (left.Tags[index_0] != right.Tags[index_0])
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
                if (obj.Id != null)
                {
                    result = (result * 31) + obj.Id.GetHashCode();
                }

                if (obj.RuleId != null)
                {
                    result = (result * 31) + obj.RuleId.GetHashCode();
                }

                if (obj.AnalysisTarget != null)
                {
                    result = (result * 31) + obj.AnalysisTarget.ValueGetHashCode();
                }

                if (obj.Message != null)
                {
                    result = (result * 31) + obj.Message.GetHashCode();
                }

                result = (result * 31) + obj.Level.GetHashCode();
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

                if (obj.Tags != null)
                {
                    foreach (var value_3 in obj.Tags)
                    {
                        result = result * 31;
                        if (value_3 != null)
                        {
                            result = (result * 31) + value_3.GetHashCode();
                        }
                    }
                }
            }

            return result;
        }
    }
}