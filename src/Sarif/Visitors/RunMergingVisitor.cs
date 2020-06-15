// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    /// <summary>
    ///  RunMergingVisitor merges Results from multiple Runs. 
    ///  It gathers things referenced by the Results, remaps indices in the Results to the merged collections,
    ///  and then populates the desired run with the merged sets.
    ///  
    ///  Used by Baselining and Run.MergeResultFrom(Run).
    /// </summary>
    /// <remarks>
    ///  Usage:
    ///   var visitor = new RunMergingVisitor();
    ///   
    ///   visitor.VisitRun(primary);
    ///   visitor.VisitRun(additional);
    ///   
    ///   visitor.PopulateMergedRun(primary);
    /// </remarks>
    public class RunMergingVisitor : SarifRewritingVisitor
    {
        private List<Result> Results { get; }
        private List<Artifact> Artifacts { get; }
        private List<LogicalLocation> LogicalLocations { get; }
        private List<ReportingDescriptor> Rules { get; }

        private Dictionary<string, int> RuleIdToIndex { get; }
        private Dictionary<OrderSensitiveValueComparisonList<LogicalLocation>, int> LogicalLocationToIndex { get; }
        private Dictionary<OrderSensitiveValueComparisonList<Artifact>, int> ArtifactToIndex { get; }

        public Run CurrentRun { get; set; }

        public RunMergingVisitor()
        {
            Results = new List<Result>();
            Artifacts = new List<Artifact>();
            LogicalLocations = new List<LogicalLocation>();
            Rules = new List<ReportingDescriptor>();

            RuleIdToIndex = new Dictionary<string, int>();
            LogicalLocationToIndex = new Dictionary<OrderSensitiveValueComparisonList<LogicalLocation>, int>();
            ArtifactToIndex = new Dictionary<OrderSensitiveValueComparisonList<Artifact>, int>();
        }

        public void PopulateWithMerged(Run targetRun)
        {
            targetRun.Results = Results;
            targetRun.Artifacts = Artifacts;
            targetRun.LogicalLocations = LogicalLocations;

            targetRun.Tool ??= new Tool();
            targetRun.Tool.Driver ??= new ToolComponent();
            targetRun.Tool.Driver.Rules = Rules;
        }

        public override Run VisitRun(Run node)
        {
            // Set CurrentRun, Visit Results, Clear Current Run
            CurrentRun = node;
            Run result = base.VisitRun(node);
            CurrentRun = null;

            return result;
        }

        public override Result VisitResult(Result node)
        {
            // We suspend the visit if there's insufficient data to perform any work
            if (CurrentRun == null)
            {
                throw new InvalidOperationException($"RemapIndicesVisitor requires CurrentRun to be set before Visiting Results.");
            }

            // Cache RuleId and set Result.RuleIndex to the (new) index
            string ruleId = node.ResolvedRuleId(CurrentRun);
            if (!string.IsNullOrEmpty(ruleId))
            {
                if (RuleIdToIndex.TryGetValue(ruleId, out int ruleIndex))
                {
                    node.RuleIndex = ruleIndex;
                }
                else
                {
                    ReportingDescriptor rule = node.GetRule(CurrentRun);
                    int newIndex = Rules.Count;
                    Rules.Add(rule);

                    RuleIdToIndex[ruleId] = newIndex;
                    node.RuleIndex = newIndex;
                }

                node.RuleId = ruleId;
            }

            Result result = base.VisitResult(node);
            Results.Add(result);
            return result;
        }

        public override LogicalLocation VisitLogicalLocation(LogicalLocation node)
        {
            if (node.Index != -1 && CurrentRun.LogicalLocations != null)
            {
                node.Index = CacheLogicalLocation(CurrentRun.LogicalLocations[node.Index], CurrentRun.LogicalLocations);
            }

            return base.VisitLogicalLocation(node);
        }

        private int CacheLogicalLocation(LogicalLocation logicalLocation, IList<LogicalLocation> currentLogicalLocations)
        {
            logicalLocation = logicalLocation.DeepClone();

            // Ensure all parent nodes are remapped.
            int parentIndex = logicalLocation.ParentIndex;
            if (parentIndex != -1)
            {
                logicalLocation.ParentIndex = CacheLogicalLocation(currentLogicalLocations[parentIndex], currentLogicalLocations);
            }

            OrderSensitiveValueComparisonList<LogicalLocation> logicalLocationChain = ConstructLogicalLocationsChain(logicalLocation, LogicalLocations);

            if (!LogicalLocationToIndex.TryGetValue(logicalLocationChain, out int remappedIndex))
            {
                remappedIndex = LogicalLocationToIndex.Count;
                logicalLocation.Index = remappedIndex;

                this.LogicalLocations.Add(logicalLocation);
                LogicalLocationToIndex[logicalLocationChain] = remappedIndex;
            }

            return remappedIndex;
        }

        public override ArtifactLocation VisitArtifactLocation(ArtifactLocation node)
        {
            if (node.Index != -1 && CurrentRun.Artifacts != null)
            {
                node.Index = CacheArtifact(CurrentRun.Artifacts[node.Index], CurrentRun.Artifacts);
            }

            return base.VisitArtifactLocation(node);
        }

        private int CacheArtifact(Artifact artifact, IList<Artifact> currentArtifacts)
        {
            artifact = artifact.DeepClone();

            int parentIndex = artifact.ParentIndex;

            // Ensure all parent nodes are remapped.
            if (parentIndex != -1)
            {
                // Important: the input results we are rewriting need to refer
                // to the historical files index in order to understand parenting.
                artifact.ParentIndex = CacheArtifact(currentArtifacts[parentIndex], currentArtifacts);
            }

            // Equally important, the artifact chain is a specially constructed key that
            // operates against the newly constructed files array in CurrentArtifacts.
            OrderSensitiveValueComparisonList<Artifact> artifactChain = ConstructArtifactsChain(artifact, Artifacts);

            if (!ArtifactToIndex.TryGetValue(artifactChain, out int remappedIndex))
            {
                remappedIndex = ArtifactToIndex.Count;

                this.Artifacts.Add(artifact);
                ArtifactToIndex[artifactChain] = remappedIndex;

                Debug.Assert(ArtifactToIndex.Count == this.Artifacts.Count);

                if (artifact.Location != null)
                {
                    artifact.Location.Index = remappedIndex;
                }
            }

            return remappedIndex;
        }

        private static OrderSensitiveValueComparisonList<Artifact> ConstructArtifactsChain(Artifact currentArtifact, IList<Artifact> existingArtifacts)
        {
            var artifactChain = new OrderSensitiveValueComparisonList<Artifact>(Artifact.ValueComparer);

            int parentIndex;

            do
            {
                currentArtifact = currentArtifact.DeepClone();
                parentIndex = currentArtifact.ParentIndex;

                // Index information is entirely irrelevant for the identity of nested artifacts,
                // as each element of the chain could be stored at arbitrary locations in
                // the run.artifacts table. And so we elide this information.
                currentArtifact.ParentIndex = -1;
                if (currentArtifact.Location != null)
                {
                    currentArtifact.Location.Index = -1;
                }

                artifactChain.Add(currentArtifact);

                if (parentIndex != -1)
                {
                    currentArtifact = existingArtifacts[parentIndex];
                }

            } while (parentIndex != -1);

            return artifactChain;
        }

        private static OrderSensitiveValueComparisonList<LogicalLocation> ConstructLogicalLocationsChain(
            LogicalLocation currentLogicalLocation,
            IList<LogicalLocation> existingLogicalLocations)
        {
            var logicalLocationChain = new OrderSensitiveValueComparisonList<LogicalLocation>(LogicalLocation.ValueComparer);

            int parentIndex;

            do
            {
                currentLogicalLocation = currentLogicalLocation.DeepClone();
                parentIndex = currentLogicalLocation.ParentIndex;

                // Index information is entirely irrelevant for the identity of nested logical locations,
                // as each element of the chain could be stored at arbitrary locations in the
                // run.logicalLocations table. And so we elide this information.
                currentLogicalLocation.ParentIndex = -1;
                currentLogicalLocation.Index = -1;

                logicalLocationChain.Add(currentLogicalLocation);

                if (parentIndex != -1)
                {
                    currentLogicalLocation = existingLogicalLocations[parentIndex];
                }

            } while (parentIndex != -1);

            return logicalLocationChain;
        }
    }
}
