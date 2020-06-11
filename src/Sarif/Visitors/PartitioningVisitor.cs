// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
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
    public delegate T PartitionFunction<T>(Result result) where T : class, IEquatable<T>;

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
    public class PartitioningVisitor<T> : SarifRewritingVisitor where T : class, IEquatable<T>
    {
        // The name of a property in the property bag of each partition log which contains the
        // partition value for the results in that log file.
        private const string PartitionValuePropertyName = "partitionValue";

        // The partition function being used to partition the original log.
        private readonly PartitionFunction<T> partitionFunction;

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

        // True if we are currently processing a result, otherwise false.
        private bool inResult;

        // Information accumulated from each run in the original log file that is used to create
        // the runs in the partition log files.
        private List<PartitionRunInfo> partitionRunInfos;

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
        public PartitioningVisitor(PartitionFunction<T> partitionFunction, bool deepClone)
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
                    $"You must call {nameof(VisitSarifLog)} before you call {nameof(GetPartitionLogs)}.");
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
            currentPartitionValue = null;
            inResult = false;
            partitionValues = new HashSet<T>();
            partitionRunInfos = new List<PartitionRunInfo>();
            partitionLogDictionary = null;

            return base.VisitSarifLog(node);
        }

        public override Run VisitRun(Run node)
        {
            if (originalLog == null)
            {
                // Could not identify the log being partitioned. Call VisitSarifLog and
                // provide the log to partition. This class is designed to create log files
                // on a per-run basis (i.e., all partioned logs will contain a single run only).
                throw new InvalidOperationException(SdkResources.PartioningVisitHappensAtSarifLogLevel);
            };

            ++currentRunIndex;
            currentRunArtifacts = originalLog.Runs[currentRunIndex].Artifacts;
            partitionRunInfos.Add(new PartitionRunInfo());

            return base.VisitRun(node);
        }

        public override Result VisitResult(Result node)
        {
            inResult = true;
            currentPartitionValue = partitionFunction(node);
            if (currentPartitionValue != null)
            {
                partitionValues.Add(currentPartitionValue);

                if (!partitionRunInfos[currentRunIndex].ResultDictionary.ContainsKey(currentPartitionValue))
                {
                    partitionRunInfos[currentRunIndex].ResultDictionary.Add(currentPartitionValue, new List<Result>());
                }

                Result partitionedResult = deepClone
                    ? node.DeepClone()
                    : node;

                partitionRunInfos[currentRunIndex].ResultDictionary[currentPartitionValue].Add(partitionedResult);
            }

            Result visitedResult = base.VisitResult(node);

            // Unset inResult and currentPartitionValue so that VisitArtifactLocation (and other
            // methods which accumulate run-level properties) knows whether the ArtifactLocation
            // occurs in the context of a result, and if so, whether the partitionFunction
            // returned non-null. The run-level properties will only contain elements that are
            // relevant to the results in a single partition, OR which are not associated with
            // a result at all.
            currentPartitionValue = null;
            inResult = false;

            return visitedResult;
        }

        public override ArtifactLocation VisitArtifactLocation(ArtifactLocation node)
        {
            HashSet<int> artifactIndices = null;

            // Does this run have any artifacts, and if so, does this ArtifactLocation specify a
            // valid index into those artifacts?
            if (currentRunArtifacts != null
                && node.Index >= 0
                && node.Index < currentRunArtifacts.Count)
            {
                // Does this artifactLocation occur in the context of a result...
                if (inResult)
                {
                    // If so, does this result belong to any partition?
                    if (currentPartitionValue != null)
                    {
                        // Remap this index if it isn't already remapped.
                        Dictionary<T, HashSet<int>> artifactIndicesDictionary = partitionRunInfos[currentRunIndex].ArtifactIndicesDictionary;
                        if (!artifactIndicesDictionary.ContainsKey(currentPartitionValue))
                        {
                            artifactIndicesDictionary.Add(currentPartitionValue, new HashSet<int>());
                        }

                        artifactIndices = artifactIndicesDictionary[currentPartitionValue];
                    }
                }
                // ... or outside of any result?
                else
                {
                    artifactIndices = partitionRunInfos[currentRunIndex].GlobalArtifactIndices;
                }

                // Either way, bring the artifact and all its ancestors into the appropriate
                // list of indices.
                if (artifactIndices != null)
                {
                    // It's safe to add the same item to a hash set multiple times, but
                    // we check whether we've already seen this item anyway to avoid walking
                    // the ancestor chain if we don't need to.
                    if (!artifactIndices.Contains(node.Index))
                    {
                        artifactIndices.Add(node.Index);

                        Artifact artifact = currentRunArtifacts[node.Index];
                        while (artifact.ParentIndex >= 0)
                        {
                            artifactIndices.Add(artifact.ParentIndex);
                            artifact = currentRunArtifacts[artifact.ParentIndex];
                        }
                    }
                }
            }

            return base.VisitArtifactLocation(node);
        }

        private void CreatePartitionLogDictionary()
        {
            partitionLogDictionary = partitionValues.ToDictionary(
                keySelector: pv => pv,
                elementSelector: CreatePartitionLog);
        }

        private SarifLog CreatePartitionLog(T partitionValue)
        {
            SarifLog partitionLog;
            if (deepClone)
            {
                // Save time and space by not cloning the runs unless and until necessary. We will
                // only need to clone the runs that have results in this partition.
                IList<Run> originalRuns = originalLog.Runs;
                originalLog.Runs = null;

                partitionLog = originalLog.DeepClone();

                originalLog.Runs = originalRuns;
            }
            else
            {
                partitionLog = new SarifLog(originalLog.SchemaUri,
                    originalLog.Version,
                    runs: null,
                    originalLog.InlineExternalProperties,
                    originalLog.Properties);
            }

            partitionLog.Runs = new List<Run>();
            partitionLog.SetProperty(PartitionValuePropertyName, partitionValue);

            var artifactIndexRemappingDictionaries = new List<Dictionary<int, int>>();
            for (int iOriginalRun = 0; iOriginalRun < partitionRunInfos.Count; ++iOriginalRun)
            {
                // Are there results for this run in this partition?
                if (partitionRunInfos[iOriginalRun].ResultDictionary.TryGetValue(partitionValue, out List<Result> results))
                {
                    // Yes, so we'll need a copy of the original run in which the results, and
                    // certain run-level collections such as Run.Artifacts, have been replaced.
                    Run originalRun = originalLog.Runs[iOriginalRun];
                    Run partitionRun;
                    if (deepClone)
                    {
                        // Save time and space by only cloning the necessary results and associated
                        // collection elements. We already cloned the relevant results in VisitResult.
                        IList<Result> originalResults = originalRun.Results;
                        IList<Artifact> originalArtifacts = originalRun.Artifacts;
                        originalRun.Results = null;
                        originalRun.Artifacts = null;

                        partitionRun = originalRun.DeepClone();

                        originalRun.Results = originalResults;
                        originalRun.Artifacts = originalArtifacts;
                    }
                    else
                    {
                        partitionRun = new Run(originalRun.Tool,
                            originalRun.Invocations,
                            originalRun.Conversion,
                            originalRun.Language,
                            originalRun.VersionControlProvenance,
                            originalRun.OriginalUriBaseIds,
                            artifacts: null,
                            originalRun.LogicalLocations,
                            originalRun.Graphs,
                            results: null,
                            originalRun.AutomationDetails,
                            originalRun.RunAggregates,
                            originalRun.BaselineGuid,
                            originalRun.RedactionTokens,
                            originalRun.DefaultEncoding,
                            originalRun.DefaultSourceLanguage,
                            originalRun.NewlineSequences,
                            originalRun.ColumnKind,
                            originalRun.ExternalPropertyFileReferences,
                            originalRun.ThreadFlowLocations,
                            originalRun.Taxonomies,
                            originalRun.Addresses,
                            originalRun.Translations,
                            originalRun.Policies,
                            originalRun.WebRequests,
                            originalRun.WebResponses,
                            originalRun.SpecialLocations,
                            originalRun.Properties);
                    }

                    partitionRun.Results = results;

                    // Construct a mapping from the indices in the original run to the indices
                    // in the partition run. This includes both the indices relevant to the
                    // results in this partition, and indices that appear in all partitions
                    // because they are mentioned outside of any result (we refer to these as
                    // "global" indices).
                    IEnumerable<int> allPartitionArtifactIndices = partitionRunInfos[iOriginalRun].GlobalArtifactIndices;

                    if (partitionRunInfos[iOriginalRun].ArtifactIndicesDictionary.TryGetValue(partitionValue, out HashSet<int> partitionResultArtifactIndices))
                    {
                        allPartitionArtifactIndices = allPartitionArtifactIndices.Union(partitionResultArtifactIndices);
                    }

                    var partitionArtifactIndexRemappingDictionary = new Dictionary<int, int>();
                    artifactIndexRemappingDictionaries.Add(partitionArtifactIndexRemappingDictionary);

                    List<int> allPartitionArtifactIndicesList = allPartitionArtifactIndices
                        .OrderBy(index => index)
                        .ToList();

                    int numPartitionIndices = 0;
                    foreach (int originalIndex in allPartitionArtifactIndicesList)
                    {
                        partitionArtifactIndexRemappingDictionary.Add(originalIndex, numPartitionIndices++);
                    }

                    // Copy the artifacts corresponding to the complete set of indices.
                    var artifacts = new List<Artifact>();
                    foreach (int originalIndex in allPartitionArtifactIndicesList)
                    {
                        Artifact originalArtifact = originalRun.Artifacts[originalIndex];
                        Artifact partitionArtifact = deepClone
                            ? originalArtifact.DeepClone()
                            : new Artifact(originalArtifact);

                        artifacts.Add(partitionArtifact);
                    }

                    if (artifacts.Any())
                    {
                        partitionRun.Artifacts = artifacts;
                    }

                    partitionLog.Runs.Add(partitionRun);
                }
            }

            // Traverse the entire log, fixing the index mappings for indices that appear
            // in the remapping dictionaries.
            var remappingVisitor = new PartitionedIndexRemappingVisitor(artifactIndexRemappingDictionaries);

            remappingVisitor.VisitSarifLog(partitionLog);

            return partitionLog;
        }

        // This class contains information accumulated from an individual run in the
        // original log file that is used to create the runs in the partition log files.
        private class PartitionRunInfo
        {
            // The set of results in each partition in the original run. That is,
            //
            //     ResultDictionary[partitionValue]
            //
            // contains the list of results in the original run whose partition value is
            // partitionValue.
            public Dictionary<T, List<Result>> ResultDictionary { get; }

            // The sets of artifact indices relevant to the results in each partition of the
            // original run. That is,
            //
            //    ArtifactIndicesDictionary[partitionValue]
            //
            // contains a list of the indices within the original run of the artifacts that are
            // relevant to the results whose partition value is partitionValue.
            public Dictionary<T, HashSet<int>> ArtifactIndicesDictionary { get; }

            // The set of artifact indices that are relevant to the original run and appear outside
            // of any result.
            public HashSet<int> GlobalArtifactIndices { get; }

            public PartitionRunInfo()
            {
                ResultDictionary = new Dictionary<T, List<Result>>();
                ArtifactIndicesDictionary = new Dictionary<T, HashSet<int>>();
                GlobalArtifactIndices = new HashSet<int>();
            }
        }

        // This class adjust the indices into the run-level property arrays in a single
        // parition log.
        private class PartitionedIndexRemappingVisitor : SarifRewritingVisitor
        {
            private readonly List<Dictionary<int, int>> artifactIndexRemappingDictionaries;

            private Run currentRun;
            private int currentRunIndex;

            internal PartitionedIndexRemappingVisitor(
                List<Dictionary<int, int>> artifactIndexRemappingDictionaries)
            {
                this.artifactIndexRemappingDictionaries = artifactIndexRemappingDictionaries;
            }

            public override SarifLog VisitSarifLog(SarifLog node)
            {
                currentRun = null;
                currentRunIndex = -1;

                return base.VisitSarifLog(node);
            }

            public override Run VisitRun(Run node)
            {
                currentRun = node;
                ++currentRunIndex;

                return base.VisitRun(node);
            }

            public override ArtifactLocation VisitArtifactLocation(ArtifactLocation node)
            {
                if (node.Index >= 0 && currentRun.Artifacts?.Any() == true)
                {
                    Dictionary<int, int> artifactIndexRemappingDictionary = artifactIndexRemappingDictionaries[currentRunIndex];

                    if (artifactIndexRemappingDictionary.ContainsKey(node.Index))
                    {
                        node.Index = artifactIndexRemappingDictionary[node.Index];
                    }
                }

                return base.VisitArtifactLocation(node);
            }

            public override Artifact VisitArtifact(Artifact node)
            {
                if (node.ParentIndex >= 0)
                {
                    Dictionary<int, int> artifactIndexRemappingDictionary = artifactIndexRemappingDictionaries[currentRunIndex];

                    if (artifactIndexRemappingDictionary.ContainsKey(node.ParentIndex))
                    {
                        node.ParentIndex = artifactIndexRemappingDictionary[node.ParentIndex];
                    }
                }

                return base.VisitArtifact(node);
            }
        }
    }
}