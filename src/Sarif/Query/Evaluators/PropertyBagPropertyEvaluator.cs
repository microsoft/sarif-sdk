// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
            if (term.PropertyName.StartsWith(ResultPropertyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                _propertyName = term.PropertyName.Substring(ResultPropertyPrefixLength);
            }
            else if (term.PropertyName.StartsWith(RulePropertyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                _propertyName = term.PropertyName.Substring(RulePropertyPrefixLength);
                _propertyBelongsToRule = true;
            }
            else
            {
                throw new ArgumentException($"Property name must start with either '{ResultPropertyPrefix}' or '{RulePropertyPrefix}'.", nameof(term));
            }

            _stringEvaluator = new StringEvaluator<Result>(GetProperty<string>, term, StringComparison.OrdinalIgnoreCase);
        }

        private T GetProperty<T>(Result result)
        {
            PropertyBagHolder holder = GetPropertyBagHolder(result);
            return holder != null
                ? GetPropertyFromHolder<T>(holder)
                : default;
        }

        private PropertyBagHolder GetPropertyBagHolder(Result result)
        {
            PropertyBagHolder propertyBagHolder = null;
            if (_propertyBelongsToRule)
            {
                ReportingDescriptor rule = result.GetRule();
                if (rule != null)
                {
                    propertyBagHolder = rule;
                }
            }
            else
            {
                propertyBagHolder = result;
            }

            return propertyBagHolder;
        }

        private T GetPropertyFromHolder<T>(PropertyBagHolder holder)
        {
            T value = default;

            List<string> propertyNames = holder.PropertyNames
                .Where(key => key.Equals(_propertyName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (propertyNames.Any())
            {
                value = holder.GetProperty<T>(propertyNames.First());
            }

            return value;
        }

        public void Evaluate(ICollection<Result> results, BitArray matches)
            => _stringEvaluator.Evaluate(results, matches);
    }
}
