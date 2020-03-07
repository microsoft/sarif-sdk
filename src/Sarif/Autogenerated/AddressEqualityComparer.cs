// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Address for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    internal sealed class AddressEqualityComparer : IEqualityComparer<Address>
    {
        internal static readonly AddressEqualityComparer Instance = new AddressEqualityComparer();

        public bool Equals(Address left, Address right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.AbsoluteAddress != right.AbsoluteAddress)
            {
                return false;
            }

            if (left.RelativeAddress != right.RelativeAddress)
            {
                return false;
            }

            if (left.Length != right.Length)
            {
                return false;
            }

            if (left.Kind != right.Kind)
            {
                return false;
            }

            if (left.Name != right.Name)
            {
                return false;
            }

            if (left.FullyQualifiedName != right.FullyQualifiedName)
            {
                return false;
            }

            if (left.OffsetFromParent != right.OffsetFromParent)
            {
                return false;
            }

            if (left.Index != right.Index)
            {
                return false;
            }

            if (left.ParentIndex != right.ParentIndex)
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

        public int GetHashCode(Address obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                result = (result * 31) + obj.AbsoluteAddress.GetHashCode();
                result = (result * 31) + obj.RelativeAddress.GetHashCode();
                result = (result * 31) + obj.Length.GetHashCode();
                if (obj.Kind != null)
                {
                    result = (result * 31) + obj.Kind.GetHashCode();
                }

                if (obj.Name != null)
                {
                    result = (result * 31) + obj.Name.GetHashCode();
                }

                if (obj.FullyQualifiedName != null)
                {
                    result = (result * 31) + obj.FullyQualifiedName.GetHashCode();
                }

                result = (result * 31) + obj.OffsetFromParent.GetHashCode();
                result = (result * 31) + obj.Index.GetHashCode();
                result = (result * 31) + obj.ParentIndex.GetHashCode();
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