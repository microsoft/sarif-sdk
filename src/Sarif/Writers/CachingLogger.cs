// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    /// <summary>
    /// This class caches all results and notifications that are passed to it.
    /// Data cached this way can subsequently be replayed into other IAnalysisLogger
    /// instances. The driver framework uses this mechanism to merge results
    /// produced by a multi-threaded analysis into a single output file.
    /// </summary>
    public class CachingLogger : IAnalysisLogger
    {

        public Dictionary<ReportingDescriptor, List<Result>> Results { get; set; }

        public void AnalysisStarted()
        {
        }

        public void AnalysisStopped(RuntimeConditions runtimeConditions)
        {            
        }

        public void AnalyzingTarget(IAnalysisContext context)
        {            
        }

        public void Log(ReportingDescriptor rule, Result result)
        {
            Results ??= new Dictionary<ReportingDescriptor, List<Result>>();

            if (!Results.TryGetValue(rule, out List<Result> results))
            {
                results = Results[rule] = new List<Result>();
            }
            results.Add(result);
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
