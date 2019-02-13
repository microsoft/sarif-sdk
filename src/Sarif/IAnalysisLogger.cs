// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif
{
    public interface IAnalysisLogger
    {
        void AnalysisStarted();
        void AnalysisStopped(RuntimeConditions runtimeConditions);

        void AnalyzingTarget(IAnalysisContext context);

        /// <summary>
        /// Log a diagnostic result
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="result"></param>
        void Log(ReportingDescriptor rule, Result result);

        /// <summary>
        /// Log a notification that describes a runtime condition detected by the tool.
        /// </summary>
        /// <param name="notification">
        /// The notification to log.
        /// </param>
        void LogToolNotification(Notification notification);

        /// <summary>
        /// Log a notification that describes a condition relevant to the configuration of the tool.
        /// </summary>
        /// <param name="notification">
        /// The notification to log.
        /// </param>
        void LogConfigurationNotification(Notification notification);

        /// <summary>
        /// Log a simple message for display to users (which 
        /// will not be persisted to log files)
        /// </summary>
        /// <param name="message"></param>
        void LogMessage(bool verbose, string message);
    }
}
