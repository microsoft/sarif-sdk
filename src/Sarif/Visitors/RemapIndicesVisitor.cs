// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    /// <summary>
    /// This class is used to update indices for data that is merged for various
    /// reasons, including result matching operations.
    /// </summary>
    public class RemapIndicesVisitor : SarifRewritingVisitor
    {
        public RemapIndicesVisitor(
            IList<Artifact> currentArtifacts,
            IList<LogicalLocation> currentLogicalLocations)
        {
            CurrentArtifacts = new List<Artifact>();
            CurrentLogicalLocations = new List<LogicalLocation>();

            RemapCurrentArtifacts(currentArtifacts);
            RemapCurrentLogicalLocations(currentLogicalLocations);
        }

        private void RemapCurrentArtifacts(IList<Artifact> currentArtifacts)
        {
            RemappedArtifacts = new Dictionary<OrderSensitiveValueComparisonList<Artifact>, int>();

            if (currentArtifacts != null)
            {
                foreach (Artifact artifact in currentArtifacts)
                {
                    CacheArtifact(artifact, currentArtifacts);
                }
            }
        }

        private void RemapCurrentLogicalLocations(IList<LogicalLocation> currentLogicalLocations)
        {
            RemappedLogicalLocations = new Dictionary<OrderSensitiveValueComparisonList<LogicalLocation>, int>();

            if (currentLogicalLocations != null)
            {
                foreach (LogicalLocation logicalLocation in currentLogicalLocations)
                {
                    CacheLogicalLocation(logicalLocation, currentLogicalLocations);
                }
            }
        }

        public IList<Artifact> CurrentArtifacts { get; set; }

        public IList<LogicalLocation> CurrentLogicalLocations { get; set; }

        public IList<Artifact> HistoricalFiles { get; set; }

        public IList<LogicalLocation> HistoricalLogicalLocations { get; set; }

        public IDictionary<OrderSensitiveValueComparisonList<LogicalLocation>, int> RemappedLogicalLocations { get; private set; }

        public IDictionary<OrderSensitiveValueComparisonList<Artifact>, int> RemappedArtifacts { get; private set; }

        public override Result VisitResult(Result node)
        {
            // We suspend the visit if there's insufficient data to perform any work
            if (HistoricalFiles == null && HistoricalLogicalLocations == null)
            {
                return node;
            }

            return base.VisitResult(node);
        }

        public override LogicalLocation VisitLogicalLocation(LogicalLocation node)
        {
            if (node.Index != -1 && HistoricalLogicalLocations != null)
            {
                node.Index = CacheLogicalLocation(HistoricalLogicalLocations[node.Index], HistoricalLogicalLocations);
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

            OrderSensitiveValueComparisonList<LogicalLocation> logicalLocationChain = ConstructLogicalLocationsChain(logicalLocation, CurrentLogicalLocations);

            if (!RemappedLogicalLocations.TryGetValue(logicalLocationChain, out int remappedIndex))
            {
                remappedIndex = RemappedLogicalLocations.Count;
                logicalLocation.Index = remappedIndex;

                this.CurrentLogicalLocations.Add(logicalLocation);
                RemappedLogicalLocations[logicalLocationChain] = remappedIndex;
            }

            return remappedIndex;
        }

        public override ArtifactLocation VisitArtifactLocation(ArtifactLocation node)
        {
            if (node.Index != -1 && HistoricalFiles != null)
            {
                node.Index = CacheArtifact(HistoricalFiles[node.Index], HistoricalFiles);
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
            OrderSensitiveValueComparisonList<Artifact> artifactChain = ConstructArtifactsChain(artifact, CurrentArtifacts);

            if (!RemappedArtifacts.TryGetValue(artifactChain, out int remappedIndex))
            {
                remappedIndex = RemappedArtifacts.Count;

                this.CurrentArtifacts.Add(artifact);
                RemappedArtifacts[artifactChain] = remappedIndex;

                Debug.Assert(RemappedArtifacts.Count == this.CurrentArtifacts.Count);

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
