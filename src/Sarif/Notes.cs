// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


namespace Microsoft.CodeAnalysis.Sarif
{
    public static class Notes
    {
        private const string MSG1001 = "MSG1001";
        private const string MSG1002 = "MSG1002";

        public static IRule AnalyzingTarget = new Rule()
        {
            // Analyzing {0}...
            Id = MSG1001,
            Name = nameof(AnalyzingTarget),
            FullDescription = SdkResources.MSG1001_AnalyzingTarget_Description,
            MessageFormats = RuleUtilities.BuildDictionary(SdkResources.ResourceManager,
                new string[] {
                    nameof(SdkResources.MSG1001_AnalyzingTarget),
                }, MSG1001)
        };

        public static IRule InvalidTarget = new Rule()
        {
            // A file was skipped as it does not appear to be a valid target for analysis.
            Id = MSG1002,
            Name = nameof(InvalidTarget),
            FullDescription = SdkResources.MSG1002_InvalidTarget_Description,
            MessageFormats = RuleUtilities.BuildDictionary(SdkResources.ResourceManager,
                new string[] {
                    nameof(SdkResources.MSG1002_InvalidFileType),
                    nameof(SdkResources.MSG1002_InvalidMetadata)
                }, MSG1002)
        };

        public static void LogNotApplicableToSpecifiedTarget(IAnalysisContext context, string reasonForNotAnalyzing)
        {
            string ruleId = context.Rule.Id;
            string ruleName = context.Rule.Name;
            context.Rule = Notes.InvalidTarget;

            // '{0}' was not evaluated for check '{1}:{2}' as the analysis
            // is not relevant based on observed metadata: {3}.
            context.Logger.Log(context.Rule,
                RuleUtilities.BuildResult(ResultLevel.NotApplicable, context, null,
                    nameof(SdkResources.MSG1002_InvalidMetadata),
                    ruleId,
                    ruleName,
                    reasonForNotAnalyzing));

            context.RuntimeErrors |= RuntimeConditions.RuleNotApplicableToTarget;
        }

        public static void LogExceptionInvalidTarget(IAnalysisContext context)
        {
            context.Rule = Notes.InvalidTarget;

            // '{0}' was not analyzed as it does not appear
            // to be a valid file type for analysis.
            context.Logger.Log(context.Rule,
                RuleUtilities.BuildResult(ResultLevel.NotApplicable, context, null,
                    nameof(SdkResources.MSG1002_InvalidFileType)));

            context.RuntimeErrors |= RuntimeConditions.TargetNotValidToAnalyze;
        }
    }
}