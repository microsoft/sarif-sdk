// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Address for sorting.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.3.0")]
    internal sealed class AddressComparer : IComparer<Address>
    {
        internal static readonly AddressComparer Instance = new AddressComparer();

        public int Compare(Address left, Address right)
        {
            int compareResult = 0;
            if (left.TryReferenceCompares(right, out compareResult))
            {
                return compareResult;
            }

            compareResult = left.AbsoluteAddress.CompareTo(right.AbsoluteAddress);
            if (compareResult != 0)
            {
                return compareResult;
            }

            if (left.RelativeAddress.TryReferenceCompares(right.RelativeAddress, out compareResult))
            {
                return compareResult;
            }

            compareResult = left.RelativeAddress.Value.CompareTo(right.RelativeAddress.Value);
            if (compareResult != 0)
            {
                return compareResult;
            }

            if (left.Length.TryReferenceCompares(right.Length, out compareResult))
            {
                return compareResult;
            }

            compareResult = left.Length.Value.CompareTo(right.Length.Value);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = string.Compare(left.Kind, right.Kind);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = string.Compare(left.Name, right.Name);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = string.Compare(left.FullyQualifiedName, right.FullyQualifiedName);
            if (compareResult != 0)
            {
                return compareResult;
            }

            if (left.OffsetFromParent.TryReferenceCompares(right.OffsetFromParent, out compareResult))
            {
                return compareResult;
            }
            compareResult = left.OffsetFromParent.Value.CompareTo(right.OffsetFromParent.Value);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.Index.CompareTo(right.Index);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.ParentIndex.CompareTo(right.ParentIndex);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.Properties.DictionaryCompares(right.Properties, SerializedPropertyInfoComparer.Instance);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}
