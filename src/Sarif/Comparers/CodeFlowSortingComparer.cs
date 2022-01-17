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
    internal class CodeFlowSortingComparer : IComparer<CodeFlow>
    {
        internal static readonly CodeFlowSortingComparer Instance = new CodeFlowSortingComparer();

        public int Compare(CodeFlow left, CodeFlow right)
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

            compareResult = MessageSortingComparer.Instance.Compare(left.Message, right.Message);
            if (compareResult != 0)
            {
                return compareResult;
            }

            if (!object.ReferenceEquals(left.ThreadFlows, right.ThreadFlows))
            {

                if (left.ThreadFlows == null)
                {
                    return -1;
                }

                if (right.ThreadFlows == null)
                {
                    return 1;
                }

                compareResult = left.ThreadFlows.Count.CompareTo(right.ThreadFlows.Count);
                if (compareResult != 0)
                {
                    return compareResult;
                }

                for (int index_1 = 0; index_1 < left.ThreadFlows.Count; ++index_1)
                {
                    compareResult = ThreadFlowSortingComparer.Instance.Compare(left.ThreadFlows[index_1], right.ThreadFlows[index_1]);
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
