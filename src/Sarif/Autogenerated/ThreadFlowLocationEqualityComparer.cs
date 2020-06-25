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
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
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

            if (left.Index != right.Index)
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

            if (!object.ReferenceEquals(left.Kinds, right.Kinds))
            {
                if (left.Kinds == null || right.Kinds == null)
                {
                    return false;
                }

                if (left.Kinds.Count != right.Kinds.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.Kinds.Count; ++index_0)
                {
                    if (left.Kinds[index_0] != right.Kinds[index_0])
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.Taxa, right.Taxa))
            {
                if (left.Taxa == null || right.Taxa == null)
                {
                    return false;
                }

                if (left.Taxa.Count != right.Taxa.Count)
                {
                    return false;
                }

                for (int index_1 = 0; index_1 < left.Taxa.Count; ++index_1)
                {
                    if (!ReportingDescriptorReference.ValueComparer.Equals(left.Taxa[index_1], right.Taxa[index_1]))
                    {
                        return false;
                    }
                }
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
                    MultiformatMessageString value_1;
                    if (!right.State.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (!MultiformatMessageString.ValueComparer.Equals(value_0.Value, value_1))
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

            if (left.ExecutionTimeUtc != right.ExecutionTimeUtc)
            {
                return false;
            }

            if (left.Importance != right.Importance)
            {
                return false;
            }

            if (!WebRequest.ValueComparer.Equals(left.WebRequest, right.WebRequest))
            {
                return false;
            }

            if (!WebResponse.ValueComparer.Equals(left.WebResponse, right.WebResponse))
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
                result = (result * 31) + obj.Index.GetHashCode();
                if (obj.Location != null)
                {
                    result = (result * 31) + obj.Location.ValueGetHashCode();
                }

                if (obj.Stack != null)
                {
                    result = (result * 31) + obj.Stack.ValueGetHashCode();
                }

                if (obj.Kinds != null)
                {
                    foreach (var value_4 in obj.Kinds)
                    {
                        result = result * 31;
                        if (value_4 != null)
                        {
                            result = (result * 31) + value_4.GetHashCode();
                        }
                    }
                }

                if (obj.Taxa != null)
                {
                    foreach (var value_5 in obj.Taxa)
                    {
                        result = result * 31;
                        if (value_5 != null)
                        {
                            result = (result * 31) + value_5.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Module != null)
                {
                    result = (result * 31) + obj.Module.GetHashCode();
                }

                if (obj.State != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_6 in obj.State)
                    {
                        xor_0 ^= value_6.Key.GetHashCode();
                        if (value_6.Value != null)
                        {
                            xor_0 ^= value_6.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                result = (result * 31) + obj.NestingLevel.GetHashCode();
                result = (result * 31) + obj.ExecutionOrder.GetHashCode();
                result = (result * 31) + obj.ExecutionTimeUtc.GetHashCode();
                result = (result * 31) + obj.Importance.GetHashCode();
                if (obj.WebRequest != null)
                {
                    result = (result * 31) + obj.WebRequest.ValueGetHashCode();
                }

                if (obj.WebResponse != null)
                {
                    result = (result * 31) + obj.WebResponse.ValueGetHashCode();
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_1 = 0;
                    foreach (var value_7 in obj.Properties)
                    {
                        xor_1 ^= value_7.Key.GetHashCode();
                        if (value_7.Value != null)
                        {
                            xor_1 ^= value_7.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_1;
                }
            }

            return result;
        }
    }
}