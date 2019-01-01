// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type ExternalPropertyFiles for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.58.0.0")]
    internal sealed class ExternalPropertyFilesEqualityComparer : IEqualityComparer<ExternalPropertyFiles>
    {
        internal static readonly ExternalPropertyFilesEqualityComparer Instance = new ExternalPropertyFilesEqualityComparer();

        public bool Equals(ExternalPropertyFiles left, ExternalPropertyFiles right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (!ExternalPropertyFile.ValueComparer.Equals(left.Conversion, right.Conversion))
            {
                return false;
            }

            if (!ExternalPropertyFile.ValueComparer.Equals(left.Graphs, right.Graphs))
            {
                return false;
            }

            if (!ExternalPropertyFile.ValueComparer.Equals(left.Resources, right.Resources))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Files, right.Files))
            {
                if (left.Files == null || right.Files == null)
                {
                    return false;
                }

                if (left.Files.Count != right.Files.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.Files.Count; ++index_0)
                {
                    if (!ExternalPropertyFile.ValueComparer.Equals(left.Files[index_0], right.Files[index_0]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.Invocations, right.Invocations))
            {
                if (left.Invocations == null || right.Invocations == null)
                {
                    return false;
                }

                if (left.Invocations.Count != right.Invocations.Count)
                {
                    return false;
                }

                for (int index_1 = 0; index_1 < left.Invocations.Count; ++index_1)
                {
                    if (!ExternalPropertyFile.ValueComparer.Equals(left.Invocations[index_1], right.Invocations[index_1]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.LogicalLocations, right.LogicalLocations))
            {
                if (left.LogicalLocations == null || right.LogicalLocations == null)
                {
                    return false;
                }

                if (left.LogicalLocations.Count != right.LogicalLocations.Count)
                {
                    return false;
                }

                for (int index_2 = 0; index_2 < left.LogicalLocations.Count; ++index_2)
                {
                    if (!ExternalPropertyFile.ValueComparer.Equals(left.LogicalLocations[index_2], right.LogicalLocations[index_2]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.Results, right.Results))
            {
                if (left.Results == null || right.Results == null)
                {
                    return false;
                }

                if (left.Results.Count != right.Results.Count)
                {
                    return false;
                }

                for (int index_3 = 0; index_3 < left.Results.Count; ++index_3)
                {
                    if (!ExternalPropertyFile.ValueComparer.Equals(left.Results[index_3], right.Results[index_3]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.Properties, right.Properties))
            {
                if (left.Properties == null || right.Properties == null || left.Properties.Count != right.Properties.Count)
                {
                    return false;
                }

                foreach (var value_0 in left.Properties)
                {
                    SerializedPropertyInfo value_1;
                    if (!right.Properties.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (!object.Equals(value_0.Value, value_1))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int GetHashCode(ExternalPropertyFiles obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Conversion != null)
                {
                    result = (result * 31) + obj.Conversion.ValueGetHashCode();
                }

                if (obj.Graphs != null)
                {
                    result = (result * 31) + obj.Graphs.ValueGetHashCode();
                }

                if (obj.Resources != null)
                {
                    result = (result * 31) + obj.Resources.ValueGetHashCode();
                }

                if (obj.Files != null)
                {
                    foreach (var value_2 in obj.Files)
                    {
                        result = result * 31;
                        if (value_2 != null)
                        {
                            result = (result * 31) + value_2.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Invocations != null)
                {
                    foreach (var value_3 in obj.Invocations)
                    {
                        result = result * 31;
                        if (value_3 != null)
                        {
                            result = (result * 31) + value_3.ValueGetHashCode();
                        }
                    }
                }

                if (obj.LogicalLocations != null)
                {
                    foreach (var value_4 in obj.LogicalLocations)
                    {
                        result = result * 31;
                        if (value_4 != null)
                        {
                            result = (result * 31) + value_4.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Results != null)
                {
                    foreach (var value_5 in obj.Results)
                    {
                        result = result * 31;
                        if (value_5 != null)
                        {
                            result = (result * 31) + value_5.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_6 in obj.Properties)
                    {
                        xor_0 ^= value_6.Key.GetHashCode();
                        if (value_6.Value != null)
                        {
                            xor_0 ^= value_6.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }
            }

            return result;
        }
    }
}