// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class Notes
    {
        public const string Msg001AnalyzingTarget = "MSG001.AnalyzingTarget";
        public const string Msg002_FileSkipped = "MSG002.FileSkipped";
        public const string Msg002_EmptyFileSkipped = "MSG002.EmptyFileSkipped";
        public const string Msg002_FileExceedingSizeLimitSkipped = "MSG002.FileExceedingSizeLimitSkipped";


        public static void LogFileSkipped(IAnalysisContext context, string skippedFile, string reason)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // '{1}' was skipped as {reason}.
            context.Logger.LogConfigurationNotification(
                Errors.CreateNotification(
                    new Uri(skippedFile, UriKind.RelativeOrAbsolute),
                    Msg002_FileSkipped,
                    ruleId: null,
                    FailureLevel.Note,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null,
                    skippedFile,
                    Path.GetFileName(skippedFile),
                    reason));

            context.RuntimeErrors |= RuntimeConditions.OneOrMoreFilesSkipped;
        }

        public static void LogEmptyFileSkipped(IAnalysisContext context, string skippedFile)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // '{0}' was skipped as it is zero bytes in size.
            context.Logger.LogConfigurationNotification(
                Errors.CreateNotification(
                    new Uri(skippedFile, UriKind.RelativeOrAbsolute),
                    Msg002_EmptyFileSkipped,
                    ruleId: null,
                    FailureLevel.Note,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null,
                    skippedFile,
                    Path.GetFileName(skippedFile),
                    context.MaxFileSizeInKilobytes.ToString()));

            context.RuntimeErrors |= RuntimeConditions.OneOrMoreEmptyFilesSkipped;
        }

        public static void LogFileExceedingSizeLimitSkipped(IAnalysisContext context, string skippedFile, long fileSizeInKb)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // '{0}' was skipped as its size ({1} kilobytes) exceeds the currently configured threshold ({2} kilobytes).
            context.Logger.LogConfigurationNotification(
                Errors.CreateNotification(
                    new Uri(skippedFile, UriKind.RelativeOrAbsolute),
                    Msg002_FileExceedingSizeLimitSkipped,
                    ruleId: null,
                    FailureLevel.Note,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null,
                    skippedFile,
                    Path.GetFileName(skippedFile),
                    fileSizeInKb.ToString(CultureInfo.CurrentCulture),
                    context.MaxFileSizeInKilobytes.ToString()));

            context.RuntimeErrors |= RuntimeConditions.OneOrMoreFilesSkippedDueToExceedingSizeLimits;
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
