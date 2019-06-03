using System.Collections;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Query.Evaluators
{
    /// <summary>
    ///  An IExpressionEvaluator knows how to find matches for a logical expression
    ///  against an IList of a specific item type. IExpressions are converted to
    ///  IExpressionEvaluators for specific types to be evaluated.
    /// </summary>
    /// <typeparam name="T">Type of items being evaluated</typeparam>
    public interface IExpressionEvaluator<T>
    {
        /// <summary>
        ///  Evaluate examines the items in 'list' and sets bits on the 'matches'
        ///  array for each item matching the expression.
        /// </summary>
        /// <param name="list">Items to Evaluate</param>
        /// <param name="matches">BitArray on which to set bits for matching items in list</param>
        void Evaluate(IList<T> list, BitArray matches);
    }
}
