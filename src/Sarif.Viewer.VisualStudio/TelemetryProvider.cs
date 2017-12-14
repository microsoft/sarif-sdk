// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.ApplicationInsights;
using Microsoft.CodeAnalysis.Sarif.Converters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Sarif.Viewer
{
    internal class TelemetryProvider
    {
        private TelemetryClient appInsightsClient;

        public TelemetryProvider(string telemetryKey)
        {
            appInsightsClient = new TelemetryClient();
            appInsightsClient.InstrumentationKey = telemetryKey;
        }

        public void WriteMenuCommandEvent(string toolFormat)
        {
            if (string.IsNullOrWhiteSpace(toolFormat))
            {
                throw new ArgumentNullException(nameof(toolFormat));
            }

            WriteEvent(TelemetryEvent.LogFileOpenedByMenuCommand, "Format".KeyWithValue(toolFormat == ToolFormat.None ? "SARIF" : toolFormat));
        }

        public void WriteEvent(TelemetryEvent eventType)
        {
            appInsightsClient.TrackEvent(eventType.ToString());
            appInsightsClient.Flush();
        }

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

        public void WriteEvent(TelemetryEvent eventType, params KeyValuePair<string, string>[] pairs)
        {
            var dict = pairs.ToDictionary(p => p.Key, p => p.Value);

            WriteEvent(eventType, dict);
        }

        public void WriteEvent(TelemetryEvent eventType, Dictionary<string, string> properties = null)
        {
            appInsightsClient.TrackEvent(eventType.ToString(), properties);
            appInsightsClient.Flush();
        }

        public void WriteException(Exception ex)
        {
            try
            {
                // Create a new exception of the same type so potentially sensitive data can be omitted
                Type type = ex.GetType();
                Exception e = Activator.CreateInstance(type, ex.Message) as Exception;

                appInsightsClient.TrackException(e);
                appInsightsClient.Flush();
            }
            // Something went wrong creating the new exception, so we'll survive but fail to report the exception
            catch (ArgumentNullException) { }
            catch (ArgumentException) { }
            catch (MethodAccessException) { }
            catch (MemberAccessException) { }
            catch (TypeLoadException) { }
        }
    }
}
