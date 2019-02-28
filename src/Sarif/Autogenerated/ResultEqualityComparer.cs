// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Result for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.61.0.0")]
    internal sealed class ResultEqualityComparer : IEqualityComparer<Result>
    {
        internal static readonly ResultEqualityComparer Instance = new ResultEqualityComparer();

        public bool Equals(Result left, Result right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.RuleId != right.RuleId)
            {
                return false;
            }

            if (left.RuleIndex != right.RuleIndex)
            {
                return false;
            }

            if (left.RuleExtensionIndex != right.RuleExtensionIndex)
            {
                return false;
            }

            if (left.Kind != right.Kind)
            {
                return false;
            }

            if (left.Level != right.Level)
            {
                return false;
            }

            if (!Message.ValueComparer.Equals(left.Message, right.Message))
            {
                return false;
            }

            if (!ArtifactLocation.ValueComparer.Equals(left.AnalysisTarget, right.AnalysisTarget))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Locations, right.Locations))
            {
                if (left.Locations == null || right.Locations == null)
                {
                    return false;
                }

                if (left.Locations.Count != right.Locations.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < left.Locations.Count; ++index_0)
                {
                    if (!Location.ValueComparer.Equals(left.Locations[index_0], right.Locations[index_0]))
                    {
                        return false;
                    }
                }
            }

            if (left.InstanceGuid != right.InstanceGuid)
            {
                return false;
            }

            if (left.CorrelationGuid != right.CorrelationGuid)
            {
                return false;
            }

            if (left.OccurrenceCount != right.OccurrenceCount)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.PartialFingerprints, right.PartialFingerprints))
            {
                if (left.PartialFingerprints == null || right.PartialFingerprints == null || left.PartialFingerprints.Count != right.PartialFingerprints.Count)
                {
                    return false;
                }

                foreach (var value_0 in left.PartialFingerprints)
                {
                    string value_1;
                    if (!right.PartialFingerprints.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (value_0.Value != value_1)
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.Fingerprints, right.Fingerprints))
            {
                if (left.Fingerprints == null || right.Fingerprints == null || left.Fingerprints.Count != right.Fingerprints.Count)
                {
                    return false;
                }

                foreach (var value_2 in left.Fingerprints)
                {
                    string value_3;
                    if (!right.Fingerprints.TryGetValue(value_2.Key, out value_3))
                    {
                        return false;
                    }

                    if (value_2.Value != value_3)
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.Stacks, right.Stacks))
            {
                if (left.Stacks == null || right.Stacks == null)
                {
                    return false;
                }

                if (left.Stacks.Count != right.Stacks.Count)
                {
                    return false;
                }

                for (int index_1 = 0; index_1 < left.Stacks.Count; ++index_1)
                {
                    if (!Stack.ValueComparer.Equals(left.Stacks[index_1], right.Stacks[index_1]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.CodeFlows, right.CodeFlows))
            {
                if (left.CodeFlows == null || right.CodeFlows == null)
                {
                    return false;
                }

                if (left.CodeFlows.Count != right.CodeFlows.Count)
                {
                    return false;
                }

                for (int index_2 = 0; index_2 < left.CodeFlows.Count; ++index_2)
                {
                    if (!CodeFlow.ValueComparer.Equals(left.CodeFlows[index_2], right.CodeFlows[index_2]))
                    {
                        return false;
                    }
                }
            }

            if (!object.ReferenceEquals(left.RelatedLocations, right.RelatedLocations))
            {
                if (left.RelatedLocations == null || right.RelatedLocations == null)
                {
                    return false;
                }

                if (left.RelatedLocations.Count != right.RelatedLocations.Count)
                {
                    return false;
                }

                for (int index_3 = 0; index_3 < left.RelatedLocations.Count; ++index_3)
                {
                    if (!Location.ValueComparer.Equals(left.RelatedLocations[index_3], right.RelatedLocations[index_3]))
                    {
                        return false;
                    }
                }
            }

            if (left.SuppressionStates != right.SuppressionStates)
            {
                return false;
            }

            if (left.BaselineState != right.BaselineState)
            {
                return false;
            }

            if (left.Rank != right.Rank)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Attachments, right.Attachments))
            {
                if (left.Attachments == null || right.Attachments == null)
                {
                    return false;
                }

                if (left.Attachments.Count != right.Attachments.Count)
                {
                    return false;
                }

                for (int index_4 = 0; index_4 < left.Attachments.Count; ++index_4)
                {
                    if (!Attachment.ValueComparer.Equals(left.Attachments[index_4], right.Attachments[index_4]))
                    {
                        return false;
                    }
                }
            }

            if (left.HostedViewerUri != right.HostedViewerUri)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.WorkItemUris, right.WorkItemUris))
            {
                if (left.WorkItemUris == null || right.WorkItemUris == null)
                {
                    return false;
                }

                if (left.WorkItemUris.Count != right.WorkItemUris.Count)
                {
                    return false;
                }

                for (int index_5 = 0; index_5 < left.WorkItemUris.Count; ++index_5)
                {
                    if (left.WorkItemUris[index_5] != right.WorkItemUris[index_5])
                    {
                        return false;
                    }
                }
            }

            if (!ResultProvenance.ValueComparer.Equals(left.Provenance, right.Provenance))
            {
                return false;
            }

            if (!object.ReferenceEquals(left.Fixes, right.Fixes))
            {
                if (left.Fixes == null || right.Fixes == null)
                {
                    return false;
                }

                if (left.Fixes.Count != right.Fixes.Count)
                {
                    return false;
                }

                for (int index_6 = 0; index_6 < left.Fixes.Count; ++index_6)
                {
                    if (!Fix.ValueComparer.Equals(left.Fixes[index_6], right.Fixes[index_6]))
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

        public int GetHashCode(Result obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                if (obj.RuleId != null)
                {
                    result = (result * 31) + obj.RuleId.GetHashCode();
                }

                result = (result * 31) + obj.RuleIndex.GetHashCode();
                result = (result * 31) + obj.RuleExtensionIndex.GetHashCode();
                result = (result * 31) + obj.Kind.GetHashCode();
                result = (result * 31) + obj.Level.GetHashCode();
                if (obj.Message != null)
                {
                    result = (result * 31) + obj.Message.ValueGetHashCode();
                }

                if (obj.AnalysisTarget != null)
                {
                    result = (result * 31) + obj.AnalysisTarget.ValueGetHashCode();
                }

                if (obj.Locations != null)
                {
                    foreach (var value_6 in obj.Locations)
                    {
                        result = result * 31;
                        if (value_6 != null)
                        {
                            result = (result * 31) + value_6.ValueGetHashCode();
                        }
                    }
                }

                if (obj.InstanceGuid != null)
                {
                    result = (result * 31) + obj.InstanceGuid.GetHashCode();
                }

                if (obj.CorrelationGuid != null)
                {
                    result = (result * 31) + obj.CorrelationGuid.GetHashCode();
                }

                result = (result * 31) + obj.OccurrenceCount.GetHashCode();
                if (obj.PartialFingerprints != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_7 in obj.PartialFingerprints)
                    {
                        xor_0 ^= value_7.Key.GetHashCode();
                        if (value_7.Value != null)
                        {
                            xor_0 ^= value_7.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                if (obj.Fingerprints != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_1 = 0;
                    foreach (var value_8 in obj.Fingerprints)
                    {
                        xor_1 ^= value_8.Key.GetHashCode();
                        if (value_8.Value != null)
                        {
                            xor_1 ^= value_8.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_1;
                }

                if (obj.Stacks != null)
                {
                    foreach (var value_9 in obj.Stacks)
                    {
                        result = result * 31;
                        if (value_9 != null)
                        {
                            result = (result * 31) + value_9.ValueGetHashCode();
                        }
                    }
                }

                if (obj.CodeFlows != null)
                {
                    foreach (var value_10 in obj.CodeFlows)
                    {
                        result = result * 31;
                        if (value_10 != null)
                        {
                            result = (result * 31) + value_10.ValueGetHashCode();
                        }
                    }
                }

                if (obj.RelatedLocations != null)
                {
                    foreach (var value_11 in obj.RelatedLocations)
                    {
                        result = result * 31;
                        if (value_11 != null)
                        {
                            result = (result * 31) + value_11.ValueGetHashCode();
                        }
                    }
                }

                result = (result * 31) + obj.SuppressionStates.GetHashCode();
                result = (result * 31) + obj.BaselineState.GetHashCode();
                result = (result * 31) + obj.Rank.GetHashCode();
                if (obj.Attachments != null)
                {
                    foreach (var value_12 in obj.Attachments)
                    {
                        result = result * 31;
                        if (value_12 != null)
                        {
                            result = (result * 31) + value_12.ValueGetHashCode();
                        }
                    }
                }

                if (obj.HostedViewerUri != null)
                {
                    result = (result * 31) + obj.HostedViewerUri.GetHashCode();
                }

                if (obj.WorkItemUris != null)
                {
                    foreach (var value_13 in obj.WorkItemUris)
                    {
                        result = result * 31;
                        if (value_13 != null)
                        {
                            result = (result * 31) + value_13.GetHashCode();
                        }
                    }
                }

                if (obj.Provenance != null)
                {
                    result = (result * 31) + obj.Provenance.ValueGetHashCode();
                }

                if (obj.Fixes != null)
                {
                    foreach (var value_14 in obj.Fixes)
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