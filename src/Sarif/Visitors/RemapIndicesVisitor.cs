// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
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
        public RemapIndicesVisitor(IList<Artifact> currentFiles)
        {
            BuildRemappedFiles(currentFiles);
            RemappedLogicalLocationIndices = new Dictionary<LogicalLocation, int>(LogicalLocation.ValueComparer);
        }

        private void BuildRemappedFiles(IList<Artifact> currentFiles)
        {
            RemappedFiles = new Dictionary<OrderSensitiveValueComparisonList<Artifact>, int>();

            if (currentFiles != null)
            {
                foreach (Artifact fileData in currentFiles)
                {
                    CacheFileData(fileData);
                }
            }
        }

        public IList<Artifact> CurrentFiles { get; set; }

        public IList<Artifact> HistoricalFiles { get; set; }

        public IList<LogicalLocation> HistoricalLogicalLocations { get; set; }

        public IDictionary<LogicalLocation, int> RemappedLogicalLocationIndices { get; private set; }

        public IDictionary<OrderSensitiveValueComparisonList<Artifact>, int> RemappedFiles { get; private set; }

        public override Result VisitResult(Result node)
        {
            // We suspend the visit if there's insufficient data to perform any work
            if (HistoricalFiles == null && HistoricalLogicalLocations == null)
            {
                return node;
            }

            return base.VisitResult(node);
        }

        public override Location VisitLocation(Location node)
        {
            if (node.LogicalLocation != null && node.LogicalLocation.Index != -1 && HistoricalLogicalLocations != null)
            {
                LogicalLocation logicalLocation = HistoricalLogicalLocations[node.LogicalLocation.Index];

                if (!RemappedLogicalLocationIndices.TryGetValue(logicalLocation, out int remappedIndex))
                {
                    remappedIndex = RemappedLogicalLocationIndices.Count;
                    RemappedLogicalLocationIndices[logicalLocation] = remappedIndex;
                }

                node.LogicalLocation.Index = remappedIndex;
            }
            return base.VisitLocation(node);
        }

        public override Artifact VisitArtifact(Artifact node)
        {
            return base.VisitArtifact(node);
        }

        public override ArtifactLocation VisitArtifactLocation(ArtifactLocation node)
        {
            if (node.Index != -1 && HistoricalFiles != null)
            {
                node.Index = CacheFileData(HistoricalFiles[node.Index]);
            }
            return node;
        }
        
        private int CacheFileData(Artifact fileData)
        {
            this.CurrentFiles = this.CurrentFiles ?? new List<Artifact>();

            int parentIndex = fileData.ParentIndex;

            // Ensure all parent nodes are already remapped
            if (parentIndex != -1)
            {
                // Important: the input results we are rewriting need to refer
                // to the historical files index in order to understand parenting
                fileData.ParentIndex = CacheFileData(HistoricalFiles[parentIndex]);
            }

            OrderSensitiveValueComparisonList<Artifact> fileChain;

            // Equally important, the file chain is a specially constructed key that
            // operates against the newly constructed files array in CurrentFiles
            fileChain = ConstructFilesChain(CurrentFiles, fileData);

            if (!RemappedFiles.TryGetValue(fileChain, out int remappedIndex))
            {
                remappedIndex = RemappedFiles.Count;

                this.CurrentFiles.Add(fileData);
                RemappedFiles[fileChain] = remappedIndex;

                Debug.Assert(RemappedFiles.Count == this.CurrentFiles.Count);

                if (fileData.Location != null)
                {
                    fileData.Location.Index = remappedIndex;
                }
            }
            return remappedIndex;
        }

        private static OrderSensitiveValueComparisonList<Artifact> ConstructFilesChain(IList<Artifact> existingFiles, Artifact currentFile)
        {
            var fileChain = new OrderSensitiveValueComparisonList<Artifact>(Artifact.ValueComparer);

            int parentIndex;

            do
            {
                currentFile = currentFile.DeepClone();
                parentIndex = currentFile.ParentIndex;

                // Index information is entirely irrelevant for the identity of nested files,
                // as each element of the chain could be stored at arbitrary locations in
                // the run.files table. And so we elide this information.
                currentFile.ParentIndex = -1;
                if (currentFile.Location != null)
                {
                    currentFile.Location.Index = -1;
                }

                fileChain.Add(currentFile);

                if (parentIndex != -1)
                {
                    currentFile = existingFiles[parentIndex];
                }

            } while (parentIndex != -1);

            return fileChain;
        }
    }
}
