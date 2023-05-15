// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Analysis;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class DumpEventsCommand
    {
        public int Run(DumpEventsOptions options)
        {
            string path = options.EventsFilePath;

            Guid guid = Guid.NewGuid();
            StreamWriter csvWriter = null;


            if (!string.IsNullOrWhiteSpace(options.CsvFilePath))
            {
                csvWriter = new StreamWriter(options.CsvFilePath);
                csvWriter.WriteLine("SessionGuid,Timestamp,ThreadId,ProcessorId,EventName,TimeStampRelativeMSec,DurationMsec,FilePath,SizeInBytes,Level,RuleId,RuleName,Message");
            }

            using (var source = new ETWTraceEventSource(path))
            {
                double enumerateArtifactsStartMSec = 0;
                TimeSpan timeSpentEnumeratingArtifacts = default;
                TimeSpan timeSpentReadingArtifacts = default;
                TimeSpan timeSpentScanningArtifacts = default;
                TimeSpan timeSpentLoggingResults = default;
                TimeSpan firstArtifactQueued = default;

                ulong? sizeInBytes;
                double? durationMsec;
                FailureLevel? level;
                string context, eventName, filePath, ruleId, ruleName;

                var timingData = new Dictionary<StartStopKey, double>();
                StartStopKey startStopKey = default;

                var artifactReservedTiming = new Dictionary<string, double>();

                source.Dynamic.All += delegate (TraceEvent traceEvent)
                {
                    eventName = traceEvent.EventName;
                    context = filePath = ruleId = ruleName = null;
                    sizeInBytes = null;
                    durationMsec = null;
                    level = null;

                    Guid correlationGuid = default;

                    if (traceEvent.Opcode == TraceEventOpcode.Start)
                    {
                        correlationGuid = new Guid(traceEvent.ThreadID, 1, 5, 45, 23, 23, 3, 5, 5, 4, 5);
                        startStopKey = new StartStopKey(traceEvent.ProviderGuid, traceEvent.Task, correlationGuid);
                        timingData.Add(startStopKey, traceEvent.TimeStampRelativeMSec);
                    }

                    if (traceEvent.Opcode == TraceEventOpcode.Stop)
                    {
                        correlationGuid = new Guid(traceEvent.ThreadID, 1, 5, 45, 23, 23, 3, 5, 5, 4, 5);
                        startStopKey = new StartStopKey(traceEvent.ProviderGuid, traceEvent.Task, correlationGuid);
                    }

                    string formattedMessage = CsvEscape(traceEvent.FormattedMessage);

                    switch (traceEvent.EventName)
                    {
                        case "ArtifactSizeInBytes":
                        {
                            filePath = (string)traceEvent.PayloadByName(nameof(filePath));
                            sizeInBytes = (ulong)traceEvent.PayloadByName(nameof(sizeInBytes));
                            break;
                        }

                        case "ArtifactReserved0":
                        {
                            filePath = (string)traceEvent.PayloadByName(nameof(filePath));
                            eventName = (string)traceEvent.PayloadByName("context");
                            break;
                        }

                        case "ArtifactReserved1/Start":
                        {
                            context = (string)traceEvent.PayloadByName("context");
                            filePath = (string)traceEvent.PayloadByName(nameof(filePath));
                            eventName = $"{context}/Start";
                            break;
                        }

                        case "ArtifactReserved1/Stop":
                        {
                            context = (string)traceEvent.PayloadByName("context");
                            filePath = (string)traceEvent.PayloadByName(nameof(filePath));
                            eventName = $"{context}/Start";

                            durationMsec = traceEvent.TimeStampRelativeMSec - timingData[startStopKey];
                            timingData.Remove(startStopKey);

                            artifactReservedTiming.TryGetValue(context, out double timingValue);
                            artifactReservedTiming[context] = timingValue + durationMsec.Value;
                            break;
                        }

                        case "EnumerateArtifacts/Start":
                        {
                            enumerateArtifactsStartMSec = traceEvent.TimeStampRelativeMSec;
                            break;
                        }

                        case "EnumerateArtifacts/Stop":
                        {
                            timeSpentEnumeratingArtifacts = TimeSpan.FromMilliseconds(
                                traceEvent.TimeStampRelativeMSec - enumerateArtifactsStartMSec);

                            durationMsec = traceEvent.TimeStampRelativeMSec - timingData[startStopKey];
                            timingData.Remove(startStopKey);

                            break;
                        }

                        case "FirstArtifactQueued":
                        {
                            filePath = (string)traceEvent.PayloadByName("filePath");
                            firstArtifactQueued = TimeSpan.FromMilliseconds(traceEvent.TimeStampRelativeMSec);
                            break;
                        }

                        case "LogResults/Start":
                        {
                            break;
                        }

                        case "LogResults/Stop":
                        {
                            durationMsec = traceEvent.TimeStampRelativeMSec - timingData[startStopKey];
                            timeSpentLoggingResults = TimeSpan.FromMilliseconds(timeSpentLoggingResults.TotalMilliseconds + durationMsec.Value);
                            timingData.Remove(startStopKey);
                            break;
                        }

                        case "ReadArtifact/Start":
                        {
                            filePath = (string)traceEvent.PayloadByName(nameof(filePath));
                            break;
                        }

                        case "ReadArtifact/Stop":
                        {
                            filePath = (string)traceEvent.PayloadByName(nameof(filePath));

                            durationMsec = traceEvent.TimeStampRelativeMSec - timingData[startStopKey];
                            timeSpentReadingArtifacts = TimeSpan.FromMilliseconds(timeSpentReadingArtifacts.TotalMilliseconds + durationMsec.Value);
                            timingData.Remove(startStopKey);
                            break;
                        }

                        case "RuleFired":
                        {
                            level = (FailureLevel)(uint)traceEvent.PayloadByName(nameof(level));
                            ruleId = (string)traceEvent.PayloadByName(nameof(ruleId));
                            filePath = (string)traceEvent.PayloadByName(nameof(filePath));
                            ruleName = (string)traceEvent.PayloadByName(nameof(ruleName));
                            break;
                        }

                        case "RuleReserved0":
                        {
                            ruleId = (string)traceEvent.PayloadByName(nameof(ruleId));
                            filePath = (string)traceEvent.PayloadByName(nameof(filePath));
                            ruleName = (string)traceEvent.PayloadByName(nameof(ruleName));
                            eventName = (string)traceEvent.PayloadByName("context");
                            break;
                        }

                        case "RuleReserved1/Start":
                        {
                            ruleId = (string)traceEvent.PayloadByName(nameof(ruleId));
                            filePath = (string)traceEvent.PayloadByName(nameof(filePath));
                            ruleName = (string)traceEvent.PayloadByName(nameof(ruleName));
                            eventName = $"{(string)traceEvent.PayloadByName("context")}/Start";
                            break;
                        }

                        case "RuleReserved1/Stop":
                        {
                            ruleId = (string)traceEvent.PayloadByName(nameof(ruleId));
                            filePath = (string)traceEvent.PayloadByName(nameof(filePath));
                            ruleName = (string)traceEvent.PayloadByName(nameof(ruleName));
                            eventName = $"{(string)traceEvent.PayloadByName("context")}/Stop";

                            durationMsec = traceEvent.TimeStampRelativeMSec - timingData[startStopKey];
                            timingData.Remove(startStopKey);
                            break;
                        }

                        case "Rule/Start":
                        {
                            ruleId = (string)traceEvent.PayloadByName(nameof(ruleId));
                            filePath = (string)traceEvent.PayloadByName(nameof(filePath));
                            ruleName = (string)traceEvent.PayloadByName(nameof(ruleName));
                            break;
                        }

                        case "Rule/Stop":
                        {
                            ruleId = (string)traceEvent.PayloadByName(nameof(ruleId));
                            filePath = (string)traceEvent.PayloadByName(nameof(filePath));
                            ruleName = (string)traceEvent.PayloadByName(nameof(ruleName));

                            durationMsec = traceEvent.TimeStampRelativeMSec - timingData[startStopKey];
                            timingData.Remove(startStopKey);
                            break;
                        }

                        case "ScanArtifact/Start":
                        {
                            filePath = (string)traceEvent.PayloadByName(nameof(filePath));
                            break;
                        }

                        case "ScanArtifact/Stop":
                        {
                            filePath = (string)traceEvent.PayloadByName(nameof(filePath));

                            durationMsec = traceEvent.TimeStampRelativeMSec - timingData[startStopKey];
                            timeSpentScanningArtifacts = TimeSpan.FromMilliseconds(timeSpentScanningArtifacts.TotalMilliseconds + durationMsec.Value);
                            timingData.Remove(startStopKey);
                            break;
                        }

                        case "ManifestData":
                        {
                            return;

                        }
                        case "EventTrace/PartitionInfoExtensionV2":
                        {
                            eventName = "SessionStarted";
                            formattedMessage = "Session started.";
                            break;
                        }

                        case "EventSourceMessage":
                        {
                            eventName = "Error";
                            break;
                        }

                        case "SessionEnded":
                        {
                            break;
                        }

                        default:
                        {
                            throw new InvalidOperationException($"Unrecognized event: {traceEvent.EventName}");
                        }
                    }

                    if (csvWriter != null)
                    {
                        filePath = CsvEscape(filePath);
                        csvWriter.WriteLine(
                            $"{guid},{traceEvent.TimeStamp:MM/dd/yyyy hh:mm:ss.ffff}, {traceEvent.ThreadID},{traceEvent.ProcessorNumber}," +
                            $"{eventName},{traceEvent.TimeStampRelativeMSec},{durationMsec}," +
                            $"{filePath},{sizeInBytes},{level},{ruleId},{ruleName},{formattedMessage}");
                    }
                };

                source.Process();
                csvWriter?.Dispose();

                Console.WriteLine($@"Time elapsed until    : First artifact queued for analysis : {firstArtifactQueued}");
                Console.WriteLine($@"Time elapsed until    : Artifact enumeration completed     : {timeSpentEnumeratingArtifacts}");
                Console.WriteLine();

                double ms =
                    timeSpentReadingArtifacts.TotalMilliseconds +
                    timeSpentScanningArtifacts.TotalMilliseconds +
                    timeSpentLoggingResults.TotalMilliseconds;

                Console.WriteLine($@"Aggregated time spent : Reading artifacts  : {string.Format("{0,6:P}", timeSpentReadingArtifacts.TotalMilliseconds / ms)} : {timeSpentReadingArtifacts}");
                Console.WriteLine($@"Aggregated time spent : Scanning artifacts : {string.Format("{0,6:P}", timeSpentScanningArtifacts.TotalMilliseconds / ms)} : {timeSpentScanningArtifacts}");
                Console.WriteLine($@"Aggregated time spent : Logging results    : {string.Format("{0,6:P}", timeSpentLoggingResults.TotalMilliseconds / ms)} : {timeSpentLoggingResults}");
                Console.WriteLine();

                DumpCustomTimingData(artifactReservedTiming);
            }
            return 0;
        }

        private void DumpCustomTimingData(Dictionary<string, double> timingData)
        {
            double totalMs = 0;
            int maxEventNameLength = 0;

            foreach (string eventName in timingData.Keys)
            {
                maxEventNameLength = Math.Max(maxEventNameLength, eventName.Length);
                totalMs += timingData[eventName];
            }

            foreach (string eventName in timingData.Keys)
            {
                double eventTimeInMs = timingData[eventName];
                string formatString = $"{{0,-{maxEventNameLength}}}";
                string formattedEventName = string.Format(formatString, eventName);
                Console.WriteLine($@"Aggregated time spent : {formattedEventName} : {string.Format("{0,6:P}", eventTimeInMs / totalMs)} : {eventTimeInMs}");
            }
        }

        private readonly static StringBuilder s_converted = new StringBuilder();
        public static string CsvEscape(string value)
        {
            if (string.IsNullOrEmpty(value)) { return string.Empty; }

            s_converted.Clear();
            s_converted.Append('"');

            int copiedTo = 0;
            while (true)
            {
                int nextQuote = value.IndexOf('"', copiedTo);
                if (nextQuote == -1) { break; }

                s_converted.Append(value, copiedTo, nextQuote - copiedTo + 1);
                s_converted.Append('"');
                copiedTo = nextQuote + 1;
            }

            if (copiedTo < value.Length) { s_converted.Append(value, copiedTo, value.Length - copiedTo); }
            s_converted.Append('"');

            return s_converted.ToString();
        }

        internal class StartStopKey : IEquatable<StartStopKey>
        {
            public StartStopKey(Guid provider, TraceEventTask task, Guid activityID) { Provider = provider; this.task = task; ActivityId = activityID; }
            public Guid Provider;
            public Guid ActivityId;
            public TraceEventTask task;

            public override int GetHashCode()
            {
                return Provider.GetHashCode() + ActivityId.GetHashCode() + (int)task;
            }

            public bool Equals(StartStopKey other)
            {
                return other.Provider == Provider && other.ActivityId == ActivityId && other.task == task;
            }

            public override bool Equals(object obj) { throw new NotImplementedException(); }

            public override string ToString()
            {
                return "<Key Provider=\"" + Provider + "\" ActivityId=\"" + ActivityId + "\" Task=\"" + ((int)task) + ">";
            }
        }
    }
}
