// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    /// <summary>
    /// A visitor that partitions a specified SARIF log into a set of "partitioned logs."
    /// </summary>
    /// <remarks>
    /// Two results are placed in the same partitioned log if and only if a specified
    /// "partition function" returns the same value for both results, except that results for
    /// which the partition function returns null are discarded (not placed in any of the
    /// partitioned logs). Each partitioned log contains only those elements of run-level
    /// collections such as Run.Artifacts that are relevant to the subset of results in that log.
    /// </remarks>
    /// <typeparam name="T">
    /// The type of the object returned from the partition function with which the visitor is
    /// constructed. It must be a reference type so that null is a valid value. It must override
    /// bool Equals(T other) so that two Ts can compare equal even if they are not reference equal.
    /// </typeparam>
    public class PartitioningVisitor<T> : SarifRewritingVisitor where T : class
    {
        /// <summary>
        /// A delegate for a function that returns a value specifying which partitioned log
        /// a specified result belongs in, or null if the result should be discarded (not
        /// placed in any of the partitioned logs).
        /// </summary>
        /// <param name="result">
        /// The result to be assigned to a partitioned log.
        /// </param>
        /// <typeparam name="T">
        /// The type of the object returned from the partition function. It must be a reference
        /// type so that null is a valid value. It must override bool Equals(T other) so that
        /// two Ts can compare equal even if they are not reference equal.
        /// </typeparam>
        public delegate T PartitionFunction(Result result);

        // The name of a property in the property bag of each partitioned log file,
        // containing the value returned by the partition function for that log file.
        private const string PartitionValuePropertyName = "partitionValue";

        private readonly PartitionFunction partitionFunction;

        // The set of all partition values encountered across all runs.
        private HashSet<T> partitionValues;

        // Partition the results independently for each run. That is,
        //
        //     partitionedResultDictionaries[runIndex][partitionValue]
        //
        // contains the list of results in the run at runIndex for which the partition function
        // returned partitionValue.
        //
        private IList<Dictionary<T, List<Result>>> partitionedResultDictionaries;

        private Dictionary<T, List<Result>> currentResultDictionary;

        // For each run, accumulate the run-level properties (such as artifacts) that are relevant
        // to the results in each partition. That is, for example,
        //
        //     partitionedArtifactDictionaries[runIndex][partitionValue]
        //
        // contains the list of artifacts that are relevant to the results in
        //
        //     partitionedResultDictionaries[runIndex][partitionValue]
        //
        private IList<Dictionary<T, List<Artifact>>> partitionedArtifactDictionaries;

        private Dictionary<T, List<Artifact>> currentArtifactDictionary;

        private SarifLog originalLog;
        private int currentRunIndex;
        private Run currentRun;

        private IDictionary<T, SarifLog> partitionedSarifLogDictionary;

        public IDictionary<T, SarifLog> GetPartitionedSarifLogDictionary()
        {
            if (originalLog == null)
            {
                throw new InvalidOperationException(
                    "You must call " + nameof(VisitSarifLog) +
                    " before you call " + nameof(GetPartitionedSarifLogDictionary) + ".");
            }

            if (partitionedSarifLogDictionary == null)
            {
                // Create a separate log file for each partition value.
                partitionedSarifLogDictionary = partitionValues.ToDictionary(
                    keySelector: partitionValue => partitionValue,
                    elementSelector: CreateSarifLogForPartitionValue);
            }

            return partitionedSarifLogDictionary;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartitioningVisitor"/> class.
        /// </summary>
        /// <param name="partitionFunction">
        /// A delegate for a function that returns a value specifying which partitioned
        /// log each result belongs to.
        /// </param>
        public PartitioningVisitor(PartitionFunction partitionFunction)
        {
            this.partitionFunction = partitionFunction;
        }

        public override SarifLog VisitSarifLog(SarifLog node)
        {
            originalLog = node;
            partitionedSarifLogDictionary = null;

            partitionValues = new HashSet<T>();
            partitionedResultDictionaries = new List<Dictionary<T, List<Result>>>();
            partitionedArtifactDictionaries = new List<Dictionary<T, List<Artifact>>>();

            currentResultDictionary = null;
            currentArtifactDictionary = null;

            currentRunIndex = -1;
            currentRun = null;

            return base.VisitSarifLog(node);
        }

        public override Run VisitRun(Run node)
        {
            ++currentRunIndex;
            currentRun = node;

            currentResultDictionary = new Dictionary<T, List<Result>>();
            currentArtifactDictionary = new Dictionary<T, List<Artifact>>();

            partitionedResultDictionaries.Add(currentResultDictionary);
            partitionedArtifactDictionaries.Add(currentArtifactDictionary);

            return base.VisitRun(node);
        }

        public override Result VisitResult(Result node)
        {
            T partitionValue = partitionFunction(node);
            if (partitionValue != null)
            {
                partitionValues.Add(partitionValue);

                if (!currentResultDictionary.ContainsKey(partitionValue))
                {
                    currentResultDictionary.Add(partitionValue, new List<Result>());
                }

                currentResultDictionary[partitionValue].Add(node);
            }

            return base.VisitResult(node);
        }

        private SarifLog CreateSarifLogForPartitionValue(T partitionValue)
        {
            SarifLog partitionedLog = originalLog.DeepClone();
            partitionedLog.SetProperty(PartitionValuePropertyName, partitionValue);
            partitionedLog.Runs = new List<Run>();

            // For each run in the original log file that had any results for this partition
            // value, create a run in the partitioned log file that contains only those results
            // and the relevant subset of the run-level properties such as artifacts.
            //
            // IS THAT RIGHT? DO WE WANT AN EMPTY RUN IN THE PARTITIONED LOG IF A GIVEN RUN
            // IN THE ORIGINAL LOG HAD NO RESULTS IN THIS PARTITION?
            //
            // If we got here, we know that there must be at least one run in original log file,
            // because there was at least one result in the log file with this partition value.
            for (int iRun = 0; iRun < originalLog.Runs.Count; ++iRun)
            {
                if (partitionedResultDictionaries[iRun].ContainsKey(partitionValue))
                {
                    List<Result> partitionedResults = partitionedResultDictionaries[iRun][partitionValue];

                    Run partitionedRun = CreateRunForPartitionValue(
                        originalLog.Runs[iRun], partitionedResults);

                    partitionedLog.Runs.Add(partitionedRun);
                }
            }

            return partitionedLog;
        }

        private Run CreateRunForPartitionValue(Run originalRun, List<Result> partitionedResults)
        {
            Run partitionedRun = originalRun.DeepClone();

            partitionedRun.Results = partitionedResults;

            return partitionedRun;
        }
    }
}
