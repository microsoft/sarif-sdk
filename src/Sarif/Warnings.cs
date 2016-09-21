// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using System;
using System.Globalization;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class Warnings
    {
        public const string Wrn997InvalidTarget = "WRN997_InvalidTarget";

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
    }
}