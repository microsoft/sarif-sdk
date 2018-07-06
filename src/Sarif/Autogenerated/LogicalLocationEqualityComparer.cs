// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type LogicalLocation for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    internal sealed class LogicalLocationEqualityComparer : IEqualityComparer<LogicalLocation>
    {
        internal static readonly LogicalLocationEqualityComparer Instance = new LogicalLocationEqualityComparer();

        public bool Equals(LogicalLocation left, LogicalLocation right)
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

            if (left.FullyQualifiedName != right.FullyQualifiedName)
            {
                return false;
            }

            if (left.DecoratedName != right.DecoratedName)
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

        public int GetHashCode(LogicalLocation obj)
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

                if (obj.FullyQualifiedName != null)
                {
                    result = (result * 31) + obj.FullyQualifiedName.GetHashCode();
                }

                if (obj.DecoratedName != null)
                {
                    result = (result * 31) + obj.DecoratedName.GetHashCode();
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