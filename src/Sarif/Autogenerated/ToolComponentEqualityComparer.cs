// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type ToolComponent for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    internal sealed class ToolComponentEqualityComparer : IEqualityComparer<ToolComponent>
    {
        internal static readonly ToolComponentEqualityComparer Instance = new ToolComponentEqualityComparer();

        public bool Equals(ToolComponent left, ToolComponent right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.Guid != right.Guid)
            {
                return false;
            }

            if (left.Name != right.Name)
            {
                return false;
            }

            if (left.Organization != right.Organization)
            {
                return false;
            }

            if (left.Product != right.Product)
            {
                return false;
            }

            if (!MultiformatMessageString.ValueComparer.Equals(left.ShortDescription, right.ShortDescription))
            {
                return false;
            }

            if (!MultiformatMessageString.ValueComparer.Equals(left.FullDescription, right.FullDescription))
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

            if (!object.ReferenceEquals(left.GlobalMessageStrings, right.GlobalMessageStrings))
            {
                if (left.GlobalMessageStrings == null || right.GlobalMessageStrings == null || left.GlobalMessageStrings.Count != right.GlobalMessageStrings.Count)
                {
                    return false;
                }

                foreach (var value_0 in left.GlobalMessageStrings)
                {
                    MultiformatMessageString value_1;
                    if (!right.GlobalMessageStrings.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (!MultiformatMessageString.ValueComparer.Equals(value_0.Value, value_1))
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

                for (int index_0 = 0; index_0 < left.NotificationDescriptors.Count; ++index_0)
                {
                    if (!ReportingDescriptor.ValueComparer.Equals(left.NotificationDescriptors[index_0], right.NotificationDescriptors[index_0]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.RuleDescriptors, right.RuleDescriptors))
            {
                if (left.RuleDescriptors == null || right.RuleDescriptors == null)
                {
                    return false;
                }

                if (left.RuleDescriptors.Count != right.RuleDescriptors.Count)
                {
                    return false;
                }

                for (int index_1 = 0; index_1 < left.RuleDescriptors.Count; ++index_1)
                {
                    if (!ReportingDescriptor.ValueComparer.Equals(left.RuleDescriptors[index_1], right.RuleDescriptors[index_1]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.ArtifactIndices, right.ArtifactIndices))
            {
                if (left.ArtifactIndices == null || right.ArtifactIndices == null)
                {
                    return false;
                }

                if (left.ArtifactIndices.Count != right.ArtifactIndices.Count)
                {
                    return false;
                }

                for (int index_2 = 0; index_2 < left.ArtifactIndices.Count; ++index_2)
                {
                    if (left.ArtifactIndices[index_2] != right.ArtifactIndices[index_2])
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

        public int GetHashCode(ToolComponent obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Guid != null)
                {
                    result = (result * 31) + obj.Guid.GetHashCode();
                }

                if (obj.Name != null)
                {
                    result = (result * 31) + obj.Name.GetHashCode();
                }

                if (obj.Organization != null)
                {
                    result = (result * 31) + obj.Organization.GetHashCode();
                }

                if (obj.Product != null)
                {
                    result = (result * 31) + obj.Product.GetHashCode();
                }

                if (obj.ShortDescription != null)
                {
                    result = (result * 31) + obj.ShortDescription.ValueGetHashCode();
                }

                if (obj.FullDescription != null)
                {
                    result = (result * 31) + obj.FullDescription.ValueGetHashCode();
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

                if (obj.NotificationDescriptors != null)
                {
                    foreach (var value_5 in obj.NotificationDescriptors)
                    {
                        result = result * 31;
                        if (value_5 != null)
                        {
                            result = (result * 31) + value_5.ValueGetHashCode();
                        }
                    }
                }

                if (obj.RuleDescriptors != null)
                {
                    foreach (var value_6 in obj.RuleDescriptors)
                    {
                        result = result * 31;
                        if (value_6 != null)
                        {
                            result = (result * 31) + value_6.ValueGetHashCode();
                        }
                    }
                }

                if (obj.ArtifactIndices != null)
                {
                    foreach (var value_7 in obj.ArtifactIndices)
                    {
                        result = result * 31;
                        result = (result * 31) + value_7.GetHashCode();
                    }
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_1 = 0;
                    foreach (var value_8 in obj.Properties)
                    {
                        xor_1 ^= value_8.Key.GetHashCode();
                        if (value_8.Value != null)
                        {
                            xor_1 ^= value_8.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_1;
                }
            }

            return result;
        }
    }
}