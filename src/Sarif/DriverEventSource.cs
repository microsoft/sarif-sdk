// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.Tracing;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    ///  EventSource is an ETW EventSource for Microsoft.CodeAnalysis.Sarif.PatternMatcher events, allowing performance tracing.
    ///  https://learn.microsoft.com/en-us/dotnet/core/diagnostics/eventsource-instrumentation
    /// </summary>
    [EventSource(Name = "Sarif-Default-Driver-Events-v0")]
    public sealed class DriverEventSource : EventSource
    {
        public static DriverEventSource Log = new DriverEventSource();

        [Event((int)DriverEvent.FirstArtifactQueued, Message = "The first artifact was put in the scan queue: {0}")]
        public void FirstArtifactQueued(string filePath)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEvent.FirstArtifactQueued, filePath);
            }
        }


        [Event((int)DriverEvent.ReadArtifactStart, Message = "Get contents: {0}")]
        public void ReadArtifactStart(string filePath)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEvent.ReadArtifactStart, filePath);
            }
        }

        [Event((int)DriverEvent.ReadArtifactStop, Message = "Contents retrieved: {0}")]
        public void ReadArtifactStop(string filePath)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEvent.ReadArtifactStop, filePath);
            }
        }

        [Event((int)DriverEvent.ScanArtifactStart, Message = "Scan start: {0}")]
        public void ScanArtifactStart(string filePath)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEvent.ScanArtifactStart, filePath);
            }
        }

        [Event((int)DriverEvent.ScanArtifactStop, Message = "Scan stop: {0}")]
        public void ScanArtifactStop(string filePath)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEvent.ScanArtifactStop, filePath);
            }
        }

        [Event((int)DriverEvent.RuleStart, Message = "'{1}.{2}' start: {0}", Keywords = Keywords.Rules, Level = EventLevel.Verbose)]
        public void RuleStart(string filePath, string ruleId, string ruleName)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEvent.RuleStart, filePath, ruleId, ruleName);
            }
        }

        [Event((int)DriverEvent.RuleStop, Message = "'{1}.{2}' stop: {0}", Keywords = Keywords.Rules)]
        public void RuleStop(string filePath, string ruleId, string ruleName)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEvent.RuleStop, filePath, ruleId, ruleName);
            }
        }

        [Event((int)DriverEvent.RuleFired, Message = "'{1}.{2}' fired with severity '{3}': {0}", Keywords = Keywords.Rules)]
        public void RuleFired(FailureLevel level, string filePath, string ruleId, string ruleName)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEvent.RuleFired, level, filePath, ruleId, ruleName);
            }
        }

        [Event((int)DriverEvent.ArtifactSizeInBytes, Message = "{0} : size {1} bytes.")]
        public void ArtifactSizeInBytes(ulong sizeInBytes, string filePath)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEvent.ArtifactSizeInBytes, sizeInBytes, filePath);
            }
        }

        [Event((int)DriverEvent.EnumerateArtifactsStart, Message = "Artifact enumeration started.")]
        public void EnumerateArtifactsStart()
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEvent.EnumerateArtifactsStart);
            }
        }

        [Event((int)DriverEvent.EnumerateArtifactsStop, Message = "Artifact enumeration stopped.")]
        public void EnumerateArtifactsStop()
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEvent.EnumerateArtifactsStop);
            }
        }

        [Event((int)DriverEvent.LogResultsStart, Message = "Artifact enumeration started.")]
        public void LogResultsStart()
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEvent.LogResultsStart);
            }
        }

        [Event((int)DriverEvent.LogResultsStop, Message = "Artifact enumeration stopped.")]
        public void LogResultsStop()
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEvent.LogResultsStop);

            }
        }

        [Event((int)DriverEvent.ArtifactReserved0, Message = "'{0}': {1}")]
        public void ArtifactReserved0(string context, string filePath)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEvent.ArtifactReserved0, context, filePath);
            }
        }

        [Event((int)DriverEvent.ArtifactReserved1Start, Message = "'{0}' start: {1}")]
        public void ArtifactReserved1Start(string context, string filePath)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEvent.ArtifactReserved1Start, context, filePath);
            }
        }

        [Event((int)DriverEvent.ArtifactReserved1Stop, Message = "'{0}' stop: {1}")]
        public void ArtifactReserved1Stop(string context, string filePath)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEvent.ArtifactReserved1Stop, context, filePath);
            }
        }

        [Event((int)DriverEvent.RuleReserved0, Message = "'{0}': {1}", Keywords = Keywords.Rules)]
        public void RuleReserved0(string context, string filePath)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEvent.RuleReserved0, context, filePath);
            }
        }

        [Event((int)DriverEvent.RuleReserved1Start, Message = "'{2}.{3}' '{0}' start: {1}", Keywords = Keywords.Rules)]
        public void RuleReserved1Start(string context, string filePath, string ruleId, string ruleName)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEvent.RuleReserved1Start, context, filePath, ruleId, ruleName);
            }
        }

        [Event((int)DriverEvent.RuleReserved1Stop, Message = "'{2}.{3}' '{0}' stop: {1}", Keywords = Keywords.Rules)]
        public void RuleReserved1Stop(string context, string filePath, string ruleId, string ruleName)
        {
            if (this.IsEnabled())
            {
                WriteEvent((int)DriverEvent.RuleReserved1Stop, context, filePath, ruleId, ruleName);
            }
        }

        public class Keywords // this is a bit vector
        {
            public const EventKeywords Rules = (EventKeywords)0x0001;
        }
    }
}
