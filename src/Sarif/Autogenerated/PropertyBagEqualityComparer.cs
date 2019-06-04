// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type PropertyBag for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    internal sealed class PropertyBagEqualityComparer : IEqualityComparer<PropertyBag>
    {
        internal static readonly PropertyBagEqualityComparer Instance = new PropertyBagEqualityComparer();

        public bool Equals(PropertyBag left, PropertyBag right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Tags, right.Tags))
            {
                if (left.Tags == null || right.Tags == null)
                {
                    return false;
                }

                if (left.Tags.Count != right.Tags.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.Tags.Count; ++index_0)
                {
                    if (left.Tags[index_0] != right.Tags[index_0])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int GetHashCode(PropertyBag obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Tags != null)
                {
                    foreach (var value_0 in obj.Tags)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            result = (result * 31) + value_0.GetHashCode();
                        }
                    }
                }
            }

            return result;
        }
    }
}