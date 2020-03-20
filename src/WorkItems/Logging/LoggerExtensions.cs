using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.WorkItems.Logging
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
