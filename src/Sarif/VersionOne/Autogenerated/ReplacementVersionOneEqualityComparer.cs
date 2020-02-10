// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type ReplacementVersionOne for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    internal sealed class ReplacementVersionOneEqualityComparer : IEqualityComparer<ReplacementVersionOne>
    {
        internal static readonly ReplacementVersionOneEqualityComparer Instance = new ReplacementVersionOneEqualityComparer();

        public bool Equals(ReplacementVersionOne left, ReplacementVersionOne right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.Offset != right.Offset)
            {
                return false;
            }

            if (left.DeletedLength != right.DeletedLength)
            {
                return false;
            }

            if (left.InsertedBytes != right.InsertedBytes)
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(ReplacementVersionOne obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                result = (result * 31) + obj.Offset.GetHashCode();
                result = (result * 31) + obj.DeletedLength.GetHashCode();
                if (obj.InsertedBytes != null)
                {
                    result = (result * 31) + obj.InsertedBytes.GetHashCode();
                }
            }

            return result;
        }
    }
}