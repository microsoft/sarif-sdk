// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace Microsoft.CodeAnalysis.Sarif.Query.Evaluators
{
    /// <summary>
    ///  BigIntegerEvaluator implements IExpressionEvaluator given a getter which can
    ///  get the desired Property Name as a BigInteger.
    ///
    ///  Usage:
    ///    if (String.Equals(term.PropertyName, "ID", StringComparison.OrdinalIgnoreCase))
    ///    {
    ///        // Show the BigIntegerEvaluator how to get the 'ID' property as a BigInteger, and it'll implement the term matching.
    ///        return new BigIntegerEvaluator&lt;Result&gt;(result => result.ID, term);
    ///    }
    /// </summary>
    /// <typeparam name="T">Type of Item Evaluator will evaluate.</typeparam>
    public class BigIntegerEvaluator<T> : IExpressionEvaluator<T>
    {
        private readonly Func<T, BigInteger> _getter;
        private readonly Action<ICollection<T>, BitArray> _evaluateSet;

        public BigInteger Value { get; }

        public BigIntegerEvaluator(Func<T, BigInteger> getter, TermExpression term)
        {
            _getter = getter;

            if (!BigInteger.TryParse(term.Value, out BigInteger parsedValue)) { throw new QueryParseException($"{term} value {term.Value} was not a valid number."); }
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
