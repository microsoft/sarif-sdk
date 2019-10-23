// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Visitors;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    /// <summary>
    /// Methods to break a SARIF log file into pieces.
    /// </summary>
    public static class SarifPartitioner
    {
        /// <summary>
        /// Filter the specified SARIF log to create a new log containing only those results for
        /// which the specified predicate returns true, and only those elements of run-level
        /// collections such as Run.Artifacts that are relevant to the filtered results.
        /// </summary>
        /// <param name="log">
        /// The log file to be filtered.
        /// </param>
        /// <param name="predicate">
        /// The predicate that selects the results in the filtered log file.
        /// </param>
        /// <returns>
        /// A new SARIF log containing only the filtered results, and only the relevant elements
        /// of the run-level collections.
        /// </returns>
        public static SarifLog Filter(SarifLog log, FilteringVisitor.IncludeResultPredicate predicate)
        {
            var newLog = log.DeepClone();

            var visitor = new FilteringVisitor(predicate);
            newLog = visitor.VisitSarifLog(newLog);

            return newLog;
        }
    }
}
