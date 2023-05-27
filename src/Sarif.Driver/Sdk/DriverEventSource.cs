// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.Tracing;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>
    ///  EventSource is an ETW EventSource for Microsoft.CodeAnalysis.Sarif.PatternMatcher events, allowing performance tracing.
    ///  https://learn.microsoft.com/en-us/dotnet/core/diagnostics/eventsource-instrumentation
    /// </summary>
    [EventSource(Name = "Sarif-Default-Driver-Events-v0")]
    public sealed class DriverEventSource : EventSource
    {
        public static DriverEventSource Log = new DriverEventSource();

        public DriverEventSource() : base(throwOnEventWriteErrors: true)
        {

        }


        public const string None = "[None]";

        [Event((int)DriverEventId.FirstArtifactQueued, Message = "The first artifact was put in the scan queue: {0}")]
        public void FirstArtifactQueued(string filePath)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEventId.FirstArtifactQueued, filePath);
            }
        }

        [Event((int)DriverEventId.ArtifactNotScanned, Message = "Artifact was not scanned ({1}): {0}")]
        public void ArtifactNotScanned(string filePath, string reason, long sizeInBytes, string data2)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEventId.ArtifactNotScanned, filePath, reason, sizeInBytes, data2 ?? None);
            }
        }

        [Event((int)DriverEventId.ReadArtifactStart, Message = "Read artifact: {0}")]
        public void ReadArtifactStart(string filePath)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEventId.ReadArtifactStart, filePath);
            }
        }

        [Event((int)DriverEventId.ReadArtifactStop, Message = "Artifact retrieved: {0}")]
        public void ReadArtifactStop(string filePath, long sizeInBytes)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEventId.ReadArtifactStop, filePath, sizeInBytes);
            }
        }

        [Event((int)DriverEventId.ScanArtifactStart, Message = "Scan start: {0}")]
        public void ScanArtifactStart(string filePath, long sizeInBytes)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEventId.ScanArtifactStart, filePath, sizeInBytes);
            }
        }

        [Event((int)DriverEventId.ScanArtifactStop, Message = "Scan stop: {0}")]
        public void ScanArtifactStop(string filePath, long sizeInBytes)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEventId.ScanArtifactStop, filePath, sizeInBytes);
            }
        }

        [Event((int)DriverEventId.RuleNotCalled, Message = "'{1}.{2}' not called for artifact ({1} : {2}): {0}")]
        public void RuleNotCalled(string filePath, string ruleId, string ruleName, string data1, string data2)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEventId.RuleNotCalled, filePath, ruleId, ruleName, data1 ?? None, data2 ?? None);
            }
        }

        [Event((int)DriverEventId.RuleStart, Message = "'{1}.{2}' start: {0}", Keywords = Keywords.Rules, Level = EventLevel.Verbose)]
        public void RuleStart(string filePath, string ruleId, string ruleName)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEventId.RuleStart, filePath, ruleId, ruleName);
            }
        }

        [Event((int)DriverEventId.RuleStop, Message = "'{1}.{2}' stop: {0}", Keywords = Keywords.Rules)]
        public void RuleStop(string filePath, string ruleId, string ruleName)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEventId.RuleStop, filePath, ruleId, ruleName);
            }
        }

        [Event((int)DriverEventId.RuleFired, Message = "'{1}.{2}' fired with severity '{3}': {0}", Keywords = Keywords.Rules)]
        public void RuleFired(string filePath, string ruleId, string ruleName, FailureLevel level, string matchIdentifier = null)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEventId.RuleFired, filePath, ruleId, ruleName, level, matchIdentifier);
            }
        }

        [Event((int)DriverEventId.EnumerateArtifactsStart, Message = "Artifact enumeration started.")]
        public void EnumerateArtifactsStart()
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEventId.EnumerateArtifactsStart);
            }
        }

        [Event((int)DriverEventId.EnumerateArtifactsStop, Message = "Artifact enumeration stopped.")]
        public void EnumerateArtifactsStop()
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEventId.EnumerateArtifactsStop);
            }
        }

        [Event((int)DriverEventId.LogResultsStart, Message = "Logging artifact results started.")]
        public void LogResultsStart()
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEventId.LogResultsStart);
            }
        }

        [Event((int)DriverEventId.LogResultsStop, Message = "Logging artifact results stopped.")]
        public void LogResultsStop()
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEventId.LogResultsStop);

            }
        }

        [Event((int)DriverEventId.ArtifactReserved0, Message = "{2}: {1}")]
        public void ArtifactReserved0(string context, string filePath, string data1, string data2)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEventId.ArtifactReserved0, context, filePath, data1 ?? None, data2 ?? None);
            }
        }

        [Event((int)DriverEventId.ArtifactReserved1Start, Message = "{0} started: {1}")]
        public void ArtifactReserved1Start(string context, string filePath, string data1, string data2)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEventId.ArtifactReserved1Start, context, filePath, data1 ?? None, data2 ?? None);
            }
        }

        [Event((int)DriverEventId.ArtifactReserved1Stop, Message = "'{0}' stopped: {1}")]
        public void ArtifactReserved1Stop(string context, string filePath, string data1, string data2)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEventId.ArtifactReserved1Stop, context, filePath, data1 ?? None, data2 ?? None);
            }
        }

        [Event((int)DriverEventId.RuleReserved0, Message = "'{2}.{3}' '{0}' ({4} : {5}) started: {1}", Keywords = Keywords.Rules)]
        public void RuleReserved0(string context, string filePath, string ruleId, string ruleName, string data1, string data2)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEventId.RuleReserved0, context, filePath, ruleId, ruleName, data1 ?? None, data2 ?? None);
            }
        }

        [Event((int)DriverEventId.RuleReserved1Start, Message = "'{2}.{3}' '{0}' ({4} : {5}) started: {1}", Keywords = Keywords.Rules)]
        public void RuleReserved1Start(string context, string filePath, string ruleId, string ruleName, string data1, string data2)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEventId.RuleReserved1Start, context, filePath, ruleId, ruleName, data1 ?? None, data2 ?? None);
            }
        }

        [Event((int)DriverEventId.RuleReserved1Stop, Message = "'{2}.{3}' '{0}' ({4} : {5}) stopped: {1}", Keywords = Keywords.Rules)]
        public void RuleReserved1Stop(string context, string filePath, string ruleId, string ruleName, string data1, string data2)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEventId.RuleReserved1Stop, context, filePath, ruleId, ruleName, data1 ?? None, data2 ?? None);
            }
        }

        [Event((int)DriverEventId.SessionEnded, Message = "Session ended.")]
        public void SessionEnded()
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEventId.SessionEnded);
            }
        }

        public class Keywords // this is a bit vector
        {
            public const EventKeywords Rules = (EventKeywords)0x0001;
        }
    }
}
