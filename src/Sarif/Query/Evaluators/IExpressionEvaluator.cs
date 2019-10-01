// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Query.Evaluators
{
    /// <summary>
    ///  An IExpressionEvaluator knows how to find matches for a logical expression
    ///  against an ICollection of a specific item type. IExpressions are converted to
    ///  IExpressionEvaluators for specific types to be evaluated.
    /// </summary>
    /// <typeparam name="T">Type of items being evaluated</typeparam>
    public interface IExpressionEvaluator<T>
    {
        /// <summary>
        ///  Evaluate examines the items in 'set' and sets bits on the 'matches'
        ///  array for each item matching the expression.
        /// </summary>
        /// <param name="set">Items to Evaluate</param>
        /// <param name="matches">BitArray on which to set bits for matching items in list</param>
        void Evaluate(ICollection<T> set, BitArray matches);
    }
}
