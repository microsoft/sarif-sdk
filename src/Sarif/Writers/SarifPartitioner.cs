// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
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
            SarifLog newLog = log.DeepClone();

            var visitor = new FilteringVisitor(predicate);
            newLog = visitor.VisitSarifLog(newLog);

            return newLog;
        }

        /// <summary>
        /// Partition the specified SARIF log into a set of "partitioned logs" according to
        /// the specified partitioning function. Each partitioned log contains only those
        /// elements of run-level collections such as Run.Artifacts that are relevant to the
        /// subset of results in that log.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object returned by the partition function. It must be a reference
        /// type so that null is a valid value. It must override bool Equals(T other) so that
        /// two Ts can compare equal even if they are not reference equal.
        /// </typeparam>
        /// <param name="log">
        /// The SARIF log to be partitioned.
        /// </param>
        /// <param name="partitionFunction">
        /// A function that returns a value specifying which partitioned log a specified result
        /// belongs in, or null if the result should be discarded (not placed in any of the
        /// partitioned logs).
        /// </param>
        /// <param name="deepClone">
        /// A value that specifies how the partitioned logs are constructed from the original log.
        /// If <code>true</code>, each partitioned log is constructed from a deep clone of the
        /// original log; if <code>false</code>, each partitioned log is constructed from a shallow
        /// copy of the original log. Deep cloning ensures that the original and partitioned logs
        /// do not share any objects, so they can be modified safely, but at a cost of increased
        /// partitioning time and  working set. Shallow copying reduces partitioning time and
        /// working set, but it is not safe to modify any of the resulting logs because this class
        /// makes no guarantee about which objects are shared.
        /// </param>
        /// <returns>
        /// A dictionary whose keys are the values returned by <paramref name="partitionFunction"/>
        /// for the results in <paramref name="log"/> and whose values are the SARIF logs
        /// containing the results for which the partition function returns those values.
        /// </returns>
        public static IDictionary<T, SarifLog> Partition<T>(
            SarifLog log,
            PartitionFunction<T> partitionFunction,
            bool deepClone)
            where T : class, IEquatable<T>
        {
            var visitor = new PartitioningVisitor<T>(partitionFunction, deepClone);
            visitor.VisitSarifLog(log);

            return visitor.GetPartitionLogs();
        }
    }
}
