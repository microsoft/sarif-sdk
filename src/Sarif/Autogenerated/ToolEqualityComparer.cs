// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Tool for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.22.0.0")]
    internal sealed class ToolEqualityComparer : IEqualityComparer<Tool>
    {
        internal static readonly ToolEqualityComparer Instance = new ToolEqualityComparer();

        public bool Equals(Tool left, Tool right)
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

            if (left.FullName != right.FullName)
            {
                return false;
            }

            if (left.Version != right.Version)
            {
                return false;
            }

            if (left.SemanticVersion != right.SemanticVersion)
            {
                return false;
            }

            if (left.FileVersion != right.FileVersion)
            {
                return false;
            }

            if (left.Language != right.Language)
            {
                return false;
            }

            if (!Object.ReferenceEquals(left.Properties, right.Properties))
            {
                if (left.Properties == null || right.Properties == null || left.Properties.Count != right.Properties.Count)
                {
                    return false;
                }

                foreach (var value_0 in left.Properties)
                {
                    string value_1;
                    if (!right.Properties.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (value_0.Value != value_1)
                    {
                        return false;
                    }
                }
            }

            if (!Object.ReferenceEquals(left.Tags, right.Tags))
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

        public int GetHashCode(Tool obj)
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

                if (obj.FullName != null)
                {
                    result = (result * 31) + obj.FullName.GetHashCode();
                }

                if (obj.Version != null)
                {
                    result = (result * 31) + obj.Version.GetHashCode();
                }

                if (obj.SemanticVersion != null)
                {
                    result = (result * 31) + obj.SemanticVersion.GetHashCode();
                }

                if (obj.FileVersion != null)
                {
                    result = (result * 31) + obj.FileVersion.GetHashCode();
                }

                if (obj.Language != null)
                {
                    result = (result * 31) + obj.Language.GetHashCode();
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_2 in obj.Properties)
                    {
                        xor_0 ^= value_2.Key.GetHashCode();
                        if (value_2.Value != null)
                        {
                            xor_0 ^= value_2.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                if (obj.Tags != null)
                {
                    foreach (var value_3 in obj.Tags)
                    {
                        result = result * 31;
                        if (value_3 != null)
                        {
                            result = (result * 31) + value_3.GetHashCode();
                        }
                    }
                }
            }

            return result;
        }
    }
}