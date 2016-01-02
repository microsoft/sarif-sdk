// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;

using Microsoft.CodeAnalysis.Sarif.Sdk;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
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

        public void Log(IRuleDescriptor rule, Result result)
        {
            Log(result.Kind, result.RuleId);
        }

        public void LogMessage(bool verbose, string message)
        {

        }

        public void Log(ResultKind messageKind, string ruleId)
        {
            switch (messageKind)
            {

                case ResultKind.Pass:
                    {
                        break;
                    }

                case ResultKind.Error:
                {
                    break;
                }

                case ResultKind.Warning:
                {
                    break;
                }

                case ResultKind.NotApplicable:
                    {
                        if (ruleId == Notes.InvalidTarget.Id)
                        {
                            _invalidTargetsCount++;
                        }
                        break;
                    }

                case ResultKind.InternalError:
                    {
                        break;
                    }

                case ResultKind.ConfigurationError:
                    {
                        break;
                    }

                default:
                    {
                        throw new InvalidOperationException();
                    }
            }
        }

        public void Dispose()
        {
        }
    }
}
