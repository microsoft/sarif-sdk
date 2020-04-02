// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type ExceptionData for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    internal sealed class ExceptionDataEqualityComparer : IEqualityComparer<ExceptionData>
    {
        internal static readonly ExceptionDataEqualityComparer Instance = new ExceptionDataEqualityComparer();

        public bool Equals(ExceptionData left, ExceptionData right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.Kind != right.Kind)
            {
                return false;
            }

            if (left.Message != right.Message)
            {
                return false;
            }

            if (!Stack.ValueComparer.Equals(left.Stack, right.Stack))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.InnerExceptions, right.InnerExceptions))
            {
                if (left.InnerExceptions == null || right.InnerExceptions == null)
                {
                    return false;
                }

                if (left.InnerExceptions.Count != right.InnerExceptions.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.InnerExceptions.Count; ++index_0)
                {
                    if (!ExceptionData.ValueComparer.Equals(left.InnerExceptions[index_0], right.InnerExceptions[index_0]))
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

        public int GetHashCode(ExceptionData obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Kind != null)
                {
                    result = (result * 31) + obj.Kind.GetHashCode();
                }

                if (obj.Message != null)
                {
                    result = (result * 31) + obj.Message.GetHashCode();
                }

                if (obj.Stack != null)
                {
                    result = (result * 31) + obj.Stack.ValueGetHashCode();
                }

                if (obj.InnerExceptions != null)
                {
                    foreach (var value_2 in obj.InnerExceptions)
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