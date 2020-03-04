// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Query.Evaluators
{
    /// <summary>
    ///  StringEvaluator implements IExpressionEvaluator given a getter which can
    ///  get the desired Property Name as a string.
    ///  
    ///  Usage: 
    ///    if (String.Equals(term.PropertyName, "FileName", StringComparison.OrdinalIgnoreCase))
    ///    {
    ///        // Show the StringEvaluator how to get the 'FileName' property string, and it'll implement the term matching.
    ///        return new StringEvaluator&lt;Result&gt;(result => result.FileName, term);
    ///    }
    /// </summary>
    /// <typeparam name="T">Type of Item Evaluator will evaluate.</typeparam>
    public class StringEvaluator<T> : IExpressionEvaluator<T>
    {
        private Func<T, string> Getter { get; set; }
        private string Value { get; set; }
        private StringComparison StringComparison { get; set; }

        private Action<ICollection<T>, BitArray> EvaluateSet { get; set; }

        public StringEvaluator(Func<T, string> getter, TermExpression term, StringComparison stringComparison)
        {
            Getter = getter;
            Value = term.Value;
            EvaluateSet = Comparer(term);
            StringComparison = stringComparison;
        }

        public void Evaluate(ICollection<T> list, BitArray matches)
        {
            EvaluateSet(list, matches);
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
                case CompareOperator.StartsWith:
                    return EvaluateStartsWith;
                case CompareOperator.Contains:
                    return EvaluateContains;
                case CompareOperator.EndsWith:
                    return EvaluateEndsWith;

                default:
                    throw new QueryParseException($"{term} does not support operator {term.Operator}");
            }
        }

        private void EvaluateEquals(ICollection<T> list, BitArray matches)
        {
            int i = 0;
            foreach (T item in list)
            {
                matches.Set(i, string.Compare(Getter(item) ?? "", Value, StringComparison) == 0);
                i++;
            }
        }

        private void EvaluateNotEquals(ICollection<T> list, BitArray matches)
        {
            int i = 0;
            foreach (T item in list)
            {
                matches.Set(i, string.Compare(Getter(item) ?? "", Value, StringComparison) != 0);
                i++;
            }
        }

        private void EvaluateLessThan(ICollection<T> list, BitArray matches)
        {
            int i = 0;
            foreach (T item in list)
            {
                matches.Set(i, string.Compare(Getter(item) ?? "", Value, StringComparison) < 0);
                i++;
            }
        }

        private void EvaluateLessThanOrEquals(ICollection<T> list, BitArray matches)
        {
            int i = 0;
            foreach (T item in list)
            {
                matches.Set(i, string.Compare(Getter(item) ?? "", Value, StringComparison) <= 0);
                i++;
            }
        }

        private void EvaluateGreaterThan(ICollection<T> list, BitArray matches)
        {
            int i = 0;
            foreach (T item in list)
            {
                matches.Set(i, string.Compare(Getter(item) ?? "", Value, StringComparison) > 0);
                i++;
            }
        }

        private void EvaluateGreaterThanOrEquals(ICollection<T> list, BitArray matches)
        {
            int i = 0;
            foreach (T item in list)
            {
                matches.Set(i, string.Compare(Getter(item) ?? "", Value, StringComparison) >= 0);
                i++;
            }
        }

        private void EvaluateStartsWith(ICollection<T> list, BitArray matches)
        {
            int i = 0;
            foreach (T item in list)
            {
                matches.Set(i, (Getter(item) ?? "").StartsWith(Value, StringComparison));
                i++;
            }
        }

        private void EvaluateContains(ICollection<T> list, BitArray matches)
        {
            int i = 0;
            foreach (T item in list)
            {
                matches.Set(i, (Getter(item) ?? "").IndexOf(Value, StringComparison) != -1);
                i++;
            }
        }

        private void EvaluateEndsWith(ICollection<T> list, BitArray matches)
        {
            int i = 0;
            foreach (T item in list)
            {
                matches.Set(i, (Getter(item) ?? "").EndsWith(Value, StringComparison));
                i++;
            }
        }
    }
}
