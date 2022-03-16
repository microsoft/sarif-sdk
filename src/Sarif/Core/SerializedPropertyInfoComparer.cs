// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.CodeAnalysis.Sarif.Comparers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type SerializedPropertyInfo.
    /// </summary>
    internal sealed class SerializedPropertyInfoComparer : IComparer<SerializedPropertyInfo>
    {
        internal static readonly SerializedPropertyInfoComparer Instance = new SerializedPropertyInfoComparer();

        public int Compare(SerializedPropertyInfo left, SerializedPropertyInfo right)
        {
            int compareResult = 0;

            if (left.TryReferenceCompares(right, out compareResult))
            {
                return compareResult;
            }

            compareResult = left.IsString.CompareTo(right.IsString);

            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = string.Compare(left.SerializedValue, right.SerializedValue);

            return compareResult;
        }
    }
}
