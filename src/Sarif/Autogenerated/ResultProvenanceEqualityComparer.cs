// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type ResultProvenance for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.58.0.0")]
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

            if (left.FirstDetectionRunInstanceGuid != right.FirstDetectionRunInstanceGuid)
            {
                return false;
            }

            if (left.LastDetectionRunInstanceGuid != right.LastDetectionRunInstanceGuid)
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
                if (obj.FirstDetectionRunInstanceGuid != null)
                {
                    result = (result * 31) + obj.FirstDetectionRunInstanceGuid.GetHashCode();
                }

                if (obj.LastDetectionRunInstanceGuid != null)
                {
                    result = (result * 31) + obj.LastDetectionRunInstanceGuid.GetHashCode();
                }

                result = (result * 31) + obj.InvocationIndex.GetHashCode();
                if (obj.ConversionSources != null)
                {
                    foreach (var value_0 in obj.ConversionSources)
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