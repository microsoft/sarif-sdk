// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Run
    {
        private static Graph EmptyGraph = new Graph();
        private static Artifact EmptyFile = new Artifact();
        private static Invocation EmptyInvocation = new Invocation();
        private static LogicalLocation EmptyLogicalLocation = new LogicalLocation();

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
            Encoding encoding = null)
        {
            if (fileLocation == null) { throw new ArgumentNullException(nameof(fileLocation)); }

            if (this.Artifacts?.Count == 0)
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

                    string mimeType = Writers.MimeType.DetermineFromFileExtension(filesTableKey.Uri.ToString());

                    var fileData = Artifact.Create(
                        filesTableKey.Uri,
                        dataToInsert,
                        mimeType: mimeType,
                        encoding);

                    fileData.Location = fileLocation;

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
            return this.Tool.GetToolComponentFromReference(reference);
        }

        public bool ShouldSerializeColumnKind()
        {
            // This serialization helper does two things. 
            // 
            // First, if ColumnKind has not been 
            // explicitly set, we will set it to the value that works for the Microsoft 
            // platform (which is not the specified SARIF default). This makes sure that
            // the value is set appropriate for code running on the Microsoft platform, 
            // even if the SARIF producer is not aware of this rather obscure value. 
            if (this.ColumnKind == ColumnKind.None)
            {
                this.ColumnKind = ColumnKind.Utf16CodeUnits;
            }

            // Second, we will always explicitly serialize this value. Otherwise, we can't easily
            // distinguish between earlier versions of the format for which this property was typically absent.
            return true;
        }

        public bool ShouldSerializeArtifacts() { return this.Artifacts.HasAtLeastOneNonDefaultValue(Artifact.ValueComparer); }

        public bool ShouldSerializeGraphs() { return this.Graphs.HasAtLeastOneNonDefaultValue(Graph.ValueComparer); }

        public bool ShouldSerializeInvocations() { return this.Invocations.HasAtLeastOneNonNullValue(); }

        public bool ShouldSerializeLogicalLocations() { return this.LogicalLocations.HasAtLeastOneNonDefaultValue(LogicalLocation.ValueComparer); }

        public bool ShouldSerializeNewlineSequences() { return this.NewlineSequences.HasAtLeastOneNonNullValue(); }
    }
}
