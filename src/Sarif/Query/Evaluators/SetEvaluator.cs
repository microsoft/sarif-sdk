// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Query.Evaluators
{
    /// <summary>
    ///  SetEvaluator implements IExpressionEvaluator given a getter which returns
    ///  multiple values. It includes an item if any of the values matches the expression.
    /// 
    ///  Usage: 
    ///    if (String.Equals(term.PropertyName, "Location.Id", StringComparison.OrdinalIgnoreCase))
    ///    {
    ///        return new SetEvaluator&lt;Result&gt;(result => return r.Locations?.Select(l => l?.Id ?? "").ToList(), new StringEvaluator&lt;string&gt;(value => value, term, StringComparison.OrdinalIgnoreCase);
    ///    }
    /// </summary>
    /// <typeparam name="T">Type of Item Evaluator will evaluate.</typeparam>
    public class SetEvaluator<T, U> : IExpressionEvaluator<T>
    {
        private Func<T, IList<U>> Getter { get; set; }
        private IExpressionEvaluator<U> InnerEvaluator { get; set; }
        private Action<IList<T>, BitArray> EvaluateSet { get; set; }

        /// <summary>
        ///  Build a SetEvaluator given a method to get a List&lt;U&gt; and an
        ///  Evaluator for type U. The inner evaluator's getter should just return
        ///  the argument itself.
        /// </summary>
        /// <param name="getter"></param>
        /// <param name="innerEvaluator"></param>
        public SetEvaluator(Func<T, IList<U>> getter, IExpressionEvaluator<U> innerEvaluator)
        {
            Getter = getter;
            InnerEvaluator = innerEvaluator;
        }

        public void Evaluate(IList<T> list, BitArray matches)
        {
            BitArray perItemResult = null;

            for (int i = 0; i < list.Count; ++i)
            {
                IList<U> values = Getter(list[i]);

                if (perItemResult == null || perItemResult.Length < values.Count)
                {
                    perItemResult = new BitArray(values.Count);
                }

                InnerEvaluator.Evaluate(values, perItemResult);

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
            }
        }
    }
}
