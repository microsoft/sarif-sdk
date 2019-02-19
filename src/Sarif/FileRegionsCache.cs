// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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

        private readonly Run _run;
        private readonly Dictionary<string, NewLineIndex> _filePathToNewLineIndexMap;

        public FileRegionsCache(Run run)
        {
            // Each file regions cache is associated with a single SARIF run.
            // The reason is that a run provides an isolated scope for 
            // things like URLs, point-in-time file contents, etc.
            _run = run;

            _fileSystem = new FileSystem();
            _filePathToNewLineIndexMap = new Dictionary<string, NewLineIndex>(StringComparer.OrdinalIgnoreCase);
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
        public Region PopulateTextRegionProperties(Region inputRegion, Uri uri, bool populateSnippet)
        {
            if (inputRegion == null || inputRegion.IsBinaryRegion)
            {
                // For binary regions, only the byteOffset and byteLength properties
                // are relevant, and their values are always specified.
                return inputRegion;
            }

            NewLineIndex newLineIndex = GetNewLineIndex(uri, out string fileText);
            return PopulateTextRegionProperties(newLineIndex, inputRegion, fileText, populateSnippet);
        }

        private Region PopulateTextRegionProperties(NewLineIndex lineIndex, Region inputRegion, string fileText, bool populateSnippet)
        {
            // A GENERAL NOTE ON THE PROPERTY POPULATION PROCESS:
            // 
            // As a rule, if we find some existing data on the region, we will trust it 
            // and avoid overwriting it. We will take every opportunity, however, to 
            // validate that the existing information matches what the new line index
            // computes. Note that we could consider making the new line index more
            // efficient by deferring its newline computations until they are 
            // actually requested. If we do so, we could update this code to 
            // avoid verifying region data in cases where regions are fully 
            // populated (and we can skip file parsing required to build
            // the map of new line offsets).
            Debug.Assert(!inputRegion.IsBinaryRegion);

            // If we have no input source file, there is no work to do
            if (lineIndex == null) { return inputRegion; }

            Region region = inputRegion.DeepClone();

            if (region.StartLine == 0)
            {
                // This means we have a region specified entirely via charOffset
                PopulatePropertiesFromCharOffsetAndLength(lineIndex, region);
            }
            else
            {
                PopulatePropertiesFromStartAndEndProperties(lineIndex, region, fileText); 
            }

            if (populateSnippet)
            {
                region.Snippet = region.Snippet ?? new ArtifactContent();

                string snippetText = fileText.Substring(region.CharOffset, region.CharLength);
                if (region.Snippet.Text == null)
                {
                    region.Snippet.Text = snippetText;
                }
                Debug.Assert(region.Snippet.Text == snippetText);
            }

            return region;
        }

        internal Region ConstructMultilineContextSnippet(Region inputRegion, Uri uri)
        {
            if (inputRegion == null || inputRegion.IsBinaryRegion)
            {
                // Context snippets are relevant only for textual regions.
                return null;
            }

            NewLineIndex newLineIndex = GetNewLineIndex(uri, out string fileText);
            if (newLineIndex == null)
            {
                return null;
            }

            int maxLineNumber = newLineIndex.MaximumLineNumber;

            // Currently, we just grab a single line before and after the region start
            // and end lines, respectively. In the future, we could make this configurable.
            var region = new Region()
            {
                StartLine = inputRegion.StartLine == 1 ? 1 : inputRegion.StartLine - 1,
                EndLine = inputRegion.EndLine == maxLineNumber ? maxLineNumber : inputRegion.EndLine + 1
            };
            return this.PopulateTextRegionProperties(region, uri, populateSnippet: true);
        }

        private void PopulatePropertiesFromCharOffsetAndLength(NewLineIndex newLineIndex, Region region)
        {
            Debug.Assert(!region.IsBinaryRegion);
            Debug.Assert(region.StartLine == 0);
            Debug.Assert(region.CharLength > 0 || region.CharOffset > 0);

            int startLine, startColumn, endLine, endColumn;

            // Retrieve start and end line and column information from the new line index
            OffsetInfo offsetInfo = newLineIndex.GetOffsetInfoForOffset(region.CharOffset);
            startLine = offsetInfo.LineNumber;
            startColumn = offsetInfo.ColumnNumber;

            offsetInfo = newLineIndex.GetOffsetInfoForOffset(region.CharOffset + region.CharLength);
            endLine = offsetInfo.LineNumber;

            // The computation above points one past our actual region, because endColumn
            // is exclusive of the region. This allows for length to easily be computed
            // for single line regions: region.EndColumn - region.StartColumn
            endColumn = offsetInfo.ColumnNumber;

            // Only set values if they aren't already specified
            if (region.StartLine == 0) { region.StartLine = startLine; }
            if (region.StartColumn == 0) { region.StartColumn = startColumn; }
            if (region.EndLine == 0) { region.EndLine = endLine; }
            if (region.EndColumn == 0) { region.EndColumn = endColumn; }

            // Validate cases where new line index disagrees with explicit values
            Debug.Assert(region.StartLine == startLine);
            Debug.Assert(region.StartColumn == startColumn);
            Debug.Assert(region.EndLine == endLine);
            Debug.Assert(region.EndColumn == endColumn);
        }

        private void PopulatePropertiesFromStartAndEndProperties(NewLineIndex lineIndex, Region region, string fileText)
        {
            Debug.Assert(region.StartLine > 0);

            // Note: execution order of these helpers is important, as some 
            // calls assume that certain preceding helpers have executed,
            // with the result that certain properties are populated

            // Populated at this point: StartLine
            PopulateEndLine(region);

            // Populated at this point: StartLine, EndLine
            PopulateStartColumn(region);

            // Populated at this point: StartLine, EndLine, StartColumn
            PopulateEndColumn(lineIndex, region, fileText);

            // Populated at this point: StartLine, EndLine, StartColumn, EndColumn
            PopulateCharOffset(lineIndex, region);

            // Populated at this point: StartLine, EndLine, StartColumn, EndColumn, CharOffset
            PopulateCharLength(lineIndex, region);

            // Populated at this point: StartLine, EndLine, StartColumn, EndColumn, CharOffset, CharLength
            Debug.Assert(region.StartLine > 0);
            Debug.Assert(region.EndLine > 0);
            Debug.Assert((region.CharOffset + region.CharLength) <= fileText.Length);
            Debug.Assert(region.StartColumn > 0);
            Debug.Assert(region.CharLength > 0 || (region.StartColumn == region.EndColumn && region.StartLine == region.EndLine));
            Debug.Assert(region.EndColumn > 0);
        }

        private static void PopulateEndLine(Region region)
        {
            // Populated at this point: StartLine
            Debug.Assert(region.StartLine > 0);

            region.EndLine = region.EndLine == 0 ? region.StartLine : region.EndLine;
        }

        private static void PopulateStartColumn(Region region)
        {
            // Populated at this point: StartLine, EndLine
            Debug.Assert(region.StartLine > 0);
            Debug.Assert(region.EndLine > 0);

            region.StartColumn = region.StartColumn == 0 ? 1 : region.StartColumn;
        }


        private void PopulateEndColumn(NewLineIndex lineIndex, Region region, string fileText)
        {
            // Populated at this point: StartLine, EndLine, StartColumn
            Debug.Assert(region.StartLine > 0);
            Debug.Assert(region.StartColumn > 0);
            Debug.Assert(region.EndLine > 0);

            if (region.EndColumn == 0)
            {
                // No explicit end column. Increment from end line through
                // the end of the line, excluding new line characters
                LineInfo lineInfo = lineIndex.GetLineInfoForLine(region.EndLine);
                int endColumnOffset = lineInfo.StartOffset;

                while (endColumnOffset < fileText.Length &&
                       !NewLineIndex.s_newLineCharSet.Contains(fileText[endColumnOffset]))
                {
                    endColumnOffset++;
                }

                // End columns are 1-indexed
                region.EndColumn = endColumnOffset - lineInfo.StartOffset + 1;
            }
        }

        private static void PopulateCharOffset(NewLineIndex lineIndex, Region region)
        {
            // Populated at this point: StartLine, EndLine, StartColumn, EndColumn
            Debug.Assert(region.StartLine > 0);
            Debug.Assert(region.EndLine > 0);
            Debug.Assert(region.StartColumn > 0);
            Debug.Assert(region.EndColumn > 0);

            LineInfo lineInfo = lineIndex.GetLineInfoForLine(region.StartLine);

            // Now we have the offset of the starting line. Populate region.CharOffset.
            int offset = lineInfo.StartOffset;
            offset += region.StartColumn - 1;

            if (region.CharOffset == 0)
            {
                region.CharOffset = offset;
            }
            Debug.Assert(region.CharOffset == offset);
        }

        private void PopulateCharLength(NewLineIndex lineIndex, Region region)
        {
            // Populated at this point: StartLine, EndLine, StartColumn, EndColumn, CharOffset
            Debug.Assert(region.StartLine > 0);
            Debug.Assert(region.EndLine > 0);
            Debug.Assert(region.StartColumn > 0);
            Debug.Assert(region.EndColumn > 0);
            Debug.Assert(region.CharOffset > 0 || (region.StartLine == 1 && region.StartColumn == 1));

            LineInfo lineInfo = lineIndex.GetLineInfoForLine(region.EndLine);
            int charLength = lineInfo.StartOffset;
            charLength -= region.CharOffset;
            charLength += region.EndColumn - 1;

            if (region.CharLength == 0)
            {
                region.CharLength = charLength;
            }
            Debug.Assert(region.CharLength == charLength);
        }

        private NewLineIndex GetNewLineIndex(Uri uri, out string fileText)
        {
            fileText = null;

            // We will expand this code later to construct all possible URLs from
            // the log file, bearing in mind things like uriBaseIds. Also, we could
            // consider downloading and caching web-hosted source files.
            try
            {
                fileText = _fileSystem.ReadAllText(uri.LocalPath);
            }
            catch (IOException) { }

            return fileText != null ? new NewLineIndex(fileText) : null;
        }
    }
}