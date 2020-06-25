// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type ResultProvenance for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    internal sealed class ResultProvenanceEqualityComparer : IEqualityComparer<ResultProvenance>
    {
        internal static readonly ResultProvenanceEqualityComparer Instance = new ResultProvenanceEqualityComparer();

        public bool Equals(ResultProvenance left, ResultProvenance right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.FirstDetectionTimeUtc != right.FirstDetectionTimeUtc)
            {
                return false;
            }

            if (left.LastDetectionTimeUtc != right.LastDetectionTimeUtc)
            {
                return false;
            }

            if (left.FirstDetectionRunGuid != right.FirstDetectionRunGuid)
            {
                return false;
            }

            if (left.LastDetectionRunGuid != right.LastDetectionRunGuid)
            {
                return false;
            }

            if (left.InvocationIndex != right.InvocationIndex)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.ConversionSources, right.ConversionSources))
            {
                if (left.ConversionSources == null || right.ConversionSources == null)
                {
                    return false;
                }

                if (left.ConversionSources.Count != right.ConversionSources.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.ConversionSources.Count; ++index_0)
                {
                    if (!PhysicalLocation.ValueComparer.Equals(left.ConversionSources[index_0], right.ConversionSources[index_0]))
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

        public int GetHashCode(ResultProvenance obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                result = (result * 31) + obj.FirstDetectionTimeUtc.GetHashCode();
                result = (result * 31) + obj.LastDetectionTimeUtc.GetHashCode();
                if (obj.FirstDetectionRunGuid != null)
                {
                    result = (result * 31) + obj.FirstDetectionRunGuid.GetHashCode();
                }

                if (obj.LastDetectionRunGuid != null)
                {
                    result = (result * 31) + obj.LastDetectionRunGuid.GetHashCode();
                }

                result = (result * 31) + obj.InvocationIndex.GetHashCode();
                if (obj.ConversionSources != null)
                {
                    foreach (var value_2 in obj.ConversionSources)
                    {
                        result = result * 31;
                        if (value_2 != null)
                        {
                            result = (result * 31) + value_2.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_3 in obj.Properties)
                    {
                        xor_0 ^= value_3.Key.GetHashCode();
                        if (value_3.Value != null)
                        {
                            xor_0 ^= value_3.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }
            }

            return result;
        }
    }
}