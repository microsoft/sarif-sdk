// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

/// <summary>
/// All Comparer implementations should be replaced by auto-generated codes by JSchema as 
/// part of EqualityComparer in a planned comprehensive solution.
/// Tracking by issue: https://github.com/microsoft/jschema/issues/141
/// </summary>
namespace Microsoft.CodeAnalysis.Sarif
{
    internal class ResultSortingComparer : IComparer<Result>
    {
        internal static readonly ResultSortingComparer Instance = new ResultSortingComparer();

        public int Compare(Result left, Result right)
        {
            // both reference to same object, or both are null
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            int compareResult = 0;

            compareResult = string.Compare(left.RuleId, right.RuleId);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.RuleIndex.CompareTo(right.RuleIndex);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.Level.CompareTo(right.Level);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.Kind.CompareTo(right.Kind);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = MessageSortingComparer.Instance.Compare(left.Message, right.Message);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = ArtifactLocationSortingComparer.Instance.Compare(left.AnalysisTarget, right.AnalysisTarget);
            if (compareResult != 0)
            {
                return compareResult;
            }

            if (!object.ReferenceEquals(left.Locations, right.Locations))
            {
                if (left.Locations == null)
                {
                    return -1;
                }

                if (right.Locations == null)
                {
                    return 1;
                }

                compareResult = left.Locations.Count.CompareTo(right.Locations.Count);
                if (compareResult != 0)
                {
                    return compareResult;
                }

                for (int i = 0; i < left.Locations.Count; ++i)
                {
                    compareResult = LocationSortingComparer.Instance.Compare(left.Locations[i], right.Locations[i]);
                    if (compareResult != 0)
                    {
                        return compareResult;
                    }
                }
            }

            compareResult = string.Compare(left.Guid, right.Guid);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = string.Compare(left.CorrelationGuid, right.CorrelationGuid);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.OccurrenceCount.CompareTo(right.OccurrenceCount);
            if (compareResult != 0)
            {
                return compareResult;
            }

            if (!object.ReferenceEquals(left.CodeFlows, right.CodeFlows))
            {
                if (left.CodeFlows == null)
                {
                    return -1;
                }

                if (right.CodeFlows == null)
                {
                    return 1;
                }

                compareResult = left.CodeFlows.Count.CompareTo(right.CodeFlows.Count);
                if (compareResult != 0)
                {
                    return compareResult;
                }

                for (int i = 0; i < left.CodeFlows.Count; ++i)
                {
                    compareResult = CodeFlowSortingComparer.Instance.Compare(left.CodeFlows[i], right.CodeFlows[i]);
                    if (compareResult != 0)
                    {
                        return compareResult;
                    }
                }
            }

            compareResult = left.BaselineState.CompareTo(right.BaselineState);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.Rank.CompareTo(right.Rank);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}
