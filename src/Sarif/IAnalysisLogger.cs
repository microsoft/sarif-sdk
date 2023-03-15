// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif
{
    public interface IAnalysisLogger
    {
        FileRegionsCache FileRegionsCache { get; set; }

        void AnalysisStarted();

        void AnalysisStopped(RuntimeConditions runtimeConditions);

        void AnalyzingTarget(IAnalysisContext context);

        void TargetAnalyzed(IAnalysisContext context);

        /// <summary>
        /// Log a diagnostic result
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="result"></param>
        void Log(ReportingDescriptor rule, Result result, int? extensionIndex = null);

        /// <summary>
        /// Log a notification that describes a runtime condition detected by the tool.
        /// </summary>
        /// <param name="notification">
        /// The notification to log.
        /// </param>
        /// <param name="associatedRule">
        /// The scan rule, if any, associated with the notification.
        /// </param>
        void LogToolNotification(Notification notification, ReportingDescriptor associatedRule = null);

        /// <summary>
        /// Log a notification that describes a condition relevant to the configuration of the tool.
        /// </summary>
        /// <param name="notification">
        /// The notification to log.
        /// </param>
        void LogConfigurationNotification(Notification notification);
    }
}
