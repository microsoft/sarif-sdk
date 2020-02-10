// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type RegionVersionOne for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    internal sealed class RegionVersionOneEqualityComparer : IEqualityComparer<RegionVersionOne>
    {
        internal static readonly RegionVersionOneEqualityComparer Instance = new RegionVersionOneEqualityComparer();

        public bool Equals(RegionVersionOne left, RegionVersionOne right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.StartLine != right.StartLine)
            {
                return false;
            }

            if (left.StartColumn != right.StartColumn)
            {
                return false;
            }

            if (left.EndLine != right.EndLine)
            {
                return false;
            }

            if (left.EndColumn != right.EndColumn)
            {
                return false;
            }

            if (left.Offset != right.Offset)
            {
                return false;
            }

            if (left.Length != right.Length)
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(RegionVersionOne obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                result = (result * 31) + obj.StartLine.GetHashCode();
                result = (result * 31) + obj.StartColumn.GetHashCode();
                result = (result * 31) + obj.EndLine.GetHashCode();
                result = (result * 31) + obj.EndColumn.GetHashCode();
                result = (result * 31) + obj.Offset.GetHashCode();
                result = (result * 31) + obj.Length.GetHashCode();
            }

            return result;
        }
    }
}