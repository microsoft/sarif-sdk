// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type ThreadFlowLocation for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.56.0.0")]
    internal sealed class ThreadFlowLocationEqualityComparer : IEqualityComparer<ThreadFlowLocation>
    {
        internal static readonly ThreadFlowLocationEqualityComparer Instance = new ThreadFlowLocationEqualityComparer();

        public bool Equals(ThreadFlowLocation left, ThreadFlowLocation right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.Step != right.Step)
            {
                return false;
            }

            if (!Location.ValueComparer.Equals(left.Location, right.Location))
            {
                return false;
            }

            if (!Stack.ValueComparer.Equals(left.Stack, right.Stack))
            {
                return false;
            }

            if (left.Kind != right.Kind)
            {
                return false;
            }

            if (left.Module != right.Module)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.State, right.State))
            {
                if (left.State == null || right.State == null || left.State.Count != right.State.Count)
                {
                    return false;
                }

                foreach (var value_0 in left.State)
                {
                    string value_1;
                    if (!right.State.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (value_0.Value != value_1)
                    {
                        return false;
                    }
                }
            }

            if (left.NestingLevel != right.NestingLevel)
            {
                return false;
            }

            if (left.ExecutionOrder != right.ExecutionOrder)
            {
                return false;
            }

            if (left.Timestamp != right.Timestamp)
            {
                return false;
            }

            if (left.Importance != right.Importance)
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

        public int GetHashCode(ThreadFlowLocation obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                result = (result * 31) + obj.Step.GetHashCode();
                if (obj.Location != null)
                {
                    result = (result * 31) + obj.Location.ValueGetHashCode();
                }

                if (obj.Stack != null)
                {
                    result = (result * 31) + obj.Stack.ValueGetHashCode();
                }

                if (obj.Kind != null)
                {
                    result = (result * 31) + obj.Kind.GetHashCode();
                }

                if (obj.Module != null)
                {
                    result = (result * 31) + obj.Module.GetHashCode();
                }

                if (obj.State != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_4 in obj.State)
                    {
                        xor_0 ^= value_4.Key.GetHashCode();
                        if (value_4.Value != null)
                        {
                            xor_0 ^= value_4.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                result = (result * 31) + obj.NestingLevel.GetHashCode();
                result = (result * 31) + obj.ExecutionOrder.GetHashCode();
                result = (result * 31) + obj.Timestamp.GetHashCode();
                result = (result * 31) + obj.Importance.GetHashCode();
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