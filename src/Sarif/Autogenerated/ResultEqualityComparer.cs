// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type Result for equality.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.22.0.0")]
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

            if (left.Level != right.Level)
            {
                return false;
            }

            if (left.Message != right.Message)
            {
                return false;
            }

            if (!FormattedRuleMessage.ValueComparer.Equals(left.FormattedRuleMessage, right.FormattedRuleMessage))
            {
                return false;
            }

            if (!Object.ReferenceEquals(left.Locations, right.Locations))
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

            if (left.CodeSnippet != right.CodeSnippet)
            {
                return false;
            }

            if (left.ToolFingerprint != right.ToolFingerprint)
            {
                return false;
            }

            if (!Object.ReferenceEquals(left.Stacks, right.Stacks))
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

            if (!Object.ReferenceEquals(left.CodeFlows, right.CodeFlows))
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

            if (!Object.ReferenceEquals(left.RelatedLocations, right.RelatedLocations))
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

            if (left.BaselineState != right.BaselineState)
            {
                return false;
            }

            if (!Object.ReferenceEquals(left.Fixes, right.Fixes))
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
                    if (!Fix.ValueComparer.Equals(left.Fixes[index_4], right.Fixes[index_4]))
                    {
                        return false;
                    }
                }
            }

            if (!Object.ReferenceEquals(left.Properties, right.Properties))
            {
                if (left.Properties == null || right.Properties == null || left.Properties.Count != right.Properties.Count)
                {
                    return false;
                }

                foreach (var value_0 in left.Properties)
                {
                    string value_1;
                    if (!right.Properties.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (value_0.Value != value_1)
                    {
                        return false;
                    }
                }
            }

            if (!Object.ReferenceEquals(left.Tags, right.Tags))
            {
                if (left.Tags == null || right.Tags == null)
                {
                    return false;
                }

                if (left.Tags.Count != right.Tags.Count)
                {
                    return false;
                }

                for (int index_5 = 0; index_5 < left.Tags.Count; ++index_5)
                {
                    if (left.Tags[index_5] != right.Tags[index_5])
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

                if (obj.CodeSnippet != null)
                {
                    result = (result * 31) + obj.CodeSnippet.GetHashCode();
                }

                if (obj.ToolFingerprint != null)
                {
                    result = (result * 31) + obj.ToolFingerprint.GetHashCode();
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

                if (obj.Tags != null)
                {
                    foreach (var value_8 in obj.Tags)
                    {
                        result = result * 31;
                        if (value_8 != null)
                        {
                            result = (result * 31) + value_8.GetHashCode();
                        }
                    }
                }
            }

            return result;
        }
    }
}