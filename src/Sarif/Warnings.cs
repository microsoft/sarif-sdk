// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class Warnings
    {
        // Conditions that may indicate an issue with command-line configuration.
        public const string Wrn997_InvalidTarget = "WRN997.InvalidTarget";
        public const string Wrn997_ObsoleteOption = "WRN997.ObsoleteOption";
        public const string Wrn997_ObsoleteOptionWithReplacement = "WRN997.ObsoleteOptionWithReplacement";
        public const string Wrn997_OneOrMoreFilesSkipped = "WRN997.OneOrMoreFilesSkipped";
        public const string Wrn997_OneOrMoreFilesSkippedDueToExceedingSizeLimits = "WRN997.OneOrMoreFilesSkippedDueToExceedingSizeLimit";

        // (Non-catastrophic) conditions that result in rules disabling themselves.
        public const string Wrn998_UnsupportedPlatform = "WRN998.UnsupportedPlatform";

        // Warnings around conditions of potential concern. An explicitly disabled rule,
        // for example, might prevent an analysis run from meeting compliance goals.
        public const string Wrn999_RuleExplicitlyDisabled = "WRN999.RuleExplicitlyDisabled";

        public static void LogOneOrMoreFilesSkippedDue(IAnalysisContext context, long skippedFilesCount, string reason)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // {0} file(s) were skipped for analysis as {1}.
            context.Logger.LogConfigurationNotification(
                Errors.CreateNotification(
                    context.CurrentTarget?.Uri,
                    Wrn997_OneOrMoreFilesSkippedDueToExceedingSizeLimits,
                    ruleId: null,
                    FailureLevel.Warning,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null,
                    skippedFilesCount.ToString(),
                    reason));

            context.RuntimeErrors |= RuntimeConditions.OneOrMoreFilesSkipped;
        }

        public static void LogOneOrMoreFilesSkippedDueToExceedingSizeLimit(IAnalysisContext context, long skippedFilesCount)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // {0} file(s)s were skipped for analysis due to exceeding size limit
            // (currently configured as {1} kilobytes). The 'max-file-size-in-kb'
            // command-line argument can be used to increase this threshold.
            context.Logger.LogConfigurationNotification(
                Errors.CreateNotification(
                    context.CurrentTarget?.Uri,
                    Wrn997_OneOrMoreFilesSkippedDueToExceedingSizeLimits,
                    ruleId: null,
                    FailureLevel.Warning,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null,
                    skippedFilesCount.ToString(),
                    context.MaxFileSizeInKilobytes.ToString()));

            context.RuntimeErrors |= RuntimeConditions.OneOrMoreFilesSkippedDueToExceedingSizeLimits;
        }

        public static void LogExceptionInvalidTarget(IAnalysisContext context)
        {
            // '{0}' was not analyzed as it does not appear to be a valid file type for analysis.
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string message = string.Format(CultureInfo.InvariantCulture,
                SdkResources.WRN997_InvalidTarget,
                context.CurrentTarget.Uri.GetFileName());

            context.Logger.LogConfigurationNotification(
                new Notification
                {
                    Locations = new List<Location>
                    {
                        new Location
                        {
                            PhysicalLocation = new PhysicalLocation
                            {
                                ArtifactLocation = new ArtifactLocation
                                {
                                    Uri = context.CurrentTarget.Uri
                                }
                            }
                        }
                    },
                    Descriptor = new ReportingDescriptorReference
                    {
                        Id = Wrn997_InvalidTarget
                    },
                    Message = new Message { Text = message },
                    Level = FailureLevel.Note,
                });

            context.RuntimeErrors |= RuntimeConditions.TargetNotValidToAnalyze;
        }

        public static void LogUnsupportedPlatformForRule(IAnalysisContext context, string ruleId, SupportedPlatform supportedOS, SupportedPlatform currentOS)
        {
            // Rule '{0}' was disabled as it cannot run on the current platform '{1}'.  It can only run on '{2}'.
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string message = string.Format(CultureInfo.InvariantCulture,
                SdkResources.WRN998_NotSupportedPlatform,
                ruleId,
                currentOS,
                supportedOS);

            context.Logger.LogConfigurationNotification(
                new Notification
                {
                    Descriptor = new ReportingDescriptorReference
                    {
                        Id = Wrn998_UnsupportedPlatform
                    },
                    Message = new Message { Text = message },
                    Level = FailureLevel.Warning,
                });

            context.RuntimeErrors |= RuntimeConditions.RuleCannotRunOnPlatform;
        }

        public static void LogRuleExplicitlyDisabled(IAnalysisContext context, string ruleId)
        {
            // Rule '{0}' was explicitly disabled by the user. As result, this too run
            // cannot be used to for compliance or other auditing processes that
            // require a comprehensive analysis.

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string message = string.Format(CultureInfo.InvariantCulture,
                SdkResources.WRN999_RuleExplicitlyDisabled,
                ruleId);

            context.Logger.LogConfigurationNotification(
                new Notification
                {
                    Descriptor = new ReportingDescriptorReference
                    {
                        Id = Wrn999_RuleExplicitlyDisabled
                    },
                    Message = new Message { Text = message },
                    Level = FailureLevel.Warning,
                });

            context.RuntimeErrors |= RuntimeConditions.RuleWasExplicitlyDisabled;
        }

        public static void LogObsoleteOption(IAnalysisContext context, string obsoleteOption, string replacement = null)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string message;

            if (string.IsNullOrWhiteSpace(replacement))
            {
                message = string.Format(CultureInfo.InvariantCulture,
                                        SdkResources.WRN997_ObsoleteOption,
                                        obsoleteOption);
            }
            else
            {
                message = string.Format(CultureInfo.InvariantCulture,
                                        SdkResources.WRN997_ObsoleteOptionWithReplacement,
                                        obsoleteOption,
                                        replacement);
            }

            context.Logger.LogConfigurationNotification(
                new Notification
                {
                    Descriptor = new ReportingDescriptorReference
                    {
                        Id = Wrn997_ObsoleteOption
                    },
                    Message = new Message { Text = message },
                    Level = FailureLevel.Warning,
                });
        }
    }
}
