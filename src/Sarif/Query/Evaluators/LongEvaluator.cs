// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Query.Evaluators
{
    /// <summary>
    ///  LongEvaluator implements IExpressionEvaluator given a getter which can
    ///  get the desired Property Name as a long.
    ///  
    ///  Usage: 
    ///    if (String.Equals(term.PropertyName, "ID", StringComparison.OrdinalIgnoreCase))
    ///    {
    ///        // Show the LongEvaluator how to get the 'ID' property as a long, and it'll implement the term matching.
    ///        return new LongEvaluator&lt;Result&gt;(result => result.ID, term);
    ///    }
    /// </summary>
    /// <typeparam name="T">Type of Item Evaluator will evaluate.</typeparam>
    public class LongEvaluator<T> : IExpressionEvaluator<T>
    {
        private Func<T, long> Getter { get; set; }
        private long Value { get; set; }

        private Action<IList<T>, BitArray> EvaluateSet { get; set; }

        public LongEvaluator(Func<T, long> getter, TermExpression term)
        {
            Getter = getter;

            if (!long.TryParse(term.Value, out long parsedValue)) { throw new QueryParseException($"{term} value {term.Value} was not a valid number."); }
            Value = parsedValue;

            EvaluateSet = Comparer(term);
        }

        public void Evaluate(IList<T> list, BitArray matches)
        {
            EvaluateSet(list, matches);
        }

        private Action<IList<T>, BitArray> Comparer(TermExpression term)
        {
            switch (term.Operator)
            {
                case CompareOperator.Equals:
                    return EvaluateEquals;
                case CompareOperator.NotEquals:
                    return EvaluateNotEquals;
                case CompareOperator.LessThan:
                    return EvaluateLessThan;
                case CompareOperator.LessThanOrEquals:
                    return EvaluateLessThanOrEquals;
                case CompareOperator.GreaterThan:
                    return EvaluateGreaterThan;
                case CompareOperator.GreaterThanOrEquals:
                    return EvaluateGreaterThanOrEquals;
                default:
                    throw new QueryParseException($"{term} does not support operator {term.Operator}");
            }
        }

        private void EvaluateEquals(IList<T> list, BitArray matches)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                matches.Set(i, Getter(list[i]) == Value);
            }
        }

        private void EvaluateNotEquals(IList<T> list, BitArray matches)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                matches.Set(i, Getter(list[i]) != Value);
            }
        }

        private void EvaluateLessThan(IList<T> list, BitArray matches)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                matches.Set(i, Getter(list[i]) < Value);
            }
        }

        private void EvaluateLessThanOrEquals(IList<T> list, BitArray matches)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                matches.Set(i, Getter(list[i]) <= Value);
            }
        }

        private void EvaluateGreaterThan(IList<T> list, BitArray matches)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                matches.Set(i, Getter(list[i]) > Value);
            }
        }

        private void EvaluateGreaterThanOrEquals(IList<T> list, BitArray matches)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                matches.Set(i, Getter(list[i]) >= Value);
            }
        }
    }
}
