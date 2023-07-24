// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Driver.Sdk;
using Microsoft.Diagnostics.Tracing;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class DumpEventsCommand
    {
        public int Run(DumpEventsOptions options)
        {
            string path = options.EventsFilePath;

            var guid = Guid.NewGuid();
            StreamWriter csvWriter = null;

            Console.WriteLine($"Parsing events from {path} (this may take a while)...");

            if (!string.IsNullOrWhiteSpace(options.CsvFilePath))
            {
                csvWriter = new StreamWriter(options.CsvFilePath);
                csvWriter.WriteLine("SessionGuid,Timestamp,ThreadId,ProcessorId,EventName,TimeStampRelativeMSec,DurationMsec,FilePath,RuleId,RuleName,Data1,Data2,Message");
            }

            using (var source = new ETWTraceEventSource(path))
            {
                double enumerateArtifactsStartMSec = 0;
                TimeSpan timeSpentEnumeratingArtifacts = default;
                TimeSpan timeSpentReadingArtifacts = default;
                TimeSpan timeSpentScanningArtifacts = default;
                TimeSpan timeSpentRunningRules = default;
                TimeSpan timeSpentLoggingResults = default;
                TimeSpan overallSessionTime = default;
                TimeSpan firstArtifactQueued = default;

                object data1, data2;

                double? durationMsec;
                string context, eventName, filePath, ruleId, ruleName;

                var timingData = new Dictionary<StartStopKey, double>();
                StartStopKey startStopKey = default;

                var ruleReservedTiming = new Dictionary<string, double>();
                var artifactReservedTiming = new Dictionary<string, double>();

                var skippedArtifacts = new Dictionary<string, Tuple<long, long, string>>();

                var rulesFired = new Dictionary<string, Dictionary<FailureLevel, int>>();

                int returnCode = 0;
                RuntimeConditions runtimeConditions = 0;

                source.Dynamic.All += delegate (TraceEvent traceEvent)
                {
                    eventName = traceEvent.EventName;
                    context = filePath = ruleId = ruleName = null;
                    data2 = data1 = durationMsec = null;

                    if (traceEvent.Opcode == TraceEventOpcode.Start)
                    {
                        string keyText = $"{traceEvent.PayloadByName(nameof(filePath))}:{traceEvent.PayloadByName(nameof(ruleId))}:{traceEvent.PayloadByName(nameof(ruleName))}:{traceEvent.PayloadByName(nameof(context))}:{traceEvent.ThreadID}";
                        startStopKey = new StartStopKey(traceEvent.ProviderGuid, traceEvent.Task, keyText);
                        timingData.Add(startStopKey, traceEvent.TimeStampRelativeMSec);
                    }

                    if (traceEvent.Opcode == TraceEventOpcode.Stop)
                    {
                        string keyText = $"{traceEvent.PayloadByName(nameof(filePath))}:{traceEvent.PayloadByName(nameof(ruleId))}:{traceEvent.PayloadByName(nameof(ruleName))}:{traceEvent.PayloadByName(nameof(context))}:{traceEvent.ThreadID}";
                        startStopKey = new StartStopKey(traceEvent.ProviderGuid, traceEvent.Task, keyText);
                    }

                    string formattedMessage = traceEvent.FormattedMessage.CsvEscape();

                    data1 = ((string)traceEvent.PayloadByName(nameof(data1))).CsvEscape();
                    data2 = ((string)traceEvent.PayloadByName(nameof(data2))).CsvEscape(); ;

                    switch (traceEvent.EventName)
                    {
                        case DriverEventNames.ArtifactNotScanned:
                        {
                            filePath = (string)traceEvent.PayloadByName(nameof(filePath));
                            string reason = (string)traceEvent.PayloadByName("reason");
                            data1 = traceEvent.PayloadByName("sizeInBytes");

                            skippedArtifacts.TryGetValue(reason, out Tuple<long, long, string> tuple);
                            tuple ??= new Tuple<long, long, string>(0, 0, null);

                            tuple = new Tuple<long, long, string>(tuple.Item1 + 1, tuple.Item2 + (long)data1, (string)data2);
                            skippedArtifacts[reason] = tuple;
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
                            eventName = $"{context}/Stop";

                            durationMsec = traceEvent.TimeStampRelativeMSec - timingData[startStopKey];
                            timingData.Remove(startStopKey);

                            artifactReservedTiming.TryGetValue(context, out double timingValue);
                            artifactReservedTiming[context] = timingValue + durationMsec.Value;
                            break;
                        }

                        case DriverEventNames.EnumerateArtifactsStart:
                        {
                            enumerateArtifactsStartMSec = traceEvent.TimeStampRelativeMSec;
                            break;
                        }

                        case DriverEventNames.EnumerateArtifactsStop:
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

                        case DriverEventNames.ReadArtifactStart:
                        {
                            filePath = (string)traceEvent.PayloadByName(nameof(filePath));
                            break;
                        }

                        case DriverEventNames.ReadArtifactStop:
                        {
                            filePath = (string)traceEvent.PayloadByName(nameof(filePath));
                            data1 = traceEvent.PayloadByName("sizeInBytes");

                            durationMsec = traceEvent.TimeStampRelativeMSec - timingData[startStopKey];
                            timeSpentReadingArtifacts = TimeSpan.FromMilliseconds(timeSpentReadingArtifacts.TotalMilliseconds + durationMsec.Value);
                            timingData.Remove(startStopKey);

                            skippedArtifacts.TryGetValue(DriverEventNames.Scanned, out Tuple<long, long, string> tuple);
                            tuple ??= new Tuple<long, long, string>(0, 0, null);

                            tuple = new Tuple<long, long, string>(tuple.Item1 + 1, tuple.Item2 + (long)data1, (string)data2);
                            skippedArtifacts[DriverEventNames.Scanned] = tuple;

                            break;
                        }

                        case "RuleFired":
                        {
                            FailureLevel level = (FailureLevel)(int)(uint)traceEvent.PayloadByName("level");
                            data1 = level;
                            data2 = traceEvent.PayloadByName("matchIdentifier");
                            ruleId = (string)traceEvent.PayloadByName(nameof(ruleId));
                            filePath = (string)traceEvent.PayloadByName(nameof(filePath));
                            ruleName = (string)traceEvent.PayloadByName(nameof(ruleName));

                            string ruleKey = $"{ruleId}.{ruleName}";
                            if (!rulesFired.TryGetValue(ruleKey, out Dictionary<FailureLevel, int> ruleFired))
                            {
                                ruleFired = rulesFired[ruleKey] = new Dictionary<FailureLevel, int>();
                            }

                            ruleFired.TryGetValue(level, out int count);
                            ruleFired[level] = ++count;

                            break;
                        }

                        case "RuleReserved0":
                        {
                            eventName = (string)traceEvent.PayloadByName("context");

                            ruleId = (string)traceEvent.PayloadByName(nameof(ruleId));
                            filePath = (string)traceEvent.PayloadByName(nameof(filePath));
                            ruleName = (string)traceEvent.PayloadByName(nameof(ruleName));
                            break;
                        }

                        case "RuleReserved1/Start":
                        {
                            eventName = $"{(string)traceEvent.PayloadByName("context")}/Start";

                            ruleId = (string)traceEvent.PayloadByName(nameof(ruleId));
                            filePath = (string)traceEvent.PayloadByName(nameof(filePath));
                            ruleName = (string)traceEvent.PayloadByName(nameof(ruleName));
                            break;
                        }

                        case "RuleReserved1/Stop":
                        {
                            context = $"{(string)traceEvent.PayloadByName("context")}";
                            eventName = $"{context}/Stop";

                            ruleId = (string)traceEvent.PayloadByName(nameof(ruleId));
                            filePath = (string)traceEvent.PayloadByName(nameof(filePath));
                            ruleName = (string)traceEvent.PayloadByName(nameof(ruleName));

                            durationMsec = traceEvent.TimeStampRelativeMSec - timingData[startStopKey];
                            timingData.Remove(startStopKey);

                            ruleReservedTiming.TryGetValue(context, out double timingValue);
                            ruleReservedTiming[context] = timingValue + durationMsec.Value;

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
                            timeSpentRunningRules = TimeSpan.FromMilliseconds(timeSpentRunningRules.TotalMilliseconds + durationMsec.Value);
                            timingData.Remove(startStopKey);
                            break;
                        }

                        case DriverEventNames.ScanArtifactStart:
                        {
                            filePath = (string)traceEvent.PayloadByName(nameof(filePath));
                            data1 = traceEvent.PayloadByName("sizeInBytes");
                            break;
                        }

                        case DriverEventNames.ScanArtifactStop:
                        {
                            filePath = (string)traceEvent.PayloadByName(nameof(filePath));
                            data1 = traceEvent.PayloadByName("sizeInBytes");

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
                            overallSessionTime = TimeSpan.FromMilliseconds(traceEvent.TimeStampRelativeMSec);
                            data1 = returnCode = (int)traceEvent.PayloadByName("returnCode");
                            data2 = runtimeConditions = (RuntimeConditions)(long)(ulong)traceEvent.PayloadByName("runtimeConditions");
                            break;
                        }

                        default:
                        {
                            throw new InvalidOperationException($"Unrecognized event: {traceEvent.EventName}");
                        }
                    }

                    if (csvWriter != null)
                    {
                        filePath = filePath.CsvEscape();
                        csvWriter.WriteLine(
                            $"{guid},{traceEvent.TimeStamp:MM/dd/yyyy hh:mm:ss.ffff}, {traceEvent.ThreadID}," +
                            $"{traceEvent.ProcessorNumber},{eventName},{traceEvent.TimeStampRelativeMSec}," +
                            $"{durationMsec},{filePath},{ruleId},{ruleName},{data1},{data2},{formattedMessage}");
                    }
                };

                source.Process();
                csvWriter?.Dispose();

                Console.WriteLine($@"Overall time elapsed: {overallSessionTime}");
                Console.WriteLine();

                DumpSkippedArtifacts(skippedArtifacts);

                Console.WriteLine($@"Time elapsed until    : First artifact queued for analysis : {firstArtifactQueued}");
                Console.WriteLine($@"Time elapsed until    : Artifact enumeration completed     : {timeSpentEnumeratingArtifacts}");
                Console.WriteLine($@"Time elapsed          : Session ended                      : {overallSessionTime}");

                Console.WriteLine();

                double ms =
                    timeSpentReadingArtifacts.TotalMilliseconds +
                    timeSpentScanningArtifacts.TotalMilliseconds +
                    timeSpentLoggingResults.TotalMilliseconds;

                Console.WriteLine($@"Aggregated time spent : Reading artifacts  : {string.Format("{0,7:P}", timeSpentReadingArtifacts.TotalMilliseconds / ms)} : {timeSpentReadingArtifacts}");
                Console.WriteLine($@"Aggregated time spent : Scanning artifacts : {string.Format("{0,7:P}", timeSpentScanningArtifacts.TotalMilliseconds / ms)} : {timeSpentScanningArtifacts}");
                Console.WriteLine($@"Aggregated time spent : Logging results    : {string.Format("{0,7:P}", timeSpentLoggingResults.TotalMilliseconds / ms)} : {timeSpentLoggingResults}");
                Console.WriteLine();


                DumpCustomTimingData(artifactReservedTiming);
                DumpCustomTimingData(ruleReservedTiming);
                DumpRulesFired(rulesFired);

                Console.WriteLine($@"Runtime conditions    : {runtimeConditions}");
                if (returnCode != 0)
                {
                    Console.WriteLine($@"ERROR                 : Analysis did not succeed (return code: {returnCode})");
                }

                if (csvWriter != null)
                {
                    Console.WriteLine();
                    Console.WriteLine($"CSV written to: {options.CsvFilePath}");
                }
            }

            return 0;
        }

        private void DumpRulesFired(Dictionary<string, Dictionary<FailureLevel, int>> rulesFired)
        {
            var sortedRules = new SortedList<string, Dictionary<FailureLevel, int>>();
            int maxRuleNameLength = 0;

            foreach (string ruleName in rulesFired.Keys)
            {
                maxRuleNameLength = Math.Max(maxRuleNameLength, ruleName.Length);
                sortedRules.Add(ruleName, rulesFired[ruleName]);
            }

            int lineLength = maxRuleNameLength + 34;
            Console.WriteLine(new string('-', lineLength));
            int padding = ((maxRuleNameLength - "Rule Name".Length) / 2);
            Console.Write($"{new string(' ', padding)}Rule Name{new string(' ', padding + 1)}");
            Console.WriteLine($"|    Error |  Warning |     Note |");
            Console.WriteLine(new string('-', lineLength));

            foreach (string sortedRuleName in sortedRules.Keys)
            {
                Dictionary<FailureLevel, int> levelCounts = sortedRules[sortedRuleName];
                padding = maxRuleNameLength - sortedRuleName.Length;
                Console.Write($"{sortedRuleName}{new string(' ', padding)}");

                foreach (FailureLevel level in new[] { FailureLevel.Error, FailureLevel.Warning, FailureLevel.Note })
                {
                    levelCounts.TryGetValue(level, out int count);
                    Console.Write(string.Format("{0, 10}", count) + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        private void DumpSkippedArtifacts(Dictionary<string, Tuple<long, long, string>> skippedArtifacts)
        {
            long totalFiles = 0;
            long totalSize = 0;
            int maxEventNameLength = 0;

            foreach (string reason in new[] { DriverEventNames.Scanned, DriverEventNames.EmptyFile, DriverEventNames.FileExceedsSizeLimits, DriverEventNames.FilePathDenied, "ContentsSniffNoMatch" })
            {
                maxEventNameLength = Math.Max(maxEventNameLength, reason.Length + 1);
                skippedArtifacts.TryGetValue(reason, out Tuple<long, long, string> tuple);
                totalFiles += tuple?.Item1 ?? 0;
                totalSize += tuple?.Item2 ?? 0;
            }

            skippedArtifacts.TryGetValue("ContentsSniffNoMatch", out Tuple<long, long, string> contentsSniffNoMatchTuple);
            skippedArtifacts.TryGetValue(DriverEventNames.FileExceedsSizeLimits, out Tuple<long, long, string> fileExceedsSizeLimitsTuple);
            skippedArtifacts.TryGetValue(DriverEventNames.FilePathDenied, out Tuple<long, long, string> filePathDeniedRegexTuple);

            // Emit supplemental data for 
            if (fileExceedsSizeLimitsTuple != default)
            {
                Console.WriteLine($@"{DriverEventNames.FileExceedsSizeLimits}  : Threshold size in KB was {fileExceedsSizeLimitsTuple.Item3}");
            }

            if (filePathDeniedRegexTuple != default)
            {
                Console.WriteLine($@"{DriverEventNames.FilePathDenied}         : Regex was {filePathDeniedRegexTuple.Item3}");
            }

            if (contentsSniffNoMatchTuple != default)
            {
                Console.WriteLine($@"ContentsSniffNoMatch                       : Regex was {contentsSniffNoMatchTuple.Item3}");
            }

            Console.WriteLine();

            foreach (string reason in new[] { DriverEventNames.Scanned, DriverEventNames.EmptyFile, DriverEventNames.FileExceedsSizeLimits, DriverEventNames.FilePathDenied, "ContentsSniffNoMatch" })
            {
                skippedArtifacts.TryGetValue(reason, out Tuple<long, long, string> tuple);
                string formatString = $"{{0,-{maxEventNameLength}:N0}}";
                string formattedEventName = string.Format(formatString, reason);

                Console.WriteLine(
                    $@"{formattedEventName}: {string.Format("{0,12:N0}", tuple?.Item1 ?? 0)} file(s) - {string.Format("{0,7:P}", ((double)(tuple?.Item1 ?? 0)) / (double)totalFiles)} : " +
                    @$"{string.Format("{0,14:N0}", (double)(tuple?.Item2 ?? 0) / (double)1000)} KB  - {string.Format("{0,7:P}", ((double)(tuple?.Item2 ?? 0)) / (double)totalSize)}");
            }

            if (skippedArtifacts.Keys.Count > 0)
            {
                Console.WriteLine();
            }
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
                Console.WriteLine($@"Aggregated time spent : {formattedEventName} : {string.Format("{0,7:P}", eventTimeInMs / totalMs)} : {TimeSpan.FromMilliseconds(eventTimeInMs)}");
            }

            if (timingData.Keys.Count > 0)
            {
                Console.WriteLine();
            }
        }

        private static Guid GenerateGuidFromText(string name)
        {
            // The algorithm below is following the guidance of http://www.ietf.org/rfc/rfc4122.txt
            // Create a blob containing a 16 byte number representing the namespace
            // followed by the unicode bytes in the name.
            byte[] bytes = new byte[name.Length * 2 + 16];
            uint namespace1 = 0x482C2DB2;
            uint namespace2 = 0xC39047c8;
            uint namespace3 = 0x87F81A15;
            uint namespace4 = 0xBFC130FB;
            // Write the bytes most-significant byte first.
            for (int i = 3; 0 <= i; --i)
            {
                bytes[i] = (byte)namespace1;
                namespace1 >>= 8;
                bytes[i + 4] = (byte)namespace2;
                namespace2 >>= 8;
                bytes[i + 8] = (byte)namespace3;
                namespace3 >>= 8;
                bytes[i + 12] = (byte)namespace4;
                namespace4 >>= 8;
            }
            // Write out  the name, most significant byte first
            for (int i = 0; i < name.Length; i++)
            {
                bytes[2 * i + 16 + 1] = (byte)name[i];
                bytes[2 * i + 16] = (byte)(name[i] >> 8);
            }

            // Compute the Sha1 hash
            var sha1 = System.Security.Cryptography.SHA1.Create(); // lgtm [cs/weak-crypto]
            byte[] hash = sha1.ComputeHash(bytes);

            // Create a GUID out of the first 16 bytes of the hash (SHA-1 create a 20 byte hash)
            int a = (((((hash[3] << 8) + hash[2]) << 8) + hash[1]) << 8) + hash[0];
            short b = (short)((hash[5] << 8) + hash[4]);
            short c = (short)((hash[7] << 8) + hash[6]);

            c = (short)((c & 0x0FFF) | 0x5000);   // Set high 4 bits of octet 7 to 5, as per RFC 4122
            var guid = new Guid(a, b, c, hash[8], hash[9], hash[10], hash[11], hash[12], hash[13], hash[14], hash[15]);
            return guid;
        }

        internal class StartStopKey : IEquatable<StartStopKey>
        {
            public StartStopKey(Guid provider, TraceEventTask task, string activityID) { Provider = provider; this.task = task; ActivityId = activityID; }
            public Guid Provider;
            public string ActivityId;
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
