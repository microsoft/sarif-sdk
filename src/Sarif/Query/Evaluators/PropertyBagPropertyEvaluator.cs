// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Query.Evaluators
{
    public class PropertyBagPropertyEvaluator : IExpressionEvaluator<Result>
    {
        private const string RulePropertyPrefix = "rule.";
        private static readonly int RulePropertyPrefixLength = RulePropertyPrefix.Length;

        private readonly string _propertyName;
        private readonly bool _propertyBelongsToRule;
        private readonly StringEvaluator<Result> _stringEvaluator;

        public PropertyBagPropertyEvaluator(TermExpression term)
        {
            _propertyName = term.PropertyName;
            _propertyBelongsToRule = _propertyName.StartsWith(RulePropertyPrefix);
            if (_propertyBelongsToRule)
            {
                _propertyName = _propertyName.Substring(RulePropertyPrefixLength);
            }

            _stringEvaluator = new StringEvaluator<Result>(GetProperty, term, StringComparison.OrdinalIgnoreCase);
        }

        private string GetProperty(Result result)
        {
            string propertyValue = null;
            if (_propertyBelongsToRule)
            {
                ReportingDescriptor rule = result.GetRule();
                if (rule != null)
                {
                    rule.TryGetProperty(_propertyName, out propertyValue);
                }
            }
            else
            {
                result.TryGetProperty(_propertyName, out propertyValue);
            }

            return propertyValue;
        }

        public void Evaluate(ICollection<Result> results, BitArray matches)
            => _stringEvaluator.Evaluate(results, matches);
    }
}
