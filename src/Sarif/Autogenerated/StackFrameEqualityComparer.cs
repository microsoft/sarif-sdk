// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type StackFrame for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.22.0.0")]
    internal sealed class StackFrameEqualityComparer : IEqualityComparer<StackFrame>
    {
        internal static readonly StackFrameEqualityComparer Instance = new StackFrameEqualityComparer();

        public bool Equals(StackFrame left, StackFrame right)
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

            if (left.Uri != right.Uri)
            {
                return false;
            }

            if (left.Line != right.Line)
            {
                return false;
            }

            if (left.Column != right.Column)
            {
                return false;
            }

            if (left.Module != right.Module)
            {
                return false;
            }

            if (left.FullyQualifiedLogicalName != right.FullyQualifiedLogicalName)
            {
                return false;
            }

            if (left.LogicalLocationKey != right.LogicalLocationKey)
            {
                return false;
            }

            if (left.Address != right.Address)
            {
                return false;
            }

            if (left.Offset != right.Offset)
            {
                return false;
            }

            if (!Object.ReferenceEquals(left.Parameters, right.Parameters))
            {
                if (left.Parameters == null || right.Parameters == null)
                {
                    return false;
                }

                if (left.Parameters.Count != right.Parameters.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.Parameters.Count; ++index_0)
                {
                    if (left.Parameters[index_0] != right.Parameters[index_0])
                    {
                        return false;
                    }
                }
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

                for (int index_1 = 0; index_1 < left.Tags.Count; ++index_1)
                {
                    if (left.Tags[index_1] != right.Tags[index_1])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int GetHashCode(StackFrame obj)
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

                if (obj.Uri != null)
                {
                    result = (result * 31) + obj.Uri.GetHashCode();
                }

                result = (result * 31) + obj.Line.GetHashCode();
                result = (result * 31) + obj.Column.GetHashCode();
                if (obj.Module != null)
                {
                    result = (result * 31) + obj.Module.GetHashCode();
                }

                if (obj.FullyQualifiedLogicalName != null)
                {
                    result = (result * 31) + obj.FullyQualifiedLogicalName.GetHashCode();
                }

                if (obj.LogicalLocationKey != null)
                {
                    result = (result * 31) + obj.LogicalLocationKey.GetHashCode();
                }

                result = (result * 31) + obj.Address.GetHashCode();
                result = (result * 31) + obj.Offset.GetHashCode();
                if (obj.Parameters != null)
                {
                    foreach (var value_2 in obj.Parameters)
                    {
                        result = result * 31;
                        if (value_2 != null)
                        {
                            result = (result * 31) + value_2.GetHashCode();
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

                if (obj.Tags != null)
                {
                    foreach (var value_4 in obj.Tags)
                    {
                        result = result * 31;
                        if (value_4 != null)
                        {
                            result = (result * 31) + value_4.GetHashCode();
                        }
                    }
                }
            }

            return result;
        }
    }
}