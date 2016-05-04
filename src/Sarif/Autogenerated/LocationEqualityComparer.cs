// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Location for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.22.0.0")]
    internal sealed class LocationEqualityComparer : IEqualityComparer<Location>
    {
        internal static readonly LocationEqualityComparer Instance = new LocationEqualityComparer();

        public bool Equals(Location left, Location right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (!PhysicalLocation.ValueComparer.Equals(left.AnalysisTarget, right.AnalysisTarget))
            {
                return false;
            }

            if (!PhysicalLocation.ValueComparer.Equals(left.ResultFile, right.ResultFile))
            {
                return false;
            }

            if (left.FullyQualifiedLogicalName != right.FullyQualifiedLogicalName)
            {
                return false;
            }

            if (left.LogicalLocationKey != right.LogicalLocationKey)
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

        public int GetHashCode(Location obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.AnalysisTarget != null)
                {
                    result = (result * 31) + obj.AnalysisTarget.ValueGetHashCode();
                }

                if (obj.ResultFile != null)
                {
                    result = (result * 31) + obj.ResultFile.ValueGetHashCode();
                }

                if (obj.FullyQualifiedLogicalName != null)
                {
                    result = (result * 31) + obj.FullyQualifiedLogicalName.GetHashCode();
                }

                if (obj.LogicalLocationKey != null)
                {
                    result = (result * 31) + obj.LogicalLocationKey.GetHashCode();
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