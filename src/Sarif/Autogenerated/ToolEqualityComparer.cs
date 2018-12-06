// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Tool for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.58.0.0")]
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

            if (left.DottedQuadFileVersion != right.DottedQuadFileVersion)
            {
                return false;
            }

            if (left.DownloadUri != right.DownloadUri)
            {
                return false;
            }

            if (left.SarifLoggerVersion != right.SarifLoggerVersion)
            {
                return false;
            }

            if (left.Language != right.Language)
            {
                return false;
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

                if (obj.DottedQuadFileVersion != null)
                {
                    result = (result * 31) + obj.DottedQuadFileVersion.GetHashCode();
                }

                if (obj.DownloadUri != null)
                {
                    result = (result * 31) + obj.DownloadUri.GetHashCode();
                }

                if (obj.SarifLoggerVersion != null)
                {
                    result = (result * 31) + obj.SarifLoggerVersion.GetHashCode();
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
            }

            return result;
        }
    }
}