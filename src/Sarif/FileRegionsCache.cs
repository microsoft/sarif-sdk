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
        public static readonly FileRegionsCache Instance = new FileRegionsCache();
        public const int DefaultCacheCapacity = 100;
        private readonly IFileSystem _fileSystem;
        internal readonly Cache<string, Tuple<string, NewLineIndex>> _cache;
        internal readonly Dictionary<string, Dictionary<int, string>> _rollingHashesCache;

        private static readonly int tab = (int)"\t"[0];
        private static readonly int space = (int)" "[0];
        private static readonly int lf = (int)"\n"[0];
        private static readonly int cr = (int)"\r"[0];
        private static readonly int EOF = 65535;
        private static readonly int BLOCK_SIZE = 5;
        private static readonly long MOD = (long)37;

        /// <summary>
        /// Creates a new <see cref="FileRegionsCache"/> object.
        /// </summary>
        /// <param name="capacity">
        /// The initial capacity of the cache.
        /// </param>
        /// <param name="fileSystem">
        /// An object that provides access to file system services.
        /// </param>
        public FileRegionsCache(int capacity = DefaultCacheCapacity, IFileSystem fileSystem = null)
        {
            _fileSystem = fileSystem ?? FileSystem.Instance;

            // Build a cache for this data, with the load method it should use to add new entries
            _cache = new Cache<string, Tuple<string, NewLineIndex>>(BuildIndexForFile, capacity);

            // Build a cache of rolling hashes (partial hash per line) for each artifact
            _rollingHashesCache = new Dictionary<string, Dictionary<int, string>>();
        }

        /// <summary>
        /// Creates a <see cref="Region"/> object, based on an existing Region, in which all
        /// text-related properties have been populated.
        /// </summary>
        /// <remarks>
        /// For example, if the input Region specifies only the StartLine property, the returned
        /// Region instance will have computed and populated other text-related properties, such
        /// as properties, such as CharOffset, CharLength, etc.
        /// </remarks>
        /// <param name="inputRegion">
        /// Region object that forms the basis of the returned Region object.
        /// </param>
        /// <param name="uri">
        /// URI of the artifact in which <paramref name="inputRegion"/> lies, used to retrieve
        /// from the cache the location of each newline in the artifact.
        /// </param>
        /// <param name="populateSnippet">
        /// Boolean that indicates if the region's Snippet property will be populated.
        /// </param>
        /// <param name="fileText">
        /// An optional argument that, if present, contains the text contents of the file
        /// specified by <paramref name="uri"/>.
        /// </param>
        /// <returns>
        /// A Region object whose text-related properties have been fully populated.
        /// </returns>
        public virtual Region PopulateTextRegionProperties(
            Region inputRegion,
            Uri uri,
            bool populateSnippet,
            string fileText = null)
        {
            if (inputRegion == null || inputRegion.IsBinaryRegion)
            {
                // For binary regions, only the byteOffset and byteLength properties
                // are relevant, and their values are always specified.
                return inputRegion;
            }

            NewLineIndex newLineIndex = GetNewLineIndex(uri, fileText);

            return PopulateTextRegionProperties(
                newLineIndex,
                inputRegion,
                newLineIndex?.Text,
                populateSnippet);
        }

        /// <summary>
        /// Clear current cache.
        /// </summary>
        public void ClearCache()
        {
            this._cache.Clear();
        }

        public void SuvamTest(
            Uri uri)
        {
            Hash(uri);
        }

        private long ComputeFirstMod()
        {
            long firstMod = (long)1;
            for (int i = 0; i < BLOCK_SIZE; i++)
            {
                firstMod = firstMod * MOD;
            }
            return firstMod;
        }

        private void Hash(Uri uri)
        {
            string filePath = uri.GetFilePath();

            if (!_rollingHashesCache.ContainsKey(filePath))
            {
                _rollingHashesCache.Add(filePath,new Dictionary<int, string>());
            }

            // Check if we have already computed the rolling hashes for this file.    
            if (_rollingHashesCache[filePath].Count > 0)
            {
                return;
            }

            // A rolling view into the input
            int[] window = new int[BLOCK_SIZE];

            int[] lineNumbers = new int[BLOCK_SIZE];
            for (int i = 0; i < lineNumbers.Length; i++)
            {
                lineNumbers[i] = -1;
            }

            long hashRaw = (long)0;
            long firstMod = ComputeFirstMod();

            // The current index in the window, will wrap around to zero when we reach BLOCK_SIZE
            int index = 0;
            // The line number of the character we are currently processing from the input
            int lineNumber = 0;
            // Is the next character to be read the start of a new line
            bool lineStart = true;
            // Was the previous character a CR (carriage return)
            bool prevCR = false;

            Dictionary<string, int> hashCounts = new Dictionary<string, int>();

            // Output the current hash and line number to the cache
            Action outputHash = () =>
            {
                ulong uhashRaw = (ulong)hashRaw;
                string hashValue = uhashRaw.ToString("x16");

                if (!hashCounts.ContainsKey(hashValue))
                {
                    hashCounts[hashValue] = 0;
                }

                hashCounts[hashValue]++;
                _rollingHashesCache[filePath][lineNumbers[index]] = $"{hashValue}:{hashCounts[hashValue]}";
                lineNumbers[index] = -1;
            };

            // Update the current hash value and increment the index in the window
            Action<int> updateHash = (current) =>
            {
                int begin = window[index];
                window[index] = current;

                hashRaw = (MOD * hashRaw) + (long)current - (firstMod * (long)begin);

                index = (index + 1) % BLOCK_SIZE;
            };

            // First process every character in the input, updating the hash and lineNumbers
            // as we go. Once we reach a point in the window again then we've processed
            // BLOCK_SIZE characters and if the last character at this point in the window
            // was the start of a line then we should output the hash for that line.
            Action<int> processCharacter = (current) =>
            {
                // skip tabs, spaces, and line feeds that come directly after a carriage return
                if (current == space || current == tab || (prevCR && current == lf))
                {
                    prevCR = false;
                    return;
                }
                // replace CR with LF
                if (current == cr)
                {
                    current = lf;
                    prevCR = true;
                }
                else
                {
                    prevCR = false;
                }
                if (lineNumbers[index] != -1)
                {
                    outputHash();
                }
                if (lineStart)
                {
                    lineStart = false;
                    lineNumber++;
                    lineNumbers[index] = lineNumber;
                }
                if (current == lf)
                {
                    lineStart = true;
                }
                updateHash(current);
            };

            string fileText = null;
            try
            {
                if (_fileSystem.FileExists(filePath))
                {
                    fileText = _fileSystem.FileReadAllText(filePath);
                }
            }
            catch (IOException) { }

            if (fileText != null)
            {
                for (int i = 0; i < fileText.Length; i++)
                {
                    processCharacter(fileText[i]);
                }

                processCharacter(EOF);

                // Flush the remaining lines
                for (int i = 0; i < BLOCK_SIZE; i++)
                {
                    if (lineNumbers[index] != -1)
                    {
                        outputHash();
                    }
                    updateHash(0);
                }
            }
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
            Assert(!inputRegion.IsBinaryRegion);

            // If we have no input source file, there is no work to do
            if (lineIndex == null) { return inputRegion; }

            Debug.Assert(fileText != null);

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

            if (populateSnippet
                && region.CharOffset >= 0
                && region.CharLength >= 0
                && (region.CharOffset + region.CharLength <= fileText.Length))
            {
                region.Snippet ??= new ArtifactContent();

                string snippetText = fileText.Substring(region.CharOffset, region.CharLength);
                if (region.Snippet.Text == null)
                {
                    region.Snippet.Text = snippetText;
                }
                Assert(region.Snippet.Text == snippetText);
            }

            return region;
        }

        public Region ConstructMultilineContextSnippet(Region inputRegion, Uri uri, string fileText = null)
        {
            if (inputRegion?.IsBinaryRegion != false)
            {
                // Context snippets are relevant only for textual regions.
                return null;
            }

            NewLineIndex newLineIndex = GetNewLineIndex(uri, fileText);
            if (newLineIndex == null)
            {
                return null;
            }

            const int bigSnippetLength = 512;
            const int smallSnippetLength = 128;

            // Generating full inputRegion to prevent issues.
            Region originalRegion = this.PopulateTextRegionProperties(inputRegion, uri, populateSnippet: true);

            int maxLineNumber = newLineIndex.MaximumLineNumber;

            var region = new Region
            {
                StartLine = inputRegion.StartLine == 1 ? 1 : inputRegion.StartLine - 1,
                EndLine = inputRegion.EndLine == maxLineNumber ? maxLineNumber : inputRegion.EndLine + 1
            };

            // Generating multilineRegion with one line before and after.
            Region multilineContextSnippet = this.PopulateTextRegionProperties(region, uri, populateSnippet: true);

            if (originalRegion.CharLength <= multilineContextSnippet.CharLength &&
                multilineContextSnippet.CharLength <= bigSnippetLength)
            {
                return multilineContextSnippet;
            }

            // We need this to re-calculate the region values when we call PopulateTextRegionProperties.
            region.StartColumn = 0;
            region.EndColumn = 0;
            region.StartLine = 0;
            region.EndLine = 0;
            region.CharOffset = originalRegion.CharOffset < smallSnippetLength
                ? 0
                : originalRegion.CharOffset - smallSnippetLength;

            region.CharLength = originalRegion.CharLength + region.CharOffset + smallSnippetLength < newLineIndex.Text.Length
                ? originalRegion.CharLength + smallSnippetLength + Math.Abs(region.CharOffset - originalRegion.CharOffset)
                : newLineIndex.Text.Length - region.CharOffset;

            // Generating  multineRegion with 128 characters to the left and right from the
            // originalRegion if possible.
            multilineContextSnippet = this.PopulateTextRegionProperties(region, uri, populateSnippet: true);

            // We can't generate a contextRegion which is smaller than the original region.
            Debug.Assert(originalRegion.CharLength <= multilineContextSnippet.CharLength);
            return multilineContextSnippet;
        }

        private void PopulatePropertiesFromCharOffsetAndLength(NewLineIndex newLineIndex, Region region)
        {
            Assert(!region.IsBinaryRegion);
            Assert(region.StartLine == 0);
            Assert(region.CharLength >= 0 || region.CharOffset >= 0);

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
            Assert(region.StartLine == startLine);
            Assert(region.StartColumn == startColumn);
            Assert(region.EndLine == endLine);
            Assert(region.EndColumn == endColumn);
        }

        private void PopulatePropertiesFromStartAndEndProperties(NewLineIndex lineIndex, Region region, string fileText)
        {
            Assert(region.StartLine > 0);

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
            Assert(region.StartLine > 0);
            Assert(region.EndLine > 0);
            Assert((region.CharOffset + region.CharLength) <= fileText.Length);
            Assert(region.StartColumn > 0);
            Assert(region.CharLength > 0 || (region.StartColumn == region.EndColumn && region.StartLine == region.EndLine));
            Assert(region.EndColumn > 0);
        }

        private static void PopulateEndLine(Region region)
        {
            // Populated at this point: StartLine
            Assert(region.StartLine > 0);

            region.EndLine = region.EndLine == 0 ? region.StartLine : region.EndLine;
        }

        private static void PopulateStartColumn(Region region)
        {
            // Populated at this point: StartLine, EndLine
            Assert(region.StartLine > 0);
            Assert(region.EndLine > 0);

            region.StartColumn = region.StartColumn == 0 ? 1 : region.StartColumn;
        }

        private void PopulateEndColumn(NewLineIndex lineIndex, Region region, string fileText)
        {
            // Populated at this point: StartLine, EndLine, StartColumn
            Assert(region.StartLine > 0);
            Assert(region.StartColumn > 0);
            Assert(region.EndLine > 0);

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
            Assert(region.StartLine > 0);
            Assert(region.EndLine > 0);
            Assert(region.StartColumn > 0);
            Assert(region.EndColumn > 0);

            LineInfo lineInfo = lineIndex.GetLineInfoForLine(region.StartLine);

            // Now we have the offset of the starting line. Populate region.CharOffset.
            int offset = lineInfo.StartOffset;
            offset += region.StartColumn - 1;

            if (region.CharOffset == 0 || region.CharOffset == -1)
            {
                region.CharOffset = offset;
            }

            Assert(region.CharOffset == offset);
        }

        private void PopulateCharLength(NewLineIndex lineIndex, Region region)
        {
            // Populated at this point: StartLine, EndLine, StartColumn, EndColumn, CharOffset
            Assert(region.StartLine > 0);
            Assert(region.EndLine > 0);
            Assert(region.StartColumn > 0);
            Assert(region.EndColumn > 0);
            Assert(region.CharOffset > 0 || (region.StartLine == 1 && region.StartColumn == 1));

            LineInfo lineInfo = lineIndex.GetLineInfoForLine(region.EndLine);
            int charLength = lineInfo.StartOffset;
            charLength -= region.CharOffset;
            charLength += region.EndColumn - 1;

            if (region.CharLength == 0)
            {
                region.CharLength = charLength;
            }
            Assert(region.CharLength == charLength);
        }

        private NewLineIndex GetNewLineIndex(Uri uri, string fileText = null)
        {
            string path = uri.GetFilePath();

            NewLineIndex newLineIndex;
            if (!_cache.ContainsKey(path) && fileText != null)
            {
                newLineIndex = new NewLineIndex(fileText);

                _cache[path] = new Tuple<string, NewLineIndex>(item1: path,
                                                               item2: newLineIndex);
            }
            else
            {
                newLineIndex = _cache[path].Item2;
            }

            return newLineIndex;
        }

        /// <summary>
        ///  Method to build cache entries which aren't already in the cache.
        /// </summary>
        /// <param name="localPath">Uri.LocalPath for the file to load</param>
        /// <returns>Cache entry to add to cache with file contents and NewLineIndex</returns>
        private Tuple<string, NewLineIndex> BuildIndexForFile(string localPath)
        {
            string fileText = null;
            NewLineIndex index = null;

            // We will expand this code later to construct all possible URLs from
            // the log file, bearing in mind things like uriBaseIds. Also, we could
            // consider downloading and caching web-hosted source files.
            try
            {
                if (_fileSystem.FileExists(localPath))
                {
                    fileText = _fileSystem.FileReadAllText(localPath);
                }
            }
            catch (IOException) { }

            if (fileText != null)
            {
                index = new NewLineIndex(fileText);
            }

            return new Tuple<string, NewLineIndex>(fileText, index);
        }

        /// <summary>
        ///  Method to build cache entries which aren't already in the cache.
        /// </summary>
        /// <param name="localPath">Uri.LocalPath for the file to load</param>
        /// <returns>Initialize a file in the cache with empty rolling hash index.</returns>
        private Dictionary<int, string> BuildRollingHashIndexForFile(string localPath)
        {
            string fileText = null;
            NewLineIndex index = null;

            // We will expand this code later to construct all possible URLs from
            // the log file, bearing in mind things like uriBaseIds. Also, we could
            // consider downloading and caching web-hosted source files.
            try
            {
                if (_fileSystem.FileExists(localPath))
                {
                    fileText = _fileSystem.FileReadAllText(localPath);
                }
            }
            catch (IOException) { }

            if (fileText != null)
            {
                index = new NewLineIndex(fileText);
            }

            return new Dictionary<int, string>();
        }

        private static void Assert(bool condition)
        {
            // Placeholder to report issues in a situationally appropriate way.
            //  We don't want Multitool rewrite to blow up.
            //  We don't want unit tests for invalid Regions to block on asserts; we want to verify the code leaves those results alone.
            //  We may want console or output log output when invalid Regions are detected during Multitool rewrite use.
            // https://github.com/microsoft/sarif-sdk/issues/1784

            //Debug.Assert(condition);
        }
    }
}
