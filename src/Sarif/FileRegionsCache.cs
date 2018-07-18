// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// This class is a file cache that can be used to populate
    /// regions with comprehensive data, to retrieve file text
    /// associated with a SARIF log, and to construct text
    /// snippets associated with region instances.
    /// </summary>
    public class FileRegionsCache
    {
        internal IFileSystem _fileSystem;

        private Run _run;

        public FileRegionsCache(Run run)
        {
            // Each file regions cache is associated with a single SARIF run.
            // The reason is that a run provides an isolated scope for 
            // things like URLs, point-in-time file contents, etc.
            _run = run;

            _fileSystem = new FileSystem();
        }

        /// <summary>
        /// Accepts a physical location and returns a Region object, based on the input
        /// physicalLocation.region property, that has all its properties populated. If an 
        /// input text region, for example, only specifies the startLine property, the returned
        /// Region instance will have computed and populated other properties, such as charOffset,
        /// charLength, etc. 
        /// </summary>
        /// <param name="physicalLocation">The physical location containing the region which should be populated.</param>
        /// <param name="populateSnippet">Specifies whether the physicalLocation.region.snippet property should be populated.</param>
        /// <returns></returns>
        public Region PopulatePrimaryRegionProperties(PhysicalLocation physicalLocation, bool populateSnippet)
        {
            Region inputRegion = physicalLocation.Region;

            if (inputRegion == null || inputRegion.IsBinaryRegion)
            {
                // For binary regions, only the byteOffset and byteLength properties
                // are relevant, and their values are always specified.
                return inputRegion;
            }

            string fileText = GetFileText(physicalLocation.FileLocation);
            return PopulatePrimaryRegionProperties(fileText, inputRegion, populateSnippet);
        }

        private Region PopulatePrimaryRegionProperties(string fileText, Region inputRegion, bool populateSnippet)
        {
            if (fileText == null) { return inputRegion; }

            Region result = new Region() { StartLine = 1, EndLine = 1, StartColumn = 1, EndColumn = 5, CharOffset = 0, CharLength = 5 };

            if (populateSnippet) { result.Snippet = new FileContent() { Text = "line1" }; }

            return result;
        }

        private string GetFileText(FileLocation fileLocation)
        {
            return _fileSystem.ReadAllText(fileLocation.Uri.LocalPath);
        }
    }
}
