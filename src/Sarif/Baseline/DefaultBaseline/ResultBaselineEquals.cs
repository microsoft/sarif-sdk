// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.DefaultBaseline
{
    internal class ResultBaselineEquals : IEqualityComparer<Result>
    {
        internal static readonly ResultBaselineEquals DefaultInstance = new ResultBaselineEquals();
        
        public bool Equals(Result x, Result y)
        {
            if (!object.ReferenceEquals(x, y))
            {
                // Rule ID/Key should match
                if (x.RuleId != y.RuleId || x.RuleKey != y.RuleKey)
                {
                    return false;
                }

                // Locations should all be the same.
                if (!ListComparisonHelpers.CompareListsAsSets(x.Locations, y.Locations, LocationBaselineEquals.Instance))
                {
                    return false;
                }

                // Related Locations should all be the same.
                if (!ListComparisonHelpers.CompareListsAsSets(x.RelatedLocations, y.RelatedLocations, AnnotatedCodeLocationBaselineEquals.DefaultInstance))
                {
                    return false;
                }

                // Finger print contributions should be the same.
                if (x.ToolFingerprintContribution != y.ToolFingerprintContribution)
                {
                    return false;
                }

                // If stacks are present, we'll make sure they're the same
                if (!ListComparisonHelpers.CompareListsAsSets(x.Stacks, y.Stacks, StackBaselineEquals.Instance))
                {
                    return false;
                }

                // If codeflows are present, we'll make sure they're the same.
                if (!ListComparisonHelpers.CompareListsAsSets(x.CodeFlows, y.CodeFlows, CodeFlowBaselineEqualityComparator.Instance))
                {
                    return false;
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
            else
            {
                int hs = 0;

                hs = hs ^ obj.RuleId.GetNullCheckedHashCode() ^ obj.RuleKey.GetNullCheckedHashCode() ^ obj.ToolFingerprintContribution.GetNullCheckedHashCode();

                hs = hs ^ ListComparisonHelpers.GetHashOfListContentsAsSets(obj.Locations, LocationBaselineEquals.Instance);

                hs = hs ^ ListComparisonHelpers.GetHashOfListContentsAsSets(obj.RelatedLocations, AnnotatedCodeLocationBaselineEquals.DefaultInstance);

                hs = hs ^ ListComparisonHelpers.GetHashOfListContentsAsSets(obj.Stacks, StackBaselineEquals.Instance);

                hs = hs ^ ListComparisonHelpers.GetHashOfListContentsAsSets(obj.CodeFlows, CodeFlowBaselineEqualityComparator.Instance);

                return hs;
            }
        }
    }
}
