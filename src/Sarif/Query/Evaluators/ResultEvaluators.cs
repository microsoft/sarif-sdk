// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Query.Evaluators
{
    public static class SarifEvaluators
    {
        public static IExpressionEvaluator<Result> ResultEvaluator(TermExpression term)
        {
            switch (term.PropertyName.ToLowerInvariant())
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
                    return new LongEvaluator<Result>(r => r.OccurrenceCount, term);
                case "rank":
                    return new DoubleEvaluator<Result>(r => r.Rank, term);
                case "ruleid":
                    return new StringEvaluator<Result>(r => r.RuleId, term, StringComparison.OrdinalIgnoreCase);
                default:
                    throw new QueryParseException($"Property Name {term.PropertyName} unrecognized. Known Names: baselineState, correlationGuid, guid, hostedViewerUri, kind, level, message.text, occurrenceCount, rank, ruleId");
            }
        }
    }
}
