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

                for (int index_0 = 0; index_0 < left.Invocations.Count; ++index_0)
                {
                    if (!Invocation.ValueComparer.Equals(left.Invocations[index_0], right.Invocations[index_0]))
                    {
                        return false;
                    }
                }
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

                for (int index_1 = 0; index_1 < left.Results.Count; ++index_1)
                {
                    if (!Result.ValueComparer.Equals(left.Results[index_1], right.Results[index_1]))
                    {
                        return false;
                    }
                }
            }

            if (!Resources.ValueComparer.Equals(left.Resources, right.Resources))
            {
                return false;
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

                foreach (var value_4 in left.Properties)
                {
                    SerializedPropertyInfo value_5;
                    if (!right.Properties.TryGetValue(value_4.Key, out value_5))
                    {
                        return false;
                    }

                    if (!object.Equals(value_4.Value, value_5))
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

                if (obj.Invocations != null)
                {
                    foreach (var value_6 in obj.Invocations)
                    {
                        result = result * 31;
                        if (value_6 != null)
                        {
                            result = (result * 31) + value_6.ValueGetHashCode();
                        }
                    }
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
                    foreach (var value_7 in obj.Files)
                    {
                        xor_0 ^= value_7.Key.GetHashCode();
                        if (value_7.Value != null)
                        {
                            xor_0 ^= value_7.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                if (obj.LogicalLocations != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_1 = 0;
                    foreach (var value_8 in obj.LogicalLocations)
                    {
                        xor_1 ^= value_8.Key.GetHashCode();
                        if (value_8.Value != null)
                        {
                            xor_1 ^= value_8.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_1;
                }

                if (obj.Results != null)
                {
                    foreach (var value_9 in obj.Results)
                    {
                        result = result * 31;
                        if (value_9 != null)
                        {
                            result = (result * 31) + value_9.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Resources != null)
                {
                    result = (result * 31) + obj.Resources.ValueGetHashCode();
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
                    int xor_2 = 0;
                    foreach (var value_10 in obj.Properties)
                    {
                        xor_2 ^= value_10.Key.GetHashCode();
                        if (value_10.Value != null)
                        {
                            xor_2 ^= value_10.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_2;
                }
            }

            return result;
        }
    }
}