// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using System;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class Warnings
    {
        public const string WRN997_InvalidTarget = "WRN997_InvalidTarget";

        public static void LogExceptionInvalidTarget(IAnalysisContext context)
        {
            // '{0}' was not analyzed as it does not appear to be a valid file type for analysis.

            string message = string.Format(
                SdkResources.WRN997_InvalidTarget,
                Path.GetFileName(context.TargetUri.LocalPath));

            context.Logger.LogConfigurationNotification(
                new Notification
                {
                    PhysicalLocation = new PhysicalLocation { Uri = context.TargetUri },
                    Id = WRN997_InvalidTarget,
                    Message = message,
                    Level = NotificationLevel.Note,
                });

            context.RuntimeErrors |= RuntimeConditions.TargetNotValidToAnalyze;
        }
    }
}