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
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
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

            if (left.RuleKey != right.RuleKey)
            {
                return false;
            }

            if (left.Level != right.Level)
            {
                return false;
            }

            if (left.Message != right.Message)
            {
                return false;
            }

            if (left.RichMessage != right.RichMessage)
            {
                return false;
            }

            if (!TemplatedMessage.ValueComparer.Equals(left.TemplatedMessage, right.TemplatedMessage))
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

            if (left.Snippet != right.Snippet)
            {
                return false;
            }

            if (left.Id != right.Id)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.ToolFingerprintContributions, right.ToolFingerprintContributions))
            {
                if (left.ToolFingerprintContributions == null || right.ToolFingerprintContributions == null || left.ToolFingerprintContributions.Count != right.ToolFingerprintContributions.Count)
                {
                    return false;
                }

                foreach (var value_0 in left.ToolFingerprintContributions)
                {
                    string value_1;
                    if (!right.ToolFingerprintContributions.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (value_0.Value != value_1)
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
                    if (!AnnotatedCodeLocation.ValueComparer.Equals(left.RelatedLocations[index_3], right.RelatedLocations[index_3]))
                    {
                        return false;
                    }
                }
            }

            if (left.SuppressionStates != right.SuppressionStates)
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

            if (left.BaselineState != right.BaselineState)
            {
                return false;
            }

            if (!object.ReferenceEquals(left.ConversionProvenance, right.ConversionProvenance))
            {
                if (left.ConversionProvenance == null || right.ConversionProvenance == null)
                {
                    return false;
                }

                if (left.ConversionProvenance.Count != right.ConversionProvenance.Count)
                {
                    return false;
                }

                for (int index_5 = 0; index_5 < left.ConversionProvenance.Count; ++index_5)
                {
                    if (!AnalysisToolLogFileContents.ValueComparer.Equals(left.ConversionProvenance[index_5], right.ConversionProvenance[index_5]))
                    {
                        return false;
                    }
                }
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

                if (obj.RuleKey != null)
                {
                    result = (result * 31) + obj.RuleKey.GetHashCode();
                }

                result = (result * 31) + obj.Level.GetHashCode();
                if (obj.Message != null)
                {
                    result = (result * 31) + obj.Message.GetHashCode();
                }

                if (obj.RichMessage != null)
                {
                    result = (result * 31) + obj.RichMessage.GetHashCode();
                }

                if (obj.TemplatedMessage != null)
                {
                    result = (result * 31) + obj.TemplatedMessage.ValueGetHashCode();
                }

                if (obj.Locations != null)
                {
                    foreach (var value_4 in obj.Locations)
                    {
                        result = result * 31;
                        if (value_4 != null)
                        {
                            result = (result * 31) + value_4.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Snippet != null)
                {
                    result = (result * 31) + obj.Snippet.GetHashCode();
                }

                if (obj.Id != null)
                {
                    result = (result * 31) + obj.Id.GetHashCode();
                }

                if (obj.ToolFingerprintContributions != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_5 in obj.ToolFingerprintContributions)
                    {
                        xor_0 ^= value_5.Key.GetHashCode();
                        if (value_5.Value != null)
                        {
                            xor_0 ^= value_5.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                if (obj.Stacks != null)
                {
                    foreach (var value_6 in obj.Stacks)
                    {
                        result = result * 31;
                        if (value_6 != null)
                        {
                            result = (result * 31) + value_6.ValueGetHashCode();
                        }
                    }
                }

                if (obj.CodeFlows != null)
                {
                    foreach (var value_7 in obj.CodeFlows)
                    {
                        result = result * 31;
                        if (value_7 != null)
                        {
                            result = (result * 31) + value_7.ValueGetHashCode();
                        }
                    }
                }

                if (obj.RelatedLocations != null)
                {
                    foreach (var value_8 in obj.RelatedLocations)
                    {
                        result = result * 31;
                        if (value_8 != null)
                        {
                            result = (result * 31) + value_8.ValueGetHashCode();
                        }
                    }
                }

                result = (result * 31) + obj.SuppressionStates.GetHashCode();
                if (obj.Attachments != null)
                {
                    foreach (var value_9 in obj.Attachments)
                    {
                        result = result * 31;
                        if (value_9 != null)
                        {
                            result = (result * 31) + value_9.ValueGetHashCode();
                        }
                    }
                }

                result = (result * 31) + obj.BaselineState.GetHashCode();
                if (obj.ConversionProvenance != null)
                {
                    foreach (var value_10 in obj.ConversionProvenance)
                    {
                        result = result * 31;
                        if (value_10 != null)
                        {
                            result = (result * 31) + value_10.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Fixes != null)
                {
                    foreach (var value_11 in obj.Fixes)
                    {
                        result = result * 31;
                        if (value_11 != null)
                        {
                            result = (result * 31) + value_11.ValueGetHashCode();
                        }
                    }
                }

                if (obj.Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_1 = 0;
                    foreach (var value_12 in obj.Properties)
                    {
                        xor_1 ^= value_12.Key.GetHashCode();
                        if (value_12.Value != null)
                        {
                            xor_1 ^= value_12.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_1;
                }
            }

            return result;
        }
    }
}