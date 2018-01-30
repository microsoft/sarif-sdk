// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using System;
using System.Globalization;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class Warnings
    {
        public const string Wrn997InvalidTarget = "WRN997.InvalidTarget";
        public const string Wrn998NotSupportedPlatform = "WRN998.NotSupportedPlatform";

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
                    PhysicalLocation = new PhysicalLocation { Uri = context.TargetUri },
                    Id = Wrn997InvalidTarget,
                    Message = message,
                    Level = NotificationLevel.Note,
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
                    PhysicalLocation = new PhysicalLocation { Uri = context.TargetUri },
                    Id = Wrn998NotSupportedPlatform,
                    Message = message,
                    Level = NotificationLevel.Note,
                });

            context.RuntimeErrors |= RuntimeConditions.RuleCannotRunOnPlatform;
        }
    }
}