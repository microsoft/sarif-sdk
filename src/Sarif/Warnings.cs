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
        public const string Wrn997_OneOrMoreFilesSkippedDueToSize = "WRN997.OneOrMoreFilesSkippedDueToSize";

        public const string Wrn997_ObsoleteOption = "WRN997.ObsoleteOption";
        public const string Wrn997_ObsoleteOptionWithReplacement = "WRN997.ObsoleteOptionWithReplacement";

        // (Non-catastrophic) conditions that result in rules disabling themselves.
        public const string Wrn998_UnsupportedPlatform = "WRN998.UnsupportedPlatform";
        public const string Wrn998_IncompatibleRuleDetected = "WRN998.IncompatibleRuleDetected";

        // Warnings around dangerous
        public const string Wrn999_RuleExplicitlyDisabled = "WRN999.RuleExplicitlyDisabled";

        public static void LogOneOrMoreFilesSkippedDueToSize(IAnalysisContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // One or more files were skipped for analysis due to exceeding size limits
            // (currently configured as {0} kilobytes). The 'max-file-size-in-kb'
            // command-line argument can be used to increase this threshold.
            context.Logger.LogConfigurationNotification(
                Errors.CreateNotification(
                    context.TargetUri,
                    Wrn997_OneOrMoreFilesSkippedDueToSize,
                    ruleId: null,
                    FailureLevel.Warning,
                    exception: null,
                    persistExceptionStack: false,
                    messageFormat: null,
                    context.MaxFileSizeInKilobytes.ToString()));

            context.RuntimeErrors |= RuntimeConditions.OneOrMoreFilesSkippedDueToSize;
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
                context.TargetUri.GetFileName());

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
                                    Uri = context.TargetUri
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
                    Locations = new List<Location>
                    {
                        new Location
                        {
                            PhysicalLocation = new PhysicalLocation
                            {
                                ArtifactLocation = new ArtifactLocation
                                {
                                    Uri = context.TargetUri
                                }
                            }
                        }
                    },
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

        public static void LogIncompatibleRule(IAnalysisContext context, string ruleId, string incompatibleRuleId)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string message = string.Format(
                CultureInfo.InvariantCulture,
                SdkResources.Wrn998_IncompatibleRuleDetected,
                ruleId,
                incompatibleRuleId);

            context.Logger.LogConfigurationNotification(
                new Notification
                {
                    Descriptor = new ReportingDescriptorReference
                    {
                        Id = Wrn998_IncompatibleRuleDetected
                    },
                    Message = new Message { Text = message },
                    Level = FailureLevel.Warning,
                });

            context.RuntimeErrors |= RuntimeConditions.RuleIsIncompatibleWithAnotherRule;
        }
    }
}
