// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Enumerates the log file splitting strategies provided out of the box by the SarifWorkItemFiler.
    /// </summary>
    public enum SplittingStrategy
    {
        /// <summary>
        /// No log file splitting strategy was specified.
        /// I.e., the total number of log files created is one (the original, unsplit log).
        /// 
        /// </summary>
        None = 0,

        /// <summary>
        /// Split the SARIF log file into a single file for each run.
        /// I.e., the total number of log files created is the sum of individual run in each log.
        /// </summary>
        PerRun,

        /// <summary>
        /// Split SARIF log files into a single log for each result.
        /// I.e., the total number of log files created is the sum of individual results in each log.
        /// </summary>
        PerResult,

        /// <summary>
        /// A grouping strategy that splits SARIF log files into a single log per run, per rule.
        /// I.e., the total number of log files created is the sum of the unique rules in each run.
        /// </summary>
        PerRunPerRule,

        /// <summary>
        /// A grouping strategy that splits SARIF log files into a single log per run, per target,
        /// per rule. I.e., the total number of log files created is the sum of the unique rules
        /// associated with each target in each run.
        /// </summary>
        PerRunPerTargetPerRule,

        /// <summary>
        /// A grouping strategy that splits SARIF log files into a single log per run, per target.
        /// I.e., the total number of log files created is the sum of the unique targets in each run.
        /// </summary>
        PerRunPerTarget,

        /// <summary>
        /// A grouping strategy that splits SARIF log files into a single log per run, per organization per entity type per partial fingerprint.
        /// I.e., the total number of log files created is the sum of the unique fingerprint by organization and entity type.
        /// </summary>
        PerRunPerOrgPerEntityTypePerPartialFingerprint,

        /// <summary>
        /// A grouping strategy that splits SARIF log files into a single log per run, per organization per entity type per repository per partial fingerprint.
        /// I.e., the total number of log files created is the sum of the unique fingerprint by organization, entity type, and repository.
        /// </summary>
        PerRunPerOrgPerEntityTypePerRepositoryPerPartialFingerprint,
    }
}