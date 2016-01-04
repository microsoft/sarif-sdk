// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Sdk;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public interface IAnalysisLogger
    {
        void AnalysisStarted();
        void AnalysisStopped(RuntimeConditions runtimeConditions);

        // TODO argument here should be IAnalysisTarget, when authored
        void AnalyzingTarget(IAnalysisContext context);

        /// <summary>
        /// Log a diagnostic result
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="result"></param>
        void Log(IRuleDescriptor rule, Result result);

        /// <summary>
        /// Log a simple message for display to users (which 
        /// will not be persisted to log files)
        /// </summary>
        /// <param name="message"></param>
        void LogMessage(bool verbose, string message);
    }
}
