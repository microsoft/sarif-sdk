// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class Notes
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Msg")]
        public const string Msg001AnalyzingTarget = "MSG001.AnalyzingTarget";

        public static void LogNotApplicableToSpecifiedTarget(IAnalysisContext context, string reasonForNotAnalyzing)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // '{0}' was not evaluated for check '{1}' as the analysis
            // is not relevant based on observed metadata: {2}.
            context.Logger.Log(context.Rule,
                RuleUtilities.BuildResult(ResultLevel.NotApplicable, context, null,
                    nameof(SdkResources.NotApplicable_InvalidMetadata),
                    context.Rule.Name,
                    reasonForNotAnalyzing));

            context.RuntimeErrors |= RuntimeConditions.RuleNotApplicableToTarget;
        }
    }
}