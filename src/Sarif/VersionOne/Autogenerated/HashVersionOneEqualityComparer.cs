// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type HashVersionOne for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    internal sealed class HashVersionOneEqualityComparer : IEqualityComparer<HashVersionOne>
    {
        internal static readonly HashVersionOneEqualityComparer Instance = new HashVersionOneEqualityComparer();

        public bool Equals(HashVersionOne left, HashVersionOne right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.Value != right.Value)
            {
                return false;
            }

            if (left.Algorithm != right.Algorithm)
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(HashVersionOne obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Value != null)
                {
                    result = (result * 31) + obj.Value.GetHashCode();
                }

                result = (result * 31) + obj.Algorithm.GetHashCode();
            }

            return result;
        }
    }
}