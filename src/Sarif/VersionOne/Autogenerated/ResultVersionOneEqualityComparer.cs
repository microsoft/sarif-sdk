// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type ResultVersionOne for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    internal sealed class ResultVersionOneEqualityComparer : IEqualityComparer<ResultVersionOne>
    {
        internal static readonly ResultVersionOneEqualityComparer Instance = new ResultVersionOneEqualityComparer();

        public bool Equals(ResultVersionOne left, ResultVersionOne right)
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

            if (!FormattedRuleMessageVersionOne.ValueComparer.Equals(left.FormattedRuleMessage, right.FormattedRuleMessage))
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
                    if (!LocationVersionOne.ValueComparer.Equals(left.Locations[index_0], right.Locations[index_0]))
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

            if (left.ToolFingerprintContribution != right.ToolFingerprintContribution)
            {
                return false;
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
                    if (!StackVersionOne.ValueComparer.Equals(left.Stacks[index_1], right.Stacks[index_1]))
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
                    if (!CodeFlowVersionOne.ValueComparer.Equals(left.CodeFlows[index_2], right.CodeFlows[index_2]))
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
                    if (!AnnotatedCodeLocationVersionOne.ValueComparer.Equals(left.RelatedLocations[index_3], right.RelatedLocations[index_3]))
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

                for (int index_4 = 0; index_4 < left.Fixes.Count; ++index_4)
                {
                    if (!FixVersionOne.ValueComparer.Equals(left.Fixes[index_4], right.Fixes[index_4]))
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

        public int GetHashCode(ResultVersionOne obj)
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

                if (obj.FormattedRuleMessage != null)
                {
                    result = (result * 31) + obj.FormattedRuleMessage.ValueGetHashCode();
                }

                if (obj.Locations != null)
                {
                    foreach (var value_2 in obj.Locations)
                    {
                        result = result * 31;
                        if (value_2 != null)
                        {
                            result = (result * 31) + value_2.ValueGetHashCode();
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

                if (obj.ToolFingerprintContribution != null)
                {
                    result = (result * 31) + obj.ToolFingerprintContribution.GetHashCode();
                }

                if (obj.Stacks != null)
                {
                    foreach (var value_3 in obj.Stacks)
                    {
                        result = result * 31;
                        if (value_3 != null)
                        {
                            result = (result * 31) + value_3.ValueGetHashCode();
                        }
                    }
                }

                if (obj.CodeFlows != null)
                {
                    foreach (var value_4 in obj.CodeFlows)
                    {
                        result = result * 31;
                        if (value_4 != null)
                        {
                            result = (result * 31) + value_4.ValueGetHashCode();
                        }
                    }
                }

                if (obj.RelatedLocations != null)
                {
                    foreach (var value_5 in obj.RelatedLocations)
                    {
                        result = result * 31;
                        if (value_5 != null)
                        {
                            result = (result * 31) + value_5.ValueGetHashCode();
                        }
                    }
                }

                result = (result * 31) + obj.SuppressionStates.GetHashCode();
                result = (result * 31) + obj.BaselineState.GetHashCode();
                if (obj.Fixes != null)
                {
                    foreach (var value_6 in obj.Fixes)
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
                    int xor_0 = 0;
                    foreach (var value_7 in obj.Properties)
                    {
                        xor_0 ^= value_7.Key.GetHashCode();
                        if (value_7.Value != null)
                        {
                            xor_0 ^= value_7.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }
            }

            return result;
        }
    }
}