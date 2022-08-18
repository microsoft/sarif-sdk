// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Query.Evaluators
{
    public static class SarifEvaluators
    {
        public static IExpressionEvaluator<Result> ResultEvaluator(TermExpression term)
        {
            string propertyNameLower = term.PropertyName.ToLowerInvariant();
            switch (propertyNameLower)
            {
                case "baselinestate":
                    return new EnumEvaluator<Result, BaselineState>(r => r.BaselineState, term);
                case "correlationguid":
                    return new StringEvaluator<Result>(r => r.CorrelationGuid, term, StringComparison.OrdinalIgnoreCase);
                case "guid":
                    return new StringEvaluator<Result>(r => r.Guid, term, StringComparison.OrdinalIgnoreCase);
                case "hostedvieweruri":
                    return new StringEvaluator<Result>(r => r.HostedViewerUri?.ToString(), term, StringComparison.OrdinalIgnoreCase);
                case "kind":
                    return new EnumEvaluator<Result, ResultKind>(r => r.Kind, term);
                case "level":
                    return new EnumEvaluator<Result, FailureLevel>(r => r.Level, term);
                case "message.text":
                    return new StringEvaluator<Result>(r => r.Message?.Text, term, StringComparison.OrdinalIgnoreCase);
                case "occurrencecount":
                    return new LongEvaluator<Result>(r => (long)r.OccurrenceCount, term);
                case "rank":
                    return new DoubleEvaluator<Result>(r => r.Rank, term);
                case "ruleid":
                    return new StringEvaluator<Result>(r => r.GetRule(r.Run).Id, term, StringComparison.OrdinalIgnoreCase);

                case "uri":
                    // Ensure the Run is provided, to look up Uri from Run.Artifacts when needed.
                    // Uri : "/Core/" will match all Results with any Uri which contains "/Core/"
                    return new SetEvaluator<Result, string>(r =>
                    {
                        r.EnsureRunProvided();
                        return r.Locations?.Select(l => l?.PhysicalLocation?.ArtifactLocation.Resolve(r.Run)?.Uri?.ToString() ?? "");
                    }, term);

                default:
                    if (propertyNameLower.StartsWith(PropertyBagPropertyEvaluator.ResultPropertyPrefix) ||
                        propertyNameLower.StartsWith(PropertyBagPropertyEvaluator.RulePropertyPrefix))
                    {
                        return new PropertyBagPropertyEvaluator(term);
                    }

                    throw new QueryParseException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            SdkResources.ErrorInvalidQueryPropertyName,
                            term.PropertyName));
            }
        }
    }
}
