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
    internal class RunSortingComparer : IComparer<Run>
    {
        internal static readonly RunSortingComparer Instance = new RunSortingComparer();

        public int Compare(Run left, Run right)
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

            if (!object.ReferenceEquals(left.Tool.Driver.Rules, right.Tool.Driver.Rules))
            {
                if (left.Tool.Driver.Rules == null)
                {
                    return -1;
                }

                if (right.Tool.Driver.Rules == null)
                {
                    return 1;
                }

                compareResult = left.Tool.Driver.Rules.Count.CompareTo(right.Tool.Driver.Rules.Count);
                if (compareResult != 0)
                {
                    return compareResult;
                }

                for (int i = 0; i < left.Tool.Driver.Rules.Count; ++i)
                {
                    compareResult = ReportingDescriptorSortingComparer.Instance.Compare(left.Tool.Driver.Rules[i], right.Tool.Driver.Rules[i]);
                    if (compareResult != 0)
                    {
                        return compareResult;
                    }
                }
            }

            return compareResult;
        }
    }
}
