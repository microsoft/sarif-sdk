// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Microsoft.WorkItems.Logging
{
    public static class LoggerExtensions
    {
        public static void LogMetrics(this ILogger logger, EventId eventId, IDictionary<string, object> customDimensions)
        {
            LogMetrics(logger, eventId, string.Empty, customDimensions);
        }

        public static void LogMetrics(this ILogger logger, EventId eventId, string message, IDictionary<string, object> customDimensions)
        {
            logger.Log(LogLevel.Information, eventId, new MetricsLogValues(message, eventId, customDimensions), null, (state, error) => state.ToString());
        }
    }
}
