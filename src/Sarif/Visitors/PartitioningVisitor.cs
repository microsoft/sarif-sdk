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

        // Certain objects (for example, artifact locations) must be visited multiple times: first
        // when we traverse the original log to determine the mappings between array indices in the
        // original log and the partitioned logs, and second when we use those mappings to create
        // the partitioned logs.
        private enum ExecutionPhase
        {
            None,
            VisitingOriginalLog,
            ConstructingPartitionedLogs
        }

        private ExecutionPhase executionPhase;

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

        // A value that specifies whether to construct each partitioned log from a deep clone of
        // the original log (if true) or from a shallow copy of the original log (if false).
        // TODO: We'll need unit tests with and without deep clone.
        private readonly bool deepClone;

        // The log file being partitioned.
        // TODO: At the end, get rid of all private variables we no longer need (victims of redesigns and refactors).
        private SarifLog originalLog;

        // The index of the current run (the run currently being partitioned) from the original log
        // file.
        private int currentRunIndex;

        private IList<Artifact> currentRunArtifacts;

        // The partition value of the current result (the result currently being processed).
        private T currentPartitionValue = null;

        // The set of results in each partition in each original run. That is,
        //
        //     partitionedResultDictionaries[runIndex][partitionValue]
        //
        // contains the list of results in the run at runIndex for whose partition value is
        // partitionValue.
        private List<Dictionary<T, List<Result>>> partitionedResultDictionaries;

        // A set of per-run, per-partition index remappings for run-level properties such as
        // artifacts. For example,
        //
        //    artifactIndexRemappings[runIndex][partitionValue][originalIndex]
        //
        // contains the index within the run at index runIndex in the partition log for
        // partitionValue of the artifact whose index in the corresponding original run was
        // originalIndex.
        private List<Dictionary<T, Dictionary<int, int>>> artifactIndexRemappings;

        // The set of all partition values encountered on all results in all runs of the original
        // log file.
        private HashSet<T> partitionValues;

        private Dictionary<T, SarifLog> partitionLogDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="PartitioningVisitor"/> class.
        /// </summary>
        /// <param name="partitionFunction">
        /// A delegate for a function that returns a value specifying which partition each result
        /// belongs to.
        /// </param>
        /// <param name="deepClone">
        /// A value that specifies how the partitioned logs are constructed from the original log.
        /// If <code>true</code>, each partitioned log is constructed from a deep clone of the
        /// original log; if <code>false</code>, each partitioned log is constructed from a shallow
        /// copy of the original log. Deep cloning ensures that the original and partitioned logs
        /// do not share any objects, so they can be modified safely, but at a cost of increased
        /// partitioning time and  working set. Shallow copying reduces partitioning time and
        /// working set, but it is not safe to modify any of the resulting logs because this class
        /// makes no guarantee about which objects are shared. The default is <code>false</code>.
        /// </param>
        public PartitioningVisitor(PartitionFunction partitionFunction, bool deepClone)
        {
            this.partitionFunction = partitionFunction;
            this.deepClone = deepClone;
        }

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
                executionPhase = ExecutionPhase.ConstructingPartitionedLogs;
                CreatePartitionLogDictionary();
            }

            return partitionLogDictionary;
        }

        public override SarifLog VisitSarifLog(SarifLog node)
        {
            executionPhase = ExecutionPhase.VisitingOriginalLog;

            originalLog = node;
            currentRunIndex = -1;
            partitionValues = new HashSet<T>();
            partitionedResultDictionaries = new List<Dictionary<T, List<Result>>>();
            artifactIndexRemappings = new List<Dictionary<T, Dictionary<int, int>>>();
            partitionLogDictionary = null;

            return base.VisitSarifLog(node);
        }

        public override Run VisitRun(Run node)
        {
            ++currentRunIndex;
            currentRunArtifacts = originalLog.Runs[currentRunIndex].Artifacts;
            partitionedResultDictionaries.Add(new Dictionary<T, List<Result>>());

            artifactIndexRemappings.Add(new Dictionary<T, Dictionary<int, int>>());

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

                Result partitionedResult = deepClone
                    ? node.DeepClone()
                    : new Result(node);

                partitionedResultDictionaries[currentRunIndex][currentPartitionValue].Add(partitionedResult);
            }

            Result visitedResult = base.VisitResult(node);

            // Unset currentPartitionValue so that VisitArtifactLocation (and other methods which
            // accumulate run-level properties) knows whether the ArtifactLocation occurs in the
            // context of a result. The run-level properties will only contain elements that are
            // relevant to the results in a single partition.
            currentPartitionValue = null;

            return visitedResult;
        }

        public override ArtifactLocation VisitArtifactLocation(ArtifactLocation node)
        {
            if (executionPhase == ExecutionPhase.VisitingOriginalLog)
            {
                VisitArtifactLocationInOriginalLog(node);
            }
            else if (executionPhase == ExecutionPhase.ConstructingPartitionedLogs)
            {
                VisitArtifactLocationForPartitionedLogs(node);
            }
            else
            {
                throw new InvalidOperationException("Invalid execution phase: " + executionPhase);
            }

            return base.VisitArtifactLocation(node);
        }

        // When visiting the artifact locations in the original log, construct the mappings
        // between the artifact indices in the original and partitioned logs.
        private void VisitArtifactLocationInOriginalLog(ArtifactLocation node)
        {
            // Does this artifactLocation occur in the context of a result? And if so, does it
            // refer to an element of the original run's artifacts array?
            if (currentPartitionValue != null
                && currentRunArtifacts != null
                && node.Index >= 0
                && node.Index < currentRunArtifacts.Count)
            {
                // Remap this index if it isn't already remapped.
                if (!artifactIndexRemappings[currentRunIndex].ContainsKey(currentPartitionValue))
                {
                    artifactIndexRemappings[currentRunIndex].Add(currentPartitionValue, new Dictionary<int, int>());
                }

                Dictionary<int, int> artifactIndexRemapping = artifactIndexRemappings[currentRunIndex][currentPartitionValue];
                if (!artifactIndexRemapping.ContainsKey(node.Index))
                {
                    artifactIndexRemapping.Add(node.Index, artifactIndexRemapping.Count);
                }

                // If the specified artifact has a parent, we'll need to bring it along as well.
                Artifact artifact = currentRunArtifacts[node.Index];
                if (artifact.ParentIndex >= 0
                    && artifact.ParentIndex < currentRunArtifacts.Count
                    && !artifactIndexRemapping.ContainsKey(artifact.ParentIndex))
                {
                    artifactIndexRemapping.Add(artifact.ParentIndex, artifactIndexRemapping.Count);
                }
            }
        }

        private void VisitArtifactLocationForPartitionedLogs(ArtifactLocation node)
        {
        }

        private void CreatePartitionLogDictionary()
        {
            partitionLogDictionary = partitionValues.ToDictionary(
                keySelector: pv => pv,
                elementSelector: CreatePartitionedLog);
        }

        private SarifLog CreatePartitionedLog(T partitionValue)
        {
            SarifLog partitionedLog;
            if (deepClone)
            {
                // Save time and space by not cloning the runs unless and until necessary. We will
                // only need to clone the runs that have results in this partition.
                IList<Run> originalRuns = originalLog.Runs;
                originalLog.Runs = null;

                partitionedLog = originalLog.DeepClone();

                originalLog.Runs = originalRuns;
            }
            else
            {
                partitionedLog = new SarifLog(originalLog)
                {
                    Runs = null
                };
            }

            partitionedLog.Runs = new List<Run>();
            partitionedLog.SetProperty(PartitionValuePropertyName, partitionValue);

            for (int iRun = 0; iRun < partitionedResultDictionaries.Count; ++iRun)
            {
                // Are there results for this run in this partition?
                if (partitionedResultDictionaries[iRun].TryGetValue(partitionValue, out List<Result> results))
                {
                    // Yes, so we'll need a copy of the original run in which the results, and
                    // certain run-level collections such as Run.Artifacts, have been replaced.
                    Run originalRun = originalLog.Runs[iRun];
                    Run partitionedRun;
                    if (deepClone)
                    {
                        // Save time and space by only cloning the necessary results and associated
                        // collection elements. We will only need to clone the results in this
                        // partition.
                        IList <Result> originalResults = originalRun.Results;
                        IList<Artifact> originalArtifacts = originalRun.Artifacts;
                        originalRun.Results = null;
                        originalRun.Artifacts = null;

                        partitionedRun = originalRun.DeepClone();

                        originalRun.Results = originalResults;
                        originalRun.Artifacts = originalArtifacts;
                    }
                    else
                    {
                        partitionedRun = new Run(originalRun)
                        {
                            Results = null,
                            Artifacts = null
                        };
                    }

                    partitionedRun.Results = results;

                    // Copy the artifacts that were mentioned in the results in this partition.
                    if (artifactIndexRemappings[iRun].TryGetValue(partitionValue, out Dictionary<int, int> indexRemappings))
                    {
                        partitionedRun.Artifacts = new List<Artifact>();
                        foreach (int originalIndex in indexRemappings.Keys)
                        {
                            // Even if the user didn't ask to deep clone the entire log, we must
                            // deep clone the artifact, because we need to modify one of its
                            // subproperties (artifact.Location.Index).
                            Artifact originalArtifact = originalRun.Artifacts[originalIndex];
                            Artifact partitionedArtifact = originalArtifact.DeepClone();

                            if (originalArtifact.Location.Index >= 0)
                            {
                                partitionedArtifact.Location.Index = originalIndex;
                            }

                            if (originalArtifact.ParentIndex >= 0)
                            {
                                partitionedArtifact.ParentIndex = indexRemappings[originalArtifact.ParentIndex];
                            }

                            partitionedRun.Artifacts.Add(partitionedArtifact);
                        }
                    }

                    partitionedLog.Runs.Add(partitionedRun);
                }

                // TODO: Traverse the entire log, fixing the index mappings for indices that appear
                // in the remapping dictionaries, and settings indices to -1 if they don't appear
                // in the remapping dictionaries. This can happen (for example) when an artifactLocation
                // that is not part of a result refers to an element of run.artifacts. For simplicity,
                // we have elected not to attempt to carry along those locations. On second thought
                // it's not that much more work so let's consider it. We'd need a remapping for
                // artifact locations in a context where currentPartitionValue is null -- and
                // how do we distinguish that from the case where the partitioning function returned null?
                // Answer: we don't overload currentPartitionValue like that. We have an explicit
                // flag "inResult".
            }

            return partitionedLog;
        }
    }
}