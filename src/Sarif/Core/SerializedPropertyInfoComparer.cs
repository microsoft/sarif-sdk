// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

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
            if (object.ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            int compareResult = string.Compare(left.SerializedValue, right.SerializedValue);

            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.IsString.CompareTo(right.IsString);

            return compareResult;
        }
    }
}
