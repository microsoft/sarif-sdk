// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Run for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    internal sealed class RunEqualityComparer : IEqualityComparer<Run>
    {
        internal static readonly RunEqualityComparer Instance = new RunEqualityComparer();

        public bool Equals(Run left, Run right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (!Tool.ValueComparer.Equals(left.Tool, right.Tool))
            {
                return false;
            }

            if (!Invocation.ValueComparer.Equals(left.Invocation, right.Invocation))
            {
                return false;
            }

            if (!Conversion.ValueComparer.Equals(left.Conversion, right.Conversion))
            {
                return false;
            }

            if (!object.Equals(left.OriginalUriBaseIds, right.OriginalUriBaseIds))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Files, right.Files))
            {
                if (left.Files == null || right.Files == null || left.Files.Count != right.Files.Count)
                {
                    return false;
                }

                foreach (var value_0 in left.Files)
                {
                    FileData value_1;
                    if (!right.Files.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (!FileData.ValueComparer.Equals(value_0.Value, value_1))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.LogicalLocations, right.LogicalLocations))
            {
                if (left.LogicalLocations == null || right.LogicalLocations == null || left.LogicalLocations.Count != right.LogicalLocations.Count)
                {
                    return false;
                }

                foreach (var value_2 in left.LogicalLocations)
                {
                    LogicalLocation value_3;
                    if (!right.LogicalLocations.TryGetValue(value_2.Key, out value_3))
                    {
                        return false;
                    }

                    if (!LogicalLocation.ValueComparer.Equals(value_2.Value, value_3))
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

                for (int index_0 = 0; index_0 < left.Results.Count; ++index_0)
                {
                    if (!Result.ValueComparer.Equals(left.Results[index_0], right.Results[index_0]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.Rules, right.Rules))
            {
                if (left.Rules == null || right.Rules == null || left.Rules.Count != right.Rules.Count)
                {
                    return false;
                }

                foreach (var value_4 in left.Rules)
                {
                    Rule value_5;
                    if (!right.Rules.TryGetValue(value_4.Key, out value_5))
                    {
                        return false;
                    }

                    if (!Rule.ValueComparer.Equals(value_4.Value, value_5))
                    {
                        return false;
                    }
                }
            }

            if (left.Id != right.Id)
            {
                return false;
            }

            if (left.StableId != right.StableId)
            {
                return false;
            }

            if (left.AutomationId != right.AutomationId)
            {
                return false;
            }

            if (left.BaselineId != right.BaselineId)
            {
                return false;
            }

            if (left.Architecture != right.Architecture)
            {
                return false;
            }

            if (left.RichMessageMimeType != right.RichMessageMimeType)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Properties, right.Properties))
            {
                if (left.Properties == null || right.Properties == null || left.Properties.Count != right.Properties.Count)
                {
                    return false;
                }

                foreach (var value_6 in left.Properties)
                {
                    SerializedPropertyInfo value_7;
                    if (!right.Properties.TryGetValue(value_6.Key, out value_7))
                    {
                        return false;
                    }

                    if (!object.Equals(value_6.Value, value_7))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int GetHashCode(Run obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Tool != null)
                {
                    result = (result * 31) + obj.Tool.ValueGetHashCode();
                }

                if (obj.Invocation != null)
                {
                    result = (result * 31) + obj.Invocation.ValueGetHashCode();
                }

                if (obj.Conversion != null)
                {
                    result = (result * 31) + obj.Conversion.ValueGetHashCode();
                }

                if (obj.OriginalUriBaseIds != null)
                {
                    result = (result * 31) + obj.OriginalUriBaseIds.GetHashCode();
                }

                if (obj.Files != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_8 in obj.Files)
                    {
                        xor_0 ^= value_8.Key.GetHashCode();
                        if (value_8.Value != null)
                        {
                            xor_0 ^= value_8.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                if (obj.LogicalLocations != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_1 = 0;
                    foreach (var value_9 in obj.LogicalLocations)
                    {
                        xor_1 ^= value_9.Key.GetHashCode();
                        if (value_9.Value != null)
                        {
                            xor_1 ^= value_9.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_1;
                }

                if (obj.Results != null)
                {
                    foreach (var value_10 in obj.Results)
                    {
                        result = result * 31;
                        if (value_10 != null)
                        {
                            result = (result * 31) + value_10.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Rules != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_2 = 0;
                    foreach (var value_11 in obj.Rules)
                    {
                        xor_2 ^= value_11.Key.GetHashCode();
                        if (value_11.Value != null)
                        {
                            xor_2 ^= value_11.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_2;
                }

                if (obj.Id != null)
                {
                    result = (result * 31) + obj.Id.GetHashCode();
                }

                if (obj.StableId != null)
                {
                    result = (result * 31) + obj.StableId.GetHashCode();
                }

                if (obj.AutomationId != null)
                {
                    result = (result * 31) + obj.AutomationId.GetHashCode();
                }

                if (obj.BaselineId != null)
                {
                    result = (result * 31) + obj.BaselineId.GetHashCode();
                }

                if (obj.Architecture != null)
                {
                    result = (result * 31) + obj.Architecture.GetHashCode();
                }

                if (obj.RichMessageMimeType != null)
                {
                    result = (result * 31) + obj.RichMessageMimeType.GetHashCode();
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_3 = 0;
                    foreach (var value_12 in obj.Properties)
                    {
                        xor_3 ^= value_12.Key.GetHashCode();
                        if (value_12.Value != null)
                        {
                            xor_3 ^= value_12.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_3;
                }
            }

            return result;
        }
    }
}