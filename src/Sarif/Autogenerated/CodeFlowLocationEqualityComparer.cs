// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type CodeFlowLocation for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    internal sealed class CodeFlowLocationEqualityComparer : IEqualityComparer<CodeFlowLocation>
    {
        internal static readonly CodeFlowLocationEqualityComparer Instance = new CodeFlowLocationEqualityComparer();

        public bool Equals(CodeFlowLocation left, CodeFlowLocation right)
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

            if (left.Module != right.Module)
            {
                return false;
            }

            if (!object.Equals(left.State, right.State))
            {
                return false;
            }

            if (left.NestingLevel != right.NestingLevel)
            {
                return false;
            }

            if (left.ExecutionOrder != right.ExecutionOrder)
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

        public int GetHashCode(CodeFlowLocation obj)
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

                if (obj.Module != null)
                {
                    result = (result * 31) + obj.Module.GetHashCode();
                }

                if (obj.State != null)
                {
                    result = (result * 31) + obj.State.GetHashCode();
                }

                result = (result * 31) + obj.NestingLevel.GetHashCode();
                result = (result * 31) + obj.ExecutionOrder.GetHashCode();
                result = (result * 31) + obj.Importance.GetHashCode();
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