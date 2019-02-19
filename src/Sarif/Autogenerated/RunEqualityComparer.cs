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
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.61.0.0")]
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
                if (left.Graphs == null || right.Graphs == null || left.Graphs.Count != right.Graphs.Count)
                {
                    return false;
                }

                foreach (var value_2 in left.Graphs)
                {
                    Graph value_3;
                    if (!right.Graphs.TryGetValue(value_2.Key, out value_3))
                    {
                        return false;
                    }

                    if (!Graph.ValueComparer.Equals(value_2.Value, value_3))
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

                for (int index_4 = 0; index_4 < left.Results.Count; ++index_4)
                {
                    if (!Result.ValueComparer.Equals(left.Results[index_4], right.Results[index_4]))
                    {
                        return false;
                    }
                }
            }

            if (!RunAutomationDetails.ValueComparer.Equals(left.Id, right.Id))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.AggregateIds, right.AggregateIds))
            {
                if (left.AggregateIds == null || right.AggregateIds == null)
                {
                    return false;
                }

                if (left.AggregateIds.Count != right.AggregateIds.Count)
                {
                    return false;
                }

                for (int index_5 = 0; index_5 < left.AggregateIds.Count; ++index_5)
                {
                    if (!RunAutomationDetails.ValueComparer.Equals(left.AggregateIds[index_5], right.AggregateIds[index_5]))
                    {
                        return false;
                    }
                }
            }

            if (left.BaselineInstanceGuid != right.BaselineInstanceGuid)
            {
                return false;
            }

            if (left.MarkdownMessageMimeType != right.MarkdownMessageMimeType)
            {
                return false;
            }

            if (left.RedactionToken != right.RedactionToken)
            {
                return false;
            }

            if (left.DefaultFileEncoding != right.DefaultFileEncoding)
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

                for (int index_6 = 0; index_6 < left.NewlineSequences.Count; ++index_6)
                {
                    if (left.NewlineSequences[index_6] != right.NewlineSequences[index_6])
                    {
                        return false;
                    }
                }
            }

            if (left.ColumnKind != right.ColumnKind)
            {
                return false;
            }

            if (!ExternalPropertyFiles.ValueComparer.Equals(left.ExternalPropertyFiles, right.ExternalPropertyFiles))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Properties, right.Properties))
            {
                if (left.Properties == null || right.Properties == null || left.Properties.Count != right.Properties.Count)
                {
                    return false;
                }

                foreach (var value_4 in left.Properties)
                {
                    SerializedPropertyInfo value_5;
                    if (!right.Properties.TryGetValue(value_4.Key, out value_5))
                    {
                        return false;
                    }

                    if (!object.Equals(value_4.Value, value_5))
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
                    foreach (var value_6 in obj.Invocations)
                    {
                        result = result * 31;
                        if (value_6 != null)
                        {
                            result = (result * 31) + value_6.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Conversion != null)
                {
                    result = (result * 31) + obj.Conversion.ValueGetHashCode();
                }

                if (obj.VersionControlProvenance != null)
                {
                    foreach (var value_7 in obj.VersionControlProvenance)
                    {
                        result = result * 31;
                        if (value_7 != null)
                        {
                            result = (result * 31) + value_7.ValueGetHashCode();
                        }
                    }
                }

                if (obj.OriginalUriBaseIds != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_8 in obj.OriginalUriBaseIds)
                    {
                        xor_0 ^= value_8.Key.GetHashCode();
                        if (value_8.Value != null)
                        {
                            xor_0 ^= value_8.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                if (obj.Artifacts != null)
                {
                    foreach (var value_9 in obj.Artifacts)
                    {
                        result = result * 31;
                        if (value_9 != null)
                        {
                            result = (result * 31) + value_9.ValueGetHashCode();
                        }
                    }
                }

                if (obj.LogicalLocations != null)
                {
                    foreach (var value_10 in obj.LogicalLocations)
                    {
                        result = result * 31;
                        if (value_10 != null)
                        {
                            result = (result * 31) + value_10.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Graphs != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_1 = 0;
                    foreach (var value_11 in obj.Graphs)
                    {
                        xor_1 ^= value_11.Key.GetHashCode();
                        if (value_11.Value != null)
                        {
                            xor_1 ^= value_11.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_1;
                }

                if (obj.Results != null)
                {
                    foreach (var value_12 in obj.Results)
                    {
                        result = result * 31;
                        if (value_12 != null)
                        {
                            result = (result * 31) + value_12.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Id != null)
                {
                    result = (result * 31) + obj.Id.ValueGetHashCode();
                }

                if (obj.AggregateIds != null)
                {
                    foreach (var value_13 in obj.AggregateIds)
                    {
                        result = result * 31;
                        if (value_13 != null)
                        {
                            result = (result * 31) + value_13.ValueGetHashCode();
                        }
                    }
                }

                if (obj.BaselineInstanceGuid != null)
                {
                    result = (result * 31) + obj.BaselineInstanceGuid.GetHashCode();
                }

                if (obj.MarkdownMessageMimeType != null)
                {
                    result = (result * 31) + obj.MarkdownMessageMimeType.GetHashCode();
                }

                if (obj.RedactionToken != null)
                {
                    result = (result * 31) + obj.RedactionToken.GetHashCode();
                }

                if (obj.DefaultFileEncoding != null)
                {
                    result = (result * 31) + obj.DefaultFileEncoding.GetHashCode();
                }

                if (obj.DefaultSourceLanguage != null)
                {
                    result = (result * 31) + obj.DefaultSourceLanguage.GetHashCode();
                }

                if (obj.NewlineSequences != null)
                {
                    foreach (var value_14 in obj.NewlineSequences)
                    {
                        result = result * 31;
                        if (value_14 != null)
                        {
                            result = (result * 31) + value_14.GetHashCode();
                        }
                    }
                }

                result = (result * 31) + obj.ColumnKind.GetHashCode();
                if (obj.ExternalPropertyFiles != null)
                {
                    result = (result * 31) + obj.ExternalPropertyFiles.ValueGetHashCode();
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_2 = 0;
                    foreach (var value_15 in obj.Properties)
                    {
                        xor_2 ^= value_15.Key.GetHashCode();
                        if (value_15.Value != null)
                        {
                            xor_2 ^= value_15.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_2;
                }
            }

            return result;
        }
    }
}