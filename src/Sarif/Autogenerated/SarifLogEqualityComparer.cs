// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type SarifLog for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    internal sealed class SarifLogEqualityComparer : IEqualityComparer<SarifLog>
    {
        internal static readonly SarifLogEqualityComparer Instance = new SarifLogEqualityComparer();

        public bool Equals(SarifLog left, SarifLog right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.SchemaUri != right.SchemaUri)
            {
                return false;
            }

            if (left.Version != right.Version)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Runs, right.Runs))
            {
                if (left.Runs == null || right.Runs == null)
                {
                    return false;
                }

                if (left.Runs.Count != right.Runs.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.Runs.Count; ++index_0)
                {
                    if (!Run.ValueComparer.Equals(left.Runs[index_0], right.Runs[index_0]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.InlineExternalProperties, right.InlineExternalProperties))
            {
                if (left.InlineExternalProperties == null || right.InlineExternalProperties == null)
                {
                    return false;
                }

                if (left.InlineExternalProperties.Count != right.InlineExternalProperties.Count)
                {
                    return false;
                }

                for (int index_1 = 0; index_1 < left.InlineExternalProperties.Count; ++index_1)
                {
                    if (!ExternalProperties.ValueComparer.Equals(left.InlineExternalProperties[index_1], right.InlineExternalProperties[index_1]))
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

        public int GetHashCode(SarifLog obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.SchemaUri != null)
                {
                    result = (result * 31) + obj.SchemaUri.GetHashCode();
                }

                result = (result * 31) + obj.Version.GetHashCode();
                if (obj.Runs != null)
                {
                    foreach (var value_2 in obj.Runs)
                    {
                        result = result * 31;
                        if (value_2 != null)
                        {
                            result = (result * 31) + value_2.ValueGetHashCode();
                        }
                    }
                }

                if (obj.InlineExternalProperties != null)
                {
                    foreach (var value_3 in obj.InlineExternalProperties)
                    {
                        result = result * 31;
                        if (value_3 != null)
                        {
                            result = (result * 31) + value_3.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_4 in obj.Properties)
                    {
                        xor_0 ^= value_4.Key.GetHashCode();
                        if (value_4.Value != null)
                        {
                            xor_0 ^= value_4.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }
            }

            return result;
        }
    }
}