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

        private Action<IList<T>, BitArray> EvaluateSet { get; set; }

        public StringEvaluator(Func<T, string> getter, TermExpression term, StringComparison stringComparison)
        {
            Getter = getter;
            Value = term.Value;
            EvaluateSet = Comparer(term);
            StringComparison = stringComparison;
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
                matches.Set(i, String.Compare(Getter(list[i]), Value, StringComparison) == 0);
            }
        }

        private void EvaluateNotEquals(IList<T> list, BitArray matches)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                matches.Set(i, String.Compare(Getter(list[i]), Value, StringComparison) != 0);
            }
        }

        private void EvaluateLessThan(IList<T> list, BitArray matches)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                matches.Set(i, String.Compare(Getter(list[i]), Value, StringComparison) < 0);
            }
        }

        private void EvaluateLessThanOrEquals(IList<T> list, BitArray matches)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                matches.Set(i, String.Compare(Getter(list[i]), Value, StringComparison) <= 0);
            }
        }

        private void EvaluateGreaterThan(IList<T> list, BitArray matches)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                matches.Set(i, String.Compare(Getter(list[i]), Value, StringComparison) > 0);
            }
        }

        private void EvaluateGreaterThanOrEquals(IList<T> list, BitArray matches)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                matches.Set(i, String.Compare(Getter(list[i]), Value, StringComparison) >= 0);
            }
        }
    }
}
