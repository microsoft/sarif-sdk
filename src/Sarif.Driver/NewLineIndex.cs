// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>
    /// An index of newline start locations in a string in order to relatively cheaply
    /// turn a given offset into a line / column number.
    /// </summary>
    public class NewLineIndex
    {
        // Each index n in this list denotes the halfopen range
        //      [lineOffsetStarts(n), lineOffsetStarts(n+1))
        // which is the line in the file at index n.
        private readonly ImmutableArray<int> _lineOffsetStarts;

        /// <summary>Initializes a new instance of the <see cref="NewLineIndex"/> class indexing the
        /// specified string.</summary>
        /// <param name="textToIndex">The text to add to this <see cref="NewLineIndex"/>.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public NewLineIndex(string textToIndex)
        {
            ImmutableArray<int>.Builder result = ImmutableArray.CreateBuilder<int>();
            result.Add(0);

            int indexLength = textToIndex.Length;
            for (int charCount = 0; charCount < indexLength; ++charCount)
            {
                char c = textToIndex[charCount];
                // Detect \r, \n, \u2028, \u2029, but NOT \r\n
                // (\r\n gets taken care of on the following loop
                // iteration and is detected as \n there)
                if (c == '\n' ||
                    (c == '\r' && (charCount + 1 >= indexLength || textToIndex[charCount + 1] != '\n')) ||
                    c == '\u2028' ||     // Unicode line separator
                    c == '\u2029'        // Unicode paragraph separator
                    )
                {
                    result.Add(charCount + 1);
                }
            }

            _lineOffsetStarts = result.ToImmutable();
        }

        /// <summary>Gets a <see cref="LineInfo"/> for the line at the specified index.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lineNumber"/> is not
        /// a valid line number; e.g. if it is zero, negative, or greater than the maximum line count in
        /// the file.</exception>
        /// <param name="lineNumber">The line number.</param>
        /// <returns>A <see cref="LineInfo"/> for <paramref name="lineNumber"/>.</returns>
        public LineInfo GetLineInfoForLine(int lineNumber)
        {
            if (lineNumber <= 0 || lineNumber > this.MaximumLineNumber)
            {
                throw new ArgumentOutOfRangeException("lineNumber", lineNumber, ExceptionStrings.LineNumberWasOutOfRange);
            }

            return new LineInfo(_lineOffsetStarts[lineNumber - 1], lineNumber);
        }

        /// <summary>Gets the maximum line number.</summary>
        /// <value>The maximum line number.</value>
        public int MaximumLineNumber
        {
            get
            {
                return _lineOffsetStarts.Length;
            }
        }

        /// <summary>Gets line information for the line containing the character at the specified offset.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="offset"/> is negative.</exception>
        /// <param name="offset">The offset.</param>
        /// <returns>The line information for the specified offset.</returns>
        public LineInfo GetLineInfoForOffset(int offset)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", offset, ExceptionStrings.ValueCannotBeNegative);
            }

            int startLine = _lineOffsetStarts.BinarySearch(offset);

            if (startLine < 0)
            {
                // If BinarySearch returns negative, returns the bitwise
                // complement of the next larger index. (upper_bound)
                // We want the next smaller index, which is where the -1 comes from.
                startLine = ~startLine - 1;
                Debug.Assert(startLine >= 0); // Because startLine < 0 if block
            }

            return new LineInfo(_lineOffsetStarts[startLine], startLine + 1);
        }

        /// <summary>Gets information for a given offset, such as the line and column numbers.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="offset"/> is negative.</exception>
        /// <param name="offset">The offset for which information shall be obtained.</param>
        /// <returns>The information for offset.</returns>
        public OffsetInfo GetOffsetInfoForOffset(int offset)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", offset, ExceptionStrings.ValueCannotBeNegative);
            }

            LineInfo lineInfo = this.GetLineInfoForOffset(offset);
            return new OffsetInfo(offset - lineInfo.StartOffset, lineInfo.LineNumber);
        }
    }
}
