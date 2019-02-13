// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class AggregatingLogger : IDisposable, IAnalysisLogger
    {
        public AggregatingLogger() : this(null)
        {
        }

        public AggregatingLogger(IEnumerable<IAnalysisLogger> loggers)
        {
            this.Loggers = loggers != null ?
                new List<IAnalysisLogger>(loggers) :
                new List<IAnalysisLogger>();
        }

        public IList<IAnalysisLogger> Loggers { get; set; }

        public void Dispose()
        {
            foreach (IAnalysisLogger logger in Loggers)
            {
                using (logger as IDisposable) { };
            }
        }

        public void AnalysisStarted()
        {
            foreach (IAnalysisLogger logger in Loggers)
            {
                logger.AnalysisStarted();
            }
        }

        public void AnalysisStopped(RuntimeConditions runtimeConditions)
        {
            foreach (IAnalysisLogger logger in Loggers)
            {
                logger.AnalysisStopped(runtimeConditions);
            }
        }

        public void AnalyzingTarget(IAnalysisContext context)
        {
            foreach (IAnalysisLogger logger in Loggers)
            {
                logger.AnalyzingTarget(context);
            }
        }

        public void LogMessage(bool verbose, string message)
        {
            foreach (IAnalysisLogger logger in Loggers)
            {
                logger.LogMessage(verbose, message);
            }
        }

        public void Log(ReportingDescriptor rule, Result result)
        {
            foreach (IAnalysisLogger logger in Loggers)
            {
                logger.Log(rule, result);
            }
        }

        public void LogToolNotification(Notification notification)
        {
            foreach (IAnalysisLogger logger in Loggers)
            {
                logger.LogToolNotification(notification);
            }
        }

        public void LogConfigurationNotification(Notification notification)
        {
            foreach (IAnalysisLogger logger in Loggers)
            {
                logger.LogConfigurationNotification(notification);
            }
        }
    }
}
