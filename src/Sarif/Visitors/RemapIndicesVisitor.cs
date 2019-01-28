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
        public RemapIndicesVisitor(IList<FileData> currentFiles)
        {
            BuildRemappedFiles(currentFiles);
            RemappedLogicalLocationIndices = new Dictionary<LogicalLocation, int>(LogicalLocation.ValueComparer);
        }

        private void BuildRemappedFiles(IList<FileData> currentFiles)
        {
            RemappedFiles = new Dictionary<OrderSensitiveValueComparisonList<FileData>, int>();

            if (currentFiles != null)
            {
                foreach (FileData fileData in currentFiles)
                {
                    CacheFileData(fileData);
                }
            }
        }

        public IList<FileData> CurrentFiles { get; set; }

        public IList<FileData> HistoricalFiles { get; set; }

        public IList<LogicalLocation> HistoricalLogicalLocations { get; set; }

        public IDictionary<LogicalLocation, int> RemappedLogicalLocationIndices { get; private set; }

        public IDictionary<OrderSensitiveValueComparisonList<FileData>, int> RemappedFiles { get; private set; }

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
            if (node.LogicalLocationIndex != -1 && HistoricalLogicalLocations != null)
            {
                LogicalLocation logicalLocation = HistoricalLogicalLocations[node.LogicalLocationIndex];

                if (!RemappedLogicalLocationIndices.TryGetValue(logicalLocation, out int remappedIndex))
                {
                    remappedIndex = RemappedLogicalLocationIndices.Count;
                    RemappedLogicalLocationIndices[logicalLocation] = remappedIndex;
                }

                node.LogicalLocationIndex = remappedIndex;
            }
            return base.VisitLocation(node);
        }

        public override FileData VisitFileData(FileData node)
        {
            return base.VisitFileData(node);
        }

        public override FileLocation VisitFileLocation(FileLocation node)
        {
            if (node.FileIndex != -1 && HistoricalFiles != null)
            {
                node.FileIndex = CacheFileData(HistoricalFiles[node.FileIndex]);
            }
            return node;
        }
        
        private int CacheFileData(FileData fileData)
        {
            this.CurrentFiles = this.CurrentFiles ?? new List<FileData>();

            int parentIndex = fileData.ParentIndex;

            // Ensure all parent nodes are already remapped
            if (parentIndex != -1)
            {
                // Important: the input results we are rewriting need to refer
                // to the historical files index in order to understand parenting
                fileData.ParentIndex = CacheFileData(HistoricalFiles[parentIndex]);
            }

            OrderSensitiveValueComparisonList<FileData> fileChain;

            // Equally important, the file chain is a specially constructed key that
            // operates against the newly constructed files array in CurrentFiles
            fileChain = ConstructFilesChain(CurrentFiles, fileData);

            if (!RemappedFiles.TryGetValue(fileChain, out int remappedIndex))
            {
                remappedIndex = RemappedFiles.Count;

                this.CurrentFiles.Add(fileData);
                RemappedFiles[fileChain] = remappedIndex;

                Debug.Assert(RemappedFiles.Count == this.CurrentFiles.Count);

                if (fileData.FileLocation != null)
                {
                    fileData.FileLocation.FileIndex = remappedIndex;
                }
            }
            return remappedIndex;
        }

        private static OrderSensitiveValueComparisonList<FileData> ConstructFilesChain(IList<FileData> existingFiles, FileData currentFile)
        {
            var fileChain = new OrderSensitiveValueComparisonList<FileData>(FileData.ValueComparer);

            int parentIndex;

            do
            {
                currentFile = currentFile.DeepClone();
                parentIndex = currentFile.ParentIndex;

                // Index information is entirely irrelevant for the identity of nested files,
                // as each element of the chain could be stored at arbitrary locations in
                // the run.files table. And so we elide this information.
                currentFile.ParentIndex = -1;
                if (currentFile.FileLocation != null)
                {
                    currentFile.FileLocation.FileIndex = -1;
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
