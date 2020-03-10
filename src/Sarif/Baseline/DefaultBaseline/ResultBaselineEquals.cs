// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.DefaultBaseline
{
    internal class ResultBaselineEquals : IEqualityComparer<Result>
    {
        internal static readonly ResultBaselineEquals DefaultInstance = new ResultBaselineEquals();

        public bool Equals(Result x, Result y)
        {
            if (!object.ReferenceEquals(x, y))
            {
                // Rule ID should match
                if (x.RuleId != y.RuleId)
                {
                    return false;
                }

                // Target file should match.
                if (!ArtifactLocationBaselineEquals.Instance.Equals(x.AnalysisTarget, y.AnalysisTarget))
                {
                    return false;
                }

                // Locations should all be the same.
                if (!ListComparisonHelpers.CompareListsAsSets(x.Locations, y.Locations, LocationBaselineEquals.Instance))
                {
                    return false;
                }

                // Related Locations should all be the same.
                if (!ListComparisonHelpers.CompareListsAsSets(x.RelatedLocations, y.RelatedLocations, LocationBaselineEquals.Instance))
                {
                    return false;
                }

                // Fingerprints (values only, ignore keys) should be the same.
                if (!ListComparisonHelpers.CompareListsAsSets(x.Fingerprints?.Values?.ToList(), y.Fingerprints?.Values?.ToList(), StringComparer.Ordinal))
                {
                    return false;
                }

                // Partial fingerprints (values only, ignore keys) should be the same.
                if (!ListComparisonHelpers.CompareListsAsSets(x.PartialFingerprints?.Values?.ToList(), y.PartialFingerprints?.Values?.ToList(), StringComparer.Ordinal))
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

                hs = hs ^ obj.RuleId.GetNullCheckedHashCode() ^ obj.PartialFingerprints.GetNullCheckedHashCode();

                hs = hs ^ ListComparisonHelpers.GetHashOfListContentsAsSets(obj.Locations, LocationBaselineEquals.Instance);

                hs = hs ^ ListComparisonHelpers.GetHashOfListContentsAsSets(obj.RelatedLocations, LocationBaselineEquals.Instance);

                hs = hs ^ ListComparisonHelpers.GetHashOfListContentsAsSets(obj.Stacks, StackBaselineEquals.Instance);

                hs = hs ^ ListComparisonHelpers.GetHashOfListContentsAsSets(obj.CodeFlows, CodeFlowBaselineEqualityComparator.Instance);

                return hs;
            }
        }
    }
}
