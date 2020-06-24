// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Visitors;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Run
    {
        private static readonly Graph EmptyGraph = new Graph();
        private static readonly Artifact EmptyFile = new Artifact();
        private static readonly Invocation EmptyInvocation = new Invocation();
        private static readonly LogicalLocation EmptyLogicalLocation = new LogicalLocation();

        private IDictionary<ArtifactLocation, int> _fileToIndexMap;

        public Uri ExpandUrisWithUriBaseId(string key, string currentValue = null)
        {
            ArtifactLocation fileLocation = this.OriginalUriBaseIds[key];

            if (fileLocation.UriBaseId == null)
            {
                return fileLocation.Uri;
            }
            throw new InvalidOperationException("Author this code along with tests for originalUriBaseIds that are nested");
        }

        public int GetFileIndex(
            ArtifactLocation fileLocation,
            bool addToFilesTableIfNotPresent = true,
            OptionallyEmittedData dataToInsert = OptionallyEmittedData.None,
            Encoding encoding = null,
            HashData hashData = null)
        {
            if (fileLocation == null) { throw new ArgumentNullException(nameof(fileLocation)); }

            if (this.Artifacts == null || this.Artifacts.Count == 0)
            {
                if (!addToFilesTableIfNotPresent)
                {
                    return -1;
                }
            }

            if (_fileToIndexMap == null)
            {
                InitializeFileToIndexMap();
            }

            if (fileLocation.Uri == null)
            {
                // We only have a file index, so just return it.
                return fileLocation.Index;
            }

            // Strictly speaking, some elements that may contribute to a files table 
            // key are case sensitive, e.g., everything but the schema and protocol of a
            // web URI. We don't have a proper comparer implementation that can handle 
            // all cases. For now, we cover the Windows happy path, which assumes that
            // most URIs in log files are file paths (which are case-insensitive)
            //
            // Tracking item for an improved comparer:
            // https://github.com/Microsoft/sarif-sdk/issues/973

            // When we perform a files table look-up, only the uri and uriBaseId
            // are relevant; these properties together comprise the unique identity
            // of the file object. The file index, of course, does not relate to the
            // file identity. We consciously exclude the properties bag as well.

            // We will normalize the input fileLocation.Uri to make URIs more consistent
            // throughout the emitted log.
            fileLocation.Uri = new Uri(UriHelper.MakeValidUri(fileLocation.Uri.OriginalString), UriKind.RelativeOrAbsolute);

            var filesTableKey = new ArtifactLocation
            {
                Uri = fileLocation.Uri,
                UriBaseId = fileLocation.UriBaseId
            };

            if (!_fileToIndexMap.TryGetValue(filesTableKey, out int fileIndex))
            {
                if (addToFilesTableIfNotPresent)
                {
                    this.Artifacts = this.Artifacts ?? new List<Artifact>();
                    fileIndex = this.Artifacts.Count;

                    var fileData = Artifact.Create(
                        filesTableKey.Uri,
                        dataToInsert,
                        hashData: hashData,
                        encoding: null);

                    // Copy ArtifactLocation to ensure changes to Result copy don't affect new Run.Artifacts copy
                    fileData.Location = new ArtifactLocation(fileLocation);

                    this.Artifacts.Add(fileData);

                    _fileToIndexMap[filesTableKey] = fileIndex;
                }
                else
                {
                    // We did not find the item. The call was not configured to add the entry.
                    // Return the default value that indicates the item isn't present.
                    fileIndex = -1;
                }
            }

            fileLocation.Index = fileIndex;
            return fileIndex;
        }

        private void InitializeFileToIndexMap()
        {
            _fileToIndexMap = new Dictionary<ArtifactLocation, int>(ArtifactLocation.ValueComparer);

            // First, we'll initialize our file object to index map
            // with any files that already exist in the table
            for (int i = 0; i < this.Artifacts?.Count; i++)
            {
                Artifact fileData = this.Artifacts[i];

                var fileLocation = new ArtifactLocation
                {
                    Uri = fileData.Location?.Uri,
                    UriBaseId = fileData.Location?.UriBaseId,
                };

                _fileToIndexMap[fileLocation] = i;
            }
        }

        /// <summary>
        ///  Find the ToolComponent corresponding to a ToolComponentReference.
        /// </summary>
        /// <param name="reference">ToolComponentReference to resolve</param>
        /// <returns>ToolComponent for reference</returns>
        public ToolComponent GetToolComponentFromReference(ToolComponentReference reference)
        {
            return this.Tool?.GetToolComponentFromReference(reference);
        }

        /// <summary>
        ///  Set the Run property on each Result to this Run, so that Result methods
        ///  and properties which may need to look up Run collections can do so.
        /// </summary>
        public void SetRunOnResults()
        {
            // Nothing: BSOA getter will handle
        }

        public void MergeResultsFrom(Run additional)
        {
            // Merge Results from the two Runs, building shared collections of result-referenced things
            var visitor = new RunMergingVisitor();

            visitor.VisitRun(this);
            visitor.VisitRun(additional);

            visitor.PopulateWithMerged(this);
        }
    }
}
