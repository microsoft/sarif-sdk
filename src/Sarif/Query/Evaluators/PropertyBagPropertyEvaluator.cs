// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private readonly string _propertyName;
        private readonly bool _propertyBelongsToRule;
        private readonly IExpressionEvaluator<Result> _evaluator;

        private static readonly Regex s_propertyNameRegex = new Regex(
            @"^
                (?<prefix>properties\.|rule\.properties\.)
                (?<name>.+?)
                $",
            RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

        public PropertyBagPropertyEvaluator(TermExpression term)
        {
            Match match = s_propertyNameRegex.Match(term.PropertyName);
            _propertyName = match.Groups["name"].Value;

            string prefix = match.Groups["prefix"].Value;
            if (prefix.Equals(RulePropertyPrefix))
            {
                _propertyBelongsToRule = true;
            }
            else if (!prefix.Equals(ResultPropertyPrefix))
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        SdkResources.ErrorInvalidQueryPropertyPrefix,
                        "'" + string.Join("', '", ResultPropertyPrefix, RulePropertyPrefix)) + "'",
                    nameof(term));
            }

            _evaluator = CreateEvaluator(term);
        }

        // Create an appropriate evaluator object for the specified term. For operators that apply
        // only to strings (":" (contains), "|>" (startswith), or ">|" (endswith), always create
        // a string evaluator.  All other operators (==, !=, >, <, etc.) can apply to both strings
        // and numbers. If the value being compared parses as a number, assume that a numeric
        // comparison was intended, and create a numeric evaluator. Otherwise, create a string evaluator.
        // This could cause problems if the comparand is string that happens to look like a number.
        private IExpressionEvaluator<Result> CreateEvaluator(TermExpression term)
        {
            if (IsStringComparison(term))
                return new StringEvaluator<Result>(GetProperty<string>, term, StringComparison.OrdinalIgnoreCase);
            else if (IsDateTimeComparison(term))
                return new DateTimeEvaluator<Result>(GetProperty<DateTime>, term);
            else if (IsDoubleComparison(term))
                return new DoubleEvaluator<Result>(GetProperty<double>, term);
            else
                return new StringEvaluator<Result>(GetProperty<string>, term, StringComparison.OrdinalIgnoreCase);
        }

        private static readonly ReadOnlyCollection<CompareOperator> s_stringSpecificOperators =
            new ReadOnlyCollection<CompareOperator>(
                new CompareOperator[]
                {
                    CompareOperator.Contains,
                    CompareOperator.EndsWith,
                    CompareOperator.StartsWith
                });

        private bool IsStringComparison(TermExpression term)
            => s_stringSpecificOperators.Contains(term.Operator);

        private bool IsDoubleComparison(TermExpression term)
            => double.TryParse(term.Value, out _);

        private bool IsDateTimeComparison(TermExpression term)
            => DateTime.TryParse(term.Value, out _);

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
            try
            {
                return holder.TryGetProperty(_propertyName, out T value) ? value : default;
            }
            catch (JsonReaderException)
            {
                // Catch exceptions due to trying to perform a numeric comparison on a
                // property that turns out to have a string value that can't be parsed
                // as a number. The result will be that in such a case, the property
                // will be treated as if its value were numeric zero.
            }

            return default;
        }

        public void Evaluate(ICollection<Result> results, BitArray matches)
            => _evaluator.Evaluate(results, matches);
    }
}
