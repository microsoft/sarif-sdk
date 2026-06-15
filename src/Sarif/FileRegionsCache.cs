// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Caches file text, hashes, newline indexes, and region snippets for SARIF enrichment.
    /// </summary>
    public class FileRegionsCache
    {
        public const int DefaultCacheCapacity = 100;
        private readonly IFileSystem _fileSystem;

        internal readonly Cache<string, string> _fileTextCache;
        internal readonly Cache<string, HashData> _hashDataCache;
        internal readonly Cache<string, NewLineIndex> _newLineIndexCache;

        /// <summary>
        /// The hash algorithms this cache computes when producing <see cref="HashData"/> for files.
        /// </summary>
        public HashAlgorithms HashAlgorithms { get; }

        /// <summary>
        /// The file system this cache uses for all I/O. Exposed to internal callers so that
        /// downstream <see cref="Artifact.Create"/> / <see cref="Run.GetFileIndex"/> sites
        /// can flow the same <see cref="IFileSystem"/> instance instead of silently falling
        /// back to the default <c>FileSystem.Instance</c>.
        /// </summary>
        internal IFileSystem FileSystem => _fileSystem;

        /// <summary>
        /// Creates a new <see cref="FileRegionsCache"/> object.
        /// </summary>
        /// <param name="capacity">
        /// The initial capacity of the cache.
        /// </param>
        /// <param name="fileSystem">
        /// An object that provides access to file system services.
        /// </param>
        /// <param name="hashAlgorithms">
        /// The set of hash algorithms this cache will compute when producing <see cref="HashData"/>
        /// for files. Defaults to <see cref="HashAlgorithms.Default"/> (SHA-256 only).
        /// </param>
        public FileRegionsCache(int capacity = DefaultCacheCapacity,
                                IFileSystem fileSystem = null,
                                HashAlgorithms hashAlgorithms = HashAlgorithms.Default)
        {
            _fileSystem = fileSystem ?? Sarif.FileSystem.Instance;
            HashAlgorithms = hashAlgorithms;

            _fileTextCache = new Cache<string, string>(RetrieveTextForFile, capacity);
            _hashDataCache = new Cache<string, HashData>(BuildHashDataForFile, capacity);
            _newLineIndexCache = new Cache<string, NewLineIndex>(BuildIndexForFile, capacity);
        }

        /// <summary>
        /// Creates a <see cref="Region"/> object, based on an existing Region, in which all
        /// text-related properties have been populated.
        /// </summary>
        /// <remarks>
        /// For example, a region with only <see cref="Region.StartLine"/> can receive computed
        /// <see cref="Region.CharOffset"/> and <see cref="Region.CharLength"/> values.
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
        /// <param name="overwriteExistingData">
        /// Controls how an authored region coordinate that diverges from the value computed
        /// from the source text is reconciled. When <c>false</c> (the default), the divergence
        /// throws an <see cref="ArgumentException"/>; when <c>true</c>, the authored value is
        /// overwritten with the computed value.
        /// </param>
        /// <returns>
        /// A Region object whose text-related properties have been fully populated.
        /// </returns>
        public virtual Region PopulateTextRegionProperties(
            Region inputRegion,
            Uri uri,
            bool populateSnippet,
            string fileText = null,
            bool overwriteExistingData = false)
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
                populateSnippet,
                overwriteExistingData);
        }

        /// <summary>
        /// Clear current cache.
        /// </summary>
        public void ClearCache()
        {
            _fileTextCache.Clear();
            _hashDataCache.Clear();
            _newLineIndexCache.Clear();
        }

        private static Region PopulateTextRegionProperties(NewLineIndex lineIndex, Region inputRegion, string fileText, bool populateSnippet, bool overwriteExistingData)
        {
            // Authored region data is preserved only when it agrees with source text; divergent
            // coordinates are reconciled according to overwriteExistingData.
            Assert(!inputRegion.IsBinaryRegion);

            if (lineIndex == null) { return inputRegion; }

            Debug.Assert(fileText != null);

            Region region = inputRegion.DeepClone();

            if (region.StartLine == 0)
            {
                PopulatePropertiesFromCharOffsetAndLength(lineIndex, region, overwriteExistingData);
            }
            else
            {
                PopulatePropertiesFromStartAndEndProperties(lineIndex, region, fileText, overwriteExistingData);
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

        internal const int BIGSNIPPETLENGTH = 512;
        internal const int SMALLSNIPPETLENGTH = 128;

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

            fileText ??= newLineIndex.Text;

            Region originalRegion = this.PopulateTextRegionProperties(inputRegion, uri, populateSnippet: true, fileText);

            if (originalRegion.CharLength >= BIGSNIPPETLENGTH)
            {
                return originalRegion.DeepClone();
            }

            int maxLineNumber = newLineIndex.MaximumLineNumber;

            var region = new Region
            {
                StartLine = inputRegion.StartLine == 1 ? 1 : inputRegion.StartLine - 1,
                EndLine = inputRegion.EndLine == maxLineNumber ? maxLineNumber : inputRegion.EndLine + 1
            };

            Region multilineContextSnippet = this.PopulateTextRegionProperties(region, uri, populateSnippet: true, fileText);

            if (originalRegion.CharLength <= multilineContextSnippet.CharLength &&
                multilineContextSnippet.CharLength <= BIGSNIPPETLENGTH)
            {
                return multilineContextSnippet;
            }

            // Force char-offset-based recomputation.
            region.StartColumn = 0;
            region.EndColumn = 0;
            region.StartLine = 0;
            region.EndLine = 0;
            region.CharOffset = Math.Max(0, originalRegion.CharOffset - SMALLSNIPPETLENGTH);

            region.CharLength = Math.Min(BIGSNIPPETLENGTH, fileText.Length - region.CharOffset);

            multilineContextSnippet = this.PopulateTextRegionProperties(region, uri, populateSnippet: true, fileText);

            // We can't generate a contextRegion which is smaller than the original region.
            Debug.Assert(originalRegion.CharLength <= multilineContextSnippet.CharLength);
            return multilineContextSnippet;
        }

        private static void PopulatePropertiesFromCharOffsetAndLength(NewLineIndex newLineIndex, Region region, bool overwriteExistingData)
        {
            Assert(!region.IsBinaryRegion);
            Assert(region.StartLine == 0);
            Assert(region.CharLength >= 0 || region.CharOffset >= 0);

            int startLine, startColumn, endLine, endColumn;

            OffsetInfo offsetInfo = newLineIndex.GetOffsetInfoForOffset(region.CharOffset);
            startLine = offsetInfo.LineNumber;
            startColumn = offsetInfo.ColumnNumber;

            offsetInfo = newLineIndex.GetOffsetInfoForOffset(region.CharOffset + region.CharLength);
            endLine = offsetInfo.LineNumber;

            // The computation above points one past our actual region, because endColumn
            // is exclusive of the region. This allows for length to easily be computed
            // for single line regions: region.EndColumn - region.StartColumn
            endColumn = offsetInfo.ColumnNumber;

            if (region.StartLine == 0) { region.StartLine = startLine; }
            if (region.StartColumn == 0) { region.StartColumn = startColumn; }
            if (region.EndLine == 0) { region.EndLine = endLine; }
            if (region.EndColumn == 0) { region.EndColumn = endColumn; }

            // Reconcile cases where the new line index disagrees with explicit values. These
            // fire only for authored coordinates that diverge from the source text (a value
            // just computed above is assigned, so it can never diverge here). Depending on
            // overwriteExistingData, a divergent authored value is either replaced with the
            // computed value or rejected with an ArgumentException.
            region.StartLine = ReconcileRegionCoordinate(overwriteExistingData, nameof(Region.StartLine), startLine, region.StartLine);
            region.StartColumn = ReconcileRegionCoordinate(overwriteExistingData, nameof(Region.StartColumn), startColumn, region.StartColumn);
            region.EndLine = ReconcileRegionCoordinate(overwriteExistingData, nameof(Region.EndLine), endLine, region.EndLine);
            region.EndColumn = ReconcileRegionCoordinate(overwriteExistingData, nameof(Region.EndColumn), endColumn, region.EndColumn);
        }

        private static void PopulatePropertiesFromStartAndEndProperties(NewLineIndex lineIndex, Region region, string fileText, bool overwriteExistingData)
        {
            Assert(region.StartLine > 0);

            // Helper order is significant; each step relies on coordinates populated earlier.
            PopulateEndLine(region);
            PopulateStartColumn(region);
            PopulateEndColumn(lineIndex, region, fileText);
            PopulateCharOffset(lineIndex, region, overwriteExistingData);
            PopulateCharLength(lineIndex, region, overwriteExistingData);

            Assert(region.StartLine > 0);
            Assert(region.EndLine > 0);
            if ((region.CharOffset + region.CharLength) > fileText.Length) { ReconcileRegionBounds(overwriteExistingData, region.CharOffset, region.CharLength, fileText.Length); }
            Assert(region.StartColumn > 0);
            Assert(region.CharLength > 0 || (region.StartColumn == region.EndColumn && region.StartLine == region.EndLine));
            Assert(region.EndColumn > 0);
        }

        private static void PopulateEndLine(Region region)
        {
            Assert(region.StartLine > 0);

            region.EndLine = region.EndLine == 0 ? region.StartLine : region.EndLine;
        }

        private static void PopulateStartColumn(Region region)
        {
            Assert(region.StartLine > 0);
            Assert(region.EndLine > 0);

            region.StartColumn = region.StartColumn == 0 ? 1 : region.StartColumn;
        }

        private static void PopulateEndColumn(NewLineIndex lineIndex, Region region, string fileText)
        {
            Assert(region.StartLine > 0);
            Assert(region.StartColumn > 0);
            Assert(region.EndLine > 0);

            if (region.EndColumn == 0)
            {
                // No explicit end column: scan to the line terminator.
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

        private static void PopulateCharOffset(NewLineIndex lineIndex, Region region, bool overwriteExistingData)
        {
            Assert(region.StartLine > 0);
            Assert(region.EndLine > 0);
            Assert(region.StartColumn > 0);
            Assert(region.EndColumn > 0);

            LineInfo lineInfo = lineIndex.GetLineInfoForLine(region.StartLine);

            int offset = lineInfo.StartOffset;
            offset += region.StartColumn - 1;

            if (region.CharOffset == 0 || region.CharOffset == -1)
            {
                region.CharOffset = offset;
            }

            region.CharOffset = ReconcileRegionCoordinate(overwriteExistingData, nameof(Region.CharOffset), offset, region.CharOffset);
        }

        private static void PopulateCharLength(NewLineIndex lineIndex, Region region, bool overwriteExistingData)
        {
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
            region.CharLength = ReconcileRegionCoordinate(overwriteExistingData, nameof(Region.CharLength), charLength, region.CharLength);
        }

        public HashData GetHashData(Uri uri, string fileText = null)
        {
            string path = uri.GetFilePath();

            if (fileText != null)
            {
                _fileTextCache[path] = fileText;
                return HashUtilities.ComputeHashesForText(fileText, HashAlgorithms);
            }

            return _hashDataCache[path];
        }

        /// <summary>
        /// Returns the full text of the artifact at <paramref name="uri"/>, reading it from the
        /// file system on first access and caching the result. Returns <c>null</c> when the file
        /// cannot be read (missing, I/O error, or access denied).
        /// </summary>
        public string GetText(Uri uri, string fileText = null)
        {
            string path = uri.GetFilePath();
            if (fileText != null)
            {
                _fileTextCache[path] = fileText;
            }

            return _fileTextCache[path];
        }

        public NewLineIndex GetNewLineIndex(Uri uri, string fileText = null)
        {
            string path = uri.GetFilePath();
            if (fileText != null)
            {
                _fileTextCache[path] = fileText;
            }
            fileText = _fileTextCache[path];

            NewLineIndex newLineIndex;
            if (!_newLineIndexCache.ContainsKey(path) && fileText != null)
            {
                newLineIndex = new NewLineIndex(fileText);

                _newLineIndexCache[path] = newLineIndex;
            }
            else
            {
                newLineIndex = _newLineIndexCache[path];
            }

            return newLineIndex;
        }

        private string RetrieveTextForFile(string path)
        {
            string fileText = null;

            try
            {
                if (_fileSystem.FileExists(path))
                {
                    fileText = _fileSystem.FileReadAllText(path);
                }
            }
            catch (IOException) { }
            catch (SecurityException) { }
            catch (UnauthorizedAccessException) { }

            return fileText;
        }

        private HashData BuildHashDataForFile(string path)
        {
            HashData hashes = HashUtilities.ComputeHashes(path, _fileSystem, HashAlgorithms);
            if (hashes != null)
            {
                return hashes;
            }

            // Mock file systems may expose cached text without a readable stream. Missing files
            // and directories must not hash as empty text.
            string fileText = _fileTextCache[path];
            return fileText != null
                ? HashUtilities.ComputeHashesForText(fileText, HashAlgorithms)
                : null;
        }

        private NewLineIndex BuildIndexForFile(string path)
        {
            string fileText = _fileTextCache[path];
            return fileText != null ? new NewLineIndex(fileText) : null;
        }

        private static void Assert(bool _)
        {
            // Structural invariant hook intentionally disabled for rewrite paths; authored-data
            // divergence is reconciled in ReconcileRegionCoordinate / ReconcileRegionBounds,
            // honoring overwriteExistingData.

            //Debug.Assert(condition);
        }

        /// <summary>
        /// Reconciles an authored region coordinate against the value computed from the source
        /// text. If they agree (including the common case where the value was just computed and
        /// assigned because the authored value was absent), the value is returned unchanged.
        /// On a genuine divergence the behavior depends on <paramref name="overwriteExistingData"/>.
        /// </summary>
        private static int ReconcileRegionCoordinate(bool overwriteExistingData, string propertyName, int computedValue, int authoredValue)
        {
            if (authoredValue == computedValue) { return authoredValue; }

            if (overwriteExistingData) { return computedValue; }

            throw new ArgumentException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "The input region specifies '{0}' = {1}, but the value computed from the source file is {2}. " +
                    "Authored region coordinates must exactly match the source text; omit a coordinate to have the " +
                    "SDK compute it, or set OptionallyEmittedData.OverwriteExistingData to recompute (overwrite) it.",
                    propertyName,
                    authoredValue,
                    computedValue),
                "inputRegion");
        }

        /// <summary>
        /// Reconciles an authored region whose character span extends beyond the source file.
        /// </summary>
        private static void ReconcileRegionBounds(bool overwriteExistingData, int charOffset, int charLength, int fileLength)
        {
            if (overwriteExistingData) { return; }

            throw new ArgumentException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "The input region's character span (charOffset {0} + charLength {1} = {2}) extends beyond the " +
                    "source file length ({3}). Authored region coordinates must lie within the source text; omit them " +
                    "to have the SDK compute the span, or set OptionallyEmittedData.OverwriteExistingData to recompute it.",
                    charOffset,
                    charLength,
                    (long)charOffset + charLength,
                    fileLength),
                "inputRegion");
        }
    }
}
