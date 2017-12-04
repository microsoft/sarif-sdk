using Microsoft.ApplicationInsights;
using Microsoft.CodeAnalysis.Sarif.Converters;
using System;
using System.Collections.Generic;

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

            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("Data", data);
        
            WriteEvent(eventType, dict);
        }

        public void WriteEvent(TelemetryEvent eventType, params KeyValuePair<string, string>[] pairs)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            
            foreach (KeyValuePair<string, string> pair in pairs)
            {
                dict.Add(pair.Key, pair.Value);
            }

            WriteEvent(eventType, dict);
        }

        public void WriteEvent(TelemetryEvent eventType, Dictionary<string, string> properties = null)
        {
            appInsightsClient.TrackEvent(eventType.ToString(), properties);
            appInsightsClient.Flush();
        }

        public void WriteException(Exception ex)
        {
            appInsightsClient.TrackException(ex);
            appInsightsClient.Flush();
        }
    }
}
