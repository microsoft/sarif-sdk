// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class ResultsCachingLogger : IAnalysisLogger
    {
        private string currentFileHash;
        private bool cacheLoggingData;
        public Dictionary<string, List<Tuple<ReportingDescriptor, Result>>> HashToResultsMap { get; private set; }

        public ResultsCachingLogger(bool verbose)
        {
            Verbose = verbose;
        }

        public void AnalysisStarted()
        {
            HashToResultsMap = new Dictionary<string, List<Tuple<ReportingDescriptor, Result>>>();
        }

        public bool Verbose { get; private set; }

        public void AnalysisStopped(RuntimeConditions runtimeConditions)
        {
        }

        public void AnalyzingTarget(IAnalysisContext context)
        {
            this.currentFileHash = context.Hashes.Sha256;

            if (HashToResultsMap.ContainsKey(currentFileHash))
            {
                // We already have results cached for this file. This means that we shouldn't collect
                // any data in calls to Log. These calls are the driver framework 'replaying' the
                // cached results via the aggregated logger instance
                cacheLoggingData = false;
            }
            else
            {
                cacheLoggingData = true;
                HashToResultsMap[currentFileHash] = new List<Tuple<ReportingDescriptor, Result>>();
            }
        }

        public void Log(ReportingDescriptor rule, Result result)
        {
            if (!cacheLoggingData) { return; }

            switch (result.Level)
            {
                // These result types are optionally emitted.
                case FailureLevel.None:
                case FailureLevel.Note:
                {
                    if (Verbose)
                    {
                        CacheResult(rule, result);
                    }
                    break;
                }

                // These result types are always emitted.
                case FailureLevel.Error:
                case FailureLevel.Warning:
                {
                    CacheResult(rule, result);
                    break;
                }

                default:
                {
                    throw new InvalidOperationException();
                }
            }
        }

        private void CacheResult(ReportingDescriptor rule, Result result)
        {
            if (!HashToResultsMap.TryGetValue(currentFileHash, out List<Tuple<ReportingDescriptor, Result>> results))
            {
                results = HashToResultsMap[currentFileHash] = new List<Tuple<ReportingDescriptor, Result>>();
            }
            results.Add(new Tuple<ReportingDescriptor, Result>(rule, result));
        }

        public void LogConfigurationNotification(Notification notification)
        {
        }

        public void LogMessage(bool verbose, string message)
        {
        }

        public void LogToolNotification(Notification notification)
        {
        }
    }
}
