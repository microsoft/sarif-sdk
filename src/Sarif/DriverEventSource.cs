// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.Tracing;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    ///  EventSource is an ETW EventSource for Microsoft.CodeAnalysis.Sarif.PatternMatcher events, allowing performance tracing.
    /// </summary>
    [EventSource(Name = "SarifDriver", Guid = "c84480b4-a77f-421f-8a11-48210c1724d4")]
    public class DriverEventSource : EventSource
    {
        public static DriverEventSource Log = new DriverEventSource();

        [Event((int)DriverEvent.GetTargetStart, Message = "Get contents: {0}")]
        public void GetTargetStart(string filePath)
        {
            WriteEvent((int)DriverEvent.GetTargetStart, filePath);
        }

        [Event((int)DriverEvent.GetTargetStop, Message = "Contents retrieved: {0}")]
        public void GetTargetStop(string filePath)
        {
            WriteEvent((int)DriverEvent.GetTargetStop, filePath);
        }

        [Event((int)DriverEvent.ScanTargetStart, Message = "Scan start: {0}")]
        public void ScanTargetStart(string filePath)
        {
            WriteEvent((int)DriverEvent.ScanTargetStart, filePath);
        }

        [Event((int)DriverEvent.ScanTargetStop, Message = "Scan stop: {0}")]
        public void ScanTargetStop(string filePath)
        {
            WriteEvent((int)DriverEvent.ScanTargetStop, filePath);
        }

        [Event((int)DriverEvent.RuleStart, Message = "'{1}.{2}' start: {0}")]
        public void RuleStart(string filePath, string ruleId, string ruleName)
        {
            WriteEvent((int)DriverEvent.RuleStart, filePath, ruleId, ruleName);
        }

        [Event((int)DriverEvent.RuleStop, Message = "'{1}.{2}' stop: {0}")]
        public void RuleStop(string filePath, string ruleId, string ruleName)
        {
            WriteEvent((int)DriverEvent.RuleStop, filePath, ruleId, ruleName);
        }

        [Event((int)DriverEvent.RuleFired, Message = "'{1}.{2}' fired with severity '{3}': {0}")]
        public void RuleFired(string filePath, string ruleId, string ruleName, FailureLevel level)
        {
            WriteEvent((int)DriverEvent.RuleFired, filePath, ruleId, ruleName, level);
        }

        [Event((int)DriverEvent.ArtifactSizeInBytes, Message = "{0} : size {1} bytes.")]
        public void ArtifactSizeInBytes(string filePath, ulong sizeInBytes)
        {
            WriteEvent((int)DriverEvent.ArtifactSizeInBytes, filePath, sizeInBytes);
        }

        [Event((int)DriverEvent.EnumerateTargetsStart, Message = "Target enumeration started.")]
        public void EnumerateTargetsStart()
        {
            WriteEvent((int)DriverEvent.EnumerateTargetsStart);
        }

        [Event((int)DriverEvent.EnumerateTargetsStop, Message = "Target enumeration stopped.")]
        public void EnumerateTargetsStop()
        {
            WriteEvent((int)DriverEvent.EnumerateTargetsStop);
        }

        [Event((int)DriverEvent.TargetReserved, Message = "'{0}': {1}")]
        public void TargetReserved(string eventId, string filePath)
        {
            WriteEvent((int)DriverEvent.TargetReserved, eventId, filePath);
        }

        [Event((int)DriverEvent.TargetReservedStart, Message = "'{0}' start: {1}")]
        public void TargetReservedStart(string eventId, string filePath)
        {
            WriteEvent((int)DriverEvent.TargetReservedStart, eventId, filePath);
        }

        [Event((int)DriverEvent.TargetReservedStop, Message = "'{0}' stop: {1}")]
        public void TargetReservedStop(string eventId, string filePath)
        {
            WriteEvent((int)DriverEvent.TargetReservedStop, eventId, filePath);
        }

        [Event((int)DriverEvent.RuleReservedStart, Message = "'{2}.{3}' '{0}' start: {1}")]
        public void RuleReservedStart(string eventId, string filePath, string ruleId, string ruleName)
        {
            WriteEvent((int)DriverEvent.RuleReservedStart, eventId, filePath, ruleId, ruleName);
        }

        [Event((int)DriverEvent.RuleReservedStop, Message = "'{2}.{3}' '{0}' stop: {1}")]
        public void RuleReservedStop(string eventId, string filePath, string ruleId, string ruleName)
        {
            WriteEvent((int)DriverEvent.RuleReservedStop, eventId, filePath, ruleId, ruleName);
        }

    }
}
