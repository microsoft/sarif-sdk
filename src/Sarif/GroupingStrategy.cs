// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Enumerates the grouping strategies provided out of the box by the WorkItemFiler.
    /// </summary>
    public enum GroupingStrategy
    {
        /// <summary>
        /// No grouping strategy was specified.
        /// 
        /// </summary>
        None = 0,

        /// <summary>
        /// The current default grouping strategy, which is to assume that all
        /// contents within a run should be persisted to a single work item. 
        /// </summary>
        PerRun = 0,

        /// <summary>
        /// A grouping strategy that splits SARIF log files into a single log per run, per rule.
        /// I.e., the total number of log files created is the sum of the unique rules in each log.
        /// </summary>
        PerRunPerRule = 1,

        /// <summary>
        /// A grouping strategy that splits SARIF log files into a single log per run, per target,
        /// per rule. I.e., the total number of log files created is the sum of the unique rules
        /// associated with each target within each run.
        /// </summary>
        PerRunPerTargetPerRule = 2,
    }
}