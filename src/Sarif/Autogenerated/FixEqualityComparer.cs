// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Fix for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.56.0.0")]
    internal sealed class FixEqualityComparer : IEqualityComparer<Fix>
    {
        internal static readonly FixEqualityComparer Instance = new FixEqualityComparer();

        public bool Equals(Fix left, Fix right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (!Message.ValueComparer.Equals(left.Description, right.Description))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.FileChanges, right.FileChanges))
            {
                if (left.FileChanges == null || right.FileChanges == null)
                {
                    return false;
                }

                if (left.FileChanges.Count != right.FileChanges.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.FileChanges.Count; ++index_0)
                {
                    if (!FileChange.ValueComparer.Equals(left.FileChanges[index_0], right.FileChanges[index_0]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int GetHashCode(Fix obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Description != null)
                {
                    result = (result * 31) + obj.Description.ValueGetHashCode();
                }

                if (obj.FileChanges != null)
                {
                    foreach (var value_0 in obj.FileChanges)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            result = (result * 31) + value_0.ValueGetHashCode();
                        }
                    }
                }
            }

            return result;
        }
    }
}