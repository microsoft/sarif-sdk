// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class ValueComparisonList<T> : List<T>, IEqualityComparer<List<T>>
    {
        private IEqualityComparer<T> _equalityComparer;

        public ValueComparisonList(IEqualityComparer<T> equalityComparer)
        {
            _equalityComparer = equalityComparer;            
        }

        public bool Equals(List<T> left, List<T> right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            if (left.Count != right.Count)
            {
                return false;
            }

            for (int i = 0; i < left.Count; i++)
            {
                if (!_equalityComparer.Equals(left[i], right[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(List<T> obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }

            int result = 17;
            unchecked
            {
                result = (result * 31) + obj.Count.GetHashCode();

                for (int i = 0; i < obj.Count; i++)
                {
                    result = (result * 31) + _equalityComparer.GetHashCode(obj[i]);
                }
            }
            return result;
        }
    }

    /// <summary>
    /// This class is used to update indices for data that is merged for various
    /// reasons, including result matching operations.
    /// </summary>
    public class RemapIndicesVisitor : SarifRewritingVisitor
    {
        public RemapIndicesVisitor(IList<FileData> currentFiles)
        {
            CurrentFiles = currentFiles;
            BuildRemappedFiles(CurrentFiles);
            RemappedLogicalLocationIndices = new Dictionary<LogicalLocation, int>(LogicalLocation.ValueComparer);
        }

        private void BuildRemappedFiles(IList<FileData> currentFiles)
        {
            RemappedFiles = new Dictionary<ValueComparisonList<FileData>, int>();

            foreach (FileData fileData in currentFiles)
            {
                ValueComparisonList<FileData> fileChain = ConstructFilesChain(fileData);

                if (!RemappedFiles.TryGetValue(fileChain, out int remappedIndex))
                {
                    remappedIndex = RemappedFiles.Count;
                    RemappedFiles[fileChain] = remappedIndex;
                }
            }
        }

        public IList<FileData> CurrentFiles { get; set; }

        public IList<FileData> HistoricalFiles { get; set; }

        public IList<LogicalLocation> HistoricalLogicalLocations { get; set; }

        public IDictionary<LogicalLocation, int> RemappedLogicalLocationIndices { get; private set; }

        public IDictionary<ValueComparisonList<FileData>, int> RemappedFiles { get; private set; }

        public override Result VisitResult(Result node)
        {
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
                FileData fileData = HistoricalFiles[node.FileIndex];

                ValueComparisonList<FileData> fileChain = ConstructFilesChain(fileData);

                if (!RemappedFiles.TryGetValue(fileChain, out int remappedIndex))
                {
                    remappedIndex = RemappedFiles.Count;
                    RemappedFiles[fileChain] = remappedIndex;
                }
                node.FileIndex = remappedIndex;
            }
            return node;
        }

        private ValueComparisonList<FileData> ConstructFilesChain(FileData fileData)
        {
            var fileChain = new ValueComparisonList<FileData>(FileData.ValueComparer);
            fileChain.Add(fileData);

            while (fileData.ParentIndex != -1)
            {
                fileData = HistoricalFiles[fileData.ParentIndex];

                // Scrub the indices. These are only relevant to the global array,
                // they are not relevant to the identity of a common chain of
                // file data instances.
                fileData.ParentIndex = -1;
                if (fileData.FileLocation != null)
                {
                    fileData.FileLocation.FileIndex = -1;
                }

                fileChain.Add(fileData);
            }

            return fileChain;
        }
    }
}
