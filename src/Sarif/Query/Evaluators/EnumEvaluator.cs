// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
        private readonly Func<T, EnumType> _getter;
        private readonly EnumType _value;
        private readonly Action<ICollection<T>, BitArray> _evaluateSet;

        public EnumEvaluator(Func<T, EnumType> getter, TermExpression term)
        {
            _getter = getter;

            if (!Enum.TryParse<EnumType>(term.Value, out EnumType parsedValue)) { throw new QueryParseException($"{term} value {term.Value} was not a valid {typeof(EnumType).Name}."); }
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
                default:
                    throw new QueryParseException($"In {term}, {term.PropertyName} only supports equals and not equals, not operator {term.Operator}");
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
    }
}
