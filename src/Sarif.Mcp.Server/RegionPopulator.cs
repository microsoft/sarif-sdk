// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Adapted from the parent SDK's FileRegionsCache.cs and the AI plug-in's
// RegionPopulator.cs. Behavior matches the AI plug-in's region cascade as of
// the date of this port; the parent SDK's FileRegionsCache has a fix
// (proper-superset semantics for context regions, see PR-2901 SDK-D) that this
// implementation does NOT yet incorporate. Convergence is tracked as a
// follow-on with a characterization matrix.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Mcp.Server
{
    /// <summary>
    /// Given a partially-populated <see cref="Region"/> (typically just StartLine)
    /// and a <see cref="NewLineIndex"/> for the source file, populates all
    /// missing text region properties: StartLine, EndLine, StartColumn, EndColumn,
    /// CharOffset, CharLength, and Snippet.
    /// </summary>
    /// <remarks>
    /// SDK <see cref="Region"/> uses primitive sentinels (StartLine etc. default
    /// to 0, CharOffset defaults to -1) rather than nullable types. The cascade
    /// treats "unset" accordingly.
    /// </remarks>
    internal static class RegionPopulator
    {
        /// <summary>
        /// Fully populates a text region's properties from whatever the caller provided.
        /// Requires either StartLine (line/column path) or CharOffset (offset path).
        /// </summary>
        public static void Populate(NewLineIndex newlineIndex, Region region, bool populateSnippet)
        {
            if (newlineIndex == null || region == null) { return; }
            if (region.ByteOffset >= 0 || region.ByteLength > 0) { return; } // binary region

            if (region.StartLine > 0)
            {
                PopulateFromStartAndEndProperties(newlineIndex, region);
            }
            else if (region.CharOffset >= 0)
            {
                PopulateFromCharOffsetAndLength(newlineIndex, region);
            }
            else
            {
                return; // nothing to work with
            }

            if (populateSnippet)
            {
                PopulateSnippet(newlineIndex, region);
            }
        }

        /// <summary>Builds a context region (±N lines) around the target region.</summary>
        public static Region? BuildContextRegion(NewLineIndex newlineIndex, Region targetRegion, int contextLines = 3)
        {
            if (newlineIndex == null || targetRegion.StartLine <= 0)
            {
                return null;
            }

            int contextStart = Math.Max(1, targetRegion.StartLine - contextLines);
            int targetEndLine = targetRegion.EndLine > 0 ? targetRegion.EndLine : targetRegion.StartLine;
            int contextEnd = Math.Min(newlineIndex.MaximumLineNumber, targetEndLine + contextLines);

            var contextRegion = new Region
            {
                StartLine = contextStart,
                StartColumn = 1,
                EndLine = contextEnd
            };

            PopulateFromStartAndEndProperties(newlineIndex, contextRegion);
            PopulateSnippet(newlineIndex, contextRegion);

            return contextRegion;
        }

        // --- Cascade: StartLine → EndLine → StartColumn → EndColumn → CharOffset → CharLength ---
        // Execution order matters: each step assumes prior steps have run.

        private static void PopulateFromStartAndEndProperties(NewLineIndex newlineIndex, Region region)
        {
            // Step 1: EndLine defaults to StartLine (single-line region).
            if (region.EndLine <= 0)
            {
                region.EndLine = region.StartLine;
            }

            // Step 2: StartColumn defaults to 1.
            if (region.StartColumn <= 0)
            {
                region.StartColumn = 1;
            }

            // Step 3: EndColumn defaults to end-of-line (exclusive, past last char).
            if (region.EndColumn <= 0)
            {
                string endLineText = newlineIndex.GetLineText(region.EndLine);
                region.EndColumn = endLineText.Length + 1;
            }

            // Step 4: CharOffset from line + column.
            if (region.CharOffset < 0)
            {
                region.CharOffset = newlineIndex.GetOffset(region.StartLine, region.StartColumn);
            }

            // Step 5: CharLength from end position - start position.
            if (region.CharLength <= 0)
            {
                int endOffset = newlineIndex.GetOffset(region.EndLine, region.EndColumn);
                region.CharLength = Math.Max(0, endOffset - region.CharOffset);
            }
        }

        private static void PopulateFromCharOffsetAndLength(NewLineIndex newlineIndex, Region region)
        {
            int charOffset = region.CharOffset;
            int charLength = region.CharLength > 0 ? region.CharLength : 0;

            (int startLine, int startColumn) = newlineIndex.GetLineAndColumn(charOffset);
            (int endLine, int endColumn) = newlineIndex.GetLineAndColumn(charOffset + charLength);

            if (region.StartLine <= 0) { region.StartLine = startLine; }
            if (region.StartColumn <= 0) { region.StartColumn = startColumn; }
            if (region.EndLine <= 0) { region.EndLine = endLine; }
            if (region.EndColumn <= 0) { region.EndColumn = endColumn; }
        }

        private static void PopulateSnippet(NewLineIndex newlineIndex, Region region)
        {
            if (region.Snippet != null) { return; }
            if (region.StartLine < 1) { return; }
            if (region.StartLine > newlineIndex.MaximumLineNumber) { return; }

            int startLine = region.StartLine;
            int endLine = Math.Min(region.EndLine > 0 ? region.EndLine : startLine, newlineIndex.MaximumLineNumber);

            int startOffset = newlineIndex.GetLineStart(startLine);
            int endOffset = endLine < newlineIndex.MaximumLineNumber
                ? newlineIndex.GetLineStart(endLine + 1)
                : newlineIndex.Text.Length;

            // Trim trailing newline characters.
            while (endOffset > startOffset &&
                   (newlineIndex.Text[endOffset - 1] == '\r' || newlineIndex.Text[endOffset - 1] == '\n'))
            {
                endOffset--;
            }

            region.Snippet = new ArtifactContent { Text = newlineIndex.Text[startOffset..endOffset] };
        }
    }
}
