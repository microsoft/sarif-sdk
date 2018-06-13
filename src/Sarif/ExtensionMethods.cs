﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.using System;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;

using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class ExtensionMethods
    {
        public static bool Includes(this LoggingOptions loggingOptions, LoggingOptions otherLoggingOptions)
        {
            return (loggingOptions & otherLoggingOptions) == otherLoggingOptions;
        }

        public static string GetFileName(this Uri uri)
        {
            if (!uri.IsAbsoluteUri)
            {
                throw new InvalidOperationException();
            }

            return Path.GetFileName(uri.LocalPath);
        }

        public static string FormatForVisualStudio(this Region region)
        {
            if (region == null)
            {
                return String.Empty;
            }

            if (region.StartLine < 0)
            {
                throw new NotImplementedException();
            }

            // VS supports the following formatting options:
            //    (startLine)
            //    (startLine-endLine)
            //    (startLine,startColumn)
            //    (startLine,startColumn-endColumn)
            //    (startLine,startColumn,endLine,endColumn)

            bool multiline = region.EndLine > region.StartLine;
            bool multicolumn = (multiline || region.EndColumn > region.StartColumn);

            if (multiline)
            {
                if (multicolumn && (region.StartColumn > 1 || region.EndColumn > 1))
                {
                    //  (startLine,startColumn,endLine,endColumn)
                    return
                        "(" +
                        region.StartLine.ToString(CultureInfo.InvariantCulture) + "," +
                        (region.StartColumn > 0 ? region.StartColumn.ToString(CultureInfo.InvariantCulture) : "1") + "," +
                        region.EndLine.ToString(CultureInfo.InvariantCulture) + "," +
                        (region.EndColumn > 0 ? region.EndColumn.ToString(CultureInfo.InvariantCulture) : "1") +
                        ")";
                }
                //  (startLine-endLine)
                return
                    "(" +
                    region.StartLine.ToString(CultureInfo.InvariantCulture) + "-" + region.EndLine.ToString(CultureInfo.InvariantCulture) +
                    ")";
            }

            if (multicolumn)
            {
                // (startLine,startColumn-endColumn)
                return
                    "(" +
                    region.StartLine.ToString(CultureInfo.InvariantCulture) + "," +
                    region.StartColumn.ToString(CultureInfo.InvariantCulture) + "-" +
                    region.EndColumn.ToString(CultureInfo.InvariantCulture) +
                    ")";
            }

            if (region.StartColumn > 1)
            {
                // (startLine,startColumn)
                return
                     "(" +
                     region.StartLine.ToString(CultureInfo.InvariantCulture) + "," + region.StartColumn.ToString(CultureInfo.InvariantCulture) +
                     ")";
            }
            // (startLine)
            return
                 "(" +
                 region.StartLine.ToString(CultureInfo.InvariantCulture) +
                 ")";
        }

        public static string FormatForVisualStudio(this Result result, IRule rule)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            var messageLines = new List<string>();

            foreach (var location in result.Locations)
            {
                Uri uri = location.PhysicalLocation.FileLocation.Uri;
                string path = uri.IsAbsoluteUri && uri.IsFile ? uri.LocalPath : uri.ToString();
                messageLines.Add(
                    string.Format(
                        CultureInfo.InvariantCulture, "{0}{1}: {2} {3}: {4}",
                        path,
                        location.PhysicalLocation.Region.FormatForVisualStudio(),
                        result.Level.FormatForVisualStudio(),
                        result.RuleId,
                        result.GetMessageText(rule)
                        ));
            }

            return string.Join(Environment.NewLine, messageLines);
        }

        public static string FormatForVisualStudio(this ResultLevel level)
        {
            switch (level)
            {
                case ResultLevel.Error:
                    return "error";

                case ResultLevel.Warning:
                    return "warning";

                default:
                    return "info";
            }
        }

        /// <summary>
        /// Completely populate all Region property members. Missing data
        /// is computed based on the values that are already present.
        /// </summary>
        /// <param name="region"></param>
        /// <param name="newLineIndex"></param>
        public static void Populate(this Region region, NewLineIndex newLineIndex)
        {
            if (region == null)
            {
                throw new ArgumentNullException(nameof(region));
            }

            if (newLineIndex == null)
            {
                throw new ArgumentNullException(nameof(newLineIndex));
            }

            // A call to Populate is an implicit indicator that we are working
            // with a text region (otherwise the offset and length would be 
            // sufficient data to constitute the region).

            if (region.StartLine == 0)
            {
                OffsetInfo offsetInfo = newLineIndex.GetOffsetInfoForOffset(region.Offset);
                region.StartLine = offsetInfo.LineNumber;
                region.StartColumn = offsetInfo.ColumnNumber;

                offsetInfo = newLineIndex.GetOffsetInfoForOffset(region.Offset + region.Length);
                region.EndLine = offsetInfo.LineNumber;
                region.EndColumn = offsetInfo.ColumnNumber;
            }
            else
            {
                // Make endColumn and endLine explicit, if not expressed
                if (region.EndLine == 0) { region.EndLine = region.StartLine; }
                if (region.StartColumn == 0) { region.StartColumn = 1; }
                if (region.EndColumn == 0) { region.EndColumn = region.StartColumn; }

                LineInfo lineInfo = newLineIndex.GetLineInfoForLine(region.StartLine);
                region.Offset = lineInfo.StartOffset + (region.StartColumn - 1);

                lineInfo = newLineIndex.GetLineInfoForLine(region.EndLine);
                region.Length = lineInfo.StartOffset + (region.EndColumn - 1) - region.Offset;
            }
        }

        public static string GetMessageText(this Result result, IRule rule)
        {
            return GetMessageText(result, rule, concise: false);
        }

        public static string GetMessageText(this Result result, IRule rule, bool concise)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            string text = result.Message?.Text;
            if (string.IsNullOrEmpty(text))
            {
                text = string.Empty;    // Ensure that it's not null.

                if (rule != null && !string.IsNullOrWhiteSpace(result.RuleMessageId))
                {
                    string ruleMessageId = result.RuleMessageId;
                    string messageString;

                    string[] arguments = null;

                    if (result.Message?.Arguments != null)
                    {
                        arguments = new string[result.Message.Arguments.Count];
                        result.Message.Arguments.CopyTo(arguments, 0);
                    }
                    else
                    {
                        arguments = new string[0];
                    }

                    if (rule.MessageStrings?.ContainsKey(ruleMessageId) == true)
                    {
                        messageString = rule.MessageStrings[ruleMessageId];

#if DEBUG
                        int argumentsCount = arguments.Length;
                        for (int i = 0; i < argumentsCount; i++)
                        {
                            // If this assert fires, there are too many arguments for the specifier
                            // or there is an argument is skipped or not consumed in the specifier
                            Debug.Assert(messageString.Contains("{" + i.ToString(CultureInfo.InvariantCulture) + "}"));
                        }
#endif

                        text = string.Format(CultureInfo.InvariantCulture, messageString, arguments);

#if DEBUG
                        // If this assert fires, an insufficient # of arguments might
                        // have been provided to String.Format.
                        Debug.Assert(!text.Contains("{"));
#endif
                    }
                }
            }

            if (concise)
            {
                text = GetFirstSentence(text);
            }

            return text;
        }

        public static string GetFirstSentence(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            int length = 0;
            bool withinQuotes = false;
            bool withinParentheses = false;
            bool lastEncounteredWasDot = false;
            bool withinEllipsis = false;

            foreach (char ch in text)
            {
                length++;
                switch (ch)
                {
                    case '\'':
                    {
                        // we'll ignore everything within parenthized text
                        if (!withinParentheses)
                        {
                            withinQuotes = !withinQuotes;
                        }
                        lastEncounteredWasDot = false;
                        break;
                    }

                    case '(':
                    {
                        if (!withinQuotes)
                        {
                            withinParentheses = true;
                        }
                        lastEncounteredWasDot = false;
                        break;
                    }

                    case ')':
                    {
                        if (!withinQuotes)
                        {
                            withinParentheses = false;
                        }
                        lastEncounteredWasDot = false;
                        break;
                    }

                    case '.':
                    {
                        if (withinQuotes || withinParentheses || withinEllipsis) { continue; }
                        if (length < text.Length && text[length] == '.')
                        {
                            withinEllipsis = true;
                            lastEncounteredWasDot = false;
                            break;
                        }

                        lastEncounteredWasDot = true;
                        break;
                    }

                    // If we encounter a line-break, we return all leading text.
                    case '\n':
                    case '\r':
                    {
                        if (withinQuotes || withinParentheses) { continue; }
                        return text.Substring(0, length).TrimEnd('\r', '\n', ' ', '.') + ".";
                    }

                    // If we encounter a space following a period, return 
                    // all text terminating in the period (inclusive).
                    case ' ':
                    {
                        if (!lastEncounteredWasDot) continue;
                        if (withinQuotes || withinParentheses) { continue; }
                        return text.Substring(0, length).TrimEnd('\r', '\n', ' ', '.') + ".";
                    }

                    default:
                    {
                        lastEncounteredWasDot = false;
                        break;
                    }
                }
            }
            return text.TrimEnd('.') + ".";
        }

        /// <summary>Retrieves a property value if it exists, or null.</summary>
        /// <param name="properties">A properties object from which the property shall be
        /// retrieved, or null.</param>
        /// <param name="key">The property name / key.</param>
        /// <returns>
        /// If <paramref name="properties"/> is not null and an entry for the supplied key exists, the
        /// value associated with that key; otherwise, null.
        /// </returns>
        internal static string PropertyValue(this Dictionary<string, string> properties, string key)
        {
            string propValue;

            if (properties != null &&
                properties.TryGetValue(key, out propValue))
            {
                return propValue;
            }

            return null;
        }

        /// <summary>Checks if a character is a newline.</summary>
        /// <param name="testedCharacter">The character to check.</param>
        /// <returns>true if newline, false if not.</returns>
        internal static bool IsNewline(this char testedCharacter)
        {
            return testedCharacter == '\r'
                || testedCharacter == '\n'
                || testedCharacter == '\u2028'  // Unicode line separator
                || testedCharacter == '\u2029'; // Unicode paragraph separator
        }

        /// <summary>
        /// Returns whether or not the range [<paramref name="startIndex"/>,
        /// <paramref name="startIndex"/> + <paramref name="target"/><c>.Length</c>) is equal to the
        /// supplied string.
        /// </summary>
        /// <param name="array">The array to check.</param>
        /// <param name="startIndex">The start index in the array to check.</param>
        /// <param name="target">Target string to look for in the array.</param>
        /// <returns>
        /// true if the range [<paramref name="startIndex"/>, <paramref name="startIndex"/> +
        /// <paramref name="target"/><c>.Length</c>) is equal to
        /// <paramref name="target"/>. If the range is undefined in the bounds of the array, false.
        /// </returns>
        internal static bool Matches(this char[] array, int startIndex, string target)
        {
            if (startIndex < 0)
            {
                return false;
            }

            int targetLength = target.Length;
            if (targetLength + startIndex >= array.Length)
            {
                return false;
            }

            for (int idx = 0; idx < targetLength; ++idx)
            {
                if (array[idx + startIndex] != target[idx])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Consumes content from an XML reader until the end element of the element at endElementDepth
        /// <paramref name="endElementDepth"/>, including the end element.
        /// </summary>
        /// <param name="xmlReader">The <see cref="XmlReader"/> whose contents shall be consumed.</param>
        /// <param name="endElementDepth">The endElementDepth of node to consume.</param>
        internal static void ConsumeElementOfDepth(this XmlReader xmlReader, int endElementDepth)
        {
            int enteringReaderDepth = xmlReader.Depth;

            if (enteringReaderDepth < endElementDepth)
            {
                return;
            }

            if (enteringReaderDepth == endElementDepth)
            {
                // Move to the following element
                xmlReader.Read();
            }

            while (xmlReader.Depth > endElementDepth && xmlReader.Read()) { }

            if (xmlReader.NodeType == XmlNodeType.EndElement)
            {
                // Consume the end element
                xmlReader.Read();
            }
        }

    }
}
