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
        private Func<T, double> Getter { get; set; }
        private double Value { get; set; }

        private Action<IList<T>, BitArray> EvaluateSet { get; set; }

        public DoubleEvaluator(Func<T, double> getter, TermExpression term)
        {
            Getter = getter;

            if (!double.TryParse(term.Value, out double parsedValue)) { throw new QueryParseException($"{term} value {term.Value} was not a valid floating point number."); }
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
