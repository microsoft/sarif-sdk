// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Text;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    /// <summary>
    /// Utility functions for sanitizing comments from a .g4 file for placement into a .cs file.
    /// </summary>
    internal static class CommentSanitizer
    {
        private const int TabLength = 4;

        /// <summary>Sanitizes a comment block for inclusion into an XML doc comment.</summary>
        /// <param name="sourceComment">Source comment to sanitize.</param>
        /// <returns>The comment with whitespace sanitized for placement in an XML doc comment.</returns>
        public static string Sanitize(string sourceComment)
        {
            string[] rawLines = SplitByNewlines(sourceComment);

            // Find the length of the common prefix whitespace 
            int commonWhitespaceStart = Int32.MaxValue;
            for (int idx = 0; idx < rawLines.Length; ++idx)
            {
                string line = rawLines[idx];
                // Logical index == whitespace index with tabs replaced with spaces.
                int firstNonWhitespaceLogicalIndex = 0;
                // Physical index == underlying memory layout index.
                int firstNonWhitespacePhysicalIndex = 0;
                while (firstNonWhitespacePhysicalIndex < line.Length)
                {
                    char c = line[firstNonWhitespacePhysicalIndex];
                    if (c == ' ')
                    {
                        ++firstNonWhitespacePhysicalIndex;
                        ++firstNonWhitespaceLogicalIndex;
                    }
                    else if (c == '\t')
                    {
                        ++firstNonWhitespacePhysicalIndex;
                        firstNonWhitespaceLogicalIndex += (TabLength - (firstNonWhitespaceLogicalIndex % TabLength));
                    }
                    else
                    {
                        break;
                    }
                }

                if (firstNonWhitespacePhysicalIndex == line.Length)
                {
                    continue; // Ignore lines consisting of all whitespace
                }

                // Replace preceding tabs with spaces.
                if (firstNonWhitespacePhysicalIndex != firstNonWhitespaceLogicalIndex)
                {
                    Debug.Assert(firstNonWhitespacePhysicalIndex < firstNonWhitespaceLogicalIndex);
                    line = String.Concat(
                        new string(' ', firstNonWhitespaceLogicalIndex),
                        line.Remove(0, firstNonWhitespacePhysicalIndex)
                        );

                    rawLines[idx] = line;
                }

                commonWhitespaceStart = Math.Min(commonWhitespaceStart, firstNonWhitespaceLogicalIndex);
            }

            var sanitizedComment = new StringBuilder(sourceComment.Length);
            sanitizedComment.AppendLine();
            foreach (string line in rawLines)
            {
                for (int lastNonWhitespace = line.Length - 1; lastNonWhitespace >= 0; --lastNonWhitespace)
                {
                    if (!Char.IsWhiteSpace(line[lastNonWhitespace]))
                    {
                        sanitizedComment.Append(line, commonWhitespaceStart, lastNonWhitespace - commonWhitespaceStart + 1);
                        sanitizedComment.AppendLine();
                        break; // for lastNonWhitespace
                    }
                }
            }

            return sanitizedComment.ToString();
        }

        private static readonly char[] s_newlineCharacters = Environment.NewLine.ToCharArray();

        internal static string[] SplitByNewlines(string sourceString)
        {
            return sourceString.Split(s_newlineCharacters, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
