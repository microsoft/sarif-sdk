// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type ToolComponentTranslation for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    internal sealed class ToolComponentTranslationEqualityComparer : IEqualityComparer<ToolComponentTranslation>
    {
        internal static readonly ToolComponentTranslationEqualityComparer Instance = new ToolComponentTranslationEqualityComparer();

        public bool Equals(ToolComponentTranslation left, ToolComponentTranslation right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.ToolComponentGuid != right.ToolComponentGuid)
            {
                return false;
            }

            if (!ArtifactLocation.ValueComparer.Equals(left.Location, right.Location))
            {
                return false;
            }

            if (left.DownloadUri != right.DownloadUri)
            {
                return false;
            }

            if (left.SemanticVersion != right.SemanticVersion)
            {
                return false;
            }

            if (left.PartialTranslation != right.PartialTranslation)
            {
                return false;
            }

            if (!object.Equals(left.GlobalMessageStrings, right.GlobalMessageStrings))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.ReportingDescriptors, right.ReportingDescriptors))
            {
                if (left.ReportingDescriptors == null || right.ReportingDescriptors == null)
                {
                    return false;
                }

                if (left.ReportingDescriptors.Count != right.ReportingDescriptors.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.ReportingDescriptors.Count; ++index_0)
                {
                    if (!ReportingDescriptorTranslation.ValueComparer.Equals(left.ReportingDescriptors[index_0], right.ReportingDescriptors[index_0]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.NotificationDescriptors, right.NotificationDescriptors))
            {
                if (left.NotificationDescriptors == null || right.NotificationDescriptors == null)
                {
                    return false;
                }

                if (left.NotificationDescriptors.Count != right.NotificationDescriptors.Count)
                {
                    return false;
                }

                for (int index_1 = 0; index_1 < left.NotificationDescriptors.Count; ++index_1)
                {
                    if (!ReportingDescriptorTranslation.ValueComparer.Equals(left.NotificationDescriptors[index_1], right.NotificationDescriptors[index_1]))
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

        public int GetHashCode(ToolComponentTranslation obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.ToolComponentGuid != null)
                {
                    result = (result * 31) + obj.ToolComponentGuid.GetHashCode();
                }

                if (obj.Location != null)
                {
                    result = (result * 31) + obj.Location.ValueGetHashCode();
                }

                if (obj.DownloadUri != null)
                {
                    result = (result * 31) + obj.DownloadUri.GetHashCode();
                }

                if (obj.SemanticVersion != null)
                {
                    result = (result * 31) + obj.SemanticVersion.GetHashCode();
                }

                result = (result * 31) + obj.PartialTranslation.GetHashCode();
                if (obj.GlobalMessageStrings != null)
                {
                    result = (result * 31) + obj.GlobalMessageStrings.GetHashCode();
                }

                if (obj.ReportingDescriptors != null)
                {
                    foreach (var value_2 in obj.ReportingDescriptors)
                    {
                        result = result * 31;
                        if (value_2 != null)
                        {
                            result = (result * 31) + value_2.ValueGetHashCode();
                        }
                    }
                }

                if (obj.NotificationDescriptors != null)
                {
                    foreach (var value_3 in obj.NotificationDescriptors)
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