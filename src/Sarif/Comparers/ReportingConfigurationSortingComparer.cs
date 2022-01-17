// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

/// <summary>
/// All Comparer implementations should be replaced by auto-generated codes by JSchema as 
/// part of EqualityComparer in a planned comprehensive solution.
/// Tracking by issue: https://github.com/microsoft/jschema/issues/141
/// </summary>
namespace Microsoft.CodeAnalysis.Sarif
{
    internal class ReportingConfigurationSortingComparer : IComparer<ReportingConfiguration>
    {
        internal static readonly ReportingConfigurationSortingComparer Instance = new ReportingConfigurationSortingComparer();

        public int Compare(ReportingConfiguration left, ReportingConfiguration right)
        {
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
            compareResult = left.Enabled.CompareTo(right.Enabled);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.Level.CompareTo(right.Level);
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
