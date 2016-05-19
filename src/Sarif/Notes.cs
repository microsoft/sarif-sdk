// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


namespace Microsoft.CodeAnalysis.Sarif
{
    public static class Notes
    {
        public const string MSG001_AnalyzingTarget = "MSG001.AnalyzingTarget";

        public static void LogNotApplicableToSpecifiedTarget(IAnalysisContext context, string reasonForNotAnalyzing)
        {
            // '{0}' was not evaluated for check '{1}' as the analysis
            // is not relevant based on observed metadata: {2}.
            context.Logger.Log(context.Rule,
                RuleUtilities.BuildResult(ResultLevel.NotApplicable, context, null,
                    nameof(SdkResources.SHARED_InvalidMetadata),
                    context.Rule.Name,
                    reasonForNotAnalyzing));

            context.RuntimeErrors |= RuntimeConditions.RuleNotApplicableToTarget;
        }
    }
}