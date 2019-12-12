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

        // A value that specifies whether to construct each partitioned log from a deep clone of
        // the original log (if true) or from a shallow copy of the original log (if false).
        private readonly bool deepClone;

        // The log file being partitioned.
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

        // For each partition value, a mapping from the run index in the partition log
        // to the index of the corresponding run in the original log. This is necessary
        // because not every run contains results for every partition value, and we omit
        // runs with no results from the partition logs.
        private Dictionary<T, Dictionary<int, int>> runIndexMappings;

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
                CreatePartitionLogDictionary();
            }

            return partitionLogDictionary;
        }

        public override SarifLog VisitSarifLog(SarifLog node)
        {
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

            return base.VisitArtifactLocation(node);
        }

        private void CreatePartitionLogDictionary()
        {
            runIndexMappings = new Dictionary<T, Dictionary<int, int>>();

            partitionLogDictionary = partitionValues.ToDictionary(
                keySelector: pv => pv,
                elementSelector: CreatePartitionedLog);
        }

        private SarifLog CreatePartitionedLog(T partitionValue)
        {
            runIndexMappings.Add(partitionValue, new Dictionary<int, int>());

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
                    runIndexMappings[partitionValue].Add(runIndexMappings[partitionValue].Count, iRun);

                    // Yes, so we'll need a copy of the original run in which the results, and
                    // certain run-level collections such as Run.Artifacts, have been replaced.
                    Run originalRun = originalLog.Runs[iRun];
                    Run partitionedRun;
                    if (deepClone)
                    {
                        // Save time and space by only cloning the necessary results and associated
                        // collection elements. We already cloned the relevant results in VisitResult.
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
                    // TOD: 
                    if (artifactIndexRemappings[iRun].TryGetValue(partitionValue, out Dictionary<int, int> indexRemappings))
                    {
                        partitionedRun.Artifacts = new List<Artifact>();
                        foreach (int originalIndex in indexRemappings.Keys)
                        {
                            Artifact originalArtifact = originalRun.Artifacts[originalIndex];
                            Artifact partitionedArtifact = deepClone
                                ? originalArtifact.DeepClone()
                                : new Artifact(originalArtifact);

                            if (originalArtifact.ParentIndex >= 0)
                            {
                                partitionedArtifact.ParentIndex = indexRemappings[originalArtifact.ParentIndex];
                            }

                            partitionedRun.Artifacts.Add(partitionedArtifact);
                        }
                    }

                    partitionedLog.Runs.Add(partitionedRun);
                }
            }

            // Traverse the entire log, fixing the index mappings for indices that appear
            // in the remapping dictionaries, and settings indices to -1 if they don't appear
            // in the remapping dictionaries. This can happen (for example) when an artifactLocation
            // that is not part of a result refers to an element of run.artifacts. For simplicity,
            // we have elected not to attempt to carry along those locations.
            // TODO: On second thought
            // it's not that much more work so let's consider it. We'd need a remapping for
            // artifact locations in a context where currentPartitionValue is null -- and
            // how do we distinguish that from the case where the partitioning function returned null?
            // Answer: we don't overload currentPartitionValue like that. We have an explicit
            // flag "inResult".
            // TODO: Figure out how to _replace_ artifact locations in our traversals -- because
            // we need to update their indices, but that means we have to replace them with either
            // deep clones or shallow copies in their _parent_ objects.
            // DO WE NEED TO DO THAT? WHY CAN'T WE UPDATE IN PLACE?
            var remappingVisitor = new PartitionedIndexRemappingVisitor<T>(
                partitionFunction,
                artifactIndexRemappings,
                runIndexMappings);

            remappingVisitor.VisitSarifLog(partitionedLog);

            return partitionedLog;
        }
    }

    internal class PartitionedIndexRemappingVisitor<T> : SarifRewritingVisitor where T : class
    {
        private readonly PartitioningVisitor<T>.PartitionFunction partitionFunction;
        private readonly List<Dictionary<T, Dictionary<int, int>>> artifactIndexRemappings;
        private readonly Dictionary<T, Dictionary<int, int>> runIndexMappings;

        private int currentRunIndex;
        private T currentPartitionValue;

        internal PartitionedIndexRemappingVisitor(
            PartitioningVisitor<T>.PartitionFunction partitionFunction,
            List<Dictionary<T, Dictionary<int, int>>> artifactIndexRemappings,
            Dictionary<T, Dictionary<int, int>> runIndexMappings)
        {
            this.partitionFunction = partitionFunction;
            this.artifactIndexRemappings = artifactIndexRemappings;
            this.runIndexMappings = runIndexMappings;
        }

        public override SarifLog VisitSarifLog(SarifLog node)
        {
            currentRunIndex = -1;
            return base.VisitSarifLog(node);
        }

        public override Run VisitRun(Run node)
        {
            ++currentRunIndex;
            return base.VisitRun(node);
        }

        public override Result VisitResult(Result node)
        {
            currentPartitionValue = partitionFunction(node);
            return base.VisitResult(node);
        }

        public override ArtifactLocation VisitArtifactLocation(ArtifactLocation node)
        {
            if (currentPartitionValue != null // TODO: This is why we can't remap indices for non-result locations. We do need to fix this, either by handling those locations or by blanking out their indices.
                && runIndexMappings.TryGetValue(currentPartitionValue, out Dictionary<int, int> runIndexMappingForPartition)
                && runIndexMappingForPartition.TryGetValue(currentRunIndex, out int originalRunIndex))
            {
                if (node.Index >= 0
                    && artifactIndexRemappings[originalRunIndex].TryGetValue(currentPartitionValue, out Dictionary<int, int> artifactIndexRemapping))
                {
                    if (artifactIndexRemapping.ContainsKey(node.Index))
                    {
                        node.Index = artifactIndexRemapping[node.Index];
                    }
                }
            }

            return base.VisitArtifactLocation(node);
        }
    }
}