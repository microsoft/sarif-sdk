// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class StatisticsLogger : IAnalysisLogger
    {
        private Stopwatch _stopwatch;
        private long _targetsCount;
        private long _invalidTargetsCount;

        public StatisticsLogger()
        {
        }

        public void AnalysisStarted()
        {
            _stopwatch = Stopwatch.StartNew();
        }

        public void AnalysisStopped(RuntimeConditions runtimeConditions)
        {
            Console.WriteLine();
            Console.WriteLine("# valid targets: " + _targetsCount.ToString());
            Console.WriteLine("# invalid targets: " + _invalidTargetsCount.ToString());
            Console.WriteLine("Time elapsed: " + _stopwatch.Elapsed.ToString());
        }

        public void AnalyzingTarget(IAnalysisContext context)
        {
            _targetsCount++;
        }

        public void Log(ReportingDescriptor rule, Result result)
        {
            Log(result.Level, result.RuleId);
        }

        public void LogToolNotification(Notification notification)
        {
        }

        public void LogConfigurationNotification(Notification notification)
        {
            if (notification.Descriptor != null && notification.Descriptor.Id == Warnings.Wrn997_InvalidTarget)
            {
                _invalidTargetsCount++;
            }
        }

        public void LogMessage(bool verbose, string message)
        {
        }

        public void Log(FailureLevel level, string ruleId)
        {
            switch (level)
            {
                case FailureLevel.None:
                    break;

                case FailureLevel.Error:
                    break;

                case FailureLevel.Warning:
                    break;

                case FailureLevel.Note:
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        public void Dispose()
        {
        }
    }
}
