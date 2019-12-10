// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    /// <summary>
    /// A visitor that partitions a specified SARIF log into a set of "partition logs."
    /// </summary>
    /// <remarks>
    /// <para>
    /// Two results are placed in the same partition log if and only if a specified "partition
    /// function" returns the same value (the "partition value") for both results, except that
    /// results whose partition value is null are discarded (not placed in any of the partition
    /// logs). We refer to a set of results with the same partition value as a "partition."
    /// </para>
    /// <para>
    /// Each run in the original log is partitioned independently. If an original run contains no
    /// results from a given partition, the partition log for that partition does not contain a run
    /// corresponding to that original run.
    /// </para>
    /// <para>
    /// Each run in each partition log contains only those elements of run-level collections such
    /// as Run.Artifacts that are relevant to the subset of results from that original run which
    /// belong to that partition.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">
    /// The type of the object returned from the partition function with which the visitor is
    /// constructed. It must be a reference type so that null is a valid value. It must override
    /// bool Equals(T other) so that two Ts can compare equal even if they are not reference equal.
    /// </typeparam>
    public class PartitioningVisitor<T> : SarifRewritingVisitor where T : class
    {
        // The name of a property in the property bag of each partition log which contains the
        // partition value for the results in that log file.
        private const string PartitionValuePropertyName = "partitionValue";

        /// <summary>
        /// A delegate for a function that returns a value specifying which partition a specified
        /// result belongs to, or null if the result should be discarded (not placed in any
        /// partition).
        /// </summary>
        /// <param name="result">
        /// The result to be assigned to a partition.
        /// </param>
        /// <typeparam name="T">
        /// The type of the object returned from the partition function. It must be a reference
        /// type so that null is a valid value. It must override bool Equals(T other) so that
        /// two Ts can compare equal even if they are not reference equal.
        /// </typeparam>
        public delegate T PartitionFunction(Result result);

        // The partition function being used to partition the original log.
        private readonly PartitionFunction partitionFunction;

        // The log file being partitioned.
        private SarifLog originalLog;

        // The index of the current run (the run currently being partitioned) from the original log
        // file.
        private int currentRunIndex;

        // The partition value of the current result (the result currently being processed).
        private T currentPartitionValue = null;

        // The set of results in each partition in each original run. That is,
        //
        //     partitionedResultDictionaries[runIndex][partitionValue]
        //
        // contains the list of results in the run at runIndex for whose partition value is
        // partitionValue.
        private List<Dictionary<T, List<Result>>> partitionedResultDictionaries;

        // The set of all partition values encountered on all results in all runs of the original
        // log file.
        private HashSet<T> partitionValues;

        private Dictionary<T, SarifLog> partitionLogDictionary;

        /// <summary>
        /// Returns a mapping from partition value to the partition log for that value.
        /// </summary>
        /// <returns></returns>
        public Dictionary<T, SarifLog> GetPartitionLogs()
        {
            if (originalLog == null)
            {
                throw new InvalidOperationException(
                    "You must call " + nameof(VisitSarifLog) +
                    " before you call " + nameof(GetPartitionLogs) + ".");
            }

            if (partitionLogDictionary == null)
            {
                CreatePartitionLogDictionary();
            }

            return partitionLogDictionary;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartitioningVisitor"/> class.
        /// </summary>
        /// <param name="partitionFunction">
        /// A delegate for a function that returns a value specifying which partition each result
        /// belongs to.
        /// </param>
        public PartitioningVisitor(PartitionFunction partitionFunction)
        {
            this.partitionFunction = partitionFunction;
        }

        public override SarifLog VisitSarifLog(SarifLog node)
        {
            originalLog = node;
            currentRunIndex = -1;
            partitionValues = new HashSet<T>();
            partitionedResultDictionaries = new List<Dictionary<T, List<Result>>>();
            partitionLogDictionary = null;

            return base.VisitSarifLog(node);
        }

        public override Run VisitRun(Run node)
        {
            ++currentRunIndex;
            partitionedResultDictionaries.Add(new Dictionary<T, List<Result>>());

            return base.VisitRun(node);
        }

        public override Result VisitResult(Result node)
        {
            currentPartitionValue = partitionFunction(node);
            if (currentPartitionValue != null)
            {
                if (!partitionValues.Contains(currentPartitionValue))
                {
                    partitionValues.Add(currentPartitionValue);
                }

                if (!partitionedResultDictionaries[currentRunIndex].ContainsKey(currentPartitionValue))
                {
                    partitionedResultDictionaries[currentRunIndex].Add(currentPartitionValue, new List<Result>());
                }

                partitionedResultDictionaries[currentRunIndex][currentPartitionValue].Add(node);
            }

            return base.VisitResult(node);
        }

        private void CreatePartitionLogDictionary()
        {
            partitionLogDictionary = partitionValues.ToDictionary(
                keySelector: pv => pv,
                elementSelector: CreatePartitionLog);
        }

        private SarifLog CreatePartitionLog(T partitionValue)
        {
            var partitionLog = new SarifLog
            {
                Runs = new List<Run>()
            };

            partitionLog.SetProperty(PartitionValuePropertyName, partitionValue);

            for (int iRun = 0; iRun < partitionedResultDictionaries.Count; ++iRun)
            {
                if (partitionedResultDictionaries[iRun].TryGetValue(partitionValue, out List<Result> results))
                {
                    Run run = originalLog.Runs[iRun].DeepClone();
                    run.Results = results;
                    partitionLog.Runs.Add(run);
                }
            }

            return partitionLog;
        }
    }
}