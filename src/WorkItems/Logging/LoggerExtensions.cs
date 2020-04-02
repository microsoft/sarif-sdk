// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Octokit;

namespace Microsoft.WorkItems.Logging
{
    public static class LoggerExtensions
    {
        public static void LogMetrics(this ILogger logger, EventId eventId, IDictionary<string, object> customDimensions)
        {
            LogMetrics(
                logger, 
                eventId, 
                message: string.Empty, 
                customDimensions);
        }

        public static void LogMetrics(this ILogger logger, EventId eventId, string message, IDictionary<string, object> customDimensions)
        {
            logger.Log(
                LogLevel.Information, 
                eventId, 
                new MetricsLogValues(message, eventId, customDimensions),
                exception: null, 
                (state, error) => state.ToString());
        }

        public static void LogDebug(this ILogger logger, string message, IDictionary<string, object> customDimensions, Exception exception = null)
        {
            LogMessage(logger, LogLevel.Debug, message, customDimensions, exception);
        }

        public static void LogInformation(this ILogger logger, string message, IDictionary<string, object> customDimensions, Exception exception = null)
        {
            LogMessage(logger, LogLevel.Information, message, customDimensions, exception);
        }

        public static void LogWarning(this ILogger logger, string message, IDictionary<string, object> customDimensions, Exception exception = null)
        {
            LogMessage(logger, LogLevel.Warning, message, customDimensions, exception);
        }

        public static void LogError(this ILogger logger, string message, IDictionary<string, object> customDimensions, Exception exception = null)
        {
            LogMessage(logger, LogLevel.Error, message, customDimensions, exception);
        }

        public static void LogCritical(this ILogger logger, string message, IDictionary<string, object> customDimensions, Exception exception = null)
        {
            LogMessage(logger, LogLevel.Critical, message, customDimensions, exception);
        }

        private static void LogMessage(this ILogger logger, LogLevel logLevel, string message, IDictionary<string, object> customDimensions, Exception exception = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message));
            }

            logger.Log(
                logLevel,
                EventIds.None,
                new MetricsLogValues(message, EventIds.None, customDimensions),
                exception,
                (state, error) => state.ToString());
        }
    }
}
