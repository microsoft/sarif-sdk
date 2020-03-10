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
        private readonly Func<T, long> _getter;
        private readonly Action<ICollection<T>, BitArray> _evaluateSet;

        public long Value { get; }

        public LongEvaluator(Func<T, long> getter, TermExpression term)
        {
            _getter = getter;

            if (!long.TryParse(term.Value, out long parsedValue)) { throw new QueryParseException($"{term} value {term.Value} was not a valid number."); }
            Value = parsedValue;

            _evaluateSet = Comparer(term);
        }

        public void Evaluate(ICollection<T> list, BitArray matches)
        {
            _evaluateSet(list, matches);
        }

        private Action<ICollection<T>, BitArray> Comparer(TermExpression term)
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

        private void EvaluateEquals(ICollection<T> list, BitArray matches)
        {
            int i = 0;
            foreach (T item in list)
            {
                matches.Set(i, _getter(item) == Value);
                i++;
            }
        }

        private void EvaluateNotEquals(ICollection<T> list, BitArray matches)
        {
            int i = 0;
            foreach (T item in list)
            {
                matches.Set(i, _getter(item) != Value);
                i++;
            }
        }

        private void EvaluateLessThan(ICollection<T> list, BitArray matches)
        {
            int i = 0;
            foreach (T item in list)
            {
                matches.Set(i, _getter(item) < Value);
                i++;
            }
        }

        private void EvaluateLessThanOrEquals(ICollection<T> list, BitArray matches)
        {
            int i = 0;
            foreach (T item in list)
            {
                matches.Set(i, _getter(item) <= Value);
                i++;
            }
        }

        private void EvaluateGreaterThan(ICollection<T> list, BitArray matches)
        {
            int i = 0;
            foreach (T item in list)
            {
                matches.Set(i, _getter(item) > Value);
                i++;
            }
        }

        private void EvaluateGreaterThanOrEquals(ICollection<T> list, BitArray matches)
        {
            int i = 0;
            foreach (T item in list)
            {
                matches.Set(i, _getter(item) >= Value);
                i++;
            }
        }
    }
}
