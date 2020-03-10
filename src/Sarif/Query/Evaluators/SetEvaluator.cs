// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Query.Evaluators
{
    /// <summary>
    ///  SetEvaluator implements IExpressionEvaluator for sets rather than single values.
    ///  If any of the values matches the term, the item matches.
    ///  
    ///  For example, a SetEvaluator for Result.Urls[] and the term 'contains /inner/'
    ///  would return true for Results which have any Url containing '/inner/'.
    /// </summary>
    /// <typeparam name="T">Type of Item Evaluator will evaluate.</typeparam>
    public class SetEvaluator<T, U> : IExpressionEvaluator<T>
    {
        private readonly Func<T, IEnumerable<U>> _getter;
        private readonly IExpressionEvaluator<U> _innerEvaluator;

        /// <summary>
        ///  Build a SetEvaluator given a method to get an IEnumerable&lt;U&gt; of a primitive type
        ///  and the term showing the operator and value to compare against.
        /// </summary>
        /// <param name="getter">Getter to return a set of U given a T</param>
        /// <param name="term">TermExpression for how to match each T</param>
        public SetEvaluator(Func<T, IEnumerable<U>> getter, TermExpression term)
        {
            _getter = getter;

            object innerEvaluator = EvaluatorFactory.BuildPrimitiveEvaluator(typeof(U), term);
            _innerEvaluator = (IExpressionEvaluator<U>)innerEvaluator;
        }

        /// <summary>
        ///  Build a SetEvaluator given a method to get an IEnumerable&lt;U&gt; and an
        ///  IExpressionEvaluator for type U.
        /// </summary>
        /// <example>
        ///  new SetEvaluator&lt;Result, string&gt;(result => result.Locations.Select(loc => loc.Id), new StringEvaluator&lt;string&gt;(value => value, StringComparison.OrdinalIgnoreCase)
        /// </example>
        /// <param name="getter">Getter to return a set of U given a T</param>
        /// <param name="innerEvaluator">IExpressionEvaluator&lt;U&gt; for which the getter returns just the item itself</param>
        public SetEvaluator(Func<T, IEnumerable<U>> getter, IExpressionEvaluator<U> innerEvaluator)
        {
            _getter = getter;
            _innerEvaluator = innerEvaluator;
        }

        public void Evaluate(ICollection<T> list, BitArray matches)
        {
            BitArray perItemResult = null;

            int i = 0;
            foreach (T item in list)
            {
                // Convert the set of values for this item to a List
                ICollection<U> values = _getter(item).ToList();

                // Build a BitArray to match each of those value
                if (perItemResult == null || perItemResult.Length < values.Count)
                {
                    perItemResult = new BitArray(values.Count);
                }

                // Ask the inner evaluator to check each value as if the values were the set of rows
                _innerEvaluator.Evaluate(values, perItemResult);

                // Make this item included if any of the values matched
                bool isAnySet = false;
                for (int j = 0; j < values.Count; ++j)
                {
                    if (perItemResult[j])
                    {
                        isAnySet = true;
                        break;
                    }
                }

                matches.Set(i, isAnySet);
                i++;
            }
        }
    }
}
