using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Query.Evaluators
{
    /// <summary>
    ///  EnumEvaluator implements IExpressionEvaluator given a getter which can
    ///  get the desired Property Name as an enum.
    ///  
    ///  Usage: 
    ///    if (String.Equals(term.PropertyName, "BaselineState", StringComparison.OrdinalIgnoreCase))
    ///    {
    ///        // Show the EnumEvaluator how to get the 'BaselineState' property as an enum, and it'll implement the term matching.
    ///        return new EnumEvaluator&lt;Result, BaselineState&gt;(result => result.BaselineState, term);
    ///    }
    /// </summary>
    /// <typeparam name="T">Type of Item Evaluator will evaluate.</typeparam>
    /// <typeparam name="EnumType">Type of Enum of property being compared</typeparam>
    public class EnumEvaluator<T, EnumType> : IExpressionEvaluator<T> where EnumType : struct, Enum
    {
        private Func<T, EnumType> Getter { get; set; }
        private EnumType Value { get; set; }

        private Action<IList<T>, BitArray> EvaluateSet { get; set; }

        public EnumEvaluator(Func<T, EnumType> getter, TermExpression term)
        {
            Getter = getter;

            if (!Enum.TryParse<EnumType>(term.Value, out EnumType parsedValue)) { throw new QueryParseException($"{term} value {term.Value} was not a valid {typeof(EnumType).Name}."); }
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
                default:
                    throw new QueryParseException($"In {term}, {term.PropertyName} only supports equals and not equals, not operator {term.Operator}");
            }
        }

        private void EvaluateEquals(IList<T> list, BitArray matches)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                matches.Set(i, Getter(list[i]).Equals(Value));
            }
        }

        private void EvaluateNotEquals(IList<T> list, BitArray matches)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                matches.Set(i, !Getter(list[i]).Equals(Value));
            }
        }
    }
}
