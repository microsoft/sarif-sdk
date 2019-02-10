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
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.61.0.0")]
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

            if (!object.ReferenceEquals(left.GlobalMessageStrings, right.GlobalMessageStrings))
            {
                if (left.GlobalMessageStrings == null || right.GlobalMessageStrings == null || left.GlobalMessageStrings.Count != right.GlobalMessageStrings.Count)
                {
                    return false;
                }

                foreach (var value_0 in left.GlobalMessageStrings)
                {
                    string value_1;
                    if (!right.GlobalMessageStrings.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (value_0.Value != value_1)
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.NotificationsMetadata, right.NotificationsMetadata))
            {
                if (left.NotificationsMetadata == null || right.NotificationsMetadata == null)
                {
                    return false;
                }

                if (left.NotificationsMetadata.Count != right.NotificationsMetadata.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.NotificationsMetadata.Count; ++index_0)
                {
                    if (!MessageDescriptor.ValueComparer.Equals(left.NotificationsMetadata[index_0], right.NotificationsMetadata[index_0]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.RulesMetadata, right.RulesMetadata))
            {
                if (left.RulesMetadata == null || right.RulesMetadata == null)
                {
                    return false;
                }

                if (left.RulesMetadata.Count != right.RulesMetadata.Count)
                {
                    return false;
                }

                for (int index_1 = 0; index_1 < left.RulesMetadata.Count; ++index_1)
                {
                    if (!MessageDescriptor.ValueComparer.Equals(left.RulesMetadata[index_1], right.RulesMetadata[index_1]))
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

                foreach (var value_2 in left.Properties)
                {
                    SerializedPropertyInfo value_3;
                    if (!right.Properties.TryGetValue(value_2.Key, out value_3))
                    {
                        return false;
                    }

                    if (!object.Equals(value_2.Value, value_3))
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

                if (obj.GlobalMessageStrings != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_4 in obj.GlobalMessageStrings)
                    {
                        xor_0 ^= value_4.Key.GetHashCode();
                        if (value_4.Value != null)
                        {
                            xor_0 ^= value_4.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                if (obj.NotificationsMetadata != null)
                {
                    foreach (var value_5 in obj.NotificationsMetadata)
                    {
                        result = result * 31;
                        if (value_5 != null)
                        {
                            result = (result * 31) + value_5.ValueGetHashCode();
                        }
                    }
                }

                if (obj.RulesMetadata != null)
                {
                    foreach (var value_6 in obj.RulesMetadata)
                    {
                        result = result * 31;
                        if (value_6 != null)
                        {
                            result = (result * 31) + value_6.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_1 = 0;
                    foreach (var value_7 in obj.Properties)
                    {
                        xor_1 ^= value_7.Key.GetHashCode();
                        if (value_7.Value != null)
                        {
                            xor_1 ^= value_7.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_1;
                }
            }

            return result;
        }
    }
}