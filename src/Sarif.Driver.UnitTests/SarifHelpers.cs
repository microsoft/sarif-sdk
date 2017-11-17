﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    internal static class SarifHelpers
    {
        public static void ValidateRun(Run run, Action<Result> resultAction, Action<Notification> toolNotificationAction)
        {
            ValidateTool(run.Tool);

            Assert.NotNull(run.Results);
            foreach (Result result in run.Results)
            {
                resultAction(result);
            }

            if (run.ToolNotifications != null)
            {
                foreach (Notification result in run.ToolNotifications)
                {
                    toolNotificationAction(result);
                }
            }
        }

        public static void ValidateTool(Tool tool)
        {
            // TODO version, etc
        }
    }
}
