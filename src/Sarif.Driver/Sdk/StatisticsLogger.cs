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

        public void Log(IRule rule, Result result)
        {
            Log(result.Level, result.RuleId);
        }

        public void LogToolNotification(Notification notification)
        {
        }

        public void LogConfigurationNotification(Notification notification)
        {
        }

        public void LogMessage(bool verbose, string message)
        {
        }

        public void Log(ResultLevel level, string ruleId)
        {
            switch (level)
            {
                case ResultLevel.Pass:
                    break;

                case ResultLevel.Error:
                    break;

                case ResultLevel.Warning:
                    break;

                case ResultLevel.NotApplicable:
                    if (ruleId == Notes.InvalidTarget.Id)
                    {
                        _invalidTargetsCount++;
                    }
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
