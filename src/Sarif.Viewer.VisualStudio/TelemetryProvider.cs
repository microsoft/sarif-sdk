// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.CodeAnalysis.Sarif.Converters;

namespace Microsoft.Sarif.Viewer
{
    /// <summary>
    /// Wrapper class for Application Insights client.
    /// </summary>
    internal static class TelemetryProvider
    {
        private static TelemetryClient s_AppInsightsClient;
        private static readonly object s_LockObject = new object();
        private static bool s_IsInitialized = false;

        /// <summary>
        /// Initializes the static instance of the TelemetryProvider.
        /// </summary>
        /// <param name="telemetryKey">The instrumentation key of the target AI instance.</param>
        public static void Initialize(TelemetryConfiguration configuration)
        {
            lock (s_LockObject)
            {
                if (!s_IsInitialized)
                {
                    s_AppInsightsClient = new TelemetryClient(configuration);
                    s_IsInitialized = true;
                }
            }
        }

        /// <summary>
        /// Resets to the uninitialized state.
        /// </summary>
        public static void Reset()
        {
            lock (s_LockObject)
            {
                s_AppInsightsClient = null;
                s_IsInitialized = false;
            }
        }

        /// <summary>
        /// Sends event telemetry when the user chooses a menu item from the Tools > Open Static Analysis Log File menu.
        /// </summary>
        /// <param name="toolFormat">The tool format of the menu item.</param>
        public static void WriteMenuCommandEvent(string toolFormat)
        {
            if (string.IsNullOrWhiteSpace(toolFormat))
            {
                throw new ArgumentNullException(nameof(toolFormat));
            }

            WriteEvent(TelemetryEvent.LogFileOpenedByMenuCommand,
                       CreateKeyValuePair("Format", toolFormat == ToolFormat.None ? "SARIF" : toolFormat));
        }

        /// <summary>
        /// Sends event telemetry for the specified event type.
        /// </summary>
        /// <param name="eventType">The type of the event.</param>
        public static void WriteEvent(TelemetryEvent eventType)
        {
            if (s_IsInitialized)
            {
                s_AppInsightsClient.TrackEvent(eventType.ToString());
                s_AppInsightsClient.Flush();
            }
        }

        /// <summary>
        /// Sends event telemetry for the specified event type with the specified data value.
        /// </summary>
        /// <param name="eventType">The type of the event.</param>
        /// <param name="data">The value of the Data property associted with the event.</param>
        public static void WriteEvent(TelemetryEvent eventType, string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                throw new ArgumentNullException(nameof(data));
            }

            var dictionary = new Dictionary<string, string>();
            dictionary.Add("Data", data);

            WriteEvent(eventType, dictionary);
        }

        /// <summary>
        /// Sends event telemetry for the specified event type with the associated named data properties.
        /// </summary>
        /// <param name="eventType">The type of the event.</param>
        /// <param name="pairs">Named string value data properties associted with this event.</param>
        public static void WriteEvent(TelemetryEvent eventType, params KeyValuePair<string, string>[] pairs)
        {
            var dictionary = pairs.ToDictionary(p => p.Key, p => p.Value);

            WriteEvent(eventType, dictionary);
        }

        /// <summary>
        /// Sends event telemetry for the specified event type with the specified named data properties.
        /// </summary>
        /// <param name="eventType">The type of the event.</param>
        /// <param name="properties">Named string value data properties associted with this event.</param>
        public static void WriteEvent(TelemetryEvent eventType, Dictionary<string, string> properties = null)
        {
            if (s_IsInitialized)
            {
                s_AppInsightsClient.TrackEvent(eventType.ToString(), properties);
                s_AppInsightsClient.Flush();
            }
        }

        /// <summary>
        /// Returns a KeyValuePair with the specified key and value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static KeyValuePair<string, string> CreateKeyValuePair(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            return new KeyValuePair<string, string>(key, value);
        }
    }
}
