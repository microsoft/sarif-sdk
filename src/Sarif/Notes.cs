// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class Notes
    {
        public const string Msg001AnalyzingTarget = "MSG001.AnalyzingTarget";

        public static void LogNotApplicableToSpecifiedTarget(IAnalysisContext context, string reasonForNotAnalyzing)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Debug.Assert(context.TargetUri != null);

            // '{0}' was not evaluated for check '{1}' because the analysis
            // is not relevant for the following reason: {2}.
            context.Logger.Log(context.Rule,
                RuleUtilities.BuildResult(ResultKind.NotApplicable, context, null,
                    nameof(SdkResources.NotApplicable_InvalidMetadata),
                    context.TargetUri.GetFileName(),
                    context.Rule.Name,
                    reasonForNotAnalyzing));

            context.RuntimeErrors |= RuntimeConditions.RuleNotApplicableToTarget;
        }
    }
}