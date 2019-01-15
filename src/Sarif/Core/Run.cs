// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Run
    {
        private static Graph EmptyGraph = new Graph();
        private static FileData EmptyFile = new FileData();
        private static Invocation EmptyInvocation = new Invocation();
        private static LogicalLocation EmptyLogicalLocation = new LogicalLocation();

        private IDictionary<FileLocation, int> _fileToIndexMap;

        public Uri ExpandUrisWithUriBaseId(string key, string currentValue = null)
        {
            FileLocation fileLocation = this.OriginalUriBaseIds[key];

            if (fileLocation.UriBaseId == null)
            {
                return fileLocation.Uri;
            }
            throw new InvalidOperationException("Author this code along with tests for originalUriBaseIds that are nested");
        }

        public int GetFileIndex(
            FileLocation fileLocation,
            bool addToFilesTableIfNotPresent = true,
            OptionallyEmittedData dataToInsert = OptionallyEmittedData.None,
            Encoding encoding = null)
        {
            if (fileLocation == null) { throw new ArgumentNullException(nameof(fileLocation)); }

            if (this.Files?.Count == 0)
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

            var filesTableKey = new FileLocation
            {
                Uri = fileLocation.Uri,
                UriBaseId = fileLocation.UriBaseId
            };

            if (!_fileToIndexMap.TryGetValue(filesTableKey, out int fileIndex))
            {
                if (addToFilesTableIfNotPresent)
                {
                    this.Files = this.Files ?? new List<FileData>();
                    fileIndex = this.Files.Count;

                    string mimeType = Writers.MimeType.DetermineFromFileExtension(filesTableKey.Uri.ToString());

                    var fileData = FileData.Create(
                        filesTableKey.Uri,
                        dataToInsert,
                        mimeType: mimeType,
                        encoding);

                    fileData.FileLocation = fileLocation;

                    this.Files.Add(fileData);

                    _fileToIndexMap[filesTableKey] = fileIndex;
                }
                else
                {
                    // We did not find the item. The call was not configured to add the entry.
                    // Return the default value that indicates the item isn't present.
                    fileIndex = -1;
                }
            }

            fileLocation.FileIndex = fileIndex;
            return fileIndex;
        }

        private void InitializeFileToIndexMap()
        {
            _fileToIndexMap = new Dictionary<FileLocation, int>(FileLocation.ValueComparer);

            // First, we'll initialize our file object to index map
            // with any files that already exist in the table
            for (int i = 0; i < this.Files?.Count; i++)
            {
                FileData fileData = this.Files[i];

                var fileLocation = new FileLocation
                {
                    Uri = fileData.FileLocation.Uri,
                    UriBaseId = fileData.FileLocation.UriBaseId,
                };

                _fileToIndexMap[fileLocation] = i;
            }
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

        public bool ShouldSerializeFiles() { return this.Files?.Count > 0; }

        public bool ShouldSerializeGraphs() { return this.Graphs != null && this.Graphs.Values.Any(); }

        public bool ShouldSerializeInvocations() { return this.Invocations != null && this.Invocations.Any((e) => e != null && !e.ValueEquals(EmptyInvocation)); }

        public bool ShouldSerializeLogicalLocations() { return this.LogicalLocations?.Count > 0; }

        public bool ShouldSerializeNewlineSequences() { return this.NewlineSequences != null && this.NewlineSequences.Any((s) => s != null); }
    }
}
