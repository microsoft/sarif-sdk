using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Query.Evaluators
{
    /// <summary>
    ///  BoolEvaluator implements IExpressionEvaluator given a getter which can
    ///  get the desired Property Name as an enum.
    ///  
    ///  Usage: 
    ///    if (String.Equals(term.PropertyName, "BaselineState", StringComparison.OrdinalIgnoreCase))
    ///    {
    ///        // Show the BoolEvaluator how to get the 'BaselineState' property as an enum, and it'll implement the term matching.
    ///        return new BoolEvaluator&lt;Result, BaselineState&gt;(result => result.BaselineState, term);
    ///    }
    /// </summary>
    /// <typeparam name="T">Type of Item Evaluator will evaluate.</typeparam>
    public class BoolEvaluator<T> : IExpressionEvaluator<T>
    {
        private Func<T, bool> Getter { get; set; }
        private bool MustEqual { get; set; }

        private Action<IList<T>, BitArray> EvaluateSet { get; set; }

        public BoolEvaluator(Func<T, bool> getter, TermExpression term)
        {
            Getter = getter;

            if (!bool.TryParse(term.Value, out bool parsedValue)) { throw new QueryParseException($"{term} value {term.Value} was not a valid boolean."); }
            MustEqual = parsedValue;

            switch (term.Operator)
            {
                case CompareOperator.Equals:
                    break;
                case CompareOperator.NotEquals:
                    MustEqual = !MustEqual;
                    break;
                default:
                    throw new QueryParseException($"In {term}, {term.PropertyName} is boolean and only supports equals and not equals, not operator {term.Operator}");
            }

        }

        public void Evaluate(IList<T> list, BitArray matches)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                matches.Set(i, Getter(list[i]) == MustEqual);
            }
        }
    }
}
