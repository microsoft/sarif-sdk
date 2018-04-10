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

            if (left.ThreadId != right.ThreadId)
            {
                return false;
            }

            if (left.Kind != right.Kind)
            {
                return false;
            }

            if (left.TaintKind != right.TaintKind)
            {
                return false;
            }

            if (left.Target != right.Target)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Values, right.Values))
            {
                if (left.Values == null || right.Values == null)
                {
                    return false;
                }

                if (left.Values.Count != right.Values.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.Values.Count; ++index_0)
                {
                    if (left.Values[index_0] != right.Values[index_0])
                    {
                        return false;
                    }
                }
            }

            if (!object.Equals(left.State, right.State))
            {
                return false;
            }

            if (left.TargetKey != right.TargetKey)
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

                result = (result * 31) + obj.ThreadId.GetHashCode();
                result = (result * 31) + obj.Kind.GetHashCode();
                result = (result * 31) + obj.TaintKind.GetHashCode();
                if (obj.Target != null)
                {
                    result = (result * 31) + obj.Target.GetHashCode();
                }

                if (obj.Values != null)
                {
                    foreach (var value_2 in obj.Values)
                    {
                        result = result * 31;
                        if (value_2 != null)
                        {
                            result = (result * 31) + value_2.GetHashCode();
                        }
                    }
                }

                if (obj.State != null)
                {
                    result = (result * 31) + obj.State.GetHashCode();
                }

                if (obj.TargetKey != null)
                {
                    result = (result * 31) + obj.TargetKey.GetHashCode();
                }

                result = (result * 31) + obj.Importance.GetHashCode();
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