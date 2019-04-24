// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class Warnings
    {
        public const string Wrn997_InvalidTarget = "WRN997.InvalidTarget";
        public const string Wrn998_UnsupportedPlatform = "WRN998.UnsupportedPlatform";
        public const string Wrn999_RuleExplicitlyDisabled = "WRN999.RuleExplicitlyDisabled";

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
    }
}