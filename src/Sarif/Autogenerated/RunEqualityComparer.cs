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
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
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

            if (left.Language != right.Language)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.VersionControlProvenance, right.VersionControlProvenance))
            {
                if (left.VersionControlProvenance == null || right.VersionControlProvenance == null)
                {
                    return false;
                }

                if (left.VersionControlProvenance.Count != right.VersionControlProvenance.Count)
                {
                    return false;
                }

                for (int index_1 = 0; index_1 < left.VersionControlProvenance.Count; ++index_1)
                {
                    if (!VersionControlDetails.ValueComparer.Equals(left.VersionControlProvenance[index_1], right.VersionControlProvenance[index_1]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.OriginalUriBaseIds, right.OriginalUriBaseIds))
            {
                if (left.OriginalUriBaseIds == null || right.OriginalUriBaseIds == null || left.OriginalUriBaseIds.Count != right.OriginalUriBaseIds.Count)
                {
                    return false;
                }

                foreach (var value_0 in left.OriginalUriBaseIds)
                {
                    ArtifactLocation value_1;
                    if (!right.OriginalUriBaseIds.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (!ArtifactLocation.ValueComparer.Equals(value_0.Value, value_1))
                    {
                        return false;
                    }
                }
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

                for (int index_2 = 0; index_2 < left.Artifacts.Count; ++index_2)
                {
                    if (!Artifact.ValueComparer.Equals(left.Artifacts[index_2], right.Artifacts[index_2]))
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
                    if (!LogicalLocation.ValueComparer.Equals(left.LogicalLocations[index_3], right.LogicalLocations[index_3]))
                    {
                        return false;
                    }
                }
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

                for (int index_4 = 0; index_4 < left.Graphs.Count; ++index_4)
                {
                    if (!Graph.ValueComparer.Equals(left.Graphs[index_4], right.Graphs[index_4]))
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
                    if (!Result.ValueComparer.Equals(left.Results[index_5], right.Results[index_5]))
                    {
                        return false;
                    }
                }
            }

            if (!RunAutomationDetails.ValueComparer.Equals(left.AutomationDetails, right.AutomationDetails))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.RunAggregates, right.RunAggregates))
            {
                if (left.RunAggregates == null || right.RunAggregates == null)
                {
                    return false;
                }

                if (left.RunAggregates.Count != right.RunAggregates.Count)
                {
                    return false;
                }

                for (int index_6 = 0; index_6 < left.RunAggregates.Count; ++index_6)
                {
                    if (!RunAutomationDetails.ValueComparer.Equals(left.RunAggregates[index_6], right.RunAggregates[index_6]))
                    {
                        return false;
                    }
                }
            }

            if (left.BaselineGuid != right.BaselineGuid)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.RedactionTokens, right.RedactionTokens))
            {
                if (left.RedactionTokens == null || right.RedactionTokens == null)
                {
                    return false;
                }

                if (left.RedactionTokens.Count != right.RedactionTokens.Count)
                {
                    return false;
                }

                for (int index_7 = 0; index_7 < left.RedactionTokens.Count; ++index_7)
                {
                    if (left.RedactionTokens[index_7] != right.RedactionTokens[index_7])
                    {
                        return false;
                    }
                }
            }

            if (left.DefaultEncoding != right.DefaultEncoding)
            {
                return false;
            }

            if (left.DefaultSourceLanguage != right.DefaultSourceLanguage)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.NewlineSequences, right.NewlineSequences))
            {
                if (left.NewlineSequences == null || right.NewlineSequences == null)
                {
                    return false;
                }

                if (left.NewlineSequences.Count != right.NewlineSequences.Count)
                {
                    return false;
                }

                for (int index_8 = 0; index_8 < left.NewlineSequences.Count; ++index_8)
                {
                    if (left.NewlineSequences[index_8] != right.NewlineSequences[index_8])
                    {
                        return false;
                    }
                }
            }

            if (left.ColumnKind != right.ColumnKind)
            {
                return false;
            }

            if (!ExternalPropertyFileReferences.ValueComparer.Equals(left.ExternalPropertyFileReferences, right.ExternalPropertyFileReferences))
            {
                return false;
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

                for (int index_9 = 0; index_9 < left.ThreadFlowLocations.Count; ++index_9)
                {
                    if (!ThreadFlowLocation.ValueComparer.Equals(left.ThreadFlowLocations[index_9], right.ThreadFlowLocations[index_9]))
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

                for (int index_10 = 0; index_10 < left.Taxonomies.Count; ++index_10)
                {
                    if (!ToolComponent.ValueComparer.Equals(left.Taxonomies[index_10], right.Taxonomies[index_10]))
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

                for (int index_11 = 0; index_11 < left.Addresses.Count; ++index_11)
                {
                    if (!Address.ValueComparer.Equals(left.Addresses[index_11], right.Addresses[index_11]))
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

                for (int index_12 = 0; index_12 < left.Translations.Count; ++index_12)
                {
                    if (!ToolComponent.ValueComparer.Equals(left.Translations[index_12], right.Translations[index_12]))
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

                for (int index_13 = 0; index_13 < left.Policies.Count; ++index_13)
                {
                    if (!ToolComponent.ValueComparer.Equals(left.Policies[index_13], right.Policies[index_13]))
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

                for (int index_14 = 0; index_14 < left.WebRequests.Count; ++index_14)
                {
                    if (!WebRequest.ValueComparer.Equals(left.WebRequests[index_14], right.WebRequests[index_14]))
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

                for (int index_15 = 0; index_15 < left.WebResponses.Count; ++index_15)
                {
                    if (!WebResponse.ValueComparer.Equals(left.WebResponses[index_15], right.WebResponses[index_15]))
                    {
                        return false;
                    }
                }
            }

            if (!SpecialLocations.ValueComparer.Equals(left.SpecialLocations, right.SpecialLocations))
            {
                return false;
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
                    foreach (var value_4 in obj.Invocations)
                    {
                        result = result * 31;
                        if (value_4 != null)
                        {
                            result = (result * 31) + value_4.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Conversion != null)
                {
                    result = (result * 31) + obj.Conversion.ValueGetHashCode();
                }

                if (obj.Language != null)
                {
                    result = (result * 31) + obj.Language.GetHashCode();
                }

                if (obj.VersionControlProvenance != null)
                {
                    foreach (var value_5 in obj.VersionControlProvenance)
                    {
                        result = result * 31;
                        if (value_5 != null)
                        {
                            result = (result * 31) + value_5.ValueGetHashCode();
                        }
                    }
                }

                if (obj.OriginalUriBaseIds != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_6 in obj.OriginalUriBaseIds)
                    {
                        xor_0 ^= value_6.Key.GetHashCode();
                        if (value_6.Value != null)
                        {
                            xor_0 ^= value_6.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                if (obj.Artifacts != null)
                {
                    foreach (var value_7 in obj.Artifacts)
                    {
                        result = result * 31;
                        if (value_7 != null)
                        {
                            result = (result * 31) + value_7.ValueGetHashCode();
                        }
                    }
                }

                if (obj.LogicalLocations != null)
                {
                    foreach (var value_8 in obj.LogicalLocations)
                    {
                        result = result * 31;
                        if (value_8 != null)
                        {
                            result = (result * 31) + value_8.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Graphs != null)
                {
                    foreach (var value_9 in obj.Graphs)
                    {
                        result = result * 31;
                        if (value_9 != null)
                        {
                            result = (result * 31) + value_9.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Results != null)
                {
                    foreach (var value_10 in obj.Results)
                    {
                        result = result * 31;
                        if (value_10 != null)
                        {
                            result = (result * 31) + value_10.ValueGetHashCode();
                        }
                    }
                }

                if (obj.AutomationDetails != null)
                {
                    result = (result * 31) + obj.AutomationDetails.ValueGetHashCode();
                }

                if (obj.RunAggregates != null)
                {
                    foreach (var value_11 in obj.RunAggregates)
                    {
                        result = result * 31;
                        if (value_11 != null)
                        {
                            result = (result * 31) + value_11.ValueGetHashCode();
                        }
                    }
                }

                if (obj.BaselineGuid != null)
                {
                    result = (result * 31) + obj.BaselineGuid.GetHashCode();
                }

                if (obj.RedactionTokens != null)
                {
                    foreach (var value_12 in obj.RedactionTokens)
                    {
                        result = result * 31;
                        if (value_12 != null)
                        {
                            result = (result * 31) + value_12.GetHashCode();
                        }
                    }
                }

                if (obj.DefaultEncoding != null)
                {
                    result = (result * 31) + obj.DefaultEncoding.GetHashCode();
                }

                if (obj.DefaultSourceLanguage != null)
                {
                    result = (result * 31) + obj.DefaultSourceLanguage.GetHashCode();
                }

                if (obj.NewlineSequences != null)
                {
                    foreach (var value_13 in obj.NewlineSequences)
                    {
                        result = result * 31;
                        if (value_13 != null)
                        {
                            result = (result * 31) + value_13.GetHashCode();
                        }
                    }
                }

                result = (result * 31) + obj.ColumnKind.GetHashCode();
                if (obj.ExternalPropertyFileReferences != null)
                {
                    result = (result * 31) + obj.ExternalPropertyFileReferences.ValueGetHashCode();
                }

                if (obj.ThreadFlowLocations != null)
                {
                    foreach (var value_14 in obj.ThreadFlowLocations)
                    {
                        result = result * 31;
                        if (value_14 != null)
                        {
                            result = (result * 31) + value_14.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Taxonomies != null)
                {
                    foreach (var value_15 in obj.Taxonomies)
                    {
                        result = result * 31;
                        if (value_15 != null)
                        {
                            result = (result * 31) + value_15.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Addresses != null)
                {
                    foreach (var value_16 in obj.Addresses)
                    {
                        result = result * 31;
                        if (value_16 != null)
                        {
                            result = (result * 31) + value_16.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Translations != null)
                {
                    foreach (var value_17 in obj.Translations)
                    {
                        result = result * 31;
                        if (value_17 != null)
                        {
                            result = (result * 31) + value_17.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Policies != null)
                {
                    foreach (var value_18 in obj.Policies)
                    {
                        result = result * 31;
                        if (value_18 != null)
                        {
                            result = (result * 31) + value_18.ValueGetHashCode();
                        }
                    }
                }

                if (obj.WebRequests != null)
                {
                    foreach (var value_19 in obj.WebRequests)
                    {
                        result = result * 31;
                        if (value_19 != null)
                        {
                            result = (result * 31) + value_19.ValueGetHashCode();
                        }
                    }
                }

                if (obj.WebResponses != null)
                {
                    foreach (var value_20 in obj.WebResponses)
                    {
                        result = result * 31;
                        if (value_20 != null)
                        {
                            result = (result * 31) + value_20.ValueGetHashCode();
                        }
                    }
                }

                if (obj.SpecialLocations != null)
                {
                    result = (result * 31) + obj.SpecialLocations.ValueGetHashCode();
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_1 = 0;
                    foreach (var value_21 in obj.Properties)
                    {
                        xor_1 ^= value_21.Key.GetHashCode();
                        if (value_21.Value != null)
                        {
                            xor_1 ^= value_21.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_1;
                }
            }

            return result;
        }
    }
}