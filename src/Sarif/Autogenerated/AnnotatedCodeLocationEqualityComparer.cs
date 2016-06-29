// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type AnnotatedCodeLocation for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.42.0.0")]
    internal sealed class AnnotatedCodeLocationEqualityComparer : IEqualityComparer<AnnotatedCodeLocation>
    {
        internal static readonly AnnotatedCodeLocationEqualityComparer Instance = new AnnotatedCodeLocationEqualityComparer();

        public bool Equals(AnnotatedCodeLocation left, AnnotatedCodeLocation right)
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

            if (left.Step != right.Step)
            {
                return false;
            }

            if (!PhysicalLocation.ValueComparer.Equals(left.PhysicalLocation, right.PhysicalLocation))
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

            if (left.Module != right.Module)
            {
                return false;
            }

            if (left.ThreadId != right.ThreadId)
            {
                return false;
            }

            if (left.Message != right.Message)
            {
                return false;
            }

            if (left.Kind != right.Kind)
            {
                return false;
            }

            if (left.Essential != right.Essential)
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

        public int GetHashCode(AnnotatedCodeLocation obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                result = (result * 31) + obj.Id.GetHashCode();
                result = (result * 31) + obj.Step.GetHashCode();
                if (obj.PhysicalLocation != null)
                {
                    result = (result * 31) + obj.PhysicalLocation.ValueGetHashCode();
                }

                if (obj.FullyQualifiedLogicalName != null)
                {
                    result = (result * 31) + obj.FullyQualifiedLogicalName.GetHashCode();
                }

                if (obj.LogicalLocationKey != null)
                {
                    result = (result * 31) + obj.LogicalLocationKey.GetHashCode();
                }

                if (obj.Module != null)
                {
                    result = (result * 31) + obj.Module.GetHashCode();
                }

                result = (result * 31) + obj.ThreadId.GetHashCode();
                if (obj.Message != null)
                {
                    result = (result * 31) + obj.Message.GetHashCode();
                }

                result = (result * 31) + obj.Kind.GetHashCode();
                result = (result * 31) + obj.Essential.GetHashCode();
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