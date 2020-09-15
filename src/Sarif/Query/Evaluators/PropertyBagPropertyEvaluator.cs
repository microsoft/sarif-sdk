// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Query.Evaluators
{
    public class PropertyBagPropertyEvaluator : IExpressionEvaluator<Result>
    {
        internal const string ResultPropertyPrefix = "properties.";
        private static readonly int ResultPropertyPrefixLength = ResultPropertyPrefix.Length;

        internal const string RulePropertyPrefix = "rule.properties.";
        private static readonly int RulePropertyPrefixLength = RulePropertyPrefix.Length;

        private readonly string _propertyName;
        private readonly bool _propertyBelongsToRule;
        private readonly StringEvaluator<Result> _stringEvaluator;

        public PropertyBagPropertyEvaluator(TermExpression term)
        {
            if (term.PropertyName.StartsWith(ResultPropertyPrefix))
            {
                _propertyName = term.PropertyName.Substring(ResultPropertyPrefixLength);
            }
            else if (term.PropertyName.StartsWith(RulePropertyPrefix))
            {
                _propertyName = term.PropertyName.Substring(RulePropertyPrefixLength);
                _propertyBelongsToRule = true;
            }
            else
            {
                throw new ArgumentException($"Property name must start with either '{ResultPropertyPrefix}' or '{RulePropertyPrefix}'.", nameof(term));
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
