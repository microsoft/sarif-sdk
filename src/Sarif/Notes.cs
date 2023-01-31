// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class Notes
    {
        public const string Msg001AnalyzingTarget = "MSG001.AnalyzingTarget";

        public const string Msg002_FileSkippedDueToSize = "MSG002.FileSkippedDueToSize";

        public static void LogFileSkippedDueToSize(IAnalysisContext context, string skippedFile, long fileSizeInKb)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // '{0}' was skipped as its size ({1} kilobytes) exceeds the currently configured threshold ({2} kilobytes).
            context.Logger.LogConfigurationNotification(
                Errors.CreateNotification(
                    new Uri(skippedFile, UriKind.RelativeOrAbsolute),
                    Msg002_FileSkippedDueToSize,
                    ruleId: null,
                    FailureLevel.Note,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null,
                    skippedFile,
                    fileSizeInKb.ToString(CultureInfo.CurrentCulture),
                    context.MaxFileSizeInKilobytes.ToString()));

            context.RuntimeErrors |= RuntimeConditions.OneOrMoreFilesSkippedDueToSize;
        }

        public static void LogNotApplicableToSpecifiedTarget(IAnalysisContext context, string reasonForNotAnalyzing)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Debug.Assert(context.CurrentTarget.Uri != null);

            // '{0}' was not evaluated for check '{1}' because the analysis
            // is not relevant for the following reason: {2}.
            context.Logger.Log(context.Rule,
                RuleUtilities.BuildResult(ResultKind.NotApplicable, context, null,
                    nameof(SdkResources.NotApplicable_InvalidMetadata),
                    context.CurrentTarget.Uri.GetFileName(),
                    context.Rule.Name,
                    reasonForNotAnalyzing));

            context.RuntimeErrors |= RuntimeConditions.RuleNotApplicableToTarget;
        }
    }
}
