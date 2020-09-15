// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Query.Evaluators
{
    public class PropertyBagPropertyEvaluator : IExpressionEvaluator<Result>
    {
        internal const string ResultPropertyPrefix = "properties.";
        internal const string RulePropertyPrefix = "rule.properties.";

        internal const string IntegerType = "n";
        internal const string FloatType = "f";
        internal const string StringType = "s";

        private readonly string _propertyName;
        private readonly bool _propertyBelongsToRule;
        private readonly IExpressionEvaluator<Result> _evaluator;

        private static readonly Regex s_propertyNameRegex = new Regex(
            @$"^
                (?<prefix>properties\.|rule\.properties\.)
                (?<name>.+?)
                (:(?<type>.))?
                $",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

        public PropertyBagPropertyEvaluator(TermExpression term)
        {
            Match match = s_propertyNameRegex.Match(term.PropertyName);
            _propertyName = match.Groups["name"].Value;

            string prefix = match.Groups["prefix"].Value;
            if (prefix.Equals(RulePropertyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                _propertyBelongsToRule = true;
            }
            else if (!prefix.Equals(ResultPropertyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        SdkResources.ErrorInvalidQueryPropertyPrefix,
                        "'" + string.Join("', '", ResultPropertyPrefix, RulePropertyPrefix)) + "'",
                    nameof(term));
            }

            string type = match.Groups["type"].Value;
            if (type.Equals(string.Empty, StringComparison.OrdinalIgnoreCase) ||
                type.Equals(StringType, StringComparison.OrdinalIgnoreCase))
            {
                _evaluator = new StringEvaluator<Result>(GetProperty<string>, term, StringComparison.OrdinalIgnoreCase);
            }
            else if (type.Equals(IntegerType, StringComparison.OrdinalIgnoreCase))
            {
                _evaluator = new LongEvaluator<Result>(GetProperty<long>, term);
            }
            else if (type.Equals(FloatType, StringComparison.OrdinalIgnoreCase))
            {
                _evaluator = new DoubleEvaluator<Result>(GetProperty<double>, term);
            }
            else
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        SdkResources.ErrorInvalidQueryPropertyType,
                        "'" + string.Join("', '", StringType, IntegerType, FloatType) + "'",
                    nameof(term)));
            }
        }

        private T GetProperty<T>(Result result)
        {
            PropertyBagHolder holder = GetPropertyBagHolder(result);
            return GetPropertyFromHolder<T>(holder);
        }

        private PropertyBagHolder GetPropertyBagHolder(Result result) =>
            _propertyBelongsToRule
            ? result.GetRule() as PropertyBagHolder
            : result;

        private T GetPropertyFromHolder<T>(PropertyBagHolder holder)
        {
            T value = default;

            // We can't just call holder.TryGetProperty because we want a case-insensitive
            // match.
            List<string> propertyNames = holder.PropertyNames
                .Where(key => key.Equals(_propertyName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (propertyNames.Any())
            {
                try
                {
                    value = holder.GetProperty<T>(propertyNames.First());
                }
                catch (JsonSerializationException)
                {
                    // A serialization exception means (for example) that we tried to compare
                    // a string-valued property that can't be parsed to an integer with an
                    // integer value.s
                }
            }

            return value;
        }

        public void Evaluate(ICollection<Result> results, BitArray matches)
            => _evaluator.Evaluate(results, matches);
    }
}
