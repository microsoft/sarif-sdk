// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type LogicalLocationVersionOne for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    internal sealed class LogicalLocationVersionOneEqualityComparer : IEqualityComparer<LogicalLocationVersionOne>
    {
        internal static readonly LogicalLocationVersionOneEqualityComparer Instance = new LogicalLocationVersionOneEqualityComparer();

        public bool Equals(LogicalLocationVersionOne left, LogicalLocationVersionOne right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.Name != right.Name)
            {
                return false;
            }

            if (left.ParentKey != right.ParentKey)
            {
                return false;
            }

            if (left.Kind != right.Kind)
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(LogicalLocationVersionOne obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Name != null)
                {
                    result = (result * 31) + obj.Name.GetHashCode();
                }

                if (obj.ParentKey != null)
                {
                    result = (result * 31) + obj.ParentKey.GetHashCode();
                }

                if (obj.Kind != null)
                {
                    result = (result * 31) + obj.Kind.GetHashCode();
                }
            }

            return result;
        }
    }
}