// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Query.Evaluators
{
    /// <summary>
    ///  DateTimeEvaluator implements IExpressionEvaluator given a getter which can
    ///  get the desired Property Name as a DateTime.
    ///
    ///  Usage:
    ///    if (String.Equals(term.PropertyName, "ID", StringComparison.OrdinalIgnoreCase))
    ///    {
    ///        // Show the DateTimeEvaluator how to get the 'Timestamp' property as a DateTime, and it'll implement the term matching.
    ///        return new DateTimeEvaluator&lt;Result&gt;(result => result.ID, term);
    ///    }
    /// </summary>
    /// <typeparam name="T">Type of Item Evaluator will evaluate.</typeparam>
    public class DateTimeEvaluator<T> : IExpressionEvaluator<T>
    {
        private readonly Func<T, DateTime> _getter;
        private readonly DateTime _value;
        private readonly Action<ICollection<T>, BitArray> _evaluateSet;

        public DateTimeEvaluator(Func<T, DateTime> getter, TermExpression term)
        {
            _getter = getter;

            if (!DateTime.TryParse(term.Value, out DateTime parsedValue)) { throw new QueryParseException($"{term} value {term.Value} was not a valid DateTime format."); }
            _value = parsedValue;

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
                matches.Set(i, _getter(item).Equals(_value));
                i++;
            }
        }

        private void EvaluateNotEquals(ICollection<T> list, BitArray matches)
        {
            int i = 0;
            foreach (T item in list)
            {
                matches.Set(i, !_getter(item).Equals(_value));
                i++;
            }
        }

        private void EvaluateLessThan(ICollection<T> list, BitArray matches)
        {
            int i = 0;
            foreach (T item in list)
            {
                matches.Set(i, _getter(item) < _value);
                i++;
            }
        }

        private void EvaluateLessThanOrEquals(ICollection<T> list, BitArray matches)
        {
            int i = 0;
            foreach (T item in list)
            {
                matches.Set(i, _getter(item) <= _value);
                i++;
            }
        }

        private void EvaluateGreaterThan(ICollection<T> list, BitArray matches)
        {
            int i = 0;
            foreach (T item in list)
            {
                matches.Set(i, _getter(item) > _value);
                i++;
            }
        }

        private void EvaluateGreaterThanOrEquals(ICollection<T> list, BitArray matches)
        {
            int i = 0;
            foreach (T item in list)
            {
                matches.Set(i, _getter(item) >= _value);
                i++;
            }
        }
    }
}
