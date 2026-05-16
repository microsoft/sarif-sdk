// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Vendored from microsoft/sarif-sdk (MIT License), simplified for the MCP server.
// See src/Sarif/NewLineIndex.cs for the canonical implementation. The two
// implementations are not API-compatible: this one exposes GetLineStart,
// GetLineAndColumn, GetOffset, and GetLineText, which the MCP enrichment cascade
// depends on. Dedup is tracked as a follow-on.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Mcp.Server
{
    /// <summary>
    /// Index of newline positions in a string, enabling efficient offset ↔ line/column conversion.
    /// </summary>
    internal sealed class NewLineIndex
    {
        private readonly int[] _lineStarts;

        public string Text { get; }

        public int MaximumLineNumber => _lineStarts.Length;

        public NewLineIndex(string text)
        {
            this.Text = text ?? throw new ArgumentNullException(nameof(text));

            var starts = new List<int> { 0 };

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '\n')
                {
                    starts.Add(i + 1);
                }
                else if (c == '\r')
                {
                    // \r\n counts as one newline; the \n on next iteration will add the start.
                    if (i + 1 < text.Length && text[i + 1] == '\n')
                    {
                        continue;
                    }

                    starts.Add(i + 1);
                }
            }

            this._lineStarts = starts.ToArray();
        }

        /// <summary>Gets the character offset where a 1-based line begins.</summary>
        public int GetLineStart(int oneBasedLine)
        {
            if (oneBasedLine < 1 || oneBasedLine > this._lineStarts.Length)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(oneBasedLine),
                    $"Line {oneBasedLine} is out of range [1..{this._lineStarts.Length}].");
            }

            return this._lineStarts[oneBasedLine - 1];
        }

        /// <summary>Returns the 1-based line number containing the given character offset.</summary>
        public int GetLineNumberForOffset(int charOffset)
        {
            if (charOffset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(charOffset), "Offset cannot be negative.");
            }

            int idx = Array.BinarySearch(this._lineStarts, charOffset);
            if (idx < 0)
            {
                idx = ~idx - 1;
            }

            return idx + 1; // 1-based
        }

        /// <summary>Converts a character offset to a 1-based (line, column) pair.</summary>
        public (int Line, int Column) GetLineAndColumn(int charOffset)
        {
            int line = GetLineNumberForOffset(charOffset);
            int col = charOffset - this._lineStarts[line - 1] + 1; // 1-based column
            return (line, col);
        }

        /// <summary>Converts a 1-based line and column to a character offset.</summary>
        public int GetOffset(int oneBasedLine, int oneBasedColumn)
        {
            return GetLineStart(oneBasedLine) + (oneBasedColumn - 1);
        }

        /// <summary>Extracts text for the given 1-based line (without trailing newline).</summary>
        public string GetLineText(int oneBasedLine)
        {
            int start = GetLineStart(oneBasedLine);
            int end = oneBasedLine < this._lineStarts.Length
                ? this._lineStarts[oneBasedLine]
                : this.Text.Length;

            // Trim trailing \r\n or \n.
            while (end > start && (this.Text[end - 1] == '\r' || this.Text[end - 1] == '\n'))
            {
                end--;
            }

            return this.Text[start..end];
        }
    }
}
