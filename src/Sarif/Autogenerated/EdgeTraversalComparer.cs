// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Defines methods to support the comparison of objects of type EdgeTraversal for sorting.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.3.0")]
    internal sealed class EdgeTraversalComparer : IComparer<EdgeTraversal>
    {
        internal static readonly EdgeTraversalComparer Instance = new EdgeTraversalComparer();

        public int Compare(EdgeTraversal left, EdgeTraversal right)
        {
            int compareResult = 0;
            if (left.TryReferenceCompares(right, out compareResult))
            {
                return compareResult;
            }

            compareResult = string.Compare(left.EdgeId, right.EdgeId);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = MessageComparer.Instance.Compare(left.Message, right.Message);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.FinalState.DictionaryCompares(right.FinalState, MultiformatMessageStringComparer.Instance);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.StepOverEdgeCount.CompareTo(right.StepOverEdgeCount);
            if (compareResult != 0)
            {
                return compareResult;
            }

            compareResult = left.Properties.DictionaryCompares(right.Properties, SerializedPropertyInfoComparer.Instance);
            if (compareResult != 0)
            {
                return compareResult;
            }

            return compareResult;
        }
    }
}