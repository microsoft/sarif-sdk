﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.ApplicationInsights;
using Microsoft.CodeAnalysis.Sarif.Converters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Sarif.Viewer
{
    /// <summary>
    /// Wrapper class for Application Insights client.
    /// </summary>
    internal class TelemetryProvider
    {
        private TelemetryClient appInsightsClient;

        /// <summary>
        /// Initializes a new instance of the TelemetryProvider. Sends telemetry to the AI instance with the corresponding instrumentation key.
        /// </summary>
        /// <param name="telemetryKey">The instrumentation key of the target AI instance.</param>
        public TelemetryProvider(string telemetryKey)
        {
            appInsightsClient = new TelemetryClient();
            appInsightsClient.InstrumentationKey = telemetryKey;
        }

        /// <summary>
        /// Sends event telemetry when the user chooses a menu item from the Tools > Open Static Analysis Log File menu.
        /// </summary>
        /// <param name="toolFormat">The tool format of the menu item.</param>
        public void WriteMenuCommandEvent(string toolFormat)
        {
            if (string.IsNullOrWhiteSpace(toolFormat))
            {
                throw new ArgumentNullException(nameof(toolFormat));
            }

            WriteEvent(TelemetryEvent.LogFileOpenedByMenuCommand, "Format".KeyWithValue(toolFormat == ToolFormat.None ? "SARIF" : toolFormat));
        }

        /// <summary>
        /// Sends event telemetry for the specified event type.
        /// </summary>
        /// <param name="eventType">The type of the event.</param>
        public void WriteEvent(TelemetryEvent eventType)
        {
            appInsightsClient.TrackEvent(eventType.ToString());
            appInsightsClient.Flush();
        }

        /// <summary>
        /// Sends event telemetry for the specified event type with the specified data value.
        /// </summary>
        /// <param name="eventType">The type of the event.</param>
        /// <param name="data">The value of the Data property associted with the event.</param>
        public void WriteEvent(TelemetryEvent eventType, string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                throw new ArgumentNullException(nameof(data));
            }

            var dict = new Dictionary<string, string>();
            dict.Add("Data", data);
        
            WriteEvent(eventType, dict);
        }

        /// <summary>
        /// Sends event telemetry for the specified event type with the associated named data properties.
        /// </summary>
        /// <param name="eventType">The type of the event.</param>
        /// <param name="pairs">Named string value data properties associted with this event.</param>
        public void WriteEvent(TelemetryEvent eventType, params KeyValuePair<string, string>[] pairs)
        {
            var dict = pairs.ToDictionary(p => p.Key, p => p.Value);

            WriteEvent(eventType, dict);
        }

        /// <summary>
        /// Sends event telemetry for the specified event type with the specified named data properties.
        /// </summary>
        /// <param name="eventType">The type of the event.</param>
        /// <param name="properties">Named string value data properties associted with this event.</param>
        public void WriteEvent(TelemetryEvent eventType, Dictionary<string, string> properties = null)
        {
            appInsightsClient.TrackEvent(eventType.ToString(), properties);
            appInsightsClient.Flush();
        }
    }
}
