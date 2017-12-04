using Microsoft.ApplicationInsights;
using Microsoft.CodeAnalysis.Sarif.Converters;
using System;
using System.Collections.Generic;

namespace Microsoft.Sarif.Viewer
{
    internal static class Telemetry
    {
        internal static TelemetryClient AppInsightsClient { get; private set; }

        static Telemetry()
        {
#if DEBUG
            string telemetryKey = SarifViewerPackage.AppConfig.AppSettings.Settings["TelemetryInstrumentationKey_Debug"].Value;
#else
            string telemetryKey = SarifViewerPackage.AppConfig.AppSettings.Settings["TelemetryInstrumentationKey_Release"].Value;
#endif

            AppInsightsClient = new TelemetryClient();
            AppInsightsClient.InstrumentationKey = telemetryKey;
        }

        public static void WriteMenuCommandEvent(string toolFormat)
        {
            if (string.IsNullOrWhiteSpace(toolFormat))
            {
                throw new ArgumentNullException(nameof(toolFormat));
            }

            WriteEvent(TelemetryEvent.LogFileOpenedByMenuCommand, "Format".KeyWithValue(toolFormat == ToolFormat.None ? "SARIF" : toolFormat));
        }

        public static void WriteEvent(TelemetryEvent eventType)
        {
            AppInsightsClient.TrackEvent(eventType.ToString());
            AppInsightsClient.Flush();
        }

        public static void WriteEvent(TelemetryEvent eventType, string data)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("Data", data);
        
            WriteEvent(eventType, dict);
        }

        public static void WriteEvent(TelemetryEvent eventType, params KeyValuePair<string, string>[] pairs)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            
            foreach (KeyValuePair<string, string> pair in pairs)
            {
                dict.Add(pair.Key, pair.Value);
            }

            WriteEvent(eventType, dict);
        }

        public static void WriteEvent(TelemetryEvent eventType, Dictionary<string, string> properties = null)
        {
            AppInsightsClient.TrackEvent(eventType.ToString(), properties);
            AppInsightsClient.Flush();
        }

        public static void WriteException(Exception ex)
        {
            AppInsightsClient.TrackException(ex);
            AppInsightsClient.Flush();
        }
    }
}
