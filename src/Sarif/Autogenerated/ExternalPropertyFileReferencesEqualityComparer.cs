// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type ExternalPropertyFileReferences for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    internal sealed class ExternalPropertyFileReferencesEqualityComparer : IEqualityComparer<ExternalPropertyFileReferences>
    {
        internal static readonly ExternalPropertyFileReferencesEqualityComparer Instance = new ExternalPropertyFileReferencesEqualityComparer();

        public bool Equals(ExternalPropertyFileReferences left, ExternalPropertyFileReferences right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (!ExternalPropertyFileReference.ValueComparer.Equals(left.Conversion, right.Conversion))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Graphs, right.Graphs))
            {
                if (left.Graphs == null || right.Graphs == null)
                {
                    return false;
                }

                if (left.Graphs.Count != right.Graphs.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.Graphs.Count; ++index_0)
                {
                    if (!ExternalPropertyFileReference.ValueComparer.Equals(left.Graphs[index_0], right.Graphs[index_0]))
                    {
                        return false;
                    }
                }
            }

            if (!ExternalPropertyFileReference.ValueComparer.Equals(left.ExternalizedProperties, right.ExternalizedProperties))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Artifacts, right.Artifacts))
            {
                if (left.Artifacts == null || right.Artifacts == null)
                {
                    return false;
                }

                if (left.Artifacts.Count != right.Artifacts.Count)
                {
                    return false;
                }

                for (int index_1 = 0; index_1 < left.Artifacts.Count; ++index_1)
                {
                    if (!ExternalPropertyFileReference.ValueComparer.Equals(left.Artifacts[index_1], right.Artifacts[index_1]))
                    {
                        return false;
                    }
                }
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

                for (int index_2 = 0; index_2 < left.Invocations.Count; ++index_2)
                {
                    if (!ExternalPropertyFileReference.ValueComparer.Equals(left.Invocations[index_2], right.Invocations[index_2]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.LogicalLocations, right.LogicalLocations))
            {
                if (left.LogicalLocations == null || right.LogicalLocations == null)
                {
                    return false;
                }

                if (left.LogicalLocations.Count != right.LogicalLocations.Count)
                {
                    return false;
                }

                for (int index_3 = 0; index_3 < left.LogicalLocations.Count; ++index_3)
                {
                    if (!ExternalPropertyFileReference.ValueComparer.Equals(left.LogicalLocations[index_3], right.LogicalLocations[index_3]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.ThreadFlowLocations, right.ThreadFlowLocations))
            {
                if (left.ThreadFlowLocations == null || right.ThreadFlowLocations == null)
                {
                    return false;
                }

                if (left.ThreadFlowLocations.Count != right.ThreadFlowLocations.Count)
                {
                    return false;
                }

                for (int index_4 = 0; index_4 < left.ThreadFlowLocations.Count; ++index_4)
                {
                    if (!ExternalPropertyFileReference.ValueComparer.Equals(left.ThreadFlowLocations[index_4], right.ThreadFlowLocations[index_4]))
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

                for (int index_5 = 0; index_5 < left.Results.Count; ++index_5)
                {
                    if (!ExternalPropertyFileReference.ValueComparer.Equals(left.Results[index_5], right.Results[index_5]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.Taxonomies, right.Taxonomies))
            {
                if (left.Taxonomies == null || right.Taxonomies == null)
                {
                    return false;
                }

                if (left.Taxonomies.Count != right.Taxonomies.Count)
                {
                    return false;
                }

                for (int index_6 = 0; index_6 < left.Taxonomies.Count; ++index_6)
                {
                    if (!ExternalPropertyFileReference.ValueComparer.Equals(left.Taxonomies[index_6], right.Taxonomies[index_6]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.Addresses, right.Addresses))
            {
                if (left.Addresses == null || right.Addresses == null)
                {
                    return false;
                }

                if (left.Addresses.Count != right.Addresses.Count)
                {
                    return false;
                }

                for (int index_7 = 0; index_7 < left.Addresses.Count; ++index_7)
                {
                    if (!ExternalPropertyFileReference.ValueComparer.Equals(left.Addresses[index_7], right.Addresses[index_7]))
                    {
                        return false;
                    }
                }
            }

            if (!ExternalPropertyFileReference.ValueComparer.Equals(left.Driver, right.Driver))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Extensions, right.Extensions))
            {
                if (left.Extensions == null || right.Extensions == null)
                {
                    return false;
                }

                if (left.Extensions.Count != right.Extensions.Count)
                {
                    return false;
                }

                for (int index_8 = 0; index_8 < left.Extensions.Count; ++index_8)
                {
                    if (!ExternalPropertyFileReference.ValueComparer.Equals(left.Extensions[index_8], right.Extensions[index_8]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.Policies, right.Policies))
            {
                if (left.Policies == null || right.Policies == null)
                {
                    return false;
                }

                if (left.Policies.Count != right.Policies.Count)
                {
                    return false;
                }

                for (int index_9 = 0; index_9 < left.Policies.Count; ++index_9)
                {
                    if (!ExternalPropertyFileReference.ValueComparer.Equals(left.Policies[index_9], right.Policies[index_9]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.Translations, right.Translations))
            {
                if (left.Translations == null || right.Translations == null)
                {
                    return false;
                }

                if (left.Translations.Count != right.Translations.Count)
                {
                    return false;
                }

                for (int index_10 = 0; index_10 < left.Translations.Count; ++index_10)
                {
                    if (!ExternalPropertyFileReference.ValueComparer.Equals(left.Translations[index_10], right.Translations[index_10]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.WebRequests, right.WebRequests))
            {
                if (left.WebRequests == null || right.WebRequests == null)
                {
                    return false;
                }

                if (left.WebRequests.Count != right.WebRequests.Count)
                {
                    return false;
                }

                for (int index_11 = 0; index_11 < left.WebRequests.Count; ++index_11)
                {
                    if (!ExternalPropertyFileReference.ValueComparer.Equals(left.WebRequests[index_11], right.WebRequests[index_11]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.WebResponses, right.WebResponses))
            {
                if (left.WebResponses == null || right.WebResponses == null)
                {
                    return false;
                }

                if (left.WebResponses.Count != right.WebResponses.Count)
                {
                    return false;
                }

                for (int index_12 = 0; index_12 < left.WebResponses.Count; ++index_12)
                {
                    if (!ExternalPropertyFileReference.ValueComparer.Equals(left.WebResponses[index_12], right.WebResponses[index_12]))
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

        public int GetHashCode(ExternalPropertyFileReferences obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.Conversion != null)
                {
                    result = (result * 31) + obj.Conversion.ValueGetHashCode();
                }

                if (obj.Graphs != null)
                {
                    foreach (var value_2 in obj.Graphs)
                    {
                        result = result * 31;
                        if (value_2 != null)
                        {
                            result = (result * 31) + value_2.ValueGetHashCode();
                        }
                    }
                }

                if (obj.ExternalizedProperties != null)
                {
                    result = (result * 31) + obj.ExternalizedProperties.ValueGetHashCode();
                }

                if (obj.Artifacts != null)
                {
                    foreach (var value_3 in obj.Artifacts)
                    {
                        result = result * 31;
                        if (value_3 != null)
                        {
                            result = (result * 31) + value_3.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Invocations != null)
                {
                    foreach (var value_4 in obj.Invocations)
                    {
                        result = result * 31;
                        if (value_4 != null)
                        {
                            result = (result * 31) + value_4.ValueGetHashCode();
                        }
                    }
                }

                if (obj.LogicalLocations != null)
                {
                    foreach (var value_5 in obj.LogicalLocations)
                    {
                        result = result * 31;
                        if (value_5 != null)
                        {
                            result = (result * 31) + value_5.ValueGetHashCode();
                        }
                    }
                }

                if (obj.ThreadFlowLocations != null)
                {
                    foreach (var value_6 in obj.ThreadFlowLocations)
                    {
                        result = result * 31;
                        if (value_6 != null)
                        {
                            result = (result * 31) + value_6.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Results != null)
                {
                    foreach (var value_7 in obj.Results)
                    {
                        result = result * 31;
                        if (value_7 != null)
                        {
                            result = (result * 31) + value_7.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Taxonomies != null)
                {
                    foreach (var value_8 in obj.Taxonomies)
                    {
                        result = result * 31;
                        if (value_8 != null)
                        {
                            result = (result * 31) + value_8.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Addresses != null)
                {
                    foreach (var value_9 in obj.Addresses)
                    {
                        result = result * 31;
                        if (value_9 != null)
                        {
                            result = (result * 31) + value_9.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Driver != null)
                {
                    result = (result * 31) + obj.Driver.ValueGetHashCode();
                }

                if (obj.Extensions != null)
                {
                    foreach (var value_10 in obj.Extensions)
                    {
                        result = result * 31;
                        if (value_10 != null)
                        {
                            result = (result * 31) + value_10.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Policies != null)
                {
                    foreach (var value_11 in obj.Policies)
                    {
                        result = result * 31;
                        if (value_11 != null)
                        {
                            result = (result * 31) + value_11.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Translations != null)
                {
                    foreach (var value_12 in obj.Translations)
                    {
                        result = result * 31;
                        if (value_12 != null)
                        {
                            result = (result * 31) + value_12.ValueGetHashCode();
                        }
                    }
                }

                if (obj.WebRequests != null)
                {
                    foreach (var value_13 in obj.WebRequests)
                    {
                        result = result * 31;
                        if (value_13 != null)
                        {
                            result = (result * 31) + value_13.ValueGetHashCode();
                        }
                    }
                }

                if (obj.WebResponses != null)
                {
                    foreach (var value_14 in obj.WebResponses)
                    {
                        result = result * 31;
                        if (value_14 != null)
                        {
                            result = (result * 31) + value_14.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_15 in obj.Properties)
                    {
                        xor_0 ^= value_15.Key.GetHashCode();
                        if (value_15.Value != null)
                        {
                            xor_0 ^= value_15.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }
            }

            return result;
        }
    }
}