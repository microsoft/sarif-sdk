// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
        private readonly Func<T, bool> _getter;
        private readonly bool _mustEqual;

        public BoolEvaluator(Func<T, bool> getter, TermExpression term)
        {
            _getter = getter;

            if (!bool.TryParse(term.Value, out bool parsedValue)) { throw new QueryParseException($"{term} value {term.Value} was not a valid boolean."); }
            _mustEqual = parsedValue;

            switch (term.Operator)
            {
                case CompareOperator.Equals:
                    break;
                case CompareOperator.NotEquals:
                    _mustEqual = !_mustEqual;
                    break;
                default:
                    throw new QueryParseException($"In {term}, {term.PropertyName} is boolean and only supports equals and not equals, not operator {term.Operator}");
            }

        }

        public void Evaluate(ICollection<T> list, BitArray matches)
        {
            int i = 0;
            foreach (T item in list)
            {
                matches.Set(i, _getter(item) == _mustEqual);
                i++;
            }
        }
    }
}
