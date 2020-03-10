// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Query.Evaluators
{
    /// <summary>
    ///  DoubleEvaluator implements IExpressionEvaluator given a getter which can
    ///  get the desired Property Name as a double.
    ///  
    ///  Usage: 
    ///    if (String.Equals(term.PropertyName, "ID", StringComparison.OrdinalIgnoreCase))
    ///    {
    ///        // Show the DoubleEvaluator how to get the 'ID' property as a double, and it'll implement the term matching.
    ///        return new DoubleEvaluator&lt;Result&gt;(result => result.ID, term);
    ///    }
    /// </summary>
    /// <typeparam name="T">Type of Item Evaluator will evaluate.</typeparam>
    public class DoubleEvaluator<T> : IExpressionEvaluator<T>
    {
        private readonly Func<T, double> _getter;
        private readonly double _value;
        private readonly Action<ICollection<T>, BitArray> _evaluateSet;

        public DoubleEvaluator(Func<T, double> getter, TermExpression term)
        {
            _getter = getter;

            if (!double.TryParse(term.Value, out double parsedValue)) { throw new QueryParseException($"{term} value {term.Value} was not a valid floating point number."); }
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
                matches.Set(i, _getter(item) == _value);
                i++;
            }
        }

        private void EvaluateNotEquals(ICollection<T> list, BitArray matches)
        {
            int i = 0;
            foreach (T item in list)
            {
                matches.Set(i, _getter(item) != _value);
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
